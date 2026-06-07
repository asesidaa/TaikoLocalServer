# Phase 12: Yellow Evidence and Era Foundation - Research

**Researched:** 2026-06-07
**Domain:** ASP.NET Core game-protocol adapter foundation, protobuf wire generation, era gating, and route/source guard tests
**Confidence:** HIGH for repo/proto patterns; MEDIUM for Yellow transport/prefix because no local Yellow runtime log or client binary evidence was found

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
## Implementation Decisions

### Foundation Scope
- Yellow is a first-class era, not a Blue or Green variant.
- Yellow adapter routes, generated wire DTOs, tests, settings, and host registration must be era-owned.
- Shared AC15 startup/version routes may remain under `/v01r00/chassis/*` only where current Yellow evidence supports shared ownership.
- Yellow game routes should use direct protobuf transport where local Yellow proto/client evidence supports it.

### Evidence Boundary
- Local proto, generated wire, route/source tests, local data layout, and later runtime logs outrank public wiki text.
- Phase 12 should record unresolved client-evidence gaps rather than invent route behavior.
- Yellow battle behavior is absent unless concrete Yellow proto/log/client evidence proves otherwise.

### Architecture Constraints
- Keep generated wire models inside a Yellow adapter project; do not create a shared AC15 wire assembly.
- Keep controller logic thin: deserialize/map/call Mediator/map response. Business behavior belongs in Application handlers in later phases.
- Use existing era adapter patterns from Blue/Green and shared protocol controller helpers where behavior really matches.
- Do not create Yellow persistence tables or runtime normal/Tokkun behavior in this phase unless a minimal compile/route scaffold requires a no-state placeholder.

### the agent's Discretion
- Choose the smallest adapter scaffold and test set that proves the Phase 12 success criteria.
- Reuse existing Blue/Green route skeleton and source-guard test patterns where they fit Yellow exactly.

### Deferred Ideas (OUT OF SCOPE)
Yellow catalog loading, profile/userdata, normal play, Dani, shop/medals, WaiWai, Tokkun, Banacoin compatibility, AdminApi/WebUI readback, and runtime RPCS3/cabinet smoke verification are deferred to Phases 13-17.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| YFND-01 | Developer can review a Yellow route/version evidence record that identifies supported endpoints, startup/version routing, direct-protobuf transport expectations, and unresolved client-evidence gaps. | Yellow message inventory comes from `proto/yellow/yellow.proto` and `proto/yellow/vsinterface.proto`; shared startup/version compatibility is supported by byte-identical Yellow/Green/Blue `vsinterface.proto`; exact Yellow game route prefix and direct-body runtime proof remain explicit evidence gaps. [VERIFIED: proto] [VERIFIED: repo] |
| YFND-02 | Yellow cabinet routes are served by a first-class Yellow adapter with generated Yellow wire DTOs, era settings, host registration, and route ownership tests. | Existing Blue/Green adapter project files, DI stubs, controller routes, generated `Wire/` folders, solution references, Host references, and xUnit route tests define the foundation pattern. [VERIFIED: repo] |
| YFND-03 | Yellow game routes are present only when Yellow is enabled, and disabled Yellow routes remain absent from the host. | `Host/Program.cs` parses `ServerSettings:Eras`, conditionally registers adapter services, and removes disabled adapter application parts; Yellow should add the same enum/settings/DI/application-part branch and source/runtime guard tests. [VERIFIED: repo] |
| YFND-04 | Yellow battle behavior is proven absent by proto/route tests; no Yellow battleuserdata route, battle fields, battle persistence, or Blue battle fallback is exposed. | Yellow proto lacks Blue battle request/response and initial-data/playresult battle fields; Blue battle fields are visible only in `proto/blue/taiko.proto`; existing Blue battle source guards provide the test pattern. [VERIFIED: proto] [VERIFIED: tests] |
</phase_requirements>

## Summary

Phase 12 should add Yellow as a thin, first-class era foundation: enum/settings surface, adapter project, generated Yellow wire DTOs, Host reference and application-part filtering, route ownership tests, and no-battle guardrails. The existing Blue and Green adapters are the correct template for project shape, controller conventions, protobuf-net usage, and xUnit source/route guards. [VERIFIED: repo]

Yellow protocol evidence currently consists of `proto/yellow/yellow.proto` and `proto/yellow/vsinterface.proto`. The game proto contains normal AC15 surfaces such as initial data, BAID, mydon entry, userdata, playresult, self-best, crowns, Taikojuku, item shop, medals, Banacoin-adjacent messages, challenge/tournament/recommendation/folder/telop metadata, and Tokkun tutorial/stage fields. It does not contain Blue battle `BattleUserData*` messages or Blue battle initial-data/playresult fields. [VERIFIED: proto]

The exact Yellow game route prefix was not found in checked-in source, proto files, `.planning`, `docs`, or local `.tools` Yellow artifacts. The planner should treat endpoint suffixes as proto-backed and the game route base prefix as an unresolved client-evidence gap until a log, binary string scan, or user-provided route proof supplies it. [VERIFIED: repo] [ASSUMED]

