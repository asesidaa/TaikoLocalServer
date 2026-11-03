using System.Buffers.Binary;

namespace TaikoLocalServer.Infrastructure.Extract.Ac15;

/// <summary>
/// The older games' drum sounds: <c>sound/se/SE_GAME_NEIRO_&lt;id&gt;_C.nub</c>, NUB v2.1 banks of Sony VAG (PS-ADPCM)
/// streams, decoded to 16-bit WAV for the Profile's tone preview.
/// </summary>
public static class Tones
{
    private const string Prefix = "sound/se/SE_GAME_NEIRO_";

    public static IEnumerable<Rule> Inputs() => [Rule.Files("sound/se/")];

    /// <summary>The bank's streams as (sample rate, PCM). Header: u32 version <c>0x00020100</c>, u32, u32 bank id, u32
    /// stream count, u32 data offset, u32 data size, u32 offset of the stream-header offsets. A stream header starts with
    /// its codec (<c>vag\0</c>) and has the stream's size in the data at +0x14, its offset at +0x18 and its sample rate at
    /// +0xBC.</summary>
    public static List<(int Rate, short[] Pcm)> Streams(byte[] b)
    {
        int Be32(int at) => at >= 0 && at + 4 <= b.Length ? (int)BinaryPrimitives.ReadUInt32BigEndian(b.AsSpan(at)) : throw new InvalidDataException("truncated NUB");
        if (Be32(0) != 0x0002_0100) throw new InvalidDataException("not a NUB v2.1 bank");
        var (count, data, table) = (Be32(0x0C), Be32(0x10), Be32(0x18));
        return Enumerable.Range(0, count).Select(i =>
        {
            var h = Be32(table + i * 4);
            if (h < 0 || h + 4 > b.Length || !b.AsSpan(h, 4).SequenceEqual("vag\0"u8)) throw new InvalidDataException($"stream {i} isn't VAG");
            var (size, off) = (Be32(h + 0x14), Be32(h + 0x18));
            if (data + off + size > b.Length) throw new InvalidDataException("stream out of range");
            return (Be32(h + 0xBC), Vag(b.AsSpan(data + off, size)));
        }).ToList();
    }

    /// <summary>PS-ADPCM: 16-byte frames of a predictor/shift byte, a flags byte (7 ends the stream) and 28 4-bit samples,
    /// low nibble first.</summary>
    public static short[] Vag(ReadOnlySpan<byte> b)
    {
        ReadOnlySpan<(int, int)> filters = [(0, 0), (60, 0), (115, -52), (98, -55), (122, -60)];
        var (h1, h2) = (0, 0);
        var output = new List<short>(b.Length / 16 * 28);
        for (var f = 0; f + 16 <= b.Length; f += 16)
        {
            var frame = b.Slice(f, 16);
            if (frame[1] == 7) break;
            var (f0, f1) = filters[Math.Min(frame[0] >> 4, 4)];
            var shift = frame[0] & 0xF;
            for (var k = 0; k < 28; k++)
            {
                var nibble = (frame[2 + k / 2] >> ((k & 1) * 4)) & 0xF;
                var s = Math.Clamp(((short)(nibble << 12) >> shift) + ((h1 * f0 + h2 * f1 + 32) >> 6), short.MinValue, short.MaxValue);
                output.Add((short)s);
                (h2, h1) = (h1, s);
            }
        }
        return [.. output];
    }

    /// <summary>A mono 16-bit PCM WAV.</summary>
    public static byte[] Wav(int rate, short[] pcm)
    {
        var bytes = pcm.Length * 2;
        var w = new byte[44 + bytes];
        "RIFF"u8.CopyTo(w);
        BinaryPrimitives.WriteInt32LittleEndian(w.AsSpan(4), 36 + bytes);
        "WAVEfmt "u8.CopyTo(w.AsSpan(8));
        int[] fmt = [16, 1 | (1 << 16), rate, rate * 2, 2 | (16 << 16)];
        for (var i = 0; i < fmt.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(w.AsSpan(16 + i * 4), fmt[i]);
        "data"u8.CopyTo(w.AsSpan(36));
        BinaryPrimitives.WriteInt32LittleEndian(w.AsSpan(40), bytes);
        for (var i = 0; i < pcm.Length; i++) BinaryPrimitives.WriteInt16LittleEndian(w.AsSpan(44 + i * 2), pcm[i]);
        return w;
    }

    /// <summary><c>sound/se/SE_GAME_NEIRO_&lt;id&gt;_C.nub</c> → <c>tone/&lt;id&gt;_{don,ka}.wav</c>: streams 0 and 1
    /// (measured, not heard: the don crosses zero ~550 times a second, the ka 4,000–7,000). Stream 2 is the same sound in
    /// every bank. <c>_L</c> and <c>_R</c> are the same sounds placed left and right; the centred ones are enough for a
    /// preview.</summary>
    public static Report Convert(Era era, GameSource src, string outDir)
    {
        var report = new Report($"{era.Name()} tones");
        var files = src.Files(p => p.StartsWith(Prefix, StringComparison.Ordinal) && p.EndsWith("_C.nub", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal).ToList();
        if (files.Count == 0) return report;
        var dir = Path.Combine(outDir, "tone");
        Directory.CreateDirectory(dir);
        foreach (var rel in files)
        {
            var id = rel[Prefix.Length..^"_C.nub".Length];
            if (src.IsFresh(Path.Combine(dir, $"{id}_don.wav"), rel))
            {
                report.UpToDate.Add(id);
                continue;
            }
            try
            {
                foreach (var ((rate, pcm), hit) in Streams(src.Read(rel)).Zip(["don", "ka"]))
                    File.WriteAllBytes(Path.Combine(dir, $"{id}_{hit}.wav"), Wav(rate, pcm));
                report.Written.Add(id);
            }
            catch (InvalidDataException e)
            {
                report.Failed.Add($"{id}: {e.Message}");
            }
        }
        return report;
    }
}
