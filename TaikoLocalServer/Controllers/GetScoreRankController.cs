using Microsoft.AspNetCore.Http;
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
        var response = new GetScoreRankResponse
        {
            Result = 1,
            IkiScoreRankFlg = GZipBytesUtil.GetGZipBytes(new byte[1000]),
            KiwamiScoreRankFlg = GZipBytesUtil.GetGZipBytes(new byte[1000]),
            MiyabiScoreRankFlg = GZipBytesUtil.GetGZipBytes(new byte[1000])
        };
        
        return Ok(response);
    }
}