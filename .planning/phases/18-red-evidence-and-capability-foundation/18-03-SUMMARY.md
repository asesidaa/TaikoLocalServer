---
phase: 18-red-evidence-and-capability-foundation
plan: 03
subsystem: host-configuration
tags: [red, ac15, host, settings, validation]

requires:
  - phase: 18-red-evidence-and-capability-foundation
    provides: Red enum, adapter shell, and generated wire from Plan 18-02
provides:
  - Red Host adapter registration behind enabled-era settings
  - Red application-part removal when disabled
  - Red `/v08r01/chassis` direct-protobuf fallback
  - Red local settings and Debug operator-data junction handling
  - Red settings validation guard proving no unsupported shop requirement
affects: [phase-18, phase-19, red-ac15, host-startup]

tech-stack:
  added: []
  patterns:
    - Host enabled-era adapter gating
    - AC15 direct-protobuf fallback scoped by route prefix
    - Era operator-data exclusion with Debug junction output

key-files:
  created:
    - Tests/Red/RedServerSettingsValidationTests.cs
  modified:
    - Host/Program.cs
    - Host/Host.csproj
    - Host/Configurations/ServerSettings.json

key-decisions:
  - "Red Host support is enabled through the existing era-gating pattern: DI registration happens only when `GameEra.Red` is enabled and the Red adapter application part is removed when disabled."
  - "Red settings intentionally omit `EnableShop` and `ActiveShopSeasonId`; Red is not added to AC15 shop validation because Red item-shop/payment behavior remains unsupported in Phase 18."
  - "Red direct-protobuf fallback is scoped to `/v08r01/chassis`, not the whole `/v08r01` prefix."

patterns-established:
  - "Committed Red local settings use `GameDataPath: wwwroot/data/red/data` with empty customization override path and no shop fields."
  - "Red operator data follows Green/Blue/Yellow Host build handling: raw operator data is excluded from content output and linked for Debug builds when present."

requirements-completed: [RFND-02, RFND-03]

duration: 8 min
completed: 2026-06-13
---

# Phase 18 Plan 03: Host Red Settings, Enabled-Era Gating, and Validation Guard Summary

**Red Host enablement with scoped protobuf fallback and settings validation that preserves unsupported-shop boundaries.**

## Performance

- **Duration:** 8 min
- **Started:** 2026-06-12T21:50:28Z
- **Completed:** 2026-06-12T21:58:26Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments

- Wired the Red adapter into Host startup using the existing enabled-era DI and disabled application-part removal pattern.
- Added `/v08r01/chassis` to the missing-content-type protobuf fallback without broadening to all `/v08r01` paths.
- Added committed local Red settings and Host build handling for `Host/wwwroot/data/red/data`.
- Added a focused Red settings validation guard proving Red can be enabled without `EnableShop` or `ActiveShopSeasonId`.

## Task Commits

Each task was committed atomically:

1. **Task 1: Add Host Red settings, adapter gating, and data-root build handling** - `05b7f6e4` (feat)
2. **Task 2: Protect Red settings from unsupported shop validation** - `c7f3cc66` (test)

## Files Created/Modified

- `Host/Program.cs` - Adds Red using, conditional `AddGameProtocolRed`, disabled Red application-part removal, Red in the no-era fatal message, and `/v08r01/chassis` protobuf fallback.
- `Host/Host.csproj` - References the Red adapter, excludes Red operator data from content output, and creates the Debug Red data junction when local operator data exists.
- `Host/Configurations/ServerSettings.json` - Adds enabled Red local settings with `GameDataPath` set to `wwwroot/data/red/data` and no shop fields.
- `Tests/Red/RedServerSettingsValidationTests.cs` - Guards Red settings validation through the public options binding path.

## Decisions Made

- Red Host wiring uses the same conditional adapter-registration and application-part gating pattern as the existing eras, keeping Red routes absent when disabled.
- Red remains outside AC15 shop validation in Phase 18. `Application/Settings/ServerSettingsOptionsValidationExtensions.cs` already limited shop-required settings to Green, Blue, and Yellow, so it was left unchanged.
- The Red protobuf fallback is scoped to `/v08r01/chassis` only, matching the Red game-route evidence boundary.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

- The Task 2 TDD red check passed immediately after adding the new Red test. Investigation showed the existing `Ac15ShopEras` whitelist already excludes Red, so no production validation change was required.

## TDD Gate Compliance

- **RED:** Advisory gap. The Red validation test passed before production changes because the expected behavior already existed.
- **GREEN:** Not applicable. No production change was needed after the already-green test.
- **Impact:** The behavior is guarded by `c7f3cc66`, and the production validation boundary remains unchanged.

## Known Stubs

None. The empty Red `CustomizationNameDataPath` is intentional committed configuration matching the existing AC15 settings shape, not a user-facing placeholder.

## Verification

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedServerSettingsValidationTests|FullyQualifiedName~YellowServerSettingsValidationTests|FullyQualifiedName~BlueServerSettingsValidationTests"` - passed, 7 tests.
- `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` - passed, 0 warnings, 0 errors.
- `git diff --check -- Host/Program.cs Host/Host.csproj Host/Configurations/ServerSettings.json` - passed.
- `git diff --check -- Tests/Red/RedServerSettingsValidationTests.cs Application/Settings/ServerSettingsOptionsValidationExtensions.cs` - passed.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for Plan 18-04 to add Red no-state route probes and the runtime smoke gate. Red gameplay/catalog persistence, `Ac15EraProfiles.Red`, AdminApi/WebUI, shop/payment behavior, and ChallengeCompe state remain intentionally absent.

## Self-Check: PASSED

- Found all modified Host files and the new Red settings validation test.
- Found `.planning/phases/18-red-evidence-and-capability-foundation/18-03-SUMMARY.md`.
- Found task commits `05b7f6e4` and `c7f3cc66`.

---
*Phase: 18-red-evidence-and-capability-foundation*
*Completed: 2026-06-13*
