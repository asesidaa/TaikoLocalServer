using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetItemShopInfoQueryHandler
{
    private partial ValueTask<CommonItemShopInfoResponse> HandleGreen(GetItemShopInfoQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var snapshot = Ac15CatalogSnapshotFactory.FromGreen(gameDataService.Green());
        if (!snapshot.ItemShopCatalog.IsEnabled || snapshot.ItemShopCatalog.ActiveSeason is null)
        {
            logger.LogInformation("Green GetItemShopInfo returning empty because item shop is disabled");
        }

        return ValueTask.FromResult(Ac15CatalogReadbackService.BuildItemShopInfo(snapshot));
    }
}
