# Phase 04 Plan 04-02: Blue Battle Data Inventory

## Scope And Guardrails

This inventory covers only local Blue battle candidate data under `Host/wwwroot/data/blue/data/config/S10100-1/battle`.

- D-06: XML files in this directory are candidate inputs until IDA/client evidence proves their exact runtime role.
- D-07: The inventory must cover exactly `battleadjsetting.xml`, `battlenpcinfo.xml`, `battlestageinfo.xml`, `battlesupportinfo.xml`, and `battletokeninfo.xml`.
- D-08: Phase 4 documents row counts, key fields, and candidate relationships only.
- D-09: Local Blue battle data is required; missing data blocks execution and fixtures must not be substituted.

No runtime loader, committed battle JSON, EF migration, entity, handler, mapper, controller, source test, generated data, or local game-data file is created by this plan.

## Local Data Availability

All five required XML files are present and were parsed with PowerShell/.NET XML objects using `[xml](Get-Content -Raw -LiteralPath $path)`. The inventory records names, sizes, hashes, parsed counts, and selected key fields only; it does not dump full local XML contents.

| Required file | Present | Parser result | Menu-entry status |
|---|---:|---|---|
| `battleadjsetting.xml` | yes | XML parsed successfully | UNKNOWN for battle menu entry until IDA/client proof |
| `battlenpcinfo.xml` | yes | XML parsed successfully | UNKNOWN for battle menu entry until IDA/client proof |
| `battlestageinfo.xml` | yes | XML parsed successfully | UNKNOWN for battle menu entry until IDA/client proof |
| `battlesupportinfo.xml` | yes | XML parsed successfully | UNKNOWN for battle menu entry until IDA/client proof |
| `battletokeninfo.xml` | yes | XML parsed successfully | UNKNOWN for battle menu entry until IDA/client proof |

## Battle XML Inventory

| File | Size | SHA-256 | Verified counts | Key fields | Candidate role | Menu Entry Classification |
|---|---:|---|---|---|---|---|
| `battleadjsetting.xml` | 3842 | `309ED4FCCBF6A637BABDA290057869EEDBA01A78FEBDE74849CAC6C05F18E8FE` | One `adjustedsetting` object; parsed tuning arrays include `special_active_hit` with 4 outer entries and repeated rate/point arrays. | `fluc_rate_min`, `fluc_rate_max`, `damage_rate_renda`, `damage_rate_critical`, `damage_rate_special`, `critical_rate_*`, `token_point_rate`, `treasurebox_rate`, `special_select_rate`. | Candidate battle adjustment/tuning table. | UNKNOWN for battle menu entry until IDA/client proof. |
| `battlenpcinfo.xml` | 3819 | `05C1D806872D3A11B42638F44C276B6E61C560444E25B46AE78E3B1CC4D0A790` | One `npcinfo` object; 65 `requred_exp` values and 65 `atk` values. | `id`, `start_exp`, `sp_frame_atk_delay`, `sp_frame_atk_end`, `sp_type`, repeated `requred_exp`, repeated `atk`. | Candidate NPC progression/attack table. | UNKNOWN for battle menu entry until IDA/client proof. |
| `battlestageinfo.xml` | 5803 | `8192799768D12E6E28F3847333E1D10C71A26B065296AA130C9492C4110256CC` | 11 `stageinfo` rows; stage ids `1..10` plus `33`; 11 `boss` nodes; 11 `enemy` nodes; 25 `species` values; 62 `lvlife` values. | `id`, `number`, `proper_lv`, `reward_exp`, `prev_id`, `next_id`, `enemy/species`, `enemy/lvlife`, `boss/is_available`, `boss/life`. | Candidate battle stage graph and boss-life table. | UNKNOWN for battle menu entry until IDA/client proof. |
| `battlesupportinfo.xml` | 586190 | `A49D9EA0102AF1EF656AED761E423F37DD972D29180C98FB720426CA207BA354` | 756 `supportinfo` rows; 756 `musicid` values; 756 `coursepatterns` nodes; 4536 `elems` nodes; 15120 `item` nodes. | `musicid`, `coursepatterns`, nested pattern arrays. | Candidate song/course support, bonds, or battle progression relationship data. | UNKNOWN for battle menu entry until IDA/client proof. |
| `battletokeninfo.xml` | 1793 | `5E92A5EDAA1AC4D3C56C0DB1661CA41C831D407B40C84AEDFF22952D6B772AD6` | 2 `tokeninfo` rows; token ids `1` and `17`; 10 `reward` rows; reward `type` values `0` and `1`. | `id`, `available_lv`, `available_releasestageid`, `rewardtbl_size`, `reward/token_val`, `reward/type`, `reward/id`, `reward/uid`. | Candidate battle token unlock/reward table. | UNKNOWN for battle menu entry until IDA/client proof. |

