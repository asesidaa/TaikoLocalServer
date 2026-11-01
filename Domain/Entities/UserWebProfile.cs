using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

/// <summary>
/// A user's WebUI-only data: things the game never sends or reads (UserData stays game data). One row per user,
/// created with the user.
/// </summary>
public class UserWebProfile
{
    public uint Baid { get; set; }

    /// <summary>See <see cref="TaikoLocalServer.Domain.FriendCode"/>. Unique.</summary>
    public string FriendCode { get; set; } = string.Empty;

    /// <summary>Player card background: a game palette index (0 = Don red).</summary>
    public uint CardColor { get; set; }
    public CardPattern CardPattern { get; set; }
    /// <summary>Player card fade: a game palette index, or -1 for none.</summary>
    public int CardFade { get; set; } = -1;
    public CardAnimation CardAnimation { get; set; }

    public virtual UserDatum? Ba { get; set; }
}
