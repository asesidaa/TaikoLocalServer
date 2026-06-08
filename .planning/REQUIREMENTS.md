# Requirements: Yellow AC15 Support

**Defined:** 2026-06-07
**Milestone:** v1.2 Yellow AC15 Support
**Core Value:** AC15 cabinets can use TaikoLocalServer through era-correct protocol, catalog, persistence, and admin surfaces without corrupting or conflating Blue, Green, Yellow, Nijiiro, or shared identity state.

## v1.2 Requirements

### Yellow Foundation

- [x] **YFND-01**: Developer can review a Yellow route/version evidence record that identifies supported endpoints, startup/version routing, direct-protobuf transport expectations, and unresolved client-evidence gaps.
- [x] **YFND-02**: Yellow cabinet routes are served by a first-class Yellow adapter with generated Yellow wire DTOs, era settings, host registration, and route ownership tests.
- [x] **YFND-03**: Yellow game routes are present only when Yellow is enabled, and disabled Yellow routes remain absent from the host.
- [x] **YFND-04**: Yellow battle behavior is proven absent by proto/route tests; no Yellow battleuserdata route, battle fields, battle persistence, or Blue battle fallback is exposed.

### Yellow Catalog And AC15 Core

- [x] **YCAT-01**: Yellow catalog initialization loads required local data from `Host/wwwroot/data/yellow/data/config/ST9100-1` and related Yellow data roots through `PathHelper` and era catalog helpers.
- [x] **YCAT-02**: Yellow uses AC15 shared catalog/loaders/services only where formats and behavior match, while preserving Yellow-owned catalog contracts and feature capability flags.
- [x] **YCAT-03**: Yellow initial data advertises only protocol-supported Yellow telop, folder, Taikojuku/Dani, item shop, legal terms, and metadata availability.
- [x] **YCAT-04**: Yellow folder, telop, recommendation, tournament, gacha, challenge, movie, and other metadata routes return Yellow catalog-backed responses where the Yellow proto/data supports them.

### Yellow Identity And Normal Play

- [x] **YUSR-01**: Yellow cabinet can register or find a card, obtain BAID/profile data, and create default Yellow-owned save state without writing Blue, Green, or Nijiiro save tables.
- [x] **YUSR-02**: Yellow cabinet can read userdata with profile fields, settings, unlock flags, favorite songs, recent songs, tutorial flags, and supported Yellow readback fields from Yellow-owned state.
- [x] **YPLY-01**: Yellow normal playresult uploads persist Yellow-owned play history, best scores, profile counters, unlocks, favorites, and recent songs without touching Blue, Green, or Nijiiro gameplay state.
- [x] **YPLY-02**: Yellow self-best requests return Yellow-owned best score rows for requested songs and difficulties with the correct normal/Ura/Shin support for Yellow.
- [x] **YCRN-01**: Yellow crown readback uses proven Yellow crown placement and response encoding, including an explicit compression/raw-byte test instead of assuming Blue/Green gzip behavior.
- [x] **YDAN-01**: Yellow Taikojuku/Dani requests and Dan playresults persist and read back Yellow-owned Dan state with Yellow profile limits and no Green/Blue Dan table writes.

### Yellow Shop, Medals, WaiWai, And Admin

- [x] **YSHOP-01**: Yellow item-shop info returns Yellow catalog seasons/items with the Yellow proto response shape and without Blue-only shop timing fields unless Yellow evidence proves them.
- [x] **YSHOP-02**: Yellow item purchases validate active shop rows, prevent duplicate purchases, spend/update Yellow-owned shop state, and apply supported item unlocks only to Yellow save data.
- [x] **YMED-01**: Yellow Don/Katsu medal totals from playresults and shop flows persist in Yellow-owned state and remain separate from Banacoin compatibility state.
- [x] **YWAI-01**: Yellow WaiWai handling, where current Yellow wire/runtime evidence exposes fields, persists and reads back only the tutorial flag and logs additional playresult fields without treating WaiWai as a special play mode.
- [x] **YUI-01**: AdminApi and WebUI can route to Yellow-era profile, score, favorites/recent, Dani, shop-relevant, Tokkun, and supported catalog readback without reading or writing Blue/Green tables.

### Yellow Tokkun And Banacoin Compatibility

- [ ] **YTOK-01**: Yellow Tokkun playresults are classified before normal handling and return success without normal score, crown, Dani, favorite, recent, shop, medal, profile, battle, or unlock writes.
- [ ] **YTOK-02**: Yellow Tokkun persists only protocol-backed nullable tutorial state and append-only raw stage history while preserving raw song order, duplicates, and client-protocol timestamp fields.
- [ ] **YTOK-03**: Yellow userdata reads back only the Tokkun tutorial flag through proven Yellow protocol fields and does not invent Tokkun summary/history response surfaces.
- [ ] **YBAN-01**: Yellow Banacoin-adjacent routes log requests and return compatibility success without wallet, balance, payment, coupon, settlement, receipt, or transaction persistence unless new Yellow evidence proves a stateful role.

