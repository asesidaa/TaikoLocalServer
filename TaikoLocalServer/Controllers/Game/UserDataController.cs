using TaikoLocalServer.Mappers;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class UserDataController : BaseController<UserDataController>
{
    [HttpPost("/v12r08_ww/chassis/userdata_gc6x17o8.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetUserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation("UserData request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new UserDataQuery(request.Baid));
        var response = UserDataMappers.MapToWW08(commonResponse);

        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/userdata.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetUserDataCN00([FromBody] Models.CN00.UserDataRequest request)
    {
        Logger.LogInformation("UserData request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new UserDataQuery((uint)request.Baid));
        var response = UserDataMappers.MapToCN00(commonResponse);

        return Ok(response);
    }
}