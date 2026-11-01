namespace TaikoLocalServer.Domain.Entities;

/// <summary>
/// A card the server refuses (at the cabinet and for Web UI sign-in). Bans follow the card, not the user: the account
/// and scores stay, and moving the code keeps its ban.
/// </summary>
public class BannedCard
{
    public string AccessCode { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public uint? BannedBy { get; set; }
    public DateTime BannedAt { get; set; }
    /// <summary>Null: permanent.</summary>
    public DateTime? Until { get; set; }
    public int RefusedCount { get; set; }
}
