using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class SongHashController : BaseProtocolController<SongHashController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/songhash.php")]
    [Produces("application/protobuf")]
    public IActionResult SongHash([FromBody] SonghashRequest request)
    {
        Logger.LogInformation(
            "Momoiro songhash.php scaffold request: ChassisId={ChassisId}",
            request.ChassisId);

        return Ok(new SonghashResponse { Result = 1 });
    }
}
