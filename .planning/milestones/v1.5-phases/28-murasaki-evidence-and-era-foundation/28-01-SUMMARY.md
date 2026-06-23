# Phase 28 Summary: Murasaki Evidence And Era Foundation

## Completed

- Recorded IDA-backed route evidence in `28-CONTEXT.md`: `v01r00`, `v06r00`, and `.php` suffixes.
- Added `GameEra.Murasaki` and a first-class `Adapters.GameProtocol.Murasaki` project.
- Generated Murasaki adapter-local wire DTOs from `proto/murasaki` without editing proto inputs.
- Registered Murasaki in Host settings, DI, solution, direct-protobuf fallback, and enabled-era application-part gating.
- Added Murasaki thin controllers as separate files.
- Did not add a Murasaki `initialdatacheck.php` controller.

## Deferred

- Murasaki special/older-version surfaces remain Phase 33: `bestscore.php`, `songhash.php`, `shoppingresult.php`, challenge arrays, `content_info`, `default_option_setting`, and reserved bytes.
