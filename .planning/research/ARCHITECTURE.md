# Architecture Research: v1.7 MOMOIRO AC15 0.11 Support

**Domain:** Brownfield AC15 cabinet protocol era integration
**Researched:** 2026-06-25
**Confidence:** HIGH for repo/proto/data architecture boundaries; MEDIUM for binary-gated route details, crown packing, song unlock semantics, and exact protocol limits until the MOMOIRO IDB is queried directly.

## Standard Architecture

### System Overview

MOMOIRO should integrate as a first-class AC15 era, not as KIMIDORI-with-different-routes. The existing architecture already has the right shape: adapter-owned protocol edges, Application handlers and shared AC15 capability modules in the middle, and era-owned EF/catalog infrastructure underneath.

```text
Cabinet / RPCS3
    |
    | direct protobuf POSTs
    v
Adapters.GameProtocol.Momoiro
    - /v04r00/chassis/*.php game routes
    - adapter-local Wire DTOs generated from proto/momoiro
    - thin controllers, Mapperly mappers, MomoiroRoutePrefixes
    |
    | Common / Ac15 application DTOs
    v
Application
    - *.Momoiro.cs handler partials
    - Ac15 shared services only where MOMOIRO evidence matches
    - Momoiro-specific mapping for userdata crowns and changed limits
    |
    | typed MOMOIRO DbSets and IMomoiroCatalog
    v
Infrastructure / Domain
    - UserSaveDataMomoiro and Momoiro-owned rows
    - root-level catalog under Host/wwwroot/data/momoiro/data
    - EF migration with no cross-era gameplay table reuse
```

Shared startup/version traffic remains under `/v01r00/chassis/*.php`. MOMOIRO game traffic belongs under `/v04r00/chassis/*.php`, with every exposed route ending in `.php`. Generated protocol classes from `proto/momoiro` should live only in `Adapters.GameProtocol.Momoiro/Wire`.

### Evidence Summary

| Finding | Evidence | Confidence | Architectural consequence |
|---------|----------|------------|---------------------------|
| MOMOIRO is the active milestone | `.planning/PROJECT.md` says v1.7 MOMOIRO 0.11 is active | HIGH | Write new architecture around MOMOIRO, not prior Murasaki/KIMIDORI content. |
| Game prefix is `/v04r00/chassis` | User milestone context and `.planning/PROJECT.md` | MEDIUM until IDB route table is recorded | Centralize in `MomoiroRoutePrefixes.Game`; verify route suffixes in `.tools/momoiro/EBOOT.ELF.i64` before closeout. |
| Startup/version prefix is `/v01r00/chassis` | User milestone context and `proto/momoiro/vsinterface.proto` startup/verup messages | HIGH | Reuse shared older-AC15 startup/version controllers; do not duplicate startup under `/v04r00`. |
| MOMOIRO data is root-level | `Host/wwwroot/data/momoiro/data` contains `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`; no versioned config root is required | HIGH | Follow KIMIDORI root-level loader shape; do not assume `config/STxxxx-*`. |
| Proto exposes crowns in userdata | `proto/momoiro/taiko.proto` has `UserDataResponse.hash_crown_flg` and no `CrownsData*` messages | HIGH | Add `Ac15EraProfiles.Momoiro` with `CrownPlacement = UserData`; do not add `crownsdata.php` unless binary evidence proves it. |
| Proto exposes song unlock/hash surfaces | `release_song_no`, `hash_release_song_flg`, `SonghashResponse.song_hash_tbl`, default/mainichi hash fields, and shopping-result release flags exist | HIGH for proto presence, MEDIUM for semantics | Block unlock persistence and readback sizes on binary/client evidence. |
| Proto lacks Taikojuku/folder/crownsdata routes | No `Taikojuku`, `FolderCheck`, `GetFolder`, or `CrownsData` messages in `proto/momoiro/taiko.proto` | HIGH | Keep these routes absent unless binary plus proto evidence changes. |
| Proto has challenge-shaped arrays but not a new feature family | Playresult and userdata contain challenge arrays | MEDIUM | Treat as compatibility/readback data only after binary proof; do not create Don Challenge or ChallengeCompe management by default. |
| `.tools/momoiro` contains local IDA evidence | Inventory shows `.tools/momoiro/EBOOT.ELF.i64` | HIGH for availability, LOW for route specifics until queried | Phase 1 must record route suffixes, limits, and packing facts from the IDB. |

