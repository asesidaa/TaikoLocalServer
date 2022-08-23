using Microsoft.AspNetCore.Http;
using TaikoLocalServer.Common;
using TaikoLocalServer.Common.Enums;
using TaikoLocalServer.Utils;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/getscorerank.php")]
[ApiController]
public class GetScoreRankController : ControllerBase
{
    private readonly ILogger<GetScoreRankController> logger;
    public GetScoreRankController(ILogger<GetScoreRankController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetScoreRank([FromBody] GetScoreRankRequest request)
    {
        logger.LogInformation("GetScoreRank request : {Request}", request.Stringify());
        var kiwamiScores = new byte[Constants.KIWAMI_SCORE_RANK_ARRAY_SIZE];
        var miyabiScores = new ushort[Constants.MIYABI_CORE_RANK_ARRAY_SIZE];
        var ikiScores = new ushort[Constants.IKI_CORE_RANK_ARRAY_SIZE];
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