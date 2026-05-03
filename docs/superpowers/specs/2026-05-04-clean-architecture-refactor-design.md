# Clean Architecture Refactor — Design

**Status:** Draft — design content approved across all six sections; awaiting file review before plan handoff
**Date:** 2026-05-04
**Scope:** Server-side restructure of TaikoLocalServer into a hexagonal / ports-and-adapters layout, plus light TaikoWebUI cleanup. Functional behavior unchanged.

## 1. Goals & non-goals

### Goals

1. Restructure the server into a hexagonal / ports-and-adapters layout so the **core game logic is shared** across game-protocol versions, while each version's wire shape, route paths, and quirks live behind a project-reference boundary.
2. Make adding a new legacy generation (older protobuf shape, different routes) a **drop-in-one-project** operation that doesn't touch Domain / Application / Infrastructure / other adapters.
3. Eliminate the mixed style where some controllers go through Mediator handlers and others directly through wrapper data-access services. After the refactor, **business operations live in Mediator handlers**; admin controllers may use `ITaikoDbContext` directly for simple CRUD.
4. Take advantage of the move to split a few overgrown files (`GameDataService` 566 LOC, `AuthService` 230 LOC) along their natural seams.
5. Tidy WebUI: drop the legacy `TaikoWebUI.sln`, fix the `Services/Extentions/` typo if it survives the move, repoint WebUI at the new `Contracts.AdminApi`.

### Non-goals

