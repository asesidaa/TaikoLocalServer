# Stack Research: v1.5 Murasaki AC15 Support

**Domain:** Brownfield AC15 cabinet protocol adapter, catalog, persistence, and admin surface
**Researched:** 2026-06-21
**Confidence:** HIGH for stack decision; MEDIUM for Murasaki route/root details until binary/log evidence is captured

## Summary

No new runtime stack is justified for Murasaki. Implement v1.5 as another first-class AC15 era on the existing .NET 10 / ASP.NET Core / EF Core SQLite / protobuf-net / Mapperly / Mediator / MudBlazor stack.

The required additions are project and tooling additions, not library additions: a new Murasaki adapter project, Murasaki-owned generated wire DTOs from `proto/murasaki`, Mapperly projection boundaries that account for Murasaki's split request families, Murasaki-owned EF/catalog/AdminApi/WebUI registrations when behavior is implemented, and evidence capture using local proto/data/log/IDA artifacts.

Murasaki should reuse White-like and shared AC15 capabilities only after the Murasaki proto/data/client evidence proves matching semantics. The local `proto/murasaki/taiko.proto` differs materially from White: it does not have the White-style monolithic `Initialdatacheck*` message and instead splits metadata into request families such as `defaultsong`, `mainichisong`, `foldercheck`, `getfolder`, `telopcheck`, `gettelop`, `songhash`, and `bestscore`. That is a mapper/controller/Application-shape concern, not a reason to add another serialization, mapping, or runtime framework.

## Recommended Stack

### Core Technologies

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| .NET / C# | `net10.0`, C# 13 | Runtime target and language | Already repo-wide in `Directory.Build.props`; changing target would widen the milestone for no Murasaki benefit. |
| ASP.NET Core MVC | 10.0.x framework reference | Cabinet `.php` protocol endpoints and AdminApi host | Existing era adapters are controller-based and hosted behind enabled-era application-part gating. Murasaki should follow that shape. |
| EF Core SQLite | 10.0.7 | Local era-owned persistence | Existing stateful AC15 eras use separate EF tables and SQLite migrations; Murasaki runtime state should be Murasaki-owned rather than sharing White/Red tables. |
| protobuf-net | Runtime `3.2.56`; generator `protogen 3.2.52+f4db4afce3` | Direct protobuf transport and generated wire DTOs | The local Murasaki proto files are proto2 inputs and existing AC15 adapters use protobuf-net-generated DTOs. |
| Riok.Mapperly | 4.3.1 | Mechanical source-generated mapping between wire DTOs and application DTO/capability records | Current AC15 mapping policy is strict and source-generator driven. Murasaki's changed wire shape should be expressed through Mapperly mappings and explicit helper conversions, not handwritten mapper bodies. |
| Mediator | 3.0.2 | Application request/handler dispatch | Existing controllers deserialize, map, call Mediator, and map back. Murasaki should add handler partials or new request handlers within that pipeline. |
| Blazor WebAssembly + MudBlazor | MudBlazor 9.4.0 | Admin WebUI parity after backend behavior exists | No new UI framework is warranted; extend existing era-routed surfaces only for implemented Murasaki-owned state. |

### Supporting Libraries

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| `protobuf-net.AspNetCore` | 3.2.52 | ASP.NET Core protobuf content negotiation and helpers | Reuse existing Host/shared adapter behavior for direct-protobuf request/response handling. |
| `EntityFrameworkCore.Exceptions.Sqlite` | 10.0.0 | SQLite exception normalization | Use through existing Infrastructure patterns if Murasaki adds unique constraints or state tables. |
| `Serilog.AspNetCore` | 10.0.0 | Request/runtime/evidence logging | Use for protocol-edge request logging, unsupported-shape diagnostics, and compatibility route logs. |
| `JsonSchema.Net` | 9.2.2 | Existing JSON sidecar validation support | Use only if Murasaki gets committed server-authored sidecar JSON with schema-backed validation. Do not add a new validation library. |
| `SharpZipLib` | 1.4.2 | Existing compression/archive utility | Reuse only where an existing AC15 parser/protocol path already needs it; Murasaki direct-protobuf transport does not justify new compression plumbing. |
| xUnit | 2.9.3 | Observable behavior and persistence regression tests | Add focused tests for Murasaki handler/catalog/persistence/protocol behavior after evidence, not source-shape tests. |

### Development Tools

