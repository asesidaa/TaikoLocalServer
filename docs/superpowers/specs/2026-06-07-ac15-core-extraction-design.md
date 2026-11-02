# AC15 Core Extraction - Design

**Date:** 2026-06-07
**Status:** approved design for a future implementation plan
**Scope:** Define a shared AC15 gameplay core for Blue, Green, and future Yellow/Red support while keeping era wire types, routes, and persistence separate.

## Purpose

Blue and Green AC15 support are now complete enough to expose substantial duplication. Yellow and Red are expected next, and Yellow already has a proto input under `proto/yellow/yellow.proto` with a message surface very close to Blue. The current pattern works for two eras but will multiply controller, mapper, catalog, handler, save, score, crown, Dani, and shop logic as older AC15 generations are added.

This design introduces a capability-driven AC15 core. The core shares behavior where the legacy clients support the same concept, while each era remains free to omit features, place the same data in different wire fields, or handle era-specific modes such as Blue battle, Blue Tokkun, or Green AI battle.

The central rule is: AC15 core shares behavior, not routes and not generated wire models.

## Existing Context

The repo already has the right outer architecture:

- Era-specific game protocol adapters own route prefixes and generated `Wire/` classes.
- Unsuffixed Mediator handlers dispatch to era-specific partial files.
- Application DTOs use shared `Common*` shapes with era partials for fields that are not universal.
- Blue and Green persistence tables are separate.
- AC15 catalog primitives already exist under `Application/Catalog/Ac15` and `Infrastructure/GameDataCatalog/Ac15`.

The current duplication falls into three groups:

- Exact or near-exact adapter/controller/mapper twins, such as folder, telop, item shop info, self-best, Taikojuku, and several success-shaped controllers.
- AC15 catalog and algorithm twins, such as music info records, Taikojuku entries, item shop entries, bitset helpers, crown packing, Dan packing, profile counters, and item-shop unlock helpers.
- Persistence-heavy flow twins, especially userdata, normal playresult, self-best/crowns, Dani, and item shop purchase, where common AC15 behavior is mixed with proven era-specific branches.

The design should eliminate the first two groups quickly and reduce the third group through shared workflow services plus typed era adapters.

## Goals

- Share AC15 behavior across Blue, Green, Yellow, Red, and later AC15 generations.
- Keep all era save data in separate tables.
- Keep generated wire classes in era adapter projects.
- Keep controller route ownership client-accurate.
- Let an era opt out of a feature by not having that controller/proto surface at all.
- Let an era place the same canonical data differently on the wire, such as crowns in `crownsdata.php` for Blue/Green versus inside `userdata.php` for older eras.
- Support era-specific mode extensions without contaminating the normal AC15 path.
- Preserve evidence-backed Blue and Green behavior during migration.

## Non-Goals

- No shared AC15 database table with an era discriminator.
- No migration of existing Blue/Green rows into shared tables.
- No generated shared AC15 wire assembly.
- No server stubs for client features that do not exist in an era.
- No implementation of Yellow or Red support in this design.
- No reinterpretation of Blue battle, Blue Tokkun, or Green AI battle as generic AC15 behavior.
- No route inference from proto similarity alone; each era adapter owns its route list.

## Architecture Boundary

Add an `Application/Ac15` core layer. It sits behind existing Mediator handlers and is called from era-specific partial files. It does not replace the outer route/controller/mapper pattern.

The high-level flow remains:

1. Era controller owns route, transport, and logging.
2. Era mapper converts generated proto wire type to a canonical AC15 or `Common*` request.
3. Existing Mediator handler dispatches by `GameEra`.
4. Era partial resolves an AC15 era profile and persistence/catalog adapters.
5. AC15 core service runs shared behavior.
6. Era mapper places canonical response data into the correct generated proto response.

Nijiiro stays outside this AC15 core unless a later design proves a shared rule is genuinely cross-generation.

## Core Pattern

Use a capability-driven strategy and adapter pattern:

```csharp
public sealed record Ac15EraProfile(
    GameEra Era,
    Ac15FeatureSet Features,
    Ac15ProtocolLimits Limits,
    Ac15WirePlacement WirePlacement,
    IAc15EraHooks Hooks);
```

The profile is explicit, typed, and small. It should not become a loose config dictionary.

`Ac15FeatureSet` declares client-supported modules, such as:

- `NormalPlay`
- `UserData`
- `SelfBest`
- `Crowns`
- `InitialData`
- `Folders`
- `Telops`
- `Recommendations`
- `Taikojuku`
- `Dani`
- `ItemShop`

If an older era does not have an item-shop route or proto messages, that era has no item-shop controller and does not call `Ac15ItemShopService`. The feature is absent because the client is absent, not because the server returns an empty implementation.

`Ac15ProtocolLimits` carries common limits and widths:

- song, tone, title, costume, Dan, and crown byte widths
- max favorite songs
- max recent songs
- max songs per Taikojuku pack
- valid Dan slot ranges
- valid difficulty/course ranges
- known safe defaults such as display Dan fallback

`Ac15WirePlacement` describes presentation quirks:

