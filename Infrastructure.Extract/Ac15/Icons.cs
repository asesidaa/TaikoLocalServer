using TaikoLocalServer.Infrastructure.Extract.Textures;

namespace TaikoLocalServer.Infrastructure.Extract.Ac15;

/// <summary>
/// The costume and drum-sound icons in <c>nutdata</c>, to <c>game_data/&lt;era&gt;/&lt;folder&gt;/&lt;name&gt;.png</c>. The name
/// images (<c>title_name</c>, <c>tone_name</c>, <c>cos_name</c>) aren't converted: the server has the names as text in
/// <c>data/shared</c>.
/// </summary>
public static class Icons
{
    private static readonly string[] Kinds = ["cos_icon", "tone_icon"];

    /// <summary><c>nutdata/&lt;kind&gt;/</c>, <c>nutdata/pack/&lt;…&gt;/&lt;kind&gt;/</c> and <c>nutdata/&lt;id&gt;/appendable/&lt;nn&gt;/&lt;kind&gt;/</c>.</summary>
    public static IEnumerable<Rule> Inputs() => ServerFiles.NameDirs.SelectMany(d => Kinds.Select(k => Rule.Files(d + k + "/")));

    /// <summary>Item <c>n</c> is <c>&lt;prefix&gt;&lt;n:D3&gt;</c>, what one-texture pack entries are called, numbered from
    /// <c>first</c>. tone_icon.nut's textures are icon_neiro_001… (checked pixel for pixel against the packs).</summary>
    private static (string Prefix, int First) ItemName(string kind) => kind == "cos_icon" ? ("cos_icon_", 0) : ("icon_neiro_", 1);

    public static List<Report> Convert(Era era, GameSource src, string outDir)
    {
        var rules = Inputs().ToList();
        // A game's own folders are applied last, over the packs it inherited. Other games' appendable folders are skipped:
        // item numbers differ between games.
        bool Own(string p) => era.Config() is { } id
            && (p.StartsWith($"nutdata/{id}/", StringComparison.Ordinal) || p.StartsWith($"nutdata/pack/{id}/", StringComparison.Ordinal));
        bool Other(string p) => !Own(p) && p.Split('/') is { Length: > 2 } parts && parts[2] == "appendable";
        var reports = new List<Report>();
        foreach (var kind in Kinds)
        {
            var report = new Report($"{era.Name()} {kind}");
            var files = src.Files(p => p.Split('/') is { Length: > 1 } parts && parts[^2] == kind && rules.Any(r => r.Matches(p)) && !Other(p))
                .OrderBy(Own).ThenBy(p => p, StringComparer.Ordinal).ToList();
            var (prefix, first) = ItemName(kind);
            var images = new SortedDictionary<string, (string Rel, RgbaImage Image)>(StringComparer.Ordinal);
            foreach (var rel in files)
            {
                try
                {
                    var bytes = src.Read(rel);
                    var blobs = rel.EndsWith(".ndp", StringComparison.Ordinal) ? NutPack.Read(bytes)
                        : rel.EndsWith(".nut", StringComparison.Ordinal) ? [(rel, bytes)] : [];
                    foreach (var (name, nut) in blobs)
                    {
                        var textures = Ntp3.Decode(nut);
                        if (textures.Count == 1 && Stem(name).StartsWith(prefix, StringComparison.Ordinal))
                        {
                            images[Stem(name)] = (rel, textures[0].Image);
                            continue;
                        }
                        // One texture per item, in order (tone_icon.nut).
                        for (var k = 0; k < textures.Count; k++)
                            images[$"{prefix}{first + k:D3}"] = (rel, textures[k].Image);
                    }
                }
                catch (InvalidDataException e)
                {
                    report.Failed.Add($"{rel}: {e.Message}");
                }
            }
            if (images.Count == 0 && report.Failed.Count == 0) continue;
            var dir = Path.Combine(outDir, kind);
            Directory.CreateDirectory(dir);
            foreach (var (name, (rel, image)) in images)
            {
                var dst = Path.Combine(dir, name + ".png");
                if (src.IsFresh(dst, rel))
                {
                    report.UpToDate.Add(name);
                    continue;
                }
                image.SavePng(dst);
                report.Written.Add(name);
            }
            reports.Add(report);
        }
        return reports;
    }

    private static string Stem(string name)
    {
        var file = name[(name.LastIndexOf('/') + 1)..];
        return file.EndsWith(".nut", StringComparison.Ordinal) || file.EndsWith(".ndp", StringComparison.Ordinal) ? file[..^4] : file;
    }
}
