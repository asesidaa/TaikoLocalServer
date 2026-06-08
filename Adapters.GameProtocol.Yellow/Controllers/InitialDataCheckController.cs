namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/initialdatacheck.php")]
public class InitialDataCheckController : BaseProtocolController<InitialDataCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> InitialDataCheck([FromBody] InitialdatacheckRequest request)
    {
        Logger.LogInformation("Yellow InitialDataCheck request: {@Request}", request);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Yellow), HttpContext.RequestAborted);
        return Ok(InitialDataMappers.Map(common));
    }
}
