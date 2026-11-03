using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
using TaikoLocalServer.Infrastructure.Extract.Models;
using TaikoLocalServer.Infrastructure.Extract.Textures;

namespace TaikoLocalServer.Infrastructure.Extract.Ac15;

/// <summary>
/// The older games' 3D don-chan parts: NUD (<c>NDP3</c>, PS3, big-endian) meshes with their <c>.nut</c> textures beside
/// them, converted to binary glTF. The layout follows Smash Forge's <c>NUD.cs</c> (MIT); this project's README has what
/// the files use.
/// </summary>
public static class Nud
{
    /// <param name="Bone">The bone a whole object moves with (faces, hats), when its vertices carry no weights.</param>
    public sealed record NudObject(string Name, ushort? Bone, List<Part> Parts);

    public sealed class Part
    {
        public List<float> Positions { get; } = [];
        public List<float> Normals { get; } = [];
        public List<float> Uvs { get; } = [];
        public List<float>? Colors;
        public List<ushort>? Joints;
        public List<float>? Weights;
        public List<uint> Indices { get; } = [];
        /// <summary>The drawn pass's first texture (a <c>GIDX</c> id in the <c>.nut</c>).</summary>
        public uint? Texture;
        /// <summary>The texture's wrap modes (S, T): 1 repeat, 2 mirror, 3 clamp.</summary>
        public byte WrapS = 1, WrapT = 1;
        public bool DoubleSided;
        /// <summary>The drawn pass's material flags (<c>0x10000001</c>: its low byte picks the shader; see the README).</summary>
        public uint Flags;
        /// <summary>Source or destination blend factor set: alpha-blended. Otherwise the pass is opaque and ignores texture alpha.</summary>
        public bool Blend;
        /// <summary>Alpha test: the reference (0–255) a texel's alpha must reach.</summary>
        public ushort? AlphaRef;
        /// <summary>The raw source and destination blend factors and alpha-test function, for <c>extras</c>.</summary>
        public ushort SrcBlend, DstBlend, AlphaFunc;
        /// <summary>A pass culling front faces: the black outline shell.</summary>
        public bool Outline;
    }

    private sealed class R(byte[] b)
    {
        public byte U8(int o) => o >= 0 && o < b.Length ? b[o] : throw new InvalidDataException("truncated NUD");
        public ushort U16(int o) => o >= 0 && o + 2 <= b.Length ? BinaryPrimitives.ReadUInt16BigEndian(b.AsSpan(o)) : throw new InvalidDataException("truncated NUD");
        public uint U32(int o) => o >= 0 && o + 4 <= b.Length ? BinaryPrimitives.ReadUInt32BigEndian(b.AsSpan(o)) : throw new InvalidDataException("truncated NUD");
        public float F32(int o) => BitConverter.UInt32BitsToSingle(U32(o));
        public float F16(int o) => (float)BitConverter.UInt16BitsToHalf(U16(o));
    }

    /// <summary>Header: <c>NDP3</c>, u32 size, u16 version (0x0200), u16 object count, u16 type, u16 bone count, then the
    /// sizes of the index, vertex and vertex-add clumps (the index clump starts at the first value + 0x30), a bounding
    /// sphere, and 0x30-byte object entries; the names follow the clumps.</summary>
    public static List<NudObject> Parse(byte[] b)
    {
        var r = new R(b);
        if (!b.AsSpan().StartsWith("NDP3"u8)) throw new InvalidDataException("not an NDP3 model");
        if (r.U16(8) != 0x0200) throw new InvalidDataException($"NDP3 version 0x{r.U16(8):x} isn't supported");
        var count = r.U16(10);
        var indexAt = (int)r.U32(0x10) + 0x30;
        var vertAt = indexAt + (int)r.U32(0x14);
        var addAt = vertAt + (int)r.U32(0x18);
        var nameAt = addAt + (int)r.U32(0x1C);
        var objects = new List<NudObject>(count);
        for (var i = 0; i < count; i++)
        {
            // Bounding sphere (8 floats), name offset, flags, single-bind bone, polygon count, polygon header offset.
            var o = 0x30 + i * 0x30;
            var nameStart = nameAt + (int)r.U32(o + 32);
            if (nameStart > b.Length) throw new InvalidDataException("truncated NUD");
            var nameEnd = Array.IndexOf(b, (byte)0, nameStart);
            var bone = r.U16(o + 40);
            var parts = Enumerable.Range(0, r.U16(o + 42)).Select(k => ReadPart(r, (int)r.U32(o + 44) + k * 0x30, indexAt, vertAt, addAt)).ToList();
            objects.Add(new NudObject(Encoding.UTF8.GetString(b, nameStart, (nameEnd < 0 ? b.Length : nameEnd) - nameStart), bone == 0xFFFF ? null : bone, parts));
        }
        return objects;
    }

