# AC15 Playresult Boundary and Command Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the AC15 capability input record set and a dedicated AC15 playresult command without changing runtime behavior yet.

**Architecture:** New records live under `Application/Dtos/Ac15` and represent facts after adapter mapping. `UpdatePlayResultCommand` remains the Nijiiro command; `UpdateAc15PlayResultCommand` is added for Blue, Green, Yellow, and Red and initially coexists with the current handler code.

**Tech Stack:** C# 13 records, Mediator request records, xUnit compile and shape tests, .NET 10.

---

## Files

- Create: `Application/Dtos/Ac15/Ac15PlayResultInput.cs`
- Create: `Application/Dtos/Ac15/Ac15PlayResultCommonBridge.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.cs`
- Create: `Application/Handlers/UpdateAc15PlayResultCommand.cs`
- Modify: `Tests/GlobalUsings.cs`
- Create: `Tests/Ac15/Ac15PlayResultInputTests.cs`

`Ac15PlayResultCommonBridge` is an internal transition tool used only between checkpoints 2 and 5. It must be deleted in checkpoint 5 before final verification.

### Task 1: Add Capability Records

**Files:**
- Create: `Application/Dtos/Ac15/Ac15PlayResultInput.cs`
- Test: `Tests/Ac15/Ac15PlayResultInputTests.cs`

- [ ] **Step 1: Write the failing record-construction test**

Add `Tests/Ac15/Ac15PlayResultInputTests.cs`:

```csharp
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15PlayResultInputTests
{
    [Fact]
    public void Envelope_CanCarryOnlySupportedCapabilityFacts()
    {
        var envelope = new Ac15PlayResultEnvelope(
            Metadata: new Ac15PlayResultMetadata(
                Baid: 1,
                ChassisId: "268410000000",
                ShopId: "JPN0JPN0123",
                PlayDatetime: "20260613120000",
                IsRight: false,
                CardType: 1,
                IsTwoPlayers: false,
                PlayMode: (uint)PlayMode.Normal,
                AreaCode: 12,
                Reserved: [1, 2],
                Accesstoken: "",
                ContentInfo: [3, 4]),
            Profile: Ac15ProfileMutationFacts.Empty with
            {
                AreaCode = 12,
                GetDonmedal = 25,
                GetToneNoes = [4]
            },
            Normal: new Ac15NormalPlayResult(
                Stages:
                [
                    new Ac15StageResult
                    {
                        SongNo = 101,
                        Level = 1,
                        StageMode = 0,
                        PlayResult = 2,
                        PlayScore = 765432,
                        IsFavorite = true,
                        IsRecent = true
                    }
                ]),
            Dani: null,
            Tokkun: null,
            BlueBattle: null,
            GreenGhost: null,
            ChallengeCompe: null);

        Assert.Equal(1u, envelope.Metadata.Baid);
        Assert.Null(envelope.Tokkun);
        Assert.Null(envelope.BlueBattle);
        Assert.Single(envelope.Normal!.Stages);
        Assert.Equal([4u], envelope.Profile.GetToneNoes);
    }
}
```

- [ ] **Step 2: Run the focused test to verify it fails**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15PlayResultInputTests"
```

Expected: fail with missing `TaikoLocalServer.Application.Dtos.Ac15` types.

- [ ] **Step 3: Add the capability record file**

Create `Application/Dtos/Ac15/Ac15PlayResultInput.cs`:

```csharp
namespace TaikoLocalServer.Application.Dtos.Ac15;

public sealed record Ac15PlayResultEnvelope(
    Ac15PlayResultMetadata Metadata,
    Ac15ProfileMutationFacts Profile,
    Ac15NormalPlayResult? Normal,
    Ac15DaniPlayResult? Dani,
    Ac15TokkunPlayResult? Tokkun,
    Ac15BlueBattlePlayResult? BlueBattle,
    Ac15GreenGhostPlayResult? GreenGhost,
    Ac15RedChallengeCompeFacts? ChallengeCompe);

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