**Primary recommendation:** Implement the Phase 12 foundation as a small adapter/wire/host/test scaffold, but gate the exact Yellow game route prefix on evidence; do not copy Blue battle or create Yellow persistence/runtime behavior. [VERIFIED: repo]

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|--------------|----------------|-----------|
| Yellow route and transport boundary | Inbound Adapter | Host | Controllers own route attributes and protobuf DTOs; Host gates adapter application parts and missing-content-type protobuf assumptions. [VERIFIED: repo] |
| Yellow era enablement | Host | Application Settings | `Host/Program.cs` parses `ServerSettings:Eras`; `Application/Settings/ServerSettings.cs` provides typed era settings. [VERIFIED: repo] |
| Yellow wire DTO generation | Inbound Adapter | Build Tooling | Generated protobuf DTOs belong under adapter-local `Wire/`; `protogen` is available locally. [VERIFIED: repo] |
| Shared startup/version ownership | Shared Game Protocol Adapter | Host | `/v01r00/chassis/startupauth.php`, `verupauth.php`, and `verupcomplete.php` are owned by `Adapters.GameProtocol.Shared`; Host always maps controllers after application-part filtering. [VERIFIED: tests] |
| No-Blue-battle guardrails | Tests | Adapter/Application/Infrastructure | Yellow absence must be proven across proto, route list, source references, and persistence/entity names. [VERIFIED: proto] [VERIFIED: tests] |

## Project Constraints (from AGENTS.md)

- Keep era state separate; do not share Blue, Green, Yellow, or Nijiiro persistence except truly shared identity data. [VERIFIED: repo]
- Yellow must be first-class, not a Blue/Green variant. [VERIFIED: repo]
- Map generated protobuf DTOs through `Application/Dtos/Common*` before handler logic; do not persist wire DTOs directly. [VERIFIED: repo]
- Controllers should deserialize, map, call Mediator, and map back; business behavior belongs in `Application/Handlers`. [VERIFIED: repo]
- Use `IGameDataCatalog.For(GameEra)` and era catalog interfaces instead of hardcoded runtime filesystem access. [VERIFIED: repo]
- Resolve runtime data roots through `PathHelper` and era data path helpers; do not hardcode `wwwroot/data/<era>` in new runtime code. [VERIFIED: repo]
- Preserve AdminApi legacy and `/api/{era}/...` route patterns where later Yellow AdminApi/WebUI work touches them. [VERIFIED: repo]
- Keep generated `Wire/` files out of manual cleanup unless regenerating protocol output. [VERIFIED: repo]
- Do not invent Yellow battle, Blue battle fallback, token rewards, battle state, or battle unlock mirrors without concrete Yellow evidence. [VERIFIED: proto] [VERIFIED: repo]
- Phase 12 must not implement catalog, normal play, Dani, shop, medals, WaiWai, Tokkun, Banacoin runtime behavior, AdminApi/WebUI readback, or Yellow persistence except a minimal no-state compile scaffold if unavoidable. [VERIFIED: repo]

## Standard Stack

### Core

| Library / Tool | Version | Purpose | Why Standard |
|----------------|---------|---------|--------------|
| .NET SDK | 10.0.201 installed; repo pins 10.0.100 with latestFeature roll-forward | Build and test all projects targeting `net10.0` | `global.json` and `Directory.Build.props` define this solution baseline. [VERIFIED: local tool] [VERIFIED: repo] |
| ASP.NET Core MVC controllers | 10.0.7 package family | Attribute-routed game protocol endpoints and Host controller registration | Existing adapters use `[ApiController]`, `[Route]`, `[HttpPost]`, and `[Produces("application/protobuf")]`. [VERIFIED: repo] |
| protobuf-net | 3.2.56 | Generated wire DTO attributes and runtime protobuf serialization | Adapter projects reference `protobuf-net`; Host registers `.AddProtoBufNet()`. [VERIFIED: repo] |
| protobuf-net.AspNetCore | 3.2.52 | ASP.NET Core protobuf input/output formatter | `Host/Host.csproj` and shared protocol project reference this package; Host calls `.AddProtoBufNet()`. [VERIFIED: repo] |
| protogen | 3.2.52+f4db4afce3 installed | Generate Yellow C# wire DTOs from `proto/yellow/*.proto` | Existing Blue foundation plan used `protogen`; local CLI is available. [VERIFIED: local tool] [VERIFIED: repo] |
| Riok.Mapperly | 4.3.1 | Source-generated adapter mappers where runtime mapping is needed | Blue/Green adapter projects use it with accepted warning suppressions. [VERIFIED: repo] |
| xUnit | 2.9.3 | Route, source, wire, and host registration tests | `Tests/Tests.csproj` uses xUnit and existing Blue/Green guard tests use it. [VERIFIED: tests] |

### Supporting

| Library / Tool | Version | Purpose | When to Use |
|----------------|---------|---------|-------------|
| Mediator.SourceGenerator / Mediator.Abstractions | 3.0.2 | Runtime use-case dispatch | Use only if a Phase 12 controller must call an already-existing handler; most scaffold routes can remain log-and-success or compile-only until later phases. [VERIFIED: repo] |
| EF Core SQLite | 10.0.7 | Persistence | Do not use for Yellow in Phase 12 except to prove no Yellow battle persistence exists; Yellow runtime tables belong to later phases. [VERIFIED: repo] |
| `ProtocolRouteTestHelper` | repo-local | Reflect controller route attributes | Reuse for `Tests/Yellow/YellowRouteSkeletonTests.cs`. [VERIFIED: tests] |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Adapter-local Yellow wire DTOs | Shared AC15 wire assembly | Rejected by phase decision and AC15 core design; generated DTO optionality and route ownership must stay era-owned. [VERIFIED: repo] |
| Controller scaffold with stubs | Full Yellow handlers/persistence now | Rejected by phase scope; runtime behavior belongs to Phases 13-16. [VERIFIED: repo] |
| Full `WebApplicationFactory` disabled-route tests | Source guard plus application-part helper tests | Existing suite has no WebApplicationFactory package; source guards are current practice. Add a focused application-part unit test only if the implementation exposes the filtering helper without broad Host refactor. [VERIFIED: tests] |