### Verification And Contract

- [ ] **YVER-01**: Yellow support has focused route, mapper, catalog, handler, EF persistence, no-cross-era-write, no-battle, Tokkun, WaiWai, crown-encoding, AdminApi/WebUI, and AC15 shared-core regression tests.
- [ ] **YVER-02**: Full server verification passes with `dotnet test Tests/Tests.csproj` and a temp-output Host build.
- [ ] **YVER-03**: Yellow normal and Tokkun cabinet/RPCS3 smoke evidence is recorded before v1.2 is closed.
- [ ] **YDOC-01**: Final Yellow route/state/semantic contract documentation records supported features, explicit non-goals, evidence gaps, and operator data expectations.

## Future Requirements

### Later AC15 Eras

- **RED-01**: Red support can be planned as a later milestone with its own proto/data evidence and era-owned state.

### WaiWai Expansion

- **YWAI-02**: Any WaiWai behavior beyond tutorial flag persistence/readback and playresult logging can be added only after Yellow-specific local evidence proves a server-facing contract.

### Banacoin Authority

- **YBAN-02**: Real Banacoin wallet/payment/transaction behavior can be reconsidered only if the repo intentionally becomes a Banacoin authority and receives concrete client/protocol evidence.

### Yellow Battle Evidence

- **YBTL-01**: Yellow battle-like behavior can be planned only if concrete Yellow proto/log/client evidence appears.

## Out of Scope

| Feature | Reason |
|---------|--------|
| Red support | Separate future milestone; do not expand v1.2 beyond Yellow. |
| Yellow battle mode | Local Yellow proto lacks Blue battle fields/routes; copying Blue battle would create fake support. |
| Blue/Green/Nijiiro table reuse for Yellow gameplay | Violates era-state separation and makes runtime audits ambiguous. |
| Shared generated AC15 wire assembly | Generated DTOs and optional-field semantics stay adapter-owned. |
| Shared AC15 EF save table | Shared behavior must flow through typed Yellow persistence adapters, not a discriminator table. |
| Real Banacoin balance/payment/transaction persistence | TaikoLocalServer is not a Banacoin authority for this milestone. |
| WaiWai as a play mode | User clarified WaiWai is not a mode; support is tutorial flag persistence/readback plus playresult logging where evidence exposes fields. |
| Invented Tokkun rewards, unlocks, score/crown writes, paid-coin behavior, practice-time accounting, jump-point behavior, autoplay behavior, or speed-change semantics | Tokkun persistence/readback must remain protocol-backed and no-cross-mode. |
| Runtime scraping of wiki/official pages | Public pages are scoping context only; local evidence decides protocol behavior. |

## Traceability

Roadmap phase mapping is created during `$gsd-new-milestone` roadmap generation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| YFND-01 | Phase 12 | Complete |
| YFND-02 | Phase 12 | Complete |
| YFND-03 | Phase 12 | Complete |
| YFND-04 | Phase 12 | Complete |
| YCAT-01 | Phase 13 | Complete |
| YCAT-02 | Phase 13 | Complete |
| YCAT-03 | Phase 13 | Complete |
| YCAT-04 | Phase 13 | Complete |
| YUSR-01 | Phase 14 | Complete |
| YUSR-02 | Phase 14 | Complete |
| YPLY-01 | Phase 14 | Complete |
| YPLY-02 | Phase 14 | Complete |
| YCRN-01 | Phase 14 | Complete |
| YDAN-01 | Phase 15 | Complete |
| YSHOP-01 | Phase 15 | Complete |
| YSHOP-02 | Phase 15 | Complete |
| YMED-01 | Phase 15 | Complete |
| YWAI-01 | Phase 15 | Complete |
| YUI-01 | Phase 15 | Complete |
| YTOK-01 | Phase 16 | Pending |
| YTOK-02 | Phase 16 | Pending |
| YTOK-03 | Phase 16 | Pending |
| YBAN-01 | Phase 16 | Pending |
| YVER-01 | Phase 17 | Pending |
| YVER-02 | Phase 17 | Pending |
| YVER-03 | Phase 17 | Pending |
| YDOC-01 | Phase 17 | Pending |

**Coverage:**

- v1.2 requirements: 27 total
- Mapped to phases: 27
- Unmapped: 0

---
*Requirements defined: 2026-06-07*
*Last updated: 2026-06-08 after Phase 15 verification closeout*
