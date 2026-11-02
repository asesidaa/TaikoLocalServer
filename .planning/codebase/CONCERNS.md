# Codebase Concerns

**Analysis Date:** 2026-06-11

## Tech Debt

**AC15 shared-core profiles assume Blue/Green/Yellow compatibility in one place:**
- Issue: `Ac15EraProfiles` uses one common `Ac15ProtocolLimits` instance and a shared `BlueGreenFeatures` feature set for Blue, Green, and Yellow, with only a few wire-placement switches. This is compact, but future AC15 eras can be over-shared by copying an existing profile before local proto/data/runtime evidence proves matching limits, feature availability, and wire placement.
- Files: `Application/Ac15/Ac15EraProfiles.cs`, `Application/Ac15/Ac15EraProfile.cs`, `Application/Ac15/Ac15FeatureSet.cs`, `Application/Ac15/Ac15ProtocolLimits.cs`, `Application/Ac15/Ac15WirePlacement.cs`
- Impact: A later era can silently inherit item shop, Dani, crown, userdata, Tokkun tutorial, or byte-width behavior that belongs only to the current AC15 set.
- Fix approach: Add new AC15 era profiles from evidence-backed capabilities, not by defaulting to `BlueGreenFeatures`; require focused tests around limits, unsupported feature omission, and adapter-local wire placement.

**Shared AC15 row-shape helpers are a high-blast-radius persistence boundary:**
- Issue: Phase 16.2 correctly avoids repository-shaped persistence and keeps concrete `DbSet` binding in handlers, but shared writers now own normal-play, Dani, item-shop, and user-settings algorithms through row-shape interfaces. Changes here affect Blue, Green, and Yellow at once.
- Files: `Application/Ac15/Ac15NormalPlayWriter.cs`, `Application/Ac15/Ac15DaniWriter.cs`, `Application/Ac15/Ac15DaniReadback.cs`, `Application/Ac15/Ac15ItemShopPurchase.cs`, `Application/Ac15/Ac15UserSettingsService.cs`, `Domain/Entities/IAc15SongPlayDatum.cs`, `Domain/Entities/IAc15SongBestDatum.cs`, `Domain/Entities/IAc15DanScoreDatum.cs`, `Domain/Entities/IAc15ShopSeasonState.cs`
- Impact: A policy change meant for one era can change all AC15 era state writes, while adding broad `IAc15*Persistence` interfaces would hide EF/table ownership again.
- Fix approach: Keep shared helpers algorithm-only, bind concrete era `DbSet`s at handler/service edges, use Mapperly for record/entity projection, and add no-cross-era/no-cross-mode tests for any shared-helper behavior change.

**Mapperly and protobuf presence semantics depend on ignored local generator inputs:**
- Issue: AC15 adapter mappers now use Mapperly with strict defaults and no production `ShouldSerialize*` calls, but generated wire files are tracked while `proto/` and `.tools/protogen.exe` are ignored/local.
- Files: `.gitignore`, `proto/blue/taiko.proto`, `proto/green/green.proto`, `proto/yellow/yellow-final.proto`, `.tools/protogen.exe`, `Adapters.GameProtocol.Blue/Wire/Game.cs`, `Adapters.GameProtocol.Green/Wire/Game.cs`, `Adapters.GameProtocol.Yellow/Wire/Game.cs`, `Adapters.GameProtocol.Blue/MapperlyDefaults.cs`, `Adapters.GameProtocol.Green/MapperlyDefaults.cs`, `Adapters.GameProtocol.Yellow/MapperlyDefaults.cs`, `Application/MapperlyDefaults.cs`
- Impact: A clean checkout cannot prove the tracked wire files came from the intended proto inputs and `+nullablevaluetype=yes` generator settings; presence/absence regressions can enter through generated output drift.
- Fix approach: Track canonical proto inputs or a generator manifest/script, then add a regeneration-diff verification that compares `Adapters.GameProtocol.*/Wire/*.cs` to generated output.

