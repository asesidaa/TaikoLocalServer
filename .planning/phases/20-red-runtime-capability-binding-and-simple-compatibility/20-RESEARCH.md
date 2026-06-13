# Phase 20 - Research Notes

## Binary/Proto/Data Evidence

- `.tools/red/EBOOT.ELF.i64` was opened through the current `ida-cli` Python API with `AgentSession.start(..., daemon=True)`. The backend probe reported an open IDA database through `idalib`.
- Phase 19 binary evidence remains the limit source for Phase 20 runtime binding:
  - Red crown readback expects 1024 packed ten-bit crown entries.
  - Red userdata accepts 128-byte song/title-style flags and a 16-byte tone-style flag.
  - Red Taikojuku response handling caps songs per pack at 10.
- Red proto/wire exposes Don point fields, reward progress, difficulty tutorial/readback fields, Tokkun tutorial/readback fields, and ChallengeCompe arrays in playresult.
- Red proto/wire does not expose later-era item-shop purchase/catalog routes, medal fields, WaiWai state, battle state, or a dedicated Dan score readback route.

## Existing Code To Compose

- `Ac15MyDonEntryService` creates shared identity and era save-data through caller-provided factory delegates.
- `Ac15UserDataService` builds release-song/tone/title/userdata readback from an `Ac15UserDataSnapshot`.
- `Ac15SelfBestService` and `Ac15CrownService` are era-neutral once supplied concrete best rows and protocol limits.
- `Ac15NormalPlayWriter` persists play history, best rows, favorites, and recent rows through generic table bindings.
- `Ac15DaniWriter` persists Dan score/stage rows and applies save-data flag updates through era delegates.
- `Ac15NormalStageFilter`, `Ac15NormalStagePolicies.Standard`, and `Ac15NormalPlayMapper` own standard AC15 stage filtering and row conversion.
- `Ac15CustomizationMutation`, `Ac15UnlockFlagAccess`, and `Ac15ProfileCounterUpdater` already centralize current costume, unlock flag, and per-genre/profile counter behavior.

## Red Deltas

- Red runtime resource fields are Don points, not Don medals or shop-season medals. Red binding should not create shop-season state.
- Red `RewardExecutionRequest` carries unlock arrays, but Phase 20 classifies the route as stateless compatibility and does not make it authoritative for unlocks.
- Red Tokkun must update only nullable `TokkunTutorialFlg` on save data and must not add raw Tokkun history, normal-play, Dani, reward, or unlock writes.
- Red ChallengeCompe arrays should be mapped into common playresult data so Phase 21 can use them, but Phase 20 should not mutate challenge state.

## Implementation Shape

- Add Red EF entities matching the shared AC15 interfaces where applicable:
  - `UserSaveDataRed`
  - `SongPlayDatumRed`
  - `SongBestDatumRed`
  - `RedFavoriteSongs`
  - `RedRecentSongs`
  - `DanScoreDatumRed`
  - `DanStageScoreDatumRed`
- Add Red DbSets in `ITaikoDbContext` and `TaikoDbContext` partials with table names distinct from Green/Blue/Yellow.
- Add Red factory/default helpers and shared AC15 binding delegates.
- Add Red mapper/controller replacements for the Phase 20-owned probes.
- Add focused tests around Red persistence and no-cross-era boundaries.
