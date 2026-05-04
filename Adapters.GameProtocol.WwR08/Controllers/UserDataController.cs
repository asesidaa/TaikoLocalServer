namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class UserDataController : BaseProtocolController<UserDataController>
{
    [HttpPost("/v12r08_ww/chassis/userdata_gc6x17o8.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetUserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation("UserData request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new UserDataQuery(request.Baid), HttpContext.RequestAborted);
        var response = UserDataMappers.MapToWW08(commonResponse);

        return Ok(response);
    }
}
