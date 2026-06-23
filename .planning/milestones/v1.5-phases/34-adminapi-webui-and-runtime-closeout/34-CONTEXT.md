# Phase 34 Context: AdminApi/WebUI and Runtime Closeout

## User Direction

The user confirmed the three previously unseen Murasaki proto request families do not have routes and are probably from even older eras. Phase 34 should proceed, with existing AC15 byte-field limits treated as valid after Phase 33 verification. WebUI work should not add special controls; unsupported Murasaki features must stay hidden.

## Current Evidence Baseline

- Phase 33 records `bestscore.php`, `songhash.php`, and `shoppingresult.php` as proto-only for the current Murasaki scope. They remain unsupported until route/client evidence exists.
- Phase 33 verifies Murasaki uses the existing AC15 byte limits through `Ac15EraProfiles.Murasaki`.
- Murasaki runtime handlers, catalog access, profile defaults, userdata, self-best, favorites, recent songs, normal playresult, Dani, reward, present/special-BAID, and Don Point mutation already exist in Application/Domain/Infrastructure.
- AdminApi/WebUI parity is incomplete: the visible operator era list and multiple era-routed AdminApi controllers stop at White.

## AdminApi/WebUI Gaps Found

- `TaikoWebUI/Utilities/WebUiEra.cs` does not include Murasaki in `Supported`, `Known`, or `IsAc15`.
- `GameDataController` lacks Murasaki music/Dani catalog branches.
- `CustomizationCatalogController` lacks Murasaki costume/title/neiro catalog branches.
- `Ac15ProfileSettingsController` lacks Murasaki read/write branches.
- `PlayDataController`, `PlayHistoryController`, `SongLeaderboardController`, `DanBestDataController`, and `FavoriteSongsController` lack Murasaki-owned persistence branches.
- Don Challenge UI is already capability-gated by `WebUiEra.SupportsOlderAc15DonChallenge`; it must remain Red/White-only.

## Scope

Implement existing AdminApi/WebUI surfaces for Murasaki-owned state only:

- Era selection and AC15 UI routing.
- Profile settings read/write through Murasaki save data.
- Normal score, history, leaderboard, favorite, and Dani readback through Murasaki tables.
- Music/Dani/customization catalog readback through the Murasaki catalog.

Keep absent:

- Global score controls.
- Shopping controls.
- Don Challenge or ChallengeCompe controls.
- Proto-only special request families from Phase 33.
- Tokkun-like or battle-like history surfaces.

## Verification Expectations

- Add Murasaki-focused AdminApi tests for era-owned persistence and catalog behavior.
- Add WebUI tests proving Murasaki is an AC15 supported era and Don Challenge remains hidden for Murasaki.
- Run focused Murasaki/WebUI tests, full build with generated source emission, inspect relevant generated Mapperly output if new nontrivial Mapperly mapping appears, then run the full test suite.
- Record automated verification separately from user-observed cabinet/RPCS3 and WebUI acceptance.
