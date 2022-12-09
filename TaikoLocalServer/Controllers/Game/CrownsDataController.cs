using Microsoft.Extensions.Options;
using TaikoLocalServer.Settings;

namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/crownsdata.php")]
[ApiController]
public class CrownsDataController : BaseController<CrownsDataController>
{
    private readonly ServerSettings settings;
    private readonly ISongBestDatumService songBestDatumService;

    public CrownsDataController(ISongBestDatumService songBestDatumService, IOptions<ServerSettings> settings)
    {
        this.songBestDatumService = songBestDatumService;
        this.settings = settings.Value;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> CrownsData([FromBody] CrownsDataRequest request)
    {
        Logger.LogInformation("CrownsData request : {Request}", request.Stringify());

        var songBestData = await songBestDatumService.GetAllSongBestData(request.Baid);

        var songIdMax = settings.EnableMoreSongs ? Constants.MUSIC_ID_MAX_EXPANDED : Constants.MUSIC_ID_MAX;
        var crown = new ushort[songIdMax + 1];
        var dondafulCrown = new byte[songIdMax + 1];

        for (var songId = 0; songId < songIdMax; songId++)
        {
            var id = songId;
            dondafulCrown[songId] = songBestData
                // Select song of this song id with dondaful crown 
                .Where(datum => datum.SongId == id &&
                                datum.BestCrown == CrownType.Dondaful)
                // Calculate flag according to difficulty
                .Aggregate((byte)0, (flag, datum) => FlagCalculator.ComputeDondafulCrownFlag(flag, datum.Difficulty));

            crown[songId] = songBestData
                // Select song of this song id with clear or fc crown
                .Where(datum => datum.SongId == id &&
                                datum.BestCrown is CrownType.Clear or CrownType.Gold)
                // Calculate flag according to difficulty
                .Aggregate((ushort)0,
                    (flag, datum) => FlagCalculator.ComputeCrownFlag(flag, datum.BestCrown, datum.Difficulty));
        }

        var response = new CrownsDataResponse
        {
            Result = 1,
            CrownFlg = GZipBytesUtil.GetGZipBytes(crown),
            DondafulCrownFlg = GZipBytesUtil.GetGZipBytes(dondafulCrown)
        };

        return Ok(response);
    }
}