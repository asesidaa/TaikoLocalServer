# Phase 09: Tokkun Mapper and Safe Playresult Acceptance - Research

**Researched:** 2026-06-05
**Domain:** Blue AC15 direct-protobuf playresult mapping, Tokkun classification, and safe no-write acceptance
**Confidence:** HIGH for implementation planning; MEDIUM for unproven cabinet/runtime semantics that are explicitly out of Phase 9 scope. [VERIFIED: local docs]

<user_constraints>
## User Constraints (from CONTEXT.md)

Copied verbatim from `.planning/phases/09-tokkun-mapper-and-safe-playresult-acceptance/09-CONTEXT.md`. [VERIFIED: local docs]

### Locked Decisions

## Implementation Decisions

### Classifier And Deterministic Client Contract
- **D-01:** Treat non-null `ary_tokkunstage_info` as the Phase 9 Tokkun classifier signal, carrying forward the Phase 7 contract. `tokkun_tutorial_flg` must not classify a payload as Tokkun by itself.
- **D-02:** Do not add `PlayMode.Tokkun`, do not assign a Tokkun numeric `play_mode`, and do not depend on a guessed play-mode value for classification. Raw `play_mode` may be preserved or logged only as context.
- **D-03:** Do not design an "ambiguous mixed payload" branch. The target is an existing deterministic legacy game with well-defined behavior. If a Tokkun payload includes normal-looking or battle-looking material by design, the server treats it as Tokkun and continues.
- **D-04:** Tokkun-classified uploads return success unless concrete client evidence proves a failure response is required.

### Mapper DTO Surface
- **D-05:** Map `tokkun_tutorial_flg` optional presence and value into the common DTO for Phase 10 readback planning, while keeping it out of the standalone classifier rule.
- **D-06:** Preserve all raw/protocol-backed `TokkunstageData` facts in `CommonPlayResultData`: `banacoin_datetime`, `tokkun_song_cnt`, `tookun_songno`, `tokkun_speedchange_cnt`, `tokkun_autoplay_cnt`, and `tokkun_jump_cnt`.
- **D-07:** Preserve the existing wire spelling when referencing `tookun_songno`; do not "correct" generated wire names or manually clean generated `Wire/` output.
- **D-08:** Tokkun DTO fields must remain raw facts. They must not infer payment, practice-time, ranking, reward, unlock, score, crown, favorite, recent-song, battle, or normal progression semantics.

### Safe Acceptance And No-Write Behavior
- **D-09:** Add a Blue Tokkun acceptance path before normal Blue save logic and before battle persistence can consume the payload. The Tokkun branch must not call normal or battle write helpers.
- **D-10:** Phase 9 should be log-and-success plus mapper/classifier preservation. It should not create EF entities, DbSets, migrations, AdminApi/WebUI surfaces, or persistent Tokkun records.
- **D-11:** Existing full Blue playresult request dumps are the primary visibility source. Do not add special speculative logging for "mixed" payload scenarios. A bounded classifier log is acceptable if useful for tests or operator debugging, but not required as a new evidence source.
- **D-12:** Behavior-based tests must prove a Tokkun upload leaves unrelated state unchanged. Source-text scans, Tokkun word bans, and allowlist-style guard tests must not be reintroduced.

### Verification Expectations
- **D-13:** Mapper tests should prove Tokkun classifier state, `tokkun_tutorial_flg` presence/value, and every raw `TokkunstageData` field are preserved.
- **D-14:** Handler tests should prove Tokkun uploads return success for existing and unknown users according to current Blue playresult conventions, while leaving normal score/best, Dani, battle, favorite/recent, profile counters, unlock flags, medal totals, customization/title, shop, and Banacoin-like state untouched.
- **D-15:** Phase 9 verification is automated source/test/build evidence only. Do not claim cabinet/RPCS3 Tokkun proof here.

### the agent's Discretion
- Choose the exact code shape that best fits existing patterns, such as `CommonPlayResultData.BlueTokkun.cs`, a Blue playresult kind/classifier helper, or a focused `HandleBlueTokkun` partial.
- Choose the exact focused test files and fixture construction, provided the tests exercise real mapper and handler behavior rather than source scans.
- Add only minimal logging needed to make the Tokkun branch observable; the full request dump already exists at the controller boundary.

### Deferred Ideas (OUT OF SCOPE)

## Deferred Ideas

None. Discussion stayed within phase scope. Phase 10 persistence/readback and Phase 11 cabinet/RPCS3 proof remain roadmap-owned follow-on work, not new deferred ideas.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| TKPR-01 | Blue `playresult.php` accepts and classifies Tokkun uploads from proven Tokkun fields such as `ary_tokkunstage_info` and `tokkun_tutorial_flg`. | Map `request.AryTokkunstageInfo is not null` to an explicit common DTO classifier and preserve tutorial optional presence/value without using tutorial as the classifier. [VERIFIED: 09-CONTEXT.md; VERIFIED: codebase grep] |
| TKPR-02 | Blue Tokkun uploads return success without writing normal score, crown, Dani, battle, favorite, recent-song, profile, unlock, medal, customization, title, or shop state. | Insert the Tokkun branch after the existing guest/unknown-user success exits and before `IsBattlePlayResult`, `GetOrCreateBlueSaveDataAsync`, `HandleBlueBattle`, and all normal write helpers. [VERIFIED: Application/Handlers/UpdatePlayResultCommand.Blue.cs:18-36] |
| TKPR-03 | Unknown or mixed Tokkun shapes are logged with bounded request context and return success unless concrete client evidence proves a failure response is required. | Current controller already logs the full direct-protobuf request before mapping; a bounded handler/classifier log can be added if useful, but mixed Tokkun must still route to Tokkun success. [VERIFIED: Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs:8-16; VERIFIED: 09-CONTEXT.md] |
</phase_requirements>

## Summary

Phase 9 is a narrow Blue-owned playresult mapper and handler-safety phase. The existing Blue `playresult.php` controller already deserializes direct protobuf, logs `request.Stringify()`, maps to `CommonPlayResultData`, sends `UpdatePlayResultCommand`, and maps `Result = 1` back to protobuf. [VERIFIED: Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs:8-16] The current mapper preserves many normal and battle fields, but it does not preserve `tokkun_tutorial_flg`, `ary_tokkunstage_info`, or `TokkunstageData` facts into common DTO fields. [VERIFIED: Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs:8-59; VERIFIED: proto/blue/taiko.proto:441-452]

