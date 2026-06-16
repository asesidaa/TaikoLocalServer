# Phase 23: White Evidence and Era Foundation - Research

**Researched:** 2026-06-17
**Domain:** ASP.NET Core game-protocol adapter foundation, protobuf-net wire generation, era-enabled Host routing, and White AC15 evidence gating
**Confidence:** MEDIUM

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

## Implementation Decisions

### Route, Root, And Transport Evidence
- **D-01:** White game routes are expected under `/v07r00/chassis/*`, but Phase 23 must verify `/v07r00` from White IDB route strings before route attributes are finalized.
- **D-02:** White startup/version routes stay under shared `/v01r00/chassis/*`. Treat this as stable older-AC15 behavior; no extra proof is needed unless White evidence contradicts it.
- **D-03:** Treat `Host/wwwroot/data/white/data/config/ST7100-1` as the active White data root from the local game-directory symlink/inventory. Do not require separate IDB root proof for Phase 23.
- **D-04:** Treat White game endpoints as direct-protobuf AC15 requests unless White evidence contradicts this. Runtime HTTP content-type capture is not a Phase 23 blocker.

### Scaffold Breadth
- **D-05:** Determine the concrete White scaffold route set by searching `.php` route strings in `.tools/white/EBOOT.ELF.i64`, then cross-checking those suffixes against `proto/white` request/response pairs. Do not guess route inventory from Red, Yellow, or proto-only presence.
- **D-06:** Phase 23 controllers should be thin no-state scaffolds: deserialize White wire DTOs, log bounded request information, and return safe success/default responses where appropriate. They must not add Mediator handlers, EF writes, catalog reads, profile state, or runtime semantics.
- **D-07:** After `/v07r00` is verified, add White only to the existing exact-prefix missing-content-type protobuf fallback. Do not replace it with a broad generic AC15 fallback.
- **D-08:** Stop Phase 23 at foundation work: generated wire, adapter project/marker/DI, enum/config/Host gating, route scaffolds, fallback scope, and existing-era preservation checks. White catalog/profile/runtime work begins in Phase 24+.

### Unsupported Or Unproven White Surfaces
- **D-09:** Keep this rule simple: if a surface is not supported by White 0.13, ignore it. If behavior already exists as shared behavior or a shared capability, it may be extracted or kept reusable, but White must not be wired to it unless White evidence supports that surface. Do not turn this into a feature-by-feature absence matrix.

### Active Planning Corrections
- **D-10:** Current filesystem evidence shows `.tools/white/EBOOT.ELF.i64` is present and nonzero. Phase 23 should correct active planning notes that still say the White IDB is zero bytes, and record current IDB file-size evidence in the Phase 23 evidence artifact.

### the agent's Discretion
None. The user made the relevant Phase 23 boundaries explicit.

### Deferred Ideas (OUT OF SCOPE)
None - discussion stayed within Phase 23 scope.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| WFND-01 | Developer can review a White evidence record that identifies route prefix, transport expectations, startup/version ownership, active data root, usable/unusable IDB evidence, and unresolved gaps before White routes are finalized. | Plan an evidence artifact as the first task, record live IDB size, record stale zero-byte notes as superseded, and make route-prefix proof a blocking evidence gate before final route attributes. [CITED: .planning/REQUIREMENTS.md] [VERIFIED: live filesystem] |
| WFND-02 | White is served by a first-class enableable `GameEra.White` adapter with generated White wire DTOs from `proto/white`, era settings, Host/DI registration, route ownership, and enabled-era gating. | Use the existing Red adapter project shape, `GameEra` enum extension, Host project references, `AddGameProtocol*` registration, `ApplicationPartManager` removal, and exact-prefix protobuf fallback. [VERIFIED: codebase grep] |
| WFND-03 | White work preserves existing supported-era behavior except where shared code changes are required and existing behavior remains covered. | Keep Phase 23 additive, add White-specific checks around disabled-era route absence and fallback scope, and run focused existing-era build/test slices when touching shared Host or settings code. [CITED: AGENTS.md] [VERIFIED: codebase grep] |
</phase_requirements>

## Project Constraints (from AGENTS.md)

