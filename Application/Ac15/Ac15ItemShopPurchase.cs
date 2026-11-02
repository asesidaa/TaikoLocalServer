using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15ItemShopPurchase
{
    public static async ValueTask<CommonItemPurchaseResponse> PurchaseAsync<TSave, TSeason, TItem>(
        ITaikoDbContext context,
        Ac15ItemShopPurchaseRequest request,
        Ac15ItemShopCatalog catalog,
        TSave saveData,
        Ac15ItemShopPurchaseTables<TSeason, TItem> tables,
        Ac15ItemShopUnlockPolicy<TSave> unlockPolicy,
        CancellationToken cancellationToken)
        where TSeason : class, IAc15ShopSeasonState
        where TItem : class, IAc15ShopItemState
    {
        cancellationToken.ThrowIfCancellationRequested();

        var activeSeason = catalog.IsEnabled ? catalog.ActiveSeason : null;
        if (activeSeason is null)
        {
            return EmptySuccess();
        }

        var seasonState = await tables.GetOrCreateSeason(activeSeason.SeasonId, cancellationToken);
        if (seasonState is null)
        {
            return EmptySuccess();
        }

        if (IsPreflight(request))
        {
            await context.SaveChangesAsync(cancellationToken);
            return Success(seasonState);
        }

        if (activeSeason.Items.Count == 0)
        {
            await context.SaveChangesAsync(cancellationToken);
            return Failure(seasonState);
        }

        if (!activeSeason.ItemsByNo.TryGetValue(request.ItemNo, out var item)
            || request.ItemType != item.ItemType.ToProtocolValue()
            || request.ItemId != item.ItemId
            || request.ItemPrice != item.Price
            || item.Price == 0
            || !unlockPolicy.IsSupported(item.ItemType))
        {
            return Failure(seasonState);
        }

        var available = seasonState.TotalGetDonmedal >= seasonState.TotalUseDonmedal
            ? seasonState.TotalGetDonmedal - seasonState.TotalUseDonmedal
            : 0;
        var itemTypeValue = item.ItemType.ToProtocolValue();
        if (await tables.ItemStates.FindAsync(
                [request.Baid, activeSeason.SeasonId, itemTypeValue, item.ItemId],
                cancellationToken) is not null
            || item.Price > available
            || !CanAdd(seasonState.TotalUseDonmedal, item.Price))
        {
            return Failure(seasonState);
        }

        var now = DateTime.UtcNow;
        seasonState.TotalUseDonmedal += item.Price;
        seasonState.UpdatedAt = now;
        unlockPolicy.ApplyUnlock(saveData, item.ItemType, item.ItemId);
        tables.ItemStates.Add(tables.CreateItem(
            new Ac15PurchasedShopItem(
                request.Baid,
                activeSeason.SeasonId,
                itemTypeValue,
                item.ItemId,
                item.ItemNo,
                item.Price),
            now));
        await context.SaveChangesAsync(cancellationToken);

        return Success(seasonState);
    }

    private static bool CanAdd(uint current, uint delta)
        => delta <= uint.MaxValue - current;

    private static bool IsPreflight(Ac15ItemShopPurchaseRequest request)
        => request.ItemNo == 0
           && request.ItemType is null
           && request.ItemId is null
           && request.ItemPrice is null;

    private static CommonItemPurchaseResponse EmptySuccess()
        => new() { Result = 1, TotalGetDonmedal = 0, TotalUseDonmedal = 0 };

    private static CommonItemPurchaseResponse Success(IAc15ShopSeasonState state)
        => new()
        {
            Result = 1,
            TotalGetDonmedal = state.TotalGetDonmedal,
            TotalUseDonmedal = state.TotalUseDonmedal
        };

    private static CommonItemPurchaseResponse Failure(IAc15ShopSeasonState state)
        => new()
        {
            Result = 0,
            TotalGetDonmedal = state.TotalGetDonmedal,
            TotalUseDonmedal = state.TotalUseDonmedal
        };
}
