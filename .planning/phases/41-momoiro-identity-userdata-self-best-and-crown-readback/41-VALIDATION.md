---
phase: 41
slug: momoiro-identity-userdata-self-best-and-crown-readback
status: draft
nyquist_compliant: true
wave_0_complete: false
created: 2026-06-26
---

# Phase 41 - Validation Strategy

> Per-phase validation contract for MOMOIRO identity, userdata, self-best, favorite/recent, release-song, song-hash, and crown readback.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.9.3 with Microsoft.NET.Test.Sdk 17.14.1 |
| **Config file** | `Tests/Tests.csproj` |
| **Quick run command** | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroReadback|FullyQualifiedName~MomoiroUserData|FullyQualifiedName~MomoiroSelfBest|FullyQualifiedName~MomoiroBaid|FullyQualifiedName~MomoiroCrown" --no-restore -- RunConfiguration.DisableParallelization=true` |
| **Full suite command** | `dotnet test Tests/Tests.csproj --no-restore -- RunConfiguration.DisableParallelization=true` |
| **Estimated runtime** | focused under 2 minutes; full serialized suite under 1 minute on current machine |

---

## Sampling Rate

- **After every task commit:** Run the focused Momoiro readback test filter once relevant tests exist.
- **After each plan wave:** Run the plan-specific tests plus the project or adapter build listed in the plan.
- **Before phase verification:** Run focused Momoiro readback tests, full serialized test suite, solution build, Momoiro adapter generated-source build, proto cleanliness, unsupported-route absence, and no-cross-era/source-scope gates.
- **Max feedback latency:** one task or one wave, whichever is shorter.

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------------|-----------|-------------------|-------------|--------|
| 41-W0-01 | 41-01 | 0 | MORDB-01 | MOMOIRO baid/mydon flow creates shared identity plus `UserSaveData_Momoiro` only; adjacent era save tables remain untouched. | handler/controller integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroReadbackHandlerTests|FullyQualifiedName~MomoiroControllerReadbackTests" --no-restore -- RunConfiguration.DisableParallelization=true` | `Tests/Momoiro/MomoiroReadbackHandlerTests.cs`; `Tests/Momoiro/MomoiroControllerReadbackTests.cs` | RED - test artifacts present; expected to fail until Momoiro readback dispatch/schema is implemented |
| 41-W0-02 | 41-01 | 0 | MORDB-02 | `selfbest.php` reads only `SongBestDatum_Momoiro`, including normal, ura, and shin rows mapped through current MOMOIRO wire. | handler/controller integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroReadbackHandlerTests|FullyQualifiedName~MomoiroControllerReadbackTests" --no-restore -- RunConfiguration.DisableParallelization=true` | `Tests/Momoiro/MomoiroReadbackHandlerTests.cs`; `Tests/Momoiro/MomoiroControllerReadbackTests.cs` | RED - expected missing Momoiro self-best dispatch/controller mapping |
| 41-W0-03 | 41-01 | 0 | MORDB-03 | Favorite and recent readback uses MOMOIRO-owned rows, Phase 40 limits, persisted favorite `DisplayOrder` ordering with `SongNo` tie-breaker, recent `LastPlayed` ordering, truncation, and no adjacent-era leakage. | handler/controller integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroReadbackHandlerTests|FullyQualifiedName~MomoiroControllerReadbackTests" --no-restore -- RunConfiguration.DisableParallelization=true` | `Tests/Momoiro/MomoiroHandlerFixture.cs`; `Tests/Momoiro/MomoiroReadbackHandlerTests.cs`; `Tests/Momoiro/MomoiroControllerReadbackTests.cs` | RED - raw SQL future table includes `DisplayOrder`; expected missing Momoiro userdata readback |
| 41-W0-04 | 41-01 | 0 | MORDB-04 | `userdata.php` returns `hash_crown_flg` packed by MOMOIRO catalog/hash order and does not expose `crownsdata.php`. | byte packing/regression | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroCrownReadbackTests|FullyQualifiedName~MomoiroRouteSurfaceTests" --no-restore -- RunConfiguration.DisableParallelization=true` | `Tests/Momoiro/MomoiroCrownReadbackTests.cs`; `Tests/Momoiro/MomoiroRouteSurfaceTests.cs` | RED - crown readback tests present; route absence guard remains the supported green check |
| 41-W0-05 | 41-01 | 0 | MORDB-05 | `userdata.php` returns MOMOIRO catalog-order release-song bytes and binary-backed song hash version/table. | handler/controller integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroReadbackHandlerTests|FullyQualifiedName~MomoiroControllerReadbackTests" --no-restore -- RunConfiguration.DisableParallelization=true` | `Tests/Momoiro/MomoiroReadbackHandlerTests.cs`; `Tests/Momoiro/MomoiroControllerReadbackTests.cs` | RED - expected missing Momoiro release/song-hash userdata mapping |
| 41-FINAL-01 | TBD | final | MORDB-01..05 | Generated Mapperly source for new MOMOIRO mappers assigns only proven response fields. | generated-source inspection | `dotnet build Adapters.GameProtocol.Momoiro/Adapters.GameProtocol.Momoiro.csproj /p:EmitCompilerGeneratedFiles=true --no-restore` | `obj/.../Riok.Mapperly/...` | pending |

