using System.Buffers.Binary;

namespace TaikoLocalServer.Infrastructure.Extract.Textures;

/// <summary>A texture and the <c>GIDX</c> id model materials refer to it by.</summary>
public sealed record Texture(uint Id, RgbaImage Image);

/// <summary>
/// The older games' textures: <c>NTP3</c> (<c>.nut</c>, PS3 NUT, big-endian). The README beside this project has the layout.
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
