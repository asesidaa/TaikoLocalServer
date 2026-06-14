---
phase: 22
slug: red-adminapi-webui-and-runtime-closeout
status: ready
created: 2026-06-15
---

# Phase 22 Pattern Map

## Inputs Read

- `.planning/PROJECT.md`
- `.planning/REQUIREMENTS.md`
- `.planning/ROADMAP.md`
- `.planning/STATE.md`
- `.planning/phases/22-red-adminapi-webui-and-runtime-closeout/22-CONTEXT.md`
- `.planning/phases/22-red-adminapi-webui-and-runtime-closeout/22-RESEARCH.md`
- `.planning/phases/22-red-adminapi-webui-and-runtime-closeout/22-VALIDATION.md`
- `.planning/phases/22-red-adminapi-webui-and-runtime-closeout/22-UI-SPEC.md`
- `.planning/phases/21-older-ac15-challengecompe-capability-and-red-binding/21-SUMMARY.md`
- `.planning/phases/21-older-ac15-challengecompe-capability-and-red-binding/21-VERIFICATION.md`
- `.planning/codebase/ARCHITECTURE.md`
- `.planning/codebase/STRUCTURE.md`
- `.planning/codebase/TESTING.md`
- `AGENTS.md`

## Working Tree Warning

The repository already has uncommitted Red ChallengeCompe work in:

- `Application/Ac15/ChallengeCompe/Ac15ChallengeCompeProgressEvaluator.cs`
- `Application/Ac15/ChallengeCompe/Ac15ChallengeCompeRewardDecisions.cs`
- `Application/Handlers/GetChallengeCompeQuery.Red.cs`
- `Application/Handlers/GetChallengeCompeQuery.cs`
- `Application/Handlers/UpdatePlayResultCommand.Red.cs`
- `Application/Handlers/UserDataQuery.Red.cs`
- `Host/wwwroot/data/red/red_telop_data.json`
- `Tests/Red/RedChallengeCompeTests.cs`

Executors must inspect these diffs before editing related files and preserve the Phase 22 correction: Don Challenge has no opt-in toggle or mutation gate. `UserSaveDataRed.IsChallengeCompe` must not gate Red Don Challenge progress, reward locks, or AdminApi/WebUI visibility.

## AdminApi Era Branch Pattern

Existing AdminApi controllers preserve legacy Nijiiro routes and add era-specific handling through `/api/{era}/...` plus `EraRoute.TryParse`.

Relevant files:

- `Adapters.AdminApi/Controllers/UserSettingsController.cs`
- `Adapters.AdminApi/Controllers/UserSettingsController.Yellow.cs`
- `Adapters.AdminApi/Controllers/PlayDataController.cs`
- `Adapters.AdminApi/Controllers/PlayDataController.Yellow.cs`
- `Adapters.AdminApi/Controllers/PlayHistoryController.cs`
- `Adapters.AdminApi/Controllers/PlayHistoryController.Yellow.cs`
- `Adapters.AdminApi/Controllers/FavoriteSongsController.cs`
- `Adapters.AdminApi/Controllers/FavoriteSongsController.Yellow.cs`
- `Adapters.AdminApi/Controllers/SongLeaderboardController.cs`
- `Adapters.AdminApi/Controllers/SongLeaderboardController.Yellow.cs`
- `Adapters.AdminApi/Controllers/DanBestDataController.cs`
- `Adapters.AdminApi/Controllers/GameDataController.cs`
- `Adapters.AdminApi/Controllers/CustomizationCatalogController.cs`

Red should follow the Yellow/Blue partial-file shape where it adds enough code to bind Red-owned tables and the Red catalog, not a generic repository or shared gameplay table. The normal surfaces are:

- User settings/profile customization through `Ac15UserSettingsService`
- Play data through `SongBestDataRed` and `SongPlayDataRed`
- Play history through `SongPlayDataRed` and `RedFavoriteSongs`
- Favorites through `RedFavoriteSongs`
- Leaderboards through non-Shin `SongBestDataRed`
- Dani readback through `DanScoreDataRed` and `DanStageScoreDataRed`
- Game data and customization through `IRedCatalog`

## Red Runtime Data Pattern

Red runtime state is already era-owned:

- `Application/Abstractions/ITaikoDbContext.Red.cs`
- `Infrastructure/Persistence/TaikoDbContext.Red.cs`
- `Domain/Entities/UserSaveDataRed.cs`
- `Domain/Entities/SongBestDatumRed.cs`
- `Domain/Entities/SongPlayDatumRed.cs`
- `Domain/Entities/RedFavoriteSongs.cs`
- `Domain/Entities/RedRecentSongs.cs`
- `Domain/Entities/DanScoreDatumRed.cs`
- `Domain/Entities/DanStageScoreDatumRed.cs`
- `Domain/Entities/RedChallengeCompeRawFact.cs`
- `Domain/Entities/RedChallengeCompeProgress.cs`
- `Application/Common/UserSaveDataRedExtensions.cs`
- `Application/Ac15/Ac15EraProfiles.cs`
- `Application/Ac15/RedAc15UserDataAdapter.cs`

The plan should keep Red behavior bound to these tables. Red must not read Blue, Green, Yellow, or Nijiiro gameplay rows except in tests that seed other eras to prove no-cross-era behavior.

## Don Challenge AdminApi Pattern

Cabinet `challengecompe.php` and the WebUI Don Challenge page are separate surfaces.

Current Red WIP intentionally trends cabinet `challengecompe.php` toward empty unsupported buckets. Phase 22 should expose Don Challenge progress through a dedicated AdminApi contract rather than by reusing the cabinet response DTO:

- `GET api/{era}/DonChallenge/availability`
- `GET api/{era}/DonChallenge/{baid}`

The API should read:

- `IRedCatalog.ChallengeCompe`
- active `Ac15ChallengeCompeMonthlyBundle`
- `RedChallengeCompeProgress`
- `UserSaveDataRed.ReleaseSongFlg`
- `UserSaveDataRed.TitleFlg`

The API should not expose:

- raw `RedChallengeCompeRawFact` rows
- `ary_user_compe_*` or `ary_bng_compe_*` buckets
- `UserSaveDataRed.IsChallengeCompe`
- reward route diagnostics
- WaiWai, battle, item shop, medal, wallet, coupon, payment, or transaction state

Suggested contract locations:

- `Contracts.AdminApi/Responses/DonChallengeResponse.cs`
- `Contracts.AdminApi/ViewModels/DonChallengeTask.cs`
- `Contracts.AdminApi/ViewModels/DonChallengeTrack.cs`
- `Contracts.AdminApi/ViewModels/DonChallengeReward.cs`
- `Adapters.AdminApi/Controllers/DonChallengeController.cs`

## WebUI Era Pattern

`TaikoWebUI/Utilities/WebUiEra.cs` centralizes supported era normalization and route construction. Existing generic pages use:

- `CurrentEra => WebUiEra.NormalizeOrDefault(Era, AuthService.DefaultEra)`
- `WebUiEra.Api(CurrentEra, "...")`
- `WebUiEra.UserRoute(Baid, CurrentEra, "...")`
- `WebUiEra.IsAc15(CurrentEra)`

Red support should be added there first, then existing pages should naturally request `/api/Red/...`. Update `Tests/WebUi/GameDataServiceTests.cs` so Red is supported and AC15-classified.

## Don Challenge WebUI Pattern

The UI contract keeps the app as a dense MudBlazor admin/player tool. Add a read-only page:

- `TaikoWebUI/Pages/DonChallenge.razor`
- `TaikoWebUI/Pages/DonChallenge.razor.cs`
- `TaikoWebUI/Services/DonChallengeService.cs` or equivalent scoped service
- `TaikoWebUI/Components/NavMenu.razor`
- `TaikoWebUI/Program.cs`

Navigation must hide the Don Challenge link unless the selected era supports the older-AC15 ChallengeCompe capability and the availability route reports an active bundle. Direct routes render a neutral unavailable state instead of redirecting or falling back to Nijiiro.

## Test Pattern

Useful Phase 22 tests are behavior guards:

- Red AdminApi controllers read and write only Red-owned rows.
- Red generic WebUI services request `/api/Red/...` after Red is enabled.
- Don Challenge AdminApi returns configured active tasks plus persisted progress and reward status without raw facts or opt-in fields.
- Don Challenge WebUI service/page requests Red routes and renders unavailable states without era fallback.
- Cabinet ChallengeCompe compatibility remains separate from AdminApi/WebUI readback.

Avoid source-shape, route-inventory, generated-wire, DI-registration, migration-text, or controller-body string assertions.