    /// <summary>A polygon header (0x30): index, vertex and vertex-add offsets, u16 vertex count, the vertex type (high
    /// nibble bones: 0 none, 1 four u32 indices + four float weights; low nibble normals: 1 float, 6 half float), the UV
    /// type (high nibble UV count; low nibble colour: 0 none, 2 RGBA bytes), four material offsets, u16 index count, strip
    /// flags. With bones the UVs and colours are in the vertex clump and the rest in the vertex-add clump.</summary>
    private static Part ReadPart(R r, int h, int indexAt, int vertAt, int addAt)
    {
        var (indexOff, vertOff, addOff) = ((int)r.U32(h), (int)r.U32(h + 4), (int)r.U32(h + 8));
        var n = r.U16(h + 12);
        var (vtype, uvtype) = (r.U8(h + 14), r.U8(h + 15));
        var (bones, normal) = (vtype >> 4, vtype & 0xF);
        var (uvCount, color) = (uvtype >> 4, uvtype & 0xF);
        if (bones is not (0 or 1) || normal is not (1 or 6) || color is not (0 or 2))
            throw new InvalidDataException($"vertex type 0x{vtype:x}/0x{uvtype:x} isn't supported");
        if (r.U8(h + 34) >> 4 == 4) throw new InvalidDataException("triangle-list polygons aren't supported");

        var p = new Part { Colors = color == 2 ? [] : null };
        // Colour and UVs: interleaved per vertex, after the position and normal when there are no bones.
        int UvBlock(int o)
        {
            if (p.Colors is { } c)
            {
                // 0x7F is 1.0, as in Smash's NUD; values above it brighten.
                for (var k = 0; k < 4; k++) c.Add(r.U8(o + k) / 127f);
                o += 4;
            }
            for (var u = 0; u < uvCount; u++, o += 4)
                if (u == 0) p.Uvs.AddRange([r.F16(o), r.F16(o + 2)]);
            return o;
        }
        var (vo, uo) = bones == 1 ? (addAt + addOff, vertAt + vertOff) : (vertAt + vertOff, 0);
        List<ushort> joints = [];
        List<float> weights = [];
        for (var v = 0; v < n; v++)
        {
            p.Positions.AddRange([r.F32(vo), r.F32(vo + 4), r.F32(vo + 8)]);
            if (normal == 1)
            {
                // Position w, normal xyz, normal w.
                p.Normals.AddRange([r.F32(vo + 16), r.F32(vo + 20), r.F32(vo + 24)]);
                vo += 32;
            }
            else
            {
                p.Normals.AddRange([r.F16(vo + 12), r.F16(vo + 14), r.F16(vo + 16)]);
                vo += 20;
            }
            if (bones == 1)
            {
                var w = Enumerable.Range(0, 4).Select(k => r.F32(vo + 16 + k * 4)).ToArray();
                var sum = 0f;
                foreach (var x in w) sum += x;
                for (var k = 0; k < 4; k++)
                {
                    joints.Add(w[k] > 0 ? (ushort)r.U32(vo + k * 4) : (ushort)0);
                    weights.Add(sum > 0 ? w[k] / sum : k == 0 ? 1 : 0);
                }
                vo += 32;
                uo = UvBlock(uo);
            }
            else
            {
                vo = UvBlock(vo);
            }
        }
        if (bones == 1) (p.Joints, p.Weights) = (joints, weights);

        // Triangle strips, 0xFFFF restarting one; every other triangle flips to keep the winding.
        var strip = Enumerable.Range(0, r.U16(h + 32)).Select(k => r.U16(indexAt + indexOff + k * 2)).ToList();
        var start = 0;
        for (var end = 0; end <= strip.Count; end++)
        {
            if (end < strip.Count && strip[end] != 0xFFFF) continue;
            for (var k = 2; k < end - start; k++)
            {
                var (a, bb, c) = k % 2 == 0 ? (strip[start + k - 2], strip[start + k - 1], strip[start + k]) : (strip[start + k - 1], strip[start + k - 2], strip[start + k]);
                if (a != bb && bb != c && a != c) p.Indices.AddRange([a, bb, c]);
            }
            start = end + 1;
        }
        if (p.Indices.Any(i => i >= n)) throw new InvalidDataException("index out of range");

        // Material: u32 flags, u32, u16 source blend, u16 texture count, u16 destination blend, u8 alpha test (2 on), u8
        // alpha function, u16 alpha reference, u16 cull mode (0x405 back, 0x404 front, 0 none), …; textures from +0x20,
        // 0x18 bytes each, the first u32 being the texture's GIDX id. Up to four passes; the outline shell is the one
        // culling front faces, and it comes first in some kigurumi (3, 4, 8, 13, 16, 17, 59). The drawn pass is the first
        // that doesn't.
        var mat = 0;
        foreach (var m in Enumerable.Range(0, 4).Select(k => (int)r.U32(h + 16 + k * 4)).Where(m => m != 0))
        {
            if (r.U16(m + 18) == 0x404) p.Outline = true;
            else if (mat == 0) mat = m;
        }
        if (mat != 0)
        {
            if (r.U16(mat + 10) > 0)
                (p.Texture, p.WrapS, p.WrapT) = (r.U32(mat + 0x20), r.U8(mat + 0x2C), r.U8(mat + 0x2D));
            p.DoubleSided = r.U16(mat + 18) == 0;
            p.Flags = r.U32(mat);
            (p.SrcBlend, p.DstBlend, p.AlphaFunc) = (r.U16(mat + 8), r.U16(mat + 12), r.U8(mat + 15));
            p.Blend = p.SrcBlend != 0 || p.DstBlend != 0;
            p.AlphaRef = r.U8(mat + 14) != 0 ? r.U16(mat + 16) : null;
        }
        return p;
    }

