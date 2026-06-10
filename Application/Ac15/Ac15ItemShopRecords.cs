namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15ItemShopPurchaseRequest(
    uint Baid,
    uint ItemNo,
    uint? ItemType,
    uint? ItemId,
    uint? ItemPrice);

public sealed record Ac15ShopSeasonState(
    uint Baid,
    uint SeasonId,
    uint TotalGetDonmedal,
    uint TotalUseDonmedal)
{
    public uint TotalGetDonmedal { get; set; } = TotalGetDonmedal;
    public uint TotalUseDonmedal { get; set; } = TotalUseDonmedal;
}

public sealed record Ac15PurchasedShopItem(
    uint Baid,
    uint SeasonId,
    uint ItemType,
    uint ItemId,
    uint ItemNo,
    uint ItemPrice);

internal static class Ac15PurchasedShopItemStates
{
    public static async ValueTask<bool> ContainsAsync<TShopItemState>(
        DbSet<TShopItemState> itemStates,
        uint baid,
        uint seasonId,
        uint itemType,
        uint itemId,
        CancellationToken cancellationToken)
        where TShopItemState : class, IAc15ShopItemState
        => await itemStates.FindAsync([baid, seasonId, itemType, itemId], cancellationToken) is not null;

    public static TShopItemState Create<TShopItemState>(Ac15PurchasedShopItem item, DateTime now)
        where TShopItemState : class, IAc15ShopItemState, new()
        => new()
        {
            Baid = item.Baid,
            SeasonId = item.SeasonId,
            ItemType = item.ItemType,
            ItemId = item.ItemId,
            ItemNo = item.ItemNo,
            ItemPrice = item.ItemPrice,
            Status = Ac15ShopItemStatus.Unlocked,
            PurchasedAt = now,
            UnlockedAt = now
        };

    public static ValueTask SaveAsync(ITaikoDbContext context, CancellationToken cancellationToken)
        => new(context.SaveChangesAsync(cancellationToken));
}
