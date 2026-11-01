namespace TaikoLocalServer.Domain.Entities;

/// <summary>Server features an admin turns on or off while the server runs (Server Management › Status). One row (Id 1).</summary>
public class ServerOptions
{
    public int Id { get; set; } = 1;

    /// <summary>Render players' avatars on the server with a headless Edge or Chrome. Off: only the WebUI renders them.</summary>
    public bool RenderAvatars { get; set; } = true;
}
