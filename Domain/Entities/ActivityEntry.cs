namespace TaikoLocalServer.Domain.Entities;

/// <summary>One admin-visible action (Access › Activity): who did what to whom. Kept 365 days.</summary>
public class ActivityEntry
{
    public long Id { get; set; }
    public DateTime Time { get; set; }
    /// <summary>The signed-in user who did it; null in Local mode (no accounts) or from the console.</summary>
    public uint? ActorBaid { get; set; }
    /// <summary>A stable dotted name, e.g. "user.promote", "score.remove", "card.move".</summary>
    public string Action { get; set; } = string.Empty;
    public uint? TargetBaid { get; set; }
    /// <summary>Human-readable specifics (song and difficulty, masked access code, …).</summary>
    public string Detail { get; set; } = string.Empty;
}
