using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;
using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static partial class FinalBaidResponseMapper
{
    [MapProperty(nameof(Ac15BaidIdentity.MyDonName), nameof(FinalWire.BAIDResponse.MydonName))]
    [MapperIgnoreSource(nameof(Ac15BaidIdentity.MyDonNameLanguage))]
    public static partial void Apply(Ac15BaidIdentity source, [MappingTarget] FinalWire.BAIDResponse response);

    [MapProperty(nameof(Ac15BaidProfile.SelectedCostume), nameof(FinalWire.BAIDResponse.AryCostumedata), Use = nameof(MapCostumeData))]
    [MapperIgnoreSource(nameof(Ac15BaidProfile.TitlePlateId))]
    [MapperIgnoreSource(nameof(Ac15BaidProfile.IsAutoCostumeOn))]
    [MapperIgnoreSource(nameof(Ac15BaidProfile.DefaultToneSetting))]
    [MapperIgnoreSource(nameof(Ac15BaidProfile.LastPlayDatetime))]
    public static partial void Apply(Ac15BaidProfile source, [MappingTarget] FinalWire.BAIDResponse response);

    [MapProperty(nameof(Ac15BaidCostumeFlags.CostumeFlg1), nameof(FinalWire.BAIDResponse.CostumeFlg1), Use = nameof(MapCostumeFlag))]
    [MapProperty(nameof(Ac15BaidCostumeFlags.CostumeFlg2), nameof(FinalWire.BAIDResponse.CostumeFlg2), Use = nameof(MapCostumeFlag))]
    [MapProperty(nameof(Ac15BaidCostumeFlags.CostumeFlg3), nameof(FinalWire.BAIDResponse.CostumeFlg3), Use = nameof(MapCostumeFlag))]
    [MapProperty(nameof(Ac15BaidCostumeFlags.CostumeFlg4), nameof(FinalWire.BAIDResponse.CostumeFlg4), Use = nameof(MapCostumeFlag))]
    [MapProperty(nameof(Ac15BaidCostumeFlags.CostumeFlg5), nameof(FinalWire.BAIDResponse.CostumeFlg5), Use = nameof(MapCostumeFlag))]
    public static partial void Apply(Ac15BaidCostumeFlags source, [MappingTarget] FinalWire.BAIDResponse response);

    [MapProperty(nameof(Ac15BaidDan.GotDanFlg), nameof(FinalWire.BAIDResponse.GotDanFlg), Use = nameof(MapDanFlag))]
    [MapperIgnoreSource(nameof(Ac15BaidDan.GotDanExtraFlg))]
    public static partial void Apply(Ac15BaidDan source, [MappingTarget] FinalWire.BAIDResponse response);

    public static partial void Apply(Ac15BaidReward source, [MappingTarget] FinalWire.BAIDResponse response);

    private static partial FinalWire.BAIDResponse.CostumeData MapCostumeData(Ac15CostumeFacts values);

    private static byte[] MapCostumeFlag(byte[]? value)
        => Ac15ProtocolBytes.FixedOrZero(value, Ac15EraProfiles.Kimidori.Limits.CostumeFlagBytes);

    private static byte[] MapDanFlag(byte[]? value)
        => Ac15ProtocolBytes.FixedOrZero(value, Ac15EraProfiles.Kimidori.Limits.DanFlagBytes);
}
