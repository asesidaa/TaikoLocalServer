# Architecture Research

**Domain:** Blue AC15 Tokkun mode integration inside TaikoLocalServer
**Researched:** 2026-06-03
**Confidence:** MEDIUM

## Standard Architecture

### System Overview

```
Blue cabinet/RPCS3
    |
    | direct protobuf POST /v10r03/chassis/playresult.php
    v
Adapters.GameProtocol.Blue.Controllers.PlayResultController
    - deserialize generated Blue wire DTO
    - log request
    - map wire DTO to CommonPlayResultData
    - send UpdatePlayResultCommand(GameEra.Blue)
    |
    v
Application.Handlers.UpdatePlayResultCommand.Blue.cs
    - validate baid/user
    - classify Blue playresult kind
    - route to exactly one mode branch
        1. Tokkun branch
        2. Battle branch
        3. Normal/Dani branch
    |
    v
Application.Handlers.UpdatePlayResultCommand.BlueTokkun.cs
    - accept Tokkun playresult
    - persist only proven Tokkun state
    - never write normal score/crown/Dani/battle/favorite/unlock state
    - never store Banacoin balance/payment/account state
    |
    v
Infrastructure.Persistence.TaikoDbContext.Blue.cs
    - UserSaveData_Blue only for proven user-data projection flags
    - BlueTokkun* tables only for proven Tokkun summaries
```

Tokkun should be integrated as a Blue-owned mode branch, not as normal enso with a different `StageMode`, not as Green AI Battle reuse, and not as Banacoin/payment support. The controller can remain thin; the new behavior belongs in Blue mapper/common DTO fields and Blue application handler partials.

### Current Evidence Shape

| Evidence | Finding | Confidence |
|----------|---------|------------|
| `.planning/PROJECT.md` | v1.1 explicitly reopens Blue Tokkun, requires evidence-backed semantics, and forbids normal/battle/profile corruption. | HIGH |
| `proto/blue/taiko.proto` and `Adapters.GameProtocol.Blue/Wire/Game.cs` | Blue has `UserDataResponse.tokkun_tutorial_flg`, `PlayResultRequest.payment_method`, `PlayResultRequest.tokkun_tutorial_flg`, and `PlayResultRequest.ary_tokkunstage_info`. | HIGH |
| IDA daemon scan `req_tokkun_architecture_20260603_codex` | Blue binary contains `GameTokkunMode`, Tokkun assets, `PlayResultRequest.TokkunstageData`, and Banacoin protocol strings. | MEDIUM |
| IDA xref scan `req_tokkun_architecture_mode_xref_20260603_codex` | A `tokkun` string returns `38` in one content-label function; that is not safe evidence for `PlayMode.Tokkun`. | MEDIUM |
| Wiki gameplay context | Tokkun is a one-player practice mode with no score/ranking/reward reflection, needs Banacoin in public gameplay context, and includes speed/autoplay/jump mechanics. | LOW for Blue protocol, MEDIUM for gameplay scoping |

## Component Responsibilities

| Component | Responsibility | Tokkun Integration |
|-----------|----------------|--------------------|
| `PlayResultController` | Direct protobuf endpoint and Mediator dispatch. | Keep as-is except optional structured log fields after mapping. Do not add business logic. |
| `PlayResultMappers` | Convert generated Blue wire DTOs into common application DTOs. | Map Tokkun optional-field presence and raw Tokkun stage summary into `CommonPlayResultData.BlueTokkun.cs`. Do not persist wire DTOs. |
| `CommonPlayResultData.BlueTokkun.cs` | Blue Tokkun-specific application DTO extension. | Add `IsTokkunPlayResult`, `TokkunTutorialFlg`, `HasTokkunTutorialFlg`, and `TokkunStageSummaryDto`. Map `payment_method` only as optional raw metadata if needed for diagnostics; do not persist it. |
| `BluePlayResultMapping` | Blue mode/stage classifier helpers. | Add a small classifier such as `GetBluePlayResultKind(common)` returning `Tokkun`, `Battle`, `Normal`, or `Ambiguous`. Do not add `PlayMode.Tokkun` until a real value is proven. |
| `UpdatePlayResultCommand.Blue.cs` | Era dispatch within Blue playresult handling. | Route Tokkun before battle/normal writes. Ambiguous Tokkun+Battle payloads should return success with warning and no cross-mode persistence unless client evidence proves the combination. |
| `UpdatePlayResultCommand.BlueTokkun.cs` | Tokkun application behavior. | Persist only proven Tokkun tutorial/summary state. Never call `SaveBlueStageAsync`, `ApplyUnlockBits`, `SaveBlueDanAsync`, `UpsertBestAsync`, or battle state helpers. |
| `UserDataQuery.Blue.cs` and `UserDataMappers` | Blue userdata projection. | Only serialize `tokkun_tutorial_flg` after evidence proves the cabinet needs it. Use common DTO projection first. |
| `TaikoDbContext.Blue.cs` and `ITaikoDbContext.Blue.cs` | Blue-owned persistence sets and mappings. | Add Blue Tokkun sets only if persistence is proven. Keep them keyed by `Baid` and FK to `UserData`. |
| Banacoin controllers | Stateless protocol compatibility. | Existing heartbeat/balance/payment/error-log success stubs remain no-persistence. Add `getbanacoininfo.php` only if Tokkun traffic proves it is cabinet-called. |

