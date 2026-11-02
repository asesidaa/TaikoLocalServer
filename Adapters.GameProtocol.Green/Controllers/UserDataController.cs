namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/userdata.php")]
public class UserDataController : BaseProtocolController<UserDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> UserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation("Green UserData request: {@Request}", request);
        var common = await Mediator.Send(new Ac15UserDataQuery(request.Baid, GameEra.Green), HttpContext.RequestAborted);
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

        // shortcut: what the cabinet does with the list isn't verified on hardware; drop it here if it misbehaves.
        response.AryFriendInfoes.AddRange(common.Friends.Select(f => new UserDataResponse.FriendInfo { Baid = f.Baid, FriendName = f.FriendName }));
    }
}
