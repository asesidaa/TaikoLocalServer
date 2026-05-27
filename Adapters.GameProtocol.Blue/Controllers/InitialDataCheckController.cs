namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/initialdatacheck.php")]
public class InitialDataCheckController : BaseProtocolController<InitialDataCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> InitialDataCheck([FromBody] InitialdatacheckRequest request)
    {
        Logger.LogInformation("Blue InitialDataCheck request: {Request}", request.Stringify());
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Blue), HttpContext.RequestAborted);
        return Ok(InitialDataMappers.Map(common));
    }
}
