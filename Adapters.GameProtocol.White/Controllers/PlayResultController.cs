using LegacyWire = TaikoLocalServer.Adapters.GameProtocol.White.LegacyWire;

namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
public sealed class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost(WhiteRoutePrefixes.Final + "/playresult.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("White PlayResult request: {@Request}", request);
        var playResult = PlayResultMappers.Map(request);

        var result = await Mediator.Send(
            new UpdateAc15PlayResultCommand(request.Baid, GameEra.White, playResult),
            HttpContext.RequestAborted);

        return Ok(PlayResultMappers.Map(result));
    }

    [HttpPost(WhiteRoutePrefixes.Compatibility + "/playresult.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> LegacyPlayResult([FromBody] LegacyWire.PlayResultRequest request)
    {
        Logger.LogInformation("White legacy PlayResult request: {@Request}", request);
        var playResult = LegacyWire.LegacyPlayResultMappers.Map(request);

        var result = await Mediator.Send(
            new UpdateAc15PlayResultCommand(request.Baid, GameEra.White, playResult),
            HttpContext.RequestAborted);

        return Ok(LegacyWire.LegacyPlayResultMappers.Map(result));
    }
}
