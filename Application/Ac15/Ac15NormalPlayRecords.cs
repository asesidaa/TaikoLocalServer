using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15SaveSnapshot(uint Baid);

public sealed record Ac15PlayRow(
    uint Baid,
    uint SongId,
    Difficulty Difficulty,
    CrownType Crown,
    uint Score,
    uint ScoreRate,
    uint GoodCount,
    uint OkCount,
    uint MissCount,
    uint ComboCount,
    uint HitCount,
    uint PoundCount,
    uint StarLevel,
    uint SupportLevel,
    byte[] OptionFlg,
    byte[] ToneFlg,
    uint PlayMode,
    uint StageMode,
    bool IsShin,
    uint MusicCategory,
    uint SelectedFolderId,
    bool IsFavorite,
    bool IsRecent,
    bool IsPapamama,
    bool IsPushed,
    uint SoulGauge,
    uint PlayDan,
    uint WaiwaiResult,
    uint WaiwaiGauge,
    Ac15GreenGhostStageData? GhostStageData,
    DateTime PlayTime);

public sealed record Ac15BestUpdatePolicy(bool AllowScoreUpdate, bool AllowCrownUpdate);
