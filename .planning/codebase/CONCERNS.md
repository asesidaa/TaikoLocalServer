# Codebase Concerns

**Analysis Date:** 2026-05-28

## Tech Debt

**Success-shaped protocol stubs still exist in live routes and handlers:**
- Issue: Several Green application handlers and Blue protocol controllers return successful empty/default responses without persistence or catalog validation.
- Files: `Application/Handlers/AddTokenCountCommand.Green.cs`, `Application/Handlers/GetAiDataQuery.Green.cs`, `Application/Handlers/GetAiScoreQuery.Green.cs`, `Application/Handlers/GetChallengeCompeQuery.Green.cs`, `Application/Handlers/GetSongIntroductionQuery.Green.cs`, `Application/Handlers/GetTokenCountQuery.Green.cs`, `Application/Handlers/PurchaseSongCommand.Green.cs`, `Application/Handlers/TournamentCheckQuery.Green.cs`, `Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs`, `Adapters.GameProtocol.Blue/Controllers/BanacoinPaymentController.cs`, `Adapters.GameProtocol.Blue/Controllers/BalanceCheckController.cs`, `Adapters.GameProtocol.Blue/Controllers/GetItemShopInfoController.cs`, `Adapters.GameProtocol.Blue/Controllers/ItemPurchaseController.cs`, `Adapters.GameProtocol.Blue/Controllers/RecommendController.cs`, `Adapters.GameProtocol.Blue/Controllers/TournamentCheckController.cs`
- Impact: Clients receive success for unsupported economy, payment, battle, tournament, recommendation, and token flows. This can hide missing persistence and make client behavior appear valid when nothing changed.
- Fix approach: For each stub, either implement the matching mediator-backed behavior or make unsupported behavior explicit with a non-success protocol response and route-level/source-guard tests that prevent accidental wiring.

