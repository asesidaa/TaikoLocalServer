using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Common;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

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

    [MapProperty(nameof(Ac15BaidCompatibility.PersonId), nameof(BAIDResponse.Personid), Use = nameof(MapPersonId))]
    public static partial void Apply(Ac15BaidCompatibility source, [MappingTarget] BAIDResponse response);

    private static BAIDResponse.CostumeData MapCostumeData(Ac15CostumeFacts values)
        => new()
        {
            Costume1 = values.Costume1,
            Costume2 = values.Costume2,
            Costume3 = values.Costume3,
            Costume4 = values.Costume4,
            Costume5 = values.Costume5
        };

    private static byte[] MapCostumeFlag(byte[]? value)
        => GreenProtocolBytes.FixedOrZero(value, GreenProtocolBytes.CostumeFlagBytes);

    private static byte[] MapDanFlag(byte[]? value)
        => GreenProtocolBytes.FixedOrZero(value, GreenProtocolBytes.DanFlagBytes);

    private static byte[] MapDanExtraFlag(byte[]? value)
        => GreenProtocolBytes.FixedOrZero(value, GreenProtocolBytes.DanExtraFlagBytes);

    private static string MapPersonId(string? value) => value ?? string.Empty;
}
