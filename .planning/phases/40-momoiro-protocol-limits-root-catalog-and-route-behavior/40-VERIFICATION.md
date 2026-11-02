---
phase: 40-momoiro-protocol-limits-root-catalog-and-route-behavior
verified: 2026-06-26T06:47:31Z
status: passed
score: "5/5 must-haves verified"
behavior_unverified: 0
overrides_applied: 0
deferred:
  - truth: "Exact native Don Point/reward cap is not implemented as a code-level limit in Phase 40."
    addressed_in: "Phase 42"
    evidence: "Phase 42 success criterion covers evidence-backed MOMOIRO Don Point and reward mutation; Phase 40 context and plans explicitly record Don Point/reward exact caps as field-presence/caveat only."
---

# Phase 40: MOMOIRO Protocol Limits, Root Catalog, and Route Behavior Verification Report

**Phase Goal:** MOMOIRO catalog data, protocol limits, and binary-proven route behavior are explicit, evidence-backed inputs for later readback and mutation.
**Verified:** 2026-06-26T06:47:31Z
**Status:** passed
**Re-verification:** No - initial verification

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|---|---|---|
| 1 | MOMOIRO catalog loading succeeds from `Host/wwwroot/data/momoiro/data` using root-level `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`. | VERIFIED | `MomoiroGameDataPaths` resolves those four root-level inputs through `PathHelper.GetDataPath(GameEra.Momoiro)`; `MomoiroRequiredDataFiles` requires them; `MomoiroEraGameDataCatalog.InitializeAsync` loads music, medley, tuning, and telop data. `musicinfo.xml` reports version `538116869` and size `380`; `musicmedleyinfo.xml` reports size `15`. `MomoiroCatalogLoaderTests` asserts 380 songs, 380 hash entries, and 760 encoded hash bytes. |
| 2 | Runtime code resolves MOMOIRO data through era path/settings abstractions instead of handler-local hardcoded filesystem paths. | VERIFIED | Source gate over `Adapters.GameProtocol.Momoiro`, `Application/Handlers`, and `Application/Ac15` found no `Host/wwwroot/data/momoiro`, `wwwroot\data\momoiro`, `File.*`, `Directory.*`, or handler/controller `Path.Combine` hits. Filesystem access is isolated in `Infrastructure/GameDataCatalog/Momoiro/*`; controllers consume `IGameDataCatalog`, `Mediator`, and `Momoiro()` catalog extension. |
| 3 | MOMOIRO has an explicit profile for byte widths, song ordering, favorite/recent limits, default-song flags, song hash, release flags, crown placement, Don Point/reward limits, and absent feature flags. | VERIFIED | `Ac15EraProfiles.Momoiro` exists with `CreateMomoiroLimits()`, `CrownPlacement = UserData`, `MaxFavoriteSongs = 5`, `MaxRecentSongs = 5`, `CrownSongCount = 380`, `CrownPackedBytes = 475`, and explicit absent feature flags for folders, Dani, and item shop. Song ordering/hash/default/release behavior flows from the Momoiro catalog hash table and shared AC15 codecs. `MomoiroProtocolLimitsTests` covers the explicit profile and low-confidence caveats. Exact native Don Point/reward cap is not overclaimed; it is deferred to Phase 42. |
| 4 | `recommend.php`, `defaultsong.php`, `songhash.php`, `telopcheck.php`, and `gettelop.php` are feature-complete for their binary-proven MOMOIRO route role, while `heartbeat.php` and `bookkeeping.php` are explicit static-result operational stubs if no stateful role is proven. | VERIFIED | Momoiro controllers dispatch `recommend.php`, `defaultsong.php`, `telopcheck.php`, and `gettelop.php` through `GetRecommendQuery`, `GetInitialDataQuery`, and `GetTelopQuery`; `songhash.php` reads `gameDataService.Momoiro()` and uses `Ac15SongHashCodec.EncodeTable`. Handler partials route Momoiro into `Ac15CatalogSnapshotFactory.FromMomoiro`, `Ac15CatalogReadbackService.BuildRecommendResponse`, and `BuildTelopResponse`. `HeartbeatController` returns static `Result/ComSvrStat/GameSvrStat = 1`; `BookkeepingController` returns static `Result = 1`. `MomoiroMetadataRouteTests` asserts concrete response fields. |
| 5 | Crown support is modeled as userdata-owned `hash_crown_flg`; no standalone `crownsdata.php` route is part of active MOMOIRO scope. | VERIFIED | `Adapters.GameProtocol.Momoiro/Wire/Game.cs` includes `UserDataResponse.HashCrownFlg` from `hash_crown_flg`; profile wire placement is `Ac15CrownWirePlacement.UserData`; route-surface tests include `crownsdata.php` in the proto-only/unsupported absence guard and assert no dedicated crown route. Unsupported-route grep over Momoiro controllers had no hits. |

