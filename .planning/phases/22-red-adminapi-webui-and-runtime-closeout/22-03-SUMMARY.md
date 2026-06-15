---
phase: 22-red-adminapi-webui-and-runtime-closeout
plan: 3
subsystem: ui
tags: [red, webui, don-challenge, challengecompe, mudblazor, tests]
requires:
  - phase: 22-red-adminapi-webui-and-runtime-closeout
    provides: Plan 22-01 Red normal AdminApi surfaces and Plan 22-02 Don Challenge AdminApi contracts
  - phase: 21-older-ac15-challengecompe-capability-and-red-binding
    provides: Red ChallengeCompe progress, rewards, and readback boundaries
provides:
  - Red-enabled WebUI era normalization and generic AdminApi route helpers
  - Don Challenge WebUI service over `api/{era}/DonChallenge/availability` and `api/{era}/DonChallenge/{baid}`
  - Read-only Don Challenge Play Data page with available, unavailable, no-active-bundle, error, and retry states
  - Availability-gated NavMenu Don Challenge link for the older-AC15 ChallengeCompe capability
affects: [22-04, red-webui, don-challenge-webui, phase-22-closeout]
tech-stack:
  added: []
  patterns:
    - Existing `WebUiEra.Api` and `WebUiEra.UserRoute` helpers preserve Red AdminApi and user routes
    - Don Challenge navigation is driven by AdminApi availability, not by enabled-era presence alone
    - Direct unsupported Don Challenge routes render neutral unavailable UI without era fallback
key-files:
  created:
    - TaikoWebUI/Services/DonChallengeService.cs
    - TaikoWebUI/Pages/DonChallenge.razor
    - TaikoWebUI/Pages/DonChallenge.razor.cs
    - Tests/WebUi/DonChallengeServiceTests.cs
  modified:
    - TaikoWebUI/Utilities/WebUiEra.cs
    - TaikoWebUI/Program.cs
    - TaikoWebUI/Components/NavMenu.razor
    - TaikoWebUI/wwwroot/css/app.css
    - Tests/WebUi/GameDataServiceTests.cs
key-decisions:
  - "Red is now a supported AC15 WebUI era and generic WebUI catalog/readout routes use `/api/Red/...` without Red-only page branches."
  - "Don Challenge WebUI availability is a capability check backed by `api/{era}/DonChallenge/availability`; Red is the only current older-AC15 ChallengeCompe UI binding."
  - "Direct unsupported Don Challenge routes render unavailable copy for the requested era instead of normalizing to Nijiiro."
patterns-established:
  - "Read-only Don Challenge page consumes dedicated AdminApi DTOs and omits raw facts, opt-in state, diagnostics, and unsupported control families."
  - "Long Don Challenge task, song, and reward labels use wrapping CSS inside stable task-card layouts."
requirements-completed: [RVER-01, RVER-02]
duration: 15 min
completed: 2026-06-15
---

# Phase 22 Plan 3: Red WebUI Routing And Don Challenge Page Summary

**Red-aware generic WebUI routing plus a read-only Don Challenge Play Data page backed by dedicated availability/readback services**

## Performance

- **Duration:** 15 min
- **Started:** 2026-06-15T06:01:37Z
- **Completed:** 2026-06-15T06:16:30Z
- **Tasks:** 3/3
- **Files modified:** 9

## Accomplishments

- Enabled Red in `WebUiEra.Supported`, `Known`, and AC15 classification so existing generic pages preserve Red `/api/Red/...` and `Users/{baid}/Red/...` routes.
- Added `DonChallengeService` and DI registration for availability and user readback routes from the Plan 22-02 contracts.
- Added the read-only Don Challenge page under both user route forms with loading, unavailable, no-active-bundle, error, retry, summary, task-card, song-row, and reward-row states.
- Updated `NavMenu.razor` so the Play Data Don Challenge link appears only when the selected era supports the capability and AdminApi availability is active.
- Added focused WebUI service tests for Red route preservation, Red-only capability gating, and known unsupported-era no-fallback behavior.

## Task Commits

