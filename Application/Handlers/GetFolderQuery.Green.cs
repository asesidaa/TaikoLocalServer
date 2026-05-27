namespace TaikoLocalServer.Application.Handlers;

public partial class GetFolderQueryHandler
{
    private partial ValueTask<CommonGetFolderResponse> HandleGreen(GetFolderQuery request, CancellationToken cancellationToken)
    {
        var eventFolders = gameDataService.Green().EventFolders;
        return ValueTask.FromResult(BuildFolderResponse(eventFolders, request.FolderIds));
    }
}
