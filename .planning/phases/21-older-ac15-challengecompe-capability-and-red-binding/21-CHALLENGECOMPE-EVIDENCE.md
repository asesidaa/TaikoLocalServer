# Phase 21 ChallengeCompe Evidence Gate

**Created:** 2026-06-14
**Scope:** Shared older-AC15 ChallengeCompe / DonChare capability and the first Red binding.

## Evidence Authority

| Source | Authority | Use |
|--------|-----------|-----|
| `proto/red/taiko.proto` | Local protocol authority | Defines the Red request, response, userdata opt-in field, and playresult challenge fact arrays. |
| `.planning/phases/18-red-evidence-and-capability-foundation/18-RED-EVIDENCE.md` | Local route and IDB authority | Records the Red `/v08r01/chassis/challengecompe.php` route and classifies ChallengeCompe as Phase 21 scope. |
| `Adapters.GameProtocol.Red/Controllers/ChallengeCompeController.cs` | Current implementation evidence | Shows the route is currently log-and-empty-success compatibility only. |
| `Adapters.GameProtocol.Red/Mappers/PlayResultMappers.cs` | Current fact-preservation evidence | Maps Red playresult challenge arrays into application ChallengeCompe fact records without state mutation. |
| Phase 21 decisions D-01 through D-26 | Approved implementation constraints | Define opt-in, typed task data, Red-owned sidecar/state, Tokkun exclusion, DonChare bucket scope, and public-source limitations. |
| Public DonChare wiki/blog pages | Product context only | Describe feature scope and user-visible concepts. They are not endpoint, payload, table, state, or response authority. |

## Product Context Only

Public DonChare material is useful only for naming the gameplay surface and documenting operator/user intent:

- DonChare is framed as a monthly challenge feature.
- Product descriptions mention 10 personal tasks plus 1 community task.
- Product descriptions distinguish personal progress from community progress.
- Product descriptions associate normal song play with task completion.
- Product descriptions exclude Tokkun/practice play.
- Product descriptions mention reward song and title thresholds/timing.
- Donder Hiroba tournaments and challenge letters are separate feature families, not DonChare task-progress authority.

Per D-25, these public facts do not authorize server endpoint behavior, payload fields, response rows, persistence tables, reward timing, or client acceptance semantics.

## Local Red Proto Evidence

`proto/red/taiko.proto` contains the Red ChallengeCompe transport surfaces:

- `ChallengeCompeRequest`
  - `baid`
  - `chassis_id`
  - `shop_id`
- `ChallengeCompeResponse`
  - `result`
  - `ary_challenge_stat`
  - `ary_user_compe_stat`
  - `ary_bng_compe_stat`
- `ChallengeCompeResponse.CompeData`
  - `compe_id`
  - `ary_track_stat`
- `ChallengeCompeResponse.TracksData`
  - `song_no`
  - `level`
  - `option_flg`
  - `stage_mode`
  - `high_score`
- `UserDataResponse.is_challengecompe`
- `PlayResultRequest.StageData.ary_challenge_id`
- `PlayResultRequest.StageData.ary_user_compe_id`
- `PlayResultRequest.StageData.ary_bng_compe_id`
- `PlayResultRequest.StageData.ResultcompeData`
  - `compe_id`
  - `track_no`

The Red proto proves that the client has a standalone ChallengeCompe readback endpoint, an opt-in-looking userdata field, and three playresult challenge fact buckets. It does not by itself prove task semantics, persistence schema, reward timing, community aggregation, or accepted non-empty response behavior.

## Local Red Route Evidence

`18-RED-EVIDENCE.md` records the Red game route suffix `chassis/challengecompe.php` at IDB address `0xDAA758`, under the Red game route family `/v08r01/chassis/*`.

The current Red controller exposes:

- `/v08r00_tw/chassis/challengecompe.php`
- `/v08r01/chassis/challengecompe.php`

The current implementation logs the request and returns `ChallengeCompeResponse { Result = 1 }`. This proves compatibility routing only. Empty success is not stateful ChallengeCompe support.

## Playresult Fact Preservation

`Adapters.GameProtocol.Red/Mappers/PlayResultMappers.cs` already preserves challenge facts from every stage with challenge data:

- `ary_challenge_id` -> `Ac15RedChallengeCompeStageFacts.ChallengeIds`
- `ary_user_compe_id` -> `Ac15RedChallengeCompeStageFacts.UserCompeIds`
- `ary_bng_compe_id` -> `Ac15RedChallengeCompeStageFacts.BngCompeIds`

Those facts are available to application code as `Ac15RedChallengeCompeFacts`. Phase 20 intentionally did not write ChallengeCompe state from them.

