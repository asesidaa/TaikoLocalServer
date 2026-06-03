using TaikoLocalServer.Domain.Enums;

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

        var seasonState = await context.GetOrCreateGreenShopSeasonStateAsync(saveData, activeSeason.SeasonId, cancellationToken);
        if (IsPreflight(request))
        {
            await context.SaveChangesAsync(cancellationToken);
            return Success(seasonState);
        }

        if (!activeSeason.ItemsByNo.TryGetValue(request.ItemNo, out var item)
            || request.ItemType != item.ItemType.ToProtocolValue()
            || request.ItemId != item.ItemId
            || request.ItemPrice != item.Price
            || item.Price == 0)
        {
            return Failure(seasonState);
        }

        var available = seasonState.TotalGetDonmedal >= seasonState.TotalUseDonmedal
            ? seasonState.TotalGetDonmedal - seasonState.TotalUseDonmedal
            : 0;

        var existingItem = await context.GreenShopItemStates.FindAsync(
            [request.Baid, activeSeason.SeasonId, item.ItemType.ToProtocolValue(), item.ItemId],
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
        var itemState = new GreenShopItemState
        {
            Baid = request.Baid,
            SeasonId = activeSeason.SeasonId,
            ItemType = item.ItemType.ToProtocolValue(),
            ItemId = item.ItemId,
            ItemNo = item.ItemNo,
            ItemPrice = item.Price,
            Status = GreenShopItemStatus.Unlocked,
            PurchasedAt = now,
            UnlockedAt = now
        };
        ApplyUnlock(saveData, itemState);
        context.GreenShopItemStates.Add(itemState);

        await context.SaveChangesAsync(cancellationToken);
        return Success(seasonState);
    }

    private static void ApplyUnlock(UserSaveDataGreen saveData, GreenShopItemState item)
    {
        switch ((Ac15ShopItemType)item.ItemType)
        {
            case Ac15ShopItemType.Song:
                return;
            case Ac15ShopItemType.Tone:
                saveData.ToneFlg = GreenShopUnlocks.SetBits(saveData.ToneFlg, [item.ItemId], GreenProtocolBytes.ToneFlagBytes);
                return;
            case Ac15ShopItemType.Kigurumi:
                saveData.CostumeFlg1 = GreenShopUnlocks.SetBits(saveData.CostumeFlg1, [item.ItemId], GreenProtocolBytes.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Body:
                saveData.CostumeFlg3 = GreenShopUnlocks.SetBits(saveData.CostumeFlg3, [item.ItemId], GreenProtocolBytes.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Head:
                saveData.CostumeFlg2 = GreenShopUnlocks.SetBits(saveData.CostumeFlg2, [item.ItemId], GreenProtocolBytes.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Face:
                saveData.CostumeFlg4 = GreenShopUnlocks.SetBits(saveData.CostumeFlg4, [item.ItemId], GreenProtocolBytes.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Puchi:
                saveData.CostumeFlg5 = GreenShopUnlocks.SetBits(saveData.CostumeFlg5, [item.ItemId], GreenProtocolBytes.CostumeFlagBytes);
                return;
            default:
                throw new InvalidOperationException($"Unsupported Green item shop item type {item.ItemType}.");
        }
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
