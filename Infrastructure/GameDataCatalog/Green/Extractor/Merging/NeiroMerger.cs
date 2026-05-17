using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Sources;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Merging;

public static class NeiroMerger
{
    public static IReadOnlyList<Neiro> Merge(
        IEnumerable<NdpEntry> ndpEntries,
        GreenCatalogOverrides overrides)
    {
        return ndpEntries
            .Select(entry =>
            {
                overrides.Neiros.TryGetValue(entry.Id, out var itemOverride);
                return new Neiro
                {
                    NeiroId = entry.Id,
                    NeiroName = itemOverride?.Name ?? string.Empty,
                    NeiroNameEN = itemOverride?.Name ?? string.Empty,
                    NeiroNameCN = itemOverride?.Name ?? string.Empty,
                    NeiroNameKO = itemOverride?.Name ?? string.Empty,
                    Source = "ndp"
                };
            })
            .GroupBy(neiro => neiro.NeiroId)
            .Select(group => group.First())
            .OrderBy(neiro => neiro.NeiroId)
            .ToArray();
    }
}
