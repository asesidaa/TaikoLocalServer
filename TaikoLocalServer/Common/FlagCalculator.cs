using TaikoLocalServer.Common.Enums;

namespace TaikoLocalServer.Common;

public static class FlagCalculator
{
    public static byte ComputeDondafulCrownFlag(byte previous, Difficulty difficulty)
    {
        // ReSharper disable once ArrangeRedundantParentheses
        var flag = 1 << ((int)difficulty - 1);
        return (byte)(previous | flag);
    }

    public static ushort ComputeCrownFlag(ushort previous, CrownType crownType, Difficulty difficulty)
    {
        int flagBase;
        switch (crownType)
        {
            case CrownType.Clear:
                flagBase = 0b10;
                break;
            case CrownType.Gold:
                flagBase = 0b11;
                break;
            case CrownType.None:
            case CrownType.Dondaful:
            default:
                throw new ArgumentOutOfRangeException(nameof(crownType), crownType, null);
        }
        var offset = ((int)difficulty - 1) * 2;
        var flag = (ushort)(flagBase << offset);
        return (ushort)(previous | flag);
    }

    public static byte ComputeKiwamiScoreRankFlag(byte previous, Difficulty difficulty)
    {
        // ReSharper disable once ArrangeRedundantParentheses
        var result = 1 << ((int)difficulty - 1);
        return (byte)(previous | result);
    }

    public static ushort ComputeMiyabiOrIkiScoreRank(ushort previous, ScoreRank scoreRank, Difficulty difficulty)
    {
        int resultBase;
        switch (scoreRank)
        {
            case ScoreRank.White:
            case ScoreRank.Gold:
                resultBase = 0b01;
                break;
            case ScoreRank.Bronze:
            case ScoreRank.Sakura:
                resultBase = 0b10;
                break;
            case ScoreRank.Silver:
            case ScoreRank.Purple:
                resultBase = 0b11;
                break;
            case ScoreRank.None:
            case ScoreRank.Dondaful:
            default:
                throw new ArgumentOutOfRangeException(nameof(scoreRank), scoreRank, "Score rank out of range");
        }
        var offset = ((int)difficulty - 1) * 2;
        var result = resultBase << offset;

        return (ushort)(previous | result);
    }
}