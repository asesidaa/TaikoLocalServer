# Phase 39: MOMOIRO Evidence and Era Foundation - Research

**Researched:** 2026-06-26
**Domain:** ASP.NET Core era adapter foundation, protobuf route scaffolding, enabled-era application-part gating
**Confidence:** MEDIUM - codebase wiring is directly verified, but the MOMOIRO route inventory is accepted from the current locked phase context rather than re-extracted from IDA offsets in this research run. [VERIFIED: codebase grep] [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md]

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
## Implementation Decisions

### Route and Evidence Scope
- Use the supplied MOMOIRO binary route inventory as the active game surface: `/v04r00/chassis/playresult.php`, `/baidcheck.php`, `/mydonentry.php`, `/userdata.php`, `/recommend.php`, `/selfbest.php`, `/heartbeat.php`, `/defaultsong.php`, `/bookkeeping.php`, `/songhash.php`, `/telopcheck.php`, and `/gettelop.php`.
- Preserve shared AC15 startup/version behavior under `/v01r00/chassis/startupauth.php`, `/verupauth.php`, and `/verupcomplete.php`; do not copy these into the MOMOIRO adapter.
- Keep proto-only route families absent when the binary route inventory does not include them, including `shoppingresult.php`, `bestscore.php`, `communicationlog.php`, and `mainichisong.php`.
- Treat `.tools/momoiro/` and `proto/momoiro` as local evidence inputs; repo implementation alone is not proof of MOMOIRO semantics.

### Foundation Wiring
- Add MOMOIRO as a first-class `GameEra` and adapter registration following KIMIDORI/Murasaki patterns, with era-owned generated wire DTOs and route controllers under `Adapters.GameProtocol.Momoiro`.
- Use direct protobuf request/response transport for game endpoints unless current MOMOIRO client evidence proves otherwise.
- Integrate MOMOIRO with Host settings, DI, application-part gating, and startup validation without changing other era behavior.
- Keep runtime endpoints conservative in this phase: controllers may map to existing no-state scaffolding only where the route is binary-proven, but no stateful runtime behavior should be claimed here.

### State and Scope Boundaries
- Do not introduce MOMOIRO gameplay persistence in Phase 39 except if a build requires inert type references; state tables and migrations belong to Phase 41/42.
- Do not wire AdminApi/WebUI MOMOIRO surfaces in Phase 39; those belong to Phase 43.
- Do not infer MOMOIRO Taikojuku, Tokkun, Banacoin, battle, Don Challenge, ChallengeCompe, event-folder, gacha, tournament, or item-shop authority from later AC15 eras.
- Keep the design simple: reuse existing older-AC15 composition patterns and only add MOMOIRO-specific seams where route prefix, generated wire, or evidence gates require them.

### the agent's Discretion
- The agent may choose the closest existing era pattern, expected to be KIMIDORI for root-era scaffolding and Murasaki/KIMIDORI for older AC15 route/controller organization, after checking live code.

### Deferred Ideas (OUT OF SCOPE)
## Deferred Ideas

- Phase 40: root-level catalog binding, limits, route behavior, song hash, default song, telop, recommendation, and crown placement research.
- Phase 41: MOMOIRO-owned userdata, self-best, favorites/recent, release, hash, and crown readback.
- Phase 42: MOMOIRO-owned normal playresult mutation, unlocks, rewards, Dan compatibility, and no-cross-era writes.
- Phase 43: AdminApi/WebUI MOMOIRO routing and supported-control filtering.
- Phase 44: full automated verification and cabinet/RPCS3 acceptance.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| MOFND-01 | MOMOIRO support records binary-proven route inventory, startup/version ownership, game prefix, direct-protobuf transport, evidence handles, and unresolved limit gaps before runtime behavior is claimed. [CITED: .planning/REQUIREMENTS.md] | Use a committed Phase 39 evidence matrix with the locked route table, `.tools/momoiro/EBOOT.ELF.i64`, `proto/momoiro`, and explicit "not yet proven" limit/crown/unlock rows. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] |
| MOFND-02 | MOMOIRO is a first-class `GameEra.Momoiro` with adapter registration, generated wire DTOs, Host settings, DI, application-part gating, and no enabled routes when disabled. [CITED: .planning/REQUIREMENTS.md] | Mirror current KIMIDORI/Murasaki foundation seams: `GameEra`, adapter project, Host references, settings, `GameProtocolApplicationParts`, `ShouldAssumeProtobufRequest`, and build/test wiring. [VERIFIED: codebase grep] |
| MOFND-03 | MOMOIRO startup/version endpoints use shared `/v01r00/chassis/*.php`; MOMOIRO game endpoints use `/v04r00/chassis/*.php`. [CITED: .planning/REQUIREMENTS.md] | Keep `startupauth.php`, `verupauth.php`, and `verupcomplete.php` in `Adapters.GameProtocol.Shared`; add only MOMOIRO game controllers under `Adapters.GameProtocol.Momoiro`. [VERIFIED: codebase grep] |
| MOFND-04 | MOMOIRO feature support requires both `proto/momoiro` message presence and corresponding binary/client `.php` route evidence; features failing either stay absent. [CITED: .planning/REQUIREMENTS.md] | `proto/momoiro` contains `BestScore`, `CommunicationLog`, `Mainichisong`, and `ShoppingResult` message families, but the locked Phase 39 route inventory excludes those route stubs. [VERIFIED: codebase grep] [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] |
</phase_requirements>

