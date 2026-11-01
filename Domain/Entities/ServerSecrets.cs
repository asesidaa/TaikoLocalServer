namespace TaikoLocalServer.Domain.Entities;

/// <summary>Secrets the server generates for itself, so operators never configure them. One row (Id 1).</summary>
public class ServerSecrets
{
    public int Id { get; set; } = 1;

    /// <summary>Signs login tokens and derives invite codes. Generated at first start (64 random bytes, base64).</summary>
    public string JwtKey { get; set; } = string.Empty;
}
