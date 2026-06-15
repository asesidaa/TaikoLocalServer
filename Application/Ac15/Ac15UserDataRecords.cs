namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15UserDataSnapshot(
    uint SongHashVersion,
    IReadOnlyList<uint> CatalogReleaseSongNoes,
    byte[] SaveReleaseSongFlg,
    byte[] ToneFlg,
    byte[] TitleFlg,
    byte[] DefaultOptionSetting,
    byte[] OptionFlg,
    IReadOnlyList<uint> Favorites,
    IReadOnlyList<uint> Recent,
    uint RecommendSong,
    IReadOnlyList<uint> RecommendBestSongs,
    Ac15ProfileCounters Counters,
    uint DisplayDan,
    IReadOnlyList<uint> LockedSongIds,
    IReadOnlyList<uint> LockedToneIds);

public sealed class Ac15ProfileCounters
{
    public uint CategJpopCnt { get; init; }
    public uint CategAnimeCnt { get; init; }
    public uint CategDoyoCnt { get; init; }
    public uint CategVarietyCnt { get; init; }
    public uint CategClassicCnt { get; init; }
    public uint CategGameCnt { get; init; }
    public uint CategNamcoCnt { get; init; }
    public uint CategVocaloidCnt { get; init; }
    public uint SongPushedCnt { get; init; }
    public uint SongFavoriteCnt { get; init; }
    public uint SongRecentCnt { get; init; }
    public uint TotalCreditCnt { get; init; }
    public uint PrevAreaCode { get; init; }
    public uint ConsecAreaCnt { get; init; }
    public bool DefaultShinSetting { get; init; }
    public uint DispLevelTotal { get; init; }
    public uint DispLevelChassis { get; init; }
    public uint DispScoreType { get; init; }
    public uint DispLevelSelf { get; init; }
    public uint DifficultyPlayedCourse { get; init; }
    public uint DifficultyPlayedStar { get; init; }
    public bool IsChallengeCompe { get; init; }
    public bool IsTojiru { get; init; }
}
