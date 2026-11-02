# Phase 23: White Evidence and Era Foundation - Pattern Map

**Mapped:** 2026-06-17  
**Files analyzed:** 22  
**Analogs found:** 21 / 22

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|---|---|---|---|---|
| `.planning/phases/23-white-evidence-and-era-foundation/23-WHITE-EVIDENCE.md` | docs/evidence | evidence matrix | `.planning/milestones/v1.2-phases/12-yellow-evidence-and-era-foundation/12-YELLOW-EVIDENCE.md` | role-match |
| `.planning/phases/23-white-evidence-and-era-foundation/23-WHITE-FEATURE-INVENTORY.md` | docs/evidence | evidence matrix | `.planning/research/FEATURES.md` + `12-YELLOW-EVIDENCE.md` | role-match |
| `.planning/STATE.md` | planning state | batch/doc-update | existing milestone notes in `.planning/STATE.md` | exact self-edit |
| `.planning/ROADMAP.md` | planning roadmap | batch/doc-update | existing Phase 23 section in `.planning/ROADMAP.md` | exact self-edit |
| `TaikoLocalServer.slnx` | solution config | build graph | existing adapter project entries | exact |
| `Domain/Enums/GameEra.cs` | model enum | config parse | existing `GameEra` values | exact |
| `Host/Program.cs` | host composition | DI + request-response fallback | existing era registration, application-part removal, protobuf fallback | exact |
| `Host/Host.csproj` | host project config | build/content graph + file-I/O | existing adapter refs, operator data exclusions, debug junctions | exact |
| `Host/Configurations/ServerSettings.json` | config | config binding | existing era blocks | exact |
| `Application/Settings/ServerSettingsOptionsValidationExtensions.cs` | config validation utility | validation | existing AC15 shop and Red ChallengeCompe validators | exact boundary |
| `Adapters.GameProtocol.White/Adapters.GameProtocol.White.csproj` | project config | build graph | `Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj` | exact |
| `Adapters.GameProtocol.White/GlobalUsings.cs` | adapter config | compile/import | `Adapters.GameProtocol.Red/GlobalUsings.cs` | exact |
| `Adapters.GameProtocol.White/DependencyInjection.cs` | DI extension | service registration | `Adapters.GameProtocol.Red/DependencyInjection.cs` | exact |
| `Adapters.GameProtocol.White/WhiteAdapterMarker.cs` | marker | reflection/application-part | `Adapters.GameProtocol.Red/RedAdapterMarker.cs` | exact |
| `Adapters.GameProtocol.White/MapperlyDefaults.cs` | mapper config | source generation | `Adapters.GameProtocol.Red/MapperlyDefaults.cs` | exact |
| `Adapters.GameProtocol.White/Wire/Game.cs` | generated wire model | protobuf transform | `Adapters.GameProtocol.Red/Wire/Game.cs` | exact |
| `Adapters.GameProtocol.White/Wire/VsInterface.cs` | generated wire model | protobuf transform | `Adapters.GameProtocol.Red/Wire/VsInterface.cs` | exact |
| `Adapters.GameProtocol.White/Controllers/*Controller.cs` | controller | request-response | Red no-state controllers + shared `BaseProtocolController` | exact for no-state scaffolds |
| `Adapters.GameProtocol.White/Mappers/*Mappers.cs` | mapper | transform | `Adapters.GameProtocol.Red/Mappers/*Mappers.cs` | role-match; avoid unless needed |
| `Tests/Tests.csproj` | test project config | build graph | existing adapter project references | exact |
| `Tests/White/WhiteServerSettingsValidationTests.cs` | test | validation | `Tests/Red/RedServerSettingsValidationTests.cs` | exact |
| `Tests/White/WhiteHostRouteGatingTests.cs` | test | request-response / route discovery | none | no analog |

## Pattern Assignments

### White Evidence Artifacts

**Files:** `23-WHITE-EVIDENCE.md`, `23-WHITE-FEATURE-INVENTORY.md`  
**Role/data flow:** docs/evidence, evidence matrix  
**Analogs:** `12-YELLOW-EVIDENCE.md`; `.planning/research/FEATURES.md`

**Evidence artifact table shape** (`12-YELLOW-EVIDENCE.md` lines 6-12):
```markdown
## Route Prefix Decision

| Question | Status | Evidence Source | Decision |
|----------|--------|-----------------|----------|
| Exact Yellow game route prefix | RESOLVED | Explicit user approval on 2026-06-07 | Use `/v09r00` for Yellow game routes in Phase 12 scaffold work. |
```

