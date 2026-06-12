# Architecture Patterns

**Domain:** Red AC15 support in TaikoLocalServer
**Researched:** 2026-06-12
**Overall confidence:** HIGH for brownfield integration points; MEDIUM for exact Red route/version/config-root and challenge competition semantics until runtime/log/IDA evidence closes them.

## Correction Note

The requirements review narrowed the architectural guidance. Keep Red integration simple: Red adapter/wire/Host registration, one shared-AC15 catalog/profile integration, Red-owned state over existing shared services, narrow Tokkun, reward/progression compatibility, and Don Challenge evidence/compatibility. Do not implement stateful challenge tables or `challengecompe.php` semantics until runtime/client evidence proves that contract. Do not touch previous eras except to preserve shared-code behavior.

## Recommendation

Add Red as a first-class AC15 era with the same outer architecture as Yellow, but do not treat Red as a Yellow clone. Red should reuse the existing `Application/Ac15` core for normal scores, crowns, self-best, Dani, catalog readback, user-data composition, and typed EF helpers where the Red proto/data shape matches. Red must own its adapter, wire DTOs, routes, catalog, persistence tables, mappers, tests, and AdminApi/WebUI switches.

Challenge competition is the key Red-specific module. Blue, Green, and Yellow currently have challenge competition proto/controller surfaces, but the code and milestone correction show those are inactive compatibility stubs for newer eras. Red and older AC15 should own meaningful Don Challenge / challenge competition behavior. Do not upgrade Green/Blue/Yellow challenge stubs while implementing Red unless separate runtime evidence proves those newer eras call the feature.

The recommended flow is:

```text
Red controller route + direct protobuf transport
  -> Red generated wire DTO
  -> Red Mapperly mapper plus explicit protocol transforms
  -> Common/AC15 canonical request
  -> Mediator handler dispatch by GameEra.Red
  -> Red Ac15EraProfile + Red catalog + Red-owned DbSets via ITaikoDbContext
  -> shared Application/Ac15 service only where evidence matches
  -> Red mapper to Red generated wire response
```

## Component Boundaries

