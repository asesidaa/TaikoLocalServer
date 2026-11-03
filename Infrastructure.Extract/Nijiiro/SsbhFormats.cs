using System.Numerics;

namespace TaikoLocalServer.Infrastructure.Extract.Nijiiro;

/// <summary>MODL 1.7 (<c>.numdlb</c>): which skeleton, mesh and materials make a model, and each mesh object's material.</summary>
public sealed record Modl(string SkeletonFileName, List<string> MaterialFileNames, string MeshFileName, List<(string Object, ulong Subindex, string Material)> Entries)
{
    public static Modl Read(byte[] bytes)
    {
        var s = Ssbh.Open(bytes, "LDOM", 1, 7);
        const int b = Ssbh.Body; // model name, skeleton, materials, animation, mesh, entries
        return new Modl(s.String(b + 8), s.Array(b + 16, 8).Select(s.String).ToList(), s.String(b + 40),
            s.Array(b + 48, 24).Select(e => (s.String(e), s.U64(e + 8), s.String(e + 16))).ToList());
    }
}

/// <summary>SKEL 1.0 (<c>.nusktb</c>): bones in order, each with its parent and local transform.</summary>
public sealed record SkelBone(string Name, int? Parent, Matrix4x4 Transform);

public static class Skel
{
    public static List<SkelBone> Read(byte[] bytes)
    {
        var s = Ssbh.Open(bytes, "LEKS", 1, 0);
        const int b = Ssbh.Body; // bones (name, u16 index, i16 parent, flags), world, inverse world, local, inverse local
        var transforms = s.Array(b + 48, 64).Select(s.Matrix).ToList();
        return s.Array(b, 16).Zip(transforms).Select(p =>
        {
            var parent = s.I16(p.First + 10);
            return new SkelBone(s.String(p.First), parent >= 0 ? parent : null, p.Second);
        }).ToList();
    }

    /// <summary>The bone's model-space transform: its local one times every ancestor's.</summary>
    public static Matrix4x4 World(List<SkelBone> bones, int index)
    {
        var transform = bones[index].Transform;
        var visited = new HashSet<int>();
        for (var bone = bones[index]; bone.Parent is { } p && p < bones.Count; bone = bones[p])
        {
            if (!visited.Add(p)) throw new InvalidDataException($"bone {p} is its own ancestor");
            transform *= bones[p].Transform;
        }
        return transform;
    }
}

/// <summary>MATL 1.5 (<c>.numatb</c>): per material label, its shader label and texture names.</summary>
public static class Matl
{
    private const ulong DiffuseMap = 30, StringParam = 11;

    /// <summary>Each entry's label, shader label and DiffuseMap texture name (null when it has none).</summary>
    public static List<(string Label, string Shader, string? Diffuse)> Read(byte[] bytes)
    {
        var s = Ssbh.Open(bytes, "LTAM", 1, 5);
        // Entry: label, attributes (u64 param id, pointer to the value, u64 value type), shader label.
        return s.Array(Ssbh.Body, 32).Select(e =>
        {
            string? diffuse = null;
            foreach (var a in s.Array(e + 8, 24))
                if (s.U64(a) == DiffuseMap && s.U64(a + 16) == StringParam && s.Ptr(a + 8) is { } value)
                    diffuse = s.String(value);
            return (s.String(e), s.String(e + 24), diffuse);
        }).ToList();
    }
}

/// <summary>A MESH object: its vertices by attribute (the first of each usage), indices and bone weights.</summary>
public sealed class MeshObject
{
    public required string Name { get; init; }
    public ulong Subindex { get; init; }
    public required string ParentBone { get; init; }
    public required int VertexCount { get; init; }
    public float[]? Positions, Normals, Colors, Uvs; // 3, 3, 4 and 2 per vertex
    public uint[] Indices = [];
    public List<(string Bone, List<(uint Vertex, float Weight)> Weights)> Influences = [];
}

/// <summary>MESH 1.9 (<c>.numshb</c>).</summary>
public static class Mesh
{
    private enum Usage : uint { Position = 0, Normal = 1, TextureCoordinate = 4, ColorSet = 5 }

