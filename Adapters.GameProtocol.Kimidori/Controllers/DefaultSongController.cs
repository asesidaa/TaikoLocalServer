namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;

[ApiController]
public sealed class DefaultSongController(IGameDataCatalog gameDataService)
    : BaseProtocolController<DefaultSongController>
{
    [HttpPost(KimidoriRoutePrefixes.Final + "/defaultsong.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> FinalDefaultSong([FromBody] FinalWire.DefaultsongRequest request)
    {
        Logger.LogInformation("Kimidori final DefaultSong request: {@Request}", request);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Kimidori), HttpContext.RequestAborted);
        var kimidori = gameDataService.Kimidori();
        return Ok(new FinalWire.DefaultsongResponse
        {
            Result = common.Result,
            SongHashVer = common.SongHashVer,
            HashDefaultSongFlg = Ac15SongHashCodec.CompactBitset(
                common.DefaultSongFlg,
                kimidori.SongHashTable)
        });
    }

    [HttpPost(KimidoriRoutePrefixes.Game + "/defaultsong.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> DefaultSong([FromBody] DefaultsongRequest request)
    {
        Logger.LogInformation("Kimidori DefaultSong request: {@Request}", request);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Kimidori), HttpContext.RequestAborted);
        var kimidori = gameDataService.Kimidori();
        return Ok(new DefaultsongResponse
        {
            Result = common.Result,
            SongHashVer = common.SongHashVer,
            HashDefaultSongFlg = Ac15SongHashCodec.CompactBitset(
                common.DefaultSongFlg,
                kimidori.SongHashTable)
        });
    }
}
