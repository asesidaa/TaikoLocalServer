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

## Milestone: v1.1 Blue Tokkun Mode Support

**Shipped:** 2026-06-07
**Phases:** 5 | **Plans:** 7 | **Sessions:** not measured

### What Was Built

- Blue Tokkun evidence contract with explicit proven, observed, deliberately ignored, and unknown/blocked rows.
- Stateless Banacoin-adjacent compatibility routes and Mucha token settings sufficient for Tokkun availability without wallet/payment persistence.
- Blue Tokkun playresult acceptance before battle/normal writes, preserving raw Tokkun facts without contaminating existing Blue state.
- Blue-owned Tokkun persistence for nullable tutorial state and append-only raw summary/history facts.
- Userdata readback for the nullable Tokkun tutorial flag only.
- Final Tokkun contract and user-confirmed cabinet/RPCS3 runtime verification.

### What Worked

- Keeping Phase 8/9 build-only proof separate from Phase 11 runtime proof prevented premature live-smoke claims.
- SQLite no-write tests made the Tokkun/normal/battle boundary concrete instead of relying on source-word bans.
- Small, evidence-scoped phases made it possible to add persistence and readback without reopening real Banacoin or reward semantics.

### What Was Inefficient

- The final milestone-close helper used UTC dates in generated archive metadata, requiring manual local-date normalization.
- Phase 11 was runtime-verified externally and then represented as a verification-only closeout phase, which required a small manual artifact bridge for the archive workflow.
- Some older planning text still referenced Phase 10 as next work, so PROJECT.md needed a full pass at closeout.

### Patterns Established

- Tokkun persistence stores only raw protocol-backed facts and keeps protocol readback limited to proven fields.
- Banacoin-adjacent routes can be permissive and stateless without modeling real wallet/payment state.
- Runtime evidence that happens outside the repo should be recorded explicitly as user-confirmed external verification, not implied by automated tests.

### Key Lessons

1. For legacy-client behavior, deterministic runtime evidence should outrank speculative mixed-payload branches.
2. Full request dumps and behavior tests are better Tokkun guardrails than source-word scans.
3. Milestone close should normalize generated archive metadata before the safety commit, especially when local time and UTC dates differ.

### Cost Observations

- Model mix: not measured.
- Sessions: not measured.
- Notable: Phase 10 carried most of the implementation cost because it combined EF schema, handler writes, and readback tests.

---

## Milestone: v1.2 Yellow AC15 Support

**Shipped:** 2026-06-12
**Phases:** 8 | **Plans:** 38 | **Sessions:** not measured

### What Was Built

- Yellow first-class AC15 adapter, `/v09r02/chassis/*` game routes, shared startup/version behavior, generated wire DTOs, and enabled-era route gating.
- Yellow-owned catalog, identity, userdata, self-best, crown, normal play, Dani, shop, medal, favorite, recent, AdminApi, and WebUI readback behavior.
- Yellow WaiWai and Tokkun support bounded to local wire/runtime evidence, including Tokkun no-cross-mode writes and nullable tutorial readback.
- Stateless Yellow Banacoin-adjacent compatibility without wallet, balance, payment, coupon, settlement, receipt, or transaction authority.
- AC15 nullable wire generation, Mapperly protocol projection, and shared-core cleanup across Green, Blue, and Yellow while preserving era-owned state.
- Final Yellow contract documentation plus user-confirmed RPCS3 smoke and full automated verification.

### What Worked

- Keeping Yellow battle absent avoided fake support and kept Blue battle behavior isolated.
- Mapperly and nullable wire work before runtime closeout made optional-field presence explicit at adapter boundaries.
- Moving repeated AC15 behavior into shared services and capability bindings reduced duplication without introducing shared gameplay tables.
- Treating RPCS3 smoke as a final closeout gate kept earlier implementation phases focused on automated regression and persistence boundaries.

### What Was Inefficient

- Phase 16.2 grew into a large refactor wave after review feedback, including a follow-up capability-composition cleanup.
- The milestone helper generated UTC dates and an overly long MILESTONES entry, requiring manual normalization.
- Phase 17 had to be represented as a closeout artifact after external runtime verification rather than a normal implementation phase.

### Patterns Established

- New AC15 eras should stay first-class at routes, wire DTOs, persistence, catalogs, handlers, mappers, tests, and UI routing.
- Shared AC15 behavior should be extracted through explicit contracts and typed bindings, not repository-shaped persistence adapters or shared EF gameplay tables.
- Unsupported cabinet surfaces should be absent, not stubbed, until proto/log/client evidence proves a server contract.

### Key Lessons

1. Optional protobuf presence should be represented in generated wire shape and mapper contracts, not scattered through production `ShouldSerialize*` checks.
2. Shared AC15 code is useful only when era-specific gates remain visible at the handler/profile boundary.
3. Runtime verification that happens outside the repo needs a small closeout artifact immediately, or planning state drifts behind user-confirmed reality.

### Cost Observations

