---
phase: 42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil
plan: "02"
subsystem: database
tags: [momoiro, ac15, playresult, ef-core, sqlite, dan]

requires:
  - phase: 42-01-wave-0-playresult-red-contracts
    provides: RED contracts and raw fixture helpers for Momoiro play-history, bounded Dan, and no-cross-era persistence
provides:
  - Momoiro-owned normal play-history entity and DbSet/table
  - Momoiro-owned bounded Dan score and stage entities plus DbSets/tables
  - EF migration adding only Phase 42 Momoiro playresult and Dan schema
affects: [phase-42, momoiro-playresult, momoiro-dan, momoiro-runtime-mutation]

tech-stack:
  added: []
  patterns:
    - AC15 era-owned play-history entity and EF table pattern copied from Kimidori/Murasaki
    - Bounded Momoiro Dan schema limited to playresult, BAID, and userdata compatibility
    - Active migration-body inspection for unsupported feature-state gating

key-files:
  created:
    - Domain/Entities/SongPlayDatumMomoiro.cs
    - Domain/Entities/DanScoreDatumMomoiro.cs
    - Domain/Entities/DanStageScoreDatumMomoiro.cs
    - Infrastructure/Persistence/Migrations/20260626140044_AddMomoiroPlayResultState.cs
    - Infrastructure/Persistence/Migrations/20260626140044_AddMomoiroPlayResultState.Designer.cs
    - .planning/phases/42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil/42-02-SUMMARY.md
  modified:
    - Application/Abstractions/ITaikoDbContext.Momoiro.cs
    - Infrastructure/Persistence/TaikoDbContext.Momoiro.cs
    - Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs
    - Tests/Green/GreenAuthConfigTests.cs
    - Tests/Momoiro/MomoiroHandlerFixture.cs

key-decisions:
  - "Momoiro play-history and bounded Dan persistence use Momoiro-owned tables only; no adjacent-era gameplay table is reused."
  - "The active AddMomoiroPlayResultState migration body is the scope gate; EF designer and full snapshot adjacent-era terms are not treated as new Momoiro state."
  - "MORUN requirement checkboxes remain pending because this plan adds schema only and does not implement playresult handler/controller runtime behavior."

patterns-established:
  - "Momoiro raw fixture helpers remain idempotent and now mirror production BAID cascade FKs plus play/Dan indexes."
  - "Focused Phase 42 RED tests may remain red only on missing later Application behavior, not schema/table absence."

requirements-completed: [MORUN-01, MORUN-03, MORUN-05]
requirements-note: "Schema surfaces for these requirements are complete; runtime requirement checkboxes were intentionally left pending for later Phase 42 implementation plans."

duration: 12min
completed: 2026-06-26
status: complete
---

# Phase 42 Plan 02: Momoiro Play-History and Bounded Dan Persistence Schema Summary

**Momoiro-owned EF schema for normal play-history and bounded Dan compatibility without playresult handler/controller behavior**

## Performance

- **Duration:** 12 min
- **Started:** 2026-06-26T13:55:47Z
- **Completed:** 2026-06-26T14:07:20Z
- **Tasks:** 3/3
- **Files modified:** 10 implementation/test files plus this summary

## Accomplishments

- Added `SongPlayDatumMomoiro`, `DanScoreDatumMomoiro`, and `DanStageScoreDatumMomoiro` Domain entities using existing AC15 interfaces.
- Exposed Momoiro play-history and Dan DbSets through `ITaikoDbContext` and `TaikoDbContext`.
- Generated `AddMomoiroPlayResultState`, which creates only `SongPlayDatum_Momoiro`, `DanScoreDatum_Momoiro`, and `DanStageScoreDatum_Momoiro`.
- Aligned Momoiro fixture raw SQL helpers with the production schema while preserving the expected RED handler failures for later plans.

## Task Commits

1. **Task 1: Add Momoiro play-history and Dan entities** - `61a76065` (`feat`)
2. **Task 2: Add Momoiro DbContext surfaces and migration** - `ed0a833c` (`feat`)
3. **Task 3: Align fixture helpers and run schema scope gates** - `04aa504b` (`test`)

## Files Created/Modified

- `Domain/Entities/SongPlayDatumMomoiro.cs` - Momoiro-owned normal play-history entity implementing `IAc15SongPlayDatum`.
- `Domain/Entities/DanScoreDatumMomoiro.cs` - Momoiro-owned bounded Dan score entity implementing `IAc15DanScoreDatum`.
- `Domain/Entities/DanStageScoreDatumMomoiro.cs` - Momoiro-owned bounded Dan stage-score child entity implementing `IAc15DanStageScoreDatum`.
- `Application/Abstractions/ITaikoDbContext.Momoiro.cs` - Application DbSet port surfaces for Momoiro play-history and Dan schema.
- `Infrastructure/Persistence/TaikoDbContext.Momoiro.cs` - EF DbSets, table names, keys, indexes, enum conversions, and cascade relationships.
- `Infrastructure/Persistence/Migrations/20260626140044_AddMomoiroPlayResultState.cs` - Active migration body for the three Momoiro tables.
- `Infrastructure/Persistence/Migrations/20260626140044_AddMomoiroPlayResultState.Designer.cs` - EF migration model metadata.
- `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs` - EF model snapshot updated by migration generation.
- `Tests/Green/GreenAuthConfigTests.cs` - Throwing test fake updated for new `ITaikoDbContext` properties.
- `Tests/Momoiro/MomoiroHandlerFixture.cs` - Raw helper DDL aligned with production FKs/indexes and existing count helpers retained.

