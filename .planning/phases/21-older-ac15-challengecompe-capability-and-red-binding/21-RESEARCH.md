# Phase 21: Older-AC15 ChallengeCompe Capability and Red Binding - Research

**Researched:** 2026-06-14
**Domain:** ASP.NET Core game protocol adapters, AC15 ChallengeCompe, Red protocol binding, era-owned SQLite persistence
**Confidence:** MEDIUM-HIGH

## Summary

Phase 21 should model ChallengeCompe as a shared older-AC15 capability with Red as the first binding, not as a Red-only endpoint. Red currently proves a standalone route/proto surface, while the user note adds an important design constraint: older AC15 versions may bind the same behavior through other transport surfaces such as userdata or BAID. The shared capability therefore belongs in application/catalog behavior, with era-owned routes, protos, tables, and adapters binding into it.

The practical Phase 21 plan is to first record the evidence boundary, then build a Red-owned task catalog and Red-owned progress persistence around the proven Red proto fields. Stateful behavior must remain limited to the DonChare bucket proven by Red proto and context decisions: `ary_challenge_id` uploads and `ary_challenge_stat` readback. `ary_user_compe_*` and `ary_bng_compe_*` remain empty until separate user-challenge or official-tournament evidence exists.

## Evidence Hierarchy

| Rank | Source | Use In Phase 21 |
|------|--------|-----------------|
| 1 | Red proto, Red route/controller, Red playresult mapper, local IDA/client traces when present | Endpoint/payload/response authority. |
| 2 | Phase 21 context decisions D-01 through D-26 | Scope and implementation constraints approved by discussion. |
| 3 | Existing repo patterns in Red/AC15 handlers, catalogs, DbContext, and tests | Implementation shape. |
| 4 | Public DonChare/wiki/blog pages | Product context only. Never endpoint/schema authority per D-25. |

## Product Scope Context

Public DonChare context supports the phase requirement to document scope, but it must not define server schema. The wiki page for DonChare describes the old recurring structure as monthly 10 personal tasks plus 1 community task, user-visible progress, score/clear/full-combo style rules, Tokkun exclusion, and reward unlock thresholds. The current official 2026 blog page confirms modern DonChare still frames the event as clearing songs under task conditions for rewards.

Sources used only for product context:

- https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E3%83%89%E3%83%B3%E3%83%81%E3%83%A3%E3%83%AC
- https://taiko-ch.net/blog/?p=16135

## Local Protocol Findings

| Finding | Evidence | Planning Impact |
|---------|----------|-----------------|
| Red has a ChallengeCompe route. | `Adapters.GameProtocol.Red/Controllers/ChallengeCompeController.cs` routes `/v08r00_tw/chassis/challengecompe.php` and `/v08r01/chassis/challengecompe.php`. | Plan Red route binding through the existing controller instead of adding a new route family. |
| Red route is currently empty success. | Controller logs request and returns `ChallengeCompeResponse { Result = 1 }`. | Empty success is compatibility only; it is not stateful support. |
| Shared query exists but excludes Red. | `Application/Handlers/GetChallengeCompeQuery.cs` supports Green and Yellow only. | Add a Red branch when stateful Red readback is implemented. |
| Current shared response DTO matches the older proto bucket names. | `Application/Dtos/CommonChallengeCompeResponse.cs` exposes `AryChallengeStat`, `AryUserCompeStat`, and `AryBngCompeStat`. | Reuse as a readback DTO if it stays behavior-level and not Red-wire-owned. |
| Red playresult mapper preserves challenge facts. | `Adapters.GameProtocol.Red/Mappers/PlayResultMappers.cs` maps `ary_challenge_id`, `ary_user_compe_id`, and `ary_bng_compe_id` into `Ac15RedChallengeCompeFacts`. | Phase 21 can consume preserved facts without changing wire DTOs. |
| Red playresult handler does not mutate ChallengeCompe state. | `Application/Handlers/UpdatePlayResultCommand.Red.cs` currently handles Tokkun, normal play, Don points/profile, Dani, and normal results only. | Add ChallengeCompe mutation after Tokkun exclusion and around validated non-Tokkun stage facts. |
| Red save data already contains opt-in state. | `Domain/Entities/UserSaveDataRed.cs` has `IsChallengeCompe`; Red userdata maps it to `is_challengecompe`. | Do not invent a new enrollment table. Use `UserSaveDataRed.IsChallengeCompe`. |
| Red has no ChallengeCompe persistence today. | `ITaikoDbContext.Red.cs` and `TaikoDbContext.Red.cs` expose no ChallengeCompe DbSets. | Add Red-owned raw fact and derived progress tables if implementing stateful behavior. |
| Existing catalog sidecars are era-owned. | `Infrastructure/GameDataCatalog/Red/RedEraGameDataCatalog.cs` loads Red JSON sidecars through `PathHelper.GetDataPath(GameEra.Red)`. | Add a Red JSON sidecar using the existing catalog loader style. |
| Song lock/readback behavior already exists for AC15. | `Ac15UserDataService` clears locked song IDs from release flags. | Challenge reward-song locks should feed Red userdata locked song IDs, not implement separate wire mutation. |

