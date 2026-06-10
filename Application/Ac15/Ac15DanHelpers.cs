namespace TaikoLocalServer.Application.Ac15;

public static class Ac15DanHelpers
{
    public static bool IsNormalDanId(uint danId, Ac15ProtocolLimits limits)
        => danId >= limits.MinNormalDanId && danId <= limits.MaxNormalDanId;

    public static bool IsExtraDanId(uint danId, Ac15ProtocolLimits limits)
        => danId >= limits.MinExtraDanId && danId <= limits.MaxKnownExtraDanId;

    public static bool IsKnownDanId(uint danId, Ac15ProtocolLimits limits)
        => IsNormalDanId(danId, limits) || IsExtraDanId(danId, limits);

    public static bool IsClear(Ac15DanClearGrade grade)
        => grade is Ac15DanClearGrade.NormalClear or Ac15DanClearGrade.GoldClear;

    public static Ac15DanClearGrade ClampGrade(uint value)
        => value >= (uint)Ac15DanClearGrade.GoldClear
            ? Ac15DanClearGrade.GoldClear
            : (Ac15DanClearGrade)value;

    public static uint GetPackedIndex(uint danId, Ac15ProtocolLimits limits)
    {
        if (IsNormalDanId(danId, limits))
        {
            return danId - limits.MinNormalDanId;
        }

        if (IsExtraDanId(danId, limits))
        {
            return danId - limits.MinExtraDanId;
        }

        throw new ArgumentOutOfRangeException(nameof(danId), danId, "AC15 Dan id must be in the configured normal or extra Dan ranges.");
    }

    public static byte[] SetPackedGrade(byte[]? source, uint packedIndex, Ac15DanClearGrade grade, int byteCount)
    {
        var result = Ac15ProtocolBytes.FixedOrZero(source, byteCount);
        Ac15ProtocolBytes.SetTwoBitValue(result, checked((int)packedIndex), ToPackedValue(ClampGrade((uint)grade)));
        return result;
    }

    public static Ac15DanClearGrade GetPackedGrade(byte[] source, uint packedIndex)
    {
        uint value = 0;
        var bitOffset = checked((int)packedIndex) * 2;
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

    public static uint GetGotDanMax(IReadOnlyDictionary<uint, Ac15DanClearGrade> normalGrades, Ac15ProtocolLimits limits)
    {
        uint max = 0;
        foreach (var row in normalGrades)
        {
            if (IsNormalDanId(row.Key, limits) && IsClear(row.Value) && row.Key > max)
            {
                max = row.Key;
            }
        }

        return max;
    }

    public static uint GetNextUnclearedNormalDan(IReadOnlyDictionary<uint, Ac15DanClearGrade> normalGrades, Ac15ProtocolLimits limits)
    {
        for (var dan = limits.MinNormalDanId; dan <= limits.MaxNormalDanId; dan++)
        {
            if (!normalGrades.TryGetValue(dan, out var grade) || !IsClear(grade))
            {
                return dan;
            }
        }

        return limits.MaxNormalDanId;
    }

    public static uint GetDisplayDanAfterNormalClear(uint clearedDanId, Ac15ProtocolLimits limits)
    {
        if (!IsNormalDanId(clearedDanId, limits))
        {
            throw new ArgumentOutOfRangeException(nameof(clearedDanId), clearedDanId, "AC15 display Dan advancement only accepts configured normal Dan ids.");
        }

        return Math.Min(clearedDanId + 1, limits.MaxNormalDanId);
    }

    public static uint NormalizeDisplayDan(
        uint savedDisplayDan,
        IReadOnlyDictionary<uint, Ac15DanClearGrade> normalGrades,
        Ac15ProtocolLimits limits)
    {
        if (!IsNormalDanId(savedDisplayDan, limits)
            || (normalGrades.TryGetValue(savedDisplayDan, out var grade) && IsClear(grade)))
        {
            return GetNextUnclearedNormalDan(normalGrades, limits);
        }

        return savedDisplayDan;
    }

    private static uint ToPackedValue(Ac15DanClearGrade grade)
        => grade switch
        {
            Ac15DanClearGrade.NotClear => 0,
            Ac15DanClearGrade.NormalClear => 2,
            Ac15DanClearGrade.GoldClear => 3,
            _ => 0
        };

    private static Ac15DanClearGrade FromPackedValue(uint value)
        => value switch
        {
            0 => Ac15DanClearGrade.NotClear,
            1 or 2 => Ac15DanClearGrade.NormalClear,
            _ => Ac15DanClearGrade.GoldClear
        };
}