## Recommended Project Structure

```
Application/
  Common/
    BluePlayResultMapping.cs             # add Blue playresult kind classifier
    BlueTokkunStateExtensions.cs         # only if Tokkun persistence is added
  Dtos/
    CommonPlayResultData.BlueTokkun.cs   # new raw Tokkun DTO fields
    CommonUserDataResponse.Blue.cs       # add Tokkun tutorial projection only if proven
  Handlers/
    UpdatePlayResultCommand.Blue.cs      # add routing branch only
    UpdatePlayResultCommand.BlueTokkun.cs# new mode-specific behavior
    UserDataQuery.Blue.cs                # project proven tutorial flag

Domain/
  Enums/
    PlayMode.cs                          # add Tokkun only after mode id proof
  Entities/
    UserSaveDataBlue.cs                  # tutorial flag can live here if it is userdata state
    BlueTokkunPlaySummary.cs             # optional, separate Tokkun telemetry

Infrastructure/
  Persistence/
    TaikoDbContext.Blue.cs               # BlueTokkun DbSets/mapping if needed
    Migrations/*AddBlueTokkun*.cs        # scoped migration, no Green/Nijiiro changes

Adapters.GameProtocol.Blue/
  Controllers/
    BalanceCheckController.cs            # keep stateless
    BanacoinPaymentController.cs         # keep stateless
    BanacoinErrorLogController.cs        # keep stateless
    GetBanacoinInfoController.cs         # add only from cabinet evidence
    PlayResultController.cs              # keep thin
  Mappers/
    PlayResultMappers.cs                 # map Tokkun wire fields to common DTO
    UserDataMappers.cs                   # map tutorial projection if proven

Tests/Blue/
  BlueTokkunPlayResultMapperTests.cs
  BlueTokkunPlayResultHandlerTests.cs
  BlueTokkunPersistenceTests.cs
  BlueTokkunSourceGuardTests.cs
  BlueRouteSkeletonTests.cs              # update only if a new route is proven
```

### Structure Rationale

- Keep `CommonPlayResultData.BlueTokkun.cs` separate from `CommonPlayResultData.BlueBattle.cs`. Tokkun and battle are disjoint modes with different persistence contracts.
- Keep handler logic in a new `UpdatePlayResultCommand.BlueTokkun.cs` partial, matching the existing Blue battle pattern.
- Put reusable Tokkun persistence helpers in `Application/Common/BlueTokkunStateExtensions.cs` only after persistence is actually needed; avoid a helper file for pure no-op acceptance.
- Prefer `UserSaveDataBlue.TokkunTutorialFlg` for the tutorial flag if evidence proves `userdata.php` readback, because existing Blue tutorial/profile flags already live there. Use separate `BlueTokkunPlaySummary` rows for stage summary telemetry so Tokkun does not pollute normal score/play tables.

## Architectural Patterns

### Pattern 1: Explicit Blue Playresult Kind

**What:** Centralize Blue playresult classification before any mode writes.

**Why:** Current `HandleBlue` checks only `IsBattlePlayResult` and otherwise falls into normal persistence. Tokkun payloads must not reach normal score/crown/profile/favorite/unlock logic.

**Recommended shape:**

```csharp
private enum BluePlayResultKind
{
    Normal,
    Battle,
    Tokkun,
    Ambiguous
}
```

Classifier rules:

1. `Tokkun` when `AryTokkunstageInfo` is present or `tokkun_tutorial_flg` is present.
2. `Battle` when battle release/stage data is present and no Tokkun marker is present.
3. `Normal` otherwise.
4. `Ambiguous` when Tokkun and battle markers coexist; return success with warning and no cross-mode writes until proven.

