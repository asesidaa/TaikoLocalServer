# Phase 42: MOMOIRO Normal Playresult, Unlocks, Rewards, and Dan Compatibility - Research

**Researched:** 2026-06-26  
**Domain:** ASP.NET Core 10 AC15 Momoiro protobuf route handling, SQLite persistence, binary-backed playresult classification  
**Confidence:** MEDIUM - route and field presence are directly verified from repo and IDA evidence, while some per-field instruction-flow semantics remain intentionally conservative.

<user_constraints>

## User Constraints (from CONTEXT.md)

### Locked Decisions

#### Phase Boundary

Phase 42 turns the binary-proven MOMOIRO `playresult.php` route from a scaffold into evidence-backed normal-play mutation. It may persist only MOMOIRO-owned gameplay state that can be read back through the Phase 41 `userdata.php`, `selfbest.php`, and BAID/MyDon surfaces: scores, best rows, userdata-owned crowns, profile counters, recent/favorite lists, release-song flags, Don Point/reward counters, and Dan/Dani-compatible fields where MOMOIRO playresult/userdata/binary evidence proves the contract.

Out of scope: proto edits, new route families, `shoppingresult.php`, `bestscore.php`, `communicationlog.php`, `mainichisong.php`, standalone `crownsdata.php`, Taikojuku practice-folder behavior, Tokkun, Banacoin, battle, gacha, tournament, event folders, newer item-shop authority, Don Challenge, ChallengeCompe management, AdminApi/WebUI, and cabinet/RPCS3 acceptance.

#### Evidence and Scope

- Treat `proto/momoiro` as field evidence and `.tools/momoiro/EBOOT.ELF.i64` as route/native evidence; require both before adding stateful behavior beyond ordinary `playresult.php`.
- Use MOMOIRO binary/client research before planning song unlock, crown mutation, Don Point/reward mutation, Dan/Dani, and challenge-shaped field handling.
- If a field is present in proto but the route/runtime contract is not proven, accept/log/ignore it rather than creating new persistence authority.
- Keep all writes MOMOIRO-owned except shared identity rows already established by Phase 41.

#### Normal Play Mutation

- Reuse shared AC15 normal-play helpers where the MOMOIRO wire shape matches adjacent AC15 normal play, but add MOMOIRO entity/DbContext surfaces instead of writing adjacent-era tables.
- Normal playresult may update score history, self-best, crown source rows, release-song flags, recent rows, favorite rows, profile counters, and profile/reward counters only through MOMOIRO-owned tables and save fields.
- Favorite ordering remains the Phase 41 explicit `DisplayOrder` contract; playresult favorite mutation must preserve or derive display order without falling back to unproven raw song-number sorting.
- Tests must prove no writes to KIMIDORI, Murasaki, White, Red, Yellow, Blue, Green, Nijiiro, Tokkun, battle, Don Challenge, or unsupported feature tables.

#### Unlocks, Rewards, and Crowns

- Song unlock mutation writes MOMOIRO `ReleaseSongFlg` only if the playresult `release_song_no` contract is verified from generated wire and binary/client strings/control flow.
- Crowns continue to be represented through userdata-owned `hash_crown_flg` sourced from MOMOIRO best rows; do not add a dedicated crown route or crown table.
- Don Point and reward fields may update the MOMOIRO save row only within Phase 40/41 limits and only after research confirms the fields are normal playresult read/write fields.
- Do not implement live item-shop, wallet, payment, season, purchase, coupon, or shop authority from reward/Don Point fields.

#### Dan and Challenge-Shaped Data

- MOMOIRO Dan/Dani support is allowed only if `play_dan`, `dan_result`, BAID/userdata Dan fields, and binary/client evidence prove the normal Dan contract.
- Do not infer Taikojuku practice-folder behavior from Dan/Dani fields; no Taikojuku route is in MOMOIRO scope.
- Challenge-shaped arrays may be parsed, logged, ignored, or stored only according to MOMOIRO evidence; they do not create Don Challenge, ChallengeCompe, or reward-management semantics.
- If challenge semantics remain unproven, tests should assert non-creation of challenge state and no cross-feature side effects.

### the agent's Discretion

- Choose conservative internal names and task splits that match existing AC15 partial-file patterns.
- Add focused tests for observable state transitions and no-cross-era/no-cross-feature boundaries; do not add implementation-shape or generated-wire property tests.

### Deferred Ideas (OUT OF SCOPE)

- AdminApi/WebUI exposure belongs to Phase 43.
- Full automated plus cabinet/RPCS3 acceptance belongs to Phase 44.
- Later MOMOIRO versions and all special feature families listed in MOSPEC-01 remain future scope.

</user_constraints>

## Summary

MOMOIRO `playresult.php` is a real native route, not merely a proto artifact: IDA opened `.tools/momoiro/EBOOT.ELF.i64` with `require_ida=True`, found the `chassis/playresult.php` route string at `0x9e7030`, found route construction in `sub_17A5BC`, and found the PlayResult request store/send path in `sub_17A768` and `sub_60CEB4`. [VERIFIED: IDA AgentSession `.tools/momoiro/EBOOT.ELF.i64`]

The binary descriptors and repo wire files both expose the Phase 42 fields: `release_song_no`, `get_donpoint`, `reward_ptn`, `reward_progress`, `play_dan`, `dan_result`, `ary_challenge_id`, `hash_release_song_flg`, `hash_crown_flg`, BAID Dan fields, and Don Point readback fields. [VERIFIED: `proto/momoiro/taiko.proto`, `Adapters.GameProtocol.Momoiro/Wire/Game.cs`, IDA descriptor strings] Instruction-flow proof for every individual field assignment inside the large request-construction caller (`sub_121D74`, call to `sub_17A768` at `0x1238d8`) was too expensive for this research pass, so semantics beyond ordinary AC15 playresult handling must stay conservative. [VERIFIED: IDA AgentSession `.tools/momoiro/EBOOT.ELF.i64`]