### Component Responsibilities

| Component | Responsibility | Typical implementation |
|-----------|----------------|------------------------|
| `Adapters.GameProtocol.Momoiro` | Own MOMOIRO game routes, wire DTOs, mapper edge, and direct-protobuf controllers | New adapter project patterned after KIMIDORI, with route prefix `/v04r00/chassis` and only proto+binary-supported `.php` controllers |
| `Adapters.GameProtocol.Shared` | Own shared `/v01r00/chassis` startup/version routes | Existing shared controllers remain the route owners; generated MOMOIRO vsinterface DTOs are evidence, not a reason to duplicate routes |
| `Application/Handlers/*.Momoiro.cs` | Own MOMOIRO behavior orchestration | Partial handler methods selected by `GameEra.Momoiro`; bind concrete MOMOIRO catalog and DbSets before calling shared AC15 modules |
| `Application/Ac15` | Share identical AC15 behavior without era table switches | Add `Ac15EraProfiles.Momoiro`, `MomoiroAc15UserDataAdapter`, catalog projection, and crown-in-userdata support where needed |
| `Domain/Entities` | Own persisted MOMOIRO gameplay state | New `UserSaveDataMomoiro`, play/best/favorite/recent/Dan rows, and only evidence-backed extra rows |
| `Application/Abstractions` and `Infrastructure/Persistence` | Expose typed MOMOIRO DbSets through `ITaikoDbContext` | Add `ITaikoDbContext.Momoiro.cs`, `TaikoDbContext.Momoiro.cs`, and a migration; do not add repository-shaped persistence abstractions |
| `Infrastructure/GameDataCatalog/Momoiro` | Own root-level data loading and MOMOIRO sidecar catalogs | `MomoiroGameDataPaths`, `MomoiroRequiredDataFiles`, `MomoiroEraGameDataCatalog`, `IMomoiroCatalog` |
| `Host` | Compose enablement, adapter registration, app-part gating, settings, content-type fallback, and debug/publish data handling | Add project reference, `AddGameProtocolMomoiro`, config entries, disabled-era removal, direct-protobuf fallback, and data copy/junction targets |
| `Adapters.AdminApi` | Expose implemented MOMOIRO state through era-routed admin APIs | Extend generic AC15 controllers with Momoiro partials over Momoiro tables only |
| `TaikoWebUI` | Make MOMOIRO selectable where implemented state exists | Add `WebUiEra.Momoiro`; show normal AC15 pages only after corresponding AdminApi support exists |

## Recommended Project Structure

```text
Adapters.GameProtocol.Momoiro/
  Controllers/
    BaidController.cs
    MyDonEntryController.cs
    UserDataController.cs
    PlayResultController.cs
    SelfBestController.cs
    RecommendController.cs
    DefaultSongController.cs
    MainichiSongController.cs
    SongHashController.cs
    ShoppingResultController.cs
    TelopCheckController.cs
    GetTelopController.cs
    HeartbeatController.cs
    BookkeepingController.cs
    CommunicationLogController.cs
    BestScoreController.cs        # only after route/readback semantics are proven
  Mappers/
    BaidResponseMapper.cs
    UserDataMappers.cs
    PlayResultMappers.cs
    SelfBestMappers.cs
    RecommendMappers.cs
    CatalogReadbackMappers.cs
  Wire/
    Game.cs
    VsInterface.cs
  MomoiroRoutePrefixes.cs

Application/
  Abstractions/
    IMomoiroCatalog.cs
    ITaikoDbContext.Momoiro.cs
  Ac15/
    Ac15EraProfiles.cs
    Ac15UserDataRecords.cs          # extend only if crown bytes must flow through canonical userdata
    Ac15UserDataService.cs          # place crown bytes when profile says UserData
    MomoiroAc15UserDataAdapter.cs
    Ac15CatalogSnapshotFactory.cs
  Handlers/
    BaidQuery.Momoiro.cs
    AddMyDonEntryCommand.Momoiro.cs
    UserDataQuery.Momoiro.cs
    UpdatePlayResultCommand.Momoiro.cs
    GetSelfBestQuery.Momoiro.cs
    GetRecommendQuery.Momoiro.cs
    GetDefaultSongQuery.Momoiro.cs
    GetMainichiSongQuery.Momoiro.cs
    GetTelopQuery.Momoiro.cs

Domain/Entities/
  UserSaveDataMomoiro.cs
  SongPlayDatumMomoiro.cs
  SongBestDatumMomoiro.cs
  MomoiroFavoriteSongs.cs
  MomoiroRecentSongs.cs
  DanScoreDatumMomoiro.cs           # only if Dan/Dani persistence is kept from field/runtime proof
  DanStageScoreDatumMomoiro.cs

Infrastructure/GameDataCatalog/Momoiro/
  MomoiroGameDataPaths.cs
  MomoiroRequiredDataFiles.cs
  MomoiroEraGameDataCatalog.cs
```

