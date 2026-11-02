namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/recommend.php")]
public class RecommendController : BaseProtocolController<RecommendController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> Recommend([FromBody] RecommendRequest request)
    {
        Logger.LogInformation("Green Recommend request: {@Request}", request);
        var common = await Mediator.Send(
            new GetRecommendQuery(GameEra.Green, request.GenderType, request.PlayerAge),
            HttpContext.RequestAborted);
        return Ok(RecommendMappers.Map(common));
    }
}
