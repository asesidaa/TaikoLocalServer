# AC15 Playresult Adapter Mapper Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make Blue, Green, Yellow, and Red playresult mappers target `Ac15PlayResultEnvelope` instead of `CommonPlayResultData`, while controllers send `UpdateAc15PlayResultCommand`.

**Architecture:** Each adapter mapper uses Mapperly for mechanical nested projections and small null-capability assembly helpers for `Tokkun`, `BlueBattle`, `GreenGhost`, and `ChallengeCompe`. Capability presence comes from wire facts; handler methods still own branch classification from play mode and capability presence.

**Tech Stack:** Mapperly strict target mapping, ASP.NET Core controllers, protobuf-net wire DTOs, xUnit mapper tests.

---

## Files

- Modify: `Adapters.GameProtocol.Blue/GlobalUsings.cs`
- Modify: `Adapters.GameProtocol.Green/GlobalUsings.cs`
- Modify: `Adapters.GameProtocol.Yellow/GlobalUsings.cs`
- Modify: `Adapters.GameProtocol.Red/GlobalUsings.cs`
- Modify: `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs`
- Modify: `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`
- Modify: `Adapters.GameProtocol.Yellow/Mappers/PlayResultMappers.cs`
- Modify: `Adapters.GameProtocol.Red/Mappers/PlayResultMappers.cs`
- Modify: `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs`
- Modify: `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`
- Modify: `Adapters.GameProtocol.Yellow/Controllers/PlayResultController.cs`
- Modify: `Adapters.GameProtocol.Red/Controllers/PlayResultController.cs`
- Modify: `Tests/Blue/BluePlayResultMapperTests.cs`
- Modify: `Tests/Green/GreenPlayResultMapperTests.cs`
- Modify: `Tests/Yellow/YellowPlayResultHandlerTests.cs`
- Modify: `Tests/Red/RedProtocolMapperTests.cs`

### Task 1: Add AC15 DTO Imports to Adapter Projects

**Files:**
- Modify: `Adapters.GameProtocol.Blue/GlobalUsings.cs`
- Modify: `Adapters.GameProtocol.Green/GlobalUsings.cs`
- Modify: `Adapters.GameProtocol.Yellow/GlobalUsings.cs`
- Modify: `Adapters.GameProtocol.Red/GlobalUsings.cs`

- [ ] **Step 1: Add the global using**

Add this line to each listed adapter `GlobalUsings.cs`:

```csharp
global using TaikoLocalServer.Application.Dtos.Ac15;
```

- [ ] **Step 2: Build the adapter compile target**

Run:

```powershell
dotnet build TaikoLocalServer.slnx
```

Expected: exit `0`.

### Task 2: Rewrite Blue Mapper Target

**Files:**
- Modify: `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs`
- Modify: `Tests/Blue/BluePlayResultMapperTests.cs`

- [ ] **Step 1: Change Blue mapper tests to assert envelope facts**

In `Tests/Blue/BluePlayResultMapperTests.cs`, update the normal mapping assertion body to this pattern:

```csharp
var envelope = PlayResultMappers.Map(request);

Assert.Equal(1u, envelope.Metadata.Baid);
Assert.Equal("20260528120000", envelope.Metadata.PlayDatetime);
Assert.Equal([104u], envelope.Profile.ReleaseSongNoes);
Assert.Equal([4u], envelope.Profile.GetToneNoes);
Assert.Equal([1u], envelope.Profile.GetCostumeNo1s);
Assert.Equal([2u], envelope.Profile.GetCostumeNo2s);
Assert.Equal([3u], envelope.Profile.GetCostumeNo3s);
Assert.Equal([4u], envelope.Profile.GetCostumeNo4s);
Assert.Equal([5u], envelope.Profile.GetCostumeNo5s);
Assert.Equal([10u], envelope.Profile.GetTitleNoes);
Assert.Equal(1u, envelope.Profile.ItemshopTutorialFlg);
Assert.True(envelope.Profile.IsDevil);
Assert.True(envelope.Profile.IsExplain);
Assert.True(envelope.Profile.HasDifficultyPlayedCourse);
Assert.True(envelope.Profile.HasDifficultyPlayedStar);
Assert.True(envelope.Profile.HasAryCurrentCostume);
Assert.Equal(11u, envelope.Profile.AryCurrentCostume.Costume1);
var stage = Assert.Single(envelope.Normal!.Stages);
Assert.Equal(101u, stage.SongNo);
Assert.Equal(0u, stage.StageMode);
Assert.True(stage.IsPushed);
```

