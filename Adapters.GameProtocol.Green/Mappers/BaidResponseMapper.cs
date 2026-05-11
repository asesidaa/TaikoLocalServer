using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class BaidResponseMapper
{
    public static BAIDResponse Map(CommonBaidResponse common)
    {
        return new BAIDResponse
        {
            Result = common.Result,
            Baid = common.Baid,
            MydonName = common.MyDonName,
            TotalGetDonmedal = common.TotalGetDonmedal.GetValueOrDefault(),
            TotalUseDonmedal = common.TotalUseDonmedal.GetValueOrDefault(),
            TotalGetKatsumedal = common.TotalGetKatsumedal.GetValueOrDefault(),
            TotalUseKatsumedal = common.TotalUseKatsumedal.GetValueOrDefault(),
            ItemshopTutorialFlg = common.ItemshopTutorialFlg.GetValueOrDefault(),
            IsAutoCostumeOn = common.IsAutoCostumeOn.GetValueOrDefault(),
            DispDanType = common.DispDanType.GetValueOrDefault(),
            GotDanextraFlg = common.GotDanExtraFlg ?? [],
            DefaultToneSetting = common.DefaultToneSetting.GetValueOrDefault(),
            Personid = common.PersonId ?? string.Empty,
            WaiwaiTutorialFlg = common.WaiwaiTutorialFlg.GetValueOrDefault()
        };
    }
}