The key implementation risk is fall-through. Today `HandleBlue` checks guest/unknown user, then immediately dispatches battle payloads, then starts normal save state, medal, unlock, profile, score, favorite/recent, and Dani writes. [VERIFIED: Application/Handlers/UpdatePlayResultCommand.Blue.cs:18-103] A Tokkun-classified payload must return success before both `HandleBlueBattle` and normal save setup, because Phase 9 forbids battle, shop, recent, score, best, Dani, profile, unlock, medal, customization, title, and Banacoin-like state writes. [VERIFIED: 09-CONTEXT.md; VERIFIED: Application/Handlers/UpdatePlayResultCommand.BlueBattle.cs:13-29]

**Primary recommendation:** implement `CommonPlayResultData.BlueTokkun.cs` plus a mapper helper and a `HandleBlueTokkun` no-write branch, with behavior tests proving Tokkun wins over normal/battle-looking material and returns `1` without changing any existing Blue state tables or save-data fields. [VERIFIED: codebase grep; VERIFIED: focused test run]

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|--------------|----------------|-----------|
| Direct Blue `playresult.php` transport | API / Backend adapter | Application handler | The Blue controller owns route binding, protobuf request logging, mapper dispatch, Mediator call, and protobuf response mapping. [VERIFIED: Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs:4-16] |
| Tokkun wire-to-common mapping | API / Backend adapter | Application DTO | Generated Blue wire DTOs must be mapped into `CommonPlayResultData` before handler logic; generated `Wire/` files are not manually edited. [CITED: AGENTS.md; VERIFIED: Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs:8-59] |
| Tokkun classification | Application handler / DTO | API mapper | Classifier state should be represented on common DTOs, while the handler owns branch order and no-write behavior. [VERIFIED: 09-CONTEXT.md; VERIFIED: Application/Handlers/UpdatePlayResultCommand.Blue.cs:30-36] |
| Safe Tokkun acceptance | Application handler | Database / Storage test assertions | Returning success without writes is business behavior in `UpdatePlayResultCommandHandler`, proven against SQLite in-memory fixture state. [VERIFIED: Tests/Blue/BlueHandlerFixture.cs:22-34] |
| Tokkun persistence/readback | Database / Storage | Application handler | Not owned by Phase 9; Phase 10 owns entities, migrations, and readback. [VERIFIED: 09-CONTEXT.md; VERIFIED: .planning/ROADMAP.md] |
| Cabinet/RPCS3 Tokkun proof | External runtime evidence | API logs | Not owned by Phase 9; Phase 11 owns live proof and final contract tightening. [VERIFIED: 09-CONTEXT.md; VERIFIED: .planning/ROADMAP.md] |

## Project Constraints (from AGENTS.md)

- Blue game routes live under `/v10r03/chassis/*`; shared AC15 startup/version routes remain under `/v01r00/chassis/*`. [CITED: AGENTS.md]
- Blue game endpoint request bodies use direct protobuf and Phase 9 must preserve that transport. [CITED: AGENTS.md]
- Blue is a first-class era, not a Green variant, and Blue state must remain separate from Green and Nijiiro except truly shared identity data. [CITED: AGENTS.md]
- Generated protobuf DTOs must be mapped through `Application/Dtos/Common*` shapes before handler logic; wire DTOs must not be persisted directly. [CITED: AGENTS.md]
- Controllers should deserialize, map, call Mediator, and map back; business behavior belongs in `Application/Handlers`. [CITED: AGENTS.md]
- Use the partial-file pattern for era behavior, with Blue behavior in `.Blue.cs` partials where applicable. [CITED: AGENTS.md]
- Do not manually clean generated `Wire/` files unless regenerating protocol output. [CITED: AGENTS.md]
- Phase 9 must not invent token rewards, battle completion, stage graph behavior, normal unlock mirrors, real Banacoin, or Tokkun progression semantics without concrete client/log/proto/IDA evidence. [CITED: AGENTS.md; VERIFIED: 09-CONTEXT.md]
- Use temp-output Host builds if `Host/bin/Debug/net10.0` is locked by a running server. [CITED: AGENTS.md]

## Standard Stack

### Core

| Library / Tool | Version | Purpose | Why Standard |
|----------------|---------|---------|--------------|
| .NET SDK | Installed `10.0.201`; repo pins `10.0.100` with `latestFeature` roll-forward | Build, test, and run the solution | Existing repo baseline uses `net10.0`, C# 13, nullable, and implicit usings. [VERIFIED: shell dotnet --version; VERIFIED: global.json; VERIFIED: Directory.Build.props] |
| ASP.NET Core MVC | Package family `10.0.7` | Serves Blue game protocol controllers | Existing Blue controller pattern is MVC controller plus protobuf response. [VERIFIED: Directory.Packages.props; VERIFIED: PlayResultController.cs] |
| protobuf-net | `3.2.56`; ASP.NET Core integration `3.2.52` | Serializes/deserializes Blue direct-protobuf DTOs | Existing generated wire DTOs and controller actions depend on protobuf shape; no schema generation change is in scope. [VERIFIED: Directory.Packages.props; VERIFIED: Adapters.GameProtocol.Blue/Wire/Game.cs] |
| Mediator.SourceGenerator / Abstractions | `3.0.2` | Dispatches controller requests to application handlers | Existing `PlayResultController` sends `UpdatePlayResultCommand` through Mediator. [VERIFIED: Directory.Packages.props; VERIFIED: PlayResultController.cs:11-14] |
| EF Core SQLite | `10.0.7` | Existing persistence and in-memory behavior tests | Phase 9 should not add EF entities or migrations, but no-write tests use the existing `TaikoDbContext` fixture. [VERIFIED: Directory.Packages.props; VERIFIED: Tests/Blue/BlueHandlerFixture.cs:22-34] |
| xUnit | `2.9.3`; runner `2.8.2`; test SDK `17.14.1` | Mapper and handler behavior tests | Existing Blue playresult tests use xUnit facts and in-memory SQLite. [VERIFIED: Directory.Packages.props; VERIFIED: Tests/Tests.csproj] |

### Supporting

