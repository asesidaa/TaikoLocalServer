# AC15 Capability Composition - Design

**Date:** 2026-06-11
**Status:** approved design for Phase 16.2 review follow-up
**Scope:** Redesign the AC15 shared-core architecture so Blue, Green, Yellow, and later AC15 eras share identical behavior without shared modules switching on `GameEra`, while preserving era-owned routes, generated wire DTOs, and EF tables.

## Purpose

Phase 16.2 extracted substantial AC15 shared code, but the result is still too era-first. Shared services such as normal play, Dani, and item shop still reopen `GameEra` switches to select tables or expose era-shaped entry points. Era handlers still duplicate normal play save prelude behavior such as timestamp parsing, medal mutation, tutorial flag updates, profile counters, unlock flags, and Dani save callbacks.

This design corrects the module shape. AC15 sharing should be capability-first:

- Era handlers and adapter controllers are composition roots.
- Shared `Application/Ac15` modules own behavior that is actually identical.
- Shared modules must not switch on era.
- Table separation is preserved by passing concrete EF `DbSet`s and Mapperly delegates into generic row helpers.
- Real era behavior stays visible in the era handler or in narrow era-specific helpers.

The goal is not to make handlers tiny. For AC15 playresult, a handler can be fat when it is showing era workflow. The problem is duplication of identical behavior and table-only switches inside shared modules.

## Relationship To Previous Design

This spec amends `docs/superpowers/specs/2026-06-07-ac15-core-extraction-design.md`.

The previous design remains valid for these rules:

- AC15 core shares behavior, not routes or generated wire models.
- Blue, Green, Yellow, and future AC15 persistence tables stay separate.
- Unsupported client features stay absent rather than receiving server stubs.
- Blue battle, Blue Tokkun, Yellow Tokkun, and Green AI/ghost behavior remain explicit era behavior.
- Mapperly remains the mechanical projection tool for canonical value/entity mapping.

This spec supersedes the previous design wherever it suggested:

- broad AC15 persistence adapters such as `IAc15EraPersistence`;
- complete shared services with `SaveBlueAsync`, `PurchaseGreenAsync`, or similar era entry points;
- broad hooks such as `IAc15EraHooks` for normal play no-ops;
- shared modules that switch on `GameEra` only to choose EF tables or Mapperly overloads.

## Goals

- Remove table-only `GameEra` switches from shared AC15 modules.
- Keep era handlers readable as orchestration roots for actual era workflows.
- Preserve clean architecture by moving AdminApi AC15 business behavior out of controllers.
- Preserve direct `ITaikoDbContext` and concrete EF table visibility.
- Preserve physical table separation for Blue, Green, Yellow, and later AC15 eras.
- Share identical behavior through switch-free capability modules and generic row helpers.
- Keep real policy variation explicit and narrow, such as Green song unlock no-op or Green AI stage policy.
- Keep tests focused on observable behavior, persistence, protocol output, and no-cross-era writes.

## Non-Goals

- Do not merge AC15 EF tables or add discriminator tables.
- Do not add repository-shaped persistence abstractions.
- Do not add Blue/Green/Yellow table-only adapter classes.
- Do not add a custom source generator for AC15 table binding.
- Do not hide Blue battle, Tokkun, Green ghost, or Yellow WaiWai behavior behind generic AC15 mode machinery.
- Do not infer routes or proto surfaces from another era.
- Do not make source-shape tests the verification strategy.

## Core Rule

Allowed `GameEra` switches:

- top-level route or Mediator dispatch to an era handler;
- adapter/controller HTTP routing such as legacy AdminApi route selection;
- era handler composition where concrete Blue, Green, or Yellow tables are bound;
- real behavior branches whose meaning differs by era.

Forbidden `GameEra` switches:

- shared AC15 modules choosing `DbSet`s internally;
- shared modules choosing Mapperly overloads internally;
- shared modules exposing era-named methods for otherwise identical behavior;
- shared modules branching only to perform the same property mutations on different entity types;
- nullable union records such as one field for Blue, one for Green, one for Yellow.

