# Phase 18: Red Evidence and Capability Foundation - Research

**Researched:** 2026-06-13
**Domain:** Red AC15 protocol evidence, adapter foundation, route/version boundaries, and capability inventory
**Confidence:** HIGH for route/root/proto/tooling evidence; MEDIUM for direct transport until manual RPCS3/cabinet routing smoke lands.

<user_constraints>
## User Constraints (from CONTEXT.md)

All bullets in this section are copied verbatim from `18-CONTEXT.md`; provenance for this copied section is `[VERIFIED: .planning/phases/18-red-evidence-and-capability-foundation/18-CONTEXT.md]`.

### Locked Decisions

#### Evidence Lock Level
- **D-01:** Use `.tools/red/EBOOT.ELF.i64` through an `ida-cli` daemon as the authority for Red route/version/root/limit evidence. Route prefixes are in `/vxxryy` form, where `xx` is the startup-auth version and `yy` is the matching revision/proto revision.
- **D-02:** Red startup/version routes remain shared `/v01r00` routes. The Red IDB contains `https://%s:%s@%s:%d/v01r00` at `0xDA76A0`, plus `chassis/startupauth.php` at `0xDA8330`, `chassis/verupauth.php` at `0xDA8348`, and `chassis/verupcomplete.php` at `0xDA8360`.
- **D-03:** Red game routes use `/v08r01`. The Red IDB contains `https://%s:%s@%s:%d/v08r01` at `0xDA7660`.
- **D-04:** Lock `ST8100-1` as the active Red runtime root. Inventory `ST5100` and `ST7100` local roots as inactive or historical only. IDB evidence includes `/data/config/ST8100-1`, `/data/nutdata/ST8100-1`, `/updates/ST8100-1`, `/cache/ST8100-1`, and `ST8100-1-NA-MPR0-K01`.
- **D-05:** Prove Red flag-array widths, packing, and response byte formats from the IDB before adding or accepting a Red AC15 profile. Do not silently inherit Blue/Yellow protocol limits.
- **D-06:** The Phase 18 evidence artifact should include both findings and exact IDB addresses so later agents can re-check the proof quickly. Known route/string evidence includes `0xDA7660`, `0xDA76A0`, `0xDA8330`, `0xDA8348`, `0xDA8360`, `0xDA96F8`, `0xDAA758`, and `0xD72FC0`.
- **D-07:** Runtime logs are acceptance evidence, not a foundation blocker. Phase 18 must still create enough minimal Red route-probe controller shape to let the user run the game and verify request routing.

#### Capability Inventory Shape
- **D-08:** Produce a Red capability evidence matrix. Classify each Red proto/data/IDB route or surface as supported, candidate, absent, or later-phase, with evidence refs and without implying behavior implementation.
- **D-09:** Do not draft the full Red `Ac15EraProfile` in Phase 18. Phase 19 owns the profile after the evidence matrix proves what can be bound.
- **D-10:** Group matrix rows by phase owner: Phase 18 foundation, Phase 19 catalog/profile, Phase 20 runtime/simple compatibility, Phase 21 ChallengeCompe, and explicit absent surfaces.
- **D-11:** Classify Red ChallengeCompe as a shared older-AC15 capability candidate with Red route/proto/IDB evidence. Phase 21 owns the shared contract and any stateful semantics.
- **D-12:** Classify rewardcard, rewardexecution, and Banacoin-adjacent rows as simple compatibility candidates only when IDB or runtime flow requires them. Do not infer item-shop, medal, wallet, payment, or unlock semantics.

#### Adapter Scaffold Exposure
- **D-13:** Expose IDB-known Red game-route suffixes as minimal route probes under `/v08r01` so runtime smoke can reveal the real request sequence. These probes do not prove feature semantics.
- **D-14:** Route probes should deserialize or map enough to log full requests and return the minimal safe Red protobuf response. They must not write DB state, call shared gameplay services, or copy Yellow/Blue behavior.
- **D-15:** Generate adapter-local Red `Wire/` output from immutable `proto/red` inputs. Do not copy Yellow wire and do not hand-author a DTO subset. Dumped proto inputs remain evidence and should not be edited except for documented protogen compatibility.
- **D-16:** Add Red host/settings support with Red enabled in the committed local config and `GameDataPath=wwwroot/data/red/data`, so immediate RPCS3/request-routing smoke is possible in this checkout.
- **D-17:** Keep Red route ownership adapter-local: new Red controllers live in a Red adapter assembly, generated Red wire stays in that adapter, and Host application-part gating controls Red route availability.

#### Preservation Gates
- **D-18:** Use focused automated verification plus a temp-output Host build. Add only meaningful tests that protect observable behavior, enabled-era gating, wire/protobuf shape, or real preservation boundaries.
- **D-19:** Do not add Red gameplay persistence or EF migrations in Phase 18. Red save, score, Dani, Tokkun, ChallengeCompe, and compatibility state belong to later capability phases.
- **D-20:** Phase 18 requires manual cabinet/RPCS3 request-routing smoke before close. The user will run it manually; implementation should make route-probe logging useful for that evidence.
- **D-21:** Reviews should verify existing Green, Blue, Yellow, Nijiiro, shared `/v01r00`, and AC15 shared-core behavior are unchanged except for covered Red additions. Keep the review lightweight and scoped to touched surfaces.

### the agent's Discretion
- Downstream agents may choose exact Red adapter project names, controller file splits, mapper class names, and route-probe response helpers as long as route ownership, generated-wire ownership, and no-state semantics are preserved.
- Downstream planning may decide whether to put route-probe evidence in one Red evidence document or split IDB findings and capability matrix into separate files, provided both are canonical Phase 18 outputs.

### Deferred Ideas (OUT OF SCOPE)
- Red `Ac15EraProfile` binding belongs to Phase 19 after the evidence matrix and IDB-backed limits exist.
- Red runtime identity/userdata/normal/Dani/Tokkun/simple compatibility behavior belongs to Phase 20.
- Shared older-AC15 ChallengeCompe contract and any stateful behavior belong to Phase 21.
- AdminApi/WebUI Red readback and full runtime closeout belong to Phase 22.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| RFND-01 | Developer can review a Red route/version evidence record that identifies supported route prefix, direct-protobuf transport expectations, shared startup/version ownership, HDD/version mapping, active data root, and unresolved client-evidence gaps before routes are finalized. | The IDA-backed evidence table records route/root strings and addresses; the transport section records direct-protobuf expectations plus runtime gaps. [VERIFIED: ida-cli IDA backend] [VERIFIED: codebase grep] |
| RFND-02 | Red is served by a first-class enableable `GameEra.Red` adapter with generated Red wire DTOs from `proto/red`, era settings, Host/DI registration, route ownership, and enabled-era gating. | The implementation-pattern sections identify `GameEra`, Host DI/application-part filtering, `ServerSettings`, `Host.csproj`, existing adapter project shape, and the verified Red protogen command. [VERIFIED: codebase grep] [VERIFIED: protogen temp generation] |
| RFND-03 | Red work preserves current supported-era behavior except where shared code changes are required and existing behavior remains covered. | The validation section maps preservation checks to existing Blue/Green/Yellow/Nijiiro/shared surfaces and recommends scoped tests plus full `Tests/Tests.csproj` and temp-output Host build. [VERIFIED: AGENTS.md] [VERIFIED: .planning/config.json] |
</phase_requirements>

## Summary

Phase 18 should be planned as an evidence-first foundation, not as Red gameplay support. The Red IDB is available through an IDA-backed daemon, and the required first probe returned `ida_available: true` for `.tools/red/EBOOT.ELF.i64`. The IDB confirms `/v08r01` as the Red game route prefix, shared `/v01r00` startup/version route strings, active `ST8100-1` root strings, and the concrete game route suffixes that should become route probes. [VERIFIED: ida-cli IDA backend]

