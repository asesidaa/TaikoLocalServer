# AC15 Tournament Mode Research and Experiments

Date: 2026-06-22

Scope: AC15 tournament mode startup and event-gacha behavior, especially the
`tournamentcheck.php` request sent after `initialdatacheck.php`.

This note corrects the earlier working conclusion: `result = 1` is not enough
to make the cabinet run tournament mode end-to-end. The binary shows it is a
network "held/available" and cache-expiry signal. The real tournament flow also
requires the local Test Mode `TOURNAMENT MODE` setting to be ON, and the event
gacha material is driven by server-authored gacha bitsets.

## Manual and Material Evidence

Local photos under `H:\taiko\tournament`:

- `IMG_4809.webp`: tournament mode is configured from Test Mode. It removes
  normal time pressure from entry/song select and result screens, skips some
  normal post-play screens/rewards, supports 1P/2P with or without card, and
  saves scores/crowns to the game server. The page explicitly says the settings
  are reflected only online.
- `IMG_4810.webp`: before using tournament mode, the cabinet should be connected
  to the network, then `TOURNAMENT OPTIONS` should be selected from Test Mode.
- `IMG_4811.webp`: `TOURNAMENT OPTIONS [DEFAULT IN GREEN]` contains
  `TOURNAMENT MODE OFF/ON`, `SCORE MODE NORMAL/SHINUCHI`, default performance
  option reflection, and player option/tone-change permission.
- `IMG_4812.webp`: close-time matters. In the 15 or 30 minutes before close,
  cards/mobile cannot be used even in tournament mode; turning tournament mode
  OFF does not restore close-time settings.
- `Cu3afn2UsAAQ5y-.webp` and `HCIrrPYbMAAybvC.webp`: real event material for
  taikai-limited gacha / Reitaisai-limited gacha. These say a card/mobile touch on the event
  day gives one free, same-day limited gacha attempt. One material is for a
  Touhou/Reitaisai petit character reward.

Working conclusion from the photos: there are two different concerns:

1. Test Mode must locally set `TOURNAMENT MODE` to ON.
2. Online/server state supplies the "available today" condition and the
   tournament/event gacha payload.

## Online Evidence

Sources checked:

- Taiko Fumen Wiki Murasaki update history:
  https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E3%82%A2%E3%83%83%E3%83%97%E3%83%87%E3%83%BC%E3%83%88%E5%B1%A5%E6%AD%B4/%E3%83%A0%E3%83%A9%E3%82%B5%E3%82%AD
- Official Taiko blog, Reitaisai/Touhou collaboration notice:
  https://taiko-ch.net/blog/?p=568
- Taiko Time supplemental article:
  https://taikotime.blogspot.com/2016/01/tournament-mode-in-action.html
- Earlier tournament/Donda Hiroba sources kept for context:
  https://wikiwiki.jp/taiko-fumen/%E5%A4%A7%E4%BC%9A/%E3%82%AA%E3%83%B3%E3%83%A9%E3%82%A4%E3%83%B3%E5%85%AC%E5%BC%8F%E5%A4%A7%E4%BC%9A/AC15
  https://donderhiroba.jp/other_about.php
  https://donderhiroba.jp/other_faq.php
  https://taiko-ch.net/blog/?p=1135
  https://taiko.namco-ch.net/taiko/wcs2016/
  https://taiko.namco-ch.net/taiko/wcs2021/gaiden/

Findings:

- Wiki records Murasaki 8.07, dated 2015-10-29, as adding a tournament mode
  that can be configured only by the store/operator side.
- The official 2016 Reitaisai blog confirms the limited gacha concept seen in
  the local photos: a venue-limited petit-character gacha, one free attempt per
  eligible BanaPassport card.
- The Taiko Time article matches the manual behavior: tournament mode is staff
  operated, removes automatic timers, can preset/override options, keeps result
  screens up for score recording, and does not grant Don Points/title unlocks.
- Donda Hiroba and WCS sources show another tournament family: web-entered
  online competitions/challenges. That flow is related to challenge/compe state,
  but it should not be conflated with the local operator Test Mode tournament
  toggle unless a captured playresult proves shared fields.

