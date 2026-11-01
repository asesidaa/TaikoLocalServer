using TaikoLocalServer.Contracts.AdminApi.Responses;

namespace TaikoLocalServer.Application.Abstractions;

/// <summary>
/// Renders players' avatars on the server (a headless browser running the WebUI's 3D embed) and stores them, so
/// every player has one, including those who never open the WebUI. Admins can turn it off.
/// </summary>
public interface IAvatarRenderer
{
    AvatarRendererStatus Status { get; }

    /// <summary>
    /// Queues a render for <paramref name="baid"/> when their stored avatar or still doesn't match their costume. No-op
    /// when off. <paramref name="changed"/>: the player just saved their costume, which (re)starts the settle wait so a
    /// burst of edits makes one render; a request for a missing image (asked again while it's drawn) doesn't.
    /// </summary>
    void Enqueue(uint baid, bool changed = false);

    Task SetEnabledAsync(bool enabled, CancellationToken cancellationToken);

    /// <summary>"Enable 3D Renderer": player cards show the 3D model (on) or still images this renderer draws (off).</summary>
    Task SetCards3DAsync(bool on, CancellationToken cancellationToken);

    /// <summary>True for the renderer's own sign-in token (asset reads only).</summary>
    bool IsRendererToken(string token);
}
