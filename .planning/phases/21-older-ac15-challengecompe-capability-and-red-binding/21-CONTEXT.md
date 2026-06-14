# Phase 21: Older-AC15 ChallengeCompe Capability and Red Binding - Context

**Gathered:** 2026-06-14
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 21 defines the shared older-AC15 DonChare/ChallengeCompe capability and binds the first implementation through Red. It may add a shared older-AC15 challenge schema, Red-owned sidecar data, Red-owned opt-in/progress/reward state, Red `challengecompe.php` readback, Red playresult progress handling, configured reward-song locking, immediate configured song/title grants, and focused behavior/persistence tests.

This phase must keep route, wire, sidecar data, and persistent rows Red-owned while sharing only value-identical ChallengeCompe behavior. It must not implement the broader Donder Hiroba user challenge-letter, user-created tournament, or official/BNG competition systems. For Phase 21, DonChare uses `ary_challenge_*`; `ary_user_compe_*` and `ary_bng_compe_*` remain empty and evidence-gated.

Reward card and reward execution routes remain simple compatibility routes. They are not ChallengeCompe reward authorities.

</domain>

<decisions>
## Implementation Decisions

### Participation And Readback
- **D-01:** ChallengeCompe participation is explicit opt-in per Red user. Do not enroll every Red user automatically.
- **D-02:** The opt-in source is always the Red `is_challengecompe` save/userdata field. Use `UserSaveDataRed.IsChallengeCompe` as the underlying state.
- **D-03:** `is_challengecompe` means "this user has opted into ChallengeCompe". It does not mean a challenge is globally active, and it does not mean progress has been made.
- **D-04:** `challengecompe.php` should not implicitly opt a user in. The readback endpoint should read state, not mutate enrollment.
- **D-05:** Phase 22 Red AdminApi/WebUI must expose an editable field for this opt-in state. Phase 21 should create the real state contract so that UI field has a backing behavior.
- **D-06:** ChallengeCompe progress is updated from non-Tokkun Red playresults. Tokkun uploads are excluded. Normal play and Dani uploads may update challenge progress unless later runtime evidence proves Red rejects Dani for this surface.

### Challenge Data Model
- **D-07:** Challenge task data lives in a Red-owned JSON sidecar under `Host/wwwroot/data/red`, using a shared older-AC15 schema. Do not put the first implementation in a shared-only data file.
- **D-08:** The schema models monthly DonChare bundles: active/available window, 10 personal tasks, 1 community task, reward thresholds, and configured song/title rewards.
- **D-09:** Completion rules are typed predicates, not free-form prose. Supported rule families should include clear, full combo, score threshold, song-set count, and community count where the local data can express them. Unknown rule types stay unsupported.
- **D-10:** Reward song/title IDs and thresholds are catalog metadata. The catalog describes what can be earned; reward mutation behavior is handled by ChallengeCompe runtime logic.
- **D-11:** ChallengeCompe can be configured or disabled. If disabled, no challenge reward-song lock applies. If enabled and active, configured reward songs are locked until earned.

### Progress And Proto Buckets
- **D-12:** Persist both raw uploaded challenge facts and derived per-task progress/completion in Red-owned ChallengeCompe tables.
- **D-13:** For DonChare in Phase 21, use only `ary_challenge_id` uploads and `ary_challenge_stat` readback. Leave `ary_user_compe_id`, `ary_bng_compe_id`, `ary_user_compe_stat`, and `ary_bng_compe_stat` empty.
- **D-14:** User challenge letters, user-created tournaments, and official/BNG competition bucket semantics are separate Donder Hiroba features. They need their own evidence before using `ary_user_compe_*` or `ary_bng_compe_*`.
- **D-15:** `compe_id` and `track_no` are trusted client context keys, not progress by themselves. `compe_id` identifies the competition/task context and `track_no` identifies the played song position in the credit; progress must be computed from the full playresult facts plus the matching task definition.
- **D-16:** A stage advances progress only when the uploaded `compe_id`/`track_no` matches active configured ChallengeCompe data and the stage satisfies the typed predicate for that task.
- **D-17:** `challengecompe.php` should return active DonChare progress in `ary_challenge_stat`, with track stats from saved progress or best scores. Do not return only completed rows, and do not echo the latest raw upload as the source of truth.

### Reward Authority
- **D-18:** Grant configured ChallengeCompe rewards immediately when the local completion condition matches. Do not simulate exact next-day timing.
- **D-19:** When ChallengeCompe is configured and active, configured reward songs are locked for opted-in Red users until earned. When ChallengeCompe is disabled or there is no active config, ChallengeCompe does not lock those songs.
- **D-20:** Song rewards mutate Red release-song state when earned.
- **D-21:** Title rewards mutate Red title flag state immediately when the configured 10-task condition is met.
- **D-22:** `rewardcardcheck.php` and `rewardexecution.php` do not participate in ChallengeCompe rewards. Keep them as simple compatibility unless later evidence proves a state-changing role.

