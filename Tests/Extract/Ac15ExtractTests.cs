using System.Buffers.Binary;
using TaikoLocalServer.Infrastructure.Extract.Ac15;
using TaikoLocalServer.Infrastructure.Extract.Lumen;
using TaikoLocalServer.Infrastructure.Extract.Textures;

namespace TaikoLocalServer.Tests.Extract;

// The older games' file formats as Infrastructure.Extract reads them (Infrastructure.Extract/README.md documents each).
public sealed class Ac15ExtractTests
{
    [Fact]
    public void UnnumberedNamePacks_ListOneNamePerItem()
    {
        Assert.True(ServerFiles.UnnumberedPack("nutdata/pack/ST4100-1/00/title_name/nutdatapack.ndp"));
        Assert.False(ServerFiles.UnnumberedPack("nutdata/pack/ST5100-1/00/title_name/nutdatapack_00000_00099.ndp"));
        Assert.False(ServerFiles.UnnumberedPack("nutdata/cos_name/nutdatapack.ndp"));
        // A NUT pack of shougou_007.nut (one texture) and image.nut (three, one per item).
        (string Name, ushort Textures)[] entries = [("shougou_007.nut", 1), ("image.nut", 3)];
        var b = new List<byte>("NUT_PACK_TYPE1\0\0"u8.ToArray());
        void Pad(int to) { while (b.Count < to) b.Add(0); }
        void Be32(int v) { var w = new byte[4]; BinaryPrimitives.WriteInt32BigEndian(w, v); b.AddRange(w); }
        Pad(0x3E);
        Be32(entries.Length);
        Pad(0x4B);
        for (var i = 0; i < entries.Length; i++)
        {
            Be32(entries[i].Name.Length);
            b.AddRange(System.Text.Encoding.ASCII.GetBytes(entries[i].Name));
            if (i == 0) b.AddRange(new byte[5]);
            Be32(i * 8);
            Be32(8);
        }
        Be32(16);
        foreach (var (_, count) in entries)
            b.AddRange([.. "NTP3\x01\x00"u8, (byte)(count >> 8), (byte)count]);

        Assert.Equal(["shougou_007.nut", "shougou_000.nut", "shougou_001.nut", "shougou_002.nut"],
            ServerFiles.EntryNames("nutdata/pack/x/00/title_name/nutdatapack.ndp", [.. b]));
        Assert.Equal("tone_name_000.nut", ServerFiles.EntryNames("nutdata/pack/x/00/tone_name/nutdatapack.ndp", [.. b])[1]);
    }

    [Fact]
    public void Vag_DecodesPsAdpcmFrames()
    {
        // Filter 0, shift 0: each nibble is the sample's top 4 bits (1 → 4096, 0xF → -4096); flags 7 ends the stream.
        var b = new byte[32];
        b[2] = 0xF1;
        b[17] = 7;
        var pcm = Tones.Vag(b);
        Assert.Equal(28, pcm.Length);
        Assert.Equal([4096, -4096, 0], pcm[..3]);
        // Filter 1 carries 60/64 of the previous sample.
        var f1 = new byte[16];
        (f1[0], f1[2]) = (0x10, 0x01);
        Assert.Equal([4096, 3840], Tones.Vag(f1)[..2]);
    }

    [Fact]
    public void AlphaMode_FollowsTheMaterialNotTheTexture()
    {
        // The bodies' recolour pass is opaque: its texture's alpha holds mask data, not transparency.
        Assert.Equal(("OPAQUE", (float?)null), Nud.AlphaMode(new Nud.Part()));
        Assert.Equal(("MASK", (float?)1f), Nud.AlphaMode(new Nud.Part { AlphaRef = 255 }));
        Assert.Equal(("BLEND", (float?)null), Nud.AlphaMode(new Nud.Part { Blend = true, AlphaRef = 255 }));
    }

