# AC15 Special Mode Capability Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Migrate Tokkun, Blue battle, Green ghost, and Red ChallengeCompe fact paths to capability records and update behavior tests to use `UpdateAc15PlayResultCommand`.

**Architecture:** Special-mode classification stays in era handlers. Blue checks Tokkun before battle before normal; Yellow and Red check Tokkun before normal; Green applies ghost facts only inside Green normal handling. Blue battle and Tokkun helpers consume capability records directly.

**Tech Stack:** C# records, EF Core SQLite behavior tests, xUnit handler tests.

---

## Files

- Modify: `Application/Handlers/UpdatePlayResultCommand.Blue.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.BlueBattle.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.BlueTokkun.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.YellowTokkun.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Red.cs`
- Modify: `Application/Common/BlueBattleStateExtensions.cs`
- Modify: `Tests/Blue/BlueBattlePlayResultHandlerTests.cs`
- Modify: `Tests/Blue/BluePlayResultHandlerTests.cs`
- Modify: `Tests/Yellow/YellowPlayResultHandlerTests.cs`
- Modify: `Tests/Red/RedPlayResultHandlerTests.cs`

### Task 1: Migrate Tokkun Helpers

**Files:**
- Modify: `Application/Handlers/UpdatePlayResultCommand.Blue.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.BlueTokkun.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.YellowTokkun.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Red.cs`

- [ ] **Step 1: Update Tokkun classifiers**

In Blue handler, replace:

```csharp
private static bool IsBlueTokkunShaped(CommonPlayResultData playResultData)
    => playResultData.IsTokkunPlayResult
       || playResultData.PlayMode == (uint)PlayMode.Tokkun
       || playResultData.TokkunTutorialFlg is not null
       || playResultData.TokkunStageData is not null;
```

with:

```csharp
private static bool IsBlueTokkunShaped(Ac15PlayResultEnvelope playResultData)
    => playResultData.Metadata.PlayMode == (uint)PlayMode.Tokkun
       || playResultData.Tokkun is not null;
```

In Yellow and Red handlers, use the same body with method names `IsYellowTokkunShaped` and `IsRedTokkunShaped`.

- [ ] **Step 2: Update Blue Tokkun helper signatures**

In `Application/Handlers/UpdatePlayResultCommand.BlueTokkun.cs`, add:

```csharp
using TaikoLocalServer.Application.Dtos.Ac15;
```

Change signatures:

```csharp
private async ValueTask<uint> HandleBlueTokkun(
    uint baid,
    Ac15PlayResultEnvelope playResultData,
    CancellationToken cancellationToken)
```

and:

```csharp
private async ValueTask SaveBlueTokkun(
    uint baid,
    Ac15PlayResultEnvelope playResultData,
    CancellationToken cancellationToken)
```

Replace field reads:

```csharp
if (playResultData.Tokkun?.TutorialFlg is { } tokkunTutorialFlg)
```

and:

```csharp
if (playResultData.Tokkun?.StageData is { } tokkunStageData)
```

Use metadata for row values:

```csharp
PlayDatetime = playResultData.Metadata.PlayDatetime,
PlayMode = playResultData.Metadata.PlayMode,
```

- [ ] **Step 3: Update Yellow Tokkun helper signatures**

In `Application/Handlers/UpdatePlayResultCommand.YellowTokkun.cs`, add the same using and change `CommonPlayResultData` to `Ac15PlayResultEnvelope`. Replace row values with metadata and Tokkun capability fields:

```csharp
if (playResultData.Tokkun?.TutorialFlg is { } tokkunTutorialFlg)
{
    saveData.TokkunTutorialFlg = tokkunTutorialFlg;
}

if (playResultData.Tokkun?.StageData is { } tokkunStageData)
{
    context.YellowTokkunStageResults.Add(new YellowTokkunStageResult
    {
        Baid = baid,
        PlayDatetime = playResultData.Metadata.PlayDatetime,
        PlayMode = playResultData.Metadata.PlayMode,
        BanacoinDatetime = tokkunStageData.BanacoinDatetime,
        TokkunSongCnt = tokkunStageData.TokkunSongCnt,
        TookunSongnoesJson = JsonSerializer.Serialize(tokkunStageData.TookunSongnoes),
        TokkunSpeedchangeCnt = tokkunStageData.TokkunSpeedchangeCnt,
        TokkunAutoplayCnt = tokkunStageData.TokkunAutoplayCnt,
        TokkunJumpCnt = tokkunStageData.TokkunJumpCnt
    });
}
```

