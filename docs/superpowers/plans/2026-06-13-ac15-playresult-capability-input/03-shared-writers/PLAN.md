# AC15 Shared Writer Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Migrate shared AC15 normal/profile/Dani writers from `CommonPlayResultData` to AC15 capability records.

**Architecture:** Shared writers receive `Ac15StageResult`, `Ac15ProfileMutationFacts`, and `Ac15DaniPlayResult` directly. Era handlers still bind concrete tables, policies, limits, and save-row delegates at the edge.

**Tech Stack:** C# records, EF Core DbSet bindings, Mapperly entity delegates, xUnit SQLite behavior tests.

---

## Files

- Modify: `Application/Ac15/Ac15NormalStageFilter.cs`
- Modify: `Application/Ac15/Ac15NormalStagePolicies.cs`
- Modify: `Application/Ac15/Ac15ProfileCounterUpdater.cs`
- Modify: `Application/Ac15/Ac15CommonProfileMutation.cs`
- Modify: `Application/Ac15/Ac15NormalPlayRecords.cs`
- Modify: `Application/Ac15/Ac15NormalPlayWriter.cs`
- Modify: `Application/Ac15/Ac15DaniWriter.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Blue.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Red.cs`
- Modify: `Tests/Ac15/Ac15NormalPlayWriterTests.cs`
- Modify: `Tests/Ac15/Ac15CommonProfileMutationTests.cs`
- Modify: `Tests/Ac15/Ac15DaniCapabilityTests.cs`

### Task 1: Migrate Stage Filtering and Policies

**Files:**
- Modify: `Application/Ac15/Ac15NormalStageFilter.cs`
- Modify: `Application/Ac15/Ac15NormalStagePolicies.cs`

- [ ] **Step 1: Change policy record signature**

Replace `Ac15NormalStagePolicy` in `Application/Ac15/Ac15NormalStageFilter.cs` with:

```csharp
public sealed record Ac15NormalStagePolicy(
    Func<Ac15StageResult, Ac15StageSupportDecision> IsSupported,
    Func<Ac15StageResult, CrownType, Ac15BestUpdatePolicy> GetBestUpdatePolicy);
```

Add this using:

```csharp
using TaikoLocalServer.Application.Dtos.Ac15;
```

- [ ] **Step 2: Change filter input and output**

Replace the `Filter` signature with:

```csharp
public static IReadOnlyList<Ac15StageResult> Filter(
    uint baid,
    IEnumerable<Ac15StageResult> stages,
    Ac15ProtocolLimits limits,
    Ac15NormalStagePolicy policy,
    ILogger logger)
```

Keep the method body unchanged except for the stage type.

- [ ] **Step 3: Update policy file imports**

In `Application/Ac15/Ac15NormalStagePolicies.cs`, add:

```csharp
using TaikoLocalServer.Application.Dtos.Ac15;
```

No policy body changes are needed because `Ac15StageResult` preserves `StageMode`, `Level`, and `SupportLevel`.

- [ ] **Step 4: Run focused Ac15 tests to verify compile failures moved to remaining writer files**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15"
```

Expected: fail with references to `CommonPlayResultData.StageData` in profile, normal writer, Dani writer, and tests.

### Task 2: Migrate Profile Counter and Mutation

**Files:**
- Modify: `Application/Ac15/Ac15ProfileCounterUpdater.cs`
- Modify: `Application/Ac15/Ac15CommonProfileMutation.cs`
- Modify: `Tests/Ac15/Ac15CommonProfileMutationTests.cs`

- [ ] **Step 1: Change profile counter updater stage type**

In `Application/Ac15/Ac15ProfileCounterUpdater.cs`, add:

```csharp
using TaikoLocalServer.Application.Dtos.Ac15;
```

Replace all four era helper signatures and the generic helper signature with `Ac15StageResult`:

```csharp
public static void ApplyBlueStage(UserSaveDataBlue saveData, Ac15StageResult stage)
    => ApplyStage(saveData, stage, Blue);

public static void ApplyGreenStage(UserSaveDataGreen saveData, Ac15StageResult stage)
    => ApplyStage(saveData, stage, Green);

public static void ApplyYellowStage(UserSaveDataYellow saveData, Ac15StageResult stage)
    => ApplyStage(saveData, stage, Yellow);

