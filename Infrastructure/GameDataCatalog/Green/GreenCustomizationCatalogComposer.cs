namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public static class GreenCustomizationCatalogComposer
{
    private const string UnknownCostumeType = "unknown";
    private static readonly uint[] BaseNeiroIds = Enumerable.Range(0, 20).Select(id => (uint)id).ToArray();

    public static GreenCustomizationCatalog Compose(
        IReadOnlyList<Costume> greenCostumes,
        IReadOnlyDictionary<uint, Title> greenTitles,
        IReadOnlyDictionary<uint, Neiro> greenNeiros,
        IReadOnlyList<Costume>? nijiiroCostumes,
        IReadOnlyDictionary<uint, Title>? nijiiroTitles,
        IReadOnlyDictionary<uint, Neiro>? nijiiroNeiros)
    {
        var effectiveNijiiroCostumes = nijiiroCostumes ?? [];
        var effectiveNijiiroTitles = nijiiroTitles ?? new Dictionary<uint, Title>();
        var effectiveNijiiroNeiros = nijiiroNeiros ?? new Dictionary<uint, Neiro>();

        return new GreenCustomizationCatalog(
            ComposeCostumes(greenCostumes, effectiveNijiiroCostumes),
            ComposeTitles(greenTitles, effectiveNijiiroTitles),
            ComposeNeiros(greenNeiros, effectiveNijiiroNeiros));
    }

    private static IReadOnlyList<Costume> ComposeCostumes(
        IReadOnlyList<Costume> greenCostumes,
        IReadOnlyList<Costume> nijiiroCostumes)
    {
        if (nijiiroCostumes.Count == 0)
        {
            return greenCostumes;
        }

        var nijiiroByKey = nijiiroCostumes
            .GroupBy(costume => BuildCostumeKey(costume.CostumeType, costume.CostumeId))
            .ToDictionary(group => group.Key, group => group.First());
        var typedNijiiroIds = nijiiroCostumes
            .Where(costume => !IsUnknownCostumeType(costume.CostumeType))
            .Select(costume => costume.CostumeId)
            .ToHashSet();
        var result = new List<Costume>();
        var seen = new HashSet<CostumeKey>();

        foreach (var greenCostume in greenCostumes)
        {
            var key = BuildCostumeKey(greenCostume.CostumeType, greenCostume.CostumeId);
            if (!seen.Add(key))
            {
                continue;
            }

            if (IsUnknownCostumeType(greenCostume.CostumeType)
                && typedNijiiroIds.Contains(greenCostume.CostumeId))
            {
                continue;
            }

            result.Add(nijiiroByKey.TryGetValue(key, out var nijiiroCostume)
                ? MergeCostume(greenCostume, nijiiroCostume)
                : greenCostume);
        }

        return result;
    }

    private static IReadOnlyDictionary<uint, Title> ComposeTitles(
        IReadOnlyDictionary<uint, Title> greenTitles,
        IReadOnlyDictionary<uint, Title> nijiiroTitles)
    {
        if (nijiiroTitles.Count == 0)
        {
            return greenTitles;
        }

        return greenTitles
            .OrderBy(pair => pair.Key)
            .ToDictionary(
                pair => pair.Key,
                pair => nijiiroTitles.TryGetValue(pair.Key, out var nijiiroTitle)
                    ? MergeTitle(pair.Value, nijiiroTitle)
                    : pair.Value);
    }

    private static IReadOnlyDictionary<uint, Neiro> ComposeNeiros(
        IReadOnlyDictionary<uint, Neiro> greenNeiros,
        IReadOnlyDictionary<uint, Neiro> nijiiroNeiros)
    {
        if (nijiiroNeiros.Count == 0)
        {
            return greenNeiros;
        }

        var useBaseRange = greenNeiros.Count < BaseNeiroIds.Length
                           && BaseNeiroIds.All(nijiiroNeiros.ContainsKey);
        IEnumerable<uint> ids = useBaseRange
            ? BaseNeiroIds
            : greenNeiros.Keys.OrderBy(id => id);

        return ids.ToDictionary(
            id => id,
            id =>
            {
                greenNeiros.TryGetValue(id, out var greenNeiro);
                nijiiroNeiros.TryGetValue(id, out var nijiiroNeiro);
                return MergeNeiro(id, greenNeiro, nijiiroNeiro);
            });
    }

    private static Costume MergeCostume(Costume greenCostume, Costume nijiiroCostume)
        => new()
        {
            CostumeId = greenCostume.CostumeId,
            CostumeType = greenCostume.CostumeType,
            CostumeName = Prefer(nijiiroCostume.CostumeName, greenCostume.CostumeName),
            CostumeNameEN = Prefer(nijiiroCostume.CostumeNameEN, greenCostume.CostumeNameEN),
            CostumeNameCN = Prefer(nijiiroCostume.CostumeNameCN, greenCostume.CostumeNameCN),
            CostumeNameKO = Prefer(nijiiroCostume.CostumeNameKO, greenCostume.CostumeNameKO),
            Source = greenCostume.Source
        };

    private static Title MergeTitle(Title greenTitle, Title nijiiroTitle)
        => new()
        {
            TitleId = greenTitle.TitleId,
            TitleName = Prefer(nijiiroTitle.TitleName, greenTitle.TitleName),
            TitleNameEN = Prefer(nijiiroTitle.TitleNameEN, greenTitle.TitleNameEN),
            TitleNameCN = Prefer(nijiiroTitle.TitleNameCN, greenTitle.TitleNameCN),
            TitleNameKO = Prefer(nijiiroTitle.TitleNameKO, greenTitle.TitleNameKO),
            TitleRarity = nijiiroTitle.TitleRarity,
            Source = greenTitle.Source
        };

    private static Neiro MergeNeiro(uint id, Neiro? greenNeiro, Neiro? nijiiroNeiro)
        => new()
        {
            NeiroId = id,
            NeiroName = Prefer(nijiiroNeiro?.NeiroName, greenNeiro?.NeiroName),
            NeiroNameEN = Prefer(nijiiroNeiro?.NeiroNameEN, greenNeiro?.NeiroNameEN),
            NeiroNameCN = Prefer(nijiiroNeiro?.NeiroNameCN, greenNeiro?.NeiroNameCN),
            NeiroNameKO = Prefer(nijiiroNeiro?.NeiroNameKO, greenNeiro?.NeiroNameKO),
            Source = greenNeiro?.Source
        };

    private static string Prefer(string? preferred, string? fallback)
        => string.IsNullOrWhiteSpace(preferred) ? fallback ?? string.Empty : preferred;

    private static bool IsUnknownCostumeType(string costumeType)
        => string.Equals(costumeType, UnknownCostumeType, StringComparison.OrdinalIgnoreCase);

    private static CostumeKey BuildCostumeKey(string costumeType, uint costumeId)
        => new(costumeType.ToLowerInvariant(), costumeId);

    private readonly record struct CostumeKey(string CostumeType, uint CostumeId);
}

public sealed record GreenCustomizationCatalog(
    IReadOnlyList<Costume> Costumes,
    IReadOnlyDictionary<uint, Title> Titles,
    IReadOnlyDictionary<uint, Neiro> Neiros);