### Structure Rationale

- **Adapter-local wire:** MOMOIRO generated DTOs must stay in `Adapters.GameProtocol.Momoiro/Wire`. Do not reuse KIMIDORI, Murasaki, or White wire types even when field names overlap.
- **Route-local controllers:** Game routes are MOMOIRO-owned under `/v04r00/chassis/*.php`; startup/version routes stay shared under `/v01r00/chassis/*.php`.
- **Application DTO boundary:** Controllers should deserialize, map, call Mediator, and map back. Application handlers and shared AC15 modules should not persist or depend on generated wire classes.
- **Root-level catalog ownership:** MOMOIRO should follow the KIMIDORI root-level path style with `PathHelper.GetDataPath(GameEra.Momoiro)/data`, not a config-root search.
- **Typed EF separation:** Every gameplay row that can differ by era gets a MOMOIRO table. Shared identity rows can remain shared.
- **Capability composition:** Reuse lives in `Application/Ac15` algorithms with MOMOIRO profile, limits, catalog snapshot, and concrete DbSets supplied by handlers.

## Architectural Patterns

### Pattern 1: Proto Plus Binary Route Gate

**What:** A MOMOIRO route is implemented only when `proto/momoiro` exposes the protocol shape and the MOMOIRO binary proves the `.php` route exists.

**When to use:** Every game route under `/v04r00/chassis`, especially `bestscore.php`, `shoppingresult.php`, `songhash.php`, and any challenge-looking surface.

**Trade-offs:** This slows the first phase slightly, but it prevents proto-only server behavior and keeps the user-provided evidence rule enforceable.

```csharp
public static class MomoiroRoutePrefixes
{
    public const string Game = "/v04r00/chassis";
}

[HttpPost(MomoiroRoutePrefixes.Game + "/userdata.php")]
[Produces("application/protobuf")]
public async Task<IActionResult> UserData([FromBody] UserDataRequest request)
{
    var common = await Mediator.Send(
        new Ac15UserDataQuery(request.Baid, GameEra.Momoiro),
        HttpContext.RequestAborted);

    var response = new UserDataResponse { Result = common.Result };
    UserDataMappers.Apply(common, response);
    return Ok(response);
}
```

### Pattern 2: Root-Level Catalog Loader

**What:** MOMOIRO catalog paths resolve directly below `Host/wwwroot/data/momoiro/data`.

**When to use:** Music info, medley info, defmusic, tuning, movies, customization extraction, telops, and sidecar data.

**Trade-offs:** This is less generic than a config-root scanner, but it matches local data and avoids silently selecting a nonexistent newer AC15 layout.

```csharp
public static class MomoiroGameDataPaths
{
    public static string GameDataRoot => Path.Combine(PathHelper.GetDataPath(GameEra.Momoiro), "data");
    public static string MusicInfoXml => Path.Combine(GameDataRoot, "musicinfo.xml");
    public static string MusicMedleyInfoXml => Path.Combine(GameDataRoot, "musicmedleyinfo.xml");
    public static string DefMusicBin => Path.Combine(GameDataRoot, "defmusic.bin");
    public static string TuningBin => Path.Combine(GameDataRoot, "fumen", "tuning.bin");
}
```

### Pattern 3: Userdata Crown Placement

**What:** Crown bytes should be computed by shared AC15 crown logic but placed in `UserDataResponse.hash_crown_flg` for MOMOIRO.

