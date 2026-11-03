using System.Buffers.Binary;
using System.Text;

namespace TaikoLocalServer.Infrastructure.Extract.Lumen;

/// <summary>One <c>.lm</c> in a pack, with its atlases (<c>img*.nut</c>, NTP3) in atlas-id order.</summary>
public sealed record LmFile(string Name, byte[] Lm, List<byte[]> Nuts);

/// <summary>
/// The older games' Lumen UI packs: <c>lumendata/packed/&lt;scene&gt;/packeddata.ddp</c> (<c>LM_NUT_TYPE1</c>). A boost
/// binary archive like the NUT packs: the <c>.lm</c> count at 0x3E, then from 0x4B per <c>.lm</c> its name (u32 length +
/// bytes; the first entry adds a 5-byte class header), offset, size and the <c>[from, to)</c> range of its textures in
/// the <c>.nut</c> list. Then 5 bytes, the <c>.nut</c> count, 9 bytes, and an (offset, size) pair per <c>.nut</c>. The
/// data starts 16 bytes after the last pair: the <c>.lm</c> files, then the <c>.nut</c> files (offsets from there).
/// </summary>
public static class LmPack
{
    public static List<LmFile> Read(byte[] b)
    {
        if (!b.AsSpan().StartsWith("LM_NUT_TYPE1"u8))
            throw new InvalidDataException("not an LM_NUT_TYPE1 pack");
        int Be32(int at) => at >= 0 && at + 4 <= b.Length ? (int)BinaryPrimitives.ReadUInt32BigEndian(b.AsSpan(at)) : throw new InvalidDataException("truncated pack");
        byte[] Slice(int at, int len) => at >= 0 && len >= 0 && at + len <= b.Length ? b[at..(at + len)] : throw new InvalidDataException("pack entry out of range");

        var count = Be32(0x3E);
        var p = 0x4B;
        var lms = new List<(string Name, int Off, int Size, int From, int To)>();
        for (var i = 0; i < count; i++)
        {
            var len = Be32(p);
            var name = Encoding.UTF8.GetString(Slice(p + 4, len)).TrimEnd('\0');
            p += 4 + len + (i == 0 ? 5 : 0);
            lms.Add((name, Be32(p), Be32(p + 4), Be32(p + 8), Be32(p + 12)));
            p += 16;
        }
        var nutCount = Be32(p + 5);
        p += 5 + 4 + 9;
        var nuts = Enumerable.Range(0, nutCount).Select(k => (Off: Be32(p + k * 8), Size: Be32(p + k * 8 + 4))).ToList();
        var dataBase = p + nutCount * 8 + 16;
        var nutBase = dataBase + lms.Select(l => l.Off + l.Size).DefaultIfEmpty(0).Max();
        return lms.Select(l =>
        {
            var lm = Slice(dataBase + l.Off, l.Size);
            if (!lm.AsSpan().StartsWith("LMB\0"u8))
                throw new InvalidDataException($"{l.Name}: not an LMB file");
            if (l.From < 0 || l.To > nuts.Count || l.From > l.To)
                throw new InvalidDataException("texture range out of bounds");
            return new LmFile(l.Name, lm, nuts[l.From..l.To].Select(n => Slice(nutBase + n.Off, n.Size)).ToList());
        }).ToList();
    }
}
