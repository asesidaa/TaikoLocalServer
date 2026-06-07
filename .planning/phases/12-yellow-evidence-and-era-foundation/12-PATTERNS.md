# Phase 12: Yellow Evidence and Era Foundation - Pattern Map

**Mapped:** 2026-06-07  
**Files analyzed:** 20  
**Analogs found:** 20 / 20

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|---|---|---|---|---|
| `.planning/phases/12-yellow-evidence-and-era-foundation/12-YELLOW-EVIDENCE.md` | docs/evidence | evidence matrix | `12-RESEARCH.md`, `BlueBattleSourceGuardTests.cs` | role-match |
| `Adapters.GameProtocol.Yellow/Adapters.GameProtocol.Yellow.csproj` | project config | build graph | `Adapters.GameProtocol.Blue/Adapters.GameProtocol.Blue.csproj` | exact |
| `Adapters.GameProtocol.Yellow/GlobalUsings.cs` | adapter config | compile/import | `Adapters.GameProtocol.Blue/GlobalUsings.cs` | exact |
| `Adapters.GameProtocol.Yellow/DependencyInjection.cs` | DI extension | service registration | `Adapters.GameProtocol.Blue/DependencyInjection.cs` | exact |
| `Adapters.GameProtocol.Yellow/YellowAdapterMarker.cs` | marker | reflection/application-part | `Adapters.GameProtocol.Blue/BlueAdapterMarker.cs` | exact |
| `Adapters.GameProtocol.Yellow/Wire/Game.cs` | generated wire model | protobuf transform | `Adapters.GameProtocol.Blue/Wire/Game.cs` | exact |
| `Adapters.GameProtocol.Yellow/Wire/VsInterface.cs` | generated wire model | protobuf transform | `Adapters.GameProtocol.Green/Wire/VsInterface.cs` | exact |
| `Adapters.GameProtocol.Yellow/Controllers/*Controller.cs` | controller | request-response | `Adapters.GameProtocol.Blue/Controllers/BookkeepingController.cs` | exact for no-state scaffold |
| `Adapters.GameProtocol.Yellow/Controllers/PlayResultController.cs` | controller | request-response transform | `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs` | role-match; runtime deferred |
| `Adapters.GameProtocol.Yellow/Controllers/InitialDataCheckController.cs` | controller | request-response transform | `Adapters.GameProtocol.Blue/Controllers/InitialDataCheckController.cs` | role-match; runtime deferred |
| `Adapters.GameProtocol.Yellow/Mappers/*Mappers.cs` | mapper | transform | `Adapters.GameProtocol.Blue/Mappers/*`, `Adapters.GameProtocol.Green/Mappers/*` | role-match; avoid unless needed |
| `TaikoLocalServer.slnx` | solution config | build graph | existing project entries in `TaikoLocalServer.slnx` | exact |
| `Host/Host.csproj` | host project config | build/content graph | existing Blue/Green project refs and data content rules | exact |
| `Host/Program.cs` | host composition | DI + route discovery | existing Blue/Green branches in `Host/Program.cs` | exact |
| `Domain/Enums/GameEra.cs` | enum/model | config parse | existing enum values | exact |
| `Application/Settings/ServerSettings.cs` | settings model | config binding | existing era dictionary model | exact |
| `Application/Settings/ServerSettingsOptionsValidationExtensions.cs` | settings validation | validation | AC15 shop-era list | role-match; only if Yellow needs shop validation now |
| `Host/Configurations/ServerSettings.json` | config | config binding | existing Green/Blue era blocks | exact |
| `Tests/Tests.csproj` | test project config | build graph | existing Blue/Green adapter references | exact |
| `Tests/Yellow/*.cs` | tests | route/source/wire guard | `Tests/Blue/*`, `Tests/Green/StartupAuthRouteTests.cs` | exact |

## Pattern Assignments

### Yellow Evidence Record

**Analog:** `12-RESEARCH.md` route and no-battle tables; `Tests/Blue/BlueBattleSourceGuardTests.cs`

**Use for:** a compact evidence artifact that separates proto-backed endpoint suffixes from unresolved route-prefix/runtime-framing proof. Keep gaps explicit; do not turn assumptions into route constants.

**Evidence-table content to preserve from research:** supported route suffixes, shared `/v01r00/chassis/*` startup/version ownership, exact Yellow route prefix gap, direct-protobuf framing gap, `getreitai.php` gap, and no-battle evidence.