Do not rely on `PlayMode` alone yet. The current IDA scan did not prove the Tokkun play-mode value. If cabinet logs or IDA later prove a value, add `PlayMode.Tokkun = <value>` and make it a secondary classifier, not the only one.

### Pattern 2: Store Observed Tokkun, Not Inferred Tokkun

**What:** Persist only client-reported, protocol-backed Tokkun fields and avoid derived semantics.

Recommended persistence split:

| Field | Persist? | Location | Reason |
|-------|----------|----------|--------|
| `tokkun_tutorial_flg` | Yes, if userdata readback is required | `UserSaveDataBlue` | It is a user-data projection flag, like other Blue tutorial flags. |
| `tokkun_song_cnt` | Yes, if stage summary persistence is in scope | `BlueTokkunPlaySummary` | Client-reported Tokkun summary, not normal play state. |
| `tookun_songno` | Yes, if stage summary persistence is in scope | `BlueTokkunPlaySummary.SongNoesJson` or child rows | Keep separate from `SongPlayDataBlue`. |
| `tokkun_speedchange_cnt` | Yes, if summary persistence is in scope | `BlueTokkunPlaySummary` | Usage telemetry only. |
| `tokkun_autoplay_cnt` | Yes, if summary persistence is in scope | `BlueTokkunPlaySummary` | Usage telemetry only. |
| `tokkun_jump_cnt` | Yes, if summary persistence is in scope | `BlueTokkunPlaySummary` | Usage telemetry only. |
| `payment_method` | No | none | Banacoin/payment metadata; no repo-owned state. |
| `banacoin_datetime` | No by default | none | Treat as payment/Banacoin provenance, not server state. Log only if needed for debugging. |

Tokkun summary rows must not imply play time accounting, rewards, crowns, self-best, favorites, recents, unlocks, or Banacoin balance.

### Pattern 3: Stateless Banacoin Compatibility

**What:** Return success-shaped Banacoin responses sufficient for Tokkun selection, but do not model money.

Current Blue routes already do this for:

- `heartbeat.php`: `BanacoinStat = 1`
- `balancecheck.php`: success with `CoinCoupon = 0`
- `banacoinpayment.php`: success echoing `personid`
- `banacoinerrorlog.php`: success

Do not add database tables for Banacoin balance, payments, coupons, `personid`, `chid`, or BNID result. If Tokkun evidence shows `getbanacoininfo.php` is called, add a Blue controller with a success/default response and no Mediator call, then update `BlueRouteSkeletonTests`. If logs do not show it, keep that route absent.

## Data Flow

### Tokkun Playresult Flow

```
PlayResultRequest
    -> PlayResultMappers.Map
        -> CommonPlayResultData.BlueTokkun fields
    -> UpdatePlayResultCommand(GameEra.Blue)
        -> Validate baid/user
        -> Classify BluePlayResultKind
        -> HandleBlueTokkun
            -> optional tutorial flag update
            -> optional BlueTokkunPlaySummary insert
            -> SaveChanges
    -> PlayResultResponse { Result = 1 }
```

### Userdata Tokkun Tutorial Flow

```
UserDataQuery.Blue
    -> load UserSaveDataBlue
    -> CommonUserDataResponse.TokkunTutorialFlg
    -> UserDataMappers.Map
    -> UserDataResponse.tokkun_tutorial_flg
```

Only enable this flow after evidence shows the client needs tutorial readback. The existing test `UserDataMapper_Blue_OmitsTokkunTutorialFlag` must be replaced with presence/absence tests that document the new rule.

### Banacoin Flow

```
Tokkun selection
    -> heartbeat/balancecheck/payment/error-log as cabinet requests them
    -> stateless success responses
    -> no Mediator
    -> no EF writes
```

## New vs Modified Modules