| Component | Responsibility | Red Integration |
|-----------|----------------|-----------------|
| `Domain` | Era enum, entity shapes, capability interfaces. | Add `GameEra.Red`, `UserSaveDataRed`, Red play/best/favorite/recent/Dani/Tokkun/challenge/reward entities. Keep Blue, Green, Yellow, and Red gameplay tables separate. |
| `Application/Abstractions` | Catalog and `ITaikoDbContext` contracts. | Add `IRedCatalog`, `ITaikoDbContext.Red.cs`, and `CatalogExtensions.Red()`. Keep `ITaikoDbContext` visible; do not introduce repository-shaped persistence. |
| `Application/Ac15` | Shared AC15 services and profiles. | Add `Ac15EraProfiles.Red`, `RedAc15UserDataAdapter`, Red Mapperly projections in `Ac15NormalPlayMapper`/`Ac15DaniMapper`, and `Ac15CatalogSnapshotFactory.FromRed`. Use a Red-specific feature set with item shop disabled. |
| `Application/Handlers` | Mediator dispatch and era behavior. | Add `.Red.cs` partials to the existing handlers. Bind Red DbSets directly into shared typed helpers. Route Tokkun before normal writes. Route challenge competition through a Red-owned handler. |
| `Infrastructure/GameDataCatalog/Red` | Filesystem Red catalog and loaders. | Load Red `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, tuning, sidecars, movies, folders, telops, recommendations, and challenge data only after source/root evidence is selected. |
| `Infrastructure/Persistence` | EF model/migrations. | Add `TaikoDbContext.Red.cs` and migration entries for Red-owned tables. Do not share Yellow/Blue/Green save, score, shop, Tokkun, or challenge rows. |
| `Adapters.GameProtocol.Red` | HTTP route ownership, generated wire, controllers, mappers. | New adapter project from `proto/red/taiko.proto` and `proto/red/vsinterface.proto`. Routes stay Red-owned under the proven Red prefix. |
| `Host` | Era enablement, application part gating, content-type fallback, data copy/debug junctions. | Add Red references, settings, `AddGameProtocolRed`, disabled-era application-part removal, Red route prefix in protobuf fallback, Red data exclusion, Red sidecar copy, and Red debug data junction. |
| `Adapters.AdminApi` | Admin era routing and DTO conversion. | Existing `EraRoute.TryParse` will accept Red after enum addition, but each controller switch needs Red cases backed by Red state/catalog. |
| `TaikoWebUI` | Era selector and API URL construction. | Add Red to `WebUiEra.Supported/Known/IsAc15` and ensure service calls use `api/Red/...` routes. |

## Shared vs Era-Owned Decisions

| Area | Reuse | Red-Owned |
|------|-------|-----------|
| Wire DTOs | None. Use protobuf-net conventions and nullable optional primitives. | `Adapters.GameProtocol.Red/Wire/Game.cs` and `VsInterface.cs`; never reuse Yellow wire because field numbers and feature fields differ. |
| Controllers | Pattern only. | Red route prefix, controller list, logging text, request/response mapper calls. Unsupported routes stay absent. |
| Normal play | `Ac15NormalPlayWriter`, stage filter policies, score/crown mapping where Red fields match. | Red `UpdatePlayResultCommand.Red.cs` special-mode gates, Red DbSet bindings, Red reward/donpoint mutation. |
| Userdata | `Ac15UserDataService`, profile counters, bitset helpers, recommendation/favorite/recent composition. | Red `UserSaveDataRed`, Red response field placement for `reward_progress`, `total_get_donpoint`, `total_use_donpoint`, `is_challengecompe`, `tokkun_tutorial_flg`, and Red-only flags. |
| Crowns | `Ac15CrownService`; Red proto has `CrownsDataRequest/Response` with `hash_crown_flg`. | Red `CrownsDataController` and mapper placement. |
| Dani/Taikojuku | `Ac15DaniWriter`, `Ac15DaniReadback`, `Ac15TaikojukuService`, `Ac15TaikojukuLoader` where XML shape matches. | Red catalog types, Red DbSets, Red Mapperly projections, Red verup sidecar if required. |
| Item shop/medals | None for Red by default. | Red proto does not expose Yellow `getitemshopinfo.php` or `itempurchase.php`, and Red uses Don-point/reward fields instead of Don/Katsu medals. Do not add Red shop state or shop routes without new evidence. |
| Tokkun | Blue/Yellow no-cross-mode pattern and `PlayMode.Tokkun = 3` if Red evidence confirms. | Red `RedTokkunStageResult`, nullable `UserSaveDataRed.TokkunTutorialFlg`, Red classifier tests, Red readback only through supported fields. |
| Banacoin compatibility | Stateless compatibility pattern only. | Red routes and parser-visible response shape must come from Red wire/runtime evidence. No wallet/payment/transaction state. |
| Challenge competition | `CommonChallengeCompeResponse` shape and Mapperly placement pattern can be reused. | Red catalog/state/service/handler semantics. Newer-era stubs are not behavior evidence. |

## Red Feature Profile

Start with a Red-specific `Ac15FeatureSet`, not the current Blue/Green/Yellow all-on feature set. The likely profile shape is:

```csharp
public static Ac15EraProfile Red { get; } = new(
    GameEra.Red,
    new Ac15FeatureSet(
        NormalPlay: true,
        UserData: true,
        SelfBest: true,
        Crowns: true,
        InitialData: true,
        Folders: true,
        Telops: true,
        Recommendations: true,
        Taikojuku: true,
        Dani: true,
        ItemShop: false),
    CreateCommonLimits(),
    new Ac15WirePlacement(
        CrownPlacement: Ac15CrownWirePlacement.DedicatedEndpoint,
        HasInitialDataItemShopRows: false,
        HasInitialDataLegalTermsRows: true,
        HasTokkunTutorialFlagInUserData: true));
