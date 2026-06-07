namespace TaikoLocalServer.Application.Catalog.Yellow;

public sealed class YellowItemShopCatalog
{
    public static YellowItemShopCatalog Disabled { get; } = new();

    public bool IsEnabled { get; init; }

    public uint? ActiveSeasonId { get; init; }

    public IReadOnlyDictionary<uint, YellowItemShopSeason> Seasons { get; init; }
        = new Dictionary<uint, YellowItemShopSeason>();

    public YellowItemShopSeason? ActiveSeason
        => ActiveSeasonId is { } id && Seasons.TryGetValue(id, out var season)
            ? season
            : null;

    public IReadOnlyDictionary<uint, YellowItemShopEntry> ActiveItemsByNo
        => ActiveSeason?.ItemsByNo ?? new Dictionary<uint, YellowItemShopEntry>();
}