- crowns through a dedicated endpoint
- crowns embedded in userdata
- initial-data item rows present or absent
- legal terms present or absent
- optional tutorial/readback fields present or absent

`IAc15EraHooks` contains narrow extension points for proven era behavior.

## Persistence Boundary

Tables stay era-specific. The shared core works through era persistence adapters, not shared EF entities.

Example shape:

```csharp
public interface IAc15EraPersistence
{
    Task<Ac15UserSave> GetOrCreateSaveAsync(uint baid, CancellationToken ct);
    Task<bool> UserExistsAsync(uint baid, CancellationToken ct);
    Task AddPlayRowAsync(Ac15PlayRow row, CancellationToken ct);
    Task UpsertBestAsync(Ac15BestRow row, Ac15BestUpdatePolicy policy, CancellationToken ct);
    Task<IReadOnlyList<Ac15BestRow>> GetBestRowsAsync(uint baid, CancellationToken ct);
    Task<IReadOnlyList<uint>> GetFavoriteSongsAsync(uint baid, CancellationToken ct);
    Task<IReadOnlyList<uint>> GetRecentSongsAsync(uint baid, int limit, CancellationToken ct);
}
```

Blue maps this adapter to `UserSaveDataBlue`, `SongPlayDatumBlue`, `SongBestDatumBlue`, `BlueFavoriteSongs`, `BlueRecentSongs`, `DanScoreDatumBlue`, and Blue shop tables.

Green maps it to `UserSaveDataGreen`, `SongPlayDatumGreen`, `SongBestDatumGreen`, `GreenFavoriteSongs`, `GreenRecentSongs`, `DanScoreDatumGreen`, and Green shop tables.

Yellow and Red will get separate entities/tables or adapter implementations when their support is planned. They should not reuse Blue/Green tables even if the shape is identical.

The adapter can expose canonical save and row records, but it must write back through strongly typed era code. Reflection-heavy generic EF access should be avoided because it makes table ownership and migrations harder to audit.

## Core Modules

### `Ac15NormalPlayService`

Owns shared normal play save behavior:

- verify user exists or follow the era's proven missing-user policy
- parse or accept play time from canonical request
- validate supported normal stages
- map level to `Difficulty`
- map play result to `CrownType`
- add play history rows
- update best scores
- update favorites and recent songs
- trim favorites/recent by era limits
- update profile counters
- apply common unlock arrays where the era supports them
- call hooks before and after normal save

It must not interpret special modes from field names alone. Era hooks decide whether a playresult is Blue battle, Blue Tokkun, Green AI battle, or normal AC15 play.

### `Ac15UserDataService`

Builds canonical userdata readback:

- song hash version
- release song flags
- tone and title flags
- profile counters
- favorites and recent songs
- recommendations
- default option and Shin settings
- display Dan slot
- optional tutorial flags when present
- optional crown bytes when that era's wire places crown data in userdata

The service returns canonical data. Era wire mappers omit fields not present in that generated proto response.

### `Ac15CrownService`

Computes AC15 crown bytes from best rows. It is endpoint-agnostic.

Blue/Green can call it for `crownsdata.php`; older eras can call it from userdata handling if their crown field is embedded there. The packing algorithm remains shared.

### `Ac15SelfBestService`

Computes self-best rows for a requested difficulty and song list:

- normal best score
- Ura best score
- Shin best score when supported
- empty rows for songs without data if that is the era contract

Wire mappers own response placement and exact generated field names.

### `Ac15InitialDataService`

Builds initial-data availability rows for supported client features:

- telops
- folders
- Taikojuku/Dani
- item shop
- legal terms
- era extras such as Blue battle availability

Unsupported client features are not stubbed. If an era has no feature, it has no route/controller or mapper call for that feature.

### `Ac15TaikojukuService` And Dani Helpers

Own shared Dan behavior:

- valid Dan slot filtering
- request fallback behavior
- max songs per pack
- valid song and course filtering
- Dan grade packing
- display Dan normalization
- normal versus extra Dan ranges
- stage-score persistence through an era adapter

Era profiles can tune ranges and limits, but the common AC15 algorithm remains in one place.

### `Ac15ItemShopService`

Runs only for eras that declare item shop support and own item-shop routes.

Shared responsibilities:

- active season readback
- preflight purchase handling
- real purchase validation
- Don medal spend
- duplicate purchase rejection
- item unlock application through era policy
- season state readback

Era profiles/adapters decide whether song unlocks, tones, titles, costumes, and other shop item types mutate save flags. Older eras without item shop do not call this service.

### `Ac15CatalogReadbackService`

Provides shared readback for catalog-backed modules:

- folders
- telops
- recommendations
- movies where AC15 startup surfaces need them
- item-shop catalog rows
- initial-data info rows

This service should build on existing `Application/Catalog/Ac15` and `Infrastructure/GameDataCatalog/Ac15` types.

## Wire Placement

Canonical AC15 response data should be independent from generated proto response classes.

Example, crowns:

