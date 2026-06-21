using TaikoLocalServer.Adapters.AdminApi.Mapping;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public partial class DanBestDataController(ITaikoDbContext context) : BaseAdminController<DanBestDataController>
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
            GameEra.Red => Ok(await BuildRedDanBestData(baid)),
            GameEra.White => Ok(await BuildWhiteDanBestData(baid)),
            GameEra.Murasaki => Ok(await BuildMurasakiDanBestData(baid)),
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
            DanBestDataList = rows.Select(row => ToAc15DanBestData(
                row.DanId,
                row.ClearGrade,
                row.SoulGaugeTotal,
                row.ComboCountTotal,
                row.DanStageScoreData,
                MapGreenClearGrade)).ToList()
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
            DanBestDataList = rows.Select(row => ToAc15DanBestData(
                row.DanId,
                row.ClearGrade,
                row.SoulGaugeTotal,
                row.ComboCountTotal,
                row.DanStageScoreData,
                MapBlueClearGrade)).ToList()
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
            DanBestDataList = rows.Select(row => ToAc15DanBestData(
                row.DanId,
                row.ClearGrade,
                row.SoulGaugeTotal,
                row.ComboCountTotal,
                row.DanStageScoreData,
                MapYellowClearGrade)).ToList()
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

    private async Task<DanBestDataResponse> BuildRedDanBestData(uint baid)
    {
        var rows = await context.DanScoreDataRed
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
                MapRedClearGrade)).ToList()
        };
    }

    private static DanClearState MapRedClearGrade(Ac15DanClearGrade grade)
    {
        return grade switch
        {
            Ac15DanClearGrade.NotClear => DanClearState.NotClear,
            Ac15DanClearGrade.NormalClear => DanClearState.RedNormalClear,
            Ac15DanClearGrade.GoldClear => DanClearState.GoldNormalClear,
            _ => DanClearState.NotClear
        };
    }

    private static DanBestData ToAc15DanBestData<TStage>(
        uint danId,
        Ac15DanClearGrade clearGrade,
        uint soulGaugeTotal,
        uint comboCountTotal,
        IEnumerable<TStage> stages,
        Func<Ac15DanClearGrade, DanClearState> mapClearGrade)
        where TStage : class, IAc15DanStageScoreDatum
    {
        var orderedStages = stages.OrderBy(stage => stage.StageIndex).ToList();

        return new DanBestData
        {
            DanId = danId,
            ClearState = mapClearGrade(clearGrade),
            SoulGaugeTotal = soulGaugeTotal,
            ComboCountTotal = GetAc15ComboCountTotal(comboCountTotal, orderedStages),
            DanBestStageDataList = BuildAc15DanBestStageData(orderedStages)
        };
    }

    private static uint GetAc15ComboCountTotal<TStage>(uint storedComboCountTotal, IReadOnlyList<TStage> orderedStages)
        where TStage : class, IAc15DanStageScoreDatum
        => storedComboCountTotal != 0
            ? storedComboCountTotal
            : orderedStages.LastOrDefault()?.ComboCount ?? 0;

    private static List<DanBestStageData> BuildAc15DanBestStageData<TStage>(IReadOnlyList<TStage> orderedStages)
        where TStage : class, IAc15DanStageScoreDatum
    {
        var result = new List<DanBestStageData>(orderedStages.Count);
        TStage? previous = default;

        foreach (var stage in orderedStages)
        {
            // AC15 Dan uploads carry additive counters as after-stage cumulative snapshots.
            result.Add(new DanBestStageData
            {
                SongNumber = stage.SongNumber,
                PlayScore = DeltaFromPrevious(stage.PlayScore, previous?.PlayScore ?? 0),
                GoodCount = DeltaFromPrevious(stage.GoodCount, previous?.GoodCount ?? 0),
                OkCount = DeltaFromPrevious(stage.OkCount, previous?.OkCount ?? 0),
                BadCount = DeltaFromPrevious(stage.BadCount, previous?.BadCount ?? 0),
                DrumrollCount = DeltaFromPrevious(stage.DrumrollCount, previous?.DrumrollCount ?? 0),
                TotalHitCount = DeltaFromPrevious(stage.TotalHitCount, previous?.TotalHitCount ?? 0),
                ComboCount = stage.ComboCount,
                HighScore = DeltaFromPrevious(stage.HighScore, previous?.HighScore ?? 0)
            });

            previous = stage;
        }

        return result;
    }

    private static uint DeltaFromPrevious(uint current, uint previous)
        => current >= previous ? current - previous : current;
}
