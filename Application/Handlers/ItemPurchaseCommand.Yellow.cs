using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class ItemPurchaseCommandHandler
{
    private partial async ValueTask<CommonItemPurchaseResponse> HandleYellow(
        ItemPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Applying Yellow item purchase for baid {Baid}, item {ItemNo}", request.Baid, request.ItemNo);
        var saveData = await context.GetOrCreateYellowSaveDataAsync(request.Baid, cancellationToken);
        var yellow = gameDataService.Yellow();
        var snapshot = Ac15CatalogSnapshotFactory.FromYellow(yellow);

        return await Ac15ItemShopPurchase.PurchaseAsync(
            context,
            new Ac15ItemShopPurchaseRequest(request.Baid, request.ItemNo, request.ItemType, request.ItemId, request.ItemPrice),
            snapshot.ItemShopCatalog,
            saveData,
            new Ac15ItemShopPurchaseTables<YellowShopSeasonState, YellowShopItemState>(
                context.YellowShopItemStates,
                async (seasonId, token) => await context.GetOrCreateYellowShopSeasonStateAsync(saveData, seasonId, token),
                Ac15ItemShopMapper.ToYellowShopItemState),
            Ac15ItemShopUnlockPolicies.Yellow,
            cancellationToken);
    }
}
