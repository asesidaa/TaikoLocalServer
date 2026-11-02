using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class ItemShopMappers
{
    public static GetItemShopInfoQuery Map(GetitemshopinfoRequest request)
    {
        return new GetItemShopInfoQuery(GameEra.Green);
    }

    public static ItemPurchaseCommand Map(ItempurchaseRequest request)
    {
        return new ItemPurchaseCommand(
            request.Baid,
            GameEra.Green,
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
