---
phase: 15-yellow-dani-shop-medals-waiwai-and-admin
plan: "01"
subsystem: database
tags: [yellow, ac15, dani, ef-core, helpers]
requires:
  - phase: 14-yellow-identity-userdata-crowns-self-best-and-normal-play
    provides: Yellow-owned save state and AC15 flag lengths used by Dan helper rules
provides:
  - Yellow-owned Dan score and stage score EF tables
  - Yellow Dan clear-grade enum and two-bit packed flag helper logic
  - Focused Yellow Dani schema/helper and persistence-boundary tests
affects: [yellow, phase-15, dani, taikojuku, userdata, adminapi]
tech-stack:
  added: []
  patterns: [Yellow-owned EF slice, AC15 Dan packed-grade helper, era-boundary tests]
key-files:
  created:
    - Domain/Enums/YellowDanClearGrade.cs
    - Domain/Entities/DanScoreDatumYellow.cs
    - Domain/Entities/DanStageScoreDatumYellow.cs
    - Application/Common/YellowDanHelpers.cs
    - Infrastructure/Persistence/Migrations/20260608021657_AddYellowDaniState.cs
    - Tests/Yellow/YellowDaniTests.cs
  modified:
    - Application/Abstractions/ITaikoDbContext.Yellow.cs
    - Infrastructure/Persistence/TaikoDbContext.Yellow.cs
    - Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs
    - Tests/Yellow/YellowPersistenceBoundaryTests.cs
    - Tests/Green/GreenAuthConfigTests.cs
key-decisions:
  - "Yellow Dani persistence uses Yellow-owned `DanScoreDatum_Yellow` and `DanStageScoreDatum_Yellow` tables instead of Blue/Green Dan rows."
  - "Yellow Dan helper rules are local to `YellowDanHelpers` and do not call Blue or Green helper code."
patterns-established:
  - "Yellow Dan schema mirrors the proven AC15 key shape while preserving era-specific table names and DbSets."
  - "Yellow Dan clear flags use the same two-bit packed-grade semantics behind Yellow-owned helpers."
requirements-completed: [YDAN-01]
duration: 25 min
completed: 2026-06-08
---

# Phase 15 Plan 01: Yellow Dani Schema Summary

**Yellow-owned Dani persistence schema with Yellow-specific packed-grade helper rules**

## Performance

- **Duration:** 25 min
- **Started:** 2026-06-08T10:00:00+08:00
- **Completed:** 2026-06-08T10:25:00+08:00
- **Tasks:** 2
- **Files modified:** 12

## Accomplishments

- Added Yellow-owned Dan best and per-stage row entities, DbSets, table mappings, and EF migration.
- Added `YellowDanClearGrade` and `YellowDanHelpers` for valid Dan id ranges, packed-grade reads/writes, max clear, and display-Dan fallback.
- Updated boundary tests to prove Yellow Dani schema/helper code does not reuse Blue/Green Dan entities, tables, or helpers.

## Task Commits

1. **Task 1: Add Yellow Dan entities and EF mapping** - `ff2c5c13` (feat)
2. **Task 2: Add Yellow Dan helper rules** - `6ddc493e` (feat)

**Plan metadata:** committed separately with this summary.

## Files Created/Modified

- `Domain/Enums/YellowDanClearGrade.cs` - Yellow clear-grade enum matching AC15 packed-grade semantics.
- `Domain/Entities/DanScoreDatumYellow.cs` - Yellow Dan best row entity keyed by BAID, Dan id, and extra flag.
- `Domain/Entities/DanStageScoreDatumYellow.cs` - Yellow Dan per-stage row entity keyed by BAID, Dan id, extra flag, and stage index.
- `Application/Abstractions/ITaikoDbContext.Yellow.cs` - Yellow Dan DbSet contract.
- `Infrastructure/Persistence/TaikoDbContext.Yellow.cs` and migration files - Yellow Dan table mappings and schema migration.
- `Application/Common/YellowDanHelpers.cs` - Yellow Dan id, packed-grade, max-clear, and display helpers.
- `Tests/Yellow/YellowDaniTests.cs` and `YellowPersistenceBoundaryTests.cs` - Focused helper and schema-boundary coverage.
- `Tests/Green/GreenAuthConfigTests.cs` - Throw-only test double update for the expanded context interface.

## Decisions Made

- Yellow Dan rows use Yellow table names and Yellow entity types even though the key shape matches Blue/Green.
- `YellowDanHelpers` duplicates the small AC15 Dan helper surface intentionally so later Yellow handler code remains auditable and era-owned.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Updated handwritten ITaikoDbContext test double**
- **Found during:** Task 1 verification
- **Issue:** `Tests/Green/GreenAuthConfigTests.ThrowingTaikoDbContext` failed to compile after `ITaikoDbContext.Yellow.cs` gained Yellow Dan DbSet properties.
- **Fix:** Added throw-only `DanScoreDataYellow` and `DanStageScoreDataYellow` members to the test double.
- **Files modified:** `Tests/Green/GreenAuthConfigTests.cs`
- **Verification:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowDani|FullyQualifiedName~YellowPersistenceBoundary"` passed.
- **Committed in:** `ff2c5c13`

---

**Total deviations:** 1 auto-fixed (1 blocking). **Impact:** Required to keep existing test infrastructure compiling after the intended interface expansion; no behavior or scope change.

## Issues Encountered

- The first Task 1 verification run exposed the expected manual context-test-double compile gap.
- A boundary test edit briefly used an untyped collection expression before `Order`; this was fixed before the Task 1 commit.

## Verification

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowDani|FullyQualifiedName~YellowPersistenceBoundary"` - passed, 16 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowDani"` - passed, 12 tests.
- `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase15-yellow"` - passed, 0 warnings, 0 errors.
- `git diff --check -- Domain Application Infrastructure Tests .planning` - passed with no whitespace errors.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for `15-02-PLAN.md`. Yellow now has the Dan EF and helper foundation needed for Dan playresult persistence and userdata/readback wiring. Phase-level verification, code review, and Phase 16 were not started.

## Self-Check: PASSED

- Created files exist on disk.
- Task commits `ff2c5c13` and `6ddc493e` exist in git history.
- Plan-level verification commands passed before summary creation.

---
*Phase: 15-yellow-dani-shop-medals-waiwai-and-admin*
*Completed: 2026-06-08*
