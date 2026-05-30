# Phase 05 - Blue Battle Runtime Resolution Gate

## Purpose

This is the Phase 5 row-level gate for Blue battle runtime work. Runtime plans may only rely on a row when that row has exact proof or a named user approval with the exact behavior being used.

Source gate: `.planning/phases/04-blue-battle-evidence-and-design/04-03-BLUE-BATTLE-DESIGN-GATE.md`.

## Resolution Matrix

| # | Phase 4 Missing-Evidence Row | Phase 5 Status | Evidence Source | Runtime Use | Next Gate |
|---:|---|---|---|---|---|
| 1 | Battle menu entry sequence and required `initialdatacheck.php` fields | STILL_MISSING_LOG_OR_CABINET_EVIDENCE | IDA proves `is_battleplay` consumption, but complete menu-entry sequence and required state checks remain unproven. | blocked | Capture cabinet/RPCS3 logs or client proof before advertising battle. |
| 2 | `battleuserdata.php` call timing and requirement | STILL_MISSING_IDA_EVIDENCE | IDA proves route setup and request checking, but not caller timing from menu entry. | blocked | Record caller sequence from IDA or cabinet/RPCS3 logs before replacing the stub based on timing assumptions. |
| 3 | `InitialdatacheckResponse.is_battleplay` | PROVEN | IDA shows optional bool read defaults absent to false and is consumed by the battle availability helper. | allowed: omission or explicit false only; true remains blocked by D-07 | Keep false or omitted until rows 1, 2, and required defaults clear. |
| 4 | `InitialdatacheckResponse.release_battle_stage_flg` | STILL_MISSING_IDA_EVIDENCE | IDA proves an 8-byte copy/zero-fill path, but default bits are not proven. | blocked | Require default-bit proof or named approval before emitting values. |
| 5 | `InitialdatacheckResponse.release_battle_special_flg` | STILL_MISSING_IDA_EVIDENCE | IDA proves a 16-byte copy/zero-fill path, but relation to NPC special flags is unresolved. | blocked | Require relation/default proof or named approval before emitting values. |
| 6 | `InitialdatacheckResponse.battle_bonds_lv_cap` | NEEDS_USER_APPROVAL | IDA proves client stores the supplied cap; Wiki-backed gameplay context suggests cap 65 but does not prove wire default. | blocked | Either omit or record explicit user approval for an exact emitted cap. |
| 7 | `BattleUserDataResponse.release_info_flg` | STILL_MISSING_IDA_EVIDENCE | No response-field consumption path for battleuserdata was found in the current research. | blocked | Find parser/use-site proof or named approval before emitting. |
| 8 | `BattleUserDataResponse.release_battle_stage_flg` | STILL_MISSING_IDA_EVIDENCE | Initialdata stage flag width is partially proven; battleuserdata field width/default and relation are not. | blocked | Prove battleuserdata-specific width/default and relation before emitting. |
| 9 | `BattleUserDataResponse.last_battle_stage_id` | STILL_MISSING_IDA_EVIDENCE | Proto/wire shape exists, but new-user default and last-stage semantics remain unresolved. | blocked | Persist only client-reported or approved values. |
| 10 | `BattleUserDataResponse.last_boss_life` | STILL_MISSING_IDA_EVIDENCE | Proto/wire shape exists, but boss-life default and persistence source remain unresolved. | blocked | Require default/progression proof before initializing or mutating. |
| 11 | `BattleUserDataResponse.last_npc_id` | STILL_MISSING_IDA_EVIDENCE | IDA found selected NPC write helpers, but not battleuserdata default/readback semantics. | blocked | Persist selected NPC only when client reports it or an approved default exists. |
| 12 | `BattleUserDataResponse.npc_data` | STILL_MISSING_IDA_EVIDENCE | Local XML counts are shape evidence only; required response row count is not proven. | blocked | Require row-count proof or approval before emitting NPC rows as defaults. |
| 13 | `BattleUserNpcData.npc_costume_flg` | STILL_MISSING_IDA_EVIDENCE | Proto/wire field exists; exact width/default is not proven. | blocked | Require width/default proof before serializing NPC costume flags. |
| 14 | `BattleUserNpcData.release_special_flg` | STILL_MISSING_IDA_EVIDENCE | Initialdata special flag width is partially proven; nested NPC flag width/default is not. | blocked | Treat this as a separate proof row before emitting. |
| 15 | `BattleUserDataResponse.ary_token_data` | STILL_MISSING_IDA_EVIDENCE | Local token rows exist, but required response row count is not proven. | blocked | Require row-count proof or approval before emitting token rows. |
| 16 | `BattleUserDataResponse.assign_stage_id` | STILL_MISSING_IDA_EVIDENCE | XML graph and XML consumption are proven, but starting assignment semantics are not. | blocked | Do not derive first stage from XML without proof or approval. |
| 17 | `PlayResultRequest.StageData.BattleStageData` | PROVEN | D-10 defines battle branch trigger from `AryBattlestagedata` or `AryReleaseBattledata`; D-12 requires normal-state bypass. | allowed: branch classification and normal-state protection only | Runtime persistence may branch away from normal Blue writes but must keep progression effects gated. |
| 18 | `BattleStageData.npc_data` result fields | DEFER_RUNTIME_USE | Proto/wire shape exists; effect semantics for NPC progress fields remain unproven. | blocked | Raw capture may be modeled only where later plans keep derived effects disabled. |
| 19 | `ReleaseBattleData.release_info_id` | DEFER_RUNTIME_USE | D-18 treats `ReleaseBattleData` as battle-owned unless a specific mirror is proven or approved. | blocked | Do not mirror to normal Blue unlock handling without row-specific clearance. |
| 20 | `ReleaseBattleData` stage/NPC/costume/special release arrays | DEFER_RUNTIME_USE | D-18 keeps these release paths battle-owned unless each path is proven or approved. | blocked | Record one decision per release path before applying unlock effects. |
| 21 | `ReleaseBattleData.ary_battletokendata` | STILL_MISSING_IDA_EVIDENCE | Proto/wire shape exists; token semantics and values remain unresolved. | blocked | Do not grant, spend, or persist token effects without proof. |
| 22 | `ReleaseBattleData.assign_next_stage_id` | STILL_MISSING_LOG_OR_CABINET_EVIDENCE | Proto/wire shape exists; transition behavior has no IDA/log proof. | blocked | Record raw value only if later plans keep assignment mutation disabled. |
| 23 | `battlestageinfo.xml` stage id `33` | STILL_MISSING_IDA_EVIDENCE | IDA has a stage `33` packed asset string and local XML has stage `33`, but progression/menu role is not proven. | blocked | Do not route progression through stage `33` without proof or approval. |
| 24 | `battletokeninfo.xml` reward `type` values `0` and `1` | STILL_MISSING_IDA_EVIDENCE | Local token XML contains reward types `0` and `1`; semantic meaning is not proven. | blocked | Do not implement reward type effects without proof or approval. |
| 25 | Battle XML file menu-entry requirements | STILL_MISSING_LOG_OR_CABINET_EVIDENCE | IDA proves all five XML files load through battle code paths, but not which files gate menu entry. | blocked | Loaders may treat files as local inputs, but menu-entry requirement claims need proof. |
| 26 | Boss-life and last-stage completion behavior | STILL_MISSING_LOG_OR_CABINET_EVIDENCE | Proto/wire and XML shapes exist; completion transition behavior remains unproven. | blocked | Defer boss-life and last-stage mutation until proof or approval. |

## Runtime Rule

Rows with `STILL_MISSING_*`, `NEEDS_USER_APPROVAL`, or `DEFER_RUNTIME_USE` must not be used to emit battle protocol values, advertise battle availability, derive rewards, mutate progression, or mirror normal Blue unlocks.

`PROVEN` rows are narrow. Row 3 permits only safe omission or explicit false for `is_battleplay` until the broader advertisement gate clears. Row 17 permits battle request classification and normal-state bypass, not progression or reward effects.

## Approval Sources

No row has a named user approval recorded in this artifact.
