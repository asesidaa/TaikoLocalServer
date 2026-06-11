namespace TaikoLocalServer.Application.Ac15;

public readonly record struct Ac15DaniScoreKey(
    uint Baid,
    uint DanId,
    bool IsExtra);

public sealed record Ac15DaniScore(
    uint Baid,
    uint DanId,
    bool IsExtra,
    uint MedleyUniqueId,
    uint ArrivalSongCount,
    uint SoulGaugeTotal,
    uint ComboCountTotal,
    Ac15DanClearGrade ClearGrade,
    IReadOnlyList<Ac15DaniStageScore> Stages);

public sealed record Ac15DaniScoreSummary(
    uint DanId,
    bool IsExtra,
    Ac15DanClearGrade ClearGrade);

public sealed record Ac15DaniChallenge(
    uint DanId,
    uint MedleyUniqueId);

public sealed record Ac15DaniStageScore(
    uint StageIndex,
    uint SongNumber,
    uint PlayScore,
    uint GoodCount,
    uint OkCount,
    uint BadCount,
    uint DrumrollCount,
    uint TotalHitCount,
    uint ComboCount,
    uint HighScore);

public sealed record Ac15DaniTables<TScore, TStage>(
    DbSet<TScore> Scores,
    IQueryable<TScore> ScoresWithStages,
    Func<TScore, ICollection<TStage>> GetStages,
    Func<TScore, Ac15DaniScore> ToScore,
    Func<TScore, Ac15DaniScoreSummary> ToSummary,
    Func<Ac15DaniScore, TScore> CreateScore,
    Action<Ac15DaniScore, TScore> ApplyScore,
    Func<Ac15DaniStageScore, Ac15DaniScore, TStage> CreateStage,
    Action<Ac15DaniStageScore, TStage> ApplyStage)
    where TScore : class, IAc15DanScoreDatum
    where TStage : class, IAc15DanStageScoreDatum;

public sealed record Ac15DaniSaveState(
    uint Baid,
    uint DisplayDan,
    bool IsAutoCostumeOn,
    uint DanCostumeId);

public sealed record Ac15DaniSaveUpdate(
    byte[] GotDanFlg,
    byte[] GotDanExtraFlg,
    uint GotDanMax,
    uint DisplayDan,
    bool ApplyDanCostume,
    uint DanCostumeId);