public sealed record Ac15CostumeFacts(uint Costume1, uint Costume2, uint Costume3, uint Costume4, uint Costume5)
{
    public static Ac15CostumeFacts Empty { get; } = new(0, 0, 0, 0, 0);
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

public sealed record Ac15RedChallengeCompeFacts(
    List<Ac15RedChallengeCompeStageFacts> Stages);

public sealed record Ac15RedChallengeCompeStageFacts(
    uint SongNo,
    List<Ac15CompeIdFact> ChallengeIds,
    List<Ac15CompeIdFact> UserCompeIds,
    List<Ac15CompeIdFact> BngCompeIds);
```

- [ ] **Step 4: Run the focused test to verify it passes**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15PlayResultInputTests"
```

Expected: pass.

### Task 2: Add AC15 Command Boundary

**Files:**
- Modify: `Application/Handlers/UpdatePlayResultCommand.cs`
- Create: `Application/Handlers/UpdateAc15PlayResultCommand.cs`

- [ ] **Step 1: Write the compile target in the command file**

Create `Application/Handlers/UpdateAc15PlayResultCommand.cs`:

```csharp
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct UpdateAc15PlayResultCommand(
    uint Baid,
    GameEra Era,
    Ac15PlayResultEnvelope PlayResultData) : IRequest<uint>;
```

- [ ] **Step 2: Extend the existing handler class to implement both request handlers**

In `Application/Handlers/UpdatePlayResultCommand.cs`, add the using and replace the class declaration and `Handle` switch with this shape:

```csharp
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Dtos.Ac15;
using TaikoLocalServer.Application.Settings;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct UpdatePlayResultCommand(uint Baid, GameEra Era, CommonPlayResultData PlayResultData) : IRequest<uint>;

public partial class UpdatePlayResultCommandHandler(
    ITaikoDbContext context,
    IGameDataCatalog gameDataService,
    ILogger<UpdatePlayResultCommandHandler> logger,
    IOptions<ServerSettings>? settings = null)
    : IRequestHandler<UpdatePlayResultCommand, uint>,
      IRequestHandler<UpdateAc15PlayResultCommand, uint>
{
    private readonly ServerSettings settings = settings?.Value ?? new ServerSettings();

    public ValueTask<uint> Handle(UpdatePlayResultCommand request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported non-Nijiiro playresult command era: {request.Era}")
    };

    public ValueTask<uint> Handle(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Green => HandleGreen(request, cancellationToken),
        GameEra.Blue => HandleBlue(request, cancellationToken),
        GameEra.Yellow => HandleYellow(request, cancellationToken),
        GameEra.Red => HandleRed(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported AC15 playresult command era: {request.Era}")
    };

    private partial ValueTask<uint> HandleNijiiro(UpdatePlayResultCommand request, CancellationToken cancellationToken);
    private partial ValueTask<uint> HandleGreen(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken);
    private partial ValueTask<uint> HandleBlue(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken);
    private partial ValueTask<uint> HandleYellow(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken);
    private partial ValueTask<uint> HandleRed(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken);
}
```

- [ ] **Step 3: Temporarily keep AC15 partials compiling**

Do not run the build at this step. Task 3 adjusts the AC15 partial signatures and adds the temporary bridge before the checkpoint build.

### Task 3: Add Temporary Common Bridge

**Files:**
- Create: `Application/Dtos/Ac15/Ac15PlayResultCommonBridge.cs`

- [ ] **Step 1: Add the bridge file**

Create `Application/Dtos/Ac15/Ac15PlayResultCommonBridge.cs`:

```csharp
namespace TaikoLocalServer.Application.Dtos.Ac15;

internal static class Ac15PlayResultCommonBridge
{
    public static CommonPlayResultData ToCommon(Ac15PlayResultEnvelope envelope)
    {
        var stages = envelope.Normal?.Stages
                     ?? envelope.BlueBattle?.Stages
                     ?? envelope.Dani?.Stages
                     ?? [];

        return new CommonPlayResultData
        {
            Baid = envelope.Metadata.Baid,
            ChassisId = envelope.Metadata.ChassisId,
            ShopId = envelope.Metadata.ShopId,
            PlayDatetime = envelope.Metadata.PlayDatetime,
            IsRight = envelope.Metadata.IsRight,
            CardType = envelope.Metadata.CardType,
            IsTwoPlayers = envelope.Metadata.IsTwoPlayers,
            PlayMode = envelope.Metadata.PlayMode,
            AreaCode = envelope.Profile.AreaCode,
            Reserved = envelope.Metadata.Reserved,
            Accesstoken = envelope.Metadata.Accesstoken,
            ContentInfo = envelope.Metadata.ContentInfo,
            GetDonmedal = envelope.Profile.GetDonmedal,
            GetKatsumedal = envelope.Profile.GetKatsumedal,
            GetDonpoint = envelope.Profile.GetDonpoint,
            RewardPtn = envelope.Profile.RewardPtn,
            RewardProgress = envelope.Profile.RewardProgress,
            DifficultyTutorialFlg = envelope.Profile.DifficultyTutorialFlg,
            ItemshopTutorialFlg = envelope.Profile.ItemshopTutorialFlg,
            WaiwaiTutorialFlg = envelope.Profile.WaiwaiTutorialFlg,
            IsDevil = envelope.Profile.IsDevil,
            IsExplain = envelope.Profile.IsExplain,
            HasDifficultyPlayedCourse = envelope.Profile.HasDifficultyPlayedCourse,
            DifficultyPlayedCourse = envelope.Profile.DifficultyPlayedCourse,
            HasDifficultyPlayedStar = envelope.Profile.HasDifficultyPlayedStar,
            DifficultyPlayedStar = envelope.Profile.DifficultyPlayedStar,
            HasAryCurrentCostume = envelope.Profile.HasAryCurrentCostume,
            AryCurrentCostume = ToCommonCostume(envelope.Profile.AryCurrentCostume),
            ReleaseSongNoes = envelope.Profile.ReleaseSongNoes,
            GetToneNoes = envelope.Profile.GetToneNoes,
            GetCostumeNo1s = envelope.Profile.GetCostumeNo1s,
            GetCostumeNo2s = envelope.Profile.GetCostumeNo2s,
            GetCostumeNo3s = envelope.Profile.GetCostumeNo3s,
            GetCostumeNo4s = envelope.Profile.GetCostumeNo4s,
            GetCostumeNo5s = envelope.Profile.GetCostumeNo5s,
            GetTitleNoes = envelope.Profile.GetTitleNoes,
            DanResult = envelope.Dani?.DanResult ?? 0,
            ComboCntTotal = envelope.Dani?.ComboCntTotal ?? 0,
            TokkunTutorialFlg = envelope.Tokkun?.TutorialFlg,
            TokkunStageData = envelope.Tokkun?.StageData is { } tokkun ? ToCommonTokkun(tokkun) : null,
            BattleReleaseData = envelope.BlueBattle?.ReleaseData is { } battleRelease ? ToCommonBattleRelease(battleRelease) : null,
            GhostReleaseData = envelope.GreenGhost?.ReleaseData is { } ghostRelease ? ToCommonGhostRelease(ghostRelease) : null,
            GhostUpdatePerfData = envelope.GreenGhost?.PerfData is { } perf ? new CommonPlayResultData.UpdateGhostPerfData
            {
                InputMedian = perf.InputMedian,
                InputVariance = perf.InputVariance
            } : null,
            GhostUpdateRankData = envelope.GreenGhost?.RankData is { } rank ? ToCommonGhostRank(rank) : null,
            AryStageInfoes = stages.Select(ToCommonStage).ToList()
        };
    }

    private static CommonPlayResultData.StageData ToCommonStage(Ac15StageResult stage)
        => new()
        {
            SongNo = stage.SongNo,
            Level = stage.Level,
            PlayResult = stage.PlayResult,
            PlayScore = stage.PlayScore,
            ScoreRate = stage.ScoreRate,
            ScoreRank = stage.ScoreRank,
            GoodCnt = stage.GoodCnt,
            OkCnt = stage.OkCnt,
            NgCnt = stage.NgCnt,
            PoundCnt = stage.PoundCnt,
            ComboCnt = stage.ComboCnt,
            HitCnt = stage.HitCnt,
            OptionFlg = stage.OptionFlg,
            ToneFlg = stage.ToneFlg,
            SupportLevel = stage.SupportLevel,
            MusicCateg = stage.MusicCateg,
            IsFavorite = stage.IsFavorite,
            IsRecent = stage.IsRecent,
            SelectedFolderId = stage.SelectedFolderId,
            StarLevel = stage.StarLevel,
            IsWin = stage.IsWin,
            StageMode = stage.StageMode,
            IsPapamama = stage.IsPapamama,
            IsPushed = stage.IsPushed,
            SoulGauge = stage.SoulGauge,
            HitCount = stage.HitCount,
            PlayDan = stage.PlayDan,
            WaiwaiResult = stage.WaiwaiResult,
            WaiwaiGauge = stage.WaiwaiGauge,
            BattleStageData = stage.BlueBattleStage is { } battle ? ToCommonBattleStage(battle) : null,
            GhostStageData = stage.GreenGhostStage is { } ghost ? ToCommonGhostStage(ghost) : null,
            AryChallengeIds = stage.ChallengeIds.Select(ToCommonCompe).ToList(),
            AryUserCompeIds = stage.UserCompeIds.Select(ToCommonCompe).ToList(),
            AryBngCompeIds = stage.BngCompeIds.Select(ToCommonCompe).ToList(),
            ArySectionDatas = stage.AiSectionData.Select(section => new CommonPlayResultData.AiStageSectionData
            {
                IsWin = section.IsWin,
                Crown = section.Crown,
                Score = section.Score,
                GoodCnt = section.GoodCnt,
                OkCnt = section.OkCnt,
                NgCnt = section.NgCnt,
                PoundCnt = section.PoundCnt
            }).ToList()
        };

    private static CommonPlayResultData.CostumeData ToCommonCostume(Ac15CostumeFacts costume)
        => new()
        {
            Costume1 = costume.Costume1,
            Costume2 = costume.Costume2,
            Costume3 = costume.Costume3,
            Costume4 = costume.Costume4,
            Costume5 = costume.Costume5
        };

    private static CommonPlayResultData.TokkunStageDataDto ToCommonTokkun(Ac15TokkunStageData data)
        => new()
        {
            BanacoinDatetime = data.BanacoinDatetime,
            TokkunSongCnt = data.TokkunSongCnt,
            TookunSongnoes = data.TookunSongnoes,
            TokkunSpeedchangeCnt = data.TokkunSpeedchangeCnt,
            TokkunAutoplayCnt = data.TokkunAutoplayCnt,
            TokkunJumpCnt = data.TokkunJumpCnt
        };

    private static CommonPlayResultData.BattleStageData ToCommonBattleStage(Ac15BlueBattleStageData data)
        => new()
        {
            SupportLv = data.SupportLv,
            BattleStageId = data.BattleStageId,
            NpcData = data.NpcData is { } npc ? new CommonPlayResultData.BattleNpcData
            {
                NpcId = npc.NpcId,
                AcquiredExp = npc.AcquiredExp,
                TotalExp = npc.TotalExp,
                Dpn = npc.Dpn,
                NpcCostumeId = npc.NpcCostumeId,
                SpecialId1 = npc.SpecialId1,
                SpecialId2 = npc.SpecialId2,
                SpecialId3 = npc.SpecialId3,
                BondsLv = npc.BondsLv
            } : null,
            KillCnt = data.KillCnt,
            BossLife = data.BossLife,
            TotalDamage = data.TotalDamage,
            CriticalCnt = data.CriticalCnt,
            SpecialMoveCnt = data.SpecialMoveCnt
        };

    private static CommonPlayResultData.BattleReleaseDataDto ToCommonBattleRelease(Ac15BlueBattleReleaseData data)
        => new()
        {
            ReleaseInfoIds = data.ReleaseInfoIds,
            ReleaseBattleStageIds = data.ReleaseBattleStageIds,
            ReleaseNpcIds = data.ReleaseNpcIds,
            ReleaseNpcCostumeIds = data.ReleaseNpcCostumeIds,
            ReleaseNpcSpecialIds = data.ReleaseNpcSpecialIds,
            BattleTokenData = data.BattleTokenData.Select(token => new CommonPlayResultData.BattleTokenData
            {
                TokenId = token.TokenId,
                TokenValue = token.TokenValue
            }).ToList(),
            AssignNextStageId = data.AssignNextStageId
        };

    private static CommonPlayResultData.GhostStageData ToCommonGhostStage(Ac15GreenGhostStageData data)
        => new()
        {
            IsWin = data.IsWin,
            SdCertifiedLevelId = data.SdCertifiedLevelId,
            ArySectionData = data.ArySectionData.Select(section => new CommonPlayResultData.GhostStageSectionData
            {
                IsWin = section.IsWin,
                GoodCnt = section.GoodCnt,
                OkCnt = section.OkCnt,
                NgCnt = section.NgCnt,
                PoundCnt = section.PoundCnt
            }).ToList()
        };

    private static CommonPlayResultData.UpdateGhostInfoData ToCommonGhostRelease(Ac15GreenGhostReleaseData data)
        => new()
        {
            ReleaseInfoId = data.ReleaseInfoId,
            AryTokendata = data.AryTokendata.Select(token => new CommonPlayResultData.GhostTokenData
            {
                TokenId = token.TokenId,
                TokenValue = token.TokenValue
            }).ToList()
        };

    private static CommonPlayResultData.UpdateGhostRankData ToCommonGhostRank(Ac15GreenGhostRankData data)
        => new()
        {
            RankId = data.RankId,
            WinPoint = data.WinPoint,
            CertifiedLevelId = data.CertifiedLevelId,
            AryWinningsData = data.AryWinningsData.Select(row => new CommonPlayResultData.GhostWinningsData
            {
                LevelId = row.LevelId,
                Winnings = row.Winnings
            }).ToList()
        };

    private static CommonPlayResultData.ResultcompeData ToCommonCompe(Ac15CompeIdFact fact)
        => new()
        {
            CompeId = fact.CompeId,
            TrackNo = fact.TrackNo
        };
}
```

- [ ] **Step 2: Keep AC15 partials compiling through the bridge**

For this checkpoint only, update the signatures in these files to accept `UpdateAc15PlayResultCommand` and immediately bridge to current old logic:

- `Application/Handlers/UpdatePlayResultCommand.Blue.cs`
- `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`
- `Application/Handlers/UpdatePlayResultCommand.Red.cs`

At the top of each file, add:

```csharp
using TaikoLocalServer.Application.Dtos.Ac15;
```

Then inside each AC15 handler method, replace:

```csharp
var playResultData = request.PlayResultData;
```

with:

```csharp
var playResultData = Ac15PlayResultCommonBridge.ToCommon(request.PlayResultData);
```

- [ ] **Step 3: Run build for the boundary checkpoint**

Run:

```powershell
dotnet build TaikoLocalServer.slnx
```

Expected: exit `0`. If `Host/bin/Debug/net10.0` is locked, run:

```powershell
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
```

Expected: exit `0`.

- [ ] **Step 4: Commit checkpoint 1**

Run:

```powershell
git add Application/Dtos/Ac15/Ac15PlayResultInput.cs Application/Dtos/Ac15/Ac15PlayResultCommonBridge.cs Application/Handlers/UpdatePlayResultCommand.cs Application/Handlers/UpdateAc15PlayResultCommand.cs Application/Handlers/UpdatePlayResultCommand.Blue.cs Application/Handlers/UpdatePlayResultCommand.Green.cs Application/Handlers/UpdatePlayResultCommand.Yellow.cs Application/Handlers/UpdatePlayResultCommand.Red.cs Tests/Ac15/Ac15PlayResultInputTests.cs
git commit -m "Add AC15 playresult input boundary"
```
