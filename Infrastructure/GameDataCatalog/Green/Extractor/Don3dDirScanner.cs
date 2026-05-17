using System.Text.RegularExpressions;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

public sealed record Don3dScanResult(
    IReadOnlyList<uint> FullCosModelPairIds,
    IReadOnlyDictionary<string, IReadOnlyList<uint>> DirectoryIds);

public static partial class Don3dDirScanner
{
    private static readonly string[] CostumeDirectories =
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

        foreach (var relative in CostumeDirectories)
        {
            var absolute = Path.Combine(gameDataRoot, relative);
            byDirectory[relative.Replace('\\', '/')] = Directory.Exists(absolute)
                ? ScanDirectoryForPairedIds(absolute)
                : [];
        }

        byDirectory.TryGetValue("don3d/cos", out var fullCosIds);
        return new Don3dScanResult(fullCosIds ?? [], byDirectory);
    }

    private static IReadOnlyList<uint> ScanDirectoryForPairedIds(string directory)
    {
        var nudIds = Directory.EnumerateFiles(directory, "*.nud", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileNameWithoutExtension)
            .Select(ParseLastNumber)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

        var nutIds = Directory.EnumerateFiles(directory, "*.nut", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileNameWithoutExtension)
            .Select(ParseLastNumber)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

        return nudIds.Intersect(nutIds).OrderBy(id => id).ToArray();
    }

    private static uint? ParseLastNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var match = NumberRegex().Matches(value).LastOrDefault();
        return match is null ? null : uint.Parse(match.Value);
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex NumberRegex();
}
