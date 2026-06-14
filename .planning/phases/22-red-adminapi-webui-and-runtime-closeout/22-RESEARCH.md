# Phase 22 Research - Red AdminApi/WebUI and Runtime Closeout

**Researched:** 2026-06-15
**Phase:** 22 - red-adminapi-webui-and-runtime-closeout
**Requirements:** RVER-01, RVER-02, RVER-03

## Research Complete

Phase 22 can be planned as a bounded Red admin/runtime closeout slice, not a new era runtime implementation. Red already has route, catalog, profile, normal play, Dani, Tokkun tutorial, simple compatibility, and ChallengeCompe runtime state from Phases 18-21. The remaining implementation work is to expose only those implemented Red-owned surfaces through AdminApi/WebUI, add a dedicated Don Challenge operator page/API, correct the Phase 21 opt-in assumption, and record final automated plus manual cabinet/RPCS3 evidence.

## Current State

### Phase Inputs

- `22-CONTEXT.md` supersedes Phase 21 opt-in decisions: Don Challenge participation is not a local enrollment toggle, and `UserSaveDataRed.IsChallengeCompe` must not gate progress mutation, readback, reward-song locks, or reward grants.
- Phase 21 shipped Red-owned ChallengeCompe sidecar/state/rewards/readback, but its summary and verification still describe opt-in-gated behavior. Phase 22 planning must treat those Phase 21 statements as historical context, not the accepted target.
- Phase 22 has `UI hint: yes` in `ROADMAP.md`; `workflow.ui_phase` and `workflow.ui_safety_gate` are both enabled. No `22-UI-SPEC.md` exists at research time, so the plan-phase UI design-contract gate will stop planning unless the user runs `$gsd-ui-phase 22` or explicitly re-runs plan-phase with `--skip-ui`.

### Current Local Code

- AdminApi normal surfaces currently accept Green/Blue/Yellow and reject Red in the main era switches:
  - `UserSettingsController.cs`
  - `PlayDataController.cs`
  - `PlayHistoryController.cs`
  - `FavoriteSongsController.cs`
  - `SongLeaderboardController.cs`
  - `DanBestDataController.cs`
  - `GameDataController.cs`
  - `CustomizationCatalogController.cs`
- WebUI normal pages are already mostly era-parametric through `WebUiEra.Api(...)` and `WebUiEra.UserRoute(...)`. The central blocker is `WebUiEra` not knowing Red:
  - `Supported` and `Known` include Nijiiro, Green, Blue, Yellow only.
  - `IsAc15(...)` excludes Red, so AC15 profile/customization behavior does not activate for Red.
  - `GameDataServiceTests.NormalizeEnabled_IgnoresUnsupportedEras` currently asserts Red is filtered out.
- `IRedCatalog` and `RedEraGameDataCatalog` already expose Red music, Taikojuku, customization, movie, and `ChallengeCompe` catalog data, so Red game-data and customization AdminApi support can follow the Yellow patterns with Red-specific catalog access.
- Red runtime rows already exist for normal play, bests, favorites, recent songs, Dani, Tokkun tutorial, and ChallengeCompe state. AdminApi should bind directly to Red-owned `DbSet`s and `Ac15EraProfiles.Red`, matching existing AC15 patterns.
- The worktree has pre-existing uncommitted Phase 22-adjacent changes in ChallengeCompe code. The important planning facts from that local state:
  - `UpdatePlayResultCommand.Red.cs` no longer checks `saveData.IsChallengeCompe` before ChallengeCompe mutation.
  - `UserDataQuery.Red.cs` no longer passes `saveData.IsChallengeCompe` into reward-song locking.
  - `GetChallengeCompeQuery.Red.cs` currently returns empty buckets unconditionally.
  - Tests in `RedChallengeCompeTests.cs` are being changed toward no-opt-in behavior and away from `challengecompe.php` progress readback.
  These changes are not committed by plan-phase and should be treated as local WIP that implementers must inspect before editing.

## Implementation Research

### Normal Red AdminApi/WebUI Parity

The normal Red AdminApi work should be thin and pattern-following:

- Add Red cases to AdminApi controller switches only for implemented Red state.
- Add `UserSettingsController.Red.cs` using `context.GetOrCreateRedSaveDataAsync`, `context.DanScoreDataRed`, `Ac15UserSettingsAccess.Red` (to be added), and `Ac15EraProfiles.Red.Limits`.
- Add Red readback builders for play data, play history, leaderboard, Dani, game data, and customization by following Yellow/Blue patterns over Red-owned rows and `catalog.Red()`.
- Add Red favorite read/write support with the existing AC15 max-5 favorite behavior, using `RedFavoriteSongs`.
- Add Red to `WebUiEra.Supported`, `Known`, and `IsAc15`. Existing generic pages should then request `/api/Red/...` routes through `WebUiEra.Api(...)`.
- Do not add Red item-shop, medal/shop-season, WaiWai, battle, or compatibility diagnostics UI. Compatibility-route evidence belongs in verification artifacts and logs for this phase.

