# Phase 39: MOMOIRO Evidence and Era Foundation - Pattern Map

**Mapped:** 2026-06-26
**Files analyzed:** 30
**Analogs found:** 29 / 30

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|-------------------|------|-----------|----------------|---------------|
| `.planning/phases/39-momoiro-evidence-and-era-foundation/39-MOMOIRO-EVIDENCE.md` | config/docs | batch evidence review | `.planning/milestones/v1.4-phases/23-white-evidence-and-era-foundation/23-WHITE-EVIDENCE.md` | role-match |
| `Domain/Enums/GameEra.cs` | model/config | config transform | `Domain/Enums/GameEra.cs` | exact existing-file extension |
| `Host/Program.cs` | config/composition | request-response | `Host/Program.cs` Kimidori/Murasaki branches | exact existing-file extension |
| `Host/Host.csproj` | config | build graph | `Host/Host.csproj` Kimidori reference | exact existing-file extension |
| `TaikoLocalServer.slnx` | config | build graph | `TaikoLocalServer.slnx` Kimidori project entry | exact existing-file extension |
| `Host/Configurations/ServerSettings.json` | config | config load | `Host/Configurations/ServerSettings.json` Kimidori/Murasaki entries | exact existing-file extension |
| `Adapters.GameProtocol.Shared/GameProtocolApplicationParts.cs` | config/gating | request-response | `Adapters.GameProtocol.Shared/GameProtocolApplicationParts.cs` Kimidori removal branch | exact existing-file extension |
| `Adapters.GameProtocol.Momoiro/Adapters.GameProtocol.Momoiro.csproj` | config | build graph | `Adapters.GameProtocol.Kimidori/Adapters.GameProtocol.Kimidori.csproj` | exact |
| `Adapters.GameProtocol.Momoiro/DependencyInjection.cs` | service/config | dependency injection | `Adapters.GameProtocol.Kimidori/DependencyInjection.cs` | exact |
| `Adapters.GameProtocol.Momoiro/MomoiroRoutePrefixes.cs` | config | request-response | `Adapters.GameProtocol.Kimidori/KimidoriRoutePrefixes.cs` | exact |
| `Adapters.GameProtocol.Momoiro/MomoiroAdapterMarker.cs` | config | application-part discovery | `Adapters.GameProtocol.Kimidori/KimidoriAdapterMarker.cs` | exact |
| `Adapters.GameProtocol.Momoiro/MapperlyDefaults.cs` | config | transform | `Adapters.GameProtocol.Kimidori/MapperlyDefaults.cs` | exact |
| `Adapters.GameProtocol.Momoiro/GlobalUsings.cs` | config | build/imports | `Adapters.GameProtocol.Kimidori/GlobalUsings.cs` | exact |
| `Adapters.GameProtocol.Momoiro/Wire/Game.cs` | generated model | request-response transform | `Adapters.GameProtocol.Kimidori/Wire/Game.cs` header | exact generated-file pattern |
| `Adapters.GameProtocol.Momoiro/Wire/VsInterface.cs` | generated model | request-response transform | `Adapters.GameProtocol.Kimidori/Wire/VsInterface.cs` header | exact generated-file pattern |
| `Adapters.GameProtocol.Momoiro/Controllers/BaidController.cs` | controller | request-response | `Adapters.GameProtocol.Kimidori/Controllers/BaidController.cs` | role-match; behavior must be reduced/gated |
| `Adapters.GameProtocol.Momoiro/Controllers/MyDonEntryController.cs` | controller | request-response | `Adapters.GameProtocol.Kimidori/Controllers/MyDonEntryController.cs` | role-match; behavior must be reduced/gated |
| `Adapters.GameProtocol.Momoiro/Controllers/UserDataController.cs` | controller | request-response | `Adapters.GameProtocol.Kimidori/Controllers/UserDataController.cs` | role-match; behavior must be reduced/gated |
| `Adapters.GameProtocol.Momoiro/Controllers/PlayResultController.cs` | controller | request-response | `Adapters.GameProtocol.Kimidori/Controllers/PlayResultController.cs` | role-match; behavior must be reduced/gated |
| `Adapters.GameProtocol.Momoiro/Controllers/SelfBestController.cs` | controller | request-response | `Adapters.GameProtocol.Kimidori/Controllers/SelfBestController.cs` | role-match; behavior must be reduced/gated |
| `Adapters.GameProtocol.Momoiro/Controllers/RecommendController.cs` | controller | request-response | `Adapters.GameProtocol.Kimidori/Controllers/RecommendController.cs` | role-match; behavior must be reduced/gated |
| `Adapters.GameProtocol.Momoiro/Controllers/HeartbeatController.cs` | controller | request-response | `Adapters.GameProtocol.Kimidori/Controllers/HeartbeatController.cs` | exact structural |
| `Adapters.GameProtocol.Momoiro/Controllers/DefaultSongController.cs` | controller | request-response | `Adapters.GameProtocol.Kimidori/Controllers/DefaultSongController.cs` | role-match; catalog behavior deferred |
| `Adapters.GameProtocol.Momoiro/Controllers/BookkeepingController.cs` | controller | request-response | `Adapters.GameProtocol.Kimidori/Controllers/BookkeepingController.cs` | exact structural |
| `Adapters.GameProtocol.Momoiro/Controllers/SongHashController.cs` | controller | request-response | `Adapters.GameProtocol.Kimidori/Controllers/SongHashController.cs` | role-match; catalog behavior deferred |
| `Adapters.GameProtocol.Momoiro/Controllers/TelopCheckController.cs` | controller | request-response | `Adapters.GameProtocol.Kimidori/Controllers/TelopCheckController.cs` | role-match; catalog behavior deferred |
| `Adapters.GameProtocol.Momoiro/Controllers/GetTelopController.cs` | controller | request-response | `Adapters.GameProtocol.Kimidori/Controllers/GetTelopController.cs` | role-match; catalog behavior deferred |
| `Tests/Momoiro/MomoiroApplicationPartTests.cs` | test | request-response gating | `Adapters.GameProtocol.Shared/GameProtocolApplicationParts.cs` plus no exact test analog | partial |
| `Tests/Momoiro/MomoiroServerSettingsValidationTests.cs` | test | config validation | `Tests/White/WhiteServerSettingsValidationTests.cs` | exact |
| `Tests/Tests.csproj` | config/test | build graph | `Tests/Tests.csproj` adapter references | exact existing-file extension |

