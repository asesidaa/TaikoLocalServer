using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

/// <summary>
/// A user's WebUI-only data: things the game never sends or reads (UserData stays game data). One row per user,
/// created with the user.
/// </summary>
public class UserWebProfile
{
    public uint Baid { get; set; }


    /// <summary>Player card background: a game palette index (0 = Don red).</summary>
    public uint CardColor { get; set; }
    public CardPattern CardPattern { get; set; }
    /// <summary>Player card fade: a game palette index, or -1 for none.</summary>
    public int CardFade { get; set; } = -1;
    public CardAnimation CardAnimation { get; set; }

    /// <summary>The Web UI theme they last picked (top bar), applied when they sign in; null: never picked.</summary>
    public bool? DarkMode { get; set; }
    public string? Accent { get; set; }
    /// <summary>The Web UI language they last picked (a culture code such as "ja"), applied when they sign in.</summary>
    public string? Language { get; set; }
    /// <summary>How finely player cards draw the 3D don-chan for them (Settings › Display); null = never picked.</summary>
    public RenderQuality? Quality3D { get; set; }
    /// <summary>The language song titles show in here (Settings › Display): a profile Language (0 Japanese … 3 Korean);
    /// null = the profile's own Language.</summary>
    public uint? SongLanguage { get; set; }

    public virtual UserDatum? Ba { get; set; }
}
