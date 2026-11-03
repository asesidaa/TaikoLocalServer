using System.Buffers.Binary;

namespace TaikoLocalServer.Infrastructure.Extract.Nijiiro;

/// <summary>
/// An ANIM v1.2 property buffer: the game's <c>nu::DecompressCurve::Decompress</c> (0x140619040) and its helpers. Every
/// buffer starts with a u32 header: bits 12–15 the component count (1 scalar, 3 vec3, 4 quat/vec4); bits 8–11 the key
/// time table (0 none, 1 f32 times, 2 u16 × step, 3 u8 × step, 4 uniform step); bits 4–5 = 0x10 for values and endpoints
/// stored as i16 / 32767 instead of f32; bits 0–3 the codec (0 linear keys, 2 stepped keys, 3 constant, 8 one DCT block,
/// 9 multi-block DCT, 33 keys per block). Rotations are plain 4-component curves; the caller normalizes.
/// </summary>
public sealed class Curve
{
    private const int Block = 33;
    /// <summary>Key times in frames.</summary>
    private readonly float[] times;
    private readonly float[][] keys;
    private readonly bool stepped;

    private Curve(float[] times, float[][] keys, bool stepped) => (this.times, this.keys, this.stepped) = (times, keys, stepped);

    private static uint U32(ReadOnlySpan<byte> b, int at) => at >= 0 && at + 4 <= b.Length ? BinaryPrimitives.ReadUInt32LittleEndian(b[at..]) : throw new InvalidDataException("truncated curve");
    private static ushort U16(ReadOnlySpan<byte> b, int at) => at >= 0 && at + 2 <= b.Length ? BinaryPrimitives.ReadUInt16LittleEndian(b[at..]) : throw new InvalidDataException("truncated curve");
    private static float F32(ReadOnlySpan<byte> b, int at) => BitConverter.UInt32BitsToSingle(U32(b, at));
    private static int Align4(int x) => (x + 3) & ~3;

    public static Curve Decode(ReadOnlySpan<byte> bytes)
    {
        var header = U32(bytes, 0);
        var comps = (int)(header >> 12);
        var isShort = (header & 0x30) == 0x10;
        var codec = header & 0xF;
        if (comps is < 1 or > 4) throw new InvalidDataException($"bad component count in header 0x{header:x}");

        float[] times;
        int data;
        var mode = header & 0xF00;
        if (mode == 0)
        {
            (times, data) = ([0f], 4);
        }
        else
        {
            var n = (int)U32(bytes, 4);
            if (n < 0 || n > bytes.Length) throw new InvalidDataException("truncated curve");
            switch (mode)
            {
                case 0x100:
                {
                    var t = new float[n];
                    for (var i = 0; i < n; i++) t[i] = F32(bytes, 8 + 4 * i);
                    (times, data) = (t, 8 + 4 * n);
                    break;
                }
                case 0x200:
                {
                    var step = F32(bytes, 8);
                    var t = new float[n];
                    for (var i = 0; i < n; i++) t[i] = U16(bytes, 12 + 2 * i) * step;
                    (times, data) = (t, Align4(12 + 2 * n));
                    break;
                }
                case 0x300:
                {
                    var step = F32(bytes, 8);
                    if (12 + n > bytes.Length) throw new InvalidDataException("truncated u8 time table");
                    var t = new float[n];
                    for (var i = 0; i < n; i++) t[i] = bytes[12 + i] * step;
                    (times, data) = (t, Align4(12 + n));
                    break;
                }
                case 0x400:
                {
                    var step = F32(bytes, 8);
                    var t = new float[n];
                    for (var i = 0; i < n; i++) t[i] = i * step;
                    (times, data) = (t, 12);
                    break;
                }
                default: throw new InvalidDataException($"unknown time mode in header 0x{header:x}");
            }
        }
        if (times.Length == 0) throw new InvalidDataException("curve has no keys");
        if (data > bytes.Length) throw new InvalidDataException("truncated curve");
        var d = bytes[data..].ToArray();
        var keys = codec switch
        {
            0 or 2 => Enumerable.Range(0, times.Length).Select(k => ReadValue(d, k * comps, comps, isShort)).ToArray(),
            3 => [ReadValue(d, 0, comps, isShort)],
            8 or 9 => DecodeDct(d, codec, comps, isShort, times.Length),
            _ => throw new InvalidDataException($"unknown codec in header 0x{header:x}"),
        };
        return new Curve(times, keys, codec == 2);
    }

