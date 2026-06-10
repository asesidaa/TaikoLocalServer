using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Application.Ac15;

[Mapper]
public static partial class Ac15DaniMapper
{
    public static Ac15DaniScore ToAc15DaniScore(DanScoreDatumBlue row)
        => new(
            row.Baid,
            row.DanId,
            row.IsExtra,
            row.MedleyUniqueId,
            row.ArrivalSongCount,
            row.SoulGaugeTotal,
            row.ComboCountTotal,
            row.ClearGrade,
            row.DanStageScoreData
                .OrderBy(stage => stage.StageIndex)
                .Select(ToAc15DaniStageScore)
                .ToArray());

    public static Ac15DaniScore ToAc15DaniScore(DanScoreDatumGreen row)
        => new(
            row.Baid,
            row.DanId,
            row.IsExtra,
            row.MedleyUniqueId,
            row.ArrivalSongCount,
            row.SoulGaugeTotal,
            row.ComboCountTotal,
            row.ClearGrade,
            row.DanStageScoreData
                .OrderBy(stage => stage.StageIndex)
                .Select(ToAc15DaniStageScore)
                .ToArray());

    public static Ac15DaniScore ToAc15DaniScore(DanScoreDatumYellow row)
        => new(
            row.Baid,
            row.DanId,
            row.IsExtra,
            row.MedleyUniqueId,
            row.ArrivalSongCount,
            row.SoulGaugeTotal,
            row.ComboCountTotal,
            row.ClearGrade,
            row.DanStageScoreData
                .OrderBy(stage => stage.StageIndex)
                .Select(ToAc15DaniStageScore)
                .ToArray());

    public static partial Ac15DaniStageScore ToAc15DaniStageScore(DanStageScoreDatumBlue stage);

    public static partial Ac15DaniStageScore ToAc15DaniStageScore(DanStageScoreDatumGreen stage);

    public static partial Ac15DaniStageScore ToAc15DaniStageScore(DanStageScoreDatumYellow stage);

    public static partial Ac15DaniScoreSummary ToAc15DaniScoreSummary(DanScoreDatumBlue row);

    public static partial Ac15DaniScoreSummary ToAc15DaniScoreSummary(DanScoreDatumGreen row);

    public static partial Ac15DaniScoreSummary ToAc15DaniScoreSummary(DanScoreDatumYellow row);

    public static partial DanScoreDatumBlue ToBlueDanScoreDatum(Ac15DaniScore score);

    public static partial DanScoreDatumGreen ToGreenDanScoreDatum(Ac15DaniScore score);

    public static partial DanScoreDatumYellow ToYellowDanScoreDatum(Ac15DaniScore score);

    public static partial void ApplyToBlueDanScoreDatum(Ac15DaniScore score, [MappingTarget] DanScoreDatumBlue row);

    public static partial void ApplyToGreenDanScoreDatum(Ac15DaniScore score, [MappingTarget] DanScoreDatumGreen row);

    public static partial void ApplyToYellowDanScoreDatum(Ac15DaniScore score, [MappingTarget] DanScoreDatumYellow row);

    public static DanStageScoreDatumBlue ToBlueDanStageScoreDatum(Ac15DaniStageScore stage, Ac15DaniScore score)
    {
        var row = ToBlueDanStageScoreDatum(stage);
        row.Baid = score.Baid;
        row.DanId = score.DanId;
        row.IsExtra = score.IsExtra;
        return row;
    }

    public static DanStageScoreDatumGreen ToGreenDanStageScoreDatum(Ac15DaniStageScore stage, Ac15DaniScore score)
    {
        var row = ToGreenDanStageScoreDatum(stage);
        row.Baid = score.Baid;
        row.DanId = score.DanId;
        row.IsExtra = score.IsExtra;
        return row;
    }

    public static DanStageScoreDatumYellow ToYellowDanStageScoreDatum(Ac15DaniStageScore stage, Ac15DaniScore score)
    {
        var row = ToYellowDanStageScoreDatum(stage);
        row.Baid = score.Baid;
        row.DanId = score.DanId;
        row.IsExtra = score.IsExtra;
        return row;
    }

    public static partial void ApplyToBlueDanStageScoreDatum(
        Ac15DaniStageScore stage,
        [MappingTarget] DanStageScoreDatumBlue row);

    public static partial void ApplyToGreenDanStageScoreDatum(
        Ac15DaniStageScore stage,
        [MappingTarget] DanStageScoreDatumGreen row);

    public static partial void ApplyToYellowDanStageScoreDatum(
        Ac15DaniStageScore stage,
        [MappingTarget] DanStageScoreDatumYellow row);

    private static partial DanStageScoreDatumBlue ToBlueDanStageScoreDatum(Ac15DaniStageScore stage);

    private static partial DanStageScoreDatumGreen ToGreenDanStageScoreDatum(Ac15DaniStageScore stage);

    private static partial DanStageScoreDatumYellow ToYellowDanStageScoreDatum(Ac15DaniStageScore stage);
}
