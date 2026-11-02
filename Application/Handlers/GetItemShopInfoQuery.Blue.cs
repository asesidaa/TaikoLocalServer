using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetItemShopInfoQueryHandler
{
    private partial ValueTask<CommonItemShopInfoResponse> HandleBlue(
        GetItemShopInfoQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var snapshot = Ac15CatalogSnapshotFactory.FromBlue(gameDataService.Blue());
        if (!snapshot.ItemShopCatalog.IsEnabled || snapshot.ItemShopCatalog.ActiveSeason is null)
        {
            logger.LogInformation("Blue GetItemShopInfo returning empty because item shop is disabled or inactive");
        }

        return ValueTask.FromResult(Ac15CatalogReadbackService.BuildItemShopInfo(snapshot));
    }
}