### Boundaries
- **D-23:** ChallengeCompe state is Red-owned for this binding. Do not write Green, Blue, Yellow, Nijiiro, or shared gameplay rows.
- **D-24:** Do not implement broad Donder Hiroba user challenge/tournament behavior as part of this phase.
- **D-25:** Do not use public wiki/blog data as endpoint/schema authority. Product sources define scope; Red proto, runtime logs, local data, and IDA/client evidence define server behavior.

### The Agent's Discretion
- The planner may choose exact JSON file names, DTO names, predicate enum names, table names, and plan splits.
- The planner may decide whether to implement challenge catalog loading, playresult mutation, readback, and reward locking in one plan or checkpoint-sized plans.
- The planner may include an intentionally empty Red challenge sidecar if the feature is supported but no operator-authored schedule is provided.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Phase Scope And Requirements
- `.planning/ROADMAP.md` - Phase 21 scope, success criteria, and evidence gate.
- `.planning/REQUIREMENTS.md` - RCOMP-02, RCHAL-01, RCHAL-02, and out-of-scope boundaries.
- `.planning/STATE.md` - Current phase position after Phase 20.1 closeout.
- `.planning/PROJECT.md` - Evidence hierarchy, Red milestone scope, and ChallengeCompe as shared older-AC15 capability.

### Prior Phase Context
- `.planning/phases/18-red-evidence-and-capability-foundation/18-RED-EVIDENCE.md` - Red route/proto/IDB evidence for `challengecompe.php`, playresult challenge arrays, and userdata `is_challengecompe`.
- `.planning/phases/18-red-evidence-and-capability-foundation/18-CONTEXT.md` - Decision that Red ChallengeCompe is shared older-AC15 candidate, not Red-only state.
- `.planning/phases/20-red-runtime-capability-binding-and-simple-compatibility/20-CONTEXT.md` - Red runtime boundaries, Tokkun-before-challenge ordering, and challenge-array preservation without state.
- `.planning/phases/20-red-runtime-capability-binding-and-simple-compatibility/20-VERIFICATION.md` - Proof that Phase 20 mapped challenge facts but added no ChallengeCompe state.
- `.planning/phases/20.1-ac15-capability-dto-and-mapper-boundary-refactor/20.1-CONTEXT.md` - Mapper/response boundary rules and ChallengeCompe response refactor-later context.
- `.planning/phases/20.1-ac15-capability-dto-and-mapper-boundary-refactor/20.1-AUDIT.md` - Notes that ChallengeCompe state/readback semantics belong to Phase 21.

### Proto And Code Evidence
- `proto/red/taiko.proto` - Red source protocol for `UserDataResponse.is_challengecompe`, `PlayResultRequest.StageData.ary_challenge_id`, `ary_user_compe_id`, `ary_bng_compe_id`, and `ChallengeCompeRequest/Response`.
- `Adapters.GameProtocol.Red/Controllers/ChallengeCompeController.cs` - Current probe-only Red route to replace with evidence-backed behavior.
- `Adapters.GameProtocol.Red/Mappers/PlayResultMappers.cs` - Existing Red mapping for challenge arrays into `Ac15RedChallengeCompeFacts`.
- `Application/Dtos/Ac15/Ac15PlayResultInput.cs` - Current canonical Red challenge fact records.
- `Application/Dtos/CommonChallengeCompeResponse.cs` - Current shared response shape for the three ChallengeCompe buckets.
- `Application/Handlers/UpdatePlayResultCommand.Red.cs` - Red playresult flow where Tokkun is checked before normal/Dani and where ChallengeCompe mutation should integrate.
- `Application/Ac15/RedAc15UserDataAdapter.cs` - Readback of `UserSaveDataRed.IsChallengeCompe` into AC15 userdata counters.
- `Application/Handlers/GetChallengeCompeQuery.cs` - Existing Green/Yellow-only empty handler to extend or replace for Red.

### Product Context
- `https://web.archive.org/web/20120701220318/http://taikoblog.namco-ch.net/blog/2012/06/post_307.html` - Official 2012 DonChare introduction: monthly 10+1 tasks, participation through Donder Hiroba, 8-task song and 10-task title rewards, later reward timing, and separate tournament/challenge-letter context.
- `https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E3%83%89%E3%83%B3%E3%83%81%E3%83%A3%E3%83%AC` - Wiki DonChare product summary: monthly structure, Tokkun exclusion, reward thresholds, and Yellow-era pause context.
- `https://donderhiroba.jp/other_faq.php` - Official Donder Hiroba FAQ proving tournaments and challenge letters are separate Hiroba features with their own rules.
- `https://taiko-ch.net/blog/?m=20170220` - Official Red transition notice proving Red-era tournament/challenge-letter creation had separate lifecycle handling around Yellow transition.
- `https://taiko.namco-ch.net/taiko/en/donhiro/` - Current official Donder Hiroba overview listing friends, tournaments, and challenges as Hiroba features.

