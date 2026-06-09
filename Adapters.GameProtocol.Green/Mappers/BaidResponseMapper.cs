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
            AryCostumedata = MapCostumeData(CostumeProjection.From(common.CostumeData)),
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

    private static partial BAIDResponse.CostumeData MapCostumeData(CostumeProjection projection);

    private readonly record struct CostumeProjection(
        uint Costume1,
        uint Costume2,
        uint Costume3,
        uint Costume4,
        uint Costume5)
    {
        public static CostumeProjection From(IReadOnlyList<uint> values)
            => new(
                values.ElementAtOrDefault(0),
                values.ElementAtOrDefault(1),
                values.ElementAtOrDefault(2),
                values.ElementAtOrDefault(3),
                values.ElementAtOrDefault(4));
    }
}