Replace Tokkun mapper classification assertions with presence assertions:

```csharp
var envelope = PlayResultMappers.Map(request);

Assert.Equal(TokkunPlayMode, envelope.Metadata.PlayMode);
Assert.NotNull(envelope.Tokkun);
Assert.Equal(1u, envelope.Tokkun!.TutorialFlg);
Assert.NotNull(envelope.Tokkun.StageData);
Assert.Equal("20260528120000", envelope.Tokkun.StageData!.BanacoinDatetime);
Assert.Equal([101u, 102u, 103u], envelope.Tokkun.StageData.TookunSongnoes);
```

For play-mode-only Tokkun mapper coverage, assert metadata only:

```csharp
var envelope = PlayResultMappers.Map(request);

Assert.Equal(TokkunPlayMode, envelope.Metadata.PlayMode);
Assert.Null(envelope.Tokkun);
```

- [ ] **Step 2: Run Blue mapper tests to verify they fail**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultMapperTests"
```

Expected: fail because `PlayResultMappers.Map` still returns `CommonPlayResultData`.

- [ ] **Step 3: Replace Blue mapper top-level target**

In `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs`, replace the public map method and supporting target-specific mappings with this structure, preserving existing response mapping:

```csharp
[Mapper]
public static partial class PlayResultMappers
{
    public static Ac15PlayResultEnvelope Map(PlayResultRequest request)
    {
        var stages = request.AryStageInfoes.Select(MapStage).ToList();
        return new Ac15PlayResultEnvelope(
            Metadata: MapMetadata(request),
            Profile: MapProfile(request),
            Normal: stages.Count == 0 ? null : new Ac15NormalPlayResult(stages),
            Dani: new Ac15DaniPlayResult(request.DanResult.GetValueOrDefault(), ComboCntTotal: 0, stages),
            Tokkun: MapTokkun(request),
            BlueBattle: MapBlueBattle(request, stages),
            GreenGhost: null,
            ChallengeCompe: MapChallengeCompe(stages));
    }

    public static PlayResultResponse Map(uint result)
        => new() { Result = result };

    private static Ac15PlayResultMetadata MapMetadata(PlayResultRequest request)
        => new(
            request.Baid,
            request.ChassisId ?? string.Empty,
            request.ShopId ?? string.Empty,
            request.PlayDatetime ?? string.Empty,
            request.IsRight,
            request.CardType,
            request.IsTwoPlayers,
            request.PlayMode,
            request.AreaCode,
            MapBytes(request.Reserved),
            request.Accesstoken ?? string.Empty,
            MapBytes(request.ContentInfo));

    private static Ac15ProfileMutationFacts MapProfile(PlayResultRequest request)
        => Ac15ProfileMutationFacts.Empty with
        {
            AreaCode = request.AreaCode,
            GetDonmedal = request.GetDonmedal,
            GetKatsumedal = request.GetKatsumedal,
            ItemshopTutorialFlg = request.ItemshopTutorialFlg,
            WaiwaiTutorialFlg = request.WaiwaiTutorialFlg,
            IsDevil = request.IsDevil,
            IsExplain = request.IsExplain,
            HasDifficultyPlayedCourse = request.DifficultyPlayedCourse is not null,
            DifficultyPlayedCourse = request.DifficultyPlayedCourse.GetValueOrDefault(),
            HasDifficultyPlayedStar = request.DifficultyPlayedStar is not null,
            DifficultyPlayedStar = request.DifficultyPlayedStar.GetValueOrDefault(),
            HasAryCurrentCostume = request.AryCurrentCostume is not null,
            AryCurrentCostume = MapCostume(request.AryCurrentCostume),
            ReleaseSongNoes = MapUIntList(request.ReleaseSongNoes),
            GetToneNoes = MapUIntList(request.GetToneNoes),
            GetCostumeNo1s = MapUIntList(request.GetCostumeNo1s),
            GetCostumeNo2s = MapUIntList(request.GetCostumeNo2s),
            GetCostumeNo3s = MapUIntList(request.GetCostumeNo3s),
            GetCostumeNo4s = MapUIntList(request.GetCostumeNo4s),
            GetCostumeNo5s = MapUIntList(request.GetCostumeNo5s),
            GetTitleNoes = MapUIntList(request.GetTitleNoes)
        };