1. **Task 1: Enable Red In WebUiEra And Generic WebUI Services** - `6d7419a3` (`feat`)
2. **Task 2: Add Don Challenge WebUI Service** - `36651120` (`feat`)
3. **Task 3: Add Don Challenge Page And Navigation** - `370b8b2e` (`feat`)

## Files Created/Modified

- `TaikoWebUI/Services/DonChallengeService.cs` - WebUI HTTP client for Don Challenge availability and readback.
- `TaikoWebUI/Pages/DonChallenge.razor` - Read-only MudBlazor page shell and task/reward rendering.
- `TaikoWebUI/Pages/DonChallenge.razor.cs` - Page loading, breadcrumbs, unsupported-route handling, and display helpers.
- `Tests/WebUi/DonChallengeServiceTests.cs` - Focused route and unavailable-state service tests.
- `TaikoWebUI/Utilities/WebUiEra.cs` - Red support, AC15 classification, and Red-only ChallengeCompe capability helper.
- `TaikoWebUI/Program.cs` - Scoped Don Challenge service registration.
- `TaikoWebUI/Components/NavMenu.razor` - Availability-gated Don Challenge Play Data nav link with selected-era refresh.
- `TaikoWebUI/wwwroot/css/app.css` - Wrapping and stable-card styling for Don Challenge content.
- `Tests/WebUi/GameDataServiceTests.cs` - Red WebUI route/catalog tests and ChallengeCompe capability test.

## Decisions Made

- Red support is enabled through existing generic WebUI route helpers and service paths, not Red-specific page branches.
- The Don Challenge nav link is hidden unless both the selected era supports the capability and the AdminApi availability endpoint reports an active bundle.
- Unsupported direct Don Challenge routes render a neutral unavailable state for the requested era and do not call a Nijiiro fallback.
- New page copy uses literal English labels for new Don Challenge-specific text rather than adding empty neutral `.resx` values that could suppress display text.

## Deviations from Plan

None - plan executed within the approved scope.

## Issues Encountered

- Browser visual verification was not run inside this executor. The focused WebUI tests compile the Razor page and verify route behavior, but there was no practical in-executor browser visual pass for desktop/mobile widths without leaving a host process running. The orchestrator can decide whether to run manual visual verification.
- `Host/wwwroot/data/red/red_telop_data.json` remains a pre-existing dirty file and was not staged or committed for this plan.

## Verification

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GameDataServiceTests|FullyQualifiedName~DonChallengeServiceTests"` - PASSED, 15 passed, 0 failed, 0 skipped.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedAdminApi|FullyQualifiedName~RedDonChallenge|FullyQualifiedName~RedChallengeCompe|FullyQualifiedName~GameDataServiceTests"` - PASSED, 54 passed, 0 failed, 0 skipped.
- Forbidden UI surface scan on the new page/nav found no opt-in, raw fact, diagnostic, WaiWai, battle, item-shop, medal, payment, coupon, wallet, transaction, or `IsChallengeCompe` controls.

## Known Stubs

None - the scan found only intentional unavailable-state copy and non-UI test initializers.

## Threat Flags

None - this plan adds a WebUI client/page over the planned AdminApi contract and does not introduce new server endpoints, persistence, auth boundaries, file access, or raw protocol surfaces.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for Plan 22-04 runtime closeout. The remaining verification gap is visual/manual UI inspection if the orchestrator wants desktop/mobile confirmation after starting the host.

## Self-Check: PASSED

- Summary file exists at `.planning/phases/22-red-adminapi-webui-and-runtime-closeout/22-03-SUMMARY.md`.
- Created files exist: `DonChallengeService.cs`, `DonChallenge.razor`, `DonChallenge.razor.cs`, and `DonChallengeServiceTests.cs`.
- Task commits found: `6d7419a3`, `36651120`, and `370b8b2e`.
- Remaining dirty file `Host/wwwroot/data/red/red_telop_data.json` is pre-existing WIP and was intentionally not staged or committed.

---
*Phase: 22-red-adminapi-webui-and-runtime-closeout*
*Completed: 2026-06-15*