## Summary

Phase 39 should be planned as a foundation slice, not a runtime-state slice. The standard implementation is to add a first-class `GameEra.Momoiro`, a `Adapters.GameProtocol.Momoiro` project with generated `Wire/Game.cs` and `Wire/VsInterface.cs`, a `/v04r00/chassis` route-prefix constant, Host project references and DI calls, `ServerSettings.json` enablement, `GameProtocolApplicationParts` disabled-era removal, and direct-protobuf fallback for `/v04r00/chassis`. [VERIFIED: codebase grep] [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md]

The active route list is the locked supplied MOMOIRO inventory: shared `/v01r00/chassis/startupauth.php`, `/verupauth.php`, `/verupcomplete.php`, plus `/v04r00/chassis/playresult.php`, `/baidcheck.php`, `/mydonentry.php`, `/userdata.php`, `/recommend.php`, `/selfbest.php`, `/heartbeat.php`, `/defaultsong.php`, `/bookkeeping.php`, `/songhash.php`, `/telopcheck.php`, and `/gettelop.php`. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] This research run verified the local evidence handles and code seams, but did not independently record IDA route-string offsets; the planner should make the evidence matrix explicit about that provenance. [VERIFIED: codebase grep]

**Primary recommendation:** Use KIMIDORI for adapter/project shape and Murasaki/KIMIDORI for separate controller organization, but gate every MOMOIRO controller by the Phase 39 binary route list and defer all catalog/profile/persistence/AdminApi/WebUI behavior. [VERIFIED: codebase grep] [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md]

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|-------------|----------------|-----------|
| MOMOIRO era identity and enabled-era settings | Host / Backend | Domain | `GameEra` is in `Domain`, while Host reads enabled eras from `ServerSettings` and composes adapter services. [VERIFIED: codebase grep] |
| MOMOIRO game route registration | ASP.NET Core adapter layer | Host | Route controllers live in era adapter assemblies; Host controls controller discovery through MVC application parts. [VERIFIED: codebase grep] [CITED: https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-10.0] |
| Shared startup/version routes | Shared game-protocol adapter | Application handlers | `/v01r00/chassis/startupauth.php`, `/verupauth.php`, and `/verupcomplete.php` already live in `Adapters.GameProtocol.Shared`. [VERIFIED: codebase grep] |
| MOMOIRO wire DTOs | MOMOIRO adapter | `proto/momoiro` input files | Existing adapter wire files are generated per adapter from proto inputs and committed under `Wire/`. [VERIFIED: codebase grep] [CITED: AGENTS.md] |
| Runtime state, catalog, AdminApi, WebUI | Later phases | Application / Infrastructure / UI | Phase 39 context defers these surfaces to Phases 40-43. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] |

## Project Constraints (from AGENTS.md)

- MOMOIRO must be treated as a first-class era, not as KIMIDORI, Murasaki, or later AC15 behavior copied under a new name. [CITED: AGENTS.md]
- Era gameplay state must remain separate unless the data is truly shared identity state. [CITED: AGENTS.md]
- Controllers should deserialize, map, call Mediator, and map back; business behavior belongs in `Application/Handlers`. [CITED: AGENTS.md]
- Generated protobuf DTOs should be mapped through `Application/Dtos/Common*` shapes before handler logic; wire DTOs should not be persisted directly. [CITED: AGENTS.md]
- Generated `Wire/` files should not be hand-cleaned except as part of deliberate protocol regeneration. [CITED: AGENTS.md]
- Mapperly mappers must remain source-generator driven, and generated Mapperly output must be inspected for Mapperly-sensitive changes. [CITED: AGENTS.md]
- Runtime data roots should be resolved through `PathHelper` and era helpers rather than hardcoded handler filesystem paths. [CITED: AGENTS.md]
- Tests should protect observable behavior and boundaries; they should not assert source text, generated wire member existence, controller attribute lists, route inventory as source strings, DI shape, enum numeric values, project-file strings, or trivial `Result = 1` echoes without a real runtime failure. [CITED: AGENTS.md]
- Cabinet/RPCS3 acceptance remains the compatibility gate for game-facing behavior; server tests are regression guards, not proof of client compatibility. [CITED: AGENTS.md]

## Standard Stack