**Installation:** No new external packages are required for Phase 12. [VERIFIED: repo]

## Package Legitimacy Audit

No package installation is recommended. The phase should reuse repo-central packages from `Directory.Packages.props` and the already installed `protogen` CLI. [VERIFIED: repo] [VERIFIED: local tool]

## Yellow Route And Message Inventory

### Route Suffixes Supported By Yellow Proto

The table maps proto request/response pairs to the endpoint suffix pattern used by Blue/Green adapter controllers. The suffixes are proto-backed; the Yellow base route prefix is unresolved because no local Yellow client route string/log was found. [VERIFIED: proto] [VERIFIED: repo] [ASSUMED]

| Endpoint Suffix | Request | Response | Phase 12 Treatment |
|-----------------|---------|----------|--------------------|
| `initialdatacheck.php` | `InitialdatacheckRequest` | `InitialdatacheckResponse` | Supported proto surface; no Blue battle fields. [VERIFIED: proto] |
| `tournamentcheck.php` | `TournamentcheckRequest` | `TournamentcheckResponse` | Supported proto surface; runtime data deferred. [VERIFIED: proto] |
| `bookkeeping.php` | `BookKeepingRequest` | `BookKeepingResponse` | Supported proto surface; log-and-success scaffold matches Blue/Green pattern. [VERIFIED: proto] [VERIFIED: repo] |
| `coinsetting.php` | `CoinsettingRequest` | `CoinsettingResponse` | Supported proto surface. [VERIFIED: proto] |
| `gettelop.php` | `GettelopRequest` | `GettelopResponse` | Supported proto surface; catalog readback deferred. [VERIFIED: proto] |
| `getfolder.php` | `GetfolderRequest` | `GetfolderResponse` | Supported proto surface; catalog readback deferred. [VERIFIED: proto] |
| `taikojuku.php` | `TaikojukuRequest` | `TaikojukuResponse` | Supported proto surface; Dani runtime deferred. [VERIFIED: proto] |
| `getitemshopinfo.php` | `GetitemshopinfoRequest` | `GetitemshopinfoResponse` | Supported proto surface; item-shop runtime deferred. [VERIFIED: proto] |
| `headclerk2.php` | `HeadClerk2Request` | `HeadClerk2Response` | Supported proto surface. [VERIFIED: proto] |
| `playresult.php` | `PlayResultRequest` | `PlayResultResponse` | Supported direct request shape in proto; runtime save behavior deferred. [VERIFIED: proto] |
| `baidcheck.php` | `BAIDRequest` | `BAIDResponse` | Supported proto surface; identity runtime deferred. [VERIFIED: proto] |
| `mydonentry.php` | `MydonEntryRequest` | `MydonEntryResponse` | Supported proto surface; identity runtime deferred. [VERIFIED: proto] |
| `userdata.php` | `UserDataRequest` | `UserDataResponse` | Supported proto surface; Tokkun tutorial field exists; runtime readback deferred. [VERIFIED: proto] |
| `challengecompe.php` | `ChallengeCompeRequest` | `ChallengeCompeResponse` | Supported proto surface; runtime data deferred. [VERIFIED: proto] |
| `balancecheck.php` | `BalancecheckRequest` | `BalancecheckResponse` | Supported Banacoin-adjacent proto surface; compatibility behavior deferred. [VERIFIED: proto] |
| `banacoinpayment.php` | `BanacoinpaymentRequest` | `BanacoinpaymentResponse` | Supported Banacoin-adjacent proto surface; no wallet persistence in Phase 12. [VERIFIED: proto] |
| `banacoinerrorlog.php` | `BanacoinerrorlogRequest` | `BanacoinerrorlogResponse` | Supported Banacoin-adjacent proto surface; no wallet persistence in Phase 12. [VERIFIED: proto] |
| `getbanacoininfo.php` | `GetbanacoininfoRequest` | `GetbanacoininfoResponse` | Supported Banacoin-adjacent proto surface; runtime deferred. [VERIFIED: proto] |
| `crownsdata.php` | `CrownsDataRequest` | `CrownsDataResponse` | Supported proto surface; crown encoding proof deferred to Phase 14. [VERIFIED: proto] |
| `recommend.php` | `RecommendRequest` | `RecommendResponse` | Supported proto surface; catalog readback deferred. [VERIFIED: proto] |
| `selfbest.php` | `SelfBestRequest` | `SelfBestResponse` | Supported proto surface; self-best runtime deferred. [VERIFIED: proto] |
| `heartbeat.php` | `HeartBeatRequest` | `HeartBeatResponse` | Supported proto surface. [VERIFIED: proto] |
| `itempurchase.php` | `ItempurchaseRequest` | `ItempurchaseResponse` | Supported proto surface; medal/shop runtime deferred. [VERIFIED: proto] |
| `rewardcardcheck.php` | `RewardcardcheckRequest` | `RewardcardcheckResponse` | Supported proto surface. [VERIFIED: proto] |
| `rewardexecution.php` | `RewardexecutionRequest` | `RewardexecutionResponse` | Supported proto surface; runtime deferred. [VERIFIED: proto] |
| `getreitai.php` | `GetreitaiRequest` | `GetreitaiResponse` | Proto exists but Blue excluded this route at foundation time; planner should only scaffold if route evidence confirms Yellow calls it. [VERIFIED: proto] [VERIFIED: tests] |

