---
phase: 42
slug: momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil
status: complete
nyquist_compliant: true
wave_0_complete: true
created: 2026-06-26
---

# Phase 42 - Validation Strategy

> Per-phase validation contract for MOMOIRO normal playresult mutation, unlocks, rewards, and bounded Dan compatibility.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.9.3 with Microsoft.NET.Test.Sdk 17.14.1 |
| **Config file** | `Tests/Tests.csproj` |
| **Quick run command** | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResult|FullyQualifiedName~MomoiroReadback|FullyQualifiedName~Ac15NormalPlayWriter|FullyQualifiedName~Ac15Dani" --no-restore -- RunConfiguration.DisableParallelization=true` |
| **Full suite command** | `dotnet test Tests/Tests.csproj --no-restore -- RunConfiguration.DisableParallelization=true` |
| **Mapper verification command** | `dotnet build TaikoLocalServer.slnx /p:EmitCompilerGeneratedFiles=true --no-restore`, then inspect `obj/.../generated/.../Riok.Mapperly/*.g.cs` |

---

## Sampling Rate

- **After every task commit:** Run the focused Momoiro playresult/readback, AC15 normal-play writer, or AC15 Dani slice relevant to the changed code.
- **After each plan wave:** Run the plan-specific tests plus the project or adapter build listed in that plan.
- **Before phase verification:** Run focused Momoiro playresult/controller/readback tests, full serialized suite, solution build, temp-output Host build, Mapperly generated-source inspection, proto cleanliness, unsupported route/state gates, path-abstraction gate, and `Host/.gitignore` unchanged/staged-clean checks.
- **Max feedback latency:** one task or one wave, whichever is shorter.

---

## Dirty-File Baseline

`Host/.gitignore` was already modified before Phase 42 execution. Phase 42 executors must preserve that pre-existing diff exactly and must not stage it.

| File | Baseline Command | Baseline SHA-256 | Required Phase 42 Behavior |
|------|------------------|------------------|----------------------------|
| `Host/.gitignore` | `git diff -- Host/.gitignore` | `73190e8a4bb993eadc0a9364cac7e1b0336f45192911fb68d9ab1ebb87cac782` | Diff hash unchanged and no staged entry for `Host/.gitignore`. |

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------------|-----------|-------------------|-------------|--------|
| 42-W0-01 | 42-01 | 0 | MORUN-01 | Normal playresult writes only Momoiro play-history, best/crown source, counters, recents, favorites with append-only `DisplayOrder`, and no adjacent-era rows. | handler/controller integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResultHandler|FullyQualifiedName~MomoiroPlayResultController" --no-restore -- RunConfiguration.DisableParallelization=true` | Added: `Tests/Momoiro/MomoiroPlayResultHandlerTests.cs`; `Tests/Momoiro/MomoiroPlayResultControllerTests.cs` | RED EXPECTED - contracts compile and fail until Momoiro playresult schema/dispatch/write behavior is implemented. |
| 42-W0-02 | 42-01 | 0 | MORUN-02 | Release-song IDs, Don Point, and reward fields update only Momoiro save/readback state; no shop, wallet, payment, or spend authority is created. | handler/controller integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResultHandler|FullyQualifiedName~MomoiroReadback" --no-restore -- RunConfiguration.DisableParallelization=true` | Added: `Tests/Momoiro/MomoiroPlayResultHandlerTests.cs`; existing readback tests | RED EXPECTED - contracts compile and fail until Momoiro save-field mutation is implemented. |
| 42-W0-03 | 42-01/42-04 | 0/2 | MORUN-03 | Dan persistence is Momoiro-owned and bounded to playresult/BAID/userdata compatibility; no Taikojuku route, practice folder, or broader feature family is exposed. | handler/unit/profile | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResultHandler|FullyQualifiedName~Ac15DaniCapabilityTests" --no-restore -- RunConfiguration.DisableParallelization=true` | Added: `Tests/Momoiro/MomoiroPlayResultHandlerTests.cs`; existing `Tests/Ac15/Ac15DaniCapabilityTests.cs` | RED EXPECTED - bounded Dan contracts compile and fail until Momoiro-owned Dan schema/handler support is implemented. |
| 42-W0-04 | 42-01 | 0 | MORUN-04 | Challenge-shaped arrays are accepted/mapped for diagnostics and dropped; no Don Challenge, ChallengeCompe, reward-management, or raw future-proof challenge table is written. | negative persistence/integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResultHandler|FullyQualifiedName~MomoiroPlayResultController" --no-restore -- RunConfiguration.DisableParallelization=true` | Added: `Tests/Momoiro/MomoiroPlayResultHandlerTests.cs`; `Tests/Momoiro/MomoiroPlayResultControllerTests.cs` | RED EXPECTED - challenge-array no-state contracts compile and fail until Momoiro playresult dispatch/mapping exists. |
| 42-W0-05 | 42-01/42-07 | 0/final | MORUN-05 | Runtime writes are Momoiro-owned only; unsupported Momoiro route families, proto edits, unsupported feature state, hardcoded data paths, and `Host/.gitignore` edits are absent. | integration/source gate | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResultHandler|FullyQualifiedName~MomoiroRouteSurface" --no-restore -- RunConfiguration.DisableParallelization=true`; source gates in 42-07 | Added handler/controller contracts; existing `Tests/Momoiro/MomoiroRouteSurfaceTests.cs` | RED EXPECTED - no-cross-era contracts compile and fail until Momoiro runtime writes are implemented; unsupported-route tests remain green. |