**When to use:** MOMOIRO userdata readback after binary proof confirms byte count, hash indexing, compression/compaction expectations, and song count.

**Trade-offs:** Requires extending the canonical userdata path, but it avoids a fake `crownsdata.php` endpoint and keeps wire placement explicit.

```csharp
public static Ac15EraProfile Momoiro { get; } = new(
    GameEra.Momoiro,
    MomoiroFeatures,
    CreateMomoiroLimits(),
    new Ac15WirePlacement(
        CrownPlacement: Ac15CrownWirePlacement.UserData,
        HasInitialDataItemShopRows: false,
        HasInitialDataLegalTermsRows: false,
        HasTokkunTutorialFlagInUserData: false),
    Ac15ProfileCapabilities.CurrentWithoutTitlePlateOrTaikojuku);
```

### Pattern 4: Switch-Free AC15 Capability Reuse

**What:** Shared AC15 modules should not pick MOMOIRO tables internally. MOMOIRO handlers bind concrete `DbSet`s, Mapperly delegates, catalog rows, limits, and policies, then call shared modules.

**When to use:** Normal play writes, self-best, favorites, recent songs, profile counters, recommendations, Dan/Dani if proven, and AdminApi profile settings.

**Trade-offs:** Handler partials remain visible and moderately large, but table ownership stays audit-friendly.

```csharp
await Ac15NormalPlayWriter.SaveAsync(
    context,
    new Ac15NormalPlayTables<SongPlayDatumMomoiro, SongBestDatumMomoiro, MomoiroFavoriteSongs, MomoiroRecentSongs>(
        context.SongPlayDataMomoiro,
        context.SongBestDataMomoiro,
        context.MomoiroFavoriteSongs,
        context.MomoiroRecentSongs,
        Ac15NormalPlayMapper.ToMomoiroSongPlayDatum,
        Ac15NormalPlayMapper.ToMomoiroSongBestDatum),
    request,
    Ac15NormalStagePolicies.Standard,
    cancellationToken);
```

### Pattern 5: Absence as Architecture

**What:** Unsupported features remain absent, not stubbed.

**When to use:** Taikojuku, Tokkun, Banacoin, battle, separate `crownsdata.php`, newer item-shop authority, Don Challenge, ChallengeCompe management, and later-era folders unless both proto and binary route evidence prove them.

**Trade-offs:** Some proto fields may remain unmapped initially, but this is preferable to inventing server state that a 0.11 client does not prove.

## Data Flow

### Startup and Version Flow

```text
Cabinet
  -> /v01r00/chassis/startupauth.php
  -> shared older-AC15 startup controller
  -> shared startup handler and response DTO
  -> StartupAuthResponse

Cabinet
  -> /v01r00/chassis/verupauth.php or verupcomplete.php
  -> shared older-AC15 version route
```

MOMOIRO `vsinterface.proto` should be used to verify schema compatibility, but it should not create duplicate `/v04r00` startup routes.

### Catalog Boot Flow

```text
Host startup
  -> enabled GameEra.Momoiro
  -> Infrastructure registers MomoiroEraGameDataCatalog
  -> MomoiroRequiredDataFiles checks root-level files
  -> AC15 loaders parse musicinfo.xml, musicmedleyinfo.xml, defmusic.bin, fumen/tuning.bin
  -> sidecar JSON loads from Host/wwwroot/data/momoiro/
  -> immutable IMomoiroCatalog snapshot is available to handlers
```

Required raw inputs should start with:

- `Host/wwwroot/data/momoiro/data/musicinfo.xml`
- `Host/wwwroot/data/momoiro/data/musicmedleyinfo.xml`
- `Host/wwwroot/data/momoiro/data/defmusic.bin`
- `Host/wwwroot/data/momoiro/data/fumen/tuning.bin`

Server-authored sidecars should be separate files under `Host/wwwroot/data/momoiro/` when MOMOIRO needs event folders, telops, movies, customization, or intentionally empty data.

### Game Request Flow

```text
Cabinet
  -> /v04r00/chassis/<route>.php
  -> Momoiro controller
  -> Momoiro Mapperly mapper or explicit protocol helper
  -> Mediator request with GameEra.Momoiro
  -> *.Momoiro.cs Application handler
  -> IMomoiroCatalog + ITaikoDbContext Momoiro DbSets
  -> Momoiro wire response
```

