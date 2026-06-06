# Milestones

## v1.1 Blue Tokkun Mode Support (Shipped: 2026-06-07)

**Delivered:** Evidence-backed Blue Tokkun support with stateless Banacoin-adjacent compatibility, Tokkun playresult handling, Blue-owned Tokkun persistence/readback, runtime verification, and final contract documentation.

**Phases completed:** 5 phases, 7 plans, 17 tasks

**Key accomplishments:**

- Blue Tokkun evidence contract plus stale source-guard reset, with archive-aware Blue regression tests restored
- Blue getbanacoininfo.php stateless compatibility route returning only `Result = 1`, with route ownership and no-state verification.
- Blue Tokkun playresult uploads now preserve protocol-backed raw facts and return success without normal, battle, shop, Dani, favorite, recent-song, profile, unlock, medal, customization, or title writes.
- Blue Tokkun mode classification and EF schema for nullable tutorial state plus append-only raw stage history
- Classified Blue Tokkun uploads now persist only raw tutorial and stage-history facts while preserving Phase 9 no-cross-write boundaries
- Blue userdata now reads back persisted Tokkun tutorial state through the proven optional protocol field only
- Blue Tokkun runtime verification is recorded and the final v1.1 contract is documented.

**Stats:**

- 5 phases complete
- 7 GSD plans complete
- 17 tracked summary tasks
- Full verification at close: `dotnet test Tests/Tests.csproj` passed 638/638 and Host temp-output build passed with 0 warnings/errors
- GSD range before archive: `a1b4d362` -> `6cf245a7`

**Archived:**

- `.planning/milestones/v1.1-ROADMAP.md`
- `.planning/milestones/v1.1-REQUIREMENTS.md`
- `.planning/milestones/v1.1-phases/`

**What's next:** Start a fresh milestone with `$gsd-new-milestone`.

---

## v1.0 Blue Support (Shipped: 2026-06-03)

**Delivered:** Full Blue-era support across normal play, AdminApi/WebUI readback, item shop/unlocking, battle evidence/design, battle runtime behavior, final guardrails, and operator documentation.

**Phases completed:** Phases 1-6, 30 roadmap plans. 20 GSD plan summaries were tracked on disk; phases 2, 3, and 6 were closed through quick-task and external verification records.

**Key accomplishments:**

- Implemented Blue item-shop advertisement, purchases, season Don medal state, configured unlocks, and locked userdata/BAID readback without Green shop state.
- Completed Blue AdminApi and WebUI parity for supported readback surfaces, including Blue customization catalog data.
- Proved and hardened normal Blue support before battle runtime work.
- Produced a strict Blue battle evidence/design gate from proto, local XML inventory, IDA/client evidence, and explicit unresolved-row handling.
- Implemented Blue-owned battle persistence, `battleuserdata.php`, battle initialdata, battle playresult storage, store/echo rewards, and normal-state protection.
- Closed final verification/docs state after external normal and battle smoke confirmation, with stale debug/UAT artifacts archived as resolved.

**Stats:**

- 6 phases complete
- 30 roadmap plans complete
- 20 tracked GSD plan summaries and 33 tracked summary tasks
- GSD range before archive: `1fbc4391` -> `6ca3cb92`

**Archived:**

- `.planning/milestones/v1.0-ROADMAP.md`
- `.planning/milestones/v1.0-REQUIREMENTS.md`
- `.planning/milestones/v1.0-phases/`

**What's next:** Start a fresh milestone with `$gsd-new-milestone`.

---
