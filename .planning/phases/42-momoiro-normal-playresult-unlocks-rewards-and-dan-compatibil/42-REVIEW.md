---
phase: 42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil
reviewed: 2026-06-26T20:34:11Z
depth: standard
files_reviewed: 27
files_reviewed_list:
  - Adapters.GameProtocol.Momoiro/Controllers/PlayResultController.cs
  - Adapters.GameProtocol.Momoiro/Mappers/PlayResultMappers.cs
  - Application/Abstractions/IMomoiroCatalog.cs
  - Application/Abstractions/ITaikoDbContext.Momoiro.cs
  - Application/Ac15/Ac15DaniMapper.cs
  - Application/Ac15/Ac15EraProfiles.cs
  - Application/Ac15/Ac15NormalPlayMapper.cs
  - Application/Ac15/Ac15NormalPlayWriter.cs
  - Application/Ac15/Ac15ProfileCounterUpdater.cs
  - Application/Ac15/Ac15UnlockFlagAccess.cs
  - Application/Handlers/UpdatePlayResultCommand.Momoiro.cs
  - Application/Handlers/UpdatePlayResultCommand.cs
  - Domain/Entities/DanScoreDatumMomoiro.cs
  - Domain/Entities/DanStageScoreDatumMomoiro.cs
  - Domain/Entities/SongPlayDatumMomoiro.cs
  - Infrastructure/GameDataCatalog/Momoiro/MomoiroEraGameDataCatalog.cs
  - Infrastructure/Persistence/Migrations/20260626140044_AddMomoiroPlayResultState.cs
  - Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs
  - Infrastructure/Persistence/TaikoDbContext.Momoiro.cs
  - Tests/Ac15/Ac15DaniCapabilityTests.cs
  - Tests/Ac15/Ac15NormalPlayWriterTests.cs
  - Tests/Ac15/Ac15RecommendQueryHandlerTests.cs
  - Tests/Green/GreenAuthConfigTests.cs
  - Tests/Momoiro/MomoiroHandlerFixture.cs
  - Tests/Momoiro/MomoiroPlayResultControllerTests.cs
  - Tests/Momoiro/MomoiroPlayResultHandlerTests.cs
  - Tests/Momoiro/MomoiroProtocolLimitsTests.cs
findings:
  critical: 1
  warning: 0
  info: 0
  total: 1
status: issues_found
---

# Phase 42: Code Review Report

**Reviewed:** 2026-06-26T20:34:11Z
**Depth:** standard
**Files Reviewed:** 27
**Status:** issues_found

## Summary

Reviewed the Momoiro playresult controller, Mapperly mapper declarations and emitted generated mapper output, AC15 shared writer/Dan helpers touched by the phase, Momoiro-owned EF schema, and focused tests. The schema and mapper surfaces are mostly bounded to Momoiro-owned state, but the application handler does not reject unsupported play modes before applying normal-play mutations.

I did not rerun the test suite during this review; findings are from source and generated-source inspection.

## Narrative Findings (AI reviewer)

## Critical Issues

### CR-01: Unsupported Momoiro Play Modes Mutate Normal State

**Severity:** BLOCKER
**File:** `Application/Handlers/UpdatePlayResultCommand.Momoiro.cs:59`

**Issue:** Momoiro only supports normal play and bounded Dan compatibility in this phase, with Tokkun, battle, Gaiden, AI battle, and other unsupported feature state explicitly out of scope. The handler only uses `PlayMode.DanMode` to decide whether to invoke the Dan writer, then always continues into profile mutation and `Ac15NormalPlayWriter.SaveAsync` for any valid staged payload. A request with `PlayMode.Tokkun`, `PlayMode.GaidenMode`, `PlayMode.AiBattle`, or another unsupported numeric mode can therefore write `UserSaveData_Momoiro`, `SongPlayDatum_Momoiro`, best rows, favorites, recents, release flags, and Don Point/reward fields as if it were normal play.

That violates MORUN-05 and the phase constraint that unsupported Momoiro feature families must not create route/state authority. The existing tests cover normal, Dan, and challenge-shaped arrays, but there is no Momoiro regression for `PlayMode.Tokkun` or other unsupported modes.

**Fix:**

Add an explicit Momoiro play-mode gate before filtering stages or applying any save mutation, and add a regression test that a valid staged `PlayMode.Tokkun` request returns success without creating Momoiro normal/Dan rows or changing save counters/flags.

```csharp
var playMode = (PlayMode)playResultData.Metadata.PlayMode;
if (playMode is not PlayMode.Normal and not PlayMode.DanMode)
{
    logger.LogWarning(
        "Skipping unsupported Momoiro playresult mode {PlayMode} for baid {Baid}",
        playResultData.Metadata.PlayMode,
        request.Baid);
    return 1;
}
```

Place this before `Ac15NormalStageFilter.Filter(...)` and before `Ac15CommonProfileMutation.TryApplyDonPoints(...)`, so unsupported modes cannot mutate release flags, Don Point/reward fields, profile counters, favorites, recents, best rows, or play-history rows.

---

_Reviewed: 2026-06-26T20:34:11Z_
_Reviewer: the agent (gsd-code-reviewer)_
_Depth: standard_
