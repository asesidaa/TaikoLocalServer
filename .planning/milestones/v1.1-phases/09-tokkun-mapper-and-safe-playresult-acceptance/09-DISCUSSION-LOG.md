# Phase 9: Tokkun Mapper and Safe Playresult Acceptance - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md. This log preserves the alternatives considered.

**Date:** 2026-06-05T01:47:37.9701760+08:00
**Phase:** 9-Tokkun Mapper and Safe Playresult Acceptance
**Areas discussed:** Mixed Tokkun payload handling, Tokkun tutorial flag mapping, Tokkun DTO raw fact preservation

---

## Area Selection

| Option | Description | Selected |
|--------|-------------|----------|
| All areas | Answer one grouped batch covering classifier, DTO surface, safe acceptance, no-write tests, and logging. | yes |
| Core branch only | Focus on classifier precedence, DTO shape, and safe success behavior first. | |
| Safety tests only | Focus on no-write verification and logging boundaries before planning. | |

**User's choice:** All areas.
**Notes:** The workflow then skipped already-locked Phase 7/8 decisions and asked only the remaining implementation-shaping questions.

---

## Mixed Tokkun Payload Handling

| Option | Description | Selected |
|--------|-------------|----------|
| Record flags | Treat it as Tokkun, return success, and expose ignored-normal or ignored-battle indicators for logging/tests only. | |
| Tokkun facts only | Treat it as Tokkun and preserve only Tokkun facts, relying on tests to prove ignored material had no effect. | |
| Ambiguous bucket | Classify as ambiguous success/no-write instead of Tokkun when other-mode material is also present. | |
| Free-form correction | Treat it as Tokkun and continue; do not model this as ambiguous design. | yes |

**User's choice:** Treat it as Tokkun and continue.
**Notes:** User clarified that this is deterministic existing legacy game behavior. The server must not design speculative "mixed payload" ambiguity for later plans. If such data appears, it is by design and should not be treated as a new branch. The existing full playresult dump is already available.

---

## Tokkun Tutorial Flag Mapping

| Option | Description | Selected |
|--------|-------------|----------|
| Map presence/value | Preserve optional presence and value for Phase 10 readback planning, but keep classifier based on `ary_tokkunstage_info`. | yes |
| Presence only | Expose only whether the optional field was present, not the numeric value. | |
| Leave unmapped | Do not map it until Phase 10 proves readback needs it. | |

**User's choice:** Map presence/value.
**Notes:** This carries forward Phase 7: tutorial flag is readback/tutorial evidence, not a standalone Tokkun classifier.

---

## Tokkun DTO Raw Fact Preservation

| Option | Description | Selected |
|--------|-------------|----------|
| All raw facts | Map `banacoin_datetime`, song count/list, speed/autoplay/jump counts, and presence for later Phase 10 use. | yes |
| Classifier plus counts | Map presence and simple counts only; leave detailed summary fields to Phase 10. | |
| Presence only | Use it only as a classifier and do not carry payload details across the adapter boundary. | |

**User's choice:** All raw facts.
**Notes:** Preserve raw protocol facts without inferring payment, practice, reward, score, unlock, or progression semantics.

---

## the agent's Discretion

- Choose exact code structure and names consistent with existing Blue partial-file patterns.
- Choose focused behavior tests that exercise real mapper/handler paths.
- Add only minimal classifier logging if useful; the full playresult request dump already exists.

## Deferred Ideas

None.