- `Ac15CrownService` returns crown bytes.
- Blue/Green `CrownsDataMappers` place them in `CrownsDataResponse.HashCrownFlg`.
- Yellow/Red `UserDataMappers`, if their protocol embeds crowns in userdata, place the same bytes in the userdata response field.
- An era with no crown readback feature does not request or map crown bytes.

Example, item shop:

- `Ac15ItemShopService` returns canonical shop season and item rows.
- Blue/Green mappers place supported fields into their generated `GetitemshopinfoResponse`.
- Yellow/Red do not expose item-shop controllers if their clients lack the feature.

Wire mappers remain era-specific because generated proto classes can differ in optionality, field presence helpers, message nesting, and route availability.

## Extension Points

Hooks should be explicit and narrow:

```csharp
public interface IAc15EraHooks
{
    Task<Ac15SpecialModeResult> TryHandleSpecialPlayModeAsync(
        Ac15PlayResultRequest request,
        IAc15EraContext context,
        CancellationToken ct);

    Ac15StageSupportDecision IsSupportedStage(Ac15StageData stage);

    Task BeforeNormalSaveAsync(Ac15NormalSaveContext context, CancellationToken ct);
    Task AfterNormalSaveAsync(Ac15NormalSaveContext context, CancellationToken ct);

    void ApplyUnlocks(Ac15UserSave save, Ac15UnlockSet unlocks);
    void BuildUserDataExtras(Ac15UserDataResponse response, Ac15UserDataContext context);
    void BuildInitialDataExtras(Ac15InitialDataResponse response, Ac15InitialDataContext context);
}
```

Default hooks should be conservative:

- no special mode handled
- normal stage modes only
- skip unsupported optional fields
- no inferred side effects

Known era extensions:

- Blue battle intercepts battle playresults before normal score persistence.
- Blue Tokkun intercepts Tokkun-classified playresults before normal score persistence.
- Green AI battle can allow AI battle stage modes and write Green-specific side data.
- Blue initial-data extras can add battle availability.
- Green initial-data extras can add ghost/AI battle availability.
- Older eras can place crown readback in userdata without changing crown computation.

The core must never infer a mode because a proto field exists. Era hooks are the evidence boundary.

## Feature Absence Rule

Unsupported features are client characteristics, not server behavior choices.

If an era does not support a function:

- there is no route/controller for it
- its generated proto does not contain the relevant messages
- the era profile marks the feature absent
- the AC15 service for that feature is not called

Do not add a log-and-success or empty stub just because another AC15 era has that endpoint.

## Migration Strategy

Migration should be staged so Blue/Green behavior remains stable:

1. Extract pure AC15 utilities and catalog records.
2. Introduce `Ac15EraProfile`, protocol limits, feature sets, and wire placement definitions.
3. Introduce Blue and Green persistence/catalog adapters without changing handler behavior.
4. Move a low-risk module first, such as crown packing/readback, self-best, or Taikojuku.
5. Move item-shop readback and purchase logic.
6. Move userdata composition.
7. Move normal play save last because it has the most mode-specific behavior.
8. Keep Blue/Green regression tests passing after each module.
9. Use the resulting AC15 core as the starting point for Yellow and Red support.

Path-limited commits should be used during migration because this repo often has unrelated local data or documentation changes.

## Testing Strategy

Add shared AC15 tests under `Tests/Ac15`:

- bitset normalization, setting, clearing, and packing
- crown packing from canonical best rows
- Dan grade packing and display Dan normalization
- Taikojuku request filtering and fallback packs
- self-best canonical response building
- userdata composition with optional crown placement
- item-shop preflight and purchase validation
- favorite/recent update and trim behavior
- normal play best-score update policy

Keep era-specific regression tests:

- Blue direct protobuf playresult logging and mapping
- Blue battle interception and no normal-score writes
- Blue Tokkun interception and narrow userdata readback
- Green compressed playresult decode
- Green AI battle behavior
- Green item-shop optional-field preflight
- Blue/Green route ownership
- generated wire namespace tests
- Blue/Green userdata and crown wire placement

Add profile contract tests:

- Blue and Green share expected AC15 defaults.
- Blue and Green differ only where the profile or hooks say they differ.
- Missing features cannot accidentally route to an AC15 service.

For Yellow/Red later, start with adapter/wire contract tests proving that routes exist only when their proto supports them.

## Acceptance Criteria

The design is successful when:

- Blue and Green can call shared AC15 modules without sharing tables.
- Existing Blue/Green tests continue to pass after each migration stage.
- A future Yellow/Red adapter can supply profile, wire mappers, and persistence adapters without copying Blue/Green normal gameplay logic.
- Features absent from an older client do not create server routes.
- Crown logic can be reused regardless of whether crown bytes are served through `crownsdata.php` or userdata.
- Blue battle, Blue Tokkun, and Green AI battle remain explicit era extensions.

## Design Decisions

- Use capability-driven services rather than inheritance from a `BlueLikeEra` or `GreenLikeEra` base class.
- Keep persistence table ownership per era.
- Keep wire placement in adapter mappers, not in core services.
- Use typed adapters and profiles instead of reflection-heavy generic EF logic.
- Move normal play save last because it combines the most shared logic with the most important era-specific exceptions.