If behavior is shared, the shared module interface should not mention era. If behavior differs, the difference should be passed as a policy, delegate, capability interface, or era-local helper call.

## Layer Responsibilities

### Adapter Controllers

Controllers own transport concerns:

- route attributes and legacy route compatibility;
- request/response HTTP status mapping;
- generated wire DTO serialization and deserialization;
- AdminApi route parsing such as `EraRoute.TryParse`;
- logging at the transport edge.

Controllers must not own AC15 business behavior such as unlock bitset encoding, selectable Dan calculation, profile mutation, or table-specific Dan readback. For AdminApi user settings, controllers should call an Application use case or service and translate its result to HTTP.

### Era Handlers

Era handlers own orchestration:

- missing-user and `baid == 0` behavior;
- special-mode gates and their ordering;
- concrete save-row, catalog, season, and `DbSet` binding;
- selection of policy objects and Mapperly delegates;
- calling era-specific helper modules for real era behavior;
- calling switch-free shared AC15 modules for identical behavior.

Handlers may remain moderately large if that size represents real workflow. They should not repeat identical mutation algorithms.

### `Application/Ac15`

`Application/Ac15` owns switch-free capability modules:

- pure AC15 parsing and bit helpers;
- common profile/save mutation;
- normal stage filtering and stage policy;
- generic normal play row/best/favorite/recent writing;
- generic Dani save and readback;
- generic item-shop purchase flow;
- generic AdminApi user settings workflow;
- canonical catalog projection helpers.

These modules may use generic type parameters constrained by narrow Domain row-shape interfaces. They may accept concrete EF `DbSet`s, `IQueryable`s, delegates, and Mapperly functions. They must not choose tables by era.

### Domain

Domain entities remain era-owned. Domain may expose narrow row-shape or save capability interfaces where properties are genuinely identical:

- `IAc15SongPlayDatum`
- `IAc15SongBestDatum`
- `IAc15FavoriteSong`
- `IAc15RecentSong`
- `IAc15DanScoreDatum`
- `IAc15DanStageScoreDatum`
- `IAc15ShopItemState`
- `IAc15ShopSeasonState`

Additional save-row capability interfaces may be added only when the same behavior must mutate the same property shape across eras. Prefer small capability interfaces over one giant `IAc15SaveData`.

## Playresult Architecture

Playresult is orchestration-heavy and should stay era-visible.

### Blue Flow

Blue handler flow:

1. Return success for `baid == 0`.
2. Return success and log for missing user.
3. Handle Blue Tokkun before battle and before normal writes.
4. Handle Blue battle before normal writes.
5. Bind Blue save row, catalog, active shop season, normal play tables, Dani tables, Mapperly delegates, and standard normal stage policy.
6. Call shared modules for common normal save mutation, Dani save, and normal play row writing.

Blue normal path should not contain duplicated timestamp parsing, medal mutation, unlock flag mutation, profile counter loops, or row write algorithms.

### Green Flow

Green handler flow:

1. Return success for `baid == 0`.
2. Return success and log for missing user.
3. Bind Green save row, catalog, active shop season, normal play tables, Dani tables, Mapperly delegates, and Green AI stage policy.
4. Use shared stage filtering with log-skip-success behavior.
5. Apply Green ghost upload facts through a Green-owned helper.
6. Call shared modules for common normal save mutation, Dani save, and normal play row writing.
7. Add Green ghost stage-section rows through a narrow Green helper during play-row insertion.

Green AI/ghost behavior is real era behavior and should not be forced into generic AC15 hooks. Green invalid or unsupported normal stage shapes should be logged and skipped with protocol success, not hard-fail the upload.

### Yellow Flow

Yellow handler flow:

1. Return success for `baid == 0`.
2. Return success and log for missing user.
3. Handle Yellow Tokkun-shaped payloads before normal writes.
4. Bind Yellow save row, catalog, active shop season, normal play tables, Dani tables, Mapperly delegates, and standard normal stage policy.
5. Use shared stage filtering with log-skip-success behavior.
6. Call shared modules for common normal save mutation, Dani save, and normal play row writing.
7. Log Yellow WaiWai stage facts after normal classification.

Yellow should be especially small after this design because its unique normal-path edges are Tokkun routing and WaiWai diagnostic logging.

## Playresult Shared Modules

### `Ac15PlayDatetime`

Interface:

```csharp
public static class Ac15PlayDatetime
{
    public static DateTime ParseOrNow(string playDatetime);
}
```

Behavior:

- Accept compact AC15 cabinet timestamps using `Constants.DateTimeFormat`.
- Accept the existing fallback string format currently supported by AC15 normal play.
- Return current local time only when parsing fails.

This module fixes the Green timestamp drift where save-row `LastPlayDatetime` differs from play/recent row timestamps.

### `Ac15NormalStageFilter`

Interface:

```csharp
public sealed record Ac15NormalStagePolicy(
    Func<CommonPlayResultData.StageData, Ac15StageSupportDecision> IsSupported,
    Func<CommonPlayResultData.StageData, CrownType, Ac15BestUpdatePolicy> GetBestUpdatePolicy);

public static class Ac15NormalStageFilter
{
    public static IReadOnlyList<CommonPlayResultData.StageData> Filter(
        uint baid,
        IEnumerable<CommonPlayResultData.StageData> stages,
        Ac15ProtocolLimits limits,
        Ac15NormalStagePolicy policy,
        ILogger logger);
}
```

Behavior:

- Enforce shared song and course bounds from `Ac15ProtocolLimits`.
- Apply the supplied stage policy for real behavior differences.
- Log unsupported stages with song, level, and stage mode.
- Return the valid subset.
- Returning an empty subset is not protocol failure; the era handler returns success without mutation.

Policies:

- Standard AC15 policy accepts normal stage modes only.
- Green policy accepts AI battle stage modes and applies the Green AI crown policy.

### `Ac15CommonProfileMutation`

Interface:

```csharp
public static class Ac15CommonProfileMutation
{
    public static Ac15ProfileMutationResult TryApply<TSave, TSeasonState>(
        TSave saveData,
        TSeasonState? shopSeasonState,
        CommonPlayResultData playResultData,
        IReadOnlyList<CommonPlayResultData.StageData> countedStages,
        Ac15SaveDataAccess<TSave> saveAccess,
        Ac15UnlockFlagAccess<TSave> unlockAccess,
        Ac15ProfileCounterAccess<TSave> counterAccess,
        Ac15ProtocolLimits limits,
        DateTime playTime);
}
```

Behavior:

- Detect Don/Katsu medal overflow before mutation.
- Add Don medals to the active shop season when present, otherwise to save data.
- Add Katsu medals to save data.
- Merge item-shop, Devil, Explain, WaiWai tutorial, and difficulty-played fields.
- Set `LastPlayDatetime` and `PrevAreaCode`.
- Apply current costume only when auto costume is enabled and the request contains current costume.
- Apply unlock flags that the save row supports.
- Apply profile counters only for the counted normal stages.

This module should use small save capability records or interfaces rather than a giant save-data abstraction. Song release unlock must be a separate capability because Green does not use the same song unlock behavior.

### `Ac15NormalPlayWriter`

Interface:

```csharp
public sealed record Ac15NormalPlayTables<TPlay, TBest, TFavorite, TRecent>(
    DbSet<TPlay> PlayRows,
    DbSet<TBest> BestRows,
    DbSet<TFavorite> FavoriteRows,
    DbSet<TRecent> RecentRows,
    Func<Ac15PlayRow, TPlay> CreatePlay,
    Func<uint, Ac15BestRow, bool, TBest> CreateBest,
    Action<TPlay, Ac15PlayRow>? AfterAddPlayRow = null)
    where TPlay : class, IAc15SongPlayDatum
    where TBest : class, IAc15SongBestDatum
    where TFavorite : class, IAc15FavoriteSong, new()
    where TRecent : class, IAc15RecentSong, new();

public static class Ac15NormalPlayWriter
{
    public static ValueTask SaveAsync<TPlay, TBest, TFavorite, TRecent>(
        ITaikoDbContext context,
        Ac15NormalPlayTables<TPlay, TBest, TFavorite, TRecent> tables,
        Ac15NormalPlayWriteRequest request,
        Ac15NormalStagePolicy policy,
        CancellationToken cancellationToken)
        where TPlay : class, IAc15SongPlayDatum
        where TBest : class, IAc15SongBestDatum
        where TFavorite : class, IAc15FavoriteSong, new()
        where TRecent : class, IAc15RecentSong, new();
}
```

Behavior:

- Map stage level to `Difficulty`.
- Map play result to `CrownType`.
- Insert play rows through the provided Mapperly delegate.
- Allow Green to attach ghost section rows through `AfterAddPlayRow`.
- Upsert best rows with the supplied best update policy.
- Preserve Dan-mode best suppression except for Shin stages.
- Update favorite rows with the era limit.
- Upsert and trim recent rows with the era limit.
- Save changes after writes.

This module replaces the repeated `profile.Era` switches in `Ac15NormalPlayService`.

## Dani Architecture

Dani save and readback share behavior across AC15 eras. Table selection does not.

### `Ac15DaniTables`

```csharp
public sealed record Ac15DaniTables<TScore, TStage>(
    DbSet<TScore> Scores,
    IQueryable<TScore> ScoresWithStages,
    Func<TScore, ICollection<TStage>> GetStages,
    Func<TScore, Ac15DaniScore> ToScore,
    Func<TScore, Ac15DaniScoreSummary> ToSummary,
    Func<Ac15DaniScore, TScore> CreateScore,
    Action<Ac15DaniScore, TScore> ApplyScore,
    Func<Ac15DaniStageScore, Ac15DaniScore, TStage> CreateStage,
    Action<Ac15DaniStageScore, TStage> ApplyStage)
    where TScore : class, IAc15DanScoreDatum
    where TStage : class, IAc15DanStageScoreDatum;
```

### `Ac15DaniWriter`

`Ac15DaniWriter` owns Dan-mode validation, challenge filtering, stage score aggregation, packed flag update calculation, `GotDanMax`, display Dan normalization, and Dan costume update calculation.

The era handler passes:

- concrete Dan score table;
- query including stage rows;
- Mapperly delegates;
- challenge rows from the era catalog;
- a typed save update callback for the era save row.

No `GameEra` switch is required.

### `Ac15DaniReadback`

`Ac15DaniReadback` owns requested-id filtering, known challenge filtering, Dan row query, stage ordering, `ArrivalSongCount` truncation, and projection to `CommonDanScoreDataResponse`.

Era `GetDanScoreQuery.*` handlers should only bind:

- concrete Dan table;
- era catalog challenge levels;
- Mapperly delegates;
- profile limits.

## Item Shop Architecture

Item shop purchase is one shared workflow with narrow policy differences.

Interface:

```csharp
public sealed record Ac15ItemShopPurchaseTables<TSeason, TItem>(
    DbSet<TItem> ItemStates,
    Func<uint, CancellationToken, ValueTask<TSeason>> GetOrCreateSeason,
    Func<Ac15PurchasedShopItem, DateTime, TItem> CreateItem)
    where TSeason : class, IAc15ShopSeasonState
    where TItem : class, IAc15ShopItemState;

public sealed record Ac15ItemShopUnlockPolicy<TSave>(
    Action<TSave, Ac15ShopItemType, uint> ApplyUnlock);

public static class Ac15ItemShopPurchase
{
    public static ValueTask<CommonItemPurchaseResponse> PurchaseAsync<TSave, TSeason, TItem>(
        ITaikoDbContext context,
        Ac15ItemShopPurchaseRequest request,
        Ac15ItemShopCatalog catalog,
        TSave saveData,
        Ac15ItemShopPurchaseTables<TSeason, TItem> tables,
        Ac15ItemShopUnlockPolicy<TSave> unlockPolicy,
        CancellationToken cancellationToken)
        where TSeason : class, IAc15ShopSeasonState
        where TItem : class, IAc15ShopItemState;
}
```