**Score:** 5/5 truths verified (0 present behavior-unverified)

### Deferred Items

Items not yet met but explicitly addressed in later milestone phases.

| # | Item | Addressed In | Evidence |
|---|---|---|---|
| 1 | Exact native Don Point/reward cap is not modeled as a code-level Phase 40 limit. | Phase 42 | Phase 40 context says reward/Don Point mutation is Phase 42; `40-02-PLAN.md` says to record Don Point cap as deferred field presence only; Phase 42 success criteria cover evidence-backed Don Point/reward mutation. |

### Required Artifacts

| Artifact | Expected | Status | Details |
|---|---|---|---|
| `Application/Abstractions/IMomoiroCatalog.cs` | Typed Momoiro catalog contract | VERIFIED | Exposes `SongHashVersion`, `SongHashTable`, `MusicInfoFileOrder`, `MomoiroMusicInfos`, and `Telops`. |
| `Infrastructure/GameDataCatalog/Momoiro/*` | Root-level path helpers, required-file guard, catalog loader | VERIFIED | Loads required root files through `MomoiroGameDataPaths`; produces shared `MusicInfos`, song hash table, and telop dictionary. |
| `Infrastructure/DependencyInjection.cs` | Enabled-era catalog registration | VERIFIED | Registers `MomoiroEraGameDataCatalog`, `IMomoiroCatalog`, and `IEraGameDataCatalog` when `GameEra.Momoiro` is enabled. |
| `Application/Ac15/Ac15EraProfiles.cs` | Explicit Momoiro AC15 profile | VERIFIED | Contains `MomoiroFeatures`, `Momoiro`, `CreateMomoiroLimits()`, and `TryGet(GameEra.Momoiro)`. |
| `Application/Ac15/Ac15CatalogSnapshotFactory.cs` | Momoiro snapshot projection | VERIFIED | `FromMomoiro` projects song hash version, file-order songs, empty unsupported event folders, Momoiro telops, disabled item shop, and empty Taikojuku packs. |
| `Application/Handlers/GetInitialDataQuery.Momoiro.cs`, `GetRecommendQuery.Momoiro.cs`, `GetTelopQuery.Momoiro.cs` | Application-layer Momoiro metadata behavior | VERIFIED | All call `Ac15CatalogSnapshotFactory.FromMomoiro(gameDataService.Momoiro())`; recommend/telop handlers call shared AC15 readback services. |
| `Adapters.GameProtocol.Momoiro/Controllers/*` metadata controllers | Catalog-backed Momoiro routes plus static operational stubs | VERIFIED | Metadata controllers call Mediator/catalog abstractions; `HeartbeatController` and `BookkeepingController` are explicit static operational success routes. |
| `Adapters.GameProtocol.Momoiro/Mappers/*` | Mapperly-generated Momoiro response mapping | VERIFIED | Source partials exist; emitted `.g.cs` assigns recommendation and telop response fields. |
| `Host/wwwroot/data/momoiro/momoiro_telop_data.json` | Server-authored Momoiro telop sidecar | VERIFIED | File contains `[]`; temp-output Host build copied the file and generated `.gz`; raw `momoiro/data` is a junction in temp output. |
| `Tests/Momoiro/*` and touched `Tests/Ac15/*` | Behavioral regression coverage | VERIFIED | Focused serialized aggregate passed 27/27; full serialized suite passed 908/908. |

### Key Link Verification

