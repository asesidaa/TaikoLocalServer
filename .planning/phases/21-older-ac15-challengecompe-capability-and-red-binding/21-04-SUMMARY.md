---
phase: 21-older-ac15-challengecompe-capability-and-red-binding
plan: 04
subsystem: protocol
tags: [red, ac15, challengecompe, readback, mapperly]

requires:
  - phase: 21-02
    provides: Red-owned ChallengeCompe progress rows
  - phase: 21-03
    provides: Red ChallengeCompe reward grants and userdata locks
provides:
  - Red ChallengeCompe query readback from active progress rows
  - Red challengecompe.php Mediator binding
  - Source-generated Red ChallengeCompe wire mapper
  - Focused handler/controller/mapper regression coverage
affects: [phase-21, red, challengecompe, older-ac15-readback]

tech-stack:
  added: []
  patterns:
    - Red controller calls Mediator and maps application DTOs back to Red wire DTOs
    - Red ChallengeCompe mapper uses Mapperly MapProperty projections only

key-files:
  created:
    - Application/Handlers/GetChallengeCompeQuery.Red.cs
    - Adapters.GameProtocol.Red/Mappers/ChallengeCompeMappers.cs
  modified:
    - Application/Handlers/GetChallengeCompeQuery.cs
    - Adapters.GameProtocol.Red/Controllers/ChallengeCompeController.cs
    - Tests/Red/RedChallengeCompeTests.cs
    - Tests/Red/RedProtocolMapperTests.cs

key-decisions:
  - "Red ChallengeCompe readback is read-only: it does not opt users in, grant rewards, or echo raw upload facts."
  - "Phase 21 readback populates only ary_challenge_stat from active saved progress; user and BNG buckets remain empty."
  - "Red wire projection is Mapperly-generated and mechanical, with business behavior kept in the application query."

patterns-established:
  - "ChallengeCompe active-progress readback filters saved Red progress through the active catalog before building response rows."
  - "Mapperly generated-source inspection is recorded for the new Red ChallengeCompe mapper."

requirements-completed: [RCOMP-02, RCHAL-02, D-04, D-13, D-14, D-15, D-16, D-17, D-23, D-24, D-25, D-26]

duration: 13 min
completed: 2026-06-14
---

# Phase 21 Plan 04: Red ChallengeCompe Readback Route Binding Summary

**Red challengecompe.php now returns active saved DonChare progress through a Mediator query and source-generated Red wire mapper.**

## Performance

- **Duration:** 13 min
- **Started:** 2026-06-14T14:48:15Z
- **Completed:** 2026-06-14T15:00:33Z
- **Tasks:** 2
- **Files modified:** 6

## Accomplishments

- Added Red support to `GetChallengeCompeQuery`, reading only enrolled users' active Red ChallengeCompe progress rows.
- Kept readback side-effect-free: no opt-in mutation, no reward mutation, and no raw fact echo.
- Replaced Red empty-success route behavior with Mediator-backed readback and a Mapperly-generated Red response mapper.
- Added focused tests for query behavior, controller readback, mapper projection, empty user/BNG buckets, and raw-fact omission.

## Task Commits

1. **Task 1: Add Red ChallengeCompe query readback** - `0f850b91` (feat)
2. **Task 2: Bind Red controller and mapper to query result** - `fe536b65` (feat)

**Plan metadata:** pending at summary creation.

## Files Created/Modified

- `Application/Handlers/GetChallengeCompeQuery.cs` - Adds the Red branch and injects Red state/catalog dependencies.
- `Application/Handlers/GetChallengeCompeQuery.Red.cs` - Reads active saved Red progress into `CommonChallengeCompeResponse`.
- `Adapters.GameProtocol.Red/Controllers/ChallengeCompeController.cs` - Calls Mediator and maps Red response output.
- `Adapters.GameProtocol.Red/Mappers/ChallengeCompeMappers.cs` - Mapperly mechanical projection from common response to Red wire response.
- `Tests/Red/RedChallengeCompeTests.cs` - Adds readback/no-mutation/no-raw-echo and controller readback coverage.
- `Tests/Red/RedProtocolMapperTests.cs` - Adds Red ChallengeCompe mapper coverage for active progress and empty unsupported buckets.

## Decisions Made

- Readback requires `UserSaveDataRed.IsChallengeCompe`; it never creates or toggles enrollment.
- `AryChallengeStat` is derived from active catalog tasks plus saved progress rows, not latest raw uploads.
- `AryUserCompeStat` and `AryBngCompeStat` remain empty because Phase 21 still has no evidence for those buckets.
- Mapperly generated source was inspected at `Adapters.GameProtocol.Red/obj/Debug/net10.0/generated/Riok.Mapperly/Riok.Mapperly.MapperGenerator/ChallengeCompeMappers.g.cs`.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## Verification

| Command | Result |
|---------|--------|
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedChallengeCompe"` | Task 1 passed: 20 passed, 0 failed |
| `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` | Task 1 passed: 0 warnings, 0 errors |
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedChallengeCompe|FullyQualifiedName~RedProtocolMapperTests"` | Final passed: 25 passed, 0 failed |
| `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` | Final passed: 0 warnings, 0 errors |
| `dotnet build Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj /p:EmitCompilerGeneratedFiles=true` | Passed: 0 warnings, 0 errors; generated Red ChallengeCompe mapper inspected |
| `rg -n 'TODO|FIXME|placeholder|coming soon|not available|=\[\]|=\{\}|=null|=""' -- [touched files]` | Passed: no matches |

## Known Stubs

None.

## Threat Flags

None. The readback endpoint, unsupported bucket omission, and mapper boundary were planned in T-21-04-01 through T-21-04-03.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for `21-05` to run Phase 21 verification and closeout. Red ChallengeCompe stateful readback is now implemented through the proven DonChare bucket only; community, user, and BNG buckets remain evidence-gated.

## Self-Check: PASSED

- Summary file exists.
- Red query partial exists.
- Red ChallengeCompe mapper exists.
- Task commits found: `0f850b91`, `fe536b65`.

---
*Phase: 21-older-ac15-challengecompe-capability-and-red-binding*
*Completed: 2026-06-14*
