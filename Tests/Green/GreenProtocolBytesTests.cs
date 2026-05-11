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
}