| Module | Action | Notes |
|--------|--------|-------|
| `Application/Dtos/CommonPlayResultData.BlueTokkun.cs` | Create | Raw Tokkun fields and presence flags. |
| `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` | Modify | Map `TokkunTutorialFlg`, `AryTokkunstageInfo`, and optional `PaymentMethod` to common DTO. |
| `Application/Common/BluePlayResultMapping.cs` | Modify | Add kind classifier and keep normal stage predicates unchanged. |
| `Application/Handlers/UpdatePlayResultCommand.Blue.cs` | Modify | Add Tokkun/ambiguous branch before normal writes. |
| `Application/Handlers/UpdatePlayResultCommand.BlueTokkun.cs` | Create | Tokkun-specific persistence/no-op logic. |
| `Domain/Entities/UserSaveDataBlue.cs` | Modify conditionally | Add `TokkunTutorialFlg` only if readback is proven. |
| `Domain/Entities/BlueTokkunPlaySummary.cs` | Create conditionally | Only for proven stage summary persistence; no Banacoin fields by default. |
| `Application/Abstractions/ITaikoDbContext.Blue.cs` | Modify conditionally | Add `DbSet<BlueTokkunPlaySummary>` if table is created. |
| `Infrastructure/Persistence/TaikoDbContext.Blue.cs` | Modify conditionally | Map Blue Tokkun table and FK; no Green/Nijiiro changes. |
| `Application/Handlers/UserDataQuery.Blue.cs` | Modify conditionally | Project tutorial flag. |
| `Adapters.GameProtocol.Blue/Mappers/UserDataMappers.cs` | Modify conditionally | Serialize tutorial only when common DTO has value. |
| `Adapters.GameProtocol.Blue/Controllers/*Banacoin*.cs` | Keep or add one route conditionally | Keep stateless; add `GetBanacoinInfoController` only from traffic evidence. |

## Suggested Build Order

1. **Evidence and classifier phase**
   - Add mapper tests for Tokkun wire fields.
   - Add `CommonPlayResultData.BlueTokkun.cs`.
   - Add `BluePlayResultKind` classifier.
   - Replace the old "Tokkun must not appear" guard with "Tokkun must stay in Blue Tokkun files and not Green/normal/battle files."

2. **Safe acceptance phase**
   - Add `HandleBlueTokkun` returning success.
   - Assert Tokkun payloads do not write `SongPlayDataBlue`, `SongBestDataBlue`, `DanScoreDataBlue`, `DanStageScoreDataBlue`, `BlueFavoriteSongs`, `BlueBattle*`, normal unlock bitsets, or Banacoin state.
   - Treat ambiguous Tokkun+Battle as success/no-cross-write until proven.

3. **Tutorial readback phase**
   - If evidence proves it, add `TokkunTutorialFlg` to `UserSaveDataBlue`, migration, common userdata DTO, handler projection, mapper serialization, and tests.
   - If evidence does not prove it, keep `tokkun_tutorial_flg` mapped from playresult for logging only and keep userdata omission explicit.

4. **Tokkun summary persistence phase**
   - If evidence proves stage summary persistence is useful/required, add `BlueTokkunPlaySummary`.
   - Store song/count/speed/autoplay/jump summary only.
   - Do not store `payment_method`, Banacoin balance, `personid`, BNID result, `chid`, coupons, or default inferred practice time.

5. **Banacoin compatibility phase**
   - Smoke current stateless heartbeat/balance/payment/error-log responses through Tokkun.
   - Add `getbanacoininfo.php` only if cabinet/RPCS3 logs show a real request or IDA proves a route call path.
   - Keep all Banacoin controllers out of Mediator and EF.

6. **Verification phase**
   - Run focused Blue Tokkun mapper/handler/persistence/source-guard tests.
   - Run `dotnet test Tests/Tests.csproj --filter BlueTokkun`.
   - Run broader Blue tests.
   - Use temp-output Host build if local server locks `Host/bin`.
   - Finish with cabinet/RPCS3 Tokkun selection and completion evidence.

## Test Architecture

| Test Area | Required Coverage |
|-----------|-------------------|
| Mapper | `AryTokkunstageInfo` maps into common DTO; `tokkun_tutorial_flg` presence is preserved; `payment_method` is not persisted. |
| Handler no-cross-write | Tokkun payload returns `1` and leaves normal score/best/Dani/favorite/battle/shop item state unchanged. |
| Tutorial readback | If enabled, playresult updates the tutorial flag and `userdata.php` serializes it; absent flag does not overwrite existing state. |
| Summary persistence | If enabled, summary rows store only Tokkun summary fields and no Banacoin/account data. |
| Banacoin statelessness | Balance/payment/error-log responses return success and do not call Mediator or write EF state. |
| Route skeleton | Add `getbanacoininfo.php` only if proven; otherwise keep absence asserted. |
| Source guards | Tokkun production files do not reference Green AI Battle, Green state, battle state helpers, normal score/best writes, or Banacoin persistence. |

Replace or retire these existing tests deliberately:

- `BlueA4SourceGuardTests.BluePlayResultCode_DoesNotPromoteTokkunFieldsIntoRuntimeSemantics`
- `BlueMapperTests.UserDataMapper_Blue_OmitsTokkunTutorialFlag`
- `BluePlayResultMapperTests.Map_BluePlayResult_DoesNotInferRuntimeSemanticsFromUnimplementedOptionalSections`

