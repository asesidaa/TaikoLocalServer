namespace TaikoLocalServer.Application.Dtos.Ac15;

public sealed record Ac15UserDataResponse
{
    public uint Result { get; init; }
    public Ac15UserDataSongFlags SongFlags { get; init; } = new();
    public Ac15UserDataSongLists SongLists { get; init; } = new();
    public Ac15UserDataProfileCounters Counters { get; init; } = new();
    public Ac15UserDataDisplaySettings Display { get; init; } = new();
    public Ac15UserDataRecommendations Recommendations { get; init; } = new();
    public Ac15UserDataTutorial? Tutorial { get; init; }
    public Ac15UserDataModeFlags? ModeFlags { get; init; }
    public Ac15UserDataReward? Reward { get; init; }
}

public sealed record Ac15UserDataSongFlags
{
    public uint SongHashVer { get; init; }
    public byte[] ReleaseSongFlg { get; init; } = [];
    public byte[] ToneFlg { get; init; } = [];
    public byte[] TitleFlg { get; init; } = [];
    public byte[] OptionFlg { get; init; } = [];
}

public sealed record Ac15UserDataSongLists
{
    public uint[] AryFavoriteSongNoes { get; init; } = [];
    public uint[] AryRecentSongNoes { get; init; } = [];
}

public sealed record Ac15UserDataProfileCounters
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
}

public sealed record Ac15UserDataDisplaySettings
{
    public byte[] DefaultOptionSetting { get; init; } = [];
    public bool DefaultShinSetting { get; init; }
    public uint DispLevelTotal { get; init; }
    public uint DispLevelChassis { get; init; }
    public uint DispLevelSelf { get; init; }
    public uint DispTaikojukuDan { get; init; }
    public uint DifficultyPlayedCourse { get; init; }
    public uint DifficultyPlayedStar { get; init; }
    public bool IsChallengeCompe { get; init; }
    public bool IsTojiru { get; init; }
}

public sealed record Ac15UserDataRecommendations
{
    public uint RecommendSong { get; init; }
    public List<uint> RecommendBestSong { get; init; } = [];
}

public sealed record Ac15UserDataTutorial(
    uint? TokkunTutorialFlg,
    uint? DifficultyTutorialFlg);

public sealed record Ac15UserDataModeFlags(
    bool? IsDevil,
    bool? IsExplain);

public sealed record Ac15UserDataReward(
    uint? TotalGetDonpoint,
    uint? TotalUseDonpoint,
    uint? RewardProgress);