    [MapProperty(nameof(PlayResultRequest.StageData.OptionFlg), nameof(Ac15StageResult.OptionFlg), Use = nameof(MapBytes))]
    [MapProperty(nameof(PlayResultRequest.StageData.ToneFlg), nameof(Ac15StageResult.ToneFlg), Use = nameof(MapBytes))]
    [MapProperty(nameof(PlayResultRequest.StageData.PlayDan), nameof(Ac15StageResult.PlayDan), Use = nameof(MapPlayDan))]
    [MapProperty(nameof(PlayResultRequest.StageData.AryBattlestagedata), nameof(Ac15StageResult.BlueBattleStage), Use = nameof(MapBattleStageData))]
    [MapProperty(nameof(PlayResultRequest.StageData.AryChallengeIds), nameof(Ac15StageResult.ChallengeIds))]
    [MapProperty(nameof(PlayResultRequest.StageData.AryUserCompeIds), nameof(Ac15StageResult.UserCompeIds))]
    [MapProperty(nameof(PlayResultRequest.StageData.AryBngCompeIds), nameof(Ac15StageResult.BngCompeIds))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.GreenGhostStage))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.AiSectionData))]
    private static partial Ac15StageResult MapStage(PlayResultRequest.StageData stage);

    private static partial Ac15CostumeFacts MapCostumeData(PlayResultRequest.CostumeData costume);

    [MapProperty(nameof(PlayResultRequest.TokkunstageData.BanacoinDatetime), nameof(Ac15TokkunStageData.BanacoinDatetime), Use = nameof(MapString))]
    private static partial Ac15TokkunStageData MapTokkunStageDataCore(PlayResultRequest.TokkunstageData data);

    [MapProperty(nameof(PlayResultRequest.StageData.BattleStageData.NpcData), nameof(Ac15BlueBattleStageData.NpcData), Use = nameof(MapBattleNpcData))]
    private static partial Ac15BlueBattleStageData MapBattleStageDataCore(PlayResultRequest.StageData.BattleStageData data);

    [MapProperty(nameof(PlayResultRequest.ReleaseBattleData.AryBattletokendatas), nameof(Ac15BlueBattleReleaseData.BattleTokenData))]
    private static partial Ac15BlueBattleReleaseData MapBattleReleaseDataCore(PlayResultRequest.ReleaseBattleData data);

    private static partial Ac15BlueBattleTokenData MapBattleTokenData(PlayResultRequest.ReleaseBattleData.BattleTokenData data);
    private static partial Ac15BlueBattleNpcData MapBattleNpcDataCore(PlayResultRequest.StageData.BattleStageData.BattleNpcData data);
    private static partial Ac15CompeIdFact MapCompe(PlayResultRequest.StageData.ResultcompeData data);

    private static Ac15CostumeFacts MapCostume(PlayResultRequest.CostumeData costume)
        => costume is null ? Ac15CostumeFacts.Empty : MapCostumeData(costume);

    private static Ac15TokkunPlayResult? MapTokkun(PlayResultRequest request)
        => request.TokkunTutorialFlg is null && request.AryTokkunstageInfo is null
            ? null
            : new Ac15TokkunPlayResult(request.TokkunTutorialFlg, MapTokkunStageData(request.AryTokkunstageInfo));

    private static Ac15BlueBattlePlayResult? MapBlueBattle(PlayResultRequest request, List<Ac15StageResult> stages)
        => request.AryReleaseBattledata is null && stages.All(stage => stage.BlueBattleStage is null)
            ? null
            : new Ac15BlueBattlePlayResult(MapBattleReleaseData(request.AryReleaseBattledata), stages, request.GetDonmedal);

    private static Ac15RedChallengeCompeFacts? MapChallengeCompe(List<Ac15StageResult> stages)
    {
        var facts = stages
            .Where(stage => stage.ChallengeIds.Count != 0 || stage.UserCompeIds.Count != 0 || stage.BngCompeIds.Count != 0)
            .Select(stage => new Ac15RedChallengeCompeStageFacts(stage.SongNo, stage.ChallengeIds, stage.UserCompeIds, stage.BngCompeIds))
            .ToList();
        return facts.Count == 0 ? null : new Ac15RedChallengeCompeFacts(facts);
    }

    private static Ac15TokkunStageData? MapTokkunStageData(PlayResultRequest.TokkunstageData data)
        => data is null ? null : MapTokkunStageDataCore(data);

    private static Ac15BlueBattleStageData? MapBattleStageData(PlayResultRequest.StageData.BattleStageData data)
        => data is null ? null : MapBattleStageDataCore(data);

    private static Ac15BlueBattleNpcData? MapBattleNpcData(PlayResultRequest.StageData.BattleStageData.BattleNpcData data)
        => data is null ? null : MapBattleNpcDataCore(data);

    private static Ac15BlueBattleReleaseData? MapBattleReleaseData(PlayResultRequest.ReleaseBattleData data)
        => data is null ? null : MapBattleReleaseDataCore(data);

    [UserMapping(Default = true)]
    private static List<uint> MapUIntList(uint[] values)
        => values is null ? [] : values.ToList();

    [UserMapping(Default = true)]
    private static uint MapNullableUInt(uint? value)
        => value.GetValueOrDefault();

    private static byte[] MapBytes(byte[] values)
        => values is null ? [] : values;

    private static string MapString(string value)
        => value ?? string.Empty;

    private static uint? MapPlayDan(uint? value)
        => value is > 0 ? value : null;
}
```

- [ ] **Step 4: Run Blue mapper tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultMapperTests"
```