```

Treat this as a starting contract, not final proof. Phase 1 must verify the Red route prefix, startup HDD version mapping, selected config root, and Red initial-data field placement. `HasInitialDataLegalTermsRows` and Tokkun readback stay evidence-gated if runtime/wire checks contradict the proto.

## Challenge Competition Ownership

Red challenge competition should be modeled as an older-AC15 capability that is currently called only by Red. The right ownership split is:

| Layer | Responsibility |
|-------|----------------|
| Red proto/wire | `ChallengeCompeRequest/Response`, `UserDataResponse.is_challengecompe`, and `PlayResultRequest.StageData` arrays `ary_challenge_id`, `ary_user_compe_id`, `ary_bng_compe_id`. |
| Red catalog | Source active challenge definitions, track lists, song/course/stage_mode/option defaults, and availability windows from Red data/runtime evidence. Local data has `dojochallenge_*.nut` assets and `present.xml` Don-point reward ladders, but no obvious `challenge*.xml`; catalog discovery is a required phase task. |
| Red persistence | Store user challenge participation and per-track high score/progress in Red-owned tables. Do not infer these from Yellow, and do not write Green/Blue/Yellow challenge rows. |
| Red playresult handler | Capture challenge result arrays from Red playresult stages before or alongside normal save. Update Red challenge progress only when a stage has challenge IDs and the catalog recognizes the competition/track. |
| `GetChallengeCompeQuery.Red` | Return catalog-backed active challenge rows plus Red user high scores through `CommonChallengeCompeResponse`. Return no challenge rows when no active/proven challenge catalog exists, rather than pretending Green/Yellow empty stubs are full behavior. |
| Admin/WebUI | Optional later readback for Red challenge progress. Keep it Red-only until older era reuse is actually needed. |

Do not make `GetChallengeCompeQueryHandler` a generic AC15 service for all eras yet. It should gain dependencies such as `ITaikoDbContext` and `IGameDataCatalog` because Red needs real state/catalog, but its switch should add `GameEra.Red` only. Green/Yellow handlers can remain stubbed or be explicitly documented as inactive compatibility paths.

## Red Persistence Shape

Add separate Red entities and DbSets, following Yellow names but adjusting fields:

| Entity | Purpose | Notes |
|--------|---------|-------|
| `UserSaveDataRed` | Red profile, flags, counters, Don-point/reward state, Tokkun tutorial, challenge flags. | Include Red fields from proto: `RewardPtn`, `RewardProgress`, `TotalGetDonpoint`, `TotalUseDonpoint`, `DifficultyTutorialFlg`, `TokkunTutorialFlg`, `IsChallengeCompe`, `IsTojiru`, `IsExplain`. Do not implement `IAc15MedalSaveData` unless Red truly uses medal semantics; prefer a Red Don-point capability or Red-specific mutation. |
| `SongPlayDatumRed` / `SongBestDatumRed` | Normal play history and self-best/crowns. | Implement `IAc15SongPlayDatum` / `IAc15SongBestDatum` so `Ac15NormalPlayWriter` can be reused. |
| `RedFavoriteSongs` / `RedRecentSongs` | User lists. | Same typed helper pattern as Yellow. |
| `DanScoreDatumRed` / `DanStageScoreDatumRed` | Dani state. | Implement `IAc15DanScoreDatum` / `IAc15DanStageScoreDatum` and add Mapperly methods. |
| `RedTokkunStageResult` | Append-only raw Tokkun history. | Same no-cross-write principle as Blue/Yellow, Red-owned table. |
| `RedChallengeCompeProgress` / `RedChallengeCompeTrackBest` | Challenge competition state. | Key by `Baid`, competition kind, `CompeId`, `TrackNo`, and catalog track identity. Keep raw uploaded IDs if semantics are still being learned. |
| `RedRewardProgress` if needed | Reward ladder tracking if `present.xml` cannot live directly on `UserSaveDataRed`. | Do not overload Yellow shop season state for Don-point progression. |

The important rule is to bind concrete DbSets directly inside Red handler partials:

```csharp
private Ac15NormalPlayTables<SongPlayDatumRed, SongBestDatumRed, RedFavoriteSongs, RedRecentSongs> RedNormalPlayTables()
    => new(
        context.SongPlayDataRed,
        context.SongBestDataRed,
        context.RedFavoriteSongs,
        context.RedRecentSongs,
        Ac15NormalPlayMapper.ToRedSongPlayDatum,
        Ac15NormalPlayMapper.ToRedSongBestDatum);
```

That preserves the user's no-repository constraint while still reusing the shared algorithm.

## Catalog Layer

Create `Application/Catalog/Red` and `Infrastructure/GameDataCatalog/Red` instead of making Yellow catalog generic. Red local data currently exposes multiple config roots:

- `config/ST5100-1`
- `config/ST5100-7`
- `config/ST7100-1`
- `config/ST8100-1`

Each contains the core config files observed for Red such as `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, `chassisinfo.xml`, and `spacialbaid.xml`. Do not lock a single root from the folder names alone. The first Red phase should prove which root the target runtime requests through client config/logs/IDA/RPCS3. If multiple roots must be supported, model the selected root as Red settings/catalog configuration rather than hardcoding `ST5100-1`.

