---
phase: 18-red-evidence-and-capability-foundation
verified: 2026-06-12T23:01:05Z
status: passed
score: 12/12 must-haves verified
overrides_applied: 0
---

# Phase 18: Red Evidence and Capability Foundation Verification Report

**Phase Goal:** Prove Red route/version/transport boundaries, inventory Red-supported capabilities, and add first-class Red adapter scaffolding.
**Verified:** 2026-06-12T23:01:05Z
**Status:** passed
**Re-verification:** No - initial verification

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | Developer can inspect a Red evidence artifact before route/controller behavior or capability binding is treated as finalized. | VERIFIED | `18-RED-EVIDENCE.md` exists, is substantive, and records `/v08r01`, shared `/v01r00`, `0xDA7660`, `0xDA76A0`, `ST8100-1`, direct-protobuf expectations, HDD mapping, and unresolved evidence gaps. |
| 2 | Red capability inventory maps local proto/data surfaces to shared AC15 capabilities, absent surfaces, and Red/older-AC15 candidates. | VERIFIED | `18-RED-EVIDENCE.md` has a phase-owner capability matrix for Phase 18 foundation, Phase 19 catalog/profile, Phase 20 runtime/simple compatibility, Phase 21 ChallengeCompe, and explicit absent surfaces. |
| 3 | Shared startup/version and capability-composition boundaries are explicitly proven or left as unresolved gaps before dependent phases assume them. | VERIFIED | Evidence keeps startup/version on shared `/v01r00`, records missing `8 => GameEra.Red` startup movie mapping as later work, and forbids `Ac15EraProfiles.Red` before Red protocol limits are proven. |
| 4 | Red exists as first-class `GameEra.Red` and an adapter assembly, not a Yellow or Blue variant. | VERIFIED | `Domain/Enums/GameEra.cs` appends `Red = 4`; `Adapters.GameProtocol.Red` has its own csproj, DI extension, marker, global usings, Mapperly defaults, and solution entry. |
| 5 | Red adapter project compiles generated Red wire DTOs from local `proto/red` inputs without modifying dumped proto files. | VERIFIED | `Adapters.GameProtocol.Red/Wire/Game.cs` and `VsInterface.cs` headers say `Input: taiko.proto` / `Input: vsinterface.proto`, use `TaikoLocalServer.Adapters.GameProtocol.Red.Wire`, include nullable optionals, `git status --porcelain -- proto/red .tools/red` had no output, and `dotnet build Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj` passed. |
| 6 | Host settings and application-part gating enable Red routes only when Red is configured. | VERIFIED | `Host/Program.cs` conditionally calls `AddGameProtocolRed()` only inside `enabledEras.Contains(GameEra.Red)` and removes `TaikoLocalServer.Adapters.GameProtocol.Red` when Red is disabled. |
| 7 | Red direct-protobuf fallback is scoped to `/v08r01/chassis`. | VERIFIED | `ShouldAssumeProtobufRequest` includes `/v08r01/chassis` and still requires POST plus missing content type; it does not broaden to all `/v08r01`. |
| 8 | Red can be enabled in committed local settings without unsupported shop/payment settings. | VERIFIED | `Host/Configurations/ServerSettings.json` has Red enabled with `GameDataPath: wwwroot/data/red/data` and no Red `EnableShop` / `ActiveShopSeasonId`; `Ac15ShopEras` remains Green/Blue/Yellow; focused settings tests passed 7/7. |
| 9 | IDB-known Red game suffixes route under `/v08r01/chassis/*` as no-state probes. | VERIFIED | Red controllers contain exactly 22 `[Route("/v08r01/chassis/...")]` attributes and 22 `[HttpPost]` actions matching the IDB-known list. |
| 10 | Route probes deserialize generated Red DTOs and do not call Mediator, EF, shared AC15 gameplay services, or payment authority code. | VERIFIED | Controller actions accept generated Red `[FromBody]` DTOs, log `Red route probe ... request`, return minimal generated responses, and the controller audit found no `Mediator`, `GetChallengeCompeQuery`, `ITaikoDbContext`, `TaikoDbContext`, `SaveChanges`, `Ac15EraProfiles`, wallet/coupon/transaction, catalog, or profile calls. |
| 11 | Proto-only `getbanacoininfo.php` and `getreitai.php` are not added as Phase 18 Red routes. | VERIFIED | No `getbanacoininfo` or `getreitai` matches exist under `Adapters.GameProtocol.Red/Controllers`; evidence records them only as proto-only candidates. |
| 12 | Regression/preservation checks cover existing-era behavior for shared-code changes, and runtime smoke is recorded without overclaiming card/gameplay support. | VERIFIED | `dotnet test Tests/Tests.csproj` passed 684/684; temp-output Host build passed with 0 warnings/errors; `18-RUNTIME-SMOKE.md` records user-confirmed basic connection only, explicitly says card scan/gameplay were not attempted, and Host log evidence shows post-fix Red route probes returning 200. |