## Initial Compatibility State Before Stateful Plans

At Phase 21 start, Red ChallengeCompe support was limited to:

- Route availability for the proven Red route.
- Request logging.
- Empty success response.
- Preservation of uploaded playresult challenge IDs as application facts.
- Userdata readback of `UserSaveDataRed.IsChallengeCompe` through the AC15 userdata response.

At Phase 21 start, support did not include:

- Challenge task catalog loading.
- Stateful progress persistence.
- Reward song or title grants.
- Reward-song locking.
- Non-empty `ary_challenge_stat` readback.
- `ary_user_compe_stat` or `ary_bng_compe_stat` behavior.
- Community progress aggregation.
- Any cross-era ChallengeCompe table.

## Accepted Phase 21 Stateful Surface

If stateful implementation proceeds, the accepted Phase 21 surface is restricted to the local evidence and decisions:

- Red remains the first binding of a shared older-AC15 transport-agnostic ChallengeCompe capability.
- Red task configuration is Red-owned sidecar JSON under `Host/wwwroot/data/red`.
- DonChare uses `ary_challenge_id` uploads and `ary_challenge_stat` readback.
- `ary_user_compe_id`, `ary_bng_compe_id`, `ary_user_compe_stat`, and `ary_bng_compe_stat` remain evidence-gated and empty for this phase.
- Enrollment comes from `UserSaveDataRed.IsChallengeCompe`.
- `challengecompe.php` readback must not mutate enrollment.
- Tokkun playresults must not update ChallengeCompe state.
- Red persistence, if added by later Phase 21 plans, must be Red-owned.
- Shared application logic may evaluate transport-agnostic typed rules, but it must not own shared gameplay tables, Red route names, Red wire DTOs, or public-source schema.

## Phase 21 Final Verified Outcome

Phase 21 implemented stateful ChallengeCompe only inside the accepted surface above:

- Red sidecar loading uses `Host/wwwroot/data/red/red_challenge_compe_data.json`, which is disabled by default and parsed into shared transport-agnostic ChallengeCompe catalog records.
- Red-owned SQLite state stores matched raw DonChare facts and derived progress rows in `RedChallengeCompeRawFacts` and `RedChallengeCompeProgress`.
- Non-Tokkun Red playresults update ChallengeCompe state only for users opted in through `UserSaveDataRed.IsChallengeCompe`, active configured personal tasks, and matched `ary_challenge_id` facts.
- Tokkun playresults return through the Tokkun branch before ChallengeCompe mutation and do not create ChallengeCompe, normal, Dani, or reward state.
- Configured rewards grant immediately from saved active completion counts and mutate only Red release-song/title flags.
- Active unearned configured reward songs are supplied to the existing AC15 userdata locked-song path for opted-in Red users.
- Red `challengecompe.php` is Mediator-backed and read-only. It returns active saved progress in `ary_challenge_stat` and leaves `ary_user_compe_stat` and `ary_bng_compe_stat` empty.
- Red wire projection is mechanical Mapperly mapping; shared business behavior remains in application/catalog code.

Phase 21 still does not implement user-created challenge letters, BNG/official competition buckets, community/global aggregation, AdminApi/WebUI opt-in editing, or cabinet/RPCS3 acceptance proof. Those remain evidence-gated or Phase 22 scope.

## Explicit Gaps

Current local evidence still does not prove:

- Real operator-authored Red task schedule data.
- The exact accepted non-empty `ary_challenge_stat` row set on cabinet/RPCS3.
- Whether the client expects rows for all active tasks, only completed tasks, or latest uploaded facts.
- Community/global count semantics.
- User challenge or BNG/official competition bucket semantics.
- Exact production reward timing.
- Cabinet/RPCS3 acceptance of configured task data beyond empty success.

These gaps do not block creating a disabled-safe catalog contract, but they limit any stateful implementation to the local Red proto route, playresult fact shape, Red sidecar data, and approved Phase 21 decisions.

## Stateful Execution Verdict

**Verdict: PROCEED WITH DISABLED-SAFE CATALOG CONTRACT AND EVIDENCE-GATED STATEFUL PLANS.**

Plans 02-04 may proceed only within the accepted surface above: Red-owned data/state, DonChare `ary_challenge_*` only, typed local sidecar rules, Tokkun exclusion, no user/BNG buckets, and no public-source schema authority. If implementation cannot cite Red proto/runtime/client/local data evidence for endpoint, payload, state shape, and accepted response contract, it must record the gap and leave that stateful behavior absent per RCHAL-02.

Empty success remains compatibility only and must not be counted as stateful ChallengeCompe support.
