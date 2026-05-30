# Phase 05 Plan 05-03 - Battle Playresult Rewards And Progression Notes

Purpose: tracked evidence notes for the 05-03 checkpoint. These notes duplicate useful local and public evidence into git-tracked planning files. `.tools` evidence remains local-only and must not be committed.

## Decision Boundary

Rows 18-24 and 26 are resolved as a client-state store/echo boundary, not as server-side battle engine behavior.

05-10 may store and echo client-reported Blue battle state where the request/response proto shape gives a matching field. 05-10 must not calculate rewards, token thresholds, NPC progression, stage graph transitions, boss completion, stage `33` behavior, or normal Blue unlock mirrors from these rows.

## Row 18 - Battle NPC Result Fields

IDA evidence is partial. The Blue client constructs outgoing `PlayResultRequest.StageData.BattleStageData.npc_data` for `playresult.php` from local battle-result state. EXP values are converted into protocol strings, DPN/costume/special IDs are copied, and `bonds_lv` is computed from total EXP before serialization.

Public Wiki/official/news evidence establishes gameplay-level semantics only: Blue battle records bonds level and best battle power, keeps normal crowns/scores separate, and has stages, bosses, specials, rewards, and a final bonds cap of 65.

Resolution: store and echo client-reported typed NPC battle state only. Do not derive server-side NPC progress, reward effects, DPN meaning, costume/special unlock effects, or EXP calculations.

## Rows 19-20 - Release Info And Release Arrays

IDA evidence proves these byte fields are raw fixed-width bitsets, not compressed values:

| Field | Width | Proven behavior |
|---|---:|---|
| `BattleUserDataResponse.release_info_flg` | 16 bytes | Parsed as protobuf bytes; consumer copies exactly 16 bytes and zero-fills missing/short input. |
| `BattleUserDataResponse.release_battle_stage_flg` | 8 bytes | Parsed as protobuf bytes; consumer copies exactly 8 bytes and zero-fills missing/short input. |
| `InitialdatacheckResponse.release_battle_stage_flg` | 8 bytes | Plain copy/zero-fill into client state. |
| `InitialdatacheckResponse.release_battle_special_flg` | 16 bytes | Plain copy/zero-fill into client state. |
| `BattleUserNpcData.npc_costume_flg` | 4 bytes | Parsed as protobuf bytes; consumer copies exactly 4 bytes and zero-fills missing/short input. |
| `BattleUserNpcData.release_special_flg` | 16 bytes | Parsed as protobuf bytes; consumer copies exactly 16 bytes and zero-fills missing/short input. |

IDA evidence also proves `ReleaseBattleData.release_*_id` request fields are repeated `uint32` IDs rather than compressed bitsets. The client emits IDs when local bits transition from clear to set:

| Request field | Resolution |
|---|---|
| `release_info_id` | Newly set IDs for the 16-byte battle `release_info_flg` bitset. |
| `release_battle_stage_id` | Newly set IDs for the 8-byte battle stage bitset. |
| `release_npc_id` | Still unresolved; population/meaning was not proven. |
| `release_npc_costume_id` | Newly set IDs for the 4-byte NPC costume bitset. |
| `release_npc_special_id` | Newly set IDs for the 16-byte NPC special bitset. |

Public resources establish progressive gameplay but not exact bit meanings: stages can be globally added while user playability is gated by prior clears; specials are obtained through magic stones and then usable/selectable; bosses/events grant songs, costumes, titles, or specials.

Resolution: update battle-owned release bitsets from client-reported release info, stage, costume, and special diffs. Keep `release_npc_id` raw-only until it is mapped to `BattleUserNpcData` and local XML. Do not mirror these IDs into normal Blue unlock bits.

## Rows 21 And 24 - Tokens And Reward Types

`ReleaseBattleData.ary_battletokendata` is protocol-owned token state with `token_id` and `token_value`. Local `battletokeninfo.xml` contains token thresholds and reward `type` values `0` and `1`, including a likely special-event token path related to stage `33`.

The 05-03 decision is that the game handles token accumulation and token-threshold unlocks. The server stores whatever token values the game sends and returns them later; the server does not implement reward type logic.

Resolution: store and echo token IDs and values only. Do not grant, spend, threshold-check, or interpret token reward types server-side.

## Row 22 - Next Stage Assignment

`ReleaseBattleData.assign_next_stage_id` maps by proto shape to the next `BattleUserDataResponse.assign_stage_id` value. The 05-03 user checkpoint accepted this as the only plausible mapping.

Resolution: persist the client-reported next assignment and return it as battleuserdata `assign_stage_id`. Do not compute stage graph transitions server-side.

## Row 23 - Stage 33

Local `battlestageinfo.xml` contains stage `33`, and public Wiki evidence makes it a likely Stage EX special event stage. The user approved recording that candidate interpretation while deferring implementation because the event has special tutorial/token behavior.

Resolution: record stage `33` as a likely special-event Stage EX candidate. Do not implement progression/menu/completion routing through it in 05-10.

## Row 26 - Boss Life And Last Stage

The server should store client-reported boss-life, last-stage, and assignment values and return them in battleuserdata. The game handles completion and transition logic.

Resolution: store and echo client-reported values only. Do not compute boss completion, last-stage transitions, or stage graph progression server-side.

## Public Sources Used

- `https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E6%BC%94%E5%A5%8F%E3%83%90%E3%83%88%E3%83%AB`
- `https://taiko-ch.net/blog/?m=201806`
- `https://taiko-ch.net/blog/?m=20180803`
- `https://taiko-ch.net/blog/?p=2699`
- `https://taiko-ch.net/blog/?m=20181022`
- `https://taiko-ch.net/blog/?m=201812`
- `https://taiko-ch.net/blog/?p=3096`
