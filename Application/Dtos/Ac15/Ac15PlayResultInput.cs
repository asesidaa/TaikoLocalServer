namespace TaikoLocalServer.Application.Dtos.Ac15;

public sealed record Ac15PlayResultEnvelope(
    Ac15PlayResultMetadata Metadata,
    Ac15ProfileMutationFacts Profile,
    Ac15NormalPlayResult? Normal,
    Ac15DaniPlayResult? Dani,
    Ac15TokkunPlayResult? Tokkun,
    Ac15BlueBattlePlayResult? BlueBattle,
    Ac15GreenGhostPlayResult? GreenGhost);

public sealed record Ac15PlayResultMetadata(
    uint Baid,
    string ChassisId,
    string ShopId,
    string PlayDatetime,
    bool IsRight,
    uint CardType,
    bool IsTwoPlayers,
    uint PlayMode,
    uint AreaCode,
    byte[] Reserved,
    string Accesstoken,
    byte[] ContentInfo);

public sealed record Ac15NormalPlayResult(List<Ac15StageResult> Stages);

public sealed record Ac15DaniPlayResult(
    uint DanResult,
    uint ComboCntTotal,
    List<Ac15StageResult> Stages);

public sealed record Ac15ProfileMutationFacts
{
    public static Ac15ProfileMutationFacts Empty { get; } = new();

    public uint AreaCode { get; init; }
    public uint GetDonmedal { get; init; }
    public uint GetKatsumedal { get; init; }
    public uint GetDonpoint { get; init; }
    public uint? RewardPtn { get; init; }
    public uint? RewardProgress { get; init; }
    public uint? DifficultyTutorialFlg { get; init; }
    public uint? ItemshopTutorialFlg { get; init; }
    public uint? WaiwaiTutorialFlg { get; init; }
    public bool? IsDevil { get; init; }
    public bool? IsExplain { get; init; }
    public bool HasDifficultyPlayedCourse { get; init; }
    public uint DifficultyPlayedCourse { get; init; }
    public bool HasDifficultyPlayedStar { get; init; }
    public uint DifficultyPlayedStar { get; init; }
    public bool HasAryCurrentCostume { get; init; } = true;
    public Ac15CostumeFacts AryCurrentCostume { get; init; } = Ac15CostumeFacts.Empty;
    public List<uint> ReleaseSongNoes { get; init; } = [];
    public List<uint> GetToneNoes { get; init; } = [];
    public List<uint> GetCostumeNo1s { get; init; } = [];
    public List<uint> GetCostumeNo2s { get; init; } = [];
    public List<uint> GetCostumeNo3s { get; init; } = [];
    public List<uint> GetCostumeNo4s { get; init; } = [];
    public List<uint> GetCostumeNo5s { get; init; } = [];
    public List<uint> GetTitleNoes { get; init; } = [];
}

public sealed record Ac15StageResult
{
    public uint SongNo { get; init; }
    public uint Level { get; init; }
    public uint PlayResult { get; init; }
    public uint PlayScore { get; init; }
    public uint ScoreRate { get; init; }
    public uint ScoreRank { get; init; }
    public uint GoodCnt { get; init; }
    public uint OkCnt { get; init; }
    public uint NgCnt { get; init; }
    public uint PoundCnt { get; init; }
    public uint ComboCnt { get; init; }
    public uint HitCnt { get; init; }
    public byte[] OptionFlg { get; init; } = [];
    public byte[] ToneFlg { get; init; } = [];
    public uint SupportLevel { get; init; }
    public uint MusicCateg { get; init; }
    public bool IsFavorite { get; init; }
    public bool IsRecent { get; init; }
    public uint SelectedFolderId { get; init; }
    public uint StarLevel { get; init; }
    public bool IsWin { get; init; }
    public uint StageMode { get; init; }
    public bool IsPapamama { get; init; }
    public bool IsPushed { get; init; }
    public uint? SoulGauge { get; init; }
    public uint? HitCount { get; init; }
    public uint? PlayDan { get; init; }
    public uint? WaiwaiResult { get; init; }
    public uint? WaiwaiGauge { get; init; }
    public Ac15BlueBattleStageData? BlueBattleStage { get; init; }
    public Ac15GreenGhostStageData? GreenGhostStage { get; init; }
    public List<Ac15CompeIdFact> ChallengeIds { get; init; } = [];
    public List<Ac15CompeIdFact> UserCompeIds { get; init; } = [];
    public List<Ac15CompeIdFact> BngCompeIds { get; init; } = [];
    public List<Ac15AiStageSectionData> AiSectionData { get; init; } = [];
}