| Library / Tool | Version | Purpose | When to Use |
|----------------|---------|---------|-------------|
| Riok.Mapperly | `4.3.1` | Source-generated mapping elsewhere in adapters | The current Blue playresult mapper is manual despite `[Mapper]`; preserve local style unless a broader mapper refactor is explicitly planned. [VERIFIED: Directory.Packages.props; VERIFIED: PlayResultMappers.cs:5-8] |
| Serilog / Microsoft logging abstractions | Serilog.AspNetCore `10.0.0`; logging abstraction via host stack | Operator/debug logs | Use only a bounded classifier log if useful; the full request dump already exists at controller entry. [VERIFIED: Directory.Packages.props; VERIFIED: PlayResultController.cs:10] |
| ripgrep | `15.1.0` installed | Source inspection during planning/verification | Use for targeted code searches; do not add source-word Tokkun guard tests. [VERIFIED: shell rg --version; VERIFIED: 09-CONTEXT.md] |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| New `PlayMode.Tokkun` enum branch | Add an enum member and switch on raw `play_mode` | Rejected by locked decisions because the numeric value is unproven. [VERIFIED: 09-CONTEXT.md; VERIFIED: Domain/Enums/PlayMode.cs] |
| EF Tokkun tables in Phase 9 | Add `BlueTokkun*` entities and migrations | Rejected by locked decisions because Phase 9 is log-and-success plus mapper/classifier preservation only. [VERIFIED: 09-CONTEXT.md] |
| Source-scanning Tokkun allowlist guard | Scan files for Tokkun terms or forbidden terms | Rejected by locked decisions; behavior tests must prove no writes. [VERIFIED: 09-CONTEXT.md; VERIFIED: 07-01-TOKKUN-EVIDENCE-CONTRACT.md] |

**Installation:** none. Phase 9 should not install external packages. [VERIFIED: 09-CONTEXT.md; VERIFIED: package audit]

**Version verification:** existing versions were verified from `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `dotnet --version`, and `Tests/Tests.csproj`. [VERIFIED: codebase grep; VERIFIED: shell]

## Package Legitimacy Audit

No external packages are recommended or installed for Phase 9, so the package legitimacy gate is not applicable. [VERIFIED: 09-CONTEXT.md]

| Package | Registry | Age | Downloads | Source Repo | slopcheck | Disposition |
|---------|----------|-----|-----------|-------------|-----------|-------------|
| None | - | - | - | - | Not run | No install planned. [VERIFIED: phase scope] |

**Packages removed due to slopcheck [SLOP] verdict:** none. [VERIFIED: phase scope]
**Packages flagged as suspicious [SUS]:** none. [VERIFIED: phase scope]

## Current Architecture And Data Flow

### System Architecture Diagram

```text
Blue cabinet / RPCS3
        |
        | POST /v10r03/chassis/playresult.php (direct protobuf)
        v
Adapters.GameProtocol.Blue.Controllers.PlayResultController
        | logs full request.Stringify()
        | maps PlayResultRequest -> CommonPlayResultData
        v
Adapters.GameProtocol.Blue.Mappers.PlayResultMappers.Map
        | preserve normal fields
        | preserve battle fields
        | Phase 9: preserve Tokkun classifier and raw Tokkun facts
        v
Application.Handlers.UpdatePlayResultCommandHandler.HandleBlue
        |
        | baid == 0? -> return 1
        | user missing? -> log warning, return 1
        | Phase 9: IsTokkunPlayResult? -> bounded log, return 1, no EF writes
        | IsBattlePlayResult? -> HandleBlueBattle() (battle writes)
        | else -> normal Blue save path (score/profile/unlock/Dani/shop writes)
        v
PlayResultResponse { Result = 1 }
```

This flow reflects the current controller, mapper, and handler branch order plus the required Phase 9 Tokkun insertion point. [VERIFIED: PlayResultController.cs:8-16; VERIFIED: PlayResultMappers.cs:8-59; VERIFIED: UpdatePlayResultCommand.Blue.cs:18-36]

### Component Responsibilities

| Component | Current Finding | Phase 9 Responsibility |
|-----------|-----------------|------------------------|
| `proto/blue/taiko.proto` | `PlayResultRequest` defines `payment_method`, optional `tokkun_tutorial_flg`, optional `ary_tokkunstage_info`, and nested `TokkunstageData` fields. [VERIFIED: proto/blue/taiko.proto:441-452] | Treat as schema source only; do not infer payment, progression, reward, or numeric play-mode semantics. [VERIFIED: 09-CONTEXT.md] |
| `Adapters.GameProtocol.Blue/Wire/Game.cs` | Generated wire exposes `ShouldSerializeTokkunTutorialFlg()` and nullable optional storage, while `AryTokkunstageInfo` is nullable by reference. [VERIFIED: Wire/Game.cs:2027-2038] | Read optional presence helpers from wire, but do not edit generated output. [CITED: AGENTS.md] |
| `PlayResultController.PlayResult` | Logs full request, maps, sends Mediator command, returns mapped result. [VERIFIED: PlayResultController.cs:8-16] | Keep transport/controller shape unchanged; no special Tokkun controller or route. [VERIFIED: 09-CONTEXT.md] |
| `PlayResultMappers.Map` | Maps normal fields and battle classifier state; omits Tokkun DTO fields today. [VERIFIED: PlayResultMappers.cs:8-59] | Add Tokkun common fields and helper mapping in this file only; preserve existing battle mapping. [VERIFIED: codebase grep] |
| `CommonPlayResultData.BlueBattle.cs` | Existing era-specific partial shows how Blue branch-specific DTO fields are isolated. [VERIFIED: CommonPlayResultData.BlueBattle.cs] | Add `CommonPlayResultData.BlueTokkun.cs` or equivalent Blue-specific partial for raw Tokkun data. [VERIFIED: 09-CONTEXT.md] |
| `UpdatePlayResultCommand.Blue.cs` | Guest and unknown users already return success before writes; battle branch runs before normal save setup. [VERIFIED: UpdatePlayResultCommand.Blue.cs:18-36] | Insert Tokkun return-success branch after user checks and before battle/normal writes. [VERIFIED: 09-CONTEXT.md] |
| `UpdatePlayResultCommand.BlueBattle.cs` | Battle branch writes battle rows, active shop Don medals, and battle recent songs. [VERIFIED: UpdatePlayResultCommand.BlueBattle.cs:13-29] | Ensure mixed Tokkun+battle payloads do not reach this branch. [VERIFIED: 09-CONTEXT.md] |
| `Domain/Enums/PlayMode.cs` | Defines `Normal = 0`, `DanMode = 1`, `GaidenMode = 4`, and `AiBattle = 6`; no Tokkun member exists. [VERIFIED: Domain/Enums/PlayMode.cs] | Do not add `Tokkun`; preserve raw `PlayMode` only as context. [VERIFIED: 09-CONTEXT.md] |

### Recommended Project Structure

```text
Application/
├── Dtos/
�?  └── CommonPlayResultData.BlueTokkun.cs   # Blue Tokkun classifier and raw fact DTOs
├── Handlers/
�?  ├── UpdatePlayResultCommand.Blue.cs      # branch insertion after user checks
�?  └── UpdatePlayResultCommand.BlueTokkun.cs # optional focused no-write return path
Adapters.GameProtocol.Blue/
└── Mappers/
    └── PlayResultMappers.cs                 # Tokkun wire-to-common mapping helpers
