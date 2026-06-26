---
phase: 41-momoiro-identity-userdata-self-best-and-crown-readback
plan: "01"
subsystem: testing
tags: [momoiro, ac15, red-tests, userdata, selfbest, crown-readback]

requires:
  - phase: 40-momoiro-protocol-limits-root-catalog-and-route-behavior
    provides: Momoiro catalog order, song hash version/table, profile limits, and userdata-owned crown placement
provides:
  - Wave 0 RED contracts for Momoiro identity, userdata, self-best, release, favorite/recent, and crown readback
  - Compile-safe raw SQLite helpers for future Momoiro readback tables, including favorite DisplayOrder
  - Validation ownership rows for MORDB-01 through MORDB-05
affects: [phase-41, phase-42, momoiro-readback, momoiro-persistence]

tech-stack:
  added: []
  patterns:
    - Raw SQL future-table helpers for RED tests that must compile before Momoiro domain entities exist
    - Direct Application handler and direct controller invocation tests over public seams

key-files:
  created:
    - Tests/Momoiro/MomoiroHandlerFixture.cs
    - Tests/Momoiro/MomoiroReadbackHandlerTests.cs
    - Tests/Momoiro/MomoiroControllerReadbackTests.cs
    - Tests/Momoiro/MomoiroCrownReadbackTests.cs
  modified:
    - .planning/phases/41-momoiro-identity-userdata-self-best-and-crown-readback/41-VALIDATION.md

key-decisions:
  - "Wave 0 remains RED-only: no production Momoiro readback dispatch, schema, controller mapping, mapper, migration, or proto code was implemented."
  - "Future Momoiro favorite rows are modeled with an explicit DisplayOrder column and SongNo tie-breaker expectation."
  - "Crown RED coverage uses a 380-entry Momoiro catalog with raw high song ID 777 at ordinal 379 to force catalog-order packing."

patterns-established:
  - "Compile-safe Momoiro RED tests seed future tables through raw SQLite instead of referencing not-yet-created domain entities or DbSets."
  - "Controller RED tests assert response objects and generated wire fields, not source strings or route attributes."

requirements-completed: [MORDB-01, MORDB-02, MORDB-03, MORDB-04, MORDB-05]
requirements-note: "Plan-level RED contracts are complete; runtime MORDB requirements remain pending for later Phase 41 implementation plans."

duration: 14min
completed: 2026-06-26
status: complete
---

# Phase 41 Plan 01: Wave 0 Readback Tests Summary

**Momoiro RED contracts for identity, userdata, self-best, release flags, favorites/recent ordering, and 475-byte catalog-order crown readback**

## Performance

- **Duration:** 14 min
- **Started:** 2026-06-26T08:01:33Z
- **Completed:** 2026-06-26T08:15:12Z
- **Tasks:** 3/3
- **Files modified:** 5 implementation/validation files plus this summary

## Accomplishments

- Added a Momoiro handler fixture with in-memory SQLite, a 380-entry Momoiro test catalog, and raw SQL future-table helpers for `UserSaveData_Momoiro`, `SongBestDatum_Momoiro`, `MomoiroFavoriteSongs`, and `MomoiroRecentSongs`.
- Added handler RED tests for shared identity plus Momoiro-owned save creation, BAID new-user behavior, self-best normal/ura/shin rows, favorite/recent caps/order, release flags, and no adjacent-era leakage.
- Added controller and crown RED tests for populated Momoiro wire responses, compact release flags, 475-byte `HashCrownFlg`, and high raw song IDs packed by catalog ordinal.
- Updated `41-VALIDATION.md` so Wave 0 rows `41-W0-01` through `41-W0-05` point to plan `41-01` and the new test files.

## Task Commits

1. **Task 1: Add handler and schema RED contracts** - `a317d8f7` (`test`)
2. **Task 2: Add controller and crown RED contracts** - `19aadd73` (`test`)
3. **Task 3: Record Wave 0 validation ownership** - `afaf5510` (`docs`)

## Files Created/Modified