**Success-shaped compatibility and stub routes still exist:**
- Issue: Some routes/handlers intentionally log and return success or empty payloads without stateful behavior. Banacoin-adjacent success is a documented non-authority contract, but Green/Yellow challenge/tournament and Green token/song-purchase/AI handlers are still stubs.
- Files: `Application/Handlers/GetChallengeCompeQuery.Green.cs`, `Application/Handlers/GetChallengeCompeQuery.Yellow.cs`, `Application/Handlers/TournamentCheckQuery.Green.cs`, `Application/Handlers/TournamentCheckQuery.Yellow.cs`, `Application/Handlers/GetAiDataQuery.Green.cs`, `Application/Handlers/GetAiScoreQuery.Green.cs`, `Application/Handlers/GetTokenCountQuery.Green.cs`, `Application/Handlers/AddTokenCountCommand.Green.cs`, `Application/Handlers/PurchaseSongCommand.Green.cs`, `Adapters.GameProtocol.Blue/Controllers/BanacoinPaymentController.cs`, `Adapters.GameProtocol.Blue/Controllers/BalanceCheckController.cs`, `Adapters.GameProtocol.Yellow/Controllers/BanacoinPaymentController.cs`, `Adapters.GameProtocol.Yellow/Controllers/BalanceCheckController.cs`
- Impact: Cabinets and tests can observe protocol success even when no behavior is implemented, making unsupported surfaces easy to mistake for complete support.
- Fix approach: Keep non-authority compatibility routes explicitly documented and no-write tested; for non-compatibility stubs, either implement catalog/persistence-backed behavior from evidence or keep the route absent.

**AC15 catalog projection still has mechanical era duplication:**
- Issue: `Ac15CatalogSnapshotFactory` has canonical snapshot output, but still repeats Blue/Green/Yellow `MapItemShop`, `MapSeason`, and `MapTaikojuku` projection methods.
- Files: `Application/Ac15/Ac15CatalogSnapshotFactory.cs`, `Application/Catalog/Blue/BlueItemShopCatalog.cs`, `Application/Catalog/Green/GreenItemShopCatalog.cs`, `Application/Catalog/Yellow/YellowItemShopCatalog.cs`, `Application/Catalog/Ac15/Ac15ItemShopCatalog.cs`
- Impact: Adding fields or correcting a projection can drift across eras, especially where catalog shapes are value-identical.
- Fix approach: Move value-identical catalog record shapes behind narrow common source records or Mapperly projections while preserving era-owned catalog loaders and data roots.

**Warnings are not build failures:**
- Issue: The shared build props keep warnings non-fatal.
- Files: `Directory.Build.props`
- Impact: Nullable, analyzer, source-generator, and API warnings can accumulate without failing local or CI builds.
- Fix approach: Raise warning strictness by project or warning category, starting with `Application/`, `Infrastructure/`, and non-generated adapter code.

## Known Bugs

**Concurrent new-user allocation can produce duplicate BAIDs:**
- Symptoms: Two first-time cards can both observe the same max BAID and return or insert the same next BAID.
- Files: `Application/Handlers/BaidQuery.Blue.cs`, `Application/Handlers/BaidQuery.Green.cs`, `Application/Handlers/BaidQuery.Yellow.cs`, `Application/Handlers/BaidQuery.Nijiiro.cs`, `Application/Ac15/Ac15MyDonEntryService.cs`, `Application/Handlers/AddMyDonEntryCommand.Nijiiro.cs`
- Trigger: Concurrent `baid.php` / `mydonentry.php` requests for unknown access codes.
- Workaround: Not detected.
- Fix approach: Allocate BAIDs through a database sequence/table, transactional insert-with-retry, or another unique insert-first flow.