| Tool | Purpose | Notes |
|------|---------|-------|
| `.tools/protogen.exe` | Generate Murasaki wire DTOs from local proto inputs | Present in this checkout and reports `protogen 3.2.52+f4db4afce3`. Use `+nullablevaluetype=yes` to preserve optional primitive presence where supported. |
| Mapperly generated-source emission | Inspect generated mapper implementation | Later Mapperly work must check current official Mapperly docs and build with `/p:EmitCompilerGeneratedFiles=true`; inspect emitted `.g.cs` under `obj/.../generated/.../Riok.Mapperly/`. |
| `ida-cli` daemon mode | Binary/client evidence when proto/data/logs are insufficient | Use against `.tools/murasaki/EBOOT.ELF.i64` only when binary proof materially affects stack/tooling or implementation decisions. Keep it as evidence tooling, not runtime dependency. |
| `rg`, PowerShell directory/file inspection | Route/data evidence capture | Use for local proto/data/config inventory and generated-output audits. Record evidence in planning artifacts rather than deriving runtime behavior from guesses. |
| `dotnet build` / `dotnet test` | Build, source-generator, and behavior verification | Use solution builds, focused tests, and temp-output Host builds if `Host/bin/Debug/net10.0` is locked by a running server. |

## Wire DTO And Mapperly Pattern

Generate Murasaki wire DTOs into a new adapter-local namespace, for example:

```powershell
.\.tools\protogen.exe --csharp_out=Adapters.GameProtocol.Murasaki\Wire -Iproto\murasaki +nullablevaluetype=yes proto\murasaki\taiko.proto
.\.tools\protogen.exe --csharp_out=Adapters.GameProtocol.Murasaki\Wire -Iproto\murasaki +nullablevaluetype=yes proto\murasaki\vsinterface.proto
```

Expected generated-file convention:

| Input | Output |
|-------|--------|
| `proto/murasaki/taiko.proto` | `Adapters.GameProtocol.Murasaki/Wire/Game.cs` |
| `proto/murasaki/vsinterface.proto` | `Adapters.GameProtocol.Murasaki/Wire/VsInterface.cs` |

Do not hand-edit generated `Wire/` files. If the generator output needs namespace or filename normalization, adjust generation/placement once and keep the generated field/tag surface intact.

Mapperly should remain source-generator driven:

- Keep the existing assembly defaults from `Application/MapperlyDefaults.cs`: `AutoUserMappings = false` and `RequiredMappingStrategy = RequiredMappingStrategy.Target`.
- Map Murasaki wire DTOs into application/common AC15 capability shapes before handler logic.
- For Murasaki split metadata endpoints, prefer small endpoint-specific application records or capability records instead of forcing everything through the White `Initialdatacheck` shape.
- Use Mapperly-native attributes and configured helper conversions for constants, byte normalization, nullable optional primitives, and field placement.
- Manual mapper code is limited to explicit helper conversions that Mapperly calls. It must not classify modes, decide persistence policy, or replace source-generated projection.
- Build with `dotnet build /p:EmitCompilerGeneratedFiles=true` and inspect the emitted Mapperly `.g.cs` output before declaring mapper implementation correct.

## Runtime Project Additions

Add the Murasaki runtime shape by copying the existing adapter/project pattern, not by changing stack:

| Area | Recommended Addition | Notes |
|------|----------------------|-------|
| Adapter project | `Adapters.GameProtocol.Murasaki` | Match Red/White project shape: `FrameworkReference` to `Microsoft.AspNetCore.App`, project references to Shared/Application/Contracts, package references to `protobuf-net` and `Riok.Mapperly`. |
| Era identity | `GameEra.Murasaki` | Add as a first-class era; update route parsing and UI enum support only where behavior exists. |
| Host configuration | `ServerSettings:Eras:Murasaki` | Include `Enabled`, `AutoExtractCatalog`, `GameDataPath`, and capability flags only when needed. Do not add shop/Don Challenge settings until Murasaki evidence requires them. |
| Host content/build | Murasaki data exclusion and debug junction target | Exclude `wwwroot\data\murasaki\data\**` from publish/content and mirror the debug symlink/junction behavior used for Blue/Yellow/Red/White. |
| Catalog | `MurasakiGameDataPaths`, `MurasakiRequiredDataFiles`, `MurasakiEraGameDataCatalog`, `IMurasakiCatalog` | Reuse AC15 loaders where file formats match, but keep the interface and active-root decision Murasaki-owned. |
| Persistence | Murasaki-owned EF entities and migrations | Add only when runtime state is implemented. Do not reuse White/Red tables. |
| AdminApi/WebUI | Existing era-routed contracts | Extend generic AC15 surfaces for implemented Murasaki-owned state; avoid Murasaki-only API shapes unless a Murasaki-only capability needs one. |

## Evidence And Tooling Notes

