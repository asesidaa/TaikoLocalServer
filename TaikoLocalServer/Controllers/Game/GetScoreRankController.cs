using Microsoft.Extensions.Options;
using TaikoLocalServer.Settings;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class GetScoreRankController(ISongBestDatumService songBestDatumService, IOptions<ServerSettings> settings)
    : BaseController<GetScoreRankController>
{
    private readonly ServerSettings settings = settings.Value;

    [HttpPost("/v12r08_ww/chassis/getscorerank_1c8l7y61.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetScoreRank([FromBody] GetScoreRankRequest request)
    {
        Logger.LogInformation("GetScoreRank request : {Request}", request.Stringify());
       
        var scoreRankData = await Handle(request.Baid);
        var response = new GetScoreRankResponse
        {
            Result = 1,
            IkiScoreRankFlg = scoreRankData.IkiScoreRankFlg,
            KiwamiScoreRankFlg = scoreRankData.KiwamiScoreRankFlg,
            MiyabiScoreRankFlg = scoreRankData.MiyabiScoreRankFlg
        };

        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/getscorerank.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetScoreRank3209([FromBody] Models.v3209.GetScoreRankRequest request)
    {
        Logger.LogInformation("GetScoreRank request : {Request}", request.Stringify());
       
        var scoreRankData = await Handle((uint)request.Baid);
        var response = new Models.v3209.GetScoreRankResponse
        {
            Result = 1,
            IkiScoreRankFlg = scoreRankData.IkiScoreRankFlg,
            KiwamiScoreRankFlg = scoreRankData.KiwamiScoreRankFlg,
            MiyabiScoreRankFlg = scoreRankData.MiyabiScoreRankFlg
        };

        return Ok(response);
    }
    
    public record ScoreRankData(byte[] IkiScoreRankFlg, byte[] KiwamiScoreRankFlg, byte[] MiyabiScoreRankFlg);
    
    private async Task<ScoreRankData> Handle(uint baid)
    {
        var songIdMax = settings.EnableMoreSongs ? Constants.MUSIC_ID_MAX_EXPANDED : Constants.MUSIC_ID_MAX;
        var kiwamiScores = new byte[songIdMax   + 1];
        var miyabiScores = new ushort[songIdMax + 1];
        var ikiScores = new ushort[songIdMax    + 1];
        var songBestData = await songBestDatumService.GetAllSongBestData(baid);

        for (var songId = 0; songId < songIdMax; songId++)
        {
            var id = songId;
            kiwamiScores[songId] = songBestData
                .Where(datum => datum.SongId        == id &&
                                datum.BestScoreRank == ScoreRank.Dondaful)
                .Aggregate((byte)0, (flag, datum) => FlagCalculator.ComputeKiwamiScoreRankFlag(flag, datum.Difficulty));

            ikiScores[songId] = songBestData
                .Where(datum => datum.SongId == id &&
                                datum.BestScoreRank is ScoreRank.White or ScoreRank.Bronze or ScoreRank.Silver)
                .Aggregate((ushort)0, (flag, datum) => FlagCalculator.ComputeMiyabiOrIkiScoreRank(flag, datum.BestScoreRank, datum.Difficulty));

            miyabiScores[songId] = songBestData
                .Where(datum => datum.SongId == id &&
                                datum.BestScoreRank is ScoreRank.Gold or ScoreRank.Purple or ScoreRank.Sakura)
                .Aggregate((ushort)0, (flag, datum) => FlagCalculator.ComputeMiyabiOrIkiScoreRank(flag, datum.BestScoreRank, datum.Difficulty));
        }

        return new ScoreRankData(
            GZipBytesUtil.GetGZipBytes(ikiScores),
            GZipBytesUtil.GetGZipBytes(kiwamiScores),
            GZipBytesUtil.GetGZipBytes(miyabiScores)
        );
    }
}