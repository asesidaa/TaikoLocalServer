using System.Text.Json;
using System.Text.RegularExpressions;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog;

/// <summary>
/// An older game's own data files (wwwroot/data/&lt;era&gt;/*.json: event folders, telop, movies, item shop, Don Challenge,
/// Taikojuku) for Server Management › Game data. Nijiiro's go through its catalog instead. Each era's catalog parses
/// its files as it loads, so saving one reloads that game, and puts the old file back when it doesn't load.
/// </summary>
public static partial class EraDataFiles
{
    // As the loaders read them (some allow comments and trailing commas).
    private static readonly JsonDocumentOptions Lenient = new() { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };

    // The costume, title and drum sound names AutoExtractCatalog writes: generated, not edited here.
    [GeneratedRegex(@"(costume|title|neiro)(_name)?_data\.json$")]
    private static partial Regex NameCache();

    public static IReadOnlyList<GameDataFileStatus> List(IGameDataCatalog catalog, GameEra era)
    {
        var dir = PathHelper.GetDataPath(era);
        if (!Directory.Exists(dir)) return [];
        var loadError = catalog.LoadResults.GetValueOrDefault(era)?.Error;
        return Directory.EnumerateFiles(dir, "*.json")
            .Select(Path.GetFileName).OfType<string>()
            .Where(name => !NameCache().IsMatch(name))
            .Order(StringComparer.Ordinal)
            .Select(name =>
            {
                var path = Path.Combine(dir, name);
                // The game's load error names the file it failed on; a file that isn't JSON at all is caught here.
                var error = loadError is not null && loadError.Contains(name, StringComparison.OrdinalIgnoreCase) ? loadError : SyntaxError(path);
                return new GameDataFileStatus(name, true, false, error, File.GetLastWriteTimeUtc(path));
            })
            .ToList();
    }

    public static async Task<string?> ReadAsync(GameEra era, string name) =>
        PathOf(era, name) is { } path && File.Exists(path) ? await File.ReadAllTextAsync(path) : null;

    /// <summary>Saves one file and reloads the game. Returns why it was refused, or null.</summary>
    public static async Task<string?> SaveAsync(IGameDataCatalog catalog, GameEra era, string name, string json, CancellationToken cancellationToken)
    {
        if (PathOf(era, name) is not { } path || !File.Exists(path)) return $"{name} isn't one of {era}'s data files.";
        try
        {
            using var _ = JsonDocument.Parse(json, Lenient);
        }
        catch (JsonException e)
        {
            return $"{name} isn't valid JSON: {e.Message}";
        }

        var backup = path + ".bak";
        File.Copy(path, backup, overwrite: true);
        var temp = path + ".tmp";
        await File.WriteAllTextAsync(temp, json, cancellationToken);
        File.Move(temp, path, overwrite: true);
        // Only an error this save caused puts the old file back: a game whose folder is missing still saves its files.
        var before = catalog.LoadResults.GetValueOrDefault(era)?.Error;
        if ((await catalog.ReloadAsync(era, cancellationToken)).Error is { } error && error != before)
        {
            File.Copy(backup, path, overwrite: true);
            await catalog.ReloadAsync(era, cancellationToken);
            return error;
        }

        return null;
    }

    // Only a file that's already in the game's folder: no paths, no new files.
    private static string? PathOf(GameEra era, string name)
    {
        if (name.Length == 0 || name != Path.GetFileName(name) || !name.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || NameCache().IsMatch(name))
            return null;
        return Path.Combine(PathHelper.GetDataPath(era), name);
    }

    private static string? SyntaxError(string path)
    {
        try
        {
            using var _ = JsonDocument.Parse(File.ReadAllText(path), Lenient);
            return null;
        }
        catch (Exception e) when (e is JsonException or IOException)
        {
            return e.Message;
        }
    }
}
