# Roadmap: TaikoLocalServer

## Milestones

- [x] **v1.0 Blue Support** - Phases 1-6 shipped on 2026-06-03. See [v1.0 roadmap archive](milestones/v1.0-ROADMAP.md), [v1.0 requirements archive](milestones/v1.0-REQUIREMENTS.md), and [v1.0 phase artifacts](milestones/v1.0-phases/).
- [x] **v1.1 Blue Tokkun Mode Support** - Phases 7-11 shipped on 2026-06-07. See [v1.1 roadmap archive](milestones/v1.1-ROADMAP.md), [v1.1 requirements archive](milestones/v1.1-REQUIREMENTS.md), and [v1.1 phase artifacts](milestones/v1.1-phases/).
- [x] **v1.2 Yellow AC15 Support** - Phases 12-17 plus inserted Phases 16.1 and 16.2 shipped on 2026-06-12. See [v1.2 roadmap archive](milestones/v1.2-ROADMAP.md), [v1.2 requirements archive](milestones/v1.2-REQUIREMENTS.md), and [v1.2 phase artifacts](milestones/v1.2-phases/).
- [ ] **v1.3 Red AC15 Support** - Phases 18-22 add Red as the next AC15 capability composition, with Red adapter/wire/table ownership, shared AC15 capability bindings, shared older-AC15 ChallengeCompe capability, admin, and runtime verification.

## Current Planning State

Active milestone: **v1.3 Red AC15 Support**

**Goal:** Add Red by composing supported AC15 capabilities with Red config, limits, wire placement, typed persistence, and Red/older-version evidence while preserving era-owned protocol, catalog, state, admin, and verification boundaries.

**Scope guardrails:**

- Red work starts from local Red proto/data/runtime evidence. Wiki Don Challenge context defines product scope only.
- The roadmap is behavior/capability-first. Red is a composition root for supported capabilities, not a clone of any previous era implementation.
- Existing supported-era behavior must be preserved except where shared-code changes are required and existing behavior remains covered.
- Absence in Red proto is not a requirement. Unsupported surfaces stay absent instead of receiving invented stubs.
- Red is not later-era shop/medal/WaiWai/battle work. Red has simple compatibility for observed protocol/profile fields and tutorial-only Tokkun.
- ChallengeCompe is a shared older-AC15 capability. Red is the first binding/proof point for this milestone, not the capability boundary.

## v1.3 Phase Summary

| Phase | Name | Goal | Requirements | Success Criteria |
|-------|------|------|--------------|------------------|
| 18 | Red Evidence and Capability Foundation | 4/4 | Complete   | 2026-06-12 |
| 19 | Red Capability Profile and Catalog Binding | Bind Red catalog/config data into shared AC15 catalog capabilities and define Red capability/profile boundaries. | RCAT-01, RCAT-02 | 5 |
| 20 | Red Runtime Capability Binding and Simple Compatibility | Bind shared identity, userdata, normal-play, Dani, tutorial-only Tokkun, and simple compatibility capabilities to Red-owned state. | RSTATE-01, RSTATE-02, RSTATE-03, RSTATE-04, RSTATE-05, RCOMP-01 | 5 |
| 21 | Older-AC15 ChallengeCompe Capability and Red Binding | Define the shared older-AC15 ChallengeCompe capability and bind/prove it through Red evidence, with stateful behavior only after client/runtime proof defines the contract. | RCOMP-02, RCHAL-01, RCHAL-02 | 5 |
| 22 | Red AdminApi/WebUI and Runtime Closeout | Expose implemented Red-owned readback surfaces and close v1.3 with automated plus cabinet/RPCS3 verification. | RVER-01, RVER-02, RVER-03 | 5 |

**Coverage:** 17/17 v1.3 requirements mapped exactly once.

## Phases

- [ ] **Phase 18: Red Evidence and Capability Foundation** - Prove Red route/version/transport boundaries, inventory Red-supported capabilities, and add first-class Red adapter scaffolding.
- [ ] **Phase 19: Red Capability Profile and Catalog Binding** - Bind Red catalog/config data into shared AC15 catalog capabilities and define Red capability/profile boundaries.
- [ ] **Phase 20: Red Runtime Capability Binding and Simple Compatibility** - Bind shared identity, userdata, normal-play, Dani, tutorial-only Tokkun, and simple compatibility capabilities to Red-owned state.
- [ ] **Phase 21: Older-AC15 ChallengeCompe Capability and Red Binding** - Define the shared older-AC15 ChallengeCompe capability and bind/prove it through Red evidence, with stateful behavior only after client/runtime proof defines the contract.
- [ ] **Phase 22: Red AdminApi/WebUI and Runtime Closeout** - Expose implemented Red-owned readback surfaces and close v1.3 with automated plus cabinet/RPCS3 verification.

