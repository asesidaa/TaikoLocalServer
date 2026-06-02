# Phase 05 Plan 05-02 - BattleUserData And InitialData IDA Notes

Source IDB: `.tools/blue/EBOOT.ELF.i64`

Purpose: bounded read-only notes for Phase 05 Plan 05-02. These notes record what the delegated IDA agents proved for `initialdatacheck.php`, `battleuserdata.php`, and battle XML menu gating. They do not mutate the IDB and do not approve behavior outside the row-specific statements below.

## Method

- Agents used `ida_cli.agent_bridge.AgentSession` with `require_ida=True`.
- Helpers included `probe_backend`, `ai.decompile`, `ai.function`, `ai.callers`, `ai.xrefs_to`, `ai.disasm`, `ai.strings`, `idautils.Strings`, `idautils.XrefsTo`, `idautils.XrefsFrom`, and Hex-Rays decompilation.
- The IDB was treated as read-only. Agents reported unchanged source IDB timestamp.

## InitialData And Menu Rows 1-6

| Row | Verdict | IDA Evidence | Gate Result |
|---:|---|---|---|
| 1 | partial | `sub_2E3A60` builds `chassis/initialdatacheck.php`; `sub_143720` handles the response; `sub_250F04` is a battle availability candidate. | Still blocked: complete battle menu sequence and required state checks are not proven. |
| 2 | partial | `sub_2DF364` builds `chassis/battleuserdata.php`; `sub_2DF8B0` initializes route state/buffers; `sub_2E0CA8` sends via `sub_9B085C` when route state is ready. | Still blocked: route existence and send plumbing do not prove menu timing. |
| 3 | proven narrowly | `sub_143720` reads optional bool has-bit `0x2000`, stores response byte `+159` to `byte_11DBD40`, and absent field initializes false. | Omission or explicit false is safe; explicit true remains blocked by the broader advertisement gate. |
| 4 | partial | `sub_13BFC8` copies first byte-array arg into `byte_11DA3AD` for exactly 8 bytes and zero-fills short input. | Width is proven; default bits are not. |
| 5 | partial | `sub_13BFC8` copies second byte-array arg into `byte_11DA3B5` for exactly 16 bytes and zero-fills short input. | Width is proven; default bits and NPC-special relation are not. |
| 6 | partial | `sub_13BFC8` stores cap into `dword_11B67FC`; `sub_7497C` and `sub_DE290` consume it as a cap/clamp input. | Client use is proven; no emitted default value is proven. |

## BattleUserData Response Rows 7-16

Response path: `sub_9BAAB4` parses the response through `BattleUserDataResponse` constructor `sub_2F7774`, generic protobuf parse `sub_4C7438`, generated parser `sub_34F3FC`, and consumer `sub_7497C`.

`sub_7497C` takes the success path only when `BattleUserDataResponse.result == 1`.

