---
phase: 42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil
plan: "03"
subsystem: application
tags: [momoiro, ac15, playresult, mapperly, favorites, unlocks, counters]

requires:
  - phase: 42-02-momoiro-playresult-schema
    provides: Momoiro-owned play-history, best, favorite, recent, and bounded Dan persistence surfaces
provides:
  - Generic AC15 normal-play favorite factory hook for era-specific favorite row creation
  - Momoiro normal play and best-row Mapperly projections
  - Momoiro release/title/tone/costume unlock accessors and profile counter accessors
affects: [phase-42, momoiro-playresult, momoiro-runtime-mutation, mapperly]

tech-stack:
  added: []
  patterns:
    - Optional async favorite factory on shared AC15 normal-play table bundles
    - Mapperly partial projections with handwritten wrappers only for BAID assignment and crown-update gating
    - Momoiro save-field accessors bounded by Ac15EraProfiles.Momoiro.Limits

key-files:
  created:
    - .planning/phases/42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil/42-03-SUMMARY.md
  modified:
    - Application/Ac15/Ac15NormalPlayWriter.cs
    - Application/Ac15/Ac15NormalPlayMapper.cs
    - Application/Ac15/Ac15UnlockFlagAccess.cs
    - Application/Ac15/Ac15ProfileCounterUpdater.cs
    - Tests/Ac15/Ac15NormalPlayWriterTests.cs

key-decisions:
  - "Momoiro favorite creation is supplied through an optional table-bundle factory so existing AC15 eras keep their default favorite-row behavior."
  - "Momoiro normal play mapping remains Mapperly source-generator driven; generated source was emitted and inspected for Momoiro play and best assignments."
  - "Momoiro unlock and counter mutation surfaces are limited to UserSaveDataMomoiro fields and Ac15EraProfiles.Momoiro limits."

patterns-established:
  - "Future Momoiro playresult handlers can pass a CreateFavorite factory that appends new favorites after the current DisplayOrder max."
  - "Momoiro shared-profile mutation can use Ac15UnlockFlagAccess.Momoiro and Ac15ProfileCounterUpdater.Momoiro without adjacent-era save rows."

requirements-completed: [MORUN-01, MORUN-02, MORUN-05]
requirements-note: "Helper/accessor surfaces for these requirements are complete; runtime requirement checkboxes remain pending until later Phase 42 handler/controller plans implement observable playresult mutation."

duration: 14min
completed: 2026-06-26
status: complete
---

# Phase 42 Plan 03: Momoiro Normal-Play Helper Summary

**Shared AC15 normal-play helpers now expose Momoiro favorite ordering, Mapperly row projections, and Momoiro save-field mutation accessors without adding handler or route behavior**

## Performance

- **Duration:** 14 min
- **Started:** 2026-06-26T14:14:32Z
- **Completed:** 2026-06-26T14:28:28Z
- **Tasks:** 3/3
- **Files modified:** 5 implementation/test files plus this summary

## Accomplishments

- Added an optional async favorite factory to `Ac15NormalPlayTables` and `Ac15NormalPlayWriter.SetFavoriteAsync`.
- Added writer coverage proving Momoiro new favorites append at `max(DisplayOrder) + 1` while preserving existing orders.
- Added Mapperly-generated Momoiro normal play and best-row projections.
- Added Momoiro unlock flag and profile counter accessors using only `UserSaveDataMomoiro` fields and Momoiro limits.

## Task Commits

1. **Task 1: Add favorite creation hook and Momoiro writer coverage** - `d3ce7578` (`feat`)
2. **Task 2: Add Momoiro normal/best Mapperly projections** - `a3f0dfbe` (`feat`)
3. **Task 3: Add Momoiro unlock and profile counter accessors** - `d3ffc454` (`feat`)

## Files Created/Modified

- `Application/Ac15/Ac15NormalPlayWriter.cs` - Optional favorite factory hook, retaining default `new TFavorite { Baid, SongNo }` behavior.
- `Tests/Ac15/Ac15NormalPlayWriterTests.cs` - Focused Momoiro favorite DisplayOrder append coverage.
- `Application/Ac15/Ac15NormalPlayMapper.cs` - Momoiro play/best Mapperly declarations and BAID/crown wrapper.
- `Application/Ac15/Ac15UnlockFlagAccess.cs` - Momoiro release, tone, title, and costume flag accessors.
- `Application/Ac15/Ac15ProfileCounterUpdater.cs` - Momoiro genre, pushed, favorite, and recent counter accessors.

## Verification