    private static float[] ReadValue(ReadOnlySpan<byte> d, int index, int comps, bool isShort)
    {
        var v = new float[4];
        for (var c = 0; c < comps; c++)
            v[c] = isShort ? (short)U16(d, 2 * (index + c)) / 32767f : F32(d, 4 * (index + c));
        return v;
    }

    /// <summary>Sample at <paramref name="frame"/>, clamping outside the key range (non-looping game behavior).</summary>
    public float[] Sample(float frame)
    {
        if (keys.Length == 1 || frame <= times[0]) return keys[0];
        var last = keys.Length - 1;
        if (frame >= times[last]) return keys[last];
        var k = 0;
        while (k + 1 < times.Length && times[k + 1] <= frame) k++; // the last key at or before frame
        if (stepped) return keys[k];
        var span = times[k + 1] - times[k];
        var f = span > 0 ? (frame - times[k]) / span : 0;
        var (a, b) = (keys[k], keys[k + 1]);
        return [a[0] + (b[0] - a[0]) * f, a[1] + (b[1] - a[1]) * f, a[2] + (b[2] - a[2]) * f, a[3] + (b[3] - a[3]) * f];
    }

    /// <summary>Codecs 8 and 9 (FUN_1406198a0 / FUN_1406195e0): per block, a linear ramp between endpoints plus a DCT
    /// residual. Blocks hold 33 key intervals; the last key of a block is the next block's first endpoint.</summary>
    private static float[][] DecodeDct(byte[] d, uint codec, int comps, bool isShort, int n)
    {
        var baseScale = F32(d, 0);
        var epSize = isShort ? 2 : 4;
        int blocks, endpoints;
        ushort[] offsets;
        if (codec == 9)
        {
            blocks = U16(d, 4);
            if (blocks == 0) throw new InvalidDataException("DCT curve with zero blocks");
            offsets = Enumerable.Range(0, blocks - 1).Select(b => U16(d, 6 + 2 * b)).ToArray();
            var tableEnd = 6 + 2 * (blocks - 1);
            endpoints = isShort ? tableEnd : Align4(tableEnd);
        }
        else
        {
            (blocks, offsets, endpoints) = (1, [], 4);
        }
        var residual = endpoints + (blocks + 1) * comps * epSize;
        if (endpoints > d.Length) throw new InvalidDataException("truncated curve");
        float[] Endpoint(int b) => ReadValue(d.AsSpan(endpoints), b * comps, comps, isShort);
        int BlockLen(int b) => b + 1 == blocks ? n - Block * b - 1 : Block;

        // Key k is block k/33 at local k%33; the final key is the last block's end endpoint.
        var keys = new float[n][];
        for (var k = 0; k < n; k++)
        {
            if (n == 1)
            {
                keys[k] = Endpoint(0);
                continue;
            }
            var (b, local) = k == n - 1 ? ((n - 2) / Block, (n - 2) % Block + 1) : (k / Block, k % Block);
            if (b >= blocks) throw new InvalidDataException($"key {k} outside {blocks} DCT blocks");
            var len = BlockLen(b);
            var (e0, e1) = (Endpoint(b), Endpoint(b + 1));
            var t = local / (float)len;
            // A zero offset for blocks after the first means "no residual".
            int? start = b == 0 ? residual : offsets[b - 1] != 0 ? residual + 4 * offsets[b - 1] : null;
            var r = start is { } s ? ResidualVector(d, s, baseScale, local, comps, len) : new float[comps];
            var key = new float[4];
            for (var i = 0; i < comps; i++) key[i] = e0[i] + (e1[i] - e0[i]) * t + r[i];
            keys[k] = key;
        }
        return keys;
    }

    private const float Short4Scale = 1f / 32767f, Int8Scale = 1f / 127f, NibbleScale = 1f / 7f, NibbleBias = 8f;