## Verification

| Command | Result |
|---------|--------|
| `dotnet build Domain/Domain.csproj --no-restore` | PASS: 0 warnings, 0 errors. |
| `rg -n "Kimidori\|Murasaki\|Green\|Blue\|Yellow\|Red\|White\|Nijiiro" Domain/Entities/*Momoiro.cs` | PASS: no adjacent-era type-name leftovers in new entity declarations. |
| `dotnet build Infrastructure/Infrastructure.csproj --no-restore` | PASS before and after migration generation: 0 warnings, 0 errors. |
| `dotnet ef migrations add AddMomoiroPlayResultState --project Infrastructure --startup-project Host` | PASS: migration `20260626140044_AddMomoiroPlayResultState` generated once. |
| Active migration-body gate for required tables and forbidden terms | PASS: 3 `CreateTable` calls; required Momoiro table names present; no `Tokkun`, `Battle`, `Banacoin`, `ChallengeCompe`, `DonChallenge`, `ShopSeason`, `Payment`, or `Wallet` terms in the active migration body. |
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResultHandlerTests\|FullyQualifiedName~MomoiroReadbackHandlerTests" --no-restore -- RunConfiguration.DisableParallelization=true` | EXPECTED RED: compiled; 4 readback tests passed, 3 playresult handler tests failed only at `Unsupported AC15 playresult command era: Momoiro`, which is later Application handler scope. |
| `git status --porcelain -- proto\momoiro` | PASS: no proto changes. |
| `git diff --cached --name-only -- Host/.gitignore` | PASS: no staged `Host/.gitignore`. |
| Trimmed SHA-256 of `git diff -- Host/.gitignore` | PASS: `73190e8a4bb993eadc0a9364cac7e1b0336f45192911fb68d9ab1ebb87cac782`, matching `42-VALIDATION.md`. |

## Decisions Made

- Used Momoiro-owned tables for play-history and Dan rows instead of sharing Kimidori/Murasaki gameplay tables.
- Kept Dan schema bounded to playresult/BAID/userdata compatibility; no Taikojuku route, practice-folder behavior, or challenge persistence was added.
- Treated EF designer/snapshot adjacent-era terms as metadata noise and validated unsupported feature scope against the active migration body.
- Left runtime MORUN requirement checkboxes pending because the plan deliberately excludes production handler/controller behavior.

## Deviations from Plan

### Auto-fixed Issues

None - no Rule 1/2/3 auto-fixes were needed.

### Workflow-Scope Adjustment

**1. [Scope Boundary] Skipped `requirements.mark-complete` for MORUN runtime requirements**
- **Found during:** Plan closeout
- **Issue:** Plan frontmatter references MORUN-01, MORUN-03, and MORUN-05, but this plan only adds schema surfaces. The requirements require later runtime mutation/no-cross-era behavior.
- **Adjustment:** Summary records schema coverage, but `.planning/REQUIREMENTS.md` checkboxes remain pending for later Phase 42 plans.
- **Files modified:** None for requirements.
- **Verification:** Requirement checkboxes are not marked complete by this plan.

**Total deviations:** 0 auto-fixed; 1 workflow-scope adjustment.
**Impact on plan:** Schema scope is complete without overclaiming runtime Momoiro playresult support.

## Issues Encountered

- Focused Momoiro RED tests still fail on missing handler dispatch for `GameEra.Momoiro`. This is expected for plan 42-02 and confirms the failure is later Application behavior, not schema absence.
- `Host/.gitignore` remains a pre-existing unstaged dirty file. Its trimmed diff hash still matches the `42-VALIDATION.md` baseline.

## Authentication Gates

None.

## User Setup Required

None - no external service configuration required.

## Known Stubs

None. Placeholder/TODO scan over this plan's changed files found no `TODO`, `FIXME`, `placeholder`, `coming soon`, or `not available` markers. Existing empty array initializers are normal entity/fixture defaults, not UI-facing stubs.

## Threat Flags

None. The new Application-to-SQLite and shared-identity-to-Momoiro-state surfaces are the planned schema surfaces covered by T-42-04, T-42-05, and T-42-06; no new endpoint, auth path, file access pattern, or unsupported feature table was introduced.

## Next Phase Readiness

Ready for `42-03`: later plans can wire Momoiro normal-play helpers, favorite ordering, unlocks, and counters against real Momoiro-owned play-history tables. Handler/controller production behavior remains intentionally unimplemented.

## Self-Check: PASSED

- Summary file exists: `.planning/phases/42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil/42-02-SUMMARY.md`.
- Created files exist on disk: `Domain/Entities/SongPlayDatumMomoiro.cs`, `Domain/Entities/DanScoreDatumMomoiro.cs`, `Domain/Entities/DanStageScoreDatumMomoiro.cs`, `Infrastructure/Persistence/Migrations/20260626140044_AddMomoiroPlayResultState.cs`, and `Infrastructure/Persistence/Migrations/20260626140044_AddMomoiroPlayResultState.Designer.cs`.
- Task commits found: `61a76065`, `ed0a833c`, and `04aa504b`.
- `git status --porcelain -- proto\momoiro` returned no output.
- `git diff --cached --name-only -- Host/.gitignore` returned no output.
- The only unrelated working-tree change remains the pre-existing unstaged `Host/.gitignore`.

---
*Phase: 42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil*
*Completed: 2026-06-26*
