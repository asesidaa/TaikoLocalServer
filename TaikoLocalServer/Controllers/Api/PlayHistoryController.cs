using Microsoft.Extensions.Options;
using SharedProject.Models;
using SharedProject.Models.Responses;
using TaikoLocalServer.Filters;
using TaikoLocalServer.Settings;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class PlayHistoryController(IUserDatumService userDatumService, ISongPlayDatumService songPlayDatumService,
    IAuthService authService, IOptions<AuthSettings> settings) : BaseController<PlayDataController>
{
    private readonly AuthSettings authSettings = settings.Value;
    
    [HttpGet("{baid}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<ActionResult<SongHistoryResponse>> GetSongHistory(uint baid)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = authService.ExtractTokenInfo(HttpContext);
            if (tokenInfo is null)
            {
                return Unauthorized();
            }
            
            if (tokenInfo.Value.baid != baid && !tokenInfo.Value.isAdmin)
            {
                return Forbid();
            }
        }
        
        var user = await userDatumService.GetFirstUserDatumOrNull(baid);
        if (user is null)
        {
            return NotFound();
        }

        var playLogs = await songPlayDatumService.GetSongPlayDatumByBaid(baid);
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
                SongNumber = play.SongNumber
            })
            .ToList();

        var favoriteSongs = await userDatumService.GetFavoriteSongIds(baid);
        var favoriteSet = favoriteSongs.ToHashSet();
        foreach (var song in songHistory.Where(song => favoriteSet.Contains(song.SongId)))
        {
            song.IsFavorite = true;
        }

        return Ok(new SongHistoryResponse{SongHistoryData = songHistory});
    }
}