namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Merging;

internal static class CostumeSlotMap
{
    public static readonly IReadOnlyDictionary<string, string> DirectoryToCostumeType =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["don3d/full/cos"] = "kigurumi",
            ["don3d/parts/head"] = "head",
            ["don3d/parts/body"] = "body",
            ["don3d/parts/paint"] = "face",
            ["don3d/parts/acc"] = "puchi"
        };

    public static IReadOnlyDictionary<string, IReadOnlyList<uint>> BuildTypeIdMap(Don3dScanResult scan)
    {
        var result = DirectoryToCostumeType.Values
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                costumeType => costumeType,
                _ => new SortedSet<uint>(),
                StringComparer.OrdinalIgnoreCase);

        foreach (var pair in scan.DirectoryIds)
        {
            if (!TryResolveCostumeType(pair.Key, out var costumeType))
            {
                continue;
            }

            foreach (var id in pair.Value)
            {
                result[costumeType].Add(id);
            }
        }

        return result.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyList<uint>)pair.Value.ToArray(),
            StringComparer.OrdinalIgnoreCase);
    }

    private static bool TryResolveCostumeType(string directory, out string costumeType)
    {
        var normalizedDirectory = directory.Replace('\\', '/');
        foreach (var pair in DirectoryToCostumeType)
        {
            if (string.Equals(normalizedDirectory, pair.Key, StringComparison.OrdinalIgnoreCase)
                || normalizedDirectory.StartsWith($"{pair.Key}/", StringComparison.OrdinalIgnoreCase))
            {
                costumeType = pair.Value;
                return true;
            }
        }

        costumeType = string.Empty;
        return false;
    }
}
