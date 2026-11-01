namespace TaikoLocalServer.Domain.Entities;

/// <summary>Server features an admin turns on or off while the server runs (Server Management › Player Settings). One row (Id 1).</summary>
public class ServerOptions
{
    public int Id { get; set; } = 1;

    /// <summary>
    /// Render players' avatars on the server with a headless Edge or Chrome. Off (the default, so admins opt in to a
    /// browser running on their server): only the WebUI renders them.
    /// </summary>
    public bool RenderAvatars { get; set; }

    /// <summary>
    /// Player cards show don-chan as the animated 3D model ("Enable 3D Renderer"). Off: a still image the server's
    /// renderer draws, lighter for phones and older PCs; that needs <see cref="RenderAvatars"/>, so without it cards stay 3D.
    /// </summary>
    public bool Cards3D { get; set; } = true;
}
