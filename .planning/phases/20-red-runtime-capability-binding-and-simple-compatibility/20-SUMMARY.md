# Phase 20 Summary - Red Runtime Capability Binding and Simple Compatibility

**Completed:** 2026-06-13
**Outcome:** Red now has first-class runtime persistence for supported AC15 capabilities without duplicating Green/Blue/Yellow mechanisms.

## Implemented

- Added Red-owned EF runtime state and migration for profile/userdata, normal play history, self-best/crowns, favorites, recent songs, and Dani score/stage rows.
- Added Red save-data defaults and Red typed bindings for shared AC15 profile counters, unlock flags, normal-play row mapping, and Dani row mapping.
- Added Red identity, mydon registration, userdata, self-best, crown, and playresult handler partials.
- Replaced Phase 18 Red probes for `baidcheck.php`, `mydonentry.php`, `userdata.php`, `selfbest.php`, `crownsdata.php`, and `playresult.php` with Mediator-backed controller flow.
- Added Red Mapperly mappers for BAID, userdata, self-best, and playresult plus Red crown packing through the shared crown service.
- Added Red tests for identity, userdata readback, normal play, Dani, tutorial-only Tokkun, and wire mapper placement.

## Boundaries Preserved

- No Red item-shop, medal, shop-season, wallet/payment, battle, WaiWai, AdminApi, WebUI, raw Tokkun history, or ChallengeCompe state was introduced.
- Red Tokkun classifies before normal/Dani/Challenge handling and writes only `TokkunTutorialFlg`.
- Red compatibility routes remain stateless success/log endpoints; they do not authorize unlocks, Don point changes, wallet balance, coupons, payments, receipts, or transactions.

## Verification

See `20-VERIFICATION.md`.

## Next

Phase 21 may start older-AC15 ChallengeCompe evidence and binding. This run stopped at Phase 20 per `--to 20`.
