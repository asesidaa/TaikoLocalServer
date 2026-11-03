using System.Text.Json;
using System.Text.Json.Nodes;
using TaikoLocalServer.Infrastructure.Extract.Lumen;
using TaikoLocalServer.Infrastructure.Extract.Textures;

namespace TaikoLocalServer.Infrastructure.Extract.Nijiiro;

/// <summary>
/// Game UI art rendered from the game's Lumen files instead of committed images: title plates and dan badges
/// (<c>common/name_plate</c>), the scoreboard panel (<c>common/player_board</c>) and the icons the WebUI shows (song options,
/// difficulties, crowns, Dani Dojo pass stamps). Sprites are found by frame labels or instance names, never by id, since
/// ids change between builds.
/// </summary>
public static class NijiiroUi
{
    public const string NameplateIndex = "plates.json", ScoreboardIndex = "scoreboard.json";
    /// <summary>The animated nameplate for the WebUI's Lumen player, with its atlas beside it as <c>atlas.png</c>.</summary>
    public const string NameplateScene = "nameplate.scene.json";
    /// <summary>The WebUI's logo and favicon (served by api/Brand/Icon).</summary>
    public const string NoteIcon = "don_note.png";
    /// <summary>The instance the game's whole nameplate places the title plate as.</summary>
    private const string PlateInstance = "title_instance";
    private const string NameplateLumen = "common/name_plate", ScoreboardLumen = "common/player_board";
    /// <summary>The notes on the lane; one sprite has a frame per note type, each with its caption (ドン) under the note.</summary>
    private const string NoteLumen = "enso_normal/enso/onpu/onp_all";

    /// <summary>Icon sets: (Lumen file under <c>lumen/000_default</c>, index name, [(index key, file prefix, a label the
    /// sprite has)]). Each set is the sprite carrying that label, one PNG per label. A comma-separated label names the
    /// sprite whose labels are exactly those, where one label alone would match a bigger animation.</summary>
    private static readonly (string File, string Index, (string Key, string Prefix, string Label)[] Sets)[] Icons =
    [
        // none, doron, abekobe (mirror), kimagure / detarame (random: whimsical / messy), note_pos, speedx1.1 … speedx4
        ("common/option", "options.json", [("options", "option", "doron")]),
        // easy, normal, hard, oni, ura
        ("common_songselect/common_song_select_song_board_icon", "difficulty.json", [("difficulty", "difficulty", "ura")]),
        // badge_crown_1…3 (clear, gold, donderful) and badge_rank_1…7 (the seven score ranks)
        ("common_songselect/common_song_select_main_diff_board", "badges.json", [("crowns", "crown", "badge_crown_1"), ("ranks", "rank", "badge_rank_1")]),
        // stamp_red_01…03, stamp_gold_01…03: a Dani Dojo pass (normal, full combo, perfect) in red or gold
        ("enso_dani/dani_select/dani_select", "dani.json", [("stamps", "dani", "stamp_gold_01")]),
        // AI Battle: win / lose (the result emblems), and each section's crown: none, crown_silver, crown_gold, crown_rainbow
        ("enso_ai/result/result_ai_section", "ai.json", [("results", "ai", "win,lose"), ("crowns", "ai", "crown_rainbow")]),
        // The results screen draws the crowns (silver = clear, gold = full combo, rainbow = donderful) and the seven score
        // ranks at 168 and 208 px, against 40 on the song board: the WebUI uses these where they show large.
        ("result/result", "result.json", [("crowns", "crown_large", "silver"), ("ranks", "rank_large", "rank_1,rank_2,rank_3,rank_4,rank_5,rank_6,rank_7")]),
    ];

    /// <summary>The Lumen files under <c>lumen/000_default</c> this step reads (<c>--inputs</c>).</summary>
    public static IEnumerable<string> LumenFiles => Icons.Select(i => i.File).Concat([NameplateLumen, ScoreboardLumen, NoteLumen]);

    private static readonly JsonSerializerOptions Indented = new() { WriteIndented = true };

    public static void IconSets(NijiiroGame game, string dstDir, Report report)
    {
        foreach (var (file, index, sets) in Icons)
            Convert(game, file, dstDir, index, null, report, lm => sets.Select(s =>
            {
                var sprite = s.Label.Contains(',') ? lm.SpriteWithLabels(s.Label) : lm.SpriteWithLabel(s.Label);
                return (s.Key, s.Prefix, sprite ?? throw new InvalidDataException($"no sprite labelled {s.Label} in {file}"));
            }).ToList());
    }

