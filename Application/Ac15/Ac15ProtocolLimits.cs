namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15ProtocolLimits(
    int SongFlagBytes,
    int ToneFlagBytes,
    int TitleFlagBytes,
    int CostumeFlagBytes,
    int DanFlagBytes,
    int DanExtraFlagBytes,
    int ContentInfoBytes,
    int CrownPackedBytes,
    int CrownSongCount,
    int MaxFavoriteSongs,
    int MaxRecentSongs,
    int MaxSongsPerTaikojukuPack,
    int MaxRequestedTaikojukuSlots,
    Difficulty MinCourseLevel,
    Difficulty MaxCourseLevel,
    uint MinNormalDanId,
    uint MaxNormalDanId,
    uint MinExtraDanId,
    uint MaxKnownExtraDanId,
    uint SafeDisplayDanFallback);
