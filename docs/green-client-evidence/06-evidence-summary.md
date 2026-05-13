# Green Client Evidence Summary

## Evidence Limits

The regenerated IDA artifacts prove endpoint strings and protobuf field-name strings in the Green client. They do not provide decompiled parser contexts for the field strings, so exact value ranges are only IDA-proven where prior manual IDA work already established behavior, such as `disp_taikojuku_dan` and Taikojuku slot indexing in `docs/green-protocol-field-audit.md` and the follow-up decompilation work in `07-disp-taikojuku-dan-no-presence-check.md`.

Most new constraints below are therefore `source-inferred`: they are conservative guards derived from the generated wire shape, fixed byte constants, catalog domains, and current echo/persistence paths.

## Conclusive Constraints

| Area | Constraint | Evidence file | Follow-up guard/test |
|---|---|---|---|
| Taikojuku/userdata Dan slot | `disp_taikojuku_dan` must serialize a value in `1..25` (sentinel `1` for any out-of-range save, including the new-card `0`). Omission is unsafe: the client reads `*(msg+0x31C)` without proto2 presence-gating. | `07-disp-taikojuku-dan-no-presence-check.md` (`sub_19CFE0`, `sub_1016F8`, `sub_24377C`, `sub_7FDFFC` decompilations plus RPCS3 crash trace). | Handler `GetSafeTaikojukuDanSlot` in `UserDataQuery.Green.cs:68` and mapper guard in `UserDataMappers.cs` force `1` for invalid input; keep `GreenUserDataMapperTests.UserData_FallsBackToSentinelOneForInvalidDispTaikojukuDan` and `GreenIdentityHandlerTests.UserData_Green_NewSaveSendsSentinelOneForDispTaikojukuDan`. |
| Fixed bitsets | Song/release flags 128 bytes, tone 16, title 128, costume 32, dan 18, dan-extra 36, ghost release 16, ghost played-song 128. | `02-userdata-baid-fields.md`, `04-ghost-fields.md`, `Application/Common/GreenProtocolBytes.cs:7-15`. | Keep `FixedOrZero`; add semantic validators before writing persisted bitsets. |
| Crown table size | `hash_crown_flg` zlib body is source-packed to 1280 bytes. | `03-score-crown-counter-fields.md`, `Application/Common/GreenProtocolBytes.cs:16`. | Keep inflate-size tests; add catalog membership filtering before crown packing. |
| Shop purchase validation | Purchase must match server catalog `item_no`, `item_type`, `item_id`, nonzero `item_price`, and affordable balance. | `05-shop-reward-catalog-fields.md`. | Current tree already validates in `ItemPurchaseCommand.Green.cs:16-21`; keep rejection tests. |
| Play result stage core domain | Stage song must exist in Green catalog, level `0..4`, play result `0..3`. | `03-score-crown-counter-fields.md`, current source at `UpdatePlayResultCommand.Green.cs:68-73`. | Current tree already guards these; add/keep handler regression tests. |

## Still Not Found

