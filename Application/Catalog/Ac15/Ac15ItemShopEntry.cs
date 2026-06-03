using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Application.Catalog.Ac15;

public sealed class Ac15ItemShopEntry
{
    public uint ItemNo { get; init; }

    public uint ItemId { get; init; }

    public Ac15ShopItemType ItemType { get; init; }

    public uint Price { get; init; }
}