- Model mix: not measured.
- Sessions: not measured.
- Notable: Phase 16.2 carried the most coordination cost because it combined architecture correction, review fixes, and broader shared-core cleanup before final Yellow smoke.

---

## Milestone: v1.4 White AC15 0.13 Support

**Shipped:** 2026-06-21
**Phases:** 6 | **Plans:** 10 | **Sessions:** not measured

### What Was Built

- White first-class adapter foundation with route/root evidence, generated wire DTOs, Host gating, and no-state scaffold limits.
- White catalog/profile binding through matching AC15 loaders, White sidecars, explicit protocol limits, and White-owned persistence boundaries.
- White runtime support for identity, userdata, normal play, self-best, crowns, favorites, recent songs, reward/Don Point state, and Dani where proven.
- White present/special-BAID provenance plus server-side White Don Challenge progress/reward readback through dedicated AdminApi/WebUI contracts.
- White AdminApi/WebUI support for implemented White-owned surfaces, with unsupported White surfaces kept absent.
- Accepted closeout with automated tests, Mapperly generated-source inspection, temp-output Host build, and user-accepted RPCS3/cabinet/WebUI verification.

### What Worked

- Route/root evidence before implementation kept White 0.13 bounded to proven `/v07r00` behavior.
- Capability-owned AC15 reuse let White assemble from existing services without adding shared gameplay tables.
- Keeping Don Challenge distinct from ChallengeCompe avoided repeating the Red naming/semantics confusion.
- The final closeout decision made the manual runtime gate explicit instead of leaving `human_needed` artifacts open.

### What Was Inefficient

- The generated milestone summary undercounted tasks because several summaries do not expose task counts in the helper's expected format.
- A stale quick-task status spelling (`completed` vs `complete`) caused a false `audit-open` warning during closeout.
- The White final 11.01 follow-up happened as quick work around the milestone and needs to remain clearly separate from the v1.4 White 0.13 archive.

### Patterns Established

- Older AC15 eras should be added by composing explicit era profiles, typed persistence, generated wire placement, and catalog sidecars.
- White and later White updates need separate protocol evidence when route or wire shape changes.
- Server-side Don Challenge can be shared as an AC15 capability, but each era must still own data, state, readback, and cabinet-route semantics.

### Key Lessons

1. Do not let later-version White evidence silently redefine White 0.13 behavior.
2. Keep manual runtime/WebUI verification status synchronized across `VERIFICATION.md`, `REQUIREMENTS.md`, `ROADMAP.md`, `STATE.md`, and archives before milestone close.
3. Generated milestone stats are useful, but phase/plan counts and verification artifacts are the authoritative closeout scope.

### Cost Observations

- Model mix: not measured.
- Sessions: not measured.
- Notable: Most process cost came from evidence/status synchronization and post-closeout narrative cleanup rather than code changes.

---

## Cross-Milestone Trends

### Process Evolution

| Milestone | Sessions | Phases | Key Change |
|-----------|----------|--------|------------|
| v1.0 | not measured | 6 | Blue support moved from staged normal support into evidence-gated battle runtime and final closeout. |
| v1.1 | not measured | 5 | Tokkun support used evidence-scoped phases with runtime proof deferred until final closeout. |
| v1.2 | not measured | 8 | Yellow support reused AC15 shared core where behavior matched while preserving era-owned state and closing with RPCS3 smoke. |
| v1.3 | not measured | 6 | Red introduced server-side Don Challenge and separate ChallengeCompe compatibility while preserving era-owned runtime state. |
| v1.4 | not measured | 6 | White reused AC15 capabilities behind White evidence/profile/state boundaries and closed with accepted runtime/WebUI verification. |

### Cumulative Quality

| Milestone | Tests | Coverage | Zero-Dep Additions |
|-----------|-------|----------|-------------------|
| v1.0 | Full server suite plus focused Blue/BlueBattle/source-guard tests | not measured | Blue-owned battle/source-guard patterns |
| v1.1 | Full server suite: 638 passed at close | not measured | Blue-owned Tokkun persistence/readback and stateless Banacoin compatibility |
| v1.2 | Full server suite: 683 passed at close | not measured | Yellow-owned AC15 support plus shared AC15 Mapperly/core patterns |
| v1.3 | Full server suite: 778 passed at close | not measured | Red-owned Don Challenge and AdminApi/WebUI closeout patterns |
| v1.4 | Full server suite: 829 passed at close | not measured | White-owned runtime state plus dedicated White Don Challenge readback |

### Top Lessons (Verified Across Milestones)

1. Keep Blue-specific runtime behavior isolated from Green unless a contract is truly shared.
2. Prefer evidence-backed protocol contracts over inferred catalog/default behavior.
3. Treat runtime smoke proof as a separate closeout gate when earlier phases are intentionally source/test/build scoped.
4. Preserve era-owned persistence and route ownership even when extracting shared AC15 algorithms.
5. Keep Don Challenge capability sharing separate from ChallengeCompe protocol compatibility.
6. For White and older eras, separate version-specific route/wire evidence before adding compatibility for later updates.
