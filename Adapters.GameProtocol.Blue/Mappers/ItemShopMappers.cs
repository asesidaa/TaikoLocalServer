using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

[Mapper]
public static partial class ItemShopMappers
{
    public static GetItemShopInfoQuery Map(GetitemshopinfoRequest request)
    {
        return new GetItemShopInfoQuery(GameEra.Blue);
    }

    public static ItemPurchaseCommand Map(ItempurchaseRequest request)
    {
        return new ItemPurchaseCommand(
            request.Baid,
            GameEra.Blue,
            request.ItemNo,
            request.ItemType,
            request.ItemId,
            request.ItemPrice);
    }

    public static GetitemshopinfoResponse Map(CommonItemShopInfoResponse common)
    {
        var response = new GetitemshopinfoResponse
        {
            Result = common.Result,
            VerupNo = common.VerupNo,
            SeasonId = common.SeasonId,
            Telop = common.Telop,
            StartDatetime = common.StartDatetime,
            EndDatetime = common.EndDatetime,
            AfterstartDays = common.AfterstartDays,
            BeforecloseDays = common.BeforecloseDays
        };

        response.AryItemshopDatas.AddRange(common.AryItemshopData.Select(MapItemShopData));

        return response;
    }

    public static partial ItempurchaseResponse Map(CommonItemPurchaseResponse common);

    private static partial GetitemshopinfoResponse.ItemshopData MapItemShopData(
        CommonItemShopInfoResponse.ItemShopData item);
}
