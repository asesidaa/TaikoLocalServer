using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using TaikoLocalServer.Infrastructure.Extract.Models;

namespace TaikoLocalServer.Infrastructure.Extract.Ac15;

/// <summary>A rig bone: its translation, rotation and scale slots in a frame (a bone without one keeps identity there).</summary>
public sealed record Bone(string Name, int? Parent, int? T, int? R, int? S);

/// <summary>A bone's local transform.</summary>
public readonly record struct BonePose(Vector3 T, Quaternion R, Vector3 S);

/// <summary>
/// The older games' don-chan rig and animations (<c>don3d/ani/*.bin</c>, reverse-engineered; see tools/extract's README).
/// A file is a big-endian f32 frame count, then that many frames of 98 vec3s (1,176 bytes). Every model shares one
/// 39-bone rig, so the slots are fixed: bone 0 (translation, rotation), bone 1 the body (translation, rotation, scale),
/// four limbs of nine bones from slot 5 (23 slots each), and slot 97, whose x is the face expression. Rotations are
/// Euler angles in radians. <c>don_bind.bin</c> is the bind pose (its first frame).
/// </summary>
public static class DonRig
{
    public const string Bind = "don3d/ani/don_bind.bin";
    /// <summary>Vec3 slots per frame: the don rig's, and a puchi's (<c>parts/acc/*_{bind,loop}.bin</c>: the first two bones only).</summary>
    public const int DonSlots = 98, PuchiSlots = 5;
    private const float Fps = 60;
    /// <summary>Expressions in a face atlas (<c>full/face/*.nut</c> holds 12 textures).</summary>
    private const int FaceRows = 12;

    /// <summary>The rig, in the bone-index order NUD vertices use. In a limb, bones 1–5 chain along +x from the limb root
    /// (whose translation is in model space, so it hangs off bone 0), bone 6 copies bone 5, bone 7 copies the root and
    /// bone 8 sits at the chain's length along bone 7's x: the IK target. Bone 38 has no slots (slot 97 is the face).</summary>
    public static List<Bone> Rig()
    {
        var bones = new List<Bone> { new("root", null, 0, 1, null), new("body", 0, 2, 3, 4) };
        for (var limb = 0; limb < 4; limb++)
        {
            var (s, b) = (5 + limb * 23, 2 + limb * 9);
            string Name(int k) => $"limb{limb + 1}_{k}";
            bones.Add(new(Name(0), 0, s, s + 1, s + 2));
            for (var k = 1; k < 6; k++)
                bones.Add(new(Name(k), b + k - 1, s + k * 3, s + k * 3 + 1, s + k * 3 + 2));
            bones.Add(new(Name(6), b + 4, s + 18, s + 19, null));
            bones.Add(new(Name(7), 0, s + 20, s + 21, null));
            bones.Add(new(Name(8), b + 7, s + 22, null, null));
        }
        bones.Add(new("face", 1, null, null, null));
        return bones;
    }

    /// <summary>The frames of an animation file.</summary>
    public static List<ReadOnlyMemory<byte>> Frames(byte[] b, int slots)
    {
        var frame = slots * 12;
        if (b.Length < 4 || (b.Length - 4) % frame != 0)
            throw new InvalidDataException($"not a don animation ({b.Length} bytes)");
        var n = (b.Length - 4) / frame;
        if ((int)BinaryPrimitives.ReadSingleBigEndian(b) != n)
            throw new InvalidDataException("frame count doesn't match the size");
        return Enumerable.Range(0, n).Select(k => new ReadOnlyMemory<byte>(b, 4 + k * frame, frame)).ToList();
    }

    private static Vector3 Slot(ReadOnlySpan<byte> frame, int s) => new(
        BinaryPrimitives.ReadSingleBigEndian(frame[(s * 12)..]),
        BinaryPrimitives.ReadSingleBigEndian(frame[(s * 12 + 4)..]),
        BinaryPrimitives.ReadSingleBigEndian(frame[(s * 12 + 8)..]));

