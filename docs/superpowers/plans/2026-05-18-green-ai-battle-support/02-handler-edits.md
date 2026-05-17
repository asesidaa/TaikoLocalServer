# Task 2 — Handler Edits

**Goal:** Edit `Application/Handlers/UpdatePlayResultCommand.Green.cs` so AI Battle plays are accepted, `IsShin` and AI-Battle status are read via the Task 1 helpers, `BestCrown` is gated by the chart-vs-AI rule, and `GhostPlayedSongFlag` is updated. No test file is added in this task — Task 3 covers new behavior. The existing Green test suite must still pass.

**Files:**
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`

**Depends on:** Task 1 must be merged first (this task references `GreenStageModeInterpreter` and `GreenAiBattleLevels`).

**Acceptance Criteria:**
- [ ] `MaxGreenStageMode` constant is removed.
- [ ] `IsValidGreenStage` requires `stage.StageMode is 0 or 1 or 3 or 4`.
- [ ] `SaveStageAsync` computes `isShin = GreenStageModeInterpreter.IsShin(stage.StageMode)`.
- [ ] `SaveStageAsync` computes `allowCrownUpdate` per the rule below and passes it into `UpsertBestAsync`.
- [ ] `UpsertBestAsync` signature gains a `bool allowCrownUpdate` parameter; the new-row insert sets `BestCrown = allowCrownUpdate ? crown : CrownType.None`; the existing-row branch gates the `BestCrown` assignment on `allowCrownUpdate`.
- [ ] A new private static `ApplyGhostPlayedSongBits(UserSaveDataGreen, CommonPlayResultData)` sets bit `stage.SongNo` in `saveData.GhostPlayedSongFlag` for every stage where `GreenStageModeInterpreter.IsAiBattle(stage.StageMode)` is true.
- [ ] `HandleGreen` calls `ApplyGhostPlayedSongBits(saveData, playResultData)` after the per-stage loop and before `await SaveChangesAsync(...)`.
- [ ] `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Green"` exits `0` — no regression in any pre-existing Green test.

**Verify:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Green"` → all pass.

---

## Background

### Crown-gating rule

The spec gates `BestCrown` mutation. Compute at the call site (where `stage` is in scope):

```csharp
var allowCrownUpdate = !GreenStageModeInterpreter.IsAiBattle(stage.StageMode)
    || stage.Level == 5
    || GreenAiBattleLevels.IsCertifiedLevel(stage.GhostStageData?.SdCertifiedLevelId ?? 0);
```

Three independent OR-clauses, in priority order:

1. Not an AI Battle stage → behave like today (always allowed).
2. Player picked the Ura chart (`Level == 5`) — Ura has no intermediate AI difficulties, so any AI Battle Ura play counts.
3. AI difficulty is a 正規 level (`SdCertifiedLevelId ∈ {1, 5, 9, 13}`).

Score / rate updates are NOT gated. Only `BestCrown` is.

### `GhostPlayedSongFlag` rule

The flag is a byte array sized to `GreenProtocolBytes.GhostPlayedSongBytes` (= 128 bytes, 1024 bits). For each stage with `IsAiBattle(stage.StageMode) == true`, set bit `stage.SongNo`. Reuse the existing `SetBits(byte[], IEnumerable<uint>, int)` helper near the bottom of the file. Songs with `SongNo >= GhostPlayedSongBytes * 8` are skipped, matching `SetBits`'s behavior.

### Existing helper signatures (do not change)

```csharp
// existing private instance method on UpdatePlayResultCommandHandler
private async Task UpsertBestAsync(
    uint baid,
    CommonPlayResultData.StageData stage,
    Difficulty difficulty,
    CrownType crown,
    bool isShin,
    CancellationToken cancellationToken)

// existing private static
private static byte[] SetBits(byte[] source, IEnumerable<uint> ids, int byteCount)
```

`UpsertBestAsync` will gain a 7th parameter (`bool allowCrownUpdate`). All other signatures stay.

---

## Steps

- [ ] **Step 1: Open the file and remove the obsolete constant**

In `Application/Handlers/UpdatePlayResultCommand.Green.cs`, delete the `MaxGreenStageMode` constant declaration. The block currently reads:

