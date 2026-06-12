---
phase: 18-red-evidence-and-capability-foundation
plan: 02
subsystem: game-protocol-adapters
tags: [red, ac15, protobuf, protogen, adapter-foundation]

requires:
  - phase: 18-red-evidence-and-capability-foundation
    provides: Red route/version/root evidence and capability matrix from Plan 18-01
provides:
  - First-class Red `GameEra` identity
  - Red game protocol adapter project shell
  - Generated Red adapter-local wire DTOs from `proto/red`
affects: [phase-18, phase-19, phase-20, phase-21, red-ac15]

tech-stack:
  added: []
  patterns:
    - Adapter-local generated protobuf wire
    - Nullable optional primitive wire generation with `protogen +nullablevaluetype=yes`
    - First-class era enum append without reordering existing values

key-files:
  created:
    - Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj
    - Adapters.GameProtocol.Red/DependencyInjection.cs
    - Adapters.GameProtocol.Red/GlobalUsings.cs
    - Adapters.GameProtocol.Red/MapperlyDefaults.cs
    - Adapters.GameProtocol.Red/RedAdapterMarker.cs
    - Adapters.GameProtocol.Red/Wire/Game.cs
    - Adapters.GameProtocol.Red/Wire/VsInterface.cs
  modified:
    - Domain/Enums/GameEra.cs
    - TaikoLocalServer.slnx

key-decisions:
  - "Red is introduced as a first-class adapter and enum identity only; Host runtime binding, route probes, catalog/profile binding, gameplay state, and tests remain deferred to Plans 18-03 and 18-04 or later phases."
  - "Generated Red wire uses `protogen --package=TaikoLocalServer.Adapters.GameProtocol.Red.Wire` so package-less dumped proto inputs stay immutable while committed DTOs land in the adapter-local namespace."

patterns-established:
  - "Red adapter shell mirrors the Yellow adapter project shape without copying Yellow runtime behavior."
  - "Red wire generation starts from immutable `proto/red` inputs with nullable optional primitive support."

requirements-completed: [RFND-02, RFND-03]

duration: 24 min
completed: 2026-06-13
---

# Phase 18 Plan 02: Red Enum, Adapter Project, and Generated Wire Foundation Summary

**Red AC15 adapter identity with generated adapter-local protobuf DTOs from immutable `proto/red` inputs.**

## Performance

- **Duration:** 24 min
- **Started:** 2026-06-12T21:19:14Z
- **Completed:** 2026-06-12T21:42:49Z
- **Tasks:** 1
- **Files modified:** 9

## Accomplishments

- Added `GameEra.Red` after existing era enum values without renaming or reordering Nijiiro, Green, Blue, or Yellow.
- Created `Adapters.GameProtocol.Red` as a first-class adapter shell with Red assembly/namespace names, `AddGameProtocolRed`, `RedAdapterMarker`, Red global usings, and Mapperly defaults.
- Generated Red `Wire/Game.cs` and `Wire/VsInterface.cs` from `proto/red/taiko.proto` and `proto/red/vsinterface.proto` with nullable optional primitive support.
- Added the Red adapter project to `TaikoLocalServer.slnx`.

## Task Commits

1. **Task 1: Create the Red adapter shell and generated wire** - `6a1a976c` (feat)

## Files Created/Modified

- `Domain/Enums/GameEra.cs` - Appended `Red = 4`.
- `TaikoLocalServer.slnx` - Included the Red adapter project in the solution build graph.
- `Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj` - New Red adapter project using the Yellow adapter project shape.
- `Adapters.GameProtocol.Red/DependencyInjection.cs` - Red adapter DI extension and era constant.
- `Adapters.GameProtocol.Red/RedAdapterMarker.cs` - Red adapter assembly marker.
- `Adapters.GameProtocol.Red/GlobalUsings.cs` - Minimal Red adapter global usings needed by the shell.
- `Adapters.GameProtocol.Red/MapperlyDefaults.cs` - Mapperly strict target defaults for future Red mappers.
- `Adapters.GameProtocol.Red/Wire/Game.cs` - Generated Red game wire DTOs from `taiko.proto`.
- `Adapters.GameProtocol.Red/Wire/VsInterface.cs` - Generated Red startup/version wire DTOs from `vsinterface.proto`.

## Decisions Made

- Red is only an adapter/wire foundation in this plan. No Host `Program.cs`, Host settings, runtime route probes, Red EF state, Red catalog/profile binding, AdminApi, WebUI, or tests were added.
- Used `protogen --package=TaikoLocalServer.Adapters.GameProtocol.Red.Wire` during generation so the committed DTOs have the required Red adapter namespace while `proto/red` remains unchanged.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Generated namespace for package-less Red proto inputs**
- **Found during:** Task 1 (Create the Red adapter shell and generated wire)
- **Issue:** The exact base generation command produced C# without a namespace because `proto/red/taiko.proto` and `proto/red/vsinterface.proto` do not declare a proto package, but the plan requires `TaikoLocalServer.Adapters.GameProtocol.Red.Wire`.
- **Fix:** Regenerated with repo-local `protogen` using `--package=TaikoLocalServer.Adapters.GameProtocol.Red.Wire` plus `+nullablevaluetype=yes`; no proto input files were edited.
- **Files modified:** `Adapters.GameProtocol.Red/Wire/Game.cs`, `Adapters.GameProtocol.Red/Wire/VsInterface.cs`
- **Verification:** Wire headers show `Input: taiko.proto` / `Input: vsinterface.proto`, both files use the Red namespace, nullable optionals are present, builds pass, and `git status --porcelain -- proto/red` is clean.
- **Committed in:** `6a1a976c`

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** The adjustment preserves the plan's generated-wire intent and proto immutability while satisfying the required adapter-local namespace.

## Issues Encountered

- `Get-Date -AsUTC` is not available in the local PowerShell version; timestamps were captured with `[DateTime]::UtcNow` instead.
- GSD metadata update tooling left `STATE.md` progress and some `ROADMAP.md` rows inconsistent after the standard update commands; the affected metadata rows were corrected to Plan 18-02 complete / Phase 18 at 2 of 4.

## Known Stubs

None. Generated protobuf default values and `ShouldSerialize*` presence helpers are tool-generated wire semantics, not user-facing placeholder data.

## Threat Flags

None. The new adapter assembly and generated DTO trust boundary are already covered by the plan threat model, and no routes, controllers, auth paths, file access paths, or persistence schema changes were introduced.

## Verification

- `dotnet build Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj` - passed, 0 warnings, 0 errors.
- `dotnet build TaikoLocalServer.slnx` - passed, 0 warnings, 0 errors.
- `if (git status --porcelain -- proto/red) { exit 1 }` - passed; no dumped proto edits.
- `if (rg -n "Ac15EraProfiles\\.Red|DbSet<.*Red|UserSaveDataRed|SongPlayDataRed|Adapters\\.GameProtocol\\.(Blue|Yellow).*Wire" Adapters.GameProtocol.Red Application Domain Infrastructure) { exit 1 }` - passed; no Red profile, gameplay persistence, or Blue/Yellow wire imports.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for Plan 18-03 to add Host Red settings, enabled-era gating, and validation guardrails. Red runtime routes, profile/catalog binding, gameplay state, AdminApi, and WebUI remain intentionally absent.

## Self-Check: PASSED

- Found all created Red adapter and wire files.
- Found `.planning/phases/18-red-evidence-and-capability-foundation/18-02-SUMMARY.md`.
- Found task commit `6a1a976c`.

---
*Phase: 18-red-evidence-and-capability-foundation*
*Completed: 2026-06-13*