Likely files:

- `Application/Ac15/Ac15UserSettingsAccess.cs`
- `Adapters.AdminApi/Controllers/UserSettingsController.cs`
- `Adapters.AdminApi/Controllers/UserSettingsController.Red.cs`
- `Adapters.AdminApi/Controllers/PlayDataController.cs`
- `Adapters.AdminApi/Controllers/PlayDataController.Red.cs`
- `Adapters.AdminApi/Controllers/PlayHistoryController.cs`
- `Adapters.AdminApi/Controllers/PlayHistoryController.Red.cs`
- `Adapters.AdminApi/Controllers/FavoriteSongsController.cs`
- `Adapters.AdminApi/Controllers/FavoriteSongsController.Red.cs`
- `Adapters.AdminApi/Controllers/SongLeaderboardController.cs`
- `Adapters.AdminApi/Controllers/DanBestDataController.cs`
- `Adapters.AdminApi/Controllers/GameDataController.cs`
- `Adapters.AdminApi/Controllers/CustomizationCatalogController.cs`
- `TaikoWebUI/Utilities/WebUiEra.cs`
- `Tests/Red/RedAdminApiTests.cs`
- `Tests/WebUi/GameDataServiceTests.cs`

### Don Challenge API and Page

Don Challenge should not be folded into `UserSetting` or profile DTOs. It needs a dedicated AdminApi contract because it is a capability surface, not a profile setting.

Recommended shape:

- Add contracts under `Contracts.AdminApi/Responses/` for a user-era Don Challenge response.
- Response should include:
  - `IsAvailable` / active bundle presence.
  - Active bundle id and optional configured window.
  - Personal task cards with slot/task id/name/rule summary.
  - Progress value, completion state, completed timestamp when present.
  - Track/song rows derived from configured active task definitions and saved progress/best scores where applicable.
  - Reward statuses: locked, earned, unavailable; include configured song/title reward ids.
  - A message or empty list when no active bundle is configured.
- Do not expose `RedChallengeCompeRawFacts`, uploaded `ary_challenge_id`, `ary_user_compe_id`, or `ary_bng_compe_id` on the page.
- Keep the page capability-gated. The first supported era is Red, but the helper/service naming should not hardcode "Red Don Challenge" as the capability boundary.
- Navigation should hide Don Challenge when the selected supported era has no active configured ChallengeCompe bundle. Since `NavMenu.razor` is currently synchronous and only knows `AuthService.EnabledEras`, the plan should include a small WebUI service/API call or cached capability endpoint to answer nav gating without making the page fail noisily.

Likely files:

- `Contracts.AdminApi/Responses/DonChallengeResponse.cs` or equivalent.
- `Adapters.AdminApi/Controllers/DonChallengeController.cs`.
- `TaikoWebUI/Services/DonChallengeService.cs` or a method on an existing service if local patterns prefer it.
- `TaikoWebUI/Pages/DonChallenge.razor`
- `TaikoWebUI/Pages/DonChallenge.razor.cs`
- `TaikoWebUI/Components/NavMenu.razor`
- `TaikoWebUI/Utilities/WebUiEra.cs`
- `Tests/Red/RedDonChallengeAdminApiTests.cs`
- `Tests/WebUi/*DonChallenge*Tests.cs` or focused service/nav behavior coverage.

### No-Opt-In Correction

The accepted Phase 22 target is "no local opt-in." Planning should include a dedicated checkpoint before or alongside Don Challenge API work:

- Remove `UserSaveDataRed.IsChallengeCompe` as a participation gate from Red ChallengeCompe mutation, reward locks, rewards, and AdminApi/WebUI presentation.
- Do not add an AdminApi/WebUI opt-in toggle.
- Keep the Red userdata field only as protocol compatibility/visibility if current mapper requires it.
- Reconcile the current dirty local `GetChallengeCompeQuery.Red.cs` state. If the cabinet route remains intentionally empty, that must be an explicit plan decision backed by Phase 22 context and tests. If Phase 21 readback remains part of the runtime contract, restore it without the opt-in gate and keep user/BNG buckets empty.
- Don Challenge AdminApi should read from the active Red catalog and Red progress/reward state directly, not from raw facts and not from another era.

