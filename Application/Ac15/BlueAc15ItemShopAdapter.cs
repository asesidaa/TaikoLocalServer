namespace TaikoLocalServer.Application.Ac15;

public sealed class BlueAc15ItemShopAdapter(ITaikoDbContext context, UserSaveDataBlue saveData)
    : IAc15ItemShopPersistence, IAc15ItemShopUnlockPolicy
{
    public async ValueTask<Ac15ShopSeasonState?> GetOrCreateActiveSeasonStateAsync(
        uint baid,
        uint seasonId,
        CancellationToken cancellationToken)
    {
        var state = await context.GetOrCreateBlueShopSeasonStateAsync(baid, seasonId, cancellationToken);
        return new Ac15ShopSeasonState(state.Baid, state.SeasonId, state.TotalGetDonmedal, state.TotalUseDonmedal);
    }

    public async ValueTask<bool> HasPurchasedItemAsync(
        uint baid,
        uint seasonId,
        uint itemType,
        uint itemId,
        CancellationToken cancellationToken)
        => await Ac15PurchasedShopItemStates.ContainsAsync(
            context.BlueShopItemStates,
            baid,
            seasonId,
            itemType,
            itemId,
            cancellationToken);

    public async ValueTask AddPurchasedItemAsync(Ac15PurchasedShopItem item, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var state = await context.GetOrCreateBlueShopSeasonStateAsync(item.Baid, item.SeasonId, cancellationToken);
        state.TotalUseDonmedal += item.ItemPrice;
        state.UpdatedAt = now;
        context.BlueShopItemStates.Add(Ac15PurchasedShopItemStates.Create<BlueShopItemState>(item, now));
    }

    public ValueTask SaveChangesAsync(CancellationToken cancellationToken)
        => Ac15PurchasedShopItemStates.SaveAsync(context, cancellationToken);

    public void ApplyUnlock(Ac15ShopItemType itemType, uint itemId)
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
}