**AC15 Green and Blue logic is duplicated across large partial handlers:**
- Issue: Blue and Green playresult, identity, userdata, taikojuku, initial-data, and profile flag logic follow similar shapes but live in separate partial files with repeated bitset, favorite/recent, Dan, and catalog projection patterns.
- Files: `Application/Handlers/UpdatePlayResultCommand.Green.cs`, `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, `Application/Handlers/BaidQuery.Green.cs`, `Application/Handlers/BaidQuery.Blue.cs`, `Application/Handlers/UserDataQuery.Green.cs`, `Application/Handlers/UserDataQuery.Blue.cs`, `Application/Handlers/GetTaikojukuQuery.Green.cs`, `Application/Handlers/GetTaikojukuQuery.Blue.cs`
- Impact: Fixes in one AC15 era can be missed in the other. The parallel source-guard tests reduce cross-era leakage but do not remove the duplicated decision logic.
- Fix approach: Extract era-neutral AC15 helpers for stage validation, favorite/recent trimming, flag packing, and catalog-backed Dan projections while keeping era-owned persistence entities and protocol byte widths.

**Nijiiro catalog remains a single loader/object with many responsibilities:**
- Issue: `NijiiroEraGameDataCatalog` owns path resolution, decryption, JSON loading, DTO shaping, dictionary construction, flag array sizing, locked content, QR codes, and token data in one class.
- Files: `Infrastructure/GameDataCatalog/Nijiiro/NijiiroEraGameDataCatalog.cs`
- Impact: Adding or changing one Nijiiro table requires editing a high-blast-radius file and makes startup/catalog failures harder to isolate.
- Fix approach: Follow the existing TODO in `Infrastructure/GameDataCatalog/Nijiiro/NijiiroEraGameDataCatalog.cs` and extract per-table loaders under `Infrastructure/GameDataCatalog/Nijiiro/Loaders/`, matching the smaller AC15 loader pattern in `Infrastructure/GameDataCatalog/Ac15/`.

**Protocol wire generation is not reproducible from tracked inputs alone:**
- Issue: Generated wire files are tracked, but the `proto/` directory and `.tools/protogen.exe` are ignored/local. Adapter projects reference the generated C# directly and do not regenerate from proto during build.
- Files: `.gitignore`, `Adapters.GameProtocol.Blue/Wire/Game.cs`, `Adapters.GameProtocol.Green/Wire/Game.cs`, `Adapters.GameProtocol.WwR08/Wire/Game.cs`, `Adapters.GameProtocol.CnR00/Wire/Game.cs`, `Adapters.GameProtocol.Blue/Adapters.GameProtocol.Blue.csproj`, `Adapters.GameProtocol.Green/Adapters.GameProtocol.Green.csproj`
- Impact: A clean checkout cannot audit or regenerate wire types without local ignored artifacts, and hand-edited generated files can drift from source protobuf definitions.
- Fix approach: Track canonical proto inputs or a documented generator script/tool manifest, then add a verification test that regenerated output matches `Adapters.GameProtocol.*/Wire/*.cs`.

**Mapperly diagnostics are suppressed in protocol adapters:**
- Issue: Green and Blue adapter projects suppress Mapperly diagnostics `RMG020` and `RMG012` while the mapper surface is central to protocol correctness.
- Files: `Adapters.GameProtocol.Blue/Adapters.GameProtocol.Blue.csproj`, `Adapters.GameProtocol.Green/Adapters.GameProtocol.Green.csproj`, `Adapters.GameProtocol.Blue/Mappers/`, `Adapters.GameProtocol.Green/Mappers/`
- Impact: Unmapped or mismatched protocol fields can be hidden during build, especially when generated wire classes change.
- Fix approach: Narrow suppressions to specific known mapper methods with comments, and add source or behavior tests for every intentionally ignored field.

**Warnings are not treated as build failures:**
- Issue: The shared build props explicitly keep warnings non-fatal.
- Files: `Directory.Build.props`
- Impact: Nullable, analyzer, source-generator, and API warnings can accumulate without failing CI or local verification.
- Fix approach: Raise warning strictness per project or warning category, starting with application/infrastructure projects before generated-wire projects.

**Admin/WebUI era routing exposes more Blue UI than the backend supports:**
- Issue: WebUI normalizes server-enabled `Blue` as a known era and can render Profile, High Scores, Play History, Song List, and Favorite actions for Blue, while AdminApi only supports Blue for a subset of data routes and Dani.
- Files: `TaikoWebUI/Utilities/WebUiEra.cs`, `TaikoWebUI/Components/NavMenu.razor`, `TaikoWebUI/Components/UserCard.razor`, `Adapters.AdminApi/Controllers/PlayDataController.cs`, `Adapters.AdminApi/Controllers/PlayHistoryController.cs`, `Adapters.AdminApi/Controllers/UserSettingsController.cs`, `Adapters.AdminApi/Controllers/FavoriteSongsController.cs`, `Adapters.AdminApi/Controllers/CustomizationCatalogController.cs`, `Adapters.AdminApi/Controllers/DanBestDataController.cs`, `Adapters.AdminApi/Controllers/GameDataController.cs`
- Impact: When Blue is enabled by server config, users can navigate to Blue WebUI pages that call endpoints returning `BadEra` or missing customization data.
- Fix approach: Gate WebUI page/menu exposure per supported surface, or add full Blue AdminApi implementations before allowing Blue in `AuthService.EnabledEras`.

## Known Bugs

**Concurrent new-user allocation can produce duplicate BAIDs:**
- Symptoms: Two first-time cards can both observe the same max BAID and return or insert the same next BAID.
- Files: `Application/Handlers/BaidQuery.Nijiiro.cs`, `Application/Handlers/BaidQuery.Green.cs`, `Application/Handlers/BaidQuery.Blue.cs`, `Application/Handlers/AddMyDonEntryCommand.Nijiiro.cs`, `Application/Handlers/AddMyDonEntryCommand.Green.cs`, `Application/Handlers/AddMyDonEntryCommand.Blue.cs`
- Trigger: Concurrent `BaidQuery`/`AddMyDonEntryCommand` requests for unknown access codes.
- Workaround: None in code; SQLite key constraints may reject one writer after the client has already seen a new BAID.
- Fix approach: Move BAID allocation behind a database sequence/table, transaction with retry, or unique insert-first flow.

**Blue WebUI navigation can lead to unsupported AdminApi calls:**
- Symptoms: Blue Profile, High Scores, Play History, Song List, Favorite, and customization pages can request Blue era APIs that return `BadEra`.
- Files: `TaikoWebUI/Utilities/WebUiEra.cs`, `TaikoWebUI/Pages/Profile.razor.cs`, `TaikoWebUI/Pages/HighScores.razor.cs`, `TaikoWebUI/Pages/PlayHistory.razor.cs`, `TaikoWebUI/Pages/SongList.razor.cs`, `Adapters.AdminApi/Controllers/PlayDataController.cs`, `Adapters.AdminApi/Controllers/PlayHistoryController.cs`, `Adapters.AdminApi/Controllers/UserSettingsController.cs`, `Adapters.AdminApi/Controllers/FavoriteSongsController.cs`, `Adapters.AdminApi/Controllers/CustomizationCatalogController.cs`
- Trigger: `AuthController.GetConfig` returns Blue in enabled eras and the WebUI renders that era in common navigation.
- Workaround: Keep Blue out of the server-enabled WebUI era list until matching AdminApi surfaces exist.
- Fix approach: Split "known protocol era" from "WebUI-supported pages" in `TaikoWebUI/Utilities/WebUiEra.cs`.

**Blue invalid medal overflow is treated as protocol success while dropping work:**
- Symptoms: Blue playresult with overflowing medal totals returns `1` without saving changes.
- Files: `Application/Handlers/UpdatePlayResultCommand.Blue.cs`
- Trigger: `GetDonmedal` or `GetKatsumedal` would overflow persisted totals.
- Workaround: None in code.
- Fix approach: Return a failure result consistently with Green invalid-payload handling and add a regression test in `Tests/Blue/BluePlayResultHandlerTests.cs`.

**Nijiiro Dan gaiden data is omitted from AdminApi/WebUI Dan best response:**
- Symptoms: Gaiden Dan achievements are not included in `DanBestDataController` or WebUI Dani display.
- Files: `Adapters.AdminApi/Controllers/DanBestDataController.cs`, `TaikoWebUI/Pages/DaniDojo.razor.cs`
- Trigger: Requesting Dan best data for a BAID with `DanType.Gaiden` rows.
- Workaround: Not detected.
- Fix approach: Add gaiden projection and UI grouping, then cover with tests under `Tests/` and `Tests/WebUi/`.

**Green token and song-purchase handlers report success without changing state:**
- Symptoms: Token-count and song-purchase flows can appear successful but do not debit tokens or unlock songs.
- Files: `Application/Handlers/AddTokenCountCommand.Green.cs`, `Application/Handlers/GetTokenCountQuery.Green.cs`, `Application/Handlers/PurchaseSongCommand.Green.cs`
- Trigger: Any internal or future route wiring to these Green handlers.
- Workaround: Existing protocol audit documentation treats these as not safely exposed; source guards should remain in place.
- Fix approach: Keep routes unexposed until catalog/token validation exists, then implement state changes with regression tests.

## Security Considerations

**Tracked local credentials and TLS artifacts:**
- Risk: JWT configuration and PFX certificate files are committed as local defaults. If reused outside a trusted local deployment, bearer tokens and HTTPS identity can be predictable or shared.
- Files: `Host/Configurations/AuthSettings.json`, `Host/Configurations/Kestrel.json`, `Host/Certificates/cert.pfx`, `Host/Certificates/root.pfx`, `Infrastructure/DependencyInjection.cs`, `Infrastructure/Identity/JwtTokenService.cs`
- Current mitigation: Authentication settings are configurable and HTTPS certificate paths are loaded through Kestrel configuration.
- Recommendations: Treat `Host/Configurations/AuthSettings.json` and `Host/Certificates/*.pfx` as development examples only, move deploy-time secrets to environment/user-secret storage, rotate the checked-in certificate material if it is ever used outside local development, and document required secret overrides.

**Local-mode authorization bypass is broad:**
- Risk: When `AuthSettings.AuthenticationRequired` is false, every `[Authorize]` policy succeeds, including admin-only policies, and WebUI synthesizes an admin principal for authorization views.
- Files: `Infrastructure/Identity/AuthAwarePolicyEvaluator.cs`, `Adapters.AdminApi/Authorization/ControllerAuthorizationExtensions.cs`, `TaikoWebUI/Services/JwtAuthenticationStateProvider.cs`, `TaikoWebUI/Program.cs`
- Current mitigation: The behavior is documented as "local mode" and keeps local use convenient.
- Recommendations: Add an explicit startup warning when binding to non-loopback URLs with authentication disabled, and consider refusing public bindings unless an override flag is set.

**HTTP logging can capture sensitive headers and request/response bodies:**
- Risk: Host logging enables `HttpLoggingFields.All` with request/response body limits and protocol controllers log full request objects or playresult dumps. This can capture passwords, bearer tokens, access codes, BAIDs, play history, and protobuf payloads.
- Files: `Host/Program.cs`, `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`, `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs`, `Adapters.GameProtocol.Blue/Controllers/BanacoinPaymentController.cs`, `Adapters.GameProtocol.Green/Controllers/ChallengeCompeController.cs`
- Current mitigation: Serilog request logging is structured, and body limits cap logged size.
- Recommendations: Exclude auth endpoints and `Authorization` headers, disable body logging by default, lower playresult dumps to development-only logging, and redact access codes/person IDs in protocol controller logs.

**CORS accepts any origin, method, and header:**
- Risk: A browser from any origin can call the AdminApi if it has access to a bearer token or if local-mode authorization is disabled.
- Files: `Host/Program.cs`, `TaikoWebUI/Services/JwtAuthenticationStateProvider.cs`
- Current mitigation: Bearer tokens are required when authentication is enabled.
- Recommendations: Restrict CORS to configured UI origins for non-local deployments and pair local-mode auth bypass with loopback-only binding.

**Anonymous auth endpoints disclose account state and have no throttling:**
- Risk: Login and register responses distinguish missing card, missing credential, unregistered user, wrong password, duplicate registration, and wrong last play time. No rate limiter is registered.
- Files: `Adapters.AdminApi/Controllers/AuthController.cs`, `Host/Program.cs`, `TaikoWebUI/Services/AuthService.cs`
- Current mitigation: Passwords are hashed with BCrypt and registration can require last-play-time or invite-code checks.
- Recommendations: Normalize external error messages, add rate limiting or lockout for anonymous auth endpoints, and keep detailed reasons server-side only.

**JWT bearer token is stored in browser localStorage:**
- Risk: Any XSS in the WebUI can read the bearer token and call AdminApi as the user.
- Files: `TaikoWebUI/Services/JwtAuthenticationStateProvider.cs`, `TaikoWebUI/Services/AuthService.cs`, `TaikoWebUI/Program.cs`
- Current mitigation: Token lifetime is 24 hours in `Infrastructure/Identity/JwtTokenService.cs`, and expired/invalid local tokens are removed client-side.
- Recommendations: Add CSP and XSS hardening headers in `Host/Program.cs`, avoid rendering untrusted HTML, and consider shorter token lifetimes or refresh-token rotation if the server is exposed beyond a trusted LAN.

## Performance Bottlenecks

**AdminApi play-data endpoints load full user histories into memory:**
- Problem: Play data and play history read all best/play rows for a BAID, then do repeated in-memory filtering/grouping per song/difficulty.
- Files: `Adapters.AdminApi/Controllers/PlayDataController.cs`, `Adapters.AdminApi/Controllers/PlayHistoryController.cs`, `TaikoWebUI/Pages/HighScores.razor.cs`, `TaikoWebUI/Pages/PlayHistory.razor.cs`, `TaikoWebUI/Pages/SongList.razor.cs`
- Cause: The response model returns full history/detail data and the controller builds counts/recent rows in memory.
- Improvement path: Add pagination or route-specific projections for high scores, song list, and per-song history; push counts and recent-row limits into SQL.

**GetAllUserSetting is an N+1 query path:**
- Problem: The admin settings endpoint loads every user and then calls `GetOrCreateNijiiroSaveDataAsync` per user.
- Files: `Adapters.AdminApi/Controllers/UserSettingsController.cs`
- Cause: Save-data loading is inside a loop and can write missing rows while servicing a read endpoint.
- Improvement path: Batch-load save data by BAID, return only paged data, and separate read-only listing from "create missing defaults" repair behavior.

**Startup catalog loading is eager and can do large synchronous filesystem work:**
- Problem: Host startup migrates the database, initializes every enabled catalog, decrypts/copies datatable files, and loads all catalog dictionaries before serving requests.
- Files: `Host/Program.cs`, `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs`, `Infrastructure/GameDataCatalog/Nijiiro/NijiiroEraGameDataCatalog.cs`, `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`, `Infrastructure/GameDataCatalog/Blue/BlueEraGameDataCatalog.cs`
- Cause: Catalogs are singleton in-memory dictionaries initialized once at startup.
- Improvement path: Keep required catalog validation eager, but split optional/heavy assets into lazy loaders or cached background initialization with readiness diagnostics.

**Green and Blue playresult handlers perform multiple per-stage database lookups:**
- Problem: Each playresult can perform per-stage best/favorite/recent queries and a second save pass to trim recent songs.
- Files: `Application/Handlers/UpdatePlayResultCommand.Green.cs`, `Application/Handlers/UpdatePlayResultCommand.Blue.cs`
- Cause: Upsert logic is written directly inside large handlers with per-stage EF calls.
- Improvement path: Preload existing best/favorite/recent rows for the BAID, update tracked rows in memory, and save once after trimming.

## Fragile Areas

**Playresult persistence and binary/protobuf mapping are high-risk edit zones:**
- Files: `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`, `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs`, `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`, `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs`, `Application/Handlers/UpdatePlayResultCommand.Green.cs`, `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, `Tests/Green/GreenPlayResultHandlerTests.cs`, `Tests/Blue/BluePlayResultHandlerTests.cs`
- Why fragile: One request drives save data, best rows, play rows, Dan rows, favorite/recent state, shop medals, ghost data, and flag byte arrays. Several values are reverse-engineered and stage-mode dependent.
- Safe modification: Add a failing handler or mapper test for the exact wire/state case first, then keep era-specific persistence separate from shared AC15 helper extraction.
- Test coverage: Strong for Green and growing for Blue, but deferred Blue battle/Tokkun fields are only logged/ignored in `Application/Handlers/UpdatePlayResultCommand.Blue.cs`.

**Era enablement spans configuration, DI, MVC application parts, WebUI routing, and catalog loading:**
- Files: `Host/Program.cs`, `Infrastructure/DependencyInjection.cs`, `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs`, `TaikoWebUI/Utilities/WebUiEra.cs`, `TaikoWebUI/Services/GameDataService.cs`, `Adapters.AdminApi/Controllers/GameDataController.cs`
- Why fragile: Adding an era requires consistent support across server settings, enabled-era service registration, controller routing, AdminApi projections, and WebUI navigation.
- Safe modification: Update route/controller support and WebUI menus together, then add source tests like `Tests/Blue/BlueHostProgramSourceTests.cs` plus behavior tests for AdminApi/WebUI endpoints.
- Test coverage: Blue source guards exist, but broad WebUI navigation compatibility for partially supported eras is not covered.

**Generated wire files are large, hand-present code surfaces:**
- Files: `Adapters.GameProtocol.Blue/Wire/Game.cs`, `Adapters.GameProtocol.Green/Wire/Game.cs`, `Adapters.GameProtocol.WwR08/Wire/Game.cs`, `Adapters.GameProtocol.CnR00/Wire/Game.cs`, `.gitignore`
- Why fragile: Files are generated, thousands of lines, and their ignored proto inputs are not available in a clean tracked state.
- Safe modification: Do not edit generated wire files manually; regenerate from documented inputs and diff generated output.
- Test coverage: Route/wire presence tests exist under `Tests/Blue/`, but clean-regeneration drift is not enforced.

**Migration history and EF snapshot are large and churn-prone:**
- Files: `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs`, `Infrastructure/Persistence/Migrations/20260527193739_AddBlueDaniSupport.Designer.cs`, `Infrastructure/Persistence/Migrations/20260527184243_AddBluePlayResultSupport.Designer.cs`, `Infrastructure/Persistence/TaikoDbContext.Blue.cs`, `Infrastructure/Persistence/TaikoDbContext.Green.cs`, `Infrastructure/Persistence/TaikoDbContext.Nijiiro.cs`
- Why fragile: Per-era tables multiply model-snapshot size and make accidental migration changes hard to review.
- Safe modification: Review generated migrations against the relevant `TaikoDbContext.*.cs` partial and run a migration script/update verification before committing schema work.
- Test coverage: Entity/handler tests exercise many tables, but CI does not run `dotnet test` automatically.

**Local game data and extraction outputs are intentionally machine-local:**
- Files: `.gitignore`, `Infrastructure/GameDataCatalog/Green/GreenRequiredDataFiles.cs`, `Infrastructure/GameDataCatalog/Blue/BlueRequiredDataFiles.cs`, `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`, `Infrastructure/GameDataCatalog/Blue/BlueEraGameDataCatalog.cs`, `Host/wwwroot/data/green/.gitkeep`, `Host/wwwroot/data/blue/.gitkeep`
- Why fragile: Green and Blue startup require local datatable inputs that are intentionally ignored, and Green can generate customization JSON on first startup.
- Safe modification: Keep tests self-contained with fixtures and never depend on ignored local `Host/wwwroot/data/*/data` files for CI-critical coverage.
- Test coverage: Loader tests include local-data-aware cases, but full runtime verification depends on operator-provided files.

## Scaling Limits

**SQLite single-file database is the central write bottleneck:**
- Current capacity: One local `taiko.db3` database file is used by default.
- Limit: Concurrent protocol writes, user registration, and AdminApi repair/create operations can contend on the same SQLite database and expose race paths such as BAID allocation.
- Scaling path: Keep SQLite for local deployments, but isolate write-heavy update paths in transactions with retry; consider a server-grade database if multi-cabinet concurrent production use is required.
- Files: `Infrastructure/DependencyInjection.cs`, `Infrastructure/Persistence/TaikoDbContext.cs`, `Application/Handlers/AddMyDonEntryCommand.Green.cs`, `Application/Handlers/AddMyDonEntryCommand.Blue.cs`, `Application/Handlers/UpdatePlayResultCommand.Green.cs`, `Application/Handlers/UpdatePlayResultCommand.Blue.cs`

**Full-history WebUI responses do not scale with long-lived accounts:**
- Current capacity: Controllers return all play/best/history rows for a BAID.
- Limit: Large accounts increase response size, memory pressure, and client render cost.
- Scaling path: Add paged history, per-song detail endpoints, and summary endpoints instead of loading every row for High Scores/Song List/Profile.
- Files: `Adapters.AdminApi/Controllers/PlayDataController.cs`, `Adapters.AdminApi/Controllers/PlayHistoryController.cs`, `TaikoWebUI/Pages/Profile.razor.cs`, `TaikoWebUI/Pages/HighScores.razor.cs`, `TaikoWebUI/Pages/SongList.razor.cs`

## Dependencies at Risk

**Beta packages are used in runtime projects:**
- Risk: Beta package APIs and behavior can change, and dependency resolution may become harder around major runtime upgrades.
- Impact: Build or runtime behavior can shift when package versions move.
- Migration plan: Track usage and replace beta dependencies with stable packages where feasible.
- Files: `Directory.Packages.props`, `LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj`

**Nightly CI publishes without running tests:**
- Risk: A release artifact can be created even when unit/source tests fail.
- Impact: Broken protocol, AdminApi, or WebUI behavior can ship if `dotnet publish` succeeds.
- Migration plan: Add a CI step for `dotnet test Tests/Tests.csproj --no-restore` after restore/build and before upload/release creation.
- Files: `.github/workflows/publishTLS.yml`, `Tests/Tests.csproj`

## Missing Critical Features

**Blue battle/Tokkun/Banacoin/shop behavior is intentionally shallow or absent:**
- Problem: Blue battle userdata, Banacoin payment/balance, item shop, reward execution, tournament, and recommendation routes return stubs; Blue playresult logs battle/Tokkun data as deferred.
- Blocks: Full Blue cabinet parity for battle/payment/shop-related flows.
- Files: `Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs`, `Adapters.GameProtocol.Blue/Controllers/BanacoinPaymentController.cs`, `Adapters.GameProtocol.Blue/Controllers/BalanceCheckController.cs`, `Adapters.GameProtocol.Blue/Controllers/GetItemShopInfoController.cs`, `Adapters.GameProtocol.Blue/Controllers/ItemPurchaseController.cs`, `Adapters.GameProtocol.Blue/Controllers/RewardExecutionController.cs`, `Application/Handlers/UpdatePlayResultCommand.Blue.cs`

**Blue AdminApi/WebUI support is incomplete outside Dani and game-data projection:**
- Problem: Blue has DanBestData and GameData support but lacks Blue `PlayData`, `PlayHistory`, `UserSettings`, `FavoriteSongs`, and customization endpoints.
- Blocks: WebUI pages for Blue profile, score browsing, song list, history, favorites, and customization.
- Files: `Adapters.AdminApi/Controllers/DanBestDataController.cs`, `Adapters.AdminApi/Controllers/GameDataController.cs`, `Adapters.AdminApi/Controllers/PlayDataController.cs`, `Adapters.AdminApi/Controllers/PlayHistoryController.cs`, `Adapters.AdminApi/Controllers/UserSettingsController.cs`, `Adapters.AdminApi/Controllers/FavoriteSongsController.cs`, `Adapters.AdminApi/Controllers/CustomizationCatalogController.cs`, `TaikoWebUI/Utilities/WebUiEra.cs`

**Green competition, AI, song-introduction, and token/purchase flows are not real implementations:**
- Problem: Handlers return empty or success-shaped responses for features that still need real source evidence and persistence.
- Blocks: Green feature parity for challenge/tournament, AI-like surfaces if routed, song introduction, token count, and song purchase flows.
- Files: `Application/Handlers/GetChallengeCompeQuery.Green.cs`, `Application/Handlers/TournamentCheckQuery.Green.cs`, `Application/Handlers/GetAiDataQuery.Green.cs`, `Application/Handlers/GetAiScoreQuery.Green.cs`, `Application/Handlers/GetSongIntroductionQuery.Green.cs`, `Application/Handlers/GetTokenCountQuery.Green.cs`, `Application/Handlers/PurchaseSongCommand.Green.cs`

## Test Coverage Gaps

**CI coverage gate is missing:**
- What's not tested: Pull/publish workflow does not run `dotnet test`.
- Files: `.github/workflows/publishTLS.yml`, `Tests/Tests.csproj`
- Risk: Regressions can reach artifacts despite existing tests.
- Priority: High

**Security configuration behavior lacks deployment-safety tests:**
- What's not tested: Local-mode auth bypass with non-loopback binding, permissive CORS, body/header logging redaction, and committed/default secret override requirements.
- Files: `Host/Program.cs`, `Infrastructure/Identity/AuthAwarePolicyEvaluator.cs`, `Adapters.AdminApi/Controllers/AuthController.cs`, `TaikoWebUI/Services/JwtAuthenticationStateProvider.cs`
- Risk: A local-convenience configuration can be exposed as a server deployment.
- Priority: High

**BAID allocation has no concurrency regression test:**
- What's not tested: Concurrent unknown-card registration/query flows across Nijiiro, Green, and Blue.
- Files: `Application/Handlers/BaidQuery.Nijiiro.cs`, `Application/Handlers/BaidQuery.Green.cs`, `Application/Handlers/BaidQuery.Blue.cs`, `Application/Handlers/AddMyDonEntryCommand.Nijiiro.cs`, `Application/Handlers/AddMyDonEntryCommand.Green.cs`, `Application/Handlers/AddMyDonEntryCommand.Blue.cs`
- Risk: Duplicate BAIDs or failed registrations under concurrent cabinet traffic.
- Priority: High

**Blue WebUI/AdminApi partial-era routing is not covered end-to-end:**
- What's not tested: A Blue-enabled auth config driving common WebUI navigation to Blue Profile/HighScores/PlayHistory/SongList and the matching AdminApi responses.
- Files: `TaikoWebUI/Utilities/WebUiEra.cs`, `TaikoWebUI/Components/NavMenu.razor`, `TaikoWebUI/Components/UserCard.razor`, `Adapters.AdminApi/Controllers/PlayDataController.cs`, `Adapters.AdminApi/Controllers/PlayHistoryController.cs`, `Adapters.AdminApi/Controllers/UserSettingsController.cs`
- Risk: Blue is visible in UI before the backend supports the route set.
- Priority: Medium

**Generated protocol drift is not enforced:**
- What's not tested: Regenerating `Adapters.GameProtocol.*/Wire/*.cs` from canonical proto inputs and comparing to tracked generated files.
- Files: `Adapters.GameProtocol.Blue/Wire/Game.cs`, `Adapters.GameProtocol.Green/Wire/Game.cs`, `Adapters.GameProtocol.WwR08/Wire/Game.cs`, `Adapters.GameProtocol.CnR00/Wire/Game.cs`, `.gitignore`
- Risk: Generated protocol models drift from ignored local proto files.
- Priority: Medium

---

*Concerns audit: 2026-05-28*
