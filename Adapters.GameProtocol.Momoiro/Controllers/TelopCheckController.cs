namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class TelopCheckController(IGameDataCatalog gameDataService)
    : BaseProtocolController<TelopCheckController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/telopcheck.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> TelopCheck([FromBody] TelopCheckRequest request)
    {
        Logger.LogInformation("Momoiro TelopCheck request: {@Request}", request);
        await EnsureMomoiroCatalogInitializedAsync(gameDataService, HttpContext.RequestAborted);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Momoiro), HttpContext.RequestAborted);
        return Ok(new TelopCheckResponse
        {
            Result = common.Result,
            TelopIds = common.AryTelopDatas.Select(row => row.InfoId).ToArray()
        });
    }

    private static async ValueTask EnsureMomoiroCatalogInitializedAsync(
        IGameDataCatalog gameDataService,
        CancellationToken cancellationToken)
    {
        if (gameDataService.Momoiro().SongHashTable.Count == 0)
        {
            await gameDataService.InitializeAsync(cancellationToken);
        }
    }
}
