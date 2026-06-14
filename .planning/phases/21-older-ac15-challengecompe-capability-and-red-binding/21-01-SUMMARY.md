---
phase: 21-older-ac15-challengecompe-capability-and-red-binding
plan: 01
subsystem: catalog
tags: [red, ac15, challengecompe, catalog, sidecar, evidence]

requires:
  - phase: 20
    provides: Red runtime state and preserved ChallengeCompe playresult facts without stateful ChallengeCompe mutation
  - phase: 20.1
    provides: AC15 mapper boundary cleanup and ChallengeCompe deferred-state audit
provides:
  - ChallengeCompe evidence gate separating product context from local protocol authority
  - Transport-agnostic shared older-AC15 ChallengeCompe catalog and rule records
  - Red ChallengeCompe sidecar loading through IRedCatalog
  - Red server-authored sidecar output copy support and schema/parser coverage
affects: [phase-21, red, challengecompe, older-ac15-catalog]

tech-stack:
  added: []
  patterns:
    - Red-owned JSON sidecar parsed into shared transport-agnostic AC15 records
    - Unknown ChallengeCompe rule kinds fail schema validation instead of entering runtime evaluation

key-files:
  created:
    - .planning/phases/21-older-ac15-challengecompe-capability-and-red-binding/21-CHALLENGECOMPE-EVIDENCE.md
    - Application/Ac15/ChallengeCompe/Ac15ChallengeCompeCatalog.cs
    - Host/wwwroot/data/red/red_challenge_compe_data.json
    - Infrastructure/GameDataCatalog/Ac15/Ac15ChallengeCompeLoader.cs
    - Tests/Red/RedChallengeCompeCatalogTests.cs
  modified:
    - Application/Abstractions/IRedCatalog.cs
    - Infrastructure/GameDataCatalog/Red/RedEraGameDataCatalog.cs
    - Host/Host.csproj
    - Tests/Red/RedHandlerFixture.cs
    - Tests/Red/RedInitialDataProtocolTests.cs

key-decisions:
  - "ChallengeCompe public wiki/blog facts are product context only; local Red proto, route, mapper, data, runtime, and client evidence are implementation authority."
  - "Plan 21-01 established the typed Red sidecar catalog contract; empty success remains compatibility, not stateful ChallengeCompe support."
  - "Unknown ChallengeCompe rule types fail schema validation and cannot enter progress behavior."

patterns-established:
  - "Shared ChallengeCompe catalog records contain no Red route, Red wire, Red table, or transport vocabulary."
  - "Red sidecar data is committed under Host/wwwroot/data/red and exposed through IRedCatalog."

requirements-completed: [RCOMP-02, RCHAL-01, RCHAL-02, D-07, D-08, D-09, D-10, D-11, D-23, D-24, D-25, D-26]

duration: 14 min
completed: 2026-06-14
---

# Phase 21 Plan 01: ChallengeCompe Evidence Gate and Red Catalog Contract Summary

**Evidence-gated older-AC15 ChallengeCompe catalog contract with Red sidecar loading.**

## Performance

- **Duration:** 14 min
- **Started:** 2026-06-14T13:31:16Z
- **Completed:** 2026-06-14T13:45:48Z
- **Tasks:** 3
- **Files modified:** 10

## Accomplishments

- Recorded the ChallengeCompe evidence gate and stateful execution verdict in `21-CHALLENGECOMPE-EVIDENCE.md`.
- Added shared AC15 ChallengeCompe catalog records for disabled/no-active config, monthly bundles, 10 personal tasks, optional community metadata, typed predicates, and reward thresholds.
- Bound Red to the committed sidecar through `IRedCatalog`, with Host output copy support and focused parser/schema tests for disabled, active, legacy-field, and invalid-rule configs.

## Task Commits

1. **Task 1: Record ChallengeCompe evidence and execution gate** - `d66a5c01` (docs)
2. **Task 2: Add transport-agnostic catalog and rule records** - `435b0c68` (feat)
3. **Task 3: Bind Red sidecar loading and publish output** - `472c7ecd` (feat)

**Plan metadata:** pending at summary creation.

## Files Created/Modified

