# Requirements: TaikoLocalServer Blue Support

**Defined:** 2026-05-28
**Core Value:** A Blue cabinet can use TaikoLocalServer for normal and battle play with repeatable cabinet/RPCS3 evidence, without corrupting or conflating Green, Nijiiro, or shared identity state.

## v1 Requirements

### Blue Item Shop And Unlocking

- [x] **SHOP-01**: Blue `initialdatacheck.php` advertises item-shop version data only when Blue shop is enabled and the active Blue shop season has rows.
- [x] **SHOP-02**: Blue `getitemshopinfo.php` returns the configured active Blue shop season, including season fields and ordered item rows with protocol `item_no` values.
- [x] **SHOP-03**: Blue item-shop catalog loading uses `blue_item_shop_data.json` and fails fast when shop is enabled but the file, active season, or required season fields are invalid.
- [x] **SHOP-04**: Blue item purchases validate `item_no`, `item_type`, `item_id`, and `item_price` against the active Blue catalog before spending medals or changing save state.
- [x] **SHOP-05**: Blue shop medal totals are stored by BAID and Blue shop season without using Green shop tables or global Green medal state.
- [x] **SHOP-06**: Blue purchase and reward execution unlock configured song, tone, costume, title, or related Blue save-state fields without writing Green or Nijiiro state.
- [x] **SHOP-07**: Blue userdata readback hides configured locked shop items until the player purchases or receives the matching reward.
- [x] **SHOP-08**: Blue item shop implementation has source guards and regression tests proving it does not reference Green shop state, Green protocol constants, or Green wire models.

### Blue AdminApi And WebUI Parity

- [x] **WEB-01**: AdminApi supports Blue profile/readback surfaces for the same shared card/user flows already available for supported eras.
- [x] **WEB-02**: AdminApi exposes Blue score, play history, favorites, Dani, customization catalog, and item-shop-relevant readback where Blue data exists.
- [x] **WEB-03**: WebUI era routing and API URL generation treat Blue as an explicit era and never fall back silently to Green or Nijiiro.
- [x] **WEB-04**: WebUI Blue pages display supported Blue profile, song, score, Dani, favorite, customization, and shop state using Blue catalog and persistence data.
- [x] **WEB-05**: WebUI hides or makes read-only any Blue edit surface whose write semantics are not implemented for Blue.
- [x] **WEB-06**: Blue AdminApi/WebUI tests prove basic readback does not mutate Green save state or query Green era tables for Blue user data.

### Normal-Mode Cabinet Smoke And Hardening

- [x] **SMOKE-01**: A documented Blue normal-mode smoke checklist covers boot, new card registration, known card login, song list, normal play, playresult save, self-best/crown readback, Dani, item shop purchase/reward unlock, and WebUI readback.
- [x] **SMOKE-02**: A Blue cabinet/RPCS3 run with Blue enabled and other game-protocol eras disabled can boot and reach the expected normal-mode flows.
- [x] **SMOKE-03**: Blue playresult, self-best, crowns, Dani, reward, and item-shop behavior have captured pass/fail evidence from cabinet/RPCS3 smoke testing.
- [x] **SMOKE-04**: Unexpected Blue endpoint calls are logged with bounded request context and triaged into Track B, a focused follow-up, or an explicit non-goal.
- [x] **SMOKE-05**: Full server regression tests and a temp-output Host build pass after normal-mode Blue hardening.

### Blue Battle Evidence And Design

- [x] **BTEV-01**: Battle menu entry and at least one attempted battle flow have cabinet/RPCS3 logs or equivalent client evidence.
- [x] **BTEV-02**: Blue battle-related proto messages are mapped to generated Blue wire types and documented with request/response ownership.
- [x] **BTEV-03**: Local Blue `config/S10100-1/battle` files are inventoried and classified as required, optional, or unknown for battle menu entry.
- [x] **BTEV-04**: Battle release flag byte widths, NPC state defaults, costume/special defaults, token defaults, stage assignment defaults, and boss/last-stage defaults are confirmed from proto, data, IDA/client evidence, or cabinet traces.
- [x] **BTEV-05**: The battle design decides whether battle playresults affect normal Blue scores/crowns based on client evidence.
- [x] **BTEV-06**: Battle implementation is blocked until BTEV-01 through BTEV-05 are satisfied or explicitly revised with user approval.

### Blue Battle Runtime

