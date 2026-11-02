namespace TaikoLocalServer.Domain.Entities;

public sealed class BlueBattleTokenState
{
    public uint Baid { get; set; }

    public uint TokenId { get; set; }

    public uint? TokenValue { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public UserDatum? Ba { get; set; }
}
