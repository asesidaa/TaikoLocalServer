using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Dtos.Ac15;
using RedV08R00 = TaikoLocalServer.Adapters.GameProtocol.Red.Wire.V08R00;

namespace TaikoLocalServer.Adapters.GameProtocol.Red.Mappers;

[Mapper]
public static partial class BaidResponseMapper
{
    [MapPropertyFromSource(nameof(BAIDResponse.MydonName), Use = nameof(MapMyDonName))]
    [MapPropertyFromSource(nameof(BAIDResponse.Title), Use = nameof(MapTitle))]
    [MapPropertyFromSource(nameof(BAIDResponse.TitleplateId), Use = nameof(MapTitlePlateId))]
    [MapPropertyFromSource(nameof(BAIDResponse.ColorFace), Use = nameof(MapColorFace))]
    [MapPropertyFromSource(nameof(BAIDResponse.ColorBody), Use = nameof(MapColorBody))]
    [MapPropertyFromSource(nameof(BAIDResponse.ColorLimb), Use = nameof(MapColorLimb))]
    [MapPropertyFromSource(nameof(BAIDResponse.AryCostumedata), Use = nameof(MapSelectedCostumeData))]
    [MapPropertyFromSource(nameof(BAIDResponse.CostumeFlg1), Use = nameof(MapCostumeFlag1))]
    [MapPropertyFromSource(nameof(BAIDResponse.CostumeFlg2), Use = nameof(MapCostumeFlag2))]
    [MapPropertyFromSource(nameof(BAIDResponse.CostumeFlg3), Use = nameof(MapCostumeFlag3))]
    [MapPropertyFromSource(nameof(BAIDResponse.CostumeFlg4), Use = nameof(MapCostumeFlag4))]
    [MapPropertyFromSource(nameof(BAIDResponse.CostumeFlg5), Use = nameof(MapCostumeFlag5))]
    [MapPropertyFromSource(nameof(BAIDResponse.RewardPtn), Use = nameof(MapRewardPtn))]
    [MapPropertyFromSource(nameof(BAIDResponse.DispDanType), Use = nameof(MapDispDanType))]
    [MapPropertyFromSource(nameof(BAIDResponse.GotDanMax), Use = nameof(MapGotDanMax))]
    [MapPropertyFromSource(nameof(BAIDResponse.GotDanFlg), Use = nameof(MapDanFlag))]
    [MapPropertyFromSource(nameof(BAIDResponse.GotDanextraFlg), Use = nameof(MapDanExtraFlag))]
    [MapPropertyFromSource(nameof(BAIDResponse.DefaultToneSetting), Use = nameof(MapDefaultToneSetting))]
    [MapPropertyFromSource(nameof(BAIDResponse.Personid), Use = nameof(MapPersonId))]
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
    public static partial BAIDResponse Map(Ac15BaidResponse common);

    public static RedV08R00.BAIDResponse MapV08R00(Ac15BaidResponse common)
        => new()
        {
            Result = common.Result,
            Baid = common.Baid,
            MydonName = MapMyDonName(common),
            Title = MapTitle(common),
            TitleplateId = MapTitlePlateId(common),
            ColorFace = MapColorFace(common),
            ColorBody = MapColorBody(common),
            ColorLimb = MapColorLimb(common),
            AryCostumedata = MapV08R00CostumeData(common.Profile?.SelectedCostume ?? Ac15CostumeFacts.Empty),
            CostumeFlg1 = MapCostumeFlag1(common),
            CostumeFlg2 = MapCostumeFlag2(common),
            CostumeFlg3 = MapCostumeFlag3(common),
            CostumeFlg4 = MapCostumeFlag4(common),
            CostumeFlg5 = MapCostumeFlag5(common),
            RewardPtn = MapRewardPtn(common),
            DispDanType = MapDispDanType(common),
            GotDanMax = MapGotDanMax(common),
            GotDanFlg = MapDanFlag(common),
            ContentInfo = GetContentInfo(),
            DefaultToneSetting = MapDefaultToneSetting(common),
            Personid = MapPersonId(common)
        };

