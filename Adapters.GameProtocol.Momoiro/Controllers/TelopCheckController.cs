using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class TelopCheckController : BaseProtocolController<TelopCheckController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/telopcheck.php")]
    [Produces("application/protobuf")]
    public IActionResult TelopCheck([FromBody] TelopCheckRequest request)
    {
        Logger.LogInformation(
            "Momoiro telopcheck.php scaffold request: ChassisId={ChassisId}",
            request.ChassisId);

        return Ok(new TelopCheckResponse { Result = 1 });
    }
}
