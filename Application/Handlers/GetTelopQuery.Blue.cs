using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetTelopQueryHandler
{
    private partial ValueTask<CommonGetTelopResponse> HandleBlue(GetTelopQuery request, CancellationToken cancellationToken)
    {
        var snapshot = Ac15CatalogSnapshotFactory.FromBlue(gameDataService.Blue());
        return ValueTask.FromResult(Ac15CatalogReadbackService.BuildTelopResponse(snapshot, request.TelopId));
    }
}
