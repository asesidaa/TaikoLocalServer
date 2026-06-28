namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;

[ApiController]
public sealed class UserDataController(IGameDataCatalog gameDataService)
    : BaseProtocolController<UserDataController>
{
    [HttpPost(KimidoriRoutePrefixes.Final + "/userdata.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> FinalUserData([FromBody] FinalWire.UserDataRequest request)
    {
        Logger.LogInformation("Kimidori final UserData request: {@Request}", request);
        var common = await Mediator.Send(new Ac15UserDataQuery(request.Baid, GameEra.Kimidori), HttpContext.RequestAborted);

        var response = new FinalWire.UserDataResponse
        {
            Result = common.Result
        };
        FinalUserDataMappers.Apply(common.SongFlags, response);
        FinalUserDataMappers.Apply(common.SongLists, response);
        FinalUserDataMappers.Apply(common.Counters, response);
        FinalUserDataMappers.Apply(common.Display, response);
        FinalUserDataMappers.Apply(common.Recommendations, response);
        var kimidori = gameDataService.Kimidori();
        response.HashReleaseSongFlg = Ac15SongHashCodec.CompactBitset(
            response.HashReleaseSongFlg,
            kimidori.SongHashTable);
        if (common.ModeFlags is { } modeFlags)
        {
            FinalUserDataMappers.Apply(modeFlags, response);
        }

        if (common.Reward is { } reward)
        {
            FinalUserDataMappers.Apply(reward, response);
        }

        return Ok(response);
    }

    [HttpPost(KimidoriRoutePrefixes.Game + "/userdata.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> UserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation("Kimidori UserData request: {@Request}", request);
        var common = await Mediator.Send(new Ac15UserDataQuery(request.Baid, GameEra.Kimidori), HttpContext.RequestAborted);

        var response = new UserDataResponse
        {
            Result = common.Result
        };
        UserDataMappers.Apply(common.SongFlags, response);
        UserDataMappers.Apply(common.SongLists, response);
        UserDataMappers.Apply(common.Counters, response);
        UserDataMappers.Apply(common.Display, response);
        UserDataMappers.Apply(common.Recommendations, response);
        var kimidori = gameDataService.Kimidori();
        response.HashReleaseSongFlg = Ac15SongHashCodec.CompactBitset(
            response.HashReleaseSongFlg,
            kimidori.SongHashTable);
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
