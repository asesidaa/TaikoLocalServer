using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetFolderQueryHandler
{
    private partial ValueTask<CommonGetFolderResponse> HandleGreen(GetFolderQuery request, CancellationToken cancellationToken)
    {
        var snapshot = Ac15CatalogSnapshotFactory.FromGreen(gameDataService.Green());
        return ValueTask.FromResult(Ac15CatalogReadbackService.BuildFolderResponse(snapshot, request.FolderIds));
    }
}
