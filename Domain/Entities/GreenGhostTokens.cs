namespace TaikoLocalServer.Domain.Entities;

public class GreenGhostTokens
{
    public uint Baid { get; set; }
    public uint TokenId { get; set; }
    public uint TokenValue { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
