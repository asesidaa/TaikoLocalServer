---
phase: 14-yellow-identity-userdata-crowns-self-best-and-normal-play
plan: "02"
subsystem: api
tags: [yellow, ac15, userdata, self-best, crowns, protobuf]
requires:
  - phase: 14-01
    provides: Yellow-owned save, best, play-history, favorite, and recent-song tables plus Yellow identity/default-save routes
provides:
  - Mediator-backed Yellow userdata readback through `Ac15UserDataService`
  - Yellow self-best readback from `SongBestDataYellow`
  - Yellow crownsdata readback with raw `hash_crown_flg` field 3 proof
affects: [yellow, phase-14, userdata, self-best, crowns, normal-play]
tech-stack:
  added: []
  patterns: [Yellow AC15 snapshot adapter, Yellow wire readback mapper, raw crown field proof]
key-files:
  created:
    - Application/Ac15/YellowAc15UserDataAdapter.cs
    - Application/Handlers/UserDataQuery.Yellow.cs
    - Application/Handlers/GetSelfBestQuery.Yellow.cs
    - Adapters.GameProtocol.Yellow/Mappers/UserDataMappers.cs
    - Adapters.GameProtocol.Yellow/Mappers/SelfBestMappers.cs
    - Adapters.GameProtocol.Yellow/Mappers/CrownsDataMappers.cs
    - Tests/Yellow/YellowUserDataProtocolTests.cs
    - Tests/Yellow/YellowSelfBestTests.cs
    - Tests/Yellow/YellowCrownsDataTests.cs
  modified:
    - Application/Handlers/UserDataQuery.cs
    - Application/Handlers/GetSelfBestQuery.cs
    - Adapters.GameProtocol.Yellow/Controllers/YellowScaffoldControllers.cs
    - Tests/Yellow/YellowCatalogBoundaryTests.cs
    - Tests/Yellow/YellowRouteSkeletonTests.cs
key-decisions:
  - "Yellow userdata uses `Ac15UserDataService` through a Yellow save/catalog snapshot and omits Tokkun tutorial readback in Phase 14."
  - "Yellow self-best reads only `SongBestDataYellow` and maps normal/Ura/Shin rows into Yellow wire DTOs."
  - "Yellow `crownsdata.php` sends raw inflated crown bytes in protobuf field 3, based on local Yellow IDA evidence for direct 10-bit packed consumption."
patterns-established:
  - "Yellow readback routes stay adapter-local at the wire boundary while shared AC15 services build common DTOs or packed bodies."
  - "Crown transport decisions require Yellow-specific byte tests instead of copying Blue/Green compression."
requirements-completed: [YUSR-02, YPLY-02, YCRN-01]
duration: 27 min
completed: 2026-06-08
---

# Phase 14 Plan 02: Yellow Readback Summary

**Yellow userdata, self-best, and raw crown readback from Yellow-owned state**

## Performance

- **Duration:** 27 min
- **Started:** 2026-06-08T06:48:49+08:00
- **Completed:** 2026-06-08T07:15:54+08:00
- **Tasks:** 3
- **Files modified:** 12

## Accomplishments

- Replaced Yellow `userdata.php` with Mediator-backed readback from Yellow save state, Yellow catalog data, Yellow favorites, and Yellow recent songs.
- Replaced Yellow `selfbest.php` with Yellow-owned best-row readback, including normal/Ura and Shin wire placement tests.
- Replaced Yellow `crownsdata.php` with Yellow-owned crown readback and byte-level tests proving `hash_crown_flg` is protobuf field 3 containing raw inflated crown bytes, not gzip.

## Task Commits

1. **Task 1: Add Yellow userdata readback through AC15 snapshot service** - `0c2cbdea` (feat)
2. **Task 2: Add Yellow self-best handler, mapper, and route** - `b6b1534e` (feat)
3. **Task 3: Add Yellow crowns readback with byte-level placement and encoding proof** - `b4b42bf0` (feat)

**Plan metadata:** committed separately with this summary.

## Files Created/Modified

