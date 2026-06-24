# Roadmap: TaikoLocalServer

## Milestones

- [x] **v1.0 Blue Support** - Phases 1-6 shipped on 2026-06-03. See [v1.0 roadmap archive](milestones/v1.0-ROADMAP.md), [v1.0 requirements archive](milestones/v1.0-REQUIREMENTS.md), and [v1.0 phase artifacts](milestones/v1.0-phases/).
- [x] **v1.1 Blue Tokkun Mode Support** - Phases 7-11 shipped on 2026-06-07. See [v1.1 roadmap archive](milestones/v1.1-ROADMAP.md), [v1.1 requirements archive](milestones/v1.1-REQUIREMENTS.md), and [v1.1 phase artifacts](milestones/v1.1-phases/).
- [x] **v1.2 Yellow AC15 Support** - Phases 12-17 plus inserted Phases 16.1 and 16.2 shipped on 2026-06-12. See [v1.2 roadmap archive](milestones/v1.2-ROADMAP.md), [v1.2 requirements archive](milestones/v1.2-REQUIREMENTS.md), and [v1.2 phase artifacts](milestones/v1.2-phases/).
- [x] **v1.3 Red AC15 Support** - Phases 18-22 shipped on 2026-06-16. See [v1.3 roadmap archive](milestones/v1.3-ROADMAP.md), [v1.3 requirements archive](milestones/v1.3-REQUIREMENTS.md), and [v1.3 phase artifacts](milestones/v1.3-phases/).
- [x] **v1.4 White AC15 0.13 Support** - Phases 23-27 plus inserted Phase 23.1 shipped on 2026-06-21. See [v1.4 roadmap archive](milestones/v1.4-ROADMAP.md), [v1.4 requirements archive](milestones/v1.4-REQUIREMENTS.md), and [v1.4 phase artifacts](milestones/v1.4-phases/).
- [x] **v1.5 Murasaki AC15 Support** - Phases 28-34 plus quick final `/v06r01` route support shipped on 2026-06-23. See [v1.5 roadmap archive](milestones/v1.5-ROADMAP.md), [v1.5 requirements archive](milestones/v1.5-REQUIREMENTS.md), [v1.5 milestone audit](milestones/v1.5-MILESTONE-AUDIT.md), and [v1.5 phase artifacts](milestones/v1.5-phases/).
- [ ] **v1.6 KIMIDORI AC15 Support** - Phases 35-38 complete; ready for milestone closeout. Define KIMIDORI 0.12 from `proto/kimidori`, `.tools/kimidori`, linked root-level game data, `/v01r00` startup/version routes, and `/v05r00` game routes.

## Current Planning State

**Active milestone:** v1.6 KIMIDORI AC15 Support

**Milestone Goal:** Add first-class KIMIDORI 0.12 support by composing existing older-AC15 capabilities around KIMIDORI-owned proto, route, root-level data, persistence, AdminApi/WebUI surfaces, and verification.

**Granularity:** standard
**Coverage:** 19/19 active v1.6 requirements mapped. Future `KLATER-01`, `KLATER-02`, and `KSPEC-01` remain deferred.

**Current closeout state:** Phases 35-38 are complete. Phase 38 automated implementation is verified, user-observed KIMIDORI cabinet/RPCS3 runtime acceptance was recorded on 2026-06-25, and v1.6 is ready for milestone closeout.

## Phases

**Phase Numbering:**

- Integer phases (35, 36, 37, 38): planned milestone work
- Decimal phases (35.1, 35.2): urgent insertions, if needed later

- [x] **Phase 35: KIMIDORI Evidence and Era Foundation** - Prove route/proto/data boundaries and introduce a no-state first-class KIMIDORI adapter.
- [x] **Phase 36: Root-Level Catalog and Metadata Binding** - Load KIMIDORI root-level data and expose only proto-and-route-backed metadata families.
- [x] **Phase 37: KIMIDORI Runtime State, Dani Dojo, and Normal Play** - Bind identity, userdata, normal play, Dani Dojo, self-best, crowns, favorites/recent, rewards, Don Points, and bounded shopping-result behavior through KIMIDORI-owned state.
- [x] **Phase 38: AdminApi, WebUI, and Runtime Closeout** - Expose implemented KIMIDORI-owned state to operators and close with automated plus user-observed runtime verification.

## Phase Details

### Phase 35: KIMIDORI Evidence and Era Foundation

**Goal**: Developers can enable a first-class no-state KIMIDORI era only after route, proto, transport, data-layout, and feature-presence evidence is recorded.
**Depends on**: v1.5 shipped through Phase 34 and quick final `/v06r01` route task
**Requirements**: KFOUND-01, KFOUND-02, KFOUND-03, KFOUND-04
**Success Criteria** (what must be TRUE):

  1. Developer can review a KIMIDORI evidence record identifying `/v01r00` startup/version routes, `/v05r00` game-route ownership, approved `.php` suffixes, direct-protobuf expectations, linked data layout, binary/IDA handles, and unsupported feature gaps.
  2. `GameEra.Kimidori` can be enabled with adapter-local generated wire DTOs from `proto/kimidori`, era settings, Host/DI/application-part gating, `/v05r00/chassis/*.php` no-state route ownership, and shared `/v01r00/chassis/*` startup/version behavior.
  3. Disabled-era checks and supported-era route/build regression checks show KIMIDORI scaffolding does not expose disabled routes or change Blue, Green, Yellow, Red, White, Murasaki, Nijiiro, or shared startup behavior.
  4. Feature inclusion rules are documented and enforced: proto message presence plus binary `.php` route evidence is required, and missing proto features such as KIMIDORI 0.12 Taikojuku remain absent.

**Plans**: 1 complete

### Phase 36: Root-Level Catalog and Metadata Binding

