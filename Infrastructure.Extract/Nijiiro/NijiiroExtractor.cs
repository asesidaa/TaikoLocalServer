using System.Text.Json;
using System.Text.Json.Nodes;
using TaikoLocalServer.Infrastructure.Extract.Textures;

namespace TaikoLocalServer.Infrastructure.Extract.Nijiiro;

/// <summary><c>extract --game &lt;Nijiiro install&gt; [--mods]</c>: the files the server and WebUI need from Nijiiro.</summary>
public static class NijiiroExtractor
{
    /// <summary>Datatables the server decrypts on startup (see <c>FileGameDataCatalog</c>). music_attribute is optional
    /// there (the Song List's "new" flag).</summary>
    private static readonly string[] Datatables =
        ["musicinfo.bin", "music_order.bin", "music_attribute.bin", "wordlist.bin", "don_cos_reward.bin", "shougou.bin", "neiro.bin"];
    /// <summary>Flat texture folders under <c>model</c>: face paint (the <c>face</c> costume slot), puchi accessories, and the
    /// per-kigurumi expression atlases.</summary>
    private static readonly string[] TextureDirs = ["paint", "acce", "face"];
    /// <summary>Model folders under <c>model</c>: bodies, heads, and kigurumi (full costumes).</summary>
    private static readonly string[] ModelDirs = ["body", "head", "cos"];
    /// <summary>The nameplate's face, one sheet per <c>MyDonNameLanguage</c> (Japanese, English, Traditional Chinese, Korean;
    /// no Simplified sheet). name_plate's text fields use Lumen font slot 1: the <c>*_30</c> sheets.</summary>
    private static readonly string[] FontSheets = ["jp_30", "en_30", "tw_30", "kr_30"];
    /// <summary>The tone ids the game ships (<c>GameData.Tones</c> in the WebUI).</summary>
    private const int Tones = 20;
    /// <summary>Records which layers the outputs came from, so switching <c>--mods</c> on or off rebuilds them.</summary>
    private const string Stamp = "extract.json";

    private static IEnumerable<string> ToneFiles() => Enumerable.Range(0, Tones).Select(id => $"sound/se_neiro_{id:D2}_v12a.nus3bank");

    /// <summary><c>--inputs</c>: paths relative to the game folder. <c>files</c> are exact files or, ending in <c>/</c>, whole
    /// folders; in <c>names</c> folders only the file names are read. <c>mods</c> is the same for <c>Data_mods</c>.</summary>
    public static JsonObject Inputs()
    {
        var rel = Datatables.Select(n => $"datatable/{n}").ToList();
        rel.AddRange(TextureDirs.Concat(ModelDirs).Select(d => $"model/{d}/"));
        rel.Add("animation/");
        rel.AddRange(FontSheets.SelectMany(s => new[] { $"font/{s}.xml", $"font/{s}.nutexb" }));
        rel.AddRange(NijiiroUi.LumenFiles.SelectMany(f => new[] { $"lumen/000_default/{f}.nulm", $"lumen/000_default/{f}.nutexb" }));
        rel.AddRange(ToneFiles());
        // Data_mods keeps its datatables encrypted in x64_enc (see NijiiroGame).
        var modFiles = rel.Where(r => !r.StartsWith("datatable/", StringComparison.Ordinal)).Select(r => $"Data_mods/x64/{r}")
            .Concat(Datatables.Select(n => $"Data_mods/x64_enc/datatable/{n}"));
        static JsonArray Strings(IEnumerable<string> v) => new(v.Select(s => (JsonNode)JsonValue.Create(s)).ToArray());
        return new JsonObject
        {
            ["files"] = Strings(rel.Select(r => $"Data/x64/{r}")),
            ["names"] = Strings(["Data/x64/movie/"]),
            ["mods"] = new JsonObject { ["files"] = Strings(modFiles), ["names"] = Strings(["Data_mods/x64/movie/"]) },
        };
    }