## Anti-Patterns

### Reusing Green AI Battle

**What people do:** Treat Tokkun as another `PlayMode.AiBattle`-style branch or reuse Green ghost fields.

**Why wrong:** Blue Tokkun has its own wire fields and no proven Green AI Battle contract. Reuse would create cross-era state coupling and false rewards/crowns.

**Do instead:** Blue-owned DTO partial, Blue-owned handler partial, Blue-owned tests.

### Falling Through to Normal Play

**What people do:** Map Tokkun fields but let the existing normal handler save stages, bests, crowns, favorites, unlocks, medals, profile counters, or Dani.

**Why wrong:** Public gameplay context says Tokkun records/rewards do not reflect as normal play, and project scope explicitly forbids invented side effects.

**Do instead:** Branch before normal writes and add no-cross-write tests.

### Persisting Banacoin

**What people do:** Add Banacoin balance/payment tables because Tokkun mentions Banacoin.

**Why wrong:** The repo must not store Banacoin state. Current Blue controllers are stateless compatibility stubs.

**Do instead:** Return success-shaped compatibility responses and persist only non-payment Tokkun state that has client evidence.

### Treating IDA String Presence as Full Semantics

**What people do:** Hardcode a `PlayMode` or payment rule from a string table.

**Why wrong:** The current IDA pass proved Tokkun code/assets/protobuf strings exist, but did not prove the playresult `play_mode` value.

**Do instead:** Use payload presence for initial classification and require logs/IDA xrefs before adding `PlayMode.Tokkun`.

## Requirement-to-Phase Hints

| Requirement | Suggested Phase | Notes |
|-------------|-----------------|-------|
| Tokkun can be selected and completed | Evidence/classifier + Banacoin compatibility | First prove route/payment sequence from logs. |
| Tokkun playresults accepted/classified/logged | Mapper + safe acceptance | No persistence needed to return success. |
| Tokkun does not corrupt normal/battle/Dani/favorites/unlocks | Safe acceptance + source guards | This is the main regression risk. |
| Tutorial flag readback | Tutorial readback | Implement only after evidence proves client dependency. |
| Stage summary state | Summary persistence | Separate table, no normal play rows, no Banacoin state. |
| Repeatable verification | Verification | Cabinet/RPCS3 evidence is required, tests are not enough. |

## Sources

- `.planning/PROJECT.md` - v1.1 Tokkun scope, constraints, and out-of-scope Banacoin/payment state.
- `Domain/Enums/PlayMode.cs` - current play-mode enum lacks proven Tokkun value.
- `Domain/Entities/UserSaveDataBlue.cs` - existing Blue profile/tutorial state shape.
- `Infrastructure/Persistence/TaikoDbContext.Blue.cs` and `Application/Abstractions/ITaikoDbContext.Blue.cs` - Blue-owned persistence boundary.
- `Application/Dtos/CommonPlayResultData*.cs` - existing common DTO partial pattern and Blue battle isolation.
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs` and `.BlueBattle.cs` - current Blue normal/battle dispatch and disjoint battle branch.
- `Application/Common/BluePlayResultMapping.cs` - current Blue stage/mode mapping helper.
- `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs` - thin controller/Mediator pattern.
- `Adapters.GameProtocol.Blue/Controllers/BalanceCheckController.cs`, `HeartbeatController.cs`, `BanacoinPaymentController.cs`, `BanacoinErrorLogController.cs` - current stateless Banacoin compatibility surface.
- `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` and `UserDataMappers.cs` - wire-to-common mapping boundary.
- `Tests/Blue/BluePlayResultHandlerTests.cs`, `BluePlayResultMapperTests.cs`, `BlueRouteSkeletonTests.cs`, `BlueMapperTests.cs`, `BlueA4SourceGuardTests.cs`, `BlueBattle*Tests.cs` - existing test and guardrail shape.
- `proto/blue/taiko.proto` and `Adapters.GameProtocol.Blue/Wire/Game.cs` - Tokkun and Banacoin wire field shape.
- IDA daemon requests `req_tokkun_architecture_20260603_codex` and `req_tokkun_architecture_mode_xref_20260603_codex` - local Blue binary Tokkun code/assets/protobuf-string confirmation and non-proof of play-mode value.
- Wiki gameplay context: https://wikiwiki.jp/taiko-fumen/%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0/%E5%9F%BA%E6%9C%AC%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0#tokkunmode
