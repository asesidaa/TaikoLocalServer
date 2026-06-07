namespace TaikoLocalServer.Application.Catalog.Yellow;

public sealed class YellowItemShopSeason
{
    public uint SeasonId { get; init; }

    public uint VerupNo { get; init; }

    public string Telop { get; init; } = string.Empty;

    public string StartDatetime { get; init; } = string.Empty;

    public string EndDatetime { get; init; } = string.Empty;

    public uint AfterstartDays { get; init; }

    public uint BeforecloseDays { get; init; }

    public IReadOnlyList<YellowItemShopEntry> Items { get; init; } = [];

    public IReadOnlyDictionary<uint, YellowItemShopEntry> ItemsByNo
        => Items.ToDictionary(item => item.ItemNo);
}
