namespace TaikoLocalServer.Application.Catalog.Blue;

public sealed class BlueItemShopCatalog
{
    public static BlueItemShopCatalog Disabled { get; } = new();

    public bool IsEnabled { get; init; }

    public uint? ActiveSeasonId { get; init; }

    public IReadOnlyDictionary<uint, BlueItemShopSeason> Seasons { get; init; }
        = new Dictionary<uint, BlueItemShopSeason>();

    public BlueItemShopSeason? ActiveSeason
        => ActiveSeasonId is { } id && Seasons.TryGetValue(id, out var season)
            ? season
            : null;

    public IReadOnlyDictionary<uint, BlueItemShopEntry> ActiveItemsByNo
        => ActiveSeason?.ItemsByNo ?? new Dictionary<uint, BlueItemShopEntry>();
}