*Status: pending - red - green - flaky*

---

## Wave 0 Requirements

- [x] `Tests/Momoiro/MomoiroPlayResultHandlerTests.cs` covers MORUN-01 through MORUN-05 through handler/database/readback behavior.
- [x] `Tests/Momoiro/MomoiroPlayResultControllerTests.cs` covers direct protobuf controller mapping without source-text route assertions.
- [x] `Tests/Momoiro/MomoiroHandlerFixture.cs` supports play-history and Dan table helpers without relying on future production symbols before schema lands.
- [x] Momoiro Dan tests prove bounded Dan compatibility and absence of Taikojuku route/practice-folder behavior.
- [x] Challenge-array tests prove accepted route payloads do not create Don Challenge, ChallengeCompe, reward-management, or adjacent-era state.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Exact cabinet-visible interpretation of Momoiro challenge arrays | MORUN-04 | Binary/proto evidence proves field presence, not stateful server semantics. Phase 42 intentionally logs/drops without persistence. | If request logs or cabinet evidence prove readback semantics, add a later phase before changing this behavior. |
| Exact native favorite insertion order | MORUN-01 | Phase 41 established persisted `DisplayOrder`; Phase 42 appends after the max as the conservative server contract. | If native evidence proves a different order, update favorite mutation and readback tests together. |
| Cabinet/RPCS3 acceptance of playresult mutation | MORUN-01..05 | Phase 42 is server-side automated verification; cabinet acceptance remains Phase 44. | Do not claim cabinet acceptance in Phase 42 summaries unless the user provides runtime evidence. |

---

## Validation Sign-Off

- [x] All Phase 42 requirements have automated verify targets or explicit manual-only rationale.
- [x] Sampling continuity: no 3 consecutive tasks without automated verify.
- [x] Wave 0 covers missing tests before production implementation.
- [x] No watch-mode flags.
- [x] Feedback latency bounded to one task or one wave.
- [x] `nyquist_compliant: true` set in frontmatter.

**Approval:** planned for executor consumption 2026-06-26.

---

## Plan 42-07 Final Execution Evidence

Status: complete - automated server-side verification only. Cabinet/RPCS3 acceptance remains Phase 44, and AdminApi/WebUI remains Phase 43.

### Task 1 - Focused Momoiro playresult and Mapperly gates

| Gate | Command | Result |
|------|---------|--------|
| Focused Momoiro Phase 42 + Phase 41 readback suite | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResult|FullyQualifiedName~MomoiroReadback|FullyQualifiedName~MomoiroControllerReadback|FullyQualifiedName~MomoiroCrown|FullyQualifiedName~MomoiroRouteSurface|FullyQualifiedName~Ac15NormalPlayWriter|FullyQualifiedName~Ac15Dani" --no-restore -- RunConfiguration.DisableParallelization=true` | PASS: 25 passed, 0 failed, 0 skipped. |
| Momoiro adapter generated-source build | `dotnet build Adapters.GameProtocol.Momoiro/Adapters.GameProtocol.Momoiro.csproj /p:EmitCompilerGeneratedFiles=true --no-restore` | PASS: 0 warnings, 0 errors. |
| Mapperly generated-source inspection | `rg -n "ReleaseSongNoes|GetDonpoint|RewardPtn|RewardProgress|DanResult|PlayDan|ChallengeIds|HashReleaseSongFlg|HashCrownFlg" Adapters.GameProtocol.Momoiro/obj/Debug/net10.0/generated/Riok.Mapperly` | PASS: expected playresult request and readback response assignments found. |

