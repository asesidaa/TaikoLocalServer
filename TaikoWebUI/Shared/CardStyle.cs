using System.Globalization;

namespace TaikoWebUI.Shared;

/// <summary>The player card's look: background colour + pattern, fade, text contrast and don-chan animation.</summary>
public static class CardStyle
{
    /// <summary>
    /// Patterns offered in the picker. The nine traditional ones (Seigaiha … Yosegi) are in the enum but wait on the
    /// licence of their source vectors (see docs/webui-design-decisions.md), so they aren't offered yet.
    /// </summary>
    public static readonly (CardPattern Value, string Name)[] Patterns =
    [
        (CardPattern.Dots, "Dots"), (CardPattern.Stripes, "Stripes"), (CardPattern.Checker, "Checker"),
        (CardPattern.Rings, "Rings"), (CardPattern.Notes, "Notes"), (CardPattern.Diamonds, "Diamonds"),
        (CardPattern.Zigzag, "Zigzag"), (CardPattern.Grid, "Grid"), (CardPattern.Sunburst, "Sunburst"), (CardPattern.None, "Plain"),
    ];

    /// <summary>Animation presets and their looping clips (only clean *_loop clips; one-shots look chopped when looped).</summary>
    public static readonly (CardAnimation Value, string Name, string Clip)[] Animations =
    [
        (CardAnimation.Celebrate, "Celebrate", "don_result_full_loop"), (CardAnimation.Clear, "Clear", "don_result_clear_loop"),
        (CardAnimation.Donderful, "Donderful", "don_result_percious_loop"), (CardAnimation.Drumming, "Drumming", "don_norm_loop"),
        (CardAnimation.Ready, "Ready", "don_entry1P_select_loop"), (CardAnimation.Pout, "Pout", "don_result_failure_loop"),
    ];

    public static string Clip(CardAnimation a) => Animations.FirstOrDefault(x => x.Value == a).Clip ?? "don_result_clear_loop";

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
            CardPattern.Grid => $"linear-gradient({ink} 2px, transparent 2px) 0 0/24px 24px, linear-gradient(90deg, {ink} 2px, transparent 2px) 0 0/24px 24px",
            CardPattern.Sunburst => $"repeating-conic-gradient(from 0deg at 50% 110%, {ink} 0deg 6deg, transparent 6deg 12deg)",
            _ => null,
        };
        return layer is null ? $"background:{hex}" : $"background:{layer}, {hex}";
    }

    // A line pattern as an SVG tile: solid ink in a group with opacity, so crossing strokes don't double up.
    private static string Zigzag(bool light)
    {
        var (stroke, opacity) = light ? ("#000", ".1") : ("#fff", ".18");
        var svg = $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"32\" height=\"20\" viewBox=\"0 0 32 20\"><g opacity=\"{opacity}\" fill=\"none\" stroke=\"{stroke}\" stroke-width=\"2.5\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M-8 14 0 6 8 14 16 6 24 14 32 6 40 14\"/></g></svg>";
        return $"url('data:image/svg+xml,{Uri.EscapeDataString(svg)}') 0 0/32px 20px";
    }
}