**Nijiiro gaiden Dan data is omitted from AdminApi/WebUI Dan best response:**
- Symptoms: `DanBestDataController` filters Nijiiro rows to normal Dan type and carries a local FIXME for gaiden handling.
- Files: `Adapters.AdminApi/Controllers/DanBestDataController.cs`, `TaikoWebUI/Pages/DaniDojo.razor.cs`
- Trigger: Requesting Dan best data for a BAID with `DanType.Gaiden` rows.
- Workaround: Not detected.
- Fix approach: Add gaiden projection and WebUI grouping, then cover with behavior tests under `Tests/`.

## Security Considerations

**Tracked local auth and TLS artifacts can be reused unsafely:**
- Risk: Local JWT configuration and PFX certificate files are present in the repo. If reused for exposed deployments, tokens and HTTPS identity can become predictable or shared.
- Files: `Host/Configurations/AuthSettings.json`, `Host/Configurations/Kestrel.json`, `Host/Certificates/cert.pfx`, `Host/Certificates/root.pfx`, `Infrastructure/DependencyInjection.cs`, `Infrastructure/Identity/JwtTokenService.cs`
- Current mitigation: Settings are configurable and Kestrel loads certificate paths from configuration.
- Recommendations: Treat checked-in auth/certificate material as development-only, move deploy-time secrets to environment/user-secret storage, rotate any certificate material used outside local development, and document required overrides.

**Local-mode authorization bypass is broad:**
- Risk: When authentication is disabled, every authorization policy succeeds and the WebUI synthesizes an admin principal.
- Files: `Infrastructure/Identity/AuthAwarePolicyEvaluator.cs`, `TaikoWebUI/Services/JwtAuthenticationStateProvider.cs`, `TaikoWebUI/Program.cs`, `Adapters.AdminApi/Authorization/ControllerAuthorizationExtensions.cs`
- Current mitigation: The behavior is intentionally local-mode convenience.
- Recommendations: Add a startup warning or refusal when binding non-loopback addresses with authentication disabled, unless an explicit override is configured.

**HTTP and protocol logging can capture sensitive data:**
- Risk: Host HTTP logging uses `HttpLoggingFields.All`, and protocol controllers/handlers log request facts. Logs can include headers, access codes, BAIDs, tokens, play history, protobuf bodies, and Banacoin-adjacent request data.
- Files: `Host/Program.cs`, `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs`, `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`, `Adapters.GameProtocol.Yellow/Controllers/PlayResultController.cs`, `Adapters.GameProtocol.Blue/Controllers/BanacoinPaymentController.cs`, `Adapters.GameProtocol.Yellow/Controllers/BanacoinPaymentController.cs`
- Current mitigation: Body log size limits cap captured data volume.
- Recommendations: Disable request/response body logging by default, redact authorization/access-code/person identifiers, and keep full protocol dumps development-only.

**CORS accepts any origin, method, and header:**
- Risk: A browser from any origin can call AdminApi if it has a bearer token, or if local-mode authorization is disabled.
- Files: `Host/Program.cs`, `TaikoWebUI/Services/JwtAuthenticationStateProvider.cs`
- Current mitigation: Bearer tokens are required when authentication is enabled.
- Recommendations: Restrict CORS to configured UI origins for non-local deployments and pair local-mode auth bypass with loopback-only binding.

**Anonymous auth endpoints disclose account state and lack rate limiting:**
- Risk: Login/register flows distinguish several account states and no rate limiter is registered.
- Files: `Adapters.AdminApi/Controllers/AuthController.cs`, `Host/Program.cs`, `TaikoWebUI/Services/AuthService.cs`
- Current mitigation: Passwords are hashed with BCrypt and registration can require last-play-time or invite-code checks.
- Recommendations: Normalize external errors, add rate limiting or lockout on anonymous auth endpoints, and keep detailed reasons server-side.

