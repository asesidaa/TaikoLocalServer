# Phase 13: Yellow Catalog and AC15 Core Foundation - Research

**Researched:** 2026-06-08
**Domain:** ASP.NET Core AC15 catalog loading, era-owned Yellow catalog contracts, Mediator-backed protocol readback, and no-persistence guardrails
**Confidence:** HIGH for local repo/proto/data patterns; MEDIUM for optional Yellow sidecars because only required operator data is present locally

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

### Yellow Profile and AC15 Core Contracts
- D-01: Define a first-class `Ac15EraProfiles.Yellow` profile and supporting contracts in Phase 13 from Yellow proto/data evidence and the approved capability-driven AC15 core design.
- D-02: Reuse existing AC15 shared catalog, initial-data, metadata, and readback services where formats and behavior match, but only through Yellow-owned catalog contracts and Yellow capability flags.
- D-03: Omit unsupported Yellow features from initial-data advertisements rather than returning empty stubs or fake availability rows.
- D-04: Make Yellow metadata routes catalog-backed in Phase 13, while keeping persistence and gameplay writes deferred to later Yellow phases.

### Yellow Catalog Data and Metadata Surfaces
- D-05: Require `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin` from `Host/wwwroot/data/yellow/data`, using `config/ST9100-1` as the versioned Yellow config root.
- D-06: Load supported sidecar JSON, XML, or binary files only when present and evidence-backed; missing optional metadata should produce absent or empty advertised rows, not startup failure.
- D-07: Add Yellow item-shop catalog/readback shape only in Phase 13; do not add purchase, medal spend, unlock, or persistence semantics until Phase 15.
- D-08: Replace Phase 12 no-state Yellow metadata scaffolds with Mediator/catalog-backed routes only for Phase 13 metadata surfaces.

### the agent's Discretion
- D-09: Choose the smallest set of Yellow catalog contracts, mappers, loaders, and tests that proves YCAT-01 through YCAT-04 without broadening into Phase 14-16 runtime behavior.
- D-10: Decide whether to extract additional shared AC15 helper methods during planning only when they remove real duplication and keep Yellow route, wire DTO, catalog contract, and persistence boundaries era-owned.

### Deferred Ideas (OUT OF SCOPE)
Yellow identity/profile/userdata, Yellow normal playresult persistence, crowns/self-best runtime readback, Dani playresult persistence, item purchases, Don/Katsu medal accounting, WaiWai persistence/logging, AdminApi/WebUI readback, Yellow Tokkun runtime behavior, Yellow Banacoin-adjacent compatibility, runtime cabinet/RPCS3 smoke verification, and final Yellow contract documentation remain deferred to Phases 14-17. Yellow battle remains out of scope unless new concrete Yellow evidence appears.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| YCAT-01 | Yellow catalog initialization loads required local data from `Host/wwwroot/data/yellow/data/config/ST9100-1` and related Yellow data roots through `PathHelper` and era catalog helpers. | Local Yellow data contains `config/ST9100-1/musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`. `PathHelper.GetDataPath(GameEra.Yellow)` already resolves era data root shape; Yellow needs `YellowGameDataPaths`, `YellowRequiredDataFiles`, and `YellowEraGameDataCatalog`. |
| YCAT-02 | Yellow uses AC15 shared catalog/loaders/services only where formats and behavior match, while preserving Yellow-owned catalog contracts and feature capability flags. | AC15 shared loaders and services already exist for music info, tuning, item shop JSON, telops, folders, recommendations, movies, initial data, catalog readback, and Taikojuku. Blue/Green expose era-owned `IBlueCatalog`/`IGreenCatalog`; Yellow should add `IYellowCatalog` and Yellow catalog entry wrappers rather than sharing Blue/Green contracts. |
| YCAT-03 | Yellow initial data advertises only protocol-supported Yellow telop, folder, Taikojuku/Dani, item shop, legal terms, and metadata availability. | Yellow proto `InitialdatacheckResponse` exposes `ary_telop_data`, `ary_eventfolder_data`, `ary_taikojuku_data`, `ary_itemshop_data`, `ary_legalterms_data`, `is_danplay`, `is_itemshop`, and no Blue battle fields. Yellow profile/wire placement should drive only these rows. |
| YCAT-04 | Yellow folder, telop, recommendation, tournament, gacha, challenge, movie, and other metadata routes return Yellow catalog-backed responses where the Yellow proto/data supports them. | Current Green/Blue catalog readback supports folder, telop, item shop, Taikojuku, recommend, tournament/challenge empty stubs, and movie discovery. Yellow Phase 13 can make supported catalog metadata routes catalog-backed while leaving stateful routes no-state. |
</phase_requirements>

