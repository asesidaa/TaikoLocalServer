using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15ItemShopService
{
    public static async ValueTask<CommonItemPurchaseResponse> PurchaseBlueAsync(
        ITaikoDbContext context,
        Ac15ItemShopPurchaseRequest request,
        Ac15ItemShopCatalog catalog,
        UserSaveDataBlue saveData,
        CancellationToken cancellationToken)
        => await PurchaseAsync(
            context,
            request,
            catalog,
            new Ac15ItemShopSaveRows(Blue: saveData),
            GameEra.Blue,
            cancellationToken);

    public static async ValueTask<CommonItemPurchaseResponse> PurchaseGreenAsync(
        ITaikoDbContext context,
        Ac15ItemShopPurchaseRequest request,
        Ac15ItemShopCatalog catalog,
        UserSaveDataGreen saveData,
        CancellationToken cancellationToken)
        => await PurchaseAsync(
            context,
            request,
            catalog,
            new Ac15ItemShopSaveRows(Green: saveData),
            GameEra.Green,
            cancellationToken);

    public static async ValueTask<CommonItemPurchaseResponse> PurchaseYellowAsync(
        ITaikoDbContext context,
        Ac15ItemShopPurchaseRequest request,
        Ac15ItemShopCatalog catalog,
        UserSaveDataYellow saveData,
        CancellationToken cancellationToken)
        => await PurchaseAsync(
            context,
            request,
            catalog,
            new Ac15ItemShopSaveRows(Yellow: saveData),
            GameEra.Yellow,
            cancellationToken);

    private static async ValueTask<CommonItemPurchaseResponse> PurchaseAsync(
        ITaikoDbContext context,
        Ac15ItemShopPurchaseRequest request,
        Ac15ItemShopCatalog catalog,
        Ac15ItemShopSaveRows saveRows,
        GameEra era,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var activeSeason = catalog.IsEnabled ? catalog.ActiveSeason : null;
        if (activeSeason is null || activeSeason.Items.Count == 0)
        {
            return new CommonItemPurchaseResponse { Result = 1, TotalGetDonmedal = 0, TotalUseDonmedal = 0 };
        }

        var seasonState = await GetOrCreateSeasonStateAsync(
            context,
            saveRows,
            era,
            activeSeason.SeasonId,
            cancellationToken);
        if (seasonState is null)
        {
            return new CommonItemPurchaseResponse { Result = 1, TotalGetDonmedal = 0, TotalUseDonmedal = 0 };
        }

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

        var itemTypeValue = item.ItemType.ToProtocolValue();
        if (await HasPurchasedItemAsync(context, era, request.Baid, activeSeason.SeasonId, itemTypeValue, item.ItemId, cancellationToken))
        {
            return Failure(seasonState);
        }

        if (item.Price > available || !CanAdd(seasonState.TotalUseDonmedal, item.Price))
        {
            return Failure(seasonState);
        }

        seasonState.TotalUseDonmedal += item.Price;
        ApplyUnlock(saveRows, era, item.ItemType, item.ItemId);
        await AddPurchasedItemAsync(
            context,
            saveRows,
            era,
            new Ac15PurchasedShopItem(request.Baid, activeSeason.SeasonId, itemTypeValue, item.ItemId, item.ItemNo, item.Price),
            cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Success(seasonState);
    }

    private static async ValueTask<Ac15ShopSeasonState?> GetOrCreateSeasonStateAsync(
        ITaikoDbContext context,
        Ac15ItemShopSaveRows saveRows,
        GameEra era,
        uint seasonId,
        CancellationToken cancellationToken)
    {
        return era switch
        {
            GameEra.Blue when saveRows.Blue is not null => await GetOrCreateSeasonStateAsync(
                token => context.GetOrCreateBlueShopSeasonStateAsync(saveRows.Blue, seasonId, token),
                Ac15ItemShopMapper.ToAc15ShopSeasonState,
                cancellationToken),
            GameEra.Green when saveRows.Green is not null => await GetOrCreateSeasonStateAsync(
                token => context.GetOrCreateGreenShopSeasonStateAsync(saveRows.Green, seasonId, token),
                Ac15ItemShopMapper.ToAc15ShopSeasonState,
                cancellationToken),
            GameEra.Yellow when saveRows.Yellow is not null => await GetOrCreateSeasonStateAsync(
                token => context.GetOrCreateYellowShopSeasonStateAsync(saveRows.Yellow, seasonId, token),
                Ac15ItemShopMapper.ToAc15ShopSeasonState,
                cancellationToken),
            _ => null
        };
    }

    private static async ValueTask<Ac15ShopSeasonState> GetOrCreateSeasonStateAsync<TSeasonState>(
        Func<CancellationToken, ValueTask<TSeasonState>> getOrCreate,
        Func<TSeasonState, Ac15ShopSeasonState> map,
        CancellationToken cancellationToken)
        where TSeasonState : class, IAc15ShopSeasonState
        => map(await getOrCreate(cancellationToken));

    private static async ValueTask<bool> HasPurchasedItemAsync(
        ITaikoDbContext context,
        GameEra era,
        uint baid,
        uint seasonId,
        uint itemType,
        uint itemId,
        CancellationToken cancellationToken)
        => era switch
        {
            GameEra.Blue => await Ac15PurchasedShopItemStates.ContainsAsync(
                context.BlueShopItemStates,
                baid,
                seasonId,
                itemType,
                itemId,
                cancellationToken),
            GameEra.Green => await Ac15PurchasedShopItemStates.ContainsAsync(
                context.GreenShopItemStates,
                baid,
                seasonId,
                itemType,
                itemId,
                cancellationToken),
            GameEra.Yellow => await Ac15PurchasedShopItemStates.ContainsAsync(
                context.YellowShopItemStates,
                baid,
                seasonId,
                itemType,
                itemId,
                cancellationToken),
            _ => false
        };

    private static async ValueTask AddPurchasedItemAsync(
        ITaikoDbContext context,
        Ac15ItemShopSaveRows saveRows,
        GameEra era,
        Ac15PurchasedShopItem item,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        switch (era)
        {
            case GameEra.Blue when saveRows.Blue is not null:
                await AddPurchasedItemAsync(
                    context.BlueShopItemStates,
                    token => context.GetOrCreateBlueShopSeasonStateAsync(saveRows.Blue, item.SeasonId, token),
                    Ac15ItemShopMapper.ToBlueShopItemState,
                    item,
                    now,
                    cancellationToken);
                return;
            case GameEra.Green when saveRows.Green is not null:
                await AddPurchasedItemAsync(
                    context.GreenShopItemStates,
                    token => context.GetOrCreateGreenShopSeasonStateAsync(saveRows.Green, item.SeasonId, token),
                    Ac15ItemShopMapper.ToGreenShopItemState,
                    item,
                    now,
                    cancellationToken);
                return;
            case GameEra.Yellow when saveRows.Yellow is not null:
                await AddPurchasedItemAsync(
                    context.YellowShopItemStates,
                    token => context.GetOrCreateYellowShopSeasonStateAsync(saveRows.Yellow, item.SeasonId, token),
                    Ac15ItemShopMapper.ToYellowShopItemState,
                    item,
                    now,
                    cancellationToken);
                return;
        }
    }

    private static async ValueTask AddPurchasedItemAsync<TSeasonState, TShopItemState>(
        DbSet<TShopItemState> itemStates,
        Func<CancellationToken, ValueTask<TSeasonState>> getOrCreateSeason,
        Func<Ac15PurchasedShopItem, DateTime, TShopItemState> createItem,
        Ac15PurchasedShopItem item,
        DateTime now,
        CancellationToken cancellationToken)
        where TSeasonState : class, IAc15ShopSeasonState
        where TShopItemState : class, IAc15ShopItemState
    {
        var state = await getOrCreateSeason(cancellationToken);
        state.TotalUseDonmedal += item.ItemPrice;
        state.UpdatedAt = now;
        itemStates.Add(createItem(item, now));
    }

    private static void ApplyUnlock(
        Ac15ItemShopSaveRows saveRows,
        GameEra era,
        Ac15ShopItemType itemType,
        uint itemId)
    {
        switch (era)
        {
            case GameEra.Blue when saveRows.Blue is not null:
                ApplyBlueUnlock(saveRows.Blue, itemType, itemId);
                return;
            case GameEra.Green when saveRows.Green is not null:
                ApplyGreenUnlock(saveRows.Green, itemType, itemId);
                return;
            case GameEra.Yellow when saveRows.Yellow is not null:
                ApplyYellowUnlock(saveRows.Yellow, itemType, itemId);
                return;
        }
    }

    private static void ApplyBlueUnlock(UserSaveDataBlue saveData, Ac15ShopItemType itemType, uint itemId)
    {
        switch (itemType)
        {
            case Ac15ShopItemType.Song:
                saveData.ReleaseSongFlg = Ac15ProtocolBytes.SetBits(saveData.ReleaseSongFlg, [itemId], BlueProtocolBytes.SongFlagBytes);
                return;
            case Ac15ShopItemType.Tone:
                saveData.ToneFlg = Ac15ProtocolBytes.SetBits(saveData.ToneFlg, [itemId], BlueProtocolBytes.ToneFlagBytes);
                return;
            case Ac15ShopItemType.Kigurumi:
                saveData.CostumeFlg1 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg1, [itemId], BlueProtocolBytes.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Body:
                saveData.CostumeFlg3 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg3, [itemId], BlueProtocolBytes.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Head:
                saveData.CostumeFlg2 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg2, [itemId], BlueProtocolBytes.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Face:
                saveData.CostumeFlg4 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg4, [itemId], BlueProtocolBytes.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Puchi:
                saveData.CostumeFlg5 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg5, [itemId], BlueProtocolBytes.CostumeFlagBytes);
                return;
            default:
                throw new InvalidOperationException($"Unsupported Blue item shop item type {itemType}.");
        }
    }

    private static void ApplyGreenUnlock(UserSaveDataGreen saveData, Ac15ShopItemType itemType, uint itemId)
    {
        switch (itemType)
        {
            case Ac15ShopItemType.Song:
                return;
            case Ac15ShopItemType.Tone:
                saveData.ToneFlg = Ac15ProtocolBytes.SetBits(saveData.ToneFlg, [itemId], GreenProtocolBytes.ToneFlagBytes);
                return;
            case Ac15ShopItemType.Kigurumi:
                saveData.CostumeFlg1 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg1, [itemId], GreenProtocolBytes.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Body:
                saveData.CostumeFlg3 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg3, [itemId], GreenProtocolBytes.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Head:
                saveData.CostumeFlg2 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg2, [itemId], GreenProtocolBytes.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Face:
                saveData.CostumeFlg4 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg4, [itemId], GreenProtocolBytes.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Puchi:
                saveData.CostumeFlg5 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg5, [itemId], GreenProtocolBytes.CostumeFlagBytes);
                return;
            default:
                throw new InvalidOperationException($"Unsupported Green item shop item type {itemType}.");
        }
    }

    private static void ApplyYellowUnlock(UserSaveDataYellow saveData, Ac15ShopItemType itemType, uint itemId)
    {
        var limits = Ac15EraProfiles.Yellow.Limits;
        switch (itemType)
        {
            case Ac15ShopItemType.Song:
                saveData.ReleaseSongFlg = Ac15ProtocolBytes.SetBits(saveData.ReleaseSongFlg, [itemId], limits.SongFlagBytes);
                return;
            case Ac15ShopItemType.Tone:
                saveData.ToneFlg = Ac15ProtocolBytes.SetBits(saveData.ToneFlg, [itemId], limits.ToneFlagBytes);
                return;
            case Ac15ShopItemType.Kigurumi:
                saveData.CostumeFlg1 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg1, [itemId], limits.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Body:
                saveData.CostumeFlg3 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg3, [itemId], limits.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Head:
                saveData.CostumeFlg2 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg2, [itemId], limits.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Face:
                saveData.CostumeFlg4 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg4, [itemId], limits.CostumeFlagBytes);
                return;
            case Ac15ShopItemType.Puchi:
                saveData.CostumeFlg5 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg5, [itemId], limits.CostumeFlagBytes);
                return;
            default:
                throw new InvalidOperationException($"Unsupported Yellow item shop item type {itemType}.");
        }
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

    private sealed record Ac15ItemShopSaveRows(
        UserSaveDataBlue? Blue = null,
        UserSaveDataGreen? Green = null,
        UserSaveDataYellow? Yellow = null);
}
