namespace TaikoLocalServer.Application.Handlers;

public partial class GetFolderQueryHandler
{
    private partial ValueTask<CommonGetFolderResponse> HandleBlue(GetFolderQuery request, CancellationToken cancellationToken)
    {
        var eventFolders = gameDataService.Blue().EventFolders;
        return ValueTask.FromResult(BuildFolderResponse(eventFolders, request.FolderIds));
    }
}