<research_summary>
## Summary

Phase 13 should turn the Phase 12 Yellow route scaffold into a first catalog-backed Yellow slice, not a gameplay runtime slice. The local Yellow data root already has the required operator data under `Host/wwwroot/data/yellow/data/config/ST9100-1` plus `fumen/tuning.bin`; the implementation should add Yellow-specific path constants and required-file checks that resolve through `PathHelper.GetDataPath(GameEra.Yellow)`.

The repo already has the correct shared AC15 primitives. `Ac15MusicInfoLoader`, `Ac15TuningLoader`, `Ac15TaikojukuLoader`, `Ac15ItemShopLoader`, `Ac15TelopLoader`, `Ac15EventFolderLoader`, `Ac15RecommendLoader`, `Ac15MovieLoader`, `Ac15InitialDataService`, `Ac15CatalogSnapshotFactory`, and `Ac15CatalogReadbackService` are the reuse points. Yellow still needs era-owned catalog interfaces and wrapper entries so route, wire DTO, and persistence boundaries remain Yellow-owned.

The safest implementation split is three waves: Yellow catalog contract/loaders and required data proof; Yellow AC15 profile/snapshot/initial-data/Taikojuku support; then Yellow metadata controller/mappers replacing only the Phase 13-owned no-state scaffold routes. BAID, userdata, playresult, selfbest, itempurchase, crowns runtime, Banacoin, Tokkun, medals, AdminApi/WebUI, and battle remain out of scope.

**Primary recommendation:** Add `IYellowCatalog`/`YellowEraGameDataCatalog` and Yellow AC15 profile support first, then wire only catalog-backed Yellow initial-data and metadata routes through Mediator while preserving no-state behavior for deferred runtime routes.
</research_summary>

<architectural_responsibility_map>
## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|--------------|----------------|-----------|
| Yellow required data roots | Infrastructure catalog | Host content layout | Runtime file paths belong under `PathHelper` plus era path helpers, not controllers or handlers. |
| Yellow catalog contract | Application abstractions/catalog DTOs | Infrastructure loaders | Application defines era-owned catalog shape; Infrastructure fills it from local data. |
| AC15 Yellow profile | Application AC15 core | Yellow adapter mappers | Profile/capability/wire-placement choices drive shared services and Yellow wire mapping. |
| Initial-data availability | Application handlers/services | Yellow adapter mapper | Handler builds common DTO from Yellow catalog snapshot; adapter maps to Yellow wire fields. |
| Metadata readback routes | Yellow adapter controllers | Application handlers/services | Controllers remain thin and call Mediator; business/catalog behavior sits in handlers/services. |
| Deferred stateful runtime routes | Later phases | Tests/source guards | No Phase 13 persistence or gameplay writes should be introduced. |
</architectural_responsibility_map>

<standard_stack>
## Standard Stack

### Core

| Library / Tool | Version | Purpose | Why Standard |
|----------------|---------|---------|--------------|
| .NET SDK | repo targets `net10.0` | Build/test Host, adapters, Application, Infrastructure | Existing solution baseline. |
| ASP.NET Core MVC | repo package set | Yellow protocol controllers | Existing Blue/Green/Yellow adapters use `[ApiController]`, `[Route]`, `[HttpPost]`, and protobuf output. |
| Mediator | repo package set | Application request dispatch | Existing Blue/Green catalog routes call Mediator from controllers. |
| protobuf-net | repo package set | Generated Yellow wire DTO serialization | Yellow wire already generated in Phase 12. |
| xUnit | repo package set | Catalog, mapper, handler, route, and source guard tests | Existing `Tests/Ac15`, `Tests/Blue`, `Tests/Green`, and `Tests/Yellow` patterns. |

