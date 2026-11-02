# Green Dani Flow Design

Date: 2026-05-15

## Context

The latest Green log at `Host/Logs/log-20260515.txt` contains a first-Dan Dani Dojo play. The relevant playresult upload at `06:09` has:

- `PlayMode = 1`
- three stage rows with `PlayDan = 1`
- stage songs `790`, `791`, and `135`
- `DanResult = 2`
- stage `PlayResult = 0`, which means normal song crown data is not enough to identify Dan clear state

The current Green implementation saves the stages as normal `SongPlayDataGreen` and `SongBestDataGreen` rows. It does not save Green Dan achievement data, because `CommonPlayResultData.DanId` remains `0` and `GetDanScoreQuery.Green` is an empty stub.

Green also differs from Nijiiro in how Dan ids are represented. In `Host/wwwroot/data/green/datatable/musicmedleyinfo.xml`, `uniqueid` is the medley catalog id (`20001` and up), while `challengelv` is the playable Dan slot/id used by the client. Normal Dans are `challengelv` `1..25`; extra Dans continue at `101..128` in the current datatable.

## Goals

- Persist Green Dani play and clear results separately from normal song bests.
- Preserve the existing normal selfbest behavior, because Green asks for Dani song scores through normal `selfbest.php` song lists.
- Support normal Dan slots and extra Dan slots from `musicmedleyinfo.xml`.
- Update BAID/userdata Dan display fields so cleared Dans show in the Dan selection menu.
- Keep `disp_taikojuku_dan` wire-safe and default it to the next uncleared normal Dan.

## Non-Goals

- Do not reuse Nijiiro Dan tables or `DanClearState` for Green.
- Do not recompute Green normal/gold clear from `Conditions` or `ExcellentConditions`; the client already sends `DanResult`.
- Do not stop saving Dani stages as normal Green song plays and bests.
- Do not make extra Dans the `disp_taikojuku_dan` value, because that field must stay in `1..25`.

## Data Model

Add Green-specific Dan entities:

- `DanScoreDatumGreen`
  - `Baid`
  - `DanId`: Green playable slot/id from `challengelv`, not medley `uniqueid`
  - `IsExtra`: true for extra Dan ids, currently `101..128`
  - `MedleyUniqueId`: optional catalog trace id from `uniqueid`
  - `ClearGrade`: Green-specific clear grade, not Nijiiro `DanClearState`
  - `ArrivalSongCount`
  - `SoulGaugeTotal`
  - `ComboCountTotal`
  - child collection of Green Dan stage score rows

- `DanStageScoreDatumGreen`
  - `Baid`
  - `DanId`
  - `IsExtra`
  - `StageIndex`
  - `SongNumber`
  - `HighScore`
  - `PlayScore`
  - `GoodCount`
  - `OkCount`
  - `BadCount`
  - `DrumrollCount`
  - `TotalHitCount`
  - `ComboCount`

Use `(Baid, DanId, IsExtra)` as the parent key. Use `(Baid, DanId, IsExtra, StageIndex)` for child stage rows so duplicate songs in a medley cannot collapse into one row.

Add a Green-specific clear grade enum or equivalent small value type:

- `0 = NotClear`
- `1 = NormalClear`
- `2 = GoldClear`

`DanResult` values outside `0..2` are invalid for Green and should reject the Dani update portion. The rest of the playresult should not silently create a higher unknown grade.

## Catalog Mapping

`GreenTaikojukuLoader` should continue reading `musicmedleyinfo.xml`, but downstream code should treat fields as follows:

- `UniqueId`: medley catalog id, such as `20001`
- `ChallengeLevel`: playable Dan id/slot, such as `1` for first Dan or `101` for the first extra Dan
- normal Dan: `ChallengeLevel` in `1..25`
- extra Dan: `ChallengeLevel >= 101`; the current datatable contains `101..128`

`Taikojuku` responses may continue to protect crash-sensitive normal Dan flows, but the persistence and read paths must accept both normal and extra catalog entries.

## Playresult Save Flow

Detect Green Dani play when `PlayMode == 1` and at least one stage has nonzero `PlayDan`.

The Dan id is the common nonzero `PlayDan` value across stage rows. Validate it against the Green Taikojuku catalog by `ChallengeLevel`. If stages disagree on `PlayDan`, reject the Dani update. If the id is not found in the catalog, reject the Dani update.

For every playresult, keep existing Green behavior:

