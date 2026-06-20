---
status: complete
quick_id: 260621-4ze
task: support-random-recommend-song-only
completed: 2026-06-21
commits:
  - 84f219d1
---

# Quick Task 260621-4ze Summary

AC15 recommendation readback now follows the clarified camera behavior: userdata and `recommend.php` each return a random positive catalog song as `recommend_song`, and handlers leave `recommend_best_song` empty so the cabinet keeps the official five recommendation slots.

## Completed Tasks

| Task | Result | Commit |
|---|---|---|
| Replace static recommendation catalog data | Added shared AC15 recommendation behavior that picks one loaded catalog song and emits no best-song append list | `84f219d1` |
| Route recommend handlers through Application | Blue and Green recommend controllers now use Mediator and Mapperly mappers; Blue hard-coded experiment values were removed from recommend and userdata responses | `84f219d1` |
| Remove obsolete recommendation sidecars | Removed AC15 recommend loader classes, catalog interface properties, committed recommend JSON files, Host copy rules, and current README config references | `84f219d1` |
| Update observable coverage | Added cross-era recommend query coverage, updated userdata/readback tests for deterministic one-song catalogs, and removed obsolete sidecar-loader tests | `84f219d1` |

## Key Files

- `Application/Ac15/Ac15RecommendationService.cs`
- `Application/Ac15/Ac15CatalogReadbackService.cs`
- `Application/Ac15/Ac15UserDataService.cs`
- `Application/Handlers/GetRecommendQuery.Blue.cs`
- `Adapters.GameProtocol.Blue/Controllers/RecommendController.cs`
- `Adapters.GameProtocol.Blue/Mappers/RecommendMappers.cs`
- `Adapters.GameProtocol.Green/Controllers/RecommendController.cs`
- `Host/Host.csproj`
- `Tests/Ac15/Ac15RecommendQueryHandlerTests.cs`

## Verification

- `dotnet build Tests\Tests.csproj` - passed, 0 warnings/errors.
- `dotnet test Tests\Tests.csproj --filter "FullyQualifiedName~Recommend|FullyQualifiedName~YellowUserDataProtocolTests|FullyQualifiedName~BlueUserDataTests|FullyQualifiedName~GreenIdentityHandlerTests|FullyQualifiedName~Ac15UserDataServiceTests"` - passed, 48 tests.
- `dotnet build Adapters.GameProtocol.Blue\Adapters.GameProtocol.Blue.csproj /t:Rebuild /p:EmitCompilerGeneratedFiles=true` - passed, 0 warnings/errors.
- Inspected `Adapters.GameProtocol.Blue\obj\Debug\net10.0\generated\Riok.Mapperly\Riok.Mapperly.MapperGenerator\RecommendMappers.g.cs`; Mapperly generated the Blue response assignment for `Result`, `RecommendSong`, and `RecommendBestSongs` from `CommonRecommendResponse.RecommendBestSong`.
- `dotnet build Host\Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` - passed, 0 warnings/errors.
- `dotnet test Tests\Tests.csproj` - passed, 838 tests.
- `rg -n "recommend_songs\.json|blue_recommend_songs|yellow_recommend_songs|red_recommend_songs|white_recommend_songs" Host Application Infrastructure Tests -S` - no current code/config hits.

## Manual Gate

No RPCS3/cabinet compatibility verification was run for this quick task. Automated tests prove handler, mapper, persistence-boundary, and build behavior only.
