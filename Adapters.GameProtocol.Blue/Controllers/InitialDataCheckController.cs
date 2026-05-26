namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/initialdatacheck.php")]
public class InitialDataCheckController : BaseProtocolController<InitialDataCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult InitialDataCheck([FromBody] InitialdatacheckRequest request)
    {
        Logger.LogInformation("Blue InitialDataCheck request: {Request}", request.Stringify());
        return Ok(new InitialdatacheckResponse { Result = 1 });
    }
}
