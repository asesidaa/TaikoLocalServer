# Coding Conventions

**Analysis Date:** 2026-06-11

## Naming Patterns

**Files:**
- Use one primary type per `.cs` file, named after the type: `Application/Ac15/Ac15NormalPlayWriter.cs`, `Application/Ac15/Ac15DaniWriter.cs`, `Adapters.AdminApi/Controllers/EraRoute.cs`.
- Use `Ac15*` prefixes for shared Blue/Green/Yellow behavior, canonical records, profiles, policies, and helper services: `Application/Ac15/Ac15EraProfiles.cs`, `Application/Ac15/Ac15FeatureSet.cs`, `Application/Ac15/Ac15ItemShopPurchase.cs`.
- Use era suffixes on partial handlers when request dispatch branches by game era: `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, `Application/Handlers/UpdatePlayResultCommand.Green.cs`, `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`.
- Keep unsuffixed partial files for shared dispatch or shared helpers: `Application/Handlers/UpdatePlayResultCommand.cs`, `Application/Handlers/UpdatePlayResultCommand.Ac15.cs`, `Application/Handlers/ItemPurchaseCommand.cs`.
- Keep adapter-local wire mapping under each era adapter: `Adapters.GameProtocol.Blue/Mappers/UserDataMappers.cs`, `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`, `Adapters.GameProtocol.Yellow/Mappers/InitialDataMappers.cs`.
- Keep generated protobuf files under adapter `Wire/` folders and do not manually clean them up: `Adapters.GameProtocol.Blue/Wire/Game.cs`, `Adapters.GameProtocol.Green/Wire/Game.cs`, `Adapters.GameProtocol.Yellow/Wire/Game.cs`.
- Keep era-owned EF state in era-named entities and context partials: `Domain/Entities/UserSaveDataBlue.cs`, `Domain/Entities/UserSaveDataGreen.cs`, `Domain/Entities/UserSaveDataYellow.cs`, `Infrastructure/Persistence/TaikoDbContext.Blue.cs`, `Infrastructure/Persistence/TaikoDbContext.Yellow.cs`.
- Razor components use `.razor` and optional `.razor.cs` code-behind: `TaikoWebUI/Pages/DaniDojo.razor`, `TaikoWebUI/Pages/DaniDojo.razor.cs`.

**Functions:**
- Use PascalCase for methods, including private helpers: `HandleBlue(...)` in `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, `BuildResponse(...)` in `Application/Ac15/Ac15UserDataService.cs`, `MapStage(...)` in `Adapters.GameProtocol.Yellow/Mappers/PlayResultMappers.cs`.
- Use `Try*` for non-throwing parse or mutation attempts: `EraRoute.TryParse(...)` in `Adapters.AdminApi/Controllers/EraRoute.cs`, `Ac15CommonProfileMutation.TryApply(...)` in `Application/Ac15/Ac15CommonProfileMutation.cs`.
- Use `Map*`, `Build*`, `Apply*`, `Save*`, `Load*`, and `Upsert*` verbs according to role: mapper methods in `Application/Ac15/Ac15DaniMapper.cs`, response builders in `Application/Ac15/Ac15InitialDataService.cs`, persistence writers in `Application/Ac15/Ac15NormalPlayWriter.cs`.
- Mediator handlers implement `Handle(...)` in the unsuffixed file and delegate to era partials: `Application/Handlers/UserDataQuery.cs`, `Application/Handlers/ItemPurchaseCommand.cs`.
- Async methods pass through `CancellationToken` and use `Async` suffix where they perform asynchronous work: `LoadFromFileAsync(...)` in `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs`, `SaveAsync(...)` in `Application/Ac15/Ac15DaniWriter.cs`.

**Variables:**
- Use camelCase for locals and parameters: `playResultData`, `validStages`, `shopSeasonState`, and `playTime` in `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`.
- Use private readonly fields in camelCase without an underscore: `connection` in `Tests/Blue/BlueHandlerFixture.cs`, `connection` in `Tests/Yellow/YellowHandlerFixture.cs`.
- Preserve protocol/domain names even when terse or legacy-shaped: `Baid`, `SongNo`, `DanId`, `VerupNo`, `TokkunTutorialFlg`, and `TookunSongnoesJson` in `Domain/Entities/` and `Application/Dtos/`.
- Use explicit tuple names when passing capability facts across AC15 helpers: `(uint ItemType, uint ItemId)` in `Application/Ac15/BlueAc15UserDataAdapter.cs` and `Application/Handlers/UserDataQuery.Yellow.cs`.

