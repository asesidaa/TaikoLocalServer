// ReSharper disable InconsistentNaming
namespace TaikoLocalServer.Application.Dtos;

public partial class CommonPlayResultData
{
    public uint GetDonmedal              { get; set; }
    public uint GetKatsumedal            { get; set; }
    public bool HasAryCurrentCostume     { get; set; } = true;
    public bool HasDifficultyPlayedCourse { get; set; } = true;
    public bool HasDifficultyPlayedStar  { get; set; } = true;
    public bool BonusDailyFlg            { get; set; }
    public bool BonusWeeklyFlg           { get; set; }
    public bool BonusMonthlyFlg          { get; set; }
    public uint GenderType               { get; set; }
    public uint PlayerAge                { get; set; }
    public uint? LowerlimitAge           { get; set; }
    public uint? UpperlimitAge           { get; set; }
    public uint? AgeScore                { get; set; }
    public uint? EstimationCount         { get; set; }
    public uint? ItemshopTutorialFlg     { get; set; }
    public bool? IsDevil                 { get; set; }
    public bool? IsExplain               { get; set; }
    public uint? WaiwaiTutorialFlg       { get; set; }
    public List<CollaboData> AryCollaboInfo { get; set; } = [];
    public UpdateGhostInfoData? GhostReleaseData { get; set; }
    public UpdateGhostPerfData? GhostUpdatePerfData { get; set; }
    public UpdateGhostRankData? GhostUpdateRankData { get; set; }

    public partial class StageData
    {
        public uint? WaiwaiResult { get; set; }
        public uint? WaiwaiGauge  { get; set; }
        public uint? SoulGauge    { get; set; }
        public uint? HitCount     { get; set; }
        public uint? PlayDan      { get; set; }
        public GhostStageData? GhostStageData { get; set; }
        public bool IsPushed { get; set; }
    }

    public class CollaboData
    {
        public uint  CollaboSelect { get; set; }
        public uint? CollaboId     { get; set; }
        public uint? CollaboResult { get; set; }
    }

    public class GhostStageData
    {
        public bool IsWin { get; set; }
        public uint SdCertifiedLevelId { get; set; }
        public List<GhostStageSectionData> ArySectionData { get; set; } = [];
    }

    public class GhostStageSectionData
    {
        public bool IsWin    { get; set; }
        public uint GoodCnt  { get; set; }
        public uint OkCnt    { get; set; }
        public uint NgCnt    { get; set; }
        public uint PoundCnt { get; set; }
    }

    public class UpdateGhostInfoData
    {
        public List<uint> ReleaseInfoId { get; set; } = [];
        public List<GhostTokenData> AryTokendata { get; set; } = [];
    }

    public class GhostTokenData
    {
        public uint TokenId    { get; set; }
        public uint TokenValue { get; set; }
    }

    public class UpdateGhostPerfData
    {
        public int  InputMedian   { get; set; }
        public uint InputVariance { get; set; }
    }

    public class UpdateGhostRankData
    {
        public uint RankId { get; set; }
        public uint WinPoint { get; set; }
        public uint CertifiedLevelId { get; set; }
        public List<GhostWinningsData> AryWinningsData { get; set; } = [];
    }

    public class GhostWinningsData
    {
        public uint LevelId  { get; set; }
        public uint Winnings { get; set; }
    }
}
