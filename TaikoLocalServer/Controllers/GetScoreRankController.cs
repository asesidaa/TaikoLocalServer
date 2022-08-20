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
        var manager = MusicAttributeManager.Instance;
        var response = new GetScoreRankResponse
        {
            Result = 1,
            IkiScoreRankFlg = GZipBytesUtil.GetGZipBytes(new byte[manager.Musics.Count * 9]),
            KiwamiScoreRankFlg = GZipBytesUtil.GetGZipBytes(new byte[manager.Musics.Count * 9]),
            MiyabiScoreRankFlg = GZipBytesUtil.GetGZipBytes(new byte[manager.Musics.Count * 9])
        };
        
        return Ok(response);
    }
}