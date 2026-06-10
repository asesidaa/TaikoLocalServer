using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Application.Ac15;

[Mapper]
public static partial class Ac15ItemShopMapper
{
    public static partial Ac15ShopSeasonState ToAc15ShopSeasonState(BlueShopSeasonState state);

    public static partial Ac15ShopSeasonState ToAc15ShopSeasonState(GreenShopSeasonState state);

    public static partial Ac15ShopSeasonState ToAc15ShopSeasonState(YellowShopSeasonState state);

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

    private static partial BlueShopItemState ToBlueShopItemState(Ac15PurchasedShopItem item);

    private static partial GreenShopItemState ToGreenShopItemState(Ac15PurchasedShopItem item);

    private static partial YellowShopItemState ToYellowShopItemState(Ac15PurchasedShopItem item);
}