- [ ] **Step 4: Update Red Tokkun inline helper**

In `Application/Handlers/UpdatePlayResultCommand.Red.cs`, change `HandleRedTokkun` signature:

```csharp
private async ValueTask<uint> HandleRedTokkun(
    uint baid,
    Ac15PlayResultEnvelope playResultData,
    CancellationToken cancellationToken)
```

Replace:

```csharp
if (playResultData.TokkunTutorialFlg is { } tokkunTutorialFlg)
```

with:

```csharp
if (playResultData.Tokkun?.TutorialFlg is { } tokkunTutorialFlg)
```

- [ ] **Step 5: Run Tokkun behavior tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Tokkun|FullyQualifiedName~RedPlayResultHandlerTests"
```

Expected: fail until Task 4 updates handler tests to construct `UpdateAc15PlayResultCommand`.

### Task 2: Migrate Blue Battle Helpers

**Files:**
- Modify: `Application/Handlers/UpdatePlayResultCommand.Blue.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.BlueBattle.cs`
- Modify: `Application/Common/BlueBattleStateExtensions.cs`

- [ ] **Step 1: Update Blue battle classifier**

Replace:

```csharp
private static bool IsBlueBattleShaped(CommonPlayResultData playResultData)
    => playResultData.IsBattlePlayResult
       || playResultData.BattleReleaseData is not null
       || playResultData.AryStageInfoes.Any(stage => stage.BattleStageData is not null);
```

with:

```csharp
private static bool IsBlueBattleShaped(Ac15PlayResultEnvelope playResultData)
    => playResultData.BlueBattle is not null;
```

- [ ] **Step 2: Update Blue battle handler signature**

In `Application/Handlers/UpdatePlayResultCommand.BlueBattle.cs`, add:

```csharp
using TaikoLocalServer.Application.Dtos.Ac15;
```

Change the helper signature:

```csharp
private async ValueTask<uint> HandleBlueBattle(
    uint baid,
    Ac15BlueBattlePlayResult battle,
    Ac15PlayResultMetadata metadata,
    CancellationToken cancellationToken)
```

Update caller in `HandleBlue`:

```csharp
if (playResultData.BlueBattle is { } battle)
{
    return await HandleBlueBattle(request.Baid, battle, playResultData.Metadata, cancellationToken);
}
```

- [ ] **Step 3: Update battle helper body**

In `HandleBlueBattle`, replace play time parsing and helper calls with:

```csharp
var now = DateTime.UtcNow;
var playTime = ParseAc15PlayDatetimeOrNow(metadata.PlayDatetime);

await context.AddBlueBattleStageResultsAsync(
    baid,
    battle.Stages,
    metadata.PlayMode,
    playTime,
    now,
    cancellationToken);
await context.ApplyBlueBattleReleaseDataAsync(
    baid,
    battle.ReleaseData,
    now,
    cancellationToken);
await AddBlueBattleShopDonmedalsAsync(baid, battle.GetDonmedal, now, cancellationToken);
await UpsertBlueBattleRecentSongsAsync(
    baid,
    battle.Stages,
    playTime,
    cancellationToken);
```

Change `UpsertBlueBattleRecentSongsAsync` to accept `IReadOnlyList<Ac15StageResult> stages` and iterate those stages.

- [ ] **Step 4: Update Blue battle state extension signatures**

In `Application/Common/BlueBattleStateExtensions.cs`, add:

```csharp
using TaikoLocalServer.Application.Dtos.Ac15;
```

Change `AddBlueBattleStageResultsAsync` to:

```csharp
public static async Task AddBlueBattleStageResultsAsync(
    this ITaikoDbContext context,
    uint baid,
    IReadOnlyList<Ac15StageResult> stages,
    uint playMode,
    DateTime playTime,
    DateTime now,
    CancellationToken cancellationToken)
