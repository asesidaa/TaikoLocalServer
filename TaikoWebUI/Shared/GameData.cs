namespace TaikoWebUI.Shared;

/// <summary>Fixed game lists the WebUI shows: colours, title plates, dan ranks and play options.</summary>
public static class GameData
{
    /// <summary>The 63 costume colours, by game index.</summary>
    public static readonly string[] Colors =
    [
        "#F84828", "#68C0C0", "#DC1500", "#F8F0E0", "#009687", "#00BF87", "#00FF9A", "#66FFC2", "#FFFFFF",
        "#690000", "#FF0000", "#FF6666", "#FFB3B3", "#00BCC2", "#00F7FF", "#66FAFF", "#B3FDFF", "#E4E4E4",
        "#993800", "#FF5E00", "#FF9E78", "#FFCFB3", "#005199", "#0088FF", "#66B8FF", "#B3DBFF", "#B9B9B9",
        "#B37700", "#FFAA00", "#FFCC66", "#FFE2B3", "#000C80", "#0019FF", "#6675FF", "#B3BAFF", "#858585",
        "#B39B00", "#FFDD00", "#FFFF00", "#FFFF71", "#2B0080", "#5500FF", "#9966FF", "#CCB3FF", "#505050",
        "#38A100", "#78C900", "#B3FF00", "#DCFF8A", "#610080", "#C400FF", "#DC66FF", "#EDB3FF", "#232323",
        "#006600", "#00B800", "#00FF00", "#8AFF9E", "#990059", "#FF0095", "#FF66BF", "#FFB3DF", "#000000",
    ];

    /// <summary>How the game groups them: 12 hue families × 4 tints (dark → light), a neutral ramp, the 8 classic colours.</summary>
    public static readonly (string Name, uint[] Ids)[] Hues =
    [
        ("Red", [9, 10, 11, 12]), ("Orange", [18, 19, 20, 21]), ("Gold", [27, 28, 29, 30]), ("Yellow", [36, 37, 38, 39]),
        ("Lime", [45, 46, 47, 48]), ("Green", [54, 55, 56, 57]), ("Cyan", [13, 14, 15, 16]), ("Blue", [22, 23, 24, 25]),
        ("Indigo", [31, 32, 33, 34]), ("Purple", [40, 41, 42, 43]), ("Violet", [49, 50, 51, 52]), ("Pink", [58, 59, 60, 61]),
    ];
    public static readonly uint[] Neutrals = [62, 53, 44, 35, 26, 17, 8];
    public static readonly uint[] Classic = [0, 1, 2, 3, 4, 5, 6, 7];
    private static readonly string[] NeutralNames = ["Black", "Charcoal", "Graphite", "Grey", "Silver", "Mist", "White"];
    private static readonly string[] ClassicNames = ["Don red", "Ka blue", "Crimson", "Cream", "Teal", "Jade", "Mint", "Seafoam"];

    public static string ColorName(uint i)
    {
        foreach (var (name, ids) in Hues)
        {
            var at = Array.IndexOf(ids, i);
            if (at >= 0) return $"{name} {at + 1}";
        }
        var n = Array.IndexOf(Neutrals, i);
        if (n >= 0) return NeutralNames[n];
        return i < ClassicNames.Length ? ClassicNames[i] : $"#{i}";
    }

    public static string Color(uint i) => i < Colors.Length ? Colors[i] : Colors[0];

    /// <summary>Title plates in TitlePlateId order: display name and the Lumen / extract label.</summary>
    public static readonly (string Name, string Label)[] TitlePlates =
    [
        ("Wood", "wood"), ("Rainbow", "rainbow"), ("Gold", "gold"), ("Purple", "purple"),
        ("AI 1", "ai_1"), ("AI 2", "ai_2"), ("AI 3", "ai_3"), ("AI 4", "ai_4"), ("Onp 1", "onp_1"),
        ("Toho Y22 QR", "toho_y22_qr"), ("Toho Y22 1", "toho_y22_1"), ("Toho Y22 2", "toho_y22_2"), ("Toho Y22 3", "toho_y22_3"),
        ("Toho Y22 4", "toho_y22_4"), ("Toho Y22 5", "toho_y22_5"), ("AprilFool 1", "aprilfool_1"), ("AprilFool 2", "aprilfool_2"),
        ("AprilFool 3", "aprilfool_3"), ("AprilFool 4", "aprilfool_4"), ("AprilFool 5", "aprilfool_5"), ("AprilFool 6", "aprilfool_6"),
    ];

    public static string PlateLabel(uint id) => id < TitlePlates.Length ? TitlePlates[id].Label : "wood";

