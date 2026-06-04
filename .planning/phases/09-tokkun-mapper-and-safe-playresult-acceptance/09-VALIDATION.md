---
phase: 09
slug: tokkun-mapper-and-safe-playresult-acceptance
status: draft
nyquist_compliant: true
wave_0_complete: false
created: 2026-06-05
---

# Phase 09 - Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit via `Tests/Tests.csproj` |
| **Config file** | `Tests/Tests.csproj` |
| **Quick run command** | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultMapperTests|FullyQualifiedName~BluePlayResultHandlerTests"` |
| **Broader phase command** | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultMapperTests|FullyQualifiedName~BluePlayResultHandlerTests|FullyQualifiedName~BlueBattlePlayResultMapperTests|FullyQualifiedName~BlueBattlePlayResultHandlerTests"` |
| **Build command** | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase9"` |
| **Estimated runtime** | Focused tests depend on local build cache; research baseline reported 28 focused Blue playresult tests passing |

## Sampling Rate

- **After every task commit touching mapper behavior:** Run `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultMapperTests"`.
- **After every task commit touching handler behavior:** Run `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultHandlerTests"`.
- **After every plan wave:** Run `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultMapperTests|FullyQualifiedName~BluePlayResultHandlerTests|FullyQualifiedName~BlueBattlePlayResultMapperTests|FullyQualifiedName~BlueBattlePlayResultHandlerTests"`.
- **Before `$gsd-verify-work`:** Run the broader phase command plus `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase9"`, or explicitly record why either could not run.
- **Build fallback:** Use a unique temp-output Host build if a running server locks `Host/bin/Debug/net10.0`.

## Requirement Verification Map

| Requirement | Behavior | Test Type | Automated Command | File Exists | Status |
|-------------|----------|-----------|-------------------|-------------|--------|
| TKPR-01 | Mapper classifies Tokkun from non-null `AryTokkunstageInfo`, does not classify tutorial-only payloads, preserves `tokkun_tutorial_flg` optional presence/value, and preserves all raw `TokkunstageData` facts. | unit mapper | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultMapperTests"` | Yes, extend existing file | pending |
| TKPR-02 | Existing-user Tokkun upload returns `1` and leaves normal score/best, Dani, battle, favorite/recent, profile counters, unlock flags, medal totals, customization/title, shop, and Banacoin-like state unchanged. | integration handler with SQLite fixture | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultHandlerTests"` | Yes, extend existing file | pending |
| TKPR-03 | Unknown-user Tokkun and mixed Tokkun-plus-battle/normal-looking payloads return `1`, route through the Tokkun branch, and do not call battle or normal write helpers. | integration handler with SQLite fixture | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultHandlerTests"` | Yes, extend existing file | pending |

## Wave 0 Requirements

- [ ] `Application/Dtos/CommonPlayResultData.BlueTokkun.cs` adds Blue Tokkun classifier and raw fact DTO fields without changing generated `Wire/` files.
- [ ] `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs` maps Tokkun optional presence/value and raw `TokkunstageData` facts while keeping `tokkun_tutorial_flg` out of the classifier.
- [ ] `Application/Handlers/UpdatePlayResultCommand.Blue.cs` or a Blue Tokkun partial inserts the Tokkun success branch after guest/unknown-user exits and before battle/normal writes.
- [ ] `Tests/Blue/BluePlayResultMapperTests.cs` and `Tests/Blue/BluePlayResultHandlerTests.cs` prove behavior through real mapper and handler execution, not source-text scans.

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Cabinet/RPCS3 Tokkun selection, route sequence, gameplay entry, final upload, and post-upload userdata behavior | TKVF-02 | Deferred to Phase 11 by roadmap scope | Do not claim live Tokkun proof in Phase 9. Record that Phase 9 verification is automated source/test/build evidence only. |
| Numeric Tokkun `play_mode` value | TKPR-01 | Current contract forbids guessing `PlayMode.Tokkun` before cabinet/RPCS3 logs or deeper IDA evidence prove it | Do not add `PlayMode.Tokkun`; preserve/log raw `PlayMode` only as context if needed. |
| Tokkun persistence/readback semantics | TKST-01, TKST-02, TKST-03, TKST-04 | Deferred to Phase 10 by roadmap scope | Phase 9 maps raw facts and returns success only; no EF entities, migrations, AdminApi/WebUI, or userdata readback changes. |

## Validation Sign-Off

- [ ] All plans include automated mapper or handler verification steps.
- [ ] Sampling continuity: every mapper/handler task runs the focused xUnit class it touches before completion.
- [ ] Wave 0 covers missing DTO, mapper, handler branch, and behavior-test work.
- [ ] No source-scanning Tokkun word guards are reintroduced.
- [ ] No watch-mode flags.
- [ ] `nyquist_compliant: true` remains set in frontmatter.

**Approval:** pending
