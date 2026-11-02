---
phase: 23-white-evidence-and-era-foundation
plan: "02"
subsystem: adapter-foundation
tags: [white, ac15, protobuf, protogen, mapperly, game-era]

requires:
  - phase: 23-01
    provides: White route/root/transport evidence and approved foundation boundaries
provides:
  - GameEra.White identity preserving existing era numeric values
  - Buildable Adapters.GameProtocol.White project shell
  - White adapter-local wire DTOs generated from proto/white inputs
  - Solution and Tests project references for the White adapter
affects: [23-03-white-route-scaffolds, 24-white-catalog-profile-and-protocol-limits, white-runtime-foundation]

tech-stack:
  added: []
  patterns:
    - Red-style first-class game protocol adapter shell
    - protobuf-net protogen wire generation with +nullablevaluetype=yes
    - Mapperly assembly defaults copied without adding mapper classes

key-files:
  created:
    - Adapters.GameProtocol.White/Adapters.GameProtocol.White.csproj
    - Adapters.GameProtocol.White/GlobalUsings.cs
    - Adapters.GameProtocol.White/DependencyInjection.cs
    - Adapters.GameProtocol.White/WhiteAdapterMarker.cs
    - Adapters.GameProtocol.White/MapperlyDefaults.cs
    - Adapters.GameProtocol.White/Wire/Game.cs
    - Adapters.GameProtocol.White/Wire/VsInterface.cs
  modified:
    - Domain/Enums/GameEra.cs
    - TaikoLocalServer.slnx
    - Tests/Tests.csproj

key-decisions:
  - "White is represented as a first-class GameEra and adapter assembly, with no Phase 23 runtime/controller/catalog/AdminApi/WebUI behavior."
  - "White generated wire uses repo-local protogen 3.2.52 with --package=TaikoLocalServer.Adapters.GameProtocol.White.Wire and +nullablevaluetype=yes."
  - "The White shell omits a non-existent White.Mappers global using instead of adding an out-of-scope placeholder mapper namespace."

patterns-established:
  - "Adapter identity before runtime wiring: create the era assembly and generated wire first, then gate Host/controller behavior in later plans."
  - "Proto inputs remain immutable; adapter-local generated wire carries the protocol boundary."

requirements-completed: [WFND-02, WFND-03]

duration: 6 min
completed: 2026-06-17
---

# Phase 23 Plan 02: White Adapter Identity And Generated Wire Summary

**White AC15 adapter identity with generated protobuf-net wire DTOs from immutable proto/white inputs**

## Performance

- **Duration:** 6 min
- **Started:** 2026-06-17T13:40:28Z
- **Completed:** 2026-06-17T13:46:10Z
- **Tasks:** 1
- **Files modified:** 10

## Accomplishments

- Added `GameEra.White = 5` without changing existing enum numeric values.
- Created `Adapters.GameProtocol.White` as a buildable adapter shell with White namespace, assembly name, marker, DI extension, and Mapperly defaults.
- Generated White `Wire/Game.cs` from `proto/white/taiko.proto` and `Wire/VsInterface.cs` from `proto/white/vsinterface.proto`.
- Added White project references to `TaikoLocalServer.slnx` and `Tests/Tests.csproj`.
- Preserved Phase 23 foundation-only scope: no controllers, Host wiring, Mediator handlers, EF state, catalog registration, AdminApi/WebUI, AC15 profiles, or extra tests.

## Task Commits

1. **Task 1: Create White Adapter Identity And Generated Wire** - `522ccf03` (`feat`)

## Files Created/Modified