### Supporting

| Library / Tool | Purpose | When to Use |
|----------------|---------|-------------|
| `PathHelper` | Resolve runtime/published data roots | Every new runtime data path should flow through `PathHelper.GetDataPath(GameEra.Yellow)`. |
| AC15 shared loaders | Parse matching AC15 XML/bin/JSON formats | Use where Yellow data shape matches Blue/Green. |
| `Ac15CatalogSnapshot` | Common catalog view for shared services | Add Yellow snapshot mapping rather than duplicating service logic. |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| `IYellowCatalog` | Cast Yellow to `IGreenCatalog` or `IBlueCatalog` | Rejected: violates era-owned catalog contracts and makes later Yellow persistence/readback harder to audit. |
| Shared generated AC15 wire DTOs | One AC15 wire assembly | Rejected by Phase 12 and AGENTS.md; Yellow wire remains adapter-local. |
| Implement all Yellow route runtime behavior now | Broad Phase 13 runtime slice | Rejected: Phase 13 is catalog/core only; Phases 14-16 own stateful routes. |
</standard_stack>

<architecture_patterns>
## Architecture Patterns

### System Architecture Diagram

```text
Yellow operator data root
  Host/wwwroot/data/yellow/data
        |
        v
YellowGameDataPaths + YellowRequiredDataFiles
        |
        v
YellowEraGameDataCatalog : IYellowCatalog
        |
        +--> Ac15 shared loaders parse matching files
        +--> Optional sidecars return empty/disabled when absent
        |
        v
Ac15CatalogSnapshotFactory.FromYellow(...)
        |
        v
Application handlers
  GetInitialDataQuery.Yellow
  GetTelopQuery.Yellow
  GetFolderQuery.Yellow
  GetTaikojukuQuery.Yellow
  GetItemShopInfoQuery.Yellow
  Recommend / tournament / challenge catalog responses
        |
        v
Adapters.GameProtocol.Yellow mappers/controllers
  Yellow wire DTOs only
  /v09r00/chassis catalog-backed metadata routes
```

### Recommended Project Structure

```text
Application/
  Abstractions/IYellowCatalog.cs
  Catalog/Yellow/*.cs
  Common/CatalogExtensions.cs
  Ac15/Ac15EraProfiles.cs
  Ac15/Ac15CatalogSnapshotFactory.cs
  Dtos/CommonInitialDataCheckResponse.Yellow.cs
  Handlers/*Query.Yellow.cs

Infrastructure/GameDataCatalog/Yellow/
  YellowGameDataPaths.cs
  YellowRequiredDataFiles.cs
  YellowMusicInfoLoader.cs
  YellowTaikojukuLoader.cs
  YellowItemShopLoader.cs
  YellowTelopLoader.cs
  YellowRecommendLoader.cs
  YellowGachaLoader.cs
  YellowTournamentLoader.cs
  YellowMovieLoader.cs
  YellowEraGameDataCatalog.cs

Adapters.GameProtocol.Yellow/
  Controllers/catalog-backed route controllers
  Mappers/InitialDataMappers.cs
  Mappers/FolderDataMappers.cs
  Mappers/GetTelopMappers.cs
  Mappers/TaikojukuMappers.cs
  Mappers/ItemShopMappers.cs
  Mappers/RecommendMappers.cs
  Mappers/TournamentMappers.cs
  Mappers/ChallengeCompeMappers.cs

Tests/
  Yellow/YellowCatalogLoaderTests.cs
  Yellow/YellowInitialDataProtocolTests.cs
  Yellow/YellowMetadataRouteTests.cs
  Yellow/YellowCatalogBoundaryTests.cs
  Ac15/Ac15EraProfileTests.cs
```

### Pattern 1: Era-Owned Catalog With Shared AC15 Loaders

**What:** Use Yellow-specific catalog entries and interface, but map from shared AC15 loader results.

**When to use:** `musicinfo.xml`, `musicmedleyinfo.xml`, `tuning.bin`, item shop JSON, telop JSON, event folders, recommendations, movies, gacha/tournament empty catalogs.

**Example analogs:** `BlueMusicInfoLoader` and `GreenMusicInfoLoader` both call `Ac15MusicInfoLoader.LoadFromFileAsync(...)` and map to era-specific entry types.