    /// <summary>Each bone's local translation, rotation (Z·Y·X, as Smash Forge reads VBN Euler angles) and scale.</summary>
    public static BonePose[] Pose(List<Bone> rig, ReadOnlySpan<byte> frame)
    {
        var pose = new BonePose[rig.Count];
        for (var i = 0; i < rig.Count; i++)
        {
            var b = rig[i];
            var r = Quaternion.Identity;
            if (b.R is { } ri)
            {
                var e = Slot(frame, ri);
                r = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, e.Z) * Quaternion.CreateFromAxisAngle(Vector3.UnitY, e.Y)
                    * Quaternion.CreateFromAxisAngle(Vector3.UnitX, e.X);
            }
            pose[i] = new BonePose(b.T is { } t ? Slot(frame, t) : Vector3.Zero, r, b.S is { } s ? Slot(frame, s) : Vector3.One);
        }
        return pose;
    }

    private static int Face(ReadOnlySpan<byte> frame) => (int)Math.Max(MathF.Round(Slot(frame, 97).X, MidpointRounding.AwayFromZero), 0);

    /// <summary>Adds the rig as nodes <c>0..rig.Count</c> in <paramref name="pose"/>, and returns the root's index.</summary>
    public static int PushBones(Gltf g, List<Bone> rig, BonePose[] pose)
    {
        for (var i = 0; i < rig.Count; i++)
        {
            var (t, r, s) = (pose[i].T, pose[i].R, pose[i].S);
            var node = new JsonObject
            {
                ["name"] = rig[i].Name,
                ["translation"] = Gltf.Floats([t.X, t.Y, t.Z]),
                ["rotation"] = Gltf.Floats([r.X, r.Y, r.Z, r.W]),
                ["scale"] = Gltf.Floats([s.X, s.Y, s.Z]),
            };
            var children = Enumerable.Range(0, rig.Count).Where(c => rig[c].Parent == i).ToArray();
            if (children.Length > 0) node["children"] = new JsonArray(children.Select(c => (JsonNode)c).ToArray());
            g.Nodes.Add(node);
        }
        return 0;
    }

    /// <summary>Model-space bone matrices (System.Numerics' row-vector convention: local × parent).</summary>
    public static Matrix4x4[] World(List<Bone> rig, BonePose[] pose)
    {
        var world = new Matrix4x4[rig.Count];
        for (var i = 0; i < rig.Count; i++)
        {
            var local = Matrix4x4.CreateScale(pose[i].S) * Matrix4x4.CreateFromQuaternion(pose[i].R) * Matrix4x4.CreateTranslation(pose[i].T);
            world[i] = rig[i].Parent is { } p ? local * world[p] : local;
        }
        return world;
    }

    /// <summary>Adds the skin over nodes <c>0..rig.Count</c>, with inverse bind matrices from <paramref name="bind"/>.</summary>
    public static void PushSkin(Gltf g, List<Bone> rig, BonePose[] bind)
    {
        // A row-vector matrix's rows are a column-vector matrix's columns: glTF's column-major order.
        var inverse = World(rig, bind).SelectMany(m => Matrix4x4.Invert(m, out var inv) ? Elements(inv) : Elements(Matrix4x4.Identity)).ToArray();
        var accessor = g.AccessorF32(inverse, "MAT4", 16, null);
        g.Skins.Add(new JsonObject
        {
            ["joints"] = new JsonArray(Enumerable.Range(0, rig.Count).Select(i => (JsonNode)i).ToArray()),
            ["inverseBindMatrices"] = accessor,
            ["skeleton"] = 0,
        });
    }

    private static float[] Elements(Matrix4x4 m) =>
        [m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44];

    /// <summary><c>game_data/&lt;era&gt;/animation/don.glb</c> (the rig and one clip per file, sampled per frame at 60 fps,
    /// LINEAR) and <c>facekeys.json</c> (the face expression over time), in the shapes Nijiiro's <c>animation/</c> uses.</summary>
    public static Report Convert(Era era, GameSource src, string outDir)
    {
        var report = new Report($"{era.Name()} animations");
        var files = src.Files(p => p.StartsWith("don3d/ani/", StringComparison.Ordinal) && !p["don3d/ani/".Length..].Contains('/')
            && p.EndsWith(".bin", StringComparison.Ordinal)).Order(StringComparer.Ordinal).ToList();
        if (!src.Exists(Bind)) return report;
        var (glbPath, keysPath) = (Path.Combine(outDir, "don.glb"), Path.Combine(outDir, "facekeys.json"));
        static string Name(string rel) => rel["don3d/ani/".Length..^".bin".Length];
        if (files.All(f => src.IsFresh(glbPath, f) && src.IsFresh(keysPath, f)))
        {
            report.UpToDate.AddRange(files.Where(f => f != Bind).Select(Name));
            return report;
        }
        var rig = Rig();
        var g = new Gltf();
        var root = PushBones(g, rig, Pose(rig, Frames(src.Read(Bind), DonSlots)[0].Span));
        var faces = new JsonObject();
        foreach (var rel in files.Where(f => f != Bind))
        {
            try
            {
                faces[Name(rel)] = Clip(g, rig, src.Read(rel), DonSlots, Name(rel));
                report.Written.Add(Name(rel));
            }
            catch (InvalidDataException e)
            {
                report.Failed.Add($"{Name(rel)}: {e.Message}");
            }
        }
        Directory.CreateDirectory(outDir);
        File.WriteAllBytes(glbPath, g.Finish([root]));
        File.WriteAllText(keysPath, new JsonObject { ["animations"] = faces }.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        return report;
    }

    /// <summary>Adds <paramref name="b"/> as the animation <paramref name="name"/>, and returns its face keys.</summary>
    public static JsonObject Clip(Gltf g, List<Bone> rig, byte[] b, int slots, string name)
    {
        var frames = Frames(b, slots);
        var poses = frames.Select(f => Pose(rig, f.Span)).ToList();
        var times = Enumerable.Range(0, frames.Count).Select(f => f / Fps).ToArray();
        var input = g.AccessorF32(times, "SCALAR", 1, null, ([times[0]], [times[^1]]));
        var (samplers, channels) = (new JsonArray(), new JsonArray());
        for (var i = 0; i < rig.Count; i++)
        {
            var tracks = new (string Path, string Type, Func<BonePose, float[]> Values, bool Present)[]
            {
                ("translation", "VEC3", p => [p.T.X, p.T.Y, p.T.Z], rig[i].T is not null),
                ("rotation", "VEC4", p => [p.R.X, p.R.Y, p.R.Z, p.R.W], rig[i].R is not null),
                ("scale", "VEC3", p => [p.S.X, p.S.Y, p.S.Z], rig[i].S is not null),
            };
            foreach (var (path, type, values, present) in tracks)
            {
                if (!present) continue;
                var output = g.AccessorF32(poses.SelectMany(p => values(p[i])).ToArray(), type, type == "VEC4" ? 4 : 3, null);
                channels.Add(new JsonObject { ["sampler"] = samplers.Count, ["target"] = new JsonObject { ["node"] = i, ["path"] = path } });
                samplers.Add(new JsonObject { ["input"] = input, ["output"] = output, ["interpolation"] = "LINEAR" });
            }
        }
        g.Animations.Add(new JsonObject { ["name"] = name, ["samplers"] = samplers, ["channels"] = channels });
        var keys = new JsonArray();
        int? last = null;
        for (var f = 0; f < frames.Count; f++)
        {
            var face = slots > 97 ? Face(frames[f].Span) : 0;
            if (last == face) continue;
            keys.Add(new JsonObject { ["time"] = (double)(f / Fps), ["faceIndex"] = face });
            last = face;
        }
        return new JsonObject { ["duration"] = (double)((frames.Count - 1) / Fps), ["frameCount"] = FaceRows, ["keys"] = keys };
    }
}
