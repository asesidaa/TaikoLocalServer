---
phase: 10
slug: evidence-backed-tokkun-state-persistence-and-readback
status: draft
nyquist_compliant: true
wave_0_complete: false
created: 2026-06-06
---

# Phase 10 - Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit via `Tests/Tests.csproj` |
| **Config file** | `Tests/Tests.csproj` |
| **Quick run command** | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultMapperTests|FullyQualifiedName~BluePlayResultHandlerTests|FullyQualifiedName~BlueUserDataTests|FullyQualifiedName~BlueMapperTests"` |
| **Broader phase command** | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultMapperTests|FullyQualifiedName~BluePlayResultHandlerTests|FullyQualifiedName~BlueUserDataTests|FullyQualifiedName~BlueMapperTests|FullyQualifiedName~BlueTokkunPersistenceTests|FullyQualifiedName~BlueTokkunPersistenceShapeTests"` |
| **Build command** | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase10"` |
| **Estimated runtime** | Focused Blue suites depend on local build cache; use class-level filters after each task. |

## Sampling Rate

- **After mapper/classifier task commits:** Run `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultMapperTests"`.
- **After persistence/schema task commits:** Run `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueTokkunPersistenceTests|FullyQualifiedName~BlueTokkunPersistenceShapeTests"`.
- **After handler task commits:** Run `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultHandlerTests"`.
- **After userdata readback task commits:** Run `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueMapperTests|FullyQualifiedName~BlueUserDataTests"`.
- **After every plan wave:** Run the broader phase command.
- **Before `$gsd-verify-work`:** Run the broader phase command plus the Host temp-output build.
- **Build fallback:** Use a unique temp-output Host build if a running server locks `Host/bin/Debug/net10.0`.

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 10-01-01 | 01 | 1 | TKST-01, TKST-02, TKST-03, TKST-04 | T-10-01 / T-10-02 | Classifier uses proven `play_mode = 3`; storage schema is Blue-owned and raw-fact only. | mapper + source shape | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultMapperTests|FullyQualifiedName~BlueTokkunPersistenceShapeTests"` | Partial; extend/create files | pending |
| 10-01-02 | 01 | 1 | TKST-02, TKST-03, TKST-04 | T-10-03 | EF schema persists/reloads raw Tokkun facts and does not create Green/normal/battle/shop/Banacoin state. | SQLite schema reload | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueTokkunPersistenceTests"` | Create file | pending |
| 10-02-01 | 02 | 2 | TKST-01, TKST-02, TKST-03, TKST-04 | T-10-01 / T-10-04 | Tokkun uploads append allowed state only and preserve Phase 9 no-cross-write guarantees. | handler integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultHandlerTests"` | Yes, extend file | pending |
| 10-03-01 | 03 | 3 | TKST-01 | T-10-02 | Userdata omits absent tutorial values and serializes raw nullable values when persisted. | mapper + query integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueMapperTests|FullyQualifiedName~BlueUserDataTests"` | Yes, extend files | pending |

## Wave 0 Requirements

- [ ] `Domain/Enums/PlayMode.cs` includes `Tokkun = 3`.
- [ ] `Domain/Entities/UserSaveDataBlue.cs` includes nullable raw `TokkunTutorialFlg`.
- [ ] `Domain/Entities/BlueTokkunStageResult.cs` exists with only protocol-backed summary/history fields.
- [ ] `ITaikoDbContext.Blue.cs` and `TaikoDbContext.Blue.cs` expose/configure the Blue Tokkun history DbSet.
- [ ] EF migration `AddBlueTokkunState` updates `UserSaveData_Blue` and creates only the Blue Tokkun history table.
- [ ] Tests prove classifier, schema, handler persistence, userdata readback, and no-cross-write behavior through runtime execution.

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Cabinet/RPCS3 Tokkun selection, upload, post-upload userdata behavior, and unexpected endpoint calls | TKVF-02 | Deferred to Phase 11 by roadmap and D-14 | Do not claim final live smoke proof in Phase 10. Record automated source/test/build proof only. |
| Additional unnamed Tokkun log fields | D-03 | User reported extra fields but no concrete committed field list was available in Phase 10 context | Do not store unnamed fields. If an exact log/proto field list appears before execution, inspect it before extending storage. |

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify commands.
- [ ] Sampling continuity: no three consecutive implementation tasks lack automated verification.
- [ ] Wave 0 covers new persistence test files.
- [ ] No watch-mode flags.
- [ ] No source-word Tokkun guard is added.
- [ ] Feedback latency remains class-filtered for task commits.
- [ ] `nyquist_compliant: true` remains set in frontmatter.

**Approval:** pending
