---
phase: 23-white-evidence-and-era-foundation
status: accepted
updated: 2026-06-18T04:58:09+08:00
---

# Phase 23 White Connection Smoke

## Trigger

The 2026-06-18 Host log showed a White cabinet identifying as `ST710JPN00.13` with `HddVer=700`, then posting direct-protobuf requests to approved `/v07r00/chassis/*` scaffold routes. The observed `initialdatacheck.php`, `heartbeat.php`, and `bookkeeping.php` requests returned HTTP 405.

## Root Cause

White was disabled in `Host/Configurations/ServerSettings.json`, so `Host/Program.cs` did not register `AddGameProtocolWhite()` and removed the White MVC application part. The controllers existed, but the running setup did not expose them.

The shared startup resolver also treated `HddVer=700` as unknown instead of resolving it to White.

## Fix Applied

- `Host/Configurations/ServerSettings.json` now enables `ServerSettings:Eras:White` for the local source setup.
- `Application/Handlers/GetStartupMovieDataQuery.cs` maps `HddVer / 100 == 7` to `GameEra.White`.

White catalog/profile/runtime behavior remains out of scope for Phase 23 and belongs to Phase 24+.

## Acceptance Gate

Passed. The user reran White cabinet/RPCS3 connection smoke against the fixed setup and reported: "Connection seems fine."

## Server Verification

- `dotnet test Tests\Tests.csproj --filter "FullyQualifiedName~WhiteHostRouteGatingTests|FullyQualifiedName~WhiteServerSettingsValidationTests|FullyQualifiedName~StartupMovieDataQueryTests|FullyQualifiedName~StartupAuthControllerTests" --no-restore --results-directory ".test-results\white-405" --logger "trx;LogFileName=white-405.trx"` passed: 10 passed, 0 failed, 0 skipped.
- `dotnet build Host\Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-white-405"` passed with 0 warnings and 0 errors.
