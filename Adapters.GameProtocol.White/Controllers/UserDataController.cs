using LegacyWire = TaikoLocalServer.Adapters.GameProtocol.White.LegacyWire;

namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
public sealed class UserDataController : BaseProtocolController<UserDataController>
{
    [HttpPost(WhiteRoutePrefixes.Final + "/userdata.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> UserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation("White UserData request: {@Request}", request);
        var common = await Mediator.Send(new Ac15UserDataQuery(request.Baid, GameEra.White), HttpContext.RequestAborted);
        var response = new UserDataResponse
        {
            Result = common.Result
        };
        ApplySections(common, response);

        return Ok(response);
    }

    private static void ApplySections(Ac15UserDataResponse common, UserDataResponse response)
    {
        UserDataMappers.Apply(common.SongFlags, response);
        UserDataMappers.Apply(common.SongLists, response);
        UserDataMappers.Apply(common.Recommendations, response);
        UserDataMappers.Apply(common.Counters, response);
        UserDataMappers.Apply(common.Display, response);

        if (common.ModeFlags is { } modeFlags)
        {
            UserDataMappers.Apply(modeFlags, response);
        }

        if (common.Tutorial is { } tutorial)
        {
            UserDataMappers.Apply(tutorial, response);
        }

        if (common.Reward is { } reward)
        {
            UserDataMappers.Apply(reward, response);
        }
    }

    [HttpPost(WhiteRoutePrefixes.Compatibility + "/userdata.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> LegacyUserData([FromBody] LegacyWire.UserDataRequest request)
    {
        Logger.LogInformation("White legacy UserData request: {@Request}", request);
        var common = await Mediator.Send(new Ac15UserDataQuery(request.Baid, GameEra.White), HttpContext.RequestAborted);
        var response = new LegacyWire.UserDataResponse
        {
            Result = common.Result
        };
        ApplyLegacySections(common, response);

        return Ok(response);
    }

    private static void ApplyLegacySections(Ac15UserDataResponse common, LegacyWire.UserDataResponse response)
    {
        LegacyWire.LegacyUserDataMappers.Apply(common.SongFlags, response);
        LegacyWire.LegacyUserDataMappers.Apply(common.SongLists, response);
        LegacyWire.LegacyUserDataMappers.Apply(common.Recommendations, response);
        LegacyWire.LegacyUserDataMappers.Apply(common.Counters, response);
        LegacyWire.LegacyUserDataMappers.Apply(common.Display, response);

        if (common.ModeFlags is { } modeFlags)
        {
            LegacyWire.LegacyUserDataMappers.Apply(modeFlags, response);
        }

        if (common.Tutorial is { } tutorial)
        {
            LegacyWire.LegacyUserDataMappers.Apply(tutorial, response);
        }

        if (common.Reward is { } reward)
        {
            LegacyWire.LegacyUserDataMappers.Apply(reward, response);
        }
    }
}