## Candidate Relationships

These relationships are data-shape observations, not runtime role proof:

| Relationship | Local evidence | Candidate interpretation | Safe Phase 5 use |
|---|---|---|---|
| Stage graph | `battlestageinfo.xml` has `prev_id` and `next_id` values over stage ids `1..10` plus `33`. | Candidate battle progression graph with an extra stage id `33`. | Do not use for first-stage, next-stage, or last-stage defaults until client/IDA proof or user approval. |
| Boss life | `battlestageinfo.xml` has `boss/is_available` and `boss/life`; stages `2`, `4`, `6`, `8`, `10`, and `33` have non-zero boss life. | Candidate boss encounter data. | Do not use as `last_boss_life` default without proof. |
| NPC level data | `battlenpcinfo.xml` has 65 `requred_exp` entries and 65 `atk` entries. | Candidate NPC level curve. | Do not infer `npc_data` response row count or `battle_bonds_lv_cap = 65` from this alone. |
| Support rows | `battlesupportinfo.xml` has 756 `supportinfo` rows with `musicid` and nested `coursepatterns`. | Candidate support/bonds/song relationship data. | Do not bind rows to battle menu availability or playresult persistence without proof. |
| Token rows | `battletokeninfo.xml` has token ids `1` and `17`, with reward `type` values `0` and `1`. | Candidate token/reward availability data. | Do not infer `ary_token_data` minimum row count or reward semantics from XML alone. |
| Adjustment settings | `battleadjsetting.xml` contains damage, critical, token point, treasure box, and special selection rate fields. | Candidate battle tuning table. | Do not use for runtime defaults until client/IDA proof confirms consumption path. |

## Default And Width Proof Matrix

D-10 requires explicit proof for every battle byte-array width and default. D-11 requires unproven optional battle protobuf fields to be omitted by default using generated presence semantics. D-12 requires repeated battle rows to have minimum safe row-count evidence. D-13 requires Phase 5 tests for `ShouldSerialize`/omission behavior, exact byte lengths, required repeated-row counts, and source guards preventing Green AI Battle implementation leakage.

