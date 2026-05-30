# Phase 05 - Blue Battle Runtime Resolution Gate

## Purpose

This is the Phase 5 row-level gate for Blue battle runtime work. Runtime plans may only rely on a row when that row has exact proof or a named user approval with the exact behavior being used.

Source gate: `.planning/phases/04-blue-battle-evidence-and-design/04-03-BLUE-BATTLE-DESIGN-GATE.md`.

Phase 5 IDA note: `.planning/phases/05-blue-battle-runtime-support/05-02-BATTLEUSERDATA-IDA-NOTES.md`.

## Resolution Matrix

| # | Phase 4 Missing-Evidence Row | Phase 5 Status | Evidence Source | Runtime Use | Next Gate |
|---:|---|---|---|---|---|
| 1 | Battle menu entry sequence and required `initialdatacheck.php` fields | STILL_MISSING_MENU_SEQUENCE_EVIDENCE | IDA proves the route (`sub_2E3A60`), response handling (`sub_143720`), and battle availability candidate (`sub_250F04`), but not complete menu-entry sequence or all required state checks. | blocked | Trace callers of `sub_250F04` and its state bytes, or capture cabinet/RPCS3 logs, before advertising battle. |
| 2 | `battleuserdata.php` call timing and requirement | STILL_MISSING_MENU_TIMING_EVIDENCE | IDA proves route setup/send plumbing (`sub_2DF364`, `sub_2DF8B0`, `sub_2E0CA8`), but not when menu entry calls it or whether display requires it. | blocked | Trace higher-level callers of `sub_2E0CA8`/route callbacks or capture cabinet/RPCS3 logs before replacing the stub based on timing. |
| 3 | `InitialdatacheckResponse.is_battleplay` | PROVEN_OMIT_OR_FALSE_ONLY | `sub_143720` reads optional bool has-bit `0x2000`, stores response byte `+159` to `byte_11DBD40`, and absent field initializes false. | allowed: omission or explicit false only; true remains blocked by D-07 | Keep false or omitted until rows 1, 2, and required defaults clear. |
| 4 | `InitialdatacheckResponse.release_battle_stage_flg` | STILL_MISSING_DEFAULT_BITS | `sub_13BFC8` copies the field into `byte_11DA3AD` for exactly 8 bytes and zero-fills short input, but safe default bits are not proven. | blocked | Require default-bit proof or named approval before emitting values. |
| 5 | `InitialdatacheckResponse.release_battle_special_flg` | STILL_MISSING_DEFAULT_RELATION_EVIDENCE | `sub_13BFC8` copies the field into `byte_11DA3B5` for exactly 16 bytes and zero-fills short input, but default bits and relation to NPC special flags are not proven. | blocked | Require relation/default proof or named approval before emitting values. |
| 6 | `InitialdatacheckResponse.battle_bonds_lv_cap` | NEEDS_USER_APPROVAL | `sub_13BFC8` stores the supplied cap to `dword_11B67FC`; `sub_7497C`/`sub_DE290` consume it as a cap/clamp input, but no safe default value is proven. | blocked | Either omit or record explicit user approval for an exact emitted cap. |
| 7 | `BattleUserDataResponse.release_info_flg` | PROVEN_16_BYTE_ZERO_FILL | Response parse path `sub_9BAAB4` -> `sub_2F7774` -> `sub_4C7438` -> `sub_34F3FC`; consumer `sub_7497C` copies exactly 16 bytes and zero-fills short/omitted data. | allowed: 16-byte emission or omitted/short zero-fill | Emit only 16 bytes when sending explicit state; extra bytes are not consumed by the proven use site. |
| 8 | `BattleUserDataResponse.release_battle_stage_flg` | PROVEN_INDEPENDENT_8_BYTE_ZERO_FILL | `sub_34F3FC` parses bytes; `sub_7497C` copies exactly 8 bytes and zero-fills short/omitted data. Relation/mirroring to initialdata is not proven. | allowed: independent 8-byte emission or omitted/short zero-fill; no initialdata mirroring | Treat battleuserdata stage flags as independent unless later proof establishes an initialdata relationship. |
| 9 | `BattleUserDataResponse.last_battle_stage_id` | PROVEN_DEFAULT_ZERO | `sub_34F3FC` parses uint32; constructor/default path initializes 0; `sub_7497C` uses it only when `assign_stage_id` is 0/omitted. | allowed: omitted/default 0 or persisted response value | Do not derive last-stage completion semantics from this row. |
| 10 | `BattleUserDataResponse.last_boss_life` | PROVEN_DEFAULT_ZERO | `sub_34F3FC` parses uint32; constructor/default path initializes 0; `sub_7497C` copies it into runtime battle state. | allowed: omitted/default 0 or persisted response value | Boss-life progression/completion effects remain row 26-gated. |
| 11 | `BattleUserDataResponse.last_npc_id` | PROVEN_DEFAULT_ZERO | `sub_34F3FC` parses uint32; constructor/default path initializes 0; `sub_7497C` copies the scalar response field without deriving it from NPC rows. | allowed: omitted/default 0 or persisted response value | Do not derive NPC identity defaults from XML rows. |
| 12 | `BattleUserDataResponse.npc_data` | PROVEN_ZERO_OR_MORE_ROWS_NO_CATALOG_COUNT | `sub_34F3FC` parses repeated rows and `sub_7497C` iterates supplied rows; no fixed required row count is enforced in the proven path. | allowed: zero or more supplied rows | Do not claim catalog-complete NPC row counts without more proof. |
| 13 | `BattleUserNpcData.npc_costume_flg` | PROVEN_4_BYTE_ZERO_FILL | Nested parser `sub_34ECE8`, constructor/default path `sub_2F3490`, and consumer `sub_7497C` copy exactly 4 bytes and zero-fill short/omitted data. | allowed: 4-byte emission per NPC row or omitted/short zero-fill | Do not size from XML max costume IDs. |
| 14 | `BattleUserNpcData.release_special_flg` | PROVEN_16_BYTE_ZERO_FILL | Nested parser `sub_34ECE8`, constructor/default path `sub_2F3490`, and consumer `sub_7497C` copy exactly 16 bytes and zero-fill short/omitted data. | allowed: 16-byte emission per NPC row or omitted/short zero-fill | Treat this separately from initialdata special flags. |
| 15 | `BattleUserDataResponse.ary_token_data` | PROVEN_ZERO_OR_MORE_ROWS_NO_CATALOG_COUNT | `sub_34F3FC`/`sub_34E738` parse token rows and `sub_7497C` iterates supplied rows; no fixed required row count is enforced in the proven path. | allowed: zero or more supplied rows | Token reward semantics remain row 21-gated. |
| 16 | `BattleUserDataResponse.assign_stage_id` | PROVEN_DEFAULT_ZERO_STAGE_FALLBACK | `sub_34F3FC` parses uint32; constructor/default path initializes 0; `sub_7497C` uses nonzero assign directly and falls back through last-stage/stage flags to stage `1` when omitted/0. | allowed: omitted/default 0 or explicit persisted assignment | Do not infer stage graph, stage `33`, or completion behavior from this fallback. |
| 17 | `PlayResultRequest.StageData.BattleStageData` | PROVEN | D-10 defines battle branch trigger from `AryBattlestagedata` or `AryReleaseBattledata`; D-12 requires normal-state bypass. | allowed: branch classification and normal-state protection only | Runtime persistence may branch away from normal Blue writes but must keep progression effects gated. |
| 18 | `BattleStageData.npc_data` result fields | DEFER_RUNTIME_USE | Proto/wire shape exists; effect semantics for NPC progress fields remain unproven. | blocked | Raw capture may be modeled only where later plans keep derived effects disabled. |
| 19 | `ReleaseBattleData.release_info_id` | DEFER_RUNTIME_USE | D-18 treats `ReleaseBattleData` as battle-owned unless a specific mirror is proven or approved. | blocked | Do not mirror to normal Blue unlock handling without row-specific clearance. |
| 20 | `ReleaseBattleData` stage/NPC/costume/special release arrays | DEFER_RUNTIME_USE | D-18 keeps these release paths battle-owned unless each path is proven or approved. | blocked | Record one decision per release path before applying unlock effects. |
| 21 | `ReleaseBattleData.ary_battletokendata` | STILL_MISSING_IDA_EVIDENCE | Proto/wire shape exists; token semantics and values remain unresolved. | blocked | Do not grant, spend, or persist token effects without proof. |
| 22 | `ReleaseBattleData.assign_next_stage_id` | STILL_MISSING_LOG_OR_CABINET_EVIDENCE | Proto/wire shape exists; transition behavior has no IDA/log proof. | blocked | Record raw value only if later plans keep assignment mutation disabled. |
| 23 | `battlestageinfo.xml` stage id `33` | STILL_MISSING_IDA_EVIDENCE | IDA has a stage `33` packed asset string and local XML has stage `33`, but progression/menu role is not proven. | blocked | Do not route progression through stage `33` without proof or approval. |
| 24 | `battletokeninfo.xml` reward `type` values `0` and `1` | STILL_MISSING_IDA_EVIDENCE | Local token XML contains reward types `0` and `1`; semantic meaning is not proven. | blocked | Do not implement reward type effects without proof or approval. |
| 25 | Battle XML file menu-entry requirements | STILL_MISSING_MENU_REQUIREMENT_EVIDENCE | IDA proves all five XML files are loaded/parsed by battle code paths (`sub_12F44`, `sub_796C24`), but not which files gate menu entry or optional fallback behavior. | blocked | Loaders may treat files as local inputs, but menu-entry requirement claims need proof. |
| 26 | Boss-life and last-stage completion behavior | STILL_MISSING_LOG_OR_CABINET_EVIDENCE | Proto/wire and XML shapes exist; completion transition behavior remains unproven. | blocked | Defer boss-life and last-stage mutation until proof or approval. |

## Runtime Rule

Rows with `STILL_MISSING_*`, `NEEDS_USER_APPROVAL`, or `DEFER_RUNTIME_USE` must not be used to emit battle protocol values, advertise battle availability, derive rewards, mutate progression, or mirror normal Blue unlocks.

`PROVEN` rows are narrow. Row 3 permits only safe omission or explicit false for `is_battleplay` until the broader advertisement gate clears. Rows 7-16 permit only the exact battleuserdata response parser/default behavior described above; they do not approve menu timing, initialdata mirroring, token rewards, stage graph behavior, stage `33`, boss-life completion, or normal unlock mirrors. Row 17 permits battle request classification and normal-state bypass, not progression or reward effects.

## Approval Sources

No row has a named user approval recorded in this artifact.
