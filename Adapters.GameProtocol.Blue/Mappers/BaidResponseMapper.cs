using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Common;
using TaikoLocalServer.Application.Dtos;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

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
            CostumeFlg1 = BlueProtocolBytes.FixedOrZero(common.CostumeFlg1, BlueProtocolBytes.CostumeFlagBytes),
            CostumeFlg2 = BlueProtocolBytes.FixedOrZero(common.CostumeFlg2, BlueProtocolBytes.CostumeFlagBytes),
            CostumeFlg3 = BlueProtocolBytes.FixedOrZero(common.CostumeFlg3, BlueProtocolBytes.CostumeFlagBytes),
            CostumeFlg4 = BlueProtocolBytes.FixedOrZero(common.CostumeFlg4, BlueProtocolBytes.CostumeFlagBytes),
            CostumeFlg5 = BlueProtocolBytes.FixedOrZero(common.CostumeFlg5, BlueProtocolBytes.CostumeFlagBytes),
            TotalGetDonmedal = common.TotalGetDonmedal.GetValueOrDefault(),
            TotalUseDonmedal = common.TotalUseDonmedal.GetValueOrDefault(),
            TotalGetKatsumedal = common.TotalGetKatsumedal.GetValueOrDefault(),
            TotalUseKatsumedal = common.TotalUseKatsumedal.GetValueOrDefault(),
            ItemshopTutorialFlg = common.ItemshopTutorialFlg.GetValueOrDefault(),
            IsAutoCostumeOn = common.IsAutoCostumeOn.GetValueOrDefault(),
            LastPlayDatetime = common.LastPlayDatetime,
            DispDanType = common.DispDanType.GetValueOrDefault(),
            GotDanMax = common.GotDanMax,
            GotDanFlg = BlueProtocolBytes.FixedOrZero(common.GotDanFlg, BlueProtocolBytes.DanFlagBytes),
            GotDanextraFlg = BlueProtocolBytes.FixedOrZero(common.GotDanExtraFlg, BlueProtocolBytes.DanExtraFlagBytes),
            ContentInfo = new byte[BlueProtocolBytes.ContentInfoBytes],
            DefaultToneSetting = common.DefaultToneSetting.GetValueOrDefault(),
            Personid = common.PersonId ?? string.Empty,
            WaiwaiTutorialFlg = common.WaiwaiTutorialFlg.GetValueOrDefault()
        };
    }
}
