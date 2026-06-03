using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Application.Catalog.Blue;

public sealed class BlueItemShopEntry
{
    public uint ItemNo { get; init; }

    public uint ItemId { get; init; }

    public Ac15ShopItemType ItemType { get; init; }

    public uint Price { get; init; }
}
