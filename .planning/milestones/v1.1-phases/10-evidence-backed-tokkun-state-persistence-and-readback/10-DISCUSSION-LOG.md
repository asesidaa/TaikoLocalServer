# Phase 10: Evidence-Backed Tokkun State Persistence and Readback - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md - this log preserves the alternatives considered.

**Date:** 2026-06-06T18:54:17.7207876+08:00
**Phase:** 10-Evidence-Backed Tokkun State Persistence and Readback
**Areas discussed:** Tokkun storage shape, Blue userdata readback behavior, Proof and test boundaries

---

## Tokkun Storage Shape

### Storage owner

| Option | Description | Selected |
|--------|-------------|----------|
| Separate Tokkun tables | Add Blue-owned Tokkun user state plus Tokkun summary/history rows, mirroring the battle separation pattern and keeping normal Blue save state cleaner. | |
| UserSave + history | Store tutorial flag on UserSaveData_Blue and use a separate table only for summary/history rows. | yes |
| Latest state only | Keep one Tokkun state row per user, replacing prior summary facts instead of keeping a history. | |

**User's choice:** UserSave + history
**Notes:** Tutorial/readback state should live on `UserSaveData_Blue`; summary/progress remains separate history storage.

### History retention

| Option | Description | Selected |
|--------|-------------|----------|
| Append every upload | Store each Tokkun-classified upload as a raw history record with server upload time; this matches existing play history behavior and avoids guessing idempotency. | yes |
| Dedupe by timestamp | Use Baid plus banacoin_datetime as an idempotency key and update/replace a duplicate row. | |
| Keep latest only | Keep only the newest summary row per user and discard older Tokkun summary facts. | |

**User's choice:** Append every upload
**Notes:** No dedupe or latest-only behavior should be inferred from `banacoin_datetime`.

### Song list fidelity

| Option | Description | Selected |
|--------|-------------|----------|
| Exact raw order | Preserve the repeated list exactly as received, including order and duplicates if the client sends them. | yes |
| Unique songs only | Normalize to a distinct set of song numbers and discard repeated/order information. | |
| Planner decides | Lock only that the list must be persisted; let the planner choose exact representation. | |

**User's choice:** Exact raw order
**Notes:** `tookun_songno` must preserve client order and duplicates.

### Timestamp storage

| Option | Description | Selected |
|--------|-------------|----------|
| Client + server | Store raw client strings such as banacoin_datetime/play_datetime where available plus server UploadedAtUtc for audit ordering. | |
| Server only | Store only server upload time and treat client timestamp strings as non-durable payload noise. | |
| Client only | Store only client-reported protocol timestamps and avoid adding server-side audit time. | yes |

**User's choice:** Client only
**Notes:** Follow-up resolved the roadmap "upload time" requirement as client-reported protocol time only, not a server `UploadedAtUtc` column.

---

## Blue Userdata Readback Behavior

### Absent tutorial value

| Option | Description | Selected |
|--------|-------------|----------|
| Omit field | Preserve current absence behavior until a protocol-backed value has been uploaded and stored. | yes |
| Return zero | Always serialize tokkun_tutorial_flg=0 for new users, treating zero as the default tutorial-not-complete state. | |
| Return one | Always serialize tokkun_tutorial_flg=1 to skip the tutorial by default. | |

**User's choice:** Omit field
**Notes:** No default `0` or `1` is approved.

### Persistence source

| Option | Description | Selected |
|--------|-------------|----------|
| Tokkun only | Update it only from Tokkun-classified uploads where ary_tokkunstage_info is present and the tutorial flag is present. | yes |
| Any upload | Update it from any Blue playresult that carries the optional tutorial flag, even if ary_tokkunstage_info is absent. | |
| Only value one | Update only when the client sends value 1, leaving zero or other values ignored. | |

**User's choice:** Tokkun only
**Notes:** After later evidence discussion, Tokkun classification should be based primarily on `play_mode = 3`.

### Value semantics

| Option | Description | Selected |
|--------|-------------|----------|
| Raw nullable uint | Persist the exact client uint value when present and return that exact value; absence remains distinct from zero. | yes |
| Boolean | Normalize any nonzero value to true/1 and zero to false/0. | |
| Clamp 0 or 1 | Persist only 0 or 1, rejecting or ignoring any other value. | |

**User's choice:** Raw nullable uint
**Notes:** Keep protocol value raw.