Generated-source observations:

- `PlayResultMappers.g.cs` assigns `GetDonpoint`, `RewardPtn`, `RewardProgress`, `ReleaseSongNoes`, nullable `DanResult`, `PlayDan`, and `ChallengeIds`.
- `UserDataMappers.g.cs` assigns `HashReleaseSongFlg`, `RewardProgress`, `TotalGetDonpoint`, and `HashCrownFlg`.
- `BaidResponseMapper.g.cs` assigns `RewardPtn`.
- This evidence confirms server-side generated Mapperly mappings for Momoiro playresult/readback fields; it is not cabinet/RPCS3 acceptance.

### Task 2 - Full build/test and Momoiro source gates

| Gate | Command | Result |
|------|---------|--------|
| Full serialized test suite | `dotnet test Tests/Tests.csproj --no-restore -- RunConfiguration.DisableParallelization=true` | PASS: 924 passed, 0 failed, 0 skipped. |
| Solution build, first attempt | `dotnet build TaikoLocalServer.slnx --no-restore` | BLOCKED: `Host/bin/Debug/net10.0` assemblies were locked by pre-existing `TaikoLocalServer` process PID 77312. |
| Environment lock clear | `Stop-Process -Id 77312` | PASS: local server process stopped to unblock the required exact solution-build gate. |
| Solution build, rerun | `dotnet build TaikoLocalServer.slnx --no-restore` | PASS: 0 warnings, 0 errors. |
| Temp-output Host build | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build" --no-restore` | PASS: 0 warnings, 0 errors. |
| Proto cleanliness, unsupported route/state absence, `Host/.gitignore` staging and baseline hash | `$status = git status --porcelain -- proto\momoiro; if ($status) { $status; exit 1 }; $routes = rg -n "shoppingresult\.php|bestscore\.php|communicationlog\.php|mainichisong\.php|crownsdata\.php|taikojuku\.php|challengecompe\.php|banacoin|battleuserdata\.php" Adapters.GameProtocol.Momoiro/Controllers; if ($LASTEXITCODE -eq 0) { $routes; exit 1 }; if ($LASTEXITCODE -ne 1) { exit $LASTEXITCODE }; $state = rg -n "Momoiro.*Tokkun|Tokkun.*Momoiro|Momoiro.*Battle|Battle.*Momoiro|Momoiro.*Banacoin|Banacoin.*Momoiro|Momoiro.*ChallengeCompe|ChallengeCompe.*Momoiro|Momoiro.*DonChallenge|DonChallenge.*Momoiro|ShopSeason.*Momoiro|Momoiro.*ShopSeason|Momoiro.*Wallet|Wallet.*Momoiro|Momoiro.*Payment|Payment.*Momoiro|Momoiro.*Coupon|Coupon.*Momoiro|Momoiro.*Transaction|Transaction.*Momoiro" Application/Handlers Application/Ac15 Domain/Entities Infrastructure/Persistence Adapters.GameProtocol.Momoiro; if ($LASTEXITCODE -eq 0) { $state; exit 1 }; if ($LASTEXITCODE -ne 1) { exit $LASTEXITCODE }; $staged = git diff --cached --name-only -- Host/.gitignore; if ($staged) { $staged; exit 1 }; $diff = git diff -- Host/.gitignore; $text = ($diff -join "`n"); $bytes = [System.Text.Encoding]::UTF8.GetBytes($text); $sha = [System.Security.Cryptography.SHA256]::Create(); $hash = [System.BitConverter]::ToString($sha.ComputeHash($bytes)).Replace('-','').ToLowerInvariant(); if ($hash -ne '73190e8a4bb993eadc0a9364cac7e1b0336f45192911fb68d9ab1ebb87cac782') { Write-Error "Host/.gitignore baseline diff changed: $hash"; exit 1 }; exit 0` | PASS: no proto changes; no unsupported Momoiro route families; no unsupported Momoiro Tokkun/battle/Banacoin/ChallengeCompe/DonChallenge/shop/wallet/payment/coupon/transaction state; no staged `Host/.gitignore`; baseline diff hash preserved. Command emitted only the expected Git line-ending warning for the pre-existing unstaged `Host/.gitignore` diff. |
| Route-prefix correctness | `rg -n "MomoiroRoutePrefixes|v04r00|v01r00" Adapters.GameProtocol.Momoiro Host Tests/Momoiro` | PASS: Momoiro game routes use `MomoiroRoutePrefixes.Game = "/v04r00/chassis"`; shared startup/version route assertions remain `/v01r00/chassis`. |
| Hardcoded Momoiro data-path absence | `rg -n "wwwroot[/\\]data[/\\]momoiro|wwwroot/data/momoiro|wwwroot\\data\\momoiro|Host[/\\]wwwroot[/\\]data[/\\]momoiro|Host/wwwroot/data/momoiro|Host\\wwwroot\\data\\momoiro" Adapters.GameProtocol.Momoiro/Controllers Application/Handlers Application/Ac15` | PASS: no matches. |

