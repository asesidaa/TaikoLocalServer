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

    public static Ac15DaniScore ToAc15DaniScore(DanScoreDatumRed row)
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

    public static Ac15DaniScore ToAc15DaniScore(DanScoreDatumWhite row)
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

    public static partial Ac15DaniStageScore ToAc15DaniStageScore(DanStageScoreDatumRed stage);

    public static partial Ac15DaniStageScore ToAc15DaniStageScore(DanStageScoreDatumWhite stage);

    public static partial Ac15DaniScoreSummary ToAc15DaniScoreSummary(DanScoreDatumBlue row);

    public static partial Ac15DaniScoreSummary ToAc15DaniScoreSummary(DanScoreDatumGreen row);

    public static partial Ac15DaniScoreSummary ToAc15DaniScoreSummary(DanScoreDatumYellow row);

    public static partial Ac15DaniScoreSummary ToAc15DaniScoreSummary(DanScoreDatumRed row);

    public static partial Ac15DaniScoreSummary ToAc15DaniScoreSummary(DanScoreDatumWhite row);

    [MapperIgnoreTarget(nameof(DanScoreDatumBlue.DanStageScoreData))]
    [MapperIgnoreTarget(nameof(DanScoreDatumBlue.Ba))]
    public static partial DanScoreDatumBlue ToBlueDanScoreDatum(Ac15DaniScore score);

    [MapperIgnoreTarget(nameof(DanScoreDatumGreen.DanStageScoreData))]
    [MapperIgnoreTarget(nameof(DanScoreDatumGreen.Ba))]
    public static partial DanScoreDatumGreen ToGreenDanScoreDatum(Ac15DaniScore score);

    [MapperIgnoreTarget(nameof(DanScoreDatumYellow.DanStageScoreData))]
    [MapperIgnoreTarget(nameof(DanScoreDatumYellow.Ba))]
    public static partial DanScoreDatumYellow ToYellowDanScoreDatum(Ac15DaniScore score);

    [MapperIgnoreTarget(nameof(DanScoreDatumRed.DanStageScoreData))]
    [MapperIgnoreTarget(nameof(DanScoreDatumRed.Ba))]
    public static partial DanScoreDatumRed ToRedDanScoreDatum(Ac15DaniScore score);

    [MapperIgnoreTarget(nameof(DanScoreDatumWhite.DanStageScoreData))]
    [MapperIgnoreTarget(nameof(DanScoreDatumWhite.Ba))]
    public static partial DanScoreDatumWhite ToWhiteDanScoreDatum(Ac15DaniScore score);

    [MapperIgnoreTarget(nameof(DanScoreDatumBlue.DanStageScoreData))]
    [MapperIgnoreTarget(nameof(DanScoreDatumBlue.Ba))]
    public static partial void ApplyToBlueDanScoreDatum(Ac15DaniScore score, [MappingTarget] DanScoreDatumBlue row);

    [MapperIgnoreTarget(nameof(DanScoreDatumGreen.DanStageScoreData))]
    [MapperIgnoreTarget(nameof(DanScoreDatumGreen.Ba))]
    public static partial void ApplyToGreenDanScoreDatum(Ac15DaniScore score, [MappingTarget] DanScoreDatumGreen row);

    [MapperIgnoreTarget(nameof(DanScoreDatumYellow.DanStageScoreData))]
    [MapperIgnoreTarget(nameof(DanScoreDatumYellow.Ba))]
    public static partial void ApplyToYellowDanScoreDatum(Ac15DaniScore score, [MappingTarget] DanScoreDatumYellow row);

    [MapperIgnoreTarget(nameof(DanScoreDatumRed.DanStageScoreData))]
    [MapperIgnoreTarget(nameof(DanScoreDatumRed.Ba))]
    public static partial void ApplyToRedDanScoreDatum(Ac15DaniScore score, [MappingTarget] DanScoreDatumRed row);

    [MapperIgnoreTarget(nameof(DanScoreDatumWhite.DanStageScoreData))]
    [MapperIgnoreTarget(nameof(DanScoreDatumWhite.Ba))]
    public static partial void ApplyToWhiteDanScoreDatum(Ac15DaniScore score, [MappingTarget] DanScoreDatumWhite row);

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

    public static DanStageScoreDatumRed ToRedDanStageScoreDatum(Ac15DaniStageScore stage, Ac15DaniScore score)
    {
        var row = ToRedDanStageScoreDatum(stage);
        row.Baid = score.Baid;
        row.DanId = score.DanId;
        row.IsExtra = score.IsExtra;
        return row;
    }

    public static DanStageScoreDatumWhite ToWhiteDanStageScoreDatum(Ac15DaniStageScore stage, Ac15DaniScore score)
    {
        var row = ToWhiteDanStageScoreDatum(stage);
        row.Baid = score.Baid;
        row.DanId = score.DanId;
        row.IsExtra = score.IsExtra;
        return row;
    }

    [MapperIgnoreTarget(nameof(DanStageScoreDatumBlue.Baid))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumBlue.DanId))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumBlue.IsExtra))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumBlue.Parent))]
    public static partial void ApplyToBlueDanStageScoreDatum(
        Ac15DaniStageScore stage,
        [MappingTarget] DanStageScoreDatumBlue row);

    [MapperIgnoreTarget(nameof(DanStageScoreDatumGreen.Baid))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumGreen.DanId))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumGreen.IsExtra))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumGreen.Parent))]
    public static partial void ApplyToGreenDanStageScoreDatum(
        Ac15DaniStageScore stage,
        [MappingTarget] DanStageScoreDatumGreen row);

    [MapperIgnoreTarget(nameof(DanStageScoreDatumYellow.Baid))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumYellow.DanId))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumYellow.IsExtra))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumYellow.Parent))]
    public static partial void ApplyToYellowDanStageScoreDatum(
        Ac15DaniStageScore stage,
        [MappingTarget] DanStageScoreDatumYellow row);

    [MapperIgnoreTarget(nameof(DanStageScoreDatumRed.Baid))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumRed.DanId))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumRed.IsExtra))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumRed.Parent))]
    public static partial void ApplyToRedDanStageScoreDatum(
        Ac15DaniStageScore stage,
        [MappingTarget] DanStageScoreDatumRed row);

    [MapperIgnoreTarget(nameof(DanStageScoreDatumWhite.Baid))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumWhite.DanId))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumWhite.IsExtra))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumWhite.Parent))]
    public static partial void ApplyToWhiteDanStageScoreDatum(
        Ac15DaniStageScore stage,
        [MappingTarget] DanStageScoreDatumWhite row);

    [MapperIgnoreTarget(nameof(DanStageScoreDatumBlue.Baid))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumBlue.DanId))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumBlue.IsExtra))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumBlue.Parent))]
    private static partial DanStageScoreDatumBlue ToBlueDanStageScoreDatum(Ac15DaniStageScore stage);

    [MapperIgnoreTarget(nameof(DanStageScoreDatumGreen.Baid))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumGreen.DanId))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumGreen.IsExtra))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumGreen.Parent))]
    private static partial DanStageScoreDatumGreen ToGreenDanStageScoreDatum(Ac15DaniStageScore stage);

    [MapperIgnoreTarget(nameof(DanStageScoreDatumYellow.Baid))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumYellow.DanId))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumYellow.IsExtra))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumYellow.Parent))]
    private static partial DanStageScoreDatumYellow ToYellowDanStageScoreDatum(Ac15DaniStageScore stage);

    [MapperIgnoreTarget(nameof(DanStageScoreDatumRed.Baid))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumRed.DanId))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumRed.IsExtra))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumRed.Parent))]
    private static partial DanStageScoreDatumRed ToRedDanStageScoreDatum(Ac15DaniStageScore stage);

    [MapperIgnoreTarget(nameof(DanStageScoreDatumWhite.Baid))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumWhite.DanId))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumWhite.IsExtra))]
    [MapperIgnoreTarget(nameof(DanStageScoreDatumWhite.Parent))]
    private static partial DanStageScoreDatumWhite ToWhiteDanStageScoreDatum(Ac15DaniStageScore stage);
}
