using TaikoLocalServer.Application.Ac15;

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
    public const int BattleInfoFlagBytes = 16;
    public const int BattleStageFlagBytes = 8;
    public const int BattleSpecialFlagBytes = 16;
    public const int BattleNpcCostumeFlagBytes = 4;
    public const uint BattleDefaultStageId = 0;
    public const uint BattleDefaultSpecialId = 1;
    public const uint BattleNpcSpecialRowGateId = 120;

    // The battle-intro storyboard selector (EBOOT sub_EEF48) does an unguarded flat_map::at(0) on
    // the battle-token map; the response must always carry a token_id 0 row or the game crashes when
    // entering battle. See .tools/blue/battleuserdata-response-xrefs.md.
    public const uint BattleIntroTokenId = 0;
    public const int CrownInflatedBytes = 1280;

    public static byte[] CreateFixedBitset(IEnumerable<uint> enabledIds, int byteCount)
    {
        return Ac15ProtocolBytes.CreateFixedBitset(enabledIds, byteCount);
    }

    public static byte[] CreateBattleSpecialBitset(IEnumerable<uint> enabledIds)
    {
        return CreateFixedBitset(
            enabledIds
                .Append(BattleDefaultSpecialId)
                .Append(BattleNpcSpecialRowGateId),
            BattleSpecialFlagBytes);
    }

    public static byte[] FixedOrZero(byte[]? source, int byteCount)
    {
        return Ac15ProtocolBytes.FixedOrZero(source, byteCount);
    }

    public static byte[] OrBitsets(byte[] left, byte[] right, int byteCount)
    {
        return Ac15ProtocolBytes.OrBitsets(left, right, byteCount);
    }

    public static ushort BuildBlueCrownValue(
        Ac15CrownState easy,
        Ac15CrownState normal,
        Ac15CrownState hard,
        Ac15CrownState oni,
        Ac15CrownState uraOni)
    {
        return Ac15ProtocolBytes.BuildCrownValue(easy, normal, hard, oni, uraOni);
    }

    public static byte[] PackBlueCrowns(IReadOnlyList<ushort> songValues)
    {
        return Ac15ProtocolBytes.PackTenBitValues(songValues, CrownInflatedBytes, 1024);
    }
}
