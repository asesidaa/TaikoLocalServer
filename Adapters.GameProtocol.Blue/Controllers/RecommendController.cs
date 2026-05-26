namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/recommend.php")]
public class RecommendController : BaseProtocolController<RecommendController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Recommend([FromBody] RecommendRequest request)
    {
        Logger.LogInformation("Blue Recommend request: {Request}", request.Stringify());
        return Ok(new RecommendResponse { Result = 1 });
    }
}
