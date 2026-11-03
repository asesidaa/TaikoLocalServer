using System.Numerics;
using System.Text.Json.Nodes;
using TaikoLocalServer.Infrastructure.Extract.Models;
using TaikoLocalServer.Infrastructure.Extract.Textures;

namespace TaikoLocalServer.Infrastructure.Extract.Nijiiro;

/// <summary>
/// Nijiiro don-chan models (MODL + MESH + SKEL + MATL) to binary glTF, in the layout <c>don-chan-previewer</c>'s
/// <c>normalize.ts</c> was written against: one node per bone (skeleton order), one skinned mesh node per MESH object
/// named <c>&lt;object&gt;-&lt;subindex&gt;</c>, one material per MATL label (named verbatim: flags are parsed out of it), PNG
/// base textures named after their material, and <c>extras.shaderType</c> from the <c>__nust__</c> MATL.
/// </summary>
public static class NijiiroModels
{
    /// <summary>Converts every model under <paramref name="rel"/> (<c>model/&lt;category&gt;</c>) to
    /// <c>&lt;dstDir&gt;/&lt;name&gt;.glb</c>. A model folder normally holds one <c>&lt;name&gt;/&lt;name&gt;.numdlb</c>, but can hold
    /// variants (<c>head_102001</c>). A modded model replaces the whole folder, textures included.</summary>
    public static void ConvertCategory(NijiiroGame game, string rel, string dstDir, Report report)
    {
        if (game.List(rel) is not { } entries)
        {
            report.Missing.Add(game.File(rel));
            return;
        }
        Directory.CreateDirectory(dstDir);
        var subs = entries.Where(Directory.Exists)
            .SelectMany(modelDir => Directory.EnumerateFileSystemEntries(modelDir).Order(StringComparer.Ordinal))
            .Where(sub => File.Exists(Path.Combine(sub, $"{Path.GetFileName(sub)}.numdlb"))).ToList();
        string Dst(string sub) => Path.Combine(dstDir, $"{Path.GetFileName(sub)}.glb");
        // Up to date only if newer than every file of the model (mesh, skeleton, materials and their textures), so a
        // changed texture (a mod, say) rebuilds it too.
        report.Each(subs, Path.GetFileName, sub => NewerThanFolder(Dst(sub), sub),
            sub => File.WriteAllBytes(Dst(sub), BuildGlb(sub, Path.GetFileName(sub))));
    }