Tests/
└── Blue/
    ├── BluePlayResultMapperTests.cs         # Tokkun classifier and field preservation
    └── BluePlayResultHandlerTests.cs        # no-write behavior for Tokkun branch
```

This structure follows existing Blue DTO partial, mapper, handler partial, and test organization patterns. [VERIFIED: codebase grep]

## Architecture Patterns

### Pattern 1: Preserve Optional Presence Separately From Value

**What:** Existing Blue mapper code uses `ShouldSerialize*` helpers to preserve optional presence as nullable values or explicit `Has*` flags. [VERIFIED: PlayResultMappers.cs:33-58]
**When to use:** Use it for `tokkun_tutorial_flg` so Phase 10 can distinguish omitted tutorial state from a present `0`. [VERIFIED: 09-CONTEXT.md]

**Example:**

```csharp
// Source: Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs:33-58
TokkunTutorialFlg = request.ShouldSerializeTokkunTutorialFlg()
    ? request.TokkunTutorialFlg
    : null;
```

### Pattern 2: Branch Before Write Helpers

**What:** Branch-specific playresult behavior belongs in `UpdatePlayResultCommandHandler` before normal write helpers mutate EF state. [VERIFIED: UpdatePlayResultCommand.Blue.cs:30-103]
**When to use:** Add Tokkun immediately after guest/unknown-user exits and before `IsBattlePlayResult`. [VERIFIED: 09-CONTEXT.md]

**Example:**

```csharp
// Source pattern: Application/Handlers/UpdatePlayResultCommand.Blue.cs:18-36
var playResultData = request.PlayResultData;
if (playResultData.IsTokkunPlayResult)
{
    return HandleBlueTokkun(request.Baid, playResultData);
}

if (playResultData.IsBattlePlayResult)
{
    return await HandleBlueBattle(request.Baid, playResultData, cancellationToken);
}
```

### Pattern 3: Use Behavior Tests Against Real In-Memory SQLite

**What:** Existing Blue handler tests construct a real `TaikoDbContext` over SQLite in-memory, seed `UserData`/`UserSaveDataBlue`, invoke the real handler, and assert persisted rows. [VERIFIED: Tests/Blue/BlueHandlerFixture.cs:22-34; VERIFIED: Tests/Blue/BluePlayResultHandlerTests.cs:51-110]
**When to use:** Prove Tokkun acceptance returns success and does not change any forbidden table or save-data field. [VERIFIED: 09-CONTEXT.md]

**Example:**

```csharp
// Source pattern: Tests/Blue/BlueHandlerFixture.cs:22-34
await using var fixture = await BlueHandlerFixture.CreateAsync();
fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
await fixture.Context.SaveChangesAsync();
```

### Anti-Patterns to Avoid

- **Classifying from `tokkun_tutorial_flg`:** The tutorial flag is a preserved readback candidate, not a standalone classifier. [VERIFIED: 09-CONTEXT.md]
- **Routing Tokkun through battle first:** `HandleBlueBattle` writes battle stage, release, token, shop Don medal, and recent-song state, which Phase 9 forbids for Tokkun. [VERIFIED: UpdatePlayResultCommand.BlueBattle.cs:13-29]
- **Letting Tokkun fall through normal save:** Normal Blue path writes medals, tutorial flags, difficulty, play date, area, costume, unlock bits, profile counters, score rows, best rows, favorite/recent rows, and Dani rows. [VERIFIED: UpdatePlayResultCommand.Blue.cs:36-103]
- **Adding source-word Tokkun guards:** Phase 7/9 decisions require behavior tests, not Tokkun term bans or allowlists. [VERIFIED: 09-CONTEXT.md; VERIFIED: 07-01-TOKKUN-EVIDENCE-CONTRACT.md]
- **Editing generated `Wire/Game.cs`:** Generated wire output is not a manual cleanup target. [CITED: AGENTS.md]

## Recommended Plan Shape And Task Boundaries

| Task Boundary | Files | Purpose | Stop Condition |
|---------------|-------|---------|----------------|
| Task 1 - Tokkun DTO and mapper preservation | `Application/Dtos/CommonPlayResultData.BlueTokkun.cs`, `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs`, `Tests/Blue/BluePlayResultMapperTests.cs` | Preserve `IsTokkunPlayResult`, `TokkunTutorialFlg` optional presence/value, and all raw `TokkunstageData` fields. [VERIFIED: 09-CONTEXT.md] | Focused mapper tests prove classifier, tutorial presence/value, and all raw facts. |
| Task 2 - Safe handler branch | `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, optional `UpdatePlayResultCommand.BlueTokkun.cs`, `Tests/Blue/BluePlayResultHandlerTests.cs` | Add log-and-success Tokkun path before battle/normal writes. [VERIFIED: UpdatePlayResultCommand.Blue.cs:18-36] | Existing-user, unknown-user, and mixed Tokkun tests return `1` and no forbidden state changes. |
| Task 3 - Regression verification | Test project and Host build only | Run focused mapper/handler tests plus full Blue filter or full Tests project if practical, then temp-output Host build. [VERIFIED: focused test run; CITED: AGENTS.md] | `dotnet test` targeted filters pass and `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase9"` passes. |

Do not split persistence, EF migrations, AdminApi/WebUI, live cabinet proof, or Banacoin behavior into this phase plan. [VERIFIED: 09-CONTEXT.md]

