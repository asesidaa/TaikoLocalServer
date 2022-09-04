namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/getscorerank.php")]
[ApiController]
public class GetScoreRankController : BaseController<GetScoreRankController>
{
    private readonly TaikoDbContext context;
    
    public GetScoreRankController(TaikoDbContext context)
    {
        this.context = context;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetScoreRank([FromBody] GetScoreRankRequest request)
    {
        Logger.LogInformation("GetScoreRank request : {Request}", request.Stringify());
        var kiwamiScores = new byte[Constants.KIWAMI_SCORE_RANK_ARRAY_SIZE];
        var miyabiScores = new ushort[Constants.MIYABI_CORE_RANK_ARRAY_SIZE];
        var ikiScores = new ushort[Constants.IKI_CORE_RANK_ARRAY_SIZE];
        var songBestData = context.SongBestData.Where(datum => datum.Baid == request.Baid).ToList();
        
        for (var songId = 0; songId < Constants.MUSIC_ID_MAX; songId++)
        {
            var id = songId;
            kiwamiScores[songId] = songBestData
                .Where(datum => datum.SongId == id &&
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