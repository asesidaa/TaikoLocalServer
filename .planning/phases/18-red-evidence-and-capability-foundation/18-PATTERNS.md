# Phase 18: Red Evidence and Capability Foundation - Pattern Map

**Mapped:** 2026-06-13
**Files analyzed:** 21 files/families
**Analogs found:** 20 / 21

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|-------------------|------|-----------|----------------|---------------|
| `.planning/phases/18-red-evidence-and-capability-foundation/18-RED-EVIDENCE.md` or split evidence/matrix docs | documentation | evidence transform | `.planning/milestones/v1.2-phases/12-yellow-evidence-and-era-foundation/12-YELLOW-EVIDENCE.md` | exact |
| `Domain/Enums/GameEra.cs` | model/config enum | request-routing config | `Domain/Enums/GameEra.cs` | exact |
| `TaikoLocalServer.slnx` | solution config | build graph | `TaikoLocalServer.slnx` | exact |
| `Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj` | adapter project config | build graph | `Adapters.GameProtocol.Yellow/Adapters.GameProtocol.Yellow.csproj` | exact |
| `Adapters.GameProtocol.Red/DependencyInjection.cs` | adapter DI | request-routing config | `Adapters.GameProtocol.Yellow/DependencyInjection.cs` | exact |
| `Adapters.GameProtocol.Red/RedAdapterMarker.cs` | adapter marker | request-routing config | `Adapters.GameProtocol.Yellow/YellowAdapterMarker.cs` | exact |
| `Adapters.GameProtocol.Red/GlobalUsings.cs` | adapter config | compile-time imports | `Adapters.GameProtocol.Yellow/GlobalUsings.cs` | exact |
| `Adapters.GameProtocol.Red/MapperlyDefaults.cs` | mapper config | transform | `Adapters.GameProtocol.Yellow/MapperlyDefaults.cs` | exact |
| `Adapters.GameProtocol.Red/Wire/Game.cs` | generated wire model | direct protobuf | `Adapters.GameProtocol.Yellow/Wire/Game.cs` | exact generated-output shape |
| `Adapters.GameProtocol.Red/Wire/VsInterface.cs` | generated wire model | direct protobuf | `Adapters.GameProtocol.Yellow/Wire/VsInterface.cs` | exact generated-output shape |
| `Adapters.GameProtocol.Red/Controllers/*Controller.cs` route probes | controller | request-response | `Adapters.GameProtocol.Blue/Controllers/GetBanacoinInfoController.cs`; `Adapters.GameProtocol.Yellow/Controllers/BookkeepingController.cs`; `Adapters.GameProtocol.Yellow/Controllers/HeartbeatController.cs` | role-match |
| `Adapters.GameProtocol.Red/Controllers/ChallengeCompeController.cs` probe only | controller | request-response | `Adapters.GameProtocol.Yellow/Controllers/ChallengeCompeController.cs` | route-shape only, semantics out of scope |
| `Adapters.GameProtocol.Red/RouteProbeResponseFactory.cs` or equivalent helper if needed | utility | transform | no exact analog; prefer inline controller responses unless generated required fields need centralization | no close analog |
| `Host/Program.cs` | host composition | request-routing config | `Host/Program.cs` | exact |
| `Host/Host.csproj` | host build config | build/file-I/O | `Host/Host.csproj` | exact |
| `Host/Configurations/ServerSettings.json` | host settings | config | `Host/Configurations/ServerSettings.json` | exact |
| `Application/Settings/ServerSettingsOptionsValidationExtensions.cs` | settings validation | config validation | `Application/Settings/ServerSettingsOptionsValidationExtensions.cs` | exact |
| `Application/Handlers/GetStartupMovieDataQuery.cs` | handler | request-response | `Application/Handlers/GetStartupMovieDataQuery.cs`; `Tests/Green/StartupMovieDataQueryTests.cs` | role-match |
| `Tests/Tests.csproj` | test project config | build graph | `Tests/Tests.csproj` | exact |
| `Tests/Red/RedServerSettingsValidationTests.cs` or focused Red settings test | test | config validation | `Tests/Yellow/YellowServerSettingsValidationTests.cs` | exact |
| `Tests/Red/RedStartupAuthControllerTests.cs` or focused shared-startup test if Red mapping is added | test | request-response | `Tests/Yellow/YellowStartupAuthControllerTests.cs`; `Tests/Green/StartupMovieDataQueryTests.cs` | role-match |

## Pattern Assignments

### `.planning/phases/18-red-evidence-and-capability-foundation/18-RED-EVIDENCE.md` (documentation, evidence transform)

