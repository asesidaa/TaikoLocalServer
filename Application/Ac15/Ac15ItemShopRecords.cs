namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15ItemShopPurchaseRequest(
    uint Baid,
    uint ItemNo,
    uint? ItemType,
    uint? ItemId,
    uint? ItemPrice);

public sealed record Ac15PurchasedShopItem(
    uint Baid,
    uint SeasonId,
    uint ItemType,
    uint ItemId,
    uint ItemNo,
    uint ItemPrice);

public sealed record Ac15ItemShopPurchaseTables<TSeason, TItem>(
    DbSet<TItem> ItemStates,
    Func<uint, CancellationToken, ValueTask<TSeason?>> GetOrCreateSeason,
    Func<Ac15PurchasedShopItem, DateTime, TItem> CreateItem)
    where TSeason : class, IAc15ShopSeasonState
    where TItem : class, IAc15ShopItemState;

public sealed record Ac15ItemShopUnlockPolicy<TSave>(
    Func<Ac15ShopItemType, bool> IsSupported,
    Action<TSave, Ac15ShopItemType, uint> ApplyUnlock);
