using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetRecommendQueryHandler
{
    private partial ValueTask<CommonRecommendResponse> HandleBlue(
        GetRecommendQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Blue recommend requested for gender {GenderType}, age {PlayerAge}", request.GenderType, request.PlayerAge);
        var snapshot = Ac15CatalogSnapshotFactory.FromBlue(gameDataService.Blue());
        return ValueTask.FromResult(Ac15CatalogReadbackService.BuildRecommendResponse(snapshot));
    }
}