### Core
| Library / Tool | Version | Purpose | Why Standard |
|----------------|---------|---------|--------------|
| .NET SDK | 10.0.201 | Build and test the ASP.NET Core 10 solution. | Installed locally and matches the repo's .NET 10 package set. [VERIFIED: `dotnet --version`] |
| ASP.NET Core MVC | 10.0.7 pinned packages, framework reference in adapters | Controller routing, model binding, protobuf responses, application parts. | Existing Host uses `AddControllers().AddProtoBufNet().ConfigureApplicationPartManager(...)`. [VERIFIED: codebase grep] |
| `protobuf-net` | 3.2.56 | Generated protobuf DTO attributes and serialization. | Existing adapter projects use `protobuf-net` for generated `Wire/` DTOs. [VERIFIED: Directory.Packages.props] |
| `protobuf-net.AspNetCore` | 3.2.52 | ASP.NET Core protobuf input/output formatters. | Host calls `AddProtoBufNet()` for game protocol controllers. [VERIFIED: codebase grep] |
| `protobuf-net.Protogen` | 3.2.52 | Generate C# wire DTOs from `proto/momoiro`. | `protogen --version` is available locally and existing `Wire/` files carry protogen-style generated headers. [VERIFIED: `protogen --version`] |
| `Riok.Mapperly` | 4.3.1 | Source-generated adapter mapping. | Existing era adapters include Mapperly defaults with strict target mapping. [VERIFIED: codebase grep] |
| xUnit | 2.9.3 | Unit/regression tests. | `Tests/Tests.csproj` uses xUnit and Microsoft.NET.Test.Sdk. [VERIFIED: codebase grep] |

### Supporting
| Library / Tool | Version | Purpose | When to Use |
|----------------|---------|---------|-------------|
| Microsoft `ApplicationPartManager` | ASP.NET Core 10 | Remove disabled era adapter assemblies from controller discovery. | Use in Host MVC configuration, not inside each controller. [CITED: https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-10.0] |
| `Microsoft.AspNetCore.Mvc.Testing` | Not currently referenced | Full Host `WebApplicationFactory` route tests. | Do not add in Phase 39 unless the planner decides full TestServer route probes are worth a new test dependency. [VERIFIED: codebase grep] [CITED: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0] |
| `rg` | 15.1.0 | Fast repo and binary-string evidence searches. | Use for route/proto/code audits before planning and verification. [VERIFIED: `rg --version`] |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| `ApplicationPartManager` removal | Per-controller runtime `if era enabled` checks | Avoid; disabled controllers would still be discoverable and route ownership would be harder to verify. [CITED: https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-10.0] |
| Generated protobuf DTOs | Hand-written request/response classes | Avoid; repo convention is generated wire DTOs under adapter `Wire/` folders. [CITED: AGENTS.md] |
| Full Host `WebApplicationFactory` tests | Controller-discovery/application-part unit tests plus Host build | WebApplicationFactory is stronger but requires adding a test dependency absent from the current test project. [VERIFIED: codebase grep] [CITED: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0] |

**Installation:**
```bash
# No new external package install is required for the recommended Phase 39 foundation.
```

**Version verification:** Versions above were read from `Directory.Packages.props`, local SDK/tool commands, and current project files; publish dates were not checked because Phase 39 should use repo-pinned packages rather than upgrade dependencies. [VERIFIED: codebase grep]

## Package Legitimacy Audit

No new external packages are recommended for Phase 39. [VERIFIED: codebase grep]

| Package | Registry | Age | Downloads | Source Repo | Verdict | Disposition |
|---------|----------|-----|-----------|-------------|---------|-------------|
| None | n/a | n/a | n/a | n/a | n/a | No install |

**Packages removed due to [SLOP] verdict:** none.
**Packages flagged as suspicious [SUS]:** none.

## Architecture Patterns

### System Architecture Diagram

```text
ServerSettings:Eras
        |
        v
GameProtocolApplicationParts.ReadEnabledEras
        |
        +--> Host DI registration: AddGameProtocolMomoiro only when enabled
        |
        v
AddControllers().AddProtoBufNet().ConfigureApplicationPartManager(...)
        |
        v
Remove disabled adapter assemblies from ApplicationParts
        |
        +--> shared /v01r00/chassis/startupauth.php, verupauth.php, verupcomplete.php
        |
        +--> enabled MOMOIRO /v04r00/chassis/{binary-proven}.php controllers
        |
        +--> absent proto-only MOMOIRO route families stay undiscovered
```

This diagram follows the current Host and shared-adapter composition pattern. [VERIFIED: codebase grep]

### Recommended Project Structure

