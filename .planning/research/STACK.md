# Stack Research

**Domain:** Blue AC15 Tokkun mode support in existing TaikoLocalServer
**Researched:** 2026-06-03
**Confidence:** MEDIUM overall; HIGH for repository stack and existing protocol fields, MEDIUM for Tokkun runtime constants until RPCS3/log/IDA follow-up resolves them

## Executive Recommendation

Use the existing TaikoLocalServer stack. Do not add a payment SDK, Banacoin service, new game-data extractor, external database, or scraper. Blue Tokkun support should be a narrow Blue-owned extension of the current direct-protobuf route, Mapperly DTO mapping, Mediator handler, and EF Core SQLite persistence pattern.

The implementation should add Tokkun recognition to `playresult.php`, map the already-generated Blue Tokkun wire fields into new Blue-specific common DTO fields, branch to a `HandleBlueTokkun` path before normal score handling, and persist only evidence-backed Tokkun state. Banacoin should remain stateless and permissive: return success and enough non-persistent balance/coupon behavior for the client to enter Tokkun, but do not store balances, purchases, deductions, or payment history.

## Recommended Stack

### Core Technologies

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| .NET SDK / C# | SDK 10.0.100, C# 13, `net10.0` | Runtime and language baseline | Already pinned in `global.json` and `Directory.Build.props`; avoids introducing a parallel runtime for one Blue mode. |
| ASP.NET Core | 10.0.7 packages | Blue protocol controllers under `/v10r03/chassis/*` | Existing Blue endpoints are direct protobuf MVC controllers with `[Produces("application/protobuf")]`; Tokkun uses the same route surface. |
| protobuf-net | `protobuf-net` 3.2.56, `protobuf-net.AspNetCore` 3.2.52 | Direct Blue request/response serialization | `proto/blue/taiko.proto` and `Adapters.GameProtocol.Blue/Wire/Game.cs` already expose Tokkun/Banacoin fields; no schema tool change needed unless proto regeneration is required. |
| Mediator.SourceGenerator | 3.0.2 | Application handler dispatch | Current `PlayResultController` maps wire to `CommonPlayResultData` and sends `UpdatePlayResultCommand`; Tokkun should stay in that use-case boundary. |
| EF Core SQLite | EF Core 10.0.7, EntityFrameworkCore.Exceptions.Sqlite 10.0.0 | Blue-owned local persistence | Existing Blue normal, item shop, Dani, and battle state use EF Core/SQLite; Tokkun state, if persisted, should use the same migration path. |
| xUnit | 2.9.3 | Contract and source-guard tests | Existing Blue behavior is protected by mapper, route, handler, persistence, and source-guard tests; Tokkun should add focused equivalents. |

### Supporting Libraries and Tools

| Library or Tool | Version / Location | Purpose | When to Use |
|-----------------|--------------------|---------|-------------|
| Riok.Mapperly | 4.3.1 | Wire-to-common DTO mapping | Continue using mapper classes in `Adapters.GameProtocol.Blue/Mappers`; add manual mapping where optional protobuf field presence matters. |
| Serilog.AspNetCore | 10.0.0 | Request evidence logging | Keep full `PlayResultRequest.Stringify()` logging for Tokkun classification until RPCS3/cabinet traffic is stable. |
| protobuf-net `protogen` | Tool used by existing Blue wire generation docs | Regenerate `Adapters.GameProtocol.Blue/Wire/Game.cs` only if `proto/blue/taiko.proto` changes | Current generated wire already has Tokkun fields, so regeneration is not a Tokkun prerequisite. |
| IDA daemon driver | `.tools/blue/idadrv.py` | Blue EBOOT evidence without opening another IDA instance | Use for exact constants, call paths, and route evidence; create request scripts under `.tools/blue/scratch/req_tokkun_*.py`. |
| RPCS3 / cabinet logs | Local operator evidence | Verify actual Tokkun request order and field values | Required before treating `PlayMode`, `StageMode`, Banacoin balance behavior, or `getbanacoininfo.php` as runtime truth. |