## Exact Mapper, Classifier, Handler, And Test Risks

| Risk | Where It Appears | Why It Matters | Planning Mitigation |
|------|------------------|----------------|---------------------|
| `AryTokkunstageInfo` is nullable but currently ignored | `PlayResultMappers.Map` omits it. [VERIFIED: PlayResultMappers.cs:8-59] | No DTO signal means the handler cannot safely branch Tokkun before writes. [VERIFIED: codebase grep] | Add explicit `IsTokkunPlayResult = request.AryTokkunstageInfo is not null`. |
| `tokkun_tutorial_flg` optional presence can be lost | Wire has `ShouldSerializeTokkunTutorialFlg()`. [VERIFIED: Wire/Game.cs:2027-2035] | Phase 10 needs omitted vs present `0` distinction. [VERIFIED: 09-CONTEXT.md] | Map to nullable `uint? TokkunTutorialFlg`; test present `0`, present nonzero, and absent. |
| Existing mapper test names Tokkun as "unimplemented optional sections" | `BluePlayResultMapperTests` creates Tokkun data but only asserts unrelated normal fields. [VERIFIED: Tests/Blue/BluePlayResultMapperTests.cs:60-84] | This can mask missing preservation. [VERIFIED: codebase grep] | Replace or split into explicit Tokkun preservation test. |
| Mixed Tokkun+battle payload can hit battle branch | Battle classification currently checks release/battle-stage data. [VERIFIED: PlayResultMappers.cs:42-44] | Battle branch writes forbidden battle/shop/recent state. [VERIFIED: UpdatePlayResultCommand.BlueBattle.cs:13-29] | Tokkun branch must have precedence in handler; mapper can still preserve battle data as raw context. |
| Normal path writes many unrelated fields before stage loop | Normal setup writes save data, medal, difficulty, costume, unlock, profile, score, best, favorite/recent, and Dani. [VERIFIED: UpdatePlayResultCommand.Blue.cs:36-103] | Tokkun must not mutate these. [VERIFIED: 09-CONTEXT.md] | Add pre/post snapshot assertions in handler tests. |
| Unknown user behavior must stay permissive | Current Blue handler returns `1` for missing users. [VERIFIED: UpdatePlayResultCommand.Blue.cs:23-28] | Phase 9 expects success for existing and unknown users according to current conventions. [VERIFIED: 09-CONTEXT.md] | Add unknown-user Tokkun test with no tables written. |
| Source-scan guard temptation | Existing Blue A4 guard only blocks Green leakage now. [VERIFIED: Tests/Blue/BlueA4SourceGuardTests.cs] | User explicitly rejected Tokkun word scans. [VERIFIED: 09-CONTEXT.md] | Keep guard changes out unless preserving Green leakage checks. |

## Do Not Hand-Roll

| Problem | Do Not Build | Use Instead | Why |
|---------|--------------|-------------|-----|
| Protobuf request parsing | Custom byte parser or manual body reader | Existing ASP.NET Core protobuf-net model binding and `PlayResultRequest` | Current route already deserializes direct protobuf successfully. [VERIFIED: PlayResultController.cs:8-16] |
| Tokkun numeric mode guessing | New enum value or hardcoded `play_mode` switch | `request.AryTokkunstageInfo is not null` classifier | Locked decisions forbid guessed `PlayMode.Tokkun`. [VERIFIED: 09-CONTEXT.md] |
| Runtime persistence proof | Source-text scans for forbidden words | Behavior tests over `TaikoDbContext` state | Phase 9 requires behavior-based no-write proof. [VERIFIED: 09-CONTEXT.md] |
| Payment/Banacoin model | Wallet, coupon, receipt, transaction, CHID/BNID state | Existing stateless compatibility routes and no playresult state | Real Banacoin is out of scope and Phase 8 kept compatibility stateless. [VERIFIED: 08-01-SUMMARY.md] |
| Generated wire cleanup | Manual renaming of `TookunSongnoes` or `tookun_songno` | Preserve generated spelling and map it as raw fact | Generated wire spelling is part of the schema and locked by D-07. [VERIFIED: 09-CONTEXT.md; VERIFIED: Wire/Game.cs:2387-2388] |

**Key insight:** Phase 9 safety is branch ordering plus state assertions, not a new subsystem. [VERIFIED: codebase grep]

## Common Pitfalls

### Pitfall 1: Tokkun Falls Through Normal Save

**What goes wrong:** A Tokkun payload with stage-looking data writes `SongPlayDataBlue`, `SongBestDataBlue`, favorite/recent rows, save counters, medal totals, unlock bits, costume/title fields, or Dani rows. [VERIFIED: UpdatePlayResultCommand.Blue.cs:36-103]
**Why it happens:** The current normal path starts persistence immediately after the battle branch. [VERIFIED: UpdatePlayResultCommand.Blue.cs:30-36]
**How to avoid:** Insert Tokkun branch before battle and normal writes. [VERIFIED: 09-CONTEXT.md]
**Warning signs:** Tokkun tests need to clean up normal rows or observe changed `UserSaveDataBlue`. [VERIFIED: test architecture]

### Pitfall 2: Tokkun Falls Through Battle Save

**What goes wrong:** Mixed Tokkun+battle payload writes `BlueBattleStageResults`, `BlueBattleUserStates`, `BlueBattleNpcStates`, `BlueBattleTokenStates`, active shop Don medals, or recent songs. [VERIFIED: UpdatePlayResultCommand.BlueBattle.cs:13-29]
**Why it happens:** Mapper battle classifier treats top-level release battle data or stage battle data as battle. [VERIFIED: PlayResultMappers.cs:42-44]
**How to avoid:** Treat `IsTokkunPlayResult` as the highest-priority handler branch after user checks. [VERIFIED: 09-CONTEXT.md]
**Warning signs:** Mixed Tokkun tests pass only when battle fields are absent. [VERIFIED: test architecture]

### Pitfall 3: Tutorial Flag Becomes Classifier

**What goes wrong:** A request with only `tokkun_tutorial_flg` is classified as Tokkun. [VERIFIED: 09-CONTEXT.md]
**Why it happens:** The field name suggests Tokkun, but Phase 7/9 distinguish tutorial/readback evidence from classifier evidence. [VERIFIED: 07-01-TOKKUN-EVIDENCE-CONTRACT.md]
**How to avoid:** Test tutorial-only payload maps tutorial value but `IsTokkunPlayResult == false`. [VERIFIED: 09-CONTEXT.md]
**Warning signs:** Mapper helper uses `ShouldSerializeTokkunTutorialFlg()` in the classifier expression. [VERIFIED: codebase grep]

