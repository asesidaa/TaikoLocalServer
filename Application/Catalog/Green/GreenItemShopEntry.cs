using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Application.Catalog.Green;

public sealed class GreenItemShopEntry
{
    public uint ItemNo { get; init; }

    public uint ItemId { get; init; }

    public Ac15ShopItemType ItemType { get; init; }

    public uint Price { get; init; }
}
