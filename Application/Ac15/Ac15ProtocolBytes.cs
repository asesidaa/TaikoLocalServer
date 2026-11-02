namespace TaikoLocalServer.Application.Ac15;

public static class Ac15ProtocolBytes
{
    public static byte[] CreateFixedBitset(IEnumerable<uint> enabledIds, int byteCount)
        => BitsetCodec.Encode(enabledIds, byteCount);

    public static byte[] FixedOrZero(byte[]? source, int byteCount)
        => BitsetCodec.Normalize(source, byteCount);

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

    public static byte[] SetBits(byte[]? source, IEnumerable<uint> ids, int byteCount)
    {
        var result = FixedOrZero(source, byteCount);
        var maxBits = byteCount * 8;
        foreach (var id in ids)
        {
            if (id >= maxBits)
            {
                continue;
            }

            result[id >> 3] |= (byte)(1 << ((int)id & 7));
        }

        return result;
    }

    public static byte[] ClearBits(byte[]? source, IEnumerable<uint> ids, int byteCount)
    {
        var result = FixedOrZero(source, byteCount);
        var maxBits = byteCount * 8;
        foreach (var id in ids)
        {
            if (id >= maxBits)
            {
                continue;
            }

            result[id >> 3] &= (byte)~(1 << ((int)id & 7));
        }

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

    public static ushort BuildCrownValue(
        Ac15CrownState easy,
        Ac15CrownState normal,
        Ac15CrownState hard,
        Ac15CrownState oni,
        Ac15CrownState uraOni)
    {
        return (ushort)(
            (((ushort)easy & 3) << 0) |
            (((ushort)normal & 3) << 2) |
            (((ushort)hard & 3) << 4) |
            (((ushort)oni & 3) << 6) |
            (((ushort)uraOni & 3) << 8));
    }

    public static byte[] PackTenBitValues(IReadOnlyList<ushort> values, int byteCount, int maxValues)
    {
        var result = new byte[byteCount];

        for (var valueIndex = 0; valueIndex < Math.Min(maxValues, values.Count); valueIndex++)
        {
            var value = values[valueIndex] & 0x03ff;
            var bitOffset = valueIndex * 10;

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