| From | To | Via | Status | Details |
|---|---|---|---|---|
| `Infrastructure/DependencyInjection.cs` | `MomoiroEraGameDataCatalog` | Enabled-era singleton registration | WIRED | Manual grep found `enabledEras.Contains(GameEra.Momoiro)` and registrations for concrete, typed, and `IEraGameDataCatalog` services. |
| `Application/Ac15/Ac15CatalogSnapshotFactory.cs` | `IMomoiroCatalog` | `FromMomoiro` projection | WIRED | `FromMomoiro(IMomoiroCatalog momoiro)` consumes Momoiro song order, hash version, and telops. |
| `GetInitialDataQuery.cs` | `GetInitialDataQuery.Momoiro.cs` | `GameEra.Momoiro => HandleMomoiro` | WIRED | Dispatcher includes Momoiro arm and partial declaration; partial builds shared initial-data response from Momoiro snapshot. |
| `GetRecommendQuery.cs` | `GetRecommendQuery.Momoiro.cs` | `GameEra.Momoiro => HandleMomoiro` | WIRED | Dispatcher includes Momoiro arm; partial builds catalog-backed recommendation response. |
| `GetTelopQuery.cs` | `GetTelopQuery.Momoiro.cs` | `GameEra.Momoiro => HandleMomoiro` | WIRED | Dispatcher includes Momoiro arm; partial builds telop response from shared readback service. |
| `DefaultSongController.cs` | `GetInitialDataQuery.Momoiro.cs` | `Mediator.Send(new GetInitialDataQuery(GameEra.Momoiro))` | WIRED | Controller sends Momoiro query and compacts default-song flags through the Momoiro hash table. |
| `SongHashController.cs` | `IMomoiroCatalog` | `gameDataService.Momoiro()` and `Ac15SongHashCodec.EncodeTable` | WIRED | Controller returns Momoiro song hash version and encoded table. |
| `GetTelopController.cs` | `GetTelopMappers.g.cs` | Mediator response mapped to Momoiro wire | WIRED | Generated mapper assigns `Result`, `StartDatetime`, `EndDatetime`, and `Telop`, omitting `VerupNo`. |

### Data-Flow Trace (Level 4)

| Artifact | Data Variable | Source | Produces Real Data | Status |
|---|---|---|---|---|
| `MomoiroEraGameDataCatalog` | `MusicInfoFileOrder`, `SongHashTable`, `SongHashVersion` | `Host/wwwroot/data/momoiro/data/musicinfo.xml` | Yes - local file reports `size=380`, `version=538116869`; tests assert 380 rows/hash entries and 760 encoded bytes. | FLOWING |
| `MomoiroEraGameDataCatalog` | medley file order | `Host/wwwroot/data/momoiro/data/musicmedleyinfo.xml` | Yes - local file reports `size=15`; loader reads through `Ac15TaikojukuLoader`. | FLOWING |
| `MomoiroEraGameDataCatalog` | tuning/star data | `Host/wwwroot/data/momoiro/data/fumen/tuning.bin` | Yes - loader reads through `Ac15TuningLoader`; required-file test fails when `defmusic.bin` is omitted, proving required-file enforcement. | FLOWING |
| `Ac15CatalogSnapshotFactory.FromMomoiro` | snapshot song/telop data | `IMomoiroCatalog` | Yes for songs/hash; telop sidecar is intentionally empty `[]`. | FLOWING / EMPTY_BY_DESIGN |
| Momoiro metadata controllers | wire response fields | Application handlers and `IGameDataCatalog.Momoiro()` | Yes - tests assert nonzero recommend song, song hash version `538116869`, 48-byte default flags, 760-byte hash table, empty telop IDs, and static operational success. | FLOWING |

### Behavioral Spot-Checks

| Behavior | Command | Result | Status |
|---|---|---|---|
| Plan completion index | `node .codex\gsd-core\bin\gsd-tools.cjs query phase-plan-index 40` | `incomplete: []`, 5 plans with summaries | PASS |
| Proto cleanliness | `git status --porcelain -- proto\momoiro` | No output | PASS |
| Focused Momoiro/AC15 behavior suite | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Momoiro|FullyQualifiedName~Ac15SongHashCodec|FullyQualifiedName~Ac15CatalogReadbackService" --no-restore -- RunConfiguration.DisableParallelization=true` | Passed 27/27 | PASS |
| Mapperly generated-source emission | `dotnet build Adapters.GameProtocol.Momoiro/Adapters.GameProtocol.Momoiro.csproj /p:EmitCompilerGeneratedFiles=true --no-restore` | Passed, 0 warnings, 0 errors | PASS |
| Full serialized test suite | `dotnet test Tests/Tests.csproj --no-restore -- RunConfiguration.DisableParallelization=true` | Passed 908/908 | PASS |
| Full solution build | `dotnet build TaikoLocalServer.slnx --no-restore` | Passed, 0 warnings, 0 errors | PASS |
| Temp-output Host build | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build" --no-restore` | Passed, 0 warnings, 0 errors | PASS |
| Temp-output Momoiro sidecar | `Test-Path "$env:TEMP\TaikoLocalServer-host-build\wwwroot\data\momoiro\momoiro_telop_data.json"` | `True` | PASS |
| Unsupported route absence | `rg -n "shoppingresult\.php|bestscore\.php|communicationlog\.php|mainichisong\.php|crownsdata\.php" Adapters.GameProtocol.Momoiro\Controllers` | No hits | PASS |
| Path-abstraction source gate | `rg -n "Host/wwwroot/data/momoiro|wwwroot\\data\\momoiro|File\.|Directory\.|Path\.Combine" Adapters.GameProtocol.Momoiro Application\Handlers Application\Ac15` | No hits | PASS |

