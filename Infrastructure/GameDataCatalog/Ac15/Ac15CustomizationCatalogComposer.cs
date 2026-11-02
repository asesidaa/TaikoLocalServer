namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

public static class Ac15CustomizationCatalogComposer
{
    private const string UnknownCostumeType = "unknown";
    private static readonly uint[] BaseNeiroIds = Enumerable.Range(0, 20).Select(id => (uint)id).ToArray();

    public static Ac15CustomizationCatalog Compose(
        IReadOnlyList<Costume> eraCostumes,
        IReadOnlyDictionary<uint, Title> eraTitles,
        IReadOnlyDictionary<uint, Neiro> eraNeiros,
        IReadOnlyList<Costume>? sharedCostumes,
        IReadOnlyDictionary<uint, Title>? sharedTitles,
        IReadOnlyDictionary<uint, Neiro>? sharedNeiros,
        IReadOnlyList<Costume>? fallbackCostumes = null,
        IReadOnlyDictionary<uint, Title>? fallbackTitles = null,
        IReadOnlyDictionary<uint, Neiro>? fallbackNeiros = null)
    {
        var effectiveSharedCostumes = sharedCostumes ?? [];
        var effectiveSharedTitles = sharedTitles ?? new Dictionary<uint, Title>();
        var effectiveSharedNeiros = sharedNeiros ?? new Dictionary<uint, Neiro>();
        var effectiveFallbackCostumes = fallbackCostumes ?? [];
        var effectiveFallbackTitles = fallbackTitles ?? new Dictionary<uint, Title>();
        var effectiveFallbackNeiros = fallbackNeiros ?? new Dictionary<uint, Neiro>();
        var nameCostumes = MergeCostumeNameSources(effectiveFallbackCostumes, effectiveSharedCostumes);
        var nameTitles = MergeNameSources(effectiveFallbackTitles, effectiveSharedTitles);
        var nameNeiros = MergeNameSources(effectiveFallbackNeiros, effectiveSharedNeiros);

        return new Ac15CustomizationCatalog(
            ComposeCostumes(eraCostumes, nameCostumes),
            ComposeTitles(eraTitles, nameTitles),
            ComposeNeiros(eraNeiros, nameNeiros));
    }

    private static IReadOnlyList<Costume> ComposeCostumes(
        IReadOnlyList<Costume> eraCostumes,
        IReadOnlyList<Costume> nameCostumes)
    {
        if (nameCostumes.Count == 0)
        {
            return eraCostumes;
        }

        var namesByKey = nameCostumes
            .GroupBy(costume => BuildCostumeKey(costume.CostumeType, costume.CostumeId))
            .ToDictionary(group => group.Key, group => group.First());
        var typedNameIds = nameCostumes
            .Where(costume => !IsUnknownCostumeType(costume.CostumeType))
            .Select(costume => costume.CostumeId)
            .ToHashSet();
        var result = new List<Costume>();
        var seen = new HashSet<CostumeKey>();

        foreach (var eraCostume in eraCostumes)
        {
            var key = BuildCostumeKey(eraCostume.CostumeType, eraCostume.CostumeId);
            if (!seen.Add(key))
            {
                continue;
            }

            if (IsUnknownCostumeType(eraCostume.CostumeType)
                && typedNameIds.Contains(eraCostume.CostumeId))
            {
                continue;
            }

            result.Add(namesByKey.TryGetValue(key, out var nameCostume)
                ? MergeCostume(eraCostume, nameCostume)
                : eraCostume);
        }

        return result;
    }

    private static IReadOnlyDictionary<uint, Title> ComposeTitles(
        IReadOnlyDictionary<uint, Title> eraTitles,
        IReadOnlyDictionary<uint, Title> nameTitles)
    {
        if (nameTitles.Count == 0)
        {
            return eraTitles;
        }

        return eraTitles
            .OrderBy(pair => pair.Key)
            .ToDictionary(
                pair => pair.Key,
                pair => nameTitles.TryGetValue(pair.Key, out var nameTitle)
                    ? MergeTitle(pair.Value, nameTitle)
                    : pair.Value);
    }

