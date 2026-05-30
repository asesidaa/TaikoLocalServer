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
| 05-01-W0 | 05-01 | 0 | BTL-01, BTL-06 | T-05-01 | Blue battle state is persisted only in Blue-owned tables and never Green AI Battle tables. | unit/source guard | `dotnet test Tests/Tests.csproj --filter BlueBattle` | pending | pending |
| 05-02-W0 | 05-02 | 0 | BTL-02, BTL-03 | T-05-02 | Unresolved battleuserdata and initialdata fields remain omitted or gated until evidence/user approval exists. | mapper/controller tests | `dotnet test Tests/Tests.csproj --filter BlueBattle` | pending | pending |
| 05-03-W0 | 05-03 | 0 | BTL-04, BTL-05 | T-05-03 | Battle playresults bypass normal Blue score, crown, history, recent/favorite, profile counter, self-best, and Dani writes. | handler/integration tests | `dotnet test Tests/Tests.csproj --filter BlueBattle` | pending | pending |
| 05-04-W0 | 05-04 | 0 | BTL-01..BTL-06 | T-05-04 | Source guards reject Green AI Battle implementation truth and unsafe default inference. | source guard/full regression | `dotnet test Tests/Tests.csproj` | pending | pending |

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
