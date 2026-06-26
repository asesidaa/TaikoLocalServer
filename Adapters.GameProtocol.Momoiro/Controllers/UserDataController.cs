namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class UserDataController(IGameDataCatalog gameDataService)
    : BaseProtocolController<UserDataController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/userdata.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> UserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation("Momoiro UserData request: {@Request}", request);
        var common = await Mediator.Send(new Ac15UserDataQuery(request.Baid, GameEra.Momoiro), HttpContext.RequestAborted);

        var response = new UserDataResponse
        {
            Result = common.Result
        };
        UserDataMappers.Apply(common.SongFlags, response);
        UserDataMappers.Apply(common.SongLists, response);
        UserDataMappers.Apply(common.Counters, response);
        UserDataMappers.Apply(common.Display, response);
        UserDataMappers.Apply(common.Recommendations, response);

        var momoiro = gameDataService.Momoiro();
        response.HashReleaseSongFlg = Ac15SongHashCodec.CompactBitset(
            response.HashReleaseSongFlg,
            momoiro.SongHashTable);

        if (common.HashCrownFlg is not null)
        {
            UserDataMappers.ApplyCrown(common, response);
        }

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
