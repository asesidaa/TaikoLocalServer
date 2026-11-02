using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

[Mapper]
public static partial class ItemShopMappers
{
    public static GetItemShopInfoQuery Map(GetitemshopinfoRequest request)
    {
        return new GetItemShopInfoQuery(GameEra.Yellow);
    }

    public static ItemPurchaseCommand Map(ItempurchaseRequest request)
    {
        return new ItemPurchaseCommand(
            request.Baid,
            GameEra.Yellow,
            request.ItemNo,
            request.ItemType,
            request.ItemId,
            request.ItemPrice);
    }

    [MapProperty(nameof(CommonItemShopInfoResponse.AryItemshopData), nameof(GetitemshopinfoResponse.AryItemshopDatas))]
    public static partial GetitemshopinfoResponse Map(CommonItemShopInfoResponse common);

    public static partial ItempurchaseResponse Map(CommonItemPurchaseResponse common);

    private static partial GetitemshopinfoResponse.ItemshopData MapItemShopData(
        CommonItemShopInfoResponse.ItemShopData item);
}