Behavior:

- Active-season missing and empty-season preflight behavior lives once.
- Purchase validation lives once.
- Duplicate purchase detection lives once.
- Don medal spend lives once.
- Purchased item row insertion lives once.
- Unlocks are delegated to an explicit policy.

Policies:

- Blue and Yellow song purchases set release-song flags.
- Green song purchases are a no-op for release-song flags.
- Tone, title, and costume unlocks use shared flag mutation where the save row supports the relevant capability.

This replaces `PurchaseBlueAsync`, `PurchaseGreenAsync`, `PurchaseYellowAsync`, nullable save-row union records, and duplicated unlock matrices.

## AdminApi User Settings Architecture

AdminApi AC15 user settings currently violate the controller rule by placing application behavior in controller partials.

### Controller Responsibility

Controllers keep:

- `/api/UserSettings/{baid}` legacy behavior;
- `/api/{era}/...` route parsing;
- `NotFound`, `BadRequest`, `Ok`, and `NoContent` translation;
- request cancellation token propagation.

Controllers do not:

- encode/decode AC15 bitsets;
- mutate AC15 save rows;
- query Dan score tables for selectable Taikojuku Dans;
- assemble the `UserSetting` object field by field.

### `Ac15UserSettingsService`

Interface shape:

```csharp
public static class Ac15UserSettingsService
{
    public static ValueTask<Ac15UserSettingsResult> GetAsync<TSave, TDanScore>(
        UserDatum user,
        TSave saveData,
        DbSet<TDanScore> danScores,
        Ac15UserSettingsAccess<TSave> saveAccess,
        Ac15ProtocolLimits limits,
        CancellationToken cancellationToken)
        where TDanScore : class, IAc15DanScoreDatum;

    public static ValueTask<Ac15UserSettingsResult> SaveAsync<TSave, TDanScore>(
        UserDatum user,
        TSave saveData,
        UserSetting request,
        DbSet<TDanScore> danScores,
        Ac15UserSettingsAccess<TSave> saveAccess,
        Ac15ProtocolLimits limits,
        CancellationToken cancellationToken)
        where TDanScore : class, IAc15DanScoreDatum;
}
```

Behavior:

- Validate display-level settings.
- Mutate shared identity fields on `UserDatum`.
- Mutate AC15 save-row customization fields.
- Encode/decode costume, title, and tone unlock bitsets.
- Select valid Taikojuku folder Dan from the era Dan table.
- Clamp unsafe stored display levels for response.
- Preserve omitted Dan setting behavior.

The era controller partial binds the correct save row, Dan table, limits, and access record. The module does not switch on era.

## Catalog Projection Architecture

`Ac15CatalogSnapshotFactory` should become one projection algorithm over a common source shape.

Options, in priority order:

1. Add narrow AC15 catalog source interfaces or records for item shop seasons, Taikojuku entries, telops, recommendations, and music order where the shapes are identical.
2. Use Mapperly overloads to map Blue, Green, and Yellow catalog entries to canonical AC15 catalog records, then run one snapshot assembly algorithm.

Do not merge era catalog loaders or source data directories. Blue, Green, and Yellow catalog implementations remain era-owned. Only mechanical projection to canonical AC15 snapshot data is shared.

Blue battle initial-data extras and Green ghost/AI advertisement remain era-local additions after canonical snapshot creation.

## Save Capability Interfaces

Add save capability interfaces only when they hide real repeated mutation. Suggested shape:

