using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

[Mapper]
public static partial class BaidResponseMapper
{
    public static BAIDResponse Map(CommonBaidResponse common)
    {
        var limits = Ac15EraProfiles.Yellow.Limits;
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
            CostumeFlg1 = Ac15ProtocolBytes.FixedOrZero(common.CostumeFlg1, limits.CostumeFlagBytes),
            CostumeFlg2 = Ac15ProtocolBytes.FixedOrZero(common.CostumeFlg2, limits.CostumeFlagBytes),
            CostumeFlg3 = Ac15ProtocolBytes.FixedOrZero(common.CostumeFlg3, limits.CostumeFlagBytes),
            CostumeFlg4 = Ac15ProtocolBytes.FixedOrZero(common.CostumeFlg4, limits.CostumeFlagBytes),
            CostumeFlg5 = Ac15ProtocolBytes.FixedOrZero(common.CostumeFlg5, limits.CostumeFlagBytes),
            TotalGetDonmedal = common.TotalGetDonmedal.GetValueOrDefault(),
            TotalUseDonmedal = common.TotalUseDonmedal.GetValueOrDefault(),
            TotalGetKatsumedal = common.TotalGetKatsumedal.GetValueOrDefault(),
            TotalUseKatsumedal = common.TotalUseKatsumedal.GetValueOrDefault(),
            ItemshopTutorialFlg = common.ItemshopTutorialFlg.GetValueOrDefault(),
            IsAutoCostumeOn = common.IsAutoCostumeOn.GetValueOrDefault(),
            LastPlayDatetime = common.LastPlayDatetime,
            DispDanType = common.DispDanType.GetValueOrDefault(),
            GotDanMax = common.GotDanMax,
            GotDanFlg = Ac15ProtocolBytes.FixedOrZero(common.GotDanFlg, limits.DanFlagBytes),
            GotDanextraFlg = Ac15ProtocolBytes.FixedOrZero(common.GotDanExtraFlg, limits.DanExtraFlagBytes),
            ContentInfo = new byte[limits.ContentInfoBytes],
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