The current repo has the exact outer shape Red needs: `GameEra` enum, Host enabled-era parsing, adapter DI registration, application-part removal for disabled eras, direct-protobuf content-type fallback, adapter-local `Wire/` folders, `MapperlyDefaults`, and xUnit tests. Red should be added as a first-class adapter project and Host era, with route probes that deserialize/log Red generated request DTOs and return minimal protobuf responses without calling gameplay Mediator handlers or writing EF state. [VERIFIED: codebase grep]

The key planning constraint is sequencing. Phase 18 may generate Red wire, add Host/config/gating, write evidence artifacts, and expose IDB-known route probes. It must not add `Ac15EraProfiles.Red`, Red EF tables, Red catalog binding, Red ChallengeCompe semantics, Red Tokkun persistence, or Banacoin/payment state; those belong to Phases 19-21. [VERIFIED: 18-CONTEXT.md]

**Primary recommendation:** Plan Phase 18 as three slices: evidence artifact and capability matrix; adapter/wire/Host gating scaffold; route-probe logging plus preservation verification. [VERIFIED: 18-CONTEXT.md] [VERIFIED: codebase grep]

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|--------------|----------------|-----------|
| Red route/version/root evidence | Planning artifact | IDA/local proto/data | The phase deliverable is an inspectable evidence record, with IDA and local files as source inputs. [VERIFIED: 18-CONTEXT.md] [VERIFIED: ida-cli IDA backend] |
| Red game route probes | Red game-protocol adapter | Host routing | `/v08r01/chassis/*` controllers should live in the Red adapter; Host only enables/disables the adapter assembly. [VERIFIED: 18-CONTEXT.md] [VERIFIED: Host/Program.cs] |
| Shared startup/version routes | Shared game-protocol adapter | Application startup movie handler | Existing `StartupAuthController`, `VerupAuthController`, and `VerupCompleteController` own `/v01r00/chassis/*`; Red should extend HDD-era resolution, not duplicate those routes under `/v08r01`. [VERIFIED: Adapters.GameProtocol.Shared] [VERIFIED: Application/Handlers/GetStartupMovieDataQuery.cs] |
| Red wire DTOs | Red adapter `Wire/` | `proto/red` inputs | Existing AC15 adapters keep generated wire in adapter-local projects; Red proto generated successfully with repo-local `protogen`. [VERIFIED: codebase grep] [VERIFIED: protogen temp generation] |
| Enabled-era gating | Host | Adapter DI extension | Host parses `ServerSettings:Eras`, conditionally calls era DI, and removes disabled adapter application parts. [VERIFIED: Host/Program.cs] |
| Capability inventory | Planning artifact | Application/Ac15 and proto/data evidence | Phase 18 must classify surfaces by phase owner without binding a Red profile. [VERIFIED: 18-CONTEXT.md] |
| Red gameplay state | Later Application/Infrastructure phases | Red adapter mappers | Phase 18 explicitly forbids Red gameplay persistence and EF migrations. [VERIFIED: 18-CONTEXT.md] |

## Project Constraints (from AGENTS.md)

- Blue, Green, Yellow, Nijiiro, and Red-era work must keep era state separate unless state is truly shared identity data such as card/user identity. [VERIFIED: AGENTS.md]
- Controllers should deserialize generated protobuf DTOs, map through `Application/Dtos/Common*`, call Mediator only for implemented behavior, and map back to wire responses. [VERIFIED: AGENTS.md]
- Generated `Wire/` files are not manually cleaned unless protocol output is being regenerated. [VERIFIED: AGENTS.md]
- Runtime data roots must use `PathHelper` and era data path helpers; new runtime code must not hardcode `wwwroot/data/<era>`. [VERIFIED: AGENTS.md]
- AdminApi era routes must preserve legacy routes where they exist and `/api/{era}/...` routes validated by `EraRoute.TryParse`. [VERIFIED: AGENTS.md]
- Tests must protect evidence-backed behavior, runtime state transitions, parser/packing rules, AdminApi/WebUI workflows, or no-cross-era/no-cross-mode persistence boundaries; do not add route inventory, source text, generated protobuf, DI shape, enum numeric, migration body, private method, or `Result = 1` echo tests unless a real runtime failure justifies that assertion. [VERIFIED: AGENTS.md]
- Use temp-output Host builds when a running server locks `Host/bin/Debug/net10.0`. [VERIFIED: AGENTS.md]

## Standard Stack

### Core

| Library / Tool | Version | Purpose | Why Standard |
|----------------|---------|---------|--------------|
| .NET SDK / C# | SDK `10.0.201` available; `global.json` requests `10.0.100` with `latestFeature`; `Directory.Build.props` uses `net10.0` and C# `13`. | Build host, adapters, Application, Infrastructure, tests. | Existing solution and all projects use this stack. [VERIFIED: dotnet --version] [VERIFIED: codebase grep] |
| ASP.NET Core MVC controllers | ASP.NET Core packages pinned to `10.0.7`. | Cabinet HTTP endpoints and protobuf request/response handling. | Host already maps controllers and adapter projects expose route controllers. [VERIFIED: Directory.Packages.props] [VERIFIED: Host/Program.cs] |
| protobuf-net / protobuf-net.AspNetCore | `protobuf-net` `3.2.56`, `protobuf-net.AspNetCore` `3.2.52`; repo-local `protogen.exe` reports `3.2.52+f4db4afce3`. | Generated Red DTOs and direct-protobuf controller binding. | Existing AC15 adapters use generated protobuf-net wire DTOs and `.AddProtoBufNet()`. [VERIFIED: Directory.Packages.props] [VERIFIED: protogen --version] [VERIFIED: Host/Program.cs] |
| Riok.Mapperly | `4.3.1`. | Mechanical wire/Common projection where Red later maps real behavior. | Existing AC15 adapters include `MapperlyDefaults.cs` and Mapperly package references. [VERIFIED: Directory.Packages.props] [VERIFIED: adapter csproj grep] |
| Mediator | `Mediator.SourceGenerator` / `Mediator.Abstractions` `3.0.2`. | Application request dispatch for implemented behavior. | Existing handlers dispatch by `GameEra`; Phase 18 route probes should avoid gameplay handlers until behavior is implemented. [VERIFIED: Directory.Packages.props] [VERIFIED: Application/Handlers grep] |
| EF Core SQLite | EF Core packages `10.0.7`; SQLite provider `10.0.7`. | Later Red-owned persistence. | Phase 18 must not add migrations, but preservation checks must ensure existing EF state is untouched. [VERIFIED: Directory.Packages.props] [VERIFIED: 18-CONTEXT.md] |
| xUnit | `xunit` `2.9.3`, runner `2.8.2`, Microsoft.NET.Test.Sdk `17.14.1`. | Focused preservation and route-probe behavior tests. | `Tests/Tests.csproj` is the active test project. [VERIFIED: Directory.Packages.props] [VERIFIED: Tests/Tests.csproj] |
| ida-cli / idalib | `ida_cli` importable from `H:/IDACLI`; `idapro` importable; Red daemon metadata exists under `C:/Users/10614/.ida-cli/daemons`. | IDA-backed Red evidence. | Required by Phase 18 context and user hint; backend probe succeeded with `ida_available: true`. [VERIFIED: ida-cli skill] [VERIFIED: shell probe] [VERIFIED: ida-cli IDA backend] |

### Supporting

