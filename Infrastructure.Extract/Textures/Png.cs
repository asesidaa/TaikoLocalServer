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
        var zeros = new byte[stride];
        var candidates = new byte[4][];
        for (var f = 0; f < 4; f++) candidates[f] = new byte[stride];
        for (var y = 0; y < image.Height; y++)
        {
            var row = image.Pixels.AsSpan(y * stride, stride);
            var up = y > 0 ? image.Pixels.AsSpan((y - 1) * stride, stride) : zeros;
            Span<long> sums = [None(row, candidates[0]), Sub(row, candidates[1]), Up(row, up, candidates[2]), Paeth(row, up, candidates[3])];
            var best = 0;
            for (var f = 1; f < 4; f++) if (sums[f] < sums[best]) best = f;
            raw[y * (stride + 1)] = (byte)(best == 3 ? 4 : best);
            candidates[best].CopyTo(raw.AsSpan(y * (stride + 1) + 1));
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

    // Each filter writes its row and returns the sum of its bytes read as signed, the usual heuristic for picking one.
    private static long None(ReadOnlySpan<byte> row, Span<byte> dst)
    {
        row.CopyTo(dst);
        return AbsSum(dst);
    }

    private static long Sub(ReadOnlySpan<byte> row, Span<byte> dst)
    {
        row[..4].CopyTo(dst);
        for (var i = 4; i < row.Length; i++) dst[i] = (byte)(row[i] - row[i - 4]);
        return AbsSum(dst);
    }

    private static long Up(ReadOnlySpan<byte> row, ReadOnlySpan<byte> up, Span<byte> dst)
    {
        for (var i = 0; i < row.Length; i++) dst[i] = (byte)(row[i] - up[i]);
        return AbsSum(dst);
    }

    private static long Paeth(ReadOnlySpan<byte> row, ReadOnlySpan<byte> up, Span<byte> dst)
    {
        for (var i = 0; i < 4; i++) dst[i] = (byte)(row[i] - up[i]); // a = c = 0: Paeth picks b
        for (var i = 4; i < row.Length; i++)
        {
            int a = row[i - 4], b = up[i], c = up[i - 4];
            int p = a + b - c, pa = Math.Abs(p - a), pb = Math.Abs(p - b), pc = Math.Abs(p - c);
            dst[i] = (byte)(row[i] - (pa <= pb && pa <= pc ? a : pb <= pc ? b : c));
        }
        return AbsSum(dst);
    }

    private static long AbsSum(ReadOnlySpan<byte> bytes)
    {
        long sum = 0;
        foreach (var v in bytes) sum += Math.Abs((int)(sbyte)v);
        return sum;
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
