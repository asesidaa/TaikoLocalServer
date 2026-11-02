using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetTelopQueryHandler
{
    private partial ValueTask<CommonGetTelopResponse> HandleMurasaki(
        GetTelopQuery request,
        CancellationToken cancellationToken)
    {
        var snapshot = Ac15CatalogSnapshotFactory.FromMurasaki(gameDataService.Murasaki());
        return ValueTask.FromResult(Ac15CatalogReadbackService.BuildTelopResponse(snapshot, request.TelopId));
    }
}