### Pattern 2: Partial Handler Dispatch

**What:** Add `GameEra.Yellow` branches in existing dispatchers and implement Yellow in `.Yellow.cs` partials.

**When to use:** `GetInitialDataQuery`, `GetTelopQuery`, `GetFolderQuery`, `GetTaikojukuQuery`, `GetItemShopInfoQuery`, and only catalog-backed metadata handlers.

**Example analogs:** `GetInitialDataQuery.Blue.cs`, `GetInitialDataQuery.Green.cs`, `GetTelopQuery.Blue.cs`, `GetFolderQuery.Green.cs`.

### Pattern 3: Mapper Copies Wire Shape, Not Business Logic

**What:** Yellow mappers translate `Common*` DTOs into Yellow wire DTO fields. They should not query catalogs, EF, settings, or make route decisions.

**When to use:** Initial data, folder, telop, Taikojuku, item shop info, recommend, tournament, challenge.

### Anti-Patterns to Avoid

- Casting Yellow catalog to Blue/Green catalog contracts.
- Adding Yellow EF tables or migrations in Phase 13.
- Making `playresult.php`, `userdata.php`, `baidcheck.php`, `selfbest.php`, `itempurchase.php`, `crownsdata.php`, Banacoin, or Tokkun routes Mediator-backed in Phase 13.
- Advertising item shop, telop, folder, or Taikojuku rows when the backing catalog is missing/empty.
- Copying Blue battle initial-data fields or Blue battle fallback into Yellow.
</architecture_patterns>

<dont_hand_roll>
## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| AC15 XML parsing | New Yellow-only parser | `Ac15MusicInfoLoader` / `Ac15TaikojukuLoader` | Formats match existing AC15 loaders. |
| Tuning star parsing | Yellow binary parser | `Ac15TuningLoader.LoadFromFileAsync(path, nameof(GameEra.Yellow), ...)` | Existing parser validates record table and Ura records. |
| Catalog readback response construction | Per-controller data assembly | `Ac15CatalogReadbackService` | Centralizes folder/telop/recommend/item shop behavior. |
| Initial-data flags | Manual bitset code in mapper | `Ac15InitialDataService` and `Ac15ProtocolBytes` | Existing bitset behavior hides active shop songs correctly. |
| Route business logic | Controller-local catalog access | Mediator handlers | Keeps controller thin per AGENTS.md. |
</dont_hand_roll>

<common_pitfalls>
## Common Pitfalls

### Pitfall 1: Yellow Contract Disappears Into Green
**What goes wrong:** Yellow reuses `IGreenCatalog`/Green loaders directly.
**Why it happens:** Current Green paths are close to Yellow and easy to copy.
**How to avoid:** Add `IYellowCatalog` and Yellow entry wrappers even when mapping from shared AC15 entries.
**Warning signs:** `GameEra.Green`, `GreenEraGameDataCatalog`, or `Green*Entry` appears in Yellow handlers/controllers.

### Pitfall 2: Optional Sidecars Become Required
**What goes wrong:** Missing Yellow telop/folder/recommend/movie/shop JSON fails startup.
**Why it happens:** Required data checks are too broad.
**How to avoid:** Required files are only `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`; optional sidecars return empty or disabled outputs unless explicitly enabled.
**Warning signs:** `YellowRequiredDataFiles` includes `yellow_telop_data.json`, event folder JSON, recommend JSON, or item shop JSON.

### Pitfall 3: Phase 13 Accidentally Starts Phase 14-16
**What goes wrong:** BAID/userdata/playresult/selfbest/shop purchase/Tokkun routes become stateful.
**Why it happens:** Phase 12 scaffolds are all in one file and can be broadly replaced.
**How to avoid:** Replace only Phase 13 metadata surfaces and keep source guards for deferred runtime routes.
**Warning signs:** New Yellow EF entities, migrations, `UpdatePlayResultCommand.Yellow.cs`, `UserDataQuery.Yellow.cs`, `ItemPurchaseCommand.Yellow.cs`, or Yellow Tokkun files.
</common_pitfalls>

<validation_architecture>
## Validation Architecture