**Source-guard style** (`Tests/Blue/BlueBattleSourceGuardTests.cs` lines 62-80, 102-114):
```csharp
private static readonly string[] ForbiddenCrossEraBattleTruth =
[
    "GreenAiBattle",
    "Adapters.GameProtocol.Green",
    "GameEra.Green",
    "GameEra.Nijiiro"
];

foreach (var forbidden in ForbiddenCrossEraBattleTruth)
{
    Assert.DoesNotContain(forbidden, source, StringComparison.Ordinal);
}
```

For Yellow, invert this into absence checks for `BattleUserData`, `battleuserdata.php`, `BlueBattle`, `Adapters.GameProtocol.Blue`, Yellow battle entities, Yellow battle migrations, and Blue battle fallback tokens.

### Yellow Adapter Project

**Analog:** `Adapters.GameProtocol.Blue/Adapters.GameProtocol.Blue.csproj` lines 3-22
```xml
<PropertyGroup>
  <RootNamespace>TaikoLocalServer.Adapters.GameProtocol.Blue</RootNamespace>
  <AssemblyName>TaikoLocalServer.Adapters.GameProtocol.Blue</AssemblyName>
  <NoWarn>$(NoWarn);RMG020;RMG012</NoWarn>
</PropertyGroup>

<ItemGroup>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>

<ItemGroup>
  <ProjectReference Include="..\Adapters.GameProtocol.Shared\Adapters.GameProtocol.Shared.csproj" />
  <ProjectReference Include="..\Application\Application.csproj" />
  <ProjectReference Include="..\Contracts.AdminApi\Contracts.AdminApi.csproj" />
</ItemGroup>
```

Copy this shape with `Yellow` namespace/assembly. Keep `protobuf-net` and `Riok.Mapperly` package references if Yellow wire and future mappers live in the adapter.

**Do not:** create a shared AC15 wire project. Phase decision says adapter-local Yellow generated DTOs.

### Yellow Global Usings, DI, Marker

**Analog:** `Adapters.GameProtocol.Blue/GlobalUsings.cs` lines 3-14
```csharp
global using Mediator;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using ProtoBuf;
global using TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;
global using TaikoLocalServer.Adapters.GameProtocol.Blue.Wire;
global using TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;
global using TaikoLocalServer.Application.Common;
global using TaikoLocalServer.Application.Dtos;
global using TaikoLocalServer.Application.Handlers;
global using TaikoLocalServer.Domain.Enums;
```

Use Yellow-local mapper/wire namespaces. If Phase 12 has no mappers, either omit the Yellow mapper using or create only compile-required empty mapper files with clear deferred runtime scope.

**Analog:** `Adapters.GameProtocol.Blue/DependencyInjection.cs` lines 3-10
```csharp
public static class DependencyInjection
{
    public const GameEra Era = GameEra.Blue;

    public static IServiceCollection AddGameProtocolBlue(this IServiceCollection services)
    {
        return services;
    }
}
```

Copy as `GameEra.Yellow` and `AddGameProtocolYellow`.

**Analog:** `Adapters.GameProtocol.Blue/BlueAdapterMarker.cs` lines 1-3
```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Blue;

internal sealed class BlueAdapterMarker;
```

Copy as an internal Yellow marker if tests or Host application-part checks need a stable assembly anchor.

### Generated Yellow Wire Files

**Analog:** `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 1-13
```csharp
// <auto-generated>
//   This file was generated by a tool; you should avoid making direct changes.
//   Consider using 'partial classes' to extend these types
//   Input: taiko.proto
// </auto-generated>

