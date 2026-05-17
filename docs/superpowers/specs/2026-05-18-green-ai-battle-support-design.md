# Green AI Battle (Ghost) Support — Design

## Problem

When the user plays AI Battle (AIバトル演奏) in Green and a `playresult.php`
credit is submitted, the server rejects it. `Host/Logs/log-20260518.txt` shows
five consecutive credits hitting the warning
`Rejecting invalid Green playresult payload for baid 1`. The rejection comes
from `UpdatePlayResultCommand.Green.cs`: `IsValidGreenStage` requires
`stage.StageMode <= 1`, but AI Battle stages carry `StageMode = 3`
(AI Battle, normal scoring) or `StageMode = 4` (AI Battle, Shin scoring).
`PlayMode = 6 = AiBattle` is already defined in `Domain/Enums/PlayMode.cs`
but is not honored anywhere.

`getghostdata.php` and `getghostscore.php` controllers/handlers/mappers
already exist and round-trip correctly against the existing
`UserSaveDataGreen.Ghost*` columns, `GreenGhostTokens`, `GreenGhostWinnings`,
and `GhostStageSectionDatumGreen` tables. The `ApplyGhostUpdates` method in
the Green play-result handler is fully written but unreachable because
validation rejects before it runs.

The wiki at
<https://wikiwiki.jp/taiko-fumen/作品/新AC/AIバトル演奏/グリーン> documents
two caveats that matter for behavior fidelity:

- AI Battle records **crowns only for `sd_certified_level_id ∈ {1, 5, 9, 13}`**
  (the 正規 levels — かんたん, ふつう, むずかしい, おに positions in the
  13-step AI Battle level scale). Intermediate sub-levels (2/3/4, 6/7/8,
  10/11/12) record score but do not update the crown column. Ura (course
  Level 5) has no intermediate sub-levels, so any Ura AI Battle play
  records crown.
- Best score, good/ok/ng/pound, donmedal/katsumedal, play counts, and the
  per-section territory data are all recorded on every sub-level.

## Goal

Faithful AI Battle support: accept the play, persist every AI Battle field
the client sends, honor the crown-gating caveat, and update
`GhostPlayedSongFlag` so the next `getghostdata.php` reflects all songs
played in AI Battle. Trust the client for `RankId / WinPoint /
CertifiedLevelId / AryWinningsData` and tokens — Green here is a private
single-player server with no leaderboard.

## Non-goals

- No schema migration. No new entities. No mapper changes.
- No WebUI changes. AI Battle stats are not currently surfaced in TaikoWebUI
  and remain out of scope.
- No server-side re-derivation of rank-up rules or anti-cheat for winnings.
  The wiki's rank-point progression table (1/9/24/54/99/159/234/324/429/
  549/699 for ranks 1–10+) is reference material only; we trust the client.
- No reverse-engineering of the AI Battle `Level` field semantics beyond
  what the existing `MapDifficulty` already does. See "Known caveats".

## StageMode encoding

Observed from `Host/Logs/log-20260518.txt`:

| StageMode | Meaning                              | Source                       |
| --------- | ------------------------------------ | ---------------------------- |
| 0         | Normal play, normal scoring          | Prior shin-split work        |
| 1         | Normal play, Shin scoring (真打)     | Prior shin-split work        |
| 3         | AI Battle, normal scoring            | log-20260518.txt line 1571   |
| 4         | AI Battle, Shin scoring              | log-20260518.txt line 3946   |

This is an enum, not a bitfield (`2` and values ≥5 are unobserved).

Two interpretations are needed everywhere we look at `StageMode`:

- `IsShin(stageMode)` ↔ `stageMode is 1 or 4`
- `IsAiBattle(stageMode)` ↔ `stageMode is 3 or 4`

A single `Application/Common/GreenStageModeInterpreter` static helper
exposes both. Raw `StageMode` continues to be persisted on
`SongPlayDatumGreen.StageMode` for audit (no schema change).

## Crown gating

`Application/Common/GreenAiBattleLevels.IsCertifiedLevel(uint sdCertifiedLevelId)`
returns `true` iff `sdCertifiedLevelId is 1 or 5 or 9 or 13`. Used by the
best-score upsert.

In `UpdatePlayResultCommand.Green.cs::UpsertBestAsync`, after computing the
candidate crown, only apply the new crown when **any** of the following
holds:

1. The stage is not an AI Battle stage (`!IsAiBattle(stage.StageMode)`), or
2. The course difficulty is Ura (`stage.Level == 5`), or
3. `GreenAiBattleLevels.IsCertifiedLevel(stage.GhostStageData?.SdCertifiedLevelId ?? 0)`.

The score / rate update logic is unchanged for AI Battle plays.

## GhostPlayedSongFlag