    public static List<Report> Run(string gameDir, string outDir, bool mods, TextWriter log)
    {
        var x64 = new[] { Path.Combine(gameDir, "Data", "x64"), gameDir }.FirstOrDefault(c => Directory.Exists(Path.Combine(c, "datatable")))
            ?? throw new IOException($"{gameDir} doesn't look like a Nijiiro install: no Data/x64/datatable folder found");
        var game = new NijiiroGame(x64, mods);
        log.WriteLine($"game:   {game.Base}{(game.Modded ? " + Data_mods" : "")}");
        log.WriteLine($"output: {outDir}");
        var gameData = Path.Combine(outDir, Ac15.Ac15Extractor.GameData);
        // The folder used to be called don-model: keep what an older extract wrote.
        var old = Path.Combine(outDir, "don-model");
        if (Directory.Exists(old) && !Directory.Exists(gameData)) Directory.Move(old, gameData);
        ResetIfLayersChanged(game, outDir, gameData, log);

        var reports = new List<Report> { CopyDatatables(game, Path.Combine(outDir, "data", "datatable")) };
        foreach (var dir in TextureDirs)
        {
            var report = new Report($"{dir} textures");
            ConvertTextures(game, $"model/{dir}", Path.Combine(gameData, dir), report);
            reports.Add(report);
        }
        foreach (var dir in ModelDirs)
        {
            var report = new Report($"{dir} models");
            NijiiroModels.ConvertCategory(game, $"model/{dir}", Path.Combine(gameData, dir), report);
            reports.Add(report);
        }
        Report Step(string label, Action<Report> step)
        {
            var report = new Report(label);
            step(report);
            reports.Add(report);
            return report;
        }
        Step("animations", r => NijiiroAnims.Convert(game, Path.Combine(gameData, "animation"), r));
        Step("nameplates", r => NijiiroUi.Nameplates(game, Path.Combine(gameData, "nameplate"), r));
        Step("scoreboard", r => NijiiroUi.Scoreboard(game, Path.Combine(gameData, "scoreboard"), r));
        Step("icons", r =>
        {
            NijiiroUi.IconSets(game, Path.Combine(gameData, "ui"), r);
            NijiiroUi.NoteIconImage(game, Path.Combine(gameData, "ui"), r);
        });
        Step("font pages", r => Fonts(game, Path.Combine(gameData, "font"), r));
        Step("movies", r => Movies(game, gameData, r));
        Step("tones", r => ConvertTones(game, Path.Combine(gameData, "tone"), r));
        File.WriteAllText(Path.Combine(gameData, Stamp), new JsonObject { ["mods"] = game.Modded }.ToJsonString());
        Manifest.Write(Path.Combine(outDir, "data", Manifest.FileName), "nijiiro", gameDir, reports, []);
        return reports;
    }

    /// <summary>Outputs are skipped when newer than their source, which can't tell a mod file from the original it replaced.
    /// So when <c>--mods</c> differs from the last run, everything extract wrote is rebuilt once.</summary>
    private static void ResetIfLayersChanged(NijiiroGame game, string outDir, string gameData, TextWriter log)
    {
        var stamp = Path.Combine(gameData, Stamp);
        bool? last = null;
        try
        {
            last = File.Exists(stamp) ? JsonNode.Parse(File.ReadAllText(stamp))?["mods"]?.GetValue<bool>() : null;
        }
        catch (Exception e) when (e is JsonException or InvalidOperationException) { }
        // No stamp: an older extract (base files only), or a first run.
        if ((last ?? false) == game.Modded) return;
        log.WriteLine($"{(game.Modded ? "Using" : "No longer using")} Data_mods since the last run: rebuilding everything.");
        if (Directory.Exists(gameData)) Directory.Delete(gameData, true);
        foreach (var path in Datatables.Select(name => Path.Combine(outDir, "data", "datatable", name)).Where(File.Exists))
            File.Delete(path);
    }

    private static Report CopyDatatables(NijiiroGame game, string dstDir)
    {
        Directory.CreateDirectory(dstDir);
        var report = new Report("datatables");
        foreach (var name in Datatables)
        {
            var (src, dst) = (game.Datatable(name), Path.Combine(dstDir, name));
            if (!File.Exists(src))
                report.Missing.Add(name);
            else if (NijiiroGame.IsNewer(dst, src) && new FileInfo(dst).Length == new FileInfo(src).Length)
                report.UpToDate.Add(name);
            else
            {
                File.Copy(src, dst, true);
                report.Written.Add(name);
            }
        }
        return report;
    }

    /// <summary>Every <c>*.nutexb</c> in the game folder <paramref name="rel"/> to <c>&lt;dstDir&gt;/&lt;name&gt;.png</c>.</summary>
    private static void ConvertTextures(NijiiroGame game, string rel, string dstDir, Report report)
    {
        if (game.List(rel) is not { } entries)
        {
            report.Missing.Add(game.File(rel));
            return;
        }
        Directory.CreateDirectory(dstDir);
        foreach (var src in entries.Where(p => p.EndsWith(".nutexb", StringComparison.Ordinal) && File.Exists(p)))
        {
            var name = Path.GetFileNameWithoutExtension(src);
            var dst = Path.Combine(dstDir, $"{name}.png");
            if (NijiiroGame.IsNewer(dst, src))
            {
                report.UpToDate.Add(name);
                continue;
            }
            // One bad texture shouldn't stop the rest.
            try
            {
                Nutexb.Decode(File.ReadAllBytes(src)).SavePng(dst);
                report.Written.Add(name);
            }
            catch (InvalidDataException e)
            {
                report.Failed.Add($"{name}: {e.Message}");
            }
        }
    }

