# Roadmap: TaikoLocalServer

## Milestones

- [x] **v1.0 Blue Support** - Phases 1-6 shipped on 2026-06-03. See [v1.0 roadmap archive](milestones/v1.0-ROADMAP.md), [v1.0 requirements archive](milestones/v1.0-REQUIREMENTS.md), and [v1.0 phase artifacts](milestones/v1.0-phases/).
- [x] **v1.1 Blue Tokkun Mode Support** - Phases 7-11 shipped on 2026-06-07. See [v1.1 roadmap archive](milestones/v1.1-ROADMAP.md), [v1.1 requirements archive](milestones/v1.1-REQUIREMENTS.md), and [v1.1 phase artifacts](milestones/v1.1-phases/).
- [ ] **v1.2 Yellow AC15 Support** - Phases 12-17 plus inserted Phases 16.1 and 16.2. Yellow becomes a first-class older AC15 era with Blue-equivalent normal/Tokkun support where Yellow proto/data/runtime evidence supports it.

## Current Planning State

Active milestone: **v1.2 Yellow AC15 Support**

**Goal:** Add Yellow as an era-owned AC15 adapter with Yellow protocol routes, local `ST9100-1` data, normal play, Dani, shop/medals, WaiWai tutorial/logging, Tokkun, compatibility routes, admin readback, and runtime verification, while keeping Blue battle behavior absent.

## v1.2 Phase Summary

| Phase | Name | Goal | Requirements | Success Criteria |
|-------|------|------|--------------|------------------|
| 12 | Yellow Evidence and Era Foundation | Prove Yellow route/version/transport boundaries and add first-class Yellow adapter scaffolding. | YFND-01, YFND-02, YFND-03, YFND-04 | 5 |
| 13 | Yellow Catalog and AC15 Core Foundation | Load Yellow `ST9100-1` catalog data and establish Yellow AC15 profile/core contracts. | YCAT-01, YCAT-02, YCAT-03, YCAT-04 | 5 |
| 14 | Yellow Identity, Userdata, Crowns, Self-Best, and Normal Play | Implement Yellow-owned profile/userdata/normal play loop, including crown encoding proof. | YUSR-01, YUSR-02, YPLY-01, YPLY-02, YCRN-01 | 5 |
| 15 | Yellow Dani, Shop, Medals, WaiWai, and Admin | Implement Yellow Dani, metadata, shop/medals, WaiWai tutorial/logging, and admin readback. | YDAN-01, YSHOP-01, YSHOP-02, YMED-01, YWAI-01, YUI-01 | 5 |
| 16 | Yellow Tokkun and Banacoin Compatibility | Add Yellow Tokkun acceptance/persistence/readback and stateless Banacoin-adjacent compatibility. | YTOK-01, YTOK-02, YTOK-03, YBAN-01 | 5 |
| 16.1 | AC15 Mapperly Mapper Rewrite and Presence Semantics | Regenerate AC15 wire DTOs with nullable optional primitives and move Green/Blue/Yellow protocol projection to real Mapperly generation. | YMAPP-01, YMAPP-02, YMAPP-03 | 6 |
| 16.2 | AC15 Shared Core Simplification and Reuse Cleanup | 11/14 | In Progress|  |
| 17 | Yellow Runtime Verification and Contract Closeout | Prove the full Yellow contract with focused tests, full build/test, runtime smoke, and docs. | YVER-01, YVER-02, YVER-03, YDOC-01 | 5 |

**Coverage:** 34/34 v1.2 requirements mapped exactly once.

### v1.2 Execution Checklist

