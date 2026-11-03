using System.Buffers.Binary;

namespace TaikoLocalServer.Infrastructure.Extract.Textures;

/// <summary>BC1–BC3 (DXT1/3/5) and BC7 decoding, rounding exactly as bcdec does (what the Rust
/// extractor this replaced used), so outputs didn't change.</summary>
public static class Bcn
{
    /// <param name="bc">1, 2, 3 or 7.</param>
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
            if (bc == 7)
                Bc7(src, block);
            else
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

    private static readonly int[] ColorBits = [4, 6, 5, 7, 5, 7, 7, 5], AlphaBits = [0, 0, 0, 0, 6, 8, 7, 5];
    private static readonly int[] Weights2 = [0, 21, 43, 64], Weights3 = [0, 9, 18, 27, 37, 46, 55, 64];
    private static readonly int[] Weights4 = [0, 4, 9, 13, 17, 21, 26, 30, 34, 38, 43, 47, 51, 55, 60, 64];
    private const int ModeHasPBits = 0b11001011;

    /// <summary>Each 4×4 block's subset per pixel, row by row; A, B and C mark subset 0, 1 and 2's anchor pixel.</summary>
    private static readonly string[] Partitions2 =
    [
        "A01100110011001B", "A00100010001000B", "A11101110111011B", "A00100110011011B",
        "A00000010001001B", "A01101110111111B", "A00100110111111B", "A00000010011011B",
        "A00000000001001B", "A01101111111111B", "A00000010111111B", "A00000000001011B",
        "A00101111111111B", "A00000001111111B", "A00011111111111B", "A00000000000111B",
        "A00010001110111B", "A1B1000100000000", "A0000000B0001110", "A1B1001100010000",
        "A0B1000100000000", "A0001000B1001110", "A0000000B0001100", "A11100110011000B",
        "A0B1000100010000", "A0001000B0001100", "A1B0011001100110", "A0B1011001101100",
        "A0010111B1101000", "A0001111B1110000", "A1B1000110001110", "A0B1100110011100",
        "A10101010101010B", "A00011110000111B", "A10110B001011010", "A0110011B1001100",
        "A0B1110000111100", "A1010101B0101010", "A11010010110100B", "A10110101010010B",
        "A1B1001111001110", "A0010011B1001000", "A0B1001001001100", "A0B1101111011100",
        "A1B0100110010110", "A01111001100001B", "A11001101001100B", "A00001B001100000",
        "A10011B001000000", "A0B0011100100000", "A00000B001110010", "A0000100B1100100",
        "A11011001001001B", "A01101101100100B", "A1B0001110011100", "A0B1100111000110",
        "A11011001100100B", "A11000110011100B", "A11111101000000B", "A00110001110011B",
        "A00011110011001B", "A0B1001111110000", "A0B0001011101110", "A10001000111011B",
    ];
    private static readonly string[] Partitions3 =
    [
        "A01B00110221222C", "A00B0011C2112221", "A0002001C211221B", "A22C00220011011B",
        "A0000000B122112C", "A01B00110022002C", "A02C00221111111B", "A0110011C211221B",
        "A0000000B111222C", "A0001111B111222C", "A00011B12222222C", "A01200B20012001C",
        "A11201B20112011C", "A1220B220122012C", "A01B01121122122C", "A01B2001C2002220",
        "A00B00110112112C", "A11B0011C0012200", "A0001122B122112C", "A02C00220022111B",
        "A11B01110222022C", "A00B0001C2212221", "A00000B10122012C", "A0001100C2B02210",
        "A12C0B2200110000", "A0120012B122222C", "A11012C1B2210110", "A00001B012C11221",
        "A0221102B102002C", "A1100B102002222C", "A011012201C2001B", "A0002000C211222B",
        "A0000002B122122C", "A22C00220012001B", "A01B00120022022C", "A1200B2001C00120",
        "A00011B122C20000", "A1201201C0B20120", "A1202012BC010120", "A011220011C2001B",
        "A01111C22200001B", "A10B01012222222C", "A0000000C121212B", "A0221B220022112C",
        "A02C00110022001B", "A22012C10220122B", "A10122C22222010B", "A0002121C121212B",
        "A10B01010101222C", "A22C01110222011B", "A0021B120002111C", "A0002B122112211C",
        "A2220B110111022C", "A0021112B112000C", "A1100B100110222C", "A000000021B2211C",
        "A1100B102222222C", "A022001100B1002C", "A0221122B122002C", "A000000000002B1C",
        "A00C00010002000B", "A22212220222B22C", "A10B22222222222C", "A11B2011C2012220",
    ];

    private struct Bits(ulong low, ulong high)
    {
        public int Read(int n)
        {
            var mask = (1UL << n) - 1;
            var bits = low & mask;
            low = (low >> n) | ((high & mask) << (64 - n));
            high >>= n;
            return (int)bits;
        }
    }

    private static int Lerp(int a, int b, int w) => (a * (64 - w) + b * w + 32) >> 6;

