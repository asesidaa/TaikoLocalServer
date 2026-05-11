namespace TaikoLocalServer.Domain.Entities;

public class GreenGhostWinnings
{
    public uint Baid { get; set; }
    public uint LevelId { get; set; }
    public uint Winnings { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
