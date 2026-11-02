using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetRecommendQueryHandler
{
    private partial ValueTask<CommonRecommendResponse> HandleMurasaki(
        GetRecommendQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Murasaki recommend requested for gender {GenderType}, age {PlayerAge}", request.GenderType, request.PlayerAge);
        var snapshot = Ac15CatalogSnapshotFactory.FromMurasaki(gameDataService.Murasaki());
        return ValueTask.FromResult(Ac15CatalogReadbackService.BuildRecommendResponse(snapshot));
    }
}