    private static void Bc7(ReadOnlySpan<byte> src, Span<byte> block)
    {
        var bits = new Bits(BinaryPrimitives.ReadUInt64LittleEndian(src), BinaryPrimitives.ReadUInt64LittleEndian(src[8..]));
        var mode = 0;
        while (mode < 8 && bits.Read(1) == 0) mode++;
        if (mode >= 8)
        {
            block.Clear();
            return;
        }
        int partition = 0, partitions = 1, rotation = 0, indexSelection = 0;
        if (mode is 0 or 1 or 2 or 3 or 7)
        {
            partitions = mode is 0 or 2 ? 3 : 2;
            partition = bits.Read(mode == 0 ? 4 : 6);
        }
        var endpointCount = partitions * 2;
        if (mode is 4 or 5)
        {
            rotation = bits.Read(2);
            if (mode == 4) indexSelection = bits.Read(1);
        }
        var ep = new int[6 * 4];
        for (var c = 0; c < 3; c++)
            for (var e = 0; e < endpointCount; e++) ep[e * 4 + c] = bits.Read(ColorBits[mode]);
        if (AlphaBits[mode] > 0)
            for (var e = 0; e < endpointCount; e++) ep[e * 4 + 3] = bits.Read(AlphaBits[mode]);
        if (mode is 0 or 1 or 3 or 6 or 7)
        {
            for (var k = 0; k < endpointCount * 4; k++) ep[k] <<= 1;
            if (mode == 1)
            {
                var (i, j) = (bits.Read(1), bits.Read(1));
                for (var c = 0; c < 3; c++)
                {
                    ep[c] |= i;
                    ep[4 + c] |= i;
                    ep[8 + c] |= j;
                    ep[12 + c] |= j;
                }
            }
            else if ((ModeHasPBits & (1 << mode)) != 0)
            {
                for (var e = 0; e < endpointCount; e++)
                {
                    var p = bits.Read(1);
                    for (var c = 0; c < 4; c++) ep[e * 4 + c] |= p;
                }
            }
        }
        var pbit = (ModeHasPBits >> mode) & 1;
        for (var e = 0; e < endpointCount; e++)
        {
            var j = ColorBits[mode] + pbit;
            for (var c = 0; c < 3; c++)
            {
                ep[e * 4 + c] <<= 8 - j;
                ep[e * 4 + c] |= ep[e * 4 + c] >> j;
            }
            j = AlphaBits[mode] + pbit;
            ep[e * 4 + 3] <<= 8 - j;
            ep[e * 4 + 3] |= ep[e * 4 + 3] >> j;
        }
        if (AlphaBits[mode] == 0)
            for (var e = 0; e < endpointCount; e++) ep[e * 4 + 3] = 0xFF;

        var indexBits = mode is 0 or 1 ? 3 : mode == 6 ? 4 : 2;
        var indexBits2 = mode == 4 ? 3 : mode == 5 ? 2 : 0;
        var weights = indexBits == 2 ? Weights2 : indexBits == 3 ? Weights3 : Weights4;
        var weights2 = indexBits2 == 2 ? Weights2 : Weights3;
        // Subset per pixel, and whether the pixel is its subset's anchor (one index bit fewer).
        int Subset(int p, out bool anchor)
        {
            if (partitions == 1)
            {
                anchor = p == 0;
                return 0;
            }
            var ch = (partitions == 2 ? Partitions2 : Partitions3)[partition][p];
            anchor = ch >= 'A';
            return anchor ? ch - 'A' : ch - '0';
        }
        Span<int> indices = stackalloc int[16];
        for (var p = 0; p < 16; p++)
        {
            Subset(p, out var anchor);
            indices[p] = bits.Read(indexBits - (anchor ? 1 : 0));
        }
        for (var p = 0; p < 16; p++)
        {
            var s = Subset(p, out _) * 2;
            var w = weights[indices[p]];
            int Channel(int c, int weight) => Lerp(ep[s * 4 + c], ep[(s + 1) * 4 + c], weight);
            int r, g, b, a;
            if (indexBits2 == 0)
            {
                (r, g, b, a) = (Channel(0, w), Channel(1, w), Channel(2, w), Channel(3, w));
            }
            else
            {
                var w2 = weights2[bits.Read(p != 0 ? indexBits2 : indexBits2 - 1)];
                (r, g, b, a) = indexSelection == 0
                    ? (Channel(0, w), Channel(1, w), Channel(2, w), Channel(3, w2))
                    : (Channel(0, w2), Channel(1, w2), Channel(2, w2), Channel(3, w));
            }
            switch (rotation)
            {
                case 1: (a, r) = (r, a); break;
                case 2: (a, g) = (g, a); break;
                case 3: (a, b) = (b, a); break;
            }
            (block[p * 4], block[p * 4 + 1], block[p * 4 + 2], block[p * 4 + 3]) = ((byte)r, (byte)g, (byte)b, (byte)a);
        }
    }
}
