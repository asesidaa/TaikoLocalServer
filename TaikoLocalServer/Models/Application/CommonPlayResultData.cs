// ReSharper disable InconsistentNaming
namespace TaikoLocalServer.Models.Application;

public class CommonPlayResultData
{
    public uint            Baid                   { get; set; }
    public string          ChassisId              { get; set; } = string.Empty;
    public string          ShopId                 { get; set; } = string.Empty;
    public string          PlayDatetime           { get; set; } = string.Empty;
    public bool            IsRight                { get; set; }
    public uint            CardType               { get; set; }
    public bool            IsTwoPlayers           { get; set; }
    public List<StageData> AryStageInfoes         { get; set; } = [];
    public List<uint>      ReleaseSongNoes        { get; set; } = [];
    public List<uint>      UraReleaseSongNoes     { get; set; } = [];
    public List<uint>      GetToneNoes            { get; set; } = [];
    public List<uint>      GetCostumeNo1s         { get; set; } = [];
    public List<uint>      GetCostumeNo2s         { get; set; } = [];
    public List<uint>      GetCostumeNo3s         { get; set; } = [];
    public List<uint>      GetCostumeNo4s         { get; set; } = [];
    public List<uint>      GetCostumeNo5s         { get; set; } = [];
    public List<uint>      GetTitleNoes           { get; set; } = [];
    public List<uint>      GetGenericInfoNoes     { get; set; } = [];
    public CostumeData     AryPlayCostume         { get; set; } = new();
    public CostumeData     AryCurrentCostume      { get; set; } = new();
    public string          Title                  { get; set; } = string.Empty;
    public uint            TitleplateId           { get; set; }
    public uint            PlayMode               { get; set; }
    public uint            CollaborationId        { get; set; }
    public uint            DanId                  { get; set; }
    public uint            DanResult              { get; set; }
    public uint            SoulGaugeTotal         { get; set; }
    public uint            ComboCntTotal          { get; set; }
    public bool            IsNotRecordedDan       { get; set; }
    public uint            AreaCode               { get; set; }
    public byte[]          Reserved               { get; set; } = [];
    public uint            TournamentMode         { get; set; }
    public string          Accesstoken            { get; set; } = string.Empty;
    public byte[]          ContentInfo            { get; set; } = [];
    public uint            DifficultyPlayedCourse { get; set; }
    public uint            DifficultyPlayedStar   { get; set; }
    public uint            DifficultyPlayedSort   { get; set; }
    public uint            IsRandomUsePlay        { get; set; }
    public string          InputMedian            { get; set; } = string.Empty;
    public string          InputVariance          { get; set; } = string.Empty;
    
    public class StageData
    {
        public uint SongNo { get; set; }
        public uint Level { get; set; }
        public uint StageMode { get; set; }
        public uint PlayResult { get; set; }
        public uint PlayScore { get; set; }
        public uint ScoreRate { get; set; }
        public uint ScoreRank { get; set; }
        public uint GoodCnt { get; set; }
        public uint OkCnt { get; set; }
        public uint NgCnt { get; set; }
        public uint PoundCnt { get; set; }
        public uint ComboCnt { get; set; }
        public uint HitCnt { get; set; }
        public byte[] OptionFlg { get; set; } = [];
        public byte[] ToneFlg { get; set; } = [];
        public int NotesPosition { get; set; }
        public bool IsVoiceOn { get; set; }
        public bool IsSkipOn { get; set; }
        public bool IsSkipUse { get; set; }
        public uint SupportLevel { get; set; }
        public List<ResultcompeData> AryChallengeIds { get; set; } = [];
        public List<ResultcompeData> AryUserCompeIds { get; set; } = [];
        public List<ResultcompeData> AryBngCompeIds { get; set; } = [];
        public uint MusicCateg { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsRecent { get; set; }
        public uint SelectedFolderId { get; set; }
        public uint? IsRandomUseStage { get; set; }
        public bool IsPapamama { get; set; }
        public uint StarLevel { get; set; }
        public bool IsWin { get; set; }
        public List<AiStageSectionData> ArySectionDatas { get; set; } = [];
    }
    
    public class ResultcompeData
    {
        public uint CompeId { get; set; }
        public uint TrackNo { get; set; }
    }
    
    public class AiStageSectionData
    {
        public bool IsWin { get; set; }
        public uint Crown { get; set; }
        public uint Score { get; set; }
        public uint GoodCnt { get; set; }
        public uint OkCnt { get; set; }
        public uint NgCnt { get; set; }
        public uint PoundCnt { get; set; }
    }
    
    public class CostumeData
    {
        public uint Costume1 { get; set; }
        public uint Costume2 { get; set; }
        public uint Costume3 { get; set; }
        public uint Costume4 { get; set; }
        public uint Costume5 { get; set; }
    }
}