```text
Adapters.GameProtocol.Momoiro/
|-- Adapters.GameProtocol.Momoiro.csproj      # adapter project, framework reference, Application/shared refs
|-- MomoiroAdapterMarker.cs                   # assembly marker
|-- MomoiroRoutePrefixes.cs                   # Game = "/v04r00/chassis"
|-- DependencyInjection.cs                    # AddGameProtocolMomoiro extension
|-- MapperlyDefaults.cs                       # same strict defaults as KIMIDORI
|-- GlobalUsings.cs                           # adapter-local common usings
|-- Wire/
|   |-- Game.cs                               # generated from proto/momoiro/taiko.proto
|   `-- VsInterface.cs                        # generated from proto/momoiro/vsinterface.proto
`-- Controllers/
    |-- BaidController.cs
    |-- BookkeepingController.cs
    |-- DefaultSongController.cs
    |-- GetTelopController.cs
    |-- HeartbeatController.cs
    |-- MyDonEntryController.cs
    |-- PlayResultController.cs
    |-- RecommendController.cs
    |-- SelfBestController.cs
    |-- SongHashController.cs
    |-- TelopCheckController.cs
    `-- UserDataController.cs
```

The controller list above is exactly the locked Phase 39 game-route inventory and intentionally excludes `shoppingresult.php`, `bestscore.php`, `communicationlog.php`, and `mainichisong.php`. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md]

### Pattern 1: Enabled-Era Application-Part Gating

**What:** Host reads enabled eras, registers enabled adapter services, and removes disabled adapter assemblies from MVC application parts before controller discovery. [VERIFIED: codebase grep]  
**When to use:** Use for MOMOIRO disabled-route absence; do not add controller-local era checks. [CITED: https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-10.0]

**Example:**
```csharp
// Source: Host/Program.cs and Adapters.GameProtocol.Shared/GameProtocolApplicationParts.cs
builder.Services.AddControllers()
    .AddProtoBufNet()
    .ConfigureApplicationPartManager(apm =>
    {
        GameProtocolApplicationParts.RemoveDisabledGameProtocolApplicationParts(apm, enabledEras);
    });
