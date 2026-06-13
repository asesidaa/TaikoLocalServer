using Riok.Mapperly.Abstractions;

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

    private static BAIDResponse.CostumeData MapCostumeData(IReadOnlyList<uint> values)
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
