# Phase 27 Plan 01 Summary: White AdminApi WebUI Exposure and Automated Closeout

## Completed

- Added White to `WebUiEra` supported/known eras and AC15 classification while keeping Don Challenge support Red-only.
- Added White AdminApi route handling for AC15 profile settings, favorites, play data, play history, song leaderboard, Dani best data, music/Dani catalog data, and customization catalog data.
- Added White-specific AdminApi partials over White-owned tables for favorites, play data, play history, leaderboard, Dani, and profile settings.
- Added focused `WhiteAdminApiTests` for White profile edit/readback, score/history/favorite/leaderboard/Dani data, catalog data, customization data, and cross-era isolation.
- Extended WebUI tests for White game-data routes, profile-display routes, route helpers, AC15 classification, server-published favorite limits, and Don Challenge unavailability.
- Kept `UserSettingsController` Nijiiro-only and kept Don Challenge unavailable for White.

## Verification

- Focused White/WebUI/Auth/Don Challenge filter passed: 54 tests.
- Broader AdminApi/controller regression filter passed: 60 tests.
- Don Challenge additive boundary rerun passed: 12 tests.
- Full test suite passed: 822 tests.
- Temp-output Host build with generated-source emission passed with 0 warnings and 0 errors.
- Mapperly generated-source inspection confirmed White normal-play and Dani projections, White playresult unsupported-mode null slots, White reward/userdata fields, and AdminApi auth config mapping generation.

## Manual Verification

Not run. Phase 27 is intentionally left at `human_needed` so the user can run the game and review the WebUI manually.