```

### Pattern 2: Shared Startup, Era-Owned Game Routes

**What:** Keep startup/version controllers in `Adapters.GameProtocol.Shared` under `/v01r00/chassis`, and put only game-route controllers in the era adapter. [VERIFIED: codebase grep]  
**When to use:** Use for MOMOIRO because Phase 39 locks shared startup/version ownership and `/v04r00/chassis` game ownership. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md]

**Example:**
```csharp
// Source: Adapters.GameProtocol.Kimidori/KimidoriRoutePrefixes.cs
public static class MomoiroRoutePrefixes
{
    public const string Game = "/v04r00/chassis";
}
```

### Pattern 3: Evidence Matrix Before Behavior

**What:** Record route presence, proto message presence, implementation action, and deferred behavior before adding stateful handlers. [CITED: .planning/REQUIREMENTS.md]  
**When to use:** Use for every MOMOIRO route and every proto-only absent family in Phase 39. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md]

**Example:**
```markdown
| Route family | Proto message present | Binary route inventory | Phase 39 action |
|--------------|-----------------------|------------------------|-----------------|
| songhash.php | yes | yes, /v04r00/chassis/songhash.php | route scaffold only; behavior in Phase 40 |
| shoppingresult.php | yes | no in locked inventory | absent; no controller |
```

### Anti-Patterns to Avoid

- **Copying KIMIDORI controllers wholesale:** KIMIDORI has active routes that MOMOIRO must keep absent, including `bestscore.php`, `communicationlog.php`, `mainichisong.php`, and `shoppingresult.php`. [VERIFIED: codebase grep] [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md]
- **Adding `Momoiro` to AdminApi/WebUI in Phase 39:** AdminApi/WebUI routing is explicitly Phase 43 scope. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md]
- **Adding `Ac15EraProfiles.Momoiro` with guessed limits:** Protocol limits, crown placement, and catalog profile binding are Phase 40 work. [CITED: .planning/REQUIREMENTS.md]
- **Testing controller attributes or route inventory source strings:** AGENTS.md forbids low-value tests over controller attribute lists, route inventory, generated wire member existence, source text, and project file strings. [CITED: AGENTS.md]

## MOMOIRO Evidence Matrix Input

| Route / Family | Prefix | Proto Present | Binary Inventory Status | Phase 39 Action |
|----------------|--------|---------------|-------------------------|-----------------|
| `startupauth.php` | `/v01r00/chassis` | `proto/momoiro/vsinterface.proto` has message family. [VERIFIED: codebase grep] | Locked as shared startup route. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] | Use shared controller; do not duplicate in Momoiro adapter. |
| `verupauth.php` | `/v01r00/chassis` | `proto/momoiro/vsinterface.proto` has message family. [VERIFIED: codebase grep] | Locked as shared version route. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] | Use shared controller; do not duplicate in Momoiro adapter. |
| `verupcomplete.php` | `/v01r00/chassis` | `proto/momoiro/vsinterface.proto` has message family. [VERIFIED: codebase grep] | Locked as shared version route. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] | Use shared controller; do not duplicate in Momoiro adapter. |
| `playresult.php` | `/v04r00/chassis` | Present. [VERIFIED: codebase grep] | Locked active game route. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] | Add route scaffold only; no persistence. |
| `baidcheck.php` | `/v04r00/chassis` | Present. [VERIFIED: codebase grep] | Locked active game route. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] | Add route scaffold only; no Momoiro save tables. |
| `mydonentry.php` | `/v04r00/chassis` | Present. [VERIFIED: codebase grep] | Locked active game route. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] | Add route scaffold only; no Momoiro save tables. |
| `userdata.php` | `/v04r00/chassis` | Present and includes `hash_crown_flg`. [VERIFIED: codebase grep] | Locked active game route. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] | Add route scaffold only; readback behavior in Phase 41. |
| `recommend.php` | `/v04r00/chassis` | Present. [VERIFIED: codebase grep] | Locked active game route. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] | Add route scaffold only; recommendation behavior in Phase 40. |
| `selfbest.php` | `/v04r00/chassis` | Present. [VERIFIED: codebase grep] | Locked active game route. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] | Add route scaffold only; score readback in Phase 41. |
| `heartbeat.php` | `/v04r00/chassis` | Present. [VERIFIED: codebase grep] | Locked active game route. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] | Static operational stub allowed; avoid extra Banacoin/status fields unless proto proves them. |
| `defaultsong.php` | `/v04r00/chassis` | Present. [VERIFIED: codebase grep] | Locked active game route. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] | Route scaffold only; behavior in Phase 40. |
| `bookkeeping.php` | `/v04r00/chassis` | Present. [VERIFIED: codebase grep] | Locked active game route. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] | Static log/success stub allowed; no persistence. |
| `songhash.php` | `/v04r00/chassis` | Present. [VERIFIED: codebase grep] | Locked active game route. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] | Route scaffold only; hash table behavior in Phase 40. |
| `telopcheck.php` | `/v04r00/chassis` | Present. [VERIFIED: codebase grep] | Locked active game route. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] | Route scaffold only; telop behavior in Phase 40. |
| `gettelop.php` | `/v04r00/chassis` | Present. [VERIFIED: codebase grep] | Locked active game route. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] | Route scaffold only; telop behavior in Phase 40. |
| `shoppingresult.php` | n/a | Present in proto. [VERIFIED: codebase grep] | Absent from locked inventory. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] | No controller. |
| `bestscore.php` | n/a | Present in proto. [VERIFIED: codebase grep] | Absent from locked inventory. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] | No controller. |
| `communicationlog.php` | n/a | Present in proto. [VERIFIED: codebase grep] | Absent from locked inventory. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] | No controller. |
| `mainichisong.php` | n/a | Present in proto. [VERIFIED: codebase grep] | Absent from locked inventory. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] | No controller. |

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Protobuf model binding and serialization | Custom byte parser or manual request stream decoder | `protobuf-net.AspNetCore` and generated `protobuf-net` DTOs | Host already uses this formatter pipeline for game protocol controllers. [VERIFIED: codebase grep] |
| Disabled route gating | Per-controller flags or route filters | `ApplicationPartManager` removal in Host | Microsoft docs support adding/removing application parts to hide or expose resources; repo already uses this pattern. [CITED: https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-10.0] [VERIFIED: codebase grep] |
| MOMOIRO feature detection | "Proto has message, so add route" heuristic | Proto presence plus locked binary route inventory | MOFND-04 requires both protocol and route evidence. [CITED: .planning/REQUIREMENTS.md] |
| Runtime behavior | Copied KIMIDORI/Murasaki handlers | Later MOMOIRO-specific phases | Phase 39 excludes gameplay persistence, catalog mutation, AdminApi, and WebUI behavior. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] |

**Key insight:** The complexity is not making controllers compile; it is preventing future phases from mistaking generated proto messages or adjacent-era behavior for MOMOIRO client route authority. [CITED: .planning/REQUIREMENTS.md]

## Common Pitfalls

### Pitfall 1: Proto-Only Routes Become Stubs
**What goes wrong:** `shoppingresult.php`, `bestscore.php`, `communicationlog.php`, or `mainichisong.php` get added because `proto/momoiro` contains message families. [VERIFIED: codebase grep]  
**Why it happens:** KIMIDORI contains these controllers, and copying KIMIDORI scaffolding blindly pulls them in. [VERIFIED: codebase grep]  
**How to avoid:** Drive controller creation from the locked Phase 39 route inventory, not from proto message enumeration alone. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md]  
**Warning signs:** New Momoiro files named `ShoppingResultController`, `BestScoreController`, `CommunicationLogController`, or `MainichiSongController`. [VERIFIED: codebase grep]

### Pitfall 2: Shared Startup Routes Get Duplicated
**What goes wrong:** The Momoiro adapter defines `/v01r00/chassis/startupauth.php`, `/verupauth.php`, or `/verupcomplete.php`. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md]  
**Why it happens:** Generated `VsInterface.cs` includes startup/version messages. [VERIFIED: codebase grep]  
**How to avoid:** Keep those controllers in `Adapters.GameProtocol.Shared` and use the Momoiro `VsInterface.cs` only as evidence or future compatibility reference unless shared wire needs regeneration. [VERIFIED: codebase grep]  
**Warning signs:** A `Controllers/StartupAuthController.cs` file inside `Adapters.GameProtocol.Momoiro`. [CITED: AGENTS.md]

### Pitfall 3: Direct-Protobuf Fallback Misses `/v04r00`
**What goes wrong:** Controllers exist but no-content-type cabinet posts fail or model binding behaves differently. [VERIFIED: codebase grep]  
**Why it happens:** Host has a static `ShouldAssumeProtobufRequest` path allowlist and currently includes KIMIDORI `/v05r00/chassis` but not MOMOIRO `/v04r00/chassis`. [VERIFIED: codebase grep]  
**How to avoid:** Add `MomoiroRoutePrefixes.Game` to `ShouldAssumeProtobufRequest` with the other AC15 game prefixes. [VERIFIED: codebase grep]  
**Warning signs:** Route returns 415/400 in runtime logs even though route discovery works. [ASSUMED]

### Pitfall 4: First-Class Enum Leaks Into Deferred Surfaces
**What goes wrong:** Adding `GameEra.Momoiro` causes AdminApi/WebUI/profile code to expose unsupported controls or bad routes. [VERIFIED: codebase grep]  
**Why it happens:** Some AdminApi and WebUI helpers parse `GameEra` broadly or maintain supported-era lists separately. [VERIFIED: codebase grep]  
**How to avoid:** Do not add Momoiro to `WebUiEra.Supported`, AdminApi switches, `Ac15EraProfiles`, catalog extensions, or persistence dispatch in Phase 39 unless needed only as an inert build seam. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md]  
**Warning signs:** Files under `TaikoWebUI/`, `Adapters.AdminApi/`, `Infrastructure/Persistence/`, or runtime handler partials changed in a foundation-only plan. [VERIFIED: codebase grep]

## Code Examples

### Add MOMOIRO To Application-Part Gating
```csharp
// Source: Adapters.GameProtocol.Shared/GameProtocolApplicationParts.cs
if (!enabledEras.Contains(GameEra.Momoiro))
{
    RemoveApplicationPart(apm, "TaikoLocalServer.Adapters.GameProtocol.Momoiro");
}
```

### Add Host DI Registration
```csharp
// Source pattern: Host/Program.cs
if (enabledEras.Contains(GameEra.Momoiro))
{
    builder.Services.AddGameProtocolMomoiro();
}
```

### Add Direct-Protobuf Path Handling
```csharp
// Source pattern: Host/Program.cs
return path.StartsWithSegments(MomoiroRoutePrefixes.Game, StringComparison.OrdinalIgnoreCase)
       || path.StartsWithSegments("/v01r00/chassis", StringComparison.OrdinalIgnoreCase);
