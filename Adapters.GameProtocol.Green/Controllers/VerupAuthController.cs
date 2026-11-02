namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/verupauth.php")]
public class VerupAuthController : BaseProtocolController<VerupAuthController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult VerupAuth([FromBody] VerupAuthRequest request)
    {
        Logger.LogInformation("Green VerupAuth request: {@Request}", request);
        return Ok(new VerupAuthResponse { Result = 1 });
    }
}