**Proto-backed suffix inventory shape** (`12-YELLOW-EVIDENCE.md` lines 14-43):
```markdown
## Proto-Backed Endpoint Suffixes

| Endpoint suffix | Proto request | Proto response | Phase 12 treatment |
|-----------------|---------------|----------------|--------------------|
| `bookkeeping.php` | `BookKeepingRequest` | `BookKeepingResponse` | Supported suffix; no-state scaffold candidate. |
| `heartbeat.php` | `HeartBeatRequest` | `HeartBeatResponse` | Supported suffix; no-state scaffold candidate. |
| `getreitai.php` | `GetreitaiRequest` | `GetreitaiResponse` | Proto messages exist, but no Yellow route-call evidence is recorded; do not scaffold in Phase 12. |
```

**Shared startup and transport gap shape** (`12-YELLOW-EVIDENCE.md` lines 45-61):
```markdown
| `/v01r00/chassis/startupauth.php` | `Adapters.GameProtocol.Shared` | Yellow `vsinterface.proto` matches the shared startup request/response shape. | Keep shared ownership. |
| Game route request body | SUPPORTED SCAFFOLD | Local `proto/yellow/yellow.proto` exposes direct request messages such as `PlayResultRequest` with `Baid` and no wrapper `PlayresultData`. | Use direct protobuf DTOs for scaffold controllers. |
| `getreitai.php` route use | UNVERIFIED ROUTE GAP | Proto messages exist, but no local log, binary string citation, or user approval says Yellow calls this route. | Do not scaffold `getreitai.php` in Phase 12. |
```

**Deferred scope shape** (`12-YELLOW-EVIDENCE.md` lines 74-76):
```markdown
## Deferred Runtime Scope

Phase 12 does not implement Yellow persistence, catalog loading, normal play state, Tokkun state, shop or medal state, Dani state, Banacoin wallet/payment behavior, AdminApi routes, or WebUI routes. Those remain assigned to later Yellow phases.
```