### Startup And Version Routes

| Route | Owner | Evidence | Phase 12 Treatment |
|-------|-------|----------|--------------------|
| `/v01r00/chassis/startupauth.php` | `Adapters.GameProtocol.Shared` | Existing route test proves shared ownership; Yellow `vsinterface.proto` is byte-identical to Green and Blue `vsinterface.proto`. [VERIFIED: tests] [VERIFIED: proto] | Keep shared unless later runtime/client evidence proves Yellow-specific behavior. |
| `/v01r00/chassis/verupauth.php` | `Adapters.GameProtocol.Shared` | Existing Blue shared-version tests prove shared ownership; Yellow `VerupAuth*` shape matches. [VERIFIED: tests] [VERIFIED: proto] | Keep shared. |
| `/v01r00/chassis/verupcomplete.php` | `Adapters.GameProtocol.Shared` | Existing Blue shared-version tests prove shared ownership; Yellow `VerupComplete*` shape matches. [VERIFIED: tests] [VERIFIED: proto] | Keep shared. |

### Explicit Yellow Gaps

| Gap | What Was Checked | Planning Impact |
|-----|------------------|-----------------|
| Exact Yellow game route prefix | Searched checked-in source, tests, docs, planning files, `proto/yellow`, and local `.tools` names for Yellow/ST9100/version route hints. [VERIFIED: repo] | Planner should include an evidence checkpoint or user-provided route prefix before hardcoding Yellow `[Route]` attributes. |
| Direct-body HTTP framing and missing `Content-Type` behavior | Yellow proto proves protobuf messages, but no local Yellow HTTP capture/log was found. [VERIFIED: proto] [VERIFIED: repo] | Direct protobuf is the likely scaffold shape by project requirement, but the evidence record must mark runtime framing as unverified until logs/cabinet proof exist. |
| `getreitai.php` route use | Yellow proto contains messages, but Blue route tests explicitly excluded Blue `getreitai.php`; no Yellow client call evidence was found. [VERIFIED: proto] [VERIFIED: tests] | Do not scaffold `getreitai.php` unless Yellow route evidence appears. |

## Yellow No-Battle Evidence

| Surface | Blue Battle Evidence | Yellow Evidence | Result |
|---------|----------------------|-----------------|--------|
| Dedicated route | Blue has `BattleUserDataRequest`, `BattleUserDataResponse`, and `/v10r03/chassis/battleuserdata.php`. [VERIFIED: proto] [VERIFIED: tests] | Yellow proto has no `BattleUserData*` messages. [VERIFIED: proto] | No Yellow `battleuserdata.php` route. |
| Initial data | Blue has `is_battleplay`, `release_battle_stage_flg`, `release_battle_special_flg`, and `battle_bonds_lv_cap`. [VERIFIED: proto] | Yellow `InitialdatacheckResponse` stops at normal/telop/folder/Taikojuku/itemshop/legal terms fields and has no battle fields. [VERIFIED: proto] | No Yellow initial-data battle advertisement. |
| Playresult | Blue has nested battle stage/NPC/release/token data under `PlayResultRequest`. [VERIFIED: proto] | Yellow `PlayResultRequest` has normal stage, reward, medal, collabo, tournament, payment, and Tokkun fields, but no battle stage/NPC/token/release fields. [VERIFIED: proto] | No battle classifier or Blue battle fallback. |
| Persistence | Blue battle entities and migrations are listed in `BlueBattleSourceGuardTests`. [VERIFIED: tests] | No Yellow persistence exists today; Phase 12 should not add Yellow battle entities/tables. [VERIFIED: repo] | Add Yellow source guard forbidding battle entities/migrations. |

## Architecture Patterns

### System Architecture Diagram

```text
-------------------------+
| Yellow cabinet request |
+------------+------------+
             |
             v
+-------------------------------+
| Yellow adapter controller      |
| Route prefix: evidence-gated   |
| Body: generated Yellow DTO     |
+---------------+---------------+
                |
                v
+-------------------------------+
| Yellow mapper / scaffold       |
| Wire DTO -> Common* only       |
+---------------+---------------+
                |
                v
+-------------------------------+
| Application handlers           |
| Later phases add Yellow logic  |
+---------------+---------------+
                |
                v
+-------------------------------+
| Era-owned state/catalog        |
| No Phase 12 Yellow tables      |
+-------------------------------+

Host composition:
ServerSettings:Eras:Yellow -> AddGameProtocolYellow -> MVC ApplicationPart kept
Yellow disabled            -> Yellow ApplicationPart removed -> routes absent
```

All arrows represent the intended request flow once later runtime behavior exists. Phase 12 should stop at route/wire/Host/test scaffold unless a minimal success response is needed to compile controller actions. [VERIFIED: repo]

### Recommended Project Structure

