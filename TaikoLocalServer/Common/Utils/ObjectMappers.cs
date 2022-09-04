using Swan.Mapping;

namespace TaikoLocalServer.Common.Utils;

using DanScoreDataStage = GetDanScoreResponse.DanScoreData.DanScoreDataStage;

public static class ObjectMappers
{

    public static readonly IObjectMap<DanStageScoreDatum, DanScoreDataStage> DanStageDbToResponseMap;
    
    public static readonly IObjectMap<DanScoreDataStage, DanStageScoreDatum> DanStageResponseToDbMap;

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

        DanStageResponseToDbMap = mapper.AddMap<DanScoreDataStage, DanStageScoreDatum>()
            .Add(t => t.ComboCount, s => s.ComboCnt)
            .Add(t => t.GoodCount, s => s.GoodCnt)
            .Add(t => t.OkCount, s => s.OkCnt)
            .Add(t => t.BadCount, s => s.NgCnt)
            .Add(t => t.TotalHitCount, s => s.HitCnt)
            .Add(t => t.DrumrollCount, s => s.PoundCnt)
            .Add(t => t.PlayScore, s => s.PlayScore)
            .Add(t => t.HighScore, s => s.HighScore);
    }
}