---
phase: quick
plan: 260621-w8x
subsystem: ui
tags: [webui, ac15, capabilities, mudblazor]
requires:
  - phase: 34-adminapi-webui-and-runtime-closeout
    provides: Murasaki AdminApi/WebUI parity and AC15 profile settings contract
provides:
  - Capability-driven WebUI user page feature matrix
  - No-auth user-card action menus grouped by era then page
  - Explicit AC15 title-plate capability separate from title customization
affects: [TaikoWebUI, Contracts.AdminApi, Application.Ac15, Tests]
tech-stack:
  added: []
  patterns:
    - Shared WebUiEra page feature helper for logged-in and no-auth navigation
    - AC15 profile editor state filters rendered customization from returned capabilities
key-files:
  created:
    - .planning/quick/260621-w8x-let-s-have-a-quick-pass-to-improve-web-u/260621-w8x-SUMMARY.md
  modified:
    - TaikoWebUI/Utilities/WebUiEra.cs
    - TaikoWebUI/Components/UserCard.razor
    - TaikoWebUI/Components/NavMenu.razor
    - Contracts.AdminApi/Ac15ProfileSettings/Ac15ProfileSettingsDto.cs
    - Application/Ac15/Ac15ProfileCapabilities.cs
    - Application/Ac15/Ac15EraProfiles.cs
    - TaikoWebUI/Pages/ProfileEditor/Ac15ProfileEditorState.cs
    - TaikoWebUI/Shared/Customize/TitlePicker.razor
    - TaikoWebUI/Shared/Customize/TitlePickerCatalog.cs
    - Tests/WebUi/GameDataServiceTests.cs
    - Tests/Ac15/Ac15ProfileCapabilitiesTests.cs
    - Tests/Ac15/Ac15ProfileSettingsServiceTests.cs
    - Tests/WebUi/Ac15ProfileSettingsWebUiTests.cs
    - Tests/White/WhiteAdminApiTests.cs
    - Tests/Murasaki/MurasakiAdminApiTests.cs
key-decisions:
  - "Keep existing AC15 title-id editing semantics and fix the user-facing TitleId wording instead of changing persisted title values."
  - "Return no WebUI user-page features for unsupported era names instead of normalizing capability checks to Nijiiro."
patterns-established:
  - "Navigation surfaces should consume WebUiEra.GetUserPageFeatures so no-auth and logged-in menus do not drift."
  - "AC15 editor state should filter rendered customization groups through the returned capability DTO."
requirements-completed: []
duration: 15min
completed: 2026-06-21
status: complete
---

# Quick Task 260621-w8x: WebUI Era Selection and Capability Visibility Summary

**Capability-driven WebUI era/page navigation plus explicit AC15 title-vs-title-plate profile capability split.**

## Performance

- **Duration:** 15 min
- **Started:** 2026-06-21T15:24:58Z
- **Completed:** 2026-06-21T15:39:54Z
- **Tasks:** 2
- **Files modified:** 15

## Accomplishments

- Added `WebUiEra.GetUserPageFeatures` for Profile, Song List, High Scores, Play History, Dani Dojo, and Red/White-only Don Challenge candidate visibility.
- Converted no-auth `UserCard` actions from flat era/page labels into nested action -> era -> page menus.
- Updated logged-in `NavMenu` to render selected-era links from the same feature helper while preserving the Don Challenge availability check.
- Added `SupportsTitlePlate` to AC15 profile capabilities and DTOs, with White and Murasaki reporting title support but no title-plate support.
- Updated AC15 editor state and title picker wording so title-ID selection is presented as title customization, not title plate customization.

## Task Commits

1. **Task 1: WebUI feature matrix and grouped no-auth menus** - `2b8c5efa` (`feat(260621-w8x): add WebUI era page feature matrix`)
2. **Task 2: AC15 title/title-plate capability split** - `7007f7cb` (`feat(260621-w8x): split AC15 title plate capability`)

## Verification

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GameDataServiceTests|FullyQualifiedName~DonChallengeServiceTests"`: passed, 28 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ProfileCapabilitiesTests|FullyQualifiedName~Ac15ProfileSettingsServiceTests|FullyQualifiedName~Ac15ProfileSettingsWebUiTests|FullyQualifiedName~WhiteAdminApiTests|FullyQualifiedName~MurasakiAdminApiTests"`: passed, 29 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~WebUi|FullyQualifiedName~Ac15ProfileCapabilitiesTests|FullyQualifiedName~Ac15ProfileSettingsServiceTests|FullyQualifiedName~WhiteAdminApiTests|FullyQualifiedName~MurasakiAdminApiTests"`: passed, 57 tests.
- `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"`: passed.

Warnings observed during verification:
- Existing `SQLitePCLRaw.lib.e_sqlite3` NU1903 advisory.
- Existing Murasaki Mapperly RMG020 unmapped-source warnings from untouched mapper files.

## Manual Checks Still Required

- Auth-disabled `/Users` WebUI check: user-card menus should be grouped as action -> era -> page, with Don Challenge only under Red/White when availability is true.
- White and Murasaki Profile -> Costume WebUI check: title customization should remain available, and the selector/dialog should present title wording rather than title plate wording.
- Cabinet/RPCS3 acceptance remains outside this quick WebUI task and was not claimed.

## Deviations from Plan

None - plan executed as written.

## Known Stubs

None. Stub scan only returned existing markup/initializer false positives and pre-existing `N/A` display text outside this task's new behavior.

## Threat Flags

None. The task changed WebUI rendering and AdminApi DTO capability metadata, but added no new endpoints, auth paths, file access, persistence schema, or protocol wire surfaces.

## Self-Check: PASSED

- Summary created at `.planning/quick/260621-w8x-let-s-have-a-quick-pass-to-improve-web-u/260621-w8x-SUMMARY.md`.
- Task commits found: `2b8c5efa`, `7007f7cb`.
- ROADMAP.md was not modified.
