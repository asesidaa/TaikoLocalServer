using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

public sealed class YellowShopItemState : IAc15ShopItemState
{
    public uint Baid { get; set; }

    public uint SeasonId { get; set; }

    public uint ItemType { get; set; }

    public uint ItemId { get; set; }

    public uint ItemNo { get; set; }

    public uint ItemPrice { get; set; }

    public Ac15ShopItemStatus Status { get; set; }

    public DateTime PurchasedAt { get; set; }

    public DateTime? UnlockedAt { get; set; }

    public UserDatum? Ba { get; set; }
}
