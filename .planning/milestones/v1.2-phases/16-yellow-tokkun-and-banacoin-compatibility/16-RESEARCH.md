# Phase 16: Yellow Tokkun and Banacoin Compatibility - Research

**Researched:** 2026-06-08
**Domain:** Yellow AC15 Tokkun playresult persistence, userdata tutorial readback, and stateless Banacoin-adjacent routes
**Confidence:** HIGH

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

Phase 16 must replace the current Yellow Tokkun success/no-write placeholder with Yellow-owned Tokkun persistence at the same early playresult branch point. Tokkun classification uses `PlayMode.Tokkun = 3` as the primary classifier, keeps non-null `ary_tokkunstage_info` as Tokkun-shaped protocol evidence, and never treats `tokkun_tutorial_flg` alone as a classifier.

Allowed Yellow Tokkun writes are limited to nullable `UserSaveDataYellow.TokkunTutorialFlg` and append-only Yellow-owned raw stage-history rows. Tokkun uploads must not write Yellow normal score/crown/history, Dani, favorite/recent, shop, medals, profile counters/settings, unlock flags, battle state, Blue/Green/Nijiiro gameplay rows, Blue Tokkun rows, Blue battle rows, or Banacoin/payment state.

Tokkun history stores only protocol-backed raw fields from Yellow `TokkunstageData`: `play_datetime`, `play_mode`, `banacoin_datetime`, `tokkun_song_cnt`, repeated `tookun_songno`, `tokkun_speedchange_cnt`, `tokkun_autoplay_cnt`, and `tokkun_jump_cnt`. Raw song order and duplicates must be preserved, upload rows are append-only, and only client protocol timestamps are stored.

Yellow userdata reads back only optional `tokkun_tutorial_flg` through the proven Yellow `UserDataResponse.tokkun_tutorial_flg` field. Before persistence exists, the optional field is omitted. Tokkun history is not exposed through userdata, AdminApi, WebUI, initial data, or any invented surface in Phase 16.

Yellow Banacoin-adjacent routes remain stateless direct-protobuf compatibility endpoints under `/v09r00/chassis/*`: `balancecheck.php`, `banacoinpayment.php`, `banacoinerrorlog.php`, and `getbanacoininfo.php`. They log requests and return success without wallet, balance, coupon, settlement, receipt, transaction, BNID, CHID, EF, Mediator, AdminApi, WebUI, configuration, or external integration behavior.

### the agent's Discretion

Exact file split, raw song-list storage representation, and focused test grouping are implementation discretion, provided Yellow ownership, existing partial-file patterns, raw protocol fidelity, and behavior-based proof remain clear.

### Deferred Ideas (OUT OF SCOPE)

Yellow Tokkun history/AdminApi/WebUI inspection, Yellow normal/Tokkun cabinet or RPCS3 smoke, final Yellow contract documentation, real Banacoin authority, and Yellow battle behavior remain outside Phase 16.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| YTOK-01 | Yellow Tokkun playresults classify before normal handling and return success without unrelated writes. | Existing `UpdatePlayResultCommand.Yellow.cs` already has the early branch to replace; Blue Phase 10 provides the narrow persistence branch pattern. |
| YTOK-02 | Yellow Tokkun persists nullable tutorial state and append-only raw stage history. | `UserSaveDataYellow.TokkunTutorialFlg` already exists; Yellow needs its own `YellowTokkunStageResult` table and EF surface. |
| YTOK-03 | Yellow userdata reads back only the tutorial flag. | `YellowAc15UserDataAdapter` already passes the nullable value into the AC15 snapshot; `Ac15EraProfiles.Yellow` and Yellow wire mapper are the blockers to change. |
| YBAN-01 | Yellow Banacoin-adjacent routes log and return compatibility success without persistence. | Yellow routes already exist; they need full request logging and statelessness/source tests. |
</phase_requirements>

## Summary