- [x] **BTL-01**: Blue battle persistence stores battle user state, NPC state, unlock flags, selected specials, stage assignment, tokens, boss life, and last-stage state in Blue-owned tables.
- [x] **BTL-02**: Blue `battleuserdata.php` returns an IDA-backed safe starter for first-use users, then reads back persisted Blue battle user, NPC, selected-special, token, and assignment state from client-reported playresults.
- [x] **BTL-03**: Blue `initialdatacheck.php` advertises battle availability and release flags according to the approved battle design.
- [x] **BTL-04**: Blue `playresult.php` safely maps and persists `BattleStageData` without corrupting normal Blue playresult, self-best, crown, Dani, or shop state.
- [x] **BTL-05**: Blue battle rewards and unlocks update Blue battle and normal save state only where the approved design says they should.
- [x] **BTL-06**: Battle controllers, handlers, mappers, entities, migrations, and tests are Blue-owned and do not depend on Green AI Battle semantics.

### Full Blue Verification

- [ ] **FULL-01**: Normal Blue and battle Blue flows have repeatable cabinet/RPCS3 smoke evidence with date, enabled eras, data paths, and observed endpoint calls.
- [ ] **FULL-02**: Automated tests cover Blue item shop, AdminApi/WebUI parity, normal-mode smoke guardrails, battle evidence assumptions, and battle runtime behavior.
- [ ] **FULL-03**: Source guard tests prove Blue code does not depend on Green protocol constants, Green wire models, Green shop state, or Green AI Battle implementation as truth.
- [ ] **FULL-04**: Documentation explains Blue data setup, Blue shop setup, battle evidence requirements, smoke checklist steps, and remaining unresolved items.
- [ ] **FULL-05**: Any unresolved Blue issues are classified as Track B follow-up, future enhancement, non-goal, or known environment limitation.

## v2 Requirements

No v2 requirements are currently planned inside this GSD project. The user selected full Blue scope, so Track B battle mode remains in v1 after the strict evidence gate.

## Out of Scope

| Feature | Reason |
|---------|--------|
| Tokkun mode behavior | Existing Blue roadmap marks Tokkun as ended before Blue service and not part of Blue support. |
| Banacoin balance/payment/error/info behavior | Payment state is explicitly excluded unless cabinet evidence proves a safe non-payment response is required. |
| Yellow or earlier era support | This project is scoped to Blue support only. |
| Green AI Battle changes | Blue battle mode needs its own evidence and design; Green AI Battle is contrast material only. |
| Runtime scraping of wiki or official pages | Official/public sources are research inputs and curation references, not runtime dependencies. |
| OCR as authoritative shop data | OCR can create candidates only when source/image provenance is preserved and human-reviewed. |
| Shared Blue/Green/Nijiiro persistence for era-owned state | Era state must stay separate except for truly shared identity/card state. |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| SHOP-01 | Phase 1 | Complete |
| SHOP-02 | Phase 1 | Complete |
| SHOP-03 | Phase 1 | Complete |
| SHOP-04 | Phase 1 | Complete |
| SHOP-05 | Phase 1 | Complete |
| SHOP-06 | Phase 1 | Complete |
| SHOP-07 | Phase 1 | Complete |
| SHOP-08 | Phase 1 | Complete |
| WEB-01 | Phase 2 | Complete |
| WEB-02 | Phase 2 | Complete |
| WEB-03 | Phase 2 | Complete |
| WEB-04 | Phase 2 | Complete |
| WEB-05 | Phase 2 | Complete |
| WEB-06 | Phase 2 | Complete |
| SMOKE-01 | Phase 3 | Complete |
| SMOKE-02 | Phase 3 | Complete |
| SMOKE-03 | Phase 3 | Complete |
| SMOKE-04 | Phase 3 | Complete |
| SMOKE-05 | Phase 3 | Complete |
| BTEV-01 | Phase 4 | Complete |
| BTEV-02 | Phase 4 | Complete |
| BTEV-03 | Phase 4 | Complete |
| BTEV-04 | Phase 4 | Complete |
| BTEV-05 | Phase 4 | Complete |
| BTEV-06 | Phase 4 | Complete |
| BTL-01 | Phase 5 | Complete |
| BTL-02 | Phase 5 | Complete |
| BTL-03 | Phase 5 | Complete |
| BTL-04 | Phase 5 | Complete |
| BTL-05 | Phase 5 | Complete |
| BTL-06 | Phase 5 | Complete |
| FULL-01 | Phase 6 | Pending |
| FULL-02 | Phase 6 | Pending |
| FULL-03 | Phase 6 | Pending |
| FULL-04 | Phase 6 | Pending |
| FULL-05 | Phase 6 | Pending |

**Coverage:**

- v1 requirements: 36 total
- Mapped to phases: 36
- Unmapped: 0

---
*Requirements defined: 2026-05-28*
*Last updated: 2026-05-28 after roadmap creation*
