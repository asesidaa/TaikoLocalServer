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

        if (TryGetUnsupportedRequestedItem(request, snapshot.ItemShopCatalog, out var seasonState))
        {
            return seasonState is not null
                ? Failure(seasonState)
                : new CommonItemPurchaseResponse { Result = 0, TotalGetDonmedal = saveData.TotalGetDonmedal, TotalUseDonmedal = saveData.TotalUseDonmedal };
        }

        var adapter = new YellowAc15ItemShopAdapter(context, saveData);
        return await Ac15ItemShopService.PurchaseAsync(
            new Ac15ItemShopPurchaseRequest(request.Baid, request.ItemNo, request.ItemType, request.ItemId, request.ItemPrice),
            snapshot.ItemShopCatalog,
            adapter,
            adapter,
            cancellationToken);

        bool TryGetUnsupportedRequestedItem(
            ItemPurchaseCommand purchase,
            Application.Catalog.Ac15.Ac15ItemShopCatalog itemShopCatalog,
            out YellowShopSeasonState? currentSeasonState)
        {
            currentSeasonState = null;
            if (IsPreflight(purchase)
                || !itemShopCatalog.IsEnabled
                || itemShopCatalog.ActiveSeason is not { } activeSeason
                || !activeSeason.ItemsByNo.TryGetValue(purchase.ItemNo, out var item)
                || item.ItemType.IsSupported())
            {
                return false;
            }

            currentSeasonState = context.YellowShopSeasonStates
                .Local
                .FirstOrDefault(row => row.Baid == saveData.Baid && row.SeasonId == activeSeason.SeasonId);
            if (currentSeasonState is null)
            {
                currentSeasonState = context.YellowShopSeasonStates
                    .FirstOrDefault(row => row.Baid == saveData.Baid && row.SeasonId == activeSeason.SeasonId);
            }

            return true;
        }
    }

    private static CommonItemPurchaseResponse Failure(YellowShopSeasonState state)
        => new()
        {
            Result = 0,
            TotalGetDonmedal = state.TotalGetDonmedal,
            TotalUseDonmedal = state.TotalUseDonmedal
        };
}
