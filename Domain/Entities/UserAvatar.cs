namespace TaikoLocalServer.Domain.Entities;

/// <summary>
/// The user's avatar: a small image the WebUI renders from the 3D model and uploads. One row per user, replaced on
/// each upload; kept out of UserWebProfiles so profile queries never load the blob.
/// </summary>
public class UserAvatar
{
    public const int KeyMaxLength = 64;
    public const int MaxBytes = 128 * 1024;

    public uint Baid { get; set; }

    public byte[] Image { get; set; } = [];

    /// <summary>image/webp or image/png, from the uploaded bytes.</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>The costume the image shows, as the WebUI writes it. A request for another key is a miss (stale).</summary>
    public string Key { get; set; } = string.Empty;

    public virtual UserDatum? Ba { get; set; }
}
