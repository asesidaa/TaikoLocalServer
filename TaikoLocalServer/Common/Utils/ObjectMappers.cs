using GameDatabase.Entities;
using Swan.Mapping;

namespace TaikoLocalServer.Common.Utils;

using DanScoreDataStage = GetDanScoreResponse.DanScoreData.DanScoreDataStage;

public static class ObjectMappers
{

    public static readonly IObjectMap<DanStageScoreDatum, DanScoreDataStage> DanStageDbToResponseMap;

    static ObjectMappers()
    {
        var mapper = new ObjectMapper();
        DanStageDbToResponseMap = mapper.AddMap<DanStageScoreDatum, DanScoreDataStage>()
            .Add(t => t.ComboCnt, s => s.ComboCount)
            .Add(t => t.GoodCnt, s => s.GoodCount)
            .Add(t => t.OkCnt, s => s.OkCount)
            .Add(t => t.NgCnt, s => s.BadCount)
            .Add(t => t.HitCnt, s => s.TotalHitCount)
            .Add(t => t.PoundCnt, s => s.DrumrollCount)
            .Add(t => t.PlayScore, s => s.PlayScore)
            .Add(t => t.HighScore, s => s.HighScore);
    }
}