using TaikoLocalServer.Adapters.AdminApi.Mapping;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DanBestDataController(ITaikoDbContext context) : BaseAdminController<DanBestDataController>
{
    [HttpGet("{baid}")]
    public Task<IActionResult> GetDanBestData(uint baid)
        => GetDanBestData(nameof(GameEra.Nijiiro), baid);

    [HttpGet("/api/{era}/[controller]/{baid}")]
    public async Task<IActionResult> GetDanBestData(string era, uint baid)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
            return EraRoute.BadEra(era);

        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        return gameEra switch
        {
            GameEra.Nijiiro => Ok(await BuildNijiiroDanBestData(baid)),
            GameEra.Green => Ok(await BuildGreenDanBestData(baid)),
            GameEra.Blue => Ok(await BuildBlueDanBestData(baid)),
            GameEra.Yellow => Ok(await BuildYellowDanBestData(baid)),
            _ => EraRoute.BadEra(era)
        };
    }

    private async Task<DanBestDataResponse> BuildNijiiroDanBestData(uint baid)
    {
        // FIXME: Handle gaiden in here and web ui
        var danScores = await context.DanScoreDataNijiiro
            .Where(d => d.Baid == baid && d.DanType == DanType.Normal)
            .Include(d => d.DanStageScoreData)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(HttpContext.RequestAborted);

        var danDataList = new List<DanBestData>();
        foreach (var danScore in danScores)
        {
            var danData = danScore.ToDanBestData();
            danData.DanBestStageDataList = danScore.DanStageScoreData
                .Select(datum => datum.ToDanBestStageData())
                .ToList();
            danDataList.Add(danData);
        }

        return new DanBestDataResponse
        {
            DanBestDataList = danDataList
        };
    }

    private async Task<DanBestDataResponse> BuildGreenDanBestData(uint baid)
    {
        var rows = await context.DanScoreDataGreen
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
                ClearState = MapGreenClearGrade(row.ClearGrade),
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

    private static DanClearState MapGreenClearGrade(Ac15DanClearGrade grade)
    {
        return grade switch
        {
            Ac15DanClearGrade.NotClear => DanClearState.NotClear,
            Ac15DanClearGrade.NormalClear => DanClearState.RedNormalClear,
            Ac15DanClearGrade.GoldClear => DanClearState.GoldNormalClear,
            _ => DanClearState.NotClear
        };
    }

    private async Task<DanBestDataResponse> BuildBlueDanBestData(uint baid)
    {
        var rows = await context.DanScoreDataBlue
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
                ClearState = MapBlueClearGrade(row.ClearGrade),
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

    private static DanClearState MapBlueClearGrade(Ac15DanClearGrade grade)
    {
        return grade switch
        {
            Ac15DanClearGrade.NotClear => DanClearState.NotClear,
            Ac15DanClearGrade.NormalClear => DanClearState.RedNormalClear,
            Ac15DanClearGrade.GoldClear => DanClearState.GoldNormalClear,
            _ => DanClearState.NotClear
        };
    }

    private async Task<DanBestDataResponse> BuildYellowDanBestData(uint baid)
    {
        var rows = await context.DanScoreDataYellow
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
                ClearState = MapYellowClearGrade(row.ClearGrade),
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

    private static DanClearState MapYellowClearGrade(Ac15DanClearGrade grade)
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
