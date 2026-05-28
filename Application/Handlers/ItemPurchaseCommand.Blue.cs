namespace TaikoLocalServer.Application.Handlers;

public partial class ItemPurchaseCommandHandler
{
    private partial async ValueTask<CommonItemPurchaseResponse> HandleBlue(
        ItemPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Applying Blue item purchase for baid {Baid}, item {ItemNo}", request.Baid, request.ItemNo);
        var saveData = await context.GetOrCreateBlueSaveDataAsync(request.Baid, cancellationToken);
        var catalog = gameDataService.Blue().ItemShopCatalog;
        var activeSeason = catalog.IsEnabled ? catalog.ActiveSeason : null;
        if (activeSeason is null || activeSeason.Items.Count == 0)
        {
            return SuccessZero();
        }

        var seasonState = await context.GetOrCreateBlueShopSeasonStateAsync(saveData, activeSeason.SeasonId, cancellationToken);
        if (IsPreflight(request))
        {
            await context.SaveChangesAsync(cancellationToken);
            return Success(seasonState);
        }

        if (!activeSeason.ItemsByNo.TryGetValue(request.ItemNo, out var item)
            || request.ItemType != item.ItemType
            || request.ItemId != item.ItemId
            || request.ItemPrice != item.Price
            || item.Price == 0)
        {
            return Failure(seasonState);
        }

        var available = seasonState.TotalGetDonmedal >= seasonState.TotalUseDonmedal
            ? seasonState.TotalGetDonmedal - seasonState.TotalUseDonmedal
            : 0;

        var existingItem = await context.BlueShopItemStates.FindAsync(
            [request.Baid, activeSeason.SeasonId, item.ItemType, item.ItemId],
            cancellationToken);

        if (existingItem is not null)
        {
            return Failure(seasonState);
        }

        if (item.Price > available || !CanAdd(seasonState.TotalUseDonmedal, item.Price))
        {
            return Failure(seasonState);
        }

        var now = DateTime.UtcNow;
        seasonState.TotalUseDonmedal += item.Price;
        seasonState.UpdatedAt = now;
        var itemState = new BlueShopItemState
        {
            Baid = request.Baid,
            SeasonId = activeSeason.SeasonId,
            ItemType = item.ItemType,
            ItemId = item.ItemId,
            ItemNo = item.ItemNo,
            ItemPrice = item.Price,
            Status = BlueShopItemStatus.Unlocked,
            PurchasedAt = now,
            UnlockedAt = now
        };
        ApplyUnlock(saveData, itemState);
        context.BlueShopItemStates.Add(itemState);

        await context.SaveChangesAsync(cancellationToken);
        return Success(seasonState);
    }

    private static void ApplyUnlock(UserSaveDataBlue saveData, BlueShopItemState item)
    {
        switch (item.ItemType)
        {
            case 1:
                saveData.ReleaseSongFlg = BlueShopUnlocks.SetBits(saveData.ReleaseSongFlg, [item.ItemId], BlueProtocolBytes.SongFlagBytes);
                return;
            case 2:
                saveData.ToneFlg = BlueShopUnlocks.SetBits(saveData.ToneFlg, [item.ItemId], BlueProtocolBytes.ToneFlagBytes);
                return;
            case 3:
                saveData.CostumeFlg1 = BlueShopUnlocks.SetBits(saveData.CostumeFlg1, [item.ItemId], BlueProtocolBytes.CostumeFlagBytes);
                return;
            case 4:
                saveData.CostumeFlg3 = BlueShopUnlocks.SetBits(saveData.CostumeFlg3, [item.ItemId], BlueProtocolBytes.CostumeFlagBytes);
                return;
            case 5:
                saveData.CostumeFlg2 = BlueShopUnlocks.SetBits(saveData.CostumeFlg2, [item.ItemId], BlueProtocolBytes.CostumeFlagBytes);
                return;
            case 6:
                saveData.CostumeFlg4 = BlueShopUnlocks.SetBits(saveData.CostumeFlg4, [item.ItemId], BlueProtocolBytes.CostumeFlagBytes);
                return;
            case 7:
                saveData.CostumeFlg5 = BlueShopUnlocks.SetBits(saveData.CostumeFlg5, [item.ItemId], BlueProtocolBytes.CostumeFlagBytes);
                return;
            default:
                throw new InvalidOperationException($"Unsupported Blue item shop item type {item.ItemType}.");
        }
    }

    private static CommonItemPurchaseResponse Failure(BlueShopSeasonState state)
        => new()
        {
            Result = 0,
            TotalGetDonmedal = state.TotalGetDonmedal,
            TotalUseDonmedal = state.TotalUseDonmedal
        };

    private static CommonItemPurchaseResponse Success(BlueShopSeasonState state)
        => new()
        {
            Result = 1,
            TotalGetDonmedal = state.TotalGetDonmedal,
            TotalUseDonmedal = state.TotalUseDonmedal
        };

    private static CommonItemPurchaseResponse SuccessZero()
        => new()
        {
            Result = 1,
            TotalGetDonmedal = 0,
            TotalUseDonmedal = 0
        };
}
