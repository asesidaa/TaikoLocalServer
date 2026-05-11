namespace TaikoLocalServer.Application.Handlers;

public partial class GetGhostDataQueryHandler
{
    public partial async ValueTask<CommonGhostDataResponse> Handle(
        GetGhostDataQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Reading Green ghost data for baid {Baid}", request.Baid);
        var saveData = await context.GetOrCreateGreenSaveDataAsync(request.Baid, cancellationToken);
        var tokens = await context.GreenGhostTokens
            .Where(row => row.Baid == request.Baid)
            .Select(row => new CommonGhostDataResponse.GhostTokenData
            {
                TokenId = row.TokenId,
                TokenValue = row.TokenValue
            })
            .ToListAsync(cancellationToken);
        var winnings = await context.GreenGhostWinnings
            .Where(row => row.Baid == request.Baid)
            .Select(row => new CommonGhostDataResponse.GhostWinningsData
            {
                LevelId = row.LevelId,
                Winnings = row.Winnings
            })
            .ToListAsync(cancellationToken);

        return new CommonGhostDataResponse
        {
            Result = 1,
            ReleaseInfoFlag = GreenProtocolBytes.FixedOrZero(saveData.GhostReleaseInfoFlag, GreenProtocolBytes.GhostReleaseInfoBytes),
            PlayedSongFlag = GreenProtocolBytes.FixedOrZero(saveData.GhostPlayedSongFlag, GreenProtocolBytes.GhostPlayedSongBytes),
            TotalWinnings = saveData.GhostTotalWinnings,
            GhostPerfData = new CommonGhostDataResponse.GhostPerfDataInfo
            {
                InputMedian = saveData.GhostInputMedian,
                InputVariance = saveData.GhostInputVariance
            },
            GhostRecordData = new CommonGhostDataResponse.GhostRankData
            {
                RankId = saveData.GhostRankId,
                WinPoint = saveData.GhostWinPoint,
                CertifiedLevelId = saveData.GhostCertifiedLevelId,
                AryWinningsData = winnings
            },
            AryTokenData = tokens
        };
    }
}
