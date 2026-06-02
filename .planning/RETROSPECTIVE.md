# Project Retrospective

*A living document updated after each milestone. Lessons feed forward into future planning.*

## Milestone: v1.0 Blue Support

**Shipped:** 2026-06-03
**Phases:** 6 | **Plans:** 30 roadmap plans | **Sessions:** not measured

### What Was Built

- Blue-owned normal support for item shop, purchases, unlocks, profile/userdata readback, Dani, AdminApi, and WebUI surfaces.
- Strict Blue battle evidence/design package from proto, local data, IDA/client evidence, and explicit unresolved-row gates.
- Blue battle runtime persistence, `battleuserdata.php`, initialdata advertisement, playresult state capture, store/echo reward handling, and normal-state protection.
- Final guardrails, source tests, docs, and closeout state for full Blue support.

### What Worked

- Evidence-first sequencing kept normal support, battle design, and battle runtime from collapsing into one speculative task.
- Source guards made Blue/Green boundary regressions visible and cheap to check.
- Temp-output Host builds avoided false failures when the normal debug output was locked by a running server.

### What Was Inefficient

- Several Phase 05 crash investigations stayed open after fixes landed, so milestone close had to normalize stale debug/UAT records.
- Some phases were completed through quick-task or external verification paths, which made generated milestone stats undercount the actual roadmap scope.

### Patterns Established

- Preserve Blue as a first-class era with Blue-owned handlers, mappers, persistence, catalogs, tests, and WebUI/AdminApi routing.
- Treat battle values as evidence-backed, nullable, or store/echo until client evidence proves stronger semantics.
- Keep operator-supplied Blue data local while committing derived JSON, docs, and tests.

### Key Lessons

1. Battle byte-array width proof is not enough; downstream client consumers decide whether a response is runtime-safe.
2. Milestone close should run `audit-open` before archiving so stale debug/UAT records do not leak into the next planning cycle.
3. External verification paths need explicit doc normalization or GSD phase stats will undercount completed roadmap work.

### Cost Observations

- Model mix: not measured.
- Sessions: not measured.
- Notable: source-guard and requirement-traceability tests paid off during repeated Blue battle fixes.

---

## Cross-Milestone Trends

### Process Evolution

| Milestone | Sessions | Phases | Key Change |
|-----------|----------|--------|------------|
| v1.0 | not measured | 6 | Blue support moved from staged normal support into evidence-gated battle runtime and final closeout. |

### Cumulative Quality

| Milestone | Tests | Coverage | Zero-Dep Additions |
|-----------|-------|----------|-------------------|
| v1.0 | Full server suite plus focused Blue/BlueBattle/source-guard tests | not measured | Blue-owned battle/source-guard patterns |

### Top Lessons (Verified Across Milestones)

1. Keep Blue-specific runtime behavior isolated from Green unless a contract is truly shared.
2. Prefer evidence-backed protocol contracts over inferred catalog/default behavior.