## Phase Details

### Phase 18: Red Evidence and Capability Foundation

**Goal:** Prove Red route/version/transport boundaries, inventory Red-supported capabilities, and add first-class Red adapter scaffolding.
**Depends on:** Phase 17
**Requirements:** RFND-01, RFND-02, RFND-03

**Key deliverables:**

- Red evidence record covering route prefix, direct-protobuf transport, shared startup/version ownership, HDD/version mapping, active data-root decision, and unresolved evidence gaps.
- Red capability inventory mapping local proto/data surfaces to shared AC15 capabilities, absent surfaces, and Red/older-AC15 capability candidates.
- First-class Red host/adapter foundation: `GameEra.Red`, generated Red wire DTOs from immutable `proto/red` inputs, Red adapter registration, settings, DI, route ownership, and enabled-era gating.
- Preservation checks showing existing supported-era behavior remains unchanged except for covered shared-code preservation.

**Success criteria:**

1. Developer can inspect a Red evidence artifact before route/controller behavior or capability binding is treated as finalized.
2. Red adapter project compiles generated Red wire DTOs from local `proto/red` inputs without modifying dumped proto files.
3. Host settings and application-part gating enable Red routes only when Red is configured.
4. Shared startup/version and capability-composition boundaries are explicitly proven or left as unresolved gaps before dependent phases assume them.
5. Regression checks cover existing era preservation for any shared-code changes made during foundation work.

**Plans:** 4 plans
Plans:
**Wave 1**

- [x] 18-01-PLAN.md - Red evidence artifact and capability matrix

**Wave 2** *(blocked on Wave 1 completion)*

- [x] 18-02-PLAN.md - Red enum, adapter project, and generated wire foundation

**Wave 3** *(blocked on Wave 2 completion)*

- [x] 18-03-PLAN.md - Host Red settings, enabled-era gating, and validation guard

**Wave 4** *(blocked on Wave 3 completion)*

- [x] 18-04-PLAN.md - Red no-state route probes and runtime smoke gate

### Phase 19: Red Capability Profile and Catalog Binding

**Goal:** Bind Red catalog/config data into shared AC15 catalog capabilities and define Red capability/profile boundaries.
**Depends on:** Phase 18
**Requirements:** RCAT-01, RCAT-02

**Key deliverables:**

- Red catalog paths and required-file validation rooted through `PathHelper` and Red era data helpers, not hardcoded handler paths.
- Shared AC15 catalog capability bindings for matching music, tuning, Taikojuku/Dani, folder, telop, recommendation, movie, and customization data.
- Red capability/profile model that names enabled capabilities, Red config roots, protocol limits, wire placement, typed persistence boundaries, and unsupported surfaces.
- Guardrails so shared `Application/Ac15` modules receive Red configuration/bindings instead of switching on Red internally.

**Success criteria:**

1. Red catalog initialization loads the proven active Red config root and fails clearly when required Red files are missing.
2. Shared AC15 catalog capabilities are reused only where Red local data shape matches; Red-specific parsers are added only for proven deltas.
3. Red capability/profile data reflects Red-supported surfaces without advertising unsupported later-era item-shop, medal, WaiWai, or battle behavior.
4. Catalog-backed metadata routes resolve Red folders, telops, recommendations, movies, customization, and Taikojuku data through shared capability modules where supported.
5. Tests prove Red catalog/profile binding through parser and catalog outputs rather than source-shape assertions.

**Plans:** TBD

### Phase 20: Red Runtime Capability Binding and Simple Compatibility

**Goal:** Bind shared identity, userdata, normal-play, Dani, tutorial-only Tokkun, and simple compatibility capabilities to Red-owned state.
**Depends on:** Phase 19
**Requirements:** RSTATE-01, RSTATE-02, RSTATE-03, RSTATE-04, RSTATE-05, RCOMP-01

**Key deliverables:**

- Red-owned EF state and typed row bindings for profile/userdata, normal play history, self-best, crowns, favorites, recent songs, Dani, and Tokkun tutorial state only.
- Shared AC15 identity/userdata, initial-data, self-best, crown, normal-play, and Taikojuku/Dani capability modules bound through Red config, protocol limits, wire placement, Mapperly delegates, and concrete Red DbSets.
- Simple compatibility for observed reward card, reward execution, Don point, and Banacoin-adjacent routes without item-shop, medal, wallet, payment, or unlock authority.
- No-cross-era tests proving Red writes never land in any existing supported-era gameplay table.

**Success criteria:**

