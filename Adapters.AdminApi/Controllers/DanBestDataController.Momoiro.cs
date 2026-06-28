namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public partial class DanBestDataController
{
    private async Task<DanBestDataResponse> BuildMomoiroDanBestData(uint baid)
    {
        var rows = await context.DanScoreDataMomoiro
            .Where(d => d.Baid == baid)
            .Include(d => d.DanStageScoreData)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(HttpContext.RequestAborted);

        return new DanBestDataResponse
        {
            DanBestDataList = rows.Select(row => ToAc15DanBestData(
                row.DanId,
                row.ClearGrade,
                row.SoulGaugeTotal,
                row.ComboCountTotal,
                row.DanStageScoreData,
                MapMomoiroClearGrade)).ToList()
        };
    }

    private static DanClearState MapMomoiroClearGrade(Ac15DanClearGrade grade)
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
