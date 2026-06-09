using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

[Mapper]
public static partial class InitialDataMappers
{
    [MapProperty(nameof(CommonInitialDataCheckResponse.DefaultSongFlg), nameof(InitialdatacheckResponse.HashDefaultSongFlg))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AchievementSongBit), nameof(InitialdatacheckResponse.HashMainichidojoAll))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.UraReleaseBit), nameof(InitialdatacheckResponse.HashMainichidojoRare))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryBlueTelopDatas), nameof(InitialdatacheckResponse.AryTelopDatas))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryBlueEventFolderDatas), nameof(InitialdatacheckResponse.AryEventfolderDatas))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryBlueTaikojukuDatas), nameof(InitialdatacheckResponse.AryTaikojukuDatas))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryBlueItemShopDatas), nameof(InitialdatacheckResponse.AryItemshopDatas))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryBlueLegaltermsDatas), nameof(InitialdatacheckResponse.AryLegaltermsDatas))]
    public static partial InitialdatacheckResponse Map(CommonInitialDataCheckResponse common);

    private static partial InitialdatacheckResponse.InformationData MapInformation(
        CommonInitialDataCheckResponse.InformationData common);
}
