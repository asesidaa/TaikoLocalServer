using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class ItemPurchaseCommandHandler
{
    private partial async ValueTask<CommonItemPurchaseResponse> HandleGreen(
        ItemPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Applying Green item purchase for baid {Baid}, item {ItemNo}", request.Baid, request.ItemNo);
        var saveData = await context.GetOrCreateGreenSaveDataAsync(request.Baid, cancellationToken);
        var green = gameDataService.Green();
        var snapshot = Ac15CatalogSnapshotFactory.FromGreen(green);

        return await Ac15ItemShopPurchase.PurchaseAsync(
            context,
            new Ac15ItemShopPurchaseRequest(request.Baid, request.ItemNo, request.ItemType, request.ItemId, request.ItemPrice),
            snapshot.ItemShopCatalog,
            saveData,
            new Ac15ItemShopPurchaseTables<GreenShopSeasonState, GreenShopItemState>(
                context.GreenShopItemStates,
                async (seasonId, token) => await context.GetOrCreateGreenShopSeasonStateAsync(saveData, seasonId, token),
                Ac15ItemShopMapper.ToGreenShopItemState),
            Ac15ItemShopUnlockPolicies.Green,
            cancellationToken);
    }
}
