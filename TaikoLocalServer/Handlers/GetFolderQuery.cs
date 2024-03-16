using TaikoLocalServer.Models.Application;

namespace TaikoLocalServer.Handlers;

public record GetFolderQuery(uint[] FolderIds) : IRequest<CommonGetFolderResponse>;

public class GetFolderQueryHandler(ILogger<GetFolderQueryHandler> logger, IGameDataService gameDataService)
    : IRequestHandler<GetFolderQuery, CommonGetFolderResponse>
{
    public Task<CommonGetFolderResponse> Handle(GetFolderQuery request, CancellationToken cancellationToken)
    {
        var response = new CommonGetFolderResponse
        {
            Result = 1
        };
        var eventFolders = gameDataService.GetEventFolderDictionary();
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
        return Task.FromResult(response);
    }
}
