using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetRecommendQueryHandler
{
    private partial ValueTask<CommonRecommendResponse> HandleKimidori(
        GetRecommendQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Kimidori recommend requested for gender {GenderType}, age {PlayerAge}", request.GenderType, request.PlayerAge);
        var snapshot = Ac15CatalogSnapshotFactory.FromKimidori(gameDataService.Kimidori());
        return ValueTask.FromResult(Ac15CatalogReadbackService.BuildRecommendResponse(snapshot));
    }
}
