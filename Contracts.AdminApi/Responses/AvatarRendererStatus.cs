namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <param name="Enabled">The admin's switch.</param>
/// <param name="Browser">The Edge or Chrome it uses; null when none was found.</param>
/// <param name="Running">The browser is open (it closes after a few idle minutes).</param>
/// <param name="Cards3D">The admin's "Enable 3D Renderer": player cards show the 3D model; off, a still image this renderer draws.</param>
public sealed record AvatarRendererStatus(bool Enabled, string? Browser, bool Running, int Queued, int Rendered, int Failed,
    string? LastError, DateTime? LastRenderedAt, bool Cards3D = true);

/// <summary>One stored avatar (the image itself is at api/Users/{baid}/Avatar?v={key}).</summary>
/// <param name="Key">The costume, colours and pose it shows (AvatarKeys).</param>
public sealed record StoredAvatar(uint Baid, string Name, string Key, string ContentType, int Bytes);