```

### Controller Skeleton For Route-Proven Static Endpoint
```csharp
// Source pattern: Adapters.GameProtocol.Kimidori/Controllers/HeartbeatController.cs
[ApiController]
public sealed class HeartbeatController : BaseProtocolController<HeartbeatController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/heartbeat.php")]
    [Produces("application/protobuf")]
    public IActionResult Heartbeat([FromBody] HeartBeatRequest request)
    {
        Logger.LogInformation("Momoiro heartbeat.php request: {@Request}", request);
        return Ok(new HeartBeatResponse { Result = 1 });
    }
}
```

Use the skeleton only for route-proven endpoints and do not extrapolate fields or state side effects from later eras. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md]

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Add route stubs from generated proto messages alone | Require proto presence plus matching binary/client `.php` route evidence | Locked in current v1.7 requirements and context. [CITED: .planning/REQUIREMENTS.md] | Prevents proto-only MOMOIRO routes from becoming server behavior. |
| Enable/disable routes with scattered controller checks | Remove disabled adapter assemblies from MVC application parts | Existing Host pattern and Microsoft application-parts guidance. [VERIFIED: codebase grep] [CITED: https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-10.0] | Disabled eras have no discovered controllers. |
| Treat AdminApi/WebUI as automatic after adding a `GameEra` | Add UI/API surfaces only in their scoped phase | Locked Phase 39 context defers AdminApi/WebUI to Phase 43. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] | Avoids exposing unsupported Momoiro controls. |

**Deprecated/outdated:**
- Copying KIMIDORI route inventory as a MOMOIRO shortcut is invalid for this phase because the locked MOMOIRO inventory excludes routes KIMIDORI currently implements. [VERIFIED: codebase grep] [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md]

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | A no-content-type post may fail if `/v04r00/chassis` is missing from `ShouldAssumeProtobufRequest`; the exact failure status is inferred from ASP.NET Core formatter behavior, not reproduced in this run. | Common Pitfalls | Planner should verify with a smoke request or build/runtime log if route probes are added. |

## Open Questions

1. **Should Phase 39 controller stubs return minimal success or another explicit "not implemented yet" shape?**
   - What we know: Phase 39 requires routes to be registered but forbids claiming runtime state behavior. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md]
   - What's unclear: The exact static response payloads for stateful-looking routes such as `userdata.php`, `selfbest.php`, and `playresult.php` are not locked by this research. [CITED: .planning/REQUIREMENTS.md]
   - Recommendation: Prefer minimal protobuf response objects that compile and make route ownership testable, and label them as no-state scaffolds in the evidence matrix. [VERIFIED: codebase grep]

2. **Should Phase 39 re-open IDA to record exact route offsets?**
   - What we know: The phase context locks a supplied binary route inventory and the local evidence handle is `.tools/momoiro/EBOOT.ELF.i64`. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] [VERIFIED: codebase grep]
   - What's unclear: Exact IDA string addresses were not captured in this research run. [VERIFIED: codebase grep]
   - Recommendation: If the planner wants stronger provenance, add a small Wave 0 evidence task to capture route-string handles with IDA; otherwise cite the supplied inventory and avoid claiming fresh offset proof. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md]

3. **Should `Ac15EraProfiles.Momoiro` exist in Phase 39?**
   - What we know: Profile limits, crown placement, catalog binding, and route behavior are Phase 40 scope. [CITED: .planning/REQUIREMENTS.md]
   - What's unclear: Some future handlers may need an inert profile placeholder, but Phase 39 can avoid those handlers. [VERIFIED: codebase grep]
   - Recommendation: Do not add `Ac15EraProfiles.Momoiro` in Phase 39 unless a compile seam requires it; if added, mark it placeholder-only and schedule Phase 40 replacement. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md]

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|-------------|-----------|---------|----------|
| .NET SDK | Build/test | yes | 10.0.201 | None needed. [VERIFIED: `dotnet --version`] |
| `protogen` | Wire generation from `proto/momoiro` | yes | 3.2.52 | Existing generated-file pattern can be copied only after generation. [VERIFIED: `protogen --version`] |
| `rg` | Evidence/code audits | yes | 15.1.0 | PowerShell `Select-String`, slower. [VERIFIED: `rg --version`] |
| Git | Commit research and future implementation | yes | 2.52.0.windows.1 | None needed. [VERIFIED: `git --version`] |
| MOMOIRO proto inputs | Wire generation | yes | `proto/momoiro/taiko.proto`, `proto/momoiro/vsinterface.proto` | Block if missing. [VERIFIED: codebase grep] |
| MOMOIRO IDB evidence handle | Evidence matrix | yes | `.tools/momoiro/EBOOT.ELF.i64` | Use locked context inventory; re-open IDA if offsets are required. [VERIFIED: codebase grep] |
| MOMOIRO data root | Later catalog phase | yes | `Host/wwwroot/data/momoiro/data` exists locally | Not required for Phase 39. [VERIFIED: codebase grep] |

**Missing dependencies with no fallback:** none for Phase 39 research. [VERIFIED: environment probe]

**Missing dependencies with fallback:** `Microsoft.AspNetCore.Mvc.Testing` is not referenced; use application-part/controller-discovery tests plus build unless the planner deliberately adds a Host integration-test dependency. [VERIFIED: codebase grep]

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 with Microsoft.NET.Test.Sdk 17.14.1. [VERIFIED: Directory.Packages.props] |
| Config file | `Tests/Tests.csproj`; no separate xUnit config found during scan. [VERIFIED: codebase grep] |
| Quick run command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Momoiro|FullyQualifiedName~GameProtocolApplicationParts"` |
| Full suite command | `dotnet test Tests/Tests.csproj` |