public sealed record Ac15CostumeFacts(uint Costume1 = 0, uint Costume2 = 0, uint Costume3 = 0, uint Costume4 = 0, uint Costume5 = 0)
{
    public static Ac15CostumeFacts Empty { get; } = new();

    public Ac15CostumeFacts() : this(0, 0, 0, 0, 0)
    {
    }
}

public sealed record Ac15CompeIdFact(uint CompeId, uint TrackNo);

public sealed record Ac15AiStageSectionData(
    bool IsWin,
    uint Crown,
    uint Score,
    uint GoodCnt,
    uint OkCnt,
    uint NgCnt,
    uint PoundCnt);

public sealed record Ac15TokkunPlayResult(
    uint? TutorialFlg,
    Ac15TokkunStageData? StageData);

public sealed record Ac15TokkunStageData(
    string BanacoinDatetime,
    uint TokkunSongCnt,
    List<uint> TookunSongnoes,
    uint TokkunSpeedchangeCnt,
    uint TokkunAutoplayCnt,
    uint TokkunJumpCnt);

public sealed record Ac15BlueBattlePlayResult(
    Ac15BlueBattleReleaseData? ReleaseData,
    List<Ac15StageResult> Stages,
    uint GetDonmedal);

public sealed record Ac15BlueBattleStageData
{
    public uint SupportLv { get; init; }
    public uint BattleStageId { get; init; }
    public Ac15BlueBattleNpcData? NpcData { get; init; }
    public uint KillCnt { get; init; }
    public uint BossLife { get; init; }
    public uint TotalDamage { get; init; }
    public uint CriticalCnt { get; init; }
    public uint SpecialMoveCnt { get; init; }
}

public sealed record Ac15BlueBattleNpcData
{
    public uint NpcId { get; init; }
    public string AcquiredExp { get; init; } = string.Empty;
    public string TotalExp { get; init; } = string.Empty;
    public uint Dpn { get; init; }
    public uint NpcCostumeId { get; init; }
    public uint SpecialId1 { get; init; }
    public uint SpecialId2 { get; init; }
    public uint SpecialId3 { get; init; }
    public uint BondsLv { get; init; }
}

public sealed record Ac15BlueBattleReleaseData
{
    public List<uint> ReleaseInfoIds { get; init; } = [];
    public List<uint> ReleaseBattleStageIds { get; init; } = [];
    public List<uint> ReleaseNpcIds { get; init; } = [];
    public List<uint> ReleaseNpcCostumeIds { get; init; } = [];
    public List<uint> ReleaseNpcSpecialIds { get; init; } = [];
    public List<Ac15BlueBattleTokenData> BattleTokenData { get; init; } = [];
    public uint AssignNextStageId { get; init; }
}

public sealed record Ac15BlueBattleTokenData(uint TokenId, uint TokenValue);

public sealed record Ac15GreenGhostPlayResult(
    Ac15GreenGhostReleaseData? ReleaseData,
    Ac15GreenGhostPerfData? PerfData,
    Ac15GreenGhostRankData? RankData);

public sealed record Ac15GreenGhostStageData
{
    public bool IsWin { get; init; }
    public uint SdCertifiedLevelId { get; init; }
    public List<Ac15GreenGhostStageSectionData> ArySectionData { get; init; } = [];
}

public sealed record Ac15GreenGhostStageSectionData(
    bool IsWin,
    uint GoodCnt,
    uint OkCnt,
    uint NgCnt,
    uint PoundCnt);

public sealed record Ac15GreenGhostReleaseData
{
    public List<uint> ReleaseInfoId { get; init; } = [];
    public List<Ac15GreenGhostTokenData> AryTokendata { get; init; } = [];
}

public sealed record Ac15GreenGhostTokenData(uint TokenId, uint TokenValue);

public sealed record Ac15GreenGhostPerfData(int InputMedian, uint InputVariance);

public sealed record Ac15GreenGhostRankData
{
    public uint RankId { get; init; }
    public uint WinPoint { get; init; }
    public uint CertifiedLevelId { get; init; }
    public List<Ac15GreenGhostWinningsData> AryWinningsData { get; init; } = [];
}

public sealed record Ac15GreenGhostWinningsData(uint LevelId, uint Winnings);