### Codebase Maps
- `.planning/codebase/ARCHITECTURE.md` - AC15 shared-core boundary, direct `ITaikoDbContext`, era-owned persistence, and adapter-owned wire DTO rules.
- `.planning/codebase/STACK.md` - Current .NET, EF Core, protobuf-net, Mapperly, and test stack.
- `.planning/codebase/INTEGRATIONS.md` - Game protocol, SQLite, filesystem sidecar, and AdminApi/WebUI integration surfaces.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `UserSaveDataRed.IsChallengeCompe` already exists and is mapped through `RedAc15UserDataAdapter` and `Ac15UserDataService`; use it as the opt-in flag.
- `Ac15RedChallengeCompeFacts` and `Ac15RedChallengeCompeStageFacts` already preserve Red playresult challenge arrays.
- `CommonChallengeCompeResponse` already matches the three response buckets, but Phase 21 should populate only `AryChallengeStat` for DonChare.
- `Ac15NormalPlayWriter`, `Ac15DaniWriter`, Red normal-play tables, and Red title/release flag helpers are the nearby patterns for safe Red-owned mutation.
- Red catalog/data path work from Phase 19 should be reused for a Red sidecar loader instead of hardcoding checkout paths.

### Established Patterns
- Controllers deserialize wire DTOs, map/send Mediator requests, and map application responses back to generated wire DTOs.
- Shared AC15 behavior belongs in `Application/Ac15`, but persistence remains direct `ITaikoDbContext` plus concrete Red `DbSet` ownership.
- Mapperly stays mechanical. ChallengeCompe business rules belong in application services/handlers, not adapter mappers.
- Server-authored data for an era-supported feature should exist under that era's `Host/wwwroot/data/<era>` area and be copied to output.

### Integration Points
- Replace `Adapters.GameProtocol.Red/Controllers/ChallengeCompeController.cs` probe behavior with Mediator-backed Red readback.
- Add a Red branch or older-AC15 replacement for `GetChallengeCompeQuery` that returns DonChare `AryChallengeStat` rows.
- Add Red-owned ChallengeCompe persistence in `Domain/Entities`, `Application/Abstractions/ITaikoDbContext.Red.cs`, `Infrastructure/Persistence/TaikoDbContext.Red.cs`, and a migration.
- Integrate progress update in `UpdatePlayResultCommand.Red.cs` after Tokkun exclusion and before or alongside normal/Dani writes.
- Add Red sidecar loader under `Infrastructure/GameDataCatalog/Red` or shared AC15 catalog helpers with Red-owned registration.
- Extend Red reward-song locking through Red userdata/initial-data/release-song readback paths only when active ChallengeCompe config exists.
- Add tests over observable handler/service behavior, SQLite persistence, readback response shape, no Tokkun challenge writes, no user/BNG bucket output, and reward lock/grant behavior.

</code_context>

<specifics>
## Specific Ideas

- Use `ary_challenge_*` for DonChare and leave the other two buckets empty. This is a deliberate scope choice, not a claim that `ary_user_compe_*` and `ary_bng_compe_*` are meaningless.
- `compe_id` plus `track_no` identify the challenge context for the uploaded stage. They do not by themselves prove completion.
- Challenge progress is computed by applying the current playresult stage facts to the configured predicate for that `compe_id`.
- Exact next-day reward timing is not required for local support. Immediate grant is acceptable once the configured condition matches.
- When ChallengeCompe is enabled/configured, reward songs should be locked until earned. When the feature is disabled or no active config exists, ChallengeCompe should not lock songs.

</specifics>

<deferred>
## Deferred Ideas

- Phase 22 Red AdminApi/WebUI should expose an editable ChallengeCompe opt-in field backed by `UserSaveDataRed.IsChallengeCompe`.
- User challenge letters, user-created tournaments, official/BNG competition rows, and any `ary_user_compe_*` or `ary_bng_compe_*` behavior require separate evidence before implementation.
- Admin editing of challenge schedules, operator schedule management UI, or broader ChallengeCompe reward-management tooling remains future scope after the runtime contract exists.

</deferred>

---

*Phase: 21-Older-AC15 ChallengeCompe Capability and Red Binding*
*Context gathered: 2026-06-14*
