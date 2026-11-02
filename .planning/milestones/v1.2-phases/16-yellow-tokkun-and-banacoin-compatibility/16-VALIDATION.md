---
phase: 16
slug: yellow-tokkun-and-banacoin-compatibility
status: draft
nyquist_compliant: true
wave_0_complete: true
created: 2026-06-08
---

# Phase 16 - Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit via `dotnet test` |
| **Config file** | `Tests/Tests.csproj` |
| **Quick run command** | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowTokkun|FullyQualifiedName~YellowPlayResult|FullyQualifiedName~YellowUserData|FullyQualifiedName~YellowRouteSkeleton"` |
| **Full suite command** | `dotnet test Tests/Tests.csproj` |
| **Build command** | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase16-yellow"` |

## Sampling Rate

- **After every task commit:** run that task's focused filter.
- **After every wave:** run `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowTokkun|FullyQualifiedName~YellowPlayResult|FullyQualifiedName~YellowUserData|FullyQualifiedName~YellowPersistenceBoundary|FullyQualifiedName~YellowRouteSkeleton|FullyQualifiedName~YellowBanacoin"`.
- **Before phase verification:** run the full test project and the temp-output Host build.
- **Max feedback latency:** one task.

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 16-01-01 | 01 | 1 | YTOK-01 | T-16-01 | Yellow mapper/classifier preserves Tokkun facts and does not use tutorial-only classification | mapper | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowPlayResult"` | yes | pending |
| 16-01-02 | 01 | 1 | YTOK-02 | T-16-02 | Yellow Tokkun schema is Yellow-owned and raw-fact-only | EF/schema | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowTokkun|FullyQualifiedName~YellowPersistenceBoundary"` | partial | pending |
| 16-04-01 | 04 | 1 | YBAN-01 | T-16-09 | Banacoin routes log full requests and stay stateless | route/source | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowBanacoin|FullyQualifiedName~YellowRouteSkeleton|FullyQualifiedName~YellowPersistenceBoundary"` | partial | pending |
| 16-02-01 | 02 | 2 | YTOK-01/YTOK-02 | T-16-03 | Classified Tokkun uploads write only tutorial/history state | handler/EF | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowTokkun|FullyQualifiedName~YellowPlayResult"` | partial | pending |
| 16-02-02 | 02 | 2 | YTOK-01 | T-16-04 | Mixed Tokkun payloads do not write normal, Dan, shop, medal, profile, favorite, recent, unlock, battle, or cross-era state | handler/boundary | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowTokkun|FullyQualifiedName~YellowPlayResult|FullyQualifiedName~YellowPersistenceBoundary"` | partial | pending |
| 16-03-01 | 03 | 3 | YTOK-03 | T-16-05 | Yellow userdata maps optional tutorial flag only when persisted | mapper/service | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowUserData|FullyQualifiedName~Ac15UserData"` | yes | pending |
| 16-03-02 | 03 | 3 | YTOK-03 | T-16-06 | Playresult persistence flows to userdata tutorial readback with no history exposure | integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowTokkun|FullyQualifiedName~YellowUserData|FullyQualifiedName~YellowPlayResult"` | partial | pending |

## Wave 0 Requirements

Existing test infrastructure covers Phase 16. New focused files are part of implementation tasks, not a separate Wave 0.

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Yellow normal/Tokkun cabinet or RPCS3 smoke | YVER-03 | Deferred by roadmap to Phase 17 | Do not run in Phase 16. Phase 17 records runtime smoke evidence. |

## Validation Sign-Off

- [x] All tasks have automated verify commands.
- [x] Sampling continuity: no three consecutive tasks lack automated verify.
- [x] Wave 0 covers all missing references.
- [x] No watch-mode flags.
- [x] Feedback latency is one task.
- [x] `nyquist_compliant: true` set in frontmatter.

**Approval:** approved 2026-06-08

## VALIDATION COMPLETE