## Shared Capability Boundary

The shared older-AC15 ChallengeCompe capability should define canonical behavior concepts only:

- enrollment/availability: opted-in user plus active catalog window
- task catalog: monthly bundle, personal tasks, community task metadata, typed predicates, reward metadata
- uploaded facts: challenge IDs from playresult stages plus `compe_id` and `track_no` context
- derived progress: per-task progress/completion and best track stats for readback
- reward grants: configured song/title mutations when completion thresholds are met
- readback shape: active progress rows for the transport that the era proves

It should not define a shared route, shared database table, or shared protocol DTO. Red owns its routes, generated wire types, controller mapper, and persistence tables. Older versions can later bind the same capability through userdata, BAID, or other protocol surfaces when their own evidence proves that transport.

## Recommended Implementation Shape

| Layer | Responsibility | Files / Analogs |
|-------|----------------|-----------------|
| Domain | Red-owned raw challenge fact and derived progress entities. | `Domain/Entities/*Red.cs`, `Domain/Entities/UserSaveDataRed.cs` |
| Application/Ac15 | Shared typed predicate evaluation, reward decision, and readback model. | `Application/Ac15/Ac15UserDataService.cs`, `Application/Ac15/Ac15UnlockFlagAccess.cs` |
| Application/Dtos | Keep request/response shapes behavior-level. Do not persist wire DTOs. | `Application/Dtos/CommonChallengeCompeResponse.cs`, `Application/Dtos/Ac15/Ac15PlayResultInput.cs` |
| Application/Handlers | Red playresult mutation, Red readback query, Red userdata lock integration. | `UpdatePlayResultCommand.Red.cs`, `GetChallengeCompeQuery.Red.cs`, `UserDataQuery.Red.cs` |
| Infrastructure | Red catalog sidecar loading and Red DbContext mapping/migration. | `RedEraGameDataCatalog.cs`, `TaikoDbContext.Red.cs`, EF migrations |
| Adapters | Red controller maps Red wire request/response to/from application DTOs. | `ChallengeCompeController.cs`, Red mapper partials |
| Tests | Observable persistence/readback/lock/reward/no-cross-mode guards. | `Tests/Red/*`, `Tests/Ac15/*` |

## Scope Guardrails

- Do not auto-enroll Red users. Enrollment is `UserSaveDataRed.IsChallengeCompe`.
- Do not let `challengecompe.php` mutate enrollment.
- Do not update ChallengeCompe from Tokkun playresults.
- Do not write normal score/crown/Dani/profile/favorite/recent state from ChallengeCompe logic.
- Do not write Green, Blue, Yellow, Nijiiro, or shared gameplay tables.
- Do not populate `ary_user_compe_stat` or `ary_bng_compe_stat`.
- Do not use wiki/blog pages as endpoint authority.
- Do not add broad Donder Hiroba user challenge or tournament behavior.
- Do not use `rewardcardcheck.php` or `rewardexecution.php` for ChallengeCompe rewards.

## Open Design Notes

