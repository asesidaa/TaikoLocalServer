namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <summary>
/// An older game's item shop for one player (Green, Blue, Yellow): every season with what it sells, the player's
/// Don medals in it (earned at the cabinet, spent on items) and what they bought. Buying happens at the cabinet.
/// </summary>
public sealed record ShopResponse(string Era, bool Enabled, uint? LiveSeasonId, IReadOnlyList<ShopSeason> Seasons);

/// <summary>Dates are the shop file's yyyyMMddHHmmss, read as local time.</summary>
public sealed record ShopSeason(uint SeasonId, string Name, DateTime? Opens, DateTime? Closes, uint Earned, uint Spent,
    IReadOnlyList<ShopItem> Items);

/// <summary>Type is the shop file's: song, tone, kigurumi, body, head, face or puchi. PurchasedAt is null when not bought.</summary>
public sealed record ShopItem(string Type, uint Id, uint Price, DateTime? PurchasedAt);

/// <summary>Server Management › Game data › Item shop: how many players bought from a season, and how many items.</summary>
public sealed record ShopSeasonSales(uint SeasonId, int Players, int Items);
