# Roadmap: TaikoLocalServer

## Milestones

- [x] **v1.0 Blue Support** - Phases 1-6 shipped on 2026-06-03. See [v1.0 roadmap archive](milestones/v1.0-ROADMAP.md), [v1.0 requirements archive](milestones/v1.0-REQUIREMENTS.md), and [v1.0 phase artifacts](milestones/v1.0-phases/).
- [x] **v1.1 Blue Tokkun Mode Support** - Phases 7-11 shipped on 2026-06-07. See [v1.1 roadmap archive](milestones/v1.1-ROADMAP.md), [v1.1 requirements archive](milestones/v1.1-REQUIREMENTS.md), and [v1.1 phase artifacts](milestones/v1.1-phases/).
- [x] **v1.2 Yellow AC15 Support** - Phases 12-17 plus inserted Phases 16.1 and 16.2 shipped on 2026-06-12. See [v1.2 roadmap archive](milestones/v1.2-ROADMAP.md), [v1.2 requirements archive](milestones/v1.2-REQUIREMENTS.md), and [v1.2 phase artifacts](milestones/v1.2-phases/).
- [x] **v1.3 Red AC15 Support** - Phases 18-22 shipped on 2026-06-16. See [v1.3 roadmap archive](milestones/v1.3-ROADMAP.md), [v1.3 requirements archive](milestones/v1.3-REQUIREMENTS.md), and [v1.3 phase artifacts](milestones/v1.3-phases/).
- [ ] **v1.4 White AC15 0.13 Support** - Phases 23-27. White 0.13 is the next older AC15 capability composition, with White-owned protocol, catalog, state, collectable data, admin, and runtime verification boundaries.

## Current Planning State

Active milestone: **v1.4 White AC15 0.13 Support**

**Goal:** Add White 0.13 by composing existing AC15 capabilities with White config, limits, wire placement, typed persistence, collectable data, and White/older-version evidence while preserving era-owned protocol, catalog, state, admin, and verification boundaries.

**Scope guardrails:**

- White starts from `proto/white`, `Host/wwwroot/data/white/data`, request logs/captures, corrected IDA/client evidence, and RPCS3/cabinet proof.
- The linked White wiki page is product/version scoping context only; it does not define route, payload, state, or response contracts.
- Current Phase 23 context records `.tools/white/EBOOT.ELF.i64` as present and nonzero (`129893515` bytes). Plan 23-01 route proof is approved for `/v07r00/chassis` and exactly fourteen no-state scaffold suffixes; any additional White route code still requires White IDB route strings, logs, captures, or equivalent local evidence.
- White 0.13 has fewer surfaces than Red and Yellow. Unsupported or later-version features stay absent instead of receiving cloned compatibility routes.
- Shared AC15 services may be reused only through explicit White limits, feature flags, wire placement, catalog data, and White-owned persistence tables.
- Collectable data, including Don Challenge if it is proven inside the White 0.13 range, is intentionally late because it depends on stable identity, catalog, playresult, reward, and readback behavior.

## v1.4 Phase Summary

| Phase | Name | Goal | Requirements | Success Criteria |
|-------|------|------|--------------|------------------|
| 23 | White Evidence and Era Foundation | Prove White route/version/transport boundaries and add first-class White adapter scaffolding. | WFND-01, WFND-02, WFND-03 | 5 |
| 24 | White Catalog Profile and Protocol Limits | Bind White `ST7100-1` data, sidecars, AC15 profile, feature flags, limits, and wire placement. | WCAT-01, WCAT-02, WCAT-03 | 5 |
| 25 | White Runtime Capability Binding and Rewards | Bind White identity, userdata, normal play, metadata readback, Taikojuku/Dani where proven, and reward/present state. | WSTATE-01, WSTATE-02, WSTATE-03, WSTATE-04, WCOLL-01 | 5 |
| 26 | White Collectable Data and Don Challenge Evidence | Collect White 0.13 collectable data and bind Don Challenge only if White evidence proves it. | WCOLL-02, WCOLL-03 | 5 |
| 27 | White AdminApi WebUI and Runtime Closeout | Expose implemented White-owned readback surfaces and close v1.4 with automated plus RPCS3/cabinet verification. | WVER-01, WVER-02, WVER-03 | 5 |