    /// <summary>The don note, without its caption, on a square transparent canvas: <c>&lt;dstDir&gt;/don_note.png</c>.</summary>
    public static void NoteIconImage(NijiiroGame game, string dstDir, Report report)
    {
        var sources = new[] { "nulm", "nutexb" }.Select(ext => game.File($"lumen/000_default/{NoteLumen}.{ext}")).ToArray();
        if (sources.FirstOrDefault(s => !File.Exists(s)) is { } missing)
        {
            report.Missing.Add(missing);
            return;
        }
        var output = Path.Combine(dstDir, NoteIcon);
        if (sources.All(s => NijiiroGame.IsNewer(output, s)))
        {
            report.UpToDate.Add(NoteIcon);
            return;
        }
        var lm = LumenFile.Parse(File.ReadAllBytes(sources[0]));
        var atlas = Nutexb.Decode(File.ReadAllBytes(sources[1]));
        // The lane's note sprite is the one with the two-player notes (wdon1p…); the others are captions.
        var sprite = lm.SpriteWithLabel("wdon1p") ?? throw new InvalidDataException("no note sprite (wdon1p) in onp_all");
        var icon = FirstBlob(lm.RenderStacked(sprite, "don", atlas, null)) ?? throw new InvalidDataException("the don note rendered empty");
        Directory.CreateDirectory(dstDir);
        icon.SavePng(output);
        report.Written.Add(NoteIcon);
    }

    /// <summary>The topmost shape (the rows down to the first empty one, e.g. the note above its caption), centred on a square.</summary>
    internal static RgbaImage? FirstBlob(RgbaImage img)
    {
        bool Opaque(int x, int y) => img.Pixel(x, y)[3] > 16;
        bool Filled(int y) => Enumerable.Range(0, img.Width).Any(x => Opaque(x, y));
        var top = Enumerable.Range(0, img.Height).Where(Filled).Select(y => (int?)y).FirstOrDefault();
        if (top is not { } t) return null;
        var bottom = Enumerable.Range(t, img.Height - t).Where(y => !Filled(y)).Select(y => (int?)y).FirstOrDefault() ?? img.Height;
        var cols = Enumerable.Range(0, img.Width).Where(x => Enumerable.Range(t, bottom - t).Any(y => Opaque(x, y))).ToList();
        var (left, right) = (cols[0], cols[^1] + 1);
        var (w, h) = (right - left, bottom - t);
        var side = Math.Max(w, h);
        var crop = new RgbaImage(w, h);
        for (var y = 0; y < h; y++) img.Pixels.AsSpan(((t + y) * img.Width + left) * 4, w * 4).CopyTo(crop.Pixels.AsSpan(y * w * 4));
        var square = new RgbaImage(side, side);
        square.Paste(crop, (side - w) / 2, (side - h) / 2);
        return square;
    }

    /// <summary>Title plates (index = <c>TitlePlateId</c>) and dan badges, plus a Lumen scene of the game's whole nameplate.</summary>
    public static void Nameplates(NijiiroGame game, string dstDir, Report report) =>
        Convert(game, NameplateLumen, dstDir, NameplateIndex, NameplateSet, report, lm =>
        [
            ("plates", "plate", lm.SpriteWithLabel("wood") ?? throw new InvalidDataException("no sprite labelled wood")),
            ("dan", "dan", lm.SpriteWithLabel("5kyuu_red") ?? throw new InvalidDataException("no sprite labelled 5kyuu_red")),
        ]);

    /// <summary>The game's whole nameplate (frame, plate slot, dan slot); its labels are layouts such as
    /// <c>newentry_shougou_dani</c>. Each layout lists its text fields (<c>text_title</c>, <c>text_name</c>) so the WebUI can
    /// draw names in the game's boxes.</summary>
    private static (string Key, JsonObject Set) NameplateSet(LumenFile lm, JsonObject sets)
    {
        var sprite = lm.SpritePlacing(PlateInstance) ?? throw new InvalidDataException("no nameplate sprite (title_instance)");
        var labels = lm.Labels(sprite);
        var bounds = lm.UnionBounds(sprite) ?? throw new InvalidDataException("empty nameplate");
        // Every plate fits where the nameplate puts it, not only the one placed by default.
        if (sets["plates"]?["bounds"] is JsonArray pb && pb.Count == 4 && labels.Count > 0 && lm.PositionOf(sprite, labels[0], PlateInstance) is [var x, var y])
        {
            var p = pb.Select(v => (float)v!.GetValue<double>()).ToArray();
            bounds = [MathF.Min(bounds[0], p[0] + x), MathF.Min(bounds[1], p[1] + y), MathF.Max(bounds[2], p[2] + x), MathF.Max(bounds[3], p[3] + y)];
        }
        var text = new JsonObject();
        foreach (var l in labels) text[l] = lm.TextFields(sprite, l);
        return ("nameplate", new JsonObject
        {
            ["sprite"] = sprite,
            ["bounds"] = Floats(bounds),
            ["labels"] = Strings(labels),
            ["plateInstance"] = PlateInstance,
            ["text"] = text,
        });
    }

