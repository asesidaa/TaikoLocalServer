using System.Buffers.Binary;
using System.Text;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Merging;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Sources;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenCustomizationExtractorTests
{
    [Fact]
    public void NdpReader_ReadsNutPackType1Entries()
    {
        var blob = BuildNdp(("cos_name_000.nut", 0x40u, 0x20u), ("cos_name_001.nut", 0x60u, 0x30u));

        var entries = NdpReader.Read(blob);

        Assert.Equal(2, entries.Count);
        Assert.Equal("cos_name_000.nut", entries[0].FileName);
        Assert.Equal(0x40u, entries[0].Offset);
        Assert.Equal(0x20u, entries[0].Size);
        Assert.Equal((uint)1, entries[1].Id);
    }

    [Fact]
    public void NdpReader_ReadsGreenMountedEntries()
    {
        var blob = BuildMountedNdp(("cos_name_000.nut", 0x40u, 0x20u), ("cos_name_001.nut", 0x60u, 0x30u));

        var entries = NdpReader.Read(blob);

        Assert.Equal(2, entries.Count);
        Assert.Equal("cos_name_000.nut", entries[0].FileName);
        Assert.Equal(0x40u, entries[0].Offset);
        Assert.Equal(0x20u, entries[0].Size);
        Assert.Equal((uint)1, entries[1].Id);
    }

    [Fact]
    public void NdpReader_IgnoresMountedFilenamePatternInsidePayload()
    {
        var blob = BuildMountedNdp(
            ("cos_name_000.nut", 0x05782E6Eu, 0x757400AAu),
            ("cos_name_001.nut", 0x60u, 0x30u));

        var entries = NdpReader.Read(blob);

        Assert.Equal(new[] { "cos_name_000.nut", "cos_name_001.nut" }, entries.Select(entry => entry.FileName));
    }

    [Fact]
    public void NdpReader_ThrowsWhenMountedDeclaredCountCannotBeSatisfied()
    {
        var blob = BuildMountedNdp(("cos_name_000.nut", 0x40u, 0x20u));
        BinaryPrimitives.WriteUInt16BigEndian(blob.AsSpan(0x40), 2);

        Assert.Throws<InvalidDataException>(() => NdpReader.Read(blob));
    }

    [Fact]
    public void NdpReader_ThrowsWhenMountedEntryPayloadIsTruncated()
    {
        var blob = BuildMountedNdpWithTruncatedPayload("cos_name_000.nut", payloadBytes: 7);

        Assert.Throws<InvalidDataException>(() => NdpReader.Read(blob));
    }

    [Fact]
    public void NdpReader_ThrowsWhenFixedEntryIsTruncated()
    {
        var blob = BuildMalformedFixedNdp(nameLength: 16, payloadBytes: 0);

        Assert.Throws<InvalidDataException>(() => NdpReader.Read(blob));
    }

    [Fact]
    public void NdpReader_ThrowsWhenFixedEntryNameIsMalformed()
    {
        var blob = BuildMalformedFixedNdp(nameLength: 4, payloadBytes: 8);
        Encoding.ASCII.GetBytes("bad!").CopyTo(blob.AsSpan(0x54));

        Assert.Throws<InvalidDataException>(() => NdpReader.Read(blob));
    }

    [Fact]
    public void NdpReader_ThrowsWhenFixedEntryNameLengthIsZero()
    {
        var blob = BuildMalformedFixedNdp(nameLength: 0, payloadBytes: 8);

        Assert.Throws<InvalidDataException>(() => NdpReader.Read(blob));
    }

    [Fact]
    public async Task BoostXmlReader_ReadsRewardTitleIds()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xml");
        await File.WriteAllTextAsync(path, """
            <?xml version="1.0" encoding="UTF-8" standalone="yes" ?>
            <boost_serialization signature="serialization::archive" version="10">
              <RewardTitleFiltering>
                <support>
                  <size>2</size>
                  <rewardtitle>131</rewardtitle>
                  <rewardtitle>132</rewardtitle>
                </support>
              </RewardTitleFiltering>
            </boost_serialization>
            """);

        var ids = await BoostXmlReader.ReadRewardTitleIdsAsync(path, CancellationToken.None);

        Assert.Equal(new uint[] { 131, 132 }, ids);
        File.Delete(path);
    }

    [Fact]
    public async Task Don3dDirScanner_ReadsModelPairs()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "don3d", "cos"));
        await File.WriteAllTextAsync(Path.Combine(root, "don3d", "cos", "cos_000007.nud"), string.Empty);
        await File.WriteAllTextAsync(Path.Combine(root, "don3d", "cos", "cos_000007.nut"), string.Empty);
        await File.WriteAllTextAsync(Path.Combine(root, "don3d", "cos", "cos_000008.nud"), string.Empty);

        var result = Don3dDirScanner.Scan(root);

        Assert.Equal(new uint[] { 7 }, result.FullCosModelPairIds);
        Directory.Delete(root, recursive: true);
    }

    [Fact]
    public async Task Don3dDirScanner_UsesRecursiveSlotDirectoriesForCostumeTypes()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "don3d", "full", "cos", "event"));
        Directory.CreateDirectory(Path.Combine(root, "don3d", "parts", "head", "campaign"));
        await File.WriteAllTextAsync(Path.Combine(root, "don3d", "full", "cos", "event", "cos_111000.nud"), string.Empty);
        await File.WriteAllTextAsync(Path.Combine(root, "don3d", "full", "cos", "event", "cos_111000.nut"), string.Empty);
        await File.WriteAllTextAsync(Path.Combine(root, "don3d", "parts", "head", "campaign", "head_222000.nud"), string.Empty);
        await File.WriteAllTextAsync(Path.Combine(root, "don3d", "parts", "head", "campaign", "head_222000.nut"), string.Empty);

        var scan = Don3dDirScanner.Scan(root);
        var costumes = CostumeMerger.Merge(
            [
                new NdpEntry(111, "cos_name_111.nut", 0, 1),
                new NdpEntry(222, "cos_name_222.nut", 0, 1)
            ],
            scan,
            new GreenCatalogOverrides());

        Assert.Equal("kigurumi", Assert.Single(costumes, costume => costume.CostumeId == 111).CostumeType);
        Assert.Equal("head", Assert.Single(costumes, costume => costume.CostumeId == 222).CostumeType);
        Directory.Delete(root, recursive: true);
    }

    [Fact]
    public async Task Don3dDirScanner_UsesGreenTrailingThousandsIdsForSlotTypes()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "don3d", "full", "cos"));
        Directory.CreateDirectory(Path.Combine(root, "don3d", "parts", "head"));
        Directory.CreateDirectory(Path.Combine(root, "don3d", "parts", "body"));
        Directory.CreateDirectory(Path.Combine(root, "don3d", "parts", "paint"));
        Directory.CreateDirectory(Path.Combine(root, "don3d", "parts", "acc"));

        await WritePairAsync(root, "don3d", "full", "cos", "cos_001000");
        await WritePairAsync(root, "don3d", "parts", "head", "head_001000");
        await WritePairAsync(root, "don3d", "parts", "body", "body_001000");
        await File.WriteAllTextAsync(Path.Combine(root, "don3d", "parts", "paint", "paint_001000.nut"), string.Empty);
        await WritePairAsync(root, "don3d", "parts", "acc", "acc_001000");

        var scan = Don3dDirScanner.Scan(root);
        var costumes = CostumeMerger.Merge(
            [new NdpEntry(1, "costume_name_001.nut", 0, 1)],
            scan,
            new GreenCatalogOverrides());

        Assert.Contains(costumes, costume => costume.CostumeId == 1 && costume.CostumeType == "kigurumi");
        Assert.Contains(costumes, costume => costume.CostumeId == 1 && costume.CostumeType == "head");
        Assert.Contains(costumes, costume => costume.CostumeId == 1 && costume.CostumeType == "body");
        Assert.Contains(costumes, costume => costume.CostumeId == 1 && costume.CostumeType == "face");
        Assert.Contains(costumes, costume => costume.CostumeId == 1 && costume.CostumeType == "puchi");
        Directory.Delete(root, recursive: true);
    }

    [Fact]
    public async Task Don3dDirScanner_IgnoresGreenPaintVariantTextureIds()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "don3d", "full", "cos"));
        Directory.CreateDirectory(Path.Combine(root, "don3d", "parts", "paint"));

        await WritePairAsync(root, "don3d", "full", "cos", "cos_036001");
        await File.WriteAllTextAsync(Path.Combine(root, "don3d", "parts", "paint", "paint_012000.nut"), string.Empty);
        await File.WriteAllTextAsync(Path.Combine(root, "don3d", "parts", "paint", "paint_012001.nut"), string.Empty);

        var scan = Don3dDirScanner.Scan(root);
        var costumes = CostumeMerger.Merge(
            [
                new NdpEntry(12, "costume_name_012.nut", 0, 1),
                new NdpEntry(12001, "costume_name_12001.nut", 0, 1),
                new NdpEntry(36001, "costume_name_36001.nut", 0, 1)
            ],
            scan,
            new GreenCatalogOverrides());

        Assert.Equal("face", Assert.Single(costumes, costume => costume.CostumeId == 12).CostumeType);
        Assert.Equal("unknown", Assert.Single(costumes, costume => costume.CostumeId == 12001).CostumeType);
        Assert.Equal("unknown", Assert.Single(costumes, costume => costume.CostumeId == 36001).CostumeType);
        Directory.Delete(root, recursive: true);
    }

    [Fact]
    public async Task Don3dDirScanner_UsesRecursiveDirectoryIdWhenModelNamesAreGeneric()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "don3d", "parts", "body", "body_000033"));
        await File.WriteAllTextAsync(Path.Combine(root, "don3d", "parts", "body", "body_000033", "model.nud"), string.Empty);
        await File.WriteAllTextAsync(Path.Combine(root, "don3d", "parts", "body", "body_000033", "model.nut"), string.Empty);

        var scan = Don3dDirScanner.Scan(root);
        var costumes = CostumeMerger.Merge(
            [new NdpEntry(33, "cos_name_033.nut", 0, 1)],
            scan,
            new GreenCatalogOverrides());

        Assert.Equal("body", Assert.Single(costumes).CostumeType);
        Directory.Delete(root, recursive: true);
    }

    [Fact]
    public void CostumeMerger_AppendsOverridesSourceWhenOverrideContributesData()
    {
        var overrides = new GreenCatalogOverrides
        {
            Costumes = new Dictionary<uint, GreenCostumeOverride>
            {
                [2] = new() { Name = "Override Costume", CostumeType = "body" }
            }
        };
        var don3d = new Don3dScanResult(
            [2],
            new Dictionary<string, IReadOnlyList<uint>> { ["don3d/cos"] = [2] });

        var costumes = CostumeMerger.Merge([new NdpEntry(2, "cos_name_002.nut", 0, 1)], don3d, overrides);

        Assert.Equal("ndp+don3d+overrides", costumes.Single().Source);
    }

    [Fact]
    public void CostumeMerger_UsesDon3dDirectorySlotMap()
    {
        var ndp = new[]
        {
            new NdpEntry(7, "cos_name_007.nut", 0, 1)
        };
        var scan = new Don3dScanResult(
            FullCosModelPairIds: [],
            DirectoryIds: new Dictionary<string, IReadOnlyList<uint>>
            {
                ["don3d/parts/head"] = [7]
            });

        var items = CostumeMerger.Merge(ndp, scan, new GreenCatalogOverrides());

        var costume = Assert.Single(items);
        Assert.Equal("head", costume.CostumeType);
    }

    [Fact]
    public void TitleMerger_AppendsOverridesSourceWhenOverrideContributesData()
    {
        var overrides = new GreenCatalogOverrides
        {
            Titles = new Dictionary<uint, GreenNamedOverride>
            {
                [131] = new() { Name = "Override Title" }
            }
        };

        var titles = TitleMerger.Merge([new NdpEntry(131, "title_name_131.nut", 0, 1)], [131u], overrides);

        Assert.Equal("ndp+rewardtitlefiltering+overrides", titles.Single().Source);
    }

    [Fact]
    public void NeiroMerger_AppendsOverridesSourceWhenOverrideContributesData()
    {
        var overrides = new GreenCatalogOverrides
        {
            Neiros = new Dictionary<uint, GreenNamedOverride>
            {
                [4] = new() { Name = "Override Neiro" }
            }
        };

        var neiros = NeiroMerger.Merge([new NdpEntry(4, "tone_name_004.nut", 0, 1)], overrides);

        Assert.Equal("ndp+overrides", neiros.Single().Source);
    }

    [Fact]
    public async Task GreenCatalogExtractor_WritesDeterministicIdsOnlyCatalogs()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var outDir = Path.Combine(root, "out");
        Directory.CreateDirectory(Path.Combine(root, "nutdata", "cos_name"));
        Directory.CreateDirectory(Path.Combine(root, "nutdata", "title_name"));
        Directory.CreateDirectory(Path.Combine(root, "nutdata", "tone_name"));
        Directory.CreateDirectory(Path.Combine(root, "config", "S11100-1"));
        Directory.CreateDirectory(Path.Combine(root, "don3d", "cos"));

        await File.WriteAllBytesAsync(Path.Combine(root, "nutdata", "cos_name", "nutdatapack.ndp"), BuildNdp(("cos_name_002.nut", 0, 1), ("cos_name_001.nut", 1, 1)));
        await File.WriteAllBytesAsync(Path.Combine(root, "nutdata", "title_name", "nutdatapack.ndp"), BuildNdp(("title_name_132.nut", 0, 1), ("title_name_131.nut", 1, 1)));
        await File.WriteAllBytesAsync(Path.Combine(root, "nutdata", "tone_name", "nutdatapack.ndp"), BuildNdp(("tone_name_004.nut", 0, 1)));
        await File.WriteAllTextAsync(Path.Combine(root, "config", "S11100-1", "rewardtitlefiltering.xml"), """
            <boost_serialization>
              <RewardTitleFiltering>
                <support>
                  <rewardtitle>131</rewardtitle>
                </support>
              </RewardTitleFiltering>
            </boost_serialization>
            """);
        await File.WriteAllTextAsync(Path.Combine(root, "don3d", "cos", "cos_000002.nud"), string.Empty);
        await File.WriteAllTextAsync(Path.Combine(root, "don3d", "cos", "cos_000002.nut"), string.Empty);

        await GreenCatalogExtractor.ExtractAsync(
            new GreenExtractorOptions(GameDataPath: root, OutputDirectory: outDir),
            CancellationToken.None);

        var costumeJson = await File.ReadAllTextAsync(Path.Combine(outDir, "green_costume_data.json"));
        var titleJson = await File.ReadAllTextAsync(Path.Combine(outDir, "green_title_data.json"));
        var neiroJson = await File.ReadAllTextAsync(Path.Combine(outDir, "green_neiro_data.json"));

        Assert.Contains("\"schemaVersion\": 1", costumeJson);
        Assert.True(costumeJson.IndexOf("\"costumeId\": 1", StringComparison.Ordinal) < costumeJson.IndexOf("\"costumeId\": 2", StringComparison.Ordinal));
        Assert.Contains("\"source\": \"ndp+don3d\"", costumeJson);
        Assert.Contains("\"titleId\": 131", titleJson);
        Assert.Contains("\"source\": \"ndp+rewardtitlefiltering\"", titleJson);
        Assert.Contains("\"neiroId\": 4", neiroJson);

        Directory.Delete(root, recursive: true);
    }

    [Fact]
    public async Task GreenCatalogExtractor_ReadsRecursiveGreenCostumeNamePacks()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var outDir = Path.Combine(root, "out");
        Directory.CreateDirectory(Path.Combine(root, "nutdata", "cos_name"));
        Directory.CreateDirectory(Path.Combine(root, "nutdata", "title_name"));
        Directory.CreateDirectory(Path.Combine(root, "nutdata", "tone_name"));
        Directory.CreateDirectory(Path.Combine(root, "nutdata", "pack", "ST4100-1", "00", "costume_head_name"));
        Directory.CreateDirectory(Path.Combine(root, "nutdata", "pack", "ST4100-1", "00", "costume_body_name"));
        Directory.CreateDirectory(Path.Combine(root, "nutdata", "pack", "ST4100-1", "00", "costume_name"));
        Directory.CreateDirectory(Path.Combine(root, "don3d", "full", "cos"));
        Directory.CreateDirectory(Path.Combine(root, "don3d", "parts", "head"));
        Directory.CreateDirectory(Path.Combine(root, "don3d", "parts", "body"));

        await File.WriteAllBytesAsync(Path.Combine(root, "nutdata", "cos_name", "nutdatapack.ndp"), BuildNdp(("cos_name_001.nut", 0, 1)));
        await File.WriteAllBytesAsync(Path.Combine(root, "nutdata", "title_name", "nutdatapack.ndp"), BuildNdp(("title_name_131.nut", 0, 1)));
        await File.WriteAllBytesAsync(Path.Combine(root, "nutdata", "tone_name", "nutdatapack.ndp"), BuildNdp(("tone_name_004.nut", 0, 1)));
        await File.WriteAllBytesAsync(Path.Combine(root, "nutdata", "pack", "ST4100-1", "00", "costume_head_name", "nutdatapack.ndp"), BuildNdp(("costume_head_name_002.nut", 0, 1)));
        await File.WriteAllBytesAsync(Path.Combine(root, "nutdata", "pack", "ST4100-1", "00", "costume_body_name", "nutdatapack.ndp"), BuildNdp(("costume_body_name_003.nut", 0, 1)));
        await File.WriteAllBytesAsync(Path.Combine(root, "nutdata", "pack", "ST4100-1", "00", "costume_name", "nutdatapack.ndp"), BuildNdp(("costume_name_004.nut", 0, 1)));
        await WritePairAsync(root, "don3d", "full", "cos", "cos_004000");
        await WritePairAsync(root, "don3d", "parts", "head", "head_002000");
        await WritePairAsync(root, "don3d", "parts", "body", "body_003000");

        await GreenCatalogExtractor.ExtractAsync(
            new GreenExtractorOptions(GameDataPath: root, OutputDirectory: outDir),
            CancellationToken.None);

        var costumeJson = await File.ReadAllTextAsync(Path.Combine(outDir, "green_costume_data.json"));

        Assert.Contains("\"costumeId\": 2", costumeJson);
        Assert.Contains("\"costumeType\": \"head\"", costumeJson);
        Assert.Contains("\"costumeId\": 3", costumeJson);
        Assert.Contains("\"costumeType\": \"body\"", costumeJson);
        Assert.Contains("\"costumeId\": 4", costumeJson);
        Assert.Contains("\"costumeType\": \"kigurumi\"", costumeJson);

        Directory.Delete(root, recursive: true);
    }

    [Fact]
    public async Task GreenCatalogExtractor_ThrowsForMissingGameDataRootWithoutWritingOutput()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var outDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
            GreenCatalogExtractor.ExtractAsync(
                new GreenExtractorOptions(GameDataPath: root, OutputDirectory: outDir),
                CancellationToken.None));

        Assert.False(Directory.Exists(outDir));
    }

    [Fact]
    public async Task GreenCatalogExtractor_ThrowsForMissingRequiredPackWithoutWritingOutput()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var outDir = Path.Combine(root, "out");
        Directory.CreateDirectory(Path.Combine(root, "nutdata", "cos_name"));
        Directory.CreateDirectory(Path.Combine(root, "nutdata", "title_name"));
        Directory.CreateDirectory(Path.Combine(root, "nutdata", "tone_name"));

        await File.WriteAllBytesAsync(Path.Combine(root, "nutdata", "cos_name", "nutdatapack.ndp"), BuildNdp(("cos_name_001.nut", 0, 1)));
        await File.WriteAllBytesAsync(Path.Combine(root, "nutdata", "title_name", "nutdatapack.ndp"), BuildNdp(("title_name_131.nut", 0, 1)));

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            GreenCatalogExtractor.ExtractAsync(
                new GreenExtractorOptions(GameDataPath: root, OutputDirectory: outDir),
                CancellationToken.None));

        Assert.False(File.Exists(Path.Combine(outDir, "green_costume_data.json")));
        Assert.False(File.Exists(Path.Combine(outDir, "green_title_data.json")));
        Assert.False(File.Exists(Path.Combine(outDir, "green_neiro_data.json")));

        Directory.Delete(root, recursive: true);
    }

    private static byte[] BuildNdp(params (string Name, uint Offset, uint Size)[] entries)
    {
        var bytes = new byte[0x50 + entries.Sum(entry => 4 + Align4(Encoding.ASCII.GetByteCount(entry.Name) + 1) + 8)];
        Encoding.ASCII.GetBytes("NUT_PACK_TYPE1").CopyTo(bytes, 0);
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(0x40), (uint)entries.Length);
        var cursor = 0x50;

        foreach (var entry in entries)
        {
            var nameBytes = Encoding.ASCII.GetBytes(entry.Name);
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(cursor), (uint)(nameBytes.Length + 1));
            cursor += 4;
            nameBytes.CopyTo(bytes.AsSpan(cursor));
            cursor += nameBytes.Length + 1;
            cursor = Align4(cursor);
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(cursor), entry.Offset);
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(cursor + 4), entry.Size);
            cursor += 8;
        }

        return bytes;
    }

    private static byte[] BuildMountedNdp(params (string Name, uint Offset, uint Size)[] entries)
    {
        var bytes = new byte[0x4E + entries.Sum(entry => 1 + Encoding.ASCII.GetByteCount(entry.Name) + 1 + 8)];
        Encoding.ASCII.GetBytes("NUT_PACK_TYPE1").CopyTo(bytes, 0);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(0x40), (ushort)entries.Length);
        var cursor = 0x4E;

        foreach (var entry in entries)
        {
            var nameBytes = Encoding.ASCII.GetBytes(entry.Name);
            bytes[cursor] = (byte)nameBytes.Length;
            cursor += 1;
            nameBytes.CopyTo(bytes.AsSpan(cursor));
            cursor += nameBytes.Length;
            bytes[cursor] = 0;
            cursor += 1;
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(cursor), entry.Offset);
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(cursor + 4), entry.Size);
            cursor += 8;
        }

        return bytes;
    }

    private static byte[] BuildMountedNdpWithTruncatedPayload(string name, int payloadBytes)
    {
        var nameBytes = Encoding.ASCII.GetBytes(name);
        var bytes = new byte[0x4E + 1 + nameBytes.Length + 1 + payloadBytes];
        Encoding.ASCII.GetBytes("NUT_PACK_TYPE1").CopyTo(bytes, 0);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(0x40), 1);

        var cursor = 0x4E;
        bytes[cursor] = (byte)nameBytes.Length;
        cursor += 1;
        nameBytes.CopyTo(bytes.AsSpan(cursor));
        cursor += nameBytes.Length;
        bytes[cursor] = 0;
        cursor += 1;

        for (var index = 0; index < payloadBytes; index++)
        {
            bytes[cursor + index] = 0xAA;
        }

        return bytes;
    }

    private static byte[] BuildMalformedFixedNdp(uint nameLength, int payloadBytes)
    {
        var bytes = new byte[0x54 + payloadBytes];
        Encoding.ASCII.GetBytes("NUT_PACK_TYPE1").CopyTo(bytes, 0);
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(0x40), 1);
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(0x50), nameLength);
        return bytes;
    }

    private static int Align4(int value) => (value + 3) & ~3;

    private static async Task WritePairAsync(string root, params string[] pathParts)
    {
        var pathWithoutExtension = Path.Combine([root, .. pathParts]);
        await File.WriteAllTextAsync($"{pathWithoutExtension}.nud", string.Empty);
        await File.WriteAllTextAsync($"{pathWithoutExtension}.nut", string.Empty);
    }
}
