# Phase 21: Older-AC15 ChallengeCompe Capability and Red Binding - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md; this log preserves the alternatives considered.

**Date:** 2026-06-14
**Phase:** 21-Older-AC15 ChallengeCompe Capability and Red Binding
**Areas discussed:** Participation and Readback, Challenge Data Model, Progress and Proto Buckets, Reward Authority

---

## Participation And Readback

| Option | Description | Selected |
|--------|-------------|----------|
| Explicit opt-in | A user participates only when Red ChallengeCompe state says so; this matches the Hiroba-login requirement without enrolling everyone. | Yes |
| All Red users | Every Red profile is treated as participating whenever an active challenge exists; simplest but less faithful to opt-in behavior. | |
| Evidence only | Keep `is_challengecompe` false and avoid participation state until a runtime trace proves the exact trigger. | |

**User's choice:** Explicit opt-in.
**Notes:** User noted that Red WebUI needs a new field for this.

| Option | Description | Selected |
|--------|-------------|----------|
| Red save field | Use Red-owned save/profile state such as `UserSaveDataRed.IsChallengeCompe`; no shared table and no endpoint side effect. | Yes |
| Challenge row | Create participation only when a ChallengeCompe state row exists; cleaner separation but needs a new table just for enrollment. | |
| Endpoint enrolls | First `challengecompe.php` request creates opt-in state; easiest for cabinet flow but turns a readback route into a mutating route. | |

**User's choice:** Red save field.
**Notes:** User clarified: "It should always be the is challenge compe field."

| Option | Description | Selected |
|--------|-------------|----------|
| Participation flag | True means this user has opted into ChallengeCompe; progress and completion remain separate facts. | Yes |
| Active challenge | True means a current challenge is available, even before the user opts in. | |
| Any progress | True only after the user has recorded ChallengeCompe progress; this may hide opt-in before first play. | |

**User's choice:** Participation flag.
**Notes:** None.

| Option | Description | Selected |
|--------|-------------|----------|
| Normal only | Only normal Red playresult stages update progress; Tokkun and Dani stay out unless later evidence proves otherwise. | |
| Non-Tokkun only | Normal and Dani could update progress, while Tokkun stays excluded per wiki context. | Yes |
| Raw only | Persist raw ChallengeCompe arrays but do not evaluate progress from any mode yet. | |

**User's choice:** Non-Tokkun only.
**Notes:** Tokkun is excluded; Dani may count.

---

## Challenge Data Model

| Option | Description | Selected |
|--------|-------------|----------|
| Red sidecar | Use a Red-owned JSON file under `Host/wwwroot/data/red` with a shared older-AC15 schema; this keeps data era-owned while sharing behavior. | Yes |
| Shared file | Use one shared `Host/wwwroot/data/shared` file for all older-AC15 ChallengeCompe schedules; easier reuse but weaker era ownership. | |
| No catalog | Do not add task data yet; keep the endpoint minimal until IDA/runtime proof identifies the accepted response contract. | |

**User's choice:** Red sidecar.
**Notes:** None.

| Option | Description | Selected |
|--------|-------------|----------|
| Monthly bundle | Model active month, ten personal tasks, one community task, reward thresholds, and availability windows from blog/wiki scope. | Yes |
| Flat rows | Model only a list of challenge IDs and tracks; lower risk but loses the 10+1 monthly structure. | |
| Wire rows only | Store exactly the three response buckets and track stats; no monthly/task semantics yet. | |

**User's choice:** Monthly bundle.
**Notes:** None.

| Option | Description | Selected |
|--------|-------------|----------|
| Typed predicates | Use explicit rule types such as clear, full combo, score threshold, song set count, and community count; unknown task types stay unsupported. | Yes |
| Text only | Store the task description and display data, but do not evaluate completion server-side. | |
| ID echo only | Treat playresult challenge IDs as authoritative completion facts and avoid defining server-side predicates. | |

**User's choice:** Typed predicates.
**Notes:** None.

| Option | Description | Selected |
|--------|-------------|----------|
| Metadata only | Record reward song/title IDs and 8/10 thresholds as catalog facts; grant behavior is decided separately and remains evidence-gated. | Yes |
| Grant rules | Include next-day grant timing and unlock mutations directly in the catalog contract. | |
| Omit rewards | Do not model rewards until the cabinet proves a reward readback or unlock path. | |

**User's choice:** Metadata only.
**Notes:** Grant behavior was later decided under Reward Authority.

---

## Progress And Proto Buckets

| Option | Description | Selected |
|--------|-------------|----------|
| Raw plus derived | Store raw Red playresult challenge facts and derived per-task progress/completion in Red-owned ChallengeCompe tables. | Yes |
| Derived only | Store only evaluated counters/completion rows; smaller state but less auditability if field meaning is later corrected. | |
| Raw only | Persist uploaded challenge arrays without evaluating completion; safest if semantics remain unclear. | |

**User's choice:** Raw plus derived.
**Notes:** None.

