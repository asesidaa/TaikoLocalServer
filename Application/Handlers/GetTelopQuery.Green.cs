using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetTelopQueryHandler
{
    private partial ValueTask<CommonGetTelopResponse> HandleGreen(GetTelopQuery request, CancellationToken cancellationToken)
    {
        var snapshot = Ac15CatalogSnapshotFactory.FromGreen(gameDataService.Green());
        return ValueTask.FromResult(Ac15CatalogReadbackService.BuildTelopResponse(snapshot, request.TelopId));
    }
}
