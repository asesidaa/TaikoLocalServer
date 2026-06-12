namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r01/chassis/recommend.php")]
public class RecommendController : BaseProtocolController<RecommendController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Recommend([FromBody] RecommendRequest request)
    {
        Logger.LogInformation("Red route probe recommend.php request: {@Request}", request);
        return Ok(new RecommendResponse
        {
            Result = 1,
            RecommendBestSongs = []
        });
    }
}