```csharp
private const uint MinGreenCourseLevel = 1;
private const uint MaxGreenCourseLevel = 5;
private const uint MaxGreenStageMode = 1;
private const uint MaxGreenPlayResult = 3;
```

Change to:

```csharp
private const uint MinGreenCourseLevel = 1;
private const uint MaxGreenCourseLevel = 5;
private const uint MaxGreenPlayResult = 3;
```

- [ ] **Step 2: Loosen `IsValidGreenStage` to accept the AI Battle stage modes**

Replace this line inside `IsValidGreenStage`:

```csharp
            && stage.StageMode <= MaxGreenStageMode
```

with:

```csharp
            && stage.StageMode is 0 or 1 or 3 or 4
```

The full updated method body:

```csharp
private static bool IsValidGreenStage(CommonPlayResultData.StageData stage, IGreenCatalog green)
{
    return stage.SongNo < GreenProtocolBytes.SongFlagBytes * 8
        && green.GreenMusicInfos.ContainsKey(stage.SongNo)
        && stage.Level is >= MinGreenCourseLevel and <= MaxGreenCourseLevel
        && stage.StageMode is 0 or 1 or 3 or 4
        && stage.PlayResult <= MaxGreenPlayResult
        && stage.MusicCateg <= 7
        && (stage.PlayDan is null || GreenDanHelpers.IsKnownGreenDanId(stage.PlayDan.Value));
}
```

- [ ] **Step 3: Route `isShin` through the interpreter and compute `allowCrownUpdate` in `SaveStageAsync`**

Locate `SaveStageAsync`. The first two statements after the parameter list are currently:

```csharp
var difficulty = GreenPlayResultMapping.MapDifficulty(stage.Level);
var crown = GreenPlayResultMapping.MapCrown(stage.PlayResult);
var isShin = stage.StageMode == 1;
```

Change to:

```csharp
var difficulty = GreenPlayResultMapping.MapDifficulty(stage.Level);
var crown = GreenPlayResultMapping.MapCrown(stage.PlayResult);
var isShin = GreenStageModeInterpreter.IsShin(stage.StageMode);
var isAiBattle = GreenStageModeInterpreter.IsAiBattle(stage.StageMode);
var allowCrownUpdate = !isAiBattle
    || stage.Level == 5
    || GreenAiBattleLevels.IsCertifiedLevel(stage.GhostStageData?.SdCertifiedLevelId ?? 0);
```

Then locate the `UpsertBestAsync` call inside `SaveStageAsync` — currently:

```csharp
// Green Dani normal scoring includes cumulative combo effects, so only Shin scores can update self-best rows.
if (playMode != (uint)PlayMode.DanMode || isShin)
{
    await UpsertBestAsync(baid, stage, difficulty, crown, isShin, cancellationToken);
}
```

Update to:

```csharp
// Green Dani normal scoring includes cumulative combo effects, so only Shin scores can update self-best rows.
if (playMode != (uint)PlayMode.DanMode || isShin)
{
    await UpsertBestAsync(baid, stage, difficulty, crown, isShin, allowCrownUpdate, cancellationToken);
}
```

- [ ] **Step 4: Update `UpsertBestAsync` to honor the crown gate**

Change the signature:

```csharp
private async Task UpsertBestAsync(
    uint baid,
    CommonPlayResultData.StageData stage,
    Difficulty difficulty,
    CrownType crown,
    bool isShin,
    bool allowCrownUpdate,
    CancellationToken cancellationToken)
```

Update both branches of the body. The full method becomes:

```csharp
private async Task UpsertBestAsync(
    uint baid,
    CommonPlayResultData.StageData stage,
    Difficulty difficulty,
    CrownType crown,
    bool isShin,
    bool allowCrownUpdate,
    CancellationToken cancellationToken)
{
    var existing = await context.SongBestDataGreen.FindAsync([baid, stage.SongNo, difficulty, isShin], cancellationToken);
    if (existing is null)
    {
        context.SongBestDataGreen.Add(new SongBestDatumGreen
        {
            Baid = baid,
            SongId = stage.SongNo,
            Difficulty = difficulty,
            IsShin = isShin,
            BestScore = stage.PlayScore,
            BestRate = stage.ScoreRate,
            BestCrown = allowCrownUpdate ? crown : CrownType.None
        });
        return;
    }

    if (stage.PlayScore > existing.BestScore)
    {
        existing.BestScore = stage.PlayScore;
        existing.BestRate = stage.ScoreRate;
    }

    if (allowCrownUpdate && CrownRank(crown) > CrownRank(existing.BestCrown))
    {
        existing.BestCrown = crown;
    }
}
```

