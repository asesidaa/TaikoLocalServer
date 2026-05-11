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
            Title = common.Title,
            TitleplateId = common.TitlePlateId,
            ColorFace = common.ColorFace,
            ColorBody = common.ColorBody,
            ColorLimb = common.ColorLimb,
            AryCostumedata = new BAIDResponse.CostumeData
            {
                Costume1 = common.CostumeData.ElementAtOrDefault(0),
                Costume2 = common.CostumeData.ElementAtOrDefault(1),
                Costume3 = common.CostumeData.ElementAtOrDefault(2),
                Costume4 = common.CostumeData.ElementAtOrDefault(3),
                Costume5 = common.CostumeData.ElementAtOrDefault(4)
            },
            CostumeFlg1 = common.CostumeFlg1 ?? new byte[GreenProtocolBytes.CostumeFlagBytes],
            CostumeFlg2 = common.CostumeFlg2 ?? new byte[GreenProtocolBytes.CostumeFlagBytes],
            CostumeFlg3 = common.CostumeFlg3 ?? new byte[GreenProtocolBytes.CostumeFlagBytes],
            CostumeFlg4 = common.CostumeFlg4 ?? new byte[GreenProtocolBytes.CostumeFlagBytes],
            CostumeFlg5 = common.CostumeFlg5 ?? new byte[GreenProtocolBytes.CostumeFlagBytes],
            TotalGetDonmedal = common.TotalGetDonmedal.GetValueOrDefault(),
            TotalUseDonmedal = common.TotalUseDonmedal.GetValueOrDefault(),
            TotalGetKatsumedal = common.TotalGetKatsumedal.GetValueOrDefault(),
            TotalUseKatsumedal = common.TotalUseKatsumedal.GetValueOrDefault(),
            ItemshopTutorialFlg = common.ItemshopTutorialFlg.GetValueOrDefault(),
            IsAutoCostumeOn = common.IsAutoCostumeOn.GetValueOrDefault(),
            LastPlayDatetime = common.LastPlayDatetime,
            DispDanType = common.DispDanType.GetValueOrDefault(),
            GotDanMax = common.GotDanMax,
            GotDanFlg = common.GotDanFlg ?? new byte[GreenProtocolBytes.DanFlagBytes],
            GotDanextraFlg = common.GotDanExtraFlg ?? new byte[GreenProtocolBytes.DanExtraFlagBytes],
            ContentInfo = new byte[GreenProtocolBytes.ContentInfoBytes],
            DefaultToneSetting = common.DefaultToneSetting.GetValueOrDefault(),
            Personid = common.PersonId ?? string.Empty,
            WaiwaiTutorialFlg = common.WaiwaiTutorialFlg.GetValueOrDefault()
        };
    }
}