**Primary recommendation:** Implement Momoiro normal playresult by wiring the existing AC15 playresult pipeline to Momoiro-owned tables and fields; persist release-song, crown-source, Don Point, reward, recent/favorite, counter, score, and self-best state, but treat challenge arrays as accepted non-stateful facts and limit Dan to playresult/BAID compatibility unless the implementation adds Momoiro-owned Dan tables plus a catalog Dan-order surface. [VERIFIED: `.planning/phases/42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil/42-CONTEXT.md`, `.planning/phases/42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil/42-PATTERNS.md`]

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|--------------|----------------|-----------|
| Momoiro `playresult.php` request intake | Adapter / Game Protocol | Application | Controller should deserialize direct protobuf, map to application DTOs, dispatch Mediator, and map response. [VERIFIED: `AGENTS.md`, `Adapters.GameProtocol.Kimidori/Controllers/PlayResultController.cs:8`] |
| Normal score/history mutation | Application | Database / Storage | Handler/service owns business rules; EF tables store Momoiro-owned rows. [VERIFIED: `Application/Ac15/Ac15NormalPlayWriter.cs`, `Infrastructure/Persistence/TaikoDbContext.Momoiro.cs:8`] |
| Self-best and crown source | Application | Database / Storage | Existing readback builds Momoiro crown bytes from best rows, so playresult should update best rows rather than add a crown route/table. [VERIFIED: `Application/Handlers/UserDataQuery.Momoiro.cs:31`, `Application/Handlers/UserDataQuery.Momoiro.cs:55`] |
| Release-song flags | Application | Database / Storage | Playresult field maps to Momoiro save bitset; readback already compacts release flags through userdata. [VERIFIED: `proto/momoiro/taiko.proto:228`, `Domain/Entities/UserSaveDataMomoiro.cs:27`] |
| Don Point and reward counters | Application | Database / Storage | `get_donpoint`, `reward_ptn`, and `reward_progress` mutate Momoiro save row only; no wallet/shop authority exists in scope. [VERIFIED: `proto/momoiro/taiko.proto:236`, `Domain/Entities/UserSaveDataMomoiro.cs:37`] |
| Dan compatibility | Application | Database / Storage | The route fields and BAID readback fields exist, but implementation must stay Momoiro-owned and must not imply Taikojuku route support. [VERIFIED: `proto/momoiro/taiko.proto:221`, `proto/momoiro/taiko.proto:262`, IDA route string list] |
| Challenge-shaped arrays | Adapter / Application | None by default | Descriptors exist, but no Momoiro challenge route/data semantics were verified; accept/map/log/ignore unless later evidence changes that. [VERIFIED: `proto/momoiro/taiko.proto:199`, IDA descriptor strings] |

<phase_requirements>

## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| MORUN-01 | Normal playresults persist scores, self-best, crowns, profile counters, recent songs, favorite songs, and related normal-play state only to Momoiro-owned tables. | Supported. Add Momoiro play-history persistence because `SongPlayDatumMomoiro` does not exist, reuse `SongBestDatumMomoiro` for best/crown source, preserve favorite `DisplayOrder`, and write only Momoiro DbSets. [VERIFIED: `Infrastructure/Persistence/TaikoDbContext.Momoiro.cs:8`, `Domain/Entities/MomoiroFavoriteSongs.cs:7`] |
| MORUN-02 | Song unlock, Don Point, and reward behavior mutates only evidence-backed Momoiro fields, with no shop/wallet/payment/season authority. | Supported for `release_song_no`, `get_donpoint`, `reward_ptn`, and `reward_progress`; unsupported shopping route strings were absent from IDA route list, so `TotalUseDonpoint` and shop/wallet state must not mutate. [VERIFIED: `proto/momoiro/taiko.proto:228`, IDA route string list] |
| MORUN-03 | Dan/Dani fields persisted/read back only where Momoiro playresult/userdata/binary/client evidence proves normal Dan; Taikojuku practice folder remains separate absent. | Partially supported. Field and BAID evidence supports normal playresult/BAID Dan compatibility; no `taikojuku.php` route string was found, and current Momoiro profile has `Dani = false`, so implementation needs an explicit bounded Dan decision. [VERIFIED: `Application/Ac15/Ac15EraProfiles.cs:35`, IDA descriptor strings] |
| MORUN-04 | Challenge-shaped arrays accepted/stored/echoed/omitted only by Momoiro evidence; no Don Challenge/ChallengeCompe/reward-management assumption. | Supported as accept/log/drop. Field descriptors exist, but no route/data semantics prove stateful challenge behavior. [VERIFIED: `proto/momoiro/taiko.proto:199`, IDA descriptor strings] |
| MORUN-05 | Runtime writes do not touch other eras or unsupported feature state. | Supported by adding Momoiro-only entities/DbSets and tests that count adjacent era tables before/after playresult handling. [VERIFIED: `AGENTS.md`, `.planning/phases/42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil/42-CONTEXT.md`] |

</phase_requirements>

## Project Constraints (from AGENTS.md)