Phase 16 is a direct Yellow analog of the shipped Blue Tokkun/Banacoin contract, constrained by Yellow-owned state and Yellow adapter-local wire DTOs. Current source is already close to the target: Yellow maps Tokkun fields, has `PlayMode.Tokkun = 3` available, has an early Tokkun branch, and already stores nullable `TokkunTutorialFlg` on `UserSaveDataYellow`. The missing work is durable Yellow Tokkun history, changing the early branch from success/no-write to allowed-only persistence, enabling optional Yellow userdata readback, and tightening Banacoin-adjacent visibility/state guards.

**Primary recommendation:** split Phase 16 into four plans: Tokkun schema/classifier proof, Tokkun handler/no-cross-write behavior, userdata tutorial readback, and stateless Banacoin compatibility.

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|--------------|----------------|-----------|
| Tokkun wire mapping and classification | Adapter + Application DTO | Application handler | Yellow wire DTOs are mapped in `Adapters.GameProtocol.Yellow/Mappers`; handler logic consumes common DTO flags. |
| Tokkun tutorial/history persistence | Application + Infrastructure | Domain | Business boundary lives in `UpdatePlayResultCommand.Yellow*`; EF exposes Yellow-owned tables. |
| Tokkun userdata readback | Application shared AC15 service + Yellow adapter mapper | Infrastructure | `Ac15UserDataService` gates optional fields by profile placement; Yellow mapper serializes optional protobuf output. |
| Banacoin-adjacent compatibility | Adapter controller | Tests/source guards | Routes are intentionally stateless direct-protobuf surfaces with no Mediator/EF state. |

## Current Source Baseline

| Area | Current State | Phase 16 Implication |
|------|---------------|----------------------|
| Yellow playresult mapper | Maps `PlayMode`, `TokkunTutorialFlg`, and raw `TokkunStageData`; classifies with `PlayMode.Tokkun` or stage data. | Add/adjust tests to lock this as the Phase 16 classifier contract; do not persist wire DTOs directly. |
| Yellow handler | `IsYellowTokkunShaped(...)` returns success before normal/Dani/shop mutations. | Replace the return-only placeholder with Yellow-owned tutorial/history persistence at the same branch point. |
| Yellow save state | `UserSaveDataYellow.TokkunTutorialFlg` is nullable and not defaulted. | Reuse it exactly; do not add a separate user-state table. |
| Yellow userdata | Adapter passes `TokkunTutorialFlg` into the shared snapshot, but Yellow profile sets `HasTokkunTutorialFlagInUserData: false` and Yellow mapper omits it. | Change Yellow profile placement and mapper only; Green remains omitted and Blue unchanged. |
| Banacoin routes | Four Yellow routes exist and return success; several log only chassis id. | Keep responses stateless but log the full request object and add source/route tests that forbid state. |

## Standard Stack

No external packages are needed. Use existing .NET 10, ASP.NET Core, protobuf-net generated wire DTOs, Mediator handlers, EF Core SQLite migrations, `System.Text.Json` for ordered song-list JSON, and xUnit tests.

## Architecture Patterns

### Yellow-Owned Tokkun Persistence

Follow the four-layer Blue Tokkun persistence pattern, but create Yellow-owned names:

1. `Domain/Entities/YellowTokkunStageResult.cs`
2. `Application/Abstractions/ITaikoDbContext.Yellow.cs`
3. `Infrastructure/Persistence/TaikoDbContext.Yellow.cs`
4. `Infrastructure/Persistence/Migrations/*_AddYellowTokkunState.cs`

Do not reuse `BlueTokkunStageResults`, do not introduce a shared discriminator table, and do not add server-observed upload timestamps.

### Runtime Branch Ordering

`HandleYellow` must keep this order: guest/unknown-user success exits, Tokkun branch, then normal-stage validation, shop medal updates, profile mutations, Dani, normal play persistence, favorites/recent, score/crown, and unlock logic. This preserves the Phase 14/15 no-cross-write boundary and makes the branch behavior testable.

### Optional Userdata Field

`Ac15UserDataService.BuildResponse` already copies snapshot `TokkunTutorialFlg` only when `profile.WirePlacement.HasTokkunTutorialFlagInUserData` is true. Phase 16 should flip Yellow only and update `UserDataMappers.Map` to assign `UserDataResponse.TokkunTutorialFlg` only when `CommonUserDataResponse.TokkunTutorialFlg` is non-null.

