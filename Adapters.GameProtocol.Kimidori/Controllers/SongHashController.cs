namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

[ApiController]
public sealed class SongHashController(IGameDataCatalog gameDataService)
    : BaseProtocolController<SongHashController>
{
    [HttpPost(KimidoriRoutePrefixes.Game + "/songhash.php")]
    [Produces("application/protobuf")]
    public IActionResult SongHash([FromBody] SonghashRequest request)
    {
        Logger.LogInformation("Kimidori SongHash request: {@Request}", request);
        var kimidori = gameDataService.Kimidori();
        return Ok(new SonghashResponse
        {
            Result = 1,
            SongHashVer = kimidori.SongHashVersion,
            SongHashTbl = Ac15SongHashCodec.EncodeTable(kimidori.SongHashTable)
        });
    }
}
