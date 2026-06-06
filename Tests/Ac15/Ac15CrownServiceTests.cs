using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15CrownServiceTests
{
    [Fact]
    public void BuildInflatedBody_EmptyRowsProduceFixedZeroBody()
    {
        var packed = Ac15CrownService.BuildInflatedBody(
            [],
            validSongNoes: [101u],
            Ac15EraProfiles.Blue.Limits);

        Assert.Equal(Ac15EraProfiles.Blue.Limits.CrownPackedBytes, packed.Length);
        Assert.All(packed, value => Assert.Equal(0, value));
    }

    [Fact]
    public void BuildInflatedBody_PacksBestCrownPerCourseAndIncludesShin()
    {
        var rows = new[]
        {
            new Ac15BestRow(101, Difficulty.Easy, false, 1000, 80, CrownType.Clear),
            new Ac15BestRow(101, Difficulty.Easy, true, 2000, 90, CrownType.Gold),
            new Ac15BestRow(999, Difficulty.Easy, false, 3000, 91, CrownType.Dondaful)
        };

        var packed = Ac15CrownService.BuildInflatedBody(rows, validSongNoes: [101u], Ac15EraProfiles.Blue.Limits);

        Assert.Equal((ushort)0b0000000011, ReadTenBitValue(packed, 101));
        Assert.Equal(0, ReadTenBitValue(packed, 999));
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