### Phase Requirements -> Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|--------------|
| MOFND-01 | Evidence matrix records active shared routes, active `/v04r00` game routes, proto-only absent route families, evidence handles, and unresolved limit gaps. [CITED: .planning/REQUIREMENTS.md] | artifact review plus grep | `rg -n "shoppingresult|bestscore|communicationlog|mainichisong|v04r00|v01r00" .planning/phases/39-momoiro-evidence-and-era-foundation` | no, Wave 0 |
| MOFND-02 | Disabled Momoiro removes Momoiro adapter controllers from MVC application parts. [CITED: .planning/REQUIREMENTS.md] | unit/integration-light | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroApplicationPart"` | no, Wave 0 |
| MOFND-03 | Shared startup/version remains shared and Momoiro game prefix is `/v04r00/chassis`. [CITED: .planning/REQUIREMENTS.md] | build plus smoke/manual route probe | `dotnet build Host/Host.csproj -o "$env:TEMP\\TaikoLocalServer-host-build"` | no, Wave 0 |
| MOFND-04 | Proto-only route families remain absent from Momoiro controller discovery. [CITED: .planning/REQUIREMENTS.md] | application-part/controller discovery | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroRouteSurface"` | no, Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Momoiro|FullyQualifiedName~GameProtocolApplicationParts"`
- **Per wave merge:** `dotnet build TaikoLocalServer.slnx`
- **Phase gate:** `dotnet build Host/Host.csproj -o "$env:TEMP\\TaikoLocalServer-host-build"` plus focused Momoiro tests and route/evidence artifact review. [CITED: AGENTS.md]

### Wave 0 Gaps
- [ ] `Tests/Momoiro/MomoiroApplicationPartTests.cs` - verifies enabled/disabled application-part discovery and absent proto-only controller types without asserting source strings. [CITED: AGENTS.md]
- [ ] `Tests/Momoiro/MomoiroServerSettingsValidationTests.cs` - verifies Momoiro can be enabled without shop/don-challenge settings, mirroring Red/White/KIMIDORI foundation expectations. [VERIFIED: codebase grep]
- [ ] `Tests/Tests.csproj` - add `Adapters.GameProtocol.Momoiro` project reference if controller/application-part tests reference the new adapter assembly. [VERIFIED: codebase grep]

## Security Domain

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|------------------|
| V2 Authentication | no for cabinet game routes in Phase 39 | Existing cabinet protocol routes are unauthenticated compatibility endpoints; do not add AdminApi/WebUI auth surfaces in this phase. [VERIFIED: codebase grep] [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] |
| V3 Session Management | no | Phase 39 does not introduce session state. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] |
| V4 Access Control | yes for route exposure | Disabled-era application-part removal is the access boundary for whether Momoiro game routes exist. [VERIFIED: codebase grep] |
| V5 Input Validation | yes | Use typed protobuf DTO model binding and avoid custom parsers. [VERIFIED: codebase grep] |
| V6 Cryptography | no | Phase 39 does not introduce cryptographic behavior. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] |

### Known Threat Patterns for ASP.NET Core Game Protocol Routes

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| Unsupported MOMOIRO route exposure | Elevation of privilege / Tampering | Remove disabled adapter assemblies and omit proto-only controllers. [VERIFIED: codebase grep] [CITED: .planning/REQUIREMENTS.md] |
| Cross-era state writes from copied handlers | Tampering | Do not wire Momoiro route stubs to KIMIDORI/Murasaki persistence or handlers in Phase 39. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md] |
| Malformed protobuf body | Denial of service / Tampering | Use ASP.NET Core model binding with `protobuf-net.AspNetCore`, request body limits already configured by Host logging, and avoid custom parsing. [VERIFIED: codebase grep] |

## Sources

### Primary (HIGH confidence)
- `AGENTS.md` - repo architecture and testing constraints. [CITED: AGENTS.md]
- `.planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md` - locked Phase 39 decisions and deferred scope. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md]
- `.planning/REQUIREMENTS.md` - MOFND requirement definitions. [CITED: .planning/REQUIREMENTS.md]
- `Domain/Enums/GameEra.cs`, `Host/Program.cs`, `Adapters.GameProtocol.Shared/GameProtocolApplicationParts.cs`, `Adapters.GameProtocol.Kimidori/*`, `Adapters.GameProtocol.Murasaki/*`, `proto/momoiro/*`, `.tools/momoiro/EBOOT.ELF.i64` - live code and local evidence handles inspected in this run. [VERIFIED: codebase grep]

### Secondary (MEDIUM confidence)
- Microsoft Learn, "Share controllers, views, Razor Pages and more with Application Parts in ASP.NET Core" - application parts, resource removal, controller discovery requirements, last updated 2025-08-28. [CITED: https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-10.0]
- Microsoft Learn, "Integration tests in ASP.NET Core" - `WebApplicationFactory`/TestServer integration-test pattern, current `view=aspnetcore-10.0`. [CITED: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0]
- Microsoft Learn, `WebApplicationFactory<TEntryPoint>` API - TestServer-backed client creation and dependency context behavior, current `view=aspnetcore-10.0`. [CITED: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.testing.webapplicationfactory-1?view=aspnetcore-10.0]

### Tertiary (LOW confidence)
- Memory-derived KIMIDORI planning reminders were used only as background; current RESEARCH recommendations are based on live files and locked Phase 39 context. [CITED: C:/Users/10614/.codex/memories/MEMORY.md]

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - versions were read from the local SDK/tooling and repo-pinned package files. [VERIFIED: codebase grep]
- Architecture: HIGH - Host/application-part/adapter patterns were verified in live source and cross-checked against Microsoft application-part docs. [VERIFIED: codebase grep] [CITED: https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-10.0]
- Route inventory: MEDIUM - locked by current Phase 39 context and PROJECT/REQUIREMENTS, but exact IDA offsets were not re-captured during this research run. [CITED: .planning/phases/39-momoiro-evidence-and-era-foundation/39-CONTEXT.md]
- Pitfalls: HIGH - pitfalls are based on direct KIMIDORI/Murasaki code comparison and current AGENTS testing constraints. [VERIFIED: codebase grep] [CITED: AGENTS.md]

**Research date:** 2026-06-26
**Valid until:** 2026-07-03 for route/evidence planning; refresh if MOMOIRO IDA evidence, proto inputs, or Phase 39 context changes. [CITED: .planning/STATE.md]
