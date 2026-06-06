namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15ItemShopPurchaseRequest(
    uint Baid,
    uint ItemNo,
    uint? ItemType,
    uint? ItemId,
    uint? ItemPrice);

public sealed record Ac15ShopSeasonState(
    uint Baid,
    uint SeasonId,
    uint TotalGetDonmedal,
    uint TotalUseDonmedal)
{
    public uint TotalGetDonmedal { get; set; } = TotalGetDonmedal;
    public uint TotalUseDonmedal { get; set; } = TotalUseDonmedal;
}

public sealed record Ac15PurchasedShopItem(
    uint Baid,
    uint SeasonId,
    uint ItemType,
    uint ItemId,
    uint ItemNo,
    uint ItemPrice);