- [ ] **Step 5: Add `ApplyGhostPlayedSongBits` and call it from `HandleGreen`**

Add this private static helper anywhere near the other private statics (e.g., directly above `ApplyGhostUpdates`):

```csharp
private static void ApplyGhostPlayedSongBits(UserSaveDataGreen saveData, CommonPlayResultData playResultData)
{
    var aiBattleSongNos = playResultData.AryStageInfoes
        .Where(stage => GreenStageModeInterpreter.IsAiBattle(stage.StageMode))
        .Select(stage => stage.SongNo);

    saveData.GhostPlayedSongFlag = SetBits(
        saveData.GhostPlayedSongFlag,
        aiBattleSongNos,
        GreenProtocolBytes.GhostPlayedSongBytes);
}
```

Then locate the per-stage loop in `HandleGreen`:

```csharp
        foreach (var stage in playResultData.AryStageInfoes)
        {
            GreenProfileCounters.ApplyStage(saveData, stage);
            await SaveStageAsync(request.Baid, stage, playResultData.PlayMode, playTime, cancellationToken);
        }

        await SaveGreenDanAsync(saveData, playResultData, green, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return 1;
```

Insert one call after the loop, before `SaveGreenDanAsync`:

```csharp
        foreach (var stage in playResultData.AryStageInfoes)
        {
            GreenProfileCounters.ApplyStage(saveData, stage);
            await SaveStageAsync(request.Baid, stage, playResultData.PlayMode, playTime, cancellationToken);
        }

        ApplyGhostPlayedSongBits(saveData, playResultData);

        await SaveGreenDanAsync(saveData, playResultData, green, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return 1;
```

Placing it before `SaveGreenDanAsync` is intentional: `SaveGreenDanAsync` calls `context.SaveChangesAsync` internally for its summary update (see existing `UpdateGreenDanSummaryAsync`), so the bits should be in place on `saveData` before that runs.

- [ ] **Step 6: Build**

Run:

```bash
dotnet build
```

Expected: build succeeds. Any compile error here means a step above was applied incorrectly — re-read the patch instructions and fix before continuing.

- [ ] **Step 7: Run the full Green test suite to confirm no regression**

Run:

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Green"
```

Expected: every Green test passes. The pre-existing `UpdatePlayResult_Green_RejectsUnknownStageMode` test uses `StageMode = 2` — still rejected by the new predicate. The shin-split tests use `StageMode = 0` and `1` — still handled identically. The Dani tests use `PlayMode = 1` and no AI Battle stages — unaffected. The reward / costume / favorite / recent tests are unaffected.

If any test fails: STOP. Investigate. Likely causes:

- `allowCrownUpdate` not threaded through correctly — re-check Steps 3 and 4.
- The crown-gating logic mistakenly fires on non-AI-Battle plays — the first OR-clause `!isAiBattle` must short-circuit; double-check the condition.
- `ApplyGhostPlayedSongBits` called twice or in the wrong place — verify Step 5.

- [ ] **Step 8: Commit**

```bash
git -C H:/TaikoLocalServer status --short
git -C H:/TaikoLocalServer add Application/Handlers/UpdatePlayResultCommand.Green.cs
git -C H:/TaikoLocalServer commit -m "$(cat <<'EOF'
Accept Green AI Battle plays in the play-result handler

Loosen the StageMode predicate to {0,1,3,4}, route IsShin via
GreenStageModeInterpreter, gate BestCrown mutation by the wiki's
正規-level rule (only sd_certified_level_id ∈ {1,5,9,13} or Ura
plays update crown in AI Battle), and set GhostPlayedSongFlag bits
for every AI Battle stage.

Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>
EOF
)"
```
