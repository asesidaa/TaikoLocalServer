using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetFolderQueryHandler
{
    private partial ValueTask<CommonGetFolderResponse> HandleMurasaki(
        GetFolderQuery request,
        CancellationToken cancellationToken)
    {
        var snapshot = Ac15CatalogSnapshotFactory.FromMurasaki(gameDataService.Murasaki());
        return ValueTask.FromResult(Ac15CatalogReadbackService.BuildFolderResponse(snapshot, request.FolderIds));
    }
}
