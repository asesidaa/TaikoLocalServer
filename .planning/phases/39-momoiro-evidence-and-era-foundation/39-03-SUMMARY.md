---
phase: 39-momoiro-evidence-and-era-foundation
plan: "03"
subsystem: host-composition
tags: [momoiro, ac15, host, application-parts, protobuf]

requires:
  - phase: 39-02
    provides: Momoiro adapter identity and generated wire DTOs
provides:
  - Momoiro disabled-era application-part gating
  - Host conditional Momoiro adapter registration
  - Exact Momoiro direct-protobuf fallback for /v04r00/chassis
  - Momoiro ServerSettings block and raw-data build-output hygiene
affects: [phase-39, momoiro, host, game-protocol]

tech-stack:
  added: []
  patterns:
    - Disabled-era route gating through shared MVC ApplicationPart removal
    - Host-only adapter composition without route controllers or runtime state
    - Root-level AC15 raw-data exclusion with debug output junction

key-files:
  created: []
  modified:
    - Adapters.GameProtocol.Shared/GameProtocolApplicationParts.cs
    - Host/Program.cs
    - Host/Host.csproj
    - Host/Configurations/ServerSettings.json
    - .planning/phases/39-momoiro-evidence-and-era-foundation/39-VALIDATION.md

key-decisions:
  - "Use GameProtocolApplicationParts as the only disabled-era route gate for Momoiro instead of adding controller-local checks."
  - "Keep Momoiro direct-protobuf fallback exact to MomoiroRoutePrefixes.Game (/v04r00/chassis) while leaving shared /v01r00 startup/version fallback unchanged."
  - "Move the Host Momoiro project reference into Task 2 because Program.cs cannot compile against the Momoiro adapter namespace before that reference exists."

patterns-established:
  - "Momoiro Host composition is additive and enabled-era gated: settings decide DI registration and MVC application-part discovery."
  - "Momoiro raw operator data is excluded from copied Host content and linked only in Debug builds when local data exists."

requirements-completed: [MOFND-02, MOFND-03]

duration: 17 min
completed: 2026-06-26
status: complete
---

# Phase 39 Plan 03: Host Settings And Application-Part Gating Summary

**Momoiro Host composition now gates disabled routes, registers enabled adapter services, and scopes direct protobuf fallback to `/v04r00/chassis` without adding route behavior.**

## Performance

- **Duration:** 17 min
- **Started:** 2026-06-25T20:42:00Z
- **Completed:** 2026-06-25T20:58:26Z
- **Tasks:** 3 completed
- **Files modified:** 5

## Accomplishments

- Added Momoiro to shared disabled-era application-part removal, making the Wave 0 application-part tests green.
- Registered `AddGameProtocolMomoiro()` only when `GameEra.Momoiro` is enabled.
- Added exact no-content-type protobuf fallback for `MomoiroRoutePrefixes.Game` (`/v04r00/chassis`) without broadening `/v04r00` or changing shared `/v01r00` fallback.
- Added Host project/build-output hygiene for Momoiro raw operator data and enabled Momoiro in `ServerSettings.json`.
- Updated Phase 39 validation rows for the now-green Wave 0 application-part and settings checks.

## Task Commits

Each task was committed atomically:

1. **Task 1: Add Momoiro Application-Part Gating** - `bfe2d6e0` (`feat`)
2. **Task 2: Add Host Registration And Exact Protobuf Fallback** - `8aa26340` (`feat`)
3. **Task 3: Add Host Project And Settings Entries** - `26068080` (`feat`)

## Files Created/Modified

- `Adapters.GameProtocol.Shared/GameProtocolApplicationParts.cs` - removes `TaikoLocalServer.Adapters.GameProtocol.Momoiro` when Momoiro is disabled.
- `Host/Program.cs` - imports Momoiro, registers its adapter service only when enabled, includes Momoiro in the no-enabled-era error, and adds exact `/v04r00/chassis` protobuf fallback.
- `Host/Host.csproj` - references the Momoiro adapter, excludes raw `wwwroot/data/momoiro/data/**` content from output, and creates a Debug-only data junction when local raw data exists.
- `Host/Configurations/ServerSettings.json` - enables Momoiro with `AutoExtractCatalog`, root-level `GameDataPath`, and the intentionally empty customization-name override path.
- `.planning/phases/39-momoiro-evidence-and-era-foundation/39-VALIDATION.md` - marks the Wave 0 Momoiro application-part and settings validation rows green.

