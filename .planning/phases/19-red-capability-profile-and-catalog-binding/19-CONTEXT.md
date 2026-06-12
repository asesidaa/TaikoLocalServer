# Phase 19: Red Capability Profile and Catalog Binding - Context

**Gathered:** 2026-06-13
**Status:** Ready for planning
**Mode:** Smart discuss with user-approved recommended options

<domain>
## Phase Boundary

Phase 19 binds Red catalog data and Red capability/profile configuration on top of the Phase 18 Red adapter foundation. It may add Red catalog loaders, Red `IGameDataCatalog` registration, `Ac15EraProfiles.Red`, Red catalog readback handlers, Red mapper/controller routing for catalog-backed metadata routes, and focused catalog/profile tests.

It must not add Red EF gameplay tables, migrations, profile/userdata writes, normal-play/Dani/Tokkun persistence, ChallengeCompe state, item-shop/medal behavior, Banacoin wallet/payment authority, AdminApi, or WebUI.

</domain>

<decisions>
## Implementation Decisions

### Catalog Binding
- Use the active Red root proven in Phase 18: `Host/wwwroot/data/red/data/config/ST8100-1`; `ST5100-*` and `ST7100-1` remain inactive or historical.
- Resolve Red runtime paths through `PathHelper.GetDataPath(GameEra.Red)` plus a Red path helper. Do not hardcode `wwwroot/data/red` from handlers.
- Reuse shared AC15 loaders for music, tuning, Taikojuku, event folders, telops, recommendations, movies, and customization where Red files match the existing shapes.
- Add Red-specific parser code only if the active Red local files fail the shared parser contract.

### Capability Profile
- Add `Ac15EraProfiles.Red` only after recording Red-specific evidence: IDB user-data handlers copy 128-byte song/title-style arrays and a 16-byte tone-style array; IDB crown handling expands 1024 ten-bit crown entries; IDB Taikojuku response handling caps songs per pack at 10; active Red Taikojuku data covers normal and extra challenge levels.
- Red features include normal play, userdata, self-best, crowns, initial-data, folders, telops, recommendations, Taikojuku, and Dani. Item shop stays disabled because Red proto/IDA evidence does not support later-era shop or medal behavior.
- Red wire placement should match proven Red proto surfaces only. Tokkun tutorial readback exists in Red userdata but Phase 20 owns tutorial state and readback behavior.

### Route Binding
- Replace Phase 18 no-state probes only for Phase 19-owned catalog metadata routes: `initialdatacheck.php`, `getfolder.php`, `gettelop.php`, `recommend.php`, and `taikojuku.php`.
- Keep `tournamentcheck.php`, `challengecompe.php`, `playresult.php`, profile/userdata/state routes, and compatibility/payment-adjacent routes as probes until their owning phases implement behavior.
- Extend shared startup movie lookup with HDD major version `8 => GameEra.Red` after the Red catalog can provide movies.

### Tests And Verification
- Tests should exercise Red catalog loader outputs, Red AC15 profile capability/limit decisions, startup movie readback, and catalog-backed handler/controller mapping. Avoid source-shape, route inventory, generated-wire, and "Result = 1" echo tests.
- Keep existing supported eras preserved; any shared code changes must be covered by focused existing AC15/Yellow/Blue/Green tests plus a temp-output Host build.

### the agent's Discretion
- The exact Red catalog entry type shape may use shared `Application.Catalog.Ac15` records rather than duplicating Yellow/Blue record classes when no Red-specific fields exist.
- Red sidecar JSON may be intentionally empty when the feature is supported but no server-authored Red data has been proven yet.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Infrastructure/GameDataCatalog/Ac15/*Loader.cs` already parses the common AC15 music, tuning, Taikojuku, folder, telop, recommendation, movie, and customization formats.
- `Application/Ac15/Ac15CatalogSnapshotFactory`, `Ac15InitialDataService`, `Ac15CatalogReadbackService`, and `Ac15TaikojukuService` already implement shared catalog response behavior.
- `Adapters.GameProtocol.Yellow` has thin controllers and Mapperly mappers for the same metadata route families; Red should mirror wire mapping shape without copying Yellow business logic.
- `BlueCustomizationCatalogExtractor` contains generic AC15 customization extraction logic that can be promoted into a shared AC15 extractor instead of copied for Red.

### Established Patterns
- Era catalogs implement `IEraGameDataCatalog` and are registered in `Infrastructure/DependencyInjection.cs` only when the era is enabled.
- Catalog-backed handlers select by `GameEra` in unsuffixed dispatcher files and put era-specific composition in `.Red.cs`, `.Yellow.cs`, `.Blue.cs`, or `.Green.cs` partials.
- AC15 shared modules receive an era profile and catalog snapshot; they should not switch internally on `GameEra.Red`.

### Integration Points
- Add `IRedCatalog` and `CatalogExtensions.Red()`.
- Add `Infrastructure/GameDataCatalog/Red/*` wrappers around shared AC15 loaders and register `RedEraGameDataCatalog`.
- Add `Ac15EraProfiles.Red` with Red features and evidence-backed limits.
- Add Red partial handlers for initial-data, folders, telops, recommendations, Taikojuku, and startup movies.
- Add Red mappers/controllers for Phase 19-owned metadata routes only.

</code_context>

<specifics>
## Specific Ideas

- User explicitly requested no code duplication and said Phases 19-20 should mostly compose mechanisms already implemented in Green, Blue, and Yellow.
- User explicitly required Red binary evidence from `.tools/red/EBOOT.ELF.i64` through `ida-cli` with the existing daemon.
- IDA evidence used in this phase:
  - `EntryNetwork::OnCrownsDataResponse` at `sub_1C0004` expands crown data with `li r5, 0x400` and 10-bit crown unpacking.
  - `EntryNetwork::OnUserDataResponse` at `sub_1BDCFC` copies 128-byte arrays and a 16-byte array into Red player state.
  - `game::net::OnTaikoJukuResponse` at `sub_D2620` branches when the pack song count exceeds 9, confirming the 10-song cap.
- Active Red local Taikojuku data under `ST8100-1/musicmedleyinfo.xml` contains challenge levels from 1 through 113, matching the existing normal/extra AC15 range model.

</specifics>

<deferred>
## Deferred Ideas

- Red profile/userdata/normal/Dani/Tokkun/simple compatibility persistence belongs to Phase 20.
- Shared older-AC15 ChallengeCompe behavior and stateful semantics belong to Phase 21.
- Red AdminApi/WebUI readback belongs to Phase 22.

</deferred>
