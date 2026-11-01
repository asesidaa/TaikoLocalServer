namespace TaikoLocalServer.Domain.Entities;

/// <summary>
/// A signed-in Web UI device (Access › Sessions). Its id is in the token (sid); revoking it signs that device out on
/// its next request.
/// </summary>
public class WebSession
{
    public Guid Id { get; set; }
    public uint Baid { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastSeenAt { get; set; }
    public string Ip { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime? RevokedAt { get; set; }
}
