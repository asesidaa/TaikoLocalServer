namespace TaikoLocalServer.Tests.Green;

public sealed class GreenProtocolBytesTests
{
    [Fact]
    public void CreateFixedBitset_SetsLittleEndianBits()
    {
        var bytes = GreenProtocolBytes.CreateFixedBitset([0, 1, 7, 8, 1023], 128);

        Assert.Equal(128, bytes.Length);
        Assert.Equal(0b1000_0011, bytes[0]);
        Assert.Equal(0b0000_0001, bytes[1]);
        Assert.Equal(0b1000_0000, bytes[127]);
    }

    [Fact]
    public void CreateFixedBitset_IgnoresOutOfRangeIds()
    {
        var bytes = GreenProtocolBytes.CreateFixedBitset([1024, 2048], 128);

        Assert.Equal(128, bytes.Length);
        Assert.All(bytes, b => Assert.Equal(0, b));
    }

    [Fact]
    public void PackTwoBitValues_PacksValuesLittleEndian()
    {
        var bytes = GreenProtocolBytes.PackTwoBitValues([1, 2, 3, 0, 1], GreenProtocolBytes.DanFlagBytes);

        Assert.Equal(GreenProtocolBytes.DanFlagBytes, bytes.Length);
        Assert.Equal(0b0011_1001, bytes[0]);
        Assert.Equal(0b0000_0001, bytes[1]);
    }

    [Fact]
    public void SetTwoBitValue_UpdatesOnlyRequestedIndex()
    {
        var bytes = new byte[GreenProtocolBytes.DanFlagBytes];

        GreenProtocolBytes.SetTwoBitValue(bytes, 0, 1);
        GreenProtocolBytes.SetTwoBitValue(bytes, 1, 2);

        Assert.Equal(0b0000_1001, bytes[0]);
    }

    [Fact]
    public void BuildGreenCrownValue_PacksFiveCourseStates()
    {
        var value = GreenProtocolBytes.BuildGreenCrownValue(
            GreenCrownState.Clear,
            GreenCrownState.FullCombo,
            GreenCrownState.Dondaful,
            GreenCrownState.None,
            GreenCrownState.Clear);

        Assert.Equal((ushort)0b01_00_11_10_01, value);
    }

    [Fact]
    public void PackGreenCrowns_Creates1280ByteBody()
    {
        var values = new ushort[1024];
        values[0] = GreenProtocolBytes.BuildGreenCrownValue(
            GreenCrownState.Clear,
            GreenCrownState.None,
            GreenCrownState.None,
            GreenCrownState.None,
            GreenCrownState.None);
        values[1023] = 0x03ff;

        var packed = GreenProtocolBytes.PackGreenCrowns(values);

        Assert.Equal(GreenProtocolBytes.CrownInflatedBytes, packed.Length);
        Assert.Equal(0b0000_0001, packed[0]);
        Assert.NotEqual(0, packed[^1]);
    }

    [Fact]
    public void CompressZlib_RoundTripsCrownBody()
    {
        var body = new byte[GreenProtocolBytes.CrownInflatedBytes];
        body[0] = 0x39;
        body[^1] = 0x7f;

        var compressed = GreenProtocolBytes.CompressZlib(body);
        using var input = new MemoryStream(compressed);
        using var zlib = new ZLibStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        zlib.CopyTo(output);

        Assert.Equal(body, output.ToArray());
    }
}
