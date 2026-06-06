namespace TaikoLocalServer.Application.Ac15;

public interface IAc15ItemShopUnlockPolicy
{
    void ApplyUnlock(Ac15ShopItemType itemType, uint itemId);
}
