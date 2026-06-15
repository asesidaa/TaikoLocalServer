---
phase: 22-red-adminapi-webui-and-runtime-closeout
plan: 2
subsystem: api
tags: [red, adminapi, don-challenge, challengecompe, ac15]
requires:
  - phase: 21-older-ac15-challengecompe-capability-and-red-binding
    provides: Red ChallengeCompe catalog, Red-owned progress rows, reward grants, and cabinet compatibility boundary
  - phase: 22-red-adminapi-webui-and-runtime-closeout
    provides: Plan 22-01 Red normal AdminApi parity and preserved Red WIP context
provides:
  - Dedicated Don Challenge AdminApi contracts for availability, tasks, tracks, and rewards
  - Red Don Challenge AdminApi read model over active configured bundle, Red progress rows, and Red save reward flags
  - No-opt-in Red ChallengeCompe runtime behavior for progress, reward locks, and reward grants
affects: [phase-22, red-adminapi, don-challenge-webui, challengecompe]
tech-stack:
  added: []
  patterns: [Mediator-backed AdminApi read model, Don Challenge contract vocabulary, no-opt-in ChallengeCompe boundary]
key-files:
  created:
    - Contracts.AdminApi/Responses/DonChallengeAvailabilityResponse.cs
    - Contracts.AdminApi/Responses/DonChallengeResponse.cs
    - Contracts.AdminApi/ViewModels/DonChallengeTask.cs
    - Contracts.AdminApi/ViewModels/DonChallengeTrack.cs
    - Contracts.AdminApi/ViewModels/DonChallengeReward.cs
    - Application/Handlers/GetDonChallengeQuery.cs
    - Adapters.AdminApi/Controllers/DonChallengeController.cs
    - Tests/Red/RedDonChallengeAdminApiTests.cs
  modified:
    - Application/Ac15/ChallengeCompe/Ac15ChallengeCompeProgressEvaluator.cs
    - Application/Ac15/ChallengeCompe/Ac15ChallengeCompeRewardDecisions.cs
    - Application/Handlers/GetChallengeCompeQuery.cs
    - Application/Handlers/GetChallengeCompeQuery.Red.cs
    - Application/Handlers/UpdatePlayResultCommand.Red.cs
    - Application/Handlers/UserDataQuery.Red.cs
    - Tests/Red/RedChallengeCompeTests.cs
key-decisions:
  - "Don Challenge AdminApi uses dedicated JSON contracts and does not expose raw facts, opt-in state, cabinet bucket names, or compatibility diagnostics."
  - "UserSaveDataRed.IsChallengeCompe is not a local gate for Red Don Challenge progress, reward locks, reward grants, AdminApi availability, or WebUI availability."
  - "The cabinet challengecompe.php path remains an empty compatibility boundary and is not reused as the AdminApi/WebUI read model."
  - "DonChallengeController is thin and delegates read-model assembly to an Application Mediator handler to satisfy repo controller-boundary rules."
patterns-established:
  - "Don Challenge availability returns successful unavailable responses for unsupported eras and no-active-bundle Red state."
  - "Reward status is derived from configured active rewards, completed configured tasks, and existing Red save reward flags only."
requirements-completed: [RVER-01, RVER-02]
duration: 11 min
completed: 2026-06-15
---

# Phase 22 Plan 2: Don Challenge AdminApi And No-Opt-In Boundary Summary

**Dedicated Red Don Challenge AdminApi read model with no opt-in gates and cabinet ChallengeCompe kept as a separate compatibility surface**

## Performance

- **Duration:** 11 min
- **Started:** 2026-06-15T05:43:30Z
- **Completed:** 2026-06-15T05:54:03Z
- **Tasks:** 3/3
- **Files modified:** 15

## Accomplishments

- Reconciled the pre-existing Red ChallengeCompe WIP with the Phase 22 correction: `UserSaveDataRed.IsChallengeCompe` no longer gates Red Don Challenge progress, reward locks, or reward grants.
- Added Don Challenge AdminApi contracts under `Contracts.AdminApi` using WebUI/domain vocabulary instead of cabinet wire or database row names.
- Added `GET /api/{era}/DonChallenge/availability` and `GET /api/{era}/DonChallenge/{baid}` through a thin AdminApi controller and Application read-model handler.
- Added focused Red tests for no-opt-in progress/locks, Tokkun no-write behavior, cabinet empty-bucket compatibility, Red Don Challenge readback, reward status, unsupported-era unavailable responses, and no Red fallback for other eras.

## Task Commits

1. **Task 1: Reconcile Current ChallengeCompe WIP** - `7399c3a3` (`fix`)
2. **Task 2: Add Don Challenge Contracts** - `202308e8` (`feat`)
3. **Task 3: Implement Don Challenge AdminApi** - `bc971844` (`feat`)

