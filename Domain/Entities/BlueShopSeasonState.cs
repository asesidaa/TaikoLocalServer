namespace TaikoLocalServer.Domain.Entities;

public sealed class BlueShopSeasonState
{
    public uint Baid { get; set; }

    public uint SeasonId { get; set; }

    public uint TotalGetDonmedal { get; set; }

    public uint TotalUseDonmedal { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public UserDatum? Ba { get; set; }
}