Server-derived. After all stages have been saved, for any stage whose
`StageMode` is AI Battle (`IsAiBattle`), set bit number `stage.SongNo` in
`saveData.GhostPlayedSongFlag`. The flag is sized to
`GreenProtocolBytes.GhostPlayedSongBytes`. Reuse the existing
`SetBits(byte[], IEnumerable<uint>, int)` helper at the bottom of
`UpdatePlayResultCommand.Green.cs`.

Songs whose `SongNo >= GhostPlayedSongBytes * 8` are skipped, matching the
behavior of `SetBits`.

## Validation changes

`IsValidGreenStage` currently fails on `StageMode > 1`. Remove the
`MaxGreenStageMode` constant and replace the predicate with C# pattern
matching for the known set `{0, 1, 3, 4}`:

```csharp
// inside IsValidGreenStage:
&& stage.StageMode is 0 or 1 or 3 or 4
```

All other validation in `IsValidGreenStage` and `HasOnlyInRangeUnlockRewards`
stays.

`SdCertifiedLevelId` is not validated against a known range. The wiki
defines a 1–13 + ura sub-level space but we have no need to reject values
outside it; an out-of-range value just fails the `IsCertifiedLevel` check
and falls through to "do not update crown", which is the safe direction.

## Rank, winnings, perf, tokens, release info

No change. `ApplyGhostUpdates` already:

- Sets bits in `GhostReleaseInfoFlag` from `ReleaseInfoId`.
- Upserts `GreenGhostTokens` rows from `AryTokendata`.
- Stores `InputMedian` / `InputVariance` on `UserSaveDataGreen`.
- Stores `RankId / WinPoint / CertifiedLevelId` on `UserSaveDataGreen`.
- Recomputes `GhostTotalWinnings` as the saturated sum of
  `AryWinningsData.Winnings`.
- Upserts `GreenGhostWinnings` per `LevelId`.

Each branch is gated on the respective top-level field being non-null, so
non-AI-Battle plays do not trigger any of this even though we are removing
the global rejection.

## Section data (per-stage ghost record)

No change. The existing `SaveStageAsync` already inserts
`GhostStageSectionDatumGreen` rows when `stage.GhostStageData` is present,
indexed by `SectionNo` ascending from 0. `GetGhostScoreQueryHandler` reads
the most recent `SongPlayDatumGreen` row for `(Baid, SongId, Difficulty)`
and joins the sections. With AI Battle now persisting, the section graph
in the cab on the next play of the same song will show the player's
previous performance.

## Best-score / IsShin handling

`isShin = GreenStageModeInterpreter.IsShin(stage.StageMode)` replaces the
current `stage.StageMode == 1`. The `(Baid, SongId, Difficulty, IsShin)`
key on `SongBestDatumGreen` then routes AI Battle Shin plays to the same
row as Normal Shin plays for that song/difficulty, and AI Battle non-Shin
to the same row as Normal non-Shin. This matches the user's intent that
"whatever mode is used for that song should be used".

The "Green Dani normal scoring only updates self-best when Shin" guard
(`if (playMode != (uint)PlayMode.DanMode || isShin)`) is unaffected — AI
Battle uses `PlayMode = AiBattle (6)`, not `DanMode`, so the guard passes
and best-score upsert runs.

## File changes

- **Edit** `Application/Handlers/UpdatePlayResultCommand.Green.cs`
  - Replace `MaxGreenStageMode` constant with `KnownGreenStageModes` set
    and update the predicate in `IsValidGreenStage`.
  - Replace `var isShin = stage.StageMode == 1;` with
    `var isShin = GreenStageModeInterpreter.IsShin(stage.StageMode);`.
  - In `UpsertBestAsync`, gate `BestCrown` mutation by the rule in
    "Crown gating" above.
  - Add a call to `ApplyGhostPlayedSongBits(saveData, playResultData)`
    after the per-stage loop but before `SaveChangesAsync`, defined as a
    small private static using `SetBits`.
- **Add** `Application/Common/GreenStageModeInterpreter.cs` — `IsShin`
  and `IsAiBattle` static methods.
- **Add** `Application/Common/GreenAiBattleLevels.cs` — `IsCertifiedLevel`
  static method, with the 正規 set `{1, 5, 9, 13}` as a constant.
- **Add** `Tests/Green/GreenAiBattlePlayResultTests.cs` — see "Test plan".
- **Add** `Tests/Green/GreenStageModeInterpreterTests.cs` — pure-function
  coverage of the 0/1/3/4 truth tables.
- **Add** `Tests/Green/GreenAiBattleLevelsTests.cs` — pure-function
  coverage of `IsCertifiedLevel` for 0..14 and a couple of out-of-range
  values.

## Test plan

`Tests/Green/GreenAiBattlePlayResultTests.cs`, modeled on the existing
`GreenPlayResultHandlerTests` fixture pattern:

