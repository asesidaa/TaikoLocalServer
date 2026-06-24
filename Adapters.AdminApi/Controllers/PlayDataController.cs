using TaikoLocalServer.Adapters.AdminApi.Mapping;
using Throw;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public partial class PlayDataController(ITaikoDbContext context) : BaseAdminController<PlayDataController>
{
    private readonly ITaikoDbContext context = context;

    [HttpGet("{baid}")]
    public Task<ActionResult<SongBestResponse>> GetSongBestRecords(uint baid)
        => GetSongBestRecords(nameof(GameEra.Nijiiro), baid);

    [HttpGet("/api/{era}/[controller]/{baid}")]
    public async Task<ActionResult<SongBestResponse>> GetSongBestRecords(string era, uint baid)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
            return EraRoute.BadEra(era);

        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        var user = await context.UserData.FindAsync(baid);
        if (user is null)
        {
            return NotFound();
        }

        return gameEra switch
        {
            GameEra.Nijiiro => Ok(await BuildNijiiroSongBestResponse(baid)),
            GameEra.Green => Ok(await BuildGreenSongBestResponse(baid)),
            GameEra.Blue => Ok(await BuildBlueSongBestResponse(baid)),
            GameEra.Yellow => Ok(await BuildYellowSongBestResponse(baid)),
            GameEra.Red => Ok(await BuildRedSongBestResponse(baid)),
            GameEra.White => Ok(await BuildWhiteSongBestResponse(baid)),
            GameEra.Murasaki => Ok(await BuildMurasakiSongBestResponse(baid)),
            GameEra.Kimidori => Ok(await BuildKimidoriSongBestResponse(baid)),
            _ => EraRoute.BadEra(era)
        };
    }

    private async Task<SongBestResponse> BuildNijiiroSongBestResponse(uint baid)
    {
        var saveData = await context.GetOrCreateNijiiroSaveDataAsync(baid, HttpContext.RequestAborted);

        var songBestDbData = await context.SongBestDataNijiiro
            .Where(d => d.Baid == baid)
            .AsNoTracking()
            .ToListAsync(HttpContext.RequestAborted);
        var songBestRecords = songBestDbData.Select(d => d.ToSongBestData()).ToList();
        var aiSectionBest = await context.AiScoreDataNijiiro
            .Where(d => d.Baid == baid)
            .Include(d => d.AiSectionScoreData)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(HttpContext.RequestAborted);
        var songPlayData = await context.SongPlayDataNijiiro
            .Where(d => d.Baid == baid)
            .AsNoTracking()
            .ToListAsync(HttpContext.RequestAborted);

        foreach (var bestData in songBestRecords)
        {
            var songPlayDatums = songPlayData
                .Where(d => d.Difficulty == bestData.Difficulty && d.SongId == bestData.SongId)
                .ToArray();
            songPlayDatums.Throw($"Play log for song id {bestData.SongId} is null! Something is wrong with db!").IfEmpty();

            bestData.LastPlayTime = songPlayDatums.MaxBy(d => d.PlayTime)!.PlayTime;

            var bestLog = songPlayDatums.MaxBy(d => d.Score);
            bestLog!.ApplyBestLogTo(bestData);

            if (bestLog is not null)
            {
                bestData.PlaySetting = PlaySettingConverter.ShortToPlaySetting((short)bestLog.OptionSetting);
            }

            var aiSection = aiSectionBest.FirstOrDefault(d =>
                d.Difficulty == bestData.Difficulty && d.SongId == bestData.SongId);
            if (aiSection is null)
            {
                continue;
            }

            bestData.AiSectionBestData = aiSection.AiSectionScoreData
                .Select(d => d.ToAiSectionBestData())
                .ToList();
        }

        foreach (var songBestData in songBestRecords)
        {
            var songPlayLogs = songPlayData
                .Where(d => d.SongId == songBestData.SongId && d.Difficulty == songBestData.Difficulty)
                .ToList();
            songBestData.PlayCount = songPlayLogs.Count;
            songBestData.ClearCount = songPlayLogs.Count(d => d.Crown >= CrownType.Clear);
            songBestData.FullComboCount = songPlayLogs.Count(d => d.Crown >= CrownType.Gold);
            songBestData.PerfectCount = songPlayLogs.Count(d => d.Crown >= CrownType.Dondaful);
        }

        var favoriteSet = saveData.FavoriteSongsArray.ToHashSet();
        foreach (var songBestRecord in songBestRecords.Where(r => favoriteSet.Contains(r.SongId)))
        {
            songBestRecord.IsFavorite = true;
        }

        foreach (var songBestRecord in songBestRecords)
        {
            songBestRecord.RecentPlayData = songPlayData
                .Where(d => d.SongId == songBestRecord.SongId && d.Difficulty == songBestRecord.Difficulty)
                .Select(d => d.ToSongPlayDatumDto())
                .ToList();
        }

        return new SongBestResponse
        {
            SongBestData = songBestRecords
        };
    }

    private async Task<SongBestResponse> BuildGreenSongBestResponse(uint baid)
    {
        var bestRows = await context.SongBestDataGreen
            .Where(d => d.Baid == baid)
            .AsNoTracking()
            .ToListAsync(HttpContext.RequestAborted);
        var playRows = await context.SongPlayDataGreen
            .Where(d => d.Baid == baid)
            .AsNoTracking()
            .ToListAsync(HttpContext.RequestAborted);
        var favoriteSet = await context.GreenFavoriteSongs
            .Where(d => d.Baid == baid)
            .Select(d => d.SongNo)
            .ToHashSetAsync(HttpContext.RequestAborted);

        var songBestRecords = bestRows
            .GroupBy(d => new { d.SongId, d.Difficulty })
            .Select(group => BuildGreenSongBestData(group, playRows, favoriteSet))
            .ToList();

        return new SongBestResponse
        {
            SongBestData = songBestRecords
        };
    }

    private static SongBestData BuildGreenSongBestData(
        IEnumerable<SongBestDatumGreen> bestRows,
        IReadOnlyCollection<SongPlayDatumGreen> playRows,
        IReadOnlySet<uint> favoriteSet)
    {
        var rows = bestRows.ToList();
        var primary = rows.FirstOrDefault(d => !d.IsShin) ?? rows.First();
        var alternate = rows.FirstOrDefault(d => d.IsShin && d != primary);
        var matchingPlays = playRows
            .Where(d => d.SongId == primary.SongId && d.Difficulty == primary.Difficulty)
            .ToList();
        var bestPlay = matchingPlays.MaxBy(d => d.Score);

        var result = new SongBestData
        {
            SongId = primary.SongId,
            Difficulty = primary.Difficulty,
            BestScore = primary.BestScore,
            BestRate = primary.BestRate,
            BestCrown = primary.BestCrown,
            BestScoreRank = ScoreRank.None,
            IsFavorite = favoriteSet.Contains(primary.SongId),
            PlayCount = matchingPlays.Count,
            ClearCount = matchingPlays.Count(d => d.Crown >= CrownType.Clear),
            FullComboCount = matchingPlays.Count(d => d.Crown >= CrownType.Gold),
            PerfectCount = matchingPlays.Count(d => d.Crown >= CrownType.Dondaful),
            RecentPlayData = matchingPlays.Select(MapGreenPlayToDto).ToList()
        };

        if (matchingPlays.Count > 0)
        {
            result.LastPlayTime = matchingPlays.Max(d => d.PlayTime);
        }

        if (bestPlay is not null)
        {
            result.PlayTime = bestPlay.PlayTime;
            result.GoodCount = bestPlay.GoodCount;
            result.OkCount = bestPlay.OkCount;
            result.MissCount = bestPlay.MissCount;
            result.ComboCount = bestPlay.ComboCount;
            result.HitCount = bestPlay.HitCount;
            result.DrumrollCount = bestPlay.PoundCount;
        }

        if (alternate is not null)
        {
            result.AlternateScore = new ScoreFacet
            {
                Label = "Shin",
                BestScore = alternate.BestScore,
                BestRate = alternate.BestRate,
                BestCrown = alternate.BestCrown
            };
        }

        return result;
    }

    private static SongPlayDatumDto MapGreenPlayToDto(SongPlayDatumGreen play)
    {
        return new SongPlayDatumDto
        {
            SongId = play.SongId,
            SongNumber = play.SongId,
            Difficulty = play.Difficulty,
            Crown = play.Crown,
            Score = play.Score,
            ScoreRate = play.ScoreRate,
            ScoreRank = ScoreRank.None,
            GoodCount = play.GoodCount,
            OkCount = play.OkCount,
            MissCount = play.MissCount,
            ComboCount = play.ComboCount,
            HitCount = play.HitCount,
            DrumrollCount = play.PoundCount,
            PlayTime = play.PlayTime
        };
    }

    private async Task<SongBestResponse> BuildBlueSongBestResponse(uint baid)
    {
        var bestRows = await context.SongBestDataBlue
            .Where(d => d.Baid == baid)
            .AsNoTracking()
            .ToListAsync(HttpContext.RequestAborted);
        var playRows = await context.SongPlayDataBlue
            .Where(d => d.Baid == baid)
            .AsNoTracking()
            .ToListAsync(HttpContext.RequestAborted);
        var favoriteSet = await context.BlueFavoriteSongs
            .Where(d => d.Baid == baid)
            .Select(d => d.SongNo)
            .ToHashSetAsync(HttpContext.RequestAborted);

        var songBestRecords = bestRows
            .GroupBy(d => new { d.SongId, d.Difficulty })
            .Select(group => BuildBlueSongBestData(group, playRows, favoriteSet))
            .ToList();

        return new SongBestResponse
        {
            SongBestData = songBestRecords
        };
    }

    private static SongBestData BuildBlueSongBestData(
        IEnumerable<SongBestDatumBlue> bestRows,
        IReadOnlyCollection<SongPlayDatumBlue> playRows,
        IReadOnlySet<uint> favoriteSet)
    {
        var rows = bestRows.ToList();
        var primary = rows.FirstOrDefault(d => !d.IsShin) ?? rows.First();
        var alternate = rows.FirstOrDefault(d => d.IsShin && d != primary);
        var matchingPlays = playRows
            .Where(d => d.SongId == primary.SongId && d.Difficulty == primary.Difficulty)
            .ToList();
        var bestPlay = matchingPlays.MaxBy(d => d.Score);

        var result = new SongBestData
        {
            SongId = primary.SongId,
            Difficulty = primary.Difficulty,
            BestScore = primary.BestScore,
            BestRate = primary.BestRate,
            BestCrown = primary.BestCrown,
            BestScoreRank = ScoreRank.None,
            IsFavorite = favoriteSet.Contains(primary.SongId),
            PlayCount = matchingPlays.Count,
            ClearCount = matchingPlays.Count(d => d.Crown >= CrownType.Clear),
            FullComboCount = matchingPlays.Count(d => d.Crown >= CrownType.Gold),
            PerfectCount = matchingPlays.Count(d => d.Crown >= CrownType.Dondaful),
            RecentPlayData = matchingPlays.Select(MapBluePlayToDto).ToList()
        };

        if (matchingPlays.Count > 0)
        {
            result.LastPlayTime = matchingPlays.Max(d => d.PlayTime);
        }

        if (bestPlay is not null)
        {
            result.PlayTime = bestPlay.PlayTime;
            result.GoodCount = bestPlay.GoodCount;
            result.OkCount = bestPlay.OkCount;
            result.MissCount = bestPlay.MissCount;
            result.ComboCount = bestPlay.ComboCount;
            result.HitCount = bestPlay.HitCount;
            result.DrumrollCount = bestPlay.PoundCount;
        }

        if (alternate is not null)
        {
            result.AlternateScore = new ScoreFacet
            {
                Label = "Shin",
                BestScore = alternate.BestScore,
                BestRate = alternate.BestRate,
                BestCrown = alternate.BestCrown
            };
        }

        return result;
    }

    private static SongPlayDatumDto MapBluePlayToDto(SongPlayDatumBlue play)
    {
        return new SongPlayDatumDto
        {
            SongId = play.SongId,
            SongNumber = play.SongId,
            Difficulty = play.Difficulty,
            Crown = play.Crown,
            Score = play.Score,
            ScoreRate = play.ScoreRate,
            ScoreRank = ScoreRank.None,
            GoodCount = play.GoodCount,
            OkCount = play.OkCount,
            MissCount = play.MissCount,
            ComboCount = play.ComboCount,
            HitCount = play.HitCount,
            DrumrollCount = play.PoundCount,
            PlayTime = play.PlayTime
        };
    }
}
