# Phase 40: MOMOIRO Protocol Limits, Root Catalog, and Route Behavior - Context

## Implementation Decisions

### Scope

- Bind MOMOIRO root-level catalog data from `Host/wwwroot/data/momoiro/data`; do not assume a newer `config/STxxxx-*` layout.
- Required MOMOIRO catalog inputs for this phase are `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`.
- Runtime data roots must resolve through existing settings/path abstractions and era catalog interfaces, not handler-local hardcoded filesystem paths.
- Add an explicit `Ac15EraProfiles.Momoiro` only after researching limits and field placement; do not copy KIMIDORI or Murasaki numeric limits blindly.
- Phase 40 may implement catalog-backed behavior for `recommend.php`, `defaultsong.php`, `songhash.php`, `telopcheck.php`, and `gettelop.php`.
- `heartbeat.php` and `bookkeeping.php` should stay explicit static-result operational stubs unless binary/client evidence proves stateful behavior.
- Do not implement MOMOIRO identity persistence, userdata readback, self-best readback, normal playresult mutation, AdminApi, or WebUI behavior in this phase; those belong to Phases 41-43.
- Do not add MOMOIRO route families absent from Phase 39 evidence, including `shoppingresult.php`, `bestscore.php`, `communicationlog.php`, `mainichisong.php`, standalone `crownsdata.php`, Taikojuku/Tokkun/Banacoin/battle/Don Challenge/ChallengeCompe/event-folder/gacha/tournament/newer item-shop surfaces.

### Evidence Inputs

- Phase 39 verified the MOMOIRO route foundation, generated wire, enabled/disabled application-part gating, and no-state controller scaffolds.
- Local MOMOIRO data exists under `Host/wwwroot/data/momoiro/data` with root-level `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`.
- `proto/momoiro` is an evidence input for fields and generated DTOs, but proto presence alone is not route or behavior authority.
- `.tools/momoiro/EBOOT.ELF.i64` opens successfully through IDA-CLI idalib. Use this for local binary research where limits or field roles are not already proven.
- Initial IDA-CLI route-table evidence:
  - `sub_17BFD4` registers `baidcheck.php`, `mydonentry.php`, `userdata.php`, `recommend.php`, `selfbest.php`, and `heartbeat.php`.
  - `sub_17CD44` registers `defaultsong.php`, `bookkeeping.php`, `songhash.php`, `telopcheck.php`, and `gettelop.php`.
  - Route strings are stored as 32-bit pointer table entries around `0xACDA78..0xACDA8C` and `0xACDD20..0xACDD30`.
- Protobuf descriptor strings in the binary include `song_hash_ver`, `hash_default_song_flg`, `hash_release_song_flg`, `song_hash_tbl`, `hash_crown_flg`, favorite/recent arrays/counts, reward/Don Point fields, and challenge-shaped arrays; these strings prove field presence but not packing sizes or server semantics.

### Required Research Before Planning

- Determine the smallest evidence-backed MOMOIRO profile: song flag byte count, crown byte count/song count, favorite/recent limits, default-song flag size, song-hash table encoding, release-song flag size, Don Point/reward limits, and absent-feature flags.
- Research `hash_crown_flg` from binary/client evidence before implementing any crown packing or readback. Crown support remains userdata-owned unless new evidence proves a standalone crown route.
- Research unlock/release representation from binary/client evidence before implementing release-song flags or playresult unlock mutation. Phase 40 can define catalog/readback metadata; Phase 42 owns mutation.
- Compare KIMIDORI/Murasaki implementations only as structural analogs. If numeric limits differ or are unproven, record the gap rather than copying.

## Known Existing Patterns To Reuse

- KIMIDORI is the closest root-level catalog layout analog.
- Murasaki/KIMIDORI handlers are useful for `recommend.php`, `defaultsong.php`, `songhash.php`, telop routes, and AC15 readback composition, but MOMOIRO route support must remain restricted to the Phase 39 route inventory.
- Shared AC15 services should be preferred where they already encode catalog snapshots, recommendation selection, song-hash encoding, telop projection, protocol byte helpers, and era profiles.
- Tests should target observable catalog parsing, profile limits/packing, route responses, and no-cross-feature boundaries. Do not test generated wire property existence, controller attributes as source text, project file strings, or implementation-only constants without behavior.

## Deferred Ideas

- Phase 41: MOMOIRO-owned BAID/mydon/userdata/self-best/favorite/recent/crown readback and save-state persistence.
- Phase 42: MOMOIRO normal playresult mutation, score/crown persistence, release/unlock mutation, reward/Don Point mutation, Dan/challenge-compatible state where proven.
- Phase 43: MOMOIRO AdminApi/WebUI routing for implemented state only.
- Phase 44: full automated verification and cabinet/RPCS3 acceptance.