Recommended Red catalog interfaces:

| Interface/Type | Purpose |
|----------------|---------|
| `IRedCatalog` | Red music, Taikojuku, telop, folder, recommend, movie, reward, challenge, customization readback. |
| `RedGameDataPaths` | `PathHelper.GetDataPath(GameEra.Red)/data` plus selected config root. |
| `RedRequiredDataFiles` | Fail fast for the proven required files only. |
| `RedMusicInfoLoader`, `RedTuningLoader`, `RedTaikojukuLoader` | Thin wrappers around shared AC15 loaders where the data shape matches. |
| `RedPresentRewardLoader` | Parse `present.xml` into Red reward/Don-point catalog rows. |
| `RedChallengeCompeLoader` | Evidence-gated loader for challenge definitions. It may start from sidecar JSON if no authoritative XML is found. |
| `RedEventFolderLoader`, `RedTelopLoader`, `RedRecommendLoader`, `RedMovieLoader` | Era-owned sidecar loaders. Add committed empty JSON files where the route exists but the data is intentionally empty. |

Do not add `RedItemShopLoader` or `red_item_shop_data.json` unless new Red evidence shows an item-shop route. Red's `present.xml` and Don-point progression are not Yellow item-shop seasons.

## Adapter and Wire Generation

Add `Adapters.GameProtocol.Red` with the same project shape as Yellow:

- reference `Adapters.GameProtocol.Shared`, `Application`, and `Contracts.AdminApi`
- package references `protobuf-net` and `Riok.Mapperly`
- generated `Wire/Game.cs` from `proto/red/taiko.proto`
- generated `Wire/VsInterface.cs` from `proto/red/vsinterface.proto`
- Red `GlobalUsings.cs`, marker, dependency injection extension, controllers, and mappers

Keep `proto/red/*.proto` immutable. If generation needs compatibility flags, change the generation command/output, not the dumped proto. Use the current AC15 nullable optional primitive convention from Phase 16.1, then replace generated Red wire files. Red and Yellow field numbers diverge because Yellow inserted item-shop/medal/WaiWai fields; shared wire DTOs are unsafe.

Mapperly should generate mechanical projection. Manual mapper code is reserved for:

- optional field presence and parser-visible omission
- byte/bit packing and gzip/raw crown payload decisions
- stage classification for Tokkun/challenge/normal
- Red Don-point/reward transforms
- Red challenge competition track grouping

## Host Integration

Red foundation should touch these host-level integration points in one early phase:

| File | Red Change |
|------|------------|
| `TaikoLocalServer.slnx` | Add `Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj`. |
| `Host/Host.csproj` | Add Red project reference, exclude `wwwroot/data/red/data/**`, copy Red sidecar JSON, and create a debug junction target for `wwwroot/data/red/data`. |
| `Host/Configurations/ServerSettings.json` | Add `ServerSettings:Eras:Red` with `Enabled`, `AutoExtractCatalog`, `GameDataPath`, and optional Red-specific settings. Do not require `EnableShop` for Red unless item shop evidence appears. |
| `Host/Program.cs` | Import Red adapter, call `AddGameProtocolRed()` when Red is enabled, remove the Red application part when disabled, include Red route prefix in content-type fallback, and update the fatal enabled-era message. |
| `Application/Handlers/GetStartupMovieDataQuery.cs` | Add Red only after proving the Red `hdd_ver / 100` mapping. Do not infer the bucket solely from `ST*` config directories. |
| `Application/Settings/ServerSettingsOptionsValidationExtensions.cs` | Do not add Red to `Ac15ShopEras` unless Red gets real item-shop settings. Add Red-specific validation for selected config root or challenge sidecar only when those settings exist. |

## AdminApi and WebUI Routing

AdminApi and WebUI should follow existing era-route patterns:

- `GameEra.Red` makes `EraRoute.TryParse("Red")` valid.
- Add Red cases to AdminApi controllers that already switch over Blue/Green/Yellow for profile settings, play history, favorites, song best, leaderboard, Dani, catalog readback, and customization.
- Add Red partial files such as `UserSettingsController.Red.cs` and `SongLeaderboardController.Red.cs` instead of embedding Red logic in Yellow partials.
- Add Red to `TaikoWebUI/Utilities/WebUiEra.cs` `Supported`, `Known`, and `IsAc15`.
- Keep WebUI service URLs as `api/Red/...`; do not add Red-only URL construction unless a workflow genuinely differs.
- Add challenge competition AdminApi/WebUI only after Red runtime semantics are useful to inspect. It should be Red-only at first.

