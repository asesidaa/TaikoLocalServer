using System.Text.Json.Nodes;
using TaikoLocalServer.Infrastructure.Extract.Textures;

namespace TaikoLocalServer.Infrastructure.Extract.Nijiiro;

/// <summary>A glyph sheet split into 256-code-point pages (see <see cref="NijiiroExtractor"/>).</summary>
public static class FontPages
{
    private const int PageWidth = 1024;

    /// <summary>Writes the pages of one sheet into <paramref name="dst"/>; returns how many.</summary>
    public static int Split(string xml, RgbaImage sheet, string dst)
    {
        var point = Attr(xml, "fontPoint") is { } fp && uint.TryParse(fp, out var pt) ? pt : throw new InvalidDataException("no fontPoint");
        var pages = new SortedDictionary<uint, List<uint[]>>();
        foreach (var tag in xml.Split("<glyph ").Skip(1))
        {
            uint? N(string name) => Attr(tag, name) is { } v && uint.TryParse(v, out var n) ? n : null;
            if (N("index") is not { } cp || N("offsetU") is not { } u || N("offsetV") is not { } v || N("width") is not { } w || N("height") is not { } h) continue;
            if ((w, h) == (4, 1) && cp != 32) continue; // the sheet's "no glyph" placeholder
            if (!pages.TryGetValue(cp >> 8, out var list)) pages[cp >> 8] = list = [];
            list.Add([cp, u, v, w, h]);
        }
        Directory.CreateDirectory(dst);
        // Which pages exist, so the WebUI never requests a missing one while walking its fallback sheets.
        File.WriteAllText(Path.Combine(dst, "pages.json"), new JsonArray(pages.Keys.Select(k => (JsonNode)k).ToArray()).ToJsonString());
        foreach (var (page, glyphs) in pages.Reverse())
        {
            // Row-pack with a 1px gap so bilinear sampling never bleeds a neighbour in.
            int x = 0, y = 0, row = 0;
            var placed = new List<(uint Cp, int U, int V, int W, int H, int X, int Y)>(glyphs.Count);
            foreach (var g in glyphs)
            {
                var (w, h) = ((int)g[3], (int)g[4]);
                if (x + w > PageWidth) (x, y, row) = (0, y + row + 1, 0);
                placed.Add((g[0], (int)g[1], (int)g[2], w, h, x, y));
                x += w + 1;
                row = Math.Max(row, h);
            }
            var img = new RgbaImage(PageWidth, Math.Max(y + row, 1));
            var map = new JsonObject();
            foreach (var (cp, u, v, w, h, px, py) in placed)
            {
                if (u + w > sheet.Width || v + h > sheet.Height) continue;
                for (var dy = 0; dy < h; dy++)
                    sheet.Pixels.AsSpan(((v + dy) * sheet.Width + u) * 4, w * 4).CopyTo(img.Pixels.AsSpan(((py + dy) * PageWidth + px) * 4));
                map[cp.ToString()] = new JsonArray(px, py, w, h);
            }
            img.SavePng(Path.Combine(dst, $"{page}.png"));
            File.WriteAllText(Path.Combine(dst, $"{page}.json"), new JsonObject { ["point"] = point, ["glyphs"] = map }.ToJsonString());
        }
        return pages.Count;
    }

    /// <summary>The value of <c>name="…"</c> in an XML tag's text; whole attribute names only (<c>width</c> doesn't match
    /// <c>texWidth</c>).</summary>
    private static string? Attr(string tag, string name)
    {
        var key = $"{name}=\"";
        for (var at = tag.IndexOf(key, StringComparison.Ordinal); at >= 0; at = tag.IndexOf(key, at + 1, StringComparison.Ordinal))
        {
            if (at > 0 && !char.IsWhiteSpace(tag[at - 1])) continue;
            var start = at + key.Length;
            var end = tag.IndexOf('"', start);
            return end < 0 ? tag[start..] : tag[start..end];
        }
        return null;
    }
}