## Pattern Assignments

### `.planning/phases/39-momoiro-evidence-and-era-foundation/39-MOMOIRO-EVIDENCE.md` (config/docs, batch evidence review)

**Analog:** `.planning/milestones/v1.4-phases/23-white-evidence-and-era-foundation/23-WHITE-EVIDENCE.md`

**Evidence artifact shape** (lines 1-16):
```markdown
# Phase 23 White Evidence Gate

**Created:** 2026-06-17
**Updated:** 2026-06-17 after route-proof checkpoint continuation
**Approval:** Task 3 human approval recorded on 2026-06-17 from user response `approved`.
**Scope:** White AC15 0.13 route, startup/version, transport, data-root, and feature-surface evidence before route/controller implementation.

## IDA Backend Evidence

| Artifact | Evidence | Status |
|----------|----------|--------|
| IDA target | `.tools/white/EBOOT.ELF.i64` | `AVAILABLE` |
| Backend probe | `database_opened=true`, `ida_available=true` | `AVAILABLE` |
| Live filesystem size | `129893515` bytes | `AVAILABLE` |
```

**Route-prefix and allowlist pattern** (lines 18-31):
```markdown
## Route Prefix Decision

| Question | Status | Evidence Source | Decision |
|----------|--------|-----------------|----------|
| Exact White game route prefix | `PROVEN_ROUTE_PREFIX` | White IDA daemon target `.tools/white/EBOOT.ELF.i64`: `v07r00` string at `0xc38468`; big-endian pointer at `0xcb0d98` in the same service table as the `v01r00` entry. Nearby table entries include `_uninitialized`, `_initialized`, `_nic_activated`, `_wait_terminate`, `_terminated`, `ResidentServiceThread`, `DelayServiceThread`, `PriorityServiceThread`, and `VersionupServiceThread`. | `/v07r00/chassis/{suffix}` is the proven White game route prefix for the Phase 23 no-state scaffold allowlist. Route code still waits for the Task 3 human approval checkpoint. |
| Shared startup/version route prefix | `PROVEN_SHARED_SERVICE_PREFIX` | White IDA daemon target `.tools/white/EBOOT.ELF.i64`: `v01r00` string at `0xc38470`; big-endian pointer at `0xcb0d9c` in the same service table. | Keep startup/version ownership under shared `/v01r00/chassis/*`. White Plan 03 must not duplicate these routes in the White adapter. |

## Route Suffix Gate
```

**Apply to MOMOIRO:** create a MOMOIRO evidence matrix with source availability, route-prefix decision, a route suffix allowlist for the 12 locked `/v04r00/chassis/*.php` game routes, and blocked/proto-only rows for `shoppingresult.php`, `bestscore.php`, `communicationlog.php`, and `mainichisong.php`. Do not claim fresh IDA offsets unless implementation actually captures them; the current phase context accepts the supplied inventory.

### `Domain/Enums/GameEra.cs` (model/config, config transform)

**Analog:** `Domain/Enums/GameEra.cs`

**Enum extension pattern** (lines 3-12):
```csharp
public enum GameEra
{
    Nijiiro = 0,
    Green = 1,
    Blue = 2,
    Yellow = 3,
    Red = 4,
    White = 5,
    Murasaki = 6,
    Kimidori = 7
}
```

**Apply to MOMOIRO:** add `Momoiro` as a new first-class value after `Kimidori`. Do not reuse another era value.

### `Host/Program.cs` (config/composition, request-response)

**Analog:** `Host/Program.cs`

**Imports pattern** (lines 6-14):
```csharp
using TaikoLocalServer.Adapters.GameProtocol.Blue;
using TaikoLocalServer.Adapters.GameProtocol.CnR00;
using TaikoLocalServer.Adapters.GameProtocol.Green;
using TaikoLocalServer.Adapters.GameProtocol.Kimidori;
using TaikoLocalServer.Adapters.GameProtocol.Murasaki;
using TaikoLocalServer.Adapters.GameProtocol.Red;
using TaikoLocalServer.Adapters.GameProtocol.White;
using TaikoLocalServer.Adapters.GameProtocol.WwR08;
using TaikoLocalServer.Adapters.GameProtocol.Yellow;
```

**Enabled-era startup guard** (lines 78-87):
```csharp
var serverSettingsConfig = builder.Configuration.GetSection("ServerSettings");
var enabledEras = GameProtocolApplicationParts.ReadEnabledEras(serverSettingsConfig);

if (enabledEras.Count == 0)
{
    Log.Fatal("ServerSettings.Eras has no enabled era. At least one era (Nijiiro, Green, Blue, Yellow, Red, White, Murasaki, or Kimidori) must be enabled in Host/Configurations/ServerSettings.json. Refusing to start.");
    throw new InvalidOperationException("No game eras enabled.");
}

Log.Information("Enabled game eras: {Eras}", string.Join(", ", enabledEras));
```

**Adapter DI registration pattern** (lines 140-147):
```csharp
if (enabledEras.Contains(GameEra.Murasaki))
{
    builder.Services.AddGameProtocolMurasaki();
}
if (enabledEras.Contains(GameEra.Kimidori))
{
    builder.Services.AddGameProtocolKimidori();
}
```

**Application-part gating pattern** (lines 149-156):
```csharp
builder.Services.AddControllers()
    .AddProtoBufNet()
    .ConfigureApplicationPartManager(apm =>
    {
        // Adapter assemblies referenced by Host are auto-discovered as ApplicationParts.
        // Remove disabled-era assemblies so their controllers are not routed.
        GameProtocolApplicationParts.RemoveDisabledGameProtocolApplicationParts(apm, enabledEras);
    });
```

**Direct-protobuf fallback pattern** (lines 271-292):
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
           || path.StartsWithSegments(WhiteRoutePrefixes.Final, StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments(WhiteRoutePrefixes.Compatibility, StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments(MurasakiRoutePrefixes.Final, StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments(MurasakiRoutePrefixes.Compatibility, StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments(KimidoriRoutePrefixes.Game, StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/v01r00/chassis", StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/v01r00_tw/chassis", StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/v12r08_ww/chassis", StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/v12r00_cn/chassis", StringComparison.OrdinalIgnoreCase);
}
```

**Apply to MOMOIRO:** import `TaikoLocalServer.Adapters.GameProtocol.Momoiro`, include `Momoiro` in the startup error text, add `builder.Services.AddGameProtocolMomoiro()` when enabled, and add `MomoiroRoutePrefixes.Game` to `ShouldAssumeProtobufRequest`.

### `Host/Host.csproj` (config, build graph)

**Analog:** `Host/Host.csproj`

**Project reference pattern** (lines 42-58):
```xml
<ItemGroup>
  <ProjectReference Include="..\Adapters.AdminApi\Adapters.AdminApi.csproj" />
  <ProjectReference Include="..\Adapters.AllnetMucha\Adapters.AllnetMucha.csproj" />
  <ProjectReference Include="..\Adapters.GameProtocol.Blue\Adapters.GameProtocol.Blue.csproj" />
  <ProjectReference Include="..\Adapters.GameProtocol.CnR00\Adapters.GameProtocol.CnR00.csproj" />
  <ProjectReference Include="..\Adapters.GameProtocol.Green\Adapters.GameProtocol.Green.csproj" />
  <ProjectReference Include="..\Adapters.GameProtocol.Kimidori\Adapters.GameProtocol.Kimidori.csproj" />
  <ProjectReference Include="..\Adapters.GameProtocol.Murasaki\Adapters.GameProtocol.Murasaki.csproj" />
  <ProjectReference Include="..\Adapters.GameProtocol.Red\Adapters.GameProtocol.Red.csproj" />
  <ProjectReference Include="..\Adapters.GameProtocol.Shared\Adapters.GameProtocol.Shared.csproj" />
  <ProjectReference Include="..\Adapters.GameProtocol.White\Adapters.GameProtocol.White.csproj" />
  <ProjectReference Include="..\Adapters.GameProtocol.WwR08\Adapters.GameProtocol.WwR08.csproj" />
  <ProjectReference Include="..\Adapters.GameProtocol.Yellow\Adapters.GameProtocol.Yellow.csproj" />
  <ProjectReference Include="..\Application\Application.csproj" />
  <ProjectReference Include="..\Domain\Domain.csproj" />
  <ProjectReference Include="..\Infrastructure\Infrastructure.csproj" />
  <ProjectReference Include="..\TaikoWebUI\TaikoWebUI.csproj" />
</ItemGroup>
```

**No-touch data-content boundary** (lines 194-199):
```xml
<!--Kimidori Game Data-->
<Content Remove="wwwroot\data\kimidori\data\**" />
<_ContentIncludedByDefault Remove="wwwroot\data\kimidori\data\**" />
<Content Update="wwwroot\data\kimidori\kimidori_telop_data.json"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></Content>
<Content Update="wwwroot\data\kimidori\kimidori_movie_data.json"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></Content>
<Content Update="wwwroot\data\kimidori\kimidori_event_folder_data.json"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></Content>
```

**Debug data-junction pattern** (lines 227-230):
```xml
<Target Name="CreateKimidoriGameDataSymlinkForDebug" AfterTargets="Build" Condition="'$(Configuration)'=='Debug' and Exists('$(MSBuildProjectDirectory)\wwwroot\data\kimidori\data')">
  <MakeDir Directories="$(OutDir)wwwroot\data\kimidori" />
  <Exec Command="powershell -NoProfile -ExecutionPolicy Bypass -Command &quot;$link = Join-Path '$(OutDir)' 'wwwroot\data\kimidori\data'; $target = Resolve-Path '$(MSBuildProjectDirectory)\wwwroot\data\kimidori\data'; if (Test-Path -LiteralPath $link) { $item = Get-Item -LiteralPath $link -Force; if (($item.LinkType -eq 'SymbolicLink' -or $item.LinkType -eq 'Junction') -and $item.Target -contains $target.Path) { exit 0 }; Remove-Item -LiteralPath $link -Force -Recurse }; New-Item -ItemType Junction -Path $link -Target $target.Path | Out-Null&quot;" />
</Target>
```

**Apply to MOMOIRO:** add the Momoiro adapter project reference. Do not add runtime data sidecar JSON content in Phase 39 unless the plan explicitly needs only `Content Remove` / debug data junction for local raw operator data; catalog sidecar files belong to Phase 40.

### `TaikoLocalServer.slnx` (config, build graph)

**Analog:** `TaikoLocalServer.slnx`

**Solution project list pattern** (lines 1-13):
```xml
<Solution>
  <Project Path="Adapters.AdminApi/Adapters.AdminApi.csproj" />
  <Project Path="Adapters.AllnetMucha/Adapters.AllnetMucha.csproj" />
  <Project Path="Adapters.GameProtocol.CnR00/Adapters.GameProtocol.CnR00.csproj" />
  <Project Path="Adapters.GameProtocol.Blue/Adapters.GameProtocol.Blue.csproj" />
  <Project Path="Adapters.GameProtocol.Green/Adapters.GameProtocol.Green.csproj" />
  <Project Path="Adapters.GameProtocol.Kimidori/Adapters.GameProtocol.Kimidori.csproj" />
  <Project Path="Adapters.GameProtocol.Murasaki/Adapters.GameProtocol.Murasaki.csproj" />
  <Project Path="Adapters.GameProtocol.Shared/Adapters.GameProtocol.Shared.csproj" />
  <Project Path="Adapters.GameProtocol.WwR08/Adapters.GameProtocol.WwR08.csproj" />
  <Project Path="Adapters.GameProtocol.Yellow/Adapters.GameProtocol.Yellow.csproj" />
  <Project Path="Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj" />
  <Project Path="Adapters.GameProtocol.White/Adapters.GameProtocol.White.csproj" />
```

**Apply to MOMOIRO:** add `Adapters.GameProtocol.Momoiro/Adapters.GameProtocol.Momoiro.csproj` near the other game protocol adapters.

### `Host/Configurations/ServerSettings.json` (config, config load)

**Analog:** `Host/Configurations/ServerSettings.json`

**Older-AC15 root-era settings pattern** (lines 51-62):
```json
"Murasaki": {
  "Enabled": true,
  "AutoExtractCatalog": true,
  "GameDataPath": "wwwroot/data/murasaki/data",
  "CustomizationNameDataPath": ""
},
"Kimidori": {
  "Enabled": true,
  "AutoExtractCatalog": true,
  "GameDataPath": "wwwroot/data/kimidori/data",
  "CustomizationNameDataPath": ""
}
```

**Apply to MOMOIRO:** add a `Momoiro` era settings block with `GameDataPath` under `wwwroot/data/momoiro/data`. Do not add `EnableShop`, `ActiveShopSeasonId`, `EnableDonChallenge`, or AdminApi/WebUI settings in Phase 39.

### `Adapters.GameProtocol.Shared/GameProtocolApplicationParts.cs` (config/gating, request-response)

**Analog:** `Adapters.GameProtocol.Shared/GameProtocolApplicationParts.cs`

**Enabled-era read pattern** (lines 9-14):
```csharp
public static HashSet<GameEra> ReadEnabledEras(IConfiguration serverSettingsConfig)
    => serverSettingsConfig.GetSection("Eras")
        .GetChildren()
        .Where(s => s.GetValue<bool>("Enabled"))
        .Select(s => Enum.Parse<GameEra>(s.Key, ignoreCase: true))
        .ToHashSet();
```

**Disabled adapter removal pattern** (lines 40-47):
```csharp
if (!enabledEras.Contains(GameEra.Murasaki))
{
    RemoveApplicationPart(apm, "TaikoLocalServer.Adapters.GameProtocol.Murasaki");
}
if (!enabledEras.Contains(GameEra.Kimidori))
{
    RemoveApplicationPart(apm, "TaikoLocalServer.Adapters.GameProtocol.Kimidori");
}
```

**Removal helper pattern** (lines 55-63):
```csharp
private static void RemoveApplicationPart(ApplicationPartManager apm, string assemblyName)
{
    var part = apm.ApplicationParts.FirstOrDefault(p =>
        p is AssemblyPart a && a.Assembly.GetName().Name == assemblyName);
    if (part is not null)
    {
        apm.ApplicationParts.Remove(part);
    }
}
```

**Apply to MOMOIRO:** add a disabled-era removal branch for `GameEra.Momoiro` and assembly name `TaikoLocalServer.Adapters.GameProtocol.Momoiro`. Do not add per-controller enablement checks.

### `Adapters.GameProtocol.Momoiro/Adapters.GameProtocol.Momoiro.csproj` (config, build graph)

**Analog:** `Adapters.GameProtocol.Kimidori/Adapters.GameProtocol.Kimidori.csproj`

**Adapter project pattern** (lines 1-23):
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <RootNamespace>TaikoLocalServer.Adapters.GameProtocol.Kimidori</RootNamespace>
    <AssemblyName>TaikoLocalServer.Adapters.GameProtocol.Kimidori</AssemblyName>
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

**Apply to MOMOIRO:** copy the project structure and replace namespace/assembly names with `Momoiro`. Keep adapter dependencies limited to Shared/Application/Contracts and protobuf-net/Mapperly.

### Adapter Skeleton Files (config/service/import patterns)

**Analogs:** KIMIDORI adapter skeleton files.

**DI extension pattern** from `Adapters.GameProtocol.Kimidori/DependencyInjection.cs` (lines 5-9):
```csharp
public static class DependencyInjection
{
    public const GameEra Era = GameEra.Kimidori;

    public static IServiceCollection AddGameProtocolKimidori(this IServiceCollection services) => services;
}
```

**Route prefix pattern** from `Adapters.GameProtocol.Kimidori/KimidoriRoutePrefixes.cs` (lines 3-6):
```csharp
public static class KimidoriRoutePrefixes
{
    public const string Game = "/v05r00/chassis";
}
```

**Marker pattern** from `Adapters.GameProtocol.Kimidori/KimidoriAdapterMarker.cs` (lines 1-3):
```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori;

public sealed class KimidoriAdapterMarker;
```

**Mapperly defaults pattern** from `Adapters.GameProtocol.Kimidori/MapperlyDefaults.cs` (lines 1-6):
```csharp
using Riok.Mapperly.Abstractions;

[assembly: MapperDefaults(
    AutoUserMappings = false,
    EnumMappingStrategy = EnumMappingStrategy.ByName,
    RequiredMappingStrategy = RequiredMappingStrategy.Target)]
```

**Global usings pattern** from `Adapters.GameProtocol.Kimidori/GlobalUsings.cs` (lines 1-16):
```csharp
global using Mediator;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Logging;
global using ProtoBuf;
global using TaikoLocalServer.Adapters.GameProtocol.Kimidori;
global using TaikoLocalServer.Adapters.GameProtocol.Kimidori.Mappers;
global using TaikoLocalServer.Adapters.GameProtocol.Kimidori.Wire;
global using TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;
global using TaikoLocalServer.Application.Abstractions;
global using TaikoLocalServer.Application.Ac15;
global using TaikoLocalServer.Application.Common;
global using TaikoLocalServer.Application.Dtos;
global using TaikoLocalServer.Application.Dtos.Ac15;
global using TaikoLocalServer.Application.Handlers;
global using TaikoLocalServer.Application.ServerData;
global using TaikoLocalServer.Domain.Enums;
```

**Apply to MOMOIRO:** create the same files with `Momoiro` names and `/v04r00/chassis`. If no Momoiro mappers are created in Phase 39, omit or delay the `.Mappers` global using until needed.

### `Adapters.GameProtocol.Momoiro/Wire/Game.cs` and `Wire/VsInterface.cs` (generated model, request-response transform)

**Analogs:** `Adapters.GameProtocol.Kimidori/Wire/Game.cs`, `Adapters.GameProtocol.Kimidori/Wire/VsInterface.cs`

**Generated header pattern** from `Wire/Game.cs` (lines 1-10):
```csharp
// <auto-generated>
//   This file was generated by a tool; you should avoid making direct changes.
//   Consider using 'partial classes' to extend these types
//   Input: taiko.proto
// </auto-generated>

#region Designer generated code
#pragma warning disable CS0612, CS0618, CS1591, CS3021, CS8981, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Wire
{
```

**VsInterface generated header pattern** from `Wire/VsInterface.cs` (lines 1-10):
```csharp
// <auto-generated>
//   This file was generated by a tool; you should avoid making direct changes.
//   Consider using 'partial classes' to extend these types
//   Input: vsinterface.proto
// </auto-generated>

#region Designer generated code
#pragma warning disable CS0612, CS0618, CS1591, CS3021, CS8981, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Wire
{
```

**MOMOIRO proto inventory evidence** from `proto/momoiro`:
```text
proto/momoiro\vsinterface.proto:3:message StartupAuthRequest {
proto/momoiro\vsinterface.proto:19:message StartupAuthResponse {
proto/momoiro\vsinterface.proto:35:message VerupAuthRequest {
proto/momoiro\vsinterface.proto:45:message VerupAuthResponse {
proto/momoiro\vsinterface.proto:49:message VerupCompleteRequest {
proto/momoiro\vsinterface.proto:59:message VerupCompleteResponse {
proto/momoiro\taiko.proto:3:message BAIDRequest {
proto/momoiro\taiko.proto:54:message BestScoreRequest {
proto/momoiro\taiko.proto:79:message BookKeepingRequest {
proto/momoiro\taiko.proto:96:message CommunicationLogRequest {
proto/momoiro\taiko.proto:112:message DefaultsongRequest {
proto/momoiro\taiko.proto:134:message HeartBeatRequest {
proto/momoiro\taiko.proto:144:message MainichisongRequest {
proto/momoiro\taiko.proto:155:message MydonEntryRequest {
proto/momoiro\taiko.proto:183:message PlayResultRequest {
proto/momoiro\taiko.proto:280:message RecommendRequest {
proto/momoiro\taiko.proto:292:message SelfBestRequest {
proto/momoiro\taiko.proto:313:message ShoppingResultRequest {
proto/momoiro\taiko.proto:339:message SonghashRequest {
proto/momoiro\taiko.proto:358:message UserDataRequest {
```

**Apply to MOMOIRO:** generate adapter-local wire files from `proto/momoiro/taiko.proto` and `proto/momoiro/vsinterface.proto`, set namespace to `TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire`, and do not manually clean generated DTOs beyond deliberate namespace/file-name normalization. Proto message presence is not route authority.

### Controller Files (controller, request-response)

**Shared controller base analog:** `Adapters.GameProtocol.Shared/Controllers/BaseProtocolController.cs`

**Base controller pattern** (lines 3-11):
```csharp
public abstract class BaseProtocolController<T> : ControllerBase where T : BaseProtocolController<T>
{
    private ILogger<T>? logger;

    private IMediator? mediator;

    protected IMediator Mediator => (mediator ??= HttpContext.RequestServices.GetService<IMediator>()) ?? throw new InvalidOperationException();

    protected ILogger<T> Logger => (logger ??= HttpContext.RequestServices.GetService<ILogger<T>>()) ?? throw new InvalidOperationException();
}
```

**Static operational endpoint pattern** from `HeartbeatController.cs` (lines 3-18):
```csharp
[ApiController]
public sealed class HeartbeatController : BaseProtocolController<HeartbeatController>
{
    [HttpPost(KimidoriRoutePrefixes.Game + "/heartbeat.php")]
    [Produces("application/protobuf")]
    public IActionResult Heartbeat([FromBody] HeartBeatRequest request)
    {
        Logger.LogInformation("Kimidori heartbeat.php request: {@Request}", request);
        return Ok(new HeartBeatResponse
        {
            Result = 1,
            ComSvrStat = 1,
            GameSvrStat = 1
        });
    }
}
```

**Bookkeeping static endpoint pattern** from `BookkeepingController.cs` (lines 3-15):
```csharp
[ApiController]
public sealed class BookkeepingController : BaseProtocolController<BookkeepingController>
{
    [HttpPost(KimidoriRoutePrefixes.Game + "/bookkeeping.php")]
    [Produces("application/protobuf")]
    public IActionResult Bookkeeping([FromBody] BookKeepingRequest request)
    {
        Logger.LogInformation(
            "Kimidori bookkeeping.php request: ChassisId={ChassisId}, ShopId={ShopId}",
            request.ChassisId,
            request.ShopId);
        return Ok(new BookKeepingResponse { Result = 1 });
    }
}
```

**Mediator + mapper endpoint pattern** from `SelfBestController.cs` (lines 3-15):
```csharp
[ApiController]
public sealed class SelfBestController : BaseProtocolController<SelfBestController>
{
    [HttpPost(KimidoriRoutePrefixes.Game + "/selfbest.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> SelfBest([FromBody] SelfBestRequest request)
    {
        Logger.LogInformation("Kimidori SelfBest request: {@Request}", request);
        var common = await Mediator.Send(
            new GetSelfBestQuery(request.Baid, GameEra.Kimidori, request.Level, request.ArySongNoes ?? []),
            HttpContext.RequestAborted);
        return Ok(SelfBestMappers.Map(common));
    }
}
```

**PlayResult route pattern** from `PlayResultController.cs` (lines 3-16):
```csharp
[ApiController]
public sealed class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost(KimidoriRoutePrefixes.Game + "/playresult.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("Kimidori PlayResult request: {@Request}", request);
        var playResult = PlayResultMappers.Map(request);
        var result = await Mediator.Send(
            new UpdateAc15PlayResultCommand(request.Baid, GameEra.Kimidori, playResult),
            HttpContext.RequestAborted);
        return Ok(PlayResultMappers.Map(result));
    }
}
```

**Murasaki multi-prefix caution** from `Murasaki/Controllers/HeartbeatController.cs` (lines 6-8):
```csharp
[HttpPost(MurasakiRoutePrefixes.Final + "/heartbeat.php")]
[HttpPost(MurasakiRoutePrefixes.Compatibility + "/heartbeat.php")]
[Produces("application/protobuf")]
```

**Apply to MOMOIRO controllers:** use one route attribute per binary-proven game route with `MomoiroRoutePrefixes.Game + "/<suffix>.php"`. Do not add Murasaki-style compatibility/final dual attributes. Do not create startup/version controllers in the Momoiro adapter. For Phase 39, copying KIMIDORI controller shape is acceptable, but copying KIMIDORI stateful runtime behavior is not evidence; any mediator-backed behavior must be explicitly no-state or deferred by the plan.

**Per-controller analog mapping:**

| MOMOIRO Controller | Copy Structural Pattern From | Phase 39 Boundary |
|--------------------|------------------------------|-------------------|
| `HeartbeatController.cs` | `Adapters.GameProtocol.Kimidori/Controllers/HeartbeatController.cs` lines 3-18 | Static success shape is the closest safe route-owned scaffold. |
| `BookkeepingController.cs` | `Adapters.GameProtocol.Kimidori/Controllers/BookkeepingController.cs` lines 3-15 | Log and return success only; no accounting persistence. |
| `BaidController.cs` | `Adapters.GameProtocol.Kimidori/Controllers/BaidController.cs` lines 3-42 | Thin controller shape only; do not introduce Momoiro persistence/profile creation in Phase 39. |
| `MyDonEntryController.cs` | `Adapters.GameProtocol.Kimidori/Controllers/MyDonEntryController.cs` lines 3-31 | Thin route shape only; no Momoiro save table in Phase 39. |
| `UserDataController.cs` | `Adapters.GameProtocol.Kimidori/Controllers/UserDataController.cs` lines 3-38 | Thin route shape only; readback state belongs to Phase 41. |
| `PlayResultController.cs` | `Adapters.GameProtocol.Kimidori/Controllers/PlayResultController.cs` lines 3-16 | Route shape only; playresult mutation belongs to Phase 42. |
| `SelfBestController.cs` | `Adapters.GameProtocol.Kimidori/Controllers/SelfBestController.cs` lines 3-15 | Route shape only; self-best readback belongs to Phase 41. |
| `RecommendController.cs` | `Adapters.GameProtocol.Kimidori/Controllers/RecommendController.cs` lines 3-15 | Route shape only; recommendation semantics belong to Phase 40. |
| `DefaultSongController.cs` | `Adapters.GameProtocol.Kimidori/Controllers/DefaultSongController.cs` lines 3-22 | Route shape only; default-song/catalog binding belongs to Phase 40. |
| `SongHashController.cs` | `Adapters.GameProtocol.Kimidori/Controllers/SongHashController.cs` lines 3-19 | Route shape only; song-hash table behavior belongs to Phase 40. |
| `TelopCheckController.cs` | `Adapters.GameProtocol.Kimidori/Controllers/TelopCheckController.cs` lines 3-17 | Route shape only; telop catalog behavior belongs to Phase 40. |
| `GetTelopController.cs` | `Adapters.GameProtocol.Kimidori/Controllers/GetTelopController.cs` lines 3-15 | Route shape only; telop content behavior belongs to Phase 40. |

### Mapper Patterns If Needed (utility/transform)

**Analogs:** KIMIDORI mapper classes.

**Simple response mapper** from `RecommendMappers.cs` (lines 5-9):
```csharp
[Mapper]
public static partial class RecommendMappers
{
    [MapProperty(nameof(CommonRecommendResponse.RecommendBestSong), nameof(RecommendResponse.RecommendBestSongs))]
    public static partial RecommendResponse Map(CommonRecommendResponse common);
}
```

**Target-apply mapper** from `UserDataMappers.cs` (lines 6-15):
```csharp
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static partial class UserDataMappers
{
    [MapProperty(nameof(Ac15UserDataSongFlags.ReleaseSongFlg), nameof(UserDataResponse.HashReleaseSongFlg))]
    public static partial void Apply(Ac15UserDataSongFlags source, [MappingTarget] UserDataResponse response);

    public static partial void Apply(Ac15UserDataSongLists source, [MappingTarget] UserDataResponse response);

    [MapProperty(nameof(Ac15UserDataRecommendations.RecommendBestSong), nameof(UserDataResponse.RecommendBestSongs), Use = nameof(MapRecommendBestSongs))]
    public static partial void Apply(Ac15UserDataRecommendations source, [MappingTarget] UserDataResponse response);
```

**PlayResult mapping caution** from `PlayResultMappers.cs` (lines 5-21):
```csharp
[Mapper(
    AllowNullPropertyAssignment = false,
    ThrowOnMappingNullMismatch = false,
    ThrowOnPropertyMappingNullMismatch = false)]
public static partial class PlayResultMappers
{
    [MapPropertyFromSource(nameof(Ac15PlayResultEnvelope.Metadata), Use = nameof(MapMetadata))]
    [MapPropertyFromSource(nameof(Ac15PlayResultEnvelope.Profile), Use = nameof(MapProfile))]
    [MapPropertyFromSource(nameof(Ac15PlayResultEnvelope.Normal), Use = nameof(MapNormal))]
    [MapPropertyFromSource(nameof(Ac15PlayResultEnvelope.Dani), Use = nameof(MapDani))]
    [MapValue(nameof(Ac15PlayResultEnvelope.Tokkun), null)]
    [MapValue(nameof(Ac15PlayResultEnvelope.BlueBattle), null)]
    [MapValue(nameof(Ac15PlayResultEnvelope.GreenGhost), null)]
    public static partial Ac15PlayResultEnvelope Map(PlayResultRequest request);

    [MapPropertyFromSource(nameof(PlayResultResponse.Result))]
    public static partial PlayResultResponse Map(uint result);
```

**Apply to MOMOIRO:** use Mapperly only if Phase 39 controllers map to common no-state DTOs. Do not copy KIMIDORI playresult mappings as proof of Momoiro mutation semantics.

### Shared Startup/Version Routes (no duplicate Momoiro controllers)

**Analog:** `Adapters.GameProtocol.Shared/Controllers/*`

**Startup route ownership** from `StartupAuthController.cs` (lines 5-12):
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

**Version route ownership** from `VerupAuthController.cs` (lines 5-15):
```csharp
[ApiController]
[Route("/v01r00/chassis/verupauth.php")]
[Route("/v01r00_tw/chassis/verupauth.php")]
public sealed class VerupAuthController : BaseProtocolController<VerupAuthController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult VerupAuth([FromBody] VerupAuthRequest request)
    {
        Logger.LogInformation("VerupAuth request: {@Request}", request);
        return Ok(new VerupAuthResponse { Result = 1 });
```

**Apply to MOMOIRO:** shared `/v01r00/chassis/startupauth.php`, `/verupauth.php`, and `/verupcomplete.php` remain in `Adapters.GameProtocol.Shared`. Do not create `StartupAuthController`, `VerupAuthController`, or `VerupCompleteController` under `Adapters.GameProtocol.Momoiro`.

### `Tests/Momoiro/MomoiroApplicationPartTests.cs` (test, request-response gating)

**Analog:** partial only. Use `Adapters.GameProtocol.Shared/GameProtocolApplicationParts.cs` as the behavior source and existing xUnit style from server settings tests.

**Behavior under test** from `GameProtocolApplicationParts.cs` (lines 16-18, 55-63):
```csharp
public static void RemoveDisabledGameProtocolApplicationParts(
    ApplicationPartManager apm,
    IReadOnlySet<GameEra> enabledEras)
```

```csharp
private static void RemoveApplicationPart(ApplicationPartManager apm, string assemblyName)
{
    var part = apm.ApplicationParts.FirstOrDefault(p =>
        p is AssemblyPart a && a.Assembly.GetName().Name == assemblyName);
    if (part is not null)
    {
        apm.ApplicationParts.Remove(part);
    }
}
```

**Testing style** from `Tests/White/WhiteServerSettingsValidationTests.cs` (lines 12-32):
```csharp
[Fact]
public void OptionsValidation_WhiteEnabledDoesNotRequireShopOrChallengeSettings()
{
    var configuration = BuildConfiguration("""
        {
          "ServerSettings": {
            "Eras": {
              "White": {
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
```

**Apply to MOMOIRO:** build an `ApplicationPartManager`, add an `AssemblyPart` for `MomoiroAdapterMarker.Assembly`, call `RemoveDisabledGameProtocolApplicationParts`, and assert the Momoiro assembly part is removed when `GameEra.Momoiro` is absent and retained when present. If controller discovery is tested, prefer feature population against application parts over controller attribute/source-string assertions.

### `Tests/Momoiro/MomoiroServerSettingsValidationTests.cs` (test, config validation)

**Analog:** `Tests/White/WhiteServerSettingsValidationTests.cs`

**Options validation pattern** (lines 66-83):
```csharp
private static IConfigurationRoot BuildConfiguration(string json)
{
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
    return new ConfigurationBuilder()
        .AddJsonStream(stream)
        .Build();
}

private static ServiceProvider BuildProvider(IConfiguration configuration)
{
    var services = new ServiceCollection();
    services.AddOptions<ServerSettings>()
        .Bind(configuration.GetSection("ServerSettings"))
        .ValidateStartupSettings(new HashSet<GameEra> { GameEra.White })
        .ValidateOnStart();

    return services.BuildServiceProvider();
}
```

**Settings model pattern** from `Application/Settings/ServerSettings.cs` (lines 12-28):
```csharp
public sealed class EraSettings
{
    public bool Enabled { get; set; }

    public bool AutoExtractCatalog { get; set; } = true;

    public string GameDataPath { get; set; } = string.Empty;

    public string? CustomizationNameDataPath { get; set; }

    public bool? EnableShop { get; set; }

    public uint? ActiveShopSeasonId { get; set; }

    public bool? EnableDonChallenge { get; set; }

    public string? ActiveDonChallengeBundleId { get; set; }
```

**Validator boundary** from `ServerSettingsOptionsValidationExtensions.cs` (lines 8-35):
```csharp
private static readonly GameEra[] Ac15ShopEras =
[
    GameEra.Green,
    GameEra.Blue,
    GameEra.Yellow
];

public static OptionsBuilder<ServerSettings> ValidateStartupSettings(
    this OptionsBuilder<ServerSettings> builder,
    ISet<GameEra> enabledEras)
{
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

    foreach (var era in new[] { GameEra.Red, GameEra.White })
    {
        builder = builder.Validate(
            settings => HasActiveDonChallengeBundle(settings, enabledEras, era),
            $"ServerSettings:Eras:{era}:ActiveDonChallengeBundleId is required when {era} EnableDonChallenge is true.");
    }
```

**Apply to MOMOIRO:** assert Momoiro can be enabled without shop or Don Challenge settings. Do not add Momoiro to `Ac15ShopEras` or the Red/White Don Challenge validation list in Phase 39.

### `Tests/Tests.csproj` (config/test, build graph)

**Analog:** `Tests/Tests.csproj`

**Adapter test references pattern** (lines 18-32):
```xml
<ItemGroup>
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
  <ProjectReference Include="..\Adapters.GameProtocol.White\Adapters.GameProtocol.White.csproj" />
  <ProjectReference Include="..\Adapters.GameProtocol.Murasaki\Adapters.GameProtocol.Murasaki.csproj" />
  <ProjectReference Include="..\TaikoWebUI\TaikoWebUI.csproj" />
</ItemGroup>
```

**Apply to MOMOIRO:** add a Momoiro adapter project reference if `MomoiroApplicationPartTests` references `MomoiroAdapterMarker` or controller types.

## Shared Patterns

### First-Class Era Wiring

**Sources:** `Domain/Enums/GameEra.cs`, `Host/Program.cs`, `Host/Configurations/ServerSettings.json`, `Adapters.GameProtocol.Shared/GameProtocolApplicationParts.cs`

**Apply to:** `GameEra`, Host DI, direct-protobuf fallback, settings, application-part gating.

Key copy points:
- `GameEra` gains a distinct `Momoiro` value after `Kimidori`.
- Host imports and conditionally registers `AddGameProtocolMomoiro()` only when `enabledEras.Contains(GameEra.Momoiro)`.
- `GameProtocolApplicationParts` removes `TaikoLocalServer.Adapters.GameProtocol.Momoiro` when Momoiro is disabled.
- `/v04r00/chassis` is included in no-content-type direct protobuf fallback.

### Thin Direct-Protobuf Controllers

**Sources:** `Adapters.GameProtocol.Kimidori/Controllers/*`, `Adapters.GameProtocol.Shared/Controllers/BaseProtocolController.cs`

**Apply to:** all Momoiro route controllers.

Pattern:
```csharp
[ApiController]
public sealed class ExampleController : BaseProtocolController<ExampleController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/example.php")]
    [Produces("application/protobuf")]
    public IActionResult Example([FromBody] ExampleRequest request)
    {
        Logger.LogInformation("Momoiro example.php request: {@Request}", request);
        return Ok(new ExampleResponse { Result = 1 });
    }
}
```

Use this as a route scaffold pattern. Do not treat KIMIDORI mediator calls or `Ac15EraProfiles.Kimidori` usage as Momoiro runtime evidence.

### Evidence-Gated Route Surface

**Sources:** `.planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md`, `proto/momoiro`, `.tools/momoiro/EBOOT.ELF.i64`

**Apply to:** evidence matrix and controller list.

Create only these Momoiro game controllers in Phase 39:
```text
playresult.php
baidcheck.php
mydonentry.php
userdata.php
recommend.php
selfbest.php
heartbeat.php
defaultsong.php
bookkeeping.php
songhash.php
telopcheck.php
gettelop.php
```

Do not create these proto-only controllers in Phase 39:
```text
shoppingresult.php
bestscore.php
communicationlog.php
mainichisong.php
```

### Generated Wire Discipline

**Sources:** `Adapters.GameProtocol.Kimidori/Wire/Game.cs`, `Adapters.GameProtocol.Kimidori/Wire/VsInterface.cs`, `AGENTS.md`

**Apply to:** `Adapters.GameProtocol.Momoiro/Wire/Game.cs`, `Adapters.GameProtocol.Momoiro/Wire/VsInterface.cs`

Use protogen output headers and adapter-local namespace. Do not hand-edit generated DTO bodies or edit `proto/momoiro` during Phase 39.

### Settings Validation Boundary

**Sources:** `Application/Settings/ServerSettingsOptionsValidationExtensions.cs`, `Tests/White/WhiteServerSettingsValidationTests.cs`

**Apply to:** Momoiro startup validation and tests.

MOMOIRO should be enabled without shop/Don Challenge settings. Do not add Momoiro to `Ac15ShopEras` or Red/White Don Challenge validation lists in this phase.

## No-Touch Boundaries For Phase 39

These are explicitly out of foundation scope and should not appear in the implementation plan except as "do not modify" verification:

| Boundary | Reason |
|----------|--------|
| `Application/Handlers/*.Momoiro.cs` runtime behavior | State, readback, and playresult mutation are Phase 41/42 work. |
| `Infrastructure/GameDataCatalog/Momoiro/*` | Runtime catalog loading and root-level parser work are Phase 40. |
| `Domain/Entities/*Momoiro*`, `Infrastructure/Persistence/*`, migrations | Gameplay persistence is Phase 41/42. |
| `Adapters.AdminApi/*` and `TaikoWebUI/*` | AdminApi/WebUI Momoiro surfaces are Phase 43. |
| `proto/momoiro/*` | Proto inputs are evidence/generation inputs; do not edit unless explicitly allowed. |
| `Adapters.GameProtocol.Momoiro/Controllers/ShoppingResultController.cs` | Proto-only route family; absent from locked binary route inventory. |
| `Adapters.GameProtocol.Momoiro/Controllers/BestScoreController.cs` | Proto-only route family; absent from locked binary route inventory. |
| `Adapters.GameProtocol.Momoiro/Controllers/CommunicationLogController.cs` | Proto-only route family; absent from locked binary route inventory. |
| `Adapters.GameProtocol.Momoiro/Controllers/MainichiSongController.cs` | Proto-only route family; absent from locked binary route inventory. |

## No Analog Found

| File | Role | Data Flow | Reason |
|------|------|-----------|--------|
| `Tests/Momoiro/MomoiroApplicationPartTests.cs` | test | request-response gating | No existing test directly exercises `ApplicationPartManager`; use `GameProtocolApplicationParts` source as behavior analog and xUnit/options tests for style. |

## Metadata

**Analog search scope:** `Domain/`, `Host/`, `Application/Settings/`, `Adapters.GameProtocol.Shared/`, `Adapters.GameProtocol.Kimidori/`, `Adapters.GameProtocol.Murasaki/`, `Tests/`, `.planning/milestones/`

**Files scanned:** 60+ source/config/test/planning files through `rg` and targeted reads.

**Strong analogs used:** KIMIDORI adapter skeleton/controllers/mappers, Murasaki route-prefix caution, shared startup/version controllers, shared application-part gating, White evidence artifact and settings validation tests.

**Pattern extraction date:** 2026-06-26
