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
            ghost_perf_data = new GetghostdataResponse.GhostPerfData
            {
                InputMedian = common.GhostPerfData.InputMedian,
                InputVariance = common.GhostPerfData.InputVariance
            },
            GhostRecordData = new GetghostdataResponse.GhostRankData
            {
                RankId = common.GhostRecordData.RankId,
                WinPoint = common.GhostRecordData.WinPoint,
                CertifiedLevelId = common.GhostRecordData.CertifiedLevelId
            }
        };

        response.GhostRecordData.AryWinningsDatas.AddRange(common.GhostRecordData.AryWinningsData
            .Select(row => new GetghostdataResponse.GhostRankData.GhostWinningsData
            {
                LevelId = row.LevelId,
                Winnings = row.Winnings
            }));
        response.AryTokenDatas.AddRange(common.AryTokenData.Select(row => new GetghostdataResponse.GhostTokenData
        {
            TokenId = row.TokenId,
            TokenValue = row.TokenValue
        }));

        return response;
    }

    public static GetghostscoreResponse Map(CommonGhostScoreResponse common)
    {
        var response = new GetghostscoreResponse { Result = common.Result };
        response.AryBestSectionDatas.AddRange(common.AryBestSectionData.Select(row => new GetghostscoreResponse.GhostBestSectionData
        {
            SectionNo = row.SectionNo,
            GoodCnt = row.GoodCnt,
            OkCnt = row.OkCnt,
            NgCnt = row.NgCnt,
            PoundCnt = row.PoundCnt
        }));

        return response;
    }
}