## Files Created/Modified

- `Contracts.AdminApi/Responses/DonChallengeAvailabilityResponse.cs` - Availability response for navigation/service checks.
- `Contracts.AdminApi/Responses/DonChallengeResponse.cs` - Full Don Challenge readback response.
- `Contracts.AdminApi/ViewModels/DonChallengeTask.cs` - Task card contract with progress and timestamps.
- `Contracts.AdminApi/ViewModels/DonChallengeTrack.cs` - Configured task-track contract.
- `Contracts.AdminApi/ViewModels/DonChallengeReward.cs` - Reward threshold contract and status values.
- `Application/Handlers/GetDonChallengeQuery.cs` - Mediator read model for Red Don Challenge availability and user progress.
- `Adapters.AdminApi/Controllers/DonChallengeController.cs` - Era-routed AdminApi endpoints.
- `Tests/Red/RedDonChallengeAdminApiTests.cs` - Focused AdminApi readback and boundary tests.
- `Application/Ac15/ChallengeCompe/Ac15ChallengeCompeProgressEvaluator.cs` - Evaluates active configured task tracks instead of relying on uploaded bucket ids.
- `Application/Ac15/ChallengeCompe/Ac15ChallengeCompeRewardDecisions.cs` - Removes opt-in from reward-song lock decisions.
- `Application/Handlers/UpdatePlayResultCommand.Red.cs` - Removes opt-in from Red progress/reward mutation while preserving Tokkun early return.
- `Application/Handlers/UserDataQuery.Red.cs` - Applies active unearned reward locks without opt-in gating.
- `Application/Handlers/GetChallengeCompeQuery.cs` and `.Red.cs` - Keeps cabinet ChallengeCompe readback as empty compatibility buckets.
- `Tests/Red/RedChallengeCompeTests.cs` - Updates runtime tests for no-opt-in and cabinet compatibility behavior.

## Decisions Made

- The dedicated AdminApi response is the WebUI read model; cabinet `challengecompe.php` remains a separate compatibility endpoint.
- Red reward status is reported as `Earned`, `Locked`, or `Unavailable`, with `Earned` based on configured task completion or existing Red release/title flags.
- Unsupported eras and Red with no active bundle return successful unavailable contracts rather than falling back to Nijiiro or Red data.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Moved Don Challenge read-model assembly into Application**
- **Found during:** Task 3 (Implement Don Challenge AdminApi)
- **Issue:** The plan listed only a controller, but `AGENTS.md` requires controllers to call Mediator and keep business behavior in Application handlers.
- **Fix:** Added `Application/Handlers/GetDonChallengeQuery.cs`; `DonChallengeController` now only validates era/auth and delegates to Mediator.
- **Files modified:** `Application/Handlers/GetDonChallengeQuery.cs`, `Adapters.AdminApi/Controllers/DonChallengeController.cs`, `Tests/Red/RedDonChallengeAdminApiTests.cs`
- **Verification:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedDonChallenge|FullyQualifiedName~RedChallengeCompe"` passed.
- **Committed in:** `bc971844`

---

**Total deviations:** 1 auto-fixed (1 missing critical)
**Impact on plan:** The endpoint behavior stayed within scope while satisfying the repo controller-boundary rule.

## Issues Encountered

- The repository had pre-existing Red ChallengeCompe WIP in the files listed by `22-PATTERNS.md`. That WIP was inspected, preserved, and reconciled into the Task 1 commit where it belonged to Plan 22-02.
- `Host/wwwroot/data/red/red_telop_data.json` remains intentionally modified from prior Plan 22-01 context and was not staged or committed as part of Plan 22-02.

## Verification

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedDonChallenge|FullyQualifiedName~RedChallengeCompe"` - PASSED, 34 passed, 0 failed, 0 skipped.

## Known Stubs

None - no placeholders or unwired Don Challenge data sources were introduced. The unavailable response messages are intentional API states.

## Threat Flags

None - the new AdminApi surface matches the plan threat model and omits raw facts, opt-in state, cabinet bucket names, wallet/payment/coupon/transaction data, item-shop data, battle data, and compatibility diagnostics.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for Plan 22-03 WebUI work. The WebUI can consume `api/{era}/DonChallenge/availability` for navigation gating and `api/{era}/DonChallenge/{baid}` for the user readback page.

## Self-Check: PASSED

- Created files exist on disk.
- Task commits `7399c3a3`, `202308e8`, and `bc971844` exist in git history.
- Remaining dirty file `Host/wwwroot/data/red/red_telop_data.json` is pre-existing Plan 22-01 WIP and was intentionally not committed in Plan 22-02.

---
*Phase: 22-red-adminapi-webui-and-runtime-closeout*
*Completed: 2026-06-15*