#region Designer generated code
#pragma warning disable CS0612, CS0618, CS1591, CS3021, CS8981, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Wire
{
```

Generate `Adapters.GameProtocol.Yellow/Wire/Game.cs` from `proto/yellow/yellow.proto`; do not hand-edit generated output.

**Analog:** `Adapters.GameProtocol.Green/Wire/VsInterface.cs` lines 1-13
```csharp
// <auto-generated>
//   This file was generated by a tool; you should avoid making direct changes.
//   Consider using 'partial classes' to extend these types
//   Input: vsinterface.proto
// </auto-generated>

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Wire
{
    [global::ProtoBuf.ProtoContract()]
    public partial class StartupAuthRequest : global::ProtoBuf.IExtensible
```

Generate `Adapters.GameProtocol.Yellow/Wire/VsInterface.cs` from `proto/yellow/vsinterface.proto` under the Yellow adapter namespace. Use tests to prove compatibility with shared startup/version DTOs instead of moving shared wire ownership.

### Yellow Controllers

**Base class analog:** `Adapters.GameProtocol.Shared/Controllers/BaseProtocolController.cs` lines 3-11
```csharp
public abstract class BaseProtocolController<T> : ControllerBase where T : BaseProtocolController<T>
{
    protected IMediator Mediator => (mediator ??= HttpContext.RequestServices.GetService<IMediator>()) ?? throw new InvalidOperationException();

    protected ILogger<T> Logger => (logger ??= HttpContext.RequestServices.GetService<ILogger<T>>()) ?? throw new InvalidOperationException();
}
```

All Yellow controllers should derive from this base.

**No-state scaffold analog:** `Adapters.GameProtocol.Blue/Controllers/BookkeepingController.cs` lines 3-13
```csharp
[ApiController]
[Route("/v10r03/chassis/bookkeeping.php")]
public class BookkeepingController : BaseProtocolController<BookkeepingController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Bookkeeping([FromBody] BookKeepingRequest request)
    {
        Logger.LogInformation("Blue Bookkeeping request: {@Request}", request);
        return Ok(new BookKeepingResponse { Result = 1 });
    }
}
```

Use this for Phase 12 Yellow no-state endpoints only after the Yellow base route prefix is evidence-backed. Replace route prefix and log label. Do not add persistence or catalog behavior.

**Mediator-backed direct-body analog:** `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs` lines 7-18
```csharp
[HttpPost]
[Produces("application/protobuf")]
public async Task<IActionResult> PlayResult([FromBody] PlayResultRequest request)
{
    Logger.LogInformation("Blue PlayResult request: {@Request}", request);
    var common = PlayResultMappers.Map(request);

    var result = await Mediator.Send(
        new UpdatePlayResultCommand(request.Baid, GameEra.Blue, common),
        HttpContext.RequestAborted);

    return Ok(PlayResultMappers.Map(result));
}
```

This is the future runtime shape, not a Phase 12 mandate. Phase 12 should not add Yellow score/profile/Tokkun/shop writes.

**Initial-data analog:** `Adapters.GameProtocol.Blue/Controllers/InitialDataCheckController.cs` lines 7-13
```csharp
[HttpPost]
[Produces("application/protobuf")]
public async Task<IActionResult> InitialDataCheck([FromBody] InitialdatacheckRequest request)
{
    Logger.LogInformation("Blue InitialDataCheck request: {@Request}", request);
    var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Blue), HttpContext.RequestAborted);
    return Ok(InitialDataMappers.Map(common));
}
```

For Yellow, use only if a minimal compile scaffold has existing common handler support. Do not add Yellow catalog loading in Phase 12.

### Shared Startup and Version Routes

**Analog:** `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs` lines 5-17, 27-34
```csharp
[ApiController]
[Route("/v01r00/chassis/startupauth.php")]
public sealed class StartupAuthController : BaseProtocolController<StartupAuthController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> StartupAuth([FromBody] StartupAuthRequest request)
    {
        var response = new StartupAuthResponse { Result = 1 };
        var movieData = await Mediator.Send(
            new GetStartupMovieDataQuery(request.HddVer),
            HttpContext.RequestAborted);

        return Ok(response);
    }
}
```

**Analog:** `Adapters.GameProtocol.Shared/Controllers/VerupAuthController.cs` lines 5-14
```csharp
[ApiController]
[Route("/v01r00/chassis/verupauth.php")]
public sealed class VerupAuthController : BaseProtocolController<VerupAuthController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult VerupAuth([FromBody] VerupAuthRequest request)
    {
        Logger.LogInformation("VerupAuth request: {@Request}", request);
        return Ok(new VerupAuthResponse { Result = 1 });
    }
}
```

Keep shared ownership unless Yellow-specific client evidence appears. Yellow adapter route tests should explicitly assert it does not own startup/version routes under the Yellow game prefix.

### Host and Settings Integration

**Enum analog:** `Domain/Enums/GameEra.cs` lines 3-8
```csharp
public enum GameEra
{
    Nijiiro = 0,
    Green = 1,
    Blue = 2
}
```

Add `Yellow` with a stable value after Blue unless planning chooses a documented ordering. Add enum parse tests before adding `"Yellow"` to JSON because Host parses settings keys through `Enum.Parse<GameEra>`.

**Settings model analog:** `Application/Settings/ServerSettings.cs` lines 3-24
```csharp
public sealed class ServerSettings
{
    public bool EnableMoreSongs { get; set; }
    public Dictionary<string, EraSettings> Eras { get; set; } = new();
}