    private static bool NewerThanFolder(string dst, string dir)
    {
        if (!File.Exists(dst)) return false;
        var built = File.GetLastWriteTimeUtc(dst);
        return Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).All(f => File.GetLastWriteTimeUtc(f) <= built);
    }

    private static byte[] Read(string path)
    {
        try
        {
            return File.ReadAllBytes(path);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            throw new InvalidDataException($"reading {path}: {e.Message}", e);
        }
    }

    /// <summary>Builds the <c>.glb</c> for the model whose <c>&lt;name&gt;.numdlb</c> is in <paramref name="dir"/>.</summary>
    public static byte[] BuildGlb(string dir, string name)
    {
        var modl = Modl.Read(Read(Path.Combine(dir, $"{name}.numdlb")));
        var objects = Mesh.Read(Read(Path.Combine(dir, modl.MeshFileName)));
        var bones = Skel.Read(Read(Path.Combine(dir, modl.SkeletonFileName)));

        // The maya MATL carries the DiffuseMap names, the nust MATL the shader labels.
        var sources = new Dictionary<string, (string? Texture, string? Shader)>();
        foreach (var file in modl.MaterialFileNames)
        {
            var path = Path.Combine(dir, file.StartsWith("./", StringComparison.Ordinal) ? file[2..] : file);
            foreach (var (label, shader, diffuse) in Matl.Read(Read(path)))
            {
                var src = sources.GetValueOrDefault(label);
                if (shader.Length > 0) src.Shader = shader;
                if (diffuse is not null) src.Texture = Path.Combine(Path.GetDirectoryName(path)!, $"{diffuse}.nutexb");
                sources[label] = src;
            }
        }

        var g = new Gltf();
        // Bones: node i is bone i, so the skin's joints are 0..n.
        var boneIndex = new Dictionary<string, int>();
        for (var i = 0; i < bones.Count; i++) boneIndex.TryAdd(bones[i].Name, i);
        var roots = PushBones(g, bones);
        if (bones.Count > 0)
        {
            var inverse = new float[bones.Count * 16];
            for (var i = 0; i < bones.Count; i++)
            {
                // Zero-scale (hidden) bones aren't invertible; identity matches numd-exporter.
                var ibm = Matrix4x4.Invert(Skel.World(bones, i), out var inv) && Finite(inv) ? inv : Matrix4x4.Identity;
                // glTF requires the bottom row to be exactly 0,0,0,1; f32 inversion leaves it at 0.9999999.
                (ibm.M14, ibm.M24, ibm.M34, ibm.M44) = (0, 0, 0, 1);
                Elements(ibm).CopyTo(inverse, i * 16);
            }
            var accessor = g.AccessorF32(inverse, "MAT4", 16, null);
            g.Skins.Add(new JsonObject { ["joints"] = new JsonArray(Enumerable.Range(0, bones.Count).Select(i => (JsonNode)i).ToArray()), ["inverseBindMatrices"] = accessor });
        }

        var materials = new Dictionary<string, int>();
        var textureViews = new Dictionary<string, (int View, bool Alpha)>();
        foreach (var o in objects)
        {
            int? material = null;
            var entry = modl.Entries.FirstOrDefault(e => e.Object == o.Name && e.Subindex == o.Subindex);
            if (entry.Material is { } label)
            {
                if (!materials.TryGetValue(label, out var m))
                    materials[label] = m = Material(g, label, sources.TryGetValue(label, out var src) ? src : default, textureViews);
                material = m;
            }
            var skinned = bones.Count > 0 && o.Influences.Count > 0;
            var mesh = MeshPrimitive(g, o, material, skinned ? boneIndex : null);
            var nodeIndex = g.Nodes.Count;
            var node = new JsonObject { ["name"] = $"{o.Name}-{o.Subindex}", ["mesh"] = mesh };
            if (skinned)
            {
                node["skin"] = 0;
                roots.Add(nodeIndex);
            }
            else if (boneIndex.TryGetValue(o.ParentBone, out var parent))
            {
                // Rigid mesh: rides along with its parent bone.
                var bone = g.Nodes[parent]!.AsObject();
                if (bone["children"] is JsonArray children) children.Add(nodeIndex);
                else bone["children"] = new JsonArray(nodeIndex);
            }
            else
            {
                roots.Add(nodeIndex);
            }
            g.Nodes.Add(node);
        }
        return g.Finish(roots);
    }

    private static bool Finite(Matrix4x4 m) => Elements(m).All(float.IsFinite);

    /// <summary>Column-major elements (System.Numerics' row-vector layout, row by row).</summary>
    public static float[] Elements(Matrix4x4 m) =>
        [m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44];

    /// <summary>Appends one node per bone (node index = bone index) with its rest TRS; returns the root bones.</summary>
    public static List<int> PushBones(Gltf g, List<SkelBone> bones)
    {
        var roots = new List<int>();
        for (var i = 0; i < bones.Count; i++)
        {
            if (bones[i].Parent is null) roots.Add(i);
            var (s, r, t) = Decompose(bones[i].Transform);
            var node = new JsonObject
            {
                ["name"] = bones[i].Name,
                ["translation"] = Gltf.Floats([t.X, t.Y, t.Z]),
                ["rotation"] = Gltf.Floats([r.X, r.Y, r.Z, r.W]),
                ["scale"] = Gltf.Floats([s.X, s.Y, s.Z]),
            };
            var children = Enumerable.Range(0, bones.Count).Where(c => bones[c].Parent == i).ToArray();
            if (children.Length > 0) node["children"] = new JsonArray(children.Select(c => (JsonNode)c).ToArray());
            g.Nodes.Add(node);
        }
        return roots;
    }

    /// <summary>Scale, rotation and translation of a bone matrix, as glam computes them (DirectXMath's quaternion from a
    /// rotation matrix). Some costumes (<c>head_003000</c>, no arms or legs) hide bones with a zero-scale basis; their
    /// rotation is meaningless, so it's identity.</summary>
    public static (Vector3 Scale, Quaternion Rotation, Vector3 Translation) Decompose(Matrix4x4 m)
    {
        // Rows of a row-vector matrix are the columns (axes) of a column-vector one.
        var (x, y, z) = (new Vector4(m.M11, m.M12, m.M13, m.M14), new Vector4(m.M21, m.M22, m.M23, m.M24), new Vector4(m.M31, m.M32, m.M33, m.M34));
        var t = new Vector3(m.M41, m.M42, m.M43);
        var scale = new Vector3(x.Length() * MathF.Sign(m.GetDeterminant()), y.Length(), z.Length());
        var (ax, ay, az) = (Xyz(x / scale.X), Xyz(y / scale.Y), Xyz(z / scale.Z));
        var r = FromAxes(ax, ay, az);
        if (!(float.IsFinite(r.X) && float.IsFinite(r.Y) && float.IsFinite(r.Z) && float.IsFinite(r.W)))
            return (new Vector3(Xyz(x).Length(), Xyz(y).Length(), Xyz(z).Length()), Quaternion.Identity, t);
        return (scale, r, t);
    }

    private static Vector3 Xyz(Vector4 v) => new(v.X, v.Y, v.Z);

    private static Quaternion FromAxes(Vector3 x, Vector3 y, Vector3 z)
    {
        var (m00, m01, m02, m10, m11, m12, m20, m21, m22) = (x.X, x.Y, x.Z, y.X, y.Y, y.Z, z.X, z.Y, z.Z);
        if (m22 <= 0)
        {
            var (dif10, omm22) = (m11 - m00, 1 - m22);
            if (dif10 <= 0)
            {
                var four = omm22 - dif10;
                var inv = 0.5f / MathF.Sqrt(four);
                return new Quaternion(four * inv, (m01 + m10) * inv, (m02 + m20) * inv, (m12 - m21) * inv);
            }
            else
            {
                var four = omm22 + dif10;
                var inv = 0.5f / MathF.Sqrt(four);
                return new Quaternion((m01 + m10) * inv, four * inv, (m12 + m21) * inv, (m20 - m02) * inv);
            }
        }
        var (sum10, opm22) = (m11 + m00, 1 + m22);
        if (sum10 <= 0)
        {
            var four = opm22 - sum10;
            var inv = 0.5f / MathF.Sqrt(four);
            return new Quaternion((m02 + m20) * inv, (m12 + m21) * inv, four * inv, (m01 - m10) * inv);
        }
        else
        {
            var four = opm22 + sum10;
            var inv = 0.5f / MathF.Sqrt(four);
            return new Quaternion((m12 - m21) * inv, (m20 - m02) * inv, (m01 - m10) * inv, four * inv);
        }
    }

    private static int MeshPrimitive(Gltf g, MeshObject o, int? material, Dictionary<string, int>? boneIndex)
    {
        var positions = o.Positions ?? throw new InvalidDataException("object has no positions");
        var count = positions.Length / 3;
        float[] min = [float.MaxValue, float.MaxValue, float.MaxValue], max = [float.MinValue, float.MinValue, float.MinValue];
        for (var v = 0; v < count; v++)
        for (var c = 0; c < 3; c++)
            (min[c], max[c]) = (MathF.Min(min[c], positions[v * 3 + c]), MathF.Max(max[c], positions[v * 3 + c]));
        var attributes = new JsonObject { ["POSITION"] = g.AccessorF32(positions, "VEC3", 3, Gltf.ArrayBuffer, (min, max)) };
        if (o.Normals is { } normals) attributes["NORMAL"] = g.AccessorF32(normals, "VEC3", 3, Gltf.ArrayBuffer);
        // Most objects carry Color1, a few Color0; the previewer reads whichever comes first as COLOR_0.
        if (o.Colors is { } colors) attributes["COLOR_0"] = g.AccessorF32(colors, "VEC4", 4, Gltf.ArrayBuffer);
        if (o.Uvs is { } uvs) attributes["TEXCOORD_0"] = g.AccessorF32(uvs, "VEC2", 2, Gltf.ArrayBuffer);
        if (boneIndex is not null)
        {
            var (joints, weights) = SkinAttributes(o, count, boneIndex);
            attributes["JOINTS_0"] = g.Accessor(System.Runtime.InteropServices.MemoryMarshal.AsBytes(joints.AsSpan()), Gltf.UnsignedShort, count, "VEC4", Gltf.ArrayBuffer);
            attributes["WEIGHTS_0"] = g.AccessorF32(weights, "VEC4", 4, Gltf.ArrayBuffer);
        }
        var indices = g.Accessor(System.Runtime.InteropServices.MemoryMarshal.AsBytes(o.Indices.AsSpan()), Gltf.UnsignedInt, o.Indices.Length, "SCALAR", Gltf.ElementArrayBuffer);
        var primitive = new JsonObject { ["attributes"] = attributes, ["indices"] = indices };
        if (material is { } m) primitive["material"] = m;
        g.Meshes.Add(new JsonObject { ["name"] = $"{o.Name}-{o.Subindex}", ["primitives"] = new JsonArray(primitive) });
        return g.Meshes.Count - 1;
    }

    /// <summary>Per-vertex top-4 joint influences, renormalized to sum to 1 as glTF requires.</summary>
    private static (ushort[] Joints, float[] Weights) SkinAttributes(MeshObject o, int count, Dictionary<string, int> boneIndex)
    {
        var perVertex = new List<(ushort Joint, float Weight)>[count];
        for (var i = 0; i < count; i++) perVertex[i] = [];
        foreach (var (bone, weights) in o.Influences)
        {
            if (!boneIndex.TryGetValue(bone, out var joint)) throw new InvalidDataException($"{o.Name} weights unknown bone {bone}");
            foreach (var (vertex, weight) in weights)
                if (vertex < count) perVertex[vertex].Add(((ushort)joint, weight));
        }
        var (joints, outWeights) = (new ushort[count * 4], new float[count * 4]);
        for (var v = 0; v < count; v++)
        {
            var top = perVertex[v].OrderByDescending(x => x.Weight).Take(4).ToList(); // stable, as Rust's sort_by
            var sum = 0f;
            foreach (var x in top) sum += x.Weight;
            for (var slot = 0; slot < 4; slot++)
            {
                var (j, w) = slot < top.Count ? top[slot] : ((ushort)0, 0f);
                joints[v * 4 + slot] = j;
                outWeights[v * 4 + slot] = sum > 0 ? w / sum : slot == 0 ? 1 : 0;
            }
        }
        return (joints, outWeights);
    }

    /// <summary>One material per MATL label, with its own image (named after the material, which <c>normalize.ts</c> reads);
    /// images of the same source texture share one PNG buffer view. <c>alphaMode</c> is BLEND exactly when the texture has
    /// any non-opaque pixel, matching <c>numd-exporter</c>.</summary>
    private static int Material(Gltf g, string label, (string? Texture, string? Shader) source, Dictionary<string, (int View, bool Alpha)> views)
    {
        var pbr = new JsonObject { ["baseColorFactor"] = Gltf.Floats([1, 1, 1, 1]), ["metallicFactor"] = 0.0, ["roughnessFactor"] = 1.0 };
        var blend = false;
        if (source.Texture is { } path && File.Exists(path))
        {
            if (!views.TryGetValue(path, out var view))
            {
                var img = Nutexb.Decode(Read(path));
                var alpha = false;
                for (var p = 3; p < img.Pixels.Length && !alpha; p += 4) alpha = img.Pixels[p] < 255;
                views[path] = view = (g.View(Png.Encode(img), null), alpha);
            }
            blend = view.Alpha;
            g.Images.Add(new JsonObject { ["name"] = label, ["mimeType"] = "image/png", ["bufferView"] = view.View });
            g.Textures.Add(new JsonObject { ["sampler"] = 0, ["source"] = g.Images.Count - 1 });
            pbr["baseColorTexture"] = new JsonObject { ["index"] = g.Textures.Count - 1, ["texCoord"] = 0 };
        }
        var m = new JsonObject
        {
            ["name"] = label,
            ["alphaMode"] = blend ? "BLEND" : "OPAQUE",
            ["doubleSided"] = false,
            ["pbrMetallicRoughness"] = pbr,
            ["emissiveFactor"] = Gltf.Floats([0, 0, 0]),
        };
        if (source.Shader is { } shader) m["extras"] = new JsonObject { ["shaderType"] = shader };
        g.Materials.Add(m);
        return g.Materials.Count - 1;
    }
}