public static void ApplyRedStage(UserSaveDataRed saveData, Ac15StageResult stage)
    => ApplyStage(saveData, stage, Red);

public static void ApplyStage<TSave>(
    TSave saveData,
    Ac15StageResult stage,
    Ac15ProfileCounterAccess<TSave> counters)
```

- [ ] **Step 2: Change profile mutation signatures**

In `Application/Ac15/Ac15CommonProfileMutation.cs`, add:

```csharp
using TaikoLocalServer.Application.Dtos.Ac15;
```

Change `TryApply` to:

```csharp
public static bool TryApply<TSave>(
    TSave saveData,
    IAc15ShopSeasonState? shopSeasonState,
    Ac15ProfileMutationFacts profile,
    IReadOnlyList<Ac15StageResult> countedStages,
    Ac15ProfileCounterAccess<TSave> counterAccess,
    Ac15UnlockFlagAccess<TSave> unlockAccess,
    Ac15ProtocolLimits limits,
    DateTime playTime)
```

Replace `playResultData` references with `profile`, including:

```csharp
var currentDonmedal = shopSeasonState?.TotalGetDonmedal ?? saveData.TotalGetDonmedal;
if (!CanAdd(currentDonmedal, profile.GetDonmedal)
    || !CanAdd(saveData.TotalGetKatsumedal, profile.GetKatsumedal))
{
    return false;
}
```

Change `TryApplyDonPoints` to:

```csharp
public static bool TryApplyDonPoints<TSave>(
    TSave saveData,
    Ac15ProfileMutationFacts profile,
    IReadOnlyList<Ac15StageResult> countedStages,
    Ac15ProfileCounterAccess<TSave> counterAccess,
    Ac15UnlockFlagAccess<TSave> unlockAccess,
    Ac15ProtocolLimits limits,
    DateTime playTime)
```

Change `ApplyShared` to accept `Ac15ProfileMutationFacts profile` and `IReadOnlyList<Ac15StageResult> countedStages`.

- [ ] **Step 3: Replace costume mutation argument**

In `ApplyShared`, replace:

```csharp
Ac15CustomizationMutation.ApplyCurrentCostume(saveData, playResultData.AryCurrentCostume, limits);
```

with:

```csharp
Ac15CustomizationMutation.ApplyCurrentCostume(saveData, ToCommonCostume(profile.AryCurrentCostume), limits);
```

Add this private helper inside `Ac15CommonProfileMutation`:

```csharp
private static CommonPlayResultData.CostumeData ToCommonCostume(Ac15CostumeFacts costume)
    => new()
    {
        Costume1 = costume.Costume1,
        Costume2 = costume.Costume2,
        Costume3 = costume.Costume3,
        Costume4 = costume.Costume4,
        Costume5 = costume.Costume5
    };
```

This helper remains local because `Ac15CustomizationMutation` still accepts the old costume type. Do not expose it.

- [ ] **Step 4: Update profile mutation tests**

In `Tests/Ac15/Ac15CommonProfileMutationTests.cs`, replace helper return types:

```csharp
private static Ac15ProfileMutationFacts PlayResult(uint getDonmedal, uint getKatsumedal)
    => Ac15ProfileMutationFacts.Empty with
    {
        GetDonmedal = getDonmedal,
        GetKatsumedal = getKatsumedal,
        AreaCode = 10,
        IsDevil = true,
        IsExplain = true,
        WaiwaiTutorialFlg = 1,
        HasDifficultyPlayedCourse = true,
        DifficultyPlayedCourse = 3,
        HasDifficultyPlayedStar = true,
        DifficultyPlayedStar = 4
    };

private static Ac15StageResult Stage(uint songNo, bool isFavorite = false)
    => new()
    {
        SongNo = songNo,
        Level = 1,
        StageMode = 0,
        MusicCateg = 2,
        IsFavorite = isFavorite,
        IsRecent = true
    };
```

For `GreenUnlockPolicyLeavesSongReleaseFlagsAbsent`, replace the inline `CommonPlayResultData` with:

```csharp
Ac15ProfileMutationFacts.Empty with
{
    ReleaseSongNoes = [101],
    GetToneNoes = [5]
}
```

- [ ] **Step 5: Run profile mutation tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15CommonProfileMutationTests"
```