```text
Adapters.GameProtocol.Yellow/
|-- Adapters.GameProtocol.Yellow.csproj
|-- DependencyInjection.cs
|-- GlobalUsings.cs
|-- README.md
|-- Controllers/
|   `-- *Controller.cs
|-- Mappers/
|   `-- later phases only unless scaffold needs a trivial mapper
`-- Wire/
    |-- Game.cs          # generated from proto/yellow/yellow.proto
    `-- VsInterface.cs   # generated from proto/yellow/vsinterface.proto

Tests/Yellow/
|-- YellowEraFoundationTests.cs
|-- YellowHostProgramSourceTests.cs
|-- YellowRouteSkeletonTests.cs
|-- YellowSharedVersionRouteTests.cs
|-- YellowWireGenerationTests.cs
`-- YellowNoBattleSourceGuardTests.cs
```

This mirrors Blue/Green adapter and test organization while keeping Yellow route, wire, and guardrails era-owned. [VERIFIED: repo] [VERIFIED: tests]

### Pattern 1: Thin Adapter Registration

**What:** Adapter DI extensions currently declare the era constant and return `services`; Host owns whether the adapter is active. [VERIFIED: repo]

**When to use:** Use for Yellow foundation before any adapter-local services exist. [VERIFIED: repo]

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

### Pattern 2: Host Era Gating

**What:** `Host/Program.cs` parses enabled eras, conditionally calls adapter DI, and removes disabled-era assemblies from MVC application parts. [VERIFIED: repo]

**When to use:** Add Yellow next to Green/Blue/Nijiiro branches. [VERIFIED: repo]

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

### Pattern 3: Route Skeleton Tests

**What:** `Tests/Blue/BlueRouteSkeletonTests.cs` reflects adapter controller attributes and compares them to an expected route list. [VERIFIED: tests]

**When to use:** Add a Yellow equivalent after the route prefix is evidence-backed. [VERIFIED: tests]

```csharp
var routes = ProtocolRouteTestHelper
    .FindPostRoutes(typeof(TaikoLocalServer.Adapters.GameProtocol.Yellow.DependencyInjection).Assembly)
    .Where(route => route.Template.StartsWith(YellowPrefix, StringComparison.OrdinalIgnoreCase))
    .Select(route => route.Template)
    .Order(StringComparer.Ordinal)
    .ToArray();
```

### Pattern 4: Generated Wire Sanity Tests

**What:** `Tests/Blue/BlueWireGenerationTests.cs` proves generated types have the adapter namespace and direct playresult shape. [VERIFIED: tests]

**When to use:** Add Yellow namespace, direct request shape, and no-battle type/property assertions. [VERIFIED: tests]

```csharp
Assert.Equal(
    "TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire",
    typeof(PlayResultRequest).Namespace);
Assert.NotNull(typeof(PlayResultRequest).GetProperty(nameof(PlayResultRequest.Baid)));
Assert.Null(typeof(PlayResultRequest).Assembly.GetType(
    "TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire.BattleUserDataRequest"));
```

### Anti-Patterns to Avoid

- **Copying Blue battle route/controller/entities:** Yellow proto lacks `BattleUserData*` and Blue battle fields; copying them would create fake support. [VERIFIED: proto]
- **Generating a shared AC15 wire project:** Phase context and AC15 core design reject shared generated wire assemblies. [VERIFIED: repo]
- **Adding Yellow config before `GameEra.Yellow`:** Host parses config keys through `Enum.Parse<GameEra>`; a `Yellow` settings key will fail until the enum exists. [VERIFIED: repo]
- **Broad missing-content-type fallback:** Blue host tests ensure `/v10r03/chassis` is scoped, not all `/v10r03`; Yellow should follow the same narrow `/.../chassis` scope after prefix is known. [VERIFIED: tests]
- **Runtime behavior in scaffold controllers:** Phase 12 should not add Yellow normal/Tokkun/shop/catalog persistence or business rules. [VERIFIED: repo]

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Protobuf C# DTO generation | Manual C# wire DTOs | `protogen --csharp_out=... --proto_path=proto/yellow yellow.proto` and equivalent for `vsinterface.proto` | Existing wire files are generated and annotated for protobuf-net; manual DTOs risk field/tag mistakes. [VERIFIED: repo] |
| HTTP protobuf serialization | Custom byte readers/writers in controllers | ASP.NET Core controllers plus `.AddProtoBufNet()` | Host already registers protobuf-net formatters. [VERIFIED: repo] |
| Disabled route filtering | Per-controller runtime checks | MVC application-part removal in Host | Existing disabled-era pattern removes adapter controllers from route discovery. [VERIFIED: repo] |
| Route discovery tests | Ad hoc source string-only route checks | `ProtocolRouteTestHelper` reflection plus source guards for Host-specific logic | Existing Blue tests use reflection for controller routes and source guards for Host local-statement code. [VERIFIED: tests] |
| Battle absence proof | Human review only | Proto/source/route tests forbidding Yellow battle strings and routes | YFND-04 requires automated guardrails. [VERIFIED: repo] |

**Key insight:** The foundation is mostly framework and repository wiring. Hand-written serializers, shared wire abstractions, or invented compatibility layers would increase risk without helping Phase 12 success criteria. [VERIFIED: repo]

## Common Pitfalls

### Pitfall 1: Treating Proto Similarity As Route Proof
**What goes wrong:** The planner copies Blue `/v10r03` route constants or all Blue route decisions because Yellow has similar message names. [VERIFIED: repo]
**Why it happens:** Proto files define messages, not necessarily HTTP base prefixes or every called endpoint. [VERIFIED: proto]
**How to avoid:** Record endpoint suffixes separately from the evidence-gated Yellow base prefix; require a route-prefix evidence row before implementation. [ASSUMED]
**Warning signs:** New Yellow route constants appear without a cited log, binary string, or user-approved route source. [ASSUMED]

### Pitfall 2: Leaking Blue Battle Into Yellow
**What goes wrong:** `battleuserdata.php`, `BattleUserData*`, `release_battle_*`, `BlueBattle*`, or battle persistence appears in Yellow code. [VERIFIED: proto] [VERIFIED: tests]
**Why it happens:** Blue support is nearby and has similar normal AC15 flow. [VERIFIED: repo]
**How to avoid:** Add Yellow no-battle proto/source/route tests in Wave 0 before scaffold implementation. [VERIFIED: tests]
**Warning signs:** Yellow adapter imports `TaikoLocalServer.Adapters.GameProtocol.Blue`, `Application/Catalog/Blue`, or any `BlueBattle*` type. [VERIFIED: tests]

### Pitfall 3: Breaking Startup With Settings Order
**What goes wrong:** Adding `ServerSettings:Eras:Yellow` before `GameEra.Yellow` causes `Enum.Parse<GameEra>` in Host startup to throw. [VERIFIED: repo]
**Why it happens:** Enabled era parsing is enum-backed. [VERIFIED: repo]
**How to avoid:** Add `GameEra.Yellow` and enum/settings tests before committed config includes a Yellow key. [VERIFIED: repo]
**Warning signs:** `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` passes but `dotnet run --project Host` fails at startup. [VERIFIED: repo]

### Pitfall 4: Making Controller Stubs Too Smart
**What goes wrong:** Phase 12 controllers start writing profile, score, medal, Tokkun, or catalog state. [VERIFIED: repo]
**Why it happens:** Blue controllers now contain real runtime behavior, but Yellow runtime phases are later. [VERIFIED: repo]
**How to avoid:** Keep Phase 12 controllers thin and no-state; add source guard that only approved scaffold controllers call Mediator, or none call Mediator until later phases. [VERIFIED: tests]
**Warning signs:** New Yellow EF entities, migrations, `ITaikoDbContext.Yellow`, or handler partials beyond compile-required placeholders appear in Phase 12. [VERIFIED: repo]

## Code Examples

### Existing Blue Thin Success Controller Pattern

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

Use this pattern for Yellow no-state scaffold endpoints only after the Yellow base prefix is evidence-backed. [VERIFIED: repo]

### Existing Direct-Protobuf Blue Playresult Pattern

```csharp
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

