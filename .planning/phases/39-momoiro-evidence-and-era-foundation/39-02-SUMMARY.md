---
phase: 39-momoiro-evidence-and-era-foundation
plan: "02"
subsystem: game-protocol-adapter
tags: [momoiro, ac15, protobuf, protogen, adapter-foundation]

requires:
  - phase: 39-01
    provides: Momoiro evidence matrix and Wave 0 validation contracts
provides:
  - GameEra.Momoiro first-class enum identity
  - Buildable Adapters.GameProtocol.Momoiro adapter shell
  - Generated Momoiro game and shared startup/version wire DTOs
  - Solution and Tests project references for Momoiro adapter assembly
affects: [phase-39, momoiro, game-protocol, tests]

tech-stack:
  added: []
  patterns:
    - KIMIDORI-style adapter shell with Momoiro-owned namespace and route-prefix identity
    - Repo-local protogen generation from proto/momoiro with nullable optional primitives

key-files:
  created:
    - Adapters.GameProtocol.Momoiro/Adapters.GameProtocol.Momoiro.csproj
    - Adapters.GameProtocol.Momoiro/DependencyInjection.cs
    - Adapters.GameProtocol.Momoiro/GlobalUsings.cs
    - Adapters.GameProtocol.Momoiro/MapperlyDefaults.cs
    - Adapters.GameProtocol.Momoiro/MomoiroAdapterMarker.cs
    - Adapters.GameProtocol.Momoiro/MomoiroRoutePrefixes.cs
    - Adapters.GameProtocol.Momoiro/Wire/Game.cs
    - Adapters.GameProtocol.Momoiro/Wire/VsInterface.cs
  modified:
    - Domain/Enums/GameEra.cs
    - TaikoLocalServer.slnx
    - Tests/Tests.csproj

key-decisions:
  - "Keep Plan 39-02 limited to Momoiro adapter identity and generated wire; Host registration, application-part gating, controllers, runtime handlers, catalog/profile/persistence, EF migrations, AdminApi, and WebUI remain deferred."
  - "Generate Momoiro wire DTOs from proto/momoiro using repo-local protogen with +nullablevaluetype=yes, then rename generated lowercase outputs to established adapter filenames."
  - "Record the Wave 0 application-part test as an expected 39-03 handoff rather than weakening tests or adding production gating in this plan."

patterns-established:
  - "Momoiro adapter identity: distinct assembly, marker type, DI extension, Mapperly defaults, and route-prefix constant without controller exposure."
  - "Generated wire discipline: proto inputs stay immutable; adapter-owned Wire files carry protogen headers and Momoiro namespace."

requirements-completed: [MOFND-02, MOFND-04]

duration: 18 min
completed: 2026-06-25
status: complete
---

# Phase 39 Plan 02: First-Class Momoiro Adapter Identity and Generated Wire Summary

**Momoiro now has a buildable first-class adapter identity and generated adapter-owned protobuf wire DTOs, with runtime route exposure intentionally deferred.**

## Performance

- **Duration:** 18 min
- **Started:** 2026-06-25T20:20:00Z
- **Completed:** 2026-06-25T20:38:09Z
- **Tasks:** 3 completed
- **Files modified:** 11

## Accomplishments

- Added `GameEra.Momoiro = 8` without changing existing era values.
- Created `Adapters.GameProtocol.Momoiro` with buildable project metadata, marker, DI extension, Mapperly defaults, and `MomoiroRoutePrefixes.Game = "/v04r00/chassis"`.
- Generated `Wire/Game.cs` from `proto/momoiro/taiko.proto` and `Wire/VsInterface.cs` from `proto/momoiro/vsinterface.proto` using repo-local `.tools/protogen.exe`.
- Added Momoiro to the solution and test project references so Wave 0 tests compile against the adapter assembly.
- Preserved all prohibited surfaces: no `proto/momoiro` edits, controllers, Host registration, runtime handlers, catalog/profile/persistence, EF migrations, AdminApi, or WebUI behavior.

## Task Commits

1. **Task 1: Create Momoiro Adapter Identity** - `2cb2fb4e` (`feat`)
2. **Task 2: Generate Momoiro Adapter Wire** - `6d18bd52` (`feat`)
3. **Task 3: Prove Adapter Build And Wave 0 Test Progress** - `692fb15b` (`test`, empty verification commit)

## Files Created/Modified

