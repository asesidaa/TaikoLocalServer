namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/verupcomplete.php")]
public class VerupCompleteController : BaseProtocolController<VerupCompleteController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult VerupComplete([FromBody] VerupCompleteRequest request)
    {
        Logger.LogInformation("Green VerupComplete request: {Request}", request.Stringify());
        return Ok(new VerupCompleteResponse { Result = 1 });
    }
}
