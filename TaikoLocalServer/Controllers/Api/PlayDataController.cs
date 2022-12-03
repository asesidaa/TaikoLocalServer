using SharedProject.Models.Responses;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class PlayDataController : BaseController<PlayDataController>
{
    private readonly ISongBestDatumService songBestDatumService;

    private readonly ISongPlayDatumService songPlayDatumService;
    private readonly IUserDatumService userDatumService;

    public PlayDataController(IUserDatumService userDatumService, ISongBestDatumService songBestDatumService,
        ISongPlayDatumService songPlayDatumService)
    {
        this.userDatumService = userDatumService;
        this.songBestDatumService = songBestDatumService;
        this.songPlayDatumService = songPlayDatumService;
    }

    [HttpGet("{baid}")]
    public async Task<ActionResult<SongBestResponse>> GetSongBestRecords(uint baid)
    {
        var user = await userDatumService.GetFirstUserDatumOrNull(baid);
        if (user is null) return NotFound();

        var songBestRecords = await songBestDatumService.GetAllSongBestAsModel(baid);
        var playLogs = await songPlayDatumService.GetSongPlayDatumByBaid(baid);
        foreach (var songBestData in songBestRecords)
        {
            var songPlayLogs = playLogs.Where(datum => datum.SongId == songBestData.SongId &&
                                                       datum.Difficulty == songBestData.Difficulty).ToList();
            songBestData.PlayCount = songPlayLogs.Count;
            songBestData.ClearCount = songPlayLogs.Count(datum => datum.Crown >= CrownType.Clear);
            songBestData.FullComboCount = songPlayLogs.Count(datum => datum.Crown >= CrownType.Gold);
            songBestData.PerfectCount = songPlayLogs.Count(datum => datum.Crown >= CrownType.Dondaful);
        }

        var favoriteSongs = await userDatumService.GetFavoriteSongIds(baid);
        var favoriteSet = favoriteSongs.ToHashSet();
        foreach (var songBestRecord in songBestRecords.Where(songBestRecord =>
                     favoriteSet.Contains(songBestRecord.SongId))) songBestRecord.IsFavorite = true;

        return Ok(new SongBestResponse
        {
            SongBestData = songBestRecords
        });
    }
}