1. Red cabinet can register or find a card, create Red profile/mydon data, and read Red-owned userdata through the shared identity/userdata capability.
2. Red normal playresult persists scores, self-best, crowns, favorites, recent songs, and profile counters through shared normal-play behavior bound to Red-owned tables.
3. Red Taikojuku/Dani requests and Dan playresults persist and read back Red-owned Dan state through shared Dani behavior bound to Red limits and rows.
4. Red Tokkun uploads are classified before normal/Dani/ChallengeCompe handling and persist/read back only tutorial state, with no raw history, progression, reward, or unlock writes.
5. Reward card, reward execution, Don point, and Banacoin-adjacent compatibility stays stateless or profile-field-only as evidence requires, and does not create item-shop, medal, wallet/payment, or non-challenge unlock state.

**Plans:** TBD

### Phase 21: Older-AC15 ChallengeCompe Capability and Red Binding

**Goal:** Define the shared older-AC15 ChallengeCompe capability and bind/prove it through Red evidence, with stateful behavior only after client/runtime proof defines the contract.
**Depends on:** Phase 20
**Requirements:** RCOMP-02, RCHAL-01, RCHAL-02

**Key deliverables:**

- Don Challenge scope record separating wiki product behavior from local implementation authority for the shared older-AC15 ChallengeCompe capability.
- Shared ChallengeCompe capability contract that defines canonical task/progress/readback concepts only where Red proto/runtime evidence proves them.
- Red binding for ChallengeCompe route/response compatibility at the minimal level proven by Red client/runtime evidence.
- Evidence-gated challenge catalog, playresult challenge-array preservation, and Red-owned stateful ChallengeCompe behavior only if request traces, proto placement, and accepted response shape prove the endpoint/payload/schema contract.

**Success criteria:**

1. Developer can inspect a ChallengeCompe evidence artifact documenting monthly task scope, individual/community distinction, normal-play completion, Tokkun exclusion, and song/title reward timing as product context only.
2. Shared ChallengeCompe behavior cites Red proto, local data, logs, RPCS3/cabinet traces, or IDA/client evidence before implementing canonical response rows or state mutation.
3. Minimal compatibility is available only for the proven Red client flow; empty success is not counted as stateful ChallengeCompe support.
4. If stateful ChallengeCompe behavior is implemented, Red playresult challenge IDs persist to Red-owned challenge state and read back through the Red wire endpoint without writing normal/Tokkun/Dani state incorrectly.
5. If evidence does not prove the stateful contract, the phase records the gap and leaves stateful ChallengeCompe behavior absent rather than invented.

**Note:** Phase 21 starts with evidence and compatibility. Stateful ChallengeCompe progress, task-list, community, reward, or schedule behavior is implemented as a shared older-AC15 capability only if Red client/runtime evidence proves the accepted endpoint, payload, schema, and response contract.

**Plans:** TBD

### Phase 22: Red AdminApi/WebUI and Runtime Closeout

**Goal:** Expose implemented Red-owned readback surfaces and close v1.3 with automated plus cabinet/RPCS3 verification.
**Depends on:** Phase 21
**Requirements:** RVER-01, RVER-02, RVER-03

**Key deliverables:**

- AdminApi and WebUI Red routing for implemented Red-owned profile, score/history, favorite/recent, Dani, catalog/customization, Tokkun tutorial, simple compatibility diagnostics if useful, and ChallengeCompe readback surfaces only where implemented.
- Focused Red and shared AC15 tests covering route, handler, catalog, persistence, mapper/classifier, no-cross-era, and no-cross-mode behavior.
- Final Red runtime verification record covering automated tests, temp-output Host build, and cabinet/RPCS3 smoke evidence for implemented normal, Tokkun tutorial, simple compatibility, and ChallengeCompe flows.

**Success criteria:**

1. AdminApi and WebUI expose Red only for implemented Red-owned readback surfaces and do not read or write another era's gameplay state.
2. Unsupported Red WaiWai, battle, item-shop, medal, and unproven challenge controls are absent from UI/admin surfaces.
3. Focused Red verification covers catalog/parser behavior, SQLite state transitions, mappers/classifiers, route behavior, and no-cross-era/no-cross-mode boundaries.
4. Full automated verification and a temp-output Host build pass before v1.3 is called complete.
5. Cabinet/RPCS3 smoke evidence records implemented Red normal, Tokkun tutorial, simple compatibility, and ChallengeCompe behavior, including explicit notes for any ChallengeCompe functionality left evidence-gated/absent.

**Plans:** TBD
**UI hint:** yes

## Coverage Map

