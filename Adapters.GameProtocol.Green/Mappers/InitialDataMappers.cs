using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class InitialDataMappers
{
    [MapProperty(nameof(CommonInitialDataCheckResponse.DefaultSongFlg), nameof(InitialdatacheckResponse.HashDefaultSongFlg))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AchievementSongBit), nameof(InitialdatacheckResponse.HashMainichidojoAll))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.UraReleaseBit), nameof(InitialdatacheckResponse.HashMainichidojoRare))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryGreenTelopDatas), nameof(InitialdatacheckResponse.AryTelopDatas))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryGreenEventFolderDatas), nameof(InitialdatacheckResponse.AryEventfolderDatas))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryGreenTaikojukuDatas), nameof(InitialdatacheckResponse.AryTaikojukuDatas))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryGreenItemShopDatas), nameof(InitialdatacheckResponse.AryItemshopDatas))]
    public static partial InitialdatacheckResponse Map(CommonInitialDataCheckResponse common);

    private static partial InitialdatacheckResponse.InformationData MapInformation(
        CommonInitialDataCheckResponse.InformationData common);
}