Note: the un-serialized aggregate test commands are documented in `40-VALIDATION.md` as exposing a shared Momoiro test-output race. This verification used serialized VSTest execution and does not claim the exact un-serialized commands are stable.

### Probe Execution

| Probe | Command | Result | Status |
|---|---|---|---|
| Phase-declared probes | `rg -n "probe-...\.sh|scripts/.../tests/probe-.*\.sh" .planning/phases/40...` | No script probe declarations found; only prose "probe" mentions in research. | SKIPPED |
| Conventional probes | `if (Test-Path scripts) { rg --files scripts | rg "(^|/)probe-.*\.sh$" }` | No `scripts/` directory in this checkout. | SKIPPED |

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
|---|---|---|---|---|
| MOCAT-01 | 40-01, 40-02, 40-05 | Root-level catalog loading with `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`. | SATISFIED | Path helpers, required-file guard, catalog loader, local file probes, and `MomoiroCatalogLoaderTests`; focused suite passed 27/27. |
| MOCAT-02 | 40-01 through 40-05 | Momoiro data paths through settings/path/catalog abstractions, not handler-local filesystem access. | SATISFIED | Source gate has no handler/controller hardcoded filesystem hits; controllers use `IGameDataCatalog`, `Mediator`, and Application handlers. |
| MOCAT-03 | 40-01, 40-02, 40-05 | Explicit Momoiro AC15 profile/limits. | SATISFIED_WITH_DEFERRED_CAVEAT | Profile and tests cover implemented byte/list/crown/feature limits; exact Don Point/reward cap remains documented field-presence only and deferred to Phase 42. |
| MOCAT-04 | 40-01, 40-03, 40-04, 40-05 | Catalog-backed metadata routes plus static operational heartbeat/bookkeeping. | SATISFIED | Controller/handler wiring, generated Mapperly output, `MomoiroMetadataRouteTests`, and focused suite pass. |
| MOCAT-05 | 40-01, 40-02, 40-04, 40-05 | Crown support userdata-owned through `hash_crown_flg`; no standalone `crownsdata.php`. | SATISFIED | Wire field exists in `UserDataResponse`, profile crown placement is `UserData`, route-surface tests and controller grep exclude `crownsdata.php`. |

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|---|---|---|---|---|
| `Application/Handlers/GetTelopQuery.cs` | 18 | Text `not implemented` in unsupported-era exception | INFO | This is a dispatcher guard for eras without telop support, not a Momoiro stub. Momoiro has an explicit dispatch arm and handler. |

No unreferenced `TBD`, `FIXME`, or `XXX` markers were found in Phase 40 modified source/test artifacts. Empty arrays in tests and the Momoiro telop sidecar are intentional expected-data cases.

### Human Verification Required

None for Phase 40. Cabinet/RPCS3 acceptance is not claimed here and remains Phase 44 scope.

### Gaps Summary

No blocking gaps found. The implemented server-side Phase 40 scope is verified: root catalog loading, path abstraction, explicit Momoiro profile/caveats, catalog-backed metadata routes, static operational stubs, Mapperly generated mapping, unsupported route absence, proto cleanliness, and build/test gates all pass.

The only notable scope boundary is exact native Don Point/reward cap behavior. Phase 40 records field presence and defers mutation/cap semantics to Phase 42; this verification does not treat that as delivered runtime behavior.

---

_Verified: 2026-06-26T06:47:31Z_
_Verifier: the agent (gsd-verifier)_
