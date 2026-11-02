using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Application.Ac15;

[Mapper]
public static partial class Ac15ItemShopMapper
{
    public static BlueShopItemState ToBlueShopItemState(Ac15PurchasedShopItem item, DateTime now)
    {
        var row = ToBlueShopItemState(item);
        row.Status = Ac15ShopItemStatus.Unlocked;
        row.PurchasedAt = now;
        row.UnlockedAt = now;
        return row;
    }

    public static GreenShopItemState ToGreenShopItemState(Ac15PurchasedShopItem item, DateTime now)
    {
        var row = ToGreenShopItemState(item);
        row.Status = Ac15ShopItemStatus.Unlocked;
        row.PurchasedAt = now;
        row.UnlockedAt = now;
        return row;
    }

    public static YellowShopItemState ToYellowShopItemState(Ac15PurchasedShopItem item, DateTime now)
    {
        var row = ToYellowShopItemState(item);
        row.Status = Ac15ShopItemStatus.Unlocked;
        row.PurchasedAt = now;
        row.UnlockedAt = now;
        return row;
    }

    [MapperIgnoreTarget(nameof(BlueShopItemState.Status))]
    [MapperIgnoreTarget(nameof(BlueShopItemState.PurchasedAt))]
    [MapperIgnoreTarget(nameof(BlueShopItemState.UnlockedAt))]
    [MapperIgnoreTarget(nameof(BlueShopItemState.Ba))]
    private static partial BlueShopItemState ToBlueShopItemState(Ac15PurchasedShopItem item);

    [MapperIgnoreTarget(nameof(GreenShopItemState.Status))]
    [MapperIgnoreTarget(nameof(GreenShopItemState.PurchasedAt))]
    [MapperIgnoreTarget(nameof(GreenShopItemState.UnlockedAt))]
    [MapperIgnoreTarget(nameof(GreenShopItemState.Ba))]
    private static partial GreenShopItemState ToGreenShopItemState(Ac15PurchasedShopItem item);

    [MapperIgnoreTarget(nameof(YellowShopItemState.Status))]
    [MapperIgnoreTarget(nameof(YellowShopItemState.PurchasedAt))]
    [MapperIgnoreTarget(nameof(YellowShopItemState.UnlockedAt))]
    [MapperIgnoreTarget(nameof(YellowShopItemState.Ba))]
    private static partial YellowShopItemState ToYellowShopItemState(Ac15PurchasedShopItem item);
}
