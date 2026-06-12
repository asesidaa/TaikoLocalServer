---
phase: 18-red-evidence-and-capability-foundation
plan: 04
subsystem: red-route-probes
tags: [red, ac15, routing, protobuf, runtime-smoke]

requires:
  - phase: 18-red-evidence-and-capability-foundation
    provides: Red enum, adapter shell, generated wire, and Host Red enablement from Plans 18-02 and 18-03
provides:
  - No-state Red route probes for 22 IDB-known `/v08r01/chassis/*` suffixes
  - Split Red controller files matching existing adapter routing shape
  - Red runtime smoke handoff and user-confirmed basic connection evidence
  - Preservation checks proving probes avoid gameplay, EF, Mediator, and payment authority
affects: [phase-18, phase-19, red-ac15, route-probes, runtime-smoke]

tech-stack:
  added: []
  patterns:
    - Adapter-local generated protobuf DTO binding via `[FromBody]`
    - Per-route controller files for AC15 adapter routes
    - Runtime smoke evidence captured in phase artifact before close

key-files:
  created:
    - Adapters.GameProtocol.Red/Controllers/PlayResultController.cs
    - Adapters.GameProtocol.Red/Controllers/InitialDataCheckController.cs
    - Adapters.GameProtocol.Red/Controllers/TournamentCheckController.cs
    - .planning/phases/18-red-evidence-and-capability-foundation/18-RUNTIME-SMOKE.md
  modified:
    - Adapters.GameProtocol.Red/GlobalUsings.cs
    - .planning/phases/18-red-evidence-and-capability-foundation/18-04-PLAN.md

key-decisions:
  - "Red route probes stay no-state: they bind generated Red wire DTOs, log requests, return minimal success-shaped generated responses, and do not call Mediator, EF, shared AC15 gameplay services, wallet, coupon, transaction, or payment authority code."
  - "Red probe controllers are split into per-route files matching the existing Blue, Green, and Yellow adapter shape; this fixed the runtime 405s seen with the single-file probe implementation after a clean Host rebuild regenerated MVC application parts."
  - "Phase 18 runtime smoke is accepted at the basic connection/request-routing level; card scan and deeper gameplay progression remain out of scope until later Red support phases."

patterns-established:
  - "Red route probes live under `/v08r01/chassis/*` only for IDB-known suffixes; proto-only candidates such as `getbanacoininfo.php` and `getreitai.php` remain absent."
  - "Manual runtime smoke records both failed pre-fix observations and user-confirmed successful basic connection after the routing fix."

requirements-completed: [RFND-01, RFND-02, RFND-03]

duration: 49 min
completed: 2026-06-13
---

# Phase 18 Plan 04: Red Route Probe and Runtime Smoke Summary

**No-state Red route probes with user-confirmed basic RPCS3/cabinet connection.**

## Performance

- **Duration:** 49 min
- **Started:** 2026-06-12T22:06:00Z
- **Completed:** 2026-06-12T22:55:00Z
- **Tasks:** 3
- **Files modified:** 27

## Accomplishments

- Added Red no-state route probes for all 22 IDB-known `/v08r01/chassis/*` suffixes.
- Preserved Phase 18 boundaries: no Red gameplay persistence, profile state, catalog state, ChallengeCompe semantics, Banacoin authority, EF writes, or Mediator gameplay handlers.
- Created and then completed the Red runtime smoke artifact with user-confirmed basic connection evidence.
- Fixed runtime 405s on Red `initialdatacheck.php` and `tournamentcheck.php` by splitting probe controllers into normal per-route files and rebuilding Host metadata.

## Task Commits

Each task was committed atomically:

1. **Task 1: Implement no-state Red route probes** - `dcbad846` (feat)
2. **Task 2: Prepare runtime smoke evidence record** - `4a79d6cf` (docs)
3. **Runtime checkpoint fix: Split Red route probe controllers** - `767edc99` (fix)

## Files Created/Modified

- `Adapters.GameProtocol.Red/Controllers/*.cs` - Split Red no-state route probes for IDB-known route suffixes.
- `Adapters.GameProtocol.Red/GlobalUsings.cs` - Adds Red adapter controller/global imports matching existing adapter shape.
- `.planning/phases/18-red-evidence-and-capability-foundation/18-RUNTIME-SMOKE.md` - Records failed pre-fix 405s, the split/rebuild fix, and user-confirmed basic connection.
- `.planning/phases/18-red-evidence-and-capability-foundation/18-04-PLAN.md` - Updates artifact references to the split-controller implementation shape.

