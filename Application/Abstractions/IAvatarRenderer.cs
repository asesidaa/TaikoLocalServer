using TaikoLocalServer.Contracts.AdminApi.Responses;

namespace TaikoLocalServer.Application.Abstractions;

/// <summary>
/// Renders players' avatars on the server (a headless browser running the WebUI's 3D embed) and stores them, so
/// every player has one, including those who never open the WebUI. Admins can turn it off.
/// </summary>
public interface IAvatarRenderer
{
    AvatarRendererStatus Status { get; }

    /// <summary>Queues a render for <paramref name="baid"/> when their stored avatar doesn't match their costume. No-op when off.</summary>
    void Enqueue(uint baid);

    Task SetEnabledAsync(bool enabled, CancellationToken cancellationToken);

    /// <summary>True for the renderer's own sign-in token (asset reads only).</summary>
    bool IsRendererToken(string token);
}
