---
created: 2026-06-27T18:14:58.677Z
title: Fix Momoiro startup movies
area: general
files:
  - Application/Handlers/GetStartupMovieDataQuery.cs:26
  - Application/Abstractions/IMomoiroCatalog.cs:5
  - Infrastructure/GameDataCatalog/Momoiro/MomoiroEraGameDataCatalog.cs:13
  - Infrastructure/GameDataCatalog/Momoiro/MomoiroGameDataPaths.cs:5
  - Host/Host.csproj:202
  - Host/wwwroot/data/momoiro/momoiro_movie_data.json
  - Tests/Green/StartupMovieDataQueryTests.cs:11
  - Tests/Momoiro/MomoiroCatalogLoaderTests.cs:79
---

## Problem

MOMOIRO support should be fixed before Phase 43 AdminApi/WebUI work proceeds. The audit found that shared `/v01r00/chassis/startupauth.php` is movie-capable and `proto/momoiro/vsinterface.proto` includes `ary_movie_info`, but MOMOIRO is not wired into startup movie resolution.

Current gaps:

- `GetStartupMovieDataQueryHandler.ResolveEra()` maps HDD version families `5..12` but not `4 => GameEra.Momoiro`, so a MOMOIRO `4xx` startupauth request is treated as unknown and returns no movie permissions when multiple eras are enabled.
- `GetStartupMovieDataQueryHandler.Handle()` has no `GameEra.Momoiro => gameDataService.Momoiro().Movies` arm.
- `IMomoiroCatalog` and `MomoiroEraGameDataCatalog` expose/load telops only; they do not expose/load attract movie data.
- `Host/Host.csproj` only copies `momoiro_telop_data.json`; there is no committed `momoiro_movie_data.json` sidecar.
- Local operator data has `Host/wwwroot/data/momoiro/data/movie/attract_cm_100.pam`, so the absence is a compatibility gap, not merely an empty local catalog.
- Existing focused MOMOIRO tests pass serialized but do not cover MOMOIRO startup movie resolution; the non-serialized slice exposed a shared process-root copy race in `MomoiroCatalogLoaderTests`.

Do not broaden MOMOIRO feature scope while fixing this. Keep unsupported event-folder, item shop, Taikojuku route, Tokkun, Banacoin, battle, challenge, and proto-only route families absent unless separate MOMOIRO evidence proves them.

## Solution

Add the narrow startup movie slice before Phase 43:

- Add `MovieDirectory` to `MomoiroGameDataPaths`.
- Add a `MovieFileName` constant, `Movies` property, and `Ac15MovieLoader.LoadFromFileAsync(...)` call to `MomoiroEraGameDataCatalog`.
- Add `IReadOnlyList<MovieData> Movies` to `IMomoiroCatalog`.
- Add `GameEra.Momoiro => gameDataService.Momoiro().Movies` and `4 => GameEra.Momoiro` to `GetStartupMovieDataQueryHandler`.
- Commit `Host/wwwroot/data/momoiro/momoiro_movie_data.json` with the same safe default shape used by adjacent AC15 eras: `override_default=false`, `movies=[]`.
- Add the Host copy metadata for `momoiro_movie_data.json`.
- Add focused tests proving MOMOIRO `4xx` startup movie lookup returns discovered nonzero attract movies and that disabled MOMOIRO returns none.
- Stabilize or avoid the shared process-root copy race in MOMOIRO catalog loader tests if new tests touch process-root catalog data.

Verification should include the focused MOMOIRO/startup movie tests serialized, a non-serialized rerun if the harness is fixed, and `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"`.