**Types:**
- Use PascalCase for classes, records, enums, and DTOs: `Ac15NormalPlayWriteRequest`, `Ac15DaniScore`, `Ac15FeatureSet`, `Ac15ShopItemStatus`.
- Use `I` prefixes for interfaces, including narrow AC15 row-shape and save-capability interfaces: `Domain/Entities/IAc15SongPlayDatum.cs`, `Domain/Entities/IAc15DanScoreDatum.cs`, `Domain/Entities/IAc15SaveDataCapabilities.cs`.
- Use `sealed record` or `readonly record struct` for immutable request/configuration values: `Application/Ac15/Ac15EraProfile.cs`, `Application/Ac15/Ac15ShopSeasonPolicy.cs`, `Application/Handlers/UpdatePlayResultCommand.cs`.
- Use `sealed` for helper/fixture/loader classes where inheritance is not intended: `Tests/Ac15/Ac15NormalPlayWriterTests.cs`, `Tests/Yellow/YellowHandlerFixture.cs`, `Infrastructure/GameDataCatalog/Ac15/Ac15MusicInfoLoader.cs`.
- Keep canonical AC15 records free of generated wire types: `Application/Ac15/Ac15DaniRecords.cs`, `Application/Ac15/Ac15NormalPlayRecords.cs`, `Application/Ac15/Ac15UserDataRecords.cs`.

## Code Style

**Formatting:**
- No `.editorconfig`, Prettier, ESLint, or Biome config is detected. Follow existing C# formatting in `Application/Ac15/Ac15NormalPlayWriter.cs`, `Adapters.GameProtocol.Yellow/Mappers/UserDataMappers.cs`, and `Tests/Ac15/Ac15ItemShopPurchaseTests.cs`.
- C# defaults live in `Directory.Build.props`: `net10.0`, nullable enabled, implicit usings enabled, `LangVersion` 13, and warnings not treated as errors.
- Package versions are centralized in `Directory.Packages.props`; project files such as `Application/Application.csproj`, `Adapters.GameProtocol.Blue/Adapters.GameProtocol.Blue.csproj`, and `Tests/Tests.csproj` use versionless `<PackageReference />` entries.
- Prefer file-scoped namespaces for new files, matching `Application/Ac15/Ac15ItemShopPurchase.cs` and `Tests/Yellow/YellowUserDataProtocolTests.cs`. Leave legacy block-scoped namespaces alone unless already editing the file for behavior, as in `Infrastructure/Persistence/TaikoDbContext.cs`.
- Prefer target-typed `new`, collection expressions, and null-coalescing defaults where already used: `[]` in `Application/Ac15/Ac15UserDataService.cs`, `new()` in `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs`, and typed collection literals in `Tests/Yellow/YellowPlayResultHandlerTests.cs`.
- Keep DTO/entity object initializers explicit when mapping protocol, AdminApi, or persistence shapes: `Application/Ac15/Ac15NormalPlayMapper.cs`, `Adapters.AdminApi/Controllers/PlayDataController.Yellow.cs`, `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs`.
- Keep generated `Wire/` files out of manual style cleanup unless regenerating protobuf output: `Adapters.GameProtocol.Blue/Wire/Game.cs`, `Adapters.GameProtocol.Yellow/Wire/Game.cs`.

**Linting:**
- No analyzer config, StyleCop, Roslynator, or `dotnet format` config is detected.
- Mapperly strictness is configured with assembly-level defaults in AC15-aware projects: `Application/MapperlyDefaults.cs`, `Adapters.GameProtocol.Blue/MapperlyDefaults.cs`, `Adapters.GameProtocol.Green/MapperlyDefaults.cs`, `Adapters.GameProtocol.Yellow/MapperlyDefaults.cs`.
- Mapperly defaults use `AutoUserMappings = false` and `RequiredMappingStrategy = RequiredMappingStrategy.Target`; new Mapperly maps must either map every target or explicitly ignore/handle target members.
- Mapperly warning suppressions remain project-local only where still configured: `Adapters.AdminApi/Adapters.AdminApi.csproj`, `Adapters.GameProtocol.WwR08/Adapters.GameProtocol.WwR08.csproj`, `Adapters.GameProtocol.CnR00/Adapters.GameProtocol.CnR00.csproj`.
- Use `dotnet build TaikoLocalServer.slnx` as the broad compile gate and `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` when a running server may lock normal Host output.

