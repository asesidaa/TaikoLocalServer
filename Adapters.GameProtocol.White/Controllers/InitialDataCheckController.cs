namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route(WhiteRoutePrefixes.Final + "/initialdatacheck.php")]
[Route(WhiteRoutePrefixes.Compatibility + "/initialdatacheck.php")]
public sealed class InitialDataCheckController : BaseProtocolController<InitialDataCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> InitialDataCheck([FromBody] InitialdatacheckRequest request)
    {
        Logger.LogInformation("White InitialDataCheck request: {@Request}", request);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.White), HttpContext.RequestAborted);
        return Ok(InitialDataMappers.Map(common));
    }
}
