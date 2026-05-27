namespace TaikoLocalServer.Application.Handlers;

public partial class GetFolderQueryHandler
{
    private partial ValueTask<CommonGetFolderResponse> HandleNijiiro(GetFolderQuery request, CancellationToken cancellationToken)
    {
        var eventFolders = gameDataService.Nijiiro().GetEventFolderDictionary();
        return ValueTask.FromResult(BuildFolderResponse(eventFolders, request.FolderIds));
    }
}