## Protocol Additions and Changes

### Existing Wire Fields to Use

`proto/blue/taiko.proto` already contains the relevant Tokkun protocol fields. The generated Blue wire model in `Adapters.GameProtocol.Blue/Wire/Game.cs` exposes the same shape.

| Message | Fields | Recommended Use |
|---------|--------|-----------------|
| `UserDataResponse` | `tokkun_tutorial_flg = 37` | Read back only if client/RPCS3 evidence proves it gates tutorial or Tokkun entry. Current mapper intentionally omits it. |
| `PlayResultRequest` | `payment_method = 43`, `tokkun_tutorial_flg = 44`, `ary_tokkunstage_info = 45` | Map into Blue Tokkun DTO fields. Do not infer score/reward semantics from these names. |
| `PlayResultRequest.TokkunstageData` | `banacoin_datetime`, `tokkun_song_cnt`, repeated `tookun_songno`, `tokkun_speedchange_cnt`, `tokkun_autoplay_cnt`, `tokkun_jump_cnt` | Treat as Tokkun session summary/observation data. Persist only if there is a readback, audit, or reproducibility requirement. Preserve the proto typo `tookun_songno` in wire mapping. |
| `BalancecheckResponse` | `coin_coupon = 4` | Candidate stateless allowance field. Current value is `0`; this is risky if Tokkun entry checks balance before payment. |
| `BanacoinpaymentRequest` | `mode`, `banacoin_price` | Log for evidence; do not store or deduct. |
| `BanacoinpaymentResponse` | `result`, `personid`, `bnid_result`, `chid` | Keep stateless success; echo `personid`; empty strings are acceptable until logs prove otherwise. |
| `Banacoinerrorlog*` | request plus `result` response | Keep log-and-success. |
| `Getbanacoininfo*` | generated types exist | Do not add route yet. Current route tests exclude `/v10r03/chassis/getbanacoininfo.php`, and IDA string probing found message names but not a `chassis/getbanacoininfo.php` route string. |

### PlayResult Classification

Add Tokkun classification to the Blue playresult mapper/common DTO, not to the controller:

1. Keep `PlayResultController` as deserialize -> map -> `UpdatePlayResultCommand(GameEra.Blue)` -> map response.
2. Add `CommonPlayResultData.BlueTokkun.cs` with:
   - `bool IsTokkunPlayResult`
   - `uint? PaymentMethod`
   - `uint? TokkunTutorialFlg`
   - `TokkunStageSummary? TokkunStage`
   - raw observed `PlayMode` and `StageMode` already exist and should remain available for tests/logging.
3. Classify Tokkun by `AryTokkunstageInfo is not null` plus the exact Tokkun `PlayMode` once proven. Do not classify by `payment_method` alone.
4. Handler precedence should be explicit: battle payloads stay battle; Tokkun payloads go to `HandleBlueTokkun`; normal payloads go to existing normal logic. Mixed battle/Tokkun payloads should log a warning and must not fall through to normal score persistence.

### Banacoin Behavior

Keep Banacoin stateless and permissive.

Recommended minimum:

| Endpoint | Current State | Tokkun Recommendation |
|----------|---------------|-----------------------|
| `/v10r03/chassis/heartbeat.php` | Returns `BanacoinStat = 1` | Keep. This advertises service availability without state. |
| `/v10r03/chassis/balancecheck.php` | Returns `Result = 1`, echoes `personid`, `CoinCoupon = 0` | If Tokkun selection refuses zero balance, change to a stateless configured allowance. Default `390` is gameplay-context-backed by the wiki, but RPCS3 should confirm the field and accepted value. |
| `/v10r03/chassis/banacoinpayment.php` | Returns `Result = 1`, echoes `personid`, empty `bnid_result`/`chid` | Keep permissive success. Log `mode` and `banacoin_price`; do not persist payment state. |
| `/v10r03/chassis/banacoinerrorlog.php` | Log-and-success | Keep. |
| `/v10r03/chassis/getbanacoininfo.php` | Not routed; route guard explicitly excludes it | Do not add unless RPCS3 logs or deeper IDA evidence show the Blue client calls it during Tokkun. If added, it must be a stateless identity/progression stub, not a Banacoin account store. |

