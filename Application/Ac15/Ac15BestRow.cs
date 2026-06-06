namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15BestRow(
    uint SongId,
    Difficulty Difficulty,
    bool IsShin,
    uint BestScore,
    uint BestRate,
    CrownType BestCrown);
