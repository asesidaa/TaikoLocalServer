using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15ItemShopService
{
    public static async ValueTask<CommonItemPurchaseResponse> PurchaseAsync(
        Ac15ItemShopPurchaseRequest request,
        Ac15ItemShopCatalog catalog,
        IAc15ItemShopPersistence persistence,
        IAc15ItemShopUnlockPolicy unlockPolicy,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var activeSeason = catalog.IsEnabled ? catalog.ActiveSeason : null;
        if (activeSeason is null || activeSeason.Items.Count == 0)
        {
            return new CommonItemPurchaseResponse { Result = 1, TotalGetDonmedal = 0, TotalUseDonmedal = 0 };
        }

        var seasonState = await persistence.GetOrCreateActiveSeasonStateAsync(
            request.Baid,
            activeSeason.SeasonId,
            cancellationToken);
        if (seasonState is null)
        {
            return new CommonItemPurchaseResponse { Result = 1, TotalGetDonmedal = 0, TotalUseDonmedal = 0 };
        }

        if (IsPreflight(request))
        {
            await persistence.SaveChangesAsync(cancellationToken);
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

        var itemTypeValue = item.ItemType.ToProtocolValue();
        if (await persistence.HasPurchasedItemAsync(request.Baid, activeSeason.SeasonId, itemTypeValue, item.ItemId, cancellationToken))
        {
            return Failure(seasonState);
        }

        if (item.Price > available || !CanAdd(seasonState.TotalUseDonmedal, item.Price))
        {
            return Failure(seasonState);
        }

        seasonState.TotalUseDonmedal += item.Price;
        unlockPolicy.ApplyUnlock(item.ItemType, item.ItemId);
        await persistence.AddPurchasedItemAsync(
            new Ac15PurchasedShopItem(request.Baid, activeSeason.SeasonId, itemTypeValue, item.ItemId, item.ItemNo, item.Price),
            cancellationToken);
        await persistence.SaveChangesAsync(cancellationToken);

        return Success(seasonState);
    }

    private static bool CanAdd(uint current, uint delta)
        => delta <= uint.MaxValue - current;

    private static bool IsPreflight(Ac15ItemShopPurchaseRequest request)
        => request.ItemNo == 0
           && request.ItemType is null
           && request.ItemId is null
           && request.ItemPrice is null;

    private static CommonItemPurchaseResponse Success(Ac15ShopSeasonState state)
        => new()
        {
            Result = 1,
            TotalGetDonmedal = state.TotalGetDonmedal,
            TotalUseDonmedal = state.TotalUseDonmedal
        };

    private static CommonItemPurchaseResponse Failure(Ac15ShopSeasonState state)
        => new()
        {
            Result = 0,
            TotalGetDonmedal = state.TotalGetDonmedal,
            TotalUseDonmedal = state.TotalUseDonmedal
        };
}