**Goal**: KIMIDORI catalog and metadata behavior are loaded from the linked root-level data layout before runtime state depends on them.
**Depends on**: Phase 35
**Requirements**: KCAT-01, KCAT-02, KCAT-03, KCAT-04
**Success Criteria** (what must be TRUE):

  1. KIMIDORI catalog initialization loads required root-level inputs from `Host/wwwroot/data/kimidori/data`, including `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`, through path/settings abstractions.
  2. KIMIDORI has an explicit AC15 capability/profile model with proven protocol limits, five-song favorite cap unless local evidence proves otherwise, root-level data support, and disabled absent surfaces.
  3. Proto-and-route-backed metadata routes such as default songs, mainichi songs, folders, telops, song hash, heartbeat, bookkeeping, recommendations, and movies return catalog-backed or conservative no-state responses without copying unsupported adjacent-era contracts.
  4. Required server-authored sidecar data exists or intentionally empty sidecars are committed for every implemented KIMIDORI feature that needs data outside raw operator files.

**Plans**: 1 complete

### Phase 37: KIMIDORI Runtime State, Dani Dojo, and Normal Play

**Goal**: KIMIDORI cabinets can create/read profile state, upload normal play, and persist/read Dani Dojo state through KIMIDORI-owned persistence without creating unsupported feature state.
**Depends on**: Phase 36
**Requirements**: KRUN-01, KRUN-02, KRUN-03, KRUN-04, KRUN-05, KDANI-01, KDANI-02
**Success Criteria** (what must be TRUE):

  1. A KIMIDORI cabinet can register or find a card, create mydon/profile defaults, and read userdata from KIMIDORI-owned save/profile tables.
  2. KIMIDORI normal and Dani Dojo playresult uploads update KIMIDORI-owned score, self-best, crown, favorite, recent-song, Dan, reward/progress, Don Point, unlock, and profile-counter rows where KIMIDORI protocol and data shapes match.
  3. KIMIDORI self-best, crown, release-song, favorite, recent-song, reward, and Don Point readback uses KIMIDORI protocol byte and array limits, including compression/packing verified against current wire/data behavior.
  4. KIMIDORI shopping-result compatibility is bounded to proto-and-route-backed uploads and does not invent wallet/payment, shop-season, medal, or unrelated item-shop authority.
  5. Challenge arrays and other ambiguous payloads do not create Don Challenge, ChallengeCompe, battle, Tokkun, Banacoin, Taikojuku practice-folder behavior, or cross-era state unless later KIMIDORI-specific evidence proves that behavior; Dani Dojo remains separate from Taikojuku.

**Plans**: 1 complete

### Phase 38: AdminApi, WebUI, and Runtime Closeout

**Goal**: Operators can use AdminApi/WebUI surfaces for implemented KIMIDORI-owned state, and KIMIDORI support closes only after automated proof plus user-accepted runtime evidence.
**Depends on**: Phase 37
**Requirements**: KADMIN-01, KADMIN-02, KVERIFY-01, KVERIFY-02
**Success Criteria** (what must be TRUE):

  1. AdminApi exposes implemented KIMIDORI-owned profile, score/history, favorite, reward, catalog, and customization surfaces through era-routed contracts, preserving legacy routes where applicable and `/api/kimidori/...` routes through `EraRoute.TryParse`.
  2. WebUI exposes KIMIDORI as a supported era with controls only for implemented KIMIDORI-owned state; unsupported Taikojuku practice-folder, challenge, battle, Tokkun, Banacoin, and full shop-authority controls remain absent.
  3. Automated verification covers KIMIDORI observable routes, handlers, root-level catalog loading, persistence, mapper/classifier behavior, protocol packing, build-output copy, AdminApi/WebUI readback, and no-cross-era/no-cross-mode boundaries.
  4. Closeout records Mapperly generated-source inspection for nontrivial mappings, a Host build using temp output if needed, and user-accepted cabinet/RPCS3 evidence for implemented KIMIDORI flows.

**Plans**: 1 complete; user runtime acceptance recorded on 2026-06-25
**UI hint**: yes

## Progress

| Phase | Milestone | Plans Complete | Status | Completed |
|-------|-----------|----------------|--------|-----------|
| 35. KIMIDORI Evidence and Era Foundation | v1.6 | 1/1 | Complete | 2026-06-23 |
| 36. Root-Level Catalog and Metadata Binding | v1.6 | 1/1 | Complete | 2026-06-23 |
| 37. KIMIDORI Runtime State, Dani Dojo, and Normal Play | v1.6 | 1/1 | Complete | 2026-06-23 |
| 38. AdminApi, WebUI, and Runtime Closeout | v1.6 | 1/1 | Complete | 2026-06-25 |

## Milestone Progress

| Milestone | Phases | Plans | Status | Shipped |
|-----------|--------|-------|--------|---------|
| v1.0 Blue Support | 1-6 | 30 roadmap plans | Shipped | 2026-06-03 |
| v1.1 Blue Tokkun Mode Support | 7-11 | 7 GSD plans | Shipped | 2026-06-07 |
| v1.2 Yellow AC15 Support | 12-17 plus 16.1 and 16.2 | 38 GSD plans | Shipped | 2026-06-12 |
| v1.3 Red AC15 Support | 18-22 | 29 GSD plans | Shipped | 2026-06-16 |
| v1.4 White AC15 0.13 Support | 23-27 plus 23.1 | 10 GSD plans | Shipped | 2026-06-21 |
| v1.5 Murasaki AC15 Support | 28-34 plus quick 260623-2ff | 7 GSD plans plus final-route quick task | Shipped | 2026-06-23 |
| v1.6 KIMIDORI AC15 Support | 35-38 | 4/4 GSD plans | Ready for closeout | - |