### Pitfall 4: Raw Facts Become Semantics

**What goes wrong:** `banacoin_datetime`, song counts, speed-change count, autoplay count, or jump count mutate payment, score, reward, unlock, recent, or profile state. [VERIFIED: 09-CONTEXT.md]
**Why it happens:** Field names are mistaken for gameplay rules. [VERIFIED: 07-01-TOKKUN-EVIDENCE-CONTRACT.md]
**How to avoid:** Store fields only as DTO raw facts in Phase 9 and assert no state changes. [VERIFIED: 09-CONTEXT.md]
**Warning signs:** New EF entity, migration, AdminApi/WebUI surface, or normal progression helper appears in a Phase 9 plan. [VERIFIED: 09-CONTEXT.md]

### Pitfall 5: Verification Claims Live Proof

**What goes wrong:** Phase 9 is marked cabinet/RPCS3-proven after automated tests only. [VERIFIED: 09-CONTEXT.md]
**Why it happens:** Automated acceptance can look like end-to-end support. [VERIFIED: ROADMAP.md]
**How to avoid:** State explicitly that Phase 9 verification is automated source/test/build evidence only. [VERIFIED: 09-CONTEXT.md]
**Warning signs:** Phase 9 summary says Tokkun has been proven playable on cabinet/RPCS3. [VERIFIED: 09-CONTEXT.md]

## Code Examples

Verified patterns from existing sources and recommended Phase 9 adaptations.

### Current Direct-Protobuf Controller Boundary

```csharp
// Source: Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs:8-16
Logger.LogInformation("Blue PlayResult request: {Request}", request.Stringify());
var common = PlayResultMappers.Map(request);

var result = await Mediator.Send(
    new UpdatePlayResultCommand(request.Baid, GameEra.Blue, common),
    HttpContext.RequestAborted);

return Ok(PlayResultMappers.Map(result));
```

### Tokkun DTO Shape

```csharp
// Recommended from CommonPlayResultData.BlueBattle.cs partial pattern. [VERIFIED: CommonPlayResultData.BlueBattle.cs]
public partial class CommonPlayResultData
{
    public bool IsTokkunPlayResult { get; set; }
    public uint? TokkunTutorialFlg { get; set; }
    public TokkunStageDataDto? TokkunStageData { get; set; }

    public sealed class TokkunStageDataDto
    {
        public string BanacoinDatetime { get; set; } = string.Empty;
        public uint TokkunSongCnt { get; set; }
        public List<uint> TookunSongnoes { get; set; } = [];
        public uint TokkunSpeedchangeCnt { get; set; }
        public uint TokkunAutoplayCnt { get; set; }
        public uint TokkunJumpCnt { get; set; }
    }
}
```

### Tokkun Mapper Helper

```csharp
// Recommended from PlayResultMappers optional-field and battle helper patterns. [VERIFIED: PlayResultMappers.cs:33-58; VERIFIED: PlayResultMappers.cs:134-155]
IsTokkunPlayResult = request.AryTokkunstageInfo is not null,
TokkunTutorialFlg = request.ShouldSerializeTokkunTutorialFlg()
    ? request.TokkunTutorialFlg
    : null,
TokkunStageData = MapTokkunStageData(request.AryTokkunstageInfo),
```

### Tokkun Handler Branch

```csharp
// Recommended insertion after Application/Handlers/UpdatePlayResultCommand.Blue.cs:23-30. [VERIFIED: UpdatePlayResultCommand.Blue.cs:23-36]
var playResultData = request.PlayResultData;
if (playResultData.IsTokkunPlayResult)
{
    logger.LogInformation(
        "Accepted Blue Tokkun playresult for baid {Baid}: play_mode={PlayMode}",
        request.Baid,
        playResultData.PlayMode);
    return 1;
}
```

## State Of The Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Ban Tokkun terms in source guards | Allow named Tokkun support and prove safety with behavior tests | Phase 7 | Planner must not add new Tokkun source-word scans. [VERIFIED: 07-01-TOKKUN-EVIDENCE-CONTRACT.md; VERIFIED: 09-CONTEXT.md] |
| Proto-only Banacoin uncertainty | Blue `getbanacoininfo.php` exists as stateless `Result = 1` route | Phase 8 | Phase 9 does not need to add Banacoin route availability or state. [VERIFIED: 08-01-SUMMARY.md] |
| Tokkun runtime behavior absent | Phase 9 may add mapper/classifier/log-and-success handling only | Phase 9 scope | No persistence/readback or live proof belongs in this phase. [VERIFIED: ROADMAP.md; VERIFIED: 09-CONTEXT.md] |
| Battle writes allowed battle/shop/recent side effects | Tokkun allows no gameplay-state side effects | Phase 9 scope | Tokkun must be stricter than battle. [VERIFIED: 09-CONTEXT.md; VERIFIED: UpdatePlayResultCommand.BlueBattle.cs:13-29] |

**Deprecated/outdated:**
- `BluePlayResultCode_DoesNotPromoteTokkunFieldsIntoRuntimeSemantics`: Phase 7 removed the stale Tokkun source guard and forbids replacement word scans. [VERIFIED: 07-01-TOKKUN-EVIDENCE-CONTRACT.md]
- `PlayMode.Tokkun` guesses: no enum member exists and the numeric value remains blocked until Phase 11 or later evidence. [VERIFIED: Domain/Enums/PlayMode.cs; VERIFIED: 09-CONTEXT.md]

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | Research remains valid until 2026-07-05 unless Blue playresult mapper/handler code changes first. [ASSUMED] | Metadata | Planner may rely on stale branch-order or test-target details if the playresult code changes before planning. |

**No user confirmation is needed before planning Phase 9, provided the plan stays inside the locked decisions above and the A1 validity estimate is treated as a planning hygiene note rather than a product decision.** [VERIFIED: 09-CONTEXT.md]

## Open Questions (RESOLVED)

