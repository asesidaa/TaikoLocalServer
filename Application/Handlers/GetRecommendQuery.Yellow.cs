using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetRecommendQueryHandler
{
    private partial ValueTask<CommonRecommendResponse> HandleYellow(
        GetRecommendQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Yellow recommend requested for gender {GenderType}, age {PlayerAge}", request.GenderType, request.PlayerAge);
        var snapshot = Ac15CatalogSnapshotFactory.FromYellow(gameDataService.Yellow());
        return ValueTask.FromResult(Ac15CatalogReadbackService.BuildRecommendResponse(snapshot));
    }
}