- `Application/Ac15/YellowAc15UserDataAdapter.cs` - Yellow save/catalog/favorite/recent bridge for `Ac15UserDataService`.
- `Application/Handlers/UserDataQuery.Yellow.cs` and `GetSelfBestQuery.Yellow.cs` - Yellow readback handler partials.
- `Adapters.GameProtocol.Yellow/Controllers/YellowScaffoldControllers.cs` - Yellow userdata, self-best, and crownsdata route replacements.
- `Adapters.GameProtocol.Yellow/Mappers/UserDataMappers.cs`, `SelfBestMappers.cs`, and `CrownsDataMappers.cs` - Yellow wire/readback mapping.
- `Tests/Yellow/YellowUserDataProtocolTests.cs`, `YellowSelfBestTests.cs`, and `YellowCrownsDataTests.cs` - Focused readback and byte-placement proof.
- `Tests/Yellow/YellowCatalogBoundaryTests.cs` and `YellowRouteSkeletonTests.cs` - Guard updates for the new Phase 14 readback surfaces.

## Decisions Made

- Yellow Tokkun tutorial state remains absent from userdata readback in Phase 14; Phase 16 owns Tokkun persistence and readback semantics.
- Yellow crowns use raw inflated crown bytes because local Yellow IDA evidence shows `OnCrownsDataResponse` directly unpacks `hash_crown_flg` as 1024 little-bit-endian 10-bit values from the protobuf bytes.
- Blue/Green gzip behavior remains era-specific contrast material and was not reused for Yellow.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Resolved Yellow crown transport evidence gate**
- **Found during:** Task 3 (Yellow crown readback)
- **Issue:** Proto/wire evidence alone proved field placement but not raw-vs-gzip transport.
- **Fix:** Inspected local Yellow IDA database `.tools/yellow/EBOOT.ELF.i64`; `sub_1D6DD8` / `OnCrownsDataResponse` directly reads `hash_crown_flg` and unpacks 10-bit values without a decompression call, so the route uses raw inflated bytes.
- **Files modified:** `Adapters.GameProtocol.Yellow/Controllers/YellowScaffoldControllers.cs`, `Adapters.GameProtocol.Yellow/Mappers/CrownsDataMappers.cs`, `Tests/Yellow/YellowCrownsDataTests.cs`
- **Verification:** `YellowCrownsDataTests.CrownsDataResponse_Yellow_Field3ContainsRawInflatedCrownBytes_NotGzip` serializes the Yellow response and asserts field 3 contains raw bytes.
- **Committed in:** `b4b42bf0`

**2. [Rule 3 - Blocking] Advanced Yellow route/boundary guards for crowns**
- **Found during:** Task 3 verification
- **Issue:** Existing Phase 14 guards still assumed crownsdata was no-state or that all runtime behavior was Mediator-only.
- **Fix:** Updated guards to allow the Phase 14 crown readback route to read `SongBestDataYellow` while keeping deferred PlayResult/shop/Banacoin-adjacent routes and battle surfaces no-state or absent.
- **Files modified:** `Tests/Yellow/YellowCatalogBoundaryTests.cs`, `Tests/Yellow/YellowRouteSkeletonTests.cs`
- **Verification:** Yellow readback and route guard filters passed.
- **Committed in:** `b4b42bf0`

---

**Total deviations:** 2 auto-fixed (2 blocking). **Impact:** Both were necessary to satisfy YCRN-01 without expanding into Phase 15/16 behavior.

## Issues Encountered

- The previous executor stream left production commits for userdata and self-best without `14-02-SUMMARY.md`; recovery resumed from the existing commits and did not duplicate those tasks.
- The repo-local `agents/gsd-executor.md` path referenced by the workflow was absent, so task commits followed the already-loaded GSD git integration format.

## Verification

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowCrownsData|FullyQualifiedName~Ac15CrownServiceTests|FullyQualifiedName~Ac15ProtocolBytesTests|FullyQualifiedName~YellowRouteSkeleton"` - passed, 18 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowUserData|FullyQualifiedName~YellowSelfBest|FullyQualifiedName~YellowCrownsData|FullyQualifiedName~YellowRouteSkeleton"` - passed, 16 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Yellow"` - passed, 95 tests.
- `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase14-yellow"` - passed, 0 warnings, 0 errors.
- Source inspection confirmed `YellowCrownsDataTests` asserts protobuf field 3, raw crown byte length, and not-gzip marker bytes.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for `14-03-PLAN.md`. Yellow now has the readback surfaces that Plan 03 normal playresult persistence must feed. Phase-level verification/review and Phase 15 were not started.

---
*Phase: 14-yellow-identity-userdata-crowns-self-best-and-normal-play*
*Completed: 2026-06-08*