*Status: pending - green - red - flaky*

---

## Wave 0 Requirements

- [x] RED tests compile without future-symbol failures and fail only for missing MOMOIRO readback behavior. Current status recorded by plan `41-01`.
- [x] Tests cover shared identity plus MOMOIRO-owned save creation, and prove adjacent era saves remain untouched.
- [x] Tests cover `userdata.php` and `selfbest.php` controller behavior through response objects, not source strings.
- [x] Tests cover catalog/hash-order crown packing so high MOMOIRO song IDs are not dropped by raw song-id indexing.
- [x] Tests cover absence of `crownsdata.php` and other unsupported/proto-only route families.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Exact native source of favorite ordering | MORDB-03 | Generated wire exposes `ary_favorite_song_no` as an ordered array but IDA did not prove whether native mutation stores selection order, UI order, or another source. Phase 41 therefore persists an explicit `DisplayOrder` and tests response ordering from that stored value. | If new client/log evidence appears, update favorite writer behavior before changing the Phase 41 readback contract. |
| Exact native crown byte constant | MORDB-04 | Field and inferred envelope are backed by generated wire and song count math; exact native constant is not tied to code flow. | Keep crown packing tests tied to 380-song catalog order and 475-byte output; do not claim native constant proof. |
| Cabinet/RPCS3 readback acceptance | MORDB-01..05 | Phase 41 is server-side readback implementation and regression coverage; milestone cabinet acceptance is Phase 44. | Do not claim cabinet acceptance in Phase 41 summaries unless the user explicitly provides runtime evidence. |

---

## Validation Sign-Off

- [x] All Phase 41 requirements have automated verify targets or explicit manual-only rationale.
- [x] Sampling continuity: no 3 consecutive tasks without automated verify.
- [x] Wave 0 covers missing tests before production implementation.
- [x] No watch-mode flags.
- [x] Feedback latency bounded to one task or one wave.
- [x] `nyquist_compliant: true` set in frontmatter.

**Approval:** draft for planner consumption 2026-06-26

---

## Plan 41-05 Final Gate Evidence

### Task 1 - Focused Readback and Mapperly Gates

**Recorded:** 2026-06-26T09:29:16Z

| Gate | Command | Result |
|------|---------|--------|
| Focused Momoiro readback/controller/crown/route/AC15 codec tests | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroReadback|FullyQualifiedName~MomoiroControllerReadback|FullyQualifiedName~MomoiroCrown|FullyQualifiedName~MomoiroRouteSurface|FullyQualifiedName~Ac15CrownService|FullyQualifiedName~Ac15SongHashCodec" --no-restore -- RunConfiguration.DisableParallelization=true` | PASS: 20 passed, 0 failed, 0 skipped. |
| Momoiro adapter generated-source build | `dotnet build Adapters.GameProtocol.Momoiro/Adapters.GameProtocol.Momoiro.csproj /p:EmitCompilerGeneratedFiles=true --no-restore` | PASS: build succeeded with 0 warnings and 0 errors. |
| Mapperly generated-source grep | `rg -n "MydonName|HashReleaseSongFlg|HashCrownFlg|ArySelfbestScores|AryShinSelfbestScores" Adapters.GameProtocol.Momoiro/obj/Debug/net10.0/generated/Riok.Mapperly` | PASS: expected assignments found in `BaidResponseMapper.g.cs`, `UserDataMappers.g.cs`, and `SelfBestMappers.g.cs`. |

Generated Mapperly files inspected:

- `Adapters.GameProtocol.Momoiro/obj/Debug/net10.0/generated/Riok.Mapperly/Riok.Mapperly.MapperGenerator/BaidResponseMapper.g.cs`
- `Adapters.GameProtocol.Momoiro/obj/Debug/net10.0/generated/Riok.Mapperly/Riok.Mapperly.MapperGenerator/UserDataMappers.g.cs`
- `Adapters.GameProtocol.Momoiro/obj/Debug/net10.0/generated/Riok.Mapperly/Riok.Mapperly.MapperGenerator/SelfBestMappers.g.cs`

Generated-source findings:

- `BaidResponseMapper.g.cs` assigns `MydonName`, title/color fields, selected costume data, update datetime, costume flags, Dan display/default flags, and `RewardPtn`.
- `UserDataMappers.g.cs` assigns `SongHashVer`, `HashReleaseSongFlg`, option/tone/title flags, favorite and recent arrays, recommendations, profile counters, display settings, mode flags, reward fields, and `HashCrownFlg` via `MapRequiredBytes(source.HashCrownFlg)`.
- `SelfBestMappers.g.cs` assigns response `Result` and `Level`, appends mapped rows to `ArySelfbestScores` and `AryShinSelfbestScores`, and maps each row's `SongNo`, `SelfBestScore`, and `UraBestScore`.
- Generated source does not add playresult mutation, AdminApi/WebUI behavior, unsupported route families, or proto-derived challenge/friend route behavior.

Mapperly documentation checked before inspection: current official Mapperly stable docs for null-value behavior, constant/generated values, and generated-source inspection:

- `https://mapperly.riok.app/docs/configuration/mapper/#null-values`
- `https://mapperly.riok.app/docs/configuration/constant-generated-values/`
- `https://mapperly.riok.app/docs/configuration/generated-source/`
