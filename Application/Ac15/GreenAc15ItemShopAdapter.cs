namespace TaikoLocalServer.Application.Ac15;

public sealed class GreenAc15ItemShopAdapter(ITaikoDbContext context, UserSaveDataGreen saveData)
    : IAc15ItemShopPersistence, IAc15ItemShopUnlockPolicy
{
    public async ValueTask<Ac15ShopSeasonState?> GetOrCreateActiveSeasonStateAsync(
        uint baid,
        uint seasonId,
        CancellationToken cancellationToken)
    {
        var state = await context.GetOrCreateGreenShopSeasonStateAsync(saveData, seasonId, cancellationToken);
        return new Ac15ShopSeasonState(state.Baid, state.SeasonId, state.TotalGetDonmedal, state.TotalUseDonmedal);
    }

    public async ValueTask<bool> HasPurchasedItemAsync(
        uint baid,
        uint seasonId,
        uint itemType,
        uint itemId,
        CancellationToken cancellationToken)
        => await Ac15PurchasedShopItemStates.ContainsAsync(
            context.GreenShopItemStates,
            baid,
            seasonId,
            itemType,
            itemId,
            cancellationToken);

    public async ValueTask AddPurchasedItemAsync(Ac15PurchasedShopItem item, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var state = await context.GetOrCreateGreenShopSeasonStateAsync(saveData, item.SeasonId, cancellationToken);
        state.TotalUseDonmedal += item.ItemPrice;
        state.UpdatedAt = now;
        context.GreenShopItemStates.Add(Ac15PurchasedShopItemStates.Create<GreenShopItemState>(item, now));
    }

    public ValueTask SaveChangesAsync(CancellationToken cancellationToken)
        => Ac15PurchasedShopItemStates.SaveAsync(context, cancellationToken);

    public void ApplyUnlock(Ac15ShopItemType itemType, uint itemId)
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
}
