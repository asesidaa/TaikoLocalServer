using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class GhostMappers
{
    [MapProperty(nameof(CommonGhostDataResponse.GhostPerfData), nameof(GetghostdataResponse.ghost_perf_data))]
    [MapProperty(nameof(CommonGhostDataResponse.AryTokenData), nameof(GetghostdataResponse.AryTokenDatas))]
    public static partial GetghostdataResponse Map(CommonGhostDataResponse common);

    [MapProperty(nameof(CommonGhostScoreResponse.AryBestSectionData), nameof(GetghostscoreResponse.AryBestSectionDatas))]
    public static partial GetghostscoreResponse Map(CommonGhostScoreResponse common);

    private static partial GetghostdataResponse.GhostPerfData MapPerfData(
        CommonGhostDataResponse.GhostPerfDataInfo common);

    [MapProperty(nameof(CommonGhostDataResponse.GhostRankData.AryWinningsData), nameof(GetghostdataResponse.GhostRankData.AryWinningsDatas))]
    private static partial GetghostdataResponse.GhostRankData MapRankData(
        CommonGhostDataResponse.GhostRankData common);

    private static partial GetghostdataResponse.GhostRankData.GhostWinningsData MapWinningsData(
        CommonGhostDataResponse.GhostWinningsData common);

    private static partial GetghostdataResponse.GhostTokenData MapTokenData(
        CommonGhostDataResponse.GhostTokenData common);

    private static partial GetghostscoreResponse.GhostBestSectionData MapBestSectionData(
        CommonGhostScoreResponse.GhostBestSectionData common);
}
