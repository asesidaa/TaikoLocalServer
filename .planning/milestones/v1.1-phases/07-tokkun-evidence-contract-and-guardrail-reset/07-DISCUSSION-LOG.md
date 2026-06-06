# Phase 7: Tokkun Evidence Contract and Guardrail Reset - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md; this log preserves the alternatives considered.

**Date:** 2026-06-03T22:29:15.4478436+08:00
**Phase:** 7-Tokkun Evidence Contract and Guardrail Reset
**Areas discussed:** Evidence Ledger Shape, Tokkun Classifier Boundary, Guardrail Replacement, Follow-on Handoff Surface

---

## Evidence Ledger Shape

| Question | Options Presented | User's Choice | Notes |
|----------|-------------------|---------------|-------|
| What form should the Phase 7 Tokkun contract use? | Battle matrix; Contract doc; Minimal ledger | Battle matrix | Use the proven battle row-by-row pattern. |
| How should the Tokkun matrix classify each field or route? | Four tiers; Strict only; Evidence notes | Four tiers | Use proven, observed, deliberately ignored, and unknown/blocked. |
| What evidence sources should Phase 7 use before it marks a Tokkun row as proven? | Local plus IDA; Local only; Require logs | Local plus IDA | Cabinet/RPCS3 proof stays for final verification unless required for a specific row. |
| What should the Tokkun evidence matrix enumerate? | Fields routes guards; Fields only; Unknowns only | Fields routes guards | Include protocol fields, endpoint/route questions, and guard reset decisions. |

---

## Tokkun Classifier Boundary

| Question | Options Presented | User's Choice | Notes |
|----------|-------------------|---------------|-------|
| Before the numeric `PlayMode` is proven, what fields may downstream phases use to recognize a Tokkun-shaped upload? | Presence allowlist; Stage info only; Log only | Presence allowlist with clarification | `ary_tokkunstage_info` is likely involved; `tokkun_tutorial_flg` likely marks tutorial shown and is not enough after first selection. |
| Should `tokkun_tutorial_flg` alone be sufficient to classify a payload as Tokkun? | No, state only; Yes, sufficient; Unknown blocked | No, state only | Treat as tutorial state/readback evidence. |
| How should the Phase 7 contract classify mixed Tokkun-shaped payloads? | Safe mixed branch; Tokkun wins; Existing precedence; Other | Other, lenient Tokkun | User clarified one credit has one mode. If Tokkun mode is identified, ignore other-mode material rather than saving scores or battle state. |
| What should the Phase 7 contract allow downstream phases to output from Tokkun classification? | Class plus facts; Class only; Raw only | Class plus facts | Expose `IsTokkun`, protocol-backed facts, and ignored-other-mode indicators, but no guessed play-mode enum. |

---

## Guardrail Replacement

| Question | Options Presented | User's Choice | Notes |
|----------|-------------------|---------------|-------|
| How should we replace the old source guard that banned Tokkun terms in Blue playresult code? | Named allowlist; Broad allow; Contract-only; Other | Other, remove them | User rejected the old source guard as unreasonable. |
| Should Phase 7 still define targeted guardrails against concrete forbidden Tokkun side effects? | Target effects; Docs only; No guards | No guards | User rejected source-scanning guardrails. Correct implementation skips other routines so scores/crowns are not saved. |
| What should replace source-guard tests for proving Tokkun does not save normal or battle state? | Real behavior; Cabinet only; Compile only | Real behavior | Use real runtime behavior and focused checks that execute the Tokkun path. |
| How should Phase 7 frame TKEV-03 for downstream planning? | Reset wording; Update requirement; Document objection | Reset wording | TKEV-03 means deleting stale source guards and preventing side effects through control flow, not new source scans. |

---

## Follow-on Handoff Surface

| Question | Options Presented | User's Choice | Notes |
|----------|-------------------|---------------|-------|
| What should Phase 7 hand to Phase 8 about Banacoin-adjacent routes? | Route unknowns; Implement hints; Minimal note | Route unknowns | Hand off route questions and evidence gates, especially around `getbanacoininfo.php`. |
| What should Phase 7 hand to Phase 9 for safe Tokkun playresult acceptance? | Classifier contract; Raw fields only; Blocked until logs | Classifier contract | Include classifier inputs, ignored-other-mode rule, logging boundary, and no normal/battle write rule. |
| What should Phase 7 allow Phase 10 to consider for Tokkun persistence? | Protocol facts; Tutorial first; Store raw blob | Protocol facts | Persist only protocol-backed facts, not inferred rewards/progression/payment. |
| What should Phase 7 say Phase 11 must prove before v1.1 is done? | Cabinet plus tests; Cabinet priority; Server tests first | Cabinet plus tests | Require cabinet/RPCS3 evidence plus real behavior checks for no unrelated state writes. |

---

## the agent's Discretion

None.

## Deferred Ideas

None.