**JWT bearer token is stored in browser localStorage:**
- Risk: Any WebUI XSS can read the bearer token and call AdminApi as the user.
- Files: `TaikoWebUI/Services/JwtAuthenticationStateProvider.cs`, `TaikoWebUI/Services/AuthService.cs`, `TaikoWebUI/Program.cs`
- Current mitigation: Expired or invalid local tokens are removed client-side.
- Recommendations: Add CSP and XSS hardening headers in `Host/Program.cs`, avoid untrusted HTML rendering, and consider shorter token lifetimes for exposed deployments.

## Performance Bottlenecks

**AdminApi play-data endpoints load full user histories into memory:**
- Problem: Best-score and history endpoints load all play rows for a BAID, then filter/group in memory.
- Files: `Adapters.AdminApi/Controllers/PlayDataController.cs`, `Adapters.AdminApi/Controllers/PlayDataController.Yellow.cs`, `Adapters.AdminApi/Controllers/PlayHistoryController.cs`, `Adapters.AdminApi/Controllers/PlayHistoryController.Yellow.cs`, `TaikoWebUI/Pages/HighScores.razor.cs`, `TaikoWebUI/Pages/PlayHistory.razor.cs`, `TaikoWebUI/Pages/SongList.razor.cs`
- Cause: Response models return full history/detail data and controller projections compute counts/recent rows client-facing in memory.
- Improvement path: Add pagination and route-specific projections; push counts, last-play, best-play, and recent limits into SQL.

**Admin user-settings list is an N+1 read/create path:**
- Problem: `GetAllUserSetting` loads all users and calls `GetOrCreateNijiiroSaveDataAsync` per user.
- Files: `Adapters.AdminApi/Controllers/UserSettingsController.cs`, `Infrastructure/Persistence/TaikoDbContext.Shared.cs`
- Cause: A read endpoint can create missing defaults while looping over all users.
- Improvement path: Batch-load save data, page the user list, and separate read-only listing from repair/default creation.

**Startup catalog loading is eager and filesystem-heavy:**
- Problem: Host startup migrates the database, initializes enabled era catalogs, validates local operator data, loads committed sidecars, and builds dictionaries before serving requests.
- Files: `Host/Program.cs`, `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs`, `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`, `Infrastructure/GameDataCatalog/Blue/BlueEraGameDataCatalog.cs`, `Infrastructure/GameDataCatalog/Yellow/YellowEraGameDataCatalog.cs`, `Infrastructure/GameDataCatalog/Nijiiro/NijiiroEraGameDataCatalog.cs`
- Cause: Catalogs are singleton in-memory structures initialized up front.
- Improvement path: Keep required startup validation eager, but lazy-load optional heavy assets or add readiness diagnostics and cached background initialization for non-critical data.

**Shared AC15 normal-play writer still performs per-stage database work:**
- Problem: Each normal playresult stage can do best/favorite/recent lookups and the writer saves once before a second recent-trim save.
- Files: `Application/Ac15/Ac15NormalPlayWriter.cs`, `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, `Application/Handlers/UpdatePlayResultCommand.Green.cs`, `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`
- Cause: The generic writer updates rows stage-by-stage across concrete era `DbSet`s.
- Improvement path: Preload existing best/favorite/recent rows for the BAID, update tracked rows in memory, and save once after trimming when possible.

## Fragile Areas

**AC15 playresult special-mode gates are high-risk edit zones:**
- Files: `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, `Application/Handlers/UpdatePlayResultCommand.BlueBattle.cs`, `Application/Handlers/UpdatePlayResultCommand.BlueTokkun.cs`, `Application/Handlers/UpdatePlayResultCommand.Green.cs`, `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`, `Application/Handlers/UpdatePlayResultCommand.YellowTokkun.cs`, `Application/Ac15/Ac15NormalStageFilter.cs`, `Application/Ac15/Ac15NormalStagePolicies.cs`
- Why fragile: Blue Tokkun, Blue battle, Yellow Tokkun, Yellow WaiWai logging, and Green AI/ghost behavior must stay outside generic normal-play behavior.
- Safe modification: Keep special-mode classification in era handler partials before `Ac15NormalPlayWriter.SaveAsync`; add no-cross-mode and no-cross-era tests before changing classification or stage filtering.
- Test coverage: Strong across `Tests/Blue/BlueBattlePlayResultHandlerTests.cs`, `Tests/Blue/BlueTokkunPersistenceTests.cs`, `Tests/Green/GreenAiBattlePlayResultTests.cs`, `Tests/Yellow/YellowPlayResultHandlerTests.cs`, and `Tests/Yellow/YellowWaiWaiTests.cs`; future era behavior still requires new evidence-specific tests.

