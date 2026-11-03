using System.Buffers.Binary;

namespace TaikoLocalServer.Infrastructure.Extract.Textures;

/// <summary>
/// Nijiiro's textures (<c>.nutexb</c>, little-endian). The data comes first: per array layer, every mip. Then per layer a
/// 0x40-byte table of mip sizes, and a 112-byte footer: magic (<c>46XT</c> on PC, <c> XNT</c> on Switch), a 0x40-byte name,
/// u32 width, height, depth, format, ?, mip count, swizzle (0x1000 Tegra-swizzled; the PC build's are all 0), layer
/// count, data size, then <c> XET</c> and the version.
/// </summary>
public static class Nutexb
{
    private const int FooterSize = 112;

    /// <summary>The base mip, with array layers (the 12-row face atlases) stacked top to bottom.</summary>
    public static RgbaImage Decode(byte[] b)
    {
        if (b.Length < FooterSize) throw new InvalidDataException("not a nutexb texture");
        var f = b.Length - FooterSize;
        var magic = b.AsSpan(f, 4);
        if (!magic.SequenceEqual("46XT"u8) && !magic.SequenceEqual(" XNT"u8)) throw new InvalidDataException("not a nutexb texture");
        uint U32(int at) => BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(f + 0x44 + at * 4));
        var (width, height, format, mips, swizzle, layers) = ((int)U32(0), (int)U32(1), U32(3), (int)Math.Max(U32(5), 1), U32(6), (int)Math.Max(U32(7), 1));
        // shortcut: Switch (Tegra-swizzled) textures aren't read; the PC build has none. Add deswizzling if a mod ships one.
        if (swizzle == 0x1000) throw new InvalidDataException("swizzled (Switch) textures aren't supported");
        var tables = f - layers * 0x40;
        if (tables < 0 || mips > 16) throw new InvalidDataException("truncated nutexb");
        int LayerSize(int layer) => Enumerable.Range(0, mips).Sum(m => (int)BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(tables + layer * 0x40 + m * 4)));
        var images = new List<RgbaImage>(layers);
        var at = 0;
        for (var layer = 0; layer < layers; layer++)
        {
            images.Add(DecodeBase(b.AsSpan(at, Math.Min(LayerSize(layer), b.Length - at)), width, height, format));
            at += LayerSize(layer);
        }
        return layers == 1 ? images[0] : RgbaImage.Stack(images);
    }

    private static RgbaImage DecodeBase(ReadOnlySpan<byte> data, int width, int height, uint format)
    {
        switch (format)
        {
            // Unorm and sRGB decode alike: the bytes are written as they are.
            case 0x0400 or 0x0405 or 0x0450 or 0x0455:
            {
                if (data.Length < width * height * 4) throw new InvalidDataException("short RGBA data");
                var image = new RgbaImage(width, height, data[..(width * height * 4)].ToArray());
                if (format >= 0x0450)
                    for (var p = 0; p < image.Pixels.Length; p += 4)
                        (image.Pixels[p], image.Pixels[p + 2]) = (image.Pixels[p + 2], image.Pixels[p]);
                return image;
            }
            // R8: grey, opaque (as image_dds decodes it).
            case 0x0100:
            {
                if (data.Length < width * height) throw new InvalidDataException("short R8 data");
                var image = new RgbaImage(width, height);
                for (var p = 0; p < width * height; p++)
                    (image.Pixels[p * 4], image.Pixels[p * 4 + 1], image.Pixels[p * 4 + 2], image.Pixels[p * 4 + 3]) = (data[p], data[p], data[p], 255);
                return image;
            }
            case 0x0480 or 0x0485: return Bcn.Decode(data, width, height, 1);
            case 0x0490 or 0x0495: return Bcn.Decode(data, width, height, 2);
            case 0x04A0 or 0x04A5: return Bcn.Decode(data, width, height, 3);
            case 0x04E0 or 0x04E5: return Bcn.Decode(data, width, height, 7);
            default: throw new InvalidDataException($"nutexb format 0x{format:x4} isn't supported");
        }
    }
}