| Row | Verdict | IDA Evidence | Gate Result |
|---:|---|---|---|
| 7 | proven | `sub_34F3FC`, `sub_2F3440`, and `sub_7497C` parse `release_info_flg`; consumer copies exactly 16 bytes and zero-fills short/omitted data. | 16-byte emission or omitted/short zero-fill is allowed. |
| 8 | proven with constraint | `sub_34F3FC`, `sub_2F3440`, and `sub_7497C` parse/copy `release_battle_stage_flg` as exactly 8 bytes with zero-fill. | Treat as independent battleuserdata field; no initialdata mirroring is proven. |
| 9 | proven | Parser stores `last_battle_stage_id` as uint32; default is 0; consumer uses it when `assign_stage_id` is 0/omitted. | Omitted/default 0 is client-handled. |
| 10 | proven | Parser stores `last_boss_life` as uint32; default is 0; consumer copies it into runtime battle state. | Omitted/default 0 is client-handled; completion behavior remains separate. |
| 11 | proven | Parser stores `last_npc_id` as uint32; default is 0; consumer copies the scalar response field. | Omitted/default 0 is accepted. |
| 12 | proven zero-or-more | Parser/use site iterates supplied `npc_data`; no fixed required row count is enforced. | Zero or more NPC rows are accepted; catalog-complete count is not proven. |
| 13 | proven | Nested parser `sub_34ECE8`, constructor `sub_2F3490`, and `sub_7497C` copy `npc_costume_flg` as exactly 4 bytes with zero-fill. | 4-byte emission or omitted/short zero-fill is allowed per NPC row. |
| 14 | proven | Nested parser `sub_34ECE8`, constructor `sub_2F3490`, and `sub_7497C` copy `release_special_flg` as exactly 16 bytes with zero-fill. | 16-byte emission or omitted/short zero-fill is allowed per NPC row. |
| 15 | proven zero-or-more | `sub_34F3FC`, `sub_34E738`, `sub_2F787C`, and `sub_7497C` parse/iterate supplied token rows; no fixed row count is enforced. | Zero or more token rows are accepted; token semantics remain separate. |
| 16 | proven | `assign_stage_id` default is 0; consumer uses nonzero assign directly, otherwise falls back through last-stage/stage flags and finally stage `1`. | Omitted/default 0 is client-handled; stage graph/completion semantics remain separate. |

### Crash Follow-Up: NPC Row Gate

The 2026-05-31 RPCS3 crash follow-up traced the `battleuserdata.php` success callback past the parser. `sub_7497C` copies nested `BattleUserNpcData.release_special_flg` into a 16-byte local buffer, ANDs it with `byte_11DA3B5` from `initialdatacheck.php` `release_battle_special_flg`, and only adds the NPC row to `BattleUserParameter` when raw bit 120 is set (`byte[15] & 1`). If that bit is clear in either field, the loop exits before incrementing the `BattleUserParameter` NPC count, so a response with one protobuf NPC row becomes a success callback with zero runtime NPC rows.

The direct RPCS3 crash remains a downstream teardown symptom in `nuRequestNud20DrawSkinPs3` (`sub_5997B0`) after the client entered shutdown. The response-level crash condition found in this follow-up is the missing bit 120 in both special flag fields, not the byte width, stage flag width, or `assign_stage_id` parser behavior.

## Battle XML Menu Row 25

| File | IDA Evidence | Proven | Not Proven |
|---|---|---|---|
| `battleadjsetting.xml` | string `0xF491A0`, pointer `0x10E46A8`, `sub_12F44` at `0x1300C`, parser/import path reaches `sub_561B0` at `0x13068` | File is loaded/consumed by battle code. | Menu-entry requirement. |
| `battlenpcinfo.xml` | string `0xF491D8`, pointer `0x10E46AC`, `sub_12F44` at `0x130BC`, parser `sub_725D4` at `0x13118` | File is loaded/consumed by battle code. | Required row count/defaults/menu entry. |
| `battlestageinfo.xml` | string `0xF49208`, pointer `0x10E46B0`, `sub_12F44` at `0x1316C`, parser `sub_72E4C` at `0x131C8` | File is loaded/consumed by battle code. | First stage, stage `33`, menu entry. |
| `battletokeninfo.xml` | string `0xF49240`, pointer `0x10E46B4`, `sub_12F44` at `0x1321C`, parser `sub_72934` at `0x13278` | File is loaded/consumed by battle code. | Reward semantics/menu entry. |
| `battlesupportinfo.xml` | string `0xF69FA8`, pointer `0x10EB364`, `sub_796C24` at `0x796CBC`, import call `sub_1DD038` at `0x796CE4`, caller `sub_F5B28` at `0xF5E34` | File is loaded/consumed by battle code and local state-machine involvement exists. | Menu-entry requirement. |

Row 25 remains blocked for menu-entry classification because `sub_250F04` does not reference the XML strings or XML loader functions, and no direct call path was found from XML loaders to the battle availability helper.

## Remaining IDA Targets

