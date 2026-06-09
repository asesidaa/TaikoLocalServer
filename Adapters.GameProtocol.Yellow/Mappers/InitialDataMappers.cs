using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

[Mapper]
public static partial class InitialDataMappers
{
    [MapProperty(nameof(CommonInitialDataCheckResponse.DefaultSongFlg), nameof(InitialdatacheckResponse.HashDefaultSongFlg))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AchievementSongBit), nameof(InitialdatacheckResponse.HashMainichidojoAll))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.UraReleaseBit), nameof(InitialdatacheckResponse.HashMainichidojoRare))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryYellowTelopDatas), nameof(InitialdatacheckResponse.AryTelopDatas))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryYellowEventFolderDatas), nameof(InitialdatacheckResponse.AryEventfolderDatas))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryYellowTaikojukuDatas), nameof(InitialdatacheckResponse.AryTaikojukuDatas))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryYellowItemShopDatas), nameof(InitialdatacheckResponse.AryItemshopDatas))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryYellowLegaltermsDatas), nameof(InitialdatacheckResponse.AryLegaltermsDatas))]
    public static partial InitialdatacheckResponse Map(CommonInitialDataCheckResponse common);

    private static partial InitialdatacheckResponse.InformationData MapInformation(
        CommonInitialDataCheckResponse.InformationData common);
}
