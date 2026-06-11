namespace TaikoLocalServer.Application.Ac15;

public static class Ac15ItemShopUnlockPolicies
{
    public static Ac15ItemShopUnlockPolicy<UserSaveDataBlue> Blue { get; } =
        Create(Ac15UnlockFlagAccess.Blue, nameof(GameEra.Blue));

    public static Ac15ItemShopUnlockPolicy<UserSaveDataGreen> Green { get; } =
        Create(Ac15UnlockFlagAccess.Green, nameof(GameEra.Green));

    public static Ac15ItemShopUnlockPolicy<UserSaveDataYellow> Yellow { get; } =
        Create(Ac15UnlockFlagAccess.Yellow, nameof(GameEra.Yellow));

    private static Ac15ItemShopUnlockPolicy<TSave> Create<TSave>(
        Ac15UnlockFlagAccess<TSave> access,
        string eraName)
        => new(
            itemType => itemType.IsSupported(),
            (saveData, itemType, itemId) => ApplyUnlock(access, eraName, saveData, itemType, itemId));

    private static void ApplyUnlock<TSave>(
        Ac15UnlockFlagAccess<TSave> access,
        string eraName,
        TSave saveData,
        Ac15ShopItemType itemType,
        uint itemId)
    {
        switch (itemType)
        {
            case Ac15ShopItemType.Song when access.ReleaseSongs is not null:
                access.ReleaseSongs(saveData, [itemId]);
                return;
            case Ac15ShopItemType.Song:
                return;
            case Ac15ShopItemType.Tone:
                access.Tones(saveData, [itemId]);
                return;
            case Ac15ShopItemType.Kigurumi:
                access.Costume1(saveData, [itemId]);
                return;
            case Ac15ShopItemType.Body:
                access.Costume3(saveData, [itemId]);
                return;
            case Ac15ShopItemType.Head:
                access.Costume2(saveData, [itemId]);
                return;
            case Ac15ShopItemType.Face:
                access.Costume4(saveData, [itemId]);
                return;
            case Ac15ShopItemType.Puchi:
                access.Costume5(saveData, [itemId]);
                return;
            default:
                throw new InvalidOperationException($"Unsupported {eraName} item shop item type {itemType}.");
        }
    }
}
