using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetFolderQueryHandler
{
    private partial ValueTask<CommonGetFolderResponse> HandleWhite(
        GetFolderQuery request,
        CancellationToken cancellationToken)
    {
        var snapshot = Ac15CatalogSnapshotFactory.FromWhite(gameDataService.White());
        return ValueTask.FromResult(Ac15CatalogReadbackService.BuildFolderResponse(snapshot, request.FolderIds));
    }
}
