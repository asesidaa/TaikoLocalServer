namespace TaikoLocalServer.Application.Catalog.Ac15;

public sealed class Ac15ItemShopCatalog
{
    public static Ac15ItemShopCatalog Disabled { get; } = new();

    public bool IsEnabled { get; init; }

    public uint? ActiveSeasonId { get; init; }

    public IReadOnlyDictionary<uint, Ac15ItemShopSeason> Seasons { get; init; }
        = new Dictionary<uint, Ac15ItemShopSeason>();

    public Ac15ItemShopSeason? ActiveSeason
        => ActiveSeasonId is { } id && Seasons.TryGetValue(id, out var season)
            ? season
            : null;

    public IReadOnlyDictionary<uint, Ac15ItemShopEntry> ActiveItemsByNo
        => ActiveSeason?.ItemsByNo ?? new Dictionary<uint, Ac15ItemShopEntry>();
}
