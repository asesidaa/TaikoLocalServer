using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetRecommendQueryHandler
{
    private partial ValueTask<CommonRecommendResponse> HandleMomoiro(
        GetRecommendQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Momoiro recommend requested for gender {GenderType}, age {PlayerAge}", request.GenderType, request.PlayerAge);
        var snapshot = Ac15CatalogSnapshotFactory.FromMomoiro(gameDataService.Momoiro());
        return ValueTask.FromResult(Ac15CatalogReadbackService.BuildRecommendResponse(snapshot));
    }
}
