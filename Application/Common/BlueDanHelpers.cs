using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Application.Common;

public static class BlueDanHelpers
{
    public const uint MinNormalDanId = 1;
    public const uint MaxNormalDanId = 25;
    public const uint MinExtraDanId = 101;
    public const uint MaxKnownExtraDanId = 128;

    public static bool IsNormalDanId(uint danId)
        => danId is >= MinNormalDanId and <= MaxNormalDanId;

    public static bool IsExtraDanId(uint danId)
        => danId is >= MinExtraDanId and <= MaxKnownExtraDanId;

    public static bool IsKnownBlueDanId(uint danId)
        => IsNormalDanId(danId) || IsExtraDanId(danId);

    public static bool IsClear(BlueDanClearGrade grade)
        => grade is BlueDanClearGrade.NormalClear or BlueDanClearGrade.GoldClear;

    public static BlueDanClearGrade ClampGrade(uint value)
        => value >= (uint)BlueDanClearGrade.GoldClear
            ? BlueDanClearGrade.GoldClear
            : (BlueDanClearGrade)value;

    public static uint GetPackedIndex(uint danId)
    {
        if (IsNormalDanId(danId))
        {
            return danId - MinNormalDanId;
        }

        if (IsExtraDanId(danId))
        {
            return danId - MinExtraDanId;
        }

        throw new ArgumentOutOfRangeException(nameof(danId), danId, "Blue Dan id must be normal 1..25 or extra 101..128.");
    }

    public static byte[] SetPackedGrade(byte[] source, uint packedIndex, BlueDanClearGrade grade)
    {
        var result = source.ToArray();
        var value = ToPackedValue(ClampGrade((uint)grade));
        var bitOffset = (int)packedIndex * 2;

        for (var bit = 0; bit < 2; bit++)
        {
            var absoluteBit = bitOffset + bit;
            var byteIndex = absoluteBit >> 3;
            if ((uint)byteIndex >= (uint)result.Length)
            {
                return result;
            }

            var mask = (byte)(1 << (absoluteBit & 7));
            if ((value & (1u << bit)) != 0)
            {
                result[byteIndex] |= mask;
            }
            else
            {
                result[byteIndex] &= (byte)~mask;
            }
        }

        return result;
    }

    public static BlueDanClearGrade GetPackedGrade(byte[] source, uint packedIndex)
    {
        uint value = 0;
        var bitOffset = (int)packedIndex * 2;
        for (var bit = 0; bit < 2; bit++)
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

        return FromPackedValue(value);
    }

    public static uint GetGotDanMax(IReadOnlyDictionary<uint, BlueDanClearGrade> normalGrades)
    {
        uint max = 0;
        foreach (var row in normalGrades)
        {
            if (IsNormalDanId(row.Key) && IsClear(row.Value) && row.Key > max)
            {
                max = row.Key;
            }
        }

        return max;
    }

    public static uint GetNextUnclearedNormalDan(IReadOnlyDictionary<uint, BlueDanClearGrade> normalGrades)
    {
        for (uint dan = MinNormalDanId; dan <= MaxNormalDanId; dan++)
        {
            if (!normalGrades.TryGetValue(dan, out var grade) || !IsClear(grade))
            {
                return dan;
            }
        }

        return MaxNormalDanId;
    }

    public static uint GetDisplayDanAfterNormalClear(uint clearedDanId)
    {
        if (!IsNormalDanId(clearedDanId))
        {
            throw new ArgumentOutOfRangeException(nameof(clearedDanId), clearedDanId, "Blue display Dan advancement only accepts normal Dan ids.");
        }

        return Math.Min(clearedDanId + 1, MaxNormalDanId);
    }

    public static uint NormalizeDisplayDan(uint savedDisplayDan, IReadOnlyDictionary<uint, BlueDanClearGrade> normalGrades)
    {
        if (!IsNormalDanId(savedDisplayDan)
            || (normalGrades.TryGetValue(savedDisplayDan, out var grade) && IsClear(grade)))
        {
            return GetNextUnclearedNormalDan(normalGrades);
        }

        return savedDisplayDan;
    }

    private static uint ToPackedValue(BlueDanClearGrade grade)
        => grade switch
        {
            BlueDanClearGrade.NotClear => 0,
            BlueDanClearGrade.NormalClear => 2,
            BlueDanClearGrade.GoldClear => 3,
            _ => 0
        };

    private static BlueDanClearGrade FromPackedValue(uint value)
        => value switch
        {
            0 => BlueDanClearGrade.NotClear,
            1 or 2 => BlueDanClearGrade.NormalClear,
            _ => BlueDanClearGrade.GoldClear
        };
}
