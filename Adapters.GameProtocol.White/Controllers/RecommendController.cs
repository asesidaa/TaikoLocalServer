using LegacyWire = TaikoLocalServer.Adapters.GameProtocol.White.LegacyWire;

namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
public sealed class RecommendController : BaseProtocolController<RecommendController>
{
    [HttpPost(WhiteRoutePrefixes.Final + "/recommend.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> Recommend([FromBody] RecommendRequest request)
    {
        Logger.LogInformation("White Recommend request: {@Request}", request);
        var common = await Mediator.Send(
            new GetRecommendQuery(GameEra.White, request.GenderType, request.PlayerAge),
            HttpContext.RequestAborted);
        return Ok(RecommendMappers.Map(common));
    }

    [HttpPost(WhiteRoutePrefixes.Compatibility + "/recommend.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> LegacyRecommend([FromBody] LegacyWire.RecommendRequest request)
    {
        Logger.LogInformation("White legacy Recommend request: {@Request}", request);
        var common = await Mediator.Send(
            new GetRecommendQuery(GameEra.White, request.GenderType, request.PlayerAge),
            HttpContext.RequestAborted);
        return Ok(LegacyWire.LegacyRecommendMappers.Map(common));
    }
}
