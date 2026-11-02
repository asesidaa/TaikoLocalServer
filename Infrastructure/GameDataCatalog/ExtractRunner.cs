using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Contracts.AdminApi.Responses;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog;

/// <summary>
/// Runs <c>extract(.exe)</c> from next to the server exe for each game asked for, one after another: Nijiiro with
/// <c>--game GamePath --out wwwroot</c>, an older game with <c>--era &lt;game&gt;</c> as well. Captures its report
/// (each step's counts as it goes) and reloads that game's data when it's done. One run at a time. With
/// AllowRemoteGameDir, a game's folder can instead be the files an admin sent from their browser (only the ones
/// <c>extract --inputs</c> lists), kept in a temp folder in the install's layout until its run.
/// </summary>
public sealed partial class ExtractRunner(IOptions<ServerSettings> settings, IGameDataCatalog catalog, ILogger<ExtractRunner> logger) : IExtractRunner
{
    private const int MaxLines = 2000;

    private readonly Lock gate = new();
    private readonly List<string> output = [];
    private readonly List<ExtractStep> steps = [];
    private readonly Dictionary<string, int> exitCodes = [];
    private readonly ConcurrentDictionary<GameEra, ExtractInputs> inputs = new();
    private IReadOnlyList<GameEra> runGames = [];
    private GameEra? current;
    private bool running, mods;
    private DateTime? startedAt, finishedAt;

