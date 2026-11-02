using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15SongHashCodecTests
{
    [Fact]
    public void EncodeTable_UsesBigEndianSongIndexes()
    {
        var table = Ac15SongHashCodec.BuildTable([236u, 5u]);

        var bytes = Ac15SongHashCodec.EncodeTable(table);

        Assert.Equal([0x00, 0xec, 0x00, 0x05], bytes);
    }

    [Fact]
    public void CompactBitset_ReindexesSongNumberBitsByHashTableOrdinal()
    {
        var inflated = Ac15ProtocolBytes.CreateFixedBitset([236u, 5u], byteCount: 128);
        var table = Ac15SongHashCodec.BuildTable([236u, 234u, 128u, 199u, 5u]);

        var compact = Ac15SongHashCodec.CompactBitset(inflated, table);

        Assert.Equal([0b0001_0001], compact);
    }

    [Fact]
    public void CompactTenBitValues_ReindexesPackedCrownValuesByHashTableOrdinal()
    {
        var values = new ushort[1024];
        values[236] = 0x02ab;
        values[5] = 0x0155;
        var inflated = Ac15ProtocolBytes.PackTenBitValues(values, byteCount: 1280, maxValues: 1024);
        var table = Ac15SongHashCodec.BuildTable([236u, 5u]);

        var compact = Ac15SongHashCodec.CompactTenBitValues(inflated, table);

        Assert.Equal(3, compact.Length);
        Assert.Equal((ushort)0x02ab, ReadTenBitValue(compact, 0));
        Assert.Equal((ushort)0x0155, ReadTenBitValue(compact, 1));
    }

    [Fact]
    public void CompactEightBitValues_ReindexesByteValuesByHashTableOrdinal()
    {
        var inflated = new byte[1024];
        inflated[236] = 0xce;
        inflated[5] = 0xc0;
        var table = Ac15SongHashCodec.BuildTable([236u, 5u]);

        var compact = Ac15SongHashCodec.CompactEightBitValues(inflated, table);

        Assert.Equal([0xce, 0xc0], compact);
    }

    private static ushort ReadTenBitValue(byte[] packed, int index)
    {
        ushort value = 0;
        var bitOffset = index * 10;
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