This shows the direct-body protobuf controller shape, but Yellow runtime playresult mapping/saving belongs to later phases. [VERIFIED: repo]

### Existing Host Protobuf Content-Type Fallback

```csharp
return path.StartsWithSegments("/v11r01/chassis", StringComparison.OrdinalIgnoreCase)
       || path.StartsWithSegments("/v10r03/chassis", StringComparison.OrdinalIgnoreCase)
       || path.StartsWithSegments("/v01r00/chassis", StringComparison.OrdinalIgnoreCase)
       || path.StartsWithSegments("/v12r08_ww/chassis", StringComparison.OrdinalIgnoreCase)
       || path.StartsWithSegments("/v12r00_cn/chassis", StringComparison.OrdinalIgnoreCase);
```

Add Yellow only at the exact Yellow game `.../chassis` prefix once proven; keep the fallback scoped. [VERIFIED: repo] [ASSUMED]

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Green adapter only for AC15 game routes | Blue and Green are separate adapters with separate route prefixes, wire DTOs, mappers, tests, and persistence | Blue v1.0 work before 2026-06-03 | Yellow should follow first-class era ownership, not a Green/Blue flag. [VERIFIED: repo] |
| Route behavior inferred from nearby eras | Evidence hierarchy requires local proto/source/tests/logs before runtime behavior | v1.1/v1.2 planning docs | Yellow gaps should be documented instead of filled with Blue behavior. [VERIFIED: repo] |
| Duplicating all Blue/Green logic per era | Approved AC15 core design shares behavior behind era-owned adapters/wire/persistence | 2026-06-07 design doc | Phase 12 can prepare Yellow as a future AC15 core consumer without sharing wire or tables. [VERIFIED: repo] |

**Deprecated/outdated:**
- Treating Yellow Tokkun as impossible because older public scope said Tokkun ended before Blue is not authoritative for this repo; Yellow proto has Tokkun fields, and local evidence outranks public wiki context. [VERIFIED: proto] [VERIFIED: repo]
- Treating Blue battle as generic AC15 behavior is rejected; Yellow proto lacks Blue battle fields and routes. [VERIFIED: proto]

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | Yellow endpoint suffixes should use the same `.php` suffix names as existing Blue/Green controllers for matching proto messages. | Yellow Route And Message Inventory | Wrong suffixes would produce routes the client never calls; require route-prefix/client evidence before implementation. |
| A2 | Yellow game endpoints can initially use the direct protobuf `[FromBody]` controller shape once route evidence exists. | Summary, Code Examples | If Yellow frames or wraps payloads differently, scaffold controllers will not deserialize cabinet requests; record as an evidence gap. |
| A3 | A focused source guard is acceptable for disabled-host route absence unless the implementation exposes application-part filtering for direct unit testing. | Standard Stack, Validation Architecture | If the project requires runtime route-table proof, planner may need a small Host testability extraction or WebApplicationFactory package decision. |

## Open Questions

