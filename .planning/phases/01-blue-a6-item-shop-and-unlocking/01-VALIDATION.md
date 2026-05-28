---
phase: 01
slug: blue-a6-item-shop-and-unlocking
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-05-29
---

# Phase 01 - Validation Strategy

Per-phase validation contract for feedback sampling during execution.

## Test Infrastructure

| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 with Microsoft.NET.Test.Sdk 17.14.1 |
| Config file | `Tests/Tests.csproj` |
| Quick run command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueItemShop|FullyQualifiedName~BlueA6|FullyQualifiedName~Ac15"` |
| Full suite command | `dotnet test Tests/Tests.csproj` |
| Host build command | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-a6"` |
| Estimated runtime | Quick target under 90 seconds; full suite and Host build depend on local machine state |

## Sampling Rate

- After every task commit: run the most specific new or existing test file for the touched layer.
- After every plan wave: run `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Blue|FullyQualifiedName~Ac15|FullyQualifiedName~GreenItemShop"`.
- Before phase verification: run `dotnet test Tests/Tests.csproj` and `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-a6"`.
- Max feedback latency target: one targeted test command per implementation task.

## Per-Requirement Verification Map

| Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| SHOP-01 | T-01 | Blue `initialdatacheck.php` advertises shop data only when enabled and active season rows exist. | unit | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueItemShopProtocolTests` | No - Wave 0 | pending |
| SHOP-02 | T-02 | Blue `getitemshopinfo.php` returns only the active Blue season and ordered protocol item rows. | unit/controller | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueItemShopProtocolTests` | No - Wave 0 | pending |
| SHOP-03 | T-03 | Enabled Blue shop fails fast for missing or invalid `blue_item_shop_data.json`, and defaults are parser-proven from the local cache. | unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueItemShopLoaderTests|FullyQualifiedName~BlueRewardShopDataParserTests"` | No - Wave 0 | pending |
| SHOP-04 | T-04 | Purchase validates `item_no`, `item_type`, `item_id`, and `item_price` before spend or unlock. | unit/mapper | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueItemShopPurchaseTests` | No - Wave 0 | pending |
| SHOP-05 | T-05 | Blue shop medal totals persist by BAID and Blue season without Green tables or global Blue medal totals. | unit/integration | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueItemShopStateTests` | No - Wave 0 | pending |
| SHOP-06 | T-06 | `itempurchase.php` applies supported Blue unlock bits; `rewardexecution.php` logs success without mutation for Phase 1. | unit/controller | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueItemShopPurchaseTests|FullyQualifiedName~BlueRewardExecution"` | No - Wave 0 | pending |
| SHOP-07 | T-07 | Blue BAID and userdata hide active-season locked songs, tones, and costume slots until purchased. | unit | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueItemShopLockingTests` | No - Wave 0 | pending |
| SHOP-08 | T-08 | Blue item-shop code has no Green shop state, Green protocol constants, or Green wire model dependencies. | source guard | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueA6SourceGuardTests` | No - Wave 0 | pending |

## Threat Model References

| Threat | Requirement | Mitigation To Verify |
|--------|-------------|----------------------|
| T-01: Disabled or empty shop is advertised to client | SHOP-01 | Active season and row-count guards suppress advertisement. |
| T-02: Wrong season or item ordering reaches client | SHOP-02 | Shop-info query maps only active-season rows and stable item numbers. |
| T-03: Malformed or partial official cache data becomes committed defaults | SHOP-03 | Parser tests prove full cache-to-JSON output or block implementation for discussion. |
| T-04: Forged purchase tuple spends medals or unlocks items | SHOP-04 | Handler compares request tuple to active catalog row before mutation. |
| T-05: Blue shop accounting leaks into Green or global Blue totals | SHOP-05 | Blue-owned tables and state helpers are the only shop accounting path. |
| T-06: Rewardexecution mutates shop state contrary to D-02 | SHOP-06 | Controller/handler tests assert success no-op semantics. |
| T-07: Locked shop content leaks through BAID or userdata | SHOP-07 | Readback tests assert hidden then purchased-visible bits. |
| T-08: Green implementation is reused as truth | SHOP-08 | Source guard scans A6 Blue files for forbidden Green references. |

## Wave 0 Requirements

- [ ] `Tests/Blue/BlueRewardShopDataParserTests.cs` - official cache parser/default JSON proof.
- [ ] `Tests/Blue/BlueItemShopLoaderTests.cs` - Blue-specific default JSON and invalid-data checks.
- [ ] `Tests/Blue/BlueItemShopStateTests.cs` - zero-start Blue season state and enabled/disabled medal behavior.
- [ ] `Tests/Blue/BlueItemShopProtocolTests.cs` - initialdata, getitemshopinfo, and mapper optional-field behavior.
- [ ] `Tests/Blue/BlueItemShopPurchaseTests.cs` - preflight, validation, duplicate, insufficient balance, and immediate unlock behavior.
- [ ] `Tests/Blue/BlueItemShopLockingTests.cs` - BAID/userdata lock and unlock readback.
- [ ] `Tests/Blue/BlueA6SourceGuardTests.cs` - A6 Green dependency guard.

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Cabinet/RPCS3 Blue item-shop smoke | SHOP-01 through SHOP-07 | Phase 1 excludes full cabinet smoke; Phase 3 owns repeatable normal-mode smoke evidence. | If opportunistically tested, record date, enabled eras, Blue data path, endpoint log snippets, purchase request/response, and observed client unlock/readback behavior. |

## Validation Sign-Off

- [ ] All tasks have automated verify commands or Wave 0 dependencies.
- [ ] Sampling continuity: no three consecutive implementation tasks lack automated verification.
- [ ] Wave 0 covers all missing test files above.
- [ ] No watch-mode flags in verification commands.
- [ ] Full suite and temp-output Host build are required before phase verification.
- [ ] Set `wave_0_complete: true` after Wave 0 tests exist.
- [ ] Set `nyquist_compliant: true` after all per-requirement tests are implemented and passing.

Approval: pending
