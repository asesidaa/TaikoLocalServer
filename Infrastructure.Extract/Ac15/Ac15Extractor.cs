using System.Text.Json;
using System.Text.Json.Nodes;

namespace TaikoLocalServer.Infrastructure.Extract.Ac15;

/// <summary><c>extract --era &lt;era&gt; --game &lt;data folder or zip&gt;</c>: the older games.</summary>
public static class Ac15Extractor
{
    /// <summary>The folder under <c>&lt;out&gt;</c> for converted assets; the server serves it through <c>api/Assets</c>.</summary>
    public const string GameData = "game_data";

    public static List<Report> Run(Era era, string game, string outDir, TextWriter log)
    {
        using var src = GameSource.Open(game);
        log.WriteLine($"game:   {src.Label} ({era.Name()})");
        log.WriteLine($"output: {outDir}");
        var dataDir = Path.Combine(outDir, "data", era.Name());
        var reports = new List<Report> { ServerFiles.Copy(era, src, Path.Combine(dataDir, "data"), log) };
        var assets = Path.Combine(outDir, GameData, era.Name());
        reports.AddRange(Icons.Convert(era, src, assets));
        reports.Add(UiSets.Convert(era, src, assets));
        Manifest.Write(Path.Combine(dataDir, Manifest.FileName), era.Name(), game, reports, Notes(era, src, assets, reports));
        return reports;
    }

    /// <summary><c>--inputs --era</c>: the same shape as Nijiiro's, relative to <c>USRDIR</c> (so paths start with
    /// <c>data/</c>). Every config variant is listed because the uploaded folder isn't known yet.</summary>
    public static JsonObject Inputs(Era era)
    {
        var config = era.Config() is null ? "" : "config/*/";
        var rules = ServerFiles.Rules(era, config).Select(r => r.Rule).ToList();
        // Unnumbered name packs are read for their entry names, so they're sent whole.
        rules.AddRange(ServerFiles.NameDirs.SelectMany(d => new[] { "title_name", "tone_name" }.Select(n => Rule.Files($"{d}{n}/nutdatapack.ndp"))));
        rules.AddRange(Icons.Inputs());
        rules.AddRange(UiSets.Inputs());
        JsonArray Pick(bool names) => new(rules.Where(r => r.NamesOnly == names).Select(r => (JsonNode)JsonValue.Create("data/" + r.Path)).ToArray());
        return new JsonObject { ["files"] = Pick(false), ["names"] = Pick(true), ["mods"] = null };
    }

    /// <summary>The manifest's notes: what the dump lacks (<c>gap</c>), what this game doesn't have (<c>absent</c>), and extras
    /// (<c>included</c>).</summary>
    private static List<ManifestNote> Notes(Era era, GameSource src, string assets, List<Report> reports)
    {
        var notes = new List<ManifestNote>();
        var (have, want) = ServerFiles.ConfigDir(era, src);
        if (have != want && src.Exists(have + "musicinfo.xml"))
            notes.Add(new("gap", "early-config", have["config/".Length..].TrimEnd('/')));
        if (era == Era.Blue && !src.Files(p => p.StartsWith(have + "battle/", StringComparison.Ordinal)).Any())
            notes.Add(new("gap", "no-battle"));
        // ui.json lists the sets that rendered (no ui.json: the dump has no Lumen packs, which the steps already show).
        var ui = Path.Combine(assets, "ui", "ui.json");
        if (File.Exists(ui) && JsonNode.Parse(File.ReadAllText(ui)) is JsonObject sets && !sets.ContainsKey("plate"))
            notes.Add(new("absent", "no-title-plates"));
        if (!reports.Any(r => r.Label.EndsWith(" cos_icon", StringComparison.Ordinal)))
            notes.Add(new("absent", "no-costume-icons"));
        if (era == Era.Green && new[] { "cos_name", "title_name", "tone_name" }.All(d => src.Exists($"nutdata/{d}/nutdatapack.ndp")))
            notes.Add(new("included", "names"));
        return notes;
    }

    internal static readonly JsonSerializerOptions Compact = new() { WriteIndented = false };
}