    /// <summary>glTF's alpha mode for a pass: blended, alpha-tested (cutoff = reference / 255), or opaque. An opaque pass
    /// ignores texture alpha: the bodies' recolour masks keep colour under alpha 0.</summary>
    public static (string Mode, float? Cutoff) AlphaMode(Part p) => (p.Blend, p.AlphaRef) switch
    {
        (true, _) => ("BLEND", null),
        (false, { } r) => ("MASK", Math.Min(r, (ushort)255) / 255f),
        _ => ("OPAQUE", null),
    };

    /// <summary>The model's objects as glTF meshes (one primitive per polygon) with their textures embedded. With the rig
    /// and its bind pose, the meshes are skinned to it (nodes <c>0..rig.Count</c>, named as in <c>animation/don.glb</c>);
    /// objects bound whole to one bone get that bone at full weight. <paramref name="clip"/> adds an animation "loop".</summary>
    public static byte[] Glb(List<NudObject> objects, List<Texture> textures, (List<Bone> Bones, BonePose[] Bind)? rig, (byte[] Bytes, int Slots)? clip)
    {
        var byId = new Dictionary<uint, RgbaImage>();
        foreach (var t in textures) byId[t.Id] = t.Image; // a repeated id: the last wins
        var g = new Gltf();
        var images = new Dictionary<uint, int>();
        var nodes = new List<int>();
        if (rig is var (bones, bind))
        {
            nodes.Add(DonRig.PushBones(g, bones, bind));
            DonRig.PushSkin(g, bones, bind);
        }
        var boneCount = rig?.Bones.Count ?? 0;
        foreach (var o in objects)
        {
            var primitives = new JsonArray();
            foreach (var p in o.Parts)
            {
                var n = p.Positions.Count / 3;
                float[] min = [float.MaxValue, float.MaxValue, float.MaxValue], max = [float.MinValue, float.MinValue, float.MinValue];
                for (var v = 0; v < n; v++)
                for (var c = 0; c < 3; c++)
                    (min[c], max[c]) = (MathF.Min(min[c], p.Positions[v * 3 + c]), MathF.Max(max[c], p.Positions[v * 3 + c]));
                var attributes = new JsonObject
                {
                    ["POSITION"] = g.AccessorF32([.. p.Positions], "VEC3", 3, Gltf.ArrayBuffer, (min, max)),
                    ["NORMAL"] = g.AccessorF32([.. p.Normals], "VEC3", 3, Gltf.ArrayBuffer),
                };
                if (p.Uvs.Count > 0) attributes["TEXCOORD_0"] = g.AccessorF32([.. p.Uvs], "VEC2", 2, Gltf.ArrayBuffer);
                if (p.Colors is { } colors) attributes["COLOR_0"] = g.AccessorF32([.. colors], "VEC4", 4, Gltf.ArrayBuffer);
                if (rig is not null)
                {
                    var joints = p.Joints?.ToArray() ?? Enumerable.Range(0, n).SelectMany(_ => new ushort[] { o.Bone ?? 0, 0, 0, 0 }).ToArray();
                    var weights = p.Weights?.ToArray() ?? Enumerable.Range(0, n).SelectMany(_ => new[] { 1f, 0, 0, 0 }).ToArray();
                    if (joints.Any(j => j >= boneCount)) throw new InvalidDataException($"bone index past the rig's {boneCount}");
                    attributes["JOINTS_0"] = g.Accessor(MemoryMarshal.AsBytes(joints.AsSpan()), Gltf.UnsignedShort, n, "VEC4", Gltf.ArrayBuffer);
                    attributes["WEIGHTS_0"] = g.AccessorF32(weights, "VEC4", 4, Gltf.ArrayBuffer);
                }
                var indices = g.Accessor(MemoryMarshal.AsBytes(p.Indices.ToArray().AsSpan()), Gltf.UnsignedInt, p.Indices.Count, "SCALAR", Gltf.ElementArrayBuffer);

                var pbr = new JsonObject { ["baseColorFactor"] = Gltf.Floats([1, 1, 1, 1]), ["metallicFactor"] = 0.0, ["roughnessFactor"] = 1.0 };
                if (p.Texture is { } id && byId.TryGetValue(id, out var img))
                {
                    if (!images.TryGetValue(id, out var source))
                    {
                        var view = g.View(Png.Encode(img), null);
                        g.Images.Add(new JsonObject { ["name"] = $"{id:x8}", ["mimeType"] = "image/png", ["bufferView"] = view });
                        images[id] = source = g.Images.Count - 1;
                    }
                    // NUD wrap modes: 1 repeat, 2 mirror, 3 clamp.
                    static int Wrap(byte w) => w switch { 2 => 33648, 3 => 33071, _ => 10497 };
                    var sampler = Gltf.Intern(g.Samplers, new JsonObject { ["magFilter"] = 9729, ["minFilter"] = 9729, ["wrapS"] = Wrap(p.WrapS), ["wrapT"] = Wrap(p.WrapT) });
                    var texture = Gltf.Intern(g.Textures, new JsonObject { ["sampler"] = sampler, ["source"] = source });
                    pbr["baseColorTexture"] = new JsonObject { ["index"] = texture, ["texCoord"] = 0 };
                }
                var (mode, cutoff) = AlphaMode(p);
                var material = new JsonObject
                {
                    ["name"] = $"{o.Name}-{primitives.Count}",
                    ["alphaMode"] = mode,
                    ["doubleSided"] = p.DoubleSided,
                    ["pbrMetallicRoughness"] = pbr,
                    ["extras"] = new JsonObject
                    {
                        ["nudFlags"] = p.Flags,
                        ["outline"] = p.Outline,
                        ["blend"] = new JsonArray(p.SrcBlend, p.DstBlend),
                        ["alphaTest"] = p.AlphaRef is { } aref ? new JsonArray(p.AlphaFunc, aref) : null,
                    },
                };
                if (cutoff is { } cut) material["alphaCutoff"] = (double)cut;
                g.Materials.Add(material);
                primitives.Add(new JsonObject { ["attributes"] = attributes, ["indices"] = indices, ["material"] = g.Materials.Count - 1 });
            }
            if (primitives.Count == 0) continue;
            g.Meshes.Add(new JsonObject { ["name"] = o.Name, ["primitives"] = primitives });
            var node = new JsonObject { ["name"] = o.Name, ["mesh"] = g.Meshes.Count - 1 };
            if (rig is not null) node["skin"] = 0;
            g.Nodes.Add(node);
            nodes.Add(g.Nodes.Count - 1);
        }
        if (nodes.Count == 0) throw new InvalidDataException("no meshes");
        if (rig is var (clipBones, _) && clip is var (bytes, slots))
            DonRig.Clip(g, clipBones, bytes, slots, "loop");
        return g.Finish(nodes);
    }

