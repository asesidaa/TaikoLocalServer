# Phase 27 Context: White AdminApi WebUI and Runtime Closeout

## Goal

Expose implemented White-owned readback and edit surfaces through existing AdminApi/WebUI contracts, run automated closeout checks, and stop before final manual RPCS3/cabinet plus WebUI verification.

## Decisions

- **D-27-01:** White joins the existing generic AC15 AdminApi surfaces where Phase 25/26 implemented White-owned data: AC15 profile settings, favorites, play data, play history, song leaderboard, Dani best data, music/Dani catalog data, and customization catalog data.
- **D-27-02:** Legacy `UserSettingsController` remains Nijiiro-only. AC15 profile/user settings are exposed through `Ac15ProfileSettingsController`, which is the existing AC15 WebUI profile editor contract.
- **D-27-03:** White WebUI support is an era routing addition only. White is a known/supported AC15 era for generic pages, route helpers, game-data loading, and profile display names.
- **D-27-04:** Don Challenge remains Red-only. White is known to the WebUI and AdminApi but direct Don Challenge calls must return unavailable without falling back to Red data or exposing Red navigation.
- **D-27-05:** White favorite-song limits come from `Ac15EraProfiles.White` through the AdminApi auth config response. The WebUI must continue consuming server-published limits instead of hardcoding AC15-wide values.

## Stop Rule

Automated verification can pass, but Phase 27 must stop with manual verification pending. Final RPCS3/cabinet flow and WebUI review require the user to run the game and inspect the UI manually.
