# Phase 5: Blue Battle Runtime Support - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md; this log preserves the alternatives considered.

**Date:** 2026-05-30T18:21:05.3452238+08:00
**Phase:** 5-Blue Battle Runtime Support
**Areas discussed:** Gate resolution path, Battleuserdata and initialdata defaults, Battle playresult handling, Battle XML rewards and unlock semantics

---

## Gate Resolution Path

| Question | Options Presented | User's Choice | Notes |
|---|---|---|---|
| How should Phase 5 handle the blocked battle evidence gate before runtime planning? | Per-row proof/approval; Groundwork only; Approve broad assumptions | Per-row proof/approval | Each missing-evidence row needs proof or named approval before runtime behavior relies on it. |
| If a Phase 4 missing-evidence row still has no proof or explicit approval when planning starts, what should the planner do with that behavior? | Exclude it; Plan guardrails; Stub placeholders; Other | Other | User clarified: address all rows; if anything is still unknown, block until addressed. |
| Where should Phase 5 record the row-by-row resolutions for the 26 blocked evidence items? | Phase 5 resolution matrix; Update Phase 4 gate; Context only | Phase 5 resolution matrix | Leave completed Phase 4 gate history intact. |
| What evidence standard should the Phase 5 resolution matrix require before marking a row resolved without user approval? | Concrete proof only; Code plus XML; Best-effort inference | Concrete proof only | User added that the Wiki page may be a source of truth for gameplay behavior and proto-field intent. |

---

## Battleuserdata And Initialdata Defaults

| Question | Options Presented | User's Choice | Notes |
|---|---|---|---|
| For battleuserdata/initialdata defaults, how far may the Wiki resolve rows without IDA/client/log proof? | Gameplay and intent; Emit cap/stage values; Gameplay only | Gameplay and intent | Wiki can guide semantics and likely field intent, not exact emitted byte widths or required response shape by itself. |
| When should Phase 5 allow initialdatacheck.php to advertise battle availability with is_battleplay? | After all gate rows resolve; Early true flag; Manual config switch | After all gate rows resolve | Keep battle advertisement omitted/disabled until all required rows are resolved. |
| How should Phase 5 resolve byte-array widths/defaults for release battle stage/special/info flags and NPC special/costume flags? | Proof or explicit approval; XML-derived widths; Omit until changed | Proof or explicit approval | Do not compute widths from XML max IDs alone. |
| What should the default new-user battle state rule be for battleuserdata.php once the gate is resolved? | Resolved row values only; XML first progression; Minimal empty state | Minimal empty state | If some field is required, fill resolved values. If a field is likely recorded after playresult, let playresult populate it. |

---

## Battle Playresult Handling

| Question | Options Presented | User's Choice | Notes |
|---|---|---|---|
| How should Phase 5 identify a Blue battle playresult payload? | Battle section presence; Play/stage mode proof; Session state | Battle section presence | User noted stage modes and flags likely change in battle; inspect via IDA or allow observed values first and update from request logs. |
| Once a request is classified as battle, what should happen to the normal Blue stage persistence path? | Bypass normal stage saves; Mixed save with guards; Reject mixed payloads | Bypass normal stage saves | Battle stages must not update normal score/crown/history/recent/favorite/counter/Dani paths. |
| How should Phase 5 persist BattleStageData fields such as NPC exp, DPN, costume, specials, bonds level, boss life, damage, and kill count? | Battle-owned state; Raw audit first; Ignore for now | Battle-owned state | Persist client-reported values into Blue battle-owned state after rows are resolved. |
| How should Phase 5 handle battle playresult play_mode, stage_mode, and other mode flags that differ from normal Blue values? | Accept in battle branch; Require known values; Normalize to normal | Accept in battle branch | Do not apply normal-stage support filters to battle-classified payloads. |

---

## Battle XML Rewards And Unlock Semantics

| Question | Options Presented | User's Choice | Notes |
|---|---|---|---|
| When may Phase 5 add runtime loaders/catalogs for the five local battle XML files? | After role resolution; Load all candidate XML; No loaders yet | After role resolution | Add loaders only for files/fields whose runtime role is proven or approved. |
| How should Phase 5 treat the XML stage graph, including first stage, next stage, last-stage behavior, boss life, and stage id 33? | Resolve each graph rule; Use XML graph directly; Ignore stage 33 | Resolve each graph rule | Do not infer progression from XML alone. |
| How should Phase 5 handle battletokeninfo.xml token rows and reward type values 0/1? | Proof/approval per type; Battle-owned only; XML reward mapping | Proof/approval per type | Token IDs, values, and reward type meanings need proof or approval. |
| Which battle rewards may mirror into normal Blue unlock/save-state behavior? | Normal arrays only by default; All battle releases mirror; No mirrors | Normal arrays only by default | `ReleaseBattleData` remains battle-owned unless a specific path is proven or approved. |

---

## the agent's Discretion

None.

## Deferred Ideas

None.