    /// <summary><c>don3d/&lt;folder&gt;</c> → <c>game_data/&lt;era&gt;/&lt;folder&gt;</c>: models (<c>.nud</c> + <c>.nut</c> →
    /// <c>.glb</c>) and textures (<c>.nut</c> → <c>.png</c>).</summary>
    private static readonly (string Dir, string Name, bool Model)[] Folders =
    [
        ("parts/body", "body", true), ("parts/head", "head", true), ("full/cos", "cos", true), ("parts/acc", "acce", true),
        ("parts/paint", "paint", false), ("full/face", "face", false),
    ];

    public static IEnumerable<Rule> Inputs() => Folders.Select(f => Rule.Files($"don3d/{f.Dir}/"));

    public static List<Report> Convert(Era era, GameSource src, string outDir)
    {
        var reports = new List<Report>();
        // Without the bind pose the models are written unskinned, in rest pose.
        var bones = DonRig.Rig();
        (List<Bone>, BonePose[])? rig = src.Exists(DonRig.Bind) ? (bones, DonRig.Pose(bones, DonRig.Frames(src.Read(DonRig.Bind), DonRig.DonSlots)[0].Span)) : null;
        foreach (var (dir, name, model) in Folders)
        {
            var prefix = $"don3d/{dir}/";
            var files = src.Files(p => p.StartsWith(prefix, StringComparison.Ordinal) && !p[prefix.Length..].Contains('/')
                && p.EndsWith(model ? ".nud" : ".nut", StringComparison.Ordinal)).Order(StringComparer.Ordinal).ToList();
            if (files.Count == 0) continue;
            var report = new Report($"{era.Name()} {name} {(model ? "models" : "textures")}");
            var dstDir = Path.Combine(outDir, name);
            Directory.CreateDirectory(dstDir);
            foreach (var rel in files)
            {
                var stem = rel[prefix.Length..^4];
                var dst = Path.Combine(dstDir, $"{stem}.{(model ? "glb" : "png")}");
                if (src.IsFresh(dst, rel))
                {
                    report.UpToDate.Add(stem);
                    continue;
                }
                try
                {
                    if (model)
                    {
                        var nut = rel.Replace(".nud", ".nut");
                        var textures = src.Exists(nut) ? Ntp3.Decode(src.Read(nut)) : [];
                        var parsed = Parse(src.Read(rel));
                        // A puchi has its own two-bone rig (the don rig's first two) and its idle loop beside it.
                        var (bind, looped) = (rel.Replace(".nud", "_bind.bin"), rel.Replace(".nud", "_loop.bin"));
                        byte[] glb;
                        if (dir == "parts/acc" && src.Exists(bind))
                        {
                            var puchi = DonRig.Rig().Take(2).ToList();
                            var pose = DonRig.Pose(puchi, DonRig.Frames(src.Read(bind), DonRig.PuchiSlots)[0].Span);
                            glb = Glb(parsed, textures, (puchi, pose), src.Exists(looped) ? (src.Read(looped), DonRig.PuchiSlots) : null);
                        }
                        else
                        {
                            glb = Glb(parsed, textures, rig, null);
                        }
                        File.WriteAllBytes(dst, glb);
                    }
                    else
                    {
                        // A face holds its 12 expressions as 12 textures: stacked top to bottom, as Nijiiro's atlases are.
                        var images = Ntp3.Decode(src.Read(rel)).Select(t => t.Image).ToList();
                        if (images.Count == 0) throw new InvalidDataException("no texture");
                        RgbaImage.Stack(images).SavePng(dst);
                    }
                    report.Written.Add(stem);
                }
                catch (InvalidDataException e)
                {
                    report.Failed.Add($"{stem}: {e.Message}");
                }
            }
            reports.Add(report);
        }
        return reports;
    }
}