- `IAc15MedalSaveData`: Don/Katsu totals.
- `IAc15TutorialSaveData`: item-shop, Devil, Explain, WaiWai, difficulty-played fields.
- `IAc15PlayProfileSaveData`: `LastPlayDatetime`, `PrevAreaCode`, profile counters.
- `IAc15DaniSummarySaveData`: `GotDanFlg`, `GotDanExtraFlg`, `GotDanMax`, `DispTaikojukuDan`, auto costume state.
- `IAc15CustomizationSaveData`: current costume fields and costume flags.
- `IAc15SongUnlockSaveData`: release-song flags, implemented only by eras that actually use song unlock flags.

These interfaces are not repositories. They are compile-time row-shape contracts for identical save-row fields.

## Error Handling

- Missing users keep current era behavior: log and return protocol success for game playresult uploads.
- Unsupported normal stages are logged and skipped.
- No valid normal stages means protocol success with no mutation.
- Medal overflow means protocol success with no mutation and a warning.
- AdminApi validation errors return `BadRequest` without mutation.
- Unsupported features remain absent rather than log-and-success stubs.

## Testing Strategy

Tests must verify observable behavior, not source shape.

Required behavior coverage:

- Green compact timestamp persists to save row and play/recent rows consistently.
- Green unsupported stages are skipped with success and valid mixed stages still persist.
- Blue Tokkun and battle remain before normal writes and do not write normal score/crown/Dani state.
- Yellow Tokkun remains before normal writes.
- Yellow WaiWai remains diagnostic/play-history only where supported.
- Green ghost upload and ghost stage-section behavior remains Green-owned.
- Normal play row, best row, favorite, and recent writes stay in the bound era tables only.
- Dani save/readback ignores unknown ids, preserves stage ordering, truncates by `ArrivalSongCount`, and never reads another era table.
- Item-shop purchase keeps Green song unlock no-op and Blue/Yellow song unlock behavior.
- AdminApi user settings preserve existing Blue, Green, and Yellow WebUI/API behavior and no-cross-era state.

Verification should include focused AC15/Blue/Green/Yellow tests, full `Tests/Tests.csproj`, and a temp-output Host build.

## Migration Plan

1. Fix the two Phase 16.2 review blockers first:
   - shared AC15 timestamp parser for Green save-row timestamp;
   - Green log-skip-success stage filtering.
2. Replace `IAc15EraHooks` in normal play with explicit `Ac15NormalStagePolicy`.
3. Convert `Ac15NormalPlayService` into switch-free normal play modules:
   - `Ac15PlayDatetime`;
   - `Ac15NormalStageFilter`;
   - `Ac15CommonProfileMutation`;
   - `Ac15NormalPlayWriter`.
4. Convert Dani save/readback to `Ac15DaniTables<TScore,TStage>` plus switch-free writer/readback modules.
5. Convert item-shop purchase to generic item-shop tables plus explicit unlock policies.
6. Extract AdminApi AC15 user settings behavior into `Ac15UserSettingsService`.
7. Simplify catalog projection after runtime behavior is stable.
8. Re-run code review against the Phase 16.2 review findings and update the review artifact.

## Acceptance Criteria

The implementation of this design is successful when:

- shared `Application/Ac15` modules contain no table-only `GameEra` switches;
- no shared module exposes era-named entry points for identical behavior;
- era handlers remain readable as composition roots;
- Blue, Green, and Yellow EF tables remain physically separate;
- no repository-shaped persistence layer or table-only era adapter classes are added;
- Green timestamp and unsupported-stage blocker fixes are covered by behavior tests;
- AdminApi controllers no longer own AC15 settings business behavior;
- existing AC15, Blue, Green, and Yellow behavior tests plus a temp-output Host build pass.

## Design Decisions

- Prefer capability composition over era inheritance or `BlueLike`/`GreenLike` base classes.
- Prefer generic row helpers over repositories for table-separated persistence.
- Prefer narrow policies over broad no-op hooks.
- Keep special modes explicit in era handlers.
- Keep Mapperly as the only generation layer for mechanical projection.
- Keep future Red support in mind, but do not add Red stubs or unsupported routes now.
