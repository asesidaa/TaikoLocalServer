---
phase: 14
slug: yellow-identity-userdata-crowns-self-best-and-normal-play
status: draft
nyquist_compliant: true
wave_0_complete: false
created: 2026-06-08
---

# Phase 14 - Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.9.3 via `Tests/Tests.csproj` |
| **Config file** | `Tests/Tests.csproj`, `Directory.Build.props`, `Directory.Packages.props` |
| **Quick run command** | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowIdentity|FullyQualifiedName~YellowUserData|FullyQualifiedName~YellowPlayResult|FullyQualifiedName~YellowSelfBest|FullyQualifiedName~YellowCrownsData|FullyQualifiedName~YellowPersistenceBoundary"` |
| **Full suite command** | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Yellow"` |
| **Build command** | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase14-yellow"` |
| **Estimated runtime** | Focused Yellow persistence/protocol tests: under 2 minutes; Yellow regression/build depends on local machine state |

---

## Sampling Rate

- **After every task commit:** Run the focused test filter named in that task.
- **After every plan wave:** Run `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Yellow"` plus the temp-output Host build when source code changed.
- **Before phase verification:** Full `dotnet test Tests/Tests.csproj` and temp-output Host build should be green unless a documented pre-existing external lock blocks them.
- **Max feedback latency:** Keep focused task checks under 2 minutes where possible.

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 14-01-01 | 01 | 1 | YUSR-01 | T-14-01 | Yellow save/best/play/favorite/recent tables are physically Yellow-owned and cascade from shared `UserData` only | EF/unit/source | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowPersistenceBoundary|FullyQualifiedName~YellowIdentity"` | W0 | pending |
| 14-01-02 | 01 | 1 | YUSR-01 | T-14-02 | BAID/mydon create shared identity plus Yellow save only; Blue/Green/Nijiiro gameplay tables remain unchanged | handler/route | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowIdentity|FullyQualifiedName~YellowPersistenceBoundary"` | W0 | pending |
| 14-02-01 | 02 | 2 | YUSR-02 | T-14-03 | Yellow userdata reads Yellow save, catalog, favorites, recent songs, and supported flags through AC15 service | handler/mapper | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowUserData"` | W0 | pending |
| 14-02-02 | 02 | 2 | YPLY-02 | T-14-04 | Yellow self-best returns requested normal/Ura/Shin rows from Yellow best table only | handler/mapper | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowSelfBest"` | W0 | pending |
| 14-02-03 | 02 | 2 | YCRN-01 | T-14-05 | Yellow crown response uses proven packing and explicitly asserted raw-vs-gzip field 3 bytes | byte/route | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowCrownsData|FullyQualifiedName~Ac15CrownServiceTests|FullyQualifiedName~Ac15ProtocolBytesTests"` | W0 | pending |
| 14-03-01 | 03 | 3 | YPLY-01 | T-14-06 | Yellow playresult mapper/handler accepts normal uploads and rejects/non-writes special mode scope | handler/mapper | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowPlayResult"` | W0 | pending |
| 14-03-02 | 03 | 3 | YPLY-01, YCRN-01 | T-14-07 | Yellow normal play writes only Yellow history/best/counters/favorites/recent and feeds self-best/crowns | integration/source | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowPlayResult|FullyQualifiedName~YellowSelfBest|FullyQualifiedName~YellowCrownsData|FullyQualifiedName~YellowPersistenceBoundary"` | W0 | pending |

---

## Wave 0 Requirements

- [ ] `Tests/Yellow/YellowIdentityHandlerTests.cs` - BAID/mydon/default Yellow save tests.
- [ ] `Tests/Yellow/YellowPersistenceBoundaryTests.cs` - Yellow-only EF/source/no-cross-era-write tests.
- [ ] `Tests/Yellow/YellowUserDataProtocolTests.cs` - Yellow userdata handler and wire mapper tests.
- [ ] `Tests/Yellow/YellowSelfBestTests.cs` - Yellow self-best handler/mapper route tests.
- [ ] `Tests/Yellow/YellowCrownsDataTests.cs` - Yellow crown packing, field placement, and raw-vs-gzip tests.
- [ ] `Tests/Yellow/YellowPlayResultHandlerTests.cs` - Yellow normal playresult mapper/handler/no-cross-write tests.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Yellow cabinet/RPCS3 normal smoke | YVER-03 | Runtime smoke is deferred to Phase 17 by roadmap/context. | Keep Phase 14 automated only; Phase 17 records cabinet/RPCS3 evidence. |

---

## Validation Sign-Off

- [x] All planned tasks have an automated verify command or Wave 0 test dependency.
- [x] Sampling continuity: no 3 consecutive implementation tasks without automated verify.
- [x] Wave 0 covers all missing test references.
- [x] No watch-mode flags.
- [x] Feedback latency target documented.
- [x] `nyquist_compliant: true` set in frontmatter.

**Approval:** draft 2026-06-08
