using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Application.Catalog.Yellow;

public sealed class YellowItemShopEntry
{
    public uint ItemNo { get; init; }

    public uint ItemId { get; init; }

    public Ac15ShopItemType ItemType { get; init; }

    public uint Price { get; init; }
}