Phase 13 should validate in three layers:

1. Catalog layer: focused Yellow tests prove required files, optional-sidecar fallback, shared loader reuse, and Yellow DI registration.
2. Application/profile layer: AC15 and Yellow tests prove `Ac15EraProfiles.Yellow`, `Ac15CatalogSnapshotFactory.FromYellow`, initial-data row advertisement, item-shop readback shape, and Taikojuku response behavior.
3. Adapter/route layer: Yellow route/mapping tests prove only Phase 13-owned routes switched from no-state scaffold to Mediator/catalog-backed behavior, and deferred runtime routes stayed no-state.

Recommended focused commands:

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowCatalog|FullyQualifiedName~YellowInitialData|FullyQualifiedName~Ac15EraProfile"`
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowMetadata|FullyQualifiedName~YellowCatalogBoundary"`
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Yellow"`
- `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase13-yellow"`
</validation_architecture>

<open_questions>
## Open Questions (RESOLVED)

1. **Should Phase 13 use real Yellow item purchases or medals?**
   - What we know: Phase context says item-shop catalog/readback shape only.
   - Resolution: Phase 13 plans only `getitemshopinfo.php` catalog readback; `itempurchase.php` and medal writes remain Phase 15.

2. **Should missing optional Yellow metadata fail startup?**
   - What we know: Phase context says optional sidecars should be absent/empty, not startup failure.
   - Resolution: Plans require missing optional sidecars to return empty/disabled rows.

3. **Should Yellow `getreitai.php` be added now?**
   - What we know: Phase 12 excluded it without route evidence, and Phase 13 context preserves Phase 12 evidence boundaries.
   - Resolution: Do not add `getreitai.php` in Phase 13.
</open_questions>

<sources>
## Sources

### Primary (HIGH confidence)
- `.planning/phases/13-yellow-catalog-and-ac15-core-foundation/13-CONTEXT.md` - locked Phase 13 scope and deferred boundaries.
- `.planning/phases/12-yellow-evidence-and-era-foundation/12-YELLOW-EVIDENCE.md` - `/v09r00`, shared startup/version, no-battle, and adapter-local Yellow wire decisions.
- `proto/yellow/yellow.proto` - Yellow initial-data, metadata, item shop, and no-battle wire fields.
- `Host/wwwroot/data/yellow/data/config/ST9100-1` and `Host/wwwroot/data/yellow/data/fumen/tuning.bin` - local required Yellow data.
- `Application/Ac15/*` - shared AC15 services and profiles.
- `Infrastructure/GameDataCatalog/Ac15/*`, `Blue/*`, and `Green/*` - loader and era catalog patterns.
- `Adapters.GameProtocol.Yellow/Controllers/YellowScaffoldControllers.cs` - Phase 12 no-state scaffold to replace only for Phase 13 metadata surfaces.

### Secondary (MEDIUM confidence)
- Existing Green recommend/tournament/challenge stub patterns: useful analogs, but Yellow should only claim catalog-backed behavior where actual catalog data/sidecars support it.
</sources>

<metadata>
## Metadata

**Research scope:**
- Core technology: ASP.NET Core 10, Mediator, protobuf-net, AC15 catalog loaders.
- Ecosystem: local repo patterns only; no internet/package research needed.
- Patterns: era-owned catalog contracts, shared AC15 loaders/services, partial handler dispatch, adapter-local Yellow wire mapping.
- Pitfalls: optional metadata failure, cross-era contract leakage, deferred runtime behavior.

**Confidence breakdown:**
- Standard stack: HIGH - verified from current repo.
- Architecture: HIGH - verified from Blue/Green/Yellow code and Phase 12 artifacts.
- Pitfalls: HIGH - derived from explicit context and existing GSD constraints.
- Optional Yellow sidecar data: MEDIUM - local required data exists; optional sidecar inventory is limited and should be treated as optional.

**Research date:** 2026-06-08
**Valid until:** 2026-07-08 for repo-local planning unless Yellow runtime/client evidence changes.
</metadata>

## RESEARCH COMPLETE

*Phase: 13-yellow-catalog-and-ac15-core-foundation*
*Research completed: 2026-06-08*
*Ready for planning: yes*
