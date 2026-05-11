using TaikoLocalServer.Application.Common;
using Xunit;

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
}
