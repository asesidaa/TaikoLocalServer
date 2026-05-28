namespace TaikoLocalServer.Application.Common;

public static class BlueShopUnlocks
{
    public static byte[] ClearBits(byte[]? source, IEnumerable<uint> ids, int byteCount)
    {
        var result = BlueProtocolBytes.FixedOrZero(source, byteCount);
        foreach (var id in ids)
        {
            if (id >= byteCount * 8)
            {
                continue;
            }

            result[id >> 3] &= (byte)~(1 << ((int)id & 7));
        }

        return result;
    }

    public static byte[] SetBits(byte[]? source, IEnumerable<uint> ids, int byteCount)
    {
        var result = BlueProtocolBytes.FixedOrZero(source, byteCount);
        foreach (var id in ids)
        {
            if (id >= byteCount * 8)
            {
                continue;
            }

            result[id >> 3] |= (byte)(1 << ((int)id & 7));
        }

        return result;
    }

    public static bool HasBit(byte[]? source, uint id, int byteCount)
    {
        if (id >= byteCount * 8)
        {
            return false;
        }

        var fixedBytes = BlueProtocolBytes.FixedOrZero(source, byteCount);
        return (fixedBytes[id >> 3] & (1 << ((int)id & 7))) != 0;
    }
}
