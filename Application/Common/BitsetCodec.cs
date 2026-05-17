namespace TaikoLocalServer.Application.Common;

public static class BitsetCodec
{
    public static List<uint> Decode(byte[]? source, int byteCount)
    {
        var bytes = Normalize(source, byteCount);
        var result = new List<uint>();

        for (var byteIndex = 0; byteIndex < bytes.Length; byteIndex++)
        {
            var value = bytes[byteIndex];
            if (value == 0)
            {
                continue;
            }

            for (var bit = 0; bit < 8; bit++)
            {
                if ((value & (1 << bit)) != 0)
                {
                    result.Add((uint)(byteIndex * 8 + bit));
                }
            }
        }

        return result;
    }

    public static byte[] Encode(IEnumerable<uint> ids, int byteCount)
    {
        var result = new byte[byteCount];
        var maxBits = byteCount * 8;

        foreach (var id in ids.Distinct())
        {
            if (id >= maxBits)
            {
                continue;
            }

            result[id >> 3] |= (byte)(1 << ((int)id & 7));
        }

        return result;
    }

    public static byte[] Normalize(byte[]? source, int byteCount)
    {
        var result = new byte[byteCount];
        if (source is null || source.Length == 0)
        {
            return result;
        }

        Array.Copy(source, result, Math.Min(source.Length, result.Length));
        return result;
    }
}
