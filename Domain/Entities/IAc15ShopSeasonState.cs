namespace TaikoLocalServer.Domain.Entities;

public interface IAc15ShopSeasonState
{
    uint Baid { get; set; }

    uint SeasonId { get; set; }

    uint TotalGetDonmedal { get; set; }

    uint TotalUseDonmedal { get; set; }

    DateTime CreatedAt { get; set; }

    DateTime UpdatedAt { get; set; }
}
