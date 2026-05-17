using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Sources;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Merging;

public static class CostumeMerger
{
    public static IReadOnlyList<Costume> Merge(
        IEnumerable<NdpEntry> ndpEntries,
        Don3dScanResult don3d,
        GreenCatalogOverrides overrides)
    {
        var don3dIds = don3d.DirectoryIds.Values.SelectMany(ids => ids).ToHashSet();
        var idToCostumeType = CostumeSlotMap.BuildIdMap(don3d);

        return ndpEntries
            .Select(entry =>
            {
                overrides.Costumes.TryGetValue(entry.Id, out var itemOverride);
                var hasDon3d = don3dIds.Contains(entry.Id);
                var hasOverride = !string.IsNullOrWhiteSpace(itemOverride?.Name)
                                  || !string.IsNullOrWhiteSpace(itemOverride?.CostumeType);
                var source = hasDon3d ? "ndp+don3d" : "ndp";
                return new Costume
                {
                    CostumeId = entry.Id,
                    CostumeType = ResolveCostumeType(entry.Id, itemOverride, idToCostumeType),
                    CostumeName = itemOverride?.Name ?? string.Empty,
                    CostumeNameEN = itemOverride?.Name ?? string.Empty,
                    CostumeNameCN = itemOverride?.Name ?? string.Empty,
                    CostumeNameKO = itemOverride?.Name ?? string.Empty,
                    Source = hasOverride ? $"{source}+overrides" : source
                };
            })
            .GroupBy(costume => costume.CostumeId)
            .Select(group => group.First())
            .OrderBy(costume => costume.CostumeId)
            .ToArray();
    }

    private static string ResolveCostumeType(
        uint id,
        GreenCostumeOverride? itemOverride,
        IReadOnlyDictionary<uint, string> idToCostumeType)
    {
        if (!string.IsNullOrWhiteSpace(itemOverride?.CostumeType))
        {
            return itemOverride.CostumeType;
        }

        return idToCostumeType.TryGetValue(id, out var costumeType)
            ? costumeType
            : "unknown";
    }
}