- `Domain/Enums/GameEra.cs` - adds first-class `GameEra.Momoiro`.
- `TaikoLocalServer.slnx` - includes the Momoiro adapter project.
- `Tests/Tests.csproj` - references the Momoiro adapter project for Wave 0 tests.
- `Adapters.GameProtocol.Momoiro/Adapters.GameProtocol.Momoiro.csproj` - adapter project using existing shared/application/protobuf/Mapperly dependencies.
- `Adapters.GameProtocol.Momoiro/GlobalUsings.cs` - build-safe adapter globals without importing future controller mapper namespaces.
- `Adapters.GameProtocol.Momoiro/DependencyInjection.cs` - declares `AddGameProtocolMomoiro` and `GameEra.Momoiro` identity.
- `Adapters.GameProtocol.Momoiro/MomoiroAdapterMarker.cs` - assembly marker for later application-part gating.
- `Adapters.GameProtocol.Momoiro/MomoiroRoutePrefixes.cs` - defines `/v04r00/chassis` game route prefix.
- `Adapters.GameProtocol.Momoiro/MapperlyDefaults.cs` - matches established strict Mapperly defaults.
- `Adapters.GameProtocol.Momoiro/Wire/Game.cs` - generated from `taiko.proto`.
- `Adapters.GameProtocol.Momoiro/Wire/VsInterface.cs` - generated from `vsinterface.proto`.

## Verification

- `.\.tools\protogen.exe --version` - PASS, `protogen 3.2.52+f4db4afce3`.
- `rg -n "Input: taiko.proto|TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire" Adapters.GameProtocol.Momoiro/Wire/Game.cs` - PASS.
- `rg -n "Input: vsinterface.proto|TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire" Adapters.GameProtocol.Momoiro/Wire/VsInterface.cs` - PASS.
- `dotnet build Adapters.GameProtocol.Momoiro/Adapters.GameProtocol.Momoiro.csproj` - PASS, 0 warnings, 0 errors.
- `dotnet build Tests/Tests.csproj --no-restore` - PASS, 0 warnings, 0 errors.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroServerSettingsValidation"` - PASS, 1 passed.
- `git status --porcelain -- proto/momoiro` - PASS, no output.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroApplicationPart"` - EXPECTED HANDOFF, 1 failed and 1 passed; disabled-era removal for the Momoiro assembly is Plan 39-03 scope.

## Decisions Made

- Followed the plan's narrower file list over the broader Phase 39 pattern map: no Host registration, direct-protobuf fallback, application-part removal branch, or controller surface was added in 39-02.
- Kept `GlobalUsings.cs` buildable before controllers exist. Future controller work can add Momoiro mapper/wire globals when those namespaces are consumed.
- Used an empty verification commit for Task 3 because the task had no source edits but the GSD execution contract requires a per-task commit.

## Deviations from Plan

None - plan executed exactly as written.

**Total deviations:** 0 auto-fixed.
**Impact on plan:** No scope expansion. The only red test is the plan-declared 39-03 handoff.

## Issues Encountered

- `MomoiroApplicationPartTests.RemoveDisabledGameProtocolApplicationParts_MomoiroDisabled_RemovesMomoiroAssemblyPart` remains red because `GameProtocolApplicationParts` has not yet been updated to remove the Momoiro adapter assembly. This is expected and intentionally deferred to Plan 39-03.
- The working tree had a pre-existing unrelated `Host/.gitignore` modification before this plan started; it was not touched or staged.

## User Setup Required

None - no external service configuration required.

## Known Stubs

None. Generated nullable protobuf presence helpers contain `= null` assignments, but no hand-written placeholder or UI-flow stubs were introduced.

## Threat Flags

None. The adapter assembly identity and generated wire are covered by the plan threat model; no unplanned network endpoint, auth path, file access, schema, controller, or persistence surface was introduced.

## Next Phase Readiness

Ready for Plan 39-03 to add Host settings, DI registration, direct-protobuf fallback, and application-part gating. The Wave 0 application-part test provides the next production gate.

## Self-Check: PASSED

- Summary file exists at `.planning/phases/39-momoiro-evidence-and-era-foundation/39-02-SUMMARY.md`.
- Key created files exist: Momoiro adapter project, `Wire/Game.cs`, and `Wire/VsInterface.cs`.
- Task commits exist in git history: `2cb2fb4e`, `6d18bd52`, and `692fb15b`.
- `proto/momoiro` remained clean.

---
*Phase: 39-momoiro-evidence-and-era-foundation*
*Completed: 2026-06-25*
