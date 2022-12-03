using Microsoft.Extensions.Options;
using TaikoLocalServer.Settings;

namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/getscorerank.php")]
[ApiController]
public class GetScoreRankController : BaseController<GetScoreRankController>
{
    private readonly ServerSettings settings;
    private readonly ISongBestDatumService songBestDatumService;

    public GetScoreRankController(ISongBestDatumService songBestDatumService, IOptions<ServerSettings> settings)
    {
        this.songBestDatumService = songBestDatumService;
        this.settings = settings.Value;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetScoreRank([FromBody] GetScoreRankRequest request)
    {
        Logger.LogInformation("GetScoreRank request : {Request}", request.Stringify());
        var songIdMax = settings.EnableMoreSongs ? Constants.MUSIC_ID_MAX_EXPANDED : Constants.MUSIC_ID_MAX;
        var kiwamiScores = new byte[songIdMax + 1];
        var miyabiScores = new ushort[songIdMax + 1];
        var ikiScores = new ushort[songIdMax + 1];
        var songBestData = await songBestDatumService.GetAllSongBestData(request.Baid);

        for (var songId = 0; songId < songIdMax; songId++)
        {
            var id = songId;
            kiwamiScores[songId] = songBestData
                .Where(datum => datum.SongId == id &&
                                datum.BestScoreRank == ScoreRank.Dondaful)
                .Aggregate((byte)0, (flag, datum) => FlagCalculator.ComputeKiwamiScoreRankFlag(flag, datum.Difficulty));

            ikiScores[songId] = songBestData
                .Where(datum => datum.SongId == id &&
                                datum.BestScoreRank is ScoreRank.White or ScoreRank.Bronze or ScoreRank.Silver)
                .Aggregate((ushort)0,
                    (flag, datum) =>
                        FlagCalculator.ComputeMiyabiOrIkiScoreRank(flag, datum.BestScoreRank, datum.Difficulty));

            miyabiScores[songId] = songBestData
                .Where(datum => datum.SongId == id &&
                                datum.BestScoreRank is ScoreRank.Gold or ScoreRank.Purple or ScoreRank.Sakura)
                .Aggregate((ushort)0,
                    (flag, datum) =>
                        FlagCalculator.ComputeMiyabiOrIkiScoreRank(flag, datum.BestScoreRank, datum.Difficulty));
        }

        var response = new GetScoreRankResponse
        {
            Result = 1,
            IkiScoreRankFlg = GZipBytesUtil.GetGZipBytes(ikiScores),
            KiwamiScoreRankFlg = GZipBytesUtil.GetGZipBytes(kiwamiScores),
            MiyabiScoreRankFlg = GZipBytesUtil.GetGZipBytes(miyabiScores)
        };

        return Ok(response);
    }
}