**Score:** 12/12 truths verified

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `.planning/phases/18-red-evidence-and-capability-foundation/18-RED-EVIDENCE.md` | Canonical evidence record and capability matrix | VERIFIED | Exists; contains required route/root addresses, direct-protobuf expectation, proto-only candidates, absent surfaces, and guardrails. |
| `Domain/Enums/GameEra.cs` | First-class `GameEra.Red` | VERIFIED | Existing enum order preserved; `Red = 4` appended. |
| `Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj` | Red protocol adapter project | VERIFIED | Builds successfully and is included in `TaikoLocalServer.slnx` and `Host/Host.csproj`. |
| `Adapters.GameProtocol.Red/Wire/Game.cs` | Generated Red game DTOs | VERIFIED | Generated from `taiko.proto`, Red namespace, nullable optional primitive support. |
| `Adapters.GameProtocol.Red/Wire/VsInterface.cs` | Generated Red startup/version DTOs | VERIFIED | Generated from `vsinterface.proto`, Red namespace, nullable optional primitive support. |
| `Host/Program.cs` | Red DI registration, app-part gating, protobuf fallback | VERIFIED | Conditional Red registration/removal and scoped `/v08r01/chassis` fallback are present. |
| `Host/Configurations/ServerSettings.json` | Committed local Red settings | VERIFIED | Red enabled with data path and no shop settings. |
| `Host/Host.csproj` | Red adapter reference and Red operator-data build handling | VERIFIED | Red project reference, Red data content exclusion, and Debug red data junction target are present. |
| `Tests/Red/RedServerSettingsValidationTests.cs` | Red no-shop validation guard | VERIFIED | Focused settings test passed as part of 7-test filter run. |
| `Adapters.GameProtocol.Red/Controllers/*.cs` | Split no-state route probes | VERIFIED | 22 split controllers, one POST route each, generated DTO binding, no stateful service calls. |
| `.planning/phases/18-red-evidence-and-capability-foundation/18-RUNTIME-SMOKE.md` | Runtime smoke record | VERIFIED | Records pre-fix 405s, fix disposition, user-confirmed basic connection, and explicit future card/gameplay boundary. |

### Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| `18-RED-EVIDENCE.md` | `proto/red/taiko.proto` | Proto-backed capability and absent-surface rows | WIRED | Evidence references Red request/response surfaces including `ChallengeCompeRequest`, `GetbanacoininfoRequest`, and `GetreitaiRequest`. |
| `18-RED-EVIDENCE.md` | `.tools/red/EBOOT.ELF.i64` | IDA address-backed route/root evidence | WIRED | Evidence records IDB addresses including `0xDA7660`, `0xDA76A0`, `0xDAA758`, and `0xD72FC0`. |
| `Adapters.GameProtocol.Red/Wire/Game.cs` | `proto/red/taiko.proto` | Generated wire header | WIRED | Header says `Input: taiko.proto`; proto inputs are clean in git status. |
| `TaikoLocalServer.slnx` | `Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj` | Solution project include | WIRED | Red project appears in the solution and builds. |
| `Host/Program.cs` | `Adapters.GameProtocol.Red/DependencyInjection.cs` | Conditional `AddGameProtocolRed` call | WIRED | Host calls Red DI only when `enabledEras.Contains(GameEra.Red)`. |
| `Host/Host.csproj` | `Host/wwwroot/data/red/data` | Operator data exclusion and Debug junction | WIRED | Red data content exclusion and `CreateRedGameDataSymlinkForDebug` target are present. |
| `Adapters.GameProtocol.Red/Controllers/PlayResultController.cs` | `Adapters.GameProtocol.Red/Wire/Game.cs` | Generated `PlayResultRequest` / response DTOs | WIRED | Controller accepts `[FromBody] PlayResultRequest` and returns `PlayResultResponse`. |
| `18-RUNTIME-SMOKE.md` | `Host/Program.cs` / Red route probes | Manual server run with Red enabled | WIRED | Smoke record and Host logs show `/v01r00/chassis/startupauth.php` 200 and post-fix `/v08r01/chassis/initialdatacheck.php` / `tournamentcheck.php` 200. |

### Data-Flow Trace (Level 4)

