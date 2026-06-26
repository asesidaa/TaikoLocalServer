namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class DefaultSongController(IGameDataCatalog gameDataService)
    : BaseProtocolController<DefaultSongController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/defaultsong.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> DefaultSong([FromBody] DefaultsongRequest request)
    {
        Logger.LogInformation("Momoiro DefaultSong request: {@Request}", request);
        var momoiro = await GetMomoiroCatalogAsync(gameDataService, HttpContext.RequestAborted);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Momoiro), HttpContext.RequestAborted);
        return Ok(new DefaultsongResponse
        {
            Result = common.Result,
            SongHashVer = common.SongHashVer,
            HashDefaultSongFlg = Ac15SongHashCodec.CompactBitset(
                common.DefaultSongFlg,
                momoiro.SongHashTable)
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