## Local Protocol Evidence

Across Green, Blue, Yellow, Red, and White final protocol:

- `TournamentcheckRequest` has `chassis_id`, `shop_id`, and `kit_id`.
- `TournamentcheckResponse` has `result`, `rare_rate`, `song_hash_ver`, and
  gacha buckets:
  - `ary_gacha_song_data`
  - `ary_gacha_tone_data`
  - `ary_gacha_costume_1_data`
  - `ary_gacha_costume_2_data`
  - `ary_gacha_costume_3_data`
  - `ary_gacha_costume_4_data`
  - `ary_gacha_costume_5_data`
  - `ary_gacha_title_data`
- Each `GachainfoData` has `normal_gacha_flg` and `rare_gacha_flg`.
- `PlayResultRequest` has top-level `tournament_mode`:
  - Green/Blue/Yellow: field 38.
  - Red/White final: field 35.

Current server state:

- Green, Blue, Red, and White controllers return only
  `TournamentcheckResponse { Result = 1 }`.
- Yellow uses `TournamentCheckQuery` and still returns the empty default
  `CommonTournamentCheckResponse`.
- `CommonTournamentCheckResponse` already has all fields needed for a proper
  tournamentcheck payload, but no handler currently fills `song_hash_ver`,
  `rare_rate`, or any gacha bitsets.

## IDA Findings

Targets checked with `ida-cli`:

- Blue: `.tools/blue/EBOOT.ELF.i64`
- White: `.tools/white/EBOOT.ELF.i64`

Blue network behavior:

- `sub_152380`: `game::net::OnTournamentCheckRequest`; sends `kit_id = 1`.
- `sub_1513EC`: `game::net::OnTournamentCheckResponse`.
  - Logs `is_held=...`.
  - Accepts result codes `1`, `904`, and `905` as non-fatal states.
  - Sets request state global `dword_11DCC4C = 2` for accepted results, and
    `3` for other results.
  - Sets `byte_11DCC48` from the result code; inferred values:
    - `result = 1`: true/held.
    - `result = 904`: true/held-like accepted state.
    - `result = 905`: false/not-held accepted state.
  - For `result = 1`, reads `rare_rate` into `dword_11DCCB0`, parses gacha
    buckets, then sets `qword_11DCCB8` to the current business day cutoff
    timestamp. The timestamp code sets hour `26:00`, so the availability cache
    is "until 2 AM" style.
  - `sub_151188` checks `now < qword_11DCCB8`.
  - `sub_151188` is called from the tournament Test Mode display path, so the
    initial server check is still related to whether the Test Mode tournament
    UI is usable.

Blue gacha behavior:

- `sub_151158(index)` returns one of 8 global gacha-vector slots.
- `sub_163760` consumes those vectors through `sub_151158`.
- `sub_16350C` consumes `rare_rate`; it rolls `0..99` and compares with
  `rare_rate`. Therefore:
  - `rare_rate = 0` means never choose rare when normal choices exist.
  - `rare_rate = 100` means always choose rare when rare choices exist.
- Tournament attraction strings include:
  `/data/lumendata/packed/attract/tournament/packeddata.ddp`
  and `TournamentInfoTask`.

White cross-check:

- `sub_9C3E8`: `game::net::OnTournamentCheckRequest`; sends `kit_id = 1`.
- `sub_9BA70`: `game::net::OnTournamentCheckResponse`.
  - Logs `is_held=...`.
  - Sets `byte_DF6418` true when `result == 1`.
  - Treats `result == 904` as accepted state `2`; other non-1 values become
    error/not-held state `3`.
- White also has `TOURNAMENT MODE`, `TOURNAMENT OPTIONS`,
  `TournamentInfoTask`, and the same tournament attract packed-data path.

Existing Green byte report:

- `proto/green/ida-byte-field-findings.md` already documents the fixed gacha
  bitset sizes and little-bit-endian layout.

## Minimal Byte/Bitset Rules

For the server-side `tournamentcheck.php` response:

1. `result = 1` is required for the held/available path that parses gacha data.
   It is not sufficient by itself to put the cabinet into tournament mode; the
   operator Test Mode setting must also be ON.
2. `song_hash_ver` should be the current era catalog song hash version. IDA did
   not show it as the primary mode gate in the Blue response handler, but it is
   part of the response contract and should not be left as zero in experiments.
3. `rare_rate` controls normal-vs-rare selection in the gacha consumer:
   - use `0` for deterministic normal-only tests;
   - use `100` for deterministic rare-only tests;
   - use `1..99` only after the deterministic paths are proven.
4. The client accepts zero gacha rows, but event-gacha material requires at
   least one non-empty bucket to be meaningful.
5. Use one `GachainfoData` row per bucket for experiments. Blue reads the first
   row in each repeated bucket path; extra rows are not proven useful.
6. Every `GachainfoData` row has two bitsets:
   - `normal_gacha_flg`
   - `rare_gacha_flg`
7. Bit numbering is little-bit-endian:
   - item id `N` maps to `bytes[N >> 3] |= 1 << (N & 7)`.
8. Fixed byte sizes:
   - song: 128 bytes, 1024 bits.
   - tone: 16 bytes, 128 bits.
   - each costume slot: 32 bytes, 256 bits.
   - title: 128 bytes, 1024 bits.
9. Costume slot names in this repo map as:
   - `ary_gacha_costume_1_data`: `kigurumi`
   - `ary_gacha_costume_2_data`: `head`
   - `ary_gacha_costume_3_data`: `body`
   - `ary_gacha_costume_4_data`: `face`
   - `ary_gacha_costume_5_data`: `puchi`
10. The Reitaisai/Touhou material says petit character, so the first event-gacha
    candidate should use `ary_gacha_costume_5_data` unless catalog evidence
    identifies a different slot for that specific reward.

Minimal deterministic event-gacha payload shape:

- `Result = 1`
- `SongHashVer = <era catalog song_hash_ver>`
- `RareRate = 0`
- One `GachainfoData` in `AryGachaCostume5Data`
  - `NormalGachaFlg = FixedBitset(32 bytes, [known puchi id])`
  - `RareGachaFlg = new byte[32]`
- Other buckets can be empty for the first probe. If the client requires
  explicit rows in every bucket, use one all-zero fixed-width row in each
  remaining bucket.

## Current Minimal Server Experiment

The active code experiment returns `Result = 904` only. This is intentionally
not the event-gacha experiment:

- no `rare_rate`
- no `song_hash_ver`
- no gacha buckets

IDA shows `904` is accepted across the checked AC15 binaries, but it skips the
`result == 1` gacha parser path. This should test the smallest known
request-complete / accepted tournamentcheck state before collecting real gacha
payload evidence.

## Experiment Matrix

### Experiment 1: Current Baseline

Goal: confirm the actual current failure mode before changing server data.

Run:

1. Start Host with one target AC15 era.
2. Boot the cabinet and confirm logs show `initialdatacheck.php`, then
   `tournamentcheck.php` with `KitId=1`.
3. Enter Test Mode and check whether `TOURNAMENT OPTIONS` appears.
4. Check whether `TOURNAMENT MODE` can be switched ON.
5. Boot back into game and inspect whether the tournament overlay appears.
6. Play one credit and capture `playresult.php`, especially top-level
   `tournament_mode`.

Expected evidence:

- If `TOURNAMENT OPTIONS` is absent or disabled, the initial network check or
  saved Test Mode setting path is still blocking.
- If it can be switched ON but gameplay is normal, the local Test Mode setting
  is not being persisted or read back.

### Experiment 2: Result Code A/B

Goal: prove the visible effect of the accepted result codes.

Temporary response variants:

1. `Result = 0`
2. `Result = 904`
3. `Result = 905`
4. `Result = 1`

Keep all gacha buckets empty.

Expected evidence:

- `0` should behave as not accepted/error.
- `904` should be accepted state but not full event-gacha data.
- `905` should be accepted but not held.
- `1` should be the only variant that parses gacha and refreshes the daily
  availability cutoff.