- [x] **Phase 12: Yellow Evidence and Era Foundation** - Prove Yellow route/version/transport boundaries and add first-class Yellow adapter scaffolding. (completed 2026-06-07)
- [x] **Phase 13: Yellow Catalog and AC15 Core Foundation** - Load Yellow `ST9100-1` catalog data and establish Yellow AC15 profile/core contracts. (completed 2026-06-08)
- [x] **Phase 14: Yellow Identity, Userdata, Crowns, Self-Best, and Normal Play** - Implement Yellow-owned profile/userdata/normal play loop, including crown encoding proof. (completed 2026-06-08)
- [x] **Phase 15: Yellow Dani, Shop, Medals, WaiWai, and Admin** - Implement Yellow Dani, metadata, shop/medals, WaiWai tutorial/logging, and admin readback. (completed 2026-06-08)
- [x] **Phase 16: Yellow Tokkun and Banacoin Compatibility** - Add Yellow Tokkun acceptance/persistence/readback and stateless Banacoin-adjacent compatibility. (completed 2026-06-08)
- [x] **Phase 16.1: AC15 Mapperly Mapper Rewrite and Presence Semantics** - Regenerate AC15 wire DTOs with nullable optional primitives, then rewrite Green/Blue/Yellow protocol mapping around real Mapperly generation and explicit optional-field presence boundaries. (inserted 2026-06-10) (completed 2026-06-10)
- [ ] **Phase 16.2: AC15 Shared Core Simplification and Reuse Cleanup** - Simplify duplicated AC15 Green/Blue/Yellow code by extracting evidence-backed shared core behavior while preserving era-owned persistence, routes, and wire DTOs. (inserted 2026-06-10)
- [ ] **Phase 17: Yellow Runtime Verification and Contract Closeout** - Prove the full Yellow contract with focused tests, full build/test, runtime smoke, and docs.

## Phase Details

### Phase 12: Yellow Evidence and Era Foundation

**Goal:** Prove Yellow route/version/transport boundaries and add first-class Yellow adapter scaffolding.

**Requirements:** YFND-01, YFND-02, YFND-03, YFND-04

**Success criteria:**

1. Yellow route/version evidence records supported endpoints, startup/version ownership, direct-protobuf expectations, and unresolved gaps.
2. Yellow adapter project compiles generated Yellow wire DTOs from `proto/yellow/yellow.proto` and `proto/yellow/vsinterface.proto`.
3. Host settings and application-part registration enable Yellow routes only when Yellow is configured.
4. Runtime behavior checks prove Yellow-supported endpoints are gated by enabled-era configuration.
5. Behavior and persistence-boundary tests prove Blue battle behavior is absent from Yellow without source, route-inventory, or generated-protobuf assertions.

**Plans:** 3/3 plans complete

Plans:

**Wave 1**

- [x] 12-01-PLAN.md - Create Yellow evidence, first-class era enum, adapter project, and generated wire foundation

**Wave 2** *(blocked on Wave 1 completion)*

- [x] 12-02-PLAN.md - Wire Yellow no-state route scaffolding and Host enablement after route-prefix evidence is supplied

**Wave 3** *(blocked on Wave 2 completion)*

- [x] 12-03-PLAN.md - Prove shared startup/version ownership and Yellow no-battle guardrails

### Phase 13: Yellow Catalog and AC15 Core Foundation

**Goal:** Load Yellow `ST9100-1` catalog data and establish Yellow AC15 profile/core contracts.

**Requirements:** YCAT-01, YCAT-02, YCAT-03, YCAT-04

**Success criteria:**

