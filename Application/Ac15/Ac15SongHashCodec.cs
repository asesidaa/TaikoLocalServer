namespace TaikoLocalServer.Application.Ac15;

public static class Ac15SongHashCodec
{
    public static ushort[] BuildTable(IEnumerable<uint> songNoes)
        => songNoes.Select(ToHashIndex).ToArray();

    public static byte[] EncodeTable(IReadOnlyList<ushort> table)
    {
        var result = new byte[checked(table.Count * 2)];
        for (var i = 0; i < table.Count; i++)
        {
            var value = table[i];
            result[i * 2] = (byte)(value >> 8);
            result[i * 2 + 1] = (byte)value;
        }

        return result;
    }

    public static byte[] CompactBitset(byte[] inflated, IReadOnlyList<ushort> table)
        => CompactValues(inflated, table, bitsPerValue: 1);

    public static byte[] CompactTenBitValues(byte[] inflated, IReadOnlyList<ushort> table)
        => CompactValues(inflated, table, bitsPerValue: 10);

    private static byte[] CompactValues(byte[] inflated, IReadOnlyList<ushort> table, int bitsPerValue)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(bitsPerValue, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitsPerValue, 24);

        if (table.Count == 0)
        {
            return [];
        }

        var bitCount = checked(table.Count * bitsPerValue);
        var result = new byte[(bitCount + 7) / 8];
        for (var ordinal = 0; ordinal < table.Count; ordinal++)
        {
            var sourceBitOffset = checked(table[ordinal] * bitsPerValue);
            var value = ReadBits(inflated, sourceBitOffset, bitsPerValue);
            WriteBits(result, ordinal * bitsPerValue, bitsPerValue, value);
        }

        return result;
    }

    private static ushort ToHashIndex(uint songNo)
    {
        if (songNo > ushort.MaxValue)
        {
            throw new InvalidDataException($"Song number {songNo} cannot fit in a 16-bit KIMIDORI song hash entry.");
        }

        return (ushort)songNo;
    }

    private static uint ReadBits(byte[] source, int bitOffset, int bitsPerValue)
    {
        uint value = 0;
        for (var bit = 0; bit < bitsPerValue; bit++)
        {
            var absoluteBit = bitOffset + bit;
            var byteIndex = absoluteBit >> 3;
            if ((uint)byteIndex >= (uint)source.Length)
            {
                break;
            }

            if ((source[byteIndex] & (1 << (absoluteBit & 7))) != 0)
            {
                value |= 1u << bit;
            }
        }

        return value;
    }

    private static void WriteBits(byte[] destination, int bitOffset, int bitsPerValue, uint value)
    {
        for (var bit = 0; bit < bitsPerValue; bit++)
        {
            if ((value & (1u << bit)) == 0)
            {
                continue;
            }

            var absoluteBit = bitOffset + bit;
            destination[absoluteBit >> 3] |= (byte)(1 << (absoluteBit & 7));
        }
    }
}
