---
quick_id: 260615-wt6
status: complete
completed: 2026-06-15
code_commit: cd824405
---

# Quick Summary: AC15 Selfbest Display and Favorite Limits

## Result

- AC15 selfbest now loads Ura Oni rows when the requested level is Oni and populates `ura_best_score` / `ura_best_score_rate` for normal and Shin rows.
- AC15 userdata now carries saved `disp_score_type` from Green, Blue, Yellow, and Red save rows through the shared response DTO and generated Mapperly wire mappings.
- AC15 favorite-song limits are profile-backed and set to `10` for current AC15 eras. AdminApi and playresult paths now enforce the same era profile limit.

## Lumen Evidence

- `.tools/green/lumen/out/song_select/entry.disasm.txt` defines `MYBEST_NONE = 0`, `MYBEST_VISIBLE = 1`, `MYBEST_AD = 2`, and `MYBEST_NUM = 3`.
- The song-select selfbest display branch accepts `MYBEST_VISIBLE` and `MYBEST_AD`, confirming score types `1` and `2` display the board.
- The score course callbacks use the normal visible Easy, Normal, Hard, and Mania/Oni course indexes. No song-select evidence for an independent `disp_level_self = 5` option was found.

## Verification

- `dotnet test Tests\Tests.csproj --filter "FullyQualifiedName~Ac15SelfBestServiceTests|FullyQualifiedName~BlueSelfBestTests|FullyQualifiedName~Ac15UserDataServiceTests|FullyQualifiedName~GreenUserDataMapperTests|FullyQualifiedName~BlueUserDataTests|FullyQualifiedName~GreenAdminApiControllerTests|FullyQualifiedName~BlueAdminApiParityTests|FullyQualifiedName~RedAdminApiTests|FullyQualifiedName~YellowAdminApiTests|FullyQualifiedName~BluePlayResultHandlerTests|FullyQualifiedName~YellowPlayResultHandlerTests"`: 119 passed.
- `dotnet build Host\Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build" /p:EmitCompilerGeneratedFiles=true`: 0 warnings, 0 errors.
- Generated Mapperly inspection confirmed `response.DispScoreType = source.DispScoreType` in Blue, Green, Yellow, and Red `UserDataMappers.g.cs`.
- `dotnet test Tests\Tests.csproj`: 767 passed.