    private static string ExePath => Path.Combine(AppContext.BaseDirectory, OperatingSystem.IsWindows() ? "extract.exe" : "extract");
    private static readonly string UploadRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "TaikoLocalServer-game-upload"));
    private static string UploadPath(GameEra game) => Path.Combine(UploadRoot, game.ToString().ToLowerInvariant());
    private bool RemoteAllowed => settings.Value.AllowRemoteGameDir && File.Exists(ExePath);

    // The games that are on, in GameEra's order (Nijiiro first).
    private IReadOnlyList<GameEra> Enabled => catalog.LoadResults.Keys.Order().ToList();

    // Nijiiro's install is Eras.Nijiiro.GameDataPath (the other games' GameDataPath is where the server reads them);
    // an older game's is Eras.<Game>.GamePath. A relative path is from the server's folder.
    private static string SettingOf(GameEra game) => game == GameEra.Nijiiro ? "Eras.Nijiiro.GameDataPath" : $"Eras.{game}.GamePath";

    private string GamePath(GameEra game) =>
        settings.Value.Eras.GetValueOrDefault(game.ToString()) is { } era
        && (game == GameEra.Nijiiro ? era.GameDataPath : era.GamePath)?.Trim() is { Length: > 0 } path
            ? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, path))
            : "";

    // A folder, or an older game's .zip.
    private static bool Found(string path) => path.Length > 0 && (Directory.Exists(path) || File.Exists(path));

    private static string ManifestPath(GameEra game) =>
        Path.Combine(game == GameEra.Nijiiro ? PathHelper.GetDataPath() : PathHelper.GetDataPath(game), "extract.json");

    private static bool Extracted(GameEra game) => game == GameEra.Nijiiro
        ? File.Exists(Path.Combine(PathHelper.GetDataPath(), "datatable", "musicinfo.bin"))
        : Directory.Exists(Path.Combine(PathHelper.GetDataPath(game), "data"));

    // What the last extract of the game wrote; null before an extract that writes extract.json.
    private ExtractManifest? Manifest(GameEra game)
    {
        var path = ManifestPath(game);
        try
        {
            return File.Exists(path) ? JsonSerializer.Deserialize<ExtractManifest>(File.ReadAllText(path), JsonSerializerOptions.Web) : null;
        }
        catch (Exception e) when (e is IOException or JsonException)
        {
            logger.LogWarning(e, "Couldn't read {Path}", path);
            return null;
        }
    }

    public ExtractStatus Status
    {
        get
        {
            var games = Enabled.Select(g => new ExtractGame(g.ToString(), SettingOf(g), GamePath(g), Found(GamePath(g)), Extracted(g), Manifest(g))).ToList();
            lock (gate)
            {
                var run = startedAt is { } started
                    ? new ExtractRun(runGames.Select(g => g.ToString()).ToList(), current?.ToString(), running, mods, started, finishedAt,
                        steps.ToList(), new Dictionary<string, int>(exitCodes), output.ToList())
                    : null;
                return new ExtractStatus(File.Exists(ExePath), RemoteAllowed, games, run);
            }
        }
    }

    public string? Start(IReadOnlyList<GameEra> games, bool withMods, IReadOnlySet<GameEra> uploaded)
    {
        if (!File.Exists(ExePath)) return "extract isn't next to the server exe.";
        if (games.Count == 0) return "Pick a game to extract.";
        if (uploaded.Count > 0 && !RemoteAllowed) return "Sending game folders from the browser is off (AllowRemoteGameDir).";
        foreach (var game in games)
        {
            if (!Enabled.Contains(game)) return $"{game} is off in ServerSettings.json.";
            if (uploaded.Contains(game) ? !Directory.Exists(UploadPath(game)) : !Found(GamePath(game)))
                return uploaded.Contains(game) ? $"No folder was sent for {game}." : $"{SettingOf(game)} isn't set, or isn't there.";
        }

        lock (gate)
        {
            if (running) return "extract is already running.";
            running = true;
            mods = withMods;
            runGames = games.Distinct().ToList();
            startedAt = DateTime.UtcNow;
            finishedAt = null;
            current = null;
            exitCodes.Clear();
            steps.Clear();
            output.Clear();
        }
        _ = Task.Run(() => RunAllAsync(withMods, uploaded));
        return null;
    }

    public async Task<(ExtractInputs? Inputs, string? Error)> InputsAsync(GameEra game, CancellationToken cancellationToken)
    {
        if (!RemoteAllowed) return (null, "Sending game folders from the browser is off (AllowRemoteGameDir).");
        if (inputs.TryGetValue(game, out var cached)) return (cached, null);
        var info = new ProcessStartInfo(ExePath) { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
        info.ArgumentList.Add("--inputs");
        if (game != GameEra.Nijiiro)
        {
            info.ArgumentList.Add("--era");
            info.ArgumentList.Add(game.ToString().ToLowerInvariant());
        }
        using var process = Process.Start(info)!;
        var json = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0) return (null, "This extract is too old to list its inputs; update it.");
        var list = JsonSerializer.Deserialize<ExtractInputs>(json, JsonSerializerOptions.Web)!;
        inputs[game] = list;
        return (list, null);
    }

    public string? ClearUpload(GameEra game)
    {
        if (!RemoteAllowed) return "Sending game folders from the browser is off (AllowRemoteGameDir).";
        lock (gate)
        {
            if (running) return "extract is running.";
            DeleteUpload(game);
        }
        return null;
    }

    public async Task<string?> SaveUploadAsync(GameEra game, string path, Stream content, CancellationToken cancellationToken)
    {
        var (list, error) = await InputsAsync(game, cancellationToken);
        if (list is null) return error;
        if (Match(list, path) is not { } namesOnly) return $"{path} isn't a file extract reads.";
        lock (gate)
            if (running) return "extract is running.";

        var root = UploadPath(game);
        var full = Path.GetFullPath(Path.Combine(root, path));
        if (!full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal)) return $"{path} isn't a file extract reads.";
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        await using var file = File.Create(full);
        // Movies, models' and names' folders: extract only lists their names.
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
            if (set.Files.Any(rule => Covers(rule, path, false))) return false;
            if (set.Names.Any(rule => Covers(rule, path, true))) return true;
        }
        return null;
    }

    // A file rule, a folder rule (ending in /: everything under it, or with namesOnly only its direct children). A *
    // segment matches any one folder name (the older games' nutdata/<…>/title_name/ folders). Mirrored in gamefolder.js.
    private static bool Covers(string rule, string path, bool namesOnly)
    {
        string[] r = rule.Split('/'), p = path.Split('/');
        var folder = rule.EndsWith('/');
        if (namesOnly || !folder ? p.Length != r.Length : p.Length < r.Length) return false;
        for (var i = 0; i < (folder ? r.Length - 1 : r.Length); i++)
            if (r[i] != "*" && r[i] != p[i]) return false;
        return true;
    }

    /// <summary>
    /// A step's line in extract's report ("red server files: 1440 written, 0 already up to date, 0 missing, 0 failed";
    /// an older game's labels start with its name, which is dropped, as extract.json has them), or null for any other.
    /// </summary>
    public static ExtractStep? ParseStep(string line, GameEra game)
    {
        if (StepLine().Match(line) is not { Success: true } m) return null;
        var label = m.Groups["label"].Value;
        var prefix = game.ToString().ToLowerInvariant() + " ";
        if (label.StartsWith(prefix, StringComparison.Ordinal)) label = label[prefix.Length..];
        return new ExtractStep(label, int.Parse(m.Groups["w"].Value), int.Parse(m.Groups["u"].Value),
            int.Parse(m.Groups["m"].Value), int.Parse(m.Groups["f"].Value));
    }

    [GeneratedRegex(@"^(?<label>\S.*?): (?<w>\d+) written, (?<u>\d+) already up to date, (?<m>\d+) missing, (?<f>\d+) failed$")]
    private static partial Regex StepLine();

    private async Task RunAllAsync(bool withMods, IReadOnlySet<GameEra> uploaded)
    {
        try
        {
            foreach (var game in runGames)
            {
                lock (gate)
                {
                    current = game;
                    steps.Clear();
                }
                var code = await RunOneAsync(game, withMods, uploaded.Contains(game));
                lock (gate) exitCodes[game.ToString()] = code;
            }
        }
        finally
        {
            lock (gate)
            {
                running = false;
                current = null;
                finishedAt = DateTime.UtcNow;
            }
        }
    }

    private async Task<int> RunOneAsync(GameEra game, bool withMods, bool uploaded)
    {
        var info = new ProcessStartInfo(ExePath)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        if (game != GameEra.Nijiiro)
        {
            info.ArgumentList.Add("--era");
            info.ArgumentList.Add(game.ToString().ToLowerInvariant());
        }
        info.ArgumentList.Add("--game");
        info.ArgumentList.Add(uploaded ? UploadPath(game) : GamePath(game));
        info.ArgumentList.Add("--out");
        info.ArgumentList.Add(PathHelper.GetRootPath());
        if (withMods && game == GameEra.Nijiiro) info.ArgumentList.Add("--mods");

        var code = -1;
        try
        {
            using var process = new Process { StartInfo = info };
            process.OutputDataReceived += (_, e) => Add(e.Data, game);
            process.ErrorDataReceived += (_, e) => Add(e.Data, game);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
            code = process.ExitCode;
            logger.LogInformation("extract {Game} finished with exit code {ExitCode}", game, code);
            await catalog.ReloadAsync(game);
        }
        catch (Exception e)
        {
            Add($"error: {e.Message}", game);
            logger.LogError(e, "extract {Game} failed to run", game);
        }
        finally
        {
            // The sent files are only needed for this run (Nijiiro's are about 600 MB).
            if (uploaded) DeleteUpload(game);
        }
        return code;
    }

    private void DeleteUpload(GameEra game)
    {
        try
        {
            if (Directory.Exists(UploadPath(game))) Directory.Delete(UploadPath(game), true);
        }
        catch (IOException e)
        {
            logger.LogWarning(e, "Couldn't remove the uploaded {Game} files in {Path}", game, UploadPath(game));
        }
    }

    private void Add(string? line, GameEra game)
    {
        if (line is null) return;
        lock (gate)
        {
            if (output.Count < MaxLines) output.Add(line);
            if (ParseStep(line, game) is { } step) steps.Add(step);
        }
    }
}