| Area | What was searched | Why still open | Safe interim behavior |
|---|---|---|---|
| `song_hash_ver` `0` semantics | `initialdatacheck.php`, `tournamentcheck.php`, `song_hash_ver`. | Field string present, but no parser/range xref was recovered. | Fail fast on missing/invalid catalog version; omit tournament version until tournament payload is real. |
| `verup_no` and dates | Taikojuku, telop, folder, item shop version/date fields. | Field strings present, but date format and zero/no-update behavior are not proven except prior Taikojuku cache inference. | Omit optional version/date fields unless catalog-backed rows are advertised. |
| `option_flg` | `userdata.php`, `playresult.php`, `option_flg`. | Field string present; length/content not found. | Omit empty/unknown userdata `option_flg`; validate stage option bytes only after length is known. |
| Dan display/max fields | `disp_dan_type`, `got_dan_max`. | Field strings present; enum/range not found. | Normalize unknown nonzero values to `0` until domain is known. |
| Catalog selected IDs | Costume, tone, title/titleplate, Don colors. | Related strings present; exact catalog/palette sets not recovered. | Echo only known defaults or unlocked/catalog-valid values; normalize unknowns. |
| Score/count maxima | `play_score`, hit/count fields. | Field strings present; chart maxima not recovered. | Validate song/course first; add chart-aware max/count checks when chart note data is available. |
| Crown bitset state `3` | `hash_crown_flg`, `play_result`. | No client evidence that Green response crown bitsets consume state `3`. | Continue outputting Dondaful as full combo in `GreenPlayResultMapping.cs:28`. |
| Ghost ranks/tokens/sections | Ghost rank/token/winnings/section strings. | Field strings present; ID domains and count caps not recovered. | Filter/cap before persistence and response once catalog/event data exists; otherwise keep fixed flags only. |
| Shop rows/date format | `getitemshopinfo.php`, item fields, dates. | Item field strings present; shop catalog and date format not recovered. | Do not advertise item shop unless `GreenItemShopLoader` returns real rows; omit empty date strings. |

## Follow-Up Code Location Map

| Priority | Area | Code location | Action |
|---|---|---|---|
| High | `song_hash_ver` | `Infrastructure/GameDataCatalog/Green/GreenMusicInfoLoader.cs:32` and `GreenEraGameDataCatalog.cs:52` | Reject missing/invalid/zero Green music version before responses can serialize `song_hash_ver=0`. |
| High | `option_flg` | `Application/Dtos/CommonUserDataResponse.Green.cs:25`, `Adapters.GameProtocol.Green/Mappers/UserDataMappers.cs:17` | Make userdata `option_flg` nullable/presence-aware and omit empty unknown bytes. |
| High | difficulty played | `Domain/Entities/UserSaveDataGreen.cs:57-58`, `UserDataMappers.cs:38-39`, `UpdatePlayResultCommand.Green.cs:38-45` | Store presence or a known-valid sentinel; do not serialize persisted default `0` as known progress. |
| High | selfbest request | `Application/Handlers/GetSelfBestQuery.Green.cs:9-10` | Reject invalid levels and filter/cap non-catalog song IDs before echoing rows. |
| High | score/count maxima | `Application/Handlers/UpdatePlayResultCommand.Green.cs:68` | Extend `IsValidGreenStage` with chart-aware `play_score` and count consistency checks. |
| Medium | BAID selected/default IDs | `Application/Handlers/BaidQuery.Green.cs:46-66` | Normalize titleplate, default tone, colors, dan enum/max, and selected costumes before mapping. |
| Medium | reward execution | `Application/Handlers/RewardExecutionCommand.Green.cs:12-18` | Replace already-unlocked placeholder with catalog/eligibility validation; implement `release_song_no` handling. |
| Medium | ghost persistence | `Application/Handlers/UpdatePlayResultCommand.Green.cs:257-299` | Validate release IDs, token IDs/values, rank/certified IDs, and winnings rows before persistence. |
| Medium | ghost score | `Application/Handlers/GetGhostScoreQuery.Green.cs:14`, `UpdatePlayResultCommand.Green.cs:178` | Guard request level and section count/count fields. |
| Medium | item shop response | `Infrastructure/GameDataCatalog/Green/GreenItemShopLoader.cs:10`, `Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs:13-17` | Implement real shop rows before advertising; keep optional season/date fields omitted until date format is known. |
| Low | tournament/telop/folder versions | `TournamentCheckQuery.Green.cs`, `GetTelopController.cs`, `GetFolderQuery.Green.cs` | Continue omitting version/date fields until those catalogs are implemented. |

## IDA Metadata Changes

| Address | Change | Reason |
|---|---|---|
| Not changed in this pass | No IDA metadata was saved during this evidence pass. | The generated artifacts were sufficient for schema-string evidence; no binary patching or new metadata persistence was needed. |
