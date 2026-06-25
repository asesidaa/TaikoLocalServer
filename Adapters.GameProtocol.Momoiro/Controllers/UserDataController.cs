using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class UserDataController : BaseProtocolController<UserDataController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/userdata.php")]
    [Produces("application/protobuf")]
    public IActionResult UserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation(
            "Momoiro userdata.php scaffold request: Baid={Baid}, ChassisId={ChassisId}",
            request.Baid,
            request.ChassisId);

        return Ok(new UserDataResponse { Result = 1 });
    }
}