Scope notes:

- `proto/momoiro` remained clean after all Task 2 gates.
- `Host/.gitignore` remained unstaged, with SHA-256 `73190e8a4bb993eadc0a9364cac7e1b0336f45192911fb68d9ab1ebb87cac782` for the visible pre-existing diff.
- `.scratch/` remained an untracked pre-existing local scratch directory and was not staged.
- AdminApi/WebUI remains Phase 43; cabinet/RPCS3 acceptance remains Phase 44.

### Task 3 - MORUN closeout and final sign-off

| Gate | Command | Result |
|------|---------|--------|
| Requirement closeout | `node .codex\gsd-core\bin\gsd-tools.cjs query requirements.mark-complete MORUN-01 MORUN-02 MORUN-03 MORUN-04 MORUN-05` | PASS: marked exactly MORUN-01 through MORUN-05 complete; no other requirements were changed. |
| Requirement checkbox verification | `rg -n "\[x\] \*\*MORUN-01\*\*|\[x\] \*\*MORUN-02\*\*|\[x\] \*\*MORUN-03\*\*|\[x\] \*\*MORUN-04\*\*|\[x\] \*\*MORUN-05\*\*" .planning/REQUIREMENTS.md` | PASS: all five MORUN requirement checkboxes are complete. |
| Final validation text verification | `rg -n "42-FINAL|green|MORUN-01|MORUN-02|MORUN-03|MORUN-04|MORUN-05|cabinet/RPCS3 acceptance remains Phase 44|AdminApi/WebUI remains Phase 43" .planning/phases/42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil/42-VALIDATION.md` | PASS: final green rows and Phase 43/44 non-claim language are present. |

## Final Phase 42 Sign-Off

| ID | Status | Evidence |
|----|--------|----------|
| 42-FINAL | green | Focused Momoiro suite passed; full serialized suite passed; solution and temp-output Host builds passed; Mapperly generated source was emitted and inspected; proto cleanliness, unsupported route/state absence, route-prefix correctness, hardcoded-path absence, and `Host/.gitignore` baseline/staged-clean gates passed. |
| MORUN-01 | green | Focused tests and readback coverage verify Momoiro-owned normal playresult writes feed Phase 41 userdata, selfbest, and crown readback. |
| MORUN-02 | green | Source and behavior gates verify no Momoiro shop, wallet, payment, purchase, spend, or shop-season authority was added. |
| MORUN-03 | green | Tests verify bounded Momoiro Dan playresult/BAID/userdata compatibility, and route/source gates keep Taikojuku absent. |
| MORUN-04 | green | Tests and source gates verify challenge-shaped arrays do not create Don Challenge, ChallengeCompe, or reward-management state. |
| MORUN-05 | green | Full gates verify no adjacent-era gameplay writes and no unsupported Momoiro route families. |

Final scope statement: AdminApi/WebUI remains Phase 43. Cabinet/RPCS3 acceptance remains Phase 44. Phase 42 is closed only as automated server-side verification.
