using LegacyWire = TaikoLocalServer.Adapters.GameProtocol.White.LegacyWire;

namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
public sealed class InitialDataCheckController : BaseProtocolController<InitialDataCheckController>
{
    [HttpPost(WhiteRoutePrefixes.Final + "/initialdatacheck.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> InitialDataCheck([FromBody] InitialdatacheckRequest request)
    {
        Logger.LogInformation("White InitialDataCheck request: {@Request}", request);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.White), HttpContext.RequestAborted);
        return Ok(InitialDataMappers.Map(common));
    }

    [HttpPost(WhiteRoutePrefixes.Compatibility + "/initialdatacheck.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> LegacyInitialDataCheck([FromBody] LegacyWire.InitialdatacheckRequest request)
    {
        Logger.LogInformation("White legacy InitialDataCheck request: {@Request}", request);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.White), HttpContext.RequestAborted);
        return Ok(LegacyWire.LegacyInitialDataMappers.Map(common));
    }
}