Expected: pass.

### Task 3: Rewrite Green Mapper Target

**Files:**
- Modify: `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`
- Modify: `Tests/Green/GreenPlayResultMapperTests.cs`

- [ ] **Step 1: Update Green mapper assertions to envelope fields**

Replace `common.AryStageInfoes` assertions with `envelope.Normal!.Stages`:

```csharp
var envelope = PlayResultMappers.Map(request);

var stage = Assert.Single(envelope.Normal!.Stages);
Assert.Equal(1u, stage.StageMode);
Assert.True(stage.IsPapamama);
```

For PlayDan tests:

```csharp
var envelope = PlayResultMappers.Map(request);

Assert.Null(Assert.Single(envelope.Normal!.Stages).PlayDan);
```

and:

```csharp
var envelope = PlayResultMappers.Map(request);

Assert.Equal(7u, Assert.Single(envelope.Normal!.Stages).PlayDan);
```

- [ ] **Step 2: Replace Green mapper target**

Use this Green top-level structure:

```csharp
public static Ac15PlayResultEnvelope Map(PlayResultDataRequest request)
{
    var stages = request.AryStageInfoes.Select(MapStage).ToList();
    return new Ac15PlayResultEnvelope(
        Metadata: MapMetadata(request),
        Profile: MapProfile(request),
        Normal: stages.Count == 0 ? null : new Ac15NormalPlayResult(stages),
        Dani: new Ac15DaniPlayResult(request.DanResult.GetValueOrDefault(), ComboCntTotal: 0, stages),
        Tokkun: null,
        BlueBattle: null,
        GreenGhost: MapGreenGhost(request),
        ChallengeCompe: MapChallengeCompe(stages));
}
```

Map Green ghost-specific wire members to AC15 ghost records:

```csharp
[MapProperty(nameof(PlayResultDataRequest.GhostReleaseData), nameof(Ac15GreenGhostPlayResult.ReleaseData), Use = nameof(MapGhostRelease))]
[MapProperty(nameof(PlayResultDataRequest.GhostUpdatePerfdata), nameof(Ac15GreenGhostPlayResult.PerfData), Use = nameof(MapGhostUpdatePerfData))]
[MapProperty(nameof(PlayResultDataRequest.GhostUpdateRank), nameof(Ac15GreenGhostPlayResult.RankData), Use = nameof(MapGhostUpdateRank))]
private static partial Ac15GreenGhostPlayResult MapGreenGhostCore(PlayResultDataRequest request);

private static Ac15GreenGhostPlayResult? MapGreenGhost(PlayResultDataRequest request)
    => request.GhostReleaseData is null && request.GhostUpdatePerfdata is null && request.GhostUpdateRank is null
        ? null
        : MapGreenGhostCore(request);
```

For stage mapping, map `GhostStagedata` to `Ac15StageResult.GreenGhostStage` and ignore `BlueBattleStage`.

