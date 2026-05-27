namespace TaikoLocalServer.Application.Catalog.Ac15;

public sealed class Ac15ItemShopSeason
{
    public uint SeasonId { get; init; }

    public uint VerupNo { get; init; }

    public string Telop { get; init; } = string.Empty;

    public string StartDatetime { get; init; } = string.Empty;

    public string EndDatetime { get; init; } = string.Empty;

    public uint AfterstartDays { get; init; }

    public uint BeforecloseDays { get; init; }

    public IReadOnlyList<Ac15ItemShopEntry> Items { get; init; } = [];

    public IReadOnlyDictionary<uint, Ac15ItemShopEntry> ItemsByNo
        => Items.ToDictionary(item => item.ItemNo);
}
