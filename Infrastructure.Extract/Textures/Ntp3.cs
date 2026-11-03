using System.Buffers.Binary;

namespace TaikoLocalServer.Infrastructure.Extract.Textures;

/// <summary>A texture and the <c>GIDX</c> id model materials refer to it by.</summary>
public sealed record Texture(uint Id, RgbaImage Image);

/// <summary>
/// The older games' textures: <c>NTP3</c> (<c>.nut</c>, PS3 NUT, big-endian). See tools/extract's README for the layout.
/// </summary>
public static class Ntp3
{
    /// <summary>Every texture's base mip. Version 0x0100 stores header, data, header, data; 0x0200 stores all headers
    /// first, each with its data's offset (from the header) at +0x20.</summary>
    public static List<Texture> Decode(ReadOnlySpan<byte> b)
    {
        if (b.Length < 0x10 || !b[..4].SequenceEqual("NTP3"u8))
            throw new InvalidDataException("not an NTP3 texture");
        var (version, count) = (Be16(b, 4), Be16(b, 6));
        var h = 0x10;
        var textures = new List<Texture>(count);
        for (var i = 0; i < count; i++)
        {
            var (total, size, header) = ((int)Be32(b, h), (int)Be32(b, h + 8), Be16(b, h + 12));
            var (format, width, height) = (b[h + 19], Be16(b, h + 20), Be16(b, h + 22));
            var data = version >= 0x0200 ? h + (int)Be32(b, h + 0x20) : h + header;
            // The GIDX block ends the header: "GIDX", its size, then the id.
            var gidx = b.Slice(h, Math.Min(header, b.Length - h)).IndexOf("GIDX"u8);
            var id = gidx >= 0 ? Be32(b, h + gidx + 8) : 0;
            if (data < 0 || data + size > b.Length)
                throw new InvalidDataException("texture data out of range");
            try
            {
                textures.Add(new Texture(id, ToRgba(format, width, height, b.Slice(data, size))));
            }
            catch (Exception e) when (e is not InvalidDataException)
            {
                throw new InvalidDataException($"texture {id:x}, format {format}: {e.Message}", e);
            }
            h += version >= 0x0200 ? header : total;
        }
        return textures;
    }

    private static RgbaImage ToRgba(byte format, int width, int height, ReadOnlySpan<byte> data)
    {
        switch (format)
        {
            case 0 or 1 or 2: return Bcn.Decode(data, width, height, format + 1);
            // 32-bit: both are A8R8G8B8, as bytes (in every format-17 texture byte 0 is the alpha, as in 14's).
            case 14 or 17:
            {
                if (data.Length < width * height * 4)
                    throw new InvalidDataException("short RGBA data");
                var image = new RgbaImage(width, height);
                for (var p = 0; p < width * height; p++)
                {
                    var s = data.Slice(p * 4, 4);
                    var d = image.Pixels.AsSpan(p * 4, 4);
                    (d[0], d[1], d[2], d[3]) = (s[1], s[2], s[3], s[0]);
                }
                return image;
            }
            default: throw new InvalidDataException($"NTP3 format {format} isn't supported");
        }
    }

    private static ushort Be16(ReadOnlySpan<byte> b, int at) =>
        at + 2 <= b.Length ? BinaryPrimitives.ReadUInt16BigEndian(b[at..]) : throw new InvalidDataException("truncated NTP3");

    private static uint Be32(ReadOnlySpan<byte> b, int at) =>
        at + 4 <= b.Length ? BinaryPrimitives.ReadUInt32BigEndian(b[at..]) : throw new InvalidDataException("truncated NTP3");
}