**Coverage:** 16/16 v1.4 requirements mapped exactly once.

## Phases

- [ ] **Phase 23: White Evidence and Era Foundation** - Prove White route/version/transport boundaries and add first-class White adapter scaffolding.
- [ ] **Phase 24: White Catalog Profile and Protocol Limits** - Bind White `ST7100-1` data, sidecars, AC15 profile, feature flags, limits, and wire placement.
- [ ] **Phase 25: White Runtime Capability Binding and Rewards** - Bind White identity, userdata, normal play, metadata readback, Taikojuku/Dani where proven, and reward/present state.
- [ ] **Phase 26: White Collectable Data and Don Challenge Evidence** - Collect White 0.13 collectable data and bind Don Challenge only if White evidence proves it.
- [ ] **Phase 27: White AdminApi WebUI and Runtime Closeout** - Expose implemented White-owned readback surfaces and close v1.4 with automated plus RPCS3/cabinet verification.

## Phase Details

### Phase 23: White Evidence and Era Foundation

**Goal:** Prove White route/version/transport boundaries and add first-class White adapter scaffolding.
**Depends on:** v1.3 Red archive
**Requirements:** WFND-01, WFND-02, WFND-03
**Plans:** 3 plans
Plans:
**Wave 1**

- [x] 23-01-PLAN.md - White evidence gate, feature inventory, and stale-note correction

**Wave 2** *(unblocked after Wave 1 approval)*

- [ ] 23-02-PLAN.md - White adapter identity and generated wire foundation

**Wave 3** *(blocked on Wave 2 completion)*

- [ ] 23-03-PLAN.md - Evidence-gated route scaffolds and Host wiring

**Key deliverables:**

- White evidence artifact covering game route prefix, startup/version ownership, direct-protobuf transport, active data root, usable/unusable binary evidence, and unresolved runtime gaps.
- White feature inventory separating White 0.13 proven surfaces, later White-only leads, other-era behavior, and unknowns.
- Generated White wire DTOs from immutable `proto/white` inputs in a first-class White adapter project.
- `GameEra.White`, Host settings, DI registration, application-part gating, content-type fallback scope, and disabled-era route safety.
- Existing-era preservation checks for any shared code touched during foundation work.

**Success criteria:**

1. Developer can inspect a White evidence artifact before route/controller behavior or data-root choices are treated as final.
2. White generated wire DTOs are produced from `proto/white/taiko.proto` and `proto/white/vsinterface.proto` without modifying dumped proto files.
3. White adapter routes are enabled only when White is configured and absent when White is disabled.
4. Route/root assumptions are evidence-tagged, and the superseded zero-byte IDB limitation is replaced with current nonzero IDB evidence plus approved route-proof boundaries for Phase 23 scaffolding.
5. Existing Blue, Green, Yellow, Red, and Nijiiro behavior remains preserved by focused checks for any shared-code changes.

### Phase 24: White Catalog Profile and Protocol Limits

**Goal:** Bind White `ST7100-1` data, sidecars, AC15 profile, feature flags, limits, and wire placement.
**Depends on:** Phase 23
**Requirements:** WCAT-01, WCAT-02, WCAT-03

**Key deliverables:**

- `WhiteGameDataPaths`, `WhiteRequiredDataFiles`, `IWhiteCatalog`, `WhiteEraGameDataCatalog`, and DI registration rooted through `PathHelper`.
- Shared AC15 catalog-loader binding for White music, medley, tuning, folder, telop, recommendation, movie/customization, Taikojuku, present, and special-BAID data where formats match.
- White server-authored sidecars for implemented metadata features, including intentionally empty files where absence is the runtime contract.
- `Ac15EraProfiles.White` with White feature flags, protocol limits, and wire placement proven from generated wire/data/payload evidence.
- Host copy/exclude rules so White operator data is not published wholesale while White sidecars are present in build output.

**Success criteria:**