**White-specific evidence to record** (`23-CONTEXT.md` lines 19-34, 107-111):
```markdown
- White game routes are expected under `/v07r00/chassis/*`, but Phase 23 must verify `/v07r00` from White IDB route strings before route attributes are finalized.
- Phase 23 controllers should be thin no-state scaffolds: deserialize White wire DTOs, log bounded request information, and return safe success/default responses where appropriate.
- Current filesystem evidence shows `.tools/white/EBOOT.ELF.i64` is present and nonzero.
- Current White IDB file-size evidence observed during discussion: `.tools/white/EBOOT.ELF.i64` length `129893515` bytes.
```

**Feature inventory inputs** (`.planning/research/FEATURES.md` lines 101-118, 146-157):
```markdown
- `proto/white/vsinterface.proto` contains startup/version messages: `StartupAuth*`, `VerupAuth*`, and `VerupComplete*`.
- Metadata/support messages include `Initialdatacheck*`, `Getfolder*`, `Gettelop*`, `Taikojuku*`, `Recommend*`, `Tournamentcheck*`, `BookKeeping*`, `HeartBeat*`, `HeadClerk2*`, `Getreitai*`, `Rewardcardcheck*`, and `Rewardexecution*`.
- The White proto does not define standalone `ChallengeCompeRequest/Response`, `Getitemshopinfo`, `Itempurchase`, `Getbanacoininfo`, `Balancecheck`, `Banacoinpayment`, `Banacoinerrorlog`, Blue battle messages, Tokkun fields, WaiWai fields, or gacha response payloads.
```

**Planner instruction:** copy the Yellow evidence structure, but do not copy Yellow route decisions. White route attributes and Host fallback are blocked until `/v07r00/chassis` and each scaffold suffix are proven from White route evidence and cross-checked against `proto/white`.

### Stale Planning Notes

**Files:** `.planning/STATE.md`, `.planning/ROADMAP.md`  
**Role/data flow:** planning docs, batch/doc-update  
**Analogs:** existing milestone note sections in the same files

**Current stale notes to correct** (`.planning/STATE.md` lines 245-246; `.planning/ROADMAP.md` lines 19-24):
```markdown
- [Milestone v1.4]: White 0.13 support starts with route/root/transport evidence because `.tools/white/EBOOT.ELF.i64` is zero bytes and local `ST7100-1` data alone is not route proof.
- `.tools/white/EBOOT.ELF.i64` is zero bytes in this checkout, so route/root proof must come from another local source unless the IDB/binary is corrected.
```

**Planner instruction:** replace active planning claims that the White IDB is zero bytes with the Phase 23 evidence result. Preserve the route-proof caution: nonzero IDB size is usable evidence availability, not automatic route-prefix proof.

### White Adapter Project Shell

**Files:** `Adapters.GameProtocol.White/Adapters.GameProtocol.White.csproj`, `GlobalUsings.cs`, `DependencyInjection.cs`, `WhiteAdapterMarker.cs`, `MapperlyDefaults.cs`  
**Role/data flow:** adapter config, build graph, DI, source generation  
**Analog:** `Adapters.GameProtocol.Red/`

**Project shape** (`Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj` lines 3-21):
```xml
<PropertyGroup>
  <RootNamespace>TaikoLocalServer.Adapters.GameProtocol.Red</RootNamespace>
  <AssemblyName>TaikoLocalServer.Adapters.GameProtocol.Red</AssemblyName>
</PropertyGroup>

<ItemGroup>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>

<ItemGroup>
  <ProjectReference Include="..\Adapters.GameProtocol.Shared\Adapters.GameProtocol.Shared.csproj" />
  <ProjectReference Include="..\Application\Application.csproj" />
  <ProjectReference Include="..\Contracts.AdminApi\Contracts.AdminApi.csproj" />
</ItemGroup>

<ItemGroup>
  <PackageReference Include="protobuf-net" />
  <PackageReference Include="Riok.Mapperly" />
</ItemGroup>
```

**Global usings** (`Adapters.GameProtocol.Red/GlobalUsings.cs` lines 3-17):
```csharp
global using Mediator;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using ProtoBuf;
global using TaikoLocalServer.Adapters.GameProtocol.Red.Mappers;
global using TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;
global using TaikoLocalServer.Adapters.GameProtocol.Red.Wire;
global using TaikoLocalServer.Application.Ac15;
global using TaikoLocalServer.Application.Common;
global using TaikoLocalServer.Application.Dtos;
global using TaikoLocalServer.Application.Dtos.Ac15;
global using TaikoLocalServer.Application.Handlers;
global using TaikoLocalServer.Application.ServerData;
global using TaikoLocalServer.Domain.Enums;
```

**DI extension and marker** (`DependencyInjection.cs` lines 3-10; `RedAdapterMarker.cs` lines 1-3):
```csharp
public static class DependencyInjection
{
    public const GameEra Era = GameEra.Red;

    public static IServiceCollection AddGameProtocolRed(this IServiceCollection services)
    {
        return services;
    }
}

internal sealed class RedAdapterMarker;
```

**Mapperly defaults** (`MapperlyDefaults.cs` lines 1-5):
```csharp
using Riok.Mapperly.Abstractions;

[assembly: MapperDefaults(
    AutoUserMappings = false,
    RequiredMappingStrategy = RequiredMappingStrategy.Target)]
```

**Planner instruction:** copy the Red adapter shell with `White` namespace, assembly name, marker, `GameEra.White`, and `AddGameProtocolWhite`. Keep the DI extension empty in Phase 23 unless a proven scaffold needs services; do not register White catalog/runtime services in this foundation phase.

### Generated White Wire

**Files:** `Adapters.GameProtocol.White/Wire/Game.cs`, `Adapters.GameProtocol.White/Wire/VsInterface.cs`  
**Role/data flow:** generated wire model, protobuf transform  
**Analog:** Red generated wire

**Game wire header** (`Adapters.GameProtocol.Red/Wire/Game.cs` lines 1-13):
```csharp
// <auto-generated>
//   This file was generated by a tool; you should avoid making direct changes.
//   Consider using 'partial classes' to extend these types
//   Input: taiko.proto
// </auto-generated>

#region Designer generated code
#pragma warning disable CS0612, CS0618, CS1591, CS3021, CS8981, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
namespace TaikoLocalServer.Adapters.GameProtocol.Red.Wire
{

    [global::ProtoBuf.ProtoContract()]
```

**VsInterface wire header** (`Adapters.GameProtocol.Red/Wire/VsInterface.cs` lines 1-13):
```csharp
// <auto-generated>
//   This file was generated by a tool; you should avoid making direct changes.
//   Consider using 'partial classes' to extend these types
//   Input: vsinterface.proto
// </auto-generated>

#region Designer generated code
#pragma warning disable CS0612, CS0618, CS1591, CS3021, CS8981, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
namespace TaikoLocalServer.Adapters.GameProtocol.Red.Wire
{

    [global::ProtoBuf.ProtoContract()]
```

**Proto message inventory to cross-check** (`proto/white/taiko.proto`, `proto/white/vsinterface.proto` grep results):
```text
BookKeeping*, Gettelop*, HeartBeat*, Getfolder*, Taikojuku*, Initialdatacheck*,
Tournamentcheck*, BAID*, MydonEntry*, UserData*, PlayResult*, SelfBest*,
Recommend*, CrownsData*, HeadClerk2*, Getreitai*, Rewardcardcheck*,
Rewardexecution*, StartupAuth*, VerupAuth*, VerupComplete*
```

**Planner instruction:** generate, do not hand-edit. Keep `proto/white` immutable and adapter-local wire under the White namespace. Shared `/v01r00` startup/version ownership does not require duplicate White startup controllers.

### White Controllers

**Files:** `Adapters.GameProtocol.White/Controllers/*Controller.cs`  
**Role/data flow:** controller, request-response  
**Analogs:** `BaseProtocolController`, Red no-state controllers

**Controller base** (`Adapters.GameProtocol.Shared/Controllers/BaseProtocolController.cs` lines 3-11):
```csharp
public abstract class BaseProtocolController<T> : ControllerBase where T : BaseProtocolController<T>
{
    private ILogger<T>? logger;

    private IMediator? mediator;

    protected IMediator Mediator => (mediator ??= HttpContext.RequestServices.GetService<IMediator>()) ?? throw new InvalidOperationException();

    protected ILogger<T> Logger => (logger ??= HttpContext.RequestServices.GetService<ILogger<T>>()) ?? throw new InvalidOperationException();
}
```

**No-state success scaffold** (`Adapters.GameProtocol.Red/Controllers/BookkeepingController.cs` lines 3-13):
```csharp
[ApiController]
[Route("/v08r00_tw/chassis/bookkeeping.php")]
[Route("/v08r01/chassis/bookkeeping.php")]
public class BookkeepingController : BaseProtocolController<BookkeepingController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Bookkeeping([FromBody] BookKeepingRequest request)
    {
        Logger.LogInformation("Red route probe bookkeeping.php request: {@Request}", request);
        return Ok(new BookKeepingResponse { Result = 1 });
    }
}
```

**Safe default response with status fields** (`Adapters.GameProtocol.Red/Controllers/HeartbeatController.cs` lines 10-20):
```csharp
public IActionResult Heartbeat([FromBody] HeartBeatRequest request)
{
    Logger.LogInformation("Red route probe heartbeat.php request: {@Request}", request);
    return Ok(new HeartBeatResponse
    {
        Result = 1,
        ComSvrStat = 1,
        GameSvrStat = 1,
        BnidSvrStat = 1,
        BanacoinStat = 1
    });
}
```

**Stateless reward probe shape** (`Adapters.GameProtocol.Red/Controllers/RewardExecutionController.cs` lines 8-13):
```csharp
[HttpPost]
[Produces("application/protobuf")]
public IActionResult RewardExecution([FromBody] RewardexecutionRequest request)
{
    Logger.LogInformation("Red route probe rewardexecution.php request: {@Request}", request);
    return Ok(new RewardexecutionResponse { Result = 1 });
}
```

**Runtime controller shape to avoid in Phase 23** (`Adapters.GameProtocol.Red/Controllers/GetTelopController.cs` lines 10-16):
```csharp
public async Task<IActionResult> GetTelop([FromBody] GettelopRequest request)
{
    Logger.LogInformation("Red GetTelop request: {@Request}", request);
    var common = await Mediator.Send(
        new GetTelopQuery(GameEra.Red, request.TelopId),
        HttpContext.RequestAborted);
    return Ok(GetTelopMappers.Map(common));
}
```

**Planner instruction:** copy the route/action/protobuf shape only for suffixes proven by White evidence. Use one `/v07r00/chassis/<suffix>.php` route only after prefix proof. Do not copy Red's two route prefixes, runtime `Mediator.Send`, catalog reads, EF writes, profile state, or full request logging for large payloads. D-06 asks for bounded request information.

### Shared Startup And Version Routes

**Files:** no new White startup/version controllers expected  
**Role/data flow:** shared controller, request-response  
**Analogs:** `Adapters.GameProtocol.Shared/Controllers/*`

**Startup owner** (`StartupAuthController.cs` lines 5-12, 17-35):
```csharp
[ApiController]
[Route("/v01r00/chassis/startupauth.php")]
[Route("/v01r00_tw/chassis/startupauth.php")]
public sealed class StartupAuthController : BaseProtocolController<StartupAuthController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> StartupAuth([FromBody] StartupAuthRequest request)
```

```csharp
var response = new StartupAuthResponse { Result = 1 };
var movieData = await Mediator.Send(
    new GetStartupMovieDataQuery(request.HddVer),
    HttpContext.RequestAborted);
response.AryMovieInfoes.AddRange(movieData.Select(movie =>
    new StartupAuthResponse.MovieData
    {
        MovieId = movie.MovieId,
        EnableDays = movie.EnableDays
    }));
return Ok(response);
```

**Version route probes** (`VerupAuthController.cs` lines 5-15; `VerupCompleteController.cs` lines 5-15):
```csharp
[Route("/v01r00/chassis/verupauth.php")]
[Route("/v01r00_tw/chassis/verupauth.php")]
public IActionResult VerupAuth([FromBody] VerupAuthRequest request)
{
    Logger.LogInformation("VerupAuth request: {@Request}", request);
    return Ok(new VerupAuthResponse { Result = 1 });
}
```

**Planner instruction:** keep `/v01r00/chassis/startupauth.php`, `verupauth.php`, and `verupcomplete.php` in the shared adapter unless White evidence contradicts D-02. Do not add duplicate `/v07r00` startup/version routes in Phase 23.

### Host Era Gating And Fallback

**Files:** `Host/Program.cs`, `Host/Host.csproj`, `Host/Configurations/ServerSettings.json`, `TaikoLocalServer.slnx`, `Domain/Enums/GameEra.cs`  
**Role/data flow:** host composition, build graph, config binding, request-response fallback

**Era enum extension point** (`Domain/Enums/GameEra.cs` lines 3-10):
```csharp
public enum GameEra
{
    Nijiiro = 0,
    Green = 1,
    Blue = 2,
    Yellow = 3,
    Red = 4
}
```

**Enabled-era parse and registration** (`Host/Program.cs` lines 75-80, 121-135):
```csharp
var enabledEras = serverSettingsConfig.GetSection("Eras")
    .GetChildren()
    .Where(s => s.GetValue<bool>("Enabled"))
    .Select(s => Enum.Parse<GameEra>(s.Key, ignoreCase: true))
    .ToHashSet();
```

```csharp
if (enabledEras.Contains(GameEra.Yellow))
{
    builder.Services.AddGameProtocolYellow();
}
if (enabledEras.Contains(GameEra.Red))
{
    builder.Services.AddGameProtocolRed();
}
```

**Disabled adapter removal** (`Host/Program.cs` lines 138-165, 279-287):
```csharp
builder.Services.AddControllers()
    .AddProtoBufNet()
    .ConfigureApplicationPartManager(apm =>
    {
        // Adapter assemblies referenced by Host are auto-discovered as ApplicationParts.
        // Remove disabled-era assemblies so their controllers are not routed.
        if (!enabledEras.Contains(GameEra.Red))
        {
            RemoveApplicationPart(apm, "TaikoLocalServer.Adapters.GameProtocol.Red");
        }
    });
```

```csharp
static void RemoveApplicationPart(ApplicationPartManager apm, string assemblyName)
{
    var part = apm.ApplicationParts.FirstOrDefault(p =>
        p is AssemblyPart a && a.Assembly.GetName().Name == assemblyName);
    if (part is not null)
    {
        apm.ApplicationParts.Remove(part);
    }
}
```

**Missing-content-type middleware and exact fallback list** (`Host/Program.cs` lines 203-211, 289-305):
```csharp
app.Use(async (context, next) =>
{
    if (ShouldAssumeProtobufRequest(context.Request))
    {
        context.Request.ContentType = "application/protobuf";
    }

    await next();
});
```

```csharp
static bool ShouldAssumeProtobufRequest(HttpRequest request)
{
    if (!HttpMethods.IsPost(request.Method) || !string.IsNullOrWhiteSpace(request.ContentType))
    {
        return false;
    }

    var path = request.Path;
    return path.StartsWithSegments("/v11r01/chassis", StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/v10r03/chassis", StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/v09r02/chassis", StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/v08r01/chassis", StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/v08r00_tw/chassis", StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/v01r00/chassis", StringComparison.OrdinalIgnoreCase);
}
```

**Host project refs and data rules** (`Host/Host.csproj` lines 42-55, 168-176, 180-195):
```xml
<ProjectReference Include="..\Adapters.GameProtocol.Blue\Adapters.GameProtocol.Blue.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.Green\Adapters.GameProtocol.Green.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.Red\Adapters.GameProtocol.Red.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.Shared\Adapters.GameProtocol.Shared.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.Yellow\Adapters.GameProtocol.Yellow.csproj" />
```

```xml
<!--Red Game Data-->
<Content Remove="wwwroot\data\red\data\**" />
<_ContentIncludedByDefault Remove="wwwroot\data\red\data\**" />
<Content Update="wwwroot\data\red\red_recommend_songs.json"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></Content>
```

```xml
<Target Name="CreateRedGameDataSymlinkForDebug" AfterTargets="Build" Condition="'$(Configuration)'=='Debug' and Exists('$(MSBuildProjectDirectory)\wwwroot\data\red\data')">
  <MakeDir Directories="$(OutDir)wwwroot\data\red" />
  <Exec Command="powershell -NoProfile -ExecutionPolicy Bypass -Command &quot;$link = Join-Path '$(OutDir)' 'wwwroot\data\red\data'; $target = Resolve-Path '$(MSBuildProjectDirectory)\wwwroot\data\red\data'; if (Test-Path -LiteralPath $link) { $item = Get-Item -LiteralPath $link -Force; if (($item.LinkType -eq 'SymbolicLink' -or $item.LinkType -eq 'Junction') -and $item.Target -contains $target.Path) { exit 0 }; Remove-Item -LiteralPath $link -Force -Recurse }; New-Item -ItemType Junction -Path $link -Target $target.Path | Out-Null&quot;" />
</Target>
```

**Solution entry pattern** (`TaikoLocalServer.slnx` lines 4-10):
```xml
<Project Path="Adapters.GameProtocol.CnR00/Adapters.GameProtocol.CnR00.csproj" />
<Project Path="Adapters.GameProtocol.Blue/Adapters.GameProtocol.Blue.csproj" />
<Project Path="Adapters.GameProtocol.Green/Adapters.GameProtocol.Green.csproj" />
<Project Path="Adapters.GameProtocol.Shared/Adapters.GameProtocol.Shared.csproj" />
<Project Path="Adapters.GameProtocol.WwR08/Adapters.GameProtocol.WwR08.csproj" />
<Project Path="Adapters.GameProtocol.Yellow/Adapters.GameProtocol.Yellow.csproj" />
<Project Path="Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj" />
```

**Server settings block shape** (`Host/Configurations/ServerSettings.json` lines 35-42):
```json
"Red": {
  "Enabled": true,
  "AutoExtractCatalog": true,
  "GameDataPath": "wwwroot/data/red/data",
  "CustomizationNameDataPath": "",
  "EnableChallengeCompe": true,
  "ActiveChallengeCompeBundleId": "red-2016-08"
}
```

**Planner instruction:** add White to every explicit Host surface: using, enabled-era message, `AddGameProtocolWhite`, application-part removal, exact fallback allowlist only after `/v07r00` proof, Host project reference, solution entry, config block, and operator-data exclusion/debug junction. Do not add White catalog registration in Infrastructure in Phase 23 unless the plan explicitly broadens beyond foundation.

### Settings Validation Boundary

**Files:** `Application/Settings/ServerSettingsOptionsValidationExtensions.cs`, `Tests/White/WhiteServerSettingsValidationTests.cs`  
**Role/data flow:** settings validation, validation test  
**Analogs:** Red and Blue settings validation

**Validation allowlist** (`Application/Settings/ServerSettingsOptionsValidationExtensions.cs` lines 8-13, 30-32):
```csharp
private static readonly GameEra[] Ac15ShopEras =
[
    GameEra.Green,
    GameEra.Blue,
    GameEra.Yellow
];
```

```csharp
return builder.Validate(
    settings => HasActiveChallengeCompeBundle(settings, enabledEras, GameEra.Red),
    $"ServerSettings:Eras:{GameEra.Red}:ActiveChallengeCompeBundleId is required when {GameEra.Red} EnableChallengeCompe is true.");
```

**Red no-shop requirement test** (`Tests/Red/RedServerSettingsValidationTests.cs` lines 10-34):
```csharp
[Fact]
public void RedEnabledDoesNotRequireShopSettings()
{
    var configuration = BuildConfiguration("""
        {
          "ServerSettings": {
            "Eras": {
              "Red": {
                "Enabled": true
              }
            }
          }
        }
        """);

    using var provider = BuildProvider(configuration);

    var exception = Record.Exception(() =>
        provider.GetRequiredService<IOptions<ServerSettings>>().Value);

    Assert.Null(exception);

    var settings = provider.GetRequiredService<IOptions<ServerSettings>>().Value;
    Assert.True(settings.Eras[nameof(GameEra.Red)].Enabled);
    Assert.Null(settings.Eras[nameof(GameEra.Red)].EnableShop);
    Assert.Null(settings.Eras[nameof(GameEra.Red)].ActiveShopSeasonId);
}
```

**Provider helper** (`Tests/Red/RedServerSettingsValidationTests.cs` lines 69-75):
```csharp
private static ServiceProvider BuildProvider(IConfiguration configuration)
{
    var services = new ServiceCollection();
    services.AddOptions<ServerSettings>()
        .Bind(configuration.GetSection("ServerSettings"))
        .ValidateStartupSettings(new HashSet<GameEra> { GameEra.Red })
        .ValidateOnStart();
```

**Planner instruction:** White should follow the Red "no shop settings required" boundary in Phase 23, not Blue/Green/Yellow shop validation. Add a White settings test only if it protects the no-shop/no-challenge foundation boundary; do not add source-shape assertions.

### Mapperly Mappers, If Any

**Files:** `Adapters.GameProtocol.White/Mappers/*Mappers.cs`  
**Role/data flow:** mapper, transform  
**Analog:** Red Mapperly mappers

**Simple response mapper** (`Adapters.GameProtocol.Red/Mappers/GetTelopMappers.cs` lines 1-13):
```csharp
using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Red.Mappers;

[Mapper]
public static partial class GetTelopMappers
{
    [MapProperty(nameof(CommonGetTelopResponse.StartDatetime), nameof(GettelopResponse.StartDatetime), Use = nameof(MapPresentString))]
    [MapProperty(nameof(CommonGetTelopResponse.EndDatetime), nameof(GettelopResponse.EndDatetime), Use = nameof(MapPresentString))]
    [MapProperty(nameof(CommonGetTelopResponse.Telop), nameof(GettelopResponse.Telop), Use = nameof(MapPresentString))]
    public static partial GettelopResponse Map(CommonGetTelopResponse common);

    private static string MapPresentString(string? value) => string.IsNullOrEmpty(value) ? null! : value;
}
```

**Mapping-target update shape** (`Adapters.GameProtocol.Red/Mappers/UserDataMappers.cs` lines 6-31):
```csharp
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static partial class UserDataMappers
{
    [MapProperty(nameof(Ac15UserDataSongFlags.ReleaseSongFlg), nameof(UserDataResponse.HashReleaseSongFlg))]
    public static partial void Apply(Ac15UserDataSongFlags source, [MappingTarget] UserDataResponse response);

    [MapProperty(nameof(Ac15UserDataRecommendations.RecommendBestSong), nameof(UserDataResponse.RecommendBestSongs), Use = nameof(MapRecommendBestSongs))]
    public static partial void Apply(Ac15UserDataRecommendations source, [MappingTarget] UserDataResponse response);

    private static uint? MapDispTaikojukuDan(uint value) => value is >= 1 and <= 25 ? value : 1u;

    private static uint[] MapRecommendBestSongs(List<uint> value) => value.ToArray();
}
```

**Planner instruction:** Phase 23 should usually not need White mappers because runtime Mediator/common DTO behavior is deferred. If a mapper is added, keep it source-generator driven and inspect generated `.g.cs` output during verification.

### Tests And Test Project

**Files:** `Tests/Tests.csproj`, `Tests/White/WhiteServerSettingsValidationTests.cs`, `Tests/White/WhiteHostRouteGatingTests.cs`  
**Role/data flow:** tests, validation and request-response

**Test project reference pattern** (`Tests/Tests.csproj` lines 18-30):
```xml
<ProjectReference Include="..\Application\Application.csproj" />
<ProjectReference Include="..\Adapters.AdminApi\Adapters.AdminApi.csproj" />
<ProjectReference Include="..\Adapters.AllnetMucha\Adapters.AllnetMucha.csproj" />
<ProjectReference Include="..\Domain\Domain.csproj" />
<ProjectReference Include="..\Infrastructure\Infrastructure.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.Shared\Adapters.GameProtocol.Shared.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.WwR08\Adapters.GameProtocol.WwR08.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.Blue\Adapters.GameProtocol.Blue.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.Green\Adapters.GameProtocol.Green.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.Yellow\Adapters.GameProtocol.Yellow.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.Red\Adapters.GameProtocol.Red.csproj" />
```

**Shared startup controller test shape** (`Tests/Green/StartupAuthControllerTests.cs` lines 17-63):
```csharp
var services = new ServiceCollection();
services.AddLogging();
services.AddOptions();
services.AddApplication();
services.Configure<ServerSettings>(settings =>
{
    settings.Eras = new Dictionary<string, EraSettings>
    {
        [nameof(GameEra.Green)] = new() { Enabled = true }
    };
});
services.AddSingleton<IGameDataCatalog>(new FileGameDataCatalog(
[
    new GreenHandlerFixture.TestGreenCatalog
    {
        Movies =
        [
            new AppMovieData { MovieId = 100, EnableDays = 999 }
        ]
    }
]));
```

```csharp
var result = await controller.StartupAuth(new SharedStartupAuthRequest
{
    ChassisId = "chassis",
    HddVer = 1113,
    ShopId = "shop"
});

var ok = Assert.IsType<OkObjectResult>(result);
var response = Assert.IsType<SharedStartupAuthResponse>(ok.Value);
var movie = Assert.Single(response.AryMovieInfoes);
```

**Planner instruction:** add the White adapter project reference to `Tests.csproj`. Copy Red settings-test structure for `WhiteServerSettingsValidationTests`. For route gating, there is no repo-local test analog; create a behavior-facing test only if it exercises enabled/disabled route behavior through MVC routing or a real request pipeline. Avoid source text, controller attribute, enum-value, or generated-type tests.

## Shared Patterns

### Evidence Gate Before Route Attributes

**Source:** `23-CONTEXT.md` lines 19-28; `12-YELLOW-EVIDENCE.md` lines 14-61  
**Apply to:** evidence artifact, White controllers, Host fallback  

Route prefix `/v07r00/chassis` and each endpoint suffix must be proven before code treats them as final. `proto/white` message presence is a cross-check, not route-call proof.

### Adapter-Local Wire And Routes

**Source:** `Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj`; `Adapters.GameProtocol.Red/Wire/Game.cs`  
**Apply to:** White adapter project, generated wire, controllers  

Keep generated wire DTOs and route controllers in `Adapters.GameProtocol.White`. Do not create a shared AC15 wire project and do not persist wire DTOs directly.

### No-State Controller Scope

**Source:** Red `BookkeepingController`, `HeartbeatController`, `RewardExecutionController`  
**Apply to:** evidence-backed Phase 23 White controller scaffolds  

Controllers may deserialize a White request DTO, log bounded request identifiers, and return a safe default response. They must not add Mediator handlers, EF writes, catalog reads, profile state, reward state, or runtime semantics in Phase 23.

### Shared Startup/Version Ownership

**Source:** `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs`, `VerupAuthController.cs`, `VerupCompleteController.cs`  
**Apply to:** White startup/version handling  

Older AC15 startup/version routes stay under `/v01r00/chassis/*` and the shared adapter unless White evidence contradicts D-02.

### Exact Host Fallback

**Source:** `Host/Program.cs` `ShouldAssumeProtobufRequest`  
**Apply to:** White missing-content-type handling  

Add `/v07r00/chassis` only after route-prefix proof. Do not replace the allowlist with a broad AC15 or catch-all protobuf fallback.

### Disabled-Era Safety

**Source:** `Host/Program.cs` application-part removal  
**Apply to:** Host registration, tests  

Because Host references adapter assemblies, disabled adapters must be removed from MVC application parts. White must have the same enabled-era registration and disabled-era removal as Red, Yellow, Blue, and Green.

### Settings Validation Boundary

**Source:** `Application/Settings/ServerSettingsOptionsValidationExtensions.cs`; `Tests/Red/RedServerSettingsValidationTests.cs`  
**Apply to:** White settings and tests  

White Phase 23 should not require shop, challenge, catalog, or runtime settings beyond `Enabled` and the data path. Keep shop validation limited to Green/Blue/Yellow and ChallengeCompe bundle validation limited to Red unless later White evidence changes the contract.

## No Analog Found

| File | Role | Data Flow | Reason |
|---|---|---|---|
| `Tests/White/WhiteHostRouteGatingTests.cs` | test | request-response / route discovery | No existing repo test uses `WebApplicationFactory`, endpoint discovery, or application-part route gating. Planner must design a behavior-facing test and avoid source/attribute assertions. |

## Metadata

**Analog search scope:** `Adapters.GameProtocol.Red`, `Adapters.GameProtocol.Shared`, `Host`, `Domain`, `Application/Settings`, `Tests`, `.planning/milestones/v1.2-phases/12-yellow-evidence-and-era-foundation`, `.planning/research`  
**Files scanned:** 338  
**Pattern extraction date:** 2026-06-17
