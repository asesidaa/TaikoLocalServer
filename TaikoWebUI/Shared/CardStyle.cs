using System.Globalization;

namespace TaikoWebUI.Shared;

/// <summary>The player card's look: background colour + pattern, fade, text contrast and don-chan animation.</summary>
public static class CardStyle
{
    /// <summary>
    /// Patterns in picker order. The nine traditional ones are vector tiles in wwwroot/patterns (see TraditionalTiles).
    /// </summary>
    public static readonly (CardPattern Value, string Name)[] Patterns =
    [
        (CardPattern.Dots, "Dots"), (CardPattern.Stripes, "Stripes"), (CardPattern.Checker, "Checker"),
        (CardPattern.Seigaiha, "Seigaiha"), (CardPattern.Asanoha, "Asanoha"), (CardPattern.Shippo, "Shippō"),
        (CardPattern.Kikko, "Kikkō"), (CardPattern.Aranami, "Aranami"), (CardPattern.Kumo, "Kumo"),
        (CardPattern.Sakura, "Sakura"), (CardPattern.Hanabishi, "Hanabishi"), (CardPattern.Yosegi, "Yosegi"),
        (CardPattern.Rings, "Rings"), (CardPattern.Notes, "Notes"), (CardPattern.Diamonds, "Diamonds"),
        (CardPattern.Zigzag, "Zigzag"), (CardPattern.Grid, "Grid"), (CardPattern.Sunburst, "Sunburst"), (CardPattern.None, "Plain"),
    ];

    /// <summary>Animation presets in picker order; the clip each loops is <see cref="Clip"/>.</summary>
    public static readonly (CardAnimation Value, string Name)[] Animations =
    [
        (CardAnimation.Celebrate, "Celebrate"), (CardAnimation.Clear, "Clear"), (CardAnimation.Aggressive, "Aggressive"),
        (CardAnimation.Miss, "Miss"), (CardAnimation.FullCombo, "Full Combo"), (CardAnimation.Pout, "Pout"),
    ];

    /// <summary>The clip each card animation loops.</summary>
    public static string Clip(CardAnimation a) => a switch
    {
        CardAnimation.Celebrate => "don_result_full_loop",
        CardAnimation.Aggressive => "don_wait_loop",
        CardAnimation.Miss => "don_miss",
        CardAnimation.FullCombo => "don_full_combo",
        CardAnimation.Pout => "don_result_failure_loop",
        _ => "don_result_clear_loop",
    };

    /// <summary>Relative luminance above 0.45 means dark text and dark pattern ink, so light cards stay readable.</summary>
    public static bool IsLight(string hex)
    {
        double Channel(int i)
        {
            var v = int.Parse(hex.AsSpan(i, 2), NumberStyles.HexNumber) / 255.0;
            return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
        }
        return 0.2126 * Channel(1) + 0.7152 * Channel(3) + 0.0722 * Channel(5) > 0.45;
    }

    /// <summary>The card's inline <c>background</c>: the pattern in a light or dark ink over the palette colour.</summary>
    public static string Background(uint color, CardPattern pattern)
    {
        var hex = GameData.Color(color);
        var light = IsLight(hex);
        var ink = light ? "rgba(0,0,0,.08)" : "rgba(255,255,255,.14)";
        var layer = pattern switch
        {
            CardPattern.Dots => $"radial-gradient(circle at 14px 14px, {ink} 2.5px, transparent 3px) 0 0/28px 28px",
            CardPattern.Stripes => $"repeating-linear-gradient(135deg, {ink} 0 12px, transparent 12px 24px)",
            CardPattern.Checker => $"conic-gradient({ink} 25%, transparent 0 50%, {ink} 0 75%, transparent 0) 0 0/32px 32px",
            CardPattern.Rings => $"radial-gradient(circle, transparent 7px, {ink} 8px 10px, transparent 11px) 0 0/32px 32px",
            CardPattern.Notes => $"radial-gradient(circle at 25% 30%, {ink} 7px, transparent 7.5px) 0 0/48px 48px, radial-gradient(circle at 72% 75%, {ink} 4.5px, transparent 5px) 0 0/48px 48px",
            CardPattern.Diamonds => $"conic-gradient(from 45deg, {ink} 25%, transparent 0 50%, {ink} 0 75%, transparent 0) 0 0/32px 32px",
            CardPattern.Zigzag => Zigzag(light),
            _ when TraditionalTiles.TryGetValue(pattern, out var t) =>
                F($"url(patterns/{t.File}-{(light ? "dark" : "light")}.svg) 0 0/{t.W}px {t.H}px"),
            CardPattern.Grid => $"linear-gradient({ink} 2px, transparent 2px) 0 0/24px 24px, linear-gradient(90deg, {ink} 2px, transparent 2px) 0 0/24px 24px",
            CardPattern.Sunburst => $"repeating-conic-gradient(from 0deg at 50% 110%, {ink} 0deg 6deg, transparent 6deg 12deg)",
            _ => null,
        };
        return layer is null ? $"background:{hex}" : $"background:{layer}, {hex}";
    }

    // The traditional patterns: one repeat tile each from the supplied Japanese pattern vector set, as static SVGs in
    // wwwroot/patterns (<file>-dark.svg = black ink for light cards, -light.svg = white ink for dark cards; ink only,
    // as the set's base shapes are the card colour). Sizes are the tile in source units × 0.55, in CSS px.
    private static readonly Dictionary<CardPattern, (string File, double W, double H)> TraditionalTiles = new()
    {
        [CardPattern.Seigaiha] = ("seigaiha", 72.606, 124.025), [CardPattern.Asanoha] = ("asanoha", 83.353, 48.12),
        [CardPattern.Shippo] = ("shippo", 78.826, 78.832), [CardPattern.Kikko] = ("kikko", 76.593, 108.075),
        [CardPattern.Aranami] = ("aranami", 37.532, 62.7), [CardPattern.Kumo] = ("kumo", 122.826, 39.842),
        [CardPattern.Sakura] = ("sakura", 38.225, 154.743), [CardPattern.Hanabishi] = ("hanabishi", 66.842, 85.052),
        [CardPattern.Yosegi] = ("yosegi", 84.309, 146.025),
    };

    // ---- SVG tiles drawn here. Ink is solid inside a group with opacity, so crossing strokes don't double up.

    private static string Ink(bool light) => light ? "#000" : "#fff";
    private static string Opacity(bool light) => light ? ".1" : ".18";

    private static string Tile(double w, double h, string body) =>
        $"url('data:image/svg+xml,{Uri.EscapeDataString(F($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{w}\" height=\"{h}\" viewBox=\"0 0 {w} {h}\">{body}</svg>"))}') 0 0/{F(w)}px {F(h)}px";

    private static string Lines(bool light, double width, string inner) =>
        $"<g opacity=\"{Opacity(light)}\" fill=\"none\" stroke=\"{Ink(light)}\" stroke-width=\"{F(width)}\" stroke-linecap=\"round\" stroke-linejoin=\"round\">{inner}</g>";

    // Invariant numbers with at most two decimals (also used on whole interpolated strings).
    private static string F(double v) => Math.Round(v, 2).ToString(CultureInfo.InvariantCulture);
    private static string F(FormattableString s) => s.ToString(CultureInfo.InvariantCulture);

    private static string Zigzag(bool light) =>
        Tile(32, 20, Lines(light, 2.5, "<path d=\"M-8 14 0 6 8 14 16 6 24 14 32 6 40 14\"/>"));
}
