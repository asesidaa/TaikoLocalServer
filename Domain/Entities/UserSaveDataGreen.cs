namespace TaikoLocalServer.Domain.Entities;

public partial class UserSaveDataGreen
{
    public uint Baid { get; set; }
    public string Title { get; set; } = string.Empty;
    public uint TitleplateId { get; set; }
    public uint ColorBody { get; set; }
    public uint ColorFace { get; set; }
    public uint ColorLimb { get; set; }
    public uint Costume1 { get; set; }
    public uint Costume2 { get; set; }
    public uint Costume3 { get; set; }
    public uint Costume4 { get; set; }
    public uint Costume5 { get; set; }
    public byte[] CostumeFlg1 { get; set; } = [];
    public byte[] CostumeFlg2 { get; set; } = [];
    public byte[] CostumeFlg3 { get; set; } = [];
    public byte[] CostumeFlg4 { get; set; } = [];
    public byte[] CostumeFlg5 { get; set; } = [];
    public byte[] ToneFlg { get; set; } = [];
    public byte[] TitleFlg { get; set; } = [];
    public byte[] OptionFlg { get; set; } = [];
    public byte[] DefaultOptionSetting { get; set; } = [];
    public bool DefaultShinSetting { get; set; }
    public uint DefaultToneSetting { get; set; }
    public uint DispDanType { get; set; }
    public uint GotDanMax { get; set; }
    public byte[] GotDanFlg { get; set; } = [];
    public byte[] GotDanExtraFlg { get; set; } = [];
    public uint DispTaikojukuDan { get; set; }
    public uint TotalGetDonmedal { get; set; }
    public uint TotalUseDonmedal { get; set; }
    public uint TotalGetKatsumedal { get; set; }
    public uint TotalUseKatsumedal { get; set; }
    public uint ItemshopTutorialFlg { get; set; }
    public bool IsAutoCostumeOn { get; set; }
    public uint CategJpopCnt { get; set; }
    public uint CategAnimeCnt { get; set; }
    public uint CategDoyoCnt { get; set; }
    public uint CategVarietyCnt { get; set; }
    public uint CategClassicCnt { get; set; }
    public uint CategGameCnt { get; set; }
    public uint CategNamcoCnt { get; set; }
    public uint CategVocaloidCnt { get; set; }
    public uint SongPushedCnt { get; set; }
    public uint SongFavoriteCnt { get; set; }
    public uint SongRecentCnt { get; set; }
    public uint TotalCreditCnt { get; set; }
    public uint PrevAreaCode { get; set; }
    public uint ConsecAreaCnt { get; set; }
    public uint DispLevelTotal { get; set; }
    public uint DispLevelChassis { get; set; }
    public uint DispLevelSelf { get; set; }
    public bool IsDevil { get; set; }
    public uint DispScoreType { get; set; }
    public uint DifficultyPlayedCourse { get; set; }
    public uint DifficultyPlayedStar { get; set; }
    public uint WaiwaiTutorialFlg { get; set; }
    public bool IsChallengeCompe { get; set; }
    public bool IsTojiru { get; set; }
    public bool IsExplain { get; set; }
    public int GhostInputMedian { get; set; }
    public uint GhostInputVariance { get; set; }
    public uint GhostRankId { get; set; }
    public uint GhostWinPoint { get; set; }
    public uint GhostCertifiedLevelId { get; set; }
    public uint GhostTotalWinnings { get; set; }
    public byte[] GhostReleaseInfoFlag { get; set; } = [];
    public byte[] GhostPlayedSongFlag { get; set; } = [];
    public DateTime LastPlayDatetime { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