### Summary protocol readback

| Option | Description | Selected |
|--------|-------------|----------|
| Tutorial only | Only tokkun_tutorial_flg is read back through UserDataResponse; summary/history records are persisted and verified server-side until Phase 11 proves another protocol surface. | yes |
| Try summary fields | Look for or invent a protocol response location for summary facts in Phase 10. | |
| Planner decides | Lock tutorial readback and let research decide whether summary facts need a protocol response now. | |

**User's choice:** Tutorial only
**Notes:** Summary/history readback is server-side persistence proof in Phase 10.

---

## Proof And Test Boundaries

### New runtime evidence

| Option | Description | Selected |
|--------|-------------|----------|
| Automated + build | Use focused tests plus Host build; explicitly leave cabinet/RPCS3 Tokkun selection/upload/readback proof to Phase 11. | |
| Require live proof | Do not call Phase 10 complete without cabinet/RPCS3 runtime proof. | |
| Build only | Only require a successful build and defer detailed automated behavior tests. | |

**User's choice:** Free-form correction
**Notes:** User stated: "We have played a session in tokkun mode, log shows play_mode=3, and there are other fields. Now we can be sure what tokkun mode results look like."

### Play mode evidence lock

| Option | Description | Selected |
|--------|-------------|----------|
| Lock as proven | Treat user-provided runtime log evidence as sufficient to add/plan PlayMode.Tokkun = 3, while still storing only raw Tokkun facts. | yes |
| Record observed | Mention play_mode=3 as observed but do not plan enum/classifier changes until a log file path is available. | |
| Pause for log | Stop this discussion until the exact log artifact can be provided and read. | |

**User's choice:** Lock as proven
**Notes:** Repo search did not find a committed `play_mode=3` log artifact in obvious paths during discussion.

### Classifier update

| Option | Description | Selected |
|--------|-------------|----------|
| Dual classifier | Classify Tokkun when play_mode is 3 or ary_tokkunstage_info is present; this preserves Phase 9 compatibility while using the new proven mode value. | |
| Mode primary | Classify Tokkun primarily from play_mode=3 and treat ary_tokkunstage_info as payload detail, not a classifier. | yes |
| Enum only | Add/plan PlayMode.Tokkun=3 for readability, but keep classification based only on ary_tokkunstage_info. | |

**User's choice:** Mode primary
**Notes:** This supersedes the older "numeric mode unknown" classifier posture.

### Additional logged fields

| Option | Description | Selected |
|--------|-------------|----------|
| Inspect before storing | Planning should require the log/proto field list to decide any extra raw fields; Phase 10 should not store unnamed fields from memory. | yes |
| Store mapped fields only | Persist only the already-mapped Tokkun tutorial/stage facts plus play_mode=3, and defer any additional logged fields. | |
| Pause for fields | Stop now so the exact additional fields or log artifact can be provided before context is written. | |

**User's choice:** Inspect before storing
**Notes:** Extra fields require concrete log/proto inspection before persistence design.

### Automated proof

| Option | Description | Selected |
|--------|-------------|----------|
| Full focused suite | Require classifier/mapper tests, handler persistence tests, userdata readback tests, schema/migration reload tests, no-cross-write tests, and Host build. | yes |
| Behavior only | Require handler/userdata behavior tests and build, but skip explicit migration/schema tests. | |
| Minimal only | Require only a build and one persistence happy-path test. | |

**User's choice:** Full focused suite
**Notes:** Include schema/migration reload and no-cross-write tests.

### Live proof boundary

| Option | Description | Selected |
|--------|-------------|----------|
| Final smoke only | Phase 10 can use existing session evidence for mode/result shape; Phase 11 still owns final cabinet/RPCS3 smoke after implementation. | yes |
| None outside | Treat the existing Tokkun session as enough live proof for v1.1 and avoid a Phase 11 smoke requirement. | |
| Run in Phase 10 | Require a fresh cabinet/RPCS3 run after Phase 10 implementation before completing the phase. | |

**User's choice:** Final smoke only
**Notes:** Existing live evidence unlocks mode/result-shape planning, but Phase 11 still owns final post-implementation smoke.

---

## the agent's Discretion

- Exact history-row representation for `tookun_songno`, provided raw order and duplicates are preserved.
- Exact handler/helper/test file organization.
- Exact migration name.

## Deferred Ideas

None.