1. **RESOLVED / routed to Phase 11: Exact numeric Tokkun `play_mode` remains unknown.** [VERIFIED: 07-01-TOKKUN-EVIDENCE-CONTRACT.md]
   What we know: current enum has no Tokkun member and Phase 9 forbids guessing. [VERIFIED: Domain/Enums/PlayMode.cs; VERIFIED: 09-CONTEXT.md]
   What is unclear: the real client numeric value. [VERIFIED: local docs]
   Recommendation: do not block Phase 9; preserve/log raw `PlayMode` only and leave proof to Phase 11. [VERIFIED: 09-CONTEXT.md]

2. **RESOLVED / routed to Phase 10: Whether Tokkun tutorial/readback must be persisted is not a Phase 9 question.** [VERIFIED: ROADMAP.md]
   What we know: `tokkun_tutorial_flg` exists on request and userdata response wire surfaces. [VERIFIED: proto/blue/taiko.proto:316; VERIFIED: proto/blue/taiko.proto:442]
   What is unclear: real first-run/readback timing. [VERIFIED: 07-01-TOKKUN-EVIDENCE-CONTRACT.md]
   Recommendation: map optional presence/value now; Phase 10 decides persistence/readback. [VERIFIED: 09-CONTEXT.md]

3. **RESOLVED / routed to Phase 11: Cabinet/RPCS3 Tokkun proof remains open.** [VERIFIED: ROADMAP.md]
   What we know: Phase 9 accepts only automated source/test/build evidence. [VERIFIED: 09-CONTEXT.md]
   What is unclear: live route sequence, retry behavior, and post-upload userdata behavior. [VERIFIED: ROADMAP.md]
   Recommendation: do not include live proof tasks in Phase 9; reserve for Phase 11. [VERIFIED: ROADMAP.md]

No blocked items prevent Phase 9 planning. [VERIFIED: local docs]

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|-------------|-----------|---------|----------|
| .NET SDK | Build/test Phase 9 | Yes | `10.0.201` installed; repo pins `10.0.100` with latestFeature roll-forward | Install compatible .NET 10 SDK if absent. [VERIFIED: shell dotnet --version; VERIFIED: global.json] |
| .NET test runner | Focused mapper/handler tests | Yes | `18.3.0.15422` from `dotnet test --version` | Use `dotnet test Tests/Tests.csproj` through SDK. [VERIFIED: shell] |
| Git | Optional doc commit | Yes | `2.52.0.windows.1` | Manual file review if git unavailable. [VERIFIED: shell git --version] |
| ripgrep | Source inspection | Yes | `15.1.0` | PowerShell `Select-String`. [VERIFIED: shell rg --version] |
| IDA shared daemon | Only if new binary evidence is required | Not checked because Phase 9 research did not need IDA | - | If needed, use `.tools/blue/idadrv.py status` and reuse the shared daemon. [VERIFIED: 09-CONTEXT.md] |

**Missing dependencies with no fallback:** none for Phase 9 automated planning. [VERIFIED: environment audit]

**Missing dependencies with fallback:** IDA is not required for current Phase 9 planning; if a planner uncovers a blocked protocol question, use `.tools/blue/idadrv.py` rather than opening the DB directly. [VERIFIED: 09-CONTEXT.md]

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | xUnit `2.9.3` with `Microsoft.NET.Test.Sdk` `17.14.1`. [VERIFIED: Directory.Packages.props] |
| Config file | `Tests/Tests.csproj`; package versions centralized in `Directory.Packages.props`. [VERIFIED: Tests/Tests.csproj; VERIFIED: Directory.Packages.props] |
| Quick run command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultMapperTests|FullyQualifiedName~BluePlayResultHandlerTests"` |
| Broader phase run command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultMapperTests|FullyQualifiedName~BluePlayResultHandlerTests|FullyQualifiedName~BlueBattlePlayResultMapperTests|FullyQualifiedName~BlueBattlePlayResultHandlerTests"` |
| Build command | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase9"` |
| Baseline result | Existing four focused Blue playresult test classes passed: 28 passed, 0 failed, 0 skipped. [VERIFIED: focused test run] |

### Phase Requirements To Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|--------------|
| TKPR-01 | Mapper classifies Tokkun from non-null `AryTokkunstageInfo`, does not classify tutorial-only payload, preserves tutorial optional presence/value and all raw `TokkunstageData` fields. | Unit mapper | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultMapperTests"` | Yes, extend existing file. [VERIFIED: Tests/Blue/BluePlayResultMapperTests.cs] |
| TKPR-02 | Existing-user Tokkun upload returns `1` and leaves normal score/best, Dani, battle, favorite/recent, profile counters, unlock bits, medal totals, customization/title, shop, and Banacoin-like state unchanged. | Integration handler with SQLite in-memory | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultHandlerTests"` | Yes, extend existing file. [VERIFIED: Tests/Blue/BluePlayResultHandlerTests.cs; VERIFIED: Tests/Blue/BlueHandlerFixture.cs] |
| TKPR-03 | Unknown-user Tokkun and mixed Tokkun+battle/normal-looking payloads return `1`, are accepted via Tokkun branch, and do not call battle or normal write helpers. | Integration handler with SQLite in-memory | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultHandlerTests"` | Yes, extend existing file. [VERIFIED: Tests/Blue/BluePlayResultHandlerTests.cs] |

### Sampling Rate

- **Per task commit:** run the test class touched by that task. [VERIFIED: repo test pattern]
- **Per wave merge:** run all four Blue playresult mapper/handler classes. [VERIFIED: focused test run]
- **Phase gate:** run the broader phase command plus temp-output Host build before `$gsd-verify-work`. [CITED: AGENTS.md]

### Wave 0 Gaps

- [ ] `Application/Dtos/CommonPlayResultData.BlueTokkun.cs` - missing DTO partial for Tokkun classifier and raw facts. [VERIFIED: codebase grep]
- [ ] `Tests/Blue/BluePlayResultMapperTests.cs` - existing Tokkun-adjacent test must be converted into explicit preservation/classifier assertions. [VERIFIED: Tests/Blue/BluePlayResultMapperTests.cs:60-84]
- [ ] `Tests/Blue/BluePlayResultHandlerTests.cs` - missing Tokkun no-write tests for existing user, unknown user, and mixed Tokkun+battle/normal payloads. [VERIFIED: codebase grep]

## Security Domain