Local evidence already available:

| Evidence | Current Finding | Stack Implication |
|----------|-----------------|-------------------|
| `.planning/PROJECT.md` | v1.5 is Murasaki AC15 support in the existing multi-era server. | Brownfield stack reuse is the default. |
| `Directory.Build.props` | `net10.0`, nullable enabled, C# 13. | Do not change target/language. |
| `Directory.Packages.props` | Central versions for EF Core, protobuf-net, Mapperly, Mediator, MudBlazor, xUnit. | Add package references without explicit versions in new projects. |
| `proto/murasaki/taiko.proto` | Murasaki has split metadata/global-score families and no White-style monolithic `Initialdatacheck*`. | Add Murasaki-owned controllers/mappers/application records for split endpoints. |
| `proto/murasaki/vsinterface.proto` | Startup/version message family exists. | Reuse shared startup/version handling only where client evidence agrees. |
| `Host/wwwroot/data/murasaki/data` | Symlink to local Murasaki operator data. | Runtime data is local/operator-supplied and should be excluded from publish. |
| `Host/wwwroot/data/murasaki/data/config` | Roots: `common`, `ST5100-1`, `ST5100-7`, `ST6100-1`. Each versioned root includes `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, and `spacialbaid.xml`. | Catalog loaders can reuse AC15 parsers, but active root selection needs evidence. |
| `.tools/murasaki/EBOOT.ELF.i64` | Present and nonzero (`122793480` bytes). | Use `ida-cli` daemon mode when binary route/root/request proof is needed. Do not make IDA a runtime dependency. |
| Current White adapter | Final `/v07r03` and compatibility `/v07r00` use separate generated wire namespaces. | If Murasaki later needs version-split support, follow the White split-wire pattern only after evidence proves the split. |

## Installation

No new package installation is expected for Murasaki support.

New project references should use central package management:

```xml
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

Verification commands:

```powershell
dotnet build TaikoLocalServer.slnx
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
dotnet build /p:EmitCompilerGeneratedFiles=true
dotnet test Tests/Tests.csproj
```

Use focused test slices during implementation, then full solution/test verification before milestone closeout. Cabinet/RPCS3 acceptance remains outside stack tooling and is still the compatibility gate.

## Alternatives Considered

| Recommended | Alternative | When to Use Alternative |
|-------------|-------------|-------------------------|
| protobuf-net generated DTOs | `Google.Protobuf` / `Grpc.Tools` | Only if the repo intentionally migrates all protocol adapters. Murasaki alone does not justify a second protobuf runtime/model. |
| Adapter-local `Wire/` namespace | Shared AC15 generated wire assembly | Do not use for Murasaki. Existing architecture keeps each era's generated wire separate because field placement/versioning differs. |
| Mapperly source-generated projections | Handwritten mapper bodies | Use handwritten code only for helper conversions Mapperly invokes; full handwritten projection hides wire/field drift and violates current mapper policy. |
| Existing EF Core SQLite tables per era | Shared `Ac15SaveData` table with era discriminator | Do not use. State separation is a core repo rule and prevents cross-era corruption. |
| Existing Application/Ac15 capability modules | New generic "old AC15" runtime library | Add no new library. Reuse capability modules through Murasaki-owned composition and extend only where Murasaki evidence proves shared behavior. |
| `ida-cli` evidence tooling | Runtime binary-analysis dependency or route scraper package | Binary analysis belongs in planning/evidence capture only. Runtime code should be static, explicit, and testable. |

## What NOT To Use

| Avoid | Why | Use Instead |
|-------|-----|-------------|
| New runtime serialization library | Existing adapters and Host already use protobuf-net; dual protobuf models would complicate transport and tests. | `protobuf-net` plus generated adapter-local DTOs. |
| Shared generated AC15 wire DTOs | Murasaki, White legacy, White final, Red, Yellow, and Blue have different field placement/version surfaces. | One `Wire/` namespace per adapter/version. |
| Hand-edited generated `Wire/` files | Manual edits lose repeatability and hide proto/generator drift. | Regenerate from `proto/murasaki` and map through partial/helper code. |
| Repository-shaped persistence abstraction | Prior AC15 direction keeps direct `ITaikoDbContext`, concrete `DbSet`s, and narrow row-shape helpers visible. | Murasaki-owned entities with shared generic helpers where behavior is identical. |
| Runtime route inference from proto similarity | Proto messages prove possible payload shapes, not actual client calls or route prefixes. | Capture route/client evidence from logs, captures, or `.tools/murasaki/EBOOT.ELF.i64` via `ida-cli`. |
| Public wiki/runtime scraping | Runtime scraping is out of scope and public sources do not outrank local proto/data/client evidence. | Local proto, game data, logs, IDA, and cabinet/RPCS3 observations. |
| New frontend framework | Existing AdminApi/WebUI routes are already Blazor WASM/MudBlazor. | Extend existing era-routed WebUI surfaces after backend state exists. |