- Keep era state separate; White, Blue, Green, Yellow, Red, and Nijiiro gameplay persistence must not be merged unless the state is truly shared identity data. [CITED: AGENTS.md]
- Use the partial-file pattern for era behavior when Application code later grows White support. [CITED: AGENTS.md]
- Map generated protobuf DTOs through Application `Common*` shapes before handler logic; do not persist wire DTOs directly. [CITED: AGENTS.md]
- Keep Mapperly mappers source-generator driven and inspect emitted generated source for Mapperly behavior using `dotnet build /p:EmitCompilerGeneratedFiles=true`. [CITED: AGENTS.md] [CITED: https://mapperly.riok.app/docs/configuration/generated-source/]
- Controllers should deserialize, map, call Mediator, and map back; business behavior belongs in `Application/Handlers`. [CITED: AGENTS.md]
- Resolve runtime data paths through `PathHelper` and era data path helpers rather than hardcoding `wwwroot/data/<era>` in runtime handlers. [CITED: AGENTS.md]
- Preserve legacy AdminApi routes where they exist and preserve `/api/{era}/...` routing through `EraRoute.TryParse`; Phase 23 should not add White AdminApi runtime surfaces. [CITED: AGENTS.md] [CITED: 23-CONTEXT.md]
- Keep generated `Wire/` files out of manual cleanup unless regenerating protocol output. [CITED: AGENTS.md]
- Add tests only for evidence-backed behavior or boundaries; avoid source-text, route-attribute, generated-type, DI-shape, enum-value, migration, private-method, `Mediator.Send`, `SaveChanges`, and superficial `Result = 1` tests. [CITED: AGENTS.md]
- Use the temp-output Host build when a running server locks `Host/bin/Debug/net10.0`. [CITED: AGENTS.md]

## Summary

Phase 23 should be planned as a gated foundation phase: first write a White evidence artifact, then generate White wire DTOs, then add additive adapter/Host scaffolding only for route suffixes proven by White evidence. [CITED: 23-CONTEXT.md] [VERIFIED: codebase grep] The live checkout contradicts stale planning notes: `.tools/white/EBOOT.ELF.i64` exists and is 129,893,515 bytes, while `.planning/STATE.md`, `.planning/ROADMAP.md`, and prior research still contain zero-byte statements. [VERIFIED: live filesystem] [VERIFIED: codebase grep]

The highest-risk item is route evidence, not .NET scaffolding. [CITED: 23-CONTEXT.md] A raw `rg -a` scan of the White `.i64` found protobuf/message-name strings but no full `/v07r00/.../*.php` route strings, and an IDA-CLI `AgentSession` probe failed with `idapro.open_database returned error code 4` while IDA sidecar files were locked by a running `ida.exe`. [VERIFIED: binary rg] [VERIFIED: ida-cli probe] [VERIFIED: local process check] The planner should make IDA route extraction or replacement request-log/capture evidence a first task and should not finalize White route attributes until that task produces route-prefix and suffix evidence. [CITED: 23-CONTEXT.md]

No new external runtime stack is needed for Phase 23. [VERIFIED: Directory.Packages.props] Use existing .NET 10, ASP.NET Core controllers, protobuf-net/protogen, Mapperly, Mediator abstractions already in the solution, xUnit tests, and the Red adapter project shape. [VERIFIED: codebase grep] [CITED: https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-10.0] [CITED: https://protobuf-net.github.io/protobuf-net/contract_first.html] [CITED: https://mapperly.riok.app/docs/configuration/generated-source/]

**Primary recommendation:** Plan Phase 23 as two waves: an evidence gate that produces `23-WHITE-EVIDENCE.md` and route inventory, followed by a strictly additive White adapter foundation gated by that evidence. [CITED: 23-CONTEXT.md] [VERIFIED: codebase grep]

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|--------------|----------------|-----------|
| White evidence artifact and feature inventory | Planning artifact | Local filesystem / IDA evidence | The artifact owns proof and unresolved gaps before code treats route/root/transport choices as final. [CITED: 23-CONTEXT.md] |
| White route prefix and suffix ownership | Adapter project | Host fallback | Adapter controllers own concrete route attributes after evidence, and Host only scopes missing-content-type fallback to exact prefixes. [VERIFIED: Host/Program.cs] |
| White generated wire DTOs | Adapter project | `proto/white` inputs | Existing AC15 adapters keep generated wire classes adapter-local and generated from immutable proto inputs. [VERIFIED: Adapters.GameProtocol.Red/Wire/Game.cs] [CITED: AGENTS.md] |
| Enabled-era route gating | Host | Adapter marker assembly | Host already registers era adapters conditionally and removes disabled adapter assemblies from MVC application parts. [VERIFIED: Host/Program.cs] [CITED: https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-10.0] |
| Startup/version routes | Shared adapter | Application movie query | Shared `/v01r00/chassis/*` controllers own startup/version behavior and use enabled-era catalog selection. [VERIFIED: Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs] [CITED: 23-CONTEXT.md] |
| White runtime data root record | Evidence artifact | Host project data rules | Phase 23 records the accepted `ST7100-1` root and may add Host content exclusion/debug junction rules, but catalog loading is later Phase 24 work. [CITED: 23-CONTEXT.md] [VERIFIED: Host/Host.csproj] |
| Existing-era preservation | Tests/build | Host/shared code review | Shared Host changes can expose or hide routes for all eras, so focused checks must cover existing prefix fallback and disabled-era behavior. [CITED: AGENTS.md] [VERIFIED: Host/Program.cs] |

## Standard Stack

### Core

| Library / Tool | Version | Purpose | Why Standard |
|----------------|---------|---------|--------------|
| .NET SDK / target framework | SDK 10.0.201 installed; repo target `net10.0` | Builds Host, adapters, Application, Infrastructure, WebUI, and Tests. | The repo uses `Directory.Build.props` with `TargetFramework` `net10.0`, and `dotnet --version` reports 10.0.201. [VERIFIED: Directory.Build.props] [VERIFIED: local command] |
| ASP.NET Core MVC controllers | 10.0.7 package family in central props | Hosts protocol controllers and protobuf formatters. | Host uses `AddControllers().AddProtoBufNet()` and conditionally removes disabled adapter assemblies. [VERIFIED: Directory.Packages.props] [VERIFIED: Host/Program.cs] |
| protobuf-net | 3.2.56 runtime; protobuf-net.AspNetCore 3.2.52 | Serializes adapter request/response wire DTOs. | Existing AC15 adapters use protobuf-net generated `[ProtoContract]` wire classes and Host uses protobuf-net MVC integration. [VERIFIED: Directory.Packages.props] [VERIFIED: Adapters.GameProtocol.Red/Wire/Game.cs] |
| repo-local `protogen.exe` | 3.2.52+f4db4afce3 | Generates White C# wire DTOs from `proto/white`. | Existing generated adapter wire files are tool-generated, and local protogen is available under `.tools/protogen.exe`. [VERIFIED: local command] [VERIFIED: Adapters.GameProtocol.Red/Wire/Game.cs] |
| Riok.Mapperly | 4.3.1 | Generates mechanical adapter/Application mappings. | Existing adapter mappers use Mapperly, and official docs confirm generated-source emission through `EmitCompilerGeneratedFiles`. [VERIFIED: Directory.Packages.props] [CITED: https://mapperly.riok.app/docs/configuration/generated-source/] |
| Mediator.SourceGenerator / Mediator.Abstractions | 3.0.2 | Application request/handler dispatch. | Existing controllers call `Mediator.Send` through `BaseProtocolController`, but Phase 23 no-state scaffold routes should avoid adding new runtime handlers unless already required. [VERIFIED: Directory.Packages.props] [VERIFIED: BaseProtocolController.cs] [CITED: 23-CONTEXT.md] |
| xUnit / Microsoft.NET.Test.Sdk | xUnit 2.9.3; test SDK 17.14.1 | Focused behavior and boundary tests. | `Tests/Tests.csproj` references xUnit and existing era tests cover settings, handler, catalog, and protocol behavior. [VERIFIED: Tests/Tests.csproj] |

### Supporting

| Library / Tool | Version | Purpose | When to Use |
|----------------|---------|---------|-------------|
| ripgrep | 15.1.0 | Fast code, proto, and binary text scanning. | Use for route/proto inventory and stale-note audits, but do not treat raw `.i64` grep as sufficient route proof when it finds no `.php` strings. [VERIFIED: local command] [VERIFIED: binary rg] |
| IDA-CLI bridge | importable; IDB probe failed with error code 4 | Intended path for IDA-backed route/root extraction. | Use after resolving the current database-open/locked-file condition or through a running IDA-compatible workflow. [VERIFIED: local command] [VERIFIED: ida-cli probe] |
| Host temp-output build | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` | Avoids locked normal Host output. | Use when a running server or local process locks `Host/bin/Debug/net10.0`. [CITED: AGENTS.md] |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Existing protobuf-net/protogen | Google.Protobuf tooling | Do not switch; existing adapters and Host formatters are protobuf-net based, and Phase 23 is not a serializer migration. [VERIFIED: codebase grep] |
| Existing Mapperly partial mappers | Handwritten mapper bodies | Do not switch; repo rules require Mapperly source-generator driven mappings and official docs provide generated-source verification. [CITED: AGENTS.md] [CITED: https://mapperly.riok.app/docs/configuration/generated-source/] |
| Exact-prefix Host fallback | Generic AC15 missing-content-type fallback | Do not generalize; current Host uses explicit `StartsWithSegments` allowlist, and D-07 requires adding White only after `/v07r00` evidence. [VERIFIED: Host/Program.cs] [CITED: 23-CONTEXT.md] |
| Route list copied from Red or Yellow | White IDB/log/capture route extraction | Do not copy; D-05 requires White `.php` route strings cross-checked against `proto/white`. [CITED: 23-CONTEXT.md] |

**Installation:**

```powershell
# No package install is recommended for Phase 23. Reuse repo-local tools and existing central package versions.
.\.tools\protogen.exe --version
```

**Version verification:** Local verification found .NET SDK 10.0.201, `protogen 3.2.52+f4db4afce3`, `protobuf-net 3.2.56`, `protobuf-net.AspNetCore 3.2.52`, `Riok.Mapperly 4.3.1`, `Mediator.SourceGenerator 3.0.2`, `xunit 2.9.3`, and `Microsoft.NET.Test.Sdk 17.14.1`. [VERIFIED: local command] [VERIFIED: Directory.Packages.props]

## Package Legitimacy Audit

Phase 23 should not install new external packages, so the package legitimacy gate is not applicable. [VERIFIED: Directory.Packages.props] Existing package names and versions are reused from central package management rather than newly recommended for installation. [VERIFIED: Directory.Packages.props]

| Package | Registry | Age | Downloads | Source Repo | slopcheck | Disposition |
|---------|----------|-----|-----------|-------------|-----------|-------------|
| None | N/A | N/A | N/A | N/A | N/A | No new package install recommended. [VERIFIED: codebase grep] |

**Packages removed due to slopcheck [SLOP] verdict:** none. [VERIFIED: no new installs]
**Packages flagged as suspicious [SUS]:** none. [VERIFIED: no new installs]

## Architecture Patterns

### System Architecture Diagram

```text
White evidence inputs
  proto/white + Host/wwwroot/data/white/data + .tools/white IDB/logs/captures
        |
        v
23-WHITE-EVIDENCE.md
  route prefix? route suffixes? transport? active root? unresolved gaps?
        |
        | only if route evidence proves a suffix
        v
Adapters.GameProtocol.White
  Wire/Game.cs + Wire/VsInterface.cs
  thin no-state controllers
        |
        v
Host composition
  GameEra.White -> ServerSettings:Eras:White -> AddGameProtocolWhite
  ApplicationPart removal when disabled
  exact /v07r00/chassis fallback only after proof
        |
        v
Verification
  proto immutability, generated wire build, disabled-era route absence,
  existing-era fallback preservation, Host temp-output build
```

The diagram reflects the current Host composition model and the Phase 23 context gate. [VERIFIED: Host/Program.cs] [CITED: 23-CONTEXT.md]

### Recommended Project Structure

```text
Adapters.GameProtocol.White/
  Adapters.GameProtocol.White.csproj
  WhiteAdapterMarker.cs
  DependencyInjection.cs
  MapperlyDefaults.cs
  GlobalUsings.cs
  Controllers/
  Mappers/
  Wire/
    Game.cs
    VsInterface.cs

.planning/phases/23-white-evidence-and-era-foundation/
  23-WHITE-EVIDENCE.md
  23-WHITE-FEATURE-INVENTORY.md
```

This structure mirrors the existing Red adapter project and separates evidence artifacts from code. [VERIFIED: Adapters.GameProtocol.Red] [CITED: 23-CONTEXT.md]

### Pattern 1: Evidence Gate Before Route Attributes

**What:** Write the evidence artifact before adding White controllers, then permit route attributes only for suffixes proven by IDB strings or replacement runtime evidence. [CITED: 23-CONTEXT.md]

**When to use:** Use this before any `/v07r00/chassis/*` route is committed. [CITED: 23-CONTEXT.md]

**Example:**

```markdown
| Claim | Status | Evidence | Planner action |
|-------|--------|----------|----------------|
| Game prefix `/v07r00/chassis` | EXPECTED, not locked | IDB/log/capture task required | Block route attributes until proven |
| Startup/version `/v01r00/chassis` | ACCEPTED unless contradicted | Phase context D-02 | Reuse shared controllers |
| Active root `config/ST7100-1` | ACCEPTED for Phase 23 | symlink/inventory | Record artifact; catalog later |
```

Source: Phase context decisions D-01 through D-04 and live filesystem checks. [CITED: 23-CONTEXT.md] [VERIFIED: live filesystem]

### Pattern 2: Adapter-Local Generated Wire

**What:** Generate White wire classes into `Adapters.GameProtocol.White/Wire`, keep proto inputs immutable, and do not manually edit generated C# files. [CITED: AGENTS.md] [VERIFIED: Adapters.GameProtocol.Red/Wire/Game.cs]

**When to use:** Use for `proto/white/taiko.proto` and `proto/white/vsinterface.proto`. [CITED: .planning/REQUIREMENTS.md]

**Example:**

```powershell
.\.tools\protogen.exe --csharp_out=Adapters.GameProtocol.White\Wire -Iproto\white --package=TaikoLocalServer.Adapters.GameProtocol.White.Wire +nullablevaluetype=yes proto\white\taiko.proto
.\.tools\protogen.exe --csharp_out=Adapters.GameProtocol.White\Wire -Iproto\white --package=TaikoLocalServer.Adapters.GameProtocol.White.Wire +nullablevaluetype=yes proto\white\vsinterface.proto
git status --porcelain -- proto/white
```

Source: local protogen is available, Red generated wire uses the adapter namespace, and protobuf-net documents `NullableValueType` / `+nullablevaluetype`. [VERIFIED: local command] [VERIFIED: Adapters.GameProtocol.Red/Wire/Game.cs] [CITED: https://protobuf-net.github.io/protobuf-net/contract_first.html] [CITED: https://github.com/protobuf-net/protobuf-net/blob/main/src/protogen/Program.cs]

### Pattern 3: Enabled-Era Host Gating

**What:** Add White as a Host-known era only when configured, and remove the White application part when disabled. [VERIFIED: Host/Program.cs]

**When to use:** Use when adding `GameEra.White`, `AddGameProtocolWhite()`, Host project references, and `ServerSettings:Eras:White`. [VERIFIED: Host/Program.cs] [VERIFIED: Domain/Enums/GameEra.cs]

**Example:**

```csharp
if (enabledEras.Contains(GameEra.White))
{
    builder.Services.AddGameProtocolWhite();
}

// Adapter assemblies referenced by Host are auto-discovered as ApplicationParts.
// Remove disabled-era assemblies so their controllers are not routed.
if (!enabledEras.Contains(GameEra.White))
{
    RemoveApplicationPart(apm, "TaikoLocalServer.Adapters.GameProtocol.White");
}
```

Source: Host already uses this pattern for Green, Blue, Yellow, Red, and Nijiiro. [VERIFIED: Host/Program.cs] Microsoft docs state `ApplicationPartManager` tracks application parts and that parts can be added or removed to make resources available or hidden. [CITED: https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-10.0]

### Anti-Patterns to Avoid

- **Route cloning from Red or Yellow:** Red and Yellow are implementation precedents, not White route proof. [CITED: 23-CONTEXT.md]
- **Broad missing-content-type fallback:** A generic AC15 fallback would contradict the current exact-prefix allowlist and D-07. [VERIFIED: Host/Program.cs] [CITED: 23-CONTEXT.md]
- **Runtime behavior in Phase 23 controllers:** D-06 forbids Mediator handlers, EF writes, catalog reads, profile state, and runtime semantics in Phase 23 scaffold controllers. [CITED: 23-CONTEXT.md]
- **Generated-wire or proto edits by hand:** The repo rules and generated file headers prohibit manual generated-wire cleanup. [CITED: AGENTS.md] [VERIFIED: Adapters.GameProtocol.Red/Wire/Game.cs]
- **Tests that grep source or assert route attributes:** Repo testing rules reject source-shape tests unless a demonstrated runtime failure requires them. [CITED: AGENTS.md]

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Protobuf DTO generation | Manual C# wire DTOs | repo-local `protogen.exe` with `+nullablevaluetype=yes` | Existing AC15 generated wire uses protobuf-net output and optional primitive presence is part of the wire contract. [VERIFIED: local command] [VERIFIED: Adapters.GameProtocol.Red/Wire/Game.cs] [CITED: https://protobuf-net.github.io/protobuf-net/contract_first.html] |
| MVC disabled-era routing | Custom route filters | `ApplicationPartManager` removal | Host already removes disabled adapter assemblies, and ASP.NET Core supports hiding resources through application parts. [VERIFIED: Host/Program.cs] [CITED: https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-10.0] |
| Mapping generator | Handwritten one-to-one mappers | Mapperly partial mappers with generated-source inspection | Repo rules require Mapperly source generation and official docs define how to emit generated files. [CITED: AGENTS.md] [CITED: https://mapperly.riok.app/docs/configuration/generated-source/] |
| Data-root resolution | Hardcoded runtime paths in handlers | `PathHelper` / era data path helpers in later catalog phase | Repo rules require path helper resolution and Phase 23 only records the accepted White root. [CITED: AGENTS.md] [CITED: 23-CONTEXT.md] |
| Feature absence matrix | Large speculative unsupported-feature table | Simple evidence-tagged inventory categories | D-09 says unsupported White 0.13 surfaces should be ignored unless evidence supports them. [CITED: 23-CONTEXT.md] |

**Key insight:** Phase 23 complexity is evidence sequencing and Host integration, not new application behavior. [CITED: 23-CONTEXT.md] The planner should spend detail on proof gates, generated-wire reproducibility, disabled-era route safety, and existing-era preservation. [CITED: .planning/REQUIREMENTS.md] [VERIFIED: Host/Program.cs]

## Common Pitfalls

### Pitfall 1: Treating The Nonzero IDB As Route Proof

**What goes wrong:** The plan records the file size correction and then finalizes `/v07r00` routes without extracting route strings. [CITED: 23-CONTEXT.md]

**Why it happens:** Live filesystem evidence proves the IDB is nonzero, but the current CLI path did not extract route strings. [VERIFIED: live filesystem] [VERIFIED: ida-cli probe]

**How to avoid:** Create a blocking route-evidence task that uses the existing IDA session, a close-and-open IDA-CLI pass, or replacement request logs/captures. [CITED: 23-CONTEXT.md] [VERIFIED: local process check]

**Warning signs:** A route set appears in the plan because Red or Yellow has the route, because the proto has request/response types, or because `/v07r00` was expected but not proven. [CITED: 23-CONTEXT.md]

### Pitfall 2: Confusing Data Root Evidence With Runtime Root Proof

**What goes wrong:** `ST7100-1` file presence is treated as client runtime selection proof. [CITED: 23-CONTEXT.md]

**Why it happens:** The White data symlink points to operator data and contains `config/ST7100-1`, but D-03 only accepts it as the Phase 23 active root from local inventory. [VERIFIED: live filesystem] [CITED: 23-CONTEXT.md]

**How to avoid:** Record `ST7100-1` as accepted root evidence for Phase 23, and leave parser/profile/root runtime confirmation to Phase 24 unless Phase 23 IDB evidence contradicts it. [CITED: 23-CONTEXT.md]

**Warning signs:** Runtime handlers or catalogs are added in Phase 23. [CITED: 23-CONTEXT.md]

### Pitfall 3: Copying Red Or Yellow Surface Area

**What goes wrong:** White gets Red `challengecompe.php`, Yellow item shop, Banacoin, Tokkun, WaiWai, battle, or gacha behavior without White 0.13 evidence. [CITED: .planning/REQUIREMENTS.md] [VERIFIED: proto/white/taiko.proto]

**Why it happens:** White proto exposes normal AC15 surfaces and embedded challenge-like fields, but no standalone ChallengeCompe request/response, item shop purchase/info, Banacoin, battle, Tokkun, WaiWai, or gacha message family was found. [VERIFIED: proto/white/taiko.proto]

**How to avoid:** Keep Phase 23 route scaffolds limited to route-string evidence plus matching White proto request/response pairs. [CITED: 23-CONTEXT.md]

**Warning signs:** The White adapter route count matches Red or Yellow exactly. [VERIFIED: codebase grep] [CITED: 23-CONTEXT.md]

### Pitfall 4: Weak Existing-Era Preservation

**What goes wrong:** Host fallback or application-part changes expose disabled routes or break existing AC15 prefixes. [VERIFIED: Host/Program.cs]

**Why it happens:** Host references every adapter project, and MVC can auto-discover referenced adapter assemblies unless disabled application parts are removed. [VERIFIED: Host/Host.csproj] [VERIFIED: Host/Program.cs] [CITED: https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-10.0]

**How to avoid:** Add focused tests or manual route-feature verification for White enabled/disabled behavior and run existing settings/build checks after shared Host edits. [CITED: AGENTS.md] [VERIFIED: Tests/Red/RedServerSettingsValidationTests.cs]

**Warning signs:** White is added to `Host.csproj` but not to application-part removal, enabled-era registration, or exact fallback scope. [VERIFIED: Host/Program.cs] [VERIFIED: Host/Host.csproj]

### Pitfall 5: Fake Mapperly Verification

**What goes wrong:** A mapper compiles, but generated mapping code is not inspected for null handling, constants, ignores, or helper selection. [CITED: AGENTS.md]

**Why it happens:** Mapper declarations can look correct while generated source differs from intended behavior. [CITED: AGENTS.md] [CITED: https://mapperly.riok.app/docs/configuration/generated-source/]

**How to avoid:** If Phase 23 adds nontrivial mappers, run `dotnet build /p:EmitCompilerGeneratedFiles=true` and inspect `obj/.../generated/.../Riok.Mapperly/*.g.cs`. [CITED: https://mapperly.riok.app/docs/configuration/generated-source/] Mapperly docs state `AllowNullPropertyAssignment` defaults to true and strict mapping strategy can be configured, so mapper behavior should be checked rather than assumed. [CITED: https://mapperly.riok.app/docs/configuration/mapper/#null-values]

**Warning signs:** Mapper bodies are handwritten for mechanical projection or no generated `.g.cs` inspection is recorded. [CITED: AGENTS.md]

## Code Examples

Verified patterns from existing code and official sources:

### Host Exact-Prefix Fallback

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
           || path.StartsWithSegments("/v01r00/chassis", StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/v01r00_tw/chassis", StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/v12r08_ww/chassis", StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/v12r00_cn/chassis", StringComparison.OrdinalIgnoreCase);
}
```

Source: `Host/Program.cs`; add `/v07r00/chassis` only after D-07 route-prefix verification. [VERIFIED: Host/Program.cs] [CITED: 23-CONTEXT.md]

### Disabled-Era Application Part Removal

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

Source: `Host/Program.cs`; Microsoft docs confirm application parts can be added or removed to hide or expose MVC resources. [VERIFIED: Host/Program.cs] [CITED: https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-10.0]

### Red Adapter Project Shape

```xml
<Project Sdk="Microsoft.NET.Sdk">
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
</Project>
```

Source: `Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj`; White should mirror this shape with White names. [VERIFIED: Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj]

### Mapperly Generated Source Inspection

```powershell
dotnet build /p:EmitCompilerGeneratedFiles=true
Get-ChildItem -Recurse -Filter '*.g.cs' .\Adapters.GameProtocol.White\obj | Select-String 'Riok.Mapperly'
```

Source: Mapperly docs document `dotnet build /p:EmitCompilerGeneratedFiles=true` and the generated output path under `obj/.../generated/.../Riok.Mapperly`. [CITED: https://mapperly.riok.app/docs/configuration/generated-source/]

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Treat `.tools/white/EBOOT.ELF.i64` as zero bytes | Treat live filesystem evidence as authoritative and record current 129,893,515 byte IDB size | Phase 23 discussion on 2026-06-17 | Evidence artifact must correct stale STATE/ROADMAP/research notes. [VERIFIED: live filesystem] [CITED: 23-CONTEXT.md] |
| Add era route scaffolds from neighboring eras | Add only evidence-backed White route suffixes cross-checked against `proto/white` | Phase 23 D-05 | Planner must include a route-evidence gate before controller tasks. [CITED: 23-CONTEXT.md] |
| Broad AC15 assumptions | Exact prefix fallback and era-specific adapter ownership | Existing Host architecture | White should extend current exact allowlist only after proof. [VERIFIED: Host/Program.cs] |
| Source-only Mapperly confidence | Generated-source inspection after build | Repo AGENTS and Mapperly 4.3.1 docs | Verification should inspect emitted `.g.cs` for nontrivial White mappings. [CITED: AGENTS.md] [CITED: https://mapperly.riok.app/docs/configuration/generated-source/] |

**Deprecated/outdated:**

- The zero-byte White IDB note is stale in this checkout because the live `.tools/white/EBOOT.ELF.i64` is nonzero. [VERIFIED: live filesystem]
- Proto-only route inference is not acceptable for Phase 23 route scaffolds because D-05 requires `.php` route strings from White evidence and proto cross-checking. [CITED: 23-CONTEXT.md]
- Red/Yellow full route inventory copying is not acceptable because D-05 and D-09 require White evidence and simple absence of unsupported White surfaces. [CITED: 23-CONTEXT.md]

## Assumptions Log

No `[ASSUMED]` claims are used as implementation authority. Route prefix `/v07r00` remains an expected but unproven locked-context hypothesis until the evidence task proves it. [CITED: 23-CONTEXT.md]

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | None. | N/A | N/A |

## Open Questions

1. **Can the current White IDB produce route strings through IDA?**
   - What we know: The IDB file exists and is 129,893,515 bytes; raw text scanning found no full `.php` route paths; IDA-CLI failed to open the database with error code 4; `ida.exe` is running and several IDB sidecar files are locked. [VERIFIED: live filesystem] [VERIFIED: binary rg] [VERIFIED: ida-cli probe] [VERIFIED: local process check]
   - What's unclear: Whether the running IDA session can export the route strings, whether IDA-CLI needs the GUI session closed, or whether replacement evidence must come from logs/captures. [VERIFIED: ida-cli probe]
   - Recommendation: Planner should make binary/capture route proof a blocking first task before route controller implementation. [CITED: 23-CONTEXT.md]

2. **Which White route suffixes are Phase 23 scaffold candidates?**
   - What we know: `proto/white/taiko.proto` defines request/response message families for bookkeeping, gettelop, heartbeat, getfolder, taikojuku, initialdatacheck, tournamentcheck, BAID, mydonentry, userdata, playresult, selfbest, recommend, crownsdata, headclerk2, getreitai, rewardcardcheck, and rewardexecution. [VERIFIED: proto/white/taiko.proto]
   - What's unclear: Which of those proto surfaces are actual White 0.13 HTTP route strings. [CITED: 23-CONTEXT.md]
   - Recommendation: Use route-string evidence first, then cross-check suffixes against proto request/response pairs. [CITED: 23-CONTEXT.md]

3. **Should Phase 23 add no-state routes when route proof is partial?**
   - What we know: D-06 allows thin no-state scaffold controllers where route evidence supports them and forbids runtime behavior. [CITED: 23-CONTEXT.md]
   - What's unclear: Whether route evidence will cover the whole proto-backed surface or only a subset. [VERIFIED: binary rg]
   - Recommendation: Split route scaffold tasks by proven route group and leave unproven suffixes in the evidence artifact. [CITED: 23-CONTEXT.md]

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|-------------|-----------|---------|----------|
| .NET SDK | Build/test/generation verification | yes | 10.0.201 | None needed. [VERIFIED: local command] |
| repo-local protogen | White wire generation | yes | 3.2.52+f4db4afce3 | Restore protobuf-net.Protogen only with approval if missing on another machine. [VERIFIED: local command] |
| ripgrep | Code/proto/binary scans | yes | 15.1.0 | PowerShell `Select-String` for text files. [VERIFIED: local command] |
| IDA-CLI Python bridge | White IDB route extraction | partially | importable; open failed with error code 4 | Use running IDA session export, close/retry IDA-CLI, or request/cabinet logs/captures. [VERIFIED: local command] [VERIFIED: ida-cli probe] |
| IDA GUI process | Current White IDB state | running | `H:\ida92pro\ida.exe` | Do not kill automatically; coordinate route export or close/retry manually. [VERIFIED: local process check] |
| Context7 CLI | Library docs lookup | no | not found | Official docs were fetched directly. [VERIFIED: local command] [CITED: https://mapperly.riok.app/docs/configuration/generated-source/] |
| git | Proto immutability and commit | yes | 2.52.0.windows.1 | None needed. [VERIFIED: local command] |

**Missing dependencies with no fallback:**
- None for writing the plan; route-proof execution is blocked until IDA export, IDA-CLI retry, or replacement evidence is available. [VERIFIED: ida-cli probe]

**Missing dependencies with fallback:**
- Context7 CLI is missing; official documentation URLs were used instead. [VERIFIED: local command] [CITED: https://mapperly.riok.app/docs/configuration/generated-source/]

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 with Microsoft.NET.Test.Sdk 17.14.1. [VERIFIED: Directory.Packages.props] |
| Config file | `Tests/Tests.csproj`; no xunit runner config file was found. [VERIFIED: Tests/Tests.csproj] [VERIFIED: rg --files] |
| Quick run command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~White|FullyQualifiedName~RedServerSettingsValidationTests|FullyQualifiedName~StartupAuthController" -x` [VERIFIED: Tests/Tests.csproj] |
| Full suite command | `dotnet test Tests/Tests.csproj` [VERIFIED: Tests/Tests.csproj] |

### Phase Requirements -> Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|--------------|
| WFND-01 | White evidence artifact exists and records live IDB size, stale zero-byte correction, accepted `ST7100-1` root, route proof status, and unresolved gaps. | artifact review / build-adjacent | `git diff -- .planning/phases/23-white-evidence-and-era-foundation` | no - Wave 0 creates artifact. [CITED: .planning/REQUIREMENTS.md] |
| WFND-02 | White adapter compiles, generated wire exists from `proto/white`, Host config can enable/disable White, and disabled White routes are absent. | build + behavior | `dotnet build TaikoLocalServer.slnx` and focused Host route/feature test | no - Wave 0 creates White tests after scaffold. [CITED: .planning/REQUIREMENTS.md] [VERIFIED: Tests/Tests.csproj] |
| WFND-03 | Existing-era behavior stays preserved after shared Host/settings edits. | regression | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~ServerSettingsValidationTests|FullyQualifiedName~StartupAuthController"` | yes - existing Red/Yellow/Green/Blue settings and startup tests exist; White-specific preservation tests do not. [VERIFIED: Tests] |

### Sampling Rate

- **Per task commit:** Run the most local compile/test slice for touched files, such as adapter project build after wire generation or settings tests after `ServerSettings` edits. [VERIFIED: Tests/Tests.csproj]
- **Per wave merge:** Run `dotnet build TaikoLocalServer.slnx` plus focused existing-era tests affected by shared changes. [CITED: AGENTS.md]
- **Phase gate:** Run `dotnet test Tests/Tests.csproj`, `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"`, `git status --porcelain -- proto/white`, and Mapperly generated-source inspection if nontrivial mappers were added. [CITED: AGENTS.md] [VERIFIED: local command] [CITED: https://mapperly.riok.app/docs/configuration/generated-source/]

### Wave 0 Gaps

- [ ] `23-WHITE-EVIDENCE.md` - covers WFND-01 route/root/transport/IDB/gap evidence. [CITED: .planning/REQUIREMENTS.md]
- [ ] `23-WHITE-FEATURE-INVENTORY.md` or a section inside the evidence artifact - separates White 0.13 proven surfaces, later leads, other-era behavior, and unknowns. [CITED: .planning/ROADMAP.md]
- [ ] `Tests/White/WhiteServerSettingsValidationTests.cs` - proves White enabled settings do not require shop or challenge settings in Phase 23. [VERIFIED: Tests/Red/RedServerSettingsValidationTests.cs] [CITED: 23-CONTEXT.md]
- [ ] `Tests/White/WhiteHostRouteGatingTests.cs` or equivalent behavior-facing route-feature test - proves White routes are absent when White is disabled and present only when enabled. [CITED: .planning/REQUIREMENTS.md] [CITED: https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-10.0]
- [ ] Build-output/debug-junction check if Host project data rules are touched. [VERIFIED: Host/Host.csproj]

## Security Domain

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|------------------|
| V2 Authentication | no | Phase 23 adds cabinet protocol scaffolding, not user authentication. [CITED: 23-CONTEXT.md] |
| V3 Session Management | no | Phase 23 does not add sessions. [CITED: 23-CONTEXT.md] |
| V4 Access Control | yes | Disabled-era application-part removal must keep White routes absent when White is disabled. [VERIFIED: Host/Program.cs] [CITED: https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-10.0] |
| V5 Input Validation | yes | Direct protobuf deserialization should remain bounded to proven route prefixes and generated DTOs; no runtime handlers should trust unproven fields in Phase 23. [CITED: 23-CONTEXT.md] [VERIFIED: Host/Program.cs] |
| V6 Cryptography | no | Phase 23 adds no cryptography. [CITED: 23-CONTEXT.md] |

### Known Threat Patterns for ASP.NET Core Protocol Adapter

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| Disabled White adapter still routable because Host references the assembly | Elevation of Privilege | Remove the White adapter application part when `GameEra.White` is disabled and verify behavior-facing route absence. [VERIFIED: Host/Program.cs] [CITED: https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-10.0] |
| Missing-content-type fallback applied too broadly | Spoofing / Tampering | Add only exact proven White prefix to `ShouldAssumeProtobufRequest`; do not generalize to all AC15 paths. [VERIFIED: Host/Program.cs] [CITED: 23-CONTEXT.md] |
| Route scaffold logs full unbounded payloads | Information Disclosure | Log bounded request summaries for scaffold routes rather than full sensitive payloads unless evidence requires detailed diagnostic logging. [CITED: 23-CONTEXT.md] |
| Proto or generated wire files manually edited | Tampering | Generate wire from immutable proto inputs and verify `git status --porcelain -- proto/white` is clean after generation. [CITED: AGENTS.md] [VERIFIED: local command] |

## Sources

### Primary (HIGH confidence)

- `AGENTS.md` - repo architecture, era boundaries, Mapperly verification, and testing rules. [CITED: AGENTS.md]
- `.planning/phases/23-white-evidence-and-era-foundation/23-CONTEXT.md` - locked Phase 23 decisions D-01 through D-10 and scope boundary. [CITED: 23-CONTEXT.md]
- `.planning/REQUIREMENTS.md` - WFND-01, WFND-02, and WFND-03 requirement definitions. [CITED: .planning/REQUIREMENTS.md]
- `Host/Program.cs` - enabled-era DI registration, disabled application-part removal, and exact-prefix protobuf fallback. [VERIFIED: codebase grep]
- `Host/Host.csproj` - adapter project references, operator data exclusions, and debug data junction pattern. [VERIFIED: codebase grep]
- `Domain/Enums/GameEra.cs` - current era enum lacks White and keeps existing values. [VERIFIED: codebase grep]
- `Adapters.GameProtocol.Red/` - closest older-AC15 adapter foundation precedent. [VERIFIED: codebase grep]
- `proto/white/taiko.proto` and `proto/white/vsinterface.proto` - White protocol schema inputs. [VERIFIED: codebase grep]
- Live filesystem checks for `.tools/white/EBOOT.ELF.i64`, `Host/wwwroot/data/white/data`, and `.tools/protogen.exe`. [VERIFIED: live filesystem]

### Secondary (MEDIUM confidence)

- Mapperly official docs for null behavior, constants, and generated-source emission. [CITED: https://mapperly.riok.app/docs/configuration/mapper/#null-values] [CITED: https://mapperly.riok.app/docs/configuration/constant-generated-values/] [CITED: https://mapperly.riok.app/docs/configuration/generated-source/]
- Microsoft Learn ASP.NET Core Application Parts documentation for application part discovery/removal behavior. [CITED: https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-10.0]
- protobuf-net docs and source help text for `NullableValueType` / `+nullablevaluetype`. [CITED: https://protobuf-net.github.io/protobuf-net/contract_first.html] [CITED: https://github.com/protobuf-net/protobuf-net/blob/main/src/protogen/Program.cs]

### Tertiary (LOW confidence)

- Prior `.planning/research/*.md` White research was useful for leads but contained stale zero-byte IDB claims; live filesystem evidence supersedes those claims. [VERIFIED: live filesystem] [VERIFIED: codebase grep]

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - local package versions, tools, and adapter patterns were verified in the checkout. [VERIFIED: Directory.Packages.props] [VERIFIED: codebase grep]
- Architecture: HIGH for Host/adapter/generation shape, MEDIUM for final route set because IDA route extraction remains unresolved. [VERIFIED: Host/Program.cs] [VERIFIED: ida-cli probe]
- Pitfalls: HIGH for repo testing/Mapperly/Host pitfalls, MEDIUM for White-specific route proof because the current CLI probe failed. [CITED: AGENTS.md] [VERIFIED: ida-cli probe]

**Research date:** 2026-06-17
**Valid until:** 2026-07-17 for codebase stack patterns; route evidence must be refreshed immediately if `.tools/white` files, request captures, or IDA availability change. [VERIFIED: live filesystem]
