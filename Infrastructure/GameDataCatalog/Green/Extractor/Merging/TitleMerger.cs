using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Sources;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Merging;

public static class TitleMerger
{
    public static IReadOnlyList<Title> Merge(
        IEnumerable<NdpEntry> ndpEntries,
        IEnumerable<uint> rewardTitleIds,
        GreenCatalogOverrides overrides)
    {
        var rewardSet = rewardTitleIds.ToHashSet();

        return ndpEntries
            .Select(entry =>
            {
                overrides.Titles.TryGetValue(entry.Id, out var itemOverride);
                return new Title
                {
                    TitleId = entry.Id,
                    TitleName = itemOverride?.Name ?? string.Empty,
                    TitleNameEN = itemOverride?.Name ?? string.Empty,
                    TitleNameCN = itemOverride?.Name ?? string.Empty,
                    TitleNameKO = itemOverride?.Name ?? string.Empty,
                    TitleRarity = 0,
                    Source = rewardSet.Contains(entry.Id) ? "ndp+rewardtitlefiltering" : "ndp"
                };
            })
            .GroupBy(title => title.TitleId)
            .Select(group => group.First())
            .OrderBy(title => title.TitleId)
            .ToArray();
    }
}