- `.planning/phases/21-older-ac15-challengecompe-capability-and-red-binding/21-CHALLENGECOMPE-EVIDENCE.md` - Evidence authority split and stateful implementation gate.
- `Application/Ac15/ChallengeCompe/Ac15ChallengeCompeCatalog.cs` - Shared transport-agnostic ChallengeCompe catalog, task, rule, and reward records.
- `Infrastructure/GameDataCatalog/Ac15/Ac15ChallengeCompeLoader.cs` - JSON sidecar parser with JSON Schema validation plus disabled/no-active behavior.
- `Application/Abstractions/IRedCatalog.cs` - Red catalog exposes `ChallengeCompe`.
- `Infrastructure/GameDataCatalog/Red/RedEraGameDataCatalog.cs` - Loads `red_challenge_compe_data.json` through `PathHelper.GetDataPath(GameEra.Red)`.
- `Host/wwwroot/data/red/red_challenge_compe_data.json` - Red ChallengeCompe sidecar.
- `Host/Host.csproj` - Copies Red server-authored sidecars, including the ChallengeCompe sidecar, to build output.
- `Tests/Red/RedChallengeCompeCatalogTests.cs` - Parser/schema coverage for disabled, valid active, committed sidecar, legacy field rejection, and unknown rule configs.
- `Tests/Red/RedHandlerFixture.cs` - Test Red catalog default ChallengeCompe property.
- `Tests/Red/RedInitialDataProtocolTests.cs` - Fake Red catalog default ChallengeCompe property.

## Decisions Made

- Public DonChare pages remain product context only; endpoint, payload, response, state, and reward behavior require local evidence.
- ChallengeCompe remains config-gated; stateful progress/reward/readback behavior is owned by later Phase 21 plans and must stay within the evidence gate.
- Unknown rule kinds and legacy ambiguous rule fields fail JSON Schema validation, preventing accidental progress execution.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Updated Red fake catalog after extending IRedCatalog**
- **Found during:** Task 3 (Bind Red sidecar loading and publish output)
- **Issue:** Focused test compilation failed because `RedInitialDataProtocolTests.FakeRedCatalog` did not implement the new `IRedCatalog.ChallengeCompe` property introduced by this task.
- **Fix:** Added a disabled default `Ac15ChallengeCompeCatalog` property to the fake catalog.
- **Files modified:** `Tests/Red/RedInitialDataProtocolTests.cs`
- **Verification:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedChallengeCompeCatalogTests"` passed with 4 tests.
- **Committed in:** `472c7ecd`

**Total deviations:** 1 auto-fixed (Rule 3 blocking)
**Impact on plan:** Required interface fixture update only; no scope expansion.

## Issues Encountered

- The first focused test run failed at compile time on the stale fake catalog. The issue was fixed before proceeding.

## Verification

| Command | Result |
|---------|--------|
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedChallengeCompeCatalogTests"` | Passed: 4 passed, 0 failed |
| `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` | Passed: 0 warnings, 0 errors |
| `Test-Path "$env:TEMP\TaikoLocalServer-host-build\wwwroot\data\red\red_challenge_compe_data.json"` | Passed: `True` |
| `rg -n "Red\|Wire\|Route\|DbSet\|ChallengeCompeController\|taiko\.proto" Application/Ac15/ChallengeCompe` | Passed: no matches, proving the shared model has no Red/wire/route/table coupling |

## Known Stubs

None. `red_challenge_compe_data.json` is the Red sidecar data contract; active execution remains controlled by era config and the evidence-gated runtime plans.

## Threat Flags

None. The planned sidecar JSON trust boundary is mitigated by JSON Schema validation and typed parsing.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for `21-02` to add Red-owned persistence and progress evaluation if it stays within the `21-CHALLENGECOMPE-EVIDENCE.md` gate. Empty `challengecompe.php` success remains compatibility-only until later plans implement stateful behavior.

## Self-Check: PASSED

- Created summary exists.
- Evidence artifact exists.
- Shared catalog records exist.
- ChallengeCompe loader exists.
- Red ChallengeCompe sidecar exists.
- Task commits found: `d66a5c01`, `435b0c68`, `472c7ecd`.

---
*Phase: 21-older-ac15-challengecompe-capability-and-red-binding*
*Completed: 2026-06-14*