    private static IReadOnlyDictionary<uint, Neiro> ComposeNeiros(
        IReadOnlyDictionary<uint, Neiro> eraNeiros,
        IReadOnlyDictionary<uint, Neiro> nameNeiros)
    {
        if (nameNeiros.Count == 0)
        {
            return eraNeiros;
        }

        var useBaseRange = eraNeiros.Count < BaseNeiroIds.Length
                           && BaseNeiroIds.All(nameNeiros.ContainsKey);
        IEnumerable<uint> ids = useBaseRange
            ? BaseNeiroIds
            : eraNeiros.Keys.OrderBy(id => id);

        return ids.ToDictionary(
            id => id,
            id =>
            {
                eraNeiros.TryGetValue(id, out var eraNeiro);
                nameNeiros.TryGetValue(id, out var nameNeiro);
                return MergeNeiro(id, eraNeiro, nameNeiro);
            });
    }

    private static IReadOnlyList<Costume> MergeCostumeNameSources(
        IReadOnlyList<Costume> fallbackCostumes,
        IReadOnlyList<Costume> preferredCostumes)
    {
        if (preferredCostumes.Count == 0)
        {
            return fallbackCostumes;
        }

        if (fallbackCostumes.Count == 0)
        {
            return preferredCostumes;
        }

        var result = fallbackCostumes
            .GroupBy(costume => BuildCostumeKey(costume.CostumeType, costume.CostumeId))
            .ToDictionary(group => group.Key, group => group.First());

        foreach (var costume in preferredCostumes)
        {
            result[BuildCostumeKey(costume.CostumeType, costume.CostumeId)] = costume;
        }

        return result.Values.ToList();
    }

    private static IReadOnlyDictionary<uint, T> MergeNameSources<T>(
        IReadOnlyDictionary<uint, T> fallbackItems,
        IReadOnlyDictionary<uint, T> preferredItems)
    {
        if (preferredItems.Count == 0)
        {
            return fallbackItems;
        }

        if (fallbackItems.Count == 0)
        {
            return preferredItems;
        }

        var result = fallbackItems.ToDictionary();
        foreach (var (id, item) in preferredItems)
        {
            result[id] = item;
        }

        return result;
    }

    private static Costume MergeCostume(Costume eraCostume, Costume nameCostume)
        => new()
        {
            CostumeId = eraCostume.CostumeId,
            CostumeType = eraCostume.CostumeType,
            CostumeName = Prefer(nameCostume.CostumeName, eraCostume.CostumeName),
            CostumeNameEN = Prefer(nameCostume.CostumeNameEN, eraCostume.CostumeNameEN),
            CostumeNameCN = Prefer(nameCostume.CostumeNameCN, eraCostume.CostumeNameCN),
            CostumeNameKO = Prefer(nameCostume.CostumeNameKO, eraCostume.CostumeNameKO),
            Source = eraCostume.Source
        };

    private static Title MergeTitle(Title eraTitle, Title nameTitle)
        => new()
        {
            TitleId = eraTitle.TitleId,
            TitleName = Prefer(nameTitle.TitleName, eraTitle.TitleName),
            TitleNameEN = Prefer(nameTitle.TitleNameEN, eraTitle.TitleNameEN),
            TitleNameCN = Prefer(nameTitle.TitleNameCN, eraTitle.TitleNameCN),
            TitleNameKO = Prefer(nameTitle.TitleNameKO, eraTitle.TitleNameKO),
            TitleRarity = nameTitle.TitleRarity,
            Source = eraTitle.Source
        };

    private static Neiro MergeNeiro(uint id, Neiro? eraNeiro, Neiro? nameNeiro)
        => new()
        {
            NeiroId = id,
            NeiroName = Prefer(nameNeiro?.NeiroName, eraNeiro?.NeiroName),
            NeiroNameEN = Prefer(nameNeiro?.NeiroNameEN, eraNeiro?.NeiroNameEN),
            NeiroNameCN = Prefer(nameNeiro?.NeiroNameCN, eraNeiro?.NeiroNameCN),
            NeiroNameKO = Prefer(nameNeiro?.NeiroNameKO, eraNeiro?.NeiroNameKO),
            Source = eraNeiro?.Source
        };

    private static string Prefer(string? preferred, string? fallback)
        => string.IsNullOrWhiteSpace(preferred) ? fallback ?? string.Empty : preferred;

    private static bool IsUnknownCostumeType(string costumeType)
        => string.Equals(costumeType, UnknownCostumeType, StringComparison.OrdinalIgnoreCase);

    private static CostumeKey BuildCostumeKey(string costumeType, uint costumeId)
        => new(costumeType.ToLowerInvariant(), costumeId);

    private readonly record struct CostumeKey(string CostumeType, uint CostumeId);
}