- Trace callers/data references around `0x10B1B28 -> sub_250F04` to identify the menu callback owner.
- Name the `dword_134AF78 + 920 + 0x70/0x71/0x76` state bytes used by `sub_250F04`.
- Trace higher-level callers of `sub_2E0CA8` and route callback table references to prove `battleuserdata.php` timing.
- Inspect battle catalog/runtime construction around `sub_7497C` token/NPC loops if catalog-complete row counts become necessary.

## Crash Follow-Up 2026-06-01: Scene-Entry `flat_map::at(0)` (disassembly-verified)

IDB annotated this session (names applied): `Battle_SceneCtor_mapBat0` (0x3BD84), `OnBattleUserDataResponse` (0x7497C), `Battle_NpcConsume_mapAB` (0xF444C), `Battle_Prep_Update` (0xF5B28), `Battle_Scene_Update` (0xF943C), `Battle_Main_Setup` (0xF7900), `flatmap_playerslot_index` (0x679934, map A operator[]), `flatmap_mapB_insert` (0x79802C), `flatmap_mapB_at` (0x68E30C), `get_g_release_battle_special_flg` (0x13BD64), `get_g_release_battle_stage_flg` (0x13BD5C), `BattleUserData_SendRequest` (0x2DF364), `Battle_RegisterNetHandlers` (0xC49EC); globals `g_release_battle_stage_flg` (0x11DA3AD, 8B), `g_release_battle_special_flg` (0x11DA3B5, 16B), `g_battleUserData_oneshot_cb` (0x11D8768).

**Throw site (disassembly 0x3BD84–0x3BF8C).** `sub_3BD84` is `flat_map::at(0)` over **map B = `bstate+0xCD0` (3280)**, 776-byte entries, key = first dword:
- `0x3BE64 beq loc_3BF94`: lower_bound(0)==end → loc_3BF94 (recovers if found, else throws).
- `0x3BE78 ble loc_3BF94`: smallest key == 0 → loc_3BF94 → **normal** (returns slot-0 entry).
- else smallest key > 0 → falls through → builds the 26-char `"flat_map::at key not found"` string (`li r4,0x1A` @0x3BEB8) → **throw `sub_ABD1D8` @0x3BF8C**.
- `loc_3BF94`: throws only if map B empty (lower_bound==end).
- Net: **scene entry throws unless map B has a key-0 (player-slot-0) entry.**

**Map architecture (player-slot-keyed, NOT npc-id-keyed).**
- map A = `bstate+0x370` (880), map B = `bstate+0xCD0` (3280). Both keyed by **player slot** (0 = local player; 1 added when `bstate+0x3F8` player-mode == 3).
- map A slot entries are created lazily via `flatmap_playerslot_index` = operator[] (insert-if-missing, **no throw**) in the battle-menu scene (`sub_4A304`/`sub_4B028`), gated on `bstate+0x3F8` (mode 1/2/3).
- map B slot entries are created **only** by `Battle_NpcConsume_mapAB` (`sub_F444C` → `flatmap_mapB_insert`), driven by the `Battle_Prep_Update` loop over slots `[a1+24 .. bstate+0x3FC)`; the source npc record is the assembled `BattleUserParameter` from `OnBattleUserDataResponse`.

**Two-response linkage (re-confirmed).** `OnBattleUserDataResponse` AND-masks each npc `release_special_flg` with `g_release_battle_special_flg` (set from `initialdatacheck.php release_battle_special_flg`), and only keeps the npc when raw bit 120 is set (byte15&1). `total_exp` is parsed as a **string** and throws if empty/unparseable.

**Open contradiction / wall.** With the current server response (npc id=1, bit-120 set, `total_exp="0"`, mode=1→slot 0, map A[0] created lazily) every statically-visible gate passes, so map B[0] *should* be created — yet it crashes at scene entry. This means the real trigger is either (a) runtime-state-dependent in the templated boost dispatch (operator[] vs at() ordering), or (b) a subtler gate not visible without proper `bstate` struct typing. Static RE has reached its practical limit here; pinning the exact differing byte needs either IDB struct annotation of `bstate` or a runtime capture-vs-`0xFF` diff.

