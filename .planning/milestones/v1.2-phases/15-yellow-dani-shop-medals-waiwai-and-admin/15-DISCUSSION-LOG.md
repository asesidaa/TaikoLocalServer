# Phase 15: Yellow Dani, Shop, Medals, WaiWai, and Admin - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md - this log preserves the alternatives considered.

**Date:** 2026-06-08
**Phase:** 15-Yellow Dani, Shop, Medals, WaiWai, and Admin
**Areas discussed:** Yellow Dani runtime, Yellow shop and reward routes, Don/Katsu medal accounting, WaiWai tutorial and logging, AdminApi and WebUI Yellow readback

---

## Yellow Dani Runtime

| Option | Description | Selected |
|--------|-------------|----------|
| Yellow-owned Dan state | Persist Dan best/stage rows and save summary fields in Yellow-owned tables, using Blue/Green only as analogs. | yes |
| Reuse Green/Blue Dan tables | Faster mechanically but violates era-state separation and auditability. | |
| Catalog-only Taikojuku | Leaves Dan playresult/readback incomplete and fails YDAN-01. | |

**User's choice:** Coordinator rule selected all required Phase 15 areas; requirements imply Yellow-owned Dan state.
**Notes:** Phase 13 already made Taikojuku catalog-backed; Phase 15 owns Dan playresult persistence/readback. Unknown Dan ids and invalid clear grades should return protocol success without Dan writes.

---

## Yellow Shop And Reward Routes

| Option | Description | Selected |
|--------|-------------|----------|
| Yellow adapter over shared AC15 shop service | Reuse proven validation/spend/unlock flow while keeping Yellow tables and unlock policy era-owned. | yes |
| Copy Blue shop implementation directly | Risks Blue-specific response/timing/state assumptions. | |
| Leave purchases no-state | Fails YSHOP-02 and success criterion 2. | |

**User's choice:** Requirements require active shop data, duplicate prevention, Yellow response shape, and Yellow-only unlock writes.
**Notes:** `getitemshopinfo.php` is already catalog-backed. `itempurchase.php` remains a no-state scaffold entering Phase 15. `rewardcardcheck.php` and `rewardexecution.php` should stay log-and-success unless Yellow evidence proves a state-changing role.

---

## Don/Katsu Medal Accounting

| Option | Description | Selected |
|--------|-------------|----------|
| Yellow-owned medal and shop-season state | Keep profile totals and shop spend state in Yellow tables, separate from Banacoin. | yes |
| Treat medals as Banacoin balance | Conflicts with YMED-01 and Banacoin non-goals. | |
| Don-only profile totals | Preserves current Phase 14 behavior but misses shop spend/readback needs. | |

**User's choice:** Requirements require Don/Katsu medal state to remain separate from Banacoin compatibility state.
**Notes:** Phase 14 already accumulates direct `get_donmedal` and `get_katsumedal` playresult counters on Yellow save data. Phase 15 should add shop-season Don medal spend state and avoid inventing Katsu spend semantics without evidence.

---

## WaiWai Tutorial And Logging

| Option | Description | Selected |
|--------|-------------|----------|
| Tutorial/readback plus diagnostic logging only | Matches YWAI-01 and the project rule that WaiWai is not a mode. | yes |
| Treat WaiWai as special play mode | Explicitly rejected by requirements. | |
| Invent full WaiWai progression state | Requires evidence that is not present in current scope. | |

**User's choice:** Requirements constrain WaiWai to tutorial flag persistence/readback and extra playresult logging where Yellow evidence exposes fields.
**Notes:** Current generated Yellow wire should be checked for exact WaiWai field presence before adding response mapping. Existing common stage rows already support `WaiwaiResult` and `WaiwaiGauge`; those values should not become authoritative unlock/score/shop state.

---

## AdminApi And WebUI Yellow Readback

| Option | Description | Selected |
|--------|-------------|----------|
| Add Yellow as supported AC15 era | Extend existing era-aware AdminApi/WebUI routes with Yellow-owned reads and supported catalog state. | yes |
| Blue/Green fallback reads | Would hide missing Yellow implementation and violate state separation. | |
| Protocol-only Phase 15 | Fails YUI-01 and success criterion 5. | |

**User's choice:** Requirements require AdminApi/WebUI inspection for supported Yellow profile, score, recent/favorite, Dani, shop, Tokkun, and catalog state.
**Notes:** Current `WebUiEra.Supported` and AdminApi Dan readback support Nijiiro/Green/Blue but not Yellow. Tokkun in Phase 15 should be routing/readback readiness only; Phase 16 owns actual Tokkun persistence/readback.

---

## the agent's Discretion

- The coordinator explicitly instructed this discuss-stage agent to cover all Phase 15 requirement/success-criteria areas rather than narrowing scope.
- No interactive workflow prompt required user input after the coordinator supplied Phase 15 requirements, success criteria, and default selection rules.
- Downstream agents retain discretion over plan slicing, precise helper extraction, and test organization during planning/execution.

## Deferred Ideas

- Yellow Tokkun classification/persistence/readback and Banacoin compatibility are Phase 16.
- Yellow runtime cabinet/RPCS3 smoke and final contract closeout are Phase 17.
- Yellow battle remains out of scope without new concrete evidence.