public sealed class EraSettings
{
    public bool Enabled { get; set; }
    public bool AutoExtractCatalog { get; set; } = true;
    public string GameDataPath { get; set; } = string.Empty;
    public bool? EnableShop { get; set; }
    public uint? ActiveShopSeasonId { get; set; }
}
```

Yellow can use the existing dictionary shape. Do not add Yellow-specific settings unless Phase 12 has compile-time need.

**Validation analog:** `Application/Settings/ServerSettingsOptionsValidationExtensions.cs` lines 8-27
```csharp
private static readonly GameEra[] Ac15ShopEras =
[
    GameEra.Green,
    GameEra.Blue
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

Only add Yellow to this list if Phase 12 deliberately ships `EnableShop` validation for Yellow. Research defers Yellow shop runtime, so default pattern is to leave this alone unless settings tests require it.

**JSON analog:** `Host/Configurations/ServerSettings.json` lines 7-26
```json
"Eras": {
  "Nijiiro": {
    "Enabled": false
  },
  "Green": {
    "Enabled": true,
    "AutoExtractCatalog": true,
    "GameDataPath": "wwwroot/data/green/data",
    "EnableShop": true,
    "ActiveShopSeasonId": 2
  },
  "Blue": {
    "Enabled": true,
    "AutoExtractCatalog": true,
    "GameDataPath": "wwwroot/data/blue/data",
    "EnableShop": true,
    "ActiveShopSeasonId": 1
  }
}
```

For Yellow, use `GameDataPath` like `wwwroot/data/yellow/data`. Decide default `Enabled` in the plan. If `EnableShop` remains deferred, avoid enabling shop validation prematurely.

**Program.cs imports and enabled-era parsing:** `Host/Program.cs` lines 6-11, 73-86
```csharp
using TaikoLocalServer.Adapters.GameProtocol.Blue;
using TaikoLocalServer.Adapters.GameProtocol.Green;
using TaikoLocalServer.Domain.Enums;

var enabledEras = serverSettingsConfig.GetSection("Eras")
    .GetChildren()
    .Where(s => s.GetValue<bool>("Enabled"))
    .Select(s => Enum.Parse<GameEra>(s.Key, ignoreCase: true))
    .ToHashSet();
```

Add Yellow `using` and `GameEra.Yellow` before config includes the key.

**Adapter service registration:** `Host/Program.cs` lines 108-126
```csharp
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, enabledEras);
builder.Services.AddAdminApi(builder.Configuration);
builder.Services.AddAllnetMucha();
if (enabledEras.Contains(GameEra.Green))
{
    builder.Services.AddGameProtocolGreen();
}
if (enabledEras.Contains(GameEra.Blue))
{
    builder.Services.AddGameProtocolBlue();
}
```

Add the Yellow branch beside Green/Blue, not as a Blue/Green variant.

**Application-part filtering:** `Host/Program.cs` lines 128-147
```csharp
builder.Services.AddControllers()
    .AddProtoBufNet()
    .ConfigureApplicationPartManager(apm =>
    {
        if (!enabledEras.Contains(GameEra.Green))
        {
            RemoveApplicationPart(apm, "TaikoLocalServer.Adapters.GameProtocol.Green");
        }
        if (!enabledEras.Contains(GameEra.Blue))
        {
            RemoveApplicationPart(apm, "TaikoLocalServer.Adapters.GameProtocol.Blue");
        }
    });
```

Add the Yellow application-part removal branch with assembly name `TaikoLocalServer.Adapters.GameProtocol.Yellow`.

**Application-part helper:** `Host/Program.cs` lines 261-268
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

Reuse as-is; do not add per-request disabled-era checks in controllers.

**Missing content-type protobuf fallback:** `Host/Program.cs` lines 271-283
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
           || path.StartsWithSegments("/v01r00/chassis", StringComparison.OrdinalIgnoreCase);
}
```

Add Yellow only for the proven exact Yellow `.../chassis` prefix. Avoid broad `/v????` fallback.

### Host and Solution Project References

**Solution analog:** `TaikoLocalServer.slnx` lines 2-17
```xml
<Project Path="Adapters.GameProtocol.Blue/Adapters.GameProtocol.Blue.csproj" />
<Project Path="Adapters.GameProtocol.Green/Adapters.GameProtocol.Green.csproj" />
<Project Path="Adapters.GameProtocol.Shared/Adapters.GameProtocol.Shared.csproj" />
<Project Path="Host/Host.csproj" />
<Project Path="Tests/Tests.csproj" />
```

Add Yellow adjacent to Blue/Green adapter entries.

**Host project analog:** `Host/Host.csproj` lines 42-53
```xml
<ProjectReference Include="..\Adapters.GameProtocol.Blue\Adapters.GameProtocol.Blue.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.Green\Adapters.GameProtocol.Green.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.Shared\Adapters.GameProtocol.Shared.csproj" />
```

Add the Yellow adapter reference.

**Host data content analog:** `Host/Host.csproj` lines 136-154
```xml
<!--Green Game Data-->
<Content Remove="wwwroot\data\green\data\**" />
<_ContentIncludedByDefault Remove="wwwroot\data\green\data\**" />