**Mapperly/wire presence is fragile despite strict defaults:**
- Files: `Adapters.GameProtocol.Blue/Mappers/UserDataMappers.cs`, `Adapters.GameProtocol.Green/Mappers/UserDataMappers.cs`, `Adapters.GameProtocol.Yellow/Mappers/UserDataMappers.cs`, `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs`, `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`, `Adapters.GameProtocol.Yellow/Mappers/PlayResultMappers.cs`, `Adapters.GameProtocol.Blue/Wire/Game.cs`, `Adapters.GameProtocol.Green/Wire/Game.cs`, `Adapters.GameProtocol.Yellow/Wire/Game.cs`
- Why fragile: Optional primitive zero-vs-absent behavior depends on generated nullable backing fields plus Mapperly conversion helpers; manually calling generated `ShouldSerialize*` in production would bypass the agreed boundary.
- Safe modification: Regenerate wire files from the same generator settings, keep production mappers free of `ShouldSerialize*`, and add serialized field-presence tests for any changed optional field.
- Test coverage: Focused Mapperly/presence tests exist in `Tests/Blue/BlueMapperTests.cs`, `Tests/Green/GreenUserDataMapperTests.cs`, `Tests/Yellow/YellowUserDataProtocolTests.cs`, and `Tests/Yellow/YellowCompatibilityResponseShapeTests.cs`; clean-regeneration drift is not enforced.

**AC15 row-shape interfaces must stay narrow:**
- Files: `Domain/Entities/IAc15SongPlayDatum.cs`, `Domain/Entities/IAc15SongBestDatum.cs`, `Domain/Entities/IAc15FavoriteSong.cs`, `Domain/Entities/IAc15RecentSong.cs`, `Domain/Entities/IAc15DanScoreDatum.cs`, `Domain/Entities/IAc15DanStageScoreDatum.cs`, `Domain/Entities/IAc15ShopItemState.cs`, `Domain/Entities/IAc15ShopSeasonState.cs`
- Why fragile: These interfaces are the approved alternative to repository-shaped persistence. Expanding them into broad behavior contracts would obscure era table ownership.
- Safe modification: Add only fields required by value-identical shared algorithms; keep era-specific behavior in concrete entity types, handlers, or explicit policy inputs.
- Test coverage: Shared AC15 tests cover current row-shape behavior in `Tests/Ac15/`, with era-specific coverage in `Tests/Blue/`, `Tests/Green/`, and `Tests/Yellow/`.

**Era enablement spans Host, adapters, catalogs, AdminApi, and WebUI:**
- Files: `Host/Program.cs`, `Infrastructure/DependencyInjection.cs`, `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs`, `Adapters.GameProtocol.Yellow/DependencyInjection.cs`, `Adapters.AdminApi/Controllers/GameDataController.cs`, `TaikoWebUI/Utilities/WebUiEra.cs`, `TaikoWebUI/Services/GameDataService.cs`
- Why fragile: Adding or removing an era requires consistent settings, DI, MVC application parts, protocol routes, AdminApi projections, data roots, and WebUI navigation.
- Safe modification: Update enabled-era configuration, route ownership, catalog initialization, AdminApi route support, and WebUI era lists together; test disabled-era absence and supported route behavior.
- Test coverage: Yellow, Blue, and Green have focused era route/config tests, but future era additions need their own coverage.

