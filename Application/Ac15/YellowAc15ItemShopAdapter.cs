namespace TaikoLocalServer.Application.Ac15;

public sealed class YellowAc15ItemShopAdapter(ITaikoDbContext context, UserSaveDataYellow saveData)
    : IAc15ItemShopPersistence, IAc15ItemShopUnlockPolicy
{
    public async ValueTask<Ac15ShopSeasonState?> GetOrCreateActiveSeasonStateAsync(
        uint baid,
        uint seasonId,
        CancellationToken cancellationToken)
    {
        var state = await context.GetOrCreateYellowShopSeasonStateAsync(saveData, seasonId, cancellationToken);
        return new Ac15ShopSeasonState(state.Baid, state.SeasonId, state.TotalGetDonmedal, state.TotalUseDonmedal);
    }

    public async ValueTask<bool> HasPurchasedItemAsync(
        uint baid,
        uint seasonId,
        uint itemType,
        uint itemId,
        CancellationToken cancellationToken)
        => await Ac15PurchasedShopItemStates.ContainsAsync(
            context.YellowShopItemStates,
            baid,
            seasonId,
            itemType,
            itemId,
            cancellationToken);

    public async ValueTask AddPurchasedItemAsync(Ac15PurchasedShopItem item, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var state = await context.GetOrCreateYellowShopSeasonStateAsync(saveData, item.SeasonId, cancellationToken);
        state.TotalUseDonmedal += item.ItemPrice;
        state.UpdatedAt = now;
        context.YellowShopItemStates.Add(Ac15PurchasedShopItemStates.Create<YellowShopItemState>(item, now));
    }

    public ValueTask SaveChangesAsync(CancellationToken cancellationToken)
        => Ac15PurchasedShopItemStates.SaveAsync(context, cancellationToken);

    public void ApplyUnlock(Ac15ShopItemType itemType, uint itemId)
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
}
