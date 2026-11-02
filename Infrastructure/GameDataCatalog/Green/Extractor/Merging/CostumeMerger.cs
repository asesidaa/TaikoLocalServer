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
        var ndpIds = ndpEntries.Select(entry => entry.Id).ToHashSet();
        var don3dIds = don3d.DirectoryIds.Values.SelectMany(ids => ids).ToHashSet();
        var typeToIds = CostumeSlotMap.BuildTypeIdMap(don3d);
        var typedDon3dIds = typeToIds.Values.SelectMany(ids => ids).ToHashSet();
        var costumes = new List<Costume>();

        foreach (var pair in typeToIds)
        {
            foreach (var id in pair.Value)
            {
                costumes.Add(BuildCostume(
                    id,
                    pair.Key,
                    ndpIds.Contains(id) ? "ndp+don3d" : "don3d",
                    overrides));
            }
        }

        foreach (var id in ndpIds.Where(id => !typedDon3dIds.Contains(id)))
        {
            costumes.Add(BuildCostume(
                id,
                "unknown",
                don3dIds.Contains(id) ? "ndp+don3d" : "ndp",
                overrides));
        }

        return costumes
            .GroupBy(costume => (costume.CostumeId, costume.CostumeType))
            .Select(group => group.First())
            .OrderBy(costume => costume.CostumeId)
            .ThenBy(costume => CostumeTypeSortKey(costume.CostumeType))
            .ThenBy(costume => costume.CostumeType, StringComparer.Ordinal)
            .ToArray();
    }

    private static Costume BuildCostume(
        uint id,
        string costumeType,
        string source,
        GreenCatalogOverrides overrides)
    {
        overrides.Costumes.TryGetValue(id, out var itemOverride);
        var resolvedType = ResolveCostumeType(costumeType, itemOverride);
        var hasOverride = !string.IsNullOrWhiteSpace(itemOverride?.Name)
                          || !string.IsNullOrWhiteSpace(itemOverride?.CostumeType);

        return new Costume
        {
            CostumeId = id,
            CostumeType = resolvedType,
            CostumeName = itemOverride?.Name ?? string.Empty,
            CostumeNameEN = itemOverride?.Name ?? string.Empty,
            CostumeNameCN = itemOverride?.Name ?? string.Empty,
            CostumeNameKO = itemOverride?.Name ?? string.Empty,
            Source = hasOverride ? $"{source}+overrides" : source
        };
    }

    private static string ResolveCostumeType(
        string costumeType,
        GreenCostumeOverride? itemOverride)
    {
        if (!string.IsNullOrWhiteSpace(itemOverride?.CostumeType))
        {
            return itemOverride.CostumeType;
        }

        return costumeType;
    }

    private static int CostumeTypeSortKey(string costumeType)
        => costumeType switch
        {
            "kigurumi" => 0,
            "head" => 1,
            "body" => 2,
            "face" => 3,
            "puchi" => 4,
            "unknown" => 5,
            _ => 6
        };
}
