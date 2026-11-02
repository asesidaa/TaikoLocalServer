---
phase: 15
slug: yellow-dani-shop-medals-waiwai-and-admin
status: draft
nyquist_compliant: true
wave_0_complete: true
created: 2026-06-08
---

# Phase 15 - Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit via `dotnet test` |
| **Config file** | `Tests/Tests.csproj` |
| **Quick run command** | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Yellow"` |
| **Full suite command** | `dotnet test Tests/Tests.csproj` |
| **Estimated runtime** | ~120 seconds for focused Yellow, broader for full suite |

## Sampling Rate

- **After every task commit:** Run the task's focused filter.
- **After every plan wave:** Run `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Yellow|FullyQualifiedName~ItemShop|FullyQualifiedName~Dan|FullyQualifiedName~WebUi"`.
- **Before phase verification:** Run `dotnet test Tests/Tests.csproj` and `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase15-yellow"`.
- **Max feedback latency:** one task.

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 15-01-01 | 01 | 1 | YDAN-01 | T-15-01 | Yellow Dan schema is Yellow-owned | EF/unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowDani|FullyQualifiedName~YellowPersistenceBoundary"` | yes | pending |
| 15-01-02 | 01 | 1 | YDAN-01 | T-15-01 | Yellow Dan helper rules are Yellow-owned | unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowDani"` | yes | pending |
| 15-02-01 | 02 | 2 | YDAN-01 | T-15-02 | Dan playresults write Yellow Dan state only | handler | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowDani|FullyQualifiedName~YellowPlayResult"` | yes | pending |
| 15-02-02 | 02 | 2 | YDAN-01 | T-15-03 | Userdata/Taikojuku readback sees Yellow Dan state | protocol | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowDani|FullyQualifiedName~YellowUserData|FullyQualifiedName~YellowTaikojuku"` | yes | pending |
| 15-03-01 | 03 | 2 | YSHOP-02/YMED-01 | T-15-04 | Yellow shop state is Yellow-owned and not Banacoin | EF/unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowItemShop|FullyQualifiedName~YellowPersistenceBoundary"` | yes | pending |
| 15-03-02 | 03 | 2 | YSHOP-02/YMED-01 | T-15-04 | Yellow active shop seasons seed Don medal balances | unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowItemShop"` | yes | pending |
| 15-04-01 | 04 | 3 | YSHOP-01/YSHOP-02 | T-15-05 | Yellow purchase validates catalog tuple and duplicate/spend state | handler/protocol | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowItemShop"` | yes | pending |
| 15-04-02 | 04 | 3 | YSHOP-01/YSHOP-02 | T-15-05 | Yellow purchase route is Mediator-backed and reward routes stay stateless | route/boundary | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowItemShop|FullyQualifiedName~YellowPersistenceBoundary"` | yes | pending |
| 15-05-01 | 05 | 4 | YMED-01 | T-15-06 | Active-season Don medals feed shop balances; Katsu stays profile-only | integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowItemShop|FullyQualifiedName~YellowPlayResult"` | yes | pending |
| 15-05-02 | 05 | 4 | YSHOP-02/YMED-01 | T-15-06 | Yellow purchased items feed userdata shop locks | protocol/boundary | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowUserData|FullyQualifiedName~YellowPersistenceBoundary"` | yes | pending |
| 15-06-01 | 06 | 5 | YWAI-01 | T-15-07 | WaiWai tutorial/readback remains evidence-bound | source/protocol | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowWaiWai|FullyQualifiedName~YellowWireGeneration"` | yes | pending |
| 15-06-02 | 06 | 5 | YWAI-01 | T-15-08 | WaiWai facts do not become score/unlock/shop/Dan authority | handler/boundary | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowWaiWai|FullyQualifiedName~YellowPlayResult|FullyQualifiedName~YellowPersistenceBoundary"` | yes | pending |
| 15-07-01 | 07 | 6 | YUI-01 | T-15-09 | AdminApi Yellow settings and play data read Yellow tables only | API | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowAdminApi"` | yes | pending |
| 15-07-02 | 07 | 6 | YUI-01 | T-15-09 | AdminApi Yellow history and favorites read/write Yellow tables only | API | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowAdminApi"` | yes | pending |
| 15-08-01 | 08 | 7 | YUI-01 | T-15-09 | AdminApi Yellow leaderboard and Dani readback use Yellow state | API | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowAdminApi"` | yes | pending |
| 15-08-02 | 08 | 7 | YUI-01 | T-15-09 | AdminApi Yellow game-data and customization readback use Yellow catalogs | API/service | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowAdminApi|FullyQualifiedName~GameDataService"` | yes | pending |
| 15-09-01 | 09 | 8 | YUI-01 | T-15-10 | WebUI treats Yellow as AC15 supported era | WebUI unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~WebUi"` | yes | pending |
| 15-09-02 | 09 | 8 | YUI-01 | T-15-10 | WebUI generic pages and data service route to Yellow APIs | WebUI/service | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~WebUi|FullyQualifiedName~GameDataService|FullyQualifiedName~YellowAdminApi"` | yes | pending |

## Wave 0 Requirements

Existing test infrastructure covers all Phase 15 requirements.

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Yellow normal/Tokkun cabinet or RPCS3 smoke | YVER-03 | Deferred by roadmap to Phase 17 | Do not run in Phase 15. Phase 17 records runtime smoke evidence. |

## Validation Sign-Off

- [x] All tasks have automated verify commands.
- [x] Sampling continuity: no three consecutive tasks lack automated verify.
- [x] Wave 0 covers all missing references.
- [x] No watch-mode flags.
- [x] Feedback latency is one task.
- [x] `nyquist_compliant: true` set in frontmatter.

**Approval:** approved 2026-06-08
