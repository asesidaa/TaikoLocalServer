namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class SongHashController(IGameDataCatalog gameDataService)
    : BaseProtocolController<SongHashController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/songhash.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> SongHash([FromBody] SonghashRequest request)
    {
        Logger.LogInformation("Momoiro SongHash request: {@Request}", request);
        var momoiro = await GetMomoiroCatalogAsync(gameDataService, HttpContext.RequestAborted);
        return Ok(new SonghashResponse
        {
            Result = 1,
            SongHashVer = momoiro.SongHashVersion,
            SongHashTbl = Ac15SongHashCodec.EncodeTable(momoiro.SongHashTable)
        });
    }

    private static async ValueTask<IMomoiroCatalog> GetMomoiroCatalogAsync(
        IGameDataCatalog gameDataService,
        CancellationToken cancellationToken)
    {
        var momoiro = gameDataService.Momoiro();
        if (momoiro.SongHashTable.Count == 0)
        {
            await gameDataService.InitializeAsync(cancellationToken);
            momoiro = gameDataService.Momoiro();
        }

        return momoiro;
    }
}