Security enforcement is enabled in `.planning/config.json`, but Phase 9 does not add auth, sessions, external integrations, admin routes, or storage. [VERIFIED: .planning/config.json; VERIFIED: 09-CONTEXT.md]

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|------------------|
| V2 Authentication | No | No new authenticated surfaces. [VERIFIED: 09-CONTEXT.md] |
| V3 Session Management | No | No sessions or cookies added. [VERIFIED: 09-CONTEXT.md] |
| V4 Access Control | No | No AdminApi/WebUI or privileged route added. [VERIFIED: 09-CONTEXT.md] |
| V5 Input Validation | Yes | Treat all Tokkun fields as raw client-reported facts; branch only on nullable presence and avoid numeric enum guesses. [VERIFIED: 09-CONTEXT.md] |
| V6 Cryptography | No | No cryptography or token changes. [VERIFIED: 09-CONTEXT.md] |
| V7 Error Handling and Logging | Yes | Return success for Tokkun-classified uploads and use existing bounded server logging without leaking raw evidence into docs. [VERIFIED: PlayResultController.cs:10; VERIFIED: 09-CONTEXT.md] |
| V8 Data Protection | Yes | Do not persist Banacoin/payment-like or unrelated gameplay state from Tokkun uploads. [VERIFIED: 09-CONTEXT.md] |

### Known Threat Patterns For This Stack

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| State pollution from untrusted client payload | Tampering | Highest-priority Tokkun branch returns success before normal/battle write helpers; behavior tests assert unchanged tables and fields. [VERIFIED: 09-CONTEXT.md] |
| Payment-like data misinterpretation | Tampering / Repudiation | Preserve `banacoin_datetime` only as raw DTO fact in Phase 9; do not create wallet, transaction, receipt, coupon, CHID, or BNID state. [VERIFIED: 09-CONTEXT.md; VERIFIED: 08-01-SUMMARY.md] |
| Log overreach with sensitive/raw proof material | Information Disclosure | Rely on existing server request logging for runtime visibility and avoid embedding raw cabinet/RPCS3 proof in Phase 9 docs. [VERIFIED: PlayResultController.cs:10; VERIFIED: 09-CONTEXT.md] |
| Denial by unsupported mode hard-fail | Denial of Service | Return `1` for Tokkun-classified unknown/mixed shapes unless client evidence proves failure is required. [VERIFIED: 09-CONTEXT.md] |

## Sources

### Primary (HIGH confidence)

- `AGENTS.md` and top-level user-provided Agent Notes - repo architecture rules, Blue direct-protobuf transport, Blue state separation, generated wire constraints, and common commands. [CITED: AGENTS.md]
- `.planning/phases/09-tokkun-mapper-and-safe-playresult-acceptance/09-CONTEXT.md` - locked Phase 9 decisions, canonical references, code context, and boundaries. [VERIFIED: local docs]
- `.planning/REQUIREMENTS.md` - TKPR-01 through TKPR-03 and out-of-scope constraints. [VERIFIED: local docs]
- `.planning/ROADMAP.md` - Phase 9/10/11 split and success criteria. [VERIFIED: local docs]
- `.planning/STATE.md` - current workflow position after Phase 8 completion. [VERIFIED: local docs]
- `.planning/PROJECT.md` - v1.1 scope, evidence hierarchy, and out-of-scope Banacoin/progression behavior. [VERIFIED: local docs]
- `.planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-01-TOKKUN-EVIDENCE-CONTRACT.md` - Tokkun row matrix, classifier contract, no-runtime-write targets, and guard policy. [VERIFIED: local docs]
- `.planning/phases/08-stateless-banacoin-compatibility-and-availability/08-01-SUMMARY.md` - stateless `getbanacoininfo.php` route and Banacoin compatibility boundary. [VERIFIED: local docs]
- `.planning/research/SUMMARY.md` - v1.1 architecture/pitfall summary for Tokkun support. [VERIFIED: local docs]
- `proto/blue/taiko.proto` - Tokkun request fields and nested `TokkunstageData`. [VERIFIED: codebase grep]
- `Adapters.GameProtocol.Blue/Wire/Game.cs` - generated optional presence helpers and field names. [VERIFIED: codebase grep]
- `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs` - current direct-protobuf playresult controller flow. [VERIFIED: codebase grep]
- `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` - current mapping and battle classifier pattern. [VERIFIED: codebase grep]
- `Application/Dtos/CommonPlayResultData.cs`, `CommonPlayResultData.Green.cs`, and `CommonPlayResultData.BlueBattle.cs` - current common DTO and partial pattern. [VERIFIED: codebase grep]
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs` and `UpdatePlayResultCommand.BlueBattle.cs` - branch ordering and write helpers. [VERIFIED: codebase grep]
- `Tests/Blue/*.cs` focused playresult files and `BlueHandlerFixture.cs` - mapper/handler test patterns and SQLite fixture. [VERIFIED: codebase grep]

### Secondary (MEDIUM confidence)

- `.planning/codebase/STACK.md` and `.planning/codebase/TESTING.md` - generated codebase maps from 2026-05-28; still cross-checked against current files and tool versions in this session. [VERIFIED: local docs; VERIFIED: codebase grep]
- Focused baseline test command: `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultMapperTests|FullyQualifiedName~BluePlayResultHandlerTests|FullyQualifiedName~BlueBattlePlayResultMapperTests|FullyQualifiedName~BlueBattlePlayResultHandlerTests"` - 28 passed, 0 failed. [VERIFIED: focused test run]

### Tertiary (LOW confidence)

- None used for implementation planning. [VERIFIED: source review]

## Metadata

**Confidence breakdown:**

- Standard stack: HIGH - versions verified from repo files and local tooling; no new packages needed. [VERIFIED: Directory.Packages.props; VERIFIED: shell]
- Architecture: HIGH - controller, mapper, DTO partial, handler, and test fixture paths were directly inspected. [VERIFIED: codebase grep]
- Pitfalls: HIGH - risks map to existing write helpers and locked Phase 9 no-write decisions. [VERIFIED: codebase grep; VERIFIED: 09-CONTEXT.md]
- Runtime Tokkun semantics: MEDIUM - exact cabinet behavior, numeric play mode, and readback timing remain Phase 11/10 questions by design. [VERIFIED: ROADMAP.md]

**Research date:** 2026-06-05
**Valid until:** 2026-07-05 for codebase planning assumptions, or earlier if Blue playresult handler/mapper code changes before planning. [ASSUMED]
