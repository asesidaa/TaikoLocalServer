namespace TaikoLocalServer.Application.Handlers;

public partial class GetFolderQueryHandler
{
    private partial ValueTask<CommonGetFolderResponse> HandleGreen(GetFolderQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green GetFolder stub for {Count} folders, returning empty", request.FolderIds.Length);
        return ValueTask.FromResult(new CommonGetFolderResponse { Result = 1 });
    }
}