1. **What is the exact Yellow game route prefix?**
   - What we know: Yellow local data uses `Host/wwwroot/data/yellow/data` and observed config root `config/ST9100-1`; Yellow proto lists endpoint messages. [VERIFIED: repo] [VERIFIED: proto]
   - What's unclear: No checked-in local Yellow HTTP route string/log/client evidence was found for the base prefix. [VERIFIED: repo]
   - Recommendation: Add a Phase 12 evidence row and checkpoint before hardcoding Yellow route attributes; accept user-provided route evidence if local logs/binary strings are unavailable. [ASSUMED]

2. **Is Yellow direct protobuf with missing/blank `Content-Type` in real cabinet traffic?**
   - What we know: Yellow messages are protobuf schemas; Blue direct-protobuf and Host content-type fallback patterns exist. [VERIFIED: proto] [VERIFIED: repo]
   - What's unclear: No Yellow runtime HTTP capture was found. [VERIFIED: repo]
   - Recommendation: Scaffold direct protobuf only where the evidence record marks this as supported or user-approved; keep fallback narrowly scoped to the proven Yellow route prefix. [ASSUMED]

3. **Should `getreitai.php` be scaffolded in Phase 12?**
   - What we know: Yellow proto contains `GetreitaiRequest/Response`; Blue route tests exclude `getreitai.php` despite Blue proto history around similar compatibility surfaces. [VERIFIED: proto] [VERIFIED: tests]
   - What's unclear: No Yellow route call evidence was found. [VERIFIED: repo]
   - Recommendation: Keep it out of the scaffold until client evidence proves the route is called. [ASSUMED]

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|-------------|-----------|---------|----------|
| .NET SDK | Build/test/Host compile | Yes | 10.0.201 installed; repo pins 10.0.100 with latestFeature roll-forward | None needed. [VERIFIED: local tool] [VERIFIED: repo] |
| protogen | Yellow wire DTO generation | Yes | 3.2.52+f4db4afce3 | If missing later, install `protobuf-net.Protogen` after user approval or generate in an environment that already has it. [VERIFIED: local tool] |
| NuGet package restore | Build/test | Expected through existing .NET CLI | Central package versions in `Directory.Packages.props` | No new packages required. [VERIFIED: repo] |

**Missing dependencies with no fallback:** None found for research/planning. [VERIFIED: local tool]

**Missing dependencies with fallback:** None found for research/planning. [VERIFIED: local tool]

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 with Microsoft.NET.Test.Sdk 17.14.1 [VERIFIED: tests] |
| Config file | `Tests/Tests.csproj`, `Directory.Build.props`, `Directory.Packages.props` [VERIFIED: repo] |
| Quick run command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Yellow|FullyQualifiedName~BlueSharedVersionRouteTests|FullyQualifiedName~StartupAuthRouteTests"` [ASSUMED] |
| Full suite command | `dotnet test Tests/Tests.csproj` [VERIFIED: tests] |
| Build command | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` [VERIFIED: repo] |

### Phase Requirements To Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|--------------|
| YFND-01 | Yellow evidence record captures route suffixes, startup/version ownership, direct-protobuf expectations, and route-prefix gaps | docs/source guard | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowEvidence"` | No - Wave 0 |
| YFND-02 | Yellow adapter compiles generated `Game.cs` and `VsInterface.cs` under Yellow namespace | compile/wire unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowWireGenerationTests"` | No - Wave 0 |
| YFND-02 | Yellow adapter owns only supported game routes | route reflection | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowRouteSkeletonTests"` | No - Wave 0 |
| YFND-03 | Host registers Yellow services and removes Yellow application part when disabled | source guard / application-part unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowHostProgramSourceTests"` | No - Wave 0 |
| YFND-03 | Shipped settings declare a parseable Yellow era setting after `GameEra.Yellow` exists | settings unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowEraFoundationTests"` | No - Wave 0 |
| YFND-04 | Yellow proto/wire/route/source has no Blue battle surface | proto/source/route unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowNoBattleSourceGuardTests"` | No - Wave 0 |

### Sampling Rate

- **Per task commit:** Run the relevant focused Yellow test filter plus `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"`. [VERIFIED: tests] [VERIFIED: repo]
- **Per wave merge:** Run `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Yellow"` and Host temp-output build. [ASSUMED]
- **Phase gate:** Run `dotnet test Tests/Tests.csproj` and `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"`. [VERIFIED: tests] [VERIFIED: repo]

### Wave 0 Gaps

- [ ] `Tests/Yellow/YellowEraFoundationTests.cs` - covers `GameEra.Yellow` stable value and shipped `ServerSettings:Eras:Yellow`. [VERIFIED: tests]
- [ ] `Tests/Yellow/YellowWireGenerationTests.cs` - covers generated Yellow namespace, direct request shape, `VsInterface` compatibility, and no battle types/properties. [VERIFIED: tests]
- [ ] `Tests/Yellow/YellowRouteSkeletonTests.cs` - covers Yellow route ownership, shared-route exclusions, no `getreitai.php` unless evidence appears, and no battle routes. [VERIFIED: tests]
- [ ] `Tests/Yellow/YellowHostProgramSourceTests.cs` - covers Host using, `enabledEras.Contains(GameEra.Yellow)`, `AddGameProtocolYellow()`, application-part removal, and narrow content-type fallback after route prefix is known. [VERIFIED: tests]
- [ ] `Tests/Yellow/YellowSharedVersionRouteTests.cs` - covers `/v01r00/chassis/*` shared ownership and Yellow `vsinterface.proto` wire compatibility with shared DTOs. [VERIFIED: tests] [VERIFIED: proto]
- [ ] `Tests/Yellow/YellowNoBattleSourceGuardTests.cs` - covers proto, adapter, application, domain, infrastructure, and migrations for absence of Yellow/Blue battle surfaces. [VERIFIED: tests]

