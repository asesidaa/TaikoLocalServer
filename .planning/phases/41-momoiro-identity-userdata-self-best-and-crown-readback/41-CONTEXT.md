---
phase: 41
slug: momoiro-identity-userdata-self-best-and-crown-readback
status: draft
created: 2026-06-26
---

# Phase 41 Context - MOMOIRO Identity, Userdata, Self-Best, and Crown Readback

## Goal

MOMOIRO users can register, log in, enter MyDon/profile flow, and read back MOMOIRO-owned profile, score, release, hash, favorite/recent, and crown state without sharing era-owned state with any other era.

## Requirements

- MORDB-01: MOMOIRO card registration, login, mydon entry, and userdata readback use MOMOIRO-owned save state while sharing only true identity data across eras.
- MORDB-02: MOMOIRO self-best readback returns MOMOIRO-owned score state with era-correct normal, ura, and shin handling where proven.
- MORDB-03: MOMOIRO favorite and recent song readback uses binary/client-backed limits, ordering, truncation, and duplicate behavior.
- MORDB-04: MOMOIRO crown bytes in `userdata.php` use binary/client-backed packing, song count, difficulty placement, and default behavior.
- MORDB-05: MOMOIRO release-song and song-hash readback uses MOMOIRO catalog order and binary-backed `song_hash_ver`, `song_hash_tbl`, and `hash_release_song_flg` semantics.

## Inputs From Prior Phases

- Phase 39 registered MOMOIRO as a first-class era under `/v04r00/chassis/*` with shared `/v01r00/chassis/*` startup/version routes.
- Phase 39 generated MOMOIRO wire from `proto/momoiro` and left proto inputs untouched.
- Phase 40 added the root MOMOIRO catalog from `Host/wwwroot/data/momoiro/data`, explicit AC15 profile limits, catalog-backed `recommend.php`, `defaultsong.php`, `songhash.php`, `telopcheck.php`, `gettelop.php`, and static `heartbeat.php`/`bookkeeping.php`.
- Phase 40 verification passed focused serialized tests 27/27, full serialized suite 908/908, solution build, temp Host build, proto cleanliness, unsupported-route absence, and path-abstraction gates.

## Native Evidence Snapshot

Target: `.tools/momoiro/EBOOT.ELF.i64`, opened with IDA-CLI idalib.

- `sub_17BFD4` registers `chassis/baidcheck.php`, `chassis/mydonentry.php`, `chassis/userdata.php`, `chassis/recommend.php`, `chassis/selfbest.php`, and `chassis/heartbeat.php`.
- Route table slots near `0xACDA78..0xACDA8C` point to those route strings.
- Generated MOMOIRO wire and IDA descriptor strings contain `ary_selfbest_score`, `ary_shin_selfbest_score`, `self_best_score`, `ura_best_score`, `ary_favorite_song_no`, `ary_recent_song_no`, `song_favorite_cnt`, `song_recent_cnt`, `song_hash_ver`, `hash_release_song_flg`, and `hash_crown_flg`.
- `UserDataResponse.hash_crown_flg` is field 8 in current generated MOMOIRO wire; no standalone `crownsdata.php` route is in the binary route list.
- Phase 40 established the MOMOIRO song count as 380, song hash version as `538116869`, 760-byte song hash table, 48-byte compact release/default song flag envelope, and an inferred 475-byte compact crown envelope for 380 songs * 10 bits.

## Scope Boundaries

In scope:

- MOMOIRO-owned identity/save data needed for `baidcheck.php`, `mydonentry.php`, `userdata.php`, and `selfbest.php`.
- MOMOIRO-owned favorite and recent rows for readback.
- MOMOIRO-owned score/best rows for readback only.
- MOMOIRO release-song and crown readback from persisted MOMOIRO fields.
- Read-only route/controller/mapper behavior for the above.
- EF migrations and tests that protect no-cross-era persistence.

Out of scope:

- `playresult.php` mutation, score writes, crown writes, favorite/recent writes, Don Point/reward mutation, Dan mutation, and challenge-shaped mutation. Those belong to Phase 42.
- AdminApi/WebUI exposure. That belongs to Phase 43.
- Cabinet/RPCS3 acceptance. That belongs to Phase 44 unless the user provides evidence early.
- Proto edits, unsupported route families, Taikojuku/Tokkun/Banacoin/battle/gacha/tournament/Don Challenge/ChallengeCompe/event-folder/newer shop authority.

## Planning Risks

- Favorite max and recent max are currently conservative Phase 40 values. Use existing profile values unless new binary/client evidence proves different values during Phase 41 research.
- Crown packing should reuse shared AC15 helpers only after tests prove MOMOIRO byte length, song ordering, and difficulty placement. Do not invent a standalone crown route.
- Self-best normal/ura/shin behavior must follow current MOMOIRO generated wire. If shin is present only as `ary_shin_selfbest_score`, read it through the same shared self-best response model used by adjacent AC15 eras.
- Persistence must add MOMOIRO-owned tables or columns without touching adjacent era state.

## Expected Validation Shape

- Tests for registration/login/mydon creating shared identity plus MOMOIRO save only.
- Tests for `userdata.php` readback from MOMOIRO-owned save, favorites, recent, release flags, song hash version/table, and crown bytes.
- Tests for `selfbest.php` returning MOMOIRO-owned normal/ura/shin score data.
- No-cross-era tests proving Blue/Green/Yellow/Red/White/Murasaki/Kimidori/Nijiiro data is not read or written by MOMOIRO routes.
- Generated Mapperly source inspection for any new MOMOIRO route mappers.
- `dotnet test Tests/Tests.csproj --no-restore -- RunConfiguration.DisableParallelization=true`, `dotnet build TaikoLocalServer.slnx --no-restore`, temp Host build if runtime artifacts change, and `git status --porcelain -- proto/momoiro`.
