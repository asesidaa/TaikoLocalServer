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
        var activeSeason = green.ItemShopCatalog.ActiveSeason;
        if (activeSeason is null)
        {
            return Failure(saveData);
        }

        if (activeSeason.Items.Count == 0)
        {
            var seasonState = await context.GetOrCreateGreenShopSeasonStateAsync(saveData, activeSeason.SeasonId, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return IsPreflight(request) ? Success(seasonState) : Failure(seasonState);
        }

        var snapshot = Ac15CatalogSnapshotFactory.FromGreen(green);
        var adapter = new GreenAc15ItemShopAdapter(context, saveData);

        return await Ac15ItemShopService.PurchaseAsync(
            new Ac15ItemShopPurchaseRequest(request.Baid, request.ItemNo, request.ItemType, request.ItemId, request.ItemPrice),
            snapshot.ItemShopCatalog,
            adapter,
            adapter,
            cancellationToken);
    }

    private static CommonItemPurchaseResponse Failure(UserSaveDataGreen saveData)
        => new()
        {
            Result = 0,
            TotalGetDonmedal = saveData.TotalGetDonmedal,
            TotalUseDonmedal = saveData.TotalUseDonmedal
        };

    private static CommonItemPurchaseResponse Failure(GreenShopSeasonState state)
        => new()
        {
            Result = 0,
            TotalGetDonmedal = state.TotalGetDonmedal,
            TotalUseDonmedal = state.TotalUseDonmedal
        };

    private static CommonItemPurchaseResponse Success(GreenShopSeasonState state)
        => new()
        {
            Result = 1,
            TotalGetDonmedal = state.TotalGetDonmedal,
            TotalUseDonmedal = state.TotalUseDonmedal
        };
}