    /// <summary>The per-difficulty achievement panel (crowns and 粋/雅/極 rank slots) and its digit glyphs.</summary>
    public static void Scoreboard(NijiiroGame game, string dstDir, Report report) =>
        Convert(game, ScoreboardLumen, dstDir, ScoreboardIndex, null, report, lm =>
        {
            // The board places the panel as bg_instance next to its rank counters; each counter places digits as n1_ins….
            var board = lm.SpritePlacing("num_kiwami_instance") ?? throw new InvalidDataException("no scoreboard sprite (num_kiwami_instance)");
            var counter = lm.Child(board, "num_kiwami_instance") ?? throw new InvalidDataException("no rank counter");
            return
            [
                ("panels", "panel", lm.Child(board, "bg_instance") ?? throw new InvalidDataException("no scoreboard panel (bg_instance)")),
                ("digits", "digit", lm.Child(counter, "n1_ins") ?? throw new InvalidDataException("no digit glyphs (n1_ins)")),
            ];
        });

    private static JsonArray Floats(float[] values) => new(values.Select(v => (JsonNode)JsonValue.Create((double)v)).ToArray());
    private static JsonArray Strings(IEnumerable<string> values) => new(values.Select(v => (JsonNode)JsonValue.Create(v)).ToArray());

    /// <summary>Renders each (index key, file prefix, sprite) set from <c>lumen/000_default/&lt;file&gt;</c> into
    /// <c>&lt;dstDir&gt;/&lt;prefix&gt;_&lt;label&gt;.png</c>, one shared canvas per set, and writes an index of the labels. With
    /// <paramref name="scene"/>, also writes those sets plus the one it adds as a Lumen scene for live playback.</summary>
    private static void Convert(NijiiroGame game, string file, string dstDir, string indexName, Func<LumenFile, JsonObject, (string, JsonObject)>? scene,
        Report report, Func<LumenFile, List<(string Key, string Prefix, uint Sprite)>> sets)
    {
        var sources = new[] { "nulm", "nutexb" }.Select(ext => game.File($"lumen/000_default/{file}.{ext}")).ToArray();
        if (sources.FirstOrDefault(s => !File.Exists(s)) is { } missing)
        {
            report.Missing.Add(missing);
            return;
        }
        var index = Path.Combine(dstDir, indexName);
        var outputs = scene is null ? [index] : new[] { index, Path.Combine(dstDir, NameplateScene) };
        if (outputs.All(o => sources.All(s => NijiiroGame.IsNewer(o, s))))
        {
            // Count what a fresh run writes: one image per label in the index (and the scene).
            if (JsonNode.Parse(File.ReadAllText(index)) is JsonObject old)
                foreach (var (key, set) in old)
                    report.UpToDate.AddRange((set?["labels"] as JsonArray ?? []).Select(l => $"{key}_{l}"));
            if (scene is not null) report.UpToDate.Add(NameplateScene);
            return;
        }
        var lm = LumenFile.Parse(File.ReadAllBytes(sources[0]));
        var atlas = Nutexb.Decode(File.ReadAllBytes(sources[1]));
        Directory.CreateDirectory(dstDir);
        var labels = new JsonObject();
        foreach (var (key, prefix, sprite) in sets(lm))
            labels[key] = RenderSet(lm, atlas, sprite, prefix, dstDir, report);
        if (scene is not null)
        {
            var all = (JsonObject)labels.DeepClone();
            var (key, set) = scene(lm, labels);
            all[key] = set;
            File.WriteAllText(Path.Combine(dstDir, NameplateScene), lm.SceneJson(all).ToJsonString());
            atlas.SavePng(Path.Combine(dstDir, "atlas.png"));
            report.Written.Add(NameplateScene);
        }
        // The index goes last: its being up to date means everything else is.
        File.WriteAllText(index, labels.ToJsonString(Indented));
    }

    /// <summary>Renders every label of <paramref name="sprite"/> on one canvas (the union of their mask-clipped bounds) so they
    /// line up. Returns the set's index entry: its labels in order, the sprite id and the shared canvas bounds (stage
    /// units), which a live Lumen player needs to frame the same canvas.</summary>
    private static JsonObject RenderSet(LumenFile lm, RgbaImage atlas, uint sprite, string prefix, string dstDir, Report report)
    {
        var names = lm.Labels(sprite);
        var union = lm.UnionBounds(sprite);
        foreach (var name in names)
        {
            try
            {
                lm.RenderStacked(sprite, name, atlas, union).SavePng(Path.Combine(dstDir, $"{prefix}_{name}.png"));
                report.Written.Add($"{prefix}_{name}");
            }
            catch (InvalidDataException e)
            {
                report.Failed.Add($"{prefix}_{name}: {e.Message}");
            }
        }
        return new JsonObject { ["sprite"] = sprite, ["bounds"] = union is null ? null : Floats(union), ["labels"] = Strings(names) };
    }
}
