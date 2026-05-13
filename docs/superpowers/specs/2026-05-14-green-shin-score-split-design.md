# Green Shin Score Split Design

## Context

Green play-result uploads now decode successfully, but the captured May 14, 2026
session is rejected after mapping. The payload contains two stages for song `729`:
one normal-looking stage with `stage_mode = 0`, and one Shin-looking stage with
`stage_mode = 1`. Other candidate markers are not useful: both stages have the
same `option_flg` bytes, and the remaining differences are expected result data
such as course level, score, counts, star level, and hit count.

The current Green handler rejects the payload before saving because
`HasOnlyKnownUnlockRewards` requires reported rewards to already be unlocked.
The real client can report newly earned reward IDs in `get_tone_no`,
`get_costume_no_1`, and `get_title_no`; those should be validated as in-range
bitset IDs and then saved.

AC15 Green has normal score and Shin score concepts, and Green `selfbest.php`
already has separate response arrays:
`ary_selfbest_score` and `ary_shin_selfbest_score`. The implementation should
therefore persist Green normal and Shin best scores separately.

External reference used for gameplay semantics:
https://wikiwiki.jp/taiko-fumen/%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0/AC%E3%81%AE%E6%AD%B4%E5%8F%B2/AC15

## Goals

- Accept the captured mixed normal/Shin Green play-result upload.
- Treat Green `stage_mode = 0` as normal scoring and `stage_mode = 1` as Shin
  scoring.
- Save normal and Shin Green best scores independently.
- Return normal bests in `ary_selfbest_score` and Shin bests in
  `ary_shin_selfbest_score`.
- Save decoded Green play-result fields that already have domain storage but
  are currently dropped by the mapper.
- Preserve existing conservative validation for unsafe IDs and unknown modes.

## Non-Goals

- Do not infer Shin mode from score values, chart paths, or `option_flg`.
- Do not add chart-aware maximum score/count validation in this change.
- Do not change Green crown packing to include Shin scores. Crowns stay based on
  normal best rows until client evidence proves a separate Shin crown source.
- Do not add broad storage for every unknown or currently unused wire field.

## Approach

Use `stage_mode` as the Green score-mode discriminator:

- `0`: normal score mode.
- `1`: Shin score mode.
- any other value: reject the play-result payload.

This is the recommended approach because it is the only observed field that
matches the normal/Shin distinction in the captured session. It also aligns with
the existing Green selfbest wire shape, which already splits normal and Shin
rows.

## Data Model

Add `IsShin` to:

- `SongPlayDatumGreen`
- `SongBestDatumGreen`

Change the Green best-score primary key from:

```text
(Baid, SongId, Difficulty)
```

to:

```text
(Baid, SongId, Difficulty, IsShin)
```

The migration defaults existing rows to `IsShin = false`, so current saved Green
bests remain normal-score bests.

`SongPlayDatumGreen.StageMode` should keep the raw stage mode value for audit
and debugging. `SongPlayDatumGreen.IsShin` stores the interpreted mode used for
querying and score splitting.

## Play-Result Mapping

Update `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs` so Green
stage mapping preserves:

- `StageMode`
- `IsPapamama`

`CommonPlayResultData.StageData` already has those properties in the shared
partial DTO files. The current Green mapper drops both fields, causing the
common dump to show `StageMode = 0` for both stages even when the raw Green
payload has `StageMode = 1`.

## Play-Result Validation

Keep the existing Green stage guards for:

- valid Green catalog song ID
- song ID within the 1024-bit Green crown/song table range
- course level `0..4`
- play result `0..3`
- `play_dan` absent or `1..25`

Add a Green stage-mode guard:

- accept only `0` and `1`
- reject any other value

Replace the reward guard with range validation:

- tone reward IDs must fit `GreenProtocolBytes.ToneFlagBytes`
- costume reward IDs must fit `GreenProtocolBytes.CostumeFlagBytes`
- title reward IDs must fit `GreenProtocolBytes.TitleFlagBytes`

Valid newly awarded rewards are then applied with `SetBits`. The handler should
not require newly awarded reward IDs to already be present in the saved bitsets.

## Save Flow

For each valid Green stage:

1. Map `Difficulty` and `Crown` as today.
2. Interpret `IsShin = stage.StageMode == 1`.
3. Insert `SongPlayDatumGreen` with both `StageMode` and `IsShin`.
4. Preserve existing saved fields:
   - score
   - score rate, currently `0` because Green has no explicit wire field
   - good/ok/miss/pound/combo/hit counts
   - crown
   - star/support level
   - option/tone bytes
   - play mode
   - music category and selected folder
   - favorite/recent flags
   - soul gauge
   - play dan
   - Waiwai result/gauge
   - play time
   - `IsPapamama`
5. Upsert best score into the normal or Shin best bucket using
   `(Baid, SongId, Difficulty, IsShin)`.
6. Keep favorite and recent updates independent of Shin mode.

Top-level save behavior remains:

- use optional-presence flags for current costume and difficulty played fields
- preserve tutorial and boolean fields when omitted
- add valid medal totals with overflow checks
- save valid newly awarded reward IDs into bitsets
- keep `default_shin_setting` as the user default setting, not as the per-stage
  result marker

## Selfbest Read Flow

For Green `selfbest.php`:

1. Load best rows for requested songs and requested difficulty.
2. Build `ary_selfbest_score` from rows where `IsShin = false`.
3. Build `ary_shin_selfbest_score` from rows where `IsShin = true`.
4. Preserve requested song order in both arrays.
5. Emit zero rows for requested songs with no saved normal or Shin best.
6. Leave `ura_best_score` and `ura_best_score_rate` at zero until separate Ura
   mapping is proven.

## Crowns

`crownsdata.php` continues to read only normal best rows. Shin best rows should
not affect normal crown flags because no client evidence currently proves a
separate Green Shin crown representation.

## Error Handling

Invalid Green play-result payloads continue to return `Result = 0` and avoid
partial persistence. The handler should log enough context to distinguish:

- invalid stage mode
- invalid song/course/play result
- invalid reward ID range
- medal overflow
- invalid equipped current costume

The controller-level BAID mismatch and payload decode errors keep their current
`Result = 0` behavior.

## Tests

Add or update tests for:

- Green mapper preserves raw `StageMode`.
- Green mapper preserves `IsPapamama`.
- A mixed normal/Shin play-result saves without rejection.
- Normal and Shin stages save into separate best buckets.
- `selfbest.php` returns normal saved scores in `ary_selfbest_score`.
- `selfbest.php` returns Shin saved scores in `ary_shin_selfbest_score`.
- Existing zero-row behavior remains for missing normal or Shin bests.
- Valid newly awarded tone, costume, and title IDs are accepted and set in
  saved bitsets.
- Out-of-range reward IDs are rejected.
- Unknown `StageMode` values are rejected.
- Existing normal best rows migrate as `IsShin = false`.

## Compatibility

Existing Green best-score data remains readable as normal scoring data after the
migration. Existing Green selfbest response shape is preserved; only the Shin
array changes from always-zero rows to real saved Shin rows when data exists.

## Open Evidence Notes

- The captured payload identifies `stage_mode = 1` as the only observed
  Shin-related marker. If later cabinet captures prove additional values or a
  different marker, `GreenShinMode` interpretation can be changed without
  altering the stored `StageMode` audit field.
- Green `score_rate` is still not present in the wire type, so stored rate stays
  `0`.
- Green Shin crown representation is unproven, so crown readback intentionally
  remains normal-only.
