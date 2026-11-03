using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using TaikoLocalServer.Infrastructure.Extract.Models;

namespace TaikoLocalServer.Infrastructure.Extract.Nijiiro;

/// <summary>
/// The game's <c>animation/*.nuanmb</c> (SSBH ANIM v1.2) to one animation-library GLB that <c>don-chan-previewer</c> loads,
/// plus <c>facekeys.json</c> with the face-expression changes. The rig comes from the game's own <c>.nusktb</c>.
/// </summary>
public static class NijiiroAnims
{
    /// <summary>Output names under <c>game_data/animation/</c>.</summary>
    public const string Library = "don.glb", FaceKeys = "facekeys.json";
    /// <summary>The game animates at 60 fps; ANIM v1.2 keys are frame indices.</summary>
    private const float Fps = 60;
    /// <summary>Any model's skeleton works (they share the don rig); the default body is always present.</summary>
    private const string RigModel = "body_000000";

    private sealed class NodeAnim
    {
        public required Vector3[] T, S;
        public required Quaternion[] R;
        public bool CompensateScale;
    }

    public static void Convert(NijiiroGame game, string dstDir, Report report)
    {
        var rig = game.File($"model/body/{RigModel}/{RigModel}/{RigModel}.numdlb");
        if (game.List("animation") is not { } entries)
        {
            report.Missing.Add(game.File("animation"));
            return;
        }
        if (!File.Exists(rig))
        {
            report.Missing.Add(rig);
            return;
        }
        var paths = entries.Where(p => p.EndsWith(".nuanmb", StringComparison.Ordinal)).ToList();
        Directory.CreateDirectory(dstDir);
        var (glbPath, keysPath) = (Path.Combine(dstDir, Library), Path.Combine(dstDir, FaceKeys));
        if (paths.Append(rig).All(src => NijiiroGame.IsNewer(glbPath, src) && NijiiroGame.IsNewer(keysPath, src)))
        {
            report.UpToDate.AddRange(paths.Select(Path.GetFileNameWithoutExtension)!);
            return;
        }
        var modl = Modl.Read(File.ReadAllBytes(rig));
        var bones = Skel.Read(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(rig)!, modl.SkeletonFileName)));
        var g = new Gltf();
        var roots = NijiiroModels.PushBones(g, bones);
        var index = new Dictionary<string, int>();
        for (var i = 0; i < bones.Count; i++) index.TryAdd(bones[i].Name, i);
        var restScale = bones.Select(b => NijiiroModels.Decompose(b.Transform).Scale).ToArray();

        var faceKeys = new JsonObject();
        foreach (var path in paths)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            try
            {
                var (animation, keys) = Clip(g, bones, index, restScale, File.ReadAllBytes(path), name);
                if (keys is not null) faceKeys[name] = keys;
                if (animation is not null) g.Animations.Add(animation);
                report.Written.Add(name);
            }
            catch (InvalidDataException e)
            {
                report.Failed.Add($"{name}: {e.Message}");
            }
        }
        File.WriteAllBytes(glbPath, g.Finish(roots));
        File.WriteAllText(keysPath, new JsonObject { ["animations"] = faceKeys }.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    /// <summary>One <c>.nuanmb</c> as a glTF animation (sampled once per frame, LINEAR) and its face keys.</summary>
    private static (JsonObject? Animation, JsonObject? FaceKeys) Clip(Gltf g, List<SkelBone> bones, Dictionary<string, int> index, Vector3[] restScale, byte[] bytes, string name)
    {
        var s = Ssbh.Open(bytes, "MINA", 1, 2);
        const int b = Ssbh.Body; // name, f32 ?, final frame (always 60), f32 last frame, f32 ?, tracks, buffers
        // Keys run 0..=last frame.
        var frameCount = (int)MathF.Round(s.F32(b + 16), MidpointRounding.AwayFromZero) + 1;
        var buffers = s.Array(b + 40, 16).ToList();
        var nodes = new SortedDictionary<int, NodeAnim>();
        JsonObject? faceKeys = null;
        // Track: name, u64 type (0 transform), properties (name, u64 buffer index).
        foreach (var track in s.Array(b + 24, 32).Where(t => s.U64(t + 8) == 0))
        {
            var bone = s.String(track);
            Curve? scale = null, rotate = null, translate = null;
            var compensate = false;
            foreach (var p in s.Array(track + 16, 16))
            {
                var bi = s.U64(p + 8);
                if (bi >= (ulong)buffers.Count) continue;
                var buffer = s.Bytes(buffers[(int)bi]);
                var property = s.String(p);
                Curve Decode(ReadOnlySpan<byte> data)
                {
                    try { return Curve.Decode(data); }
                    catch (InvalidDataException e) { throw new InvalidDataException($"{bone}.{property}: {e.Message}", e); }
                }
                switch (property)
                {
                    case "Scale": scale = Decode(buffer); break;
                    case "Rotate": rotate = Decode(buffer); break;
                    case "Translate": translate = Decode(buffer); break;
                    case "CompensateScale":
                        try { compensate = Curve.Decode(buffer).Sample(0)[0] != 0; }
                        catch (InvalidDataException) { compensate = false; }
                        break;
                    // Expression row in the 12-row face atlas, held until the next key.
                    case "nfCustomInteger0":
                    {
                        var curve = Decode(buffer);
                        var keys = new JsonArray();
                        int? last = null;
                        for (var f = 0; f < frameCount; f++)
                        {
                            var face = (int)Math.Max(MathF.Round(curve.Sample(f)[0], MidpointRounding.AwayFromZero), 0);
                            if (last == face) continue;
                            keys.Add(new JsonObject { ["time"] = (double)(f / Fps), ["faceIndex"] = face });
                            last = face;
                        }
                        faceKeys = new JsonObject { ["duration"] = (double)((frameCount - 1) / Fps), ["frameCount"] = 12, ["keys"] = keys };
                        break;
                    }
                }
            }
            if (!index.TryGetValue(bone, out var node)) continue;
            Vector3 V3(Curve? c, int f, Vector3 fallback) => c is null ? fallback : new Vector3(c.Sample(f)[0], c.Sample(f)[1], c.Sample(f)[2]);
            nodes[node] = new NodeAnim
            {
                S = Enumerable.Range(0, frameCount).Select(f => V3(scale, f, Vector3.One)).ToArray(),
                R = Enumerable.Range(0, frameCount).Select(f => rotate is null ? Quaternion.Identity
                    : Quaternion.Normalize(new Quaternion(rotate.Sample(f)[0], rotate.Sample(f)[1], rotate.Sample(f)[2], rotate.Sample(f)[3]))).ToArray(),
                T = Enumerable.Range(0, frameCount).Select(f => V3(translate, f, Vector3.Zero)).ToArray(),
                CompensateScale = compensate,
            };
        }

        // CompensateScale (Maya segment scale compensate): a bone doesn't inherit its parent's scale, but its translation
        // is still in the parent's scaled space. three.js always inherits scale, so divide it back out.
        var rawScale = nodes.ToDictionary(n => n.Key, n => (Vector3[])n.Value.S.Clone());
        foreach (var (i, anim) in nodes)
        {
            if (!anim.CompensateScale || bones[i].Parent is not { } p) continue;
            for (var f = 0; f < anim.S.Length; f++)
                anim.S[f] /= rawScale.TryGetValue(p, out var ps) && f < ps.Length ? ps[f] : restScale[p];
        }

        if (nodes.Count == 0) return (null, faceKeys);
        var times = Enumerable.Range(0, frameCount).Select(f => f / Fps).ToArray();
        var input = g.AccessorF32(times, "SCALAR", 1, null, ([times[0]], [times[^1]]));
        var (samplers, channels) = (new JsonArray(), new JsonArray());
        foreach (var (node, anim) in nodes)
        {
            foreach (var (target, type, width, values) in new[]
            {
                ("translation", "VEC3", 3, anim.T.SelectMany(v => new[] { v.X, v.Y, v.Z }).ToArray()),
                ("rotation", "VEC4", 4, anim.R.SelectMany(q => new[] { q.X, q.Y, q.Z, q.W }).ToArray()),
                ("scale", "VEC3", 3, anim.S.SelectMany(v => new[] { v.X, v.Y, v.Z }).ToArray()),
            })
            {
                var output = g.AccessorF32(values, type, width, null);
                channels.Add(new JsonObject { ["sampler"] = samplers.Count, ["target"] = new JsonObject { ["node"] = node, ["path"] = target } });
                samplers.Add(new JsonObject { ["input"] = input, ["output"] = output, ["interpolation"] = "LINEAR" });
            }
        }
        return (new JsonObject { ["name"] = name, ["samplers"] = samplers, ["channels"] = channels }, faceKeys);
    }
}