| Option | Description | Selected |
|--------|-------------|----------|
| Distinct provisional | Keep `challenge`, `user_compe`, and `bng_compe` as separate categories, but do not assign personal/community meaning without proof. | |
| Map product roles | Map `challenge` to personal tasks and `bng_compe` to community tasks now, using blog/wiki product behavior. | Initial |
| Collapse together | Treat all three arrays as one challenge ID set; simpler but may lose client-visible distinctions. | |

**User's choice:** User first selected map product roles, then requested more online research about user-specified challenge features before locking the mapping.
**Notes:** Follow-up research found Donder Hiroba tournament and challenge-letter evidence separate from DonChare.

| Option | Description | Selected |
|--------|-------------|----------|
| Research gate | Keep three buckets distinct and require further online/IDA/runtime research before mapping them to DonChare, user challenge, or official/community roles. | |
| Generic categories | Implement the three buckets as configurable categories in JSON, with display names supplied by data rather than hardcoded product meanings. | |
| DonChare only | Use only the DonChare-related bucket now and leave user challenge/official competition buckets empty. | Yes |

**User's choice:** DonChare only.
**Notes:** User reasoned that the game likely only needs `compe_id` and `track_no`, with backend-side checks; use `ary_challenge_*` for ChallengeCompe/DonChare and leave the other buckets empty.

| Option | Description | Selected |
|--------|-------------|----------|
| ID plus predicate | Require the uploaded `compe_id/track_no` to match the active catalog and the stage to satisfy the typed predicate; prevents random IDs from completing tasks. | Yes |
| Trust upload IDs | If the client uploads a matching `compe_id/track_no`, count it as progress; simpler but gives the client too much authority. | |
| Predicate only | Ignore uploaded IDs and evaluate every active task against every eligible stage; may not match the cabinet's challenge selection behavior. | |

**User's choice:** ID plus predicate.
**Notes:** User clarified that the client is still trusted for context, but `compe_id` and `track_no` alone do not carry progress. Progress comes from playresult facts plus the configured task definition.

| Option | Description | Selected |
|--------|-------------|----------|
| Active progress | Return `ary_challenge_stat` rows for active DonChare entries with track stats from saved progress/best scores; leave other buckets empty. | Yes |
| Completed only | Return only rows that have completed progress; smaller response but may hide available tasks from the cabinet. | |
| Latest upload | Echo latest raw uploaded challenge facts; simple but does not represent current task state. | |

**User's choice:** Active progress.
**Notes:** None.

---

## Reward Authority

| Option | Description | Selected |
|--------|-------------|----------|
| No direct grant | Record ChallengeCompe reward eligibility/readback facts only; do not mutate release song or title flags without client/runtime proof. | |
| Delayed grant | After threshold and next-day timing, mutate Red release/title flags so the cabinet sees local unlocks. | |
| Immediate grant | Mutate unlock/title flags as soon as the threshold is reached; easiest but least faithful to source timing. | Yes |

**User's choice:** Immediate grant.
**Notes:** User said exact timing is not needed; as long as the condition matches, unlock the reward. User also noted configured ChallengeCompe reward songs should be locked until earned.

| Option | Description | Selected |
|--------|-------------|----------|
| Configured locks | A configured active ChallengeCompe reward song is locked for opted-in Red users until earned; if ChallengeCompe is disabled or no active config exists, no challenge lock applies. | Yes |
| Always lock | Reward song IDs in any ChallengeCompe JSON stay locked even if the feature is disabled. | |
| Never lock | ChallengeCompe grants rewards but does not hide reward songs before completion. | |

**User's choice:** Configured locks.
**Notes:** None.

| Option | Description | Selected |
|--------|-------------|----------|
| Immediate title flag | When the 10-task condition matches, set the Red title flag immediately using the configured title reward ID. | Yes |
| Eligibility only | Track title eligibility but do not mutate title flags until runtime proof shows cabinet readback expectations. | |
| No title reward | Ignore title rewards in Phase 21 and implement only song reward behavior. | |

**User's choice:** Immediate title flag.
**Notes:** None.

| Option | Description | Selected |
|--------|-------------|----------|
| No route role | ChallengeCompe playresult/readback logic grants configured rewards; rewardcard/rewardexecution stay simple compatibility unless new evidence proves otherwise. | Yes |
| Execution hook | Use `rewardexecution.php` as the point that finalizes earned ChallengeCompe rewards. | |
| Rewardcard hook | Use `rewardcardcheck.php` to advertise or gate ChallengeCompe reward availability. | |

**User's choice:** No route role.
**Notes:** None.

---

## The Agent's Discretion

- Exact JSON file names, DTO names, predicate enum names, table names, and plan splits.
- Exact checkpoint plan sequencing, as long as the locked decisions are preserved.

## Deferred Ideas

- Phase 22 Red AdminApi/WebUI editable ChallengeCompe opt-in field.
- User challenge letters, user-created tournaments, official/BNG competition rows, and `ary_user_compe_*` / `ary_bng_compe_*` behavior.
- Admin editing of challenge schedules and broader reward-management tooling.
