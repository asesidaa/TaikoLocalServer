using System.Text.Json.Nodes;
using TaikoLocalServer.Infrastructure.Extract;
using TaikoLocalServer.Infrastructure.Extract.Nijiiro;
using TaikoLocalServer.Infrastructure.Extract.Textures;

namespace TaikoLocalServer.Tests.Extract;

// Nijiiro's file formats as Infrastructure.Extract reads them (Infrastructure.Extract/README.md documents each).
public sealed class NijiiroExtractTests
{
    // A minimal linear (PC-style) R8G8B8A8 nutexb with Nijiiro's "46XT" footer magic.
    private static byte[] NijiiroRgba8(int width, int height, int layers, byte[] pixels)
    {
        var b = new List<byte>(pixels);
        for (var l = 0; l < layers; l++)
        {
            var mips = new byte[0x40];
            BitConverter.GetBytes(width * height * 4).CopyTo(mips, 0);
            b.AddRange(mips);
        }
        b.AddRange("46XT"u8.ToArray());
        b.AddRange(new byte[0x40]); // name
        foreach (var v in new[] { width, height, 1, 0x0400, 4, 1, 0, layers, pixels.Length }) b.AddRange(BitConverter.GetBytes(v));
        b.AddRange(" XET"u8.ToArray());
        b.AddRange([1, 0, 2, 0]);
        return [.. b];
    }

    [Fact]
    public void Nutexb_StacksArrayLayersTopToBottom()
    {
        var pixels = Enumerable.Range(0, 2 * 2 * 4 * 2).Select(i => (byte)i).ToArray(); // 2x2, 2 layers
        var img = Nutexb.Decode(NijiiroRgba8(2, 2, 2, pixels));
        Assert.Equal((2, 4), (img.Width, img.Height));
        Assert.Equal(pixels, img.Pixels);
    }

    private static byte[] Wav(string body)
    {
        var b = new List<byte>("RIFF"u8.ToArray());
        b.AddRange(BitConverter.GetBytes(4 + body.Length));
        b.AddRange("WAVE"u8.ToArray());
        b.AddRange(System.Text.Encoding.ASCII.GetBytes(body));
        return [.. b];
    }

    [Fact]
    public void Tones_FindWholeWavsAndSkipStrayMagic()
    {
        var (don, ka) = (Wav("fmt don"), Wav("fmt RIFF ka"));
        byte[] bank = [.. "NUS3 RIFF junk"u8.ToArray(), .. don, .. "PACK"u8.ToArray(), .. ka];
        var wavs = NijiiroExtractor.Wavs(bank);
        Assert.Equal(2, wavs.Count);
        Assert.Equal(don, wavs[0]);
        Assert.Equal(ka, wavs[1]);
    }

    [Fact]
    public void Movies_NumberedOnly()
    {
        string[] names = ["00000012.wmv", "00000001.wmv", "attractdemo_en_us.wmv", "00000003.WMV", "readme.txt", ".wmv", "00000001.wmv"];
        Assert.Equal([1u, 3, 12], NijiiroExtractor.MovieIds(names));
    }

    [Fact]
    public void FontPages_SplitGlyphsAndSkipPlaceholders()
    {
        const string xml = """
            <root><font texWidth="8" texHeight="4" fontPoint="32">
                <glyph index="65" type="1" offsetU="0" offsetV="0" width="2" height="2" />
                <glyph index="66" type="1" offsetU="0" offsetV="0" width="4" height="1" />
                <glyph index="12354" type="1" offsetU="2" offsetV="0" width="3" height="4" />
            </font></root>
            """;
        var sheet = new RgbaImage(8, 4);
        new byte[] { 1, 2, 3, 4 }.CopyTo(sheet.Pixel(2, 3));
        var dst = Path.Combine(Path.GetTempPath(), $"extract-font-test-{Guid.NewGuid():N}");
        try
        {
            Assert.Equal(2, FontPages.Split(xml, sheet, dst)); // ASCII page 0 and page 0x30
            var page0 = JsonNode.Parse(File.ReadAllText(Path.Combine(dst, "0.json")))!;
            Assert.Equal(32, page0["point"]!.GetValue<int>());
            Assert.Equal("[0,0,2,2]", page0["glyphs"]!["65"]!.ToJsonString());
            Assert.Null(page0["glyphs"]!["66"]); // the placeholder
            Assert.Equal("[0,48]", File.ReadAllText(Path.Combine(dst, "pages.json")));
        }
        finally
        {
            Directory.Delete(dst, true);
        }
    }

    [Fact]
    public void NoteIcon_KeepsTheShapeAboveTheCaptionOnASquare()
    {
        var img = new RgbaImage(20, 20);
        for (var y = 2; y < 8; y++) for (var x = 4; x < 14; x++) new byte[] { 255, 0, 0, 255 }.CopyTo(img.Pixel(x, y)); // the note: 10 x 6
        for (var y = 12; y < 15; y++) for (var x = 1; x < 19; x++) new byte[] { 0, 0, 0, 255 }.CopyTo(img.Pixel(x, y)); // the caption
        var icon = NijiiroUi.FirstBlob(img)!;
        Assert.Equal((10, 10), (icon.Width, icon.Height));
        Assert.Equal(0, icon.Pixel(0, 1)[3]); // padding above the note
        Assert.Equal([255, 0, 0, 255], icon.Pixel(0, 2).ToArray());
        Assert.Null(NijiiroUi.FirstBlob(new RgbaImage(4, 4)));
    }

    // A game update that changed only the datatables: they are copied again (even when not older, as a mod layer may have
    // changed), the rest of the extract is left alone, and the last run's other steps stay in extract.json.
    [Fact]
    public void DatatablesOnly_CopiesThemAndKeepsTheRest()
    {
        var root = Directory.CreateTempSubdirectory("tls-dt-").FullName;
        try
        {
            var (game, outDir) = (Path.Combine(root, "game"), Path.Combine(root, "out"));
            var src = Directory.CreateDirectory(Path.Combine(game, "Data", "x64", "datatable")).FullName;
            File.WriteAllText(Path.Combine(src, "musicinfo.bin"), "new");
            var dst = Directory.CreateDirectory(Path.Combine(outDir, "data", "datatable")).FullName;
            File.WriteAllText(Path.Combine(dst, "musicinfo.bin"), "old");
            File.SetLastWriteTimeUtc(Path.Combine(dst, "musicinfo.bin"), DateTime.UtcNow.AddDays(1));
            var model = Directory.CreateDirectory(Path.Combine(outDir, NijiiroExtractor.GameData, "body")).FullName;
            File.WriteAllText(Path.Combine(model, "body_0001.glb"), "kept");
            File.WriteAllText(Path.Combine(outDir, "data", "extract.json"),
                """{"era":"nijiiro","steps":[{"label":"datatables","written":0},{"label":"body models","written":5}],"notes":[]}""");

            ExtractCli.Run(["--game", game, "--datatables"], outDir, TextWriter.Null, TextWriter.Null);

            Assert.Equal("new", File.ReadAllText(Path.Combine(dst, "musicinfo.bin")));
            Assert.Equal("kept", File.ReadAllText(Path.Combine(model, "body_0001.glb")));
            var steps = JsonNode.Parse(File.ReadAllText(Path.Combine(outDir, "data", "extract.json")))!["steps"]!.AsArray();
            Assert.Equal(["datatables", "body models"], steps.Select(st => (string)st!["label"]!).ToList());
            Assert.Equal(1, (int)steps[0]!["written"]!);
            Assert.Equal(5, (int)steps[1]!["written"]!);
        }
        finally { Directory.Delete(root, true); }
    }
}
