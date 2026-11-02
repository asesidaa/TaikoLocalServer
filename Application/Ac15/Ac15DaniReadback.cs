namespace TaikoLocalServer.Application.Ac15;

public static class Ac15DaniReadback
{
    public static async ValueTask<IReadOnlyList<Ac15DaniScore>> GetScoresAsync<TScore, TStage>(
        Ac15DaniTables<TScore, TStage> tables,
        uint baid,
        IReadOnlySet<uint> requestedDanIds,
        IReadOnlySet<uint> knownChallengeLevels,
        CancellationToken cancellationToken)
        where TScore : class, IAc15DanScoreDatum
        where TStage : class, IAc15DanStageScoreDatum
    {
        var validRequestedIds = requestedDanIds.Where(knownChallengeLevels.Contains).ToHashSet();
        var rows = await tables.ScoresWithStages
            .Where(row => row.Baid == baid && validRequestedIds.Contains(row.DanId))
            .ToListAsync(cancellationToken);

        return rows
            .OrderBy(row => row.DanId)
            .Select(tables.ToScore)
            .ToArray();
    }

    public static CommonDanScoreDataResponse BuildResponse(IReadOnlyList<Ac15DaniScore> rows)
    {
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

            foreach (var stage in row.Stages.OrderBy(stage => stage.StageIndex).Take((int)row.ArrivalSongCount))
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