    /// <summary>The DCT basis rows, per size 4, 8, … 32: <c>cos((col + 0.5) · (row + 0.5) · π / dim) · √(2 / dim)</c>.</summary>
    private static readonly float[][] Kernels = Enumerable.Range(1, 8).Select(k =>
    {
        var dim = k * 4;
        var scale = MathF.Sqrt(2f / dim);
        var data = new float[dim * dim];
        for (var row = 0; row < dim; row++)
        for (var col = 0; col < dim; col++)
            data[row * dim + col] = MathF.Cos((col + 0.5f) * ((row + 0.5f) * (MathF.PI / dim))) * scale;
        return data;
    }).ToArray();

    private static float[] ResidualVector(byte[] d, int start, float baseScale, int local, int comps, int blockLen)
    {
        var result = new float[comps];
        var at = start;
        for (var c = 0; c < comps; c++) (result[c], at) = ResidualComponent(d, at, baseScale, local, blockLen);
        return result;
    }

    private static (float Value, int Next) ResidualComponent(byte[] d, int at, float baseScale, int local, int blockLen)
    {
        if (local == 0 || local >= blockLen) return (0, at);
        if (at + 8 > d.Length) throw new InvalidDataException("truncated curve residual");
        var (word0, word1, amp) = ((float)U16(d, at), (float)U16(d, at + 2), (float)d[at + 4]);
        var (b5, b6, b7) = (d[at + 5], d[at + 6], d[at + 7]);
        at += 8;
        int shorts = b5 >> 4, bytes = b5 & 0xF, nibbles = b6 >> 4, dims = shorts + bytes + (b6 & 0xF) + nibbles;
        if (dims is 0 or > 8) throw new InvalidDataException("bad curve residual");
        var baseAmp = word0 * (1f / 65535f) * baseScale;
        var slope = word1 * (1f / 65535f) * baseAmp;
        int w16 = b7 >> 4, w17 = b7 & 0xF;
        if (w16 + w17 > 8) throw new InvalidDataException("bad curve residual");
        var w = new float[8];
        var wi = 0;
        for (var i = 0; i < w16; i++) w[wi++] = baseAmp;
        for (var i = 0; i < w17; i++) w[wi++] = slope;
        var scaledSlope = slope * (amp / 255f);
        for (var i = 0; i < 8 - w16 - w17; i++) w[wi++] = scaledSlope;

        var dim = dims * 4;
        var (kernel, row) = (Kernels[dims - 1], (local - 1) * dim);
        var acc = 0f;
        var (entry, basis) = (0, 0);
        void Add(ReadOnlySpan<float> v)
        {
            var weight = w[entry++ % 8];
            for (var i = 0; i < 4; i++) acc += kernel[row + basis + i] * v[i] * weight;
            basis += 4;
        }
        for (var i = 0; i < shorts; i++, at += 8)
        {
            if (at + 8 > d.Length) throw new InvalidDataException("truncated curve residual");
            Add([BitConverter.ToInt16(d, at) * Short4Scale, BitConverter.ToInt16(d, at + 2) * Short4Scale, BitConverter.ToInt16(d, at + 4) * Short4Scale, BitConverter.ToInt16(d, at + 6) * Short4Scale]);
        }
        for (var i = 0; i < bytes; i++, at += 4)
        {
            if (at + 4 > d.Length) throw new InvalidDataException("truncated curve residual");
            Add([(sbyte)d[at] * Int8Scale, (sbyte)d[at + 1] * Int8Scale, (sbyte)d[at + 2] * Int8Scale, (sbyte)d[at + 3] * Int8Scale]);
        }
        static float Nibble(byte v, bool hi) => ((hi ? v >> 4 : v & 0xF) - NibbleBias) * NibbleScale;
        for (var i = 0; i < nibbles >> 1; i++, at += 4)
        {
            if (at + 4 > d.Length) throw new InvalidDataException("truncated curve residual");
            Add([Nibble(d[at], true), Nibble(d[at + 1], true), Nibble(d[at + 2], true), Nibble(d[at + 3], true)]);
            Add([Nibble(d[at], false), Nibble(d[at + 1], false), Nibble(d[at + 2], false), Nibble(d[at + 3], false)]);
        }
        return (acc, at);
    }
}
