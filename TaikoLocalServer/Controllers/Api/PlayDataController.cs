using SharedProject.Models.Responses;
using TaikoLocalServer.Services.Interfaces;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class PlayDataController : BaseController<PlayDataController>
{
    private readonly IUserDatumService userDatumService;

    private readonly ISongBestDatumService songBestDatumService;

    public PlayDataController(IUserDatumService userDatumService, ISongBestDatumService songBestDatumService)
    {
        this.userDatumService = userDatumService;
        this.songBestDatumService = songBestDatumService;
    }

    [HttpGet("{baid}")]
    public async Task<ActionResult<SongBestResponse>> GetSongBestRecords(uint baid)
    {
        var user = await userDatumService.GetFirstUserDatumOrNull(baid);
        if (user is null)
        {
            return NotFound();
        }

        var songBestRecords = await songBestDatumService.GetAllSongBestAsModel(baid);
        var favoriteSongs = await userDatumService.GetFavoriteSongIds(baid);
        var favoriteSet = favoriteSongs.ToHashSet();
        foreach (var songBestRecord in songBestRecords.Where(songBestRecord => favoriteSet.Contains(songBestRecord.SongId)))
        {
            songBestRecord.IsFavorite = true;
        }

        return Ok(new SongBestResponse
        {
            SongBestData = songBestRecords
        });
    }
}