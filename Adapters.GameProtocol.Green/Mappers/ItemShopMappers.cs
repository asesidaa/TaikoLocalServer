using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class ItemShopMappers
{
    public static GetitemshopinfoResponse Map(CommonItemShopInfoResponse common)
    {
        return new GetitemshopinfoResponse
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