Controllers should stay transport-only. Business decisions such as unlock mutation, crown bytes, favorite trimming, display Dan normalization, and missing-user behavior belong in Application.

### Userdata With Crown Flow

```text
userdata.php
  -> Ac15UserDataQuery(GameEra.Momoiro)
  -> UserSaveDataMomoiro + favorites + recent + best rows
  -> MomoiroAc15UserDataAdapter creates canonical snapshot
  -> Ac15UserDataService builds common fields
  -> Ac15CrownService builds crown bytes when CrownPlacement is UserData
  -> UserDataMappers places hash_crown_flg
```

Do not add a dedicated crown route unless IDB/client evidence proves `crownsdata.php` is called for MOMOIRO despite the proto shape.

### Normal Play Write Flow

```text
playresult.php
  -> PlayResultMappers.Map(request)
  -> UpdateAc15PlayResultCommand(GameEra.Momoiro, Ac15PlayResultEnvelope)
  -> MOMOIRO handler rejects baid=0/missing-user with era-standard success semantics
  -> special-mode gates remain absent unless evidence proves them
  -> Ac15NormalStageFilter validates normal stages using Momoiro limits
  -> Ac15CommonProfileMutation applies proven profile/unlock counters
  -> Ac15NormalPlayWriter writes Momoiro play/best/favorite/recent rows only
  -> optional Dan/Dani writer only if play mode and data contract are proven
```

`release_song_no`, `reward_ptn`, `reward_progress`, Don Point totals, and challenge arrays should not become state changes until binary research proves the 0.11 semantics.

### Song Unlock and Shopping Flow

```text
shoppingresult.php or playresult.php
  -> proto exposes release-song and Don Point fields
  -> binary research determines whether these mutate save flags, total counters, both, or neither
  -> Application mutates only UserSaveDataMomoiro fields backed by that evidence
  -> userdata.php / shoppingresult.php returns hash_release_song_flg using Momoiro song hash table
```

This should not use Blue/Yellow item-shop authority. MOMOIRO has shopping-result compatibility fields, not proven newer item-shop wallet/season behavior.

### AdminApi and WebUI Flow

```text
WebUI era selector
  -> WebUiEra.Momoiro
  -> /api/momoiro/... AdminApi route
  -> controller partial binds Momoiro DbSets and Ac15EraProfiles.Momoiro
  -> Contracts.AdminApi DTOs stay shared where AC15 surfaces are identical
  -> UI shows only implemented Momoiro-owned state
```

AdminApi/WebUI should not expose Don Challenge, ChallengeCompe management, Taikojuku folder editing, Tokkun, Banacoin, battle, or standalone item-shop pages for MOMOIRO without implementation evidence.

## New vs Modified Components

| Area | New components | Modified components |
|------|----------------|---------------------|
| Era identity | `GameEra.Momoiro` | `EraRoute`, WebUI era lists, localization/display labels if needed |
| Adapter | `Adapters.GameProtocol.Momoiro`, `MomoiroRoutePrefixes`, controllers, mappers, generated `Wire/Game.cs`, `Wire/VsInterface.cs` | Solution/Host project references, app-part gating, direct-protobuf fallback path checks |
| Application handlers | `*.Momoiro.cs` partials for BAID, MyDon, userdata, playresult, self-best, recommend, catalog readback | Unsuffixed dispatch handlers add `GameEra.Momoiro` cases |
| AC15 shared | `Ac15EraProfiles.Momoiro`, `MomoiroAc15UserDataAdapter`, `Ac15CatalogSnapshotFactory.FromMomoiro` | `Ac15UserDataRecords/Service` may need canonical crown bytes for `CrownPlacement.UserData`; normal play/Dani mappers add Momoiro delegates |
| Domain/EF | `UserSaveDataMomoiro`, `SongPlayDatumMomoiro`, `SongBestDatumMomoiro`, favorite/recent rows, optional Dan rows | `ITaikoDbContext`, `TaikoDbContext`, migration snapshot |
| Catalog | `IMomoiroCatalog`, `MomoiroGameDataPaths`, `MomoiroRequiredDataFiles`, `MomoiroEraGameDataCatalog`, MOMOIRO sidecar JSON | `IGameDataCatalog.For(GameEra)`, dependency injection |
| Host/config | Momoiro `ServerSettings` era entry and data copy/junction handling | `Program.cs`, `Host.csproj`, startup validation messages, content-type route predicate |
| AdminApi | Momoiro partials for implemented AC15 APIs | Era switch expressions in existing controllers |
| WebUI | `WebUiEra.Momoiro` support and optional era label | Era selector, profile/play-data/high-score/history pages once AdminApi state exists |
| Tests/verification | Focused Momoiro tests for catalog loading, route/controller mapping, persistence boundaries, crown-in-userdata, unlock gates | Shared AC15 tests where new crown placement or limits affect common behavior |

