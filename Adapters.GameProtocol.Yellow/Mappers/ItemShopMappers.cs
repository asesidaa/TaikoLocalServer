namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

public static class ItemShopMappers
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
            request.ShouldSerializeItemType() ? request.ItemType : null,
            request.ShouldSerializeItemId() ? request.ItemId : null,
            request.ShouldSerializeItemPrice() ? request.ItemPrice : null);
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

    public static ItempurchaseResponse Map(CommonItemPurchaseResponse common)
    {
        return new ItempurchaseResponse
        {
            Result = common.Result,
            TotalGetDonmedal = common.TotalGetDonmedal,
            TotalUseDonmedal = common.TotalUseDonmedal
        };
    }
}
