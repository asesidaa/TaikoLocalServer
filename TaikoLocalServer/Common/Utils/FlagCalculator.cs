using System.Collections;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using GameDatabase.Entities;

namespace TaikoLocalServer.Common.Utils;

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

    public static byte[] ComputeGotDanFlags(List<DanScoreDatum> danScoreData)
    {
        var gotDanFlagList = new List<int>();
        var gotDanFlag = new BitVector32();
        var section1 = BitVector32.CreateSection(8);
        var section2 = BitVector32.CreateSection(8, section1);
        var section3 = BitVector32.CreateSection(8, section2);
        var section4 = BitVector32.CreateSection(8, section3);
        var section5 = BitVector32.CreateSection(8, section4);
        var section6 = BitVector32.CreateSection(8, section5);
        var section7 = BitVector32.CreateSection(8, section6);
        var section8 = BitVector32.CreateSection(8, section7);

        var sections = new[] { section1, section2, section3, section4, section5, section6, section7, section8 };

        for (var i = Constants.MIN_DAN_ID; i < Constants.MAX_DAN_ID; i++)
        {
            var danId = i;
            var flag = 0;
            if (danScoreData.Any(datum => datum.DanId == danId))
            {
                var danScore = danScoreData.First(datum => datum.DanId == danId);
                flag = (int)danScore.ClearState + 1;
            }

            var section = sections[(danId - 1) % 8];
            gotDanFlag[section] = flag;

            if (!section.Equals(section8)) continue;
            gotDanFlagList.Add(gotDanFlag.Data);
            gotDanFlag = new BitVector32();
        }

        gotDanFlagList.Add(gotDanFlag.Data);
        return MemoryMarshal.AsBytes(new ReadOnlySpan<int>(gotDanFlagList.ToArray())).ToArray();
    }

    public static byte[] GetBitArrayFromIds(IEnumerable<uint> idArray, int bitArraySize, ILogger logger)
    {
        var result = new byte[bitArraySize / 8 + 1];
        var bitSet = new BitArray(bitArraySize + 1);
        foreach (var id in idArray)
        {
            if (id >= bitArraySize)
            {
                logger.LogWarning("Id {Id} out of range!", id);
                continue;
            }

            bitSet.Set((int)id, true);
        }

        bitSet.CopyTo(result, 0);

        return result;
    }
}