using TaikoLocalServer.Adapters.GameProtocol.Shared.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;

[ApiController]
[Route("/v01r00/chassis/verupcomplete.php")]
public sealed class VerupCompleteController : BaseProtocolController<VerupCompleteController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult VerupComplete([FromBody] VerupCompleteRequest request)
    {
        Logger.LogInformation("VerupComplete request: {@Request}", request);
        return Ok(new VerupCompleteResponse { Result = 1 });
    }
}