| Library / Tool | Version | Purpose | When to Use |
|----------------|---------|---------|-------------|
| Serilog | `Serilog.AspNetCore` `10.0.0`, `Serilog.Expressions` `5.0.0`. | Route-probe request logging and unknown-route diagnostics. | Use existing controller `Logger.LogInformation("Red ... request: {@Request}", request)` style for manual smoke visibility. [VERIFIED: Directory.Packages.props] [VERIFIED: existing controllers] |
| `PathHelper` | repo-local | Resolves `wwwroot/data/<era>` roots from process path. | Use from Red catalog/data-path helpers in later catalog work; Phase 18 Host data junction follows current `Host.csproj` pattern. [VERIFIED: Infrastructure/GameDataCatalog/PathHelper.cs] |
| `Application/Ac15` | repo-local | Shared AC15 capability services, protocol limits, and profiles. | Read but do not bind Red profile in Phase 18; later phases bind only after Red limits are proven. [VERIFIED: Application/Ac15 grep] [VERIFIED: 18-CONTEXT.md] |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| New `Adapters.GameProtocol.Red` project | Fold Red into Yellow or Blue adapter | Rejected: Red route ownership, generated wire, and data roots must be first-class and adapter-local. [VERIFIED: 18-CONTEXT.md] [VERIFIED: AGENTS.md] |
| Red `Ac15EraProfile` in Phase 18 | Add profile immediately with common AC15 limits | Rejected: Red flag widths and byte formats must be proven before profile binding; Phase 19 owns profile. [VERIFIED: 18-CONTEXT.md] |
| Route probes for every `proto/red` message | Route only IDB-known suffixes under `/v08r01` | Use IDB-known suffixes in Phase 18; proto-only surfaces such as `getbanacoininfo` and `getreitai` remain candidates until runtime/IDA route evidence requires them. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| Hand-authored DTO subset | Generated Red wire from `proto/red` | Hand-authored DTOs risk field/tag drift and violate the dumped-proto evidence rule. [VERIFIED: AGENTS.md] [VERIFIED: 18-CONTEXT.md] |

**Installation:**

No new external package installation is required for Phase 18. Use existing solution packages and repo-local `.tools/protogen.exe`. [VERIFIED: Directory.Packages.props] [VERIFIED: protogen --version]

**Wire generation command verified in temp:**

```powershell
.\.tools\protogen.exe --csharp_out=$out -Iproto\red +nullablevaluetype=yes taiko.proto vsinterface.proto
```

The verified temp run generated `taiko.cs` and `vsinterface.cs`, and optional primitive fields are nullable value types such as `uint?` and `bool?`. [VERIFIED: protogen temp generation]

For implementation, generate into a temp directory first, then place outputs as `Adapters.GameProtocol.Red/Wire/Game.cs` and `Adapters.GameProtocol.Red/Wire/VsInterface.cs` under `namespace TaikoLocalServer.Adapters.GameProtocol.Red.Wire`, matching existing adapter convention. [VERIFIED: existing Wire files] [VERIFIED: protogen temp generation]

## Package Legitimacy Audit

Phase 18 does not install external packages, so the Package Legitimacy Gate is not required. Existing package names and versions above come from `Directory.Packages.props`; they are project-pinned dependencies, not new recommendations to install. [VERIFIED: Directory.Packages.props]

**Packages removed due to slopcheck [SLOP] verdict:** none; no package candidates were evaluated because the phase installs no packages. [VERIFIED: phase scope]
**Packages flagged as suspicious [SUS]:** none; no package candidates were evaluated because the phase installs no packages. [VERIFIED: phase scope]

## Red Evidence Record

### IDA Backend

The IDA probe used `AgentSession.start(".tools/red/EBOOT.ELF.i64", daemon=True, require_ida=True)` and returned `{'database_opened': True, 'ida_available': True, 'name': 'idalib', 'target_path': 'H:\\TaikoLocalServer\\.tools\\red\\EBOOT.ELF.i64'}`. [VERIFIED: ida-cli IDA backend]

### Route and Root Strings

