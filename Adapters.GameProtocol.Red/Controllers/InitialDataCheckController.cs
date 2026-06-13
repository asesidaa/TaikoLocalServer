namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r00/chassis/initialdatacheck.php")]
[Route("/v08r01/chassis/initialdatacheck.php")]
public class InitialDataCheckController : BaseProtocolController<InitialDataCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> InitialDataCheck([FromBody] InitialdatacheckRequest request)
    {
        Logger.LogInformation("Red InitialDataCheck request: {@Request}", request);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Red), HttpContext.RequestAborted);
        return Ok(InitialDataMappers.Map(common));
    }
}
