---
phase: 07-tokkun-evidence-contract-and-guardrail-reset
plan: 01
subsystem: planning
tags: [blue, tokkun, evidence-contract, source-guards, tests]
requires:
  - phase: v1.0-blue-support
    provides: Blue normal and battle runtime support plus archived evidence gates
provides:
  - Blue Tokkun evidence contract with proven, observed, deliberately ignored, and unknown/blocked rows
  - Classifier policy that keeps PlayMode.Tokkun numeric value unknown
  - Guard reset that removes the stale Tokkun word-scan test without replacement
affects: [phase-08-banacoin, phase-09-tokkun-mapper, phase-10-tokkun-state, phase-11-smoke]
tech-stack:
  added: []
  patterns: [battle-style evidence matrix, behavior-based guard policy, archive-aware planning tests]
key-files:
  created:
    - .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-01-TOKKUN-EVIDENCE-CONTRACT.md
  modified:
    - Tests/Blue/BlueA4SourceGuardTests.cs
    - Tests/Blue/BlueBattleEvidenceGateTests.cs
    - Tests/Blue/BlueBattleRequirementTests.cs
    - Tests/Blue/BlueBattleSourceGuardTests.cs
    - Tests/Blue/BlueDocsTests.cs
key-decisions:
  - "Phase 7 remains documentation and guard reset only; runtime Tokkun behavior is deferred to owning phases."
  - "Blue Tokkun classification must not depend on a guessed numeric PlayMode.Tokkun value."
  - "Tokkun safety moves to behavior-based runtime tests in later phases, not replacement word scans."
patterns-established:
  - "Tokkun evidence rows use the locked statuses proven, observed, deliberately ignored, and unknown/blocked."
  - "Archived v1.0 Blue battle evidence tests resolve milestone archive paths instead of live active-phase paths."
requirements-completed: [TKEV-01, TKEV-02, TKEV-03]
duration: 10 min
completed: 2026-06-03
---

# Phase 07 Plan 01: Tokkun Evidence Contract Summary

**Blue Tokkun evidence contract plus stale source-guard reset, with archive-aware Blue regression tests restored**

## Performance

- **Duration:** 10 min
- **Started:** 2026-06-03T18:40:46Z
- **Completed:** 2026-06-03T18:50:08Z
- **Tasks:** 2 completed
- **Files modified:** 6

## Accomplishments

- Created `07-01-TOKKUN-EVIDENCE-CONTRACT.md` with the required Tokkun row matrix, classifier contract, no-runtime-write targets, follow-on gates, and D-01 through D-17 coverage.
- Removed `BluePlayResultCode_DoesNotPromoteTokkunFieldsIntoRuntimeSemantics` from `BlueA4SourceGuardTests` without adding a replacement Tokkun word scan.
- Fixed stale v1.0 archive/doc test assumptions uncovered by the Blue-focused gate so archived battle evidence guards remain executable after milestone closeout.

## Task Commits

1. **Task 1: Create the Tokkun evidence contract** - `7aba0c74` (docs)
2. **Task 2: Remove the stale Tokkun source guard** - `58c2bb74` (test)
3. **Deviation: Align stale archive/doc guards with v1.0 closeout** - `e6cb2f00` (test)

Plan metadata will be committed with this summary and the GSD tracking updates.

## Files Created/Modified

- `.planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-01-TOKKUN-EVIDENCE-CONTRACT.md` - Phase 7 Tokkun evidence contract and handoff gates.
- `Tests/Blue/BlueA4SourceGuardTests.cs` - Keeps Green leakage guards and removes the stale Tokkun word-scan method.
- `Tests/Blue/BlueBattleEvidenceGateTests.cs` - Reads archived Phase 4/5 evidence from `.planning/milestones/v1.0-phases`.
- `Tests/Blue/BlueBattleRequirementTests.cs` - Reads archived v1.0 requirements and Phase 5 verification artifacts.
- `Tests/Blue/BlueBattleSourceGuardTests.cs` - Reads the archived Phase 5 resolution matrix.
- `Tests/Blue/BlueDocsTests.cs` - Matches current README wording for Blue setup and battle runtime data.

## Decisions Made

- Followed Phase 7 scope exactly for runtime behavior: no Tokkun DTO, mapper, handler, route, persistence, migration, wire, Banacoin, or cabinet/RPCS3 proof changes were made.
- Treated the Blue-focused test failure as a blocking verification drift issue because it came from archived planning paths and stale README wording, not from product behavior.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Updated stale v1.0 archive and README guard tests**
- **Found during:** Post-wave Blue-focused gate.
- **Issue:** `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~Blue` failed because older Blue battle tests still read `.planning/phases/04...` and `.planning/phases/05...` after those artifacts were archived under `.planning/milestones/v1.0-phases`. README tests also asserted old "Track B" and "Blue AC15 Test Support" wording after battle runtime shipped.
- **Fix:** Updated the tests to read archived v1.0 planning artifacts and to assert the current Blue setup/battle runtime README text.
- **Files modified:** `Tests/Blue/BlueBattleEvidenceGateTests.cs`, `Tests/Blue/BlueBattleRequirementTests.cs`, `Tests/Blue/BlueBattleSourceGuardTests.cs`, `Tests/Blue/BlueDocsTests.cs`.
- **Verification:** The previously failing test group passed, then the full Blue-focused gate passed.
- **Committed in:** `e6cb2f00`.

---

**Total deviations:** 1 auto-fixed blocking test-drift issue.
**Impact on plan:** Verification-only scope expansion. No runtime behavior changed.

## Issues Encountered

- Initial Blue-focused gate failed with 10 stale archive/doc expectation failures. The focused Phase 7 checks had already passed. The failures were fixed as a deviation and reverified.

## Verification

- `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueA4SourceGuardTests` - passed, 2 tests.
- Contract term/source checks for required Phase 7 terms and no replacement Tokkun source-scan guard - passed.
- `Domain/Enums/PlayMode.cs` check for absent `Tokkun` enum member - passed.
- `Adapters.GameProtocol.Blue/Controllers/GetBanacoinInfoController.cs` absence check - passed.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueBattleEvidenceGateTests|FullyQualifiedName~BlueBattleRequirementTests|FullyQualifiedName~BlueBattleSourceGuardTests|FullyQualifiedName~BlueDocsTests"` - passed, 14 tests.
- `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~Blue` - passed, 236 tests.
- `dotnet test Tests/Tests.csproj` - passed, 617 tests.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Phase 8 can start from the contract's Banacoin route handoff. `getbanacoininfo.php` remains unknown/blocked, real Banacoin state remains out of scope, and any required Banacoin-adjacent route behavior must be evidence-backed and stateless.

---
*Phase: 07-tokkun-evidence-contract-and-guardrail-reset*
*Completed: 2026-06-03*