1. **`UpdatePlayResult_Green_AcceptsAiBattlePlay`** — Build a
   `CommonPlayResultData` with `PlayMode = AiBattle`, one stage with
   `StageMode = 3, SdCertifiedLevelId = 5` and a `GhostStageData` with
   three section rows. Top-level `GhostUpdateRankData` with one winnings
   entry, one token, one release-info id, and `GhostUpdatePerfData`. Run
   the handler. Assert:
   - Result is `1`.
   - One `SongPlayDatumGreen` row with `StageMode = 3, IsShin = false`.
   - Three `GhostStageSectionDatumGreen` rows linked to it.
   - `GreenGhostWinnings` row matches the supplied `LevelId`/`Winnings`.
   - `GreenGhostTokens` row matches.
   - `UserSaveDataGreen.GhostInputMedian / Variance / RankId / WinPoint /
     CertifiedLevelId / TotalWinnings` reflect the request.
   - Bit `SongNo` is set in `GhostPlayedSongFlag`.
   - Bit `1` is set in `GhostReleaseInfoFlag`.

2. **`UpdatePlayResult_Green_AiBattleShinRoutesToShinBest`** — Same as 1
   but with `StageMode = 4`. Assert the `SongBestDatumGreen` row that was
   upserted has `IsShin = true`.

3. **`UpdatePlayResult_Green_AiBattleIntermediateLevelDoesNotUpdateCrown`**
   — Pre-seed a `SongBestDatumGreen` row with `BestCrown = CrownType.None`.
   Submit an AI Battle play with `SdCertifiedLevelId = 11` and
   `PlayResult = 2 (Gold)`. Assert the persisted `BestCrown` is still
   `None`; assert score updated.

4. **`UpdatePlayResult_Green_AiBattleCertifiedLevelUpdatesCrown`** — Same
   as 3 but with `SdCertifiedLevelId = 13`. Assert `BestCrown` is now
   `Gold`.

5. **`UpdatePlayResult_Green_AiBattleUraAlwaysUpdatesCrown`** — Submit an
   AI Battle play with `Level = 5 (UraOni)` and an arbitrary
   `SdCertifiedLevelId` (e.g., 7). Assert `BestCrown` updates.

6. **`UpdatePlayResult_Green_RejectsUnknownStageMode`** — Submit a stage
   with `StageMode = 2`. Assert result is `0` (rejected) and nothing is
   persisted. Repeat for `StageMode = 5`.

7. **`UpdatePlayResult_Green_NonAiBattlePlayDoesNotTouchGhostFields`** —
   Submit a normal `PlayMode = 0` credit with all `GhostUpdate*` fields
   null. Assert ghost columns on `UserSaveDataGreen` are unchanged and
   no `GreenGhostWinnings` / `GreenGhostTokens` rows were inserted.

`Tests/Green/GreenStageModeInterpreterTests.cs` — table-driven, asserts
the `(IsShin, IsAiBattle)` truth values for stage modes 0, 1, 2, 3, 4, 5.
The 2 and 5 cases assert both functions return `false` (consistent with
"unknown stage mode" failing validation upstream).

`Tests/Green/GreenAiBattleLevelsTests.cs` — asserts `IsCertifiedLevel`
returns `true` for `{1, 5, 9, 13}` and `false` for `{0, 2, 3, 4, 6, 7, 8,
10, 11, 12, 14, 100}`.

## Known caveats (documented, not addressed in this iteration)

- **AI Battle `Level` semantics are unresolved.** The two captures show
  `Level = 2, SdCertifiedLevelId = 11` (stage 0 of credit 1) and
  `Level = 1, SdCertifiedLevelId = 2` (credit 2). The first does not fit
  any clean partition of the 13-step sub-level space into the 5 course
  difficulties. We continue to use the existing
  `GreenPlayResultMapping.MapDifficulty(stage.Level)` for the
  `SongBestDatumGreen.Difficulty` key. If a player's Normal best ends up
  reflecting a Hard-tier AI Battle play, that is acceptable for a private
  server; flag for follow-up if the user reports best-score corruption.
- **No server-side rank-up validation.** The client decides
  `RankId / WinPoint`. A modded client could claim arbitrary rank. Not
  in scope.
- **No deduplication of winnings claims.** If the client replays the
  same credit, `GreenGhostWinnings.Winnings` is overwritten with the
  client-supplied cumulative value (existing behavior), so total
  winnings stay consistent. We do not detect intentional cumulative
  inflation.

## Acceptance

- `dotnet test` passes (all new and existing tests green).
- Manually: replay the `log-20260518.txt` payload through the running
  server and confirm:
  - No `Rejecting invalid Green playresult payload` warning.
  - `getghostdata.php` after the play returns `total_winnings`,
    `ghost_record_data`, `ary_token_data`, and `played_song_flag` bits
    reflecting the persisted state.
  - `getghostscore.php` for one of the played songs returns the
    section data persisted from the play.
- User runs the server and verifies the cab behavior end-to-end (per
  CLAUDE.md feedback: the user runs the server, not Claude).
