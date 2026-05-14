namespace TaikoLocalServer.Application.Common;

public static class GreenProtocolBytes
{
    public const int SongFlagBytes = 128;
    public const int ToneFlagBytes = 16;
    public const int TitleFlagBytes = 128;
    public const int CostumeFlagBytes = 32;
    public const int DanFlagBytes = 18;
    public const int DanExtraFlagBytes = 36;
    public const int ContentInfoBytes = 32;
    public const int GhostReleaseInfoBytes = 16;
    public const int GhostPlayedSongBytes = 128;
    public const int CrownInflatedBytes = 1280;

    public static byte[] CreateFixedBitset(IEnumerable<uint> enabledIds, int byteCount)
    {
        var result = new byte[byteCount];
        var maxBits = byteCount * 8;

        foreach (var id in enabledIds)
        {
            if (id >= maxBits)
            {
                continue;
            }

            result[id >> 3] |= (byte)(1 << ((int)id & 7));
        }

        return result;
    }

    public static byte[] FixedOrZero(byte[]? source, int byteCount)
    {
        var result = new byte[byteCount];
        if (source is null || source.Length == 0)
        {
            return result;
        }

        Array.Copy(source, result, Math.Min(source.Length, result.Length));
        return result;
    }

    public static byte[] PackTwoBitValues(IEnumerable<uint> values, int byteCount)
    {
        var result = new byte[byteCount];
        var index = 0;

        foreach (var value in values)
        {
            if (((index * 2) >> 3) >= byteCount)
            {
                break;
            }

            SetTwoBitValue(result, index, value);
            index++;
        }

        return result;
    }

    public static void SetTwoBitValue(byte[] buffer, int index, uint value)
    {
        var masked = value & 0b11;
        var bitOffset = index * 2;

        for (var bit = 0; bit < 2; bit++)
        {
            var absoluteBit = bitOffset + bit;
            var byteIndex = absoluteBit >> 3;
            if ((uint)byteIndex >= (uint)buffer.Length)
            {
                return;
            }

            var mask = (byte)(1 << (absoluteBit & 7));
            if ((masked & (1u << bit)) != 0)
            {
                buffer[byteIndex] |= mask;
            }
            else
            {
                buffer[byteIndex] &= (byte)~mask;
            }
        }
    }

    public static ushort BuildGreenCrownValue(
        GreenCrownState easy,
        GreenCrownState normal,
        GreenCrownState hard,
        GreenCrownState oni,
        GreenCrownState uraOni)
    {
        return (ushort)(
            (((ushort)easy & 3) << 0) |
            (((ushort)normal & 3) << 2) |
            (((ushort)hard & 3) << 4) |
            (((ushort)oni & 3) << 6) |
            (((ushort)uraOni & 3) << 8));
    }

    public static byte[] PackGreenCrowns(IReadOnlyList<ushort> songValues)
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
