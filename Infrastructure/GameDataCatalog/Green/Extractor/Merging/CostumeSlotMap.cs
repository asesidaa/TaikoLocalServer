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

    public static IReadOnlyDictionary<uint, string> BuildIdMap(Don3dScanResult scan)
    {
        var result = new Dictionary<uint, string>();

        foreach (var pair in scan.DirectoryIds)
        {
            if (!DirectoryToCostumeType.TryGetValue(pair.Key, out var costumeType))
            {
                continue;
            }

            foreach (var id in pair.Value)
            {
                result.TryAdd(id, costumeType);
            }
        }

        return result;
    }
}
