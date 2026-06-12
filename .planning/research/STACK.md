# Stack Research

**Domain:** Red AC15 era support and Red/older challenge competition in TaikoLocalServer
**Researched:** 2026-06-12
**Confidence:** HIGH for repo stack/tooling reuse, MEDIUM for exact Red route prefix and challenge semantics until runtime/client evidence lands

## Correction Note

The requirements review corrected the first-pass interpretation. Red should reuse the existing stack and shared AC15 code. Do not turn absent Red proto surfaces into requirements. Do not change Blue, Green, Yellow, or Nijiiro behavior for Red work except where shared code must preserve existing behavior. Don Challenge is Red product scope from wiki, but local Red data/proto names are not enough to specify stateful challenge behavior; endpoint/schema semantics remain runtime evidence questions.

## Recommendation

Do not add a new platform stack for Red. Implement Red as another first-class AC15 era inside the existing .NET 10 / ASP.NET Core host, with adapter-local protobuf wire DTOs, Mapperly mappers, Mediator handlers, EF Core SQLite tables, AC15 catalog loaders, and AdminApi/WebUI era routing where needed.

The one stack change Red needs is a narrower AC15 capability model. Current `Ac15EraProfiles` gives Blue, Green, and Yellow the same `Ac15FeatureSet`, including `ItemShop`. Red's proto lacks Yellow item-shop/medal messages but exposes older `reward_ptn`, `reward_progress`, Don point, and `ChallengeCompe*` fields. Add Red to the profile system with `ItemShop = false` and a new explicit challenge competition / older reward capability instead of forcing Red through the Yellow shop path.

Challenge competition must be scoped to Red and older versions. Blue, Green, and Yellow generated wire classes and some controllers contain `ChallengeCompe*` shapes, but local code shows Green/Yellow handlers return empty stub responses and Blue returns success directly. Per the user correction, treat those newer-era surfaces as compatibility residue, not active behavior evidence.

## Recommended Stack

### Core Technologies

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| .NET / ASP.NET Core | `net10.0` / repo .NET 10 packages | Host, controllers, DI, route gating | Existing process already composes Nijiiro, Green, Blue, and Yellow. Red should plug into the same host and application-part gating. |
| EF Core SQLite | `Microsoft.EntityFrameworkCore.*` 10.0.7 | Red-owned persistence and migrations | Matches current per-era state separation. Use concrete Red tables through `ITaikoDbContext`; do not introduce repository-shaped persistence. |
| protobuf-net + repo-local protogen | `protobuf-net` 3.2.56, `protobuf-net.AspNetCore` 3.2.52 | Red direct-protobuf wire DTOs | AC15 adapters use generated adapter-local `Wire/` classes. Generate Red from local `proto/red/*.proto`; keep proto read-only and generated wire replace-only. |
| Riok.Mapperly | 4.3.1 | Wire/Common DTO projection | Phase 16.1 established real Mapperly partial mapping with nullable optional primitive wire fields. Red should copy that mapper convention, including `MapperDefaults`. |
| Mediator.SourceGenerator | 3.0.2 | Application request dispatch | Existing handlers dispatch by `GameEra` to era-specific partials. Add Red partials where behavior is real. |
| MudBlazor WASM / Contracts.AdminApi | repo current | Admin UI routing and readback | No new UI stack. Add Red to the existing era route model only for Red-supported admin surfaces. |
| xUnit | 2.9.3 | Regression tests | Add behavior tests for Red handler/catalog/persistence boundaries, not source-shape or generated-wire tests. |

### Brownfield Additions