- `Domain/Enums/GameEra.cs` - Adds `White = 5` after existing era values.
- `TaikoLocalServer.slnx` - Adds the White adapter project to the solution.
- `Tests/Tests.csproj` - Adds the White adapter project reference.
- `Adapters.GameProtocol.White/Adapters.GameProtocol.White.csproj` - Defines the White adapter project.
- `Adapters.GameProtocol.White/GlobalUsings.cs` - Adds shared adapter/application imports for the White assembly.
- `Adapters.GameProtocol.White/DependencyInjection.cs` - Adds `GameEra.White` and `AddGameProtocolWhite`.
- `Adapters.GameProtocol.White/WhiteAdapterMarker.cs` - Adds the White adapter marker type.
- `Adapters.GameProtocol.White/MapperlyDefaults.cs` - Adds assembly-level Mapperly defaults.
- `Adapters.GameProtocol.White/Wire/Game.cs` - Generated from `taiko.proto`.
- `Adapters.GameProtocol.White/Wire/VsInterface.cs` - Generated from `vsinterface.proto`.

## Verification Results

| Command | Result |
|---------|--------|
| `.\.tools\protogen.exe --version` | PASS - `protogen 3.2.52+f4db4afce3` |
| `dotnet build Adapters.GameProtocol.White/Adapters.GameProtocol.White.csproj` | PASS - 0 warnings, 0 errors |
| `dotnet build Tests/Tests.csproj --no-restore` | PASS - 0 warnings, 0 errors |
| `git status --porcelain -- proto/white` | PASS - no output; `proto/white` unchanged |
| Generated header inspection | PASS - `Game.cs` has `Input: taiko.proto`; `VsInterface.cs` has `Input: vsinterface.proto`; both use `TaikoLocalServer.Adapters.GameProtocol.White.Wire` |

## Decisions Made

- White remains foundation-only in this plan. The adapter assembly and wire DTOs exist, but routable behavior remains deferred to later evidence-gated plans.
- Generated wire was produced with `--package=TaikoLocalServer.Adapters.GameProtocol.White.Wire` because White proto inputs have no package declaration and must remain immutable.
- The Red `Mappers` global using was not copied because Plan 23-02 forbids adding White mapper files. Adding the using without a namespace would either break the build or force an out-of-scope placeholder namespace.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Avoided non-existent White mapper namespace in global usings**
- **Found during:** Task 1 (Create White Adapter Identity And Generated Wire)
- **Issue:** The Red shell imports `TaikoLocalServer.Adapters.GameProtocol.Red.Mappers`, but Plan 23-02 does not allow adding White mapper files.
- **Fix:** Mirrored the adapter shell imports that are valid for this foundation project and omitted the non-existent `White.Mappers` import.
- **Files modified:** `Adapters.GameProtocol.White/GlobalUsings.cs`
- **Verification:** `dotnet build Adapters.GameProtocol.White/Adapters.GameProtocol.White.csproj` passed.
- **Committed in:** `522ccf03`

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Preserved the foundation-only scope and kept the White adapter buildable without adding out-of-scope mapper files.

## Issues Encountered

- The first protogen invocation used `proto\white\taiko.proto` and `proto\white\vsinterface.proto` while also passing `-Iproto\white`; protogen resolved inputs relative to the proto path and reported file-not-found before writing output. Reran with `taiko.proto` and `vsinterface.proto`, then renamed the generated files to the plan-required `Game.cs` and `VsInterface.cs` without editing generated contents.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for Plan 23-03. White now has a first-class adapter assembly and generated wire DTOs, while Host routing, controllers, fallback behavior, catalog/profile/runtime state, AdminApi/WebUI, and tests remain deferred to their owning plans.

## Self-Check: PASSED

- Found `.planning/phases/23-white-evidence-and-era-foundation/23-02-SUMMARY.md`.
- Found `Adapters.GameProtocol.White/Adapters.GameProtocol.White.csproj`.
- Found `Adapters.GameProtocol.White/Wire/Game.cs`.
- Found `Adapters.GameProtocol.White/Wire/VsInterface.cs`.
- Found task commit `522ccf03`.

---
*Phase: 23-white-evidence-and-era-foundation*
*Completed: 2026-06-17*