1. White catalog initialization validates the proven White data root and fails clearly when required White files are missing.
2. White catalog outputs are populated through shared AC15 loaders only where White data format matches; deltas are handled as White-specific parser work.
3. White feature flags do not advertise item shop, Banacoin authority, battle, Tokkun, WaiWai, gacha runtime, later White behavior, or standalone ChallengeCompe without evidence.
4. White protocol limits and wire placement are explicit before runtime handlers depend on them.
5. Tests prove parser/catalog behavior and build-output copy for White data/sidecars without source-shape assertions.

### Phase 25: White Runtime Capability Binding and Rewards

**Goal:** Bind White identity, userdata, normal play, metadata readback, Taikojuku/Dani where proven, and reward/present state.
**Depends on:** Phase 24
**Requirements:** WSTATE-01, WSTATE-02, WSTATE-03, WSTATE-04, WCOLL-01

**Key deliverables:**

- White-owned EF entities, DbSets, migrations, typed accessors, and handler partials for identity, save/profile, userdata, score/self-best, crowns, favorites, recent songs, profile counters, and reward/progress state.
- White BAID, mydon entry, userdata, initial data, self-best, crowns, folder, telop, recommend, Taikojuku, tournament/check probe, heartbeat, bookkeeping, and related compatibility route behavior according to White evidence.
- White normal `playresult.php` classification and state mutation through shared AC15 helpers bound to concrete White tables and White limits.
- White Taikojuku/Dani runtime writes/readback only if White payload and catalog evidence proves the same contract; otherwise documented absence.
- White reward/present and Don Point behavior from `present.xml` and White protocol fields without Yellow shop/medal or Banacoin state.

**Success criteria:**

1. White cabinet flow can create/find a card, create mydon/profile data, and read White-owned userdata without writing another era's gameplay state.
2. White normal playresult persists and reads back supported normal state through White-owned tables and White protocol limits.
3. Metadata/readback routes are catalog-backed or no-state according to White evidence and do not silently alias Red/Yellow routes.
4. White Dani and reward behavior is implemented only where the White contract is proven; unproven behavior is explicitly absent rather than guessed.
5. Tests cover White handler state changes, no-cross-era/no-cross-mode boundaries, protocol packing, and reward/profile mutations through observable readback.

### Phase 26: White Collectable Data and Don Challenge Evidence

**Goal:** Collect White 0.13 collectable data and bind Don Challenge only if White evidence proves it.
**Depends on:** Phase 25
**Requirements:** WCOLL-02, WCOLL-03

**Key deliverables:**

- White collectable data record with provenance for songs, tones, costumes, titles, special BAID rows, presents, and other 0.13-scoped rewards.
- White sidecar data and schema usage for collectables that cannot be derived directly from raw operator files.
- Evidence decision for White Don Challenge/ChallengeCompe: absent, data-only, embedded-userdata readback, or stateful runtime binding.
- Optional White ChallengeCompe binding only if White 0.13 evidence proves data, field placement, readback surface, reward timing, and state semantics.
- Guardrails preventing Red standalone `challengecompe.php`, Yellow item-shop reward semantics, or generic threshold schemas from leaking into White.

**Success criteria:**

1. White collectable data can be regenerated or audited from local/proven sources, with wiki/OCR material treated only as scoping context unless backed by local evidence.
2. Collectable reward data mutates only White-owned release-song, title, tone, costume, Don Point, or profile flags proven by White runtime behavior.
3. Don Challenge remains absent or data-only unless White-specific evidence proves a runtime/readback contract.
4. If Don Challenge is implemented, White progress/reward state uses explicit field names and White-owned persistence, not Red endpoint assumptions.
5. Tests cover sidecar parsing, provenance-backed data, reward readback, and any implemented ChallengeCompe behavior through observable White surfaces.

### Phase 27: White AdminApi WebUI and Runtime Closeout

**Goal:** Expose implemented White-owned readback surfaces and close v1.4 with automated plus RPCS3/cabinet verification.
**Depends on:** Phase 26
**Requirements:** WVER-01, WVER-02, WVER-03

**Key deliverables:**

