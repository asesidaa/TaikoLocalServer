namespace TaikoLocalServer.Application.Dtos.Ac15;

public sealed record Ac15BaidResponse
{
    public uint Result { get; init; }
    public bool IsNewUser { get; init; }
    public uint Baid { get; init; }
    public Ac15BaidIdentity? Identity { get; init; }
    public Ac15BaidProfile? MydonProfile { get; init; }
    public Ac15BaidCostumeFlags? CustomizationInventory { get; init; }
    public Ac15BaidShopMedals? ShopMedalBalance { get; init; }
    public Ac15BaidDan? DanStatus { get; init; }
    public Ac15BaidCompatibility? CompatibilityProfile { get; init; }
    public Ac15BaidReward? RewardProgress { get; init; }
}

public sealed record Ac15BaidIdentity(
    string MyDonName,
    uint MyDonNameLanguage);

public sealed record Ac15BaidProfile
{
    public string Title { get; init; } = string.Empty;
    public uint TitlePlateId { get; init; }
    public uint ColorFace { get; init; }
    public uint ColorBody { get; init; }
    public uint ColorLimb { get; init; }
    public Ac15CostumeFacts SelectedCostume { get; init; } = Ac15CostumeFacts.Empty;
    public bool? IsAutoCostumeOn { get; init; }
    public uint? DefaultToneSetting { get; init; }
    public string LastPlayDatetime { get; init; } = string.Empty;
}

public sealed record Ac15BaidCostumeFlags(
    byte[] CostumeFlg1,
    byte[] CostumeFlg2,
    byte[] CostumeFlg3,
    byte[] CostumeFlg4,
    byte[] CostumeFlg5);

public sealed record Ac15BaidShopMedals(
    uint TotalGetDonmedal,
    uint TotalUseDonmedal,
    uint TotalGetKatsumedal,
    uint TotalUseKatsumedal,
    uint? ItemshopTutorialFlg);

public sealed record Ac15BaidDan(
    uint DispDanType,
    uint GotDanMax,
    byte[] GotDanFlg,
    byte[] GotDanExtraFlg);

public sealed record Ac15BaidCompatibility(
    string? PersonId,
    uint? WaiwaiTutorialFlg);

public sealed record Ac15BaidReward(uint? RewardPtn);
