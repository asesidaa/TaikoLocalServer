using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15CostumeFlags(
    byte[] CostumeFlg1,
    byte[] CostumeFlg2,
    byte[] CostumeFlg3,
    byte[] CostumeFlg4,
    byte[] CostumeFlg5);

public static class Ac15CustomizationMutation
{
    public static void ApplyCurrentCostume<TSave>(
        TSave saveData,
        CommonPlayResultData.CostumeData costume,
        Ac15ProtocolLimits limits)
        where TSave : IAc15CustomizationSaveData
    {
        saveData.Costume1 = costume.Costume1;
        saveData.Costume2 = costume.Costume2;
        saveData.Costume3 = costume.Costume3;
        saveData.Costume4 = costume.Costume4;
        saveData.Costume5 = costume.Costume5;
        saveData.CostumeFlg1 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg1, [costume.Costume1], limits.CostumeFlagBytes);
        saveData.CostumeFlg2 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg2, [costume.Costume2], limits.CostumeFlagBytes);
        saveData.CostumeFlg3 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg3, [costume.Costume3], limits.CostumeFlagBytes);
        saveData.CostumeFlg4 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg4, [costume.Costume4], limits.CostumeFlagBytes);
        saveData.CostumeFlg5 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg5, [costume.Costume5], limits.CostumeFlagBytes);
    }

    public static void ApplyDanCostume<TSave>(
        TSave saveData,
        uint costumeId,
        Ac15ProtocolLimits limits)
        where TSave : IAc15CustomizationSaveData
    {
        saveData.Costume1 = costumeId;
        saveData.CostumeFlg1 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg1, [costumeId], limits.CostumeFlagBytes);
    }

    public static Ac15CostumeFlags ApplyActiveShopCostumeLocks<TSave>(
        TSave saveData,
        Ac15ItemShopSeason? activeSeason,
        IReadOnlyCollection<(uint ItemType, uint ItemId)> unlockedShopItems,
        Ac15ProtocolLimits limits)
        where TSave : IAc15CustomizationSaveData
    {
        IEnumerable<uint> LockedIds(Ac15ShopItemType itemType) => activeSeason?.Items
            .Where(item => item.ItemType == itemType && !unlockedShopItems.Contains((item.ItemType.ToProtocolValue(), item.ItemId)))
            .Select(item => item.ItemId) ?? [];

        return new Ac15CostumeFlags(
            Ac15ProtocolBytes.ClearBits(saveData.CostumeFlg1, LockedIds(Ac15ShopItemType.Kigurumi), limits.CostumeFlagBytes),
            Ac15ProtocolBytes.ClearBits(saveData.CostumeFlg2, LockedIds(Ac15ShopItemType.Head), limits.CostumeFlagBytes),
            Ac15ProtocolBytes.ClearBits(saveData.CostumeFlg3, LockedIds(Ac15ShopItemType.Body), limits.CostumeFlagBytes),
            Ac15ProtocolBytes.ClearBits(saveData.CostumeFlg4, LockedIds(Ac15ShopItemType.Face), limits.CostumeFlagBytes),
            Ac15ProtocolBytes.ClearBits(saveData.CostumeFlg5, LockedIds(Ac15ShopItemType.Puchi), limits.CostumeFlagBytes));
    }
}
