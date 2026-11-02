# Phase 13: Yellow Catalog and AC15 Core Foundation - Context

**Gathered:** 2026-06-08
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 13 loads Yellow `ST9100-1` catalog data from the Yellow operator data root and establishes first-class Yellow AC15 profile/core contracts. It replaces Phase 12 no-state Yellow metadata scaffolds with Mediator/catalog-backed behavior only for catalog, initial-data, item-shop readback shape, and metadata surfaces. It does not add Yellow persistence tables, gameplay writes, purchases, medals, normal play, Dani playresult persistence, Tokkun runtime behavior, Banacoin wallet/payment behavior, AdminApi/WebUI readback, or Yellow battle behavior.

</domain>

<decisions>
## Implementation Decisions

### Yellow Profile and AC15 Core Contracts
- Define a first-class `Ac15EraProfiles.Yellow` profile and supporting contracts in Phase 13 from Yellow proto/data evidence and the approved capability-driven AC15 core design.
- Reuse existing AC15 shared catalog, initial-data, metadata, and readback services where formats and behavior match, but only through Yellow-owned catalog contracts and Yellow capability flags.
- Omit unsupported Yellow features from initial-data advertisements rather than returning empty stubs or fake availability rows.
- Make Yellow metadata routes catalog-backed in Phase 13, while keeping persistence and gameplay writes deferred to later Yellow phases.

### Yellow Catalog Data and Metadata Surfaces
- Require `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin` from `Host/wwwroot/data/yellow/data`, using `config/ST9100-1` as the versioned Yellow config root.
- Load supported sidecar JSON, XML, or binary files only when present and evidence-backed; missing optional metadata should produce absent or empty advertised rows, not startup failure.
- Add Yellow item-shop catalog/readback shape only in Phase 13; do not add purchase, medal spend, unlock, or persistence semantics until Phase 15.
- Replace Phase 12 no-state Yellow metadata scaffolds with Mediator/catalog-backed routes only for Phase 13 metadata surfaces.

### the agent's Discretion
- Choose the smallest set of Yellow catalog contracts, mappers, loaders, and tests that proves YCAT-01 through YCAT-04 without broadening into Phase 14-16 runtime behavior.
- Decide whether to extract additional shared AC15 helper methods during planning only when they remove real duplication and keep Yellow route, wire DTO, catalog contract, and persistence boundaries era-owned.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Application/Ac15/Ac15EraProfiles.cs`, `Application/Ac15/Ac15EraProfile.cs`, `Application/Ac15/Ac15FeatureSet.cs`, `Application/Ac15/Ac15ProtocolLimits.cs`, and `Application/Ac15/Ac15WirePlacement.cs` are the current Blue/Green AC15 profile anchors to extend for Yellow.
- `Application/Ac15/Ac15InitialDataService.cs`, `Application/Ac15/Ac15CatalogReadbackService.cs`, and `Application/Ac15/Ac15CatalogSnapshot.cs` already model shared initial-data and metadata readback behavior for folders, telops, recommendations, item-shop info, and Taikojuku advertisement rows.
- `Infrastructure/GameDataCatalog/Ac15/` contains reusable loaders for AC15 music info, tuning, Taikojuku, item shop, event folders, telops, recommendations, and movies.
- `Infrastructure/GameDataCatalog/Blue/BlueEraGameDataCatalog.cs` and `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs` show how AC15 era catalogs initialize required files, optional sidecars, shared music dictionaries, item-shop catalogs, event folders, telops, gacha/tournament data, recommendations, and movies.
- `Adapters.GameProtocol.Yellow/Controllers/YellowScaffoldControllers.cs` contains the Phase 12 direct-protobuf no-state route scaffolds to replace with Mediator/catalog-backed controllers where Phase 13 owns the behavior.
- `Tests/Ac15/` has profile, catalog loader, optional loader, initial-data, and catalog-readback service tests that can be extended for Yellow; `Tests/Yellow/` has route, host, wire-generation, evidence, shared-version, and no-battle guardrails from Phase 12.

### Established Patterns
- Era behavior stays first-class and separate: route adapters, wire DTOs, catalog interfaces, mappers, and persistence contracts are era-owned even when shared AC15 services perform common logic.
- `IGameDataCatalog.For(GameEra)` and `IEraGameDataCatalog` are the runtime catalog access points; `FileGameDataCatalog` initializes enabled era catalogs at startup.
- Required catalog files throw with concrete paths; optional sidecar catalogs return empty or disabled models and log recoverable absence where appropriate.
- Controllers should deserialize Yellow wire DTOs, map to common DTOs, call Mediator/application services, and map back to Yellow wire responses; business behavior should not stay inside controllers.
- Generated Yellow wire models under `Adapters.GameProtocol.Yellow/Wire/` are generated artifacts and should not be manually cleaned up unless protocol generation is intentionally rerun.
- Phase 12 locked `/v09r00/chassis/*` for Yellow game routes and `/v01r00/chassis/*` shared startup/version ownership; Phase 13 should preserve those boundaries.

### Integration Points
- Add a Yellow catalog interface/implementation alongside existing Blue and Green catalog contracts, then register it in `Infrastructure/DependencyInjection.cs` when `GameEra.Yellow` is enabled.
- Resolve Yellow paths through `PathHelper` and era data helpers, not hardcoded runtime paths in handlers or controllers.
- Map Yellow initial-data and metadata controllers through Mediator/catalog-backed application requests before returning Yellow wire responses.
- Use Yellow proto evidence for supported metadata route fields: `initialdatacheck.php`, `gettelop.php`, `getfolder.php`, `taikojuku.php`, `getitemshopinfo.php`, `tournamentcheck.php`, `recommend.php`, and `challengecompe.php`; preserve Phase 12 absence for unproven `getreitai.php`.
- Add focused Yellow catalog tests for required data files, optional metadata absence, Yellow profile/capability flags, initial-data advertisements, and metadata route/mapping behavior.

</code_context>

<specifics>
## Specific Ideas

Use `proto/yellow/yellow.proto`, `proto/yellow/vsinterface.proto`, and `.planning/phases/12-yellow-evidence-and-era-foundation/12-YELLOW-EVIDENCE.md` as the Phase 13 protocol/evidence boundary. Use `Host/wwwroot/data/yellow/data/config/ST9100-1` for versioned Yellow config and keep local Yellow operator data under `Host/wwwroot/data/yellow/data`.

</specifics>

<deferred>
## Deferred Ideas

Yellow identity/profile/userdata, Yellow normal playresult persistence, crowns/self-best runtime readback, Dani playresult persistence, item purchases, Don/Katsu medal accounting, WaiWai persistence/logging, AdminApi/WebUI readback, Yellow Tokkun runtime behavior, Yellow Banacoin-adjacent compatibility, runtime cabinet/RPCS3 smoke verification, and final Yellow contract documentation remain deferred to Phases 14-17. Yellow battle remains out of scope unless new concrete Yellow evidence appears.

</deferred>
