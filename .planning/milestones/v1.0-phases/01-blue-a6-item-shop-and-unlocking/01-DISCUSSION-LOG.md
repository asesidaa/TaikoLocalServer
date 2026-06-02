# Phase 1: Blue A6 Item Shop And Unlocking - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md; this log preserves the alternatives considered.

**Date:** 2026-05-29
**Phase:** 1-Blue A6 Item Shop And Unlocking
**Areas discussed:** Purchase vs rewardexecution flow, Blue shop data source and committed defaults, Season medal state and first-touch seeding, Unlock domains and title handling

---

## Purchase vs rewardexecution flow

| Question | Options Presented | User's Choice | Notes |
|----------|-------------------|---------------|-------|
| What should Blue A6 assume for the cabinet shop unlock sequence? | Direct purchase unlock; Two-step pending reward; Evidence-gated hybrid; Other | Other | User clarified that reward execution is not related to item purchase. |
| What should Phase 1 do with Blue `rewardexecution.php`? | Keep safe stub; Implement non-shop rewards; Evidence gate; Other | Keep safe stub | Log and return success if any request is sent. |
| How should Blue handle the `itempurchase.php` preflight request? | Mirror Green preflight; Treat as no-op success; Reject preflight; Other | Mirror Green preflight | `item_no == 0` with omitted optional item fields returns active-season totals. |
| Should a successful Blue purchase unlock immediately inside `itempurchase.php`? | Unlock immediately; Persist purchase only; You decide; Other | Unlock immediately | Purchase spends medals, persists item, applies Blue save bits, and returns success. |

---

## Blue shop data source and committed defaults

| Question | Options Presented | User's Choice | Notes |
|----------|-------------------|---------------|-------|
| What should Phase 1 deliver for `blue_item_shop_data.json`? | Operator-supplied only; Commit curated defaults; Minimal sample only; Other | Other | Provide default values from official cache at `H:\taiko\blue\rewardshopdata.bin`; other data is user-supplied. |
| What if cache rows cannot resolve to known Blue catalog IDs? | Commit only resolved rows; Block until fully resolved; Use placeholders; Other | Other | This should not happen for official data; if it does, discuss because parser/data parsing likely has an issue. |
| Commit derived JSON or parser only? | Commit derived JSON; Commit parser only; You decide; Other | Commit derived JSON | Commit derived `blue_item_shop_data.json` and tests proving it matches the cache. |
| Commit/copy `rewardshopdata.bin` as a fixture? | Do not commit binary; Commit binary fixture; You decide; Other | Do not commit binary | Keep binary local; tests should use repo-friendly expected values. |

---

## Season medal state and first-touch seeding

| Question | Options Presented | User's Choice | Notes |
|----------|-------------------|---------------|-------|
| How should active-season Don medal state be initialized? | Mirror Green seeding; Always start seasons at zero; Use global totals directly; Other | Always start seasons at zero | No legacy Blue shop data exists, so separate from the beginning. |
| Where should newly earned Don medals from `playresult.php` go when shop is enabled? | Active season only; Both global and active season; Global only; Other | Active season only | `UserSaveDataBlue.TotalGetDonmedal` should not be used for Blue shop accounting. |
| When shop is disabled, should `playresult.php` write Blue Don medal totals anywhere? | Do not track shop medals; Keep legacy global update; You decide; Other | Do not track shop medals | Disabled shop means no shop medal accounting path. |
| What should Blue BAID and `itempurchase.php` responses report for medal totals? | Active-season totals when enabled, zero when disabled; Active-season totals when enabled, global when disabled; Always omit/zero item-shop totals; Other | Active-season totals when enabled, zero when disabled | Enabled reports active-season totals; disabled reports `0/0`. |

---

## Unlock domains and title handling

| Question | Options Presented | User's Choice | Notes |
|----------|-------------------|---------------|-------|
| Which Blue item types should `itempurchase.php` support in Phase 1? | Supported AC15 shop types only; Include title unlocks too; Cache-driven; Other | Supported AC15 shop types only | Support item types `1..7`; title shop purchases are out of scope unless cache proves a title domain and mapping is discussed. |
| Should Blue locking apply to both `initialdatacheck.php` and profile-scoped readback? | Mirror Green locking; Profile-scoped only; Songs only in initialdata; Other | Mirror Green locking | Initialdata hides active-season shop songs; userdata/BAID hide locked active-season songs, tones, and costume slots. |
| What should unsupported or future shop item types do? | Fail catalog load; Load but never unlock; Ignore unknown rows; Other | Fail catalog load | Unknown rows must not be advertised, purchased, or silently dropped. |
| Should Blue purchase set the same save bitsets as Green for item types `1..7`? | Yes, same mapping; Cache/client evidence must confirm; Songs only for now; Other | Yes, same mapping | Use Blue fields and `BlueProtocolBytes`: 1 song, 2 tone, 3 kigurumi, 4 body, 5 head, 6 face, 7 puchi. |

---

## the agent's Discretion

None.

## Deferred Ideas

None.