If a configurable allowance is needed, add it to `EraSettings` or a small Blue-specific options class, for example `BlueTokkunBanacoinAllowance`, defaulting to the smallest proven value. Do not add per-user balances.

## Data Model Recommendation

### Required If Tokkun Tutorial Readback Is Proven

Add one Blue-owned save field:

| Location | Field | Why |
|----------|-------|-----|
| `Domain/Entities/UserSaveDataBlue.cs` | `uint TokkunTutorialFlg` | Mirrors existing `ItemshopTutorialFlg` and `WaiwaiTutorialFlg`; can be surfaced through `UserDataResponse.tokkun_tutorial_flg`. |
| `Application/Dtos/CommonUserDataResponse.Blue.cs` | `uint? TokkunTutorialFlg` | Keeps field era-specific and optional. |
| `Application/Handlers/UserDataQuery.Blue.cs` | map from save data | Readback only when intentionally serialized. |
| `Adapters.GameProtocol.Blue/Mappers/UserDataMappers.cs` | assign `response.TokkunTutorialFlg` only when common value is present | Preserve optional field absence unless evidence says it is needed. |

This requires an EF migration adding the column to `UserSaveData_Blue`. Do not store this in shared `UserData`, Green save data, or battle state.

### Optional If Tokkun Session Summary Must Be Audited

If the roadmap decides to retain Tokkun playresult summaries, use a Blue-only observation table:

| Entity | Key Fields | Notes |
|--------|------------|-------|
| `BlueTokkunPlaySession` | `Id`, `Baid`, `CreatedAt`, `PlayDatetime`, `PaymentMethod`, `BanacoinDatetime`, `TokkunSongCnt`, `TokkunSpeedchangeCnt`, `TokkunAutoplayCnt`, `TokkunJumpCnt`, `ObservedPlayMode` | Observation/audit data only. No score, crown, unlock, balance, or deduction columns. |
| `BlueTokkunPlayedSong` | `SessionId`, `SongIndex`, `SongNo` | Use child rows instead of ad hoc serialized lists if the repeated `tookun_songno` list must be queryable. |

Do not reuse `SongPlayDatum_Blue`, `SongBestDatum_Blue`, `BlueBattleStageResults`, `BlueFavoriteSongs`, `BlueRecentSongs`, `DanScoreDatum_Blue`, or shop item state for Tokkun unless future evidence proves a specific side effect.

## Integration Points

