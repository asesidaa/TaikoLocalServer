using System.Buffers.Binary;
using System.Text;
using System.Text.Json.Nodes;
using TaikoLocalServer.Infrastructure.Extract.Textures;

namespace TaikoLocalServer.Infrastructure.Extract.Lumen;

/// <summary>
/// Lumen (<c>LMB\0</c>) UI files: the Flash-like format the games' 2D UI uses. Parses shapes and sprite timelines and
/// rasterizes a sprite at a frame label from its texture atlases. Layout: a 0x40-byte header, then tags
/// <c>u32 type, u32 size_in_words, payload</c> until <c>0xFF00</c> (docs/webui-refactor-plan.md §3d has the tag table).
/// </summary>
public sealed class LumenFile
{
    private const uint TSymbols = 0xF001, TColors = 0xF002, TMatrices = 0xF003, TPositions = 0xF103, TBounds = 0xF004;
    private const uint TAtlases = 0xF007, TShape = 0xF022, TGraphic = 0xF024, TSprite = 0x0027, TLabel = 0x002B;
    private const uint TFrame = 0x0001, TKeyframe = 0xF105, TPlace = 0x0004, TRemove = 0x0005, TText = 0x0025, TEnd = 0xFF00;
    /// <summary>The older games' (PS3) graphic: one textured quad, <c>x y u v</c> × 4, then the atlas id and the fill.</summary>
    private const uint TQuad = 0xF023;
    /// <summary><c>F024</c> fill type 0: a solid colour, with the colour index stored where the atlas id normally is.</summary>
    private const ushort FillSolid = 0;

    private static readonly float[] Identity = [1, 0, 0, 1, 0, 0]; // a b c d tx ty

    public List<string> Symbols { get; } = [];
    private readonly List<float[]> colors = [], matrices = [], positions = [], bounds = [];
    /// <summary>Every atlas in a Nijiiro file has the same size; the PS3 files' differ (<see cref="RenderLayers"/>).</summary>
    private (float W, float H) atlasSize;
    private int atlasCount;
    private readonly Dictionary<uint, Text> texts = [];
    private readonly Dictionary<uint, List<Graphic>> shapes = [];
    private readonly Dictionary<uint, Sprite> sprites = [];

    /// <summary>A text field: the game fills it at runtime, so only its box and style matter here. The template is a symbol
    /// holding HTML: <c>&lt;p align="center"&gt;&lt;font ... size="22" color="#000000"&gt;sample&lt;/font&gt;&lt;/p&gt;</c>.</summary>
    private sealed record Text(uint Bounds, uint Template, float Outline);

    private sealed record Graphic(uint Atlas, ushort Fill, float[][] Verts, ushort[] Indices); // verts: x y u v

    private sealed class Sprite
    {
        public List<(string Name, uint Frame)> Labels { get; } = [];
        public List<(uint Id, List<Op> Ops)> Frames { get; } = [];
    }

    /// <summary>A place (<see cref="Remove"/> false) or remove op.</summary>
    private sealed record Op(bool Remove, uint Character, uint Name, ushort Mode, ushort Depth, ushort Clip, uint Transform, int ColorMult, int ColorAdd);

