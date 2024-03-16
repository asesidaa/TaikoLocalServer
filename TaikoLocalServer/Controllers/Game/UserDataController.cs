using TaikoLocalServer.Handlers;
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
        var response = UserDataMappers.MapTo3906(commonResponse);

        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/userdata.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetUserData3209([FromBody] Models.v3209.UserDataRequest request)
    {
        Logger.LogInformation("UserData request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new UserDataQuery((uint)request.Baid));
        var response = UserDataMappers.MapTo3209(commonResponse);

        return Ok(response);
    }
}