using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

[Mapper]
public static partial class InitialDataMappers
{
    [MapProperty(nameof(CommonInitialDataCheckResponse.DefaultSongFlg), nameof(InitialdatacheckResponse.HashDefaultSongFlg))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AchievementSongBit), nameof(InitialdatacheckResponse.HashMainichidojoAll))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.UraReleaseBit), nameof(InitialdatacheckResponse.HashMainichidojoRare))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryTelopDatas), nameof(InitialdatacheckResponse.AryTelopDatas))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryEventFolderDatas), nameof(InitialdatacheckResponse.AryEventfolderDatas))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryTaikojukuDatas), nameof(InitialdatacheckResponse.AryTaikojukuDatas))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryItemShopDatas), nameof(InitialdatacheckResponse.AryItemshopDatas))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryLegaltermsDatas), nameof(InitialdatacheckResponse.AryLegaltermsDatas))]
    public static partial InitialdatacheckResponse Map(CommonInitialDataCheckResponse common);

    private static partial InitialdatacheckResponse.InformationData MapInformation(
        CommonInitialDataCheckResponse.InformationData common);
}
