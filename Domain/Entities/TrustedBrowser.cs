namespace TaikoLocalServer.Domain.Entities;

/// <summary>
/// "Trust this browser for 30 days" at 2-step sign-in: that browser skips the code until it expires. Ends when 2-step
/// is turned off or reset.
/// </summary>
public class TrustedBrowser
{
    public int Id { get; set; }
    public uint Baid { get; set; }
    /// <summary>SHA-256 hex of the token the browser keeps.</summary>
    public string TokenHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string UserAgent { get; set; } = string.Empty;
}
