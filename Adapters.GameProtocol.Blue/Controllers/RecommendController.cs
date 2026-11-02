namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/recommend.php")]
public class RecommendController : BaseProtocolController<RecommendController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> Recommend([FromBody] RecommendRequest request)
    {
        Logger.LogInformation("Blue Recommend request: {@Request}", request);
        var common = await Mediator.Send(
            new GetRecommendQuery(GameEra.Blue, request.GenderType, request.PlayerAge),
            HttpContext.RequestAborted);
        return Ok(RecommendMappers.Map(common));
    }
}
