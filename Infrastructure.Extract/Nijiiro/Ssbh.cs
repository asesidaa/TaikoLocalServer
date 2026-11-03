using System.Buffers.Binary;
using System.Numerics;
using System.Text;

namespace TaikoLocalServer.Infrastructure.Extract.Nijiiro;

/// <summary>
/// The SSBH container Nijiiro's models and animations use (as Smash Ultimate's; the layouts follow ssbh_lib, MIT).
/// Little-endian: <c>HBSS</c>, u64 64, u32 0, then at 0x10 the format's magic (<c>LDOM</c>, <c>HSEM</c>, <c>LEKS</c>,
/// <c>LTAM</c>, <c>MINA</c>) and u16 major, u16 minor version; the format's fields start at 0x18. Every pointer is a
/// u64 offset from where the pointer itself is (0 is null). An array is such an offset and a u64 count; a string is
/// an offset to NUL-terminated UTF-8.
/// </summary>
public sealed class Ssbh
{
    public const int Body = 0x18;
    private readonly byte[] b;

    private Ssbh(byte[] bytes) => b = bytes;

    /// <summary>Checks the header is <paramref name="magic"/> at version <paramref name="major"/>.<paramref name="minor"/>.</summary>
    public static Ssbh Open(byte[] bytes, string magic, int major, int minor)
    {
        if (bytes.Length < Body || !bytes.AsSpan(0, 4).SequenceEqual("HBSS"u8))
            throw new InvalidDataException("not an SSBH file");
        var found = Encoding.ASCII.GetString(bytes, 0x10, 4);
        if (found != magic) throw new InvalidDataException($"expected {magic}, found {found}");
        var s = new Ssbh(bytes);
        var (ma, mi) = (s.U16(0x14), s.U16(0x16));
        if (ma != major || mi != minor) throw new InvalidDataException($"{magic} version {ma}.{mi} isn't supported (only {major}.{minor})");
        return s;
    }

    private void Check(long at, long length)
    {
        if (at < 0 || length < 0 || at + length > b.Length) throw new InvalidDataException("truncated SSBH file");
    }

    public ushort U16(int at) { Check(at, 2); return BinaryPrimitives.ReadUInt16LittleEndian(b.AsSpan(at)); }
    public short I16(int at) => (short)U16(at);
    public uint U32(int at) { Check(at, 4); return BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(at)); }
    public ulong U64(int at) { Check(at, 8); return BinaryPrimitives.ReadUInt64LittleEndian(b.AsSpan(at)); }
    public float F32(int at) => BitConverter.UInt32BitsToSingle(U32(at));

    /// <summary>The absolute position a relative pointer at <paramref name="at"/> points to, or null.</summary>
    public int? Ptr(int at)
    {
        var off = U64(at);
        if (off == 0) return null;
        var target = at + (long)off;
        Check(target, 0);
        return (int)target;
    }

    public string String(int at)
    {
        if (Ptr(at) is not { } p) return "";
        var end = System.Array.IndexOf(b, (byte)0, p);
        return Encoding.UTF8.GetString(b, p, (end < 0 ? b.Length : end) - p);
    }

    /// <summary>An array's element positions, <paramref name="size"/> bytes apart.</summary>
    public IEnumerable<int> Array(int at, int size)
    {
        var count = U64(at + 8);
        if (Ptr(at) is not { } p || count == 0) return [];
        Check(p, (long)count * size);
        return Enumerable.Range(0, (int)count).Select(i => p + i * size);
    }

    public ReadOnlySpan<byte> Bytes(int at)
    {
        var count = U64(at + 8);
        if (Ptr(at) is not { } p || count == 0) return [];
        Check(p, (long)count);
        return b.AsSpan(p, (int)count);
    }

    /// <summary>A column-major 4×4 matrix, as System.Numerics stores a row-vector one (so <c>local × parent</c> composes).</summary>
    public Matrix4x4 Matrix(int at) => new(
        F32(at), F32(at + 4), F32(at + 8), F32(at + 12),
        F32(at + 16), F32(at + 20), F32(at + 24), F32(at + 28),
        F32(at + 32), F32(at + 36), F32(at + 40), F32(at + 44),
        F32(at + 48), F32(at + 52), F32(at + 56), F32(at + 60));
}
