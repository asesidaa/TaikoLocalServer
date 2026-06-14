# Phase 21 Summary - Older-AC15 ChallengeCompe Capability and Red Binding

**Completed:** 2026-06-14  
**Outcome:** Red now binds the shared older-AC15 ChallengeCompe capability through local Red evidence, Red-owned state, configured rewards, userdata locks, and read-only Red wire readback.

## Implemented

- Recorded the ChallengeCompe evidence gate separating public DonChare product context from local protocol, route, data, state, and client authority.
- Added a shared transport-agnostic ChallengeCompe catalog model and disabled-safe Red sidecar loading from `Host/wwwroot/data/red/red_challenge_compe_data.json`.
- Added Red-owned raw fact and derived progress persistence for matched DonChare `ary_challenge_id` facts from opted-in, non-Tokkun Red playresults.
- Added shared typed predicate evaluation for clear, full-combo, score-threshold, song-set-count, and unsupported rule handling.
- Added configured ChallengeCompe song/title reward grants through Red release/title flags, plus active unearned reward-song locks through existing AC15 userdata behavior.
- Replaced Red empty-success-only `challengecompe.php` with Mediator-backed, read-only `ary_challenge_stat` readback from saved active progress.
- Added focused observable tests for catalog parsing, SQLite state/no-write behavior, Tokkun exclusion, reward grants, userdata locks, readback, Mapperly projection, and unsupported bucket boundaries.

## Boundaries Preserved

- ChallengeCompe route, wire DTOs, sidecar data, and persistence rows remain Red-owned for this binding.
- Shared ChallengeCompe behavior is transport-agnostic under `Application/Ac15/ChallengeCompe`; it does not own Red routes, Red wire DTOs, or shared gameplay tables.
- `ary_user_compe_*`, `ary_bng_compe_*`, user-created challenge letters, official/BNG competition rows, and community/global aggregation remain evidence-gated and absent.
- Tokkun playresults do not update ChallengeCompe, normal play, Dani, reward, or unlock state.
- Red reward compatibility routes remain compatibility-only and do not participate in ChallengeCompe grants.
- No Phase 22 AdminApi/WebUI opt-in editing or runtime closeout work was added.

## Verification

See `21-VERIFICATION.md`.

Final closeout commands passed:

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedChallengeCompe|FullyQualifiedName~RedPlayResultHandlerTests|FullyQualifiedName~RedProtocolMapperTests|FullyQualifiedName~Ac15UserDataService"` - 30 passed.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Red"` - 83 passed after the scoped Red sidecar contract test fix.
- `dotnet test Tests/Tests.csproj` - 731 passed.
- `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` - passed with 0 warnings and 0 errors.

## Phase 22 Handoff

Phase 22 should expose only implemented Red-owned readback/editing surfaces through AdminApi/WebUI, including editable ChallengeCompe opt-in backed by `UserSaveDataRed.IsChallengeCompe`. It should also record cabinet/RPCS3 smoke evidence for Red normal, Tokkun tutorial, simple compatibility, and ChallengeCompe flows.

Community/global progress, user challenge letters, BNG/official competition buckets, operator schedule editing, and exact production reward timing still require new evidence before implementation.
