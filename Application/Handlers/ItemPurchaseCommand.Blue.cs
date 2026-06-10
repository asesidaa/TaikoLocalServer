using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class ItemPurchaseCommandHandler
{
    private partial async ValueTask<CommonItemPurchaseResponse> HandleBlue(
        ItemPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Applying Blue item purchase for baid {Baid}, item {ItemNo}", request.Baid, request.ItemNo);
        var saveData = await context.GetOrCreateBlueSaveDataAsync(request.Baid, cancellationToken);
        var snapshot = Ac15CatalogSnapshotFactory.FromBlue(gameDataService.Blue());

        return await Ac15ItemShopService.PurchaseBlueAsync(
            context,
            new Ac15ItemShopPurchaseRequest(request.Baid, request.ItemNo, request.ItemType, request.ItemId, request.ItemPrice),
            snapshot.ItemShopCatalog,
            saveData,
            cancellationToken);
    }
}
