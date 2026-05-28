# Coding Conventions

**Analysis Date:** 2026-05-28

## Naming Patterns

**Files:**
- Use one primary type per `.cs` file, named after the type: `Application/Common/BitsetCodec.cs`, `Adapters.AdminApi/Controllers/EraRoute.cs`, `Domain/Entities/UserSaveDataGreen.cs`.
- Use era suffixes on partial files when behavior branches by game era: `Application/Handlers/UpdatePlayResultCommand.Green.cs`, `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, `Application/Dtos/CommonUserDataResponse.Green.cs`.
- Use feature or adapter folders as namespaces: `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`, `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs`, `Tests/Green/GreenPlayResultHandlerTests.cs`.
- Generated protobuf files live under `Adapters.GameProtocol.* /Wire/` and are treated as generated extension points: `Adapters.GameProtocol.Green/Wire/Game.cs`, `Adapters.GameProtocol.Blue/Wire/Game.cs`, `Adapters.GameProtocol.WwR08/Wire/VsInterface.cs`.
- Razor pages use `.razor` plus optional `.razor.cs` code-behind: `TaikoWebUI/Pages/DaniDojo.razor`, `TaikoWebUI/Pages/DaniDojo.razor.cs`, `TaikoWebUI/Components/Song/SongLeaderboardCard.razor.cs`.

**Functions:**
- Use PascalCase for methods, including private helpers: `HandleGreen(...)` in `Application/Handlers/UpdatePlayResultCommand.Green.cs`, `MapStage(...)` in `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`, `IsDate(...)` in `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs`.
- Use `Try*` names for non-throwing parse/lookup methods with `out` parameters: `EraRoute.TryParse(...)` in `Adapters.AdminApi/Controllers/EraRoute.cs`, `TryGetSelectedDan(...)` in `TaikoWebUI/Pages/DaniDojo.razor.cs`.
- Use `Map*`, `Build*`, `Apply*`, and `Upsert*` verbs for transformations and persistence steps: `MapGreenClearGrade(...)` in `Adapters.AdminApi/Controllers/DanBestDataController.cs`, `ApplyUnlockBits(...)` and `UpsertBestAsync(...)` in `Application/Handlers/UpdatePlayResultCommand.Green.cs`.
- Handler entry methods implement `Handle(...)` for mediator requests and delegate to era-specific partial handlers: `Application/Handlers/UpdatePlayResultCommand.cs`.

**Variables:**
- Use camelCase for locals and parameters: `activeShopSeason`, `shopSeasonState`, `playResultData` in `Application/Handlers/UpdatePlayResultCommand.Green.cs`.
- Use private readonly fields in camelCase without an underscore: `connection` in `Tests/Green/GreenHandlerFixture.cs`, `client` and cache dictionaries in `TaikoWebUI/Services/GameDataService.cs`.
- Some WebUI state fields use leading underscores for backing maps such as `_bestDataMap` in `TaikoWebUI/Pages/DaniDojo.razor.cs`; keep this local to component backing state rather than spreading it to service or handler code.
- Use domain names from the protocol/database model as-is even when terse: `Baid`, `SongNo`, `DanId`, `VerupNo` in `Domain/Entities/UserSaveDataGreen.cs`, `Application/Catalog/Green/GreenItemShopEntry.cs`, and `Adapters.GameProtocol.Green/Wire/Game.cs`.

**Types:**
- Use PascalCase for classes, records, enums, and request/response DTOs: `GreenItemShopCatalog` in `Application/Catalog/Green/GreenItemShopCatalog.cs`, `CommonPlayResultData` in `Application/Dtos/CommonPlayResultData.cs`, `GameEra` in `Domain/Enums/GameEra.cs`.
- Use `I` prefixes for interfaces: `IGameDataCatalog`, `IGreenCatalog`, and `ITaikoDbContext` in `Application/Abstractions/`.
- Use `readonly record struct` for small immutable mediator request values: `UpdatePlayResultCommand` in `Application/Handlers/UpdatePlayResultCommand.cs`.
- Use `sealed` on helper, fixture, and loader classes where inheritance is not intended: `BlueHandlerFixture` in `Tests/Blue/BlueHandlerFixture.cs`, `GreenMusicInfoLoader` in `Infrastructure/GameDataCatalog/Green/GreenMusicInfoLoader.cs`.

## Code Style

**Formatting:**
- No `.editorconfig` is detected. Follow the style already enforced by examples in `Application/Handlers/UpdatePlayResultCommand.Green.cs`, `Adapters.AdminApi/Controllers/DanBestDataController.cs`, and `Tests/Green/GreenPlayResultHandlerTests.cs`.
- C# defaults are centralized in `Directory.Build.props`: `net10.0`, nullable enabled, implicit usings enabled, `LangVersion` 13, and warnings not treated as errors.
- Package versions are centralized in `Directory.Packages.props`; project files such as `Application/Application.csproj` and `Tests/Tests.csproj` use versionless `<PackageReference />` entries.
- Use file-scoped namespaces: `namespace TaikoLocalServer.Application.Common;` in `Application/Common/BitsetCodec.cs` and `namespace TaikoLocalServer.Tests.Green;` in `Tests/Green/GreenProtocolBytesTests.cs`.
- Prefer target-typed `new`, collection expressions, and null-coalescing defaults where the codebase uses them: `new()` and `[]` in `Application/Catalog/Green/GreenItemShopCatalog.cs`, `Tests/Green/GreenHandlerFixture.cs`, and `Application/Dtos/CommonUserDataResponse.Green.cs`.
- Keep object initializers explicit for DTO and entity projection code: `CommonInitialDataCheckResponse` in `Application/Handlers/GetInitialDataQuery.Green.cs`, `DanBestDataResponse` in `Adapters.AdminApi/Controllers/DanBestDataController.cs`.
- Keep generated wire files out of style cleanup unless regenerating the protocol output: `Adapters.GameProtocol.Green/Wire/Game.cs`, `Adapters.GameProtocol.Blue/Wire/Game.cs`.

**Linting:**
- No analyzer config, StyleCop, Roslynator, or `dotnet format` config is detected. `Directory.Build.props` explicitly sets `<TreatWarningsAsErrors>false</TreatWarningsAsErrors>`.
- Mapperly warnings `RMG020` and `RMG012` are suppressed per adapter project where generated mapper behavior is accepted: `Adapters.GameProtocol.Green/Adapters.GameProtocol.Green.csproj`, `Adapters.GameProtocol.Blue/Adapters.GameProtocol.Blue.csproj`, `Adapters.AdminApi/Adapters.AdminApi.csproj`.
- Use `dotnet build TaikoLocalServer.slnx` as the broad compile quality gate and `dotnet build Host/Host.csproj -o <temp-output>` when avoiding locked `Host/bin` output.

## Import Organization

**Order:**
1. Project and framework global usings first; each project owns its broad imports in `Application/GlobalUsings.cs`, `Adapters.AdminApi/GlobalUsings.cs`, `Tests/GlobalUsings.cs`, and `TaikoWebUI/GlobalUsings.cs`.
2. File-local `using` directives for exceptional framework or package imports: `System.Buffers.Binary` and `System.Text` in `Tests/Green/GreenCatalogLoaderTests.cs`, `Riok.Mapperly.Abstractions` in `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`.
3. File-local project aliases only when names would collide: `AppMovieData`, `GreenStartupAuthRequest`, and `SharedStartupAuthRequest` in `Tests/Green/StartupAuthRouteTests.cs`.

**Path Aliases:**
- No C# path aliasing is configured. Use project references from `.csproj` files and normal namespaces, as in `Application/Application.csproj`, `Host/Host.csproj`, and `Tests/Tests.csproj`.
- For Blazor, use `_Imports.razor` and project `GlobalUsings.cs` instead of repeating common imports in every component: `TaikoWebUI/_Imports.razor`, `TaikoWebUI/GlobalUsings.cs`.

## Error Handling

**Patterns:**
- Domain/service code throws `InvalidOperationException` for unsupported eras or impossible state: `Application/Handlers/UpdatePlayResultCommand.cs`, `Application/Handlers/GetInitialDataQuery.cs`, `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs`.
- Data loader code throws `InvalidDataException` or `FileNotFoundException` with file paths and row context for malformed required inputs: `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs`, `Infrastructure/GameDataCatalog/Ac15/Ac15MusicInfoLoader.cs`, `Infrastructure/GameDataCatalog/Green/GreenRequiredDataFiles.cs`.
- Optional or disabled catalog features return empty/disabled models instead of throwing: `Ac15ItemShopCatalog.Disabled` via `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs`, missing event folders via `Infrastructure/GameDataCatalog/Ac15/Ac15EventFolderLoader.cs`.
- Protocol controllers catch decode/processing exceptions when the cabinet expects success-shaped protobuf responses and return protocol result `0`: `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`.
- Admin API controllers return HTTP results (`BadRequest`, `NotFound`, `Unauthorized`, `Forbid`) for caller-visible validation and authorization failures: `Adapters.AdminApi/Controllers/AuthController.cs`, `Adapters.AdminApi/Controllers/FavoriteSongsController.cs`, `Adapters.AdminApi/Controllers/UserSettingsController.Green.cs`.
- Use `Throw` package guards where existing code uses them for compact validation: `Contracts.AdminApi/Converters/PlaySettingConverter.cs`, `TaikoWebUI/Pages/DaniDojo.razor.cs`, `Application/Handlers/PurchaseSongCommand.Nijiiro.cs`.
- Use BCL argument helpers for shared utilities: `ArgumentOutOfRangeException.ThrowIfNegative(...)` in `Application/Common/BitsetCodec.cs`.

## Logging

**Framework:** Serilog in `Host/Program.cs`, `Microsoft.Extensions.Logging` in handlers/loaders/controllers.

**Patterns:**
- Use structured message templates with named properties: `logger.LogWarning("Rejecting invalid Green playresult payload for baid {Baid}", request.Baid)` in `Application/Handlers/UpdatePlayResultCommand.Green.cs`.
- Use controller `Logger` from the base controller for protocol request logging: `Adapters.GameProtocol.Green/Controllers/PlayResultController.cs`, `Adapters.GameProtocol.Shared/Controllers/BaseProtocolController.cs`.
- Use boot and request logging through Serilog setup in `Host/Program.cs`, including fatal startup errors, unknown 404s, and unsuccessful non-401 responses.
- Use warning logs for recoverable catalog/runtime fallbacks: `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`, `Infrastructure/GameDataCatalog/Ac15/Ac15MovieLoader.cs`.
- Tests use `NullLogger<T>.Instance` for quiet handler tests and local recording loggers only when assertions inspect log events: `Tests/Green/GreenPlayResultHandlerTests.cs`, `Tests/Green/GreenCatalogLoaderTests.cs`.

## Comments

**When to Comment:**
- Comment non-obvious runtime or deployment behavior, not routine assignments: disabled adapter filtering and cache behavior in `Host/Program.cs`, Green game-data copy behavior in `Host/Host.csproj`.
- Keep comments on protocol-specific semantic exceptions where a future maintainer needs the rule: Green Dani normal scoring comment in `Application/Handlers/UpdatePlayResultCommand.Green.cs`.
- Avoid adding comments to straightforward DTO/entity properties such as `Domain/Entities/UserSaveDataGreen.cs` or `Application/Dtos/CommonUserDataResponse.Green.cs`.

**JSDoc/TSDoc:**
- XML documentation is not a dominant pattern. Prefer clear type and method names in `Application/Handlers/`, `Infrastructure/GameDataCatalog/`, and `Adapters.*`.
- Generated wire files may include generator comments; do not extend them manually in `Adapters.GameProtocol.Green/Wire/Game.cs` or `Adapters.GameProtocol.Blue/Wire/Game.cs`.

## Function Design

**Size:** Handler partials can be long when encoding protocol persistence rules, but keep reusable logic in private helpers inside the same era partial. `Application/Handlers/UpdatePlayResultCommand.Green.cs` uses `IsValidGreenStage`, `ApplyCostume`, `SaveStageAsync`, and `UpsertBestAsync` rather than one monolithic block.

**Parameters:** Pass `CancellationToken` through async handlers and loaders: `HandleGreen(..., CancellationToken)` in `Application/Handlers/UpdatePlayResultCommand.Green.cs`, `LoadFromFileAsync(..., CancellationToken)` in `Infrastructure/GameDataCatalog/Green/GreenMusicInfoLoader.cs`.

**Return Values:** Use domain/protocol result codes where cabinet protocols require them (`uint` result in `Application/Handlers/UpdatePlayResultCommand.cs`); use DTO responses for Admin API projections (`DanBestDataResponse` in `Adapters.AdminApi/Controllers/DanBestDataController.cs`); use disabled/empty models for optional catalogs (`GreenItemShopCatalog.Disabled` in `Application/Catalog/Green/GreenItemShopCatalog.cs`).

## Module Design

**Exports:** Prefer narrow public surfaces. Many helpers remain `private static` inside their owning class, such as `MapSeason` in `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs` and `ReadTenBitValue` in `Tests/Green/GreenProtocolBytesTests.cs`.

**Barrel Files:** No barrel-file pattern is used. Cross-project access is through project references, namespaces, and global usings: `TaikoLocalServer.slnx`, `Application/GlobalUsings.cs`, `Tests/GlobalUsings.cs`.

**Mediator/Partial Handlers:**
- Put shared request records and era dispatch in the unsuffixed file: `Application/Handlers/UpdatePlayResultCommand.cs`.
- Put era-specific behavior in `.Nijiiro.cs`, `.Green.cs`, and `.Blue.cs` partial files: `Application/Handlers/UpdatePlayResultCommand.Green.cs`, `Application/Handlers/UpdatePlayResultCommand.Blue.cs`.
- Throw on unsupported eras in shared dispatch rather than silently falling through: `Application/Handlers/GetTaikojukuQuery.cs`, `Application/Handlers/GetSelfBestQuery.cs`.

**Mapper Design:**
- Use Mapperly `[Mapper]` static partial classes for adapter mappers, but keep custom field semantics explicit in hand-written methods: `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`, `Adapters.AdminApi/Mapping/AuthConfigMapper.cs`.
- Do not map generated protobuf DTOs directly into persistence entities; map through common DTOs in `Application/Dtos/`, as shown by `CommonPlayResultData` and `PlayResultMappers` in `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`.

**Project Skills:**
- No repository-local `.codex/skills/` or `.agents/skills/` directories are detected, so no additional project-skill conventions apply.

---

*Convention analysis: 2026-05-28*