    public static List<MeshObject> Read(byte[] bytes)
    {
        var s = Ssbh.Open(bytes, "HSEM", 1, 9);
        const int b = Ssbh.Body;
        // Model name, bounding info (100 bytes), u32, objects, buffer sizes, u64 index size, vertex buffers, index buffer,
        // rigging groups.
        var vertexBuffers = s.Array(b + 152, 16).ToList();
        var indexBuffer = s.Bytes(b + 168).ToArray();
        var rigging = s.Array(b + 184, 40).ToList();
        var objects = new List<MeshObject>();
        foreach (var o in s.Array(b + 112, 208))
        {
            var obj = new MeshObject { Name = s.String(o), Subindex = s.U64(o + 8), ParentBone = s.String(o + 16), VertexCount = (int)s.U32(o + 24) };
            // Index count, ?, four vertex buffer offsets, four strides, index buffer offset, ?, u32 index type (0 u16, 1 u32).
            var (indexCount, indexOffset, wide) = ((int)s.U32(o + 28), (int)s.U32(o + 68), s.U32(o + 76) == 1);
            if (indexOffset + (long)indexCount * (wide ? 4 : 2) > indexBuffer.Length) throw new InvalidDataException("index buffer out of range");
            obj.Indices = Enumerable.Range(0, indexCount)
                .Select(i => wide ? BitConverter.ToUInt32(indexBuffer, indexOffset + i * 4) : BitConverter.ToUInt16(indexBuffer, indexOffset + i * 2)).ToArray();
            // Attribute: u32 usage, u32 data type, u32 buffer, u32 offset, u64 subindex, name, names.
            float[]? First(Usage usage, int width)
            {
                var a = s.Array(o + 192, 48).FirstOrDefault(a => s.U32(a) == (uint)usage, -1);
                if (a < 0) return null;
                var (type, buffer, offset) = (s.U32(a + 4), (int)s.U32(a + 8), (int)s.U32(a + 12));
                if (buffer > 3 || buffer >= vertexBuffers.Count) throw new InvalidDataException($"vertex buffer {buffer} out of range");
                var data = s.Bytes(vertexBuffers[buffer]);
                var start = offset + (int)s.U32(o + 36 + buffer * 4);
                var stride = (int)s.U32(o + 52 + buffer * 4);
                return ReadVectors(data, obj.VertexCount, start, stride, type, width);
            }
            obj.Positions = First(Usage.Position, 3);
            obj.Normals = First(Usage.Normal, 3);
            obj.Uvs = First(Usage.TextureCoordinate, 2);
            obj.Colors = First(Usage.ColorSet, 4);
            // Rigging group: object name, subindex, flags, bone buffers (bone name, u32 vertex + f32 weight pairs).
            foreach (var r in rigging.Where(r => s.String(r) == obj.Name && s.U64(r + 8) == obj.Subindex))
                foreach (var bb in s.Array(r + 24, 24))
                    obj.Influences.Add((s.String(bb), s.Array(bb + 8, 8).Select(w => (s.U32(w), s.F32(w + 4))).ToList()));
            objects.Add(obj);
        }
        return objects;
    }

    /// <summary>Vertex data as <paramref name="width"/> floats each: shorter vectors pad with 0 (and w with 1), longer
    /// ones are cut. Byte4 is 0–255 scaled to 0–1.</summary>
    private static float[] ReadVectors(ReadOnlySpan<byte> data, int count, int start, int stride, uint type, int width)
    {
        var (components, size) = type switch
        {
            820 => (3, 4), // Float3
            1076 => (4, 4), // Float4
            1077 => (4, 2), // HalfFloat4
            1079 => (2, 4), // Float2
            1024 => (4, 1), // Byte4
            _ => throw new InvalidDataException($"vertex data type {type} isn't supported"),
        };
        var output = new float[count * width];
        for (var i = 0; i < count; i++)
        {
            var at = start + i * stride;
            if (at < 0 || at + components * size > data.Length) throw new InvalidDataException("vertex data out of range");
            Span<float> v = [0, 0, 0, 1];
            for (var c = 0; c < components; c++)
                v[c] = size switch
                {
                    4 => BitConverter.ToSingle(data[(at + c * 4)..]),
                    2 => (float)BitConverter.UInt16BitsToHalf(BitConverter.ToUInt16(data[(at + c * 2)..])),
                    _ => data[at + c] / 255f,
                };
            v[..width].CopyTo(output.AsSpan(i * width));
        }
        return output;
    }
}
