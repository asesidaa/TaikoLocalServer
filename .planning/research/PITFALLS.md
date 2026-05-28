# Blue Support Research: Pitfalls

**Date:** 2026-05-28
**Scope:** Risks and evidence gaps for the remaining Blue support project.

## A6 Pitfalls

- Reusing Green shop state would corrupt era separation. Blue needs Blue-owned shop season/item rows and Blue DbContext sets.
- Copying Green unlock code blindly can write the wrong byte arrays. Blue must use `BlueProtocolBytes` and `UserSaveDataBlue`.
- Treating `item_type` as obvious display order is risky. Green evidence proved a non-obvious head/body ordering; Blue can use the shared AC15 mapping only if tests and protocol shapes support it.
- Leaving Blue `getitemshopinfo.php` and `itempurchase.php` as success stubs would make the client appear happy while no server state changes.
- Reward execution and item purchase need a coherent lock/unlock model so configured shop items start locked and become visible after purchase/reward.

## A7 Pitfalls

- WebUI routes can accidentally normalize unsupported eras to Nijiiro or Green. Blue must be explicit.
- AdminApi readback can accidentally query Green tables if Blue branches are missing.
- Editable settings are higher risk than read-only projections. Prefer read-only or hidden controls until Blue write semantics are proven.
- Customization preview depends on Blue catalogs/assets being available; missing assets should degrade cleanly.

## A8 Pitfalls

- Server tests are necessary but not enough. Full Blue support requires cabinet/RPCS3 smoke evidence.
- Running Host builds against default `Host/bin/Debug/net10.0` can fail when a local server is locking files; use a temp output path.
- Logs for unexpected Blue-only calls must capture enough context without dumping sensitive or huge protobuf payloads.
- A8 should not silently absorb battle issues into normal support. Battle findings should become Track B requirements or explicit non-goals.

## Track B Pitfalls

- Blue battle mode is not Green AI Battle. Reusing `GreenStageModeInterpreter` or Green certified-level logic as truth would be a protocol/design error.
- Required byte widths are currently unknown for battle release flags and NPC/costume/special state. Do not implement these from guesses.
- Default battle state can be unsafe. The client may expect at least one NPC, stage, token, or assignment row.
- Battle playresult may affect normal scores/crowns, or it may not. This needs cabinet/client evidence before updating normal Blue score state.
- Banacoin and Tokkun messages are present or adjacent in proto surface but remain non-goals unless traffic proves a safe response is needed.

## Evidence Tasks Before Battle Runtime Code

- Capture cabinet/RPCS3 logs for battle menu entry and first battle flow.
- Inspect `proto/blue/taiko.proto` battle-related messages against generated `Adapters.GameProtocol.Blue/Wire/Game.cs`.
- Inspect local Blue `config/S10100-1/battle` data layout and identify which files are required for menu entry.
- Use IDA/client evidence to confirm byte widths, default values, and required repeated rows.
- Decide whether battle result persistence updates normal Blue score/crown state.

## Practical Guardrails

- Every Blue phase should include source guards that detect Green protocol constants, Green wire models, or Green shop/battle state in Blue implementation files.
- Every persistence phase should include a migration and tests proving Blue rows do not create or mutate Green rows.
- Every protocol phase should include mapper tests for optional field presence/absence.
- Cabinet smoke evidence should be recorded with exact dates, enabled eras, data paths, and observed endpoint calls.