<!--Blue Game Data-->
<Content Remove="wwwroot\data\blue\data\**" />
<_ContentIncludedByDefault Remove="wwwroot\data\blue\data\**" />
```

Add Yellow raw data exclusion if Phase 12 introduces `Host/wwwroot/data/yellow/data`. Do not add Yellow catalog JSON copy rules unless runtime data is added in a later phase.

**Debug data junction analog:** `Host/Host.csproj` lines 158-164
```xml
<Target Name="CreateGreenGameDataSymlinkForDebug" AfterTargets="Build" Condition="'$(Configuration)'=='Debug' and Exists('$(MSBuildProjectDirectory)\wwwroot\data\green\data')">
  <MakeDir Directories="$(OutDir)wwwroot\data\green" />
  <Exec Command="powershell -NoProfile -ExecutionPolicy Bypass -Command &quot;$link = Join-Path '$(OutDir)' 'wwwroot\data\green\data'; ...&quot;" />
</Target>
<Target Name="CreateBlueGameDataSymlinkForDebug" AfterTargets="Build" Condition="'$(Configuration)'=='Debug' and Exists('$(MSBuildProjectDirectory)\wwwroot\data\blue\data')">
```

If Yellow data root is added, copy this target shape with Yellow path names. Preserve the safe junction replacement logic.

### Yellow Tests

**Test project reference analog:** `Tests/Tests.csproj` lines 18-28
```xml
<ProjectReference Include="..\Adapters.GameProtocol.Shared\Adapters.GameProtocol.Shared.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.WwR08\Adapters.GameProtocol.WwR08.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.Blue\Adapters.GameProtocol.Blue.csproj" />
<ProjectReference Include="..\Adapters.GameProtocol.Green\Adapters.GameProtocol.Green.csproj" />
```

Add Yellow adapter reference so `Tests/Yellow/*` can reflect routes and wire namespaces.

**Era foundation test analog:** `Tests/Blue/BlueEraFoundationTests.cs` lines 8-30
```csharp
[Fact]
public void GameEra_Blue_HasStableNumericValue()
{
    Assert.Equal(2, (int)GameEra.Blue);
    Assert.Equal(GameEra.Blue, Enum.Parse<GameEra>("Blue", ignoreCase: true));
}

[Fact]
public void ShippedServerSettings_DeclaresBlueEnabledSetting()
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile(path)
        .Build();

    var blue = configuration.GetSection("ServerSettings:Eras:Blue");
    Assert.True(blue.Exists());
    Assert.True(blue.GetSection("Enabled").Exists());
}
```

Create Yellow equivalent for stable enum value and parseable shipped settings.

**Wire test analog:** `Tests/Blue/BlueWireGenerationTests.cs` lines 7-23
```csharp
[Fact]
public void BlueWireTypes_ContainBlueOnlyA0Messages()
{
    Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Blue.Wire", typeof(CoinsettingRequest).Namespace);
    Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Blue.Wire", typeof(BattleUserDataRequest).Namespace);
}

[Fact]
public void BluePlayResultRequest_IsDirectRequestShape()
{
    Assert.NotNull(typeof(PlayResultRequest).GetProperty(nameof(PlayResultRequest.Baid)));
    Assert.Null(typeof(PlayResultRequest).GetProperty("PlayresultData"));
}
```

For Yellow, assert namespace is `TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire`, direct request shapes exist, `VsInterface` compiles, and battle DTOs/properties do not exist.

**Route reflection helper:** `Tests/Blue/ProtocolRouteTestHelper.cs` lines 7-43, 61-75
```csharp
public static IEnumerable<RouteInfo> FindPostRoutes(params Assembly[] assemblies)
{
    foreach (var type in assemblies.SelectMany(assembly => assembly.DefinedTypes))
    {
        if (!type.IsClass || type.IsAbstract || !type.Name.EndsWith("Controller", StringComparison.Ordinal))
        {
            continue;
        }

        var postActions = type.DeclaredMethods.Where(HasHttpPostAttribute).ToArray();
        ...
    }
}
```

Reuse this helper from `Tests/Blue` or move/copy only if namespace visibility requires it. Prefer avoiding duplicate reflection logic.

**Route skeleton test analog:** `Tests/Blue/BlueRouteSkeletonTests.cs` lines 5-45, 47-59, 61-70
```csharp
private static readonly string[] ExpectedBlueGameRoutes =
[
    "/v10r03/chassis/initialdatacheck.php",
    "/v10r03/chassis/playresult.php",
    "/v10r03/chassis/battleuserdata.php",
    "/v10r03/chassis/rewardexecution.php"
];

var routes = ProtocolRouteTestHelper.FindPostRoutes(typeof(TaikoLocalServer.Adapters.GameProtocol.Blue.DependencyInjection).Assembly)
    .Where(route => route.Template.StartsWith("/v10r03/chassis", StringComparison.OrdinalIgnoreCase))
    .Select(route => route.Template)
    .Order(StringComparer.Ordinal)
    .ToArray();

Assert.Equal(ExpectedBlueGameRoutes.Order(StringComparer.Ordinal).ToArray(), routes);
```

For Yellow, build expected routes from proto-backed suffixes only after route prefix evidence is resolved. Explicitly exclude startup/version, `getreitai.php` unless proven, and `battleuserdata.php`.

**Thin-controller guard analog:** `Tests/Blue/BlueRouteSkeletonTests.cs` lines 73-104
```csharp
var mediatorBackedControllers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "BaidController.cs",
    "InitialDataCheckController.cs",
    "PlayResultController.cs"
};

foreach (var file in Directory.EnumerateFiles(controllersRoot, "*.cs", SearchOption.AllDirectories))
{
    if (mediatorBackedControllers.Contains(Path.GetFileName(file)))
    {
        continue;
    }

    var source = File.ReadAllText(file);
    Assert.DoesNotContain("Mediator.Send", source, StringComparison.Ordinal);
}
```

For Phase 12 Yellow, the allowed Mediator list should be empty or very small. This prevents accidental runtime behavior.

**Host source-guard analog:** `Tests/Blue/BlueHostProgramSourceTests.cs` lines 5-23
```csharp
Assert.Contains("using TaikoLocalServer.Adapters.GameProtocol.Blue;", source, StringComparison.Ordinal);
Assert.Contains("enabledEras.Contains(GameEra.Blue)", source, StringComparison.Ordinal);
Assert.Contains("builder.Services.AddGameProtocolBlue();", source, StringComparison.Ordinal);
Assert.Contains("RemoveApplicationPart(apm, \"TaikoLocalServer.Adapters.GameProtocol.Blue\");", source, StringComparison.Ordinal);
Assert.Contains("path.StartsWithSegments(\"/v10r03/chassis\", StringComparison.OrdinalIgnoreCase)", source, StringComparison.Ordinal);
Assert.DoesNotContain("path.StartsWithSegments(\"/v10r03\", StringComparison.OrdinalIgnoreCase)", source, StringComparison.Ordinal);
```

Copy as Yellow, using exact route prefix once known. Keep the negative broad-prefix assertion.

**Shared version route analog:** `Tests/Blue/BlueSharedVersionRouteTests.cs` lines 15-40
```csharp
[Theory]
[InlineData("/v01r00/chassis/startupauth.php")]
[InlineData("/v01r00/chassis/verupauth.php")]
[InlineData("/v01r00/chassis/verupcomplete.php")]
public void VersionRoutes_AreOwnedBySharedProtocolAdapter(string routeTemplate)
{
    var routes = ProtocolRouteTestHelper.FindPostRoutes(typeof(BaseProtocolController<>).Assembly)
        .Where(route => route.Template == routeTemplate)
        .ToList();

    var route = Assert.Single(routes);
    Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Shared", route.AssemblyName);
}
```

Create Yellow equivalent and assert Yellow adapter does not own shared startup/version routes under the Yellow game prefix.

**Wire compatibility analog:** `Tests/Blue/BlueSharedVersionRouteTests.cs` lines 42-74, 110-120
```csharp
var shared = Deserialize<SharedVerupAuthRequest>(Serialize(request));

Assert.Equal("chassis", shared.ChassisId);
Assert.Equal(123u, shared.HddVer);

private static byte[] Serialize<T>(T value)
{
    using var stream = new MemoryStream();
    ProtoBuf.Serializer.Serialize(stream, value);
    return stream.ToArray();
}
```

Use Yellow `VsInterface` aliases instead of Green aliases. This proves shared startup/version DTO compatibility without changing route ownership.

**Startup route analog:** `Tests/Green/StartupAuthRouteTests.cs` lines 20-45, 47-70
```csharp
public void StartupAuthRoute_IsOwnedBySharedProtocolAdapter()
{
    var routes = FindPostRoutes(
            typeof(BaseProtocolController<>).Assembly,
            typeof(TaikoLocalServer.Adapters.GameProtocol.Green.DependencyInjection).Assembly)
        .Where(route => route.Template == "/v01r00/chassis/startupauth.php")
        .ToList();

    var route = Assert.Single(routes);
    Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Shared", route.AssemblyName);
}
```

Use for Yellow startup route ownership plus request/response serialization compatibility.

## Shared Patterns

### Controller Contract

Apply to every Yellow controller:
- `[ApiController]`
- absolute `[Route("...")]`
- `[HttpPost]`
- `[Produces("application/protobuf")]`
- `[FromBody]` generated Yellow wire request
- log request
- return generated Yellow wire response
- call Mediator only where Phase 12 explicitly approves an existing handler path

### Era Separation

Yellow adapter code should use Yellow namespaces, `GameEra.Yellow`, and Yellow wire DTOs. It must not import Blue adapter, Blue battle, Green adapter, Green AI battle, or persistence types as behavior truth.

### Route Evidence

Endpoint suffixes are proto-backed; the Yellow game route prefix is not proven in current phase inputs. Pattern map consumers should require the evidence artifact to state whether the prefix was supplied by logs, binary strings, or user-approved evidence before controllers/Host fallback hardcode it.

### Disabled-Era Exposure

The Host pattern is build-time assembly reference plus runtime application-part removal. Do not put enabled-era checks inside Yellow controllers.

### Generated Code

Generated `Wire/` files are created from `proto/yellow/*.proto` and left alone. Tests should assert namespace and shape rather than manual cleanup.

### No-Battle Guardrails

Yellow no-battle tests should combine:
- proto/source checks for absence of `BattleUserData*` and battle fields
- route reflection absence for `battleuserdata.php`
- source guards across Yellow adapter/Application/Domain/Infrastructure/migrations for no Yellow battle entities/tables and no Blue battle fallback

## No Analog Found

None. Every Phase 12 file category has a direct Blue/Green/Shared/Host/test analog. The only unresolved item is not a missing code pattern; it is the Yellow client evidence gap for the game route prefix and real HTTP framing.

## Metadata

**Analog search scope:** `Adapters.GameProtocol.Blue/`, `Adapters.GameProtocol.Green/`, `Adapters.GameProtocol.Shared/`, `Host/`, `Domain/`, `Application/Settings/`, `Tests/Blue/`, `Tests/Green/`, `Tests/Ac15/`, `proto/yellow/`, `proto/blue/`, `proto/green/`  
**Files scanned:** 70+ targeted source/test/config/proto files via file lists and focused excerpts  
**Pattern extraction date:** 2026-06-07  
**Write scope:** only `.planning/phases/12-yellow-evidence-and-era-foundation/12-PATTERNS.md`
