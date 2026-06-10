using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetDanScoreQueryHandler
{
    private partial async ValueTask<CommonDanScoreDataResponse> HandleBlue(
        GetDanScoreQuery request,
        CancellationToken cancellationToken)
    {
        var requestedIds = request.DanIds.ToHashSet();
        var limits = Ac15EraProfiles.Blue.Limits;
        var knownChallengeLevels = gameDataService.Blue().TaikojukuFileOrder
            .Select(pack => pack.ChallengeLevel)
            .Where(id => Ac15DanHelpers.IsKnownDanId(id, limits))
            .ToHashSet();

        var validRequestedIds = requestedIds
            .Where(id => knownChallengeLevels.Contains(id))
            .ToHashSet();

        var rows = await context.DanScoreDataBlue
            .Where(row => row.Baid == request.Baid && validRequestedIds.Contains(row.DanId))
            .Include(row => row.DanStageScoreData)
            .ToListAsync(cancellationToken);

        var response = new CommonDanScoreDataResponse { Result = 1 };
        foreach (var row in rows.OrderBy(row => row.DanId))
        {
            var responseData = new CommonDanScoreDataResponse.DanScoreData
            {
                DanId = row.DanId,
                ArrivalSongCnt = row.ArrivalSongCount,
                SoulGaugeTotal = row.SoulGaugeTotal,
                ComboCntTotal = row.ComboCountTotal
            };

            foreach (var stage in row.DanStageScoreData.OrderBy(stage => stage.StageIndex).Take((int)row.ArrivalSongCount))
            {
                responseData.AryDanScoreDataStages.Add(new CommonDanScoreDataResponse.DanScoreDataStage
                {
                    PlayScore = stage.PlayScore,
                    GoodCnt = stage.GoodCount,
                    OkCnt = stage.OkCount,
                    NgCnt = stage.BadCount,
                    PoundCnt = stage.DrumrollCount,
                    HitCnt = stage.TotalHitCount,
                    ComboCnt = stage.ComboCount,
                    HighScore = stage.HighScore
                });
            }

            response.AryDanScoreDatas.Add(responseData);
        }

        return response;
    }
}