| Area | Add / Change | Reuse |
|------|--------------|-------|
| Era registration | Add `GameEra.Red`, `ServerSettings:Eras:Red`, Host DI call, app-part removal, solution/project references, and debug data junction target. | Follow Yellow/Blue/Green host registration and `Host.csproj` data exclusion/copy patterns. |
| Red adapter | Add `Adapters.GameProtocol.Red` with `Wire/`, `Controllers/`, `Mappers/`, `MapperlyDefaults.cs`, `GlobalUsings.cs`, and marker/DI files. | Start from Yellow adapter structure, but only expose routes proven by `proto/red/taiko.proto`, `proto/red/vsinterface.proto`, and runtime evidence. |
| Wire generation | Generate `Game.cs` from `proto/red/taiko.proto` and `VsInterface.cs` from `proto/red/vsinterface.proto`. | Use `.tools/protogen.exe` with `+nullablevaluetype=yes`; do not hand-edit generated files. |
| AC15 profile | Add `Ac15EraProfiles.Red` and expand `Ac15FeatureSet`/`Ac15WirePlacement` only where needed. | Reuse `Ac15ProtocolLimits`, crown/userdata/initialdata services, and adapter-local wire placement. |
| Catalog paths | Add `RedGameDataPaths` after the runtime target config root is proven. | Follow `YellowGameDataPaths` and `PathHelper.GetDataPath(GameEra.Red)`; do not hardcode raw `Host/wwwroot/data/red/...` in handlers. |
| Catalog loaders | Add Red catalog interfaces/loaders using `Infrastructure/GameDataCatalog/Ac15` loaders where file shapes match. | Reuse AC15 `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `tuning.bin`, telop/movie/event-folder/Taikojuku loaders only after Red data shape is checked. |
| Persistence | Add Red-owned save, best, play-history, favorite/recent, Dani/Tokkun, and challenge tables as evidence requires. | Reuse AC15 generic helpers over concrete Red DbSets and narrow row-shape interfaces. Keep direct `ITaikoDbContext` visible. |
| Challenge competition | Add Red-backed `GetChallengeCompeQuery` handling, Red mappers, and Red challenge persistence/catalog parsing. | Reuse `CommonChallengeCompeResponse` and playresult stage challenge ID arrays as the DTO starting point. Do not activate Green/Yellow/Blue challenge semantics. |
| AdminApi/WebUI | Add Red era routing and Red readback only for implemented Red state. | Follow Yellow AdminApi/WebUI patterns and legacy route preservation rules. |

## Supporting Local Evidence

| Source | Stack Implication |
|--------|-------------------|
| `.planning/PROJECT.md` | Red v1.3 is active; Red uses `proto/red/taiko.proto`, `proto/red/vsinterface.proto`, and `Host/wwwroot/data/red/data`; Red should be Yellow-like but without WaiWai and with Don Challenge / challenge competition. |
| `proto/red/taiko.proto` | Red has `ChallengeCompeRequest/Response`, `UserDataResponse.is_challengecompe`, playresult challenge ID arrays, `reward_ptn`, `reward_progress`, `get_donpoint`, and total Don point fields. |
| `proto/red/taiko.proto` | Red lacks Yellow `GetitemshopinfoRequest/Response`, `ItempurchaseRequest/Response`, Don/Katsu medal fields, and WaiWai result/gauge/tutorial fields. |
| `proto/red/vsinterface.proto` | Red startup/version message shapes are local proto inputs; exact route ownership should still be proven against current runtime traces before locking routes. |
| `Host/wwwroot/data/red/data` | Red local data exists as an operator data tree with multiple candidate config roots: `ST5100-1`, `ST5100-7`, `ST7100-1`, and `ST8100-1`. Select the runtime root from evidence before hardcoding `RedGameDataPaths`. |
| `Application/Ac15/Ac15EraProfiles.cs` and `Ac15FeatureSet.cs` | Current shared AC15 feature set is too broad for Red because all Blue/Green/Yellow profiles share `ItemShop = true`. |
| `Application/Ac15/Ac15InitialDataService.cs` | Initialdata item-shop rows and `IsItemshop` are driven by profile feature flags, so Red needs profile-level absence rather than controller hacks. |
| `Application/Dtos/CommonChallengeCompeResponse.cs` | A common challenge DTO already exists and matches the high-level Red response shape. |
| `Application/Dtos/CommonPlayResultData.cs` | Stage-level challenge ID arrays already exist in the common playresult DTO. |
| `Application/Handlers/GetChallengeCompeQuery*.cs` | Green/Yellow challenge handling is currently stub/empty; Red must add real behavior without changing newer-era meaning. |
| `Adapters.GameProtocol.Yellow/Wire/Game.cs` | Newer generated wire contains challenge classes plus Yellow-only item-shop/WaiWai fields; field presence alone is not an activity signal. |

## What Not To Add

| Avoid | Why | Use Instead |
|-------|-----|-------------|
| New web framework, serializer, queue, service bus, or external scheduler | Red is another cabinet protocol adapter inside the existing ASP.NET Core host. | Existing controllers, protobuf-net, Mediator, EF Core, and hosted startup pipeline. |
| Shared `Ac15SaveData` table or broad repository layer | Violates era state separation and the Phase 16.2 direct-`ITaikoDbContext` boundary. | Red-specific EF entities/DbSets plus narrow generic helper reuse where row shapes match. |
| Shared generated AC15 wire assembly | Red/Yellow/Blue field numbers and message presence differ. | Adapter-local Red generated `Wire/` files and Red mappers. |
| Manual edits to `proto/red/*.proto` | Proto files are dumped local evidence. | Regenerate adapter wire and replace generated C# only. |
| Manual cleanup of generated `Wire/` files | Generated output is tool-owned. | Regenerate with the correct options; use partial classes only if extension is needed. |
| Yellow item-shop / Don-Katsu medal stack for Red | Red proto has older Don point/reward fields and no item-shop request/response messages. | Model Red reward/progression separately after evidence. |
| WaiWai support for Red | Red proto lacks Yellow WaiWai fields. | Keep absent unless local Red evidence proves otherwise. |
| Blue battle support for Red | Red proto/data inputs do not establish Blue battle messages or battle XML semantics. | Keep battle absent unless concrete Red proto/log/IDA evidence appears. |
| Real Banacoin wallet/payment authority | Project scope keeps Banacoin stateless compatibility only where needed. | Log-and-success compatibility surfaces only when Red client evidence requires them. |
| Activating challenge competition for Blue/Green/Yellow | User correction and local code show newer-era challenge is not meaningful. | Implement Red/older challenge only; leave newer-era stubs as compatibility. |
| Wiki-driven route or semantic claims | Local evidence outranks wiki. | Use proto, local data, RPCS3/cabinet logs, and IDA/client evidence. |

## Commands And Conventions To Preserve

### Wire Generation

Use repo-local protogen and nullable optional primitive generation:

```powershell
.\.tools\protogen.exe --csharp_out=Adapters.GameProtocol.Red\Wire -Iproto\red +nullablevaluetype=yes proto\red\taiko.proto
.\.tools\protogen.exe --csharp_out=Adapters.GameProtocol.Red\Wire -Iproto\red +nullablevaluetype=yes proto\red\vsinterface.proto
```

Conventions:

- Generate into a temp directory first if output names or namespace shape are uncertain.
- Rename or regenerate outputs to adapter conventions (`Game.cs`, `VsInterface.cs`) after confirming generated shape.
- Keep namespace under `TaikoLocalServer.Adapters.GameProtocol.Red.Wire`.
- Do not edit `proto/red/*.proto`.
- Do not hand-clean generated `Wire/` files.
- Preserve nullable optional primitive semantics; absent and explicit zero must remain distinguishable.

### Mapperly

Red adapter should include:

```csharp
using Riok.Mapperly.Abstractions;

[assembly: MapperDefaults(
    AutoUserMappings = false,
    RequiredMappingStrategy = RequiredMappingStrategy.Target)]
```

Conventions:

- Use partial Mapperly methods for mechanical projection.
- Use wrapper methods only for protocol semantics, null collection normalization, classification, or response composition.
- Do not reintroduce broad `RMG020` / `RMG012` warning suppression.
- Production mappers should read nullable wire properties directly, not `ShouldSerialize*`.

### Build And Verification

Use focused builds while scaffolding, then solution and host checks:

```powershell
dotnet build Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj
dotnet build TaikoLocalServer.slnx
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Red|FullyQualifiedName~Ac15"
dotnet test Tests/Tests.csproj
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
```

Use the temp-output Host build when `Host/bin/Debug/net10.0` is locked by a running server.

### EF Core

Use normal repo migration commands when Red persistence lands:

```powershell
dotnet ef migrations add AddRedAc15Support --project Infrastructure --startup-project Host
dotnet ef database update --project Infrastructure --startup-project Host
```

Conventions:

- Red gameplay state gets Red-owned tables.
- Shared identity state can remain shared only where it is already truly era-neutral.
- Challenge competition state should be Red-owned unless a later older-era milestone proves the same state contract.

### Data Layout

Keep operator game data outside publish output and junctioned in Debug, matching Green/Blue/Yellow:

- Exclude `Host/wwwroot/data/red/data/**` from publish content.
- Add a Debug build junction target from output `wwwroot/data/red/data` to source `Host/wwwroot/data/red/data`.
- Add committed Red server-authored JSON sidecars only for supported Red features that need server data. Empty files are acceptable only when the feature contract expects a file and local evidence supports empty data.
- Do not choose among `ST5100-1`, `ST5100-7`, `ST7100-1`, and `ST8100-1` by filename guesswork; prove the runtime target first.

## Version And Compatibility Notes

- Red direct-protobuf game transport should be preserved unless current Red client evidence proves otherwise.
- Shared `/v01r00/chassis/*` startup/version ownership may be reusable, but route prefix and version routing must be proven from local runtime/log/IDA evidence before implementation.
- Red `InitialdatacheckResponse` has no Yellow item-shop row fields, so Red initialdata mapping must omit item-shop advertisement rather than sending empty Yellow-shaped data.
- Red `PlayResultRequest.play_mode` sits at the Red field placement, not Yellow's shifted placement; do not reuse Yellow generated wire or field-number assumptions.
- Red challenge readback should start from `ChallengeCompeResponse`'s three lists: `ary_challenge_stat`, `ary_user_compe_stat`, and `ary_bng_compe_stat`.
- Red playresult challenge uploads should preserve raw `ary_challenge_id`, `ary_user_compe_id`, and `ary_bng_compe_id` facts until semantics are proven.
- Red reward/progression should be modeled from `reward_ptn`, `reward_progress`, `get_donpoint`, `total_get_donpoint`, and `total_use_donpoint`; do not map these to Yellow Don/Katsu medal semantics without evidence.

## Alternatives Considered

| Category | Recommended | Alternative | Why Not |
|----------|-------------|-------------|---------|
| Adapter implementation | New `Adapters.GameProtocol.Red` project | Fold Red into Yellow adapter | Wire field numbers, routes, data roots, and persistence are era-owned. |
| Challenge support | Red-backed challenge capability | Enable Green/Yellow/Blue challenge behavior | User correction plus local stub handlers say newer-era challenge is not meaningful. |
| Persistence reuse | Red tables plus narrow generic helpers | Shared AC15 repository/table | Violates established state boundaries and hides EF/table ownership. |
| Catalog paths | `RedGameDataPaths` after runtime root proof | Hardcode one `ST*` root now | Local Red data has multiple roots and route/root proof is pending. |
| Reward model | Red Don point/reward model | Yellow item-shop/medal model | Red proto lacks Yellow item-shop and medal messages. |

## Sources

- `.planning/PROJECT.md` - v1.3 Red milestone scope, evidence hierarchy, and challenge scope.
- `.planning/STATE.md` - current planning status and v1.2 stack decisions.
- `.planning/ROADMAP.md` - shipped Blue/Yellow milestones and current planning baseline.
- `proto/red/taiko.proto` - Red game protocol messages.
- `proto/red/vsinterface.proto` - Red startup/version protocol messages.
- `Host/wwwroot/data/red/data` - local Red operator game-data tree and config roots.
- `Application/Ac15/Ac15EraProfiles.cs` and `Application/Ac15/Ac15FeatureSet.cs` - current capability/profile shape.
- `Application/Ac15/Ac15InitialDataService.cs` - feature-driven initialdata composition.
- `Infrastructure/GameDataCatalog/Yellow/YellowGameDataPaths.cs` - era data path convention.
- `Adapters.GameProtocol.Yellow/Wire/Game.cs` - generated nullable wire shape and newer-era challenge/item-shop/WaiWai contrast.
- `Application/Handlers/GetChallengeCompeQuery.cs` and era partials - current challenge dispatch/stub behavior.
- `.planning/milestones/v1.2-phases/16.1-ac15-mapperly-mapper-rewrite-and-presence-semantics/16.1-RESEARCH.md` - protogen and Mapperly conventions.

---
*Stack research for: Red AC15 Support*
*Researched: 2026-06-12*
