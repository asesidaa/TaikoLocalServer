namespace TaikoLocalServer.Application.Handlers;

public partial class GetFolderQueryHandler
{
    private partial ValueTask<CommonGetFolderResponse> HandleNijiiro(GetFolderQuery request, CancellationToken cancellationToken)
    {
        var response = new CommonGetFolderResponse
        {
            Result = 1
        };
        var eventFolders = gameDataService.Nijiiro().GetEventFolderDictionary();
        foreach (var folderId in request.FolderIds)
        {
            eventFolders.TryGetValue(folderId, out var folderData);
            if (folderData is null)
            {
                logger.LogWarning("Folder data for folder {FolderId} not found", folderId);
                continue;
            }
            response.AryEventfolderDatas.Add(folderData);
        }
        return ValueTask.FromResult(response);
    }
}
