---
quick_id: 260615-wt6
slug: now-let-s-fix-several-issues-1-currently
status: completed
created: 2026-06-15
---

# Quick Plan: AC15 Selfbest Display and Favorite Limits

## Evidence

- Green song-select Lumen unpack output in `.tools/green/lumen/out/song_select/entry.disasm.txt` defines `MYBEST_NONE = 0`, `MYBEST_VISIBLE = 1`, `MYBEST_AD = 2`, and `MYBEST_NUM = 3`.
- The same script gates the song-select selfbest board on `MYBEST_VISIBLE` or `MYBEST_AD`, so response `disp_score_type` values `1` and `2` are display-capable.
- Green song-select course callbacks use the normal visible course indexes for Easy, Normal, Hard, and Mania/Oni; no independent `disp_level_self = 5` option was found in the song-select path.

## Tasks

1. Load and project Ura Oni selfbest rows for AC15 selfbest responses so `ura_best_score` and `ura_best_score_rate` are populated alongside the requested difficulty.
2. Carry saved `disp_score_type` through AC15 userdata snapshots, shared response DTOs, and protocol mapper output.
3. Move AC15 favorite caps to era profile limits and set current AC15 eras to `10`, including AdminApi enforcement.
4. Add observable regression tests for these behaviors and verify with targeted tests plus a temp-output host build with generated Mapperly source inspection.