/// <summary>BC1–BC3 (DXT1/3/5) decoding, rounding exactly as bcdec does (tools/extract's decoder), so both extractors
/// write the same pixels.</summary>
public static class Bcn
{
    /// <param name="bc">1, 2 or 3.</param>
    public static RgbaImage Decode(ReadOnlySpan<byte> data, int width, int height, int bc)
    {
        var blockSize = bc == 1 ? 8 : 16;
        int bw = (width + 3) / 4, bh = (height + 3) / 4;
        if (data.Length < bw * bh * blockSize)
            throw new InvalidDataException("short block data");
        var image = new RgbaImage(width, height);
        Span<byte> block = stackalloc byte[64];
        for (var by = 0; by < bh; by++)
        for (var bx = 0; bx < bw; bx++)
        {
            var src = data.Slice((by * bw + bx) * blockSize, blockSize);
            Color(bc == 1 ? src : src[8..], block, bc != 1);
            if (bc == 2)
                for (var i = 0; i < 16; i++)
                    block[i * 4 + 3] = (byte)(((src[i / 4 * 2 + (i & 2) / 2] >> (i & 1) * 4) & 0x0F) * 17);
            else if (bc == 3)
                Alpha(src, block);
            for (var y = 0; y < 4 && by * 4 + y < height; y++)
            {
                var w = Math.Min(4, width - bx * 4);
                block.Slice(y * 16, w * 4).CopyTo(image.Pixels.AsSpan(((by * 4 + y) * width + bx * 4) * 4));
            }
        }
        return image;
    }

    private static void Color(ReadOnlySpan<byte> b, Span<byte> block, bool opaque)
    {
        uint c0 = BinaryPrimitives.ReadUInt16LittleEndian(b), c1 = BinaryPrimitives.ReadUInt16LittleEndian(b[2..]);
        uint r0 = c0 >> 11 & 0x1F, g0 = c0 >> 5 & 0x3F, b0 = c0 & 0x1F, r1 = c1 >> 11 & 0x1F, g1 = c1 >> 5 & 0x3F, b1 = c1 & 0x1F;
        Span<uint> refs = stackalloc uint[4]; // 0xAABBGGRR
        static uint Rgba(uint r, uint g, uint b, uint a = 255) => r | g << 8 | b << 16 | a << 24;
        refs[0] = Rgba((r0 * 527 + 23) >> 6, (g0 * 259 + 33) >> 6, (b0 * 527 + 23) >> 6);
        refs[1] = Rgba((r1 * 527 + 23) >> 6, (g1 * 259 + 33) >> 6, (b1 * 527 + 23) >> 6);
        if (c0 > c1 || opaque)
        {
            refs[2] = Rgba(((2 * r0 + r1) * 351 + 61) >> 7, ((2 * g0 + g1) * 2763 + 1039) >> 11, ((2 * b0 + b1) * 351 + 61) >> 7);
            refs[3] = Rgba(((r0 + 2 * r1) * 351 + 61) >> 7, ((g0 + 2 * g1) * 2763 + 1039) >> 11, ((b0 + 2 * b1) * 351 + 61) >> 7);
        }
        else
        {
            refs[2] = Rgba(((r0 + r1) * 1053 + 125) >> 8, ((g0 + g1) * 4145 + 1019) >> 11, ((b0 + b1) * 1053 + 125) >> 8);
            refs[3] = 0;
        }
        var indices = BinaryPrimitives.ReadUInt32LittleEndian(b[4..]);
        for (var i = 0; i < 16; i++, indices >>= 2)
            BinaryPrimitives.WriteUInt32LittleEndian(block[(i * 4)..], refs[(int)(indices & 3)]);
    }

    private static void Alpha(ReadOnlySpan<byte> b, Span<byte> block)
    {
        Span<uint> a = stackalloc uint[8];
        (a[0], a[1]) = (b[0], b[1]);
        if (a[0] > a[1])
            for (uint i = 1; i < 7; i++) a[(int)i + 1] = ((7 - i) * a[0] + i * a[1] + 1) / 7;
        else
        {
            for (uint i = 1; i < 5; i++) a[(int)i + 1] = ((5 - i) * a[0] + i * a[1] + 1) / 5;
            (a[6], a[7]) = (0, 255);
        }
        var indices = BinaryPrimitives.ReadUInt64LittleEndian(b) >> 16;
        for (var i = 0; i < 16; i++, indices >>= 3)
            block[i * 4 + 3] = (byte)a[(int)(indices & 7)];
    }
}
