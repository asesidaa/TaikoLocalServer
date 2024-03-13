using Riok.Mapperly.Abstractions;
using SharedProject.Models.Responses;
using SharedProject.Models;
using GameDatabase.Entities;

namespace TaikoLocalServer.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayDataController : BaseController<PlayDataController>
    {
        private readonly IUserDatumService userDatumService;
        private readonly ISongBestDatumService songBestDatumService;
        private readonly ISongPlayDatumService songPlayDatumService;
        private readonly SongBestResponseMapper _songBestResponseMapper; // Inject SongBestResponseMapper

        public PlayDataController(IUserDatumService userDatumService, ISongBestDatumService songBestDatumService,
            ISongPlayDatumService songPlayDatumService, SongBestResponseMapper songBestResponseMapper)
        {
            this.userDatumService = userDatumService;
            this.songBestDatumService = songBestDatumService;
            this.songPlayDatumService = songPlayDatumService;
            _songBestResponseMapper = songBestResponseMapper; // Assign the injected mapper
        }

        [HttpGet("{baid}")]
        public async Task<ActionResult<SongBestResponse>> GetSongBestRecords(uint baid)
        {
            {
                var user = await userDatumService.GetFirstUserDatumOrNull(baid);
                if (user is null)
                {
                    return NotFound();
                }

                var songBestRecords = await songBestDatumService.GetAllSongBestAsModel(baid);
                var songPlayData = await songPlayDatumService.GetSongPlayDatumByBaid(baid);
                foreach (var songBestData in songBestRecords)
                {
                    var songPlayLogs = songPlayData.Where(datum => datum.SongId == songBestData.SongId &&
                                                               datum.Difficulty == songBestData.Difficulty).ToList();
                    songBestData.PlayCount = songPlayLogs.Count;
                    songBestData.ClearCount = songPlayLogs.Count(datum => datum.Crown >= CrownType.Clear);
                    songBestData.FullComboCount = songPlayLogs.Count(datum => datum.Crown >= CrownType.Gold);
                    songBestData.PerfectCount = songPlayLogs.Count(datum => datum.Crown >= CrownType.Dondaful);
                }
                var favoriteSongs = await userDatumService.GetFavoriteSongIds(baid);
                var favoriteSet = favoriteSongs.ToHashSet();
                foreach (var songBestRecord in songBestRecords.Where(songBestRecord => favoriteSet.Contains(songBestRecord.SongId)))
                {
                    songBestRecord.IsFavorite = true;
                }

                foreach (var songBestRecord in songBestRecords)
                {
                    songBestRecord.RecentPlayData = songPlayData
                        .Where(datum => datum.SongId == songBestRecord.SongId && datum.Difficulty == songBestRecord.Difficulty)
                        .Select(SongBestResponseMapper.MapToDto)
                        .ToList();
                }

                return Ok(new SongBestResponse
                {
                    SongBestData = songBestRecords
                });
            }
        }
    }

    [Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
    public partial class SongBestResponseMapper
    {
        public static partial SongPlayDatumDto MapToDto(SongPlayDatum entity);
    }
}