namespace TaikoLocalServer.Application.Handlers;

public partial class ItemPurchaseCommandHandler
{
    public partial async ValueTask<CommonItemPurchaseResponse> Handle(
        ItemPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Applying Green item purchase for baid {Baid}, item {ItemNo}", request.Baid, request.ItemNo);
        var saveData = await context.GetOrCreateGreenSaveDataAsync(request.Baid, cancellationToken);
        var green = gameDataService.Green();
        var activeSeason = green.ItemShopCatalog.ActiveSeason;
        if (activeSeason is null
            || !activeSeason.ItemsByNo.TryGetValue(request.ItemNo, out var item)
            || request.ItemType != item.ItemType
            || request.ItemId != item.ItemId
            || request.ItemPrice != item.Price
            || item.Price == 0)
        {
            return Failure(saveData);
        }

        var seasonState = await context.GetOrCreateGreenShopSeasonStateAsync(saveData, activeSeason.SeasonId, cancellationToken);
        var available = seasonState.TotalGetDonmedal >= seasonState.TotalUseDonmedal
            ? seasonState.TotalGetDonmedal - seasonState.TotalUseDonmedal
            : 0;

        var existingItem = await context.GreenShopItemStates.FindAsync(
            [request.Baid, activeSeason.SeasonId, item.ItemType, item.ItemId],
            cancellationToken);

        if (existingItem is not null || item.Price > available || !CanAdd(seasonState.TotalUseDonmedal, item.Price))
        {
            return Failure(seasonState);
        }

        var now = DateTime.UtcNow;
        seasonState.TotalUseDonmedal += item.Price;
        seasonState.UpdatedAt = now;
        context.GreenShopItemStates.Add(new GreenShopItemState
        {
            Baid = request.Baid,
            SeasonId = activeSeason.SeasonId,
            ItemType = item.ItemType,
            ItemId = item.ItemId,
            ItemNo = item.ItemNo,
            ItemPrice = item.Price,
            Status = GreenShopItemStatus.PendingReward,
            PurchasedAt = now
        });

        await context.SaveChangesAsync(cancellationToken);
        return Success(seasonState);
    }

    private static bool CanAdd(uint current, uint delta)
        => delta <= uint.MaxValue - current;

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