## Patterns to Follow

### Pattern 1: Partial Handler Dispatch

**What:** Keep unsuffixed handlers as dispatchers and add `.Red.cs` partials for Red behavior.

**When:** Every feature already follows `GameEra` dispatch, such as playresult and userdata.

**Example:**

```csharp
public ValueTask<uint> Handle(UpdatePlayResultCommand request, CancellationToken ct) => request.Era switch
{
    GameEra.Red => HandleRed(request, ct),
    GameEra.Yellow => HandleYellow(request, ct),
    GameEra.Blue => HandleBlue(request, ct),
    GameEra.Green => HandleGreen(request, ct),
    GameEra.Nijiiro => HandleNijiiro(request, ct),
    _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
};
```

### Pattern 2: Typed DbSets Into Shared Core

**What:** Reuse `Application/Ac15` by passing typed Red rows and Mapperly creation delegates.

**When:** Normal play, self-best, crowns, recent/favorite, and Dani rows match AC15 row-shape interfaces.

**Avoid:** New repository interfaces wrapping `ITaikoDbContext`.

### Pattern 3: Evidence-Gated Special Mode Gates

**What:** Red playresult must classify Tokkun and challenge competition before normal writes when those shapes are present.

**When:** `play_mode`, `ary_tokkunstage_info`, and challenge result arrays are protocol-backed.

**Avoid:** Inferring modes from field existence in Blue/Green/Yellow proto definitions.

### Pattern 4: Sidecar JSON For Server-Authored Data

**What:** For route-backed data that is server-authored or not directly present in operator `USRDIR/data`, commit Red sidecar JSON under `Host/wwwroot/data/red/` and copy it in `Host.csproj`.

**When:** Folders, telops, movies, recommendations, Taikojuku verup overrides, challenge definitions, and intentionally empty datasets.

**Avoid:** Runtime handlers reading arbitrary filesystem paths or silent missing-data fallbacks.

## Anti-Patterns To Avoid

### Yellow Clone Adapter

Red and Yellow share many message names, but Yellow inserted item-shop, medal, and WaiWai fields. Copying Yellow wire, controllers, profile, or mutation code would import unsupported routes and wrong field placement.

### Treating Newer Challenge Stubs As Runtime Evidence

Green and Blue challenge controllers are success-only. Yellow routes through `GetChallengeCompeQuery.Yellow`, but that handler logs and returns empty. These are not proof that challenge competition is active in newer eras. Red challenge implementation must come from Red proto, Red data, Red runtime traces, and client behavior.

### Shared AC15 Persistence Tables

Do not add a generic `Ac15UserSave` or `Ac15ChallengeState` table with an era discriminator. The repo's safety model is era-owned tables plus shared algorithms.

### Red Item Shop By Analogy

Red has `present.xml`, `reward_ptn`, `reward_progress`, and Don-point fields. That is not Yellow item shop. Do not add `getitemshopinfo.php`, `itempurchase.php`, shop season state, or Don/Katsu medal state for Red unless Red evidence specifically shows it.

### Route and Config Root Guessing

The data roots and version names are leads, not route proof. Route prefix, startup HDD mapping, and selected config root must be proven from local client config, logs, IDA, or RPCS3.

## Build Order

1. **Red Evidence and Era Foundation**
   - Prove Red route prefix, direct protobuf transport, shared `/v01r00/chassis/*` startup/version ownership, HDD version mapping, and selected config root.
   - Add `GameEra.Red`, Red adapter project, generated wire DTOs, solution/Host references, server settings, application-part gating, and content-type fallback.
   - Add only evidence-backed no-state controllers. Keep unsupported item-shop/WaiWai/battle routes absent.

