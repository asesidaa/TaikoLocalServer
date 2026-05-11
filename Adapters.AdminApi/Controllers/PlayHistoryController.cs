namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlayHistoryController(ITaikoDbContext context) : BaseAdminController<PlayHistoryController>
{
    [HttpGet("{baid}")]
    public async Task<ActionResult<SongHistoryResponse>> GetSongHistory(uint baid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        var user = await context.UserData.FindAsync(baid);
        if (user is null)
        {
            return NotFound();
        }
        var saveData = await context.GetOrCreateNijiiroSaveDataAsync(baid, HttpContext.RequestAborted);

        var playLogs = await context.SongPlayDataNijiiro.Where(d => d.Baid == baid).ToListAsync();
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
                PlaySetting = PlaySettingConverter.ShortToPlaySetting((short)play.OptionSetting)
            })
            .ToList();

        var favoriteSet = saveData.FavoriteSongsArray.ToHashSet();
        foreach (var song in songHistory.Where(song => favoriteSet.Contains(song.SongId)))
        {
            song.IsFavorite = true;
        }

        return Ok(new SongHistoryResponse { SongHistoryData = songHistory });
    }
}