Expected: pass.

### Task 3: Migrate Normal Play Writer

**Files:**
- Modify: `Application/Ac15/Ac15NormalPlayRecords.cs`
- Modify: `Application/Ac15/Ac15NormalPlayWriter.cs`
- Modify: `Tests/Ac15/Ac15NormalPlayWriterTests.cs`

- [ ] **Step 1: Update normal play write request**

In `Application/Ac15/Ac15NormalPlayWriter.cs`, add:

```csharp
using TaikoLocalServer.Application.Dtos.Ac15;
```

Change `Ac15NormalPlayWriteRequest` to:

```csharp
public sealed record Ac15NormalPlayWriteRequest(
    uint Baid,
    uint PlayMode,
    IReadOnlyList<Ac15StageResult> Stages,
    Ac15ProtocolLimits Limits,
    DateTime PlayTime);
```

- [ ] **Step 2: Update `ToPlayRow` input**

Change:

```csharp
CommonPlayResultData.StageData stage,
```

to:

```csharp
Ac15StageResult stage,
```

Replace the ghost argument at the end of `new Ac15PlayRow(...)` with:

```csharp
stage.GreenGhostStage,
```

- [ ] **Step 3: Update play row ghost type**

In `Application/Ac15/Ac15NormalPlayRecords.cs`, add:

```csharp
using TaikoLocalServer.Application.Dtos.Ac15;
```

Replace:

```csharp
CommonPlayResultData.GhostStageData? GhostStageData,
```

with:

```csharp
Ac15GreenGhostStageData? GhostStageData,
```

- [ ] **Step 4: Update normal writer tests**

In `Tests/Ac15/Ac15NormalPlayWriterTests.cs`, replace helper types:

```csharp
private static Ac15StageResult Stage(
    uint songNo,
    bool isFavorite = false,
    bool isRecent = false,
    Ac15GreenGhostStageData? ghostStageData = null)
    => new()
    {
        SongNo = songNo,
        Level = 1,
        StageMode = 0,
        PlayResult = 2,
        PlayScore = 123456,
        ScoreRate = 87,
        IsFavorite = isFavorite,
        IsRecent = isRecent,
        GreenGhostStage = ghostStageData
    };
```

Replace test ghost construction with:

```csharp
ghostStageData: new Ac15GreenGhostStageData
{
    ArySectionData =
    [
        new(IsWin: true, GoodCnt: 10, OkCnt: 2, NgCnt: 1, PoundCnt: 4)
    ]
}
```

In the `AfterAddPlayRow` lambda, replace `section.GoodCnt`, `section.OkCnt`, `section.NgCnt`, and `section.PoundCnt` exactly as before because the new record uses the same property names.

- [ ] **Step 5: Run normal writer tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15NormalPlayWriterTests"
```

Expected: pass.

### Task 4: Migrate Dani Writer

**Files:**
- Modify: `Application/Ac15/Ac15DaniWriter.cs`
- Modify: `Tests/Ac15/Ac15DaniCapabilityTests.cs`

- [ ] **Step 1: Change Dani writer input**

In `Application/Ac15/Ac15DaniWriter.cs`, add:

```csharp
using TaikoLocalServer.Application.Dtos.Ac15;
```

Change the `SaveAsync` parameter:

```csharp
Ac15DaniPlayResult? playResultData,
```

At the start of `SaveAsync`, replace the play mode check with:

```csharp
if (playResultData is null)
{
    return;
}
```

The handler remains responsible for calling Dani writer only after normal-stage filtering.

- [ ] **Step 2: Replace stage collection references**

In `Ac15DaniWriter`, replace:

```csharp
playResultData.AryStageInfoes
```

with:

```csharp
playResultData.Stages
```

Replace:

```csharp
playResultData.AryStageInfoes.Count
```

with:

```csharp
playResultData.Stages.Count
```

Change `BuildUpdatedStage` stage parameter from `CommonPlayResultData.StageData` to `Ac15StageResult`.

- [ ] **Step 3: Update Dani test helper**

In `Tests/Ac15/Ac15DaniCapabilityTests.cs`, replace `PlayResultDanClear` with:

```csharp
private static Ac15DaniPlayResult PlayResultDanClear(uint danId)
    => new(
        DanResult: (uint)Ac15DanClearGrade.NormalClear,
        ComboCntTotal: 300,
        Stages:
        [
            new Ac15StageResult
            {
                SongNo = 101,
                Level = 1,
                PlayScore = 1000,
                GoodCnt = 10,
                OkCnt = 2,
                NgCnt = 1,
                PoundCnt = 4,
                HitCnt = 13,
                ComboCnt = 12,
                SoulGauge = 150,
                PlayDan = danId
            }
        ]);
