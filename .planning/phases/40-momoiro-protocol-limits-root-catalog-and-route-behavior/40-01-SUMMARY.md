---
phase: 40-momoiro-protocol-limits-root-catalog-and-route-behavior
plan: "01"
subsystem: testing
tags: [momoiro, ac15, catalog, protocol-limits, route-surface, red-tests]

requires:
  - phase: 39-momoiro-evidence-and-era-foundation
    provides: Momoiro era identity, generated wire, adapter route scaffolds, and route-surface evidence
provides:
  - Wave 0 RED contracts for Momoiro root catalog loading and required files
  - Wave 0 RED contracts for Momoiro AC15 profile limits and userdata-owned crown placement
  - Wave 0 RED contracts for Momoiro metadata route behavior and unsupported route absence
affects: [phase-40-production-plans, momoiro-catalog, momoiro-metadata-routes, ac15-profile]

tech-stack:
  added: []
  patterns:
    - Compile-safe RED tests target GameEra.Momoiro, IGameDataCatalog.For, IEraGameDataCatalog, and existing controllers
    - Low-confidence Momoiro constants are labeled in assertion messages instead of overclaimed as native proof

key-files:
  created:
    - Tests/Momoiro/MomoiroCatalogLoaderTests.cs
    - Tests/Momoiro/MomoiroProtocolLimitsTests.cs
    - Tests/Momoiro/MomoiroMetadataRouteTests.cs
    - .planning/phases/40-momoiro-protocol-limits-root-catalog-and-route-behavior/40-01-SUMMARY.md
  modified:
    - Tests/Momoiro/MomoiroRouteSurfaceTests.cs
    - Tests/Ac15/Ac15RecommendQueryHandlerTests.cs
    - .planning/phases/40-momoiro-protocol-limits-root-catalog-and-route-behavior/40-VALIDATION.md

key-decisions:
  - "Wave 0 remains RED-only: no production code was added for catalog, profile, handler, or controller behavior."
  - "Momoiro catalog tests use current IGameDataCatalog/IEraGameDataCatalog seams plus reflection for future catalog output values, avoiding future concrete symbols."
  - "STATE/ROADMAP/REQUIREMENTS closeout updates were skipped because the user explicitly limited work and commits to the plan file set plus SUMMARY."

patterns-established:
  - "Momoiro RED contracts should fail on missing registration, missing Ac15EraProfiles.TryGet support, or scaffolded metadata responses, not missing future symbols."

requirements-completed: [MOCAT-01, MOCAT-02, MOCAT-03, MOCAT-04, MOCAT-05]

duration: 15 min
completed: 2026-06-25
status: complete
---

# Phase 40 Plan 01: Wave 0 RED Test Contracts Summary

**Momoiro root catalog, AC15 profile, metadata route, and route absence RED contracts without production implementation.**

## Performance

- **Duration:** 15 min
- **Started:** 2026-06-25T22:48:47Z
- **Completed:** 2026-06-25T23:03:29Z
- **Tasks:** 3/3
- **Files modified:** 7

## Accomplishments

- Added Momoiro catalog loader RED tests for root-level required files, 380 parsed music rows, `song_hash_ver` `538116869`, a 380-entry hash table, and 760 encoded bytes.
- Added Momoiro protocol limit RED tests for explicit profile lookup, userdata-owned crowns, unsupported feature flags, favorite/recent caps, 128-byte song flags, and the low-confidence 475-byte crown contract.
- Added Momoiro metadata route RED tests for recommendation, default-song, song-hash, telop behavior, static heartbeat/bookkeeping success, and extended route-surface absence coverage for `crownsdata.php`.
- Updated Phase 40 validation ownership rows for `40-W0-01` through `40-W0-05`.

## Task Commits

1. **Task 1: Add root catalog and profile RED tests** - `8efe14ec` (test)
2. **Task 2: Add metadata route and absence RED tests** - `c6643865` (test)
3. **Task 3: Record Wave 0 validation ownership** - `8330b085` (docs)

## Files Created/Modified

