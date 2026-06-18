# Phase 24 Research: White Catalog Profile and Protocol Limits

## Evidence

- Phase 23 proved White game routes are `/v07r00/chassis/*`, startup/version routes stay `/v01r00/chassis/*`, and White direct protobuf game endpoints are the only approved transport.
- Active White data files are present under `Host/wwwroot/data/white/data/config/ST7100-1`: `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, `spacialbaid.xml`, and `chassisinfo.xml`. `Host/wwwroot/data/white/data/fumen/tuning.bin` is also present.
- The White IDB at `.tools/white/EBOOT.ELF.i64` is non-empty and exposes descriptor strings for `ary_favorite_song_no`, `ary_recent_song_no`, `song_favorite_cnt`, and `song_recent_cnt`, but a light string/symbol pass did not surface a named max favorite cap.
- Red/Yellow catalog loaders already prove the shared AC15 loader shape for `musicinfo.xml`, `musicmedleyinfo.xml`, `tuning.bin`, event folders, telops, recommendations, movies, Taikojuku verup defaults, and customization sidecars.

## Implementation Shape

- Add White-owned catalog interfaces/classes rather than treating White as Red.
- Keep White sidecars named with a `white_` prefix and committed under `Host/wwwroot/data/white`.
- Register White catalogs only when `GameEra.White` is enabled.
- Add `Ac15EraProfiles.White` and route it through `TryGet`/auth-config limit publishing.

## Deferred

- Exact older-era favorite cap change remains deferred until a deeper IDA pass identifies the White runtime constant. The code is structured so `CreateWhiteLimits()` can change independently.
- `present.xml`, `spacialbaid.xml`, reward state, Don Challenge, and collectable data are Phase 25/26 responsibilities.