| Note | Resolution For Plan |
|------|---------------------|
| Public sources mention next-day reward timing. | Phase 21 grants configured rewards immediately per D-18; do not simulate exact Hiroba timing. |
| Community task semantics are product-scoped but not locally proven. | Catalog may describe community metadata, but Phase 21 should not derive global community progress unless local data/evidence exists. |
| Older AC15 versions may not call `challengecompe.php`. | Keep shared behavior transport-agnostic and bind Red through its proven route only. |
| Red task data is not committed yet. | Add a Red sidecar contract and a starter JSON file that can be disabled or empty without breaking runtime. |

## Validation Architecture

### Test Infrastructure

| Property | Value |
|----------|-------|
| Framework | xUnit in `Tests/Tests.csproj` |
| Focused command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedChallengeCompe|FullyQualifiedName~RedPlayResultHandlerTests|FullyQualifiedName~RedProtocolMapperTests|FullyQualifiedName~Ac15UserDataService"` |
| Full suite command | `dotnet test Tests/Tests.csproj` |
| Build command | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` |

### Behaviors To Sample

| Behavior | Requirement | Verification Type |
|----------|-------------|-------------------|
| Product scope record separates public context from local protocol authority. | RCHAL-01, RCOMP-02 | Research/evidence artifact review. |
| Red catalog sidecar loads enabled/disabled challenge data through era catalog paths. | RCOMP-02 | Catalog parser tests. |
| Non-Tokkun Red playresult challenge facts persist raw and derived progress only when enrolled and active. | RCHAL-02 | SQLite handler tests. |
| Tokkun playresult challenge-looking data writes no ChallengeCompe, normal, Dani, or reward state. | RCHAL-02 | SQLite no-write test. |
| Red reward song lock affects userdata release flags only while enabled/active and until earned. | RCHAL-02 | Handler/service readback test. |
| Song/title rewards mutate Red release/title flags when configured thresholds complete. | RCHAL-02 | SQLite handler test. |
| `challengecompe.php` returns active progress rows in `ary_challenge_stat` and empty user/bng buckets. | RCOMP-02, RCHAL-02 | Controller/mapper or handler readback test. |
| Red ChallengeCompe state does not write other era tables. | RCOMP-02 | SQLite boundary assertions. |

### Manual Verification

- Record in `21-CHALLENGECOMPE-EVIDENCE.md` which facts are local protocol/runtime evidence and which facts are product context only.
- If execution cannot prove a stateful contract beyond empty success, record the gap and stop before stateful mutation plans.

## Sources

### Primary Local

- `AGENTS.md`
- `.planning/ROADMAP.md`
- `.planning/REQUIREMENTS.md`
- `.planning/phases/21-older-ac15-challengecompe-capability-and-red-binding/21-CONTEXT.md`
- `.planning/phases/18-red-evidence-and-capability-foundation/18-RED-EVIDENCE.md`
- `.planning/phases/20-red-runtime-capability-binding-and-simple-compatibility/20-CONTEXT.md`
- `.planning/phases/20-red-runtime-capability-binding-and-simple-compatibility/20-VERIFICATION.md`
- `proto/red/taiko.proto`
- `Adapters.GameProtocol.Red/Controllers/ChallengeCompeController.cs`
- `Adapters.GameProtocol.Red/Mappers/PlayResultMappers.cs`
- `Application/Handlers/UpdatePlayResultCommand.Red.cs`
- `Application/Handlers/GetChallengeCompeQuery.cs`
- `Application/Dtos/CommonChallengeCompeResponse.cs`
- `Application/Dtos/Ac15/Ac15PlayResultInput.cs`
- `Domain/Entities/UserSaveDataRed.cs`
- `Infrastructure/GameDataCatalog/Red/RedEraGameDataCatalog.cs`

### Product Context Only

- DonChare wiki context: https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E3%83%89%E3%83%B3%E3%83%81%E3%83%A3%E3%83%AC
- Official 2026 DonChare blog context: https://taiko-ch.net/blog/?p=16135

## Metadata

**Confidence breakdown:**
- Local protocol surface: HIGH, from current Red proto/controller/mapper source.
- Shared behavior boundary: HIGH, from Phase 21 context and user note D-26.
- Product details: MEDIUM, from public pages and treated as non-authoritative context.
- Stateful runtime contract: MEDIUM, because empty success route exists today and execution must record concrete local acceptance evidence before claiming support.

## RESEARCH COMPLETE
