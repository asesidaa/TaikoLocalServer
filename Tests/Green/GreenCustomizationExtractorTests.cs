using System.Buffers.Binary;
using System.Text;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

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

    private static int Align4(int value) => (value + 3) & ~3;
}
