namespace TaikoLocalServer.Application.Common;

public static class BlueProtocolBytes
{
    public const int SongFlagBytes = 128;
    public const int ToneFlagBytes = 16;
    public const int TitleFlagBytes = 128;
    public const int CostumeFlagBytes = 32;
    public const int DanFlagBytes = 18;
    public const int DanExtraFlagBytes = 36;
    public const int ContentInfoBytes = 32;
    public const int BattleStageFlagBytes = 8;
    public const int BattleSpecialFlagBytes = 16;
    public const int CrownInflatedBytes = 1280;

    public static byte[] CreateFixedBitset(IEnumerable<uint> enabledIds, int byteCount)
    {
        return BitsetCodec.Encode(enabledIds, byteCount);
    }

    public static byte[] FixedOrZero(byte[]? source, int byteCount)
    {
        return BitsetCodec.Normalize(source, byteCount);
    }

    public static byte[] OrBitsets(byte[] left, byte[] right, int byteCount)
    {
        var result = FixedOrZero(left, byteCount);
        var normalizedRight = FixedOrZero(right, byteCount);
        for (var i = 0; i < result.Length; i++)
        {
            result[i] |= normalizedRight[i];
        }

        return result;
    }

    public static ushort BuildBlueCrownValue(
        BlueCrownState easy,
        BlueCrownState normal,
        BlueCrownState hard,
        BlueCrownState oni,
        BlueCrownState uraOni)
    {
        return (ushort)(
            (((ushort)easy & 3) << 0) |
            (((ushort)normal & 3) << 2) |
            (((ushort)hard & 3) << 4) |
            (((ushort)oni & 3) << 6) |
            (((ushort)uraOni & 3) << 8));
    }

    public static byte[] PackBlueCrowns(IReadOnlyList<ushort> songValues)
    {
        var result = new byte[CrownInflatedBytes];

        for (var songIndex = 0; songIndex < Math.Min(1024, songValues.Count); songIndex++)
        {
            var value = songValues[songIndex] & 0x03ff;
            var bitOffset = songIndex * 10;

            for (var bit = 0; bit < 10; bit++)
            {
                if ((value & (1 << bit)) == 0)
                {
                    continue;
                }

                var absoluteBit = bitOffset + bit;
                result[absoluteBit >> 3] |= (byte)(1 << (absoluteBit & 7));
            }
        }

        return result;
    }
}