- `Tests/Momoiro/MomoiroHandlerFixture.cs` - In-memory SQLite fixture, raw future Momoiro tables, seed helpers, and 380-entry test catalog.
- `Tests/Momoiro/MomoiroReadbackHandlerTests.cs` - Application handler RED contracts for MORDB-01, MORDB-02, MORDB-03, and MORDB-05.
- `Tests/Momoiro/MomoiroControllerReadbackTests.cs` - Direct controller RED contracts for BAID, MyDon, userdata, and self-best wire responses.
- `Tests/Momoiro/MomoiroCrownReadbackTests.cs` - Dedicated userdata-owned crown byte length and high-song-ID catalog-order RED contracts.
- `.planning/phases/41-momoiro-identity-userdata-self-best-and-crown-readback/41-VALIDATION.md` - Wave 0 validation ownership and expected RED status.

## Verification

| Command | Result |
|---------|--------|
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroReadbackHandlerTests" --no-restore -- RunConfiguration.DisableParallelization=true` | RED as expected: compiled, then 4/4 tests failed on `Unsupported era: Momoiro` handler dispatch. |
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroControllerReadbackTests|FullyQualifiedName~MomoiroCrownReadbackTests|FullyQualifiedName~MomoiroRouteSurfaceTests" --no-restore -- RunConfiguration.DisableParallelization=true` | RED as expected: compiled, 6 new readback tests failed on scaffold-only omitted fields, and 5 existing route-surface tests passed. |
| `rg -n "41-01|MomoiroReadbackHandlerTests.cs|MomoiroControllerReadbackTests.cs|MomoiroCrownReadbackTests.cs|red|RED" .planning/phases/41-momoiro-identity-userdata-self-best-and-crown-readback/41-VALIDATION.md` | PASS: validation rows reference plan `41-01`, new test files, and RED status. |
| `git status --porcelain -- proto\momoiro` | PASS: no proto changes. |
| `git diff --cached --name-only -- Host/.gitignore` | PASS: `Host/.gitignore` was not staged. |

## Decisions Made

- Used raw SQLite helpers for future Momoiro-owned tables so tests compile before Momoiro entity and DbSet symbols exist.
- Kept the RED failure mode at public seams: handler dispatch currently reports unsupported Momoiro readback, and controllers currently return scaffold-only responses.
- Preserved manual-only caveats from validation: favorite display order source and cabinet/RPCS3 acceptance are not claimed by this automated Wave 0 work.

## Deviations from Plan

### Workflow-Scope Adjustment

**1. [Scope Boundary] Skipped `requirements.mark-complete` for MORDB runtime requirements**
- **Found during:** Plan closeout
- **Issue:** Standard GSD closeout can mark frontmatter requirements complete, but this plan is explicitly RED-only and does not implement Momoiro runtime readback.
- **Adjustment:** Updated plan progress and session metadata, but left `.planning/REQUIREMENTS.md` MORDB checkboxes pending for implementation plans.
- **Files modified:** None for requirements.
- **Verification:** `.planning/REQUIREMENTS.md` has no diff from this plan.

**Total deviations:** 0 auto-fixed; 1 workflow-scope adjustment.
**Impact on plan:** RED contracts and validation ownership are complete without overclaiming runtime behavior.

## Known Stubs

None. The new RED tests intentionally fail because production Momoiro readback behavior is missing; no placeholder production behavior was added.

## Threat Flags

None. This plan added tests and planning validation rows only; no production endpoint, auth path, file access, or runtime schema surface was introduced.

## TDD Gate Compliance

Wave 0 was explicitly scoped to RED tests only by the user. RED commits exist (`a317d8f7`, `19aadd73`); no GREEN commit is expected for plan `41-01`.

## Issues Encountered

None. Expected RED failures were limited to missing Momoiro dispatch/readback behavior and scaffold-only controller fields.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for `41-02`: persistence and EF migration can implement the future Momoiro tables shaped by the RED contracts, including `MomoiroFavoriteSongs.DisplayOrder`.

## Self-Check: PASSED

- Created files exist on disk.
- Task commits found: `a317d8f7`, `19aadd73`, `afaf5510`.
- `Host/.gitignore` remains outside this plan's staged/committed files.

---
*Phase: 41-momoiro-identity-userdata-self-best-and-crown-readback*
*Completed: 2026-06-26*
