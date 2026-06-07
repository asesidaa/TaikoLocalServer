namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class UserDataController : BaseProtocolController<UserDataController>
{
    [HttpPost("/v12r00_cn/chassis/userdata.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetUserDataCN00([FromBody] UserDataRequest request)
    {
        Logger.LogInformation("UserData request : {@Request}", request);

        var commonResponse = await Mediator.Send(new UserDataQuery((uint)request.Baid, GameEra.Nijiiro), HttpContext.RequestAborted);
        var response = UserDataMappers.MapToCN00(commonResponse);

        return Ok(response);
    }
}