    public static LumenFile Parse(byte[] bytes)
    {
        if (bytes.Length < 8 || !bytes.AsSpan(0, 4).SequenceEqual("LMB\0"u8))
            throw new InvalidDataException("not a Lumen file (missing LMB magic)");
        // Nijiiro's files are little-endian, the PS3 games' big-endian; the header's second word is 0x10 either way round.
        var be = bytes.AsSpan(4, 4).SequenceEqual(new byte[] { 0, 0, 0, 0x10 });
        uint U32(int o) => o >= 0 && o + 4 <= bytes.Length
            ? be ? BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(o)) : BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(o))
            : throw new InvalidDataException("truncated LMB");
        ushort U16(int o) => o >= 0 && o + 2 <= bytes.Length
            ? be ? BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(o)) : BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(o))
            : throw new InvalidDataException("truncated LMB");
        float F32(int o) => BitConverter.UInt32BitsToSingle(U32(o));
        float[] Vec(int o, int n) => Enumerable.Range(0, n).Select(k => F32(o + k * 4)).ToArray();

        var lm = new LumenFile();
        uint? shape = null, sprite = null;
        // Ops after a keyframe tag belong to that keyframe (a snapshot for seeking), not to the last frame.
        var inKeyframe = false;
        Sprite Current() => sprite is { } s && lm.sprites.TryGetValue(s, out var sp) ? sp : throw new InvalidDataException("tag outside a sprite");
        List<Graphic> Shape() => shape is { } s ? lm.shapes[s] : throw new InvalidDataException("graphic outside a shape");
        void Push(Op op)
        {
            var s = Current();
            if (s.Frames.Count == 0) throw new InvalidDataException("display op before any frame");
            s.Frames[^1].Ops.Add(op);
        }

        var o = 0x40;
        while (o + 8 <= bytes.Length)
        {
            var (tag, size) = (U32(o), (int)U32(o + 4));
            var p = o + 8;
            o = p + size * 4;
            int Count() => (int)U32(p);
            switch (tag)
            {
                case TSymbols:
                {
                    var q = p + 4;
                    for (var i = Count(); i > 0; i--)
                    {
                        var len = (int)U32(q);
                        if (q + 4 + len > bytes.Length) throw new InvalidDataException("truncated symbol");
                        lm.Symbols.Add(Encoding.UTF8.GetString(bytes, q + 4, len));
                        q += 4 + ((len + 4) & ~3); // NUL-terminated, padded to 4
                    }
                    break;
                }
                case TColors:
                    for (var i = 0; i < Count(); i++)
                    {
                        var c = p + 4 + i * 8;
                        lm.colors.Add([U16(c) / 256f, U16(c + 2) / 256f, U16(c + 4) / 256f, U16(c + 6) / 256f]);
                    }
                    break;
                case TMatrices:
                    for (var i = 0; i < Count(); i++) lm.matrices.Add(Vec(p + 4 + i * 24, 6));
                    break;
                case TPositions:
                    for (var i = 0; i < Count(); i++) lm.positions.Add(Vec(p + 4 + i * 8, 2));
                    break;
                case TBounds:
                    for (var i = 0; i < Count(); i++) lm.bounds.Add(Vec(p + 4 + i * 16, 4));
                    break;
                case TAtlases:
                    // Entries: id, name symbol, width, height.
                    lm.atlasCount = Count();
                    if (lm.atlasCount > 0) lm.atlasSize = (F32(p + 12), F32(p + 16));
                    break;
                case TText:
                    // Words: id, ?, template symbol, ?, ?, bounds index, … word 15 = outline width.
                    lm.texts[U32(p)] = new Text(U32(p + 20), U32(p + 8), F32(p + 60));
                    break;
                case TQuad:
                    Shape().Add(new Graphic(U32(p + 64), U16(p + 68), Enumerable.Range(0, 4).Select(k => Vec(p + k * 16, 4)).ToArray(), [0, 1, 2, 0, 2, 3]));
                    break;
                case TShape:
                    shape = U32(p);
                    lm.shapes[shape.Value] = [];
                    break;
                case TGraphic:
                {
                    var (nv, ni) = (U16(p + 6), (int)U32(p + 8));
                    var verts = Enumerable.Range(0, nv).Select(k => Vec(p + 12 + k * 16, 4)).ToArray();
                    var ib = p + 12 + nv * 16;
                    Shape().Add(new Graphic(U32(p), U16(p + 4), verts, Enumerable.Range(0, ni).Select(k => U16(ib + k * 2)).ToArray()));
                    break;
                }
                case TSprite:
                    sprite = U32(p);
                    lm.sprites[sprite.Value] = new Sprite();
                    inKeyframe = false;
                    break;
                case TLabel:
                    Current().Labels.Add((lm.Symbol(U32(p)), U32(p + 4)));
                    break;
                case TFrame:
                    Current().Frames.Add((U32(p), []));
                    inKeyframe = false;
                    break;
                case TKeyframe:
                    inKeyframe = true; // duplicates state already reachable by playing frames
                    break;
                case TPlace or TRemove when inKeyframe:
                    break;
                case TPlace:
                    Push(new Op(false, U32(p), U32(p + 12), U16(p + 16), U16(p + 20), U16(p + 22), U32(p + 28), (int)U32(p + 32), (int)U32(p + 36)));
                    break;
                case TRemove:
                    Push(new Op(true, 0, 0, 0, U16(p + 4), 0, 0, 0, 0));
                    break;
                case TEnd:
                    o = int.MaxValue - 8;
                    break;
            }
        }
        if (lm.atlasSize.W <= 0)
            throw new InvalidDataException("Lumen file has no texture atlas");
        return lm;
    }

    private string Symbol(uint index) => index < Symbols.Count ? Symbols[(int)index] : "";

    /// <summary>The sprite whose labels include <paramref name="label"/> and that has the most labels (lowest id on a
    /// tie). Sprite ids differ between builds, so callers find sprites by what they contain.</summary>
    public uint? SpriteWithLabel(string label) => sprites.Where(s => s.Value.Labels.Any(l => l.Name == label))
        .OrderByDescending(s => s.Value.Labels.Count).ThenBy(s => s.Key).Select(s => (uint?)s.Key).FirstOrDefault();

    /// <summary>The sprite whose labels are exactly <paramref name="labels"/> (comma-separated, in order).</summary>
    public uint? SpriteWithLabels(string labels)
    {
        var want = labels.Split(',');
        return sprites.Where(s => s.Value.Labels.Select(l => l.Name).SequenceEqual(want)).Select(s => (uint?)s.Key).Min();
    }

    /// <summary>Characters <paramref name="sprite"/> places on any frame, with their instance names (stable across builds).</summary>
    private IEnumerable<(uint Character, string Name)> Placements(uint sprite) =>
        sprites.TryGetValue(sprite, out var s)
            ? s.Frames.SelectMany(f => f.Ops).Where(op => !op.Remove).Select(op => (op.Character, Symbol(op.Name)))
            : [];

    /// <summary>The sprite that places an instance called <paramref name="instance"/> (lowest id if several do).</summary>
    public uint? SpritePlacing(string instance) =>
        sprites.Keys.Where(id => Placements(id).Any(p => p.Name == instance)).Select(id => (uint?)id).Min();

    /// <summary>The character <paramref name="parent"/> places under the instance name <paramref name="instance"/>.</summary>
    public uint? Child(uint parent, string instance) =>
        Placements(parent).Where(p => p.Name == instance).Select(p => (uint?)p.Character).FirstOrDefault();

    /// <summary>The labels of <paramref name="sprite"/>, in timeline order.</summary>
    public List<string> Labels(uint sprite) => sprites.TryGetValue(sprite, out var s) ? s.Labels.Select(l => l.Name).ToList() : [];

    /// <summary>The atlases <paramref name="sprite"/> draws from, over all its labels.</summary>
    public SortedSet<uint> Atlases(uint sprite) =>
        new(Labels(sprite).SelectMany(l => Scene(sprite, l).Tris).Where(t => t.Solid is null).Select(t => t.Atlas));

    /// <summary>Union of <paramref name="sprite"/>'s mask-clipped bounds over all its labels: a canvas every label fits on.</summary>
    public float[]? UnionBounds(uint sprite)
    {
        float[]? union = null;
        foreach (var label in Labels(sprite))
            if (Scene(sprite, label).Bounds() is { } b)
                union = union is null ? b : [Math.Min(union[0], b[0]), Math.Min(union[1], b[1]), Math.Max(union[2], b[2]), Math.Max(union[3], b[3])];
        return union;
    }

    /// <summary>Stage-space bounds of what <paramref name="sprite"/> draws at <paramref name="label"/>, clipped by its masks.</summary>
    public float[]? Bounds(uint sprite, string label) => Scene(sprite, label).Bounds();

    /// <summary>Renders <paramref name="sprite"/> at <paramref name="label"/> from one image per atlas, each its own size
    /// (the PS3 files' <c>img*.nut</c>). Atlases the sprite doesn't use (see <see cref="Atlases"/>) may be null.
    /// <paramref name="bounds"/> (stage units, x0 y0 x1 y1) fixes the canvas; otherwise it fits the drawn content.</summary>
    public RgbaImage RenderLayers(uint sprite, string label, IReadOnlyList<RgbaImage?> layers, float[]? bounds) =>
        Render(sprite, label, bounds, (layer, u, v) => layer < layers.Count && layers[(int)layer] is { } img
            ? Sample(img, img.Width, img.Height, 0, u, v)
            : default);

    /// <summary>Renders from one image holding every atlas stacked vertically (Nijiiro's <c>.nutexb</c> layers).</summary>
    public RgbaImage RenderStacked(uint sprite, string label, RgbaImage atlas, float[]? bounds) =>
        Render(sprite, label, bounds, (layer, u, v) => Sample(atlas, atlasSize.W, atlasSize.H, layer, u, v));

    private RgbaImage Render(uint sprite, string label, float[]? bounds, Func<uint, float, float, Rgba> texel)
    {
        var scene = Scene(sprite, label);
        var b = bounds ?? scene.Bounds() ?? throw new InvalidDataException("nothing drawn");
        var (x0, y0) = (b[0], b[1]);
        var (w, h) = ((int)MathF.Max(MathF.Ceiling(b[2] - x0), 1), (int)MathF.Max(MathF.Ceiling(b[3] - y0), 1));
        var outPx = new Rgba[w * h];
        foreach (var tri in scene.Tris)
        {
            var pts = tri.P.Select(q => new[] { q[0] - x0, q[1] - y0, q[2], q[3] }).ToArray();
            var (a, bb, c) = (pts[0], pts[1], pts[2]);
            var den = (bb[1] - c[1]) * (a[0] - c[0]) + (c[0] - bb[0]) * (a[1] - c[1]);
            if (den == 0) continue;
            float bx0 = float.MaxValue, bx1 = float.MinValue, by0 = float.MaxValue, by1 = float.MinValue;
            foreach (var q in pts)
            {
                (bx0, bx1) = (MathF.Min(bx0, q[0]), MathF.Max(bx1, q[0]));
                (by0, by1) = (MathF.Min(by0, q[1]), MathF.Max(by1, q[1]));
            }
            for (var y = (int)MathF.Max(MathF.Floor(by0), 0); y <= (int)MathF.Max(MathF.Min(MathF.Ceiling(by1), h - 1f), 0); y++)
            for (var x = (int)MathF.Max(MathF.Floor(bx0), 0); x <= (int)MathF.Max(MathF.Min(MathF.Ceiling(bx1), w - 1f), 0); x++)
            {
                var (px, py) = (x + 0.5f, y + 0.5f);
                var l1 = ((bb[1] - c[1]) * (px - c[0]) + (c[0] - bb[0]) * (py - c[1])) / den;
                var l2 = ((c[1] - a[1]) * (px - c[0]) + (a[0] - c[0]) * (py - c[1])) / den;
                var l3 = 1f - l1 - l2;
                if (l1 < -1e-6f || l2 < -1e-6f || l3 < -1e-6f) continue;
                if (!tri.Masks.All(m => scene.Masks[m].Contains(px + x0, py + y0))) continue;
                var t = tri.Solid ?? texel(tri.Atlas, l1 * a[2] + l2 * bb[2] + l3 * c[2], l1 * a[3] + l2 * bb[3] + l3 * c[3]);
                float Src(float v, int k) => Math.Clamp(v * tri.Mult[k] + tri.Add[k], 0, 1);
                var (sr, sg, sb, sa) = (Src(t.R, 0), Src(t.G, 1), Src(t.B, 2), Src(t.A, 3));
                var dst = outPx[y * w + x];
                var alpha = sa + dst.A * (1 - sa);
                float Mix(float s, float d) => alpha > 0 ? (s * sa + d * dst.A * (1 - sa)) / alpha : 0;
                outPx[y * w + x] = new Rgba(Mix(sr, dst.R), Mix(sg, dst.G), Mix(sb, dst.B), alpha);
            }
        }
        var image = new RgbaImage(w, h);
        static byte Byte(float v) => (byte)Math.Clamp(MathF.Round(v * 255f, MidpointRounding.AwayFromZero), 0, 255);
        for (var i = 0; i < outPx.Length; i++)
            (image.Pixels[i * 4], image.Pixels[i * 4 + 1], image.Pixels[i * 4 + 2], image.Pixels[i * 4 + 3]) =
                (Byte(outPx[i].R), Byte(outPx[i].G), Byte(outPx[i].B), Byte(outPx[i].A));
        return image;
    }

    private uint? FrameOf(uint sprite, string label, out Sprite? s) =>
        sprites.TryGetValue(sprite, out s) ? s.Labels.Where(l => l.Name == label).Select(l => (uint?)l.Frame).FirstOrDefault() : null;

    /// <summary>Where <paramref name="sprite"/> places <paramref name="instance"/> at <paramref name="label"/> (its translation).</summary>
    public float[]? PositionOf(uint sprite, string label, string instance) =>
        FrameOf(sprite, label, out var s) is { } frame
            ? DisplayList(s!, frame).Where(e => Symbol(e.Obj.Name) == instance).Select(e => new[] { e.Obj.Matrix[4], e.Obj.Matrix[5] }).FirstOrDefault()
            : null;

    /// <summary>The text fields <paramref name="sprite"/> shows at <paramref name="label"/>, by instance name: <c>{ box: [x0,y0,x1,y1],
    /// size, color, align, outline }</c> in the sprite's stage units, with the style read from the field's HTML template.</summary>
    public JsonObject TextFields(uint sprite, string label)
    {
        var fields = new JsonObject();
        if (FrameOf(sprite, label, out var s) is not { } frame) return fields;
        foreach (var (_, o) in DisplayList(s!, frame))
        {
            if (!texts.TryGetValue(o.Character, out var t)) continue;
            var b = t.Bounds < bounds.Count ? bounds[(int)t.Bounds] : [0, 0, 0, 0];
            var m = o.Matrix;
            var html = Symbol(t.Template);
            string? Attr(string name)
            {
                var at = html.IndexOf($"{name}=\"", StringComparison.Ordinal);
                if (at < 0) return null;
                var start = at + name.Length + 2;
                var end = html.IndexOf('"', start);
                return end < 0 ? html[start..] : html[start..end];
            }
            fields[Symbol(o.Name)] = new JsonObject
            {
                ["box"] = Floats([m[0] * b[0] + m[4], m[3] * b[1] + m[5], m[0] * b[2] + m[4], m[3] * b[3] + m[5]]),
                ["size"] = (double)(float.TryParse(Attr("size"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var size) ? size : 0),
                ["color"] = Attr("color") ?? "#000000",
                ["align"] = Attr("align") ?? "left",
                ["outline"] = (double)t.Outline,
            };
        }
        return fields;
    }

    private static JsonArray Floats(IEnumerable<float> values) => new(values.Select(v => (JsonNode)JsonValue.Create((double)v)).ToArray());

    /// <summary>A <c>lumen-scene</c> v1 document (played by the WebUI's lumen-player.js) holding <paramref name="sets"/> (each
    /// <c>{ "sprite": id, … }</c>) and every shape and sprite they reach. The atlas is saved beside it as <c>atlas.png</c>.</summary>
    public JsonObject SceneJson(JsonObject sets)
    {
        var keep = new SortedSet<uint>();
        var stack = new Stack<uint>(sets.Select(e => e.Value?["sprite"]?.GetValue<uint>()).OfType<uint>());
        while (stack.TryPop(out var id))
            if (keep.Add(id))
                foreach (var (c, _) in Placements(id)) stack.Push(c);
        static double Round(float v, double k) => Math.Round(v * k, MidpointRounding.AwayFromZero) / k;
        static JsonArray Rounded(IEnumerable<float> values, double k) => new(values.Select(v => (JsonNode)JsonValue.Create(Round(v, k))).ToArray());
        var (shapeJson, spriteJson) = (new JsonObject(), new JsonObject());
        foreach (var id in keep)
        {
            if (shapes.TryGetValue(id, out var graphics))
                shapeJson[id.ToString()] = new JsonArray(graphics.Select(g => (JsonNode)new JsonObject
                {
                    ["atlas"] = g.Atlas,
                    ["fill"] = g.Fill,
                    ["verts"] = Rounded(g.Verts.SelectMany(v => v), 1e5),
                    ["indices"] = new JsonArray(g.Indices.Select(i => (JsonNode)i).ToArray()),
                }).ToArray());
            if (!sprites.TryGetValue(id, out var sp)) continue;
            // place: [1, char, mode, depth, clip, transform, colorMult, colorAdd, instanceName?]; remove: [0, depth]
            JsonNode Op(Op op)
            {
                if (op.Remove) return new JsonArray(0, op.Depth);
                var v = new JsonArray(1, op.Character, op.Mode, op.Depth, op.Clip, op.Transform, op.ColorMult, op.ColorAdd);
                if (Symbol(op.Name).Length > 0) v.Add(Symbol(op.Name));
                return v;
            }
            var labels = new JsonObject();
            foreach (var (name, frame) in sp.Labels) labels[name] = frame;
            spriteJson[id.ToString()] = new JsonObject
            {
                ["frameCount"] = sp.Frames.Count == 0 ? 1 : sp.Frames.Max(f => f.Id + 1),
                ["labels"] = labels,
                ["frames"] = new JsonArray(sp.Frames.Select(f => (JsonNode)new JsonArray(f.Id, new JsonArray(f.Ops.Select(Op).ToArray()))).ToArray()),
            };
        }
        return new JsonObject
        {
            ["format"] = "lumen-scene",
            ["version"] = 1,
            ["fps"] = 60,
            ["atlas"] = new JsonObject { ["image"] = "atlas.png", ["width"] = (double)atlasSize.W, ["height"] = (double)atlasSize.H, ["layers"] = atlasCount },
            ["colors"] = new JsonArray(colors.Select(c => (JsonNode)Rounded(c, 1e5)).ToArray()),
            ["matrices"] = new JsonArray(matrices.Select(m => (JsonNode)Rounded(m, 1e5)).ToArray()),
            ["positions"] = new JsonArray(positions.Select(p => (JsonNode)Rounded(p, 1e3)).ToArray()),
            ["shapes"] = shapeJson,
            ["sprites"] = spriteJson,
            ["sets"] = sets.DeepClone(),
        };
    }

    private SceneData Scene(uint sprite, string label)
    {
        var s = sprites.TryGetValue(sprite, out var sp) ? sp : throw new InvalidDataException("no such sprite");
        var frame = s.Labels.Where(l => l.Name == label).Select(l => (uint?)l.Frame).FirstOrDefault()
            ?? throw new InvalidDataException($"no label {label}");
        var scene = new SceneData();
        Collect(sprite, frame, Identity, [1, 1, 1, 1], [0, 0, 0, 0], [], scene, 0);
        return scene;
    }

    private sealed class Placed
    {
        public uint Character, Name;
        public float[] Matrix = Identity, Mult = [1, 1, 1, 1], Add = [0, 0, 0, 0];
        public ushort Clip;
        public Placed Clone() => (Placed)MemberwiseClone();
    }

    /// <summary>Plays <paramref name="sprite"/>'s frames up to <paramref name="frame"/> into a depth-ordered display list.</summary>
    private List<(ushort Depth, Placed Obj)> DisplayList(Sprite sprite, uint frame)
    {
        var list = new Dictionary<ushort, Placed>();
        foreach (var (_, ops) in sprite.Frames.TakeWhile(f => f.Id <= frame))
        foreach (var op in ops)
        {
            if (op.Remove)
            {
                list.Remove(op.Depth);
                continue;
            }
            // A placement on an occupied depth keeps what it doesn't give (transform 0xFFFF, colour -1); mode 2 (move)
            // also keeps the character.
            var obj = list.TryGetValue(op.Depth, out var prev) ? prev.Clone() : new Placed { Character = op.Character, Name = op.Name, Clip = op.Clip };
            if (op.Mode != 2)
                (obj.Character, obj.Name, obj.Clip) = (op.Character, op.Name, op.Clip);
            var (kind, index) = (op.Transform >> 16, (int)(op.Transform & 0xFFFF));
            if (kind == 0x0000)
                obj.Matrix = index < matrices.Count ? matrices[index] : Identity;
            else if (kind == 0x8000)
                obj.Matrix = index < positions.Count ? [1, 0, 0, 1, positions[index][0], positions[index][1]] : [1, 0, 0, 1, 0, 0];
            if (op.ColorMult >= 0 && op.ColorMult < colors.Count) obj.Mult = colors[op.ColorMult];
            if (op.ColorAdd >= 0 && op.ColorAdd < colors.Count) obj.Add = colors[op.ColorAdd];
            list[op.Depth] = obj;
        }
        return list.OrderBy(e => e.Key).Select(e => (e.Key, e.Value)).ToList();
    }

    private void Collect(uint character, uint frame, float[] m, float[] mult, float[] add, int[] masks, SceneData scene, int depth)
    {
        if (depth > 32) return; // malformed recursion guard
        if (shapes.TryGetValue(character, out var graphics))
        {
            foreach (var g in graphics)
            {
                Rgba? solid = g.Fill == FillSolid
                    ? g.Atlas < colors.Count ? new Rgba(colors[(int)g.Atlas][0], colors[(int)g.Atlas][1], colors[(int)g.Atlas][2], colors[(int)g.Atlas][3]) : default(Rgba)
                    : null;
                for (var i = 0; i + 3 <= g.Indices.Length; i += 3)
                {
                    if (g.Indices.Skip(i).Take(3).Any(k => k >= g.Verts.Length)) continue;
                    var p = g.Indices.Skip(i).Take(3).Select(k => g.Verts[k])
                        .Select(v => new[] { m[0] * v[0] + m[2] * v[1] + m[4], m[1] * v[0] + m[3] * v[1] + m[5], v[2], v[3] }).ToArray();
                    scene.Tris.Add(new Tri(p, g.Atlas, solid, mult, add, masks));
                }
            }
            return;
        }
        if (!sprites.TryGetValue(character, out var sprite)) return;
        // Nested sprites are drawn at frame 0; only the root plays to its label.
        var active = new List<(int Mask, ushort Until)>();
        foreach (var (d, obj) in DisplayList(sprite, frame))
        {
            active.RemoveAll(a => d > a.Until);
            var child = Mul(m, obj.Matrix);
            var current = masks.Concat(active.Select(a => a.Mask)).ToArray();
            if (obj.Clip > 0)
            {
                // A mask: its shape limits depths d+1..=clip and is never drawn itself.
                var maskScene = new SceneData();
                Collect(obj.Character, 0, child, [1, 1, 1, 1], [0, 0, 0, 0], [], maskScene, depth + 1);
                scene.Masks.Add(new Mask(maskScene.Tris.Select(t => t.P.Select(q => (q[0], q[1])).ToArray()).ToList()));
                active.Add((scene.Masks.Count - 1, obj.Clip));
                continue;
            }
            var childMult = Enumerable.Range(0, 4).Select(k => mult[k] * obj.Mult[k]).ToArray();
            var childAdd = Enumerable.Range(0, 4).Select(k => add[k] + obj.Add[k]).ToArray();
            Collect(obj.Character, 0, child, childMult, childAdd, current, scene, depth + 1);
        }
    }

    private static float[] Mul(float[] a, float[] b) =>
    [
        a[0] * b[0] + a[2] * b[1],
        a[1] * b[0] + a[3] * b[1],
        a[0] * b[2] + a[2] * b[3],
        a[1] * b[2] + a[3] * b[3],
        a[0] * b[4] + a[2] * b[5] + a[4],
        a[1] * b[4] + a[3] * b[5] + a[5],
    ];

    /// <summary>Bilinear sample of atlas <paramref name="layer"/> (layers stacked vertically), clamped to that layer.</summary>
    private static Rgba Sample(RgbaImage atlas, float aw, float ah, uint layer, float u, float v)
    {
        var (x, y) = (u * aw - 0.5f, v * ah - 0.5f + layer * ah);
        var (xi, yi) = (MathF.Floor(x), MathF.Floor(y));
        var (fx, fy) = (x - xi, y - yi);
        var (top, bottom) = (layer * ah, layer * ah + ah - 1f);
        Span<float> acc = stackalloc float[4];
        foreach (var (dx, dy, wt) in new[] { (0f, 0f, (1 - fx) * (1 - fy)), (1f, 0f, fx * (1 - fy)), (0f, 1f, (1 - fx) * fy), (1f, 1f, fx * fy) })
        {
            var sx = (uint)Math.Clamp(xi + dx, 0, aw - 1);
            var sy = (uint)Math.Clamp(yi + dy, top, bottom);
            if (sx >= atlas.Width || sy >= atlas.Height) continue;
            var p = atlas.Pixel((int)sx, (int)sy);
            for (var k = 0; k < 4; k++) acc[k] += p[k] / 255f * wt;
        }
        return new Rgba(acc[0], acc[1], acc[2], acc[3]);
    }

    private readonly record struct Rgba(float R, float G, float B, float A);

    /// <summary>A triangle: stage x y and atlas u v per point; <see cref="Solid"/> set for a solid fill.</summary>
    private sealed record Tri(float[][] P, uint Atlas, Rgba? Solid, float[] Mult, float[] Add, int[] Masks);

    private sealed class Mask
    {
        private readonly List<(float X, float Y)[]> tris;
        public float[] Box { get; } = [float.MaxValue, float.MaxValue, float.MinValue, float.MinValue];

        public Mask(List<(float X, float Y)[]> tris)
        {
            this.tris = tris;
            foreach (var (x, y) in tris.SelectMany(t => t))
                Box = [MathF.Min(Box[0], x), MathF.Min(Box[1], y), MathF.Max(Box[2], x), MathF.Max(Box[3], y)];
        }

        public bool Contains(float x, float y)
        {
            if (x < Box[0] || x > Box[2] || y < Box[1] || y > Box[3]) return false;
            float S((float X, float Y) p, (float X, float Y) q) => (q.X - p.X) * (y - p.Y) - (q.Y - p.Y) * (x - p.X);
            return tris.Any(t =>
            {
                var (d1, d2, d3) = (S(t[0], t[1]), S(t[1], t[2]), S(t[2], t[0]));
                return !((d1 < 0 || d2 < 0 || d3 < 0) && (d1 > 0 || d2 > 0 || d3 > 0));
            });
        }
    }

    private sealed class SceneData
    {
        public List<Tri> Tris { get; } = [];
        public List<Mask> Masks { get; } = [];

        /// <summary>Bounds of the drawn triangles, each clipped to its masks' boxes.</summary>
        public float[]? Bounds()
        {
            float[]? result = null;
            foreach (var t in Tris)
            {
                float[] b = [float.MaxValue, float.MaxValue, float.MinValue, float.MinValue];
                foreach (var p in t.P)
                    b = [MathF.Min(b[0], p[0]), MathF.Min(b[1], p[1]), MathF.Max(b[2], p[0]), MathF.Max(b[3], p[1])];
                foreach (var mb in t.Masks.Select(m => Masks[m].Box))
                    b = [MathF.Max(b[0], mb[0]), MathF.Max(b[1], mb[1]), MathF.Min(b[2], mb[2]), MathF.Min(b[3], mb[3])];
                if (b[0] >= b[2] || b[1] >= b[3]) continue; // fully masked out
                result = result is null ? b : [MathF.Min(result[0], b[0]), MathF.Min(result[1], b[1]), MathF.Max(result[2], b[2]), MathF.Max(result[3], b[3])];
            }
            return result;
        }
    }
}
