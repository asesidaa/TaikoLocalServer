using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Contracts.AdminApi.Responses;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog;

/// <summary>
/// Runs <c>extract(.exe)</c> from next to the server exe with <c>--game GamePath --out wwwroot</c>, captures its
/// report, and reloads the game data when it's done. One run at a time. With AllowRemoteGameDir, the game folder can
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

    private static string ExePath => Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, OperatingSystem.IsWindows() ? "extract.exe" : "extract");
    private static readonly string UploadPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "TaikoLocalServer-game-upload"));
    private string GamePath => settings.Value.GamePath?.Trim() ?? "";
    private bool RemoteAllowed => settings.Value.AllowRemoteGameDir && File.Exists(ExePath);

    public ExtractStatus Status
    {
        get
        {
            lock (gate)
                return new ExtractStatus(File.Exists(ExePath), GamePath, GamePath.Length > 0 && Directory.Exists(GamePath), RemoteAllowed,
                    running, mods, startedAt, finishedAt, exitCode, output.ToList());
        }
    }

    public string? Start(bool withMods, bool uploaded = false)
    {
        if (!File.Exists(ExePath)) return "extract isn't next to the server exe.";
        if (uploaded && !RemoteAllowed) return "Sending the game folder from the browser is off (AllowRemoteGameDir).";
        if (uploaded && !File.Exists(Path.Combine(UploadPath, "Data", "x64", "datatable", "musicinfo.bin"))) return "No game folder was sent.";
        if (!uploaded && (GamePath.Length == 0 || !Directory.Exists(GamePath))) return "GamePath isn't set, or the folder doesn't exist.";

        var info = new ProcessStartInfo(ExePath)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        info.ArgumentList.Add("--game");
        info.ArgumentList.Add(uploaded ? UploadPath : GamePath);
        info.ArgumentList.Add("--out");
        info.ArgumentList.Add(PathHelper.GetRootPath());
        if (withMods) info.ArgumentList.Add("--mods");

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
        _ = Task.Run(() => RunAsync(info, uploaded));
        return null;
    }

    public async Task<(ExtractInputs? Inputs, string? Error)> InputsAsync(CancellationToken cancellationToken)
    {
        if (!RemoteAllowed) return (null, "Sending the game folder from the browser is off (AllowRemoteGameDir).");
        if (inputs is not null) return (inputs, null);
        var info = new ProcessStartInfo(ExePath, "--inputs") { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
        using var process = Process.Start(info)!;
        var json = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0) return (null, "This extract is too old to list its inputs; update it.");
        inputs = JsonSerializer.Deserialize<ExtractInputs>(json, JsonSerializerOptions.Web);
        return (inputs, null);
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

    private async Task RunAsync(ProcessStartInfo info, bool uploaded)
    {
        var code = -1;
        try
        {
            using var process = new Process { StartInfo = info };
            process.OutputDataReceived += (_, e) => Add(e.Data);
            process.ErrorDataReceived += (_, e) => Add(e.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
            code = process.ExitCode;
            logger.LogInformation("extract finished with exit code {ExitCode}", code);
            await catalog.ReloadAsync();
        }
        catch (Exception e)
        {
            Add($"error: {e.Message}");
            logger.LogError(e, "extract failed to run");
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
}
