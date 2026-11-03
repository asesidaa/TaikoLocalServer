using System.Buffers.Binary;
using System.IO.Compression;
using System.IO.Hashing;
using System.Text;

namespace TaikoLocalServer.Infrastructure.Extract.Textures;

/// <summary>RGBA8 PNG encoding: each row filtered with whichever of None/Sub/Up/Paeth gives the smallest sum (a cheap
/// heuristic), deflated by zlib.</summary>
public static class Png
{
    private static readonly byte[] Signature = [0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A];

    public static byte[] Encode(RgbaImage image)
    {
        var stride = image.Width * 4;
        var raw = new byte[(stride + 1) * image.Height];
        var candidate = new byte[stride];
        for (var y = 0; y < image.Height; y++)
        {
            var row = image.Pixels.AsSpan(y * stride, stride);
            var up = y > 0 ? image.Pixels.AsSpan((y - 1) * stride, stride) : Span<byte>.Empty;
            var best = long.MaxValue;
            foreach (var filter in (ReadOnlySpan<byte>)[0, 1, 2, 4])
            {
                long sum = 0;
                for (var i = 0; i < stride; i++)
                {
                    int a = i >= 4 ? row[i - 4] : 0, b = up.IsEmpty ? 0 : up[i], c = i >= 4 && !up.IsEmpty ? up[i - 4] : 0;
                    var predictor = filter switch { 1 => a, 2 => b, 4 => Paeth(a, b, c), _ => 0 };
                    candidate[i] = (byte)(row[i] - predictor);
                    sum += Math.Abs((sbyte)candidate[i]);
                }
                if (sum >= best) continue;
                best = sum;
                raw[y * (stride + 1)] = filter;
                candidate.CopyTo(raw.AsSpan(y * (stride + 1) + 1));
            }
        }
        using var deflated = new MemoryStream();
        using (var z = new ZLibStream(deflated, CompressionLevel.Optimal, leaveOpen: true))
            z.Write(raw);

        using var png = new MemoryStream();
        png.Write(Signature);
        var header = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, image.Width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), image.Height);
        header[8] = 8; // bit depth
        header[9] = 6; // RGBA
        Chunk(png, "IHDR", header);
        Chunk(png, "IDAT", deflated.ToArray());
        Chunk(png, "IEND", []);
        return png.ToArray();
    }

    private static int Paeth(int a, int b, int c)
    {
        int p = a + b - c, pa = Math.Abs(p - a), pb = Math.Abs(p - b), pc = Math.Abs(p - c);
        return pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
    }

    private static void Chunk(Stream png, string type, byte[] data)
    {
        Span<byte> word = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(word, data.Length);
        png.Write(word);
        var typeBytes = Encoding.ASCII.GetBytes(type);
        png.Write(typeBytes);
        png.Write(data);
        var crc = new Crc32();
        crc.Append(typeBytes);
        crc.Append(data);
        BinaryPrimitives.WriteUInt32BigEndian(word, crc.GetCurrentHashAsUInt32());
        png.Write(word);
    }
}
