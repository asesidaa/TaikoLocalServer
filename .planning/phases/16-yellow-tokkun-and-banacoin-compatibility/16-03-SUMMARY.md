---
phase: 16-yellow-tokkun-and-banacoin-compatibility
plan: "03"
subsystem: api
tags: [yellow, ac15, tokkun, userdata, protobuf]
requires:
  - phase: 16-02
    provides: Yellow Tokkun playresult persistence into nullable tutorial state and raw history
provides:
  - Yellow AC15 profile placement for optional Tokkun tutorial userdata readback
  - Yellow wire mapper serialization for optional `tokkun_tutorial_flg`
  - Query and integration tests proving absent/raw/playresult-persisted tutorial readback without history exposure
affects: [yellow, phase-16, userdata, tokkun, ac15]
tech-stack:
  added: []
  patterns: [Yellow optional userdata field mapping, profile-gated AC15 readback, tutorial-only protocol readback]
key-files:
  created: []
  modified:
    - Application/Ac15/Ac15EraProfiles.cs
    - Adapters.GameProtocol.Yellow/Mappers/UserDataMappers.cs
    - Tests/Ac15/Ac15EraProfileTests.cs
    - Tests/Ac15/Ac15UserDataServiceTests.cs
    - Tests/Yellow/YellowUserDataProtocolTests.cs
key-decisions:
  - "Yellow now sets `HasTokkunTutorialFlagInUserData` true while Green remains false and Blue remains true."
  - "Yellow userdata serializes `tokkun_tutorial_flg` only when the common nullable value is present, preserving raw values such as `0`, `1`, and `7`."
  - "Tokkun stage-history facts remain server-side only; userdata exposes no Tokkun summary, stage, song-list, timestamp, or history surface."
patterns-established:
  - "Yellow Tokkun readback follows the same optional-field mapper pattern as Blue without sharing wire DTOs."
  - "Shared AC15 userdata placement stays profile-gated so Green continues omitting Tokkun tutorial."
requirements-completed: [YTOK-03]
duration: 12 min
completed: 2026-06-08
---

# Phase 16 Plan 03: Yellow Tokkun Userdata Summary

**Yellow userdata optional Tokkun tutorial readback through the proven protocol field only**

## Performance

- **Duration:** 12 min
- **Started:** 2026-06-08T15:38:43+08:00
- **Completed:** 2026-06-08T15:50:28+08:00
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments

- Enabled Yellow AC15 userdata placement for `TokkunTutorialFlg` while preserving Blue and Green behavior.
- Added Yellow wire mapper support for optional `tokkun_tutorial_flg`, including raw values `0`, `1`, and `7`.
- Added integration tests proving absent tutorial state is omitted, persisted raw tutorial state is read back, and a Yellow Tokkun playresult feeds userdata without exposing stage-history fields.

## Task Commits

Each task was committed atomically:

1. **Task 1 RED: Prove Yellow Tokkun userdata readback** - `1f28ba36` (test)
2. **Task 1 GREEN: Enable Yellow Tokkun tutorial userdata** - `9ca9212f` (feat)
3. **Task 2: Prove persisted Yellow Tokkun tutorial flows to userdata only** - `accdfe3b` (test)

**Plan metadata:** committed separately with this summary.

## Files Created/Modified

- `Application/Ac15/Ac15EraProfiles.cs` - Enables Yellow profile placement for Tokkun tutorial userdata readback.
- `Adapters.GameProtocol.Yellow/Mappers/UserDataMappers.cs` - Serializes optional `TokkunTutorialFlg` only when common userdata contains a value.
- `Tests/Ac15/Ac15EraProfileTests.cs` - Locks Yellow true, Blue true, and Green false profile placement.
- `Tests/Ac15/Ac15UserDataServiceTests.cs` - Proves shared AC15 userdata carries Yellow tutorial values and still omits Green.
- `Tests/Yellow/YellowUserDataProtocolTests.cs` - Covers optional wire absence/raw values, persisted query readback, playresult-to-userdata integration, and no history surface exposure.

## Decisions Made

- Reused the existing `YellowAc15UserDataAdapter` snapshot value instead of adding a Yellow-only query path.
- Kept Tokkun history out of userdata by proving response/wire properties do not expose stage, timestamp, song-list, or summary facts.
- Did not add AdminApi, WebUI, initial-data, generated wire, Blue production, or Green production changes.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

- After enabling Yellow profile placement, one pre-existing Yellow userdata test still expected a persisted tutorial value to be omitted. The expectation was updated in the GREEN commit to match the Phase 16 contract.
- Task 2 integration tests passed immediately because Task 1 enabled the production path; Task 2 was committed as coverage only.

## Verification

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15UserData|FullyQualifiedName~Ac15EraProfile|FullyQualifiedName~YellowUserData"` - RED failed first on Yellow profile placement and Yellow mapper optional serialization, then passed, 15 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowTokkun|FullyQualifiedName~YellowUserData|FullyQualifiedName~YellowPlayResult"` - passed, 50 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowTokkun|FullyQualifiedName~YellowPlayResult|FullyQualifiedName~YellowUserData|FullyQualifiedName~Ac15UserData|FullyQualifiedName~Ac15EraProfile"` - passed, 56 tests.
- `git diff --name-only 4dc78b4e..HEAD` showed only Yellow userdata mapper/profile code and focused AC15/Yellow tests; no AdminApi, WebUI, initial-data, generated Yellow wire, Green production mapper, or Blue production behavior changed.
- Phase 16 protocol readback remains limited to `UserDataResponse.tokkun_tutorial_flg`. Phase 17 owns runtime smoke.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for `16-04-PLAN.md`. Plan 16-03 completes YTOK-03 while leaving Banacoin compatibility to Plan 16-04.

---
*Phase: 16-yellow-tokkun-and-banacoin-compatibility*
*Completed: 2026-06-08*