## Import Organization

**Order:**
1. Prefer project `GlobalUsings.cs` for broad imports: `Application/GlobalUsings.cs`, `Adapters.AdminApi/GlobalUsings.cs`, `Tests/GlobalUsings.cs`, `TaikoWebUI/GlobalUsings.cs`.
2. Use file-local `using` directives for exceptional framework/package imports: `System.Text.Json` in `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs`, `Riok.Mapperly.Abstractions` in `Application/Ac15/Ac15DaniMapper.cs`, `Microsoft.AspNetCore.Mvc` in controller tests.
3. Use file-local aliases only when generated wire or controller names collide across eras: `Tests/Yellow/YellowCompatibilityResponseShapeTests.cs`.

**Path Aliases:**
- No C# path aliasing is configured. Use project references and namespaces from `.csproj` files such as `Application/Application.csproj`, `Adapters.GameProtocol.Yellow/Adapters.GameProtocol.Yellow.csproj`, and `Tests/Tests.csproj`.
- For Blazor components, use `_Imports.razor` and `TaikoWebUI/GlobalUsings.cs` instead of repeating common imports in every component.

## Error Handling

**Patterns:**
- Shared dispatch throws `InvalidOperationException` for unsupported eras instead of falling through: `Application/Handlers/UpdatePlayResultCommand.cs`, `Application/Handlers/UserDataQuery.cs`, `Application/Handlers/ItemPurchaseCommand.cs`.
- Required catalog/load data failures use `InvalidDataException` or `FileNotFoundException` with the era name, path, and row context: `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs`, `Infrastructure/GameDataCatalog/Ac15/Ac15EventFolderLoader.cs`, `Infrastructure/GameDataCatalog/Blue/BlueRequiredDataFiles.cs`.
- Optional catalog features return disabled or empty models instead of throwing: `Application/Catalog/Ac15/Ac15ItemShopCatalog.cs`, `Infrastructure/GameDataCatalog/Ac15/Ac15EventFolderLoader.cs`, `Application/Ac15/Ac15CatalogSnapshotFactory.cs`.
- Protocol controllers log decode/processing failures and return protocol-shaped failure when the cabinet expects protobuf instead of HTTP errors: `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`.
- Stateless compatibility endpoints log and return success-shaped protocol responses without creating authority or persistence: `Adapters.GameProtocol.Blue/Controllers/GetBanacoinInfoController.cs`, `Adapters.GameProtocol.Yellow/Controllers/BanacoinPaymentController.cs`, `Adapters.GameProtocol.Yellow/Controllers/RewardExecutionController.cs`.
- Admin API controllers use HTTP results for caller-visible failures: `BadRequest`, `NotFound`, `Unauthorized`, `Forbid`, and `NoContent` in `Adapters.AdminApi/Controllers/AuthController.cs`, `Adapters.AdminApi/Controllers/UserSettingsController.Yellow.cs`, and `Adapters.AdminApi/Controllers/EraRoute.cs`.
- Use `CancellationToken.ThrowIfCancellationRequested()` in shared operations that may be called before I/O: `Application/Ac15/Ac15ItemShopPurchase.cs`.
- Use BCL argument helpers or the existing `Throw` package only where the surrounding code already does so: `Application/Common/BitsetCodec.cs`, `Contracts.AdminApi/Converters/PlaySettingConverter.cs`.

## Logging

**Framework:** Serilog in `Host/Program.cs`; `Microsoft.Extensions.Logging` in handlers, loaders, controllers, and tests.