- [ ] **Step 3: Run Green mapper tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayResultMapperTests"
```

Expected: pass.

### Task 4: Rewrite Yellow and Red Mapper Targets

**Files:**
- Modify: `Adapters.GameProtocol.Yellow/Mappers/PlayResultMappers.cs`
- Modify: `Adapters.GameProtocol.Red/Mappers/PlayResultMappers.cs`
- Modify: `Tests/Yellow/YellowPlayResultHandlerTests.cs`
- Modify: `Tests/Red/RedProtocolMapperTests.cs`

- [ ] **Step 1: Update Yellow mapper tests to envelope fields**

In `YellowPlayResultHandlerTests`, update mapper-only tests to assert:

```csharp
var envelope = PlayResultMappers.Map(request);

Assert.Equal(1u, envelope.Metadata.Baid);
Assert.Equal("268410000000", envelope.Metadata.ChassisId);
Assert.Equal("JPN0JPN0123", envelope.Metadata.ShopId);
Assert.Equal("20260608120000", envelope.Metadata.PlayDatetime);
Assert.Equal([104u], envelope.Profile.ReleaseSongNoes);
Assert.Equal(10u, envelope.Profile.GetDonmedal);
Assert.Equal(2u, envelope.Profile.GetKatsumedal);
Assert.True(envelope.Profile.HasAryCurrentCostume);
var stage = Assert.Single(envelope.Normal!.Stages);
Assert.Equal(101u, stage.SongNo);
Assert.Null(stage.PlayDan);
```

For play-mode-only Tokkun mapper coverage:

```csharp
var envelope = PlayResultMappers.Map(request);

Assert.Equal((uint)PlayMode.Tokkun, envelope.Metadata.PlayMode);
Assert.Null(envelope.Tokkun);
```

For Tokkun stage facts:

```csharp
var envelope = PlayResultMappers.Map(request);

Assert.Equal((uint)PlayMode.Normal, envelope.Metadata.PlayMode);
Assert.NotNull(envelope.Tokkun);
Assert.Equal(9u, envelope.Tokkun!.TutorialFlg);
Assert.Equal([101u, 102u, 101u], envelope.Tokkun.StageData!.TookunSongnoes);
```

- [ ] **Step 2: Update Red mapper test to envelope fields**

In `RedProtocolMapperTests.PlayResultMapper_Red_MapsDonPointTokkunAndChallengeFacts`, assert:

```csharp
var envelope = PlayResultMappers.Map(request);

Assert.Equal(25u, envelope.Profile.GetDonpoint);
Assert.Equal(4u, envelope.Profile.RewardPtn);
Assert.Equal(9u, envelope.Profile.RewardProgress);
Assert.Equal(2u, envelope.Profile.DifficultyTutorialFlg);
Assert.Equal(3u, envelope.Profile.DifficultyPlayedCourse);
Assert.Equal(8u, envelope.Profile.DifficultyPlayedStar);
Assert.NotNull(envelope.Tokkun);
Assert.Equal(7u, envelope.Tokkun!.TutorialFlg);
Assert.Equal([101u, 102u, 101u], envelope.Tokkun.StageData!.TookunSongnoes);
var stage = Assert.Single(envelope.Normal!.Stages);
Assert.Equal(42u, Assert.Single(stage.ChallengeIds).CompeId);
Assert.Equal(1u, stage.PlayDan);
Assert.Equal(80u, stage.SoulGauge);
Assert.NotNull(envelope.ChallengeCompe);
```

- [ ] **Step 3: Replace Yellow and Red mapper targets**

Use this Yellow top-level structure:

```csharp
public static Ac15PlayResultEnvelope Map(PlayResultRequest request)
{
    var stages = request.AryStageInfoes.Select(MapStage).ToList();
    return new Ac15PlayResultEnvelope(
        Metadata: MapMetadata(request),
        Profile: MapProfile(request),
        Normal: stages.Count == 0 ? null : new Ac15NormalPlayResult(stages),
        Dani: new Ac15DaniPlayResult(request.DanResult.GetValueOrDefault(), ComboCntTotal: 0, stages),
        Tokkun: MapTokkun(request),
        BlueBattle: null,
        GreenGhost: null,
        ChallengeCompe: MapChallengeCompe(stages));
}
```

Use this Red top-level structure:

```csharp
public static Ac15PlayResultEnvelope Map(PlayResultRequest request)
{
    var stages = request.AryStageInfoes.Select(MapStage).ToList();
    return new Ac15PlayResultEnvelope(
        Metadata: MapMetadata(request),
        Profile: MapProfile(request),
        Normal: stages.Count == 0 ? null : new Ac15NormalPlayResult(stages),
        Dani: new Ac15DaniPlayResult(request.DanResult.GetValueOrDefault(), ComboCntTotal: 0, stages),
        Tokkun: MapTokkun(request),
        BlueBattle: null,
        GreenGhost: null,
        ChallengeCompe: MapChallengeCompe(stages));
}
```

Red `MapProfile` must populate these Red-owned facts:

```csharp
GetDonpoint = request.GetDonpoint,
RewardPtn = request.RewardPtn,
RewardProgress = request.RewardProgress,
DifficultyTutorialFlg = request.DifficultyTutorialFlg,
```

Yellow `MapProfile` must populate Don/Katsu medal facts and Yellow tutorial facts:

```csharp
GetDonmedal = request.GetDonmedal,
GetKatsumedal = request.GetKatsumedal,
ItemshopTutorialFlg = request.ItemshopTutorialFlg,
WaiwaiTutorialFlg = request.WaiwaiTutorialFlg,
```

Both Yellow and Red mappers should set these capability slots in the top-level constructor:

```csharp
Dani: new Ac15DaniPlayResult(request.DanResult.GetValueOrDefault(), ComboCntTotal: 0, stages),
Tokkun: MapTokkun(request),
BlueBattle: null,
GreenGhost: null,
ChallengeCompe: MapChallengeCompe(stages)
```

- [ ] **Step 4: Run Yellow and Red mapper tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~PlayResultMapper_Yellow|FullyQualifiedName~PlayResultMapper_Red"
```