## Crash Reanalysis 2026-06-01: Screenshot Baseline Supersedes Server Assumptions

The user provided a known-working `BattleUserDataResponse` screenshot and noted that `ReleaseSpecialFlg` must unlock at least one special attack and `LastSelectSpecial1` must select it. Treat the earlier catalog-derived/persisted starter response assumptions as superseded for the immediate crash fix.

Fresh IDA daemon queries and focused subagents found:

- `OnBattleUserDataResponse` copies only fixed slices from byte fields: 16 bytes from top-level `release_info_flg`, 8 bytes from top-level `release_battle_stage_flg`, 4 bytes from nested `npc_costume_flg`, and 16 bytes from nested `release_special_flg`. The 128-byte arrays in the screenshot are tolerated but not consumed by this path beyond those widths.
- Explicit zero optional scalars and omitted defaults behave the same in this consumer for `last_battle_stage_id`, `last_boss_life`, `last_npc_id`, and `assign_stage_id`.
- The risky difference is semantic state, not protobuf length: catalog-derived nonzero NPC/stage/special state drives client runtime setup paths that were not proven safe.
- `sub_7497C` validates selected specials against the effective nested special bitset after ANDing it with initialdata `release_battle_special_flg`. A starter response must therefore select a real unlocked special. The safe baseline is selected special `1` with bit `1` set in both initialdata and nested battleuserdata special masks, while preserving bit `120` for the row-retention branch.
- `NpcId = 0` is not proven mandatory, but it is the safer baseline because the constructor/default path accepts it and downstream AS/UI setup receives the exact NPC id without catalog validation in the inspected path. Emitting catalog NPC id `1` remains unproven.
- Token row `0/0` is baseline parity only; token rows are parsed into a map and no fixed row count was proven.

## Played Session Reanalysis 2026-06-01: Persisted Readback And NPC Index

The follow-up RPCS3 session reached battle playresult and produced live server log evidence in `Host/Logs/log-20260601.txt`. The request at `2026-06-01 13:03:36 +08:00` reported `BattleStageId = 1`, `NpcId = 0`, `TotalExp = 175`, `Dpn = 34`, `SpecialId1/2/3 = 1`, `BondsLv = 2`, `ReleaseInfoIds = [1]`, and `AssignNextStageId = 1`.

SQLite state in `Host/bin/Debug/net10.0/wwwroot/taiko.db3` confirms the write path persisted those raw runtime values: `BlueBattleUserStates` has release info bit 1, `LastBattleStageId = 1`, `LastBossLife = 0`, `LastNpcId = 0`, and `AssignStageId = 1`; `BlueBattleNpcStates` is keyed by `(Baid, NpcId) = (2, 0)` with total exp 175, max DPN 34, selected specials 1/1/1, and bonds level 2.

Warm-daemon IDA follow-up confirms NPC ids are zero-based at runtime:

- `0x725D4` loads `battlenpcinfo.xml` rows into slots `unk_11C8548 + 1592 * n0x1F`, with `n0x1F` starting at 0. The XML row `<id>1</id>` is not the wire/runtime `NpcId`.
- `0x34ECE8` parses protobuf `BattleUserNpcData.npc_id` into the row object without conversion.
- `0x7497C` copies response `npc_data.npc_id` raw into runtime NPC state.
- `0x666CAC` sends raw NPC id through `NpcNewEntry`, `SetNpcExpPow`, and `SetSelectedNpcFromNpcId`.
- `0x78C714` builds/logs battle result NPC data from the raw runtime NPC id that later appears in playresult.

Resulting server rule: use the screenshot shape only as the no-persisted-state starter. After battle playresult, `battleuserdata.php` must read back persisted client-reported Blue battle state. Normalize only `battlenpcinfo.xml` NPC ids from one-based XML/catalog ids to zero-based runtime ids; keep battle stage ids and special ids raw because the played session reported stage 1 and special 1 unchanged.
