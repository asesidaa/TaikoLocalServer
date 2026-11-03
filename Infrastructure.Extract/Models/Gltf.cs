using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;

namespace TaikoLocalServer.Infrastructure.Extract.Models;

/// <summary>A binary glTF (<c>.glb</c>) being built: one buffer, its views and accessors, and the document's arrays.</summary>
public sealed class Gltf
{
    public const int ArrayBuffer = 34962, ElementArrayBuffer = 34963, Float = 5126, UnsignedShort = 5123, UnsignedInt = 5125;

    private readonly MemoryStream bin = new();
    private readonly JsonArray views = [], accessors = [];
    public JsonArray Nodes { get; } = [];
    public JsonArray Meshes { get; } = [];
    public JsonArray Skins { get; } = [];
    public JsonArray Materials { get; } = [];
    public JsonArray Textures { get; } = [];
    public JsonArray Images { get; } = [];
    public JsonArray Animations { get; } = [];
    public JsonArray Samplers { get; } = [];

    public int View(ReadOnlySpan<byte> bytes, int? target)
    {
        while (bin.Length % 4 != 0) bin.WriteByte(0);
        var v = new JsonObject { ["buffer"] = 0, ["byteOffset"] = bin.Length, ["byteLength"] = bytes.Length };
        if (target is { } t) v["target"] = t;
        bin.Write(bytes);
        views.Add(v);
        return views.Count - 1;
    }

    public int Accessor(ReadOnlySpan<byte> bytes, int component, int count, string type, int? target, (float[] Min, float[] Max)? minMax = null)
    {
        var a = new JsonObject { ["bufferView"] = View(bytes, target), ["componentType"] = component, ["count"] = count, ["type"] = type };
        if (minMax is var (min, max))
        {
            a["min"] = Floats(min);
            a["max"] = Floats(max);
        }
        accessors.Add(a);
        return accessors.Count - 1;
    }

    /// <summary>Floats, little-endian (as every platform this runs on stores them).</summary>
    public int AccessorF32(float[] values, string type, int width, int? target, (float[] Min, float[] Max)? minMax = null) =>
        Accessor(MemoryMarshal.AsBytes(values.AsSpan()), Float, values.Length / width, type, target, minMax);

    public static JsonArray Floats(IEnumerable<float> values) => new(values.Select(v => (JsonNode)JsonValue.Create((double)v)).ToArray());

    /// <summary>The index of <paramref name="item"/> in <paramref name="list"/>, adding it if no equal one is there.</summary>
    public static int Intern(JsonArray list, JsonObject item)
    {
        for (var i = 0; i < list.Count; i++)
            if (JsonNode.DeepEquals(list[i], item)) return i;
        list.Add(item);
        return list.Count - 1;
    }

    public byte[] Finish(IEnumerable<int> sceneNodes)
    {
        while (bin.Length % 4 != 0) bin.WriteByte(0);
        var doc = new JsonObject
        {
            ["asset"] = new JsonObject { ["version"] = "2.0", ["generator"] = "TaikoLocalServer extract" },
            ["scene"] = 0,
            ["scenes"] = new JsonArray(new JsonObject { ["nodes"] = new JsonArray(sceneNodes.Select(n => (JsonNode)n).ToArray()) }),
            ["nodes"] = Nodes.DeepClone(),
            ["accessors"] = accessors.DeepClone(),
            ["bufferViews"] = views.DeepClone(),
            ["buffers"] = new JsonArray(new JsonObject { ["byteLength"] = bin.Length }),
        };
        if (Samplers.Count > 0)
            doc["samplers"] = Samplers.DeepClone();
        else if (Textures.Count > 0)
            doc["samplers"] = new JsonArray(new JsonObject { ["magFilter"] = 9729, ["minFilter"] = 9729, ["wrapS"] = 10497, ["wrapT"] = 10497 });
        // glTF arrays must be non-empty when present.
        foreach (var (key, list) in new[] { ("meshes", Meshes), ("materials", Materials), ("skins", Skins), ("textures", Textures), ("images", Images), ("animations", Animations) })
            if (list.Count > 0) doc[key] = list.DeepClone();

        var json = Encoding.UTF8.GetBytes(doc.ToJsonString());
        var jsonLength = (json.Length + 3) & ~3;
        var total = 12 + 8 + jsonLength + 8 + (int)bin.Length;
        var glb = new byte[total];
        "glTF"u8.CopyTo(glb);
        BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(4), 2);
        BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(8), (uint)total);
        BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(12), (uint)jsonLength);
        "JSON"u8.CopyTo(glb.AsSpan(16));
        glb.AsSpan(20, jsonLength).Fill((byte)' ');
        json.CopyTo(glb, 20);
        BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(20 + jsonLength), (uint)bin.Length);
        "BIN\0"u8.CopyTo(glb.AsSpan(24 + jsonLength));
        bin.ToArray().CopyTo(glb, 28 + jsonLength);
        return glb;
    }
}
