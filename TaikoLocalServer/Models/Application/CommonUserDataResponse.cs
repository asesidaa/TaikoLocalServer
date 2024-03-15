namespace TaikoLocalServer.Models.Application;

public class CommonUserDataResponse
{
    public uint   Result                  { get; set; }
    public byte[] ToneFlg                 { get; set; } = [];
    public byte[] TitleFlg                { get; set; } = [];
    public byte[] ReleaseSongFlg          { get; set; } = [];
    public byte[] UraReleaseSongFlg       { get; set; } = [];
    public uint[] AryFavoriteSongNoes     { get; set; } = [];
    public uint[] AryRecentSongNoes       { get; set; } = [];
    public uint   DispScoreType           { get; set; }
    public uint   DispLevelChassis        { get; set; }
    public uint   DispLevelSelf           { get; set; }
    public bool   IsDispTojiruOn          { get; set; }
    public byte[] DefaultOptionSetting    { get; set; } = [];
    public int    NotesPosition           { get; set; }
    public bool   IsVoiceOn               { get; set; }
    public bool   IsSkipOn                { get; set; }
    public uint    DifficultySettingCourse { get; set; }
    public uint    DifficultySettingStar   { get; set; }
    public uint    DifficultySettingSort   { get; set; }
    public uint    DifficultyPlayedCourse  { get; set; }
    public uint    DifficultyPlayedStar    { get; set; }
    public uint    DifficultyPlayedSort    { get; set; }
    public uint    TotalCreditCnt          { get; set; }
    public uint    SongRecentCnt           { get; set; }
    public bool   IsChallengecompe        { get; set; }
}