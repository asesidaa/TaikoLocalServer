namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlayHistoryController(ITaikoDbContext context) : BaseAdminController<PlayHistoryController>
{
    [HttpGet("{baid}")]
    public Task<ActionResult<SongHistoryResponse>> GetSongHistory(uint baid)
        => GetSongHistory(nameof(GameEra.Nijiiro), baid);

    [HttpGet("/api/{era}/[controller]/{baid}")]
    public async Task<ActionResult<SongHistoryResponse>> GetSongHistory(string era, uint baid)
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
            GameEra.Nijiiro => Ok(await BuildNijiiroSongHistory(baid)),
            GameEra.Green => Ok(await BuildGreenSongHistory(baid)),
            _ => EraRoute.BadEra(era)
        };
    }

    private async Task<SongHistoryResponse> BuildNijiiroSongHistory(uint baid)
    {
        var saveData = await context.GetOrCreateNijiiroSaveDataAsync(baid, HttpContext.RequestAborted);

        var favoriteSet = saveData.FavoriteSongsArray.ToHashSet();
        var playLogs = await context.SongPlayDataNijiiro
            .Where(d => d.Baid == baid)
            .AsNoTracking()
            .ToListAsync(HttpContext.RequestAborted);
        var songHistory = playLogs.Select(play => new SongHistoryData
            {
                SongId = play.SongId,
                Difficulty = play.Difficulty,
                Score = play.Score,
                ScoreRank = play.ScoreRank,
                Crown = play.Crown,
                GoodCount = play.GoodCount,
                OkCount = play.OkCount,
                MissCount = play.MissCount,
                HitCount = play.HitCount,
                DrumrollCount = play.DrumrollCount,
                ComboCount = play.ComboCount,
                PlayTime = play.PlayTime,
                SongNumber = play.SongNumber,
                PlaySetting = PlaySettingConverter.ShortToPlaySetting((short)play.OptionSetting),
                IsFavorite = favoriteSet.Contains(play.SongId)
            })
            .ToList();

        return new SongHistoryResponse { SongHistoryData = songHistory };
    }

    private async Task<SongHistoryResponse> BuildGreenSongHistory(uint baid)
    {
        var playLogs = await context.SongPlayDataGreen
            .Where(d => d.Baid == baid)
            .AsNoTracking()
            .ToListAsync(HttpContext.RequestAborted);
        var favoriteSet = await context.GreenFavoriteSongs
            .Where(d => d.Baid == baid)
            .Select(d => d.SongNo)
            .ToHashSetAsync(HttpContext.RequestAborted);

        var songHistory = playLogs.Select(play => new SongHistoryData
            {
                SongId = play.SongId,
                Difficulty = play.Difficulty,
                Score = play.Score,
                ScoreRank = ScoreRank.None,
                Crown = play.Crown,
                GoodCount = play.GoodCount,
                OkCount = play.OkCount,
                MissCount = play.MissCount,
                HitCount = play.HitCount,
                DrumrollCount = play.PoundCount,
                ComboCount = play.ComboCount,
                PlayTime = play.PlayTime,
                SongNumber = play.SongId,
                IsFavorite = favoriteSet.Contains(play.SongId)
            })
            .ToList();

        return new SongHistoryResponse { SongHistoryData = songHistory };
    }
}