- save each stage to `SongPlayDataGreen`
- update `SongBestDataGreen`
- preserve normal and Shin best separation by `StageMode`
- apply rewards, costumes, medals, difficulty state, and ghost updates as today

For Dani playresults, additionally upsert Green Dan data:

- `ClearGrade = max(existing.ClearGrade, DanResult)`
- `ArrivalSongCount = max(existing.ArrivalSongCount, stage count)`
- `ComboCountTotal = max(existing.ComboCountTotal, ComboCntTotal)`
- `SoulGaugeTotal = max(existing.SoulGaugeTotal, SoulGaugeTotal)`
- per stage, preserve best score and best positive counters with max semantics
- per stage, preserve `BadCount` with min semantics after initializing the row from the first observed attempt

Stage/catalog song mismatch should be logged and the Dan score should still save if the Dan id is known. Current evidence is limited, and Green already uses normal selfbest requests for the underlying songs.

## Clear Flags and Display

Green BAID flags use fixed-size packed 2-bit values:

- `GotDanFlg`: 18 bytes for normal Dan states
- `GotDanExtraFlg`: 36 bytes for extra Dan states

Pack Green clear grades directly:

- `0 = none/not clear`
- `1 = normal clear`
- `2 = gold clear`
- `3 = reserved/unknown`; do not emit this from current persisted values

Mapping:

- normal Dan slot `1` maps to packed index `0`
- normal Dan slot `25` maps to packed index `24`
- extra Dan slot `101` maps to packed index `0`
- extra Dan slot `128` maps to packed index `27`

Sanitize any stored grade above `2` down to `2` before output.

Any grade `1` or `2` counts as cleared for `GotDanMax` and display advancement. `GotDanMax` is the highest cleared normal Dan slot. Extra Dans do not affect `GotDanMax`.

`DispTaikojukuDan` should represent the next uncleared normal Dan by default:

- if the saved value is unset, invalid, or already cleared, recompute it as the first normal slot in `1..25` whose `ClearGrade == 0`
- if all normal Dans are cleared, keep `25` so the wire value remains safe
- after clearing the currently displayed Dan, advance to the next uncleared normal Dan
- after failing a Dan (`DanResult = 0`), keep the current display Dan
- after clearing a non-displayed Dan, leave the current display Dan unless it is invalid, unset, or already cleared
- never set `DispTaikojukuDan` to an extra Dan id

The existing user data mapper must keep serializing a safe `disp_taikojuku_dan` value in `1..25`, because the Green client crashes on absent or out-of-range values.

## Read Flow

`BaidQuery.Green` should return Dan fields from `UserSaveDataGreen` after sanitization:

- fixed-length `GotDanFlg`
- fixed-length `GotDanExtraFlg`
- clamped `GotDanMax`
- normalized `DispDanType` if later evidence defines its domain

`UserDataQuery.Green` should return the safe current `DispTaikojukuDan`, using the next-uncleared rule above.

`GetDanScoreQuery.Green` should read `DanScoreDatumGreen` and return requested Dan score rows. The handler should primarily trust raw requested ids and filter them against the Green Taikojuku catalog. If the request type is useful, map normal requests to `1..25` and extra requests to `101+`, but do not translate to medley `uniqueid`.

Normal `selfbest.php` remains unchanged: it returns Green song bests for requested song ids and levels. Dani songs therefore appear through the same normal song score path used by the client today.

## Error Handling

- Unknown Dan ids: log and skip the Dani-specific update, while preserving existing normal playresult behavior.
- Invalid Green `DanResult > 2`: reject the Dani-specific update and log a warning.
- Conflicting nonzero `PlayDan` values across stages: reject the Dani-specific update and log a warning.
- Out-of-range packed flag data from storage: sanitize on output rather than failing BAID/userdata requests.

## Tests

Add focused tests for:

- first-Dan Green playresult saves normal song play/best rows and Green Dan rows
- `DanResult = 1` marks normal clear and advances `DispTaikojukuDan`
- `DanResult = 2` marks gold clear and preserves that higher grade
- `DanResult = 0` saves stage stats but does not update flags or display advancement
- normal slot `1` maps to `GotDanFlg` packed index `0`
- extra slot `101` maps to `GotDanExtraFlg` packed index `0` and does not affect `GotDanMax`
- duplicate songs in one medley keep separate `StageIndex` rows
- `GetDanScoreQuery.Green` returns saved Green Dan stage data for requested `challengelv` ids
- `UserDataQuery.Green` sends next uncleared normal Dan and never emits `0` or extra ids for `disp_taikojuku_dan`