## Decisions Made

- Red route probes remain compatibility/evidence probes only. They do not create or read gameplay state and do not invent unsupported Red semantics.
- `getbanacoininfo.php` and `getreitai.php` remain absent because Phase 18 evidence treats them as proto-only candidates, not IDB-known route probes.
- User-confirmed basic connection is sufficient for Phase 18 close. Card scan was intentionally not tested because it requires later Red profile/gameplay support.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Split Red route probe controllers after runtime 405s**
- **Found during:** Task 3 (User verifies Red request-routing smoke)
- **Issue:** `Host/Logs/log-20260613.txt` showed POST requests to `/v08r01/chassis/initialdatacheck.php` and `/v08r01/chassis/tournamentcheck.php` returning 405. The single-file Red probe shape also left Red absent from generated Host MVC application-parts metadata until a clean rebuild.
- **Fix:** Replaced the single `RedRouteProbeControllers.cs` file with per-route controller files matching the existing adapter pattern, kept the same no-state behavior, and rebuilt Host so `TaikoLocalServer.Adapters.GameProtocol.Red` appears in MVC application-parts metadata.
- **Files modified:** `Adapters.GameProtocol.Red/Controllers/*.cs`, `Adapters.GameProtocol.Red/GlobalUsings.cs`, `18-RUNTIME-SMOKE.md`, `18-04-PLAN.md`.
- **Verification:** Route count found 22 Red route attributes, 22 `[HttpPost]` actions, Red application part appeared in generated Host metadata, no-state/proto-only route scans passed, Host build passed, full tests passed, and user confirmed basic connection succeeds.
- **Committed in:** `767edc99`.

---

**Total deviations:** 1 auto-fixed blocking runtime issue.
**Impact on plan:** Scope stayed within Plan 18-04. The implementation now matches established adapter structure and preserves all no-state route-probe boundaries.

## Issues Encountered

- Initial user smoke found 405s for Red `initialdatacheck.php` and `tournamentcheck.php`.
- Root cause was route discovery/build shape around the initial single-file Red probe implementation. Splitting controllers and rebuilding Host regenerated MVC metadata with Red included.

## Runtime Smoke

- `Host/Logs/log-20260613.txt` captured shared startup/auth and Mucha requests, then the initial Red 405s.
- After the split/rebuild fix, the user confirmed the Red client connects successfully.
- Card scan and in-game progression were not attempted because they require future Red gameplay/profile support and are outside Phase 18.

## Verification

- `dotnet test Tests/Tests.csproj` - passed, 684 passed, 0 failed, 0 skipped.
- `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` - passed, 0 warnings, 0 errors.
- `Select-String -Path 'Adapters.GameProtocol.Red/Controllers/*.cs' -SimpleMatch '[Route("/v08r01/chassis/'` - found 22 routes.
- Red `[HttpPost]` count check - found 22 actions.
- `rg` audit for `Mediator|GetChallengeCompeQuery|ITaikoDbContext|TaikoDbContext|SaveChanges|Ac15EraProfiles|Wallet|Coupon|Transaction` in Red controllers - passed with no matches.
- `rg` audit for `getbanacoininfo|getreitai` in Red controllers - passed with no matches.
- Generated Host MVC application-parts metadata includes `TaikoLocalServer.Adapters.GameProtocol.Red`.
- User RPCS3/cabinet smoke - basic connection passed after split/rebuild.

## User Setup Required

None for Phase 18 close. A debug Host process from `Host/bin/Debug/net10.0` may still be running locally from the smoke run.

## Next Phase Readiness

Phase 19 can bind Red capability/profile/catalog behavior on top of the first-class Red adapter and routable no-state probe foundation. Red card scan, profile reads/writes, gameplay progression, item/shop/payment semantics, and ChallengeCompe behavior remain intentionally unimplemented until evidence-backed later phases.

## Self-Check: PASSED

- Found `.planning/phases/18-red-evidence-and-capability-foundation/18-04-SUMMARY.md`.
- Found task commits `dcbad846`, `4a79d6cf`, and `767edc99`.
- Found user-confirmed runtime smoke disposition in `18-RUNTIME-SMOKE.md`.
- Found all 22 IDB-known Red route suffixes as split controller files.

---
*Phase: 18-red-evidence-and-capability-foundation*
*Completed: 2026-06-13*
