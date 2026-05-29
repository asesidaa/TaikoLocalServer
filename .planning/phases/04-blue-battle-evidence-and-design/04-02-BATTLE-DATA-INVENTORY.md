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
