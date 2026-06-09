using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class GhostMappers
{
    public static GetghostdataResponse Map(CommonGhostDataResponse common)
    {
        var response = new GetghostdataResponse
        {
            Result = common.Result,
            ReleaseInfoFlag = common.ReleaseInfoFlag,
            PlayedSongFlag = common.PlayedSongFlag,
            TotalWinnings = common.TotalWinnings,
            ghost_perf_data = MapPerfData(common.GhostPerfData),
            GhostRecordData = MapRankDataCore(common.GhostRecordData)
        };

        response.GhostRecordData.AryWinningsDatas.AddRange(common.GhostRecordData.AryWinningsData
            .Select(MapWinningsData));
        response.AryTokenDatas.AddRange(common.AryTokenData.Select(MapTokenData));

        return response;
    }

    public static GetghostscoreResponse Map(CommonGhostScoreResponse common)
    {
        var response = new GetghostscoreResponse { Result = common.Result };
        response.AryBestSectionDatas.AddRange(common.AryBestSectionData.Select(MapBestSectionData));

        return response;
    }

    private static partial GetghostdataResponse.GhostPerfData MapPerfData(
        CommonGhostDataResponse.GhostPerfDataInfo common);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial GetghostdataResponse.GhostRankData MapRankDataCore(
        CommonGhostDataResponse.GhostRankData common);

    private static partial GetghostdataResponse.GhostRankData.GhostWinningsData MapWinningsData(
        CommonGhostDataResponse.GhostWinningsData common);

    private static partial GetghostdataResponse.GhostTokenData MapTokenData(
        CommonGhostDataResponse.GhostTokenData common);

    private static partial GetghostscoreResponse.GhostBestSectionData MapBestSectionData(
        CommonGhostScoreResponse.GhostBestSectionData common);
}
