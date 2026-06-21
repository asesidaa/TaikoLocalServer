namespace TaikoLocalServer.Adapters.GameProtocol.Murasaki.Controllers;

[ApiController]
public sealed class UserDataController : BaseProtocolController<UserDataController>
{
    [HttpPost(MurasakiRoutePrefixes.Game + "/userdata.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> UserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation("Murasaki UserData request: {@Request}", request);
        var common = await Mediator.Send(new Ac15UserDataQuery(request.Baid, GameEra.Murasaki), HttpContext.RequestAborted);

        var response = new UserDataResponse
        {
            Result = common.Result
        };
        UserDataMappers.Apply(common.SongFlags, response);
        UserDataMappers.Apply(common.SongLists, response);
        UserDataMappers.Apply(common.Counters, response);
        UserDataMappers.Apply(common.Display, response);
        UserDataMappers.Apply(common.Recommendations, response);
        if (common.ModeFlags is { } modeFlags)
        {
            UserDataMappers.Apply(modeFlags, response);
        }

        if (common.Reward is { } reward)
        {
            UserDataMappers.Apply(reward, response);
        }

        return Ok(response);
    }
}
