---
phase: 04
slug: blue-battle-evidence-and-design
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-05-30
---

# Phase 04 - Validation Strategy

Per-phase validation contract for feedback sampling during execution.

## Test Infrastructure

| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 with Microsoft.NET.Test.Sdk 17.14.1 |
| Config file | `Tests/Tests.csproj` |
| Quick run command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueInitialDataTests|FullyQualifiedName~BluePlayResultMapperTests|FullyQualifiedName~BlueRouteSkeletonTests|FullyQualifiedName~BlueDocsTests"` |
| Full suite command | `dotnet test Tests/Tests.csproj` |
| Host build command | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-battle-evidence"` |
| Estimated runtime | Quick target under 90 seconds; full suite and Host build depend on local machine state |

## Sampling Rate

- After every evidence/design task commit: run deterministic checklist, grep, or doc-test validation for the artifact touched by the task.
- After any source or test change: run the quick Blue-focused test command above.
- After every plan wave: run `dotnet test Tests/Tests.csproj` if source or test files changed; docs-only waves may use checklist validation until closeout.
- Before phase verification: confirm BTEV-01 through BTEV-05 evidence coverage, BTEV-06 gate status, no Phase 4 runtime battle implementation files, and run the full suite plus temp-output Host build if code/tests changed.
- Max feedback latency target: one targeted validation command per task.

## Per-Requirement Verification Map

| Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| BTEV-01 | T-01 | Battle menu entry and attempted battle flow have equivalent client, IDA, or trace evidence with route ownership recorded. | evidence review | `Select-String -Path .planning/phases/04-blue-battle-evidence-and-design/* -Pattern "battleuserdata.php","initialdatacheck.php","playresult.php"` | No - Wave 0 | pending |
| BTEV-02 | T-02 | Blue battle proto fields are mapped to generated wire types and response/request owners without Green AI Battle semantics. | doc/source guard | `Select-String -Path .planning/phases/04-blue-battle-evidence-and-design/* -Pattern "InitialdatacheckResponse","BattleUserDataResponse","BattleStageData","ReleaseBattleData"` | No - Wave 0 | pending |
| BTEV-03 | T-03 | All five local `config/S10100-1/battle` XML files are inventoried with row counts, hashes, and required/optional/unknown classification. | local data audit | `Get-ChildItem Host/wwwroot/data/blue/data/config/S10100-1/battle -Filter *.xml | Select-Object Name,Length` | No - Wave 0 | pending |
| BTEV-04 | T-04 | Every battle byte width, default, repeated-row count, NPC/token field, stage assignment, boss, and last-stage field is proven or explicitly user-approved. | evidence matrix | `Select-String -Path .planning/phases/04-blue-battle-evidence-and-design/* -Pattern "UNKNOWN","APPROVED_WITH_USER_EXCEPTIONS","release_info_flg","npc_costume_flg"` | No - Wave 0 | pending |
| BTEV-05 | T-05 | Battle score/crown/history effects are separated from normal Blue records, with unlock mirrors documented only where approved. | design review | `Select-String -Path .planning/phases/04-blue-battle-evidence-and-design/* -Pattern "normal crowns","normal scores","ReleaseBattleData","unlock"` | Partial - existing mapper guard only | pending |
| BTEV-06 | T-06 | Phase 5 remains blocked unless BTEV-01 through BTEV-05 pass or each exception has explicit user approval. | gate review | `Select-String -Path .planning/phases/04-blue-battle-evidence-and-design/* -Pattern "APPROVED","BLOCKED","APPROVED_WITH_USER_EXCEPTIONS"` | No - Wave 0 | pending |

## Threat Model References

| Threat | Requirement | Mitigation To Verify |
|--------|-------------|----------------------|
| T-01: Phase 4 treats missing battle route evidence as sufficient | BTEV-01 | Evidence pack lists battle route sequence, request/response ownership, and the concrete source for each claim. |
| T-02: Proto or generated wire fields are missed before Phase 5 | BTEV-02 | Field matrix names `InitialdatacheckResponse`, `BattleUserDataResponse`, `BattleStageData`, and `ReleaseBattleData`. |
| T-03: Local XML candidate roles become runtime truth | BTEV-03 | Inventory marks each battle XML file as candidate data and records required/optional/unknown status separately from semantics. |
| T-04: Unsafe byte/default guesses enter Phase 5 plans | BTEV-04 | Gate fails on any required field left `UNKNOWN` without explicit user-approved exception. |
| T-05: Battle playresult corrupts normal Blue score/crown/history state | BTEV-05 | Design states battle progress is battle-owned and normal records are not updated except approved unlock mirrors. |
| T-06: Battle implementation starts before evidence gate closes | BTEV-06 | Final gate artifact records `APPROVED`, `BLOCKED`, or `APPROVED_WITH_USER_EXCEPTIONS` before Phase 5. |

## Wave 0 Requirements

- [ ] `04-01` evidence pack or equivalent artifact covering battle route sequence, proto/wire ownership, and static client/IDA evidence for battle menu entry.
- [ ] `04-02` XML/default-width analysis artifact covering all five battle XML files, hashes/counts, candidate relationships, and byte/default proof matrix.
- [ ] `04-03` design spec and implementation gate artifact with final Phase 5 input table and approved/blocking status.
- [ ] Optional `Tests/Blue/BlueBattleEvidenceTests.cs` or equivalent doc guard if the executor chooses automated evidence completeness checks.
- [ ] Source guard or checklist proving Phase 4 did not add committed runtime battle loaders, generated battle JSON, EF migrations, or battle behavior implementation.

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Static client/IDA battle flow evidence review | BTEV-01, BTEV-04 | IDA/client evidence interpretation is not fully automatable from current tooling because IDA executable is not on PATH and source truth may come from existing artifacts or user notes. | Record source artifact, date, route or field observed, exact field/default claim, and whether it is proven, unknown, or user-approved. |
| Phase 5 implementation gate approval | BTEV-06 | The context requires case-by-case user approval for unresolved fields/defaults. | Review final gate artifact and record `APPROVED`, `BLOCKED`, or `APPROVED_WITH_USER_EXCEPTIONS` with each exception named explicitly. |

## Validation Sign-Off

- [ ] All tasks have automated validation commands or Wave 0 dependencies.
- [ ] Sampling continuity: no three consecutive evidence/design tasks lack validation.
- [ ] Wave 0 covers all missing evidence artifacts above.
- [ ] No watch-mode flags in verification commands.
- [ ] Full suite and temp-output Host build are required before phase verification if code/tests changed.
- [ ] Phase 4 contains no runtime battle loaders, generated battle JSON, EF migrations, or battle behavior implementation.
- [ ] Set `wave_0_complete: true` after Wave 0 evidence artifacts exist.
- [ ] Set `nyquist_compliant: true` after all per-requirement checks are implemented and passing.

Approval: pending