## Security Domain

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|------------------|
| V2 Authentication | No for game protocol scaffold | Cabinet game routes are not JWT-admin routes; do not add auth changes in Phase 12. [VERIFIED: repo] |
| V3 Session Management | No for game protocol scaffold | No browser/session behavior in Phase 12. [VERIFIED: repo] |
| V4 Access Control | Yes, as era enablement | Host application-part filtering must prevent disabled Yellow routes from being routable. [VERIFIED: repo] |
| V5 Input Validation | Yes | Use protobuf-net model binding and keep controllers thin; no custom parsing unless Yellow evidence proves a wrapper. [VERIFIED: repo] |
| V6 Cryptography | No new crypto | Do not introduce Banacoin wallet/payment cryptography or client-auth changes. [VERIFIED: repo] |
| V7 Error Handling | Yes | Scaffold endpoints should return protocol success/failure shapes only where the client contract is known; avoid leaking internal errors in logs. [VERIFIED: repo] |
| V9 Communications | Existing host concern | Do not change Kestrel/Mucha/GameUrl settings in Phase 12. [VERIFIED: repo] |

### Known Threat Patterns

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| Disabled-era route exposure | Elevation of privilege / information disclosure | Remove Yellow application part when Yellow is disabled and add focused tests. [VERIFIED: repo] |
| Cross-era state write | Tampering | No Yellow persistence in Phase 12; later phases must add Yellow-owned tables and no-cross-era tests. [VERIFIED: repo] |
| Overbroad protobuf fallback | Spoofing / denial of service | Scope missing-content-type fallback to exact Yellow `.../chassis` prefix after route evidence exists. [VERIFIED: tests] |
| Invented Banacoin state | Tampering / repudiation | Treat Banacoin-adjacent Yellow routes as compatibility only in later phases unless evidence proves state authority. [VERIFIED: proto] [VERIFIED: repo] |

## Sources

### Primary (HIGH confidence)

- `.planning/phases/12-yellow-evidence-and-era-foundation/12-CONTEXT.md` - Phase decisions, discretion, deferred scope. [VERIFIED: repo]
- `.planning/REQUIREMENTS.md` - YFND-01 through YFND-04 descriptions and Yellow out-of-scope items. [VERIFIED: repo]
- `.planning/ROADMAP.md` - Phase 12 goal and success criteria. [VERIFIED: repo]
- `AGENTS.md` - Era separation, adapter/controller/handler/wire/data constraints. [VERIFIED: repo]
- `proto/yellow/yellow.proto` - Yellow game message inventory and no-battle evidence. [VERIFIED: proto]
- `proto/yellow/vsinterface.proto` - Yellow startup/version message shape. [VERIFIED: proto]
- `proto/blue/taiko.proto` - Contrast source for Blue-only battle fields/routes. [VERIFIED: proto]
- `Host/Program.cs` - enabled-era parsing, adapter registration, application-part removal, protobuf fallback, MVC mapping. [VERIFIED: repo]
- `Adapters.GameProtocol.Blue/` and `Adapters.GameProtocol.Green/` - adapter project/controller/mapper/wire patterns. [VERIFIED: repo]
- `Tests/Blue/*Route*`, `Tests/Blue/*SourceGuard*`, `Tests/Blue/ProtocolRouteTestHelper.cs`, and `Tests/Green/StartupAuthRouteTests.cs` - current route, shared startup/version, wire, and source-guard testing patterns. [VERIFIED: tests]
- `docs/superpowers/specs/2026-06-07-ac15-core-extraction-design.md` - approved sharing boundary: behavior can be shared, wire/routes/persistence remain era-owned. [VERIFIED: repo]

### Secondary (MEDIUM confidence)

- `.planning/codebase/STRUCTURE.md`, `CONVENTIONS.md`, `TESTING.md`, `ARCHITECTURE.md`, `STACK.md` - generated codebase maps from 2026-05-28; useful and consistent with current source, but source files are preferred when they differ. [VERIFIED: repo]

### Tertiary (LOW confidence)

- None used. No web browsing was performed. [VERIFIED: repo]

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - versions and tools were verified from repo files and local CLI output. [VERIFIED: repo] [VERIFIED: local tool]
- Architecture: HIGH - adapter/Host/test patterns are present in current source. [VERIFIED: repo] [VERIFIED: tests]
- Yellow proto inventory: HIGH - message names and no-battle contrast are from local proto files. [VERIFIED: proto]
- Yellow route prefix and direct transport runtime proof: MEDIUM/LOW - proto supports protobuf DTO generation, but no local Yellow route string/log/capture was found. [VERIFIED: repo] [ASSUMED]
- Pitfalls: HIGH for cross-era/battle/settings pitfalls from current source; MEDIUM for route-prefix transport pitfalls because they depend on missing client evidence. [VERIFIED: repo] [ASSUMED]

**Research date:** 2026-06-07
**Valid until:** 2026-07-07 for repo architecture; refresh immediately if Yellow route logs/client binary evidence or new proto files are added.