## Reuse Boundaries

| Reuse candidate | Reuse when | Do not reuse |
|-----------------|------------|--------------|
| `Ac15MusicInfoLoader`, `Ac15TuningLoader`, `Ac15SongHashCodec` | Root-level files parse with existing AC15 formats | New config-root assumptions or directory auto-selection |
| `Ac15CustomizationCatalogSupport` | MOMOIRO customization extraction follows the same packed/sidecar rules | Any hardcoded KIMIDORI/Murasaki filenames other than new `momoiro_*` sidecars |
| `Ac15UserDataService` | MOMOIRO canonical fields match and crown placement is explicitly added | Dedicated crown endpoint assumptions from newer eras |
| `Ac15CrownService` | Packing and hash indexing are proven for MOMOIRO | Assuming Blue/KIMIDORI `CrownSongCount` or byte width without binary proof |
| `Ac15NormalPlayWriter` | Normal play stage policy and score/crown semantics match | Special modes, challenge arrays, or unsupported stage modes |
| `Ac15DaniWriter` | MOMOIRO play mode and medley/Dan semantics are proven | Adding Taikojuku route or folder UI from Dan fields alone |
| `Ac15ProfileSettingsService` | MOMOIRO save row supports the same settings fields | Title-plate/Taikojuku settings when profile capabilities say absent |
| AdminApi AC15 controllers | They can bind concrete Momoiro DbSets | Repository wrappers or cross-era queries |

## Scaling Considerations

| Scale | Architecture adjustments |
|-------|--------------------------|
| Single local cabinet | Current ASP.NET Core monolith, SQLite, and singleton catalogs are appropriate. |
| Multiple local cabinets | Add indexes and tests around Momoiro score/favorite/recent writes before optimizing; keep per-era tables. |
| Larger private deployment | Optimize catalog startup and readback byte generation; cache immutable song hash/default/mainichi/crown payloads where safe. |

### Scaling Priorities

1. **Byte payload correctness:** `hash_release_song_flg`, `hash_crown_flg`, default-song flags, and song-hash tables are small enough to recompute but easy to corrupt. Verify size and indexing first.
2. **SQLite write consistency:** Playresult, best-score, favorite, and recent writes must stay in Momoiro tables only. Index later if actual use shows slow queries.
3. **Catalog startup diagnostics:** Root-level file checks should log exact missing paths and sidecar counts so operator data issues are obvious.

## Anti-Patterns

### Anti-Pattern 1: Cloning KIMIDORI Route Inventory

**What people do:** Copy every KIMIDORI controller into MOMOIRO and change the prefix.

**Why it is wrong:** MOMOIRO proto lacks several KIMIDORI/Murasaki surfaces, and MOMOIRO places crowns in userdata.

**Do this instead:** Start from `proto/momoiro` plus IDB `.php` route proof; implement only the intersection.

### Anti-Pattern 2: Adding `crownsdata.php`

**What people do:** Add the familiar AC15 crown endpoint because newer eras have one.

**Why it is wrong:** MOMOIRO proto exposes `UserDataResponse.hash_crown_flg` and no `CrownsData` messages.

**Do this instead:** Extend canonical userdata to carry crown bytes when `Ac15WirePlacement.CrownPlacement == UserData`.

### Anti-Pattern 3: Config-Root Catalog Loading

**What people do:** Search for `config/STxxxx-*` or borrow later-era path constants.

**Why it is wrong:** The MOMOIRO linked data has root-level `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`.