| Requirement | Phase | Status |
|-------------|-------|--------|
| RFND-01 | Phase 18 | Complete |
| RFND-02 | Phase 18 | Complete |
| RFND-03 | Phase 18 | Complete |
| RCAT-01 | Phase 19 | Pending |
| RCAT-02 | Phase 19 | Pending |
| RSTATE-01 | Phase 20 | Pending |
| RSTATE-02 | Phase 20 | Pending |
| RSTATE-03 | Phase 20 | Pending |
| RSTATE-04 | Phase 20 | Pending |
| RSTATE-05 | Phase 20 | Pending |
| RCOMP-01 | Phase 20 | Pending |
| RCOMP-02 | Phase 21 | Pending |
| RCHAL-01 | Phase 21 | Pending |
| RCHAL-02 | Phase 21 | Pending |
| RVER-01 | Phase 22 | Pending |
| RVER-02 | Phase 22 | Pending |
| RVER-03 | Phase 22 | Pending |

## Archived Phases

<details>
<summary>v1.0 Blue Support (Phases 1-6) - shipped 2026-06-03</summary>

- [x] Phase 1: Blue A6 item shop and unlocking
- [x] Phase 2: Blue A7 AdminApi and WebUI parity
- [x] Phase 3: Blue A8 normal-mode cabinet smoke and hardening
- [x] Phase 4: Blue battle evidence and design
- [x] Phase 5: Blue battle runtime support
- [x] Phase 6: Full Blue verification and release hardening

See `.planning/milestones/v1.0-ROADMAP.md` and `.planning/milestones/v1.0-phases/`.

</details>

<details>
<summary>v1.1 Blue Tokkun Mode Support (Phases 7-11) - shipped 2026-06-07</summary>

- [x] Phase 7: Tokkun Evidence Contract and Guardrail Reset (1/1 plan, completed 2026-06-03)
- [x] Phase 8: Stateless Banacoin Compatibility and Availability (1/1 plan, completed 2026-06-04)
- [x] Phase 9: Tokkun Mapper and Safe Playresult Acceptance (1/1 plan, completed 2026-06-05)
- [x] Phase 10: Evidence-Backed Tokkun State Persistence and Readback (3/3 plans, completed 2026-06-06)
- [x] Phase 11: Cabinet/RPCS3 Smoke and Contract Tightening (1/1 plan, completed 2026-06-07)

See `.planning/milestones/v1.1-ROADMAP.md` and `.planning/milestones/v1.1-phases/`.

</details>

<details>
<summary>v1.2 Yellow AC15 Support (Phases 12-17, 16.1, 16.2) - shipped 2026-06-12</summary>

- [x] Phase 12: Yellow Evidence and Era Foundation (3/3 plans, completed 2026-06-07)
- [x] Phase 13: Yellow Catalog and AC15 Core Foundation (3/3 plans, completed 2026-06-08)
- [x] Phase 14: Yellow Identity, Userdata, Crowns, Self-Best, and Normal Play (3/3 plans, completed 2026-06-08)
- [x] Phase 15: Yellow Dani, Shop, Medals, WaiWai, and Admin (9/9 plans, completed 2026-06-08)
- [x] Phase 16: Yellow Tokkun and Banacoin Compatibility (4/4 plans, completed 2026-06-08)
- [x] Phase 16.1: AC15 Mapperly Mapper Rewrite and Presence Semantics (1/1 plan, completed 2026-06-10)
- [x] Phase 16.2: AC15 Shared Core Simplification and Reuse Cleanup (14/14 plans, completed 2026-06-11)
- [x] Phase 17: Yellow Runtime Verification and Contract Closeout (1/1 plan, completed 2026-06-12)

See `.planning/milestones/v1.2-ROADMAP.md` and `.planning/milestones/v1.2-phases/`.

</details>

## Progress

| Milestone | Phases | Plans | Status | Shipped |
|-----------|--------|-------|--------|---------|
| v1.0 Blue Support | 1-6 | 30 roadmap plans | Shipped | 2026-06-03 |
| v1.1 Blue Tokkun Mode Support | 7-11 | 7 GSD plans | Shipped | 2026-06-07 |
| v1.2 Yellow AC15 Support | 12-17 plus 16.1 and 16.2 | 38 GSD plans | Shipped | 2026-06-12 |
| v1.3 Red AC15 Support | 18-22 | 0 plans | Planning | - |

## Phase Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 18. Red Evidence and Capability Foundation | 2/4 | In Progress | - |
| 19. Red Capability Profile and Catalog Binding | 0/TBD | Not started | - |
| 20. Red Runtime Capability Binding and Simple Compatibility | 0/TBD | Not started | - |
| 21. Older-AC15 ChallengeCompe Capability and Red Binding | 0/TBD | Not started | - |
| 22. Red AdminApi/WebUI and Runtime Closeout | 0/TBD | Not started | - |