```

- [ ] **Step 4: Run Dani tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15DaniCapabilityTests"
```

Expected: pass.

### Task 5: Update Normal AC15 Handlers to Use Records

**Files:**
- Modify: `Application/Handlers/UpdatePlayResultCommand.Blue.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Red.cs`

- [ ] **Step 1: Remove bridge from normal handler flow**

In each AC15 handler, replace:

```csharp
var playResultData = Ac15PlayResultCommonBridge.ToCommon(request.PlayResultData);
```

with:

```csharp
var playResultData = request.PlayResultData;
var normal = playResultData.Normal;
var stages = normal?.Stages ?? [];
```

- [ ] **Step 2: Update filter calls**

Replace each filter call source:

```csharp
playResultData.AryStageInfoes,
```

with:

```csharp
stages,
```

Delete every mutation of `playResultData.AryStageInfoes`.

- [ ] **Step 3: Update profile mutation calls**

For Blue, Green, and Yellow, replace:

```csharp
playResultData,
validStages,
```

with:

```csharp
playResultData.Profile,
validStages,
```

For Red `TryApplyDonPoints`, replace the same arguments with `playResultData.Profile` and `validStages`.

- [ ] **Step 4: Update play time parsing**

Replace:

```csharp
var playTime = ParseAc15PlayDatetimeOrNow(playResultData.PlayDatetime);
```

with:

```csharp
var playTime = ParseAc15PlayDatetimeOrNow(playResultData.Metadata.PlayDatetime);
```

- [ ] **Step 5: Update Dani writer calls**

For each AC15 handler, replace:

```csharp
playResultData,
```

in `Ac15DaniWriter.SaveAsync(...)` with:

```csharp
playResultData.Metadata.PlayMode == (uint)PlayMode.DanMode
    ? playResultData.Dani with { Stages = validStages.ToList() }
    : null,
```

Use this local variable if preferred:

```csharp
var dani = playResultData.Metadata.PlayMode == (uint)PlayMode.DanMode && playResultData.Dani is { } inputDani
    ? inputDani with { Stages = validStages.ToList() }
    : null;
```

- [ ] **Step 6: Update normal writer request**

Replace:

```csharp
new Ac15NormalPlayWriteRequest(request.Baid, playResultData.PlayMode, validStages, Ac15EraProfiles.Blue.Limits, playTime)
```

with:

```csharp
new Ac15NormalPlayWriteRequest(request.Baid, playResultData.Metadata.PlayMode, validStages, Ac15EraProfiles.Blue.Limits, playTime)
```

Use each era's `Ac15EraProfiles.*.Limits`.

- [ ] **Step 7: Run focused AC15 writer and Red tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15|FullyQualifiedName~RedPlayResultHandlerTests"
```

Expected: pass after tests that still construct commands are updated in checkpoint 4.

- [ ] **Step 8: Commit checkpoint 3**

Run:

```powershell
git add Application/Ac15/Ac15NormalStageFilter.cs Application/Ac15/Ac15NormalStagePolicies.cs Application/Ac15/Ac15ProfileCounterUpdater.cs Application/Ac15/Ac15CommonProfileMutation.cs Application/Ac15/Ac15NormalPlayRecords.cs Application/Ac15/Ac15NormalPlayWriter.cs Application/Ac15/Ac15DaniWriter.cs Application/Handlers/UpdatePlayResultCommand.Blue.cs Application/Handlers/UpdatePlayResultCommand.Green.cs Application/Handlers/UpdatePlayResultCommand.Yellow.cs Application/Handlers/UpdatePlayResultCommand.Red.cs Tests/Ac15/Ac15NormalPlayWriterTests.cs Tests/Ac15/Ac15CommonProfileMutationTests.cs Tests/Ac15/Ac15DaniCapabilityTests.cs
git commit -m "Migrate AC15 shared writers to capability records"
```

