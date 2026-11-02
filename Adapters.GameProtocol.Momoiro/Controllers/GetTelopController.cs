namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class GetTelopController(IGameDataCatalog gameDataService)
    : BaseProtocolController<GetTelopController>
{
    [HttpPost(MomoiroRoutePrefixes.Final + "/gettelop.php")]
    [HttpPost(MomoiroRoutePrefixes.Game + "/gettelop.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetTelop([FromBody] GetTelopRequest request)
    {
        Logger.LogInformation("Momoiro GetTelop request: {@Request}", request);
        await EnsureMomoiroCatalogInitializedAsync(gameDataService, HttpContext.RequestAborted);
        var common = await Mediator.Send(
            new GetTelopQuery(GameEra.Momoiro, request.TelopId),
            HttpContext.RequestAborted);
        return Ok(GetTelopMappers.Map(common));
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
