namespace TaikoLocalServer.Adapters.GameProtocol.Murasaki.Controllers;

[ApiController]
public sealed class MainichiSongController : BaseProtocolController<MainichiSongController>
{
    [HttpPost(MurasakiRoutePrefixes.Game + "/mainichisong.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> MainichiSong([FromBody] MainichisongRequest request)
    {
        Logger.LogInformation("Murasaki MainichiSong request: {@Request}", request);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Murasaki), HttpContext.RequestAborted);
        return Ok(new MainichisongResponse
        {
            Result = common.Result,
            SongHashVer = common.SongHashVer,
            HashMainichidojoAll = common.AchievementSongBit,
            HashMainichidojoRare = common.UraReleaseBit
        });
    }
}
