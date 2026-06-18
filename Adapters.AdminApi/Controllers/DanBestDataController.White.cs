namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public partial class DanBestDataController
{
    private async Task<DanBestDataResponse> BuildWhiteDanBestData(uint baid)
    {
        var rows = await context.DanScoreDataWhite
            .Where(d => d.Baid == baid)
            .Include(d => d.DanStageScoreData)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(HttpContext.RequestAborted);

        return new DanBestDataResponse
        {
            DanBestDataList = rows.Select(row => new DanBestData
            {
                DanId = row.DanId,
                ClearState = MapWhiteClearGrade(row.ClearGrade),
                SoulGaugeTotal = row.SoulGaugeTotal,
                ComboCountTotal = row.ComboCountTotal,
                DanBestStageDataList = row.DanStageScoreData
                    .OrderBy(stage => stage.StageIndex)
                    .Select(stage => new DanBestStageData
                    {
                        SongNumber = stage.SongNumber,
                        PlayScore = stage.PlayScore,
                        GoodCount = stage.GoodCount,
                        OkCount = stage.OkCount,
                        BadCount = stage.BadCount,
                        DrumrollCount = stage.DrumrollCount,
                        TotalHitCount = stage.TotalHitCount,
                        ComboCount = stage.ComboCount,
                        HighScore = stage.HighScore
                    })
                    .ToList()
            }).ToList()
        };
    }

    private static DanClearState MapWhiteClearGrade(Ac15DanClearGrade grade)
    {
        return grade switch
        {
            Ac15DanClearGrade.NotClear => DanClearState.NotClear,
            Ac15DanClearGrade.NormalClear => DanClearState.RedNormalClear,
            Ac15DanClearGrade.GoldClear => DanClearState.GoldNormalClear,
            _ => DanClearState.NotClear
        };
    }
}
