using System.Buffers.Binary;
using System.Text;

namespace TaikoLocalServer.Infrastructure.Extract.Textures;

/// <summary>
/// <c>NUT_PACK_TYPE1</c> (<c>.ndp</c>): the <c>.nut</c> files of a pack, by name. A boost binary archive: the entry count at
/// 0x3E, then each entry's name (u32 length + bytes), offset and size; the first entry also carries a 5-byte class header
/// after its name. The data follows the table and a u32.
/// </summary>
public static class NutPack
{
    public static List<(string Name, byte[] Nut)> Read(byte[] b)
    {
        if (!b.AsSpan().StartsWith("NUT_PACK_TYPE1"u8))
            throw new InvalidDataException("not a NUT_PACK_TYPE1 pack");
        var count = (int)Be32(b, 0x3E);
        var p = 0x4B;
        var entries = new List<(string, int, int)>(count);
        for (var i = 0; i < count; i++)
        {
            var len = (int)Be32(b, p);
            if (p + 4 + len > b.Length) throw new InvalidDataException("truncated name");
            var name = Encoding.UTF8.GetString(b, p + 4, len).TrimEnd('\0');
            p += 4 + len + (i == 0 ? 5 : 0);
            entries.Add((name, (int)Be32(b, p), (int)Be32(b, p + 4)));
            p += 8;
        }
        var data = p + 4;
        return entries.Select(e =>
        {
            if (data + e.Item2 + e.Item3 > b.Length) throw new InvalidDataException("pack entry out of range");
            return (e.Item1, b.AsSpan(data + e.Item2, e.Item3).ToArray());
        }).ToList();
    }

    private static uint Be32(byte[] b, int at) =>
        at + 4 <= b.Length ? BinaryPrimitives.ReadUInt32BigEndian(b.AsSpan(at)) : throw new InvalidDataException("truncated pack");
}
