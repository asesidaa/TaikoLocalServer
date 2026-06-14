---
phase: 21-older-ac15-challengecompe-capability-and-red-binding
plan: 03
subsystem: runtime
tags: [red, ac15, challengecompe, rewards, userdata]

requires:
  - phase: 21-01
    provides: ChallengeCompe evidence gate, sidecar catalog, and configured reward metadata
  - phase: 21-02
    provides: Red-owned ChallengeCompe raw facts, progress rows, and completion evaluation
provides:
  - Immediate configured ChallengeCompe song and title reward grants for Red
  - Active unearned ChallengeCompe reward-song locks through Red userdata
  - Focused regression coverage for grants, idempotence, locks, and reward route non-participation
affects: [phase-21, red, challengecompe, ac15-userdata]

tech-stack:
  added: []
  patterns:
    - Transport-agnostic ChallengeCompe reward decisions in Application/Ac15/ChallengeCompe
    - Red-owned save mutation through Ac15UnlockFlagAccess.Red
    - Challenge reward-song locks reuse Ac15UserDataService locked-song behavior

key-files:
  created:
    - Application/Ac15/ChallengeCompe/Ac15ChallengeCompeRewardDecisions.cs
  modified:
    - Application/Handlers/UpdatePlayResultCommand.Red.cs
    - Application/Handlers/UserDataQuery.Red.cs
    - Application/Ac15/RedAc15UserDataAdapter.cs
    - Tests/Red/RedChallengeCompeTests.cs

key-decisions:
  - "Configured ChallengeCompe rewards grant immediately from active completed-task thresholds and mutate only Red release-song/title flags."
  - "ChallengeCompe reward-song locks are derived from active configured reward songs for opted-in Red users and cleared by the existing AC15 userdata locked-song path."
  - "Red rewardcardcheck.php and rewardexecution.php remain compatibility-only and do not participate in ChallengeCompe reward grants."

patterns-established:
  - "Shared ChallengeCompe reward decision code stays transport-agnostic; Red handler code owns concrete Red save mutation."
  - "Red userdata receives ChallengeCompe locks as LockedSongIds input instead of adding wire/controller lock code."

requirements-completed: [RCHAL-02, D-10, D-11, D-18, D-19, D-20, D-21, D-22, D-23]

duration: 16 min
completed: 2026-06-14
---

# Phase 21 Plan 03: ChallengeCompe Rewards and Red Userdata Locks Summary

**Configured Red ChallengeCompe rewards now grant Red unlock flags and hide active unearned reward songs through userdata.**

## Performance

- **Duration:** 16 min
- **Started:** 2026-06-14T14:25:47Z
- **Completed:** 2026-06-14T14:41:00Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments

- Added shared ChallengeCompe reward decision logic for active completed-task thresholds and active unearned reward-song locks.
- Mutated Red `ReleaseSongFlg` and `TitleFlg` immediately when configured ChallengeCompe rewards are earned.
- Fed active unearned reward-song IDs into Red userdata through `LockedSongIds`, preserving the existing AC15 release-flag clearing path.
- Added focused SQLite/controller tests for song rewards, title rewards, idempotent replay, disabled/inactive no-grants, Red-only save mutation, compatibility-route non-participation, and userdata locks.

## Task Commits

1. **Task 1: Grant configured song/title rewards** - `e30a9d21` (feat)
2. **Task 2: Lock active configured reward songs in Red userdata** - `0678c624` (feat)

**Plan metadata:** pending at summary creation.

## Files Created/Modified

- `Application/Ac15/ChallengeCompe/Ac15ChallengeCompeRewardDecisions.cs` - Shared reward grant and reward-song lock decisions.
- `Application/Handlers/UpdatePlayResultCommand.Red.cs` - Applies configured ChallengeCompe reward grants after Red progress updates.
- `Application/Handlers/UserDataQuery.Red.cs` - Computes active unearned ChallengeCompe reward-song locks for opted-in Red users.
- `Application/Ac15/RedAc15UserDataAdapter.cs` - Accepts optional locked song IDs for Red userdata snapshots.
- `Tests/Red/RedChallengeCompeTests.cs` - Focused grant, lock, idempotence, no-participation, and no-cross-era coverage.

## Decisions Made

- Immediate reward grant follows D-18; no next-day timing simulation was added.
- Earned reward songs are represented by the Red release-song flag. Active configured reward songs remain locked only while that flag is absent.
- Reward compatibility routes stay outside ChallengeCompe authority; reward behavior is reached through playresult progress and userdata readback only.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## Verification

| Command | Result |
|---------|--------|
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedChallengeCompe"` | Passed: 15 passed, 0 failed |
| `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` | Task 1 build passed: 0 warnings, 0 errors |
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedChallengeCompe|FullyQualifiedName~Ac15UserDataService"` | Passed: 20 passed, 0 failed |
| `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` | Task 2 build passed: 0 warnings, 0 errors |
| `rg -n "LockedSongIds|GetLockedRewardSongIds" Adapters.GameProtocol.Red` | Passed: no matches, so locks remain outside Red wire/controller code |
| `rg -n "TODO|FIXME|placeholder|coming soon|not available" [touched files]` | Passed: no matches |

## Known Stubs

None.

## Threat Flags

None. The reward mutation, reward lock, and compatibility-route boundaries were planned in T-21-03-01 through T-21-03-03.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for `21-04` to bind Red `challengecompe.php` readback to the Red-owned progress state. Reward grants and userdata locks are now observable through existing Red save/userdata behavior.

## Self-Check: PASSED

- Summary file exists.
- Created reward decision helper exists.
- Task commits found: `e30a9d21`, `0678c624`.

---
*Phase: 21-older-ac15-challengecompe-capability-and-red-binding*
*Completed: 2026-06-14*
