using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

public interface IAc15ShopItemState
{
    uint Baid { get; set; }

    uint SeasonId { get; set; }

    uint ItemType { get; set; }

    uint ItemId { get; set; }

    uint ItemNo { get; set; }

    uint ItemPrice { get; set; }

    Ac15ShopItemStatus Status { get; set; }

    DateTime PurchasedAt { get; set; }

    DateTime? UnlockedAt { get; set; }
}