| Finding | Evidence | Planning Impact |
|---------|----------|-----------------|
| Red game route prefix is `/v08r01`. | IDA string `https://%s:%s@%s:%d/v08r01` at `0xDA7660`, xref from `0xEB9B50`. [VERIFIED: ida-cli IDA backend] | Add Red game route probes under `/v08r01/chassis/*`. |
| Startup/version prefix remains `/v01r00`. | IDA string `https://%s:%s@%s:%d/v01r00` at `0xDA76A0`, xref from `0xEB9B60`. [VERIFIED: ida-cli IDA backend] | Keep `startupauth.php`, `verupauth.php`, and `verupcomplete.php` in shared adapter; extend HDD mapping rather than duplicate routes. |
| Shared startup suffixes are present. | `chassis/startupauth.php` at `0xDA8330`, `chassis/verupauth.php` at `0xDA8348`, `chassis/verupcomplete.php` at `0xDA8360`. [VERIFIED: ida-cli IDA backend] | Preserve shared `/v01r00/chassis/*` ownership. |
| Active Red root is `ST8100-1`. | IDA strings include `/data/config/ST8100-1` at `0xD4DA40`, `/data/nutdata/ST8100-1` at `0xD4DAA8`, `/updates/ST8100-1` at `0xD4DAC0`, `/cache/ST8100-1` at `0xD6E408`, `ST8100-1` at `0xD72FC0`, and `ST8100-1-NA-MPR0-K01` at `0xDB13D0`. [VERIFIED: ida-cli IDA backend] | Treat `Host/wwwroot/data/red/data/config/ST8100-1` as active; inventory `ST5100-*` and `ST7100-1` only as historical/inactive. |
| Local Red data has multiple config roots. | `Host/wwwroot/data/red/data/config` contains `common`, `ST5100-1`, `ST5100-7`, `ST7100-1`, and `ST8100-1`. [VERIFIED: filesystem audit] | Planner must not choose a root by filename guesswork; use IDA-proven `ST8100-1`. |
| Active root required-file candidates exist. | `ST8100-1/musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, `spacialbaid.xml`, `fumen/tuning.bin`, and `movie/` exist in the local Red tree. [VERIFIED: filesystem audit] | Phase 19 can bind catalog loaders after shape validation; Phase 18 should only document existence and avoid catalog support claims. |
| No Red sidecar JSON exists yet. | `Host/wwwroot/data/red` contains only the `data` directory; `rg --files Host/wwwroot/data/red | rg '\.json$'` found no JSON files. [VERIFIED: filesystem audit] | Phase 18 should not advertise Red sidecar-backed telop/recommend/movie/folder data; later phases add committed JSON only where evidence requires it. |

### IDB-Known Red Game Route Suffixes

| Route Suffix | Address | Red Proto Message Surface | Phase 18 Classification |
|--------------|---------|---------------------------|-------------------------|
| `chassis/playresult.php` | `0xDA96F8` | `PlayResultRequest` / `PlayResultResponse` | Route probe only; no writes. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/banacoinerrorlog.php` | `0xDA9710` | `BanacoinerrorlogRequest` / `BanacoinerrorlogResponse` | Stateless compatibility probe candidate; no payment state. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/baidcheck.php` | `0xDAA710` | `BAIDRequest` / `BAIDResponse` | Route probe only; identity implementation belongs to Phase 20. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/mydonentry.php` | `0xDAA728` | `MydonEntryRequest` / `MydonEntryResponse` | Route probe only; identity/profile creation belongs to Phase 20. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/userdata.php` | `0xDAA740` | `UserDataRequest` / `UserDataResponse` | Route probe only; userdata readback belongs to Phase 20. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/challengecompe.php` | `0xDAA758` | `ChallengeCompeRequest` / `ChallengeCompeResponse` | Older-AC15 ChallengeCompe candidate; stateful semantics belong to Phase 21. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/balancecheck.php` | `0xDAA778` | `BalancecheckRequest` / `BalancecheckResponse` | Banacoin-adjacent stateless compatibility candidate only. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/banacoinpayment.php` | `0xDAA798` | `BanacoinpaymentRequest` / `BanacoinpaymentResponse` | Banacoin-adjacent stateless compatibility candidate only. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/crownsdata.php` | `0xDAA7B8` | `CrownsDataRequest` / `CrownsDataResponse` | Route probe only; crown byte format/width proof required before profile binding. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/recommend.php` | `0xDAA7D0` | `RecommendRequest` / `RecommendResponse` | Route probe only; catalog-backed behavior belongs to Phase 19/20. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/selfbest.php` | `0xDAA7E8` | `SelfBestRequest` / `SelfBestResponse` | Route probe only; score state belongs to Phase 20. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/heartbeat.php` | `0xDAA800` | `HeartBeatRequest` / `HeartBeatResponse` | Minimal safe probe can return status fields; no state. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/rewardcardcheck.php` | `0xDAA818` | `RewardcardcheckRequest` / `RewardcardcheckResponse` | Simple compatibility candidate only; no unlock/payment semantics. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/rewardexecution.php` | `0xDAA838` | `RewardexecutionRequest` / `RewardexecutionResponse` | Simple compatibility candidate only; no unlock state in Phase 18. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/initialdatacheck.php` | `0xDAADE8` | `InitialdatacheckRequest` / `InitialdatacheckResponse` | Route probe only; feature rows must not advertise unsupported behavior. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/tournamentcheck.php` | `0xDAAE08` | `TournamentcheckRequest` / `TournamentcheckResponse` | Route probe only; no gacha/tournament state in Phase 18. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/bookkeeping.php` | `0xDAAE28` | `BookKeepingRequest` / `BookKeepingResponse` | Log-and-success probe only. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/coinsetting.php` | `0xDAAE40` | `CoinsettingRequest` / `CoinsettingResponse` | Log-and-success probe only. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/gettelop.php` | `0xDAAE58` | `GettelopRequest` / `GettelopResponse` | Route probe only; catalog sidecar/data proof belongs later. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/getfolder.php` | `0xDAAE70` | `GetfolderRequest` / `GetfolderResponse` | Route probe only; folder data proof belongs later. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/taikojuku.php` | `0xDAAE88` | `TaikojukuRequest` / `TaikojukuResponse` | Route probe only; Dani/Taikojuku binding belongs Phase 19/20. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |
| `chassis/headclerk2.php` | `0xDAAEA0` | `HeadClerk2Request` / `HeadClerk2Response` | Log-and-success probe only. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] |

### Proto-Only Surfaces Not Proven as IDB Routes

`proto/red/taiko.proto` contains `GetreitaiRequest/Response` and `GetbanacoininfoRequest/Response`, but the IDA string pass did not find `chassis/getreitai.php` or `chassis/getbanacoininfo.php` among the route suffix strings. [VERIFIED: proto/red/taiko.proto] [VERIFIED: ida-cli IDA backend]

Treat those as candidate compatibility surfaces only if runtime logs or a deeper IDA path proves the client requests them. [VERIFIED: 18-CONTEXT.md]

### Explicit Absence / Later-Phase Boundaries

| Surface | Evidence | Planning Decision |
|---------|----------|-------------------|
| Item shop purchase/info | Red proto has no `Getitemshopinfo*` or `Itempurchase*` messages in the message inventory. [VERIFIED: proto/red/taiko.proto] | Do not add Red item-shop controllers or settings validation in Phase 18. |
| Blue-style battle | Red proto route/message inventory has no battle userdata or battle route messages; IDA route list has no `battleuserdata.php`. [VERIFIED: proto/red/taiko.proto] [VERIFIED: ida-cli IDA backend] | Keep battle absent. |
| WaiWai | Red proto search did not show WaiWai fields; the roadmap explicitly says Red is not WaiWai work. [VERIFIED: proto/red/taiko.proto] [VERIFIED: ROADMAP.md] | Keep WaiWai absent. |
| Don/Katsu medal model | Red proto exposes Don point/reward fields (`reward_ptn`, `reward_progress`, `get_donpoint`, `total_get_donpoint`, `total_use_donpoint`) rather than Yellow/Blue/Green shop medal semantics. [VERIFIED: proto/red/taiko.proto] | Do not map Red reward fields to item-shop/medal behavior in Phase 18. |
| Red `Ac15EraProfile` | Phase context defers profile binding to Phase 19 after evidence matrix and IDB-backed limits. [VERIFIED: 18-CONTEXT.md] | Do not add `Ac15EraProfiles.Red` in Phase 18. |

### Unresolved Evidence Gaps

- Manual RPCS3/cabinet smoke has not yet shown the actual Red request sequence, request content types, or whether proto-only routes are requested. [VERIFIED: 18-CONTEXT.md]
- Red flag-array byte widths and crown/protocol packing must still be proven from IDB/runtime before a Red `Ac15EraProfile` or reusable limits are accepted. [VERIFIED: 18-CONTEXT.md]
- Route probes can prove routing and logging, but they do not prove gameplay semantics, catalog readback, ChallengeCompe state, Tokkun state, or Banacoin authority. [VERIFIED: 18-CONTEXT.md]
- `GetStartupMovieDataQuery.ResolveEra` currently maps HDD version prefixes `9`, `10`, `11`, and `12` to Yellow, Blue, Green, and Nijiiro; Red needs `8 => GameEra.Red` before Red startup movie permissions can work. [VERIFIED: Application/Handlers/GetStartupMovieDataQuery.cs]

## Capability Inventory

| Phase Owner | Capability / Surface | Classification | Evidence | Planning Guidance |
|-------------|----------------------|----------------|----------|-------------------|
| Phase 18 | Red adapter, generated wire, route probes, Host gating | Supported foundation work | `proto/red` generated with `protogen`; IDA route list exists; Host has era gating pattern. [VERIFIED: protogen temp generation] [VERIFIED: ida-cli IDA backend] [VERIFIED: Host/Program.cs] | Implement first-class scaffold without EF/gameplay writes. |
| Phase 18 | Shared startup/version ownership | Supported foundation work | `/v01r00` and startup/version strings in IDB; shared controllers already own `/v01r00/chassis/*`. [VERIFIED: ida-cli IDA backend] [VERIFIED: Adapters.GameProtocol.Shared] | Extend HDD-era mapping; do not duplicate startup/version routes under `/v08r01`. |
| Phase 19 | Red catalog/profile binding | Later-phase candidate | Active `ST8100-1` files exist, but profile limits and catalog sidecar decisions are not yet proven. [VERIFIED: filesystem audit] [VERIFIED: 18-CONTEXT.md] | Prepare evidence matrix now; bind catalog/profile in Phase 19. |
| Phase 20 | Red identity/userdata/normal/self-best/crowns/Dani/Tokkun tutorial | Later-phase candidate | Proto has BAID, MydonEntry, UserData, PlayResult, SelfBest, CrownsData, Taikojuku, Tokkun fields. [VERIFIED: proto/red/taiko.proto] | No state in Phase 18; later Red-owned tables and handlers only. |
| Phase 20 | Rewardcard/rewardexecution/Don point/simple compatibility | Later-phase simple compatibility candidate | IDA routes and proto messages exist for rewardcard/rewardexecution; Red proto has reward/Don point fields. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] | Keep stateless/probe-only now; no unlock/shop semantics. |
| Phase 20 | Banacoin-adjacent balance/payment/error | Later-phase simple compatibility candidate | IDA routes and proto messages exist for balancecheck, banacoinpayment, banacoinerrorlog. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] | Return minimal safe responses only as probes; no wallet/payment state. |
| Phase 21 | ChallengeCompe | Shared older-AC15 candidate | IDA route `challengecompe.php`; proto has ChallengeCompe request/response and playresult challenge arrays. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] | Phase 18 records evidence only; Phase 21 owns contract and state. |
| Explicit absent | Red item shop | Absent for Phase 18 | No Red item-shop route strings or item-shop request/response messages found. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto] | Do not add Red `EnableShop` requirement or shop controller. |
| Explicit absent | Blue battle/WaiWai/AI/ghost/token-count/shop-folder | Absent for Phase 18 | Roadmap out-of-scope plus no matching Red route/proto evidence in current inventory. [VERIFIED: ROADMAP.md] [VERIFIED: proto/red/taiko.proto] | Leave absent; do not invent stubs. |

## Architecture Patterns

### System Architecture Diagram

```text
Red cabinet POST
  |
  | /v01r00/chassis/startupauth.php, verupauth.php, verupcomplete.php
  v
Adapters.GameProtocol.Shared controllers
  |
  | startupauth hdd_ver -> ResolveEra(...)
  v
Application startup movie query -> enabled-era catalog readback if configured

Red cabinet POST
  |
  | /v08r01/chassis/{IDA-known suffix}.php
  v
Adapters.GameProtocol.Red controller probe
  |
  | deserialize generated Red Wire request, structured log, no DB write
  v
minimal generated Red Wire response
  |
  v
manual RPCS3/cabinet smoke logs decide next evidence gaps

Host startup
  |
  | ServerSettings:Eras -> enabledEras
  v
conditional adapter DI + ApplicationPart removal
  |
  v
Red routes exist only when GameEra.Red is enabled
```

This data flow matches existing Host controller mapping, shared startup/version controllers, and adapter-local controller patterns. [VERIFIED: Host/Program.cs] [VERIFIED: Adapters.GameProtocol.Shared] [VERIFIED: existing AC15 adapters]

### Recommended Project Structure

```text
Adapters.GameProtocol.Red/
|-- Adapters.GameProtocol.Red.csproj
|-- DependencyInjection.cs
|-- GlobalUsings.cs
|-- MapperlyDefaults.cs
|-- RedAdapterMarker.cs
|-- Controllers/
|   |-- BaidController.cs
|   |-- PlayResultController.cs
|   |-- ...
|-- Mappers/
|   `-- (only route-probe helpers now; real mappers later)
`-- Wire/
    |-- Game.cs
    `-- VsInterface.cs

docs or .planning phase artifact:
`-- 18-RED-EVIDENCE.md or equivalent evidence/capability matrix
```

This structure mirrors existing Blue/Green/Yellow adapter project shape while preserving Red route and generated-wire ownership. [VERIFIED: existing adapter csproj/files] [VERIFIED: 18-CONTEXT.md]

### Pattern 1: Host Era Gating

**What:** Add `GameEra.Red`, parse `ServerSettings:Eras:Red`, conditionally call `AddGameProtocolRed()`, and remove the Red application part when Red is disabled. [VERIFIED: Host/Program.cs]

**When to use:** Phase 18 scaffold and route gating. [VERIFIED: RFND-02]

**Example:**

```csharp
// Source: Host/Program.cs [VERIFIED: codebase grep]
if (enabledEras.Contains(GameEra.Yellow))
{
    builder.Services.AddGameProtocolYellow();
}

// Existing application-part removal pattern:
if (!enabledEras.Contains(GameEra.Yellow))
{
    RemoveApplicationPart(apm, "TaikoLocalServer.Adapters.GameProtocol.Yellow");
}
```

Apply the same pattern for `GameEra.Red` and `TaikoLocalServer.Adapters.GameProtocol.Red`; keep route ownership in the adapter. [VERIFIED: Host/Program.cs] [VERIFIED: 18-CONTEXT.md]

### Pattern 2: Direct-Protobuf Fallback

**What:** Host currently assumes `application/protobuf` for POST requests with missing content type under AC15/Nijiiro route prefixes. [VERIFIED: Host/Program.cs]

**When to use:** Add `/v08r01/chassis` to `ShouldAssumeProtobufRequest` so Red cabinet probes deserialize like other direct-protobuf game endpoints. [VERIFIED: Host/Program.cs] [VERIFIED: ida-cli IDA backend]

**Example:**

```csharp
// Source: Host/Program.cs [VERIFIED: codebase grep]
return path.StartsWithSegments("/v11r01/chassis", StringComparison.OrdinalIgnoreCase)
       || path.StartsWithSegments("/v10r03/chassis", StringComparison.OrdinalIgnoreCase)
       || path.StartsWithSegments("/v09r02/chassis", StringComparison.OrdinalIgnoreCase)
       || path.StartsWithSegments("/v01r00/chassis", StringComparison.OrdinalIgnoreCase);
```

### Pattern 3: Route Probe Controller

**What:** A Phase 18 route probe should deserialize the Red request, log it, and return the minimal generated Red response without Mediator or EF. [VERIFIED: 18-CONTEXT.md]

**When to use:** For IDB-known game routes before Phase 20 implements runtime behavior. [VERIFIED: ida-cli IDA backend]

**Example:**

```csharp
// Source pattern: existing Blue/Yellow stateless compatibility controllers [VERIFIED: codebase grep]
[ApiController]
[Route("/v08r01/chassis/bookkeeping.php")]
public sealed class BookkeepingController : BaseProtocolController<BookkeepingController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Bookkeeping([FromBody] BookKeepingRequest request)
    {
        Logger.LogInformation("Red BookKeeping request: {@Request}", request);
        return Ok(new BookKeepingResponse { Result = 1 });
    }
}
```

For response DTOs with required non-`result` fields, planners must inspect generated Red wire before writing the minimal response; required fields must be populated enough for serialization to succeed. [VERIFIED: proto/red/taiko.proto] [VERIFIED: protogen temp generation]

### Pattern 4: Shared Startup HDD Mapping

**What:** `GetStartupMovieDataQuery.ResolveEra` maps `hdd_ver / 100` to an enabled era and returns no movies when unknown or disabled. [VERIFIED: Application/Handlers/GetStartupMovieDataQuery.cs]

**When to use:** Add Red as `8 => GameEra.Red` only after Red enum/catalog registration exists. [VERIFIED: ida-cli IDA backend] [VERIFIED: 18-CONTEXT.md]

**Example:**

```csharp
// Source: Application/Handlers/GetStartupMovieDataQuery.cs [VERIFIED: codebase grep]
var requestedEra = (hddVer / 100) switch
{
    9 => GameEra.Yellow,
    10 => GameEra.Blue,
    11 => GameEra.Green,
    12 => GameEra.Nijiiro,
    _ => (GameEra?)null
};
```

### Anti-Patterns to Avoid

- **Adding `Ac15EraProfiles.Red` in Phase 18:** Red protocol limits and byte formats are not yet proven, and context defers the profile to Phase 19. [VERIFIED: 18-CONTEXT.md]
- **Copying Yellow controllers as behavior:** Red route probes can mirror structure, but they must not call Yellow/Blue handlers, copy shop semantics, or write persistence. [VERIFIED: 18-CONTEXT.md] [VERIFIED: AGENTS.md]
- **Editing `proto/red/*.proto`:** Proto files are dumped source-of-truth evidence; generate wire from them and leave inputs untouched. [VERIFIED: AGENTS.md] [VERIFIED: 18-CONTEXT.md]
- **Adding Red EF migrations:** Phase 18 explicitly forbids gameplay persistence and migrations. [VERIFIED: 18-CONTEXT.md]
- **Testing route inventory/source text:** Project test rules reject route inventory/source-shape tests unless backed by a specific runtime failure. Use build, controller behavior where useful, and runtime smoke logs. [VERIFIED: AGENTS.md]

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Red protobuf DTOs | Hand-written request/response classes | Repo-local `.tools/protogen.exe` with `+nullablevaluetype=yes` | Field numbers, optional presence, and generated names must match dumped proto evidence. [VERIFIED: protogen temp generation] |
| Red route prefix/root evidence | Manual string scan or wiki guess | IDA-backed `ida-cli` daemon over `.tools/red/EBOOT.ELF.i64` | Phase context locks IDA as authority; backend probe succeeded. [VERIFIED: 18-CONTEXT.md] [VERIFIED: ida-cli IDA backend] |
| Enabled-era gating | Custom middleware per era | Existing Host `enabledEras` + `ApplicationPartManager` removal | Host already gates Green/Blue/Yellow/Nijiiro routes this way. [VERIFIED: Host/Program.cs] |
| Red runtime state in Phase 18 | New EF tables, repositories, or shared AC15 tables | No-state route probes only | Phase 18 forbids Red gameplay persistence; later phases use Red-owned tables where proven. [VERIFIED: 18-CONTEXT.md] |
| Data root resolution | Hardcoded handler paths | `PathHelper.GetDataPath(GameEra.Red)` and Red data path helpers in later phases | AGENTS.md forbids hardcoded runtime `wwwroot/data/<era>` paths in new runtime code. [VERIFIED: AGENTS.md] |
| Red capability semantics | Copying Blue/Yellow feature behavior | Evidence matrix by surface and phase owner | Red support must be capability composition, not a clone. [VERIFIED: ROADMAP.md] |

**Key insight:** Phase 18 is proving boundaries and creating a safe probe surface; custom gameplay shortcuts now would become false evidence for later phases. [VERIFIED: 18-CONTEXT.md]

## Common Pitfalls

### Pitfall 1: Treating Route Probes as Feature Support

**What goes wrong:** A probe returning protobuf `Result = 1` is mistaken for implemented Red userdata, ChallengeCompe, reward, or Banacoin behavior. [VERIFIED: 18-CONTEXT.md]
**Why it happens:** Existing AC15 eras have many implemented controllers, so a route controller can look like feature support. [VERIFIED: codebase grep]
**How to avoid:** Name evidence rows and summaries as "route probe only" until a later phase adds handlers/state. [VERIFIED: 18-CONTEXT.md]
**Warning signs:** Controller calls Mediator gameplay services or writes EF state in Phase 18. [VERIFIED: 18-CONTEXT.md]

### Pitfall 2: Accidentally Advertising Item Shop

**What goes wrong:** Red config/settings validation or initial-data mapping inherits Blue/Green/Yellow `EnableShop` expectations. [VERIFIED: Application/Settings/ServerSettingsOptionsValidationExtensions.cs]
**Why it happens:** Current `Ac15ShopEras` includes Green, Blue, and Yellow; adding Red there would imply unsupported shop settings. [VERIFIED: Application/Settings/ServerSettingsOptionsValidationExtensions.cs]
**How to avoid:** Add Red settings without `EnableShop`/`ActiveShopSeasonId` validation unless a later evidence phase proves shop support. [VERIFIED: proto/red/taiko.proto] [VERIFIED: 18-CONTEXT.md]
**Warning signs:** `ServerSettings:Eras:Red:EnableShop` becomes required in Phase 18. [VERIFIED: 18-CONTEXT.md]

### Pitfall 3: Inheriting AC15 Limits Before Proof

**What goes wrong:** Red is added to `Ac15EraProfiles` with common Blue/Green/Yellow byte widths and ranges. [VERIFIED: Application/Ac15/Ac15EraProfiles.cs]
**Why it happens:** Current `Ac15EraProfiles` uses a common Blue/Green/Yellow limit factory. [VERIFIED: Application/Ac15/Ac15EraProfiles.cs]
**How to avoid:** Record Red limits as unresolved until IDB/runtime proof; Phase 19 owns profile binding. [VERIFIED: 18-CONTEXT.md]
**Warning signs:** Tests assert Red flag byte lengths using `Ac15EraProfiles.Blue/Yellow` constants. [VERIFIED: AGENTS.md]

### Pitfall 4: Adding Proto-Only Routes Without Runtime Evidence

**What goes wrong:** `getreitai.php` or `getbanacoininfo.php` is added because messages exist in `proto/red`. [VERIFIED: proto/red/taiko.proto]
**Why it happens:** Proto surface is mistaken for route evidence. [VERIFIED: ROADMAP.md]
**How to avoid:** Add Phase 18 probes only for IDB-known suffixes; record proto-only surfaces as candidates. [VERIFIED: ida-cli IDA backend] [VERIFIED: 18-CONTEXT.md]
**Warning signs:** Route list includes a suffix not found in the IDA route inventory or manual smoke logs. [VERIFIED: ida-cli IDA backend]

### Pitfall 5: Modifying Dumped Proto Inputs

**What goes wrong:** Generated C# is made easier by editing `proto/red/*.proto`. [VERIFIED: memory] [VERIFIED: AGENTS.md]
**Why it happens:** Protogen output names or namespace require adapter cleanup. [VERIFIED: protogen temp generation]
**How to avoid:** Generate to temp, move/namespace generated C# as adapter convention requires, and leave proto inputs untouched unless a documented protogen compatibility issue forces a minimal adjustment. [VERIFIED: 18-CONTEXT.md]
**Warning signs:** Git diff includes `proto/red/taiko.proto` or `proto/red/vsinterface.proto`. [VERIFIED: 18-CONTEXT.md]

## Code Examples

### Existing Adapter Project Shape

```xml
<!-- Source: Adapters.GameProtocol.Yellow/Adapters.GameProtocol.Yellow.csproj [VERIFIED: codebase grep] -->
<Project Sdk="Microsoft.NET.Sdk">
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

### Existing Mapperly Defaults

```csharp
// Source: Adapters.GameProtocol.Yellow/MapperlyDefaults.cs [VERIFIED: codebase grep]
using Riok.Mapperly.Abstractions;

[assembly: MapperDefaults(
    AutoUserMappings = false,
    RequiredMappingStrategy = RequiredMappingStrategy.Target)]
```

### Existing Shared Startup Route

```csharp
// Source: Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs [VERIFIED: codebase grep]
[ApiController]
[Route("/v01r00/chassis/startupauth.php")]
public sealed class StartupAuthController : BaseProtocolController<StartupAuthController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> StartupAuth([FromBody] StartupAuthRequest request)
    {
        Logger.LogInformation("StartupAuth request: {@Request}", request);
        var response = new StartupAuthResponse { Result = 1 };
        var movieData = await Mediator.Send(
            new GetStartupMovieDataQuery(request.HddVer),
            HttpContext.RequestAborted);
        // ...
        return Ok(response);
    }
}
```

### Existing Stateless Compatibility Shape

```csharp
// Source: Adapters.GameProtocol.Blue/Controllers/GetBanacoinInfoController.cs [VERIFIED: codebase grep]
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

For Red, use this shape only for Phase 18 route probes and simple compatibility candidates; do not infer wallet/payment semantics. [VERIFIED: 18-CONTEXT.md]

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Add each AC15 era by copying another adapter's behavior. | Capability composition: each era owns routes/wire/state, shared `Application/Ac15` modules are used only where behavior is proven identical. | AC15 core/composition specs dated 2026-06-07 and 2026-06-11. [VERIFIED: docs/superpowers/specs/2026-06-07-ac15-core-extraction-design.md] [VERIFIED: docs/superpowers/specs/2026-06-11-ac15-capability-composition-design.md] | Red should be a first-class composition root, not a Yellow/Blue variant. |
| Generated optional primitive presence handled by production `ShouldSerialize*` calls. | `protogen +nullablevaluetype=yes` for nullable optional primitive properties; production mappers should read nullable values directly. | Phase 16.1. [VERIFIED: .planning/milestones/v1.2-phases/16.1-ac15-mapperly-mapper-rewrite-and-presence-semantics/16.1-CONTEXT.md] | Red generation should use `+nullablevaluetype=yes` from the start. |
| Shared AC15 persistence adapters/repositories. | Direct `ITaikoDbContext`, concrete era `DbSet`s, narrow row-shape interfaces, generic helpers, and Mapperly delegates. | Phase 16.2 correction. [VERIFIED: .planning/milestones/v1.2-phases/16.2-ac15-shared-core-simplification-and-reuse-cleanup/16.2-CONTEXT.md] | Red later state should use Red-owned tables without repository-shaped hiding. |
| Wiki/public pages as behavior authority. | Local proto/wire/runtime/IDA/data evidence outranks wiki and stale notes. | Current AGENTS/requirements/roadmap. [VERIFIED: AGENTS.md] [VERIFIED: REQUIREMENTS.md] | Phase 18 evidence artifact must cite local proof and gaps. |

**Deprecated/outdated:**

- Treating Red as "Yellow-like" by default is outdated for Phase 18; the current roadmap says Red is capability-first and not later-era shop/medal/WaiWai/battle work. [VERIFIED: ROADMAP.md]
- Adding route stubs for absent features is outdated; unsupported client features stay absent unless route/runtime evidence requires a probe or compatibility endpoint. [VERIFIED: docs/superpowers/specs/2026-06-07-ac15-core-extraction-design.md] [VERIFIED: 18-CONTEXT.md]

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | ASVS applicability mapping is based on the phase technology surface and project config, not an external ASVS lookup in this session. [ASSUMED] | Security Domain | Security checklist may need refinement during plan review if a stricter project security interpretation is required. |
| A2 | Minimal route-probe responses are assumed safe when they populate all generated required response fields and write no state. [ASSUMED] | Architecture Patterns / Validation Architecture | A specific Red route may require more fields for the client to continue routing smoke; manual RPCS3/cabinet logs must decide. |

## Open Questions (RESOLVED)

1. **Which Red routes are actually called in the first runtime smoke?**
   - What we know: IDA route strings identify the route suffix inventory. [VERIFIED: ida-cli IDA backend]
   - What's unclear: actual ordering, retry behavior, content-type behavior, and whether proto-only routes appear. [VERIFIED: 18-CONTEXT.md]
   - Recommendation: Route probes should log every full request object and unknown routes; manual RPCS3/cabinet smoke closes this gap. [VERIFIED: 18-CONTEXT.md]
   - RESOLVED disposition: Plan 18-04 owns this closure through IDB-known no-state route probes, useful unknown-route/request logging, and a manual RPCS3/cabinet routing smoke checkpoint. Phase 18 does not assume a final runtime sequence before that smoke evidence exists.

2. **What are the Red protocol byte widths and packing rules?**
   - What we know: Red proto has flag and byte fields, and current Blue/Green/Yellow profiles share common limits. [VERIFIED: proto/red/taiko.proto] [VERIFIED: Application/Ac15/Ac15EraProfiles.cs]
   - What's unclear: whether Red's runtime byte widths and crown packing match later AC15 eras. [VERIFIED: 18-CONTEXT.md]
   - Recommendation: Leave `Ac15EraProfiles.Red` out of Phase 18 and capture this as a Phase 19 proof task. [VERIFIED: 18-CONTEXT.md]
   - RESOLVED disposition: Deferred to Phase 19. Phase 18 must not add `Ac15EraProfiles.Red`, Red gameplay packing, or Red profile/catalog binding.

3. **Should `getbanacoininfo.php` and `getreitai.php` become Red routes?**
   - What we know: Red proto contains these messages, but the IDA route string inventory did not find matching suffixes. [VERIFIED: proto/red/taiko.proto] [VERIFIED: ida-cli IDA backend]
   - What's unclear: whether runtime flow reaches them through computed strings or alternate code paths not captured by the string inventory. [VERIFIED: ida-cli IDA backend]
   - Recommendation: Do not add them in the initial route-probe list unless runtime logs or deeper IDA proof requires them. [VERIFIED: 18-CONTEXT.md]
   - RESOLVED disposition: Exclude proto-only routes from the initial Phase 18 route-probe list unless manual runtime logs or additional IDA proof shows the client reaches them. Do not infer Banacoin/payment or Reitai behavior from proto presence alone.

4. **Where should the canonical Red evidence live?**
   - What we know: Phase context allows one evidence document or split IDB findings and capability matrix as long as both are canonical Phase 18 outputs. [VERIFIED: 18-CONTEXT.md]
   - What's unclear: exact filenames and whether the planner wants docs under `.planning/phases/18...` only or an additional docs artifact. [VERIFIED: 18-CONTEXT.md]
   - Recommendation: Plan a single `.planning/phases/18-red-evidence-and-capability-foundation/18-RED-EVIDENCE.md` unless planner chooses a split. [ASSUMED]
   - RESOLVED disposition: Use `.planning/phases/18-red-evidence-and-capability-foundation/18-RED-EVIDENCE.md` as the canonical Phase 18 evidence artifact. Plan 18-01 may include both the route/version evidence and the capability matrix in that single file.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|-------------|-----------|---------|----------|
| .NET SDK | Build/tests/Host scaffold | yes | `10.0.201` available; `global.json` requests `10.0.100` with latestFeature roll-forward | none needed. [VERIFIED: dotnet --version] [VERIFIED: global.json] |
| `.tools/protogen.exe` | Red generated wire | yes | `protogen 3.2.52+f4db4afce3` | none needed; repo-local tool is present. [VERIFIED: protogen --version] |
| `ida_cli` Python package | IDA evidence | yes | importable from `H:/IDACLI/src/ida_cli` | use direct IDA only if daemon fails; current daemon works. [VERIFIED: shell probe] |
| `idapro` Python package | IDA-backed backend | yes | importable in current Python | blocking if absent; current probe succeeded. [VERIFIED: shell probe] [VERIFIED: ida-cli IDA backend] |
| Red IDA daemon | Repeated IDA probes | yes | live Python process with recent daemon metadata under `C:/Users/10614/.ida-cli/daemons` | `AgentSession.start(..., daemon=True)` can start/connect if needed. [VERIFIED: shell probe] [VERIFIED: ida-cli source read] |
| Red operator data tree | Data-root evidence and later catalog | yes | `Host/wwwroot/data/red/data` exists and contains active `ST8100-1` files | Phase 19 must handle missing data gracefully if absent on another checkout. [VERIFIED: filesystem audit] |

**Missing dependencies with no fallback:**
- None for research and Phase 18 planning. [VERIFIED: environment audit]

**Missing dependencies with fallback:**
- No project knowledge graph exists (`.planning/graphs/graph.json` absent); codebase grep and direct file reads were used instead. [VERIFIED: graph check]

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | xUnit `2.9.3` with Microsoft.NET.Test.Sdk `17.14.1`. [VERIFIED: Directory.Packages.props] |
| Config file | `Tests/Tests.csproj`; central versions in `Directory.Packages.props`. [VERIFIED: Tests/Tests.csproj] [VERIFIED: Directory.Packages.props] |
| Quick run command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Red|FullyQualifiedName~Ac15|FullyQualifiedName~YellowStartupAuth"` [VERIFIED: test infrastructure] |
| Full suite command | `dotnet test Tests/Tests.csproj` [VERIFIED: test infrastructure] |
| Build command | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` [VERIFIED: AGENTS.md] |

### Phase Requirements to Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|--------------|
| RFND-01 | Evidence artifact exists and records IDA route/root/version findings plus unresolved gaps. | review/static artifact | Planner should include review of `18-RED-EVIDENCE.md`; no source-text test recommended by project rules. [VERIFIED: AGENTS.md] | No; Wave 0 artifact task. |
| RFND-02 | Red generated wire compiles from `proto/red` and Red adapter project builds under Host. | build/compile | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` [VERIFIED: AGENTS.md] | No Red project yet. |
| RFND-02 | Red routes are enabled only when Red is configured. | focused integration/behavior | Prefer a minimal Host/controller smoke or application factory only if it verifies actual enabled-era behavior; avoid route-inventory reflection tests unless a runtime failure requires it. [VERIFIED: AGENTS.md] | No Red test yet. |
| RFND-02 | Shared `/v01r00` startup remains shared and Red HDD prefix maps to Red when enabled. | handler/controller behavior | Add focused `StartupAuthController`/`GetStartupMovieDataQuery` behavior test after Red catalog placeholder exists. [VERIFIED: existing YellowStartupAuthControllerTests.cs] | Existing Yellow pattern only. |
| RFND-03 | Existing supported-era behavior remains unchanged for touched shared code. | regression | Focused existing tests for touched shared code plus `dotnet test Tests/Tests.csproj`; always run temp Host build. [VERIFIED: AGENTS.md] [VERIFIED: .planning/config.json] | Existing suites present. |

### Sampling Rate

- **Per task commit:** run the narrow tests/build for touched surfaces, or at minimum `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` after scaffold changes. [VERIFIED: AGENTS.md]
- **Per wave merge:** run `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Red|FullyQualifiedName~Ac15|FullyQualifiedName~Blue|FullyQualifiedName~Green|FullyQualifiedName~Yellow"` when shared code changes; otherwise route-probe tests plus Host build. [VERIFIED: test infrastructure]
- **Phase gate:** `dotnet test Tests/Tests.csproj`, temp-output Host build, and user-run manual RPCS3/cabinet routing smoke. [VERIFIED: 18-CONTEXT.md] [VERIFIED: AGENTS.md]

### Wave 0 Gaps

- [ ] `Adapters.GameProtocol.Red/` - adapter project and generated wire do not exist yet. [VERIFIED: codebase grep]
- [ ] `Tests/Red/` - no Red-focused tests exist yet. [VERIFIED: test file inventory]
- [ ] Red evidence artifact - `18-RED-EVIDENCE.md` or equivalent does not exist yet. [VERIFIED: phase directory audit]
- [ ] Host Red config/gating - `GameEra.Red`, `ServerSettings:Eras:Red`, Red app-part removal, and `/v08r01` direct-protobuf fallback do not exist yet. [VERIFIED: codebase grep]

## Security Domain

Security enforcement is enabled by default because `.planning/config.json` has `workflow.security_enforcement: true`. [VERIFIED: .planning/config.json]

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|------------------|
| V2 Authentication | no for Phase 18 cabinet route probes | Do not add auth changes in Phase 18; preserve existing cabinet protocol exposure. [VERIFIED: phase scope] |
| V3 Session Management | no | No user sessions are introduced by Phase 18 route probes. [VERIFIED: phase scope] |
| V4 Access Control | yes for enabled-era route exposure | Host application-part gating must make Red routes available only when Red is enabled. [VERIFIED: Host/Program.cs] |
| V5 Input Validation | yes | Use protobuf-net model binding and generated DTO required-field serialization; do not parse binary request bodies manually. [VERIFIED: Host/Program.cs] [VERIFIED: proto/red/taiko.proto] |
| V6 Cryptography | no new crypto | Do not add Banacoin/payment crypto or wallet authority in Phase 18. [VERIFIED: 18-CONTEXT.md] |
| V8 Data Protection | low | Route probes log full request DTOs by design for smoke evidence; avoid adding secrets or payment state and keep no persistence. [VERIFIED: 18-CONTEXT.md] |
| V10 Malicious Code | yes for generated code handling | Generate Red wire from local dumped proto and repo-local `protogen`; do not fetch codegen packages or edit proto inputs. [VERIFIED: protogen temp generation] [VERIFIED: AGENTS.md] |

### Known Threat Patterns for This Stack

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| Disabled Red routes accidentally exposed | Elevation of privilege / information disclosure | Host application-part removal for disabled Red adapter; add Red only to enabled-era DI path. [VERIFIED: Host/Program.cs] |
| Probe endpoint mutates gameplay state | Tampering | Phase 18 controllers must not call gameplay Mediator handlers or EF. [VERIFIED: 18-CONTEXT.md] |
| Request body parsing drift | Tampering / denial of service | Use generated protobuf-net DTOs and direct-protobuf content type fallback; avoid manual binary parsing. [VERIFIED: Host/Program.cs] [VERIFIED: protogen temp generation] |
| Logs mistaken for payment authority | Repudiation / information disclosure | Banacoin-adjacent probes log and return compatibility only; no wallet/payment/coupon/transaction persistence. [VERIFIED: 18-CONTEXT.md] |

## Sources

### Primary (HIGH confidence)

- `AGENTS.md` - project architecture, testing, data, and generated-wire constraints.
- `.planning/phases/18-red-evidence-and-capability-foundation/18-CONTEXT.md` - locked Phase 18 decisions and deferred boundaries.
- `.planning/REQUIREMENTS.md` - RFND-01, RFND-02, RFND-03 and v1.3 out-of-scope boundaries.
- `.planning/ROADMAP.md` - Phase 18 deliverables and Red milestone sequencing.
- `.planning/STATE.md` - current v1.3 state and recent AC15 decisions.
- `.planning/config.json` - validation/security workflow settings.
- `.tools/red/EBOOT.ELF.i64` via `ida-cli` IDA-backed daemon - route prefixes, route suffixes, active root strings, addresses.
- `proto/red/taiko.proto` and `proto/red/vsinterface.proto` - dumped Red protocol source inputs.
- `Host/wwwroot/data/red/data` - local Red operator data tree and active-root file inventory.
- `Host/Program.cs`, `Host/Host.csproj`, `Host/Configurations/ServerSettings.json` - Host era gating, direct-protobuf fallback, project references, and data handling.
- `Application/Ac15/*`, `Application/Handlers/GetStartupMovieDataQuery.cs`, `Application/Settings/*` - AC15 profile boundary, HDD mapping, settings validation.
- `Adapters.GameProtocol.Blue`, `Adapters.GameProtocol.Green`, `Adapters.GameProtocol.Yellow`, `Adapters.GameProtocol.Shared` - adapter, route, generated wire, Mapperly, and controller precedents.
- `Directory.Packages.props`, `global.json`, `Directory.Build.props`, `Tests/Tests.csproj` - pinned stack and test infrastructure.

### Secondary (MEDIUM confidence)

- `.planning/research/STACK.md` - prior Red stack research, cross-checked against current Phase 18 context; its recommendation to add Red profile is superseded for Phase 18 by D-09. [VERIFIED: .planning/research/STACK.md] [VERIFIED: 18-CONTEXT.md]
- `docs/superpowers/specs/2026-06-07-ac15-core-extraction-design.md` - approved AC15 sharing direction.
- `docs/superpowers/specs/2026-06-11-ac15-capability-composition-design.md` - approved capability-composition correction.
- `.planning/milestones/v1.2-phases/16.1.../16.1-CONTEXT.md` - protogen nullable and Mapperly decisions.
- `.planning/milestones/v1.2-phases/16.2.../16.2-CONTEXT.md` - direct `ITaikoDbContext`, narrow row-shape, no repository boundary.
- `.planning/milestones/v1.2-phases/16.../16-CONTEXT.md` - Tokkun/Banacoin compatibility precedent.

### Tertiary (LOW confidence)

- No web-only or unverified community sources were used. [VERIFIED: research process]

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - package versions, SDK, and project shapes are pinned in local repo files and verified with commands. [VERIFIED: codebase grep] [VERIFIED: dotnet --version]
- Red route/root evidence: HIGH - IDA-backed daemon with `ida_available: true` returned concrete route/root strings and addresses. [VERIFIED: ida-cli IDA backend]
- Direct transport: MEDIUM - current Host direct-protobuf fallback and Red proto support the expectation, but manual Red runtime smoke has not yet confirmed content-type/framing. [VERIFIED: Host/Program.cs] [VERIFIED: proto/red/taiko.proto] [VERIFIED: 18-CONTEXT.md]
- Capability inventory: HIGH for proto/IDA/data presence and absence, MEDIUM for runtime sequence until smoke logs arrive. [VERIFIED: ida-cli IDA backend] [VERIFIED: proto/red/taiko.proto]
- Pitfalls: HIGH - pitfalls derive from locked Phase 18 boundaries and current project rules. [VERIFIED: 18-CONTEXT.md] [VERIFIED: AGENTS.md]

**Research date:** 2026-06-13
**Valid until:** 2026-07-13 for local code/proto/IDA evidence; revisit immediately if Red proto/data/IDB files change or manual runtime smoke contradicts route/transport findings.
