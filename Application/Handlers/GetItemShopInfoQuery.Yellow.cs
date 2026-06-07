using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetItemShopInfoQueryHandler
{
    private partial ValueTask<CommonItemShopInfoResponse> HandleYellow(
        GetItemShopInfoQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var snapshot = Ac15CatalogSnapshotFactory.FromYellow(gameDataService.Yellow());
        if (!snapshot.ItemShopCatalog.IsEnabled || snapshot.ItemShopCatalog.ActiveSeason is null)
        {
            logger.LogInformation("Yellow GetItemShopInfo returning empty because item shop is disabled or inactive");
        }

        return ValueTask.FromResult(Ac15CatalogReadbackService.BuildItemShopInfo(snapshot));
    }
}
