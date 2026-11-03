using System.Text.Json;
using System.Text.Json.Nodes;
using TaikoLocalServer.Infrastructure.Extract.Lumen;
using TaikoLocalServer.Infrastructure.Extract.Textures;

namespace TaikoLocalServer.Infrastructure.Extract.Ac15;

/// <summary>
/// UI art rendered from the Lumen packs to <c>game_data/&lt;era&gt;/ui/&lt;prefix&gt;_&lt;label&gt;.png</c>, with one
/// <c>ui.json</c> listing each set's labels in order. Each set comes from the first (scene, <c>.lm</c>, selector) the game
/// has; sprites are found by labels or instance names, never ids, which change between builds.
/// </summary>
public static class UiSets
{
    private enum By { Label, Labels, Instance }

    private static readonly (string Prefix, (string Scene, string Lm, By By, string Key)[] Choices)[] Sets =
    [
        ("crown", [("song_select", "song_select/song_select.lm", By.Labels, "none,silver,gold")]),
        // A crown beside each difficulty's face: gold_easy … silver_extreme.
        ("crown_course", [("song_select", "song_select/song_select.lm", By.Label, "gold_mania")]),
        ("difficulty", [
            ("song_select", "song_select/song_select.lm", By.Instance, "courseicon"),
            ("enso_result", "result/result.lm", By.Labels, "easy,normal,hard,mania"),
        ]),
        ("dan", [("dani_result", "dani_result/dani_result.lm", By.Instance, "dani_title")]),
        // Title plates by rarity: normal, gold, platinum, rainbow.
        ("plate", [("enso_result", "result/result.lm", By.Instance, "titlepanel")]),
    ];

    private static string PackPath(string scene) => $"lumendata/packed/{scene}/packeddata.ddp";

    public static IEnumerable<Rule> Inputs() =>
        Sets.SelectMany(s => s.Choices.Select(c => c.Scene)).Distinct().Order(StringComparer.Ordinal).Select(s => Rule.Files(PackPath(s)));

    public static Report Convert(Era era, GameSource src, string outDir)
    {
        var report = new Report($"{era.Name()} ui");
        var dir = Path.Combine(outDir, "ui");
        var index = Path.Combine(dir, "ui.json");
        var packs = Inputs().Select(r => r.Path).Where(src.Exists).ToList();
        if (packs.Count == 0) return report;
        if (packs.All(p => src.IsFresh(index, p)))
        {
            // One per image, as a fresh run counts them: ui.json lists each set's labels.
            if (JsonNode.Parse(File.ReadAllText(index)) is JsonObject sets)
                foreach (var (prefix, labels) in sets)
                    report.UpToDate.AddRange(labels?.AsArray().Select(l => $"{prefix}_{l}") ?? []);
            return report;
        }
        Directory.CreateDirectory(dir);
        var wanted = Sets.SelectMany(s => s.Choices.Select(c => c.Lm)).ToHashSet();
        var lumens = new Dictionary<(string Pack, string Lm), (LumenFile Lm, List<byte[]> Nuts)>();
        foreach (var pack in packs)
        {
            try
            {
                foreach (var f in LmPack.Read(src.Read(pack)).Where(f => wanted.Contains(f.Name)))
                    lumens[(pack, f.Name)] = (LumenFile.Parse(f.Lm), f.Nuts);
            }
            catch (InvalidDataException e)
            {
                throw new InvalidDataException($"reading {pack}: {e.Message}", e);
            }
        }
        var json = new JsonObject();
        foreach (var (prefix, choices) in Sets)
        {
            var found = choices.Select(c =>
            {
                if (!lumens.TryGetValue((PackPath(c.Scene), c.Lm), out var entry)) return default;
                var lm = entry.Lm;
                var sprite = c.By switch
                {
                    By.Label => lm.SpriteWithLabel(c.Key),
                    By.Labels => lm.SpriteWithLabels(c.Key),
                    _ => lm.SpritePlacing(c.Key) is { } parent ? lm.Child(parent, c.Key) : null,
                };
                return sprite is { } s ? (lm, entry.Nuts, s) : default;
            }).FirstOrDefault(f => f.lm is not null);
            if (found.lm is not { } chosen) continue;
            var (nuts, spriteId) = (found.Nuts, found.s);
            var used = chosen.Atlases(spriteId);
            var layers = Enumerable.Range(0, nuts.Count).Select(k => used.Contains((uint)k) ? Ntp3.Decode(nuts[k])[0].Image : null).ToList();
            var union = chosen.UnionBounds(spriteId);
            var labels = chosen.Labels(spriteId);
            foreach (var label in labels)
            {
                try
                {
                    chosen.RenderLayers(spriteId, label, layers, union).SavePng(Path.Combine(dir, $"{prefix}_{label}.png"));
                    report.Written.Add($"{prefix}_{label}");
                }
                catch (InvalidDataException e)
                {
                    report.Failed.Add($"{prefix}_{label}: {e.Message}");
                }
            }
            json[prefix] = new JsonArray(labels.Select(l => (JsonNode)JsonValue.Create(l)).ToArray());
        }
        // Last, so its being newer than the packs means everything else is too.
        File.WriteAllText(index, json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        return report;
    }
}