- DB schema changes — `Migrations/` history is preserved; entity files move location/namespace, not column shape. Operators' `taiko.db3` keeps working untouched.
- Functional changes to game endpoints, admin API, or WebUI behavior. Same wire bytes in, same wire bytes out.
- Replacing protobuf-net, Mediator, EF Core, MudBlazor, Mapperly, Serilog, or BCrypt.
- Adding tests (separate project; the new layout enables it but doesn't deliver it).
- Touching `wwwroot/data/` formats, `Configurations/` JSON shapes, or certificate paths.
- Restructuring TaikoWebUI internals (`Pages/`, `Components/`).
- LocalSaveModScoreMigrator behavior — only its `<ProjectReference>` updates.

### Decisions locked in during brainstorming

| Decision | Choice |
|---|---|
| Refactor scope | Server-focused; light WebUI cleanup |
| Architectural style | Hexagonal / ports-and-adapters |
| Multi-version handling | One `<ProjectReference>` per game generation under `Adapters.GameProtocol.*` |
| Core split | Domain + Application + Infrastructure (separate projects) |
| Entity location | Domain |
| Persistence | `GameDatabase/` becomes `TaikoLocalServer.Infrastructure/Persistence/` |
| Outer granularity | 1 Infrastructure + 2 web adapters (AdminApi, AllnetMucha) + per-version protocol adapters + Host |
| Repository pattern | **Dropped.** Wrapper data-access services deleted. Handlers and admin controllers use `ITaikoDbContext` (port in Application, implemented by `TaikoDbContext` in Infrastructure) directly. |
| Real ports | `ITaikoDbContext`, `IGameDataCatalog`, `IJwtTokenService`, `IClock` (4 total) |
| SharedProject fate | Split into `Contracts.AdminApi` (admin API DTOs only) + redistribute the rest |
| Project naming | `TaikoLocalServer.*` prefix on every new project |
| Published exe name | `TaikoLocalServer.exe` preserved via `<AssemblyName>` override |
| PR sequencing | 4 staged PRs by layer |

## 2. Final project list

11 projects total (9 server + WebUI + migrator). Dependency arrows enforced by `<ProjectReference>`.

```
TaikoLocalServer.Domain                          no refs
TaikoLocalServer.Application                     -> Domain
TaikoLocalServer.Infrastructure                  -> Application, Domain
TaikoLocalServer.Contracts.AdminApi              -> Domain (enums used in ViewModels)
TaikoLocalServer.Adapters.AdminApi               -> Application, Infrastructure, Contracts.AdminApi
TaikoLocalServer.Adapters.AllnetMucha            -> Application, Infrastructure, Adapters.GameProtocol.Shared
TaikoLocalServer.Adapters.GameProtocol.Shared    -> Application
TaikoLocalServer.Adapters.GameProtocol.WwR08     -> Application, Adapters.GameProtocol.Shared
TaikoLocalServer.Adapters.GameProtocol.CnR00     -> Application, Adapters.GameProtocol.Shared
TaikoLocalServer.Host                            -> ALL of the above + TaikoWebUI
TaikoWebUI                                       -> Contracts.AdminApi
LocalSaveModScoreMigrator                        -> Domain, Infrastructure
```

Rules:

- Domain has zero refs (pure C# types).
- Application refs only Domain. Pulls `Microsoft.EntityFrameworkCore` (abstractions) for `DbSet<T>` typing, plus `Mediator.Abstractions` + `Mediator.SourceGenerator` (`PrivateAssets="all"`) and `Throw`. Does **not** ref Infrastructure.
- Infrastructure refs Application + Domain. Owns EF + JWT + BCrypt + SharpZipLib + datatable loaders.
- `Contracts.AdminApi` refs Domain only (for the enums used in ViewModels). No packages. Both server and WebUI can ref it without leaking transitive infrastructure deps.
- Game-protocol adapters depend on Application only — they cannot accidentally pull EF.
- Host is the **only** project that knows the full set of versions/adapters loaded. Adding a generation = one `<ProjectReference>` line in Host.

## 3. File-by-file mapping

### TaikoLocalServer.Domain

```
Entities/                 ← from GameDatabase/Entities/ (verbatim; namespace
                            change only)
  AiScoreDatum.cs, AiSectionScoreDatum.cs, Card.cs, Credential.cs,
  DanScoreDatum.cs, DanStageScoreDatum.cs, SongBestDatum.cs,
  SongBestDatumMethods.cs, SongPlayDatum.cs, Token.cs, UserDatum.cs

Enums/                    ← from SharedProject/Enums/
  CrownType.cs, DanBorderType.cs, DanClearState.cs, DanConditionType.cs,
  DanType.cs, Difficulty.cs, NameLanguage.cs, PlayMode.cs, RandomType.cs,
  ScoreRank.cs, SongGenre.cs

DomainConstants.cs        ← split from TaikoLocalServer/Common/Constants.cs
                            (MusicIdMax, MusicIdMaxExpanded, FunctionId* flags,
                             *VerupMasterType constants)
```

No `<PackageReference>`.

### TaikoLocalServer.Application

```
Handlers/                 ← from TaikoLocalServer/Handlers/ (verbatim, namespace
                            updated to TaikoLocalServer.Application.Handlers)

Dtos/                     ← from TaikoLocalServer/Models/Application/Common*.cs

Abstractions/             ← 4 ports (interfaces)
  ITaikoDbContext.cs        new — see Section 4
  IGameDataCatalog.cs       renamed from IGameDataService; same surface
  IJwtTokenService.cs       carved from old AuthService JWT methods
  IClock.cs                 small but useful for the few DateTime.Now calls

Catalog/                  ← read-side value objects the catalog returns
  MusicInfoEntry.cs, MusicOrderEntry.cs, NeiroEntry.cs, ShougouEntry.cs,
  WordListEntry.cs, DonCosRewardEntry.cs, DonCosRewards.cs, MusicInfos.cs,
  MusicOrder.cs, Neiros.cs, Shougous.cs, WordList.cs
                          (← TaikoLocalServer/Models/*.cs, the bare ones not
                           under cn_r00/ww_r08)

ServerData/               ← strongly-typed shapes for wwwroot/data/*.json
  DanData.cs, EventFolderData.cs, MovieData.cs, QRCodeData.cs,
  ShopFolderData.cs, IVerupNo.cs, MusicDetail.cs, SongIntroductionData.cs
                          (← from SharedProject/Models/, server-internal use)

Common/                   ← cross-handler utilities
  Constants.cs              DateTimeFormat (split from old Constants)
  FlagCalculator.cs         from TaikoLocalServer/Common/Utils/
  Extensions.cs             from TaikoLocalServer/Common/Utils/
  ValueHelpers.cs           from SharedProject/Utils/
  OrderedSet.cs             from TaikoLocalServer/Common/

Settings/                  (consumed POCOs — see §6)
  ServerSettings.cs         EnableMoreSongs, MoreSongsSize portion only

DependencyInjection.cs    ← AddApplication() — calls
                            AddMediator(opt => opt.Namespace =
                                "TaikoLocalServer.Application")
```

`<PackageReference>`: `Mediator.Abstractions`, `Mediator.SourceGenerator` (`PrivateAssets="all"`), `Microsoft.EntityFrameworkCore`, `Throw`.

### TaikoLocalServer.Infrastructure

```
Persistence/              ← from GameDatabase/
  TaikoDbContext.cs              now declares ": DbContext, ITaikoDbContext"
  TaikoDbContextPartial.cs
  Configurations/                IEntityTypeConfiguration<T> classes if any
  Migrations/                    verbatim; model snapshot regenerated for new
                                 entity namespace via PR1 empty migration
  PersistenceConstants.cs        DefaultDbName

GameDataCatalog/          ← split from the 566-line GameDataService
  FileGameDataCatalog.cs         singleton; aggregates loaders below; impl of
                                 Application.Abstractions.IGameDataCatalog
  Loaders/                       one file per source datatable (~18 files)
    MusicInfoLoader, MusicOrderLoader, NeiroLoader, ShougouLoader,
    WordlistLoader, DonCosRewardLoader, DanDataLoader, EventFolderDataLoader,
    MovieDataLoader, ShopFolderDataLoader, IntroDataLoader, QrCodeDataLoader,
    TokenDataLoader, GaidenDataLoader, LockedSongsLoader, LockedCostumeLoader,
    LockedTitleLoader, SpecialSongsLoader
  PathHelper.cs                  ← from SharedProject/Utils/
  CatalogConstants.cs            *BaseName entries (MusicInfoBaseName, etc.)
  Settings/DataSettings.cs       ← from TaikoLocalServer/Settings/

Identity/                 ← split from old AuthService + JWT plumbing
  JwtTokenService.cs             impl of IJwtTokenService; ExtractTokenInfo
                                 carved from AuthService; IssueToken carved
                                 from inline JwtSecurityTokenHandler usage in
                                 AuthController login flow
  Settings/AuthSettings.cs       ← from TaikoLocalServer/Settings/

Time/
  SystemClock.cs                 impl of IClock

Settings/
  AllnetSettings.cs              MuchaUrl + GameUrl portion of old ServerSettings

DependencyInjection.cs    ← AddInfrastructure(IConfiguration) registers
                            DbContext, IGameDataCatalog, IJwtTokenService,
                            IClock, options bindings
```

`<PackageReference>`: EF Core + EF Core SQLite + EF Core Tools, EFCore.Exceptions.Sqlite, JwtBearer, BCrypt.Net-Next, SharpZipLib, Yoh.Text.Json.NamingPolicies, Throw.

**Files explicitly deleted, not moved:** `TaikoLocalServer/Services/UserDatumService.cs`, `SongBestDatumService.cs`, `SongPlayDatumService.cs`, `DanScoreDatumService.cs`, `SongLeaderboardService.cs`, all `Services/Interfaces/I*Datum*.cs`, `Services/Interfaces/ISongLeaderboardService.cs`, `Services/Extentions/ServiceExtensions.cs`, the data-access methods of `Services/AuthService.cs` (only the JWT methods survive, in `Identity/JwtTokenService.cs`).

### TaikoLocalServer.Contracts.AdminApi

```
Requests/                 ← from SharedProject/Models/Requests/ (verbatim)
Responses/                ← from SharedProject/Models/Responses/ (verbatim)

ViewModels/               ← from SharedProject/Models/, admin-API contract
  User.cs, UserCredential.cs, UserSetting.cs, SongBestData.cs,
  SongHistoryData.cs, SongLeaderboard.cs, SongPlayDatumDto.cs,
  DanBestData.cs, DanBestStageData.cs, AiSectionBestData.cs,
  Title.cs, Costume.cs, PlaySetting.cs

Converters/
  PlaySettingConverter.cs       from SharedProject/Utils/
```

Project ref: Domain (for enums used in ViewModels). No packages.

### TaikoLocalServer.Adapters.AdminApi

```
Controllers/              ← from TaikoLocalServer/Controllers/Api/, rewritten
                            in PR2 to inject ITaikoDbContext + IJwtTokenService
                            directly (no wrapper services), then physically
                            relocated in PR3
  AuthController.cs (271 LOC), CardsController.cs, DanBestDataController.cs,
  FavoriteSongsController.cs, GameDataController.cs, PlayDataController.cs,
  PlayHistoryController.cs, SongLeaderboardController.cs,
  UserSettingsController.cs (233 LOC), UsersController.cs

BaseAdminController.cs    ← thin variant of current BaseController<T>;
                            keeps the lazy IMediator/ILogger pattern

Filters/
  AuthorizeIfRequiredAttribute.cs  ← from TaikoLocalServer/Filters/

DependencyInjection.cs    ← AddAdminApi(IConfiguration) — registers
                            AuthorizeIfRequiredAttribute, JWT auth scheme,
                            authorization policy
```

`<PackageReference>`: `Microsoft.AspNetCore.Authentication.JwtBearer` (the `AddAuthentication().AddJwtBearer(...)` call moves out of `Program.cs` into `AddAdminApi(IConfiguration)` since JWT is admin-API-specific). Project refs: Application, Infrastructure, Contracts.AdminApi.

> The 271-line `AuthController` and 233-line `UserSettingsController` are kept whole in this refactor. If review surfaces clear sub-splits (e.g., Auth login flow vs OTP flow), those land in a follow-up. Out of scope for the structural refactor.

### TaikoLocalServer.Adapters.AllnetMucha

```
Controllers/
  AmAuth/                        ← from TaikoLocalServer/Controllers/AmAuth/
  AmUpdater/                     ← from TaikoLocalServer/Controllers/AmUpdater/
  Garmc/                         ← from TaikoLocalServer/Controllers/Garmc/
  MuchaActivation/               ← from TaikoLocalServer/Controllers/MuchaActivation/

Middleware/
  AllNetRequestMiddleware.cs     ← from TaikoLocalServer/Middlewares/

Wire/
  MuchaBoardAuthRequest.cs       ← from TaikoLocalServer/Models/
  MuchaUpdateCheckRequest.cs     ← from TaikoLocalServer/Models/
  PowerOnRequest.cs              ← from TaikoLocalServer/Models/

Common/
  FormOutputUtil.cs              ← from TaikoLocalServer/Common/Utils/

DependencyInjection.cs    ← AddAllnetMucha() and a UseAllnetMucha() helper
                            that wraps the current conditional UseWhen wiring
                            for /sys/servlet/PowerOn
```

Project refs: Application, Infrastructure (for `AllnetSettings`), `Adapters.GameProtocol.Shared` (for gzip util).

### TaikoLocalServer.Adapters.GameProtocol.Shared

```
Controllers/
  BaseProtocolController.cs      thin variant of current BaseController<T>
                                 specialized for protobuf endpoints (lazy
                                 IMediator + ILogger via service locator)

Compression/
  GZipBytesUtil.cs               ← from TaikoLocalServer/Common/Utils/
  HeaderStripUtil.cs             extracts the inline Skip(32).ToArray()
                                 pattern; not all endpoints use it

Marker.cs                        empty class for assembly identity
```

`<PackageReference>`: `protobuf-net`, `protobuf-net.AspNetCore`. Project ref: Application.

### TaikoLocalServer.Adapters.GameProtocol.WwR08

```
Wire/                     ← from TaikoLocalServer/Models/ww_r08/ (verbatim)

Mappers/                  ← from TaikoLocalServer/Mappers/, only the _ww
                            mapper methods. Files mixing both versions split.

Controllers/              ← from TaikoLocalServer/Controllers/Game/, only
                            the [HttpPost("/v12r08_ww/...")] methods. Each
                            mixed file splits across WwR08 and CnR00.

DependencyInjection.cs    ← AddGameProtocolWwR08() — currently no-op other
                            than the project being referenced; kept as a
                            future hook
```

`<PackageReference>`: `Riok.Mapperly`. Project refs: Application, Adapters.GameProtocol.Shared.

### TaikoLocalServer.Adapters.GameProtocol.CnR00

Same shape as WwR08. Wire types from `Models/cn_r00/` and `Models/CN00/`. Mappers for the `_cn` direction. Controllers for `/v12r00_cn/...` routes.

### TaikoLocalServer.Host

```
Program.cs                ← slimmed; composition root using AddApplication(),
                            AddInfrastructure(...), AddAdminApi(),
                            AddAllnetMucha(), AddGameProtocolWwR08(),
                            AddGameProtocolCnR00(); still owns Kestrel
                            binding, response compression, CORS, Blazor
                            hosting, the migration step, Serilog config,
                            the 404/non-200 logger pipeline
GlobalUsings.cs           ← slim per-project version
Properties/launchSettings.json
app.manifest

Configurations/           ← from TaikoLocalServer/Configurations/ (verbatim;
                            still copied to publish output via <None Include
                            ... CopyToOutputDirectory="PreserveNewest"/>)
  AuthSettings.json, Database.json, DataSettings.json, Kestrel.json,
  Logging.json, ServerSettings.json

Certificates/             ← from TaikoLocalServer/Certificates/
  cert.pfx, root.pfx

Logging/CsvFormatter.cs   ← from TaikoLocalServer/Logging/

wwwroot/                  ← unchanged location relative to publish output
  data/, taiko.db3 (operator-supplied, not bundled)
```

`<AssemblyName>TaikoLocalServer</AssemblyName>` preserves the published exe name. `PublishSingleFile`/`SelfContained` settings move here from the current `TaikoLocalServer.csproj`.

`<PackageReference>`: `Microsoft.AspNetCore.Components.WebAssembly.Server`, `Microsoft.AspNetCore.ResponseCompression`, `Serilog.AspNetCore`, `Serilog.Expressions`, `Serilog.Sinks.File.Header`, `DotNetZip`, `Otp.NET`, `Throw`, `Swashbuckle.AspNetCore`. Project refs: every server-side project + TaikoWebUI.

### TaikoWebUI (cleanup-only)

- Delete `TaikoWebUI.sln` (legacy; root `.slnx` is canonical).
- Swap `<ProjectReference>`: drop `SharedProject`, add `TaikoLocalServer.Contracts.AdminApi`.
- Update `GlobalUsings.cs`: re-exports point at `TaikoLocalServer.Contracts.AdminApi.*`. Enums come from `TaikoLocalServer.Domain.Enums` (re-exported by Contracts if convenient).
- `Services/GameDataService.cs` (WebUI's): no structural change; still loads datatable JSONs over HTTP from server's `wwwroot`. A future refactor can replace with admin API endpoint calls — out of scope.

### LocalSaveModScoreMigrator

- `<ProjectReference>`: drop `GameDatabase` + `SharedProject`, add `TaikoLocalServer.Domain` + `TaikoLocalServer.Infrastructure`. Source-file `using` updates only.

## 4. Port surface (4 interfaces) and admin-controller flow

### The four ports

```csharp
// Application/Abstractions/ITaikoDbContext.cs
public interface ITaikoDbContext
{
    DbSet<UserDatum> UserData { get; }
    DbSet<Card> Cards { get; }
    DbSet<Credential> Credentials { get; }
    DbSet<Token> Tokens { get; }
    DbSet<SongBestDatum> SongBestData { get; }
    DbSet<SongPlayDatum> SongPlayData { get; }
    DbSet<DanScoreDatum> DanScoreData { get; }
    DbSet<DanStageScoreDatum> DanStageScoreData { get; }
    DbSet<AiScoreDatum> AiScoreData { get; }
    DbSet<AiSectionScoreDatum> AiSectionScoreData { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

`TaikoDbContext` in Infrastructure adds `: DbContext, ITaikoDbContext`. DI registers both: the concrete class for EF tooling and migrations, plus a scoped `services.AddScoped<ITaikoDbContext>(sp => sp.GetRequiredService<TaikoDbContext>())` so Application code never sees the concrete class.

```csharp
// Application/Abstractions/IGameDataCatalog.cs
public interface IGameDataCatalog
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    IReadOnlyDictionary<uint, MusicInfoEntry> MusicInfoes { get; }
    IReadOnlyDictionary<uint, MusicOrderEntry> MusicOrders { get; }
    IReadOnlyList<uint> EnabledSongIds { get; }
    bool IsSongLocked(uint songId);

    IReadOnlyDictionary<uint, DanData> DanMap { get; }
    IReadOnlyDictionary<uint, DanData> GaidenMap { get; }

    IReadOnlyList<EventFolderData> EventFolderData { get; }
    IReadOnlyList<ShopFolderData> ShopFolderData { get; }
    IReadOnlyList<MovieData> MovieData { get; }
    IReadOnlyList<SongIntroductionData> IntroData { get; }
    IReadOnlyList<QRCodeData> QrCodeData { get; }
    IReadOnlyList<TokenData> TokenData { get; }
    IReadOnlySet<uint> LockedSongs { get; }
    IReadOnlySet<uint> LockedCostumes { get; }
    IReadOnlySet<uint> LockedTitles { get; }
    IReadOnlyList<uint> SpecialSongs { get; }
    IReadOnlyList<GaidenData> GaidenData { get; }

    IReadOnlyDictionary<uint, NeiroEntry> Neiros { get; }
    IReadOnlyDictionary<uint, ShougouEntry> Shougous { get; }
    IReadOnlyDictionary<uint, WordListEntry> WordList { get; }
    IReadOnlyDictionary<uint, DonCosRewardEntry> DonCosRewards { get; }
}
```

The exact list mirrors what the current 566-line `GameDataService` already exposes — same surface, split implementation.

```csharp
// Application/Abstractions/IJwtTokenService.cs
public interface IJwtTokenService
{
    string IssueToken(uint baid, bool isAdmin);
    JwtTokenInfo? ExtractTokenInfo(HttpContext httpContext);
}

public readonly record struct JwtTokenInfo(uint Baid, bool IsAdmin);
```

```csharp
// Application/Abstractions/IClock.cs
public interface IClock
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}
```

### Admin-controller flow

| Operation kind | Goes through | Why |
|---|---|---|
| Simple read or simple CRUD | Controller injects `ITaikoDbContext` directly | Mediator round-trip is pure ceremony for these. |
| Combines multiple entities, has business rules, or is also called from game-protocol controllers | Mediator command/query in `Application/Handlers/` | Single source of truth when reachable from both audiences. |
| Transaction across multiple aggregates | Mediator command (handler owns `SaveChangesAsync`) | Keeps transaction boundary visible. |

Concretely:

- `UsersController.GetUser/GetUsers/DeleteUser`, `UserSettingsController`, `CardsController`, `FavoriteSongsController`, `DanBestDataController`, `PlayHistoryController`, `PlayDataController`, `SongLeaderboardController` — direct `ITaikoDbContext`.
- `UsersController.GetUser` claim check — injects `IJwtTokenService`.
- `AuthController` — direct `ITaikoDbContext` + `IJwtTokenService` for login/register/change-password/OTP flows.
- `GameDataController` — injects `IGameDataCatalog`.
- Game-protocol controllers — always Mediator.

This drops ~700 LOC of wrapper code (5 service files + 5 interfaces + DI registrations).

## 5. Game-version pluggability mechanism

### The contract per adapter project

Every `TaikoLocalServer.Adapters.GameProtocol.<Generation>` project owns three things and only these three things:

```
Wire/                     protobuf-decorated request/response types for THIS
                          generation's wire format
Mappers/                  Mapperly partial classes: Wire <-> Application.Dtos
Controllers/              ASP.NET controllers using THIS generation's route
                          convention. Each method:
                            1. accepts a Wire request
                            2. calls a mapper -> Common* DTO
                            3. dispatches to Mediator
                            4. maps Common* response -> Wire response
                            5. returns Ok(wire)
DependencyInjection.cs    AddGameProtocol<Gen>() — currently a no-op
```

What an adapter never contains: business logic, DbContext access, cross-version helpers, datatable lookups. csproj has exactly two project refs: Application + GameProtocol.Shared. The build rejects any other.

### Discovery (zero registration burden)

| Component | Mechanism | Required action |
|---|---|---|
| Controllers | `app.MapControllers()` scans every referenced assembly for `[ApiController]` types | Add `<ProjectReference>` to Host. Done. |
| Mapperly mappers | Compile-time source-generated static classes | Add `using` for the mapper namespace. Done. |
| Mediator handlers | Source generator scans namespaces under `opt.Namespace = "TaikoLocalServer.Application"` | Adapter dispatches existing handlers. No per-version action. |
| Configurations / wwwroot files | Nothing version-specific lives there | None. |

### Common DTO evolution rules

1. **Additive only.** New optional fields are fine; removing or renaming is breaking.
2. **No version discriminators leak in.** Wire-shape differences resolved in mappers, not handlers.
3. **Optional/nullable when divergent.** Older mapper leaves the field `null`; handler tolerates `null`.
4. **No `enum Version { ... }` in Common DTOs.** If a handler genuinely needs different behavior per generation, the divergence is large enough that it's a separate handler — and the mapper picks which Mediator command to dispatch.
5. **Two adapters dispatching the same Mediator command must produce semantically equivalent state changes.**

### Worked example: adding `Adapters.GameProtocol.JpR05`

Hypothetical discontinued JP generation, very different protobuf, routes at `/jp_v9_5/...`. Work:

1. New csproj with `Wire/`, `Mappers/`, `Controllers/`. References Application + GameProtocol.Shared.
2. `Host.csproj` gains one `<ProjectReference>` line.
3. JpR05 wire field not in Common DTO: add optional Common field; old WwR08/CnR00 mappers leave it `null`; handler uses if non-null.
4. JpR05 lacks an endpoint: omit the controller. Fewer routes is fine.
5. JpR05 endpoint shape Common can't represent: introduce new Mediator command; only JpR05 dispatches it.
6. **No edits to:** Domain, Infrastructure, AdminApi, AllnetMucha, GameProtocol.Shared, GameProtocol.WwR08, GameProtocol.CnR00, TaikoWebUI, LocalSaveModScoreMigrator, Program.cs.

Adding a generation is local, additive, short.

## 6. Cross-cutting concerns

### Mediator namespace and discovery

`AddMediator(opt => { opt.Namespace = "TaikoLocalServer.Application"; opt.ServiceLifetime = ServiceLifetime.Scoped; })` called inside `Application.DependencyInjection.AddApplication()`. Source generator runs against handlers in Application; emitted DI registrations are picked up when Host calls `AddApplication()`.

- `Application.csproj` carries `Mediator.SourceGenerator` (`PrivateAssets="all"`).
- `Host.csproj` carries it too — needed for the registration glue if Mediator's generator emits into the assembly that calls `AddMediator`.
- Adapter projects carry only `Mediator.Abstractions`.

If cross-assembly registration turns out not to work after a 30-min PR2 prototype, fallback is to colocate `AddMediator` in Host with explicit assembly markers. Risk #2.

### EF Core tooling commands (CLAUDE.md update)

```bash
# Was
dotnet ef migrations add <Name> --project GameDatabase --startup-project TaikoLocalServer

# After
dotnet ef migrations add <Name> \
  --project TaikoLocalServer.Infrastructure \
  --startup-project TaikoLocalServer.Host
```

```bash
dotnet run --project TaikoLocalServer.Host
```

Publish output: `TaikoLocalServer.Host/bin/Release/net10.0/win-x64/publish/`. The exe is still `TaikoLocalServer.exe` thanks to `<AssemblyName>TaikoLocalServer</AssemblyName>` in Host.csproj.

### Settings POCO distribution

| Class | New home | Why |
|---|---|---|
| `AuthSettings` | `Infrastructure/Identity/Settings/` | Bound by JWT setup, consumed by `JwtTokenService` and `AuthorizeIfRequiredAttribute`. |
| `DataSettings` | `Infrastructure/GameDataCatalog/Settings/` | Filenames for the loaders. |
| `ServerSettings` (split) | `EnableMoreSongs` + `MoreSongsSize` → `Application/Settings/`; `MuchaUrl` + `GameUrl` → `Infrastructure/Settings/AllnetSettings.cs` | Used by handlers vs by AllnetMucha. JSON file shape unchanged; bound twice. |

### Composition root (Program.cs after slimming)

~80 lines. Configuration loading, Serilog, HttpLogging, ResponseCompression, CORS, MemoryCache: unchanged. The registration-phase shrinks to:

```csharp
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAdminApi(builder.Configuration);
builder.Services.AddAllnetMucha();
builder.Services.AddGameProtocolWwR08();
builder.Services.AddGameProtocolCnR00();
builder.Services.AddControllers().AddProtoBufNet();
```

Middleware pipeline byte-for-byte identical. Migrate-on-startup and `IGameDataCatalog.InitializeAsync()` calls preserved.

### `Constants.cs` split

| Constant | Destination |
|---|---|
| `DateTimeFormat` | `Application/Common/Constants.cs` |
| `MusicIdMax`, `MusicIdMaxExpanded`, `*VerupMasterType`, `FunctionId*` | `Domain/DomainConstants.cs` |
| `DefaultDbName` | `Infrastructure/Persistence/PersistenceConstants.cs` |
| `MusicInfoBaseName`, `WordlistBaseName`, `MusicOrderBaseName`, `DonCosRewardBaseName`, `ShougouBaseName`, `NeiroBaseName` | `Infrastructure/GameDataCatalog/CatalogConstants.cs` |

### Filesystem-relative paths

`PathHelper.GetRootPath()` returns `wwwroot` next to the running exe via `AppContext.BaseDirectory`. Running exe is still Host. Host owns `wwwroot/`, `Certificates/`, `Configurations/` via `<Content>` entries with `CopyToOutputDirectory="PreserveNewest"`. Operators see no difference. PathHelper itself moves to `Infrastructure/GameDataCatalog/`.

### Serilog CSV side-channel

The filter `StartsWith(@m, 'CSV WRITE:')` matches on message content, not source type. When `HeadClerk2Controller` moves into `Adapters.GameProtocol.WwR08` (and a copy in CnR00 if applicable), the filter still picks up its log lines. No change needed.

### GlobalUsings split

Each project gets its own `GlobalUsings.cs` scoped to its concerns. The current monolithic `TaikoLocalServer/GlobalUsings.cs` is broken into:

- `Application/GlobalUsings.cs`: Domain.Entities, Domain.Enums, EF Core, Mediator, Application's own sub-namespaces.
- `Infrastructure/GlobalUsings.cs`: Domain, Application.Abstractions, EF Core.
- Adapter projects: Application, MVC types as needed, version-specific Wire namespace.
- `Host/GlobalUsings.cs`: Serilog, every server-side project root namespace.

The current `Models.WW08` global using (a stale alias — folder is `ww_r08`) is dropped, not replaced.

### CLAUDE.md updates (PR4)

Sections rewritten:

- Solution layout: 5 → 11 projects.
- Common commands: new `--project`/`--startup-project` paths.
- Configuration model: paths under `TaikoLocalServer.Host/Configurations/`.
- Where data files live: paths under `TaikoLocalServer.Host/wwwroot/`.
- Request architecture (game side): reframed around per-version adapter projects, the Common DTO contract, and the version-plug procedure.
- Conventions to follow: expanded with adapter project rules.
- Mediator handler conventions: namespace updated.
- New "Adapter contract" subsection: the additive-only, no-version-discriminator rules from §5.

## 7. PR sequencing — four staged PRs

| # | Branch | Base | Smoke test |
|---|---|---|---|
| 1 | `dev/clean-arch-domain` | `dev` | Build green; server starts; DB migrates; one game endpoint hand-tested; admin UI loads |
| 2 | `dev/clean-arch-app-infra` | merged PR1 | + Mediator handlers resolve at runtime; AdminApi rewritten controllers serve auth-on AND auth-off paths; LocalSaveModScoreMigrator runs |
| 3 | `dev/clean-arch-adapters` | merged PR2 | + route count matches PR2 baseline; both `_ww` and `_cn` endpoints work; Allnet/Mucha controllers wired; HeadClerk CSV log writes; published `TaikoLocalServer.exe` works |
| 4 | `dev/clean-arch-contracts-webui` | merged PR3 | + WebUI page-by-page smoke test; round-trip Login + SetFavorite; CLAUDE.md paths resolve |

### PR1 — Domain carve-out

**Moves:** entities + enums + domain constants (see §3).

**csproj changes:** new Domain project; GameDatabase, SharedProject, TaikoLocalServer, TaikoWebUI, LocalSaveModScoreMigrator gain `<ProjectReference>` to Domain.

**Migration:** `dotnet ef migrations add EntityNamespaceMove` against `--project GameDatabase --startup-project TaikoLocalServer`. The migration's `Up`/`Down` should be empty (no schema change); it exists only to record the regenerated model snapshot.

**Verification gates:**

1. `dotnet build` green across solution.
2. `dotnet ef migrations script EntityNamespaceMove` produces empty SQL (or schema-only no-op).
3. Server starts. Existing `taiko.db3` migrates without error. New empty migration applies cleanly.
4. Hand-test one game endpoint and one admin endpoint (`GET /api/users` with auth off).
5. Blazor admin UI loads without console errors.

### PR2 — Application + Infrastructure carve-out

**Moves:** see §3. Handlers + Common DTOs + 4 ports → Application. GameDatabase folder structure refolded into Infrastructure.

**Deletes (do not move):** the 5 wrapper data-access services + their interfaces + `ServiceExtensions.cs` + the data-access half of `AuthService`.

**Rewrites in place (still under `TaikoLocalServer/Controllers/Api/`):** all admin controllers, to inject `ITaikoDbContext` + `IJwtTokenService` directly. Physical relocation deferred to PR3.

**csproj changes:** new Application project; `GameDatabase` renamed to `TaikoLocalServer.Infrastructure`; TaikoLocalServer adds project refs to Application + Infrastructure; LocalSaveModScoreMigrator updated.

**Mediator namespace move:** `AddMediator(opt => opt.Namespace = "TaikoLocalServer.Application")` inside `Application.DependencyInjection.AddApplication()`. Both Application and Host carry `Mediator.SourceGenerator`.

**Verification gates:**

1. Build green. `obj/Generated/Mediator.SourceGenerator/` in Application contains handler registrations.
2. Server starts, migrates DB, initializes catalog, binds all multi-port Kestrel endpoints.
3. Game endpoint smoke test for both `_ww` and `_cn` route variants.
4. Admin API smoke test with `AuthenticationRequired: false`: `GET /api/users`, `GET /api/users/{baid}`, `DELETE /api/users/{baid}`.
5. Admin API smoke test with `AuthenticationRequired: true`: `POST /api/auth/login` returns JWT; JWT works on `GET /api/users/{baid}` and is rejected for a different user's baid.
6. WebUI loads, Dashboard renders, login flow round-trips against the rewritten `AuthController`.
7. `LocalSaveModScoreMigrator` builds and at minimum starts up + parses args.

### PR3 — Adapters extraction

**Moves:** physical relocation only. Each file moved was already restructured in PR1 or PR2. AdminApi → Adapters.AdminApi; AmAuth+AmUpdater+Garmc+MuchaActivation → Adapters.AllnetMucha; BaseController split + GZipBytesUtil → Adapters.GameProtocol.Shared; Game/ controllers + ww_r08/CN00 wire types + Mappers/ split per version → WwR08 + CnR00; Configurations + Certificates + Logging + wwwroot → Host.

**csproj changes:** 5 new adapter csprojs; `TaikoLocalServer.csproj` renamed to `TaikoLocalServer.Host.csproj` with `<AssemblyName>TaikoLocalServer</AssemblyName>`. Program.cs slimmed to extension-method composition. `.slnx` updated.

**Verification gates:**

1. Build green. Solution count is 11 projects.
2. Server starts. Route count via `app.Services.GetRequiredService<EndpointDataSource>().Endpoints.Count` matches PR2 baseline.
3. One endpoint per game adapter smoke-tested.
4. AllnetMucha smoke test if a fixture exists; otherwise startup-log confirmation.
5. AdminApi smoke tests from PR2 re-run.
6. HeadClerk2 endpoint hit; `HeadClerkLog-*.csv` writes into `Logs/`.
7. Blazor UI loads from `wwwroot/`. `app.UseBlazorFrameworkFiles()` works.
8. `dotnet publish -c Release` produces `TaikoLocalServer.Host/bin/Release/net10.0/win-x64/publish/TaikoLocalServer.exe` with `wwwroot/`, `Configurations/`, `Certificates/` populated. Run the published exe and repeat smoke tests 3 + 5 + 7.

### PR4 — Contracts + WebUI cleanup + CLAUDE.md rewrite

**Moves:** SharedProject content split: Requests/Responses/admin-API ViewModels + PlaySettingConverter → `TaikoLocalServer.Contracts.AdminApi`; remaining content was already moved in PR2. SharedProject project deleted.

**Changes:** new Contracts.AdminApi csproj; AdminApi adapter and TaikoWebUI swap project refs from SharedProject to Contracts.AdminApi; using statements + GlobalUsings updated. Delete `TaikoWebUI/TaikoWebUI.sln`. Rewrite `CLAUDE.md` (see §6). Skim top-level `README.md` for path references.

**Verification gates:**

1. Build green; SharedProject no longer in solution.
2. Server starts and admin API serves.
3. WebUI page-by-page check: Dashboard, Login, Register, Users, Profile, AccessCode, ChangePassword, HighScores, PlayHistory, SongList, Song, DaniDojo. Each loads without console errors.
4. Round-trip a Login: WebUI → AdminApi → JWT → WebUI navigates to Dashboard with auth context.
5. Round-trip a SetFavorite: WebUI button → AdminApi → DB update → WebUI reflects state.
6. `TaikoWebUI.sln` is gone from the repo.
7. CLAUDE.md "Common commands" `--project` paths all resolve to real csprojs.

## 8. Risk register

| # | Risk | PR | Mitigation |
|---|---|---|---|
| 1 | EF Core model snapshot regen produces non-empty migration when entities move namespaces | PR1 | Add the empty `EntityNamespaceMove` migration intentionally. Inspect `Up/Down` are empty; if not, back out and hand-edit the snapshot instead. |
| 2 | Mediator source generator finds zero handlers after namespace change | PR2 | First build of PR2: confirm `obj/Generated/Mediator.SourceGenerator/` contains registration code listing every handler. If empty, fall back to colocating `AddMediator` call in Host with explicit assembly markers. 30-min prototype before committing to the PR2 plan. |
| 3 | AdminApi controller rewrite changes auth behavior | PR2 | Manually exercise both auth-on and auth-off paths for the 271-line `AuthController`. Compare wire-level responses (status codes, JWT structure) against a captured baseline from `dev`. |
| 4 | Game-side controller assemblies not discovered by `app.MapControllers()` | PR3 | Log route count at startup; compare to PR2 baseline. Smoke-test one route per adapter project. |
| 5 | `dotnet publish` layout regression — missing `wwwroot/`, `Configurations/`, certs | PR3 | First successful Release build runs `dotnet publish` and inspects output before declaring PR ready. |
| 6 | Stale `TaikoLocalServer.sln` confuses tooling/CI | PR3 | Repo already uses `.slnx`. Verify CI targets `dotnet build` at the repo root or `.slnx` explicitly. |
| 7 | Operator's existing shortcuts/scripts pointing at `TaikoLocalServer.exe` break after rename | n/a | `<AssemblyName>TaikoLocalServer</AssemblyName>` in Host.csproj keeps the binary name unchanged. Verified in PR3 gate 8. |
| 8 | Common DTO namespace collisions (`PlayResultDataRequest` in WwR08 wire vs `CommonPlayResultData` in Application) | PR3 | Distinct namespaces resolve this. Mapperly will surface using-statement ambiguity at compile time. |
| 9 | Operator-supplied `wwwroot/data/datatable/*.bin` files accidentally not copied to publish output after wwwroot relocation | PR3 | Existing `<Content Update="...">` entries only apply if the file exists; operators add post-publish today; behavior preserved. Verify publish output contains the empty parent directory structure. |
| 10 | `WebUI/TaikoWebUI.sln` interferes with project discovery before its deletion | PR4 | Deletion is in PR4 — no operational risk before then; just stale. |