**Analog:** `.planning/milestones/v1.2-phases/12-yellow-evidence-and-era-foundation/12-YELLOW-EVIDENCE.md`

**Header and scope pattern** (lines 1-12):
```markdown
# Phase 12 Yellow Evidence

**Created:** 2026-06-07
**Scope:** Yellow route/version/transport foundation only.

## Route Prefix Decision

| Question | Status | Evidence Source | Decision |
|----------|--------|-----------------|----------|
| Exact Yellow game route prefix | RESOLVED | Explicit user approval on 2026-06-07 | Use `/v09r00` for Yellow game routes in Phase 12 scaffold work. |
```

**Endpoint matrix pattern** (lines 14-43):
```markdown
## Proto-Backed Endpoint Suffixes

| Endpoint suffix | Proto request | Proto response | Phase 12 treatment |
|-----------------|---------------|----------------|--------------------|
| `bookkeeping.php` | `BookKeepingRequest` | `BookKeepingResponse` | Supported suffix; no-state scaffold candidate. |
| `getreitai.php` | `GetreitaiRequest` | `GetreitaiResponse` | Proto messages exist, but no Yellow route-call evidence is recorded; do not scaffold in Phase 12. |
```

**Shared startup/version ownership pattern** (lines 45-53):
```markdown
## Shared Startup And Version Routes

| Route | Owner | Evidence | Phase 12 decision |
|-------|-------|----------|-------------------|
| `/v01r00/chassis/startupauth.php` | `Adapters.GameProtocol.Shared` | Yellow `vsinterface.proto` matches the shared startup request/response shape. | Keep shared ownership. |

Yellow must not add duplicate startup/version controllers under `/v09r00/chassis/*` unless later client evidence proves Yellow-specific ownership.
```

**Transport gap pattern** (lines 55-61):
```markdown
## Transport Evidence And Gaps

| Surface | Status | Evidence | Phase 12 treatment |
|---------|--------|----------|--------------------|
| Game route request body | SUPPORTED SCAFFOLD | Local `proto/yellow/yellow.proto` exposes direct request messages such as `PlayResultRequest` with `Baid` and no wrapper `PlayresultData`. | Use direct protobuf DTOs for scaffold controllers. |
```

**Apply to Red:** Use the Phase 18 research route table as the Red endpoint matrix source. Include IDB addresses for every IDB-known Red suffix, especially `/v08r01` at `0xDA7660`, shared `/v01r00` at `0xDA76A0`, startup/version suffix addresses, and route suffix addresses from `18-RESEARCH.md` lines 167-192. Mark `getbanacoininfo.php` and `getreitai.php` as proto-only candidates unless deeper IDA/runtime evidence requires them.

### `Domain/Enums/GameEra.cs` (model/config enum, request-routing config)

**Analog:** `Domain/Enums/GameEra.cs`

**Enum append pattern** (lines 3-8):
```csharp
public enum GameEra
{
    Nijiiro = 0,
    Green = 1,
    Blue = 2,
    Yellow = 3
}
```

**Apply to Red:** Add `Red` as a first-class enum value. Do not treat Red as Yellow/Blue in switch fallbacks. Update settings parsing and any affected exhaustive switches deliberately.

### `TaikoLocalServer.slnx` (solution config, build graph)

**Analog:** `TaikoLocalServer.slnx`

**Project include pattern** (lines 1-18):
```xml
<Solution>
  <Project Path="Adapters.GameProtocol.Blue/Adapters.GameProtocol.Blue.csproj" />
  <Project Path="Adapters.GameProtocol.Green/Adapters.GameProtocol.Green.csproj" />
  <Project Path="Adapters.GameProtocol.Shared/Adapters.GameProtocol.Shared.csproj" />
  <Project Path="Adapters.GameProtocol.Yellow/Adapters.GameProtocol.Yellow.csproj" />
  <Project Path="Tests/Tests.csproj" />
</Solution>
```

**Apply to Red:** Add `Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj` next to the other game protocol adapter projects so solution builds include the new adapter.

### `Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj` (adapter project config, build graph)

**Analog:** `Adapters.GameProtocol.Yellow/Adapters.GameProtocol.Yellow.csproj`

**Project shape pattern** (lines 1-21):
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <RootNamespace>TaikoLocalServer.Adapters.GameProtocol.Yellow</RootNamespace>
    <AssemblyName>TaikoLocalServer.Adapters.GameProtocol.Yellow</AssemblyName>
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
</Project>
```

**Apply to Red:** Replace Yellow namespace/assembly names with Red. Keep Shared/Application references. Keep `protobuf-net` and `Riok.Mapperly`; do not add new packages.

### `Adapters.GameProtocol.Red/DependencyInjection.cs` (adapter DI, request-routing config)

**Analog:** `Adapters.GameProtocol.Yellow/DependencyInjection.cs`

**Adapter DI pattern** (lines 1-10):
```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Yellow;

public static class DependencyInjection
{
    public const GameEra Era = GameEra.Yellow;

    public static IServiceCollection AddGameProtocolYellow(this IServiceCollection services)
    {
        return services;
    }
}
```

**Apply to Red:** Expose `public const GameEra Era = GameEra.Red` and `AddGameProtocolRed`. Keep the method empty unless Red route probes require adapter-local services; Phase 18 probes should not register gameplay state services.

### `Adapters.GameProtocol.Red/GlobalUsings.cs`, `RedAdapterMarker.cs`, `MapperlyDefaults.cs`

**Analogs:** Yellow adapter config files.

**Global usings pattern** (`Adapters.GameProtocol.Yellow/GlobalUsings.cs` lines 3-15):
```csharp
global using Mediator;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using ProtoBuf;
global using TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;
global using TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;
global using TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire;
global using TaikoLocalServer.Application.Handlers;
global using TaikoLocalServer.Domain.Enums;
```

**Marker pattern** (`Adapters.GameProtocol.Yellow/YellowAdapterMarker.cs` lines 1-3):
```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Yellow;

internal sealed class YellowAdapterMarker;
```

**Mapperly defaults pattern** (`Adapters.GameProtocol.Yellow/MapperlyDefaults.cs` lines 1-5):
```csharp
using Riok.Mapperly.Abstractions;

[assembly: MapperDefaults(
    AutoUserMappings = false,
    RequiredMappingStrategy = RequiredMappingStrategy.Target)]
```

**Apply to Red:** Mirror file shape with Red namespaces. Include `Mappers` only if Red creates real mappers or response helpers; otherwise keep imports minimal enough to compile. Keep Mapperly defaults even if Phase 18 has few mappings, because later generated-wire mapping should inherit the same strict target strategy.

### `Adapters.GameProtocol.Red/Wire/Game.cs` and `Wire/VsInterface.cs` (generated wire model, direct protobuf)

**Analog:** `Adapters.GameProtocol.Yellow/Wire/Game.cs` and `Adapters.GameProtocol.Yellow/Wire/VsInterface.cs`

**Generated file header/namespace pattern** (`Game.cs` lines 1-10):
```csharp
// <auto-generated>
//   This file was generated by a tool; you should avoid making direct changes.
//   Consider using 'partial classes' to extend these types
//   Input: yellow-final.proto
// </auto-generated>

#region Designer generated code
#pragma warning disable CS0612, CS0618, CS1591, CS3021, CS8981, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire
{
```

**Nullable optional primitive pattern** (`VsInterface.cs` lines 35-43):
```csharp
[global::ProtoBuf.ProtoMember(4, Name = @"usbmem_ver")]
public uint? UsbmemVer
{
    get => __pbn__UsbmemVer;
    set => __pbn__UsbmemVer = value;
}
public bool ShouldSerializeUsbmemVer() => __pbn__UsbmemVer != null;
```

**Generation command from research** (`18-RESEARCH.md` lines 135-140):
```powershell
.\.tools\protogen.exe --csharp_out=$out -Iproto\red +nullablevaluetype=yes taiko.proto vsinterface.proto
```

**Apply to Red:** Generate from `proto/red`, do not hand-author DTOs, and do not edit dumped proto inputs unless a documented protogen compatibility issue forces it. Place generated outputs under `Adapters.GameProtocol.Red/Wire/` with namespace `TaikoLocalServer.Adapters.GameProtocol.Red.Wire`.

### `Adapters.GameProtocol.Red/Controllers/*Controller.cs` (controller, request-response route probes)

**Analogs:** `BaseProtocolController`, Blue no-state compatibility controllers, Yellow no-state controllers.

**Base controller pattern** (`Adapters.GameProtocol.Shared/Controllers/BaseProtocolController.cs` lines 3-11):
```csharp
public abstract class BaseProtocolController<T> : ControllerBase where T : BaseProtocolController<T>
{
    protected IMediator Mediator => (mediator ??= HttpContext.RequestServices.GetService<IMediator>()) ?? throw new InvalidOperationException();

    protected ILogger<T> Logger => (logger ??= HttpContext.RequestServices.GetService<ILogger<T>>()) ?? throw new InvalidOperationException();
}
```

**No-state full-request log pattern** (`Adapters.GameProtocol.Blue/Controllers/GetBanacoinInfoController.cs` lines 3-12):
```csharp
[ApiController]
[Route("/v10r03/chassis/getbanacoininfo.php")]
public class GetBanacoinInfoController : BaseProtocolController<GetBanacoinInfoController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetBanacoinInfo([FromBody] GetbanacoininfoRequest request)
    {
        Logger.LogInformation("Blue GetBanacoinInfo request: {@Request}", request);
        return Ok(new GetbanacoininfoResponse { Result = 1 });
    }
}
```

**No-state compact log pattern** (`Adapters.GameProtocol.Yellow/Controllers/BookkeepingController.cs` lines 3-12):
```csharp
[ApiController]
[Route("/v09r02/chassis/bookkeeping.php")]
public class BookkeepingController : BaseProtocolController<BookkeepingController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Bookkeeping([FromBody] BookKeepingRequest request)
    {
        Logger.LogInformation("Yellow Bookkeeping request from {ChassisId}", request.ChassisId);
        return Ok(new BookKeepingResponse { Result = 1 });
    }
}
```

**Required response fields pattern** (`Adapters.GameProtocol.Yellow/Controllers/HeartbeatController.cs` lines 12-19):
```csharp
return Ok(new HeartBeatResponse
{
    Result = 1,
    ComSvrStat = 1,
    GameSvrStat = 1,
    BnidSvrStat = 1,
    BanacoinStat = 1
});
```

**Apply to Red:** Create probes only for IDB-known suffixes from `18-RESEARCH.md` lines 171-192:
`playresult.php`, `banacoinerrorlog.php`, `baidcheck.php`, `mydonentry.php`, `userdata.php`, `challengecompe.php`, `balancecheck.php`, `banacoinpayment.php`, `crownsdata.php`, `recommend.php`, `selfbest.php`, `heartbeat.php`, `rewardcardcheck.php`, `rewardexecution.php`, `initialdatacheck.php`, `tournamentcheck.php`, `bookkeeping.php`, `coinsetting.php`, `gettelop.php`, `getfolder.php`, `taikojuku.php`, and `headclerk2.php`.

Probe rules:
- Use route prefix `/v08r01/chassis/...`.
- Deserialize generated Red wire request DTOs via `[FromBody]`.
- Log enough request data for manual RPCS3/cabinet smoke. Prefer `{@Request}` when field discovery matters.
- Return minimal generated Red response objects with all required fields populated.
- Do not call gameplay Mediator handlers or EF-backed services.
- Do not add proto-only `getbanacoininfo.php` or `getreitai.php` unless later IDA/runtime evidence requires them.

### `Adapters.GameProtocol.Red/Controllers/ChallengeCompeController.cs` (controller, request-response probe only)

**Analog:** `Adapters.GameProtocol.Yellow/Controllers/ChallengeCompeController.cs`

**Route shape to copy, semantics to avoid** (lines 3-15):
```csharp
[ApiController]
[Route("/v09r02/chassis/challengecompe.php")]
public class ChallengeCompeController : BaseProtocolController<ChallengeCompeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> ChallengeCompe([FromBody] ChallengeCompeRequest request)
    {
        Logger.LogInformation("Yellow ChallengeCompe request: {@Request}", request);
        var common = await Mediator.Send(
            new GetChallengeCompeQuery(GameEra.Yellow, request.Baid),
            HttpContext.RequestAborted);
        return Ok(ChallengeCompeMappers.Map(common));
    }
}
```

**Apply to Red:** Copy the controller/route/protobuf shape only. Do not copy the `Mediator.Send`, `GetChallengeCompeQuery`, or Mapperly-backed stateful response behavior. Phase 21 owns ChallengeCompe contract and semantics.

### `Host/Program.cs` (host composition, request-routing config)

**Analog:** `Host/Program.cs`

**Enabled-era parsing pattern** (lines 74-83):
```csharp
var serverSettingsConfig = builder.Configuration.GetSection("ServerSettings");
var enabledEras = serverSettingsConfig.GetSection("Eras")
    .GetChildren()
    .Where(s => s.GetValue<bool>("Enabled"))
    .Select(s => Enum.Parse<GameEra>(s.Key, ignoreCase: true))
    .ToHashSet();

if (enabledEras.Count == 0)
{
    Log.Fatal("ServerSettings.Eras has no enabled era. At least one era (Nijiiro, Green, Blue, or Yellow) must be enabled in Host/Configurations/ServerSettings.json. Refusing to start.");
```

**Era adapter DI pattern** (lines 115-130):
```csharp
if (enabledEras.Contains(GameEra.Blue))
{
    builder.Services.AddGameProtocolBlue();
}
if (enabledEras.Contains(GameEra.Yellow))
{
    builder.Services.AddGameProtocolYellow();
}
```

**Application-part gating pattern** (lines 133-156):
```csharp
builder.Services.AddControllers()
    .AddProtoBufNet()
    .ConfigureApplicationPartManager(apm =>
    {
        // Adapter assemblies referenced by Host are auto-discovered as ApplicationParts.
        // Remove disabled-era assemblies so their controllers are not routed.
        if (!enabledEras.Contains(GameEra.Yellow))
        {
            RemoveApplicationPart(apm, "TaikoLocalServer.Adapters.GameProtocol.Yellow");
        }
    });
```

**Missing content-type fallback pattern** (lines 194-199, 280-293):
```csharp
if (ShouldAssumeProtobufRequest(context.Request))
{
    context.Request.ContentType = "application/protobuf";
}

return path.StartsWithSegments("/v11r01/chassis", StringComparison.OrdinalIgnoreCase)
       || path.StartsWithSegments("/v10r03/chassis", StringComparison.OrdinalIgnoreCase)
       || path.StartsWithSegments("/v09r02/chassis", StringComparison.OrdinalIgnoreCase)
       || path.StartsWithSegments("/v01r00/chassis", StringComparison.OrdinalIgnoreCase);
```

**Apply to Red:** Add Red using/import, conditional `AddGameProtocolRed`, disabled-era removal for `TaikoLocalServer.Adapters.GameProtocol.Red`, and `/v08r01/chassis` in `ShouldAssumeProtobufRequest`. Update the no-enabled-era message to include Red.

### `Host/Host.csproj` (host build config, build/file-I/O)

**Analog:** `Host/Host.csproj`

**Adapter project reference pattern** (lines 42-50):
```xml
<ProjectReference Include="..\Adapters.GameProtocol.Blue\Adapters.GameProtocol.Blue.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.Green\Adapters.GameProtocol.Green.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.Shared\Adapters.GameProtocol.Shared.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.Yellow\Adapters.GameProtocol.Yellow.csproj" />
```

**Era operator-data exclusion and sidecar-copy pattern** (lines 147-165):
```xml
<!--Blue Game Data-->
<Content Remove="wwwroot\data\blue\data\**" />
<_ContentIncludedByDefault Remove="wwwroot\data\blue\data\**" />
<Content Update="wwwroot\data\blue\blue_recommend_songs.json"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></Content>

<!--Yellow Game Data-->
<Content Remove="wwwroot\data\yellow\data\**" />
<_ContentIncludedByDefault Remove="wwwroot\data\yellow\data\**" />
<Content Update="wwwroot\data\yellow\yellow_recommend_songs.json"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></Content>
```

**Debug junction pattern** (lines 169-179):
```xml
<Target Name="CreateYellowGameDataSymlinkForDebug" AfterTargets="Build" Condition="'$(Configuration)'=='Debug' and Exists('$(MSBuildProjectDirectory)\wwwroot\data\yellow\data')">
  <MakeDir Directories="$(OutDir)wwwroot\data\yellow" />
  <Exec Command="powershell -NoProfile -ExecutionPolicy Bypass -Command &quot;$link = Join-Path '$(OutDir)' 'wwwroot\data\yellow\data'; $target = Resolve-Path '$(MSBuildProjectDirectory)\wwwroot\data\yellow\data'; ...&quot;" />
</Target>
```

**Apply to Red:** Add Host project reference to Red adapter. Exclude `wwwroot\data\red\data\**` and `_ContentIncludedByDefault` for Red operator data. Add a Red Debug junction target. Do not add Red sidecar JSON copy rows unless Phase 18 creates committed server-authored Red sidecars; Red runtime catalog JSON belongs to Phase 19+.

### `Host/Configurations/ServerSettings.json` (host settings, config)

**Analog:** `Host/Configurations/ServerSettings.json`

**Era settings pattern** (lines 7-34):
```json
"Eras": {
  "Blue": {
    "Enabled": true,
    "AutoExtractCatalog": true,
    "GameDataPath": "wwwroot/data/blue/data",
    "CustomizationNameDataPath": "",
    "EnableShop": true,
    "ActiveShopSeasonId": 1
  },
  "Yellow": {
    "Enabled": true,
    "AutoExtractCatalog": true,
    "GameDataPath": "wwwroot/data/yellow/data",
    "CustomizationNameDataPath": "",
    "EnableShop": true,
    "ActiveShopSeasonId": 4
  }
}
```

**Apply to Red:** Add `"Red"` with `"Enabled": true`, `"AutoExtractCatalog": true` only if harmless for no-catalog Phase 18, `"GameDataPath": "wwwroot/data/red/data"`, and empty customization path. Do not require or set `EnableShop`/`ActiveShopSeasonId` for Red in Phase 18, because Red shop/medal behavior is out of scope.

### `Application/Settings/ServerSettingsOptionsValidationExtensions.cs` (settings validation, config validation)

**Analog:** same file plus Yellow validation tests.

**Shop-era whitelist pattern** (lines 8-27):
```csharp
private static readonly GameEra[] Ac15ShopEras =
[
    GameEra.Green,
    GameEra.Blue,
    GameEra.Yellow
];

foreach (var era in Ac15ShopEras)
{
    builder = builder
        .Validate(
            settings => HasExplicitShopSetting(settings, enabledEras, era),
            $"ServerSettings:Eras:{era}:EnableShop is required when {era} is enabled.")
        .Validate(
            settings => HasActiveShopSeason(settings, enabledEras, era),
            $"ServerSettings:Eras:{era}:ActiveShopSeasonId must be a nonzero season id when {era} EnableShop is true.");
}
```

**Validation test pattern** (`Tests/Yellow/YellowServerSettingsValidationTests.cs` lines 14-31, 58-95):
```csharp
var configuration = BuildConfiguration("""
    {
      "ServerSettings": {
        "Eras": {
          "Yellow": {
            "Enabled": true
          }
        }
      }
    }
    """);

using var provider = BuildProvider(configuration);

var ex = Assert.Throws<OptionsValidationException>(() =>
    provider.GetRequiredService<IOptions<ServerSettings>>().Value);
```

**Apply to Red:** Do not add `GameEra.Red` to `Ac15ShopEras` in Phase 18. If adding tests, create a Red settings validation test proving Red can be enabled without `EnableShop`/`ActiveShopSeasonId`, because Phase 18 must not imply unsupported shop behavior.

### `Application/Handlers/GetStartupMovieDataQuery.cs` (handler, request-response)

**Analog:** same file and startup tests.

**HDD-version-to-era pattern** (lines 42-64):
```csharp
private GameEra? ResolveEra(uint hddVer)
{
    var requestedEra = (hddVer / 100) switch
    {
        9 => GameEra.Yellow,
        10 => GameEra.Blue,
        11 => GameEra.Green,
        12 => GameEra.Nijiiro,
        _ => (GameEra?)null
    };

    if (requestedEra is not null)
    {
        if (IsEnabled(requestedEra.Value))
        {
            return requestedEra.Value;
        }
```

**Enabled-era fallback pattern** (lines 84-101):
```csharp
private IReadOnlyList<GameEra> GetEnabledEras()
{
    return settings.Eras
        .Where(pair => pair.Value.Enabled)
        .Select(pair => Enum.TryParse<GameEra>(pair.Key, ignoreCase: true, out var era)
            ? era
            : (GameEra?)null)
        .Where(era => era is not null)
        .Select(era => era!.Value)
        .Distinct()
        .Order()
        .ToArray();
}
```

**Controller behavior test pattern** (`Tests/Yellow/YellowStartupAuthControllerTests.cs` lines 17-63):
```csharp
services.AddLogging();
services.AddOptions();
services.AddApplication();
services.Configure<ServerSettings>(settings =>
{
    settings.Eras = new Dictionary<string, EraSettings>
    {
        [nameof(GameEra.Yellow)] = new() { Enabled = true }
    };
});

var result = await controller.StartupAuth(new SharedStartupAuthRequest
{
    ChassisId = "chassis",
    HddVer = 913,
    ShopId = "shop"
});
```

**Apply to Red:** Shared startup/version routes stay in `Adapters.GameProtocol.Shared`. If Phase 18 adds Red HDD mapping, add a focused test for the Red HDD major version and ensure it does not require Red gameplay/catalog data. If a Red catalog placeholder is not in scope, record Red startup movie readback as an explicit evidence gap instead of adding a half-supported catalog path.

### `Tests/Tests.csproj` and `Tests/Red/*` (test config and focused tests)

**Analog:** `Tests/Tests.csproj`, Yellow/Green focused tests.

**Project reference pattern** (`Tests/Tests.csproj` lines 18-29):
```xml
<ProjectReference Include="..\Adapters.GameProtocol.Shared\Adapters.GameProtocol.Shared.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.WwR08\Adapters.GameProtocol.WwR08.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.Blue\Adapters.GameProtocol.Blue.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.Green\Adapters.GameProtocol.Green.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.Yellow\Adapters.GameProtocol.Yellow.csproj" />
```

**Handler-test fixture helper pattern** (`Tests/Green/StartupMovieDataQueryTests.cs` lines 107-123):
```csharp
private static GetStartupMovieDataQueryHandler CreateHandler(
    IGameDataCatalog catalog,
    ServerSettings settings)
    => new(
        catalog,
        NullLogger<GetStartupMovieDataQueryHandler>.Instance,
        Options.Create(settings));

private static ServerSettings Enabled(params GameEra[] eras)
{
    return new ServerSettings
    {
        Eras = eras.ToDictionary(
            era => era.ToString(),
            _ => new EraSettings { Enabled = true })
    };
}
```

**Apply to Red:** Add Red adapter reference to `Tests/Tests.csproj` only if tests instantiate Red controllers or wire types. Avoid tests over generated property existence, controller attributes, route inventory, DI shape, enum numeric values, source text, or "returns Result = 1" echoes unless a real runtime failure requires that exact assertion. Prefer:
- Settings behavior: Red enabled does not require shop settings.
- Startup behavior if Red HDD mapping is added.
- Build verification for generated Red wire and Host scaffold.

## Shared Patterns

### Shared `/v01r00` Startup/Version Ownership

**Source:** `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs` lines 5-18 and `VerupAuthController.cs` lines 5-14.

```csharp
[ApiController]
[Route("/v01r00/chassis/startupauth.php")]
public sealed class StartupAuthController : BaseProtocolController<StartupAuthController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> StartupAuth([FromBody] StartupAuthRequest request)
    {
        Logger.LogInformation(
            "StartupAuth request: {@Request}", request);

        var response = new StartupAuthResponse { Result = 1 };
        var movieData = await Mediator.Send(
            new GetStartupMovieDataQuery(request.HddVer),
            HttpContext.RequestAborted);
```

**Apply to:** Red startup/version evidence and any Red HDD mapping. Do not add duplicate Red startup/version controllers under `/v08r01`.

### Route-Probe No-State Boundary

**Source:** `Adapters.GameProtocol.Blue/Controllers/GetBanacoinInfoController.cs` lines 7-12 and `Adapters.GameProtocol.Yellow/Controllers/BookkeepingController.cs` lines 7-12.

```csharp
[HttpPost]
[Produces("application/protobuf")]
public IActionResult Bookkeeping([FromBody] BookKeepingRequest request)
{
    Logger.LogInformation("Yellow Bookkeeping request from {ChassisId}", request.ChassisId);
    return Ok(new BookKeepingResponse { Result = 1 });
}
```

**Apply to:** All Phase 18 Red route probes. The controller may log and return minimal Red wire responses; it must not call gameplay handlers, mutate EF state, or infer runtime semantics.

### Host Era Gating

**Source:** `Host/Program.cs` lines 115-156 and 270-293.

```csharp
if (enabledEras.Contains(GameEra.Yellow))
{
    builder.Services.AddGameProtocolYellow();
}

if (!enabledEras.Contains(GameEra.Yellow))
{
    RemoveApplicationPart(apm, "TaikoLocalServer.Adapters.GameProtocol.Yellow");
}
```

**Apply to:** Red adapter registration and route exposure. Red routes should exist only when `ServerSettings:Eras:Red:Enabled` is true.

### Direct Protobuf Transport

**Source:** `Host/Program.cs` lines 194-199 and 280-293.

```csharp
if (ShouldAssumeProtobufRequest(context.Request))
{
    context.Request.ContentType = "application/protobuf";
}
```

**Apply to:** Add `/v08r01/chassis` to the same fallback list so Red direct-protobuf requests deserialize when the client omits `Content-Type`.

### Generated Wire Handling

**Source:** `Adapters.GameProtocol.Yellow/Wire/Game.cs` lines 1-10 and `18-RESEARCH.md` lines 135-140.

```powershell
.\.tools\protogen.exe --csharp_out=$out -Iproto\red +nullablevaluetype=yes taiko.proto vsinterface.proto
```

**Apply to:** Red wire generation only. Do not manually clean generated Red `Wire/` beyond required namespace/output normalization, and do not edit `proto/red` unless protogen compatibility forces a documented minimal change.

### Settings Validation Boundary

**Source:** `Application/Settings/ServerSettingsOptionsValidationExtensions.cs` lines 8-27.

```csharp
private static readonly GameEra[] Ac15ShopEras =
[
    GameEra.Green,
    GameEra.Blue,
    GameEra.Yellow
];
```

**Apply to:** Keep Red out of `Ac15ShopEras` in Phase 18. Red settings support must not imply item-shop, medal, wallet, payment, coupon, or transaction behavior.

## Explicitly Out Of Scope Patterns

### Do Not Add `Ac15EraProfiles.Red`

**Do not copy yet:** `Application/Ac15/Ac15EraProfiles.cs` lines 5-18 and 48-68.

```csharp
private static readonly Ac15FeatureSet BlueGreenFeatures = new(
    NormalPlay: true,
    UserData: true,
    SelfBest: true,
    Crowns: true,
    InitialData: true,
    Folders: true,
    Telops: true,
    Recommendations: true,
    Taikojuku: true,
    Dani: true,
    ItemShop: true);
```

Phase 18 must prove Red flag widths, packing, response byte formats, and capability ownership first. Phase 19 owns Red profile binding.

### Do Not Copy ChallengeCompe Semantics

**Route shape only:** `Adapters.GameProtocol.Yellow/Controllers/ChallengeCompeController.cs` lines 9-15.

```csharp
var common = await Mediator.Send(
    new GetChallengeCompeQuery(GameEra.Yellow, request.Baid),
    HttpContext.RequestAborted);
return Ok(ChallengeCompeMappers.Map(common));
```

Red ChallengeCompe is a Phase 18 candidate/probe surface only. Phase 21 owns shared contract and any stateful semantics.

### Do Not Add Red EF Migrations Or Gameplay Persistence

**Relevant existing persistence registration:** `Infrastructure/DependencyInjection.cs` lines 43-59.

```csharp
services.AddDbContext<TaikoDbContext>(option =>
{
    var dbName = configuration["DbFileName"];
    ...
});
services.AddScoped<ITaikoDbContext>(sp => sp.GetRequiredService<TaikoDbContext>());
```

Phase 18 must not add Red tables, migrations, save data, score data, Dani data, Tokkun history, ChallengeCompe state, item-shop state, medal state, wallet state, payment state, coupon state, or transaction state.

### Do Not Copy Banacoin/Payment Authority

**Safe compatibility shape only:** `Adapters.GameProtocol.Blue/Controllers/BanacoinPaymentController.cs` lines 11-18.

```csharp
Logger.LogInformation("Blue BanacoinPayment request: {@Request}", request);
return Ok(new BanacoinpaymentResponse
{
    Result = 1,
    Personid = request.Personid,
    BnidResult = "Ok",
    Chid = "1"
});
```

For Red, this pattern is only a stateless route-probe/simple compatibility shape when IDB/runtime evidence requires the route. It is not wallet, balance, payment, coupon, unlock, or transaction authority.

### Do Not Add AdminApi/WebUI Runtime Closeout

No Phase 18 file should add Red AdminApi readback routes, WebUI Red workflows, or runtime closeout behavior. Phase 22 owns AdminApi/WebUI runtime closeout.

## No Analog Found

| File | Role | Data Flow | Reason |
|------|------|-----------|--------|
| `Adapters.GameProtocol.Red/RouteProbeResponseFactory.cs` or equivalent helper | utility | transform | Existing adapters mostly inline simple responses or use real Mapperly mappers for implemented behavior. Create a helper only if generated Red required fields make inline probe responses repetitive. |
| Full Host route-gating integration test using a live test server | test | request-response | Current `Tests/Tests.csproj` has no `Microsoft.AspNetCore.Mvc.Testing`/`WebApplicationFactory` pattern. Prefer behavior tests over adding a new test-stack dependency in Phase 18. |

## Metadata

**Analog search scope:** `Adapters.GameProtocol.Blue/`, `Adapters.GameProtocol.Green/`, `Adapters.GameProtocol.Yellow/`, `Adapters.GameProtocol.Shared/`, `Host/`, `Application/`, `Domain/`, `Infrastructure/`, `Tests/`, `.planning/milestones/`, `docs/superpowers/`.
**Files scanned:** current phase docs plus 30 code/test/config analogs.
**Pattern extraction date:** 2026-06-13
**Phase 18 route source:** `18-RESEARCH.md` lines 167-192.
**Foundation-only guardrail:** Red EF migrations/gameplay persistence, `Ac15EraProfiles.Red`, ChallengeCompe semantics, Tokkun persistence, Banacoin/payment authority, and AdminApi/WebUI runtime closeout are explicitly out of scope.