## Verification

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroApplicationPart"` - PASS after Task 1, 2 passed.
- `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` - PASS after Task 2 and at plan close, 0 warnings, 0 errors.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroApplicationPart|FullyQualifiedName~MomoiroServerSettingsValidation"` - PASS, 3 passed.
- `rg -n "Momoiro|wwwroot/data/momoiro/data|CreateMomoiroGameDataSymlinkForDebug|Adapters.GameProtocol.Momoiro" Host/Configurations/ServerSettings.json Host/Host.csproj .planning/phases/39-momoiro-evidence-and-era-foundation/39-VALIDATION.md` - PASS.
- `git status --porcelain -- proto/momoiro` - PASS, no output.
- Scope check - PASS, no Momoiro controllers and no Momoiro additions under AdminApi, WebUI, handlers, persistence, or entity files.

## Decisions Made

- Followed the shared application-part gating pattern instead of duplicating disabled-era checks in controllers.
- Kept Host fallback scoped to `MomoiroRoutePrefixes.Game`, preserving the shared `/v01r00/chassis` startup/version behavior.
- Used existing Wave 0 tests as the TDD RED gates; no new tests were added because the plan restricted edits to production Host/shared files plus validation docs.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Moved Host Momoiro project reference into Task 2**
- **Found during:** Task 2 (Add Host Registration And Exact Protobuf Fallback)
- **Issue:** The Task 2 Host build failed with `CS0234` because `Program.cs` imported `TaikoLocalServer.Adapters.GameProtocol.Momoiro` before `Host/Host.csproj` referenced the Momoiro adapter project, even though that reference was scheduled for Task 3.
- **Fix:** Added the Momoiro adapter project reference to `Host/Host.csproj` in the Task 2 commit so the Task 2 build gate could compile.
- **Files modified:** `Host/Host.csproj`
- **Verification:** Re-ran `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` successfully with 0 warnings and 0 errors.
- **Committed in:** `8aa26340`

---

**Total deviations:** 1 auto-fixed (1 blocking).
**Impact on plan:** No scope expansion beyond files already listed in the plan. The reference was moved earlier only to satisfy the plan's own Task 2 build gate.

## Issues Encountered

- The working tree had a pre-existing unrelated `Host/.gitignore` modification before this plan started. It was not touched or staged.
- The broad stub scan pattern matched ordinary C# assignments, so the final stub check used targeted placeholder/TODO text. No placeholder or TODO stubs were introduced. The empty `CustomizationNameDataPath` is intentional and specified by the plan.

## User Setup Required

None - no external service configuration required.

## Known Stubs

None. This plan intentionally adds Host composition and configuration only; it does not add route controllers, catalog loading, persistence, AdminApi, WebUI, or runtime state behavior.

## Threat Flags

None. The security-relevant surfaces changed by this plan were covered by the plan threat model: disabled-route gating, exact direct-protobuf fallback, no custom parsers, and raw operator data output handling.

## Next Phase Readiness

Ready for Plan 39-04 to add the evidence-gated Momoiro game route surface and build gates. Host now composes the Momoiro adapter only when enabled and keeps disabled Momoiro route discovery blocked.

## Self-Check: PASSED

- Summary file exists at `.planning/phases/39-momoiro-evidence-and-era-foundation/39-03-SUMMARY.md`.
- Key modified files exist: `GameProtocolApplicationParts.cs`, `Host/Program.cs`, `Host/Host.csproj`, `ServerSettings.json`, and `39-VALIDATION.md`.
- Task commits exist in git history: `bfe2d6e0`, `8aa26340`, and `26068080`.
- `proto/momoiro` remained clean.
- No Momoiro controller, catalog, persistence, AdminApi, WebUI, or AC15 profile behavior was added.

---
*Phase: 39-momoiro-evidence-and-era-foundation*
*Completed: 2026-06-26*