**Do this instead:** Use `MomoiroGameDataPaths.GameDataRoot` under `PathHelper.GetDataPath(GameEra.Momoiro)/data`.

### Anti-Pattern 4: Repository-Shaped Persistence

**What people do:** Hide Momoiro EF tables behind a broad repository to make shared AC15 services feel generic.

**Why it is wrong:** The current architecture uses `ITaikoDbContext` and concrete `DbSet`s as the boundary. Broad repositories obscure table ownership and make cross-era writes harder to audit.

**Do this instead:** Add typed Momoiro DbSets and pass them into switch-free AC15 row helpers.

### Anti-Pattern 5: Treating Challenge Arrays as Don Challenge

**What people do:** Turn `ary_challenge_stat` and playresult challenge IDs into Don Challenge or ChallengeCompe management.

**Why it is wrong:** The milestone says no new feature families, and challenge-shaped protocol fields are not semantic proof.

**Do this instead:** Leave challenge state absent or store/read only after binary route and runtime contract proof.

### Anti-Pattern 6: Claiming Completion From Build Tests Alone

**What people do:** Close the milestone after generated DTOs compile and server tests pass.

**Why it is wrong:** AC15 support is cabinet compatibility work. Automated verification is necessary but not sufficient.

**Do this instead:** Require generated-source inspection, focused server tests, temp-output Host build when needed, and repeatable cabinet/RPCS3 runtime evidence.

## Integration Points

### External Services

| Service | Integration pattern | Notes |
|---------|---------------------|-------|
| MOMOIRO cabinet/RPCS3 | Direct protobuf POSTs to `/v04r00/chassis/*.php` and shared `/v01r00/chassis/*.php` startup/version routes | Runtime verification is the closeout gate. |
| Local MOMOIRO IDB | `.tools/momoiro/EBOOT.ELF.i64` | Use for route suffix inventory, changed limits, crown packing, and song unlock behavior before runtime semantics. |
| Local operator data | `Host/wwwroot/data/momoiro/data` via `PathHelper.GetDataPath(GameEra.Momoiro)` | Data is a local link and may be gitignored; commit only server-authored sidecars. |

### Internal Boundaries

| Boundary | Communication | Notes |
|----------|---------------|-------|
| Host -> Momoiro adapter | Project reference, DI registration, ApplicationPart gating | Disabled MOMOIRO must remove its controllers from routing. |
| Momoiro adapter -> Application | Mediator requests over Common/Ac15 DTOs | Generated wire types stay adapter-local. |
| Application -> Infrastructure | `ITaikoDbContext` concrete Momoiro DbSets and `IMomoiroCatalog` | No repository-shaped persistence abstraction. |
| Infrastructure -> data files | Path helpers plus `MomoiroGameDataPaths` | Required-file checks should name root-level files. |
| AdminApi/WebUI -> state | Existing era-routed APIs and shared AC15 DTOs | Add Momoiro only for implemented state; hide unsupported feature pages. |

## Phase Order Recommendation

1. **Evidence and era foundation**
   - Record IDB route suffixes for `/v04r00/chassis/*.php`, shared `/v01r00` startup/version proof, direct-protobuf transport, and changed limits.
   - Add `GameEra.Momoiro`, generated wire DTOs, adapter project, route prefix, Host registration, settings, app-part gating, and direct-protobuf fallback.
   - Avoids: wrong route ownership, proto-only controllers, and unsupported route families.

2. **Root-level catalog and AC15 profile binding**
   - Add `IMomoiroCatalog`, root-level path constants, required-file checks, catalog loader, sidecar JSON names, `Ac15CatalogSnapshotFactory.FromMomoiro`, and `Ac15EraProfiles.Momoiro`.
   - Binary-gate `Ac15ProtocolLimits` before finalizing favorite/recent limits, byte widths, crown song count, and song hash sizing.
   - Avoids: later-era config-root leakage and incorrect byte payload sizes.

3. **Catalog/no-state route readback**
   - Implement proven route controllers for heartbeat, bookkeeping/communication log success shapes, defaultsong, mainichisong, songhash, telopcheck/gettelop, recommend, and other catalog-backed responses.
   - Keep `bestscore.php` separate until route order and ranking semantics are proven.
   - Avoids: blocking the cabinet during early readiness flows while state persistence is still being built.

