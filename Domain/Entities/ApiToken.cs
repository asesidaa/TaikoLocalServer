namespace TaikoLocalServer.Domain.Entities;

/// <summary>
/// An admin's API token (Access › API tokens). It acts as the admin who made it, limited to its scopes, and stops
/// working when that admin isn't one any more. Only the SHA-256 of the secret is stored.
/// </summary>
public class ApiToken
{
    public int Id { get; set; }
    public uint CreatedByBaid { get; set; }
    public string Name { get; set; } = string.Empty;
    /// <summary>The first characters of the secret ("tls_ab12"), to tell tokens apart.</summary>
    public string Prefix { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    /// <summary>Space-separated, e.g. "read:scores admin:users".</summary>
    public string Scopes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public string? LastUsedIp { get; set; }
    public DateTime? RevokedAt { get; set; }
}