### Stateless Banacoin Compatibility

Keep Banacoin-adjacent controllers in `YellowScaffoldControllers.cs` unless implementation chooses a split. The route methods should log `{@Request}` and return direct success DTOs. They should not call `Mediator.Send`, `ITaikoDbContext`, `SaveChanges`, or any wallet/payment abstraction.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Raw ordered repeated song storage | Custom delimiter parser | `System.Text.Json` over `IReadOnlyList<uint>` | Existing Blue Tokkun uses JSON and tests can prove order/duplicates. |
| Banacoin authority | Wallet/payment/coupon transaction model | Direct success responses | Project scope explicitly excludes real Banacoin state. |
| Tokkun guardrails | Source word bans or allowlists | Mapper/handler/EF behavior tests | User rejected Tokkun source-scan guards; behavior is the real proof. |

## Common Pitfalls

| Pitfall | How It Fails | Required Prevention |
|---------|--------------|---------------------|
| Treating `tokkun_tutorial_flg` as classifier | Tutorial-only normal uploads mutate Tokkun state. | Test non-Tokkun tutorial-only upload leaves `TokkunTutorialFlg` unchanged. |
| Reusing Blue Tokkun table | Cross-era audits become ambiguous. | Create `YellowTokkunStageResult` and `YellowTokkunStageResults`. |
| Falling through to normal/Dani/shop | Tokkun upload writes score, crowns, Dan, medals, favorites, recent, unlocks, or profile counters. | Keep branch before all those writes; test mixed payloads. |
| Defaulting tutorial readback | Cabinet sees an invented `0` or `1` before any persisted value exists. | Tests assert optional field absence before persistence. |
| Over-logging only chassis id for Banacoin | Operators cannot inspect compatibility requests. | Log full request object, still no persistence. |

## Validation Architecture

| Property | Value |
|----------|-------|
| Framework | xUnit through `dotnet test Tests/Tests.csproj` |
| Focused Yellow command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowTokkun|FullyQualifiedName~YellowPlayResult|FullyQualifiedName~YellowUserData|FullyQualifiedName~YellowPersistenceBoundary|FullyQualifiedName~YellowRouteSkeleton"` |
| Full suite command | `dotnet test Tests/Tests.csproj` |
| Build command | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase16-yellow"` |

### Phase Requirements To Tests

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|--------------|
| YTOK-01 | Tokkun classifies before normal and writes no unrelated state. | mapper/handler/EF | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowPlayResult|FullyQualifiedName~YellowTokkun"` | partial |
| YTOK-02 | Tutorial/history persistence preserves raw facts. | EF/schema/reload | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowTokkun"` | no, add in plan |
| YTOK-03 | Userdata reads back only optional tutorial flag. | query/mapper | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowUserData|FullyQualifiedName~Ac15UserData"` | partial |
| YBAN-01 | Banacoin routes are stateless log/success surfaces. | route/source | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowRouteSkeleton|FullyQualifiedName~YellowBanacoin|FullyQualifiedName~YellowPersistenceBoundary"` | partial |

## Open Questions (RESOLVED)

None. All Phase 16 areas map directly to YTOK-01, YTOK-02, YTOK-03, YBAN-01, and the five Phase 16 roadmap success criteria.

## Sources

- `AGENTS.md` - repo architecture and Blue Tokkun/Banacoin boundary.
- `.planning/phases/16-yellow-tokkun-and-banacoin-compatibility/16-CONTEXT.md` - locked Phase 16 decisions.
- `.planning/milestones/v1.1-phases/10-evidence-backed-tokkun-state-persistence-and-readback/10-PATTERNS.md` and `10-VERIFICATION.md` - shipped Blue Tokkun analog.
- Current source files under `Adapters.GameProtocol.Yellow`, `Application/Handlers`, `Application/Ac15`, `Domain/Entities`, `Infrastructure/Persistence`, and `Tests/Yellow`.

## RESEARCH COMPLETE