Expected: pass for Yellow and Red mapper-specific tests.

### Task 5: Update Controllers to Send AC15 Command

**Files:**
- Modify: `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs`
- Modify: `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`
- Modify: `Adapters.GameProtocol.Yellow/Controllers/PlayResultController.cs`
- Modify: `Adapters.GameProtocol.Red/Controllers/PlayResultController.cs`

- [ ] **Step 1: Replace command construction**

In Blue, Yellow, and Red controllers, replace:

```csharp
new UpdatePlayResultCommand(request.Baid, GameEra.Blue, common)
```

with the era-specific AC15 command:

```csharp
new UpdateAc15PlayResultCommand(request.Baid, GameEra.Blue, common)
```

Use `GameEra.Yellow` and `GameEra.Red` in the Yellow and Red controllers.

In Green controller, replace:

```csharp
new UpdatePlayResultCommand(request.BaidConf, GameEra.Green, commonRequest)
```

with:

```csharp
new UpdateAc15PlayResultCommand(request.BaidConf, GameEra.Green, commonRequest)
```

- [ ] **Step 2: Update Green log variable naming only where needed**

If `mapped_common` now has the wrong meaning, rename the message property to `mapped_ac15`:

```csharp
Logger.LogInformation(
    "Green PlayResult received dump: wire={@Request} mapped_ac15={@Ac15}",
    decoded.Request,
    commonRequest);
```

- [ ] **Step 3: Run controller compile**

Run:

```powershell
dotnet build TaikoLocalServer.slnx
```

Expected: exit `0`. Mapperly `RMG012` warnings from AC15 playresult mapper targets should be reduced or gone for files already migrated in this checkpoint.

- [ ] **Step 4: Commit checkpoint 2**

Run:

```powershell
git add Adapters.GameProtocol.Blue/GlobalUsings.cs Adapters.GameProtocol.Green/GlobalUsings.cs Adapters.GameProtocol.Yellow/GlobalUsings.cs Adapters.GameProtocol.Red/GlobalUsings.cs Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs Adapters.GameProtocol.Yellow/Mappers/PlayResultMappers.cs Adapters.GameProtocol.Red/Mappers/PlayResultMappers.cs Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs Adapters.GameProtocol.Green/Controllers/PlayResultController.cs Adapters.GameProtocol.Yellow/Controllers/PlayResultController.cs Adapters.GameProtocol.Red/Controllers/PlayResultController.cs Tests/Blue/BluePlayResultMapperTests.cs Tests/Green/GreenPlayResultMapperTests.cs Tests/Yellow/YellowPlayResultHandlerTests.cs Tests/Red/RedProtocolMapperTests.cs
git commit -m "Map AC15 playresults to capability input"
```
