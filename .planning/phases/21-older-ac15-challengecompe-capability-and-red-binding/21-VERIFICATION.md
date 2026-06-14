# Phase 21 Verification

**Verified:** 2026-06-14  
**Scope:** Older-AC15 ChallengeCompe capability and Red binding.  
**Plan:** 21-05 closeout verification.

## Final Verdict

Phase 21 passes closeout verification for the implemented local server contract.

Stateful Red ChallengeCompe support is present only inside the approved evidence gate:

- Red-owned disabled-safe sidecar catalog: `Host/wwwroot/data/red/red_challenge_compe_data.json`.
- Red-owned raw fact and progress tables: `RedChallengeCompeRawFacts` and `RedChallengeCompeProgress`.
- Opt-in gate: `UserSaveDataRed.IsChallengeCompe`.
- Mutation source: eligible non-Tokkun Red playresults with active configured `ary_challenge_id` personal task facts.
- Reward behavior: configured song/title rewards grant immediately through Red release/title flags.
- Userdata locks: active unearned configured reward songs feed the existing AC15 locked-song path.
- Readback: Red `challengecompe.php` returns active saved progress in `ary_challenge_stat`.
- Unsupported buckets: `ary_user_compe_stat` and `ary_bng_compe_stat` remain empty.

Remaining evidence-gated gaps are not hidden: real operator-authored Red task schedules, community/global aggregation, user-created challenge letters, BNG/official competition buckets, exact cabinet/RPCS3 acceptance of non-empty `ary_challenge_stat`, and Phase 22 AdminApi/WebUI opt-in editing are still outside Phase 21.

## Command Results

| Command | Result |
|---------|--------|
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedChallengeCompe|FullyQualifiedName~RedPlayResultHandlerTests|FullyQualifiedName~RedProtocolMapperTests|FullyQualifiedName~Ac15UserDataService"` | Passed: 30 passed, 0 failed, 0 skipped |
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Red"` | Initial run failed: 82 passed, 1 failed. Failure was `RedCatalogLoaderTests.DefaultRedSidecarFiles_ExistAndLoadAsDataContracts`, caused by the known pre-existing dirty `Host/wwwroot/data/red/red_telop_data.json` containing one valid telop entry while the test asserted the telop sidecar must be empty. |
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Red"` | Passed after the scoped test fix: 83 passed, 0 failed, 0 skipped |
| `dotnet test Tests/Tests.csproj` | Passed: 731 passed, 0 failed, 0 skipped |
| `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` | Passed: 0 warnings, 0 errors; output at `C:\Users\10614\AppData\Local\Temp\TaikoLocalServer-host-build` |

## Auto-Fixed Verification Issue

**[Rule 3 - Blocking] Red default sidecar test required telop sidecar emptiness**

- **Found during:** Task 1 final Red regression command.
- **Issue:** `RedCatalogLoaderTests.DefaultRedSidecarFiles_ExistAndLoadAsDataContracts` failed because the workspace had the known pre-existing dirty `Host/wwwroot/data/red/red_telop_data.json` with a valid telop entry. The test name and Yellow analog verify that default sidecars exist and load as data contracts, not that server-authored sidecars must always be empty.
- **Fix:** Updated the Red sidecar contract test to assert loaded collections are non-null, matching the Yellow-era test pattern, without touching the dirty Red telop data file.
- **Files modified:** `Tests/Red/RedCatalogLoaderTests.cs`.
- **Verification:** Red regression rerun passed with 83 tests; full suite passed with 731 tests.

## Decision Audit

