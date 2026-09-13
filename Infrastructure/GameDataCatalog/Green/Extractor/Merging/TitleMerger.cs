using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Sources;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Merging;

public static class TitleMerger
{
    public static IReadOnlyList<Title> Merge(
        IEnumerable<NdpEntry> ndpEntries,
        IEnumerable<uint> rewardTitleIds,
        GreenCatalogOverrides overrides)
        => Merge(ndpEntries, ndpEntries.Select(entry => entry.Id), rewardTitleIds, overrides);

    public static IReadOnlyList<Title> Merge(
        IEnumerable<NdpEntry> ndpEntries,
        IEnumerable<uint> discoveredTitleIds,
        IEnumerable<uint> rewardTitleIds,
        GreenCatalogOverrides overrides)
    {
        var ndpIds = ndpEntries
            .Select(entry => entry.Id)
            .ToHashSet();
        var rewardSet = rewardTitleIds.ToHashSet();
        var allIds = ndpIds
            .Concat(discoveredTitleIds)
            .Distinct()
            .Order()
            .ToArray();

        return allIds
            .Select(id =>
            {
                overrides.Titles.TryGetValue(id, out var itemOverride);
                var hasOverride = !string.IsNullOrWhiteSpace(itemOverride?.Name);
                var source = ndpIds.Contains(id) ? "ndp" : "nut";
                if (rewardSet.Contains(id))
                {
                    source += "+rewardtitlefiltering";
                }

                return new Title
                {
                    TitleId = id,
                    TitleName = itemOverride?.Name ?? string.Empty,
                    TitleNameEN = itemOverride?.Name ?? string.Empty,
                    TitleNameCN = itemOverride?.Name ?? string.Empty,
                    TitleNameKO = itemOverride?.Name ?? string.Empty,
                    TitleRarity = 0,
                    Source = hasOverride ? $"{source}+overrides" : source
                };
            })
            .ToArray();
    }

}
