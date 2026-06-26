namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class RecommendController(IGameDataCatalog gameDataService)
    : BaseProtocolController<RecommendController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/recommend.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> Recommend([FromBody] RecommendRequest request)
    {
        Logger.LogInformation("Momoiro Recommend request: {@Request}", request);
        await EnsureMomoiroCatalogInitializedAsync(gameDataService, HttpContext.RequestAborted);
        var common = await Mediator.Send(
            new GetRecommendQuery(GameEra.Momoiro, request.GenderType, request.PlayerAge),
            HttpContext.RequestAborted);
        return Ok(RecommendMappers.Map(common));
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
