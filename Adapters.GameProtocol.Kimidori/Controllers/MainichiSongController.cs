namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

[ApiController]
public sealed class MainichiSongController(IGameDataCatalog gameDataService)
    : BaseProtocolController<MainichiSongController>
{
    [HttpPost(KimidoriRoutePrefixes.Game + "/mainichisong.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> MainichiSong([FromBody] MainichisongRequest request)
    {
        Logger.LogInformation("Kimidori MainichiSong request: {@Request}", request);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Kimidori), HttpContext.RequestAborted);
        var kimidori = gameDataService.Kimidori();
        return Ok(new MainichisongResponse
        {
            Result = common.Result,
            SongHashVer = common.SongHashVer,
            HashMainichidojoAll = Ac15SongHashCodec.CompactBitset(common.AchievementSongBit, kimidori.SongHashTable),
            HashMainichidojoRare = Ac15SongHashCodec.CompactBitset(common.UraReleaseBit, kimidori.SongHashTable)
        });
    }
}
