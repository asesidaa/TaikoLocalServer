using System.Text.RegularExpressions;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

public sealed record Don3dScanResult(
    IReadOnlyList<uint> FullCosModelPairIds,
    IReadOnlyDictionary<string, IReadOnlyList<uint>> DirectoryIds);

public static partial class Don3dDirScanner
{
    private static readonly string[] KnownCostumeDirectories =
    [
        Path.Combine("don3d", "cos"),
        Path.Combine("don3d", "face"),
        Path.Combine("don3d", "parts", "acc"),
        Path.Combine("don3d", "parts", "body"),
        Path.Combine("don3d", "parts", "head"),
        Path.Combine("don3d", "parts", "paint"),
        Path.Combine("don3d", "full", "cos"),
        Path.Combine("don3d", "full", "face")
    ];

    public static Don3dScanResult Scan(string gameDataRoot)
    {
        var byDirectory = new Dictionary<string, IReadOnlyList<uint>>(StringComparer.OrdinalIgnoreCase);
        var don3dRoot = Path.Combine(gameDataRoot, "don3d");

        if (Directory.Exists(don3dRoot))
        {
            var directories = Directory.EnumerateDirectories(don3dRoot, "*", SearchOption.AllDirectories)
                .Prepend(don3dRoot)
                .Order(StringComparer.OrdinalIgnoreCase);

            foreach (var absolute in directories)
            {
                var ids = ScanDirectoryForPairedIds(absolute);
                if (ids.Count == 0)
                {
                    continue;
                }

                byDirectory[ToDon3dRelativePath(don3dRoot, absolute)] = ids;
            }
        }

        foreach (var relative in KnownCostumeDirectories)
        {
            byDirectory.TryAdd(relative.Replace('\\', '/'), []);
        }

        var fullCosIds = byDirectory
            .Where(pair => IsUnder(pair.Key, "don3d/cos") || IsUnder(pair.Key, "don3d/full/cos"))
            .SelectMany(pair => pair.Value)
            .Distinct()
            .Order()
            .ToArray();
        return new Don3dScanResult(fullCosIds, byDirectory);
    }

    private static IReadOnlyList<uint> ScanDirectoryForPairedIds(string directory)
    {
        var directoryId = ParseLastNumber(Path.GetFileName(directory));
        var nudIds = Directory.EnumerateFiles(directory, "*.nud", SearchOption.TopDirectoryOnly)
            .Select(path => ParseFileOrDirectoryId(path, directoryId))
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

        var nutIds = Directory.EnumerateFiles(directory, "*.nut", SearchOption.TopDirectoryOnly)
            .Select(path => ParseFileOrDirectoryId(path, directoryId))
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

        return nudIds.Intersect(nutIds).OrderBy(id => id).ToArray();
    }

    private static uint? ParseFileOrDirectoryId(string path, uint? directoryId)
        => ParseLastNumber(Path.GetFileNameWithoutExtension(path)) ?? directoryId;

    private static uint? ParseLastNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var match = NumberRegex().Matches(value).LastOrDefault();
        return match is null ? null : uint.Parse(match.Value);
    }

    private static string ToDon3dRelativePath(string don3dRoot, string absoluteDirectory)
    {
        var relative = Path.GetRelativePath(don3dRoot, absoluteDirectory).Replace('\\', '/');
        return relative == "."
            ? "don3d"
            : $"don3d/{relative}";
    }

    private static bool IsUnder(string path, string root)
    {
        var normalizedPath = path.Replace('\\', '/');
        var normalizedRoot = root.Replace('\\', '/');
        return string.Equals(normalizedPath, normalizedRoot, StringComparison.OrdinalIgnoreCase)
               || normalizedPath.StartsWith($"{normalizedRoot}/", StringComparison.OrdinalIgnoreCase);
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex NumberRegex();
}
