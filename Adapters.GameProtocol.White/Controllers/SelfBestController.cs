using LegacyWire = TaikoLocalServer.Adapters.GameProtocol.White.LegacyWire;

namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
public sealed class SelfBestController : BaseProtocolController<SelfBestController>
{
    [HttpPost(WhiteRoutePrefixes.Final + "/selfbest.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> SelfBest([FromBody] SelfBestRequest request)
    {
        Logger.LogInformation("White SelfBest request: {@Request}", request);
        var common = await Mediator.Send(
            new GetSelfBestQuery(request.Baid, GameEra.White, request.Level, request.ArySongNoes ?? []),
            HttpContext.RequestAborted);
        return Ok(SelfBestMappers.Map(common));
    }

    [HttpPost(WhiteRoutePrefixes.Compatibility + "/selfbest.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> LegacySelfBest([FromBody] LegacyWire.SelfBestRequest request)
    {
        Logger.LogInformation("White legacy SelfBest request: {@Request}", request);
        var common = await Mediator.Send(
            new GetSelfBestQuery(request.Baid, GameEra.White, request.Level, request.ArySongNoes ?? []),
            HttpContext.RequestAborted);
        return Ok(LegacyWire.LegacySelfBestMappers.Map(common));
    }
}