1. Yellow runtime catalog resolves data paths through `PathHelper` and era data helpers.
2. Yellow catalog tests load `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and relevant `ST9100-1` supporting files.
3. AC15 profile/limits/wire-placement contracts describe Yellow capabilities without shared EF tables or shared wire DTOs.
4. Initial-data availability is catalog-backed and omits unsupported feature advertisements.
5. Metadata routes use Yellow catalog-backed data and pass route/mapping tests.

**Plans:** 3/3 plans complete

Plans:

**Wave 1**

- [x] 13-01-PLAN.md - Add the Yellow catalog foundation and DI registration

**Wave 2** *(blocked on Wave 1 completion)*

- [x] 13-02-PLAN.md - Add the Yellow AC15 profile, catalog snapshot bridge, and initial-data/Taikojuku application handlers

**Wave 3** *(blocked on Wave 2 completion)*

- [x] 13-03-PLAN.md - Replace Phase 12 no-state Yellow metadata scaffolds with catalog-backed Mediator routes where Phase 13 owns behavior

### Phase 14: Yellow Identity, Userdata, Crowns, Self-Best, and Normal Play

**Goal:** Implement Yellow-owned profile/userdata/normal play loop, including crown encoding proof.

**Requirements:** YUSR-01, YUSR-02, YPLY-01, YPLY-02, YCRN-01

**Success criteria:**

1. Yellow BAID/profile/default save flow creates and reads Yellow-owned state without Blue/Green/Nijiiro gameplay writes.
2. Yellow userdata readback includes supported profile, settings, unlock, tutorial, favorite, recent, and normal readback fields.
3. Yellow normal playresult persists play history, best rows, profile counters, unlocks, favorites, and recent songs through Yellow-owned tables.
4. Yellow self-best returns correct rows for requested songs/difficulties.
5. Yellow crown tests prove both shared crown packing and exact Yellow response placement/encoding.

**Plans:** 3/3 plans complete

Plans:

**Wave 1**

- [x] 14-01-PLAN.md - Add Yellow-owned persistence and identity/default-save route behavior

**Wave 2** *(blocked on Wave 1 completion)*

- [x] 14-02-PLAN.md - Add Yellow userdata, self-best, and crown readback

**Wave 3** *(blocked on Wave 2 completion)*

- [x] 14-03-PLAN.md - Add Yellow normal playresult persistence and route behavior

### Phase 15: Yellow Dani, Shop, Medals, WaiWai, and Admin

**Goal:** Implement Yellow Dani, metadata, shop/medals, WaiWai tutorial/logging, and admin readback.

**Requirements:** YDAN-01, YSHOP-01, YSHOP-02, YMED-01, YWAI-01, YUI-01

**Success criteria:**

1. Yellow Taikojuku/Dani requests and Dan playresults persist/read back Yellow-owned Dan state.
2. Yellow item-shop info and purchase flows use Yellow response shape, active shop data, duplicate prevention, and Yellow-only unlock writes.
3. Yellow Don/Katsu medal state is updated from playresult/shop flows and remains separate from Banacoin compatibility state.
4. WaiWai tutorial flag persistence/readback and playresult extra logging work where Yellow evidence exposes fields, without special-mode branching.
5. AdminApi/WebUI routes can inspect supported Yellow profile, score, recent/favorite, Dani, shop, Tokkun, and catalog state without cross-era reads/writes.

**Plans:** 9/9 plans complete

Plans:

**Wave 1**

- [x] 15-01-PLAN.md - Add the Yellow-owned Dani schema and helper layer

**Wave 2** *(blocked on Wave 1 completion)*

- [x] 15-02-PLAN.md - Persist and read back Yellow Dani results
- [x] 15-03-PLAN.md - Add Yellow-owned shop state schema

**Wave 3** *(blocked on 15-03 completion)*

- [x] 15-04-PLAN.md - Make Yellow itempurchase stateful and catalog-validated

**Wave 4** *(blocked on 15-02 and 15-04 completion)*

- [x] 15-05-PLAN.md - Route Yellow playresult medals and userdata shop locks through Yellow state

**Wave 5** *(blocked on 15-05 completion)*

- [x] 15-06-PLAN.md - Constrain Yellow WaiWai handling to current protocol evidence

**Wave 6** *(blocked on 15-06 completion)*

- [x] 15-07-PLAN.md - Add Yellow AdminApi profile, score, history, and favorite routes

**Wave 7** *(blocked on 15-07 completion)*

- [x] 15-08-PLAN.md - Add Yellow AdminApi Dani, leaderboard, and catalog readback

**Wave 8** *(blocked on 15-08 completion)*

- [x] 15-09-PLAN.md - Enable Yellow in the existing WebUI readback workflows

### Phase 16: Yellow Tokkun and Banacoin Compatibility

**Goal:** Add Yellow Tokkun acceptance/persistence/readback and stateless Banacoin-adjacent compatibility.

**Requirements:** YTOK-01, YTOK-02, YTOK-03, YBAN-01

**Success criteria:**

1. Yellow Tokkun uploads are classified before normal play handling.
2. Tokkun no-cross-write tests prove no normal, crown, Dani, favorite, recent, shop, medal, profile, battle, or unlock writes.
3. Yellow Tokkun persistence stores nullable tutorial state and append-only raw stage history with raw order, duplicates, and protocol timestamps preserved.
4. Yellow userdata reads back only the proven Tokkun tutorial flag.
5. Yellow Banacoin-adjacent routes log and return compatibility success without wallet/payment/transaction persistence.

**Plans:** 4/4 plans complete

Plans:

**Wave 1**

- [x] 16-01-PLAN.md - Define the Yellow Tokkun classifier contract and Yellow-owned raw history schema
- [x] 16-04-PLAN.md - Tighten Yellow Banacoin-adjacent compatibility as stateless log/success routes

**Wave 2** *(blocked on 16-01 completion)*

- [x] 16-02-PLAN.md - Persist allowed Yellow Tokkun state from classified `playresult.php` uploads

**Wave 3** *(blocked on 16-02 completion)*

- [x] 16-03-PLAN.md - Read back Yellow Tokkun tutorial state through the proven Yellow userdata field only

### Phase 16.1: AC15 Mapperly Mapper Rewrite and Presence Semantics (INSERTED)

**Goal:** Regenerate Green, Blue, and Yellow AC15 wire DTOs with nullable optional primitives, then rewrite protocol mappers so mechanical wire/Common DTO projection is generated by Mapperly, manual code is limited to explicit protocol/domain transforms, and protobuf optional-field presence is represented by nullable wire values instead of scattered `ShouldSerialize*` helper calls.

**Requirements:** YMAPP-01, YMAPP-02, YMAPP-03
**Depends on:** Phase 16
**Plans:** 1/1 plans complete

**Success criteria:**

1. Green, Blue, and Yellow AC15 wire files are regenerated from immutable local proto inputs with repo-local `protogen` nullable optional primitive support, e.g. `+nullablevaluetype=yes`, so optional primitive wire properties are nullable where possible.
2. Green, Blue, and Yellow AC15 mapper classes use real Mapperly-generated `partial` mapping methods for mechanical DTO projection instead of body-only `[Mapper]` methods or plain hand-written mapper classes.
3. Manual mapper/post-processing code remains only where protocol behavior requires it, such as byte packing, collection shaping, era capability gates, raw crown payload construction, or evidence-backed classification.
4. `ShouldSerialize*` generated protobuf presence helpers are removed from production mapper bodies; zero-vs-absent behavior flows through nullable wire properties and explicit common DTO fields.
5. Mapperly diagnostics such as `RMG020` and `RMG012` are no longer globally suppressed for AC15 protocol adapters; any remaining suppressions are narrow, documented, and behavior-tested.
6. Focused tests prove nullable-wire generation, Mapperly-backed mapping parity, optional-field presence/absence, and no cross-era behavior regressions before Phase 17 runtime closeout starts.

Plans:

- [x] 16.1-01-PLAN.md - Rewrite AC15 Green, Blue, and Yellow protocol mappers so optional primitive presence comes from nullable generated wire DTO properties and Mapperly generates mechanical projection code.

### Phase 16.2: AC15 Shared Core Simplification and Reuse Cleanup (INSERTED)

**Goal:** Simplify duplicated AC15 Green/Blue/Yellow code by extracting evidence-backed shared core behavior while preserving era-owned persistence, routes, and wire DTOs.

**Requirements:** AC15REUSE-01, AC15REUSE-02, AC15REUSE-03, AC15REUSE-04
**Depends on:** Phase 16.1

**Success criteria:**

1. Canonical Dan/shop/crown/counter helpers preserve packed flags, clear grades, statuses, and userdata/profile counters.
2. Shop season seed policy and purchase/unlock behavior stay era-correct with no Banacoin authority state.
3. Dani save/readback uses shared behavior without cross-era table writes.
4. Normal play and special-mode boundaries remain intact after playresult, adapter, catalog, and Common DTO cleanup.
5. Focused regression slices, full `dotnet test Tests/Tests.csproj`, and temp-output Host build pass before Phase 17 starts.

**Plans:** 13 plans

Plans:

**Wave 1**

- [x] 16.2-01-PLAN.md - Canonicalize AC15 Dan clear-grade and shop item-status domain values

**Wave 2** *(blocked on 16.2-01 completion)*

- [x] 16.2-02-PLAN.md - Collapse duplicate AC15 Dan helper logic and misleading crown aliases
- [x] 16.2-04-PLAN.md - Make AC15 shop season seed behavior explicit while keeping typed shop state tables
- [x] 16.2-06-PLAN.md - Define AC15 Dani service contracts and typed era adapters

**Wave 3** *(blocked on wave 2 helper/shop slices)*

- [x] 16.2-03-PLAN.md - Replace duplicated AC15 profile counter updates with a typed counter updater
- [x] 16.2-05-PLAN.md - Factor typed AC15 item-shop adapter plumbing without changing unlock policy

**Wave 4** *(blocked on profile counters, item-shop plumbing, and Dani contracts)*

- [x] 16.2-07-PLAN.md - Move AC15 Dan playresult save behavior into `Ac15DaniService`
- [x] 16.2-11-PLAN.md - Canonicalize shared AC15 Common DTO fields while preserving adapter-local wire placement

**Wave 5** *(blocked on Dani save, post-review row-shape contracts, and DTO canonicalization)*

- [x] 16.2-08-PLAN.md - Lock AC15 row-shape interfaces and generic EF helper architecture
- [ ] 16.2-09-PLAN.md - Factor repeated AC15 normal-play internals through generic EF helpers while preserving typed DbSet ownership
- [x] 16.2-12-PLAN.md - Clean up AC15 folder and telop catalog helper duplication without adding routes
- [ ] 16.2-13-PLAN.md - Clean up remaining AC15 recommend, item-shop info, userdata, and initial-data helper duplication

**Wave 6** *(blocked on row-shape architecture and normal-play helper cleanup)*

- [ ] 16.2-10-PLAN.md - Clean up AC15 playresult prelude orchestration without moving special-mode gates

### Phase 17: Yellow Runtime Verification and Contract Closeout

**Goal:** Prove the full Yellow contract with focused tests, full build/test, runtime smoke, and docs.

**Requirements:** YVER-01, YVER-02, YVER-03, YDOC-01
**Depends on:** Phase 16.2

**Success criteria:**

1. Focused Yellow and shared AC15 tests cover nullable generated AC15 wire DTOs, evidence-backed Mapperly mapping, optional-field presence semantics, mapper classification, catalog/parser behavior, handler persistence, SQLite readback, no-cross-era/no-battle boundaries, Tokkun, WaiWai, crown encoding, and AdminApi/WebUI behavior.
2. Full `dotnet test Tests/Tests.csproj` passes.
3. Temp-output Host build passes.
4. Yellow normal and Tokkun cabinet/RPCS3 smoke evidence is recorded.
5. Final Yellow contract documentation records supported features, non-goals, evidence gaps, and operator data expectations.

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

## Progress

| Milestone | Phases | Plans | Status | Shipped |
|-----------|--------|-------|--------|---------|
| v1.0 Blue Support | 1-6 | 30 roadmap plans | Shipped | 2026-06-03 |
| v1.1 Blue Tokkun Mode Support | 7-11 | 7 GSD plans | Shipped | 2026-06-07 |
| v1.2 Yellow AC15 Support | 12-17 plus 16.1 and 16.2 | 34 GSD plans complete; 3 Phase 16.2 plans pending | Active, Phase 16.2 planned before Phase 17 | - |