## Stack Patterns By Variant

**If Murasaki is a single confirmed game protocol version:**

- Generate one `Adapters.GameProtocol.Murasaki/Wire` namespace from `proto/murasaki`.
- Add one Murasaki route-prefix constant for `/v06r00/chassis` after route evidence is locked.
- Keep `/v01r00/chassis` startup/version routing shared if `vsinterface` and client evidence agree.

**If later evidence proves multiple Murasaki protocol versions:**

- Follow the current White final/legacy split pattern with separate generated namespaces.
- Do not force newer fields onto older wire DTOs.
- Keep controller methods explicit per route/version.

**If a Murasaki endpoint is White-like but split differently:**

- Reuse the underlying Application/Ac15 service or catalog helper only after mapping Murasaki wire facts into an application shape that represents the actual Murasaki request.
- Do not fake a White `Initialdatacheck` contract just to reuse a controller.

**If a feature needs binary proof:**

- Use `ida-cli` daemon mode against `.tools/murasaki/EBOOT.ELF.i64`.
- Capture route strings, config-root selection, request family sequencing, or byte-field interpretation as evidence artifacts.
- Keep any generated JSON/evidence outputs out of runtime dependencies unless deliberately converted into committed server-authored sidecars.

## Version Compatibility

| Package / Tool | Compatible With | Notes |
|----------------|-----------------|-------|
| `net10.0` / C# 13 | EF Core 10.0.7, ASP.NET Core 10.0.7 packages | Current repo baseline. |
| `protobuf-net` 3.2.56 | `protogen` 3.2.52 | Runtime and generator versions differ slightly in the existing repo; record exact generator version when regenerating Murasaki wire files. |
| `protobuf-net.AspNetCore` 3.2.52 | ASP.NET Core 10 Host | Reuse existing direct-protobuf handling and missing-content-type fallback patterns. |
| Mapperly 4.3.1 | Current Mapperly docs stable 4.3.1 | Official docs confirm generated-source emission via `/p:EmitCompilerGeneratedFiles=true`; implementation should recheck docs at mapper work time. |
| Mediator 3.0.2 | Existing Application handlers | Use current request/handler patterns; no new mediator library. |
| MudBlazor 9.4.0 | Existing Blazor WebAssembly Admin UI | Extend only after backend/AdminApi behavior exists. |

## Sources

- `.planning/PROJECT.md` - active v1.5 Murasaki scope, AC15 reuse constraints, evidence hierarchy.
- `.planning/MILESTONES.md` and `.planning/STATE.md` - current milestone state and prior White/AC15 closeout context.
- `.codex/gsd-core/templates/research-project/STACK.md` - stack research structure.
- `Directory.Build.props`, `Directory.Packages.props` - current target framework and central package versions.
- `proto/murasaki/taiko.proto`, `proto/murasaki/vsinterface.proto` - Murasaki wire inputs.
- `proto/white/taiko.proto`, `proto/white-final/taiko.proto` - White legacy/final comparison for split-wire precedent.
- `Host/wwwroot/data/murasaki/data/config` - local Murasaki config roots and required data candidates.
- `.tools/murasaki/EBOOT.ELF.i64` - local nonzero binary evidence handle for `ida-cli`.
- `docs/superpowers/specs/2026-06-07-ac15-core-extraction-design.md` - AC15 sharing rule: share behavior, not routes or generated wire models.
- `docs/superpowers/specs/2026-06-11-ac15-capability-composition-design.md` - capability-driven Application/Ac15 composition and no shared EF tables.
- `docs/superpowers/specs/2026-06-13-ac15-playresult-capability-input-design.md` - Mapperly and capability-shaped input boundary direction.
- `https://mapperly.riok.app/docs/configuration/mapper/#null-values` - Mapperly null and strict mapping behavior.
- `https://mapperly.riok.app/docs/configuration/constant-generated-values/` - Mapperly `MapValue` support for constants/generated values.
- `https://mapperly.riok.app/docs/configuration/generated-source/` - Mapperly generated-source inspection with `EmitCompilerGeneratedFiles`.
- `https://protobuf-net.github.io/protobuf-net/` - protobuf-net project documentation.

---
*Stack research for: v1.5 Murasaki AC15 Support*
*Researched: 2026-06-21*
