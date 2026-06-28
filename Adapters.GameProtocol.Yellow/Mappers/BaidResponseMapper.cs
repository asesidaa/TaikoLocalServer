using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static partial class BaidResponseMapper
{
    [MapProperty(nameof(Ac15BaidIdentity.MyDonName), nameof(BAIDResponse.MydonName))]
    [MapperIgnoreSource(nameof(Ac15BaidIdentity.MyDonNameLanguage))]
    public static partial void Apply(Ac15BaidIdentity source, [MappingTarget] BAIDResponse response);

    [MapProperty(nameof(Ac15BaidProfile.TitlePlateId), nameof(BAIDResponse.TitleplateId))]
    [MapProperty(nameof(Ac15BaidProfile.SelectedCostume), nameof(BAIDResponse.AryCostumedata), Use = nameof(MapCostumeData))]
    public static partial void Apply(Ac15BaidProfile source, [MappingTarget] BAIDResponse response);

    [MapProperty(nameof(Ac15BaidCostumeFlags.CostumeFlg1), nameof(BAIDResponse.CostumeFlg1), Use = nameof(MapCostumeFlag))]
    [MapProperty(nameof(Ac15BaidCostumeFlags.CostumeFlg2), nameof(BAIDResponse.CostumeFlg2), Use = nameof(MapCostumeFlag))]
    [MapProperty(nameof(Ac15BaidCostumeFlags.CostumeFlg3), nameof(BAIDResponse.CostumeFlg3), Use = nameof(MapCostumeFlag))]
    [MapProperty(nameof(Ac15BaidCostumeFlags.CostumeFlg4), nameof(BAIDResponse.CostumeFlg4), Use = nameof(MapCostumeFlag))]
    [MapProperty(nameof(Ac15BaidCostumeFlags.CostumeFlg5), nameof(BAIDResponse.CostumeFlg5), Use = nameof(MapCostumeFlag))]
    public static partial void Apply(Ac15BaidCostumeFlags source, [MappingTarget] BAIDResponse response);

    public static partial void Apply(Ac15BaidShopMedals source, [MappingTarget] BAIDResponse response);

    [MapProperty(nameof(Ac15BaidDan.GotDanFlg), nameof(BAIDResponse.GotDanFlg), Use = nameof(MapDanFlag))]
    [MapProperty(nameof(Ac15BaidDan.GotDanExtraFlg), nameof(BAIDResponse.GotDanextraFlg), Use = nameof(MapDanExtraFlag))]
    public static partial void Apply(Ac15BaidDan source, [MappingTarget] BAIDResponse response);

    [MapProperty(nameof(Ac15BaidCompatibility.PersonId), nameof(BAIDResponse.Personid), Use = nameof(@Ac15MapperNormalization.StringOrEmpty))]
    public static partial void Apply(Ac15BaidCompatibility source, [MappingTarget] BAIDResponse response);

    private static partial BAIDResponse.CostumeData MapCostumeData(Ac15CostumeFacts values);

    private static byte[] MapCostumeFlag(byte[]? value)
        => Ac15MapperNormalization.FixedOrZero(value, Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes);

    private static byte[] MapDanFlag(byte[]? value)
        => Ac15MapperNormalization.FixedOrZero(value, Ac15EraProfiles.Yellow.Limits.DanFlagBytes);

    private static byte[] MapDanExtraFlag(byte[]? value)
        => Ac15MapperNormalization.FixedOrZero(value, Ac15EraProfiles.Yellow.Limits.DanExtraFlagBytes);
}