- `Tests/Momoiro/MomoiroCatalogLoaderTests.cs` - RED catalog registration/loading and required-file tests.
- `Tests/Momoiro/MomoiroProtocolLimitsTests.cs` - RED Momoiro AC15 profile and protocol limit tests.
- `Tests/Momoiro/MomoiroMetadataRouteTests.cs` - RED controller/handler tests for catalog-backed metadata behavior and static operational routes.
- `Tests/Momoiro/MomoiroRouteSurfaceTests.cs` - Adds explicit dedicated crown-route absence guard.
- `Tests/Ac15/Ac15RecommendQueryHandlerTests.cs` - Adds Momoiro to recommendation handler and reserved-medley filtering matrices.
- `.planning/phases/40-momoiro-protocol-limits-root-catalog-and-route-behavior/40-VALIDATION.md` - Records 40-01 ownership and RED handoff status.

## Verification

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroCatalogLoaderTests|FullyQualifiedName~MomoiroProtocolLimitsTests" --no-restore`
  - **Result:** RED as expected, compiled successfully; 5 failures from missing Momoiro catalog registration and missing `Ac15EraProfiles.TryGet(GameEra.Momoiro)` support.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroMetadataRouteTests|FullyQualifiedName~MomoiroRouteSurfaceTests|FullyQualifiedName~Ac15RecommendQueryHandlerTests" --no-restore`
  - **Result:** RED as expected, compiled successfully; 20 passed and 7 failed from missing Momoiro recommendation/telop dispatch and scaffolded metadata response fields.
- `rg -n "40-01|MomoiroCatalogLoaderTests.cs|MomoiroProtocolLimitsTests.cs|MomoiroMetadataRouteTests.cs|RED|red" .planning/phases/40-momoiro-protocol-limits-root-catalog-and-route-behavior/40-VALIDATION.md`
  - **Result:** PASS.
- `git status --porcelain -- proto\momoiro`
  - **Result:** PASS, no output.

## Decisions Made

- Kept this plan RED-only per the Wave 0 plan and user instruction; no production code, proto, generated wire, or route implementation files were edited.
- Used reflection only in catalog tests to inspect future Momoiro catalog output values while avoiding compile-time references to future symbols such as `IMomoiroCatalog` or `MomoiroEraGameDataCatalog`.
- Kept favorite max, crown byte length, and related constants labeled as low-confidence/inferred contracts in assertion messages.

## Deviations from Plan

### Auto-fixed Issues

None.

### Workflow-Scope Adjustment

**1. [Scope Boundary] Skipped standard STATE/ROADMAP/REQUIREMENTS updates**
- **Found during:** Plan closeout
- **Issue:** Standard GSD closeout normally updates `.planning/STATE.md`, `.planning/ROADMAP.md`, and `.planning/REQUIREMENTS.md`.
- **Adjustment:** The user explicitly constrained this execution to the plan's files plus required `SUMMARY.md`, so those broader planning files were not modified or committed.
- **Files modified:** None
- **Verification:** `git status --short` shows only the pre-existing unrelated `Host/.gitignore` after task commits before summary creation.

**Total deviations:** 0 auto-fixed; 1 explicit workflow-scope adjustment.
**Impact on plan:** Plan artifacts and RED tests are complete. Project-level progress files remain for the orchestrator or a later explicit closeout to update.

## Issues Encountered

- Initial required-file RED test called `IGameDataCatalog.InitializeAsync()` before resolving the Momoiro era. Because Momoiro is not registered yet, that did not exercise the missing-file contract. The test was corrected to resolve `IGameDataCatalog.For(GameEra.Momoiro)` first, so current RED failure is missing registration and future failure becomes missing required file validation.
- An initial stub scan command was too broad due shell handling of regex alternation. It was rerun with `Select-String` over only the plan files and found no TODO/FIXME/placeholder text.

## Known Stubs

None in plan-created or plan-modified files. Empty arrays in tests are explicit expected protocol/test values, not UI/data-source stubs.

## Threat Flags

None - this plan added tests and validation documentation only. No new network endpoints, auth paths, file access production paths, or trust-boundary code were introduced.

## TDD Gate Compliance

This plan is a Wave 0 RED-only TDD plan. It intentionally has `test(...)` commits and no `feat(...)` GREEN commit because the plan and user instruction both prohibit production code in this wave.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for Phase 40 production plans to make these RED contracts pass by adding Momoiro catalog registration/loading, explicit profile limits, catalog-backed metadata handlers/controllers, and route behavior without expanding unsupported feature surfaces.

## Self-Check: PASSED

- Created/modified files exist in the committed task history.
- Task commits exist: `8efe14ec`, `c6643865`, `8330b085`.
- `proto/momoiro` remained untouched.

---
*Phase: 40-momoiro-protocol-limits-root-catalog-and-route-behavior*
*Completed: 2026-06-25*
