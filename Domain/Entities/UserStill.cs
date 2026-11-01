namespace TaikoLocalServer.Domain.Entities;

/// <summary>
/// The user's don-chan as a still image (full body with the puchi, transparent), drawn by the server's avatar
/// renderer for the WebUI's 2D view. One row per user, replaced on each render; kept apart like UserAvatar.
/// </summary>
public class UserStill
{
    public const int KeyMaxLength = 64;
    public const int MaxBytes = 512 * 1024;

    public uint Baid { get; set; }

    public byte[] Image { get; set; } = [];

    public string ContentType { get; set; } = string.Empty;

    /// <summary>The costume the image shows (AvatarKeys.StillKey). A request for another key is a miss (stale).</summary>
    public string Key { get; set; } = string.Empty;

    public virtual UserDatum? Ba { get; set; }
}