**Local operator data, proto inputs, and reverse-engineering evidence are machine-local:**
- Files: `.gitignore`, `Host/wwwroot/data/green/.gitkeep`, `Host/wwwroot/data/blue/.gitkeep`, `Host/wwwroot/data/yellow/.gitkeep`, `Infrastructure/GameDataCatalog/Green/GreenRequiredDataFiles.cs`, `Infrastructure/GameDataCatalog/Blue/BlueRequiredDataFiles.cs`, `Infrastructure/GameDataCatalog/Yellow/YellowRequiredDataFiles.cs`, `proto/yellow/yellow-final.proto`, `.tools/blue/battleuserdata-response-xrefs.md`
- Why fragile: Runtime catalog support and protocol mapping depend on local operator data and ignored proto/evidence inputs that are not fully reproducible from tracked files.
- Safe modification: Keep tests fixture-backed, treat `.tools/` and `proto/` as evidence inputs unless intentionally promoted, and document required operator data paths for each era.
- Test coverage: Loader tests cover many sidecars and optional local-data paths; full runtime compatibility still depends on cabinet/RPCS3 evidence.

**Migration history and EF snapshot are large and churn-prone:**
- Files: `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs`, `Infrastructure/Persistence/TaikoDbContext.Blue.cs`, `Infrastructure/Persistence/TaikoDbContext.Green.cs`, `Infrastructure/Persistence/TaikoDbContext.Yellow.cs`, `Infrastructure/Persistence/TaikoDbContext.Nijiiro.cs`
- Why fragile: Per-era table growth makes migration diffs large and cross-era schema mistakes hard to review.
- Safe modification: Review generated migrations against the relevant `TaikoDbContext.*.cs` partial and run database update/script verification for schema work.
- Test coverage: Handler and schema tests exercise many tables; migration drift is still manually reviewed.

## Scaling Limits

**SQLite single-file database is the central write bottleneck:**
- Current capacity: The default deployment uses one local SQLite database file.
- Limit: Concurrent protocol writes, registration, shop updates, and AdminApi mutations contend on one writer and expose race paths such as BAID allocation.
- Scaling path: Keep SQLite for local deployments, but wrap high-conflict writes in transactions with retry; consider a server-grade database for multi-cabinet deployments.
- Files: `Infrastructure/DependencyInjection.cs`, `Infrastructure/Persistence/TaikoDbContext.cs`, `Application/Ac15/Ac15MyDonEntryService.cs`, `Application/Ac15/Ac15NormalPlayWriter.cs`, `Application/Ac15/Ac15ItemShopPurchase.cs`

**Full-history WebUI responses do not scale with long-lived accounts:**
- Current capacity: Controllers return all play/best/history rows for a BAID.
- Limit: Large accounts increase SQL result size, memory pressure, network response size, and client render cost.
- Scaling path: Add paged history, per-song detail endpoints, and summary endpoints instead of loading every row for profile/high-score/song-list pages.
- Files: `Adapters.AdminApi/Controllers/PlayDataController.cs`, `Adapters.AdminApi/Controllers/PlayDataController.Yellow.cs`, `Adapters.AdminApi/Controllers/PlayHistoryController.cs`, `Adapters.AdminApi/Controllers/PlayHistoryController.Yellow.cs`, `TaikoWebUI/Pages/Profile.razor.cs`, `TaikoWebUI/Pages/HighScores.razor.cs`, `TaikoWebUI/Pages/SongList.razor.cs`

