using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class GetTelopController : BaseProtocolController<GetTelopController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/gettelop.php")]
    [Produces("application/protobuf")]
    public IActionResult GetTelop([FromBody] GetTelopRequest request)
    {
        Logger.LogInformation(
            "Momoiro gettelop.php scaffold request: ChassisId={ChassisId}, TelopId={TelopId}",
            request.ChassisId,
            request.TelopId);

        return Ok(new GetTelopResponse { Result = 1 });
    }
}