| Command | Result |
|---------|--------|
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15NormalPlayWriterTests" --no-restore -- RunConfiguration.DisableParallelization=true` | PASS: 3 passed, 0 failed, 0 skipped. |
| `dotnet build Application/Application.csproj /p:EmitCompilerGeneratedFiles=true --no-restore` | PASS: 0 warnings, 0 errors. |
| `rg -n "ToMomoiroSongPlayDatum\|ToMomoiroSongBestDatum\|SongPlayDatumMomoiro\|SongBestDatumMomoiro" Application/obj/Debug/net10.0/generated/Riok.Mapperly/Riok.Mapperly.MapperGenerator/Ac15NormalPlayMapper.g.cs` | PASS: emitted Momoiro mapper methods found at generated lines 261 and 388. |
| Generated-source inspection of `Ac15NormalPlayMapper.g.cs` | PASS: `SongPlayDatumMomoiro` assignments include BAID/play fields; `SongBestDatumMomoiro` assignments include song, difficulty, shin, score/rate, and crown fields. |
| `dotnet build Application/Application.csproj --no-restore` | PASS: 0 warnings, 0 errors. |
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResultHandlerTests\|FullyQualifiedName~MomoiroReadbackHandlerTests" --no-restore -- RunConfiguration.DisableParallelization=true` | EXPECTED RED: compiled; 4 readback tests passed, 3 playresult handler tests failed only at `Unsupported AC15 playresult command era: Momoiro`. |
| `git status --porcelain -- proto\momoiro` | PASS: no proto changes. |
| Case-sensitive forbidden-route/state scan over changed files | PASS: no `shoppingresult`, `bestscore`, `communicationlog`, `mainichisong`, `taikojuku.php`, Tokkun, Banacoin, battle, Don Challenge, ChallengeCompe, gacha, tournament, wallet, or payment terms. |
| `git diff --cached --name-only -- Host/.gitignore` | PASS: no staged `Host/.gitignore`. |
| `git status --short` | PASS: only pre-existing unstaged `Host/.gitignore` remains. |

## Decisions Made

- Kept the favorite-order rule out of the generic writer body by passing it as an optional factory from Momoiro table bundles.
- Kept Mapperly as the source of mechanical Momoiro row projection code; handwritten code only wraps BAID assignment and crown suppression.
- Kept Task 3 limited to shared accessors so later handler plans can call `Ac15CommonProfileMutation.TryApplyDonPoints` without adding handler/controller behavior here.

## Deviations from Plan

### Auto-fixed Issues

None - no Rule 1/2/3 auto-fixes were needed.

### Workflow-Scope Adjustment

**1. [Scope Boundary] Skipped `requirements.mark-complete` for MORUN runtime requirements**
- **Found during:** Plan closeout
- **Issue:** Plan frontmatter references MORUN-01, MORUN-02, and MORUN-05, but this plan only prepares helper/accessor surfaces. The requirements require later runtime mutation/no-cross-era behavior.
- **Adjustment:** Summary records helper/accessor coverage, but `.planning/REQUIREMENTS.md` checkboxes remain pending for later Phase 42 plans.
- **Files modified:** None for requirements.
- **Verification:** Requirement checkboxes are not marked complete by this plan.

**Total deviations:** 0 auto-fixed; 1 workflow-scope adjustment.
**Impact on plan:** Helper/accessor scope is complete without overclaiming runtime Momoiro playresult support.

## Issues Encountered

- The Task 1 RED iteration initially referenced future Task 2 Momoiro mapper methods; the test was tightened before production implementation so Task 1 covered only the favorite factory hook.
- `Host/.gitignore` remained unstaged and visibly matches the known pre-existing Momoiro data ignore diff, but the recorded `42-VALIDATION.md` baseline hash `73190e8a4bb993eadc0a9364cac7e1b0336f45192911fb68d9ab1ebb87cac782` could not be reproduced with raw or normalized hash methods in this shell. The file was not edited or staged by this plan; current raw trimmed diff hash from pipeline bytes was `3e04c0e4557024f8c42faa725b77649d8085c760c51ac0a8bfafef997b2c309b`.

## Authentication Gates

None.

## User Setup Required

None - no external service configuration required.

## Known Stubs

None. Scoped scan over changed files found only ordinary nullable optional parameters in helper/test signatures, not UI-facing placeholders or unwired data.

## Threat Flags

None. This plan modified only planned Application helper/accessor trust surfaces and a focused test; it introduced no new endpoint, auth path, filesystem access, schema, unsupported route, or unsupported feature-state surface.

## Next Phase Readiness

Ready for `42-04`: Momoiro bounded Dan catalog/profile/mapper support can build on the prepared shared accessors. Momoiro playresult handler/controller behavior remains intentionally unimplemented until later Phase 42 plans.

## Self-Check: PASSED

- Summary file exists: `.planning/phases/42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil/42-03-SUMMARY.md`.
- Modified files exist on disk: `Application/Ac15/Ac15NormalPlayWriter.cs`, `Application/Ac15/Ac15NormalPlayMapper.cs`, `Application/Ac15/Ac15UnlockFlagAccess.cs`, `Application/Ac15/Ac15ProfileCounterUpdater.cs`, and `Tests/Ac15/Ac15NormalPlayWriterTests.cs`.
- Task commits found: `d3ce7578`, `a3f0dfbe`, and `d3ffc454`.
- `git status --porcelain -- proto\momoiro` returned no output.
- `git diff --cached --name-only -- Host/.gitignore` returned no output.
- Final `git status --short` showed only the pre-existing unstaged `Host/.gitignore`.
- Scoped stub and forbidden-route scans found no plan-blocking stubs or unsupported feature additions in changed files.

---
*Phase: 42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil*
*Completed: 2026-06-26*