- White AdminApi era routing for implemented profile, user settings, scores/history, self-best/crowns, favorites/recent, catalog/customization, Dani if implemented, reward/collectable data, and Don Challenge only if implemented.
- White WebUI routing through existing generic pages and capability-gated navigation, with unsupported surfaces hidden or unavailable.
- Focused tests for White route behavior, handler persistence, catalog parsing, mapper/classifier behavior, protocol packing, build-output copy, AdminApi/WebUI behavior, and no-cross-era/no-cross-mode boundaries.
- Mapperly generated-source inspection evidence for nontrivial White mappings.
- Final runtime verification record covering automated tests, solution/Host build, temp-output Host build if needed, and user-accepted RPCS3/cabinet smoke for implemented White flows.

**Success criteria:**

1. AdminApi and WebUI expose White only for implemented White-owned surfaces and never read/write Blue, Green, Yellow, Red, or Nijiiro gameplay state.
2. Unsupported White item shop, Banacoin authority, battle, Tokkun, WaiWai, gacha runtime, later White updates, and unproven Don Challenge controls are absent.
3. Automated verification covers meaningful White behavior and state boundaries, not superficial generated type, controller attribute, route inventory, or source-text assertions.
4. Mapperly generated-source inspection confirms important White mappings remain source-generator driven and mechanically correct.
5. v1.4 is not called complete until automated verification, build evidence, and RPCS3/cabinet smoke evidence for implemented White flows are recorded.

## Coverage Map

| Requirement | Phase | Status |
|-------------|-------|--------|
| WFND-01 | Phase 23 | Complete |
| WFND-02 | Phase 23 | Pending |
| WFND-03 | Phase 23 | Complete |
| WCAT-01 | Phase 24 | Pending |
| WCAT-02 | Phase 24 | Pending |
| WCAT-03 | Phase 24 | Pending |
| WSTATE-01 | Phase 25 | Pending |
| WSTATE-02 | Phase 25 | Pending |
| WSTATE-03 | Phase 25 | Pending |
| WSTATE-04 | Phase 25 | Pending |
| WCOLL-01 | Phase 25 | Pending |
| WCOLL-02 | Phase 26 | Pending |
| WCOLL-03 | Phase 26 | Pending |
| WVER-01 | Phase 27 | Pending |
| WVER-02 | Phase 27 | Pending |
| WVER-03 | Phase 27 | Pending |

## Archived Phases

<details>
<summary>v1.0 Blue Support (Phases 1-6) - shipped 2026-06-03</summary>

See `.planning/milestones/v1.0-ROADMAP.md` and `.planning/milestones/v1.0-phases/`.

</details>

<details>
<summary>v1.1 Blue Tokkun Mode Support (Phases 7-11) - shipped 2026-06-07</summary>

See `.planning/milestones/v1.1-ROADMAP.md` and `.planning/milestones/v1.1-phases/`.

</details>

<details>
<summary>v1.2 Yellow AC15 Support (Phases 12-17, 16.1, 16.2) - shipped 2026-06-12</summary>

See `.planning/milestones/v1.2-ROADMAP.md` and `.planning/milestones/v1.2-phases/`.

</details>

<details>
<summary>v1.3 Red AC15 Support (Phases 18-22) - shipped 2026-06-16</summary>

See `.planning/milestones/v1.3-ROADMAP.md` and `.planning/milestones/v1.3-phases/`.

</details>

## Progress

| Milestone | Phases | Plans | Status | Shipped |
|-----------|--------|-------|--------|---------|
| v1.0 Blue Support | 1-6 | 30 roadmap plans | Shipped | 2026-06-03 |
| v1.1 Blue Tokkun Mode Support | 7-11 | 7 GSD plans | Shipped | 2026-06-07 |
| v1.2 Yellow AC15 Support | 12-17 plus 16.1 and 16.2 | 38 GSD plans | Shipped | 2026-06-12 |
| v1.3 Red AC15 Support | 18-22 | 29 GSD plans | Shipped | 2026-06-16 |
| v1.4 White AC15 0.13 Support | 23-27 | 0 planned yet | Planning | - |

## Phase Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 23. White Evidence and Era Foundation | 1/3 | In Progress | - |
| 24. White Catalog Profile and Protocol Limits | 0 | Pending | - |
| 25. White Runtime Capability Binding and Rewards | 0 | Pending | - |
| 26. White Collectable Data and Don Challenge Evidence | 0 | Pending | - |
| 27. White AdminApi WebUI and Runtime Closeout | 0 | Pending | - |
