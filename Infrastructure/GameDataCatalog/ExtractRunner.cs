using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Infrastructure.Extract;
using TaikoLocalServer.Infrastructure.Extract.Ac15;
using TaikoLocalServer.Infrastructure.Extract.Nijiiro;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog;

/// <summary>
/// Runs the extractor (Infrastructure.Extract, the same code as <c>TaikoLocalServer extract</c>) for each game asked for,
/// one after another: Nijiiro with <c>--game GamePath --out wwwroot</c>, an older game with <c>--era &lt;game&gt;</c> as
/// well. Captures its report (each step's counts as it goes) and reloads that game's data when it's done. One run at a
/// time. With AllowRemoteGameDir, a game's folder can instead be the files an admin sent from their browser (only the
/// ones <c>extract --inputs</c> lists), kept in a temp folder in the install's layout until its run.
/// </summary>
public sealed partial class ExtractRunner(IOptions<ServerSettings> settings, IGameDataCatalog catalog, IServerControl server, ILogger<ExtractRunner> logger) : IExtractRunner
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

    private static readonly string UploadRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "TaikoLocalServer-game-upload"));
    private static string UploadPath(GameEra game) => Path.Combine(UploadRoot, game.ToString().ToLowerInvariant());
    private bool RemoteAllowed => settings.Value.AllowRemoteGameDir;

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
                return new ExtractStatus(RemoteAllowed, games, run);
            }
        }
    }

    public string? Start(IReadOnlyList<GameEra> games, bool withMods, IReadOnlySet<GameEra> uploaded)
    {
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

    public Task<(ExtractInputs? Inputs, string? Error)> InputsAsync(GameEra game, CancellationToken cancellationToken)
    {
        if (!RemoteAllowed) return Task.FromResult<(ExtractInputs?, string?)>((null, "Sending game folders from the browser is off (AllowRemoteGameDir)."));
        var list = inputs.GetOrAdd(game, g =>
        {
            var json = g == GameEra.Nijiiro ? NijiiroExtractor.Inputs() : Ac15Extractor.Inputs(EraOf(g));
            return json.Deserialize<ExtractInputs>(JsonSerializerOptions.Web)!;
        });
        return Task.FromResult<(ExtractInputs?, string?)>((list, null));
    }

    public FolderListing Browse(GameEra game, string? path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                // The drives on Windows; elsewhere the file system's top.
                if (!OperatingSystem.IsWindows()) return Browse(game, "/");
                var drives = DriveInfo.GetDrives().Where(d => d.IsReady).Select(d => new FolderEntry(d.Name, d.Name, false)).ToList();
                return new FolderListing(null, null, false, drives);
            }
            var full = Path.GetFullPath(path);
            if (!Directory.Exists(full)) return new FolderListing(full, null, false, [], "That folder isn't there.");
            var entries = new DirectoryInfo(full).EnumerateDirectories()
                .Where(d => (d.Attributes & (FileAttributes.Hidden | FileAttributes.System)) == 0)
                .Select(d => new FolderEntry(d.Name, d.FullName, LooksLike(game, d.FullName)))
                .Concat(game == GameEra.Nijiiro ? [] : new DirectoryInfo(full).EnumerateFiles("*.zip").Select(f => new FolderEntry(f.Name, f.FullName, true, true)))
                .OrderBy(e => e.IsFile).ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            return new FolderListing(full, Directory.GetParent(full)?.FullName, LooksLike(game, full), entries);
        }
        catch (Exception e) when (e is UnauthorizedAccessException or IOException)
        {
            return new FolderListing(path, path is null ? null : Directory.GetParent(path)?.FullName, false, [], e.Message);
        }
    }

    // Nijiiro's install has Data/x64/datatable, or it's that Data/x64 itself (as extract takes it). An older game's folder
    // is (or holds) its USRDIR/data, which has fumen (the places GameSource.Open looks), or is a .zip of it (not looked into).
    private static bool LooksLike(GameEra game, string path)
    {
        try
        {
            if (game == GameEra.Nijiiro)
                return Directory.Exists(Path.Combine(path, "Data", "x64", "datatable")) || Directory.Exists(Path.Combine(path, "datatable"));
            if (File.Exists(path)) return path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
            return new[] { "", "data", "USRDIR/data", "dev_hdd0/game/SCEEXE000/USRDIR/data" }
                .Select(rel => Path.Combine(path, rel))
                .FirstOrDefault(data => Directory.Exists(Path.Combine(data, "fumen"))) is { } found && OlderGameOf(found) == game;
        }
        catch (Exception e) when (e is UnauthorizedAccessException or IOException)
        {
            return false;
        }
    }

    // Which older game a USRDIR/data is. Each keeps the older games' config folders too, so it's the newest config there
    // (Red's dump has only ST8100-7, so any variant counts). Kimidori and Momoiro have none: their PARAM.SFO (next to
    // USRDIR) is titled "…(ST51)" and "…(ST41)".
    private static readonly (GameEra Game, string Config)[] ConfigsNewestFirst =
        [(GameEra.Green, "S11100"), (GameEra.Blue, "S10100"), (GameEra.Yellow, "ST9100"), (GameEra.Red, "ST8100"), (GameEra.White, "ST7100"), (GameEra.Murasaki, "ST6100")];

    public static GameEra? OlderGameOf(string data)
    {
        var config = Path.Combine(data, "config");
        var names = Directory.Exists(config) ? Directory.EnumerateDirectories(config).Select(Path.GetFileName).ToList() : [];
        foreach (var (game, id) in ConfigsNewestFirst)
            if (names.Any(n => n!.StartsWith(id + "-", StringComparison.OrdinalIgnoreCase)))
                return game;
        var sfo = Path.Combine(data, "..", "..", "PARAM.SFO");
        var title = File.Exists(sfo) ? Encoding.ASCII.GetString(File.ReadAllBytes(sfo)) : "";
        return title.Contains("(ST51)") ? GameEra.Kimidori : title.Contains("(ST41)") ? GameEra.Momoiro : null;
    }

    public async Task<string?> SetGamePathAsync(GameEra game, string path, CancellationToken cancellationToken)
    {
        if (!Enabled.Contains(game)) return $"{game} is off in ServerSettings.json.";
        var full = Path.GetFullPath(path);
        if (!LooksLike(game, full))
            return game == GameEra.Nijiiro
                ? "That folder isn't Nijiiro's game folder: there's no Data/x64/datatable in it."
                : $"That folder isn't {game}'s game folder: there's no USRDIR/data in it for {game}.";
        lock (gate)
            if (running) return "extract is running.";

        // Now, for the next extract; and in the file, for the next start.
        var era = settings.Value.Eras.TryGetValue(game.ToString(), out var e) ? e : settings.Value.Eras[game.ToString()] = new EraSettings { Enabled = true };
        if (game == GameEra.Nijiiro) era.GameDataPath = full; else era.GamePath = full;
        await server.UpdateSettingsAsync(s =>
        {
            if (s["Eras"] is not System.Text.Json.Nodes.JsonObject eras) s["Eras"] = eras = [];
            if (eras[game.ToString()] is not System.Text.Json.Nodes.JsonObject one) eras[game.ToString()] = one = new System.Text.Json.Nodes.JsonObject { ["Enabled"] = true };
            one[game == GameEra.Nijiiro ? "GameDataPath" : "GamePath"] = full;
        }, cancellationToken);
        logger.LogInformation("{Game} is read from {Path} now ({Setting})", game, full, SettingOf(game));
        return null;
    }

    private static Era EraOf(GameEra game) =>
        EraInfo.TryParse(game.ToString().ToLowerInvariant(), out var era) ? era : throw new ArgumentOutOfRangeException(nameof(game), game, null);

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
        var args = new List<string>();
        if (game != GameEra.Nijiiro) args.AddRange(["--era", game.ToString().ToLowerInvariant()]);
        args.AddRange(["--game", uploaded ? UploadPath(game) : GamePath(game)]);
        if (withMods && game == GameEra.Nijiiro) args.Add("--mods");

        var code = -1;
        try
        {
            using var report = new LineWriter(line => Add(line, game));
            code = await Task.Run(() => ExtractCli.Run([.. args], PathHelper.GetRootPath(), report, report));
            logger.LogInformation("extract {Game} finished with exit code {ExitCode}", game, code);
            await catalog.ReloadAsync(game);
        }
        catch (Exception e)
        {
            Add($"error: {e.Message}", game);
            logger.LogError(e, "extract {Game} failed", game);
        }
        finally
        {
            // The sent files are only needed for this run (Nijiiro's are about 600 MB).
            if (uploaded) DeleteUpload(game);
        }
        return code;
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
