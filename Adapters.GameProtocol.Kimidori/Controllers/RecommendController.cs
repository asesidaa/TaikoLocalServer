namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;

[ApiController]
public sealed class RecommendController : BaseProtocolController<RecommendController>
{
    [HttpPost(KimidoriRoutePrefixes.Final + "/recommend.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> FinalRecommend([FromBody] FinalWire.RecommendRequest request)
    {
        Logger.LogInformation("Kimidori final Recommend request: {@Request}", request);
        var common = await Mediator.Send(
            new GetRecommendQuery(GameEra.Kimidori, request.GenderType, request.PlayerAge),
            HttpContext.RequestAborted);
        return Ok(FinalRecommendMappers.Map(common));
    }

    [HttpPost(KimidoriRoutePrefixes.Game + "/recommend.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> Recommend([FromBody] RecommendRequest request)
    {
        Logger.LogInformation("Kimidori Recommend request: {@Request}", request);
        var common = await Mediator.Send(
            new GetRecommendQuery(GameEra.Kimidori, request.GenderType, request.PlayerAge),
            HttpContext.RequestAborted);
        return Ok(RecommendMappers.Map(common));
    }
}