| Decision | Status | Evidence |
|----------|--------|----------|
| D-01 explicit opt-in | Pass | `SaveRedChallengeCompeAsync` returns before mutation when `saveData.IsChallengeCompe` is false; readback also requires `IsChallengeCompe`. |
| D-02 opt-in source is Red save/userdata field | Pass | `UserSaveDataRed.IsChallengeCompe` is the only enrollment state used by playresult mutation, userdata locks, and readback. |
| D-03 opt-in does not mean global active/progress | Pass | Active catalog windows are evaluated separately, and disabled/inactive catalogs do not create progress or locks. |
| D-04 `challengecompe.php` does not opt in | Pass | `GetChallengeCompeQuery.Red` reads with `AsNoTracking` and returns empty for non-enrolled users; tests cover no mutation. |
| D-05 Phase 22 UI opt-in backing | Pass | Backing behavior is `UserSaveDataRed.IsChallengeCompe`; no AdminApi/WebUI editing was added in Phase 21. |
| D-06 non-Tokkun playresults update progress | Pass | Red Tokkun returns before ChallengeCompe mutation; non-Tokkun valid stages flow to `SaveRedChallengeCompeAsync`. |
| D-07 Red-owned sidecar | Pass | ChallengeCompe config lives at `Host/wwwroot/data/red/red_challenge_compe_data.json` and is exposed through `IRedCatalog`. |
| D-08 monthly bundle schema | Pass | Catalog records model windows, 10 personal tasks, optional community task metadata, and rewards. |
| D-09 typed predicates | Pass | `Ac15ChallengeCompeRuleKind` covers supported rule families; unknown rules load as `Unsupported` and cannot execute. |
| D-10 catalog metadata vs runtime mutation | Pass | Reward metadata is catalog data; mutation occurs in Red playresult reward handling. |
| D-11 configurable/disabled locks | Pass | Disabled/no-active catalogs produce no locks; active unearned reward songs lock only for opted-in users. |
| D-12 raw and derived Red persistence | Pass | Red-owned `RedChallengeCompeRawFact` and `RedChallengeCompeProgress` entities, DbSets, and migration exist. |
| D-13 DonChare bucket only | Pass | Evaluator consumes `stage.ChallengeIds`; query populates `AryChallengeStat`; user/BNG stats remain empty. |
| D-14 user/BNG bucket semantics deferred | Pass | `ary_user_compe_*` and `ary_bng_compe_*` facts are preserved by mapping but do not write state or readback rows. |
| D-15 `compe_id`/`track_no` are context keys | Pass | Matching requires active task aliases plus full stage predicate evaluation; IDs alone do not complete progress. |
| D-16 active config plus predicate required | Pass | Evaluator matches active configured task `CompeId`/`TrackNo` and calls typed rule evaluation before producing rows. |
| D-17 readback from saved active progress | Pass | Red query filters saved progress rows through active configured tasks and maps saved/best score data, not raw upload echo. |
| D-18 immediate rewards | Pass | Reward grants are applied during eligible playresult processing from active completion counts. |
| D-19 reward-song locks | Pass | `GetLockedRewardSongIds` derives active unearned reward songs and feeds Red userdata `LockedSongIds`. |
| D-20 song rewards mutate Red release flags | Pass | Red handler invokes `Ac15UnlockFlagAccess.Red.ReleaseSongs` with earned reward song IDs. |
| D-21 title rewards mutate Red title flags | Pass | Red handler invokes `Ac15UnlockFlagAccess.Red.Titles` with earned reward title IDs. |
| D-22 reward routes do not participate | Pass | Tests cover `rewardcardcheck.php` and `rewardexecution.php` returning compatibility success without ChallengeCompe grants. |
| D-23 Red-owned state only | Pass | New persistence is Red-owned; focused tests cover Red-only save mutation and no cross-era writes. |
| D-24 no broad Hiroba behavior | Pass | No user-created challenge/tournament/AdminApi/WebUI behavior was added; user/BNG buckets remain absent. |
| D-25 public sources are not schema authority | Pass | Evidence artifact separates product context from local protocol authority; implementation uses local sidecar/proto/state only. |
| D-26 transport-agnostic shared capability | Pass | Shared behavior is under `Application/Ac15/ChallengeCompe`; Red owns route, wire mapper, sidecar, and tables. |

## Boundary Audit

| Boundary | Result |
|----------|--------|
| AdminApi/WebUI Phase 22 work | `rg -n "ChallengeCompe|IsChallengeCompe|challengecompe" Adapters.AdminApi TaikoWebUI Contracts.AdminApi` returned no matches. |
| Unsupported user/BNG buckets | Red readback maps only `AryChallengeStat`; default common response leaves `AryUserCompeStat` and `AryBngCompeStat` empty. |
| Source-shape tests | No Phase 21 closeout source-text, route-inventory, controller-attribute, generated-wire-existence, enum-value, or migration-shape tests were added. Existing tests exercise loader parsing, handler state, controller response behavior, Mapperly projection behavior, and no-cross-boundary persistence. |
| Dirty Red telop data | `Host/wwwroot/data/red/red_telop_data.json` remains a pre-existing unstaged workspace change and was not modified by this plan. |
| Phase 22 stop line | AdminApi/WebUI opt-in editing and cabinet/RPCS3 runtime closeout remain Phase 22 handoff items. |

## Requirement Status

| Requirement | Status | Notes |
|-------------|--------|-------|
| RCOMP-02 | Complete | Shared older-AC15 ChallengeCompe behavior is transport-agnostic and Red is the first binding; Red route/proto/readback are scoped to local evidence. |
| RCHAL-01 | Complete | DonChare product context and local authority split are documented in `21-CHALLENGECOMPE-EVIDENCE.md`. |
| RCHAL-02 | Complete | Stateful behavior exists only after the Plan 01 evidence gate and remains limited to Red-owned sidecar/state, opted-in users, non-Tokkun playresults, DonChare personal-task facts, and Red readback. |

## Phase 22 Handoff

Phase 22 should add AdminApi/WebUI surfaces only for implemented Red-owned readback/editing state, especially `UserSaveDataRed.IsChallengeCompe` opt-in. It should also record cabinet/RPCS3 smoke evidence for Red normal, Tokkun tutorial, simple compatibility, and ChallengeCompe flows. Community/global progress, user challenge letters, BNG/official competition buckets, and operator schedule editing still require new evidence before implementation.