**Patterns:**
- Use structured message templates with named properties: `"Skipping AC15 Dani save for baid {Baid}: unknown Dan id {DanId}"` in `Application/Ac15/Ac15DaniWriter.cs`.
- Protocol controllers log incoming requests or key request metadata before mapping: `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs`, `Adapters.GameProtocol.Yellow/Controllers/PlayResultController.cs`, `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`.
- Use warning logs for recoverable no-write decisions and invalid cabinet payloads: `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, `Application/Handlers/UpdatePlayResultCommand.Green.cs`, `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`.
- Use information logs for captured diagnostic facts that are intentionally not state-changing: Yellow WaiWai facts in `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`.
- Tests use `NullLogger<T>.Instance` for quiet handler tests and local recording loggers only when assertions inspect log behavior: `Tests/Blue/BluePlayResultHandlerTests.cs`, `Tests/Blue/BlueCatalogLoaderTests.cs`.

## Comments

**When to Comment:**
- Comment evidence-backed protocol exceptions and client-crash constraints: Green Taikojuku display-Dan safety in `Application/Handlers/UserDataQuery.Green.cs` and `Adapters.GameProtocol.Green/Mappers/UserDataMappers.cs`.
- Comment runtime/deployment behavior where future changes could break local or published setups: `Host/Host.csproj`, `Host/Program.cs`.
- Avoid comments for routine DTO/entity assignments or one-to-one property copies: `Domain/Entities/UserSaveDataYellow.cs`, `Application/Ac15/Ac15ItemShopMapper.cs`.

**JSDoc/TSDoc:**
- XML documentation is not a dominant pattern. Prefer clear type names and local evidence comments in `Application/Ac15/`, `Application/Handlers/`, and `Infrastructure/GameDataCatalog/`.
- Generated wire comments belong to generated files only; do not manually add documentation to `Adapters.GameProtocol.Blue/Wire/Game.cs`, `Adapters.GameProtocol.Green/Wire/Game.cs`, or `Adapters.GameProtocol.Yellow/Wire/Game.cs`.

## Function Design

**Size:** Keep shared AC15 algorithms in `Application/Ac15/` and keep era partial handlers focused on selecting concrete era tables, profiles, catalogs, and side effects. Use `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, `Application/Handlers/UpdatePlayResultCommand.Green.cs`, and `Application/Handlers/UpdatePlayResultCommand.Yellow.cs` as the model.

**Parameters:** Pass `CancellationToken` through handler, loader, and persistence calls. Pass explicit capability records/tables instead of hiding EF behind repository-shaped wrappers: `Ac15NormalPlayTables<TPlay,TBest,TFavorite,TRecent>` in `Application/Ac15/Ac15NormalPlayWriter.cs`, `Ac15DaniTables<TScore,TStage>` in `Application/Ac15/Ac15DaniRecords.cs`.

**Return Values:** Use cabinet protocol result codes where protocol routes require them (`uint` from `UpdatePlayResultCommand`), common DTOs for application-level responses (`CommonUserDataResponse`, `CommonInitialDataCheckResponse`), and disabled/empty catalog models for optional data (`Ac15ItemShopCatalog.Disabled`).

## Module Design

**Exports:** Prefer narrow public surfaces. Shared AC15 services are public static only when reused across handlers/tests, while implementation helpers remain private: `Application/Ac15/Ac15NormalPlayWriter.cs`, `Application/Ac15/Ac15DaniWriter.cs`, `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs`.

**Barrel Files:** No barrel-file pattern is used. Cross-project access is through project references, namespaces, and global usings: `TaikoLocalServer.slnx`, `Application/GlobalUsings.cs`, `Tests/GlobalUsings.cs`.

**Mediator/Partial Handlers:**
- Put request records and era dispatch in unsuffixed files: `Application/Handlers/UpdatePlayResultCommand.cs`, `Application/Handlers/UserDataQuery.cs`, `Application/Handlers/ItemPurchaseCommand.cs`.
- Put era-specific behavior in `.Nijiiro.cs`, `.Green.cs`, `.Blue.cs`, and `.Yellow.cs` partials: `Application/Handlers/UserDataQuery.Yellow.cs`, `Application/Handlers/GetInitialDataQuery.Blue.cs`, `Application/Handlers/GetTelopQuery.Green.cs`.
- Put cross-era AC15-only helpers in `.Ac15.cs` partials only when they are handler-local glue: `Application/Handlers/UpdatePlayResultCommand.Ac15.cs`.
- Throw on unsupported eras in shared dispatch; do not silently no-op an era branch.