### Experiment 3: Full Empty Fixed-Width Rows

Goal: verify safe byte shapes without granting rewards.

Response:

- `Result = 1`
- `SongHashVer = <era catalog song_hash_ver>`
- `RareRate = 0`
- One all-zero `GachainfoData` in every bucket:
  - song/title: 128-byte normal + 128-byte rare.
  - tone: 16-byte normal + 16-byte rare.
  - costume 1..5: 32-byte normal + 32-byte rare.

Expected evidence:

- No crash/retry proves the fixed rows are safe.
- Any UI change from baseline means the client expects bucket presence, not only
  nonzero bits.

### Experiment 4: Single Normal Puchi Bit

Goal: test the event-gacha material path with the smallest plausible reward.

Response:

- Start from Experiment 3 or from the minimal deterministic shape.
- Set exactly one known puchi/petit character id in
  `AryGachaCostume5Data[0].NormalGachaFlg`.
- Keep `RareRate = 0`.

Expected evidence:

- Tournament/event gacha UI or `TournamentInfoTask` behavior should change if
  these bytes drive the limited gacha screen.
- Do not persist unlocks from this experiment unless the cabinet reports a real
  acquisition/result field.

### Experiment 5: Forced Rare Puchi Bit

Goal: prove rare bit handling separately from normal bit handling.

Response:

- Clear the normal puchi bit.
- Set the same id in `AryGachaCostume5Data[0].RareGachaFlg`.
- Set `RareRate = 100`.

Expected evidence:

- If the same gacha surface appears and chooses the rare reward, `rare_rate`
  plus rare bitset semantics are confirmed.

### Experiment 6: Test Mode ON Flow

Goal: distinguish server availability from the local operator setting.

Run after a `Result = 1` response:

1. Enter `TOURNAMENT OPTIONS`.
2. Set `TOURNAMENT MODE = ON`.
3. Set `SCORE MODE` to normal first, then shinuchi in a later pass.
4. Leave default option reflection/change settings at the manual defaults for
   the first pass.
5. Exit Test Mode normally, boot gameplay, and capture the title/select/result
   screens.

Expected evidence:

- If this works only after `Result = 1`, server availability is gating the local
  Test Mode setting.
- If this still cannot be enabled, inspect the cabinet's Test Mode storage path
  and the local setting backend in IDA; more tournamentcheck bytes are probably
  not the missing toggle.

### Experiment 7: Tournament Playresult Capture

Goal: identify server persistence obligations once tournament mode is actually
entered.

Capture:

- top-level `tournament_mode`
- stage song id/difficulty/options
- challenge/compe arrays:
  - `ary_challenge_id`
  - `ary_user_compe_id`
  - `ary_bng_compe_id`
- score mode indicators
- whether score/crown/recent/favorite behavior differs from normal play

Expected evidence:

- If only `tournament_mode` changes, first implementation can log and preserve
  normal score handling.
- If challenge/compe ids are populated, tournament scoring must be modeled as a
  separate AC15 tournament capability and must not reuse White/Red Don Challenge
  state without binary/capture proof.

### Experiment 8: Close-Time Boundary

Goal: avoid misdiagnosing close-time lockout as tournament server failure.

Run:

1. Set close time far away from the current time.
2. Repeat the Test Mode ON flow.
3. Move close time into the 15/30 minute pre-close window and repeat.

Expected evidence:

- Card/mobile unavailability near close time should match the manual. It should
  not be treated as a bad tournamentcheck response.

## Implementation Notes If Experiments Pass

- Add an explicit experimental tournament-gacha sidecar instead of hardcoding
  bits in controllers.
- Populate through `TournamentCheckQuery` and `CommonTournamentCheckResponse`;
  do not edit `proto/`.
- Start with one era, probably Blue or Yellow, where the current wire includes
  all gacha buckets and the Blue IDB evidence is strongest.
- Keep tournament mode itself separate from Donda Hiroba challenge/compe state
  until a tournament-mode playresult proves shared ids.
- Do not add normal score/crown behavior changes until a captured tournament
  playresult proves them.