**Evidence and generated-wire workflow does not scale cleanly across more eras:**
- Current capacity: Blue, Green, and Yellow AC15 wire models are tracked, with local ignored proto/generator inputs available in this workspace.
- Limit: Each added era increases generated wire size, Mapperly map surface, and local evidence needed to prove optional presence semantics.
- Scaling path: Create a reproducible wire-generation command and per-era evidence manifest before adding more AC15 adapters.
- Files: `Adapters.GameProtocol.Blue/Wire/Game.cs`, `Adapters.GameProtocol.Green/Wire/Game.cs`, `Adapters.GameProtocol.Yellow/Wire/Game.cs`, `proto/blue/taiko.proto`, `proto/green/green.proto`, `proto/yellow/yellow-final.proto`, `.tools/protogen.exe`

## Dependencies at Risk

**Local protogen version is not a tracked build dependency:**
- Risk: Wire DTO output depends on a local `.tools/protogen.exe` and local tool package cache, while runtime packages are versioned in `Directory.Packages.props`.
- Impact: Another checkout can build tracked generated files but cannot regenerate or audit them reliably.
- Migration plan: Add a tracked tool manifest or script that installs the exact `protobuf-net.Protogen` version and runs the AC15 generation commands with nullable optional primitive settings.
- Files: `.tools/protogen.exe`, `.tools/.store/protobuf-net.protogen/3.2.52/`, `Directory.Packages.props`, `Adapters.GameProtocol.Blue/Wire/Game.cs`, `Adapters.GameProtocol.Green/Wire/Game.cs`, `Adapters.GameProtocol.Yellow/Wire/Game.cs`

**Beta package remains in the solution:**
- Risk: `System.CommandLine` beta APIs can change and complicate restore/upgrades.
- Impact: Utility builds can break on dependency movement even if the main Host is stable.
- Migration plan: Replace beta APIs with a stable package version when available, or pin and isolate the utility.
- Files: `Directory.Packages.props`, `LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj`

**Nightly CI publishes without running tests:**
- Risk: Release artifacts can be produced when `dotnet publish` succeeds but tests fail.
- Impact: Protocol, AdminApi, WebUI, or persistence regressions can ship to nightly artifacts.
- Migration plan: Add `dotnet test Tests/Tests.csproj --no-restore` after restore/build and before upload/release steps.
- Files: `.github/workflows/publishTLS.yml`, `Tests/Tests.csproj`

## Missing Critical Features

**Repeatable Yellow runtime evidence artifacts are not committed:**
- Problem: Phase 17 records full automated verification and user-confirmed RPCS3 smoke, but raw RPCS3 logs/captures are not committed.
- Blocks: Future agents cannot replay exact Yellow normal/Tokkun runtime evidence from repo state alone.
- Files: `.planning/milestones/v1.2-phases/17-yellow-runtime-verification-and-contract-closeout/17-VERIFICATION.md`, `.planning/milestones/v1.2-phases/17-yellow-runtime-verification-and-contract-closeout/17-YELLOW-CONTRACT.md`, `Adapters.GameProtocol.Yellow/Controllers/PlayResultController.cs`, `Tests/Yellow/YellowPlayResultHandlerTests.cs`

**Green challenge/tournament/AI/token/song-purchase semantics remain unimplemented:**
- Problem: Several Green handlers still return success or empty data without catalog-backed or persistence-backed behavior.
- Blocks: Full Green parity for those surfaces if clients rely on them.
- Files: `Application/Handlers/GetChallengeCompeQuery.Green.cs`, `Application/Handlers/TournamentCheckQuery.Green.cs`, `Application/Handlers/GetAiDataQuery.Green.cs`, `Application/Handlers/GetAiScoreQuery.Green.cs`, `Application/Handlers/GetTokenCountQuery.Green.cs`, `Application/Handlers/AddTokenCountCommand.Green.cs`, `Application/Handlers/PurchaseSongCommand.Green.cs`

