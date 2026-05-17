namespace TaikoLocalServer.Tests.Green;

public sealed class BitsetCodecTests
{
    [Fact]
    public void Decode_ReturnsSetIdsInAscendingOrder()
    {
        var bytes = new byte[] { 0b1000_0001, 0b0000_0010 };

        var ids = BitsetCodec.Decode(bytes, byteCount: 2);

        Assert.Equal(new List<uint> { 0, 7, 9 }, ids);
    }

    [Fact]
    public void Encode_IgnoresIdsBeyondCapacity()
    {
        var bytes = BitsetCodec.Encode([0, 7, 8, 1024], byteCount: 2);

        Assert.Equal(new byte[] { 0b1000_0001, 0b0000_0001 }, bytes);
    }

    [Fact]
    public void RoundTrip_HandlesGreenBoundaryIds()
    {
        var titleLast = (uint)(GreenProtocolBytes.TitleFlagBytes * 8 - 1);
        var ids = new uint[] { 0, 1, titleLast };

        var bytes = BitsetCodec.Encode(ids, GreenProtocolBytes.TitleFlagBytes);
        var decoded = BitsetCodec.Decode(bytes, GreenProtocolBytes.TitleFlagBytes);

        Assert.Equal(ids, decoded);
    }

    [Fact]
    public void Normalize_ReturnsFixedWidthCopy()
    {
        var normalized = BitsetCodec.Normalize([1, 2, 3, 4], byteCount: 2);

        Assert.Equal(new byte[] { 1, 2 }, normalized);
    }
}
