using TaikoLocalServer.Adapters.GameProtocol.Shared.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;

[ApiController]
[Route("/v01r00/chassis/verupauth.php")]
public sealed class VerupAuthController : BaseProtocolController<VerupAuthController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult VerupAuth([FromBody] VerupAuthRequest request)
    {
        Logger.LogInformation("VerupAuth request: {Request}", request.Stringify());
        return Ok(new VerupAuthResponse { Result = 1 });
    }
}
