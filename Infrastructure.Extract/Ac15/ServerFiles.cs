using System.Buffers.Binary;
using TaikoLocalServer.Infrastructure.Extract.Textures;

namespace TaikoLocalServer.Infrastructure.Extract.Ac15;

/// <summary>
/// Copies the game files the server reads into <c>&lt;out&gt;/data/&lt;era&gt;/data</c> (<c>PathHelper.GetDataPath(era)/data</c>),
/// keeping their paths. Files the server only reads names from become empty files.
/// </summary>
public static class ServerFiles
{
    /// <summary>The folders the server scans for costume, title and tone ids by file name (<c>Ac15Don3dDirScanner</c>,
    /// <c>Ac15CustomizationSourceParser.ScanNameIds</c>): <c>nutdata/**/title_name</c> sits 0, 2 or 3 folders down.</summary>
    private static readonly string[] Don3dDirs = ["cos", "face", "parts/acc", "parts/body", "parts/head", "parts/paint", "full/cos", "full/face"];
    internal static readonly string[] NameDirs = ["nutdata/", "nutdata/*/*/", "nutdata/*/*/*/"];

    /// <summary>What the server reads, with the config folder this game actually has (see <see cref="ConfigDir"/>).</summary>
    internal static List<(Rule Rule, bool Required)> Rules(Era era, string config)
    {
        var rules = era.ConfigFiles().Select(f => (Rule.Files(config + f.Name), f.Required)).ToList();
        rules.Add((Rule.Files("fumen/tuning.bin"), true));
        if (era == Era.Blue)
            rules.Add((Rule.Files(config + "battle/"), false));
        if (era == Era.Green)
            rules.AddRange(new[] { "cos_name", "title_name", "tone_name" }.Select(d => (Rule.Files($"nutdata/{d}/nutdatapack.ndp"), true)));
        rules.Add((Rule.Names("movie/"), false));
        rules.AddRange(Don3dDirs.Select(d => (Rule.Names($"don3d/{d}/"), false)));
        rules.AddRange(NameDirs.SelectMany(d => new[] { "title_name", "tone_name" }.Select(n => (Rule.Names($"{d}{n}/"), false))));
        return rules;
    }

    /// <summary><c>config/&lt;id&gt;/</c>, or <c>""</c> for Kimidori and Momoiro. When the game has only another variant of the id
    /// (Red's 10.01 dump has <c>ST8100-7</c>, not <c>ST8100-1</c>), that one is used and copied under the id the server reads.</summary>
    internal static (string Have, string Want) ConfigDir(Era era, GameSource game)
    {
        if (era.Config() is not { } id) return ("", "");
        var want = $"config/{id}/";
        if (game.Exists(want + "musicinfo.xml")) return (want, want);
        var stem = id[..^1];
        var found = game.Files(p => p.StartsWith($"config/{stem}", StringComparison.Ordinal) && p.EndsWith("/musicinfo.xml", StringComparison.Ordinal))
            .Select(p => p[..^"musicinfo.xml".Length]).FirstOrDefault();
        return (found ?? want, want);
    }

    /// <summary>A title or drum-sound name pack without ids in its file name (<c>pack/ST4100-1/00/title_name/nutdatapack.ndp</c>,
    /// unlike <c>nutdatapack_00000_00099.ndp</c>). The server reads ids from file names, so its entries are written beside it.</summary>
    internal static bool UnnumberedPack(string rel)
    {
        var parts = rel.Split('/');
        var (file, dir) = (parts[^1], parts.Length > 1 ? parts[^2] : "");
        return file.EndsWith(".ndp", StringComparison.Ordinal) && !file.Any(char.IsAsciiDigit) && dir is "title_name" or "tone_name";
    }

    /// <summary>The pack's entry names (<c>shougou_012.nut</c>). An entry without a number holds one texture per item, from 0
    /// (Murasaki's drum-sound names are one <c>image.nut</c> of 16), and gives <c>&lt;prefix&gt;_NNN.nut</c> for each.</summary>
    internal static List<string> EntryNames(string rel, byte[] pack)
    {
        var prefix = rel.Contains("/title_name/", StringComparison.Ordinal) ? "shougou_" : "tone_name_";
        var names = new List<string>();
        foreach (var (name, nut) in NutPack.Read(pack))
        {
            if (name.Any(char.IsAsciiDigit))
            {
                names.Add(name);
                continue;
            }
            if (nut.Length < 8) throw new InvalidDataException("truncated NTP3");
            var count = BinaryPrimitives.ReadUInt16BigEndian(nut.AsSpan(6));
            names.AddRange(Enumerable.Range(0, count).Select(k => $"{prefix}{k:D3}.nut"));
        }
        return names;
    }

    public static Report Copy(Era era, GameSource src, string dst, TextWriter log)
    {
        var report = new Report($"{era.Name()} server files");
        var (have, want) = ConfigDir(era, src);
        if (have != want)
            log.WriteLine($"{era.Name()}: no {want}, using {have} (copied as {want})");
        // Green's name packs match a content rule and a names rule: the first one wins, as in ExtractRunner.Match.
        var done = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (rule, required) in Rules(era, have))
        {
            // A config/ match is written under the id the server reads.
            string Target(string p) => have != want && p.StartsWith(have, StringComparison.Ordinal) ? want + p[have.Length..] : p;
            var any = false;
            foreach (var rel in src.Files(rule.Matches).ToList())
            {
                any = true;
                if (!done.Add(rel)) continue;
                var to = Path.Combine(dst, Target(rel));
                if (UnnumberedPack(rel))
                {
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(to)!);
                        foreach (var name in EntryNames(rel, src.Read(rel)))
                        {
                            var entry = Path.Combine(Path.GetDirectoryName(to)!, name);
                            if (!File.Exists(entry)) File.WriteAllBytes(entry, []);
                        }
                    }
                    catch (InvalidDataException e)
                    {
                        report.Failed.Add($"{rel}: {e.Message}");
                    }
                }
                if (rule.NamesOnly)
                {
                    // Only the name is read; never replace a real file someone copied in by hand.
                    if (File.Exists(to))
                    {
                        report.UpToDate.Add(rel);
                        continue;
                    }
                    Directory.CreateDirectory(Path.GetDirectoryName(to)!);
                    File.WriteAllBytes(to, []);
                    report.Written.Add(rel);
                    continue;
                }
                if (File.Exists(to) && new FileInfo(to).Length == src.Size(rel) && src.IsFresh(to, rel))
                {
                    report.UpToDate.Add(rel);
                    continue;
                }
                Directory.CreateDirectory(Path.GetDirectoryName(to)!);
                File.WriteAllBytes(to, src.Read(rel));
                report.Written.Add(rel);
            }
            if (!any && required)
                report.Missing.Add(rule.Path);
        }
        return report;
    }
}