### Closeout Evidence

Final closeout needs both automated and manual evidence:

- Focused Red AdminApi/WebUI tests for route behavior, Red-owned read/write rows, no-cross-era rows, catalog/customization data, Don Challenge availability, reward status, and no-opt-in behavior.
- Focused Red ChallengeCompe tests after the opt-in correction, including no-cross-mode Tokkun exclusion and reward lock/grant behavior.
- Full Red regression, full test suite, and temp-output Host build.
- A `22-VERIFICATION.md` or closeout artifact that records automated commands and separates manual cabinet/RPCS3 smoke status from automated proof.
- Manual user-reported issues in Phase 22 scope must block closeout until fixed; automated tests alone cannot claim final client compatibility.

## Risks and Constraints

- **UI-SPEC gate:** Phase 22 includes a new WebUI page. Planning should not proceed without a UI design contract unless the user explicitly chooses `--skip-ui`.
- **Dirty worktree:** Existing uncommitted ChallengeCompe changes must be inspected and incorporated, not overwritten. Plan tasks should warn executors to read current diffs before editing ChallengeCompe files.
- **Readback ambiguity:** Phase 22 context demands Don Challenge page/API progress, but the local WIP currently empties the Red cabinet `challengecompe.php` response. The plan must make this an explicit implementation decision, not an accidental side effect.
- **No diagnostics creep:** Reward-card, reward-execution, Banacoin-adjacent, and other compatibility routes should not gain UI pages in this phase.
- **No operator authoring UI:** Schedule editing, bundle management, JSON/schema editing, and reward-management tooling remain future scope.
- **No cross-era state:** All Red AdminApi and Don Challenge behavior must stay on Red-owned rows and shared identity only.

## Validation Architecture

### Automated Coverage

- Red AdminApi route/row tests:
  - `UserSettings_Red_ReadsAndSavesRedProfileOnly`
  - `PlayData_Red_UsesRedBestRowsOnly`
  - `PlayHistory_Red_UsesRedPlaysAndFavoritesOnly`
  - `FavoriteSongs_Red_ReadsAndWritesRedRowsOnly`
  - `SongLeaderboard_Red_UsesRedBestRowsOnly`
  - `DanBestData_Red_UsesRedDanRowsOnly`
  - `GameData_Red_ReturnsRedMusicAndDanCatalogData`
  - `CustomizationCatalog_Red_ReturnsRedCatalogSlices`
- WebUI service/route tests:
  - `NormalizeEnabled_KeepsRedWhenEnabled`
  - `InitializeAsync_LoadsRedDanDataWhenEnabled`
  - `CatalogLookups_RequestRedAdminApiRoutes`
  - Don Challenge service requests `/api/Red/...` and handles unavailable active bundle.
- Don Challenge AdminApi behavior tests:
  - Red active bundle returns personal task cards and reward status from Red catalog/progress.
  - No active bundle returns unavailable/empty data and navigation capability false.
  - Raw facts and user/BNG buckets are not exposed.
  - Red reward-song/title status is derived from Red release/title flags and active configured rewards.
  - Other era rows remain untouched/unused.
- ChallengeCompe no-opt-in tests:
  - `IsChallengeCompe=false` does not block progress mutation.
  - `IsChallengeCompe=false` does not block reward-song locks.
  - No AdminApi/WebUI opt-in field or toggle is introduced.
  - Tokkun uploads still do not create ChallengeCompe, normal, Dani, reward, or unlock state.

### Verification Commands

Use focused filters first, then full verification:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedAdminApi|FullyQualifiedName~RedDonChallenge|FullyQualifiedName~RedChallengeCompe|FullyQualifiedName~GameDataServiceTests"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Red"
dotnet test Tests/Tests.csproj
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
```

Final closeout should also record user-provided cabinet/RPCS3 smoke for implemented Red normal, Tokkun tutorial, simple compatibility, and ChallengeCompe/Don Challenge flows. If manual smoke is not yet available, the phase should remain not complete or should record the manual gap explicitly rather than claiming compatibility.

## Planning Recommendation

Plan Phase 22 in checkpoint-sized slices:

1. Red normal AdminApi/WebUI parity using existing generic pages and Red-owned rows/catalogs.
2. Don Challenge no-opt-in correction and dedicated AdminApi read model.
3. Don Challenge WebUI page and capability-gated navigation, after `UI-SPEC.md`.
4. Final automated verification plus manual smoke evidence record and v1.3 closeout updates.

Do not proceed to PLAN.md creation until the UI design-contract gate is satisfied or explicitly skipped.

