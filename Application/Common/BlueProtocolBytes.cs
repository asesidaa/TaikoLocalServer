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
}