- TaikoLocalServer is an ASP.NET Core 10 host with SQLite persistence, era-specific catalogs, and a Blazor WebAssembly admin UI. [VERIFIED: `AGENTS.md`]
- Blue, Yellow, Red, White, Green, Nijiiro, and Momoiro-style AC15 eras must keep gameplay state separate unless the state is shared identity. [VERIFIED: `AGENTS.md`]
- Use existing partial-file era behavior patterns: shared dispatcher in unsuffixed files, era implementations in `.Momoiro.cs`, `.Kimidori.cs`, `.Murasaki.cs`, etc. [VERIFIED: `AGENTS.md`]
- Controllers should deserialize, map, call Mediator, and map back; business behavior belongs in `Application/Handlers`. [VERIFIED: `AGENTS.md`]
- Generated protobuf DTOs should map through Application/Common shapes before handler logic; do not persist generated wire DTOs directly. [VERIFIED: `AGENTS.md`]
- Mapperly mappers must remain source-generator driven; inspect emitted generated source after building with `dotnet build /p:EmitCompilerGeneratedFiles=true`. [VERIFIED: `AGENTS.md`; CITED: https://mapperly.riok.app/docs/configuration/generated-source/]
- Use `IGameDataCatalog.For(GameEra)` and era catalog interfaces instead of hardcoded filesystem access from handlers. [VERIFIED: `AGENTS.md`]
- Keep generated `Wire/` files out of manual cleanup unless regenerating protocol output. [VERIFIED: `AGENTS.md`]
- Do not add tests to satisfy a checkbox; tests must protect evidence-backed cabinet behavior, runtime state transitions, parser/packing rules, AdminApi/WebUI workflows, or no-cross-era/no-cross-mode boundaries. [VERIFIED: `AGENTS.md`]
- Do not test generated wire type/property existence, route inventory, controller attribute lists, enum numeric values, static config key presence, migrations, private methods, or source text unless a demonstrated runtime failure requires it. [VERIFIED: `AGENTS.md`]

## Binary Research Findings

| Topic | Finding | Evidence | Confidence |
|-------|---------|----------|------------|
| IDA backend | `AgentSession(..., require_ida=True)` opened `.tools/momoiro/EBOOT.ELF.i64`; backend reported `idalib`, `database_opened=true`, `ida_available=true`. | IDA probe backend result. [VERIFIED: IDA AgentSession `.tools/momoiro/EBOOT.ELF.i64`] | HIGH for availability |
| Route string | Binary contains `chassis/playresult.php` at `0x9e7030`. | IDA string scan. [VERIFIED: IDA string `0x9e7030`] | HIGH |
| Route builder | `sub_17A5BC` formats `"%s/%s"` with `chassis/playresult.php`; caller `sub_1742DC` initializes route URLs. | IDA decompile/disasm. [VERIFIED: IDA functions `sub_17A5BC`, `sub_1742DC`] | HIGH for route construction |
| Request store | `sub_17A768` has the demangled-ish string `network::details::PlayResultManager::storeRequest(...)` and stores/copies the request before send. | IDA decompile; caller `sub_121D74` at `0x1238d8`. [VERIFIED: IDA functions `sub_17A768`, `sub_121D74`] | MEDIUM for store role |
| Request send | `sub_60CEB4` contains `network::details::PlayResultManager::operator()()` and an HTTP sendRequest call shape. | IDA decompile. [VERIFIED: IDA function `sub_60CEB4`] | MEDIUM for send role |
| Playresult field descriptors | Binary descriptors include `release_song_no`, `get_donpoint`, `reward_ptn`, `reward_progress`, `play_dan`, `dan_result`, and challenge arrays. | IDA descriptor strings around `0x9e9923` through `0x9e9f79`. [VERIFIED: IDA descriptor strings] | HIGH for field presence |
| Userdata/BAID readback descriptors | Binary descriptors include `hash_release_song_flg`, `hash_crown_flg`, challenge stat arrays, `reward_progress`, `total_get_donpoint`, `total_use_donpoint`, `disp_dan_type`, `got_dan_max`, and `got_dan_flg`. | IDA descriptor strings around `0x9e905a`, `0x9ea642`, `0x9ea673`, `0x9eaa19`. [VERIFIED: IDA descriptor strings] | HIGH for field presence |
| Unsupported route strings | `.php` route scan found startup/version/playresult/BAID/MyDon/userdata/recommend/selfbest/heartbeat/defaultsong/bookkeeping/songhash/telop routes, but not `shoppingresult.php`, `crownsdata.php`, `challengecompe.php`, `taikojuku.php`, `bestscore.php`, `communicationlog.php`, or `mainichisong.php`. | IDA route string scan. [VERIFIED: IDA route string list] | HIGH for absent string scan |
| Descriptor-only unsupported messages | Descriptors for `ShoppingResultRequest/Response`, `CommunicationLogRequest/Response`, `MainichisongRequest/Response`, and `BestScoreRequest/Response` exist despite absent route strings. | IDA descriptor strings and route string scan. [VERIFIED: IDA descriptor strings] | HIGH for descriptor presence, LOW for semantics |
| Dani assets | Binary strings include `AssignDani`, `SetDaniStatus`, `NotifyDaniSelect`, `dani_gameover.lm`, `dani_result.lm`, `dani_select.lm`, and `dani_enso.lm`. | IDA string scan. [VERIFIED: IDA strings] | MEDIUM for client UI support |

**Instruction-flow boundary:** This research did not fully reverse the huge request-construction function that leads to `sub_17A768`; therefore field roles are verified from route existence, generated/proto schema, and binary descriptors, not from a complete per-field data-flow proof. [VERIFIED: IDA function `sub_121D74`; ASSUMED where field semantics follow adjacent AC15 server patterns]

## Standard Stack

### Core

| Library / Tool | Version | Purpose | Why Standard |
|----------------|---------|---------|--------------|
| .NET SDK / ASP.NET Core | SDK `10.0.201` installed; project targets ASP.NET Core 10 patterns. | Host and adapters. | Existing repo stack and `Host` composition. [VERIFIED: `dotnet --version`, `AGENTS.md`] |
| Microsoft.EntityFrameworkCore / Sqlite | `10.0.7` | SQLite persistence and migrations. | Existing central package version and Momoiro DbContext partial. [VERIFIED: `Directory.Packages.props:17`, `Directory.Packages.props:19`] |
| Mediator.SourceGenerator / Abstractions | `3.0.2` | Request dispatch from controllers to handlers. | Existing `UpdateAc15PlayResultCommand` pipeline. [VERIFIED: `Directory.Packages.props:35`, `Application/Handlers/UpdatePlayResultCommand.cs:15`] |
| protobuf-net / protobuf-net.AspNetCore | `3.2.56` / `3.2.52` | Direct protobuf request/response models. | Existing game protocol adapter stack. [VERIFIED: `Directory.Packages.props:37`, `Directory.Packages.props:38`] |
| Riok.Mapperly | `4.3.1` | Source-generated mapper implementation. | Existing adapter/application mapper standard; official docs show the stable docs page is for 4.3.1. [VERIFIED: `Directory.Packages.props:39`; CITED: https://mapperly.riok.app/docs/configuration/mapper/] |
| xUnit | `2.9.3` | Regression tests. | Existing `Tests` project framework. [VERIFIED: `Directory.Packages.props:26`, `Tests/Tests.csproj:11`] |
| IDA-CLI AgentSession | local skill | Required binary route/descriptor research. | User explicitly required `.tools\momoiro\EBOOT.ELF.i64` with `require_ida=True`. [VERIFIED: IDA AgentSession `.tools/momoiro/EBOOT.ELF.i64`] |

### Supporting

| Library / Tool | Version | Purpose | When to Use |
|----------------|---------|---------|-------------|
| Mapperly generated-source emission | Mapperly `4.3.1` | Inspect generated mapper `.g.cs` output. | Run after adding Momoiro playresult mappers or changing mapper helpers. [CITED: https://mapperly.riok.app/docs/configuration/generated-source/] |
| EF migrations CLI | via .NET SDK | Add Momoiro-owned playresult/Dan tables. | Run after Domain/DbContext changes; migration should add only Momoiro tables/columns. [VERIFIED: `AGENTS.md`] |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Existing AC15 playresult helpers | New Momoiro-only mutation service | Not recommended; duplicate crown/best/recent/release logic risks diverging from adjacent AC15 behavior. [VERIFIED: `Application/Ac15/Ac15NormalPlayWriter.cs`] |
| Mapperly mappers | Hand-written mapper bodies | Not allowed by AGENTS.md except configured helper conversions; generated source must remain source-generator driven. [VERIFIED: `AGENTS.md`] |
| Stateful challenge tables | Accept/log/drop challenge arrays | Use accept/log/drop because descriptors exist but challenge semantics are unproven. [VERIFIED: IDA descriptor strings; ASSUMED semantics] |

**Installation:** No new external packages are recommended for Phase 42. [VERIFIED: `Directory.Packages.props`, phase context]

## Package Legitimacy Audit

No external package installation is recommended. The phase should use the repo's existing central package versions. [VERIFIED: `Directory.Packages.props`]

## Architecture Patterns

### System Architecture Diagram

```text
Momoiro cabinet
  -> POST /v04r00/chassis/playresult.php (direct protobuf)
  -> Adapters.GameProtocol.Momoiro PlayResultController
  -> PlayResultMappers.Map(PlayResultRequest)
  -> UpdateAc15PlayResultCommand(GameEra.Momoiro)
  -> HandleMomoiro
       -> validate shared UserDatum / Momoiro save row
       -> classify normal vs Dan by play_mode + play_dan/dan_result evidence
       -> apply evidence-backed profile/release/reward mutation
       -> Ac15NormalPlayWriter for scores, best/crown, recent, favorite
       -> optional Ac15DaniWriter only for bounded Momoiro Dan compatibility
       -> ignore/log challenge arrays unless new evidence proves state
  -> Momoiro-owned EF tables
  -> Phase 41 readback through userdata.php, selfbest.php, BAID/MyDon
```

### Recommended Project Structure

```text
Adapters.GameProtocol.Momoiro/
├── Controllers/PlayResultController.cs       # replace scaffold with map/send/map response
└── Mappers/PlayResultMappers.cs              # Momoiro wire -> AC15 application DTOs

Application/
├── Handlers/UpdatePlayResultCommand.cs       # add GameEra.Momoiro dispatch arm
├── Handlers/UpdatePlayResultCommand.Momoiro.cs
└── Ac15/                                     # add Momoiro entries to shared helpers

Domain/Entities/
├── SongPlayDatumMomoiro.cs                   # required normal score history
├── DanScoreDatumMomoiro.cs                   # conditional Dan compatibility
└── DanStageScoreDatumMomoiro.cs              # conditional Dan compatibility

Infrastructure/Persistence/
├── TaikoDbContext.Momoiro.cs                 # add Momoiro-owned DbSets/config
└── Migrations/*AddMomoiroPlayResultState.cs  # Momoiro-only schema migration

Tests/Momoiro/
├── MomoiroPlayResultHandlerTests.cs
├── MomoiroPlayResultControllerTests.cs
└── MomoiroHandlerFixture.cs                  # extend table setup/helpers
```

### Pattern 1: Direct Protobuf Controller

**What:** Controller logs/maps a direct protobuf request, dispatches `UpdateAc15PlayResultCommand`, and maps the numeric result to `PlayResultResponse`. [VERIFIED: `Adapters.GameProtocol.Kimidori/Controllers/PlayResultController.cs:8`]

**When to use:** Use for Momoiro `playresult.php`; do not place mutation logic in the controller. [VERIFIED: `AGENTS.md`]

```csharp
var playResult = PlayResultMappers.Map(request);
var result = await Mediator.Send(
    new UpdateAc15PlayResultCommand(request.Baid, GameEra.Momoiro, playResult),
    HttpContext.RequestAborted);
return Ok(PlayResultMappers.Map(result));
```

### Pattern 2: Momoiro-Owned Normal Mutation

**What:** Reuse `Ac15NormalPlayWriter` with Momoiro play/best/favorite/recent tables and Momoiro mapper methods. [VERIFIED: `.planning/phases/42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil/42-PATTERNS.md`]

**When to use:** Use after request mapping and normal-stage filtering; do not write adjacent-era tables. [VERIFIED: `AGENTS.md`]

### Pattern 3: Conservative Reward and Unlock Mutation

**What:** Use `Ac15CommonProfileMutation.TryApplyDonPoints` with Momoiro `Ac15UnlockFlagAccess` and `Ac15ProfileCounterUpdater` additions. [VERIFIED: `.planning/phases/42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil/42-PATTERNS.md`]

**When to use:** Use for `release_song_no`, `get_donpoint`, `reward_ptn`, and `reward_progress`; do not mutate `TotalUseDonpoint` or shop state. [VERIFIED: `proto/momoiro/taiko.proto:228`, `proto/momoiro/taiko.proto:236`]

### Pattern 4: Bounded Dan Compatibility

**What:** If implemented, add Momoiro-owned Dan score/stage tables and `Ac15DaniMapper` methods, expose Momoiro medley/Dan order from catalog, and save only playresult Dan facts. [VERIFIED: `Application/Ac15/Ac15DaniWriter.cs`, `Infrastructure/GameDataCatalog/Momoiro/MomoiroEraGameDataCatalog.cs:20`]

**When to use:** Use only for `play_mode == DanMode` with exactly one valid `play_dan` and a known Momoiro challenge row; keep Taikojuku practice routes absent. [VERIFIED: `proto/momoiro/taiko.proto:221`, IDA route string list]

### Anti-Patterns to Avoid

- **Creating `shoppingresult.php` or shop/wallet authority:** Proto descriptors exist, but the native route string was absent and the phase excludes shop authority. [VERIFIED: IDA route string list; `.planning/.../42-CONTEXT.md`]
- **Creating a crown route/table:** `hash_crown_flg` is userdata readback and current code builds it from best rows. [VERIFIED: `Adapters.GameProtocol.Momoiro/Wire/Game.cs:1827`, `Application/Handlers/UserDataQuery.Momoiro.cs:31`]
- **Sorting favorites by raw song number:** Phase 41 readback orders by `DisplayOrder`, then `SongNo` only as tie-breaker. [VERIFIED: `Application/Handlers/UserDataQuery.Momoiro.cs:18`, `Application/Handlers/UserDataQuery.Momoiro.cs:20`]
- **Inferring ChallengeCompe/Don Challenge from `ary_challenge_id`:** Descriptors prove field presence, not stateful semantics. [VERIFIED: `proto/momoiro/taiko.proto:199`; ASSUMED semantics]
- **Flipping broad Momoiro feature flags casually:** Current profile explicitly has `Dani = false`; any change must be tied to playresult/BAID support and not route exposure. [VERIFIED: `Application/Ac15/Ac15EraProfiles.cs:35`, `Application/Ac15/Ac15EraProfiles.cs:38`]

## Direct Answers for Field Roles

| Field / Surface | Classification | Recommended Handling |
|-----------------|----------------|----------------------|
| `release_song_no` | PlayResultRequest top-level repeated field; binary descriptor present; readback surface is `hash_release_song_flg`. [VERIFIED: `proto/momoiro/taiko.proto:228`, IDA descriptor strings] | Set bits in Momoiro `ReleaseSongFlg` for in-range song ids through a new `Ac15UnlockFlagAccess.Momoiro`; ignore out-of-range ids. [VERIFIED: `Domain/Entities/UserSaveDataMomoiro.cs:27`] |
| `hash_crown_flg` / crown source | UserDataResponse field; no standalone crown route string; existing readback builds crown body from Momoiro best rows. [VERIFIED: `Adapters.GameProtocol.Momoiro/Wire/Game.cs:1827`, `Application/Handlers/UserDataQuery.Momoiro.cs:31`] | Update `SongBestDatumMomoiro.BestCrown` through normal play best-row logic; do not add crown table or route. [VERIFIED: `Domain/Entities/SongBestDatumMomoiro.cs:5`] |
| `get_donpoint` | PlayResultRequest top-level optional field; userdata readback has `total_get_donpoint`. [VERIFIED: `proto/momoiro/taiko.proto:236`, `Adapters.GameProtocol.Momoiro/Wire/Game.cs:2112`] | Increment `UserSaveDataMomoiro.TotalGetDonpoint`; do not alter `TotalUseDonpoint` without shopping evidence. [VERIFIED: `Domain/Entities/UserSaveDataMomoiro.cs:37`] |
| `reward_ptn` | BAID/MyDon/playresult field; Momoiro save has `RewardPtn`; BAID reads it. [VERIFIED: `proto/momoiro/taiko.proto:45`, `proto/momoiro/taiko.proto:237`, `Application/Handlers/BaidQuery.Momoiro.cs:72`] | Store incoming pattern on Momoiro save row when present; do not infer season/shop rewards. [VERIFIED: `Domain/Entities/UserSaveDataMomoiro.cs:39`] |
| `reward_progress` | Playresult/userdata field; Momoiro save has `RewardProgress`. [VERIFIED: `proto/momoiro/taiko.proto:238`, `Application/Handlers/UserDataQuery.Momoiro.cs:57`] | Store incoming progress on Momoiro save row when present; expose through existing userdata readback. [VERIFIED: `Domain/Entities/UserSaveDataMomoiro.cs:40`] |
| `play_dan` | StageData required field; binary descriptor and Dani UI strings present. [VERIFIED: `proto/momoiro/taiko.proto:221`, IDA strings] | Use only in bounded DanMode handling with exactly one valid Dan id and Momoiro-owned Dan persistence; otherwise parse/log/ignore. [VERIFIED: `Application/Ac15/Ac15DaniWriter.cs`; ASSUMED for Momoiro semantics] |
| `dan_result` | PlayResultRequest optional field; binary descriptor present. [VERIFIED: `proto/momoiro/taiko.proto:262`, IDA descriptor strings] | Use with `play_dan` for bounded Dan score/state if Dan implementation is added; do not create Taikojuku practice behavior. [VERIFIED: IDA route string list; ASSUMED for Momoiro semantics] |
| BAID Dan fields | `disp_dan_type`, `got_dan_max`, `got_dan_flg` are proto/wire fields and existing BAID readback surfaces. [VERIFIED: `proto/momoiro/taiko.proto:47`, `Application/Handlers/BaidQuery.Momoiro.cs:68`] | If Dan is implemented, ensure save update sets display/type and flags consistently enough that BAID readback reflects the result. [VERIFIED: `Domain/Entities/UserSaveDataMomoiro.cs:32`; ASSUMED display policy] |
| `ary_challenge_id`, `ary_user_compe_id`, `ary_bng_compe_id` | StageData field descriptors and userdata stat arrays exist; route/data semantics not proven. [VERIFIED: `proto/momoiro/taiko.proto:199`, IDA descriptor strings] | Accept/map/log/drop by default; add no ChallengeCompe/Don Challenge/reward-management persistence. [VERIFIED: `.planning/.../42-CONTEXT.md`; ASSUMED semantics] |

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Protobuf body parsing | Custom byte parser | Existing generated Momoiro wire models and protobuf-net ASP.NET binding | Existing route adapters already use direct protobuf models. [VERIFIED: `Adapters.GameProtocol.Momoiro/Wire/Game.cs`] |
| Normal play score/best/crown logic | New per-field SQL writes | `Ac15NormalPlayWriter` plus Momoiro mappings/tables | Shared helper already handles score rows, best rows, crown mapping, recent/favorite basics. [VERIFIED: `Application/Ac15/Ac15NormalPlayWriter.cs`] |
| Release/crown bit packing | Custom bit packing | `Ac15ProtocolBytes`, `Ac15SongHashCodec`, `Ac15UnlockFlagAccess` | Existing readback already depends on these helpers. [VERIFIED: `Application/Ac15/Ac15SongHashCodec.cs`, `.planning/.../42-PATTERNS.md`] |
| Mapper implementations | Hand-written mapper bodies | Mapperly partial mappers plus helper conversions | AGENTS.md requires source-generator-driven Mapperly mappers. [VERIFIED: `AGENTS.md`; CITED: https://mapperly.riok.app/docs/configuration/generated-source/] |
| Dan score persistence | Ad hoc save fields only | `Ac15DaniWriter` pattern with Momoiro-owned Dan tables if implemented | Adjacent eras already have score/stage separation and flag updates. [VERIFIED: `Application/Ac15/Ac15DaniWriter.cs`] |
| Challenge semantics | Don Challenge/ChallengeCompe clone | Accept/log/drop until Momoiro evidence proves semantics | Phase context explicitly excludes those feature families. [VERIFIED: `.planning/.../42-CONTEXT.md`] |

**Key insight:** The binary proves Momoiro sends a playresult request with these fields; it does not prove every proto descriptor is a server-authoritative persistence contract. [VERIFIED: IDA route/descriptor research; ASSUMED semantic boundary]

## Common Pitfalls

### Pitfall 1: Treating Descriptor Presence as Route Support
**What goes wrong:** Implementing `shoppingresult.php`, `bestscore.php`, or challenge management because descriptor names exist. [VERIFIED: IDA descriptor strings]  
**Why it happens:** Momoiro binary contains descriptor strings for messages that are not present in the `.php` route string list. [VERIFIED: IDA route string list]  
**How to avoid:** Require route-string/control-flow evidence before adding route families; descriptor-only messages stay out of scope. [VERIFIED: `.planning/.../42-CONTEXT.md`]  
**Warning signs:** New controllers or migrations for shop/challenge features in Phase 42. [ASSUMED]

### Pitfall 2: Losing Momoiro Favorite Order
**What goes wrong:** Shared favorite add path writes `DisplayOrder = 0`, causing Phase 41 readback to fall back to raw song sorting for ties. [VERIFIED: `.planning/.../42-PATTERNS.md`, `Domain/Entities/MomoiroFavoriteSongs.cs:7`]  
**Why it happens:** Adjacent AC15 favorite tables do not all carry the explicit Momoiro `DisplayOrder` contract. [VERIFIED: `.planning/.../42-PATTERNS.md`]  
**How to avoid:** Preserve existing order and assign new favorites after current max order, or gate favorite mutation if ordering cannot be derived. [ASSUMED]  
**Warning signs:** Multiple playresult-added favorites with `DisplayOrder = 0`. [ASSUMED]

### Pitfall 3: Dan Feature Flag Conflation
**What goes wrong:** Flipping `Ac15EraProfiles.Momoiro.Features.Dani` and accidentally implying route/readback behavior beyond playresult/BAID compatibility. [VERIFIED: `Application/Ac15/Ac15EraProfiles.cs:35`]  
**Why it happens:** The current feature flag may mean broad Dani route capability, while Phase 42 only proves fields/assets/playresult compatibility. [ASSUMED]  
**How to avoid:** Keep Taikojuku false; if Dan is implemented, limit it to playresult/BAID fields and document whether the `Dani` flag is route exposure or save capability. [VERIFIED: IDA route string list; ASSUMED flag interpretation]  
**Warning signs:** New `taikojuku.php` route or practice-folder behavior in Phase 42. [VERIFIED: `.planning/.../42-CONTEXT.md`]

### Pitfall 4: Cross-Era Writes Through Shared Helpers
**What goes wrong:** Handler reuses a Kimidori/Murasaki table accessor or mapper and writes adjacent-era rows. [ASSUMED]  
**Why it happens:** Existing helpers are generic and require correct era-specific `DbSet` and mapper functions. [VERIFIED: `.planning/.../42-PATTERNS.md`]  
**How to avoid:** Add explicit Momoiro mapper/accessor/table methods and tests that count adjacent tables. [VERIFIED: `.planning/.../42-CONTEXT.md`]  
**Warning signs:** Handler references `Kimidori`, `Murasaki`, or non-Momoiro DbSets in write paths. [ASSUMED]

## Code Examples

### Momoiro Dispatch Shape

```csharp
// Source pattern: Application/Handlers/UpdatePlayResultCommand.cs
public ValueTask<uint> Handle(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken)
    => request.Era switch
    {
        GameEra.Momoiro => HandleMomoiro(request, cancellationToken),
        _ => existingArms
    };
```

### Evidence-Backed Release/Reward Save Shape

```csharp
// Source pattern: Application/Ac15/Ac15CommonProfileMutation.cs and Ac15UnlockFlagAccess.cs
Ac15CommonProfileMutation.TryApplyDonPoints(
    saveData,
    playResultData.Profile,
    Ac15EraProfiles.Momoiro.Limits,
    Ac15ProfileCounterUpdater.Momoiro,
    Ac15UnlockFlagAccess.Momoiro,
    ids => Ac15UnlockFlagAccess.Momoiro.ReleaseSongs?.Invoke(saveData, ids));
```

### Conservative Challenge Handling

```csharp
// Source: Phase 42 context and binary descriptor boundary
if (stage.ChallengeIds.Count != 0 || stage.UserCompeIds.Count != 0 || stage.BngCompeIds.Count != 0)
{
    logger.LogInformation("Momoiro playresult challenge-shaped fields received for Baid={Baid}; stateful semantics not implemented", baid);
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Scaffold `playresult.php` success response | Evidence-backed direct-protobuf controller + AC15 application pipeline | Phase 42 planning target | Replace scaffold while keeping business logic in Application. [VERIFIED: `Adapters.GameProtocol.Momoiro/Controllers/PlayResultController.cs:10`] |
| Proto-only field interpretation | Require proto/wire plus binary route/native evidence | Phase 42 context | Fields can be parsed without becoming persistence authority. [VERIFIED: `.planning/.../42-CONTEXT.md`] |
| Crown table/route assumption | Userdata `hash_crown_flg` from best rows | Phase 41 readback | Implement crown mutation by updating best rows only. [VERIFIED: `Application/Handlers/UserDataQuery.Momoiro.cs:31`] |
| Generated mapper trust by declaration | Build and inspect Mapperly generated source | Repo rule and official Mapperly docs | Mapper changes need emitted `.g.cs` inspection after build. [VERIFIED: `AGENTS.md`; CITED: https://mapperly.riok.app/docs/configuration/generated-source/] |

**Deprecated/outdated:** Treating adjacent Red/White Don Challenge or Yellow shop-season logic as Momoiro evidence is out of scope for Phase 42. [VERIFIED: `.planning/.../42-CONTEXT.md`]

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | Adjacent AC15 `release_song_no` semantics apply to Momoiro as an in-range song-id bitset mutation. | Direct Answers, Architecture Patterns | Wrong unlock bits or over-persistence; mitigate with bounds checks and readback tests. |
| A2 | `get_donpoint` should increment total earned points, while `TotalUseDonpoint` should remain unchanged without shopping route evidence. | Direct Answers | Don Point totals could diverge from cabinet expectation; mitigate by not inventing spend authority. |
| A3 | Dan descriptors/assets plus BAID fields are sufficient for bounded playresult/BAID Dan compatibility, but not Taikojuku routes. | Direct Answers, Pitfalls | Dan may be under-implemented or feature flag semantics may be wrong; mitigate with explicit planner checkpoint. |
| A4 | Challenge arrays should not persist state in Phase 42. | Direct Answers, Common Pitfalls | If cabinet expects challenge stat readback, state will be missing; mitigate by logging fields and leaving open question for runtime evidence. |
| A5 | New favorite ordering can be derived by appending after existing `DisplayOrder` max. | Common Pitfalls | Cabinet may expect a different favorite insertion order; mitigate with tests around Phase 41 readback contract and no raw song sorting. |

## Open Questions

1. **Should Momoiro Dan compatibility be implemented in Phase 42 or gated behind a human checkpoint?**
   - What we know: `play_dan`, `dan_result`, BAID Dan fields, and Dani UI/asset strings are present. [VERIFIED: `proto/momoiro/taiko.proto:221`, `proto/momoiro/taiko.proto:262`, IDA strings]
   - What's unclear: Complete instruction-flow proof for Dan field construction and whether `Ac15EraProfiles.Momoiro.Features.Dani` should mean route exposure, save capability, or both. [ASSUMED]
   - Recommendation: Implement only if the plan adds Momoiro-owned Dan tables and catalog Dan-order read surface; keep Taikojuku routes absent. [ASSUMED]

2. **How should `DispDanType` be initialized when Momoiro Dan is enabled?**
   - What we know: Existing BAID readback emits display type `0` when `saveData.DispDanType == 0`; current Momoiro save default is zero. [VERIFIED: `Application/Handlers/BaidQuery.Momoiro.cs:68`, `Domain/Entities/UserSaveDataMomoiro.cs:32`]
   - What's unclear: Whether Momoiro expects display type to become `1` after first Dan result or to use a richer type value. [ASSUMED]
   - Recommendation: If Dan is implemented, set a conservative nonzero display type only after a validated Dan result and cover BAID readback in tests. [ASSUMED]

3. **Should challenge arrays be raw-stored for future proofing?**
   - What we know: Descriptors exist; stateful challenge semantics and route families are unproven. [VERIFIED: `proto/momoiro/taiko.proto:199`, IDA route string list]
   - What's unclear: Whether cabinet later consumes userdata challenge stat arrays for Momoiro. [ASSUMED]
   - Recommendation: Do not persist challenge state in Phase 42; log received values if useful for future captures. [ASSUMED]

4. **Should Momoiro favorites mutate from playresult at all if ordering is ambiguous?**
   - What we know: `is_favorite` exists on stage data, and Phase 41 readback uses `DisplayOrder`. [VERIFIED: `proto/momoiro/taiko.proto:216`, `Application/Handlers/UserDataQuery.Momoiro.cs:20`]
   - What's unclear: Exact client order update semantics when a played song is marked favorite. [ASSUMED]
   - Recommendation: Preserve existing order and append newly added favorites after max `DisplayOrder`; never sort by raw song number. [ASSUMED]

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|-------------|-----------|---------|----------|
| .NET SDK | Build/test/migrations | Yes | `10.0.201` | None needed. [VERIFIED: `dotnet --version`] |
| Python | IDA-CLI local tooling | Yes | `3.13.2` | None needed. [VERIFIED: `python --version`] |
| Node.js | GSD tools seam | Yes | `v24.12.0` | None needed. [VERIFIED: `node --version`] |
| IDA database | Binary research | Yes | `.tools/momoiro/EBOOT.ELF.i64` opened by `idalib` | If unavailable, block binary-backed planning. [VERIFIED: IDA AgentSession] |
| Cabinet/RPCS3 | Runtime acceptance | Not required | Not probed | Phase 44 / user-provided evidence. [VERIFIED: `.planning/.../42-CONTEXT.md`] |

**Missing dependencies with no fallback:** None for research and planning. [VERIFIED: environment probes]

**Missing dependencies with fallback:** Cabinet/RPCS3 acceptance is out of Phase 42 scope. [VERIFIED: `.planning/.../42-CONTEXT.md`]

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | xUnit `2.9.3` on .NET SDK `10.0.201`. [VERIFIED: `Directory.Packages.props:26`, `dotnet --version`] |
| Config file | `Tests/Tests.csproj`. [VERIFIED: `Tests/Tests.csproj:11`] |
| Quick run command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResult|FullyQualifiedName~MomoiroReadback|FullyQualifiedName~Ac15NormalPlayWriter|FullyQualifiedName~Ac15Dani" --no-restore` |
| Full suite command | `dotnet test Tests/Tests.csproj --no-restore` |
| Mapper verification command | `dotnet build TaikoLocalServer.slnx /p:EmitCompilerGeneratedFiles=true` then inspect `obj/.../generated/.../Riok.Mapperly/*.g.cs`. [CITED: https://mapperly.riok.app/docs/configuration/generated-source/] |

### Phase Requirements To Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|--------------|
| MORUN-01 | Normal playresult writes Momoiro play rows, best rows, crown source, counters, recents, favorites, and no adjacent era rows. | integration/unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResultHandler"` | No - Wave 0 add `Tests/Momoiro/MomoiroPlayResultHandlerTests.cs`. [VERIFIED: `rg --files Tests/Momoiro`] |
| MORUN-02 | Release, Don Point, and reward fields update only Momoiro save fields; no shop/spend state. | integration/unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResultHandler"` | No - Wave 0 add coverage. [VERIFIED: `Tests/Momoiro/MomoiroReadbackHandlerTests.cs`] |
| MORUN-03 | Dan fields save/read back only if implemented; no Taikojuku route/state. | integration/unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResultHandler|FullyQualifiedName~Ac15Dani"` | No - conditional Wave 0 add coverage. [VERIFIED: `Tests/Ac15/Ac15DaniCapabilityTests.cs`] |
| MORUN-04 | Challenge arrays do not create Don Challenge/ChallengeCompe/reward-management state. | unit/integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResultHandler"` | No - Wave 0 add negative assertions. [VERIFIED: `.planning/.../42-CONTEXT.md`] |
| MORUN-05 | Runtime writes are Momoiro-owned only. | integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResultHandler"` | No - Wave 0 add cross-era row count assertions. [VERIFIED: `.planning/.../42-CONTEXT.md`] |

### Sampling Rate

- **Per task commit:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResult|FullyQualifiedName~MomoiroReadback|FullyQualifiedName~Ac15NormalPlayWriter|FullyQualifiedName~Ac15Dani" --no-restore`
- **Per wave merge:** `dotnet test Tests/Tests.csproj --no-restore`
- **Phase gate:** Full suite green plus Mapperly generated output inspected for new Momoiro mappers. [VERIFIED: `AGENTS.md`; CITED: https://mapperly.riok.app/docs/configuration/generated-source/]

### Wave 0 Gaps

- [ ] `Tests/Momoiro/MomoiroPlayResultHandlerTests.cs` - covers MORUN-01..05. [VERIFIED: `rg --files Tests/Momoiro`]
- [ ] `Tests/Momoiro/MomoiroPlayResultControllerTests.cs` - covers direct protobuf controller mapping without route inventory/source-text assertions. [VERIFIED: `AGENTS.md`]
- [ ] `Tests/Momoiro/MomoiroHandlerFixture.cs` - extend setup for `SongPlayDatum_Momoiro` and optional Dan tables. [VERIFIED: `.planning/.../42-PATTERNS.md`]
- [ ] Conditional Dan test additions if Dan persistence is implemented. [VERIFIED: `Tests/Ac15/Ac15DaniCapabilityTests.cs`; ASSUMED Momoiro decision]

## Security Domain

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|------------------|
| V2 Authentication | No for this phase | Cabinet route auth is outside Phase 42; use existing route hosting/auth posture. [VERIFIED: `.planning/.../42-CONTEXT.md`] |
| V3 Session Management | No | No user web session changes. [VERIFIED: `.planning/.../42-CONTEXT.md`] |
| V4 Access Control | Yes | Enforce era-owned writes and shared identity-only access. [VERIFIED: `AGENTS.md`] |
| V5 Input Validation | Yes | Validate Baid/user existence, stage bounds, song ids within limits, Dan ids against catalog, and ignore unsupported arrays/routes. [VERIFIED: `.planning/.../42-CONTEXT.md`; ASSUMED exact validation placement] |
| V6 Cryptography | No | No new cryptography; do not invent token/payment/wallet behavior. [VERIFIED: `.planning/.../42-CONTEXT.md`] |

### Known Threat Patterns for Momoiro Playresult

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| Cross-era state tampering by handler bug | Tampering | Use Momoiro-specific DbSets/mappers and negative tests over adjacent-era row counts. [VERIFIED: `AGENTS.md`] |
| Overlarge/out-of-range release or stage arrays | Denial of Service / Tampering | Bound by `Ac15EraProfiles.Momoiro.Limits`, ignore out-of-range ids, and avoid unbounded state creation. [VERIFIED: `Application/Ac15/Ac15EraProfiles.cs:179`] |
| Descriptor-only route abuse | Tampering | Do not expose unsupported route families absent in binary route strings. [VERIFIED: IDA route string list] |
| Reward/shop privilege escalation | Elevation of Privilege | Update only earned total and reward display/progress fields; do not create wallet, spend, payment, coupon, or season state. [VERIFIED: `.planning/.../42-CONTEXT.md`] |
| Mapper null/default surprises | Tampering / Integrity | Keep Mapperly null handling explicit and inspect generated mapper output after build. [CITED: https://mapperly.riok.app/docs/configuration/mapper/#null-values] |

## Sources

### Primary (HIGH confidence)

- `.tools/momoiro/EBOOT.ELF.i64` via IDA-CLI AgentSession - backend probe, route strings, descriptor strings, `sub_17A5BC`, `sub_17A768`, `sub_60CEB4`, `sub_121D74`. [VERIFIED: IDA AgentSession]
- `proto/momoiro/taiko.proto` - Momoiro protobuf field definitions. [VERIFIED: local repo]
- `Adapters.GameProtocol.Momoiro/Wire/Game.cs` - generated Momoiro wire fields. [VERIFIED: local repo]
- `.planning/phases/42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil/42-CONTEXT.md` - phase constraints. [VERIFIED: local repo]
- `.planning/phases/42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil/42-PATTERNS.md` - local pattern map. [VERIFIED: local repo]
- `AGENTS.md` - project rules. [VERIFIED: local repo]

### Secondary (MEDIUM confidence)

- Mapperly official docs for null behavior and generated-source emission. [CITED: https://mapperly.riok.app/docs/configuration/mapper/#null-values] [CITED: https://mapperly.riok.app/docs/configuration/generated-source/]

### Tertiary (LOW confidence)

- GSD research seam classified local/IDA providers conservatively as LOW even when direct evidence was inspected; semantic inferences in the Assumptions Log remain LOW until implementation/runtime evidence confirms them. [VERIFIED: `node .codex\gsd-core\bin\gsd-tools.cjs query classify-confidence --provider ida-cli --verified`]

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - versions and project references verified from central package props, csproj files, and installed SDK probes.
- Architecture: HIGH - follows AGENTS.md and phase-local pattern map.
- Binary route existence: HIGH - route string, route builder, manager store/send path verified in IDA.
- Field presence: HIGH - proto, generated wire, and binary descriptors agree.
- Field semantics: MEDIUM - release/reward/crown roles align with readback surfaces, but full per-field instruction flow was not traced.
- Dan compatibility: MEDIUM/LOW - fields and assets are verified, but broad feature flag and catalog-order semantics need explicit implementation choices.
- Challenge semantics: LOW - descriptors exist; stateful meaning is unverified and should remain non-stateful.

**Research date:** 2026-06-26  
**Valid until:** 2026-07-26 for repo-local planning evidence; refresh if Momoiro proto/binary artifacts change.
