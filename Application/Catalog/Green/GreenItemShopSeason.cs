namespace TaikoLocalServer.Application.Catalog.Green;

public sealed class GreenItemShopSeason
{
    public uint SeasonId { get; init; }

    public uint VerupNo { get; init; }

    public string Telop { get; init; } = string.Empty;

    public string StartDatetime { get; init; } = string.Empty;

    public string EndDatetime { get; init; } = string.Empty;

    public uint AfterstartDays { get; init; }

    public uint BeforecloseDays { get; init; }

    public IReadOnlyList<GreenItemShopEntry> Items { get; init; } = [];

    public IReadOnlyDictionary<uint, GreenItemShopEntry> ItemsByNo
        => Items.ToDictionary(item => item.ItemNo);
}