**AC15 Shared-Core Design:**
- Share behavior by capability, not by pretending one era is another. Use canonical AC15 profiles and records in `Application/Ac15/Ac15EraProfiles.cs`, `Application/Ac15/Ac15FeatureSet.cs`, and `Application/Ac15/Ac15DaniRecords.cs`.
- Keep direct `ITaikoDbContext` and concrete `DbSet` selection visible at the handler boundary; do not introduce repository-shaped persistence wrappers for AC15 state. Examples: `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, `Application/Handlers/GetDanScoreQuery.Yellow.cs`.
- Use narrow Domain row-shape interfaces for generic EF helpers only when the row shape is truly identical: `Domain/Entities/IAc15SongPlayDatum.cs`, `Domain/Entities/IAc15SongBestDatum.cs`, `Domain/Entities/IAc15ShopItemState.cs`.
- Keep era-only protocol facts and gameplay semantics local: Green ghost behavior in `Application/Handlers/UpdatePlayResultCommand.Green.cs`, Blue battle behavior in `Application/Handlers/UpdatePlayResultCommand.BlueBattle.cs`, Yellow Tokkun handling in `Application/Handlers/UpdatePlayResultCommand.YellowTokkun.cs`.
- Use `Ac15EraProfile` and `Ac15WirePlacement` for shared response-building decisions; keep actual generated wire placement in adapter-local mappers: `Application/Ac15/Ac15UserDataService.cs`, `Adapters.GameProtocol.Blue/Mappers/UserDataMappers.cs`, `Adapters.GameProtocol.Yellow/Mappers/UserDataMappers.cs`.

**Mapper Design:**
- Use Mapperly `[Mapper]` static partial classes for mechanical projections: `Application/Ac15/Ac15DaniMapper.cs`, `Application/Ac15/Ac15NormalPlayMapper.cs`, `Adapters.GameProtocol.Yellow/Mappers/InitialDataMappers.cs`.
- Keep nontrivial semantics in handwritten methods around Mapperly-generated partials: Tokkun/battle classification in `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs`, Yellow Tokkun classification in `Adapters.GameProtocol.Yellow/Mappers/PlayResultMappers.cs`, status/timestamp setting in `Application/Ac15/Ac15ItemShopMapper.cs`.
- Use `[MapProperty]` for wire name differences and `[MapperIgnoreTarget]` for intentionally owner-populated/navigation fields: `Adapters.GameProtocol.Green/Mappers/UserDataMappers.cs`, `Application/Ac15/Ac15DaniMapper.cs`.
- Do not persist generated protobuf DTOs directly. Map wire DTOs to `Application/Dtos/Common*` shapes before handlers and map common responses back to wire in the adapter: `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs`, `Application/Dtos/CommonPlayResultData.cs`, `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs`.

**Persistence Boundaries:**
- Keep Blue, Green, Yellow, and Nijiiro gameplay state in separate EF entities and DbSets unless the state is truly shared identity/card data: `Infrastructure/Persistence/TaikoDbContext.Blue.cs`, `Infrastructure/Persistence/TaikoDbContext.Green.cs`, `Infrastructure/Persistence/TaikoDbContext.Yellow.cs`.
- Shared AC15 writers may operate over generic row-shape interfaces, but handlers must bind them to concrete era tables and Mapperly projections: `Application/Ac15/Ac15NormalPlayWriter.cs`, `Application/Handlers/UpdatePlayResultCommand.Blue.cs`.
- New era state belongs in `Domain/Entities/` and the matching `Infrastructure/Persistence/TaikoDbContext.<Era>.cs` partial, with migrations under `Infrastructure/Persistence/Migrations/`.

**Project Skills:**
- Repo-local GSD skills exist under `.codex/skills/`; codebase mapping is governed by `.codex/skills/gsd-map-codebase/SKILL.md`.
- Source conventions and constraints are anchored by `AGENTS.md`, `.planning/STATE.md`, and current implementation files, not by stale planning summaries.

---

*Convention analysis: 2026-06-11*