| Field or concept | Candidate source | Required/optional wire status | Proven byte width/default/row count | Proof source | Status | Phase 5 impact |
|---|---|---|---|---|---|---|
| InitialdatacheckResponse `release_battle_stage_flg` | Stage ids in `battlestageinfo.xml` | Optional `bytes`; generated `ReleaseBattleStageFlg` has `ShouldSerializeReleaseBattleStageFlg()`. | Byte width is not proven; default is omitted when unset. | `proto/blue/taiko.proto` lines 111-114; `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 433-441; D-11. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must add omission and exact-length tests before setting it. |
| InitialdatacheckResponse `release_battle_special_flg` | No direct XML field; possible relationship to NPC special data. | Optional `bytes`; generated `ReleaseBattleSpecialFlg` has `ShouldSerializeReleaseBattleSpecialFlg()`. | Byte width is not proven; default is omitted when unset. | `proto/blue/taiko.proto` lines 111-114; `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 443-451; D-10/D-11. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must prove width and add `ShouldSerializeReleaseBattleSpecialFlg` tests before setting it. |
| InitialdatacheckResponse `battle_bonds_lv_cap` | `battlenpcinfo.xml` has 65 `requred_exp` and 65 `atk` values. | Optional `uint32`; generated `BattleBondsLvCap` has `ShouldSerializeBattleBondsLvCap()`. | Exact value is not proven; do not infer `65`; default is omitted when unset. | `proto/blue/taiko.proto` lines 111-114; `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 453-461; local XML count. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must prove cap value and add omission/positive presence tests. |
| BattleUserDataResponse `release_info_flg` | No direct XML field. | Optional `bytes`; generated `ReleaseInfoFlg` has `ShouldSerializeReleaseInfoFlg()`. | Byte width is not proven; default is omitted when unset. | `proto/blue/taiko.proto` lines 705-708; `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 3355-3363. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must prove width and protect against zero-filled guesses. |
| BattleUserDataResponse `release_battle_stage_flg` | Stage ids in `battlestageinfo.xml`. | Optional `bytes`; generated `ReleaseBattleStageFlg` has `ShouldSerializeReleaseBattleStageFlg()`. | Byte width is not proven; default is omitted when unset. | `proto/blue/taiko.proto` lines 705-708; `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 3365-3373. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must align this with initialdata stage flag width or prove divergence. |
| BattleUserDataResponse `last_battle_stage_id` | `battlestageinfo.xml` stage ids `1..10` plus `33`. | Optional `uint32`; generated `LastBattleStageId` has `ShouldSerializeLastBattleStageId()`. | Default and last-stage value are not proven; omitted when unset. | `proto/blue/taiko.proto` lines 709-711; `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 3375-3383; local stage rows. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must prove initial, persisted, and last-stage behavior before responding. |
| BattleUserDataResponse `last_boss_life` | `battlestageinfo.xml` boss `life` values including non-zero rows for stages `2`, `4`, `6`, `8`, `10`, and `33`. | Optional `uint32`; generated `LastBossLife` has `ShouldSerializeLastBossLife()`. | Boss-life default is not proven; omitted when unset. | `proto/blue/taiko.proto` lines 709-711; `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 3385-3393; parsed XML boss fields. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must prove default and persistence source before setting `last_boss_life`. |
| BattleUserDataResponse `last_npc_id` | NPC table has one `npcinfo` object, not response NPC identity proof. | Optional `uint32`; generated `LastNpcId` has `ShouldSerializeLastNpcId()`. | Default is not proven; omitted when unset. | `proto/blue/taiko.proto` lines 709-711; `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 3395-3403. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must prove first/last NPC semantics and persistence. |
| BattleUserDataResponse `npc_data` | `battlenpcinfo.xml` has 65 `requred_exp` and 65 `atk` values. | Repeated `BattleUserNpcData`; nested fields are required when a row is emitted. | Minimum safe row count is not proven; XML level values do not prove response rows. | `proto/blue/taiko.proto` lines 713-724; `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 3405-3460; D-12. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must prove row count and add required repeated-row tests. |
| BattleUserDataResponse `npc_costume_flg` | No direct XML byte-width field. | Required `bytes` inside each emitted `BattleUserNpcData`. | Byte width and default are not proven; cannot emit NPC rows safely until proven. | `proto/blue/taiko.proto` lines 715-723; `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 3428-3441; D-10. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must prove width before any `npc_data` row is serialized. |
| BattleUserDataResponse `release_special_flg` | No direct XML byte-width field. | Optional `bytes` inside each emitted `BattleUserNpcData`; generated `ShouldSerializeReleaseSpecialFlg()`. | Byte width is not proven; default is omitted when unset. | `proto/blue/taiko.proto` lines 715-723; `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 3452-3460. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must prove width and add nested presence tests. |
| BattleUserDataResponse `ary_token_data` | `battletokeninfo.xml` token ids `1` and `17`. | Repeated `BattleUserTokenData`; nested `token_id` and `token_value` are required when a row is emitted. | Minimum safe row count is not proven; XML token ids do not prove response rows. | `proto/blue/taiko.proto` lines 726-730; `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 3408-3409 and 3464-3475; D-12. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must prove token row count and add required repeated-row tests. |
| BattleUserDataResponse `assign_stage_id` | `battlestageinfo.xml` graph has first candidate id `1` and `next_id` values. | Optional `uint32`; generated `AssignStageId` has `ShouldSerializeAssignStageId()`. | First-stage assignment is not proven; default is omitted when unset. | `proto/blue/taiko.proto` line 732; `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 3411-3419; parsed stage graph. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must prove first-stage assignment before enabling battleuserdata state. |
| PlayResultRequest `BattleStageData` fields | Cabinet request section, not local XML default data. | Optional `ary_battlestagedata`; if present, `support_lv`, `battle_stage_id`, `npc_data`, `kill_cnt`, `boss_life`, `total_damage`, `critical_cnt`, and `special_move_cnt` are required. | Wire shape is proven; server persistence/default effects are not proven. | `proto/blue/taiko.proto` lines 365-388; `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 2213-2237. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must map battle-owned progress without updating normal Blue scores, crowns, history, or counters. |
| PlayResultRequest `BattleStageData.npc_data` | Cabinet request section. | Required nested message when `BattleStageData` is present; fields include `npc_id`, exp strings, `dpn`, costume, specials, and `bonds_lv`. | Wire shape is proven; persistence semantics are not proven. | `proto/blue/taiko.proto` lines 370-381; generated `BattleNpcData` under `BattleStageData`. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must prove how request NPC progress updates battle-owned state. |
| PlayResultRequest `ReleaseBattleData` fields | Cabinet request section; possible unlock mirror source. | Optional `ary_release_battledata`; repeated release arrays plus required `assign_next_stage_id` when present. | Wire shape is proven; reward/unlock side effects are not proven. | `proto/blue/taiko.proto` lines 456-471; `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 2402-2427. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must design Blue-owned release handling and explicit normal-unlock mirroring only where approved. |
| `ReleaseBattleData.ary_battletokendata` | Cabinet request section; candidate relation to `battletokeninfo.xml`. | Repeated nested `BattleTokenData`; `token_id` and `token_value` required per row. | Row count and token value semantics are not proven. | `proto/blue/taiko.proto` lines 464-468; `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 2423-2430. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must prove request token handling and persistence. |
| `ReleaseBattleData.assign_next_stage_id` | `battlestageinfo.xml` `next_id` graph. | Required `uint32` when `ReleaseBattleData` is present. | Next-stage assignment semantics are not proven. | `proto/blue/taiko.proto` line 470; `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 2426-2427; parsed stage graph. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must prove how this affects `assign_stage_id` and last-stage state. |
| stage id `33` | `battlestageinfo.xml` has a stage row with `id=33`, `number=5`, `prev_id=3`, `next_id=5`, boss available, and boss life `1000000`. | Not a standalone wire field. | Existence is proven; role, menu visibility, and last-stage semantics are not proven. | Structured XML parse of `battlestageinfo.xml`. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must treat id `33` as blocked or user-approved before routing progression through it. |
| `battletokeninfo.xml` reward `type` values `0` and `1` | `battletokeninfo.xml` reward rows. | Not a standalone wire field. | Values `0` and `1` are proven in local XML; meaning is not proven. | Structured XML parse of `battletokeninfo.xml`. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must prove reward type semantics before applying token rewards or unlock mirrors. |
| Minimum safe `npc_data` row count | `battlenpcinfo.xml` has 65 exp/attack values. | Repeated response rows. | Not proven; do not assume empty list, one row, or 65 rows. | D-12 plus structured XML parse. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must add repeated-row count tests from approved evidence. |
| Minimum safe `ary_token_data` row count | `battletokeninfo.xml` has token ids `1` and `17`. | Repeated response rows. | Not proven; do not assume empty list, one row, or two rows. | D-12 plus structured XML parse. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must add repeated-row count tests from approved evidence. |
| First-stage assignment | `battlestageinfo.xml` row id `1` has `prev_id=0` and `next_id=2`. | Candidate value for optional `assign_stage_id`. | Not proven; do not set by default. | Structured XML parse of stage graph; generated `ShouldSerializeAssignStageId()`. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must prove initial assignment before setting battleuserdata response fields. |
| Next-stage assignment | `battlestageinfo.xml` has `next_id` values; `ReleaseBattleData.assign_next_stage_id` is required in request when present. | Candidate update source for optional `assign_stage_id`. | Not proven; do not persist or echo automatically. | Proto/wire request shape and structured XML parse. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must prove request-to-state transition behavior. |
| Boss-life default | `battlestageinfo.xml` has boss life values and zeroes. | Candidate value for optional `last_boss_life`. | Not proven; omit by default. | Parsed boss fields plus generated `ShouldSerializeLastBossLife()`. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must prove default, stage transition, and persistence behavior. |
| Last-stage default | `battlestageinfo.xml` row id `10` has `next_id=11`, while stage id `33` also points to `next_id=5`. | Candidate progression boundary only. | Not proven; no last-stage default may be assumed. | Structured XML parse of stage graph. | UNKNOWN - requires case-by-case user approval before Phase 5 relies on it | Phase 5 must gate completion behavior on client/IDA proof or explicit user approval. |

## Menu Entry Classification

Every file remains `UNKNOWN for battle menu entry until IDA/client proof`.

| File | Required for menu entry? | Optional for menu entry? | Unknown? | Reason |
|---|---:|---:|---:|---|
| `battleadjsetting.xml` | no proof | no proof | yes | Local XML shape is known, but no client proof in this task ties it to entering the battle menu. |
| `battlenpcinfo.xml` | no proof | no proof | yes | NPC rows are parsed candidate data only. |
| `battlestageinfo.xml` | no proof | no proof | yes | Stage rows are parsed candidate data only. |
| `battlesupportinfo.xml` | no proof | no proof | yes | Support rows are parsed candidate data only. |
| `battletokeninfo.xml` | no proof | no proof | yes | Token rows are parsed candidate data only. |

## Unknowns For 04-03 Gate

The 04-03 gate must keep Phase 5 `BLOCKED` or `APPROVED_WITH_USER_EXCEPTIONS` until each unresolved case is proven or explicitly approved one by one.

| Unknown | Why it remains unresolved |
|---|---|
| Battle menu entry file requirements | The local XML proves candidate data availability only, not which files the client requires before showing or entering battle. |
| Stage id `33` semantics | The stage row exists, but its runtime role and last-stage behavior are not proven. |
| Minimum safe `npc_data` rows | The XML has 65 level values, but repeated response row count is not proven. |
| Minimum safe `ary_token_data` rows | The XML has two token ids, but repeated response row count is not proven. |
| First-stage and next-stage defaults | The stage graph has `prev_id`/`next_id`, but assignment defaults are not proven. |
| Boss-life default | The stage data has boss life values, but default `last_boss_life` behavior is not proven. |
| XML-to-runtime field mapping | Candidate relationships are not enough to create loaders or committed runtime data in Phase 4. |
