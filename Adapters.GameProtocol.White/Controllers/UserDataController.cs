namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route("/v07r00/chassis/userdata.php")]
public sealed class UserDataController : BaseProtocolController<UserDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult UserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation(
            "White scaffold userdata.php request: Baid={Baid}, ChassisId={ChassisId}, ShopId={ShopId}",
            request.Baid,
            request.ChassisId,
            request.ShopId);
        return Ok(new UserDataResponse { Result = 1 });
    }
}
