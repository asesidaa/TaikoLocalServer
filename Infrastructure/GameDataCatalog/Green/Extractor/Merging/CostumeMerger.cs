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

        return ndpEntries
            .Select(entry =>
            {
                overrides.Costumes.TryGetValue(entry.Id, out var itemOverride);
                var hasDon3d = don3dIds.Contains(entry.Id);
                return new Costume
                {
                    CostumeId = entry.Id,
                    CostumeType = string.IsNullOrWhiteSpace(itemOverride?.CostumeType) ? "unknown" : itemOverride!.CostumeType!,
                    CostumeName = itemOverride?.Name ?? string.Empty,
                    CostumeNameEN = itemOverride?.Name ?? string.Empty,
                    CostumeNameCN = itemOverride?.Name ?? string.Empty,
                    CostumeNameKO = itemOverride?.Name ?? string.Empty,
                    Source = hasDon3d ? "ndp+don3d" : "ndp"
                };
            })
            .GroupBy(costume => costume.CostumeId)
            .Select(group => group.First())
            .OrderBy(costume => costume.CostumeId)
            .ToArray();
    }
}