**Yellow battle and real Banacoin authority are absent by contract:**
- Problem: These are explicit non-goals, not implementation gaps, but future work must not mistake success-shaped compatibility routes for wallet/payment or battle support.
- Blocks: Any claim of full Yellow battle/payment parity.
- Files: `.planning/milestones/v1.2-phases/17-yellow-runtime-verification-and-contract-closeout/17-YELLOW-CONTRACT.md`, `Adapters.GameProtocol.Yellow/Controllers/GetBanacoinInfoController.cs`, `Adapters.GameProtocol.Yellow/Controllers/BanacoinPaymentController.cs`, `Adapters.GameProtocol.Yellow/Controllers/BalanceCheckController.cs`, `Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs`

## Test Coverage Gaps

**CI coverage gate is missing:**
- What's not tested: The publish workflow does not run `dotnet test`.
- Files: `.github/workflows/publishTLS.yml`, `Tests/Tests.csproj`
- Risk: Regressions can reach artifacts despite existing local tests.
- Priority: High

**BAID allocation has no concurrency regression test:**
- What's not tested: Concurrent unknown-card `baid.php` / `mydonentry.php` flows across Nijiiro, Green, Blue, and Yellow.
- Files: `Application/Handlers/BaidQuery.Blue.cs`, `Application/Handlers/BaidQuery.Green.cs`, `Application/Handlers/BaidQuery.Yellow.cs`, `Application/Handlers/BaidQuery.Nijiiro.cs`, `Application/Ac15/Ac15MyDonEntryService.cs`
- Risk: Duplicate BAIDs or failed registrations under concurrent cabinet traffic.
- Priority: High

**Generated protocol regeneration drift is not enforced:**
- What's not tested: Regenerating AC15 wire files from canonical proto inputs and comparing them with tracked generated files.
- Files: `Adapters.GameProtocol.Blue/Wire/Game.cs`, `Adapters.GameProtocol.Green/Wire/Game.cs`, `Adapters.GameProtocol.Yellow/Wire/Game.cs`, `proto/blue/taiko.proto`, `proto/green/green.proto`, `proto/yellow/yellow-final.proto`, `.tools/protogen.exe`
- Risk: Generated protocol models drift from local proto files, especially optional presence/nullability.
- Priority: High

**Security deployment behavior lacks tests:**
- What's not tested: Local-mode auth bypass with non-loopback binding, permissive CORS, body/header logging redaction, anonymous auth throttling, and token storage hardening.
- Files: `Host/Program.cs`, `Infrastructure/Identity/AuthAwarePolicyEvaluator.cs`, `Adapters.AdminApi/Controllers/AuthController.cs`, `TaikoWebUI/Services/JwtAuthenticationStateProvider.cs`
- Risk: A local-convenience configuration can be exposed as a server deployment.
- Priority: High

**Runtime smoke evidence is not replayable from repository artifacts:**
- What's not tested: A scripted or captured Yellow normal/Tokkun RPCS3 request sequence that can be rerun after shared-core changes.
- Files: `.planning/milestones/v1.2-phases/17-yellow-runtime-verification-and-contract-closeout/17-VERIFICATION.md`, `.planning/milestones/v1.2-phases/17-yellow-runtime-verification-and-contract-closeout/17-YELLOW-CONTRACT.md`, `Adapters.GameProtocol.Yellow/Controllers/PlayResultController.cs`, `Tests/Yellow/YellowCompatibilityResponseShapeTests.cs`
- Risk: Yellow runtime compatibility can regress in a way automated server tests do not catch.
- Priority: Medium

**Future AC15 over-sharing is not covered by tests until a new era exists:**
- What's not tested: That a later AC15 era starts with unsupported features absent instead of inheriting Blue/Green/Yellow shared feature flags, limits, or wire placement.
- Files: `Application/Ac15/Ac15EraProfiles.cs`, `Application/Ac15/Ac15FeatureSet.cs`, `Application/Ac15/Ac15WirePlacement.cs`, `Host/Program.cs`
- Risk: Future Yellow-like or Red-like work can introduce fake compatibility through profile reuse.
- Priority: Medium

---

*Concerns audit: 2026-06-11*
