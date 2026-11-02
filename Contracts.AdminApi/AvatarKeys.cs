namespace TaikoLocalServer.Contracts.AdminApi;

/// <summary>
/// What an avatar shows, shared by the WebUI (which renders and uploads its own) and the server's renderer, so both
/// store and request the same key.
/// </summary>
public static class AvatarKeys
{
    /// <summary>The clip each card animation loops; an avatar shows its first frame.</summary>
    public static string Clip(CardAnimation a) => a switch
    {
        CardAnimation.Celebrate => "don_result_full_loop",
        CardAnimation.Aggressive => "don_wait_loop",
        CardAnimation.Miss => "don_miss",
        CardAnimation.FullCombo => "don_full_combo",
        CardAnimation.Pout => "don_result_failure_loop",
        _ => "don_result_clear_loop",
    };

    /// <summary>
    /// The costume (without the puchi, which avatars leave out), colours and pose. It's in the avatar's URL, so any
    /// change, in game too, misses the stored image until a new one is rendered. Bump the leading version to re-render
    /// all, e.g. after changing the framing (AvatarFraming in the server renderer).
    /// </summary>
    public static string Key(uint kigurumi, uint head, uint body, uint face, uint bodyColor, uint faceColor, uint limbColor, CardAnimation animation) =>
        $"4-{kigurumi}-{head}-{body}-{face}-{bodyColor}-{faceColor}-{limbColor}-{(int)animation}";

    /// <summary>
    /// The still image (the 2D view's full-body don-chan) for this costume, with its puchi. Same idea as <see cref="Key"/>:
    /// bump the version after changing its framing (StillFraming in the server renderer).
    /// </summary>
    public static string StillKey(uint kigurumi, uint head, uint body, uint face, uint puchi, uint bodyColor, uint faceColor, uint limbColor, CardAnimation animation) =>
        $"s4-{kigurumi}-{head}-{body}-{face}-{puchi}-{bodyColor}-{faceColor}-{limbColor}-{(int)animation}";
}