    /// <summary>Dani Dojo ranks by DanId (1-based): badge id (extract's dan_&lt;id&gt;_{red,gold}), kanji, localisation key.</summary>
    public static readonly (string Badge, string Kanji, string Key)[] Dans =
    [
        ("5kyuu", "五級", "Fifth Kyuu"), ("4kyuu", "四級", "Fourth Kyuu"), ("3kyuu", "三級", "Third Kyuu"), ("2kyuu", "二級", "Second Kyuu"),
        ("1kyuu", "一級", "First Kyuu"), ("1dan", "初段", "First Dan"), ("2dan", "二段", "Second Dan"), ("3dan", "三段", "Third Dan"),
        ("4dan", "四段", "Fourth Dan"), ("5dan", "五段", "Fifth Dan"), ("6dan", "六段", "Sixth Dan"), ("7dan", "七段", "Seventh Dan"),
        ("8dan", "八段", "Eighth Dan"), ("9dan", "九段", "Ninth Dan"), ("10dan", "十段", "Tenth Dan"), ("11dan", "玄人", "Kuroto"),
        ("12dan", "名人", "Meijin"), ("13dan", "超人", "Chojin"), ("14dan", "達人", "Tatsujin"),
    ];

    public static bool IsGold(DanClearState s) => s >= DanClearState.GoldNormalClear;

    /// <summary>0 = clear, 1 = full combo, 2 = donderful (perfect).</summary>
    public static int ClearType(DanClearState s) => s == DanClearState.NotClear ? 0 : ((int)s - 1) % 3;

    public static readonly string[] Speeds = ["1.0", "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "1.7", "1.8", "1.9", "2.0", "2.5", "3.0", "3.5", "4.0"];

    // ---- Game icons, extracted by tools/extract into game_data/ui (served by api/Assets; see ui.js hydrateAssets) ----

    /// <summary>A song option icon: doron, abekobe (inverse), kimagure / detarame (random), note_pos.</summary>
    public static string OptionIcon(string label) => $"ui/option_{label}.png";

    /// <summary>The speed icon for <see cref="Speeds"/>[index]; ×1.0 has none in the game.</summary>
    public static string? SpeedIcon(uint index) =>
        index is > 0 and < 15 ? OptionIcon($"speedx{Speeds[index].TrimEnd('0').TrimEnd('.')}") : null;

    public static string DifficultyIcon(Difficulty d) => $"ui/difficulty_{d switch
    {
        Difficulty.Easy => "easy", Difficulty.Normal => "normal", Difficulty.Hard => "hard", Difficulty.Oni => "oni", _ => "ura",
    }}.png";

    /// <summary>1 = clear (silver), 2 = full combo (gold), 3 = donderful (rainbow).</summary>
    public static string CrownIcon(int crown) => $"ui/crown_badge_crown_{crown}.png";

    /// <summary>The Dani Dojo pass stamp (合格, red or gold; clear / full combo / perfect); null when not passed.</summary>
    public static string? DanStamp(DanClearState s) => s == DanClearState.NotClear ? null
        : $"ui/dani_stamp_{(IsGold(s) ? "gold" : "red")}_0{ClearType(s) + 1}.png";

    public static readonly string[] Tones =
    [
        "Taiko", "Matsuri", "Inuneko", "Wonderfultaiko", "Drum", "Tambourine", "Wadadon", "Clapping", "Conga", "8bittaiko",
        "Soya", "Mekadon", "Funassyi", "Wrap", "Isogai", "Akemi", "Synthdrum", "Shuriken", "Puchipuchi", "Electric Guitar",
    ];

    public static readonly string[] NameLanguages = ["Japanese", "English", "Chinese Traditional", "Korean", "Chinese Simplified"];

    public static readonly (Difficulty Value, string Name)[] Difficulties =
        [(Difficulty.Easy, "Easy"), (Difficulty.Normal, "Normal"), (Difficulty.Hard, "Hard"), (Difficulty.Oni, "Oni"), (Difficulty.UraOni, "Ura Oni")];

    public static readonly string[] CourseOptions = ["None", "Set Up Each Time", "Easy", "Normal", "Hard", "Oni", "Ura Oni"];
    public static readonly string[] StarOptions = ["None", "Set Up Each Time", "1 Star", "2 Star", "3 Star", "4 Star", "5 Star", "6 Star", "7 Star", "8 Star", "9 Star", "10 Star"];
    public static readonly string[] SortOptions = ["None", "Set Up Each Time", "Default", "Not Cleared", "Not Full Combo", "Not Donderful Combo"];

    /// <summary>Genre label + chip colours (the game's genre colours).</summary>
    public static (string Name, string Bg, string Fg) Genre(SongGenre g) => g switch
    {
        SongGenre.Pop => ("Pop", "#42c0d2", "#fff"),
        SongGenre.Anime => ("Anime", "#ff90d3", "#fff"),
        SongGenre.Kids => ("Kids", "#fec000", "#fff"),
        SongGenre.Vocaloid => ("Vocaloid", "#dddddd", "#18181b"),
        SongGenre.GameMusic => ("Game Music", "#cc8aea", "#fff"),
        SongGenre.NamcoOriginal => ("NAMCO Original", "#ff7027", "#fff"),
        SongGenre.Variety => ("Variety", "#1dc83b", "#fff"),
        _ => ("Classical", "#bfa356", "#000"),
    };
}
