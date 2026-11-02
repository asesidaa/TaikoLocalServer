namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/recommend.php")]
public class RecommendController : BaseProtocolController<RecommendController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> Recommend([FromBody] RecommendRequest request)
    {
        Logger.LogInformation("Yellow Recommend request: {@Request}", request);
        var common = await Mediator.Send(
            new GetRecommendQuery(GameEra.Yellow, request.GenderType, request.PlayerAge),
            HttpContext.RequestAborted);
        return Ok(RecommendMappers.Map(common));
    }
}
