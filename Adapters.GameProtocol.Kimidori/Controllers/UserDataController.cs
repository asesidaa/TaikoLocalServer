namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

[ApiController]
public sealed class UserDataController(IGameDataCatalog gameDataService)
    : BaseProtocolController<UserDataController>
{
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
