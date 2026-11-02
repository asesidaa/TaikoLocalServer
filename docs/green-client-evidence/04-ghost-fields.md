# Ghost Field Evidence

## Scope

Endpoints: `getghostdata.php`, `getghostscore.php`, request-side ghost fields in `playresult.php`.

## Evidence Table

| Field | IDA evidence | Constraint | 0 valid? | Required length/count | Server behavior at audit baseline/current tree | Recommendation |
|---|---|---|---|---|---|---|
| `release_info_flag` | IDA-proven field string `0xF0302D`; length source-inferred. | Source-inferred bitset capacity `<128`. | All-zero valid none. | Source emits `GreenProtocolBytes.GhostReleaseInfoBytes == 16`. | Current tree normalizes output in `Application/Handlers/GetGhostDataQuery.Green.cs:31`; request IDs are set at `UpdatePlayResultCommand.Green.cs:257`. | Reject invalid release IDs before `UpdatePlayResultCommand.Green.cs:257` if skip-on-overflow is not acceptable. |
| `played_song_flag` | IDA-proven field string `0xF03048`; length source-inferred. | Source-inferred song bitset capacity `<1024`. | All-zero valid none. | Source emits `GreenProtocolBytes.GhostPlayedSongBytes == 128`. | Current tree normalizes output in `GetGhostDataQuery.Green.cs:32`; no update path was found. | If required, update this flag during `UpdatePlayResultCommand.Green.cs:59` stage processing or inside `SaveStageAsync`. |
| token IDs and values | IDA-proven container `ary_token_data` at `0xF030F8`; exact token ID/value domains not found. | Not found; treat token IDs as catalog/event IDs when catalog exists. | Not found. | List count not found. | Current tree returns all persisted tokens from `GetGhostDataQuery.Green.cs:11` and upserts request tokens at `UpdatePlayResultCommand.Green.cs:262`. | Validate/cap tokens before `UpdatePlayResultCommand.Green.cs:262`; filter output in `GetGhostDataQuery.Green.cs:11`. |
| rank IDs | IDA-proven field strings `rank_id` at `0xF02538` and `0xF03185`; domain not found. | Not found. | Not found. | Scalar. | Current tree persists `GhostRankId` at `UpdatePlayResultCommand.Green.cs:292` and echoes in `GetGhostDataQuery.Green.cs:41`. | Add rank validator before `UpdatePlayResultCommand.Green.cs:292`; normalize output in `GetGhostDataQuery.Green.cs:39`. |
| certified level IDs | IDA-proven strings `sd_certified_level_id` `0xF02272`, `certified_level_id` `0xF0255E`/`0xF031AB`; domain not found. | Not found. | Not found. | Scalar. | Top-level persists at `UpdatePlayResultCommand.Green.cs:294`; stage value maps in `PlayResultMappers.cs:157` but is ignored in persistence. | Validate top-level before `UpdatePlayResultCommand.Green.cs:294`; decide reject-vs-ignore for stage `SdCertifiedLevelId` near `UpdatePlayResultCommand.Green.cs:175`. |
| winnings level IDs | IDA-proven container `ary_winnings_data` at `0xF0257A` and `0xF031C7`; exact `level_id` domain not found. | Not found. | Not found. | Row count/value max not found. | Current tree sums and upserts all request winnings at `UpdatePlayResultCommand.Green.cs:295` and `:299`; output returns all rows from `GetGhostDataQuery.Green.cs:19`. | Validate/cap `AryWinningsData` before the sum at `UpdatePlayResultCommand.Green.cs:295`; filter output in `GetGhostDataQuery.Green.cs:19`. |
| ghost score sections | IDA-proven container `ary_best_section_data` at `0xF03320`; count/domain not found. | Source-inferred: request `level` should be `0..4`; counts should be chart/section-consistent. | `0` valid for section counters. | Section count cap not found. | Current tree persists all sections at `UpdatePlayResultCommand.Green.cs:178`; response returns all ordered rows in `GetGhostScoreQuery.Green.cs:28`; invalid level maps to `Difficulty.None`. | Add level guard in `GetGhostScoreQuery.Green.cs:14`; add section count/count guards before `UpdatePlayResultCommand.Green.cs:178`. |

## Artifact Files

- `.tools/green-field-evidence/ghost_strings.json`
- `.tools/green-field-evidence/ghost_xrefs.json`
