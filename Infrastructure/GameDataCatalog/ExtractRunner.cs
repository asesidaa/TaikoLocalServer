using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Infrastructure.Extract;
using TaikoLocalServer.Infrastructure.Extract.Nijiiro;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog;

/// <summary>
/// Runs the extractor (Infrastructure.Extract, the same code as <c>TaikoLocalServer extract</c>) with
/// <c>--game GamePath --out wwwroot</c>, captures its report, and reloads the game data when it's done. One run at a time. With AllowRemoteGameDir, the game folder can
/// instead be the files an admin sent from their browser (only the ones <c>extract --inputs</c> lists), kept in a temp
/// folder in the install's layout until the run.
/// </summary>
public sealed class ExtractRunner(IOptions<ServerSettings> settings, IGameDataCatalog catalog, ILogger<ExtractRunner> logger) : IExtractRunner
{
    private const int MaxLines = 2000;

    private readonly Lock gate = new();
    private readonly List<string> output = [];
    private bool running, mods;
    private DateTime? startedAt, finishedAt;
    private int? exitCode;
    private ExtractInputs? inputs;

    private static readonly string UploadPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "TaikoLocalServer-game-upload"));
    private string GamePath => settings.Value.GamePath?.Trim() ?? "";
    private bool RemoteAllowed => settings.Value.AllowRemoteGameDir;

    public ExtractStatus Status
    {
        get
        {
            lock (gate)
                return new ExtractStatus(GamePath, GamePath.Length > 0 && Directory.Exists(GamePath), RemoteAllowed,
                    running, mods, startedAt, finishedAt, exitCode, output.ToList());
        }
    }

    public string? Start(bool withMods, bool uploaded = false)
    {
        if (uploaded && !RemoteAllowed) return "Sending the game folder from the browser is off (AllowRemoteGameDir).";
        if (uploaded && !File.Exists(Path.Combine(UploadPath, "Data", "x64", "datatable", "musicinfo.bin"))) return "No game folder was sent.";
        if (!uploaded && (GamePath.Length == 0 || !Directory.Exists(GamePath))) return "GamePath isn't set, or the folder doesn't exist.";

        List<string> args = ["--game", uploaded ? UploadPath : GamePath];
        if (withMods) args.Add("--mods");

        lock (gate)
        {
            if (running) return "extract is already running.";
            running = true;
            mods = withMods;
            startedAt = DateTime.UtcNow;
            finishedAt = null;
            exitCode = null;
            output.Clear();
        }
        _ = Task.Run(() => RunAsync([.. args], uploaded));
        return null;
    }

    public Task<(ExtractInputs? Inputs, string? Error)> InputsAsync(CancellationToken cancellationToken)
    {
        if (!RemoteAllowed) return Task.FromResult<(ExtractInputs?, string?)>((null, "Sending the game folder from the browser is off (AllowRemoteGameDir)."));
        inputs ??= NijiiroExtractor.Inputs().Deserialize<ExtractInputs>(JsonSerializerOptions.Web);
        return Task.FromResult<(ExtractInputs?, string?)>((inputs, null));
    }

    public string? ClearUpload()
    {
        if (!RemoteAllowed) return "Sending the game folder from the browser is off (AllowRemoteGameDir).";
        lock (gate)
        {
            if (running) return "extract is running.";
            DeleteUpload();
        }
        return null;
    }

    public async Task<string?> SaveUploadAsync(string path, Stream content, CancellationToken cancellationToken)
    {
        var (list, error) = await InputsAsync(cancellationToken);
        if (list is null) return error;
        if (Match(list, path) is not { } namesOnly) return $"{path} isn't a file extract reads.";
        lock (gate)
            if (running) return "extract is running.";

        var full = Path.GetFullPath(Path.Combine(UploadPath, path));
        if (!full.StartsWith(UploadPath + Path.DirectorySeparatorChar, StringComparison.Ordinal)) return $"{path} isn't a file extract reads.";
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        await using var file = File.Create(full);
        // Movies: extract only lists their names.
        if (!namesOnly) await content.CopyToAsync(file, cancellationToken);
        return null;
    }

    /// <summary>
    /// Whether extract reads <paramref name="path"/> (relative to the game folder, <c>/</c>-separated): null if not,
    /// true if only its name matters, false if its contents do. Anything that could leave the folder is refused.
    /// </summary>
    public static bool? Match(ExtractInputs inputs, string path)
    {
        if (path.Length is 0 or > 260 || path.Contains('\\') || path.Contains(':') || path.StartsWith('/')
            || path.Split('/').Any(part => part is "" or "." or ".."))
            return null;
        foreach (var set in new[] { inputs, inputs.Mods })
        {
            if (set is null) continue;
            if (set.Files.Any(rule => rule.EndsWith('/') ? path.StartsWith(rule, StringComparison.Ordinal) : path == rule)) return false;
            if (set.Names.Any(rule => path.StartsWith(rule, StringComparison.Ordinal) && !path[rule.Length..].Contains('/'))) return true;
        }
        return null;
    }

    private async Task RunAsync(string[] args, bool uploaded)
    {
        var code = -1;
        try
        {
            using var report = new LineWriter(Add);
            code = await Task.Run(() => ExtractCli.Run(args, PathHelper.GetRootPath(), report, report));
            logger.LogInformation("extract finished with exit code {ExitCode}", code);
            await catalog.ReloadAsync();
        }
        catch (Exception e)
        {
            Add($"error: {e.Message}");
            logger.LogError(e, "extract failed");
        }
        finally
        {
            lock (gate)
            {
                running = false;
                finishedAt = DateTime.UtcNow;
                exitCode = code;
                // The sent files are only needed for this run (about 600 MB).
                if (uploaded) DeleteUpload();
            }
        }
    }

    private void DeleteUpload()
    {
        try
        {
            if (Directory.Exists(UploadPath)) Directory.Delete(UploadPath, true);
        }
        catch (IOException e)
        {
            logger.LogWarning(e, "Couldn't remove the uploaded game files in {Path}", UploadPath);
        }
    }

    private void Add(string? line)
    {
        if (line is null) return;
        lock (gate)
            if (output.Count < MaxLines) output.Add(line);
    }

    /// <summary>Hands each line written to it to <paramref name="onLine"/>, as the report goes.</summary>
    private sealed class LineWriter(Action<string> onLine) : TextWriter
    {
        private readonly StringBuilder line = new();
        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            if (value == '\n')
            {
                onLine(line.ToString().TrimEnd('\r'));
                line.Clear();
            }
            else
            {
                line.Append(value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (line.Length > 0) onLine(line.ToString());
            line.Clear();
            base.Dispose(disposing);
        }
    }
}