| Artifact | Data Variable | Source | Produces Real Data | Status |
|----------|---------------|--------|--------------------|--------|
| `18-RED-EVIDENCE.md` | Evidence rows and capability matrix | Local IDA/proto/data findings captured in `18-RESEARCH.md` plus phase decisions | Yes for evidence artifact; runtime semantics deliberately marked unresolved | VERIFIED |
| Red route probe controllers | Generated Red request DTOs | ASP.NET Core protobuf `[FromBody]` binding on `/v08r01/chassis/*` | Yes for request logging; no persistence by design | VERIFIED |
| Red route probe responses | Minimal generated Red response DTOs | Controller-local response creation | Static by design for Phase 18 no-state probes | VERIFIED |
| `18-RUNTIME-SMOKE.md` | Manual smoke result | User RPCS3/cabinet confirmation plus Host log observations | Yes for Phase 18 basic connection scope | VERIFIED |

### Behavioral Spot-Checks

| Behavior | Command | Result | Status |
|----------|---------|--------|--------|
| Red settings do not require shop settings; Blue/Yellow still guarded | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedServerSettingsValidationTests|FullyQualifiedName~YellowServerSettingsValidationTests|FullyQualifiedName~BlueServerSettingsValidationTests"` | 7 passed, 0 failed, 0 skipped | PASS |
| Red adapter compiles | `dotnet build Adapters.GameProtocol.Red/Adapters.GameProtocol.Red.csproj` | Passed, 0 warnings, 0 errors | PASS |
| Host compiles with Red adapter to temp output | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` | Passed, 0 warnings, 0 errors | PASS |
| Full preservation test suite | `dotnet test Tests/Tests.csproj` | 684 passed, 0 failed, 0 skipped | PASS |
| Red route inventory | `Select-String ... '[Route("/v08r01/chassis/'` and `[HttpPost]` count | 22 routes, 22 POST actions | PASS |
| No-state controller audit | `rg "Mediator|GetChallengeCompeQuery|ITaikoDbContext|TaikoDbContext|SaveChanges|Ac15EraProfiles|Wallet|Coupon|Transaction|PaymentAuthority|DbSet|UserSaveData|SongPlayData|Challenge.*Query|Catalog|Profile" Adapters.GameProtocol.Red/Controllers` | No matches | PASS |
| Proto-only route audit | `rg "/v01r00|startupauth|verupauth|verupcomplete|getbanacoininfo|getreitai" Adapters.GameProtocol.Red/Controllers` | No matches | PASS |
| Dumped evidence preservation | `git status --porcelain -- proto/red .tools/red` | No output | PASS |

### Probe Execution

No `scripts/*/tests/probe-*.sh` probes were declared or applicable for this phase. Runtime smoke was manual RPCS3/cabinet evidence, already recorded in `18-RUNTIME-SMOKE.md`.

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
|-------------|-------------|-------------|--------|----------|
| RFND-01 | 18-01, 18-04 | Reviewable Red route/version evidence record with route prefix, transport expectations, shared startup/version ownership, HDD/version mapping, active root, and unresolved gaps. | SATISFIED | `18-RED-EVIDENCE.md` contains the route/root/transport/HDD record and `18-RUNTIME-SMOKE.md` records basic routing smoke. |
| RFND-02 | 18-02, 18-03, 18-04 | First-class enableable `GameEra.Red` adapter with generated Red wire, settings, Host/DI registration, route ownership, and enabled-era gating. | SATISFIED | Red enum/project/wire/Host registration/settings/routes exist and compile; Host gates Red application parts by enabled era. |
| RFND-03 | 18-01, 18-02, 18-03, 18-04 | Preserve current supported-era behavior except covered Red foundation changes. | SATISFIED | Full tests pass; no Red EF/profile/AdminApi/WebUI/gameplay state added; route probes are no-state and proto-only candidates remain absent. |

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| `.planning/ROADMAP.md` | Phase Progress table | Stale planning metadata says Phase 18 is `2/4 In Progress` while Phase 18 detail and plan checkboxes show 4/4 complete | INFO | Not a phase-goal blocker and not corrected here because the verifier was asked to create only the verification artifact. |

Debt-marker and placeholder scan over Phase 18 touched non-generated source/planning artifacts found no `TBD`, `FIXME`, `XXX`, `TODO`, `HACK`, placeholder text, empty implementations, or console-only implementations. Generated wire was excluded from manual cleanup by project rules.

### Human Verification Required

None outstanding. The required manual Red RPCS3/cabinet smoke has already been recorded as user-confirmed basic connection. The smoke artifact does not claim card scan, profile, or gameplay support; it explicitly defers those to later Red phases.

### Gaps Summary

No blocking gaps found. The items still not implemented are explicitly outside Phase 18 and are scheduled in later roadmap phases: Red catalog/profile and protocol limits in Phase 19, Red runtime/profile/gameplay/simple compatibility in Phase 20, ChallengeCompe semantics in Phase 21, and AdminApi/WebUI plus full runtime closeout in Phase 22.

---

_Verified: 2026-06-12T23:01:05Z_
_Verifier: the agent (gsd-verifier)_