    /// <summary>The glyph sheets (<c>font/&lt;sheet&gt;.{nutexb,xml}</c>), split into 256-code-point pages so the WebUI's
    /// nameplate and scoreboard previews draw names in the game's lettering without loading a 4096² sheet:
    /// <c>&lt;dst&gt;/&lt;sheet&gt;/&lt;page&gt;.png</c> (glyphs row-packed) and <c>&lt;page&gt;.json</c>
    /// <c>{ "point": 32, "glyphs": { "&lt;cp&gt;": [x, y, w, h] } }</c>.</summary>
    private static void Fonts(NijiiroGame game, string dstDir, Report report)
    {
        foreach (var sheet in FontSheets)
        {
            var (xml, tex) = (game.File($"font/{sheet}.xml"), game.File($"font/{sheet}.nutexb"));
            if (new[] { xml, tex }.FirstOrDefault(p => !File.Exists(p)) is { } missing)
            {
                report.Missing.Add(missing);
                continue;
            }
            // Page 0 (ASCII) is written last, so its being up to date means the whole sheet is.
            var marker = Path.Combine(dstDir, sheet, "0.json");
            if (NijiiroGame.IsNewer(marker, xml) && NijiiroGame.IsNewer(marker, tex))
            {
                report.UpToDate.Add(sheet);
                continue;
            }
            try
            {
                var pages = FontPages.Split(File.ReadAllText(xml), Nutexb.Decode(File.ReadAllBytes(tex)), Path.Combine(dstDir, sheet));
                report.Written.Add($"{sheet} ({pages} pages)");
            }
            catch (InvalidDataException e)
            {
                report.Failed.Add($"{sheet}: {e.Message}");
            }
        }
    }

    /// <summary>The attract movies a cabinet can play: <c>movie/&lt;8-digit id&gt;.wmv</c>, listed as <c>movies.json</c>
    /// (<c>{ "movies": [1, 2, …] }</c>) so the WebUI offers them instead of asking for numbers. The attract demo videos
    /// (<c>attractdemo_&lt;lang&gt;.wmv</c>) aren't numbered and aren't listed.</summary>
    private static void Movies(NijiiroGame game, string dstDir, Report report)
    {
        const string index = "movies.json";
        if (game.List("movie") is not { } files)
        {
            report.Missing.Add(game.File("movie"));
            return;
        }
        var body = new JsonObject { ["movies"] = new JsonArray(MovieIds(files.Select(Path.GetFileName)!).Select(id => (JsonNode)id).ToArray()) }
            .ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        var output = Path.Combine(dstDir, index);
        if (File.Exists(output) && File.ReadAllText(output) == body)
        {
            report.UpToDate.Add(index);
            return;
        }
        Directory.CreateDirectory(dstDir);
        File.WriteAllText(output, body);
        report.Written.Add(index);
    }

    /// <summary>Ids from file names such as <c>00000012.wmv</c>, ascending.</summary>
    internal static List<uint> MovieIds(IEnumerable<string> names) =>
        names.Select(n => n.EndsWith(".wmv", StringComparison.Ordinal) || n.EndsWith(".WMV", StringComparison.Ordinal) ? n[..^4] : null)
            .Where(stem => !string.IsNullOrEmpty(stem) && stem.All(char.IsAsciiDigit))
            .Select(stem => uint.TryParse(stem, out var id) ? (uint?)id : null).OfType<uint>()
            .Distinct().Order().ToList();

    /// <summary>Drum tones: <c>sound/se_neiro_NN_v12a.nus3bank</c> holds the tone's don and ka hits as two plain PCM WAVs, in
    /// that order. They're cut out as they are, since browsers play PCM WAV.</summary>
    private static void ConvertTones(NijiiroGame game, string dstDir, Report report)
    {
        Directory.CreateDirectory(dstDir);
        foreach (var (rel, id) in ToneFiles().Select((r, i) => (r, i)))
        {
            var name = $"{id:D2}";
            var src = game.File(rel);
            if (!File.Exists(src))
            {
                report.Missing.Add(src);
                continue;
            }
            var ka = Path.Combine(dstDir, $"{name}_ka.wav");
            if (NijiiroGame.IsNewer(ka, src))
            {
                report.UpToDate.Add(name);
                continue;
            }
            var wavs = Wavs(File.ReadAllBytes(src));
            if (wavs.Count != 2)
            {
                report.Failed.Add($"tone {name}: expected 2 WAVs, found {wavs.Count}");
                continue;
            }
            File.WriteAllBytes(Path.Combine(dstDir, $"{name}_don.wav"), wavs[0]);
            File.WriteAllBytes(ka, wavs[1]); // written last: its timestamp marks the pair as done
            report.Written.Add(name);
        }
    }

    /// <summary>Every embedded <c>RIFF....WAVE</c> chunk, whole, in file order.</summary>
    internal static List<byte[]> Wavs(byte[] bank)
    {
        var found = new List<byte[]>();
        var i = 0;
        while (bank.AsSpan(i).IndexOf("RIFF"u8) is var rel && rel >= 0)
        {
            var at = i + rel;
            var end = at + 8 < bank.Length ? at + 8 + (long)BitConverter.ToUInt32(bank, at + 4) : long.MaxValue;
            if (at + 12 <= bank.Length && end <= bank.Length && bank.AsSpan(at + 8, 4).SequenceEqual("WAVE"u8))
            {
                found.Add(bank[at..(int)end]);
                i = (int)end;
            }
            else
            {
                i = at + 4;
            }
        }
        return found;
    }
}
