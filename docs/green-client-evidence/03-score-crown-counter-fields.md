# Score, Crown, and Counter Field Evidence

## Scope

Endpoints: `selfbest.php`, `crownsdata.php`, request-side `playresult.php`.

## Evidence Table

| Field | IDA evidence | Constraint | 0 valid? | Max/list length | Server behavior at audit baseline/current tree | Recommendation |
|---|---|---|---|---|---|---|
| `selfbest.level` | Source-inferred; no `level` string in the score/crown artifact. | Course `0..4`. | Yes, Easy. | Not found. | Current tree maps any value; invalid maps to `Difficulty.None` in `Application/Handlers/GetSelfBestQuery.Green.cs:9`. | Add `0..4` request guard in `GetSelfBestQuery.Green.cs:9` or `SelfBestController.cs:13`. |
| `selfbest.ary_selfbest_score[].song_no` | IDA-proven list field `ary_selfbest_score` at `0xF026B6`; `song_no` value constraint source-inferred. | Valid Green catalog song and `<1024`. | `0` is not a valid catalog song for persistence/echo. | List cap not found. | Current tree echoes every requested song at `GetSelfBestQuery.Green.cs:10` and `GetSelfBestQuery.Green.cs:23`. | Validate/filter requested song IDs in `GetSelfBestQuery.Green.cs:10`; add list cap once client cap is known. |
| `selfbest.ura_best_score` | IDA-proven field string `0xF0276D`; Ura semantics not found. | Not found; keep zero until real Ura mapping exists. | `0` valid as no score. | Same row count as requested songs by current source. | Current tree leaves `UraBestScore` zero in normal rows at `GetSelfBestQuery.Green.cs:18`. | Do not synthesize Ura values without chart/course evidence; update `GetSelfBestQuery.Green.cs:18` if Ura mapping is proven. |
| `selfbest.ary_shin_selfbest_score` | IDA-proven field string `0xF026F2`; runtime capture maps Green `stage_mode=1` to Shin score mode. | Source-inferred: same requested-song row shape as normal selfbest. | `0` valid as no saved Shin score. | Same row count as requested songs. | Green selfbest now reads saved `SongBestDatum_Green` rows where `IsShin=true`; missing rows remain zero. | Keep `stage_mode` evidence documented and do not infer Shin from score range. |
| `crownsdata.hash_crown_flg` | IDA-proven field string `0xF028C6`; encoding source-inferred. | Source-inferred zlib body inflates to `1280` bytes, 1024 songs * 10 bits. | All-zero table valid empty state. | Fixed 1280-byte inflated body. | Current tree builds and compresses in `CrownsDataController.cs:16` and `CrownsDataController.cs:22`; builder ignores `SongId >= 1024` at `GreenCrownResponseBuilder.cs:9`. | Add catalog membership filtering before `GreenCrownResponseBuilder.BuildInflatedBody`, not only `<1024`. |
| crown state value `3` | Not found for response bitset; only `hash_crown_flg` schema is proven. | Safe response states remain `0 none`, `1 clear`, `2 full combo`; `3` not client-proven for Green crown bitsets. | `0` valid none. | Two bits per course by source packing. | Current tree maps inbound `play_result=3` to Dondaful but outputs it as FullCombo in `GreenPlayResultMapping.cs:28`. | Keep downgrade unless client bitset evidence proves `3`; then change `GreenPlayResultMapping.cs:28`. |
| `playresult.stage.play_score` | IDA-proven field string `0xF01F31`; max not found. | Source-inferred: valid only for chart/catalog song/course; exact max needs chart data. | `0` valid no score. | Chart max not found. | Current tree validates song/course and play result in `UpdatePlayResultCommand.Green.cs:68`; score remains raw `uint` at `UpdatePlayResultCommand.Green.cs:149` and `:212`. | Add chart-aware max score guard in `IsValidGreenStage` at `UpdatePlayResultCommand.Green.cs:68`. |
| hit/count fields | IDA-proven field strings: `good_cnt` `0xF0202A`, `ok_cnt` `0xF0203E`, `ng_cnt` `0xF0204E`, `pound_cnt` `0xF0205C`, `combo_cnt` `0xF0206F`, `hit_cnt` `0xF02133`. | Source-inferred: counts must be chart-consistent; exact maxima require chart data. | `0` valid for empty/no count fields. | Chart note max and ghost section cap not found. | Current tree copies raw counts at `UpdatePlayResultCommand.Green.cs:151-156` and ghost section counts at `UpdatePlayResultCommand.Green.cs:185-188`. | Add chart-aware count consistency in `IsValidGreenStage` and section count/count guards before `UpdatePlayResultCommand.Green.cs:178`. |

## Artifact Files

- `.tools/green-field-evidence/score_crown_strings.json`
- `.tools/green-field-evidence/score_crown_xrefs.json`