    private static string MapMyDonName(Ac15BaidResponse response)
        => response.Identity?.MyDonName ?? string.Empty;

    private static string MapTitle(Ac15BaidResponse response)
        => response.Profile?.Title ?? string.Empty;

    private static uint? MapTitlePlateId(Ac15BaidResponse response)
        => response.Profile?.TitlePlateId;

    private static uint? MapColorFace(Ac15BaidResponse response)
        => response.Profile?.ColorFace;

    private static uint? MapColorBody(Ac15BaidResponse response)
        => response.Profile?.ColorBody;

    private static uint? MapColorLimb(Ac15BaidResponse response)
        => response.Profile?.ColorLimb;

    private static BAIDResponse.CostumeData MapSelectedCostumeData(Ac15BaidResponse response)
        => MapCostumeData(response.Profile?.SelectedCostume ?? Ac15CostumeFacts.Empty);

    private static BAIDResponse.CostumeData MapCostumeData(Ac15CostumeFacts values)
        => new()
        {
            Costume1 = values.Costume1,
            Costume2 = values.Costume2,
            Costume3 = values.Costume3,
            Costume4 = values.Costume4,
            Costume5 = values.Costume5
        };

    private static RedV08R00.BAIDResponse.CostumeData MapV08R00CostumeData(Ac15CostumeFacts values)
        => new()
        {
            Costume1 = values.Costume1,
            Costume2 = values.Costume2,
            Costume3 = values.Costume3,
            Costume4 = values.Costume4,
            Costume5 = values.Costume5
        };

    private static byte[] MapCostumeFlag1(Ac15BaidResponse response)
        => MapCostumeFlag(response.CostumeFlags?.CostumeFlg1);

    private static byte[] MapCostumeFlag2(Ac15BaidResponse response)
        => MapCostumeFlag(response.CostumeFlags?.CostumeFlg2);

    private static byte[] MapCostumeFlag3(Ac15BaidResponse response)
        => MapCostumeFlag(response.CostumeFlags?.CostumeFlg3);

    private static byte[] MapCostumeFlag4(Ac15BaidResponse response)
        => MapCostumeFlag(response.CostumeFlags?.CostumeFlg4);

    private static byte[] MapCostumeFlag5(Ac15BaidResponse response)
        => MapCostumeFlag(response.CostumeFlags?.CostumeFlg5);

    private static byte[] MapCostumeFlag(byte[]? value)
        => Ac15ProtocolBytes.FixedOrZero(value, Ac15EraProfiles.Red.Limits.CostumeFlagBytes);

    private static uint? MapRewardPtn(Ac15BaidResponse response)
        => response.Reward?.RewardPtn;

    private static uint? MapDispDanType(Ac15BaidResponse response)
        => response.Dan?.DispDanType;

    private static uint? MapGotDanMax(Ac15BaidResponse response)
        => response.Dan?.GotDanMax;

    private static byte[] MapDanFlag(Ac15BaidResponse response)
        => Ac15ProtocolBytes.FixedOrZero(response.Dan?.GotDanFlg, Ac15EraProfiles.Red.Limits.DanFlagBytes);

    private static byte[] MapDanExtraFlag(Ac15BaidResponse response)
        => Ac15ProtocolBytes.FixedOrZero(response.Dan?.GotDanExtraFlg, Ac15EraProfiles.Red.Limits.DanExtraFlagBytes);

    private static uint? MapDefaultToneSetting(Ac15BaidResponse response)
        => response.Profile?.DefaultToneSetting;

    private static string MapPersonId(Ac15BaidResponse response)
        => response.Compatibility?.PersonId ?? string.Empty;

    private static byte[] GetContentInfo() => new byte[Ac15EraProfiles.Red.Limits.ContentInfoBytes];
}
