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
}