4. **Identity, userdata, self-best, and crown readback**
   - Add Momoiro save/best/play/favorite/recent tables and basic BAID/MyDon/userdata/self-best readback.
   - Extend canonical userdata for `hash_crown_flg` and validate crown packing with binary/client evidence before declaring done.
   - Avoids: fake `crownsdata.php` and cross-era score reads.

5. **Normal playresult, unlocks, rewards, and Dan where proven**
   - Implement playresult classification, normal stage filtering, score/best/crown/favorite/recent writes, Don Point/reward fields, song unlock mutation, and optional Dan/Dani persistence only where MOMOIRO proof supports it.
   - Treat shopping-result as compatibility plus evidence-backed unlock/Don Point mutation, not newer item-shop authority.
   - Avoids: KIMIDORI/Murasaki behavior copy and unsupported new feature families.

6. **AdminApi and WebUI routing**
   - Add Momoiro partials to play data, history, favorites, leaderboard, profile settings, customization catalog, and Dan endpoints only for implemented state.
   - Add `WebUiEra.Momoiro` and era selector support; hide unsupported feature pages.
   - Avoids: UI links to absent server behavior.

7. **Verification closeout**
   - Run focused Momoiro tests, shared AC15 tests affected by new crown placement/limits, full build, generated Mapperly source inspection, and temp-output Host build if the normal output is locked.
   - Close only after repeatable cabinet/RPCS3 runtime evidence for startup, login, userdata, catalog readback, playresult, self-best/crowns, and AdminApi/WebUI paths.
   - Avoids: overclaiming compatibility from server-only verification.

## Verification Strategy

Required automated coverage should protect observable behavior, not source shape:

- MOMOIRO root-level catalog loads required files and sidecar data from the correct paths.
- Disabled MOMOIRO removes Momoiro adapter controllers from routing.
- Controllers deserialize direct protobuf, map to Application DTOs, and return Momoiro wire DTOs.
- Userdata includes `hash_crown_flg` only through the MOMOIRO userdata route once packing is proven.
- Playresult writes only Momoiro save/play/best/favorite/recent/Dan tables.
- Song unlock and shopping-result behavior follows IDB/client evidence and does not call newer item-shop state.
- AdminApi/WebUI reads and writes only Momoiro-owned state.
- Build emits Mapperly generated sources, and generated mapping output is inspected for nontrivial protocol placement.

Manual/runtime closeout should record exact cabinet/RPCS3 observations. It should not broaden acceptance beyond what the user actually verified.

## Sources

- `.planning/PROJECT.md` - active v1.7 MOMOIRO scope, constraints, route/data context, evidence hierarchy, and pending decisions.
- `.planning/ROADMAP.md` - prior milestone ordering and current planning lifecycle context.
- `.planning/config.json` - GSD config; external research providers disabled, repo-local research enabled.
- `proto/momoiro/taiko.proto` - MOMOIRO game protocol messages, crown placement, song unlock/hash fields, challenge arrays, and absent route families.
- `proto/momoiro/vsinterface.proto` - startup/version message family for shared `/v01r00` routes.
- `Host/wwwroot/data/momoiro/data` - root-level MOMOIRO data inventory and required raw files.
- `.tools/momoiro/EBOOT.ELF.i64` - local binary evidence source for route inventory, limits, song unlock, and crown packing research.
- `docs/superpowers/specs/2026-06-07-ac15-core-extraction-design.md` - AC15 shared behavior, separate routes/wire/persistence, and crown wire placement principles.
- `docs/superpowers/specs/2026-06-11-ac15-capability-composition-design.md` - switch-free capability composition, direct EF table binding, and no repository-shaped persistence rule.
- `Application/Ac15`, `Application/Handlers/*.Kimidori.cs`, `Infrastructure/GameDataCatalog/Kimidori`, `Adapters.GameProtocol.Kimidori`, `Adapters.AdminApi`, `TaikoWebUI/Utilities/WebUiEra.cs`, and `Host/Program.cs` - current KIMIDORI and shared AC15 integration patterns.

---
*Architecture research for: v1.7 MOMOIRO AC15 0.11 Support*
*Researched: 2026-06-25*
