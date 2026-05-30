---
phase: 05
slug: blue-battle-runtime-support
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-05-30
---

# Phase 05 - Validation Strategy

Per-phase validation contract for feedback sampling during Blue battle runtime execution.

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.9.3 via `dotnet test` |
| **Config file** | `Tests/Tests.csproj` |
| **Quick run command** | `dotnet test Tests/Tests.csproj --filter <phase-specific-filter>` |
| **Full suite command** | `dotnet test Tests/Tests.csproj` |
| **Estimated runtime** | Focused filters: under 90 seconds; full suite depends on local machine state |

## Sampling Rate

- **After every task commit:** Run the focused `dotnet test Tests/Tests.csproj --filter <phase-specific-filter>` command named in the PLAN.md task.
- **After every plan wave:** Run `dotnet test Tests/Tests.csproj`.
- **Before `$gsd-verify-work`:** Run `dotnet test Tests/Tests.csproj` and `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase05"`.
- **Max feedback latency:** No three consecutive implementation tasks may pass without an automated focused test, source guard, or explicit manual-gate artifact.

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 05-01-T1 | 05-01 | 1 | BTL-01..BTL-06 | T-05-01 | Row-resolution matrix blocks unresolved Phase 4 missing-evidence rows. | docs/unit | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattleEvidenceGateTests` | pending | pending |
| 05-02-T1 | 05-02 | 2 | BTL-02, BTL-03, BTL-06 | T-05-02 | Battleuserdata and initialdata emissions stay row-gated. | blocking checkpoint | PowerShell row-approval source check in `05-02-PLAN.md` | pending | pending |
| 05-03-T1 | 05-03 | 3 | BTL-04, BTL-05, BTL-06 | T-05-03 | Reward, progression, token, stage, boss-life, and unlock effects stay row-gated. | blocking checkpoint | PowerShell row-approval source check in `05-03-PLAN.md` | present | green |
| 05-04-T1 | 05-04 | 2 | BTL-01, BTL-06 | T-05-04 | Blue battle entity/DbContext shape is Blue-owned and nullable/raw where unresolved. | unit/source guard | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattlePersistenceShapeTests` | present | green |
| 05-05-T1 | 05-05 | 3 | BTL-01, BTL-06 | T-05-05 | BlueBattle* migration persists battle state without touching normal Blue or Green state. | migration/integration | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattlePersistenceTests` | present | green |
| 05-06-T1 | 05-06 | 2 | BTL-01, BTL-06 | T-05-06 | Raw battle XML inventory cannot derive runtime battle behavior. | loader/source guard | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattleCatalogLoaderTests` | present | green |
| 05-07-T1 | 05-07 | 4 | BTL-02, BTL-06 | T-05-07 | `battleuserdata.php` emits only row-approved persisted fields. | mapper/controller tests | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattleUserDataTests` | pending | pending |
| 05-08-T1 | 05-08 | 5 | BTL-03, BTL-06 | T-05-08 | `initialdatacheck.php` battle fields are omitted or exact-row approved. | mapper/query tests | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueInitialDataTests` | partial | pending |
| 05-09-T1 | 05-09 | 2 | BTL-04, BTL-06 | T-05-09 | Battle playresult mapping triggers only from Blue battle sections and preserves raw values. | mapper tests | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattlePlayResultMapperTests` | present | green |
| 05-10-T1 | 05-10 | 4 | BTL-04, BTL-05, BTL-06 | T-05-10 | Battle playresults bypass normal Blue state and raw-capture only approved battle state. | handler/integration tests | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattlePlayResultHandlerTests` | pending | pending |
| 05-11-T1..T3 | 05-11 | 6 | BTL-01..BTL-06 | T-05-11 | Source guards, requirement traceability, focused tests, full tests, and Host temp build close the phase. | source guard/full regression | `dotnet test Tests/Tests.csproj --filter BlueBattle`; `dotnet test Tests/Tests.csproj`; `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase05"` | pending | pending |

Status values: pending, green, red, flaky.

## Wave 0 Requirements

- [ ] Add or extend Blue battle focused tests before runtime tasks emit new battle values.
- [ ] Add source guards proving Blue battle code does not depend on Green AI Battle implementation truth.
- [ ] Add row-resolution artifact checks so 05-02 and 05-03 cannot silently use unresolved Phase 4 `MISSING_EVIDENCE` rows.
- [ ] Preserve existing xUnit infrastructure; no new test framework is needed.

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Battle menu entry and full battle flow smoke | FULL-01, Phase 5 closeout input | Server tests cannot prove real cabinet/RPCS3 client timing, menu availability, or battle flow acceptance. | Capture date, enabled eras, Blue data path, observed endpoints, request/response logs, and pass/fail notes during cabinet/RPCS3 battle smoke. |
| Remaining IDA/log/cabinet field evidence | BTL-02, BTL-03, BTL-05 | Most battleuserdata defaults, token/reward semantics, boss-life, and stage completion rules remain unresolved by current research. | Record proof or named approval in the Phase 5 resolution matrix before implementing any gated behavior. |

## Validation Sign-Off

- [ ] All tasks have automated verify commands or explicit manual-gate artifacts.
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify.
- [ ] Wave 0 covers all missing row-resolution references before gated runtime emission.
- [ ] No watch-mode flags.
- [ ] Focused feedback latency remains bounded by task-local filters.
- [ ] `nyquist_compliant: true` set in frontmatter after plans define concrete task filters and Wave 0 evidence.

**Approval:** pending
