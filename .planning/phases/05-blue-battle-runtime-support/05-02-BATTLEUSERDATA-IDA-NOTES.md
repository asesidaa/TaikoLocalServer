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
