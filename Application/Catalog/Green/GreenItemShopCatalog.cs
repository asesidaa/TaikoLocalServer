namespace TaikoLocalServer.Application.Catalog.Green;

public sealed class GreenItemShopCatalog
{
    public static GreenItemShopCatalog Disabled { get; } = new();

    public bool IsEnabled { get; init; }

    public uint? ActiveSeasonId { get; init; }

    public IReadOnlyDictionary<uint, GreenItemShopSeason> Seasons { get; init; }
        = new Dictionary<uint, GreenItemShopSeason>();

    public GreenItemShopSeason? ActiveSeason
        => ActiveSeasonId is { } id && Seasons.TryGetValue(id, out var season)
            ? season
            : null;

    public IReadOnlyDictionary<uint, GreenItemShopEntry> ActiveItemsByNo
        => ActiveSeason?.ItemsByNo ?? new Dictionary<uint, GreenItemShopEntry>();
}
