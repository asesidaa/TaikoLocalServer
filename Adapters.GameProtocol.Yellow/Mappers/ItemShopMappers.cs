namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

public static class ItemShopMappers
{
    public static GetItemShopInfoQuery Map(GetitemshopinfoRequest request)
    {
        return new GetItemShopInfoQuery(GameEra.Yellow);
    }

    public static GetitemshopinfoResponse Map(CommonItemShopInfoResponse common)
    {
        var response = new GetitemshopinfoResponse
        {
            Result = common.Result,
            VerupNo = common.VerupNo,
            SeasonId = common.SeasonId,
            Telop = common.Telop
        };

        response.AryItemshopDatas.AddRange(common.AryItemshopData.Select(item => new GetitemshopinfoResponse.ItemshopData
        {
            ItemNo = item.ItemNo,
            ItemType = item.ItemType,
            ItemId = item.ItemId,
            ItemPrice = item.ItemPrice
        }));

        return response;
    }
}