| Area | Files to Change | Recommendation |
|------|-----------------|----------------|
| Wire mapping | `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` | Map `PaymentMethod`, `TokkunTutorialFlg`, and `AryTokkunstageInfo` into new common DTO fields with `ShouldSerialize*` checks. |
| Common DTO | `Application/Dtos/CommonPlayResultData.BlueTokkun.cs` | Keep Tokkun fields separate from Green/Nijiiro/common fields. |
| PlayResult handler | `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, new `UpdatePlayResultCommand.BlueTokkun.cs` | Add `HandleBlueTokkun` branch before normal state writes. |
| Normal state guard | `Application/Handlers/UpdatePlayResultCommand.Blue.cs` | Ensure Tokkun cannot call `ApplyUnlockBits`, `BlueProfileCounters.ApplyStage`, `SaveBlueStageAsync`, `UpsertBestAsync`, `SaveBlueDanAsync`, or favorite/recent updates. |
| Banacoin stubs | `BalanceCheckController.cs`, `BanacoinPaymentController.cs`, `BanacoinErrorLogController.cs`, maybe route tests | Keep stateless success; change balance only if needed to make Tokkun selectable. |
| UserData readback | `UserDataQuery.Blue.cs`, `UserDataMappers.cs`, `CommonUserDataResponse.Blue.cs`, `UserSaveDataBlue.cs` | Add only if `tokkun_tutorial_flg` readback is proven necessary. |
| Persistence | `Domain/Entities`, `Infrastructure/Persistence/TaikoDbContext.Blue.cs`, `ITaikoDbContext.Blue.cs`, EF migration | Add only Blue-owned Tokkun rows/columns. |
| Tests | `Tests/Blue/*Tokkun*`, replace/update old source guard | Existing `BlueA4SourceGuardTests.BluePlayResultCode_DoesNotPromoteTokkunFieldsIntoRuntimeSemantics` must be replaced with a guard that allows Tokkun only in named Tokkun files and still forbids normal/battle state corruption. |

## What NOT to Use

| Avoid | Why | Use Instead |
|-------|-----|-------------|
| Banacoin SDK, payment provider, account balance table, purchase ledger | User explicitly forbids storing Banacoin state; local server only needs to let Tokkun play. | Stateless success/coupon responses and logging. |
| Green AI Battle helpers or storage | Tokkun is not Green AI Battle, and Blue battle already has separate storage. | Blue-only DTOs, handlers, and source guards. |
| Normal Blue score/crown persistence for Tokkun | Wiki context says Tokkun records/rewards do not reflect normal play; project requirements forbid corrupting normal state without proof. | A Tokkun-only handler that returns success and optionally stores observations. |
| Runtime wiki scraping | Wiki is only gameplay context and not protocol truth. | Local proto, current code, logs, IDA daemon, and RPCS3/cabinet traffic. |
| Hardcoded `wwwroot/data/blue` filesystem access | Tokkun support does not need new server catalog data; if it ever does, use existing path helpers/catalog services. | `IGameDataCatalog.For(GameEra.Blue)` and Blue catalog/path helpers. |
| Adding `/v10r03/chassis/getbanacoininfo.php` preemptively | Existing route tests exclude it and IDA route probing did not find the route string. | Keep absent until logs or deeper IDA evidence prove a call. |

## Evidence Still Needed

| Question | Why It Matters | Required Evidence |
|----------|----------------|-------------------|
| Exact Tokkun `PlayMode` value | Needed for robust classification when `ary_tokkunstage_info` is absent or partial. | RPCS3/cabinet `playresult.php` logs and/or IDA trace from `GameTokkunMode` result submission. |
| Exact Tokkun `StageMode` values | Needed to avoid accidentally treating Tokkun as normal/Shin and writing best/crown rows. | Tokkun playresult logs, then source guard tests. |
| Whether `balancecheck.php` `coin_coupon=0` blocks Tokkun selection | Current stub may be too strict for "always playable." | RPCS3 menu selection logs with current stub; if blocked, prove accepted stateless value. |
| Whether `getbanacoininfo.php` is called | Generated message exists, but route string was not found in the stack IDA probe and current route guard excludes it. | HTTP logs or a focused IDA route-builder xref. |
| Whether `UserDataResponse.tokkun_tutorial_flg` is required | Current `UserDataMappers` omits it by test. | First-run and second-run Tokkun cabinet/RPCS3 traces. |
| Whether Tokkun summary must be persisted or only logged | The proto sends summary fields, but no readback path is proven. | Compare post-Tokkun `userdata.php`/`baidcheck.php` behavior and client re-entry behavior. |

## Installation and Commands

No new runtime packages are recommended.

```powershell
# Verify current stack
dotnet build TaikoLocalServer.slnx
dotnet test Tests/Tests.csproj --filter Blue

# If Host bin/obj is locked by a running server
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"

# If a migration is needed for proven Tokkun state
dotnet ef migrations add AddBlueTokkunState --project Infrastructure --startup-project Host
dotnet ef database update --project Infrastructure --startup-project Host

# If Blue proto changes require regeneration
protogen --csharp_out=Adapters.GameProtocol.Blue/Wire --proto_path=proto/blue taiko.proto
Move-Item -Force 'Adapters.GameProtocol.Blue/Wire/taiko.cs' 'Adapters.GameProtocol.Blue/Wire/Game.cs'
```

## Alternatives Considered

| Recommended | Alternative | Why Not |
|-------------|-------------|---------|
| Blue-owned `HandleBlueTokkun` path | Extend normal `HandleBlue` logic with conditionals | Too easy to leak score, crown, unlock, favorite, or Dan side effects into Tokkun. |
| Stateless Banacoin stubs | Persist balance/payment data | Contradicts project scope and user note. |
| Optional Tokkun persistence | Store every raw request permanently | Heavyweight and privacy/noise-prone; only summary fields are currently visible in proto. |
| Keep `getbanacoininfo.php` absent until proven | Add it because message types exist | Proto messages alone do not prove a cabinet-called route. |
| Use existing protobuf-net generated types | Introduce another protobuf generator | Current generated model already has the fields and is integrated into ASP.NET Core serialization. |

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Existing stack versions | HIGH | Verified from `global.json`, `Directory.Build.props`, and `Directory.Packages.props`. |
| Tokkun wire fields | HIGH | Verified in `proto/blue/taiko.proto` and `Adapters.GameProtocol.Blue/Wire/Game.cs`. |
| Tokkun exists in Blue binary | HIGH | IDA daemon probe found `GameTokkunMode`, Tokkun assets, and Tokkun playresult/proto strings. |
| Banacoin endpoint surface | MEDIUM | Existing routes and IDA route strings confirm balance/payment/error; `getbanacoininfo.php` remains unproven as a route. |
| Exact runtime constants and side effects | LOW until capture | Need Tokkun RPCS3/cabinet playresult logs and/or deeper IDA tracing. |

## Sources

- `.planning/PROJECT.md` - v1.1 scope, constraints, and out-of-scope Banacoin state.
- `.planning/STATE.md` - current milestone status.
- `proto/blue/taiko.proto` - Tokkun, PlayResult, UserData, Balancecheck, and Banacoin message fields.
- `Adapters.GameProtocol.Blue/Wire/Game.cs` - generated Blue protobuf model.
- `Domain/Enums/PlayMode.cs` - current known play modes, no Tokkun constant yet.
- `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs` - direct protobuf playresult dispatch.
- `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` - current mapper drops Tokkun fields.
- `Application/Dtos/CommonPlayResultData*.cs` - current common DTO shape.
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs` and `.BlueBattle.cs` - normal/battle branching and side effects.
- `Adapters.GameProtocol.Blue/Controllers/BalanceCheckController.cs`, `BanacoinPaymentController.cs`, `BanacoinErrorLogController.cs`, `HeartbeatController.cs` - current Banacoin-adjacent stubs.
- `Tests/Blue/BluePlayResultMapperTests.cs`, `BlueRouteSkeletonTests.cs`, `BlueA4SourceGuardTests.cs`, `BlueMapperTests.cs` - current guardrails that Tokkun work must update.
- `.tools/blue/scratch/req_tokkun_stack_20260603_codex.py` via `python .tools/blue/idadrv.py run req-tokkun-stack-20260603-codex ...` - IDA daemon string/name probe.
- Wiki gameplay context: <https://wikiwiki.jp/taiko-fumen/%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0/%E5%9F%BA%E6%9C%AC%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0#tokkunmode> - used only for high-level Tokkun gameplay, Banacoin price context, and non-record/reward behavior.

---
*Stack research for: Blue Tokkun mode support*
*Researched: 2026-06-03*