    // Minimal little-endian LMB: header, then (tag, payload words), as Nijiiro's files are laid out.
    private static byte[] Lmb(params (uint Tag, uint[] Words)[] tags)
    {
        var b = new List<byte>("LMB\0"u8.ToArray());
        b.AddRange(new byte[0x3C]);
        void Word(uint v) => b.AddRange(BitConverter.GetBytes(v));
        foreach (var (tag, words) in tags.Append((0xFF00u, [])))
        {
            Word(tag);
            Word((uint)words.Length);
            foreach (var w in words) Word(w);
        }
        return [.. b];
    }

    private static uint F(float v) => BitConverter.SingleToUInt32Bits(v);

    // A solid-colour quad (colour index, x0 y0 x1 y1) as an F024 payload.
    private static uint[] SolidQuad(uint color, float x0, float y0, float x1, float y1) =>
        [color, 4u << 16, 6, F(x0), F(y0), 0, 0, F(x1), F(y0), 0, 0, F(x1), F(y1), 0, 0, F(x0), F(y1), 0, 0, 0 | (1u << 16), 2, 2 | (3u << 16)];

    // Place: character, mode 1, depth, clip, transform unchanged, no colours.
    private static uint[] Place(uint character, uint depth, uint clip) => [character, 0, 0, 0, 1, depth | (clip << 16), 0, 0xFFFF_0000, uint.MaxValue, uint.MaxValue, 0, 0];

    private const uint One = 0x100; // colour component 1.0

    [Fact]
    public void Lumen_ClipMaskLimitsContentAndIsNotDrawn()
    {
        var lm = LumenFile.Parse(Lmb(
            (0xF001, [1, 4, BitConverter.ToUInt32("wood"u8), 0]),
            (0xF002, [2, One | (One << 16), One | (One << 16), One, One << 16]), // white, red
            (0xF007, [1, 0, 0, F(4), F(4)]),
            (0xF022, [1, 0, 0, 0, 1]),
            (0xF024, SolidQuad(0, 0, 0, 2, 4)), // mask: left half
            (0xF022, [2, 0, 0, 0, 1]),
            (0xF024, SolidQuad(1, 0, 0, 4, 4)), // content: whole square, red
            (0x0027, [10, 0, 0, 1, 1, 1, 1]),
            (0x002B, [0, 0, 0]),
            (0x0001, [0, 2]),
            (0x0004, Place(1, 1, 2)),
            (0x0004, Place(2, 2, 0))));

        var img = lm.RenderLayers(10, "wood", [], [0, 0, 4, 4]);

        Assert.Equal([255, 0, 0, 255], img.Pixel(0, 1).ToArray());
        Assert.Equal(0, img.Pixel(3, 1)[3]);
        Assert.Equal([0, 0, 2, 4], lm.Bounds(10, "wood")!);
        Assert.Equal(10u, lm.SpriteWithLabel("wood"));
    }

    [Fact]
    public void Lumen_KeyframeOpsAreNotAddedToTheLastFrame()
    {
        var lm = LumenFile.Parse(Lmb(
            (0xF001, [1, 4, BitConverter.ToUInt32("wood"u8), 0]),
            (0xF002, [1, One | (One << 16), One | (One << 16)]),
            (0xF007, [1, 0, 0, F(4), F(4)]),
            (0xF022, [1, 0, 0, 0, 1]),
            (0xF024, SolidQuad(0, 0, 0, 1, 1)),
            (0xF022, [2, 0, 0, 0, 1]),
            (0xF024, SolidQuad(0, 0, 0, 9, 9)),
            (0x0027, [10, 0, 0, 1, 1, 1, 1]),
            (0x002B, [0, 0, 0]),
            (0x0001, [0, 1]),
            (0x0004, Place(1, 1, 0)),
            (0xF105, [0, 1]),
            (0x0004, Place(2, 2, 0)))); // a keyframe's snapshot, not part of frame 0

        Assert.Equal([0, 0, 1, 1], lm.Bounds(10, "wood")!);
    }
}
