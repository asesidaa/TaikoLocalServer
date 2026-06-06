using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15ProtocolBytesTests
{
    [Fact]
    public void CreateFixedBitset_SetsLittleEndianBitsAndIgnoresOutOfRangeIds()
    {
        var bytes = Ac15ProtocolBytes.CreateFixedBitset([0, 1, 7, 8, 1023, 1024], 128);

        Assert.Equal(128, bytes.Length);
        Assert.Equal(0b1000_0011, bytes[0]);
        Assert.Equal(0b0000_0001, bytes[1]);
        Assert.Equal(0b1000_0000, bytes[127]);
    }

    [Fact]
    public void SetBits_NormalizesExistingBufferAndSkipsOutOfRangeIds()
    {
        var bytes = Ac15ProtocolBytes.SetBits([0b0000_0001], [1, 16], 2);

        Assert.Equal([0b0000_0011, 0b0000_0000], bytes);
    }

    [Fact]
    public void PackTwoBitValues_PacksValuesLittleEndian()
    {
        var bytes = Ac15ProtocolBytes.PackTwoBitValues([1, 2, 3, 0, 1], 18);

        Assert.Equal(18, bytes.Length);
        Assert.Equal(0b0011_1001, bytes[0]);
        Assert.Equal(0b0000_0001, bytes[1]);
    }

    [Fact]
    public void BuildCrownValue_PacksFiveCourseStates()
    {
        var value = Ac15ProtocolBytes.BuildCrownValue(
            Ac15CrownState.Clear,
            Ac15CrownState.FullCombo,
            Ac15CrownState.FullCombo,
            Ac15CrownState.None,
            Ac15CrownState.Clear);

        Assert.Equal((ushort)0b10_00_11_11_10, value);
    }

    [Fact]
    public void PackTenBitValues_CreatesFixedBodyAndMasksValues()
    {
        var values = new ushort[1024];
        values[0] = 0xffff;
        values[1023] = 0x03ff;

        var bytes = Ac15ProtocolBytes.PackTenBitValues(values, byteCount: 1280, maxValues: 1024);

        Assert.Equal(1280, bytes.Length);
        Assert.Equal(0x03ff, ReadTenBitValue(bytes, 0));
        Assert.Equal(0x03ff, ReadTenBitValue(bytes, 1023));
    }

    private static ushort ReadTenBitValue(byte[] packed, int songIndex)
    {
        ushort value = 0;
        var bitOffset = songIndex * 10;
        for (var bit = 0; bit < 10; bit++)
        {
            var absoluteBit = bitOffset + bit;
            if ((packed[absoluteBit >> 3] & (1 << (absoluteBit & 7))) != 0)
            {
                value |= (ushort)(1 << bit);
            }
        }

        return value;
    }
}
