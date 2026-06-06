using TaikoLocalServer.Application.Ac15;

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
        return Ac15ProtocolBytes.CreateFixedBitset(enabledIds, byteCount);
    }

    public static byte[] FixedOrZero(byte[]? source, int byteCount)
    {
        return Ac15ProtocolBytes.FixedOrZero(source, byteCount);
    }

    public static byte[] PackTwoBitValues(IEnumerable<uint> values, int byteCount)
    {
        return Ac15ProtocolBytes.PackTwoBitValues(values, byteCount);
    }

    public static void SetTwoBitValue(byte[] buffer, int index, uint value)
    {
        Ac15ProtocolBytes.SetTwoBitValue(buffer, index, value);
    }

    public static ushort BuildGreenCrownValue(
        GreenCrownState easy,
        GreenCrownState normal,
        GreenCrownState hard,
        GreenCrownState oni,
        GreenCrownState uraOni)
    {
        return Ac15ProtocolBytes.BuildCrownValue(
            (Ac15CrownState)easy,
            (Ac15CrownState)normal,
            (Ac15CrownState)hard,
            (Ac15CrownState)oni,
            (Ac15CrownState)uraOni);
    }

    public static byte[] PackGreenCrowns(IReadOnlyList<ushort> songValues)
    {
        return Ac15ProtocolBytes.PackTenBitValues(songValues, CrownInflatedBytes, 1024);
    }
}
