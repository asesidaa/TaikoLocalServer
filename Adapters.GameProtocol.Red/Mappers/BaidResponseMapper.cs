using Riok.Mapperly.Abstractions;
using RedV08R00 = TaikoLocalServer.Adapters.GameProtocol.Red.Wire.V08R00;

namespace TaikoLocalServer.Adapters.GameProtocol.Red.Mappers;

[Mapper]
public static partial class BaidResponseMapper
{
    [MapProperty(nameof(CommonBaidResponse.MyDonName), nameof(BAIDResponse.MydonName))]
    [MapProperty(nameof(CommonBaidResponse.TitlePlateId), nameof(BAIDResponse.TitleplateId))]
    [MapProperty(nameof(CommonBaidResponse.CostumeData), nameof(BAIDResponse.AryCostumedata), Use = nameof(MapCostumeData))]
    [MapProperty(nameof(CommonBaidResponse.CostumeFlg1), nameof(BAIDResponse.CostumeFlg1), Use = nameof(MapCostumeFlag))]
    [MapProperty(nameof(CommonBaidResponse.CostumeFlg2), nameof(BAIDResponse.CostumeFlg2), Use = nameof(MapCostumeFlag))]
    [MapProperty(nameof(CommonBaidResponse.CostumeFlg3), nameof(BAIDResponse.CostumeFlg3), Use = nameof(MapCostumeFlag))]
    [MapProperty(nameof(CommonBaidResponse.CostumeFlg4), nameof(BAIDResponse.CostumeFlg4), Use = nameof(MapCostumeFlag))]
    [MapProperty(nameof(CommonBaidResponse.CostumeFlg5), nameof(BAIDResponse.CostumeFlg5), Use = nameof(MapCostumeFlag))]
    [MapProperty(nameof(CommonBaidResponse.RewardPtn), nameof(BAIDResponse.RewardPtn), Use = nameof(MapPresentUInt32))]
    [MapProperty(nameof(CommonBaidResponse.DispDanType), nameof(BAIDResponse.DispDanType), Use = nameof(MapPresentUInt32))]
    [MapProperty(nameof(CommonBaidResponse.GotDanFlg), nameof(BAIDResponse.GotDanFlg), Use = nameof(MapDanFlag))]
    [MapProperty(nameof(CommonBaidResponse.GotDanExtraFlg), nameof(BAIDResponse.GotDanextraFlg), Use = nameof(MapDanExtraFlag))]
    [MapProperty(nameof(CommonBaidResponse.DefaultToneSetting), nameof(BAIDResponse.DefaultToneSetting), Use = nameof(MapPresentUInt32))]
    [MapProperty(nameof(CommonBaidResponse.PersonId), nameof(BAIDResponse.Personid), Use = nameof(MapStringOrEmpty))]
    [MapValue(nameof(BAIDResponse.ContentInfo), Use = nameof(GetContentInfo))]
    [MapperIgnoreTarget(nameof(BAIDResponse.PlayerType))]
    [MapperIgnoreTarget(nameof(BAIDResponse.ComSvrResult))]
    [MapperIgnoreTarget(nameof(BAIDResponse.MbId))]
    [MapperIgnoreTarget(nameof(BAIDResponse.AccessCode))]
    [MapperIgnoreTarget(nameof(BAIDResponse.IsPublish))]
    [MapperIgnoreTarget(nameof(BAIDResponse.CardOwnNum))]
    [MapperIgnoreTarget(nameof(BAIDResponse.RegCountryId))]
    [MapperIgnoreTarget(nameof(BAIDResponse.PurposeId))]
    [MapperIgnoreTarget(nameof(BAIDResponse.RegionId))]
    [MapperIgnoreTarget(nameof(BAIDResponse.AryFavoriteCostumedatas))]
    [MapperIgnoreTarget(nameof(BAIDResponse.UpdateDatetime))]
    [MapperIgnoreTarget(nameof(BAIDResponse.Accesstoken))]
    public static partial BAIDResponse Map(CommonBaidResponse common);

    public static RedV08R00.BAIDResponse MapV08R00(CommonBaidResponse common)
        => new()
        {
            Result = common.Result,
            Baid = common.Baid,
            MydonName = common.MyDonName,
            Title = common.Title,
            TitleplateId = common.TitlePlateId,
            ColorFace = common.ColorFace,
            ColorBody = common.ColorBody,
            ColorLimb = common.ColorLimb,
            AryCostumedata = MapV08R00CostumeData(common.CostumeData),
            CostumeFlg1 = MapCostumeFlag(common.CostumeFlg1),
            CostumeFlg2 = MapCostumeFlag(common.CostumeFlg2),
            CostumeFlg3 = MapCostumeFlag(common.CostumeFlg3),
            CostumeFlg4 = MapCostumeFlag(common.CostumeFlg4),
            CostumeFlg5 = MapCostumeFlag(common.CostumeFlg5),
            RewardPtn = MapPresentUInt32(common.RewardPtn),
            DispDanType = MapPresentUInt32(common.DispDanType),
            GotDanMax = MapPresentUInt32(common.GotDanMax),
            GotDanFlg = MapDanFlag(common.GotDanFlg),
            ContentInfo = GetContentInfo(),
            DefaultToneSetting = MapPresentUInt32(common.DefaultToneSetting),
            Personid = MapStringOrEmpty(common.PersonId)
        };

    private static BAIDResponse.CostumeData MapCostumeData(IReadOnlyList<uint> values)
        => new()
        {
            Costume1 = values.ElementAtOrDefault(0),
            Costume2 = values.ElementAtOrDefault(1),
            Costume3 = values.ElementAtOrDefault(2),
            Costume4 = values.ElementAtOrDefault(3),
            Costume5 = values.ElementAtOrDefault(4)
        };

    private static RedV08R00.BAIDResponse.CostumeData MapV08R00CostumeData(IReadOnlyList<uint> values)
        => new()
        {
            Costume1 = values.ElementAtOrDefault(0),
            Costume2 = values.ElementAtOrDefault(1),
            Costume3 = values.ElementAtOrDefault(2),
            Costume4 = values.ElementAtOrDefault(3),
            Costume5 = values.ElementAtOrDefault(4)
        };

    private static byte[] MapCostumeFlag(byte[]? value)
        => Ac15ProtocolBytes.FixedOrZero(value, Ac15EraProfiles.Red.Limits.CostumeFlagBytes);

    private static uint? MapPresentUInt32(uint? value) => value.GetValueOrDefault();

    private static byte[] MapDanFlag(byte[] value)
        => Ac15ProtocolBytes.FixedOrZero(value, Ac15EraProfiles.Red.Limits.DanFlagBytes);

    private static byte[] MapDanExtraFlag(byte[]? value)
        => Ac15ProtocolBytes.FixedOrZero(value, Ac15EraProfiles.Red.Limits.DanExtraFlagBytes);

    private static string MapStringOrEmpty(string? value) => value ?? string.Empty;

    private static byte[] GetContentInfo() => new byte[Ac15EraProfiles.Red.Limits.ContentInfoBytes];
}
