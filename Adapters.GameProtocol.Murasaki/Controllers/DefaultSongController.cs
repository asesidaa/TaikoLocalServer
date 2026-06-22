namespace TaikoLocalServer.Adapters.GameProtocol.Murasaki.Controllers;

[ApiController]
public sealed class DefaultSongController : BaseProtocolController<DefaultSongController>
{
    [HttpPost(MurasakiRoutePrefixes.Final + "/defaultsong.php")]
    [HttpPost(MurasakiRoutePrefixes.Compatibility + "/defaultsong.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> DefaultSong([FromBody] DefaultsongRequest request)
    {
        Logger.LogInformation("Murasaki DefaultSong request: {@Request}", request);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Murasaki), HttpContext.RequestAborted);
        return Ok(new DefaultsongResponse
        {
            Result = common.Result,
            SongHashVer = common.SongHashVer,
            HashDefaultSongFlg = common.DefaultSongFlg
        });
    }
}
