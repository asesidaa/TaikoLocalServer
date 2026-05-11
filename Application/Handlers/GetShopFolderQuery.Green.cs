namespace TaikoLocalServer.Application.Handlers;

public partial class GetShopFolderHandler
{
    private partial ValueTask<CommonGetShopFolderResponse> HandleGreen(GetShopFolderQuery request, CancellationToken cancellationToken) =>
        ValueTask.FromResult(new CommonGetShopFolderResponse { Result = 1 });
}