```

Inside, replace:

```csharp
for (var index = 0; index < playResultData.AryStageInfoes.Count; index++)
{
    var stage = playResultData.AryStageInfoes[index];
    var battleStage = stage.BattleStageData;
```

with:

```csharp
for (var index = 0; index < stages.Count; index++)
{
    var stage = stages[index];
    var battleStage = stage.BlueBattleStage;
```

Replace `PlayMode = playResultData.PlayMode` with `PlayMode = playMode`.

Change `ApplyBlueBattleReleaseDataAsync` release parameter to:

```csharp
Ac15BlueBattleReleaseData? releaseData,
```

Change `UpsertBlueBattleNpcStateAsync` parameter to `Ac15BlueBattleNpcData npc`.

Change `UpsertBlueBattleTokenStateAsync` parameter to `Ac15BlueBattleTokenData token`.

- [ ] **Step 5: Run Blue battle tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueBattlePlayResultHandlerTests"
```

Expected: fail until tests use the new command and records in Task 4.

### Task 3: Migrate Green Ghost Helpers

**Files:**
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`

- [ ] **Step 1: Update ghost stage section helper**

In `AddGreenGhostStageSections`, no signature change is needed if `Ac15PlayRow.GhostStageData` was migrated in checkpoint 3. Keep the body and read `row.GhostStageData.ArySectionData`.

- [ ] **Step 2: Update played-song bit helper**

Change:

```csharp
private static void ApplyGhostPlayedSongBits(UserSaveDataGreen saveData, CommonPlayResultData playResultData)
```

to:

```csharp
private static void ApplyGhostPlayedSongBits(UserSaveDataGreen saveData, IReadOnlyList<Ac15StageResult> stages)
```

Replace:

```csharp
var aiBattleSongNos = playResultData.AryStageInfoes
```

with:

```csharp
var aiBattleSongNos = stages
```

Update caller:

```csharp
ApplyGhostPlayedSongBits(saveData, validStages);
```

- [ ] **Step 3: Update ghost updates helper**

Change:

```csharp
private async Task ApplyGhostUpdatesAsync(UserSaveDataGreen saveData, CommonPlayResultData playResultData, CancellationToken cancellationToken)
```

to:

```csharp
private async Task ApplyGhostUpdatesAsync(
    UserSaveDataGreen saveData,
    Ac15GreenGhostPlayResult? ghost,
    CancellationToken cancellationToken)
```

At method start add:

```csharp
if (ghost is null)
{
    return;
}
```

Replace `playResultData.GhostReleaseData` with `ghost.ReleaseData`, `playResultData.GhostUpdatePerfData` with `ghost.PerfData`, and `playResultData.GhostUpdateRankData` with `ghost.RankData`.

Update caller:

```csharp
await ApplyGhostUpdatesAsync(saveData, playResultData.GreenGhost, cancellationToken);
```

- [ ] **Step 4: Run Green behavior tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayResultHandlerTests|FullyQualifiedName~GreenGhostRewardTests"
```

Expected: handler tests that still use `CommonPlayResultData` fail until Task 4 updates command construction.

### Task 4: Update Handler Tests to AC15 Command Records

**Files:**
- Modify: `Tests/Blue/BlueBattlePlayResultHandlerTests.cs`
- Modify: `Tests/Blue/BluePlayResultHandlerTests.cs`
- Modify: `Tests/Yellow/YellowPlayResultHandlerTests.cs`
- Modify: `Tests/Red/RedPlayResultHandlerTests.cs`

- [ ] **Step 1: Add test helper builders**

In each listed test class, add helpers like these and adapt era-specific profile facts:

```csharp
private static UpdateAc15PlayResultCommand Command(
    uint baid,
    GameEra era,
    uint playMode = 0,
    Ac15ProfileMutationFacts? profile = null,
    List<Ac15StageResult>? stages = null,
    Ac15TokkunPlayResult? tokkun = null,
    Ac15BlueBattlePlayResult? battle = null,
    Ac15GreenGhostPlayResult? ghost = null)
{
    var stageList = stages ?? [];
    var metadata = new Ac15PlayResultMetadata(
        Baid: baid,
        ChassisId: "268410000000",
        ShopId: "JPN0JPN0123",
        PlayDatetime: "20260608120000",
        IsRight: false,
        CardType: 1,
        IsTwoPlayers: false,
        PlayMode: playMode,
        AreaCode: profile?.AreaCode ?? 1,
        Reserved: [],
        Accesstoken: "",
        ContentInfo: []);

    var envelope = new Ac15PlayResultEnvelope(
        metadata,
        profile ?? Ac15ProfileMutationFacts.Empty,
        stageList.Count == 0 ? null : new Ac15NormalPlayResult(stageList),
        new Ac15DaniPlayResult(DanResult: 0, ComboCntTotal: 0, Stages: stageList),
        tokkun,
        battle,
        ghost,
        null);

    return new UpdateAc15PlayResultCommand(baid, era, envelope);
}
```

For Dani tests, pass:

```csharp
new Ac15DaniPlayResult(
    DanResult: (uint)Ac15DanClearGrade.GoldClear,
    ComboCntTotal: 320,
    Stages: stages)
```

inside the envelope instead of the default Dani record.

- [ ] **Step 2: Replace AC15 command construction**

Replace every AC15 handler test call like:

```csharp
new UpdatePlayResultCommand(
    1,
    GameEra.Yellow,
    new CommonPlayResultData
    {
        Baid = 1,
        AryStageInfoes = [CreateStage(101, 1, 0)]
    })
```

with:

```csharp
Command(
    1,
    GameEra.Yellow,
    stages: [CreateStage(101, 1, 0)])
```

Keep `UpdatePlayResultCommand` only in Nijiiro tests.

- [ ] **Step 3: Change test stage helper return type**

Replace `CommonPlayResultData.StageData` helpers in AC15 handler tests with:

```csharp
private static Ac15StageResult CreateStage(
    uint songNo,
    uint level,
    uint stageMode,
    uint score = 765432,
    uint playDan = 0,
    uint soulGauge = 100,
    uint comboCnt = 120,
    uint goodCnt = 100,
    uint okCnt = 20,
    uint ngCnt = 3)
    => new()
    {
        SongNo = songNo,
        Level = level,
        StageMode = stageMode,
        PlayResult = 2,
        PlayScore = score,
        ScoreRate = 95,
        GoodCnt = goodCnt,
        OkCnt = okCnt,
        NgCnt = ngCnt,
        PoundCnt = 4,
        ComboCnt = comboCnt,
        HitCnt = 123,
        OptionFlg = [1, 2, 3],
        ToneFlg = [4],
        MusicCateg = 1,
        IsPushed = true,
        IsFavorite = true,
        IsRecent = true,
        SelectedFolderId = 9,
        PlayDan = playDan == 0 ? null : playDan,
        SoulGauge = soulGauge
    };
```

For Blue battle tests, use:

```csharp
BlueBattleStage = new Ac15BlueBattleStageData
{
    SupportLv = 3,
    BattleStageId = battleStageId,
    NpcData = new Ac15BlueBattleNpcData
    {
        NpcId = npcId,
        AcquiredExp = "77",
        TotalExp = "888",
        Dpn = dpn,
        NpcCostumeId = 30,
        SpecialId1 = specialId1,
        SpecialId2 = specialId2,
        SpecialId3 = specialId3,
        BondsLv = 6
    },
    KillCnt = 5,
    BossLife = 12345,
    TotalDamage = 54321,
    CriticalCnt = 7,
    SpecialMoveCnt = 2
}
```

For battle release helper, return:

```csharp
private static Ac15BlueBattleReleaseData CreateReleaseData(uint assignNextStageId)
    => new()
    {
        ReleaseInfoIds = [101],
        ReleaseBattleStageIds = [2],
        ReleaseNpcIds = [4],
        ReleaseNpcCostumeIds = [5],
        ReleaseNpcSpecialIds = [6],
        BattleTokenData = [new(TokenId: 17, TokenValue: 765)],
        AssignNextStageId = assignNextStageId
    };
```

- [ ] **Step 4: Run AC15 handler slices**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultHandlerTests|FullyQualifiedName~BlueBattlePlayResultHandlerTests|FullyQualifiedName~YellowPlayResultHandlerTests|FullyQualifiedName~RedPlayResultHandlerTests|FullyQualifiedName~GreenPlayResultHandlerTests"
```

Expected: pass.

### Task 5: Run Special-Mode Regression Set

**Files:**
- No additional file edits.

- [ ] **Step 1: Run Tokkun, battle, and ghost tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Tokkun|FullyQualifiedName~BattlePlayResult|FullyQualifiedName~Ghost"
```

Expected: pass.

- [ ] **Step 2: Commit checkpoint 4**

Run:

```powershell
git add Application/Handlers/UpdatePlayResultCommand.Blue.cs Application/Handlers/UpdatePlayResultCommand.BlueBattle.cs Application/Handlers/UpdatePlayResultCommand.BlueTokkun.cs Application/Handlers/UpdatePlayResultCommand.Green.cs Application/Handlers/UpdatePlayResultCommand.Yellow.cs Application/Handlers/UpdatePlayResultCommand.YellowTokkun.cs Application/Handlers/UpdatePlayResultCommand.Red.cs Application/Common/BlueBattleStateExtensions.cs Tests/Blue/BlueBattlePlayResultHandlerTests.cs Tests/Blue/BluePlayResultHandlerTests.cs Tests/Yellow/YellowPlayResultHandlerTests.cs Tests/Red/RedPlayResultHandlerTests.cs
git commit -m "Migrate AC15 special playresult paths"
```

