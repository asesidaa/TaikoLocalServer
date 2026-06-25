using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class DefaultSongController : BaseProtocolController<DefaultSongController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/defaultsong.php")]
    [Produces("application/protobuf")]
    public IActionResult DefaultSong([FromBody] DefaultsongRequest request)
    {
        Logger.LogInformation(
            "Momoiro defaultsong.php scaffold request: ChassisId={ChassisId}",
            request.ChassisId);

        return Ok(new DefaultsongResponse { Result = 1 });
    }
}