2. **Red Catalog and AC15 Profile**
   - Add `IRedCatalog`, Red catalog entities/loaders, `RedGameDataPaths`, required-file validation, sidecar files, and `Ac15CatalogSnapshotFactory.FromRed`.
   - Add `Ac15EraProfiles.Red` with item shop disabled and challenge marked outside the current all-on Blue/Green/Yellow profile.
   - Load and verify `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, tuning, `present.xml`, and sidecars from the proven Red root.

3. **Red Identity, Userdata, Crowns, Self-Best, and Normal Play**
   - Add Red EF schema, default save creation, BAID/mydon entry, userdata readback, crownsdata, selfbest, favorites/recent, and normal `playresult.php`.
   - Reuse AC15 normal/crown/userdata helpers where Red fields match.
   - Add Red-specific Don-point/reward fields to Common DTOs and `UserSaveDataRed`; do not map them onto medal/shop concepts.

4. **Red Dani, Rewards, Tokkun, and Banacoin Compatibility**
   - Add Red Dani read/write using typed Red Dani rows and shared AC15 Dani helpers.
   - Implement Red `present.xml` reward progression and rewardexecution behavior only as evidence supports.
   - Add Red Tokkun classification/persistence/readback with no normal, Dani, challenge, reward, battle, or cross-era side effects.
   - Add Banacoin-adjacent responses as stateless compatibility only if the Red runtime calls them.

5. **Red Challenge Competition**
   - Discover challenge definition source from Red data/assets/runtime evidence.
   - Add Red challenge catalog, Red challenge progress tables, Red playresult challenge write path, and real `challengecompe.php` readback.
   - Keep Green/Blue/Yellow challenge behavior unchanged unless separate evidence changes their scope.

6. **AdminApi/WebUI and Verification Closeout**
   - Add Red AdminApi and WebUI readback for supported profile, score/history, favorites, Dani, catalog, customization, Tokkun, rewards, and challenge state.
   - Run focused Red tests, full test suite, temp-output Host build, and cabinet/RPCS3 smoke before claiming Red support complete.

## Research Flags

| Topic | Confidence | Required Follow-Up |
|-------|------------|--------------------|
| Red outer architecture | HIGH | Existing Yellow/AC15 patterns are clear. |
| Red route prefix | MEDIUM | Prior research did not fully prove the prefix. Must verify before controllers are locked. |
| Red config root | MEDIUM | Local roots `ST5100-1`, `ST5100-7`, `ST7100-1`, and `ST8100-1` exist. Runtime target must choose. |
| Red normal/Dani/crowns reuse | HIGH | Proto and current `Application/Ac15` row helpers align well. |
| Red item shop absence | HIGH | Red proto lacks Yellow `Getitemshopinfo` and `Itempurchase` messages. |
| Red reward/Don-point semantics | MEDIUM | Proto and `present.xml` prove fields/data exist, but mutation/readback rules need runtime evidence. |
| Red challenge competition | MEDIUM | Proto and assets prove a meaningful surface, but active challenge catalog and scoring semantics still need deeper evidence. |
| Red Tokkun | MEDIUM | Proto has Tokkun fields; runtime classifier and readback must be proven as Red, not copied blindly. |

## Sources

- `.planning/PROJECT.md` - active v1.3 Red milestone, scope, constraints, and challenge competition correction.
- `.planning/milestones/v1.2-ROADMAP.md` - shipped Yellow phases and AC15 reuse/mapping baseline.
- `docs/superpowers/specs/2026-06-07-ac15-core-extraction-design.md` - approved shared AC15 behavior boundary.
- `Application/Ac15/Ac15EraProfiles.cs` and `Application/Ac15/Ac15CatalogSnapshotFactory.cs` - current profile/snapshot integration points.
- `Application/Handlers/UpdatePlayResultCommand.Yellow.cs` - current Yellow normal/Tokkun/prelude pattern.
- `Infrastructure/Persistence/TaikoDbContext.Yellow.cs` and `Domain/Entities/UserSaveDataYellow.cs` - era-owned EF schema pattern.
- `Adapters.GameProtocol.Yellow/Adapters.GameProtocol.Yellow.csproj`, `Adapters.GameProtocol.Yellow/Mappers/*.cs`, and `Adapters.GameProtocol.Yellow/Controllers/*` - adapter, Mapperly, and route patterns.
- `Host/Program.cs`, `Host/Host.csproj`, `Host/Configurations/ServerSettings.json`, and `TaikoLocalServer.slnx` - host enablement and output-copy points.
- `proto/red/taiko.proto` and `proto/red/vsinterface.proto` - Red protocol source of truth.
- `Host/wwwroot/data/red/data/config/*` - local Red data roots and reward/catalog files.
- Memory: Red-vs-Yellow proto research from 2026-06-10 and AC15 reuse guardrails. Used only as a pointer; repo files above were rechecked in this run.

---
*Architecture research for: Red AC15 Support*
*Researched: 2026-06-12*
