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
