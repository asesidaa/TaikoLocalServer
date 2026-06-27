---
status: complete
quick_id: 260628-3ef
task: fix-momoiro-startup-movie-support-before
completed: 2026-06-27
commits:
  - aa93a2d5
---

# Quick Task 260628-3ef Summary

MOMOIRO startup movie permissions are now enabled through the same AC15 sidecar and raw `data/movie` discovery path used by neighboring eras.

## Completed Tasks

| Task | Result | Commit |
|---|---|---|
| Add MOMOIRO movie catalog contract | `IMomoiroCatalog` and `MomoiroEraGameDataCatalog` now expose `Movies` loaded by `Ac15MovieLoader` from `momoiro_movie_data.json` plus `data/movie` | `aa93a2d5` |
| Wire startup movie era resolution | `hdd_ver / 100 == 4` now resolves to `GameEra.Momoiro`, and startup movie lookup returns `gameDataService.Momoiro().Movies` | `aa93a2d5` |
| Add server-authored sidecar | Added committed `Host/wwwroot/data/momoiro/momoiro_movie_data.json` and Host copy metadata | `aa93a2d5` |
| Add regression coverage | Added MOMOIRO startup movie query coverage and catalog-loader coverage for the sidecar/movie directory path | `aa93a2d5` |

## Key Files

- `Application/Handlers/GetStartupMovieDataQuery.cs`
- `Application/Abstractions/IMomoiroCatalog.cs`
- `Infrastructure/GameDataCatalog/Momoiro/MomoiroEraGameDataCatalog.cs`
- `Infrastructure/GameDataCatalog/Momoiro/MomoiroGameDataPaths.cs`
- `Host/wwwroot/data/momoiro/momoiro_movie_data.json`
- `Host/Host.csproj`
- `Tests/Momoiro/MomoiroStartupMovieDataQueryTests.cs`
- `Tests/Momoiro/MomoiroCatalogLoaderTests.cs`

## Verification

- RED: `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroStartupMovieDataQueryTests|FullyQualifiedName~MomoiroCatalogLoaderTests.CatalogInitialize_LoadsMomoiroMovieSidecarAndDiscoversAttractMovies" --no-restore -- RunConfiguration.DisableParallelization=true` failed as expected on missing `momoiro_movie_data.json` and empty `hdd_ver` 413 movie resolution.
- `dotnet build Tests/Tests.csproj --no-restore /m:1 /nodeReuse:false` - passed, 0 warnings/errors.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroStartupMovieDataQueryTests|FullyQualifiedName~MomoiroCatalogLoaderTests.CatalogInitialize_LoadsMomoiroMovieSidecarAndDiscoversAttractMovies|FullyQualifiedName~StartupMovieDataQueryTests" --no-build -- RunConfiguration.DisableParallelization=true` - passed, 7 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Momoiro|FullyQualifiedName~StartupMovieData" --no-build -- RunConfiguration.DisableParallelization=true` - passed, 47 tests.
- `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build" /m:1 /nodeReuse:false` - passed, 0 warnings/errors.
- Verified `wwwroot/data/momoiro/momoiro_movie_data.json` exists in the temp Host build output.

## Manual Gate

No cabinet/RPCS3 compatibility verification was run for this quick task. Automated checks prove server-side startup movie dispatch, catalog loading, and build output only.

## Notes

- Did not touch or copy over `Host/wwwroot/data/momoiro/data`.
- Did not add unsupported MOMOIRO route families or WebUI/AdminApi behavior.
