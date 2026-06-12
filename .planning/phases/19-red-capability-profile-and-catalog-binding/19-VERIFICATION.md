---
phase: 19
slug: red-capability-profile-and-catalog-binding
status: passed
completed: 2026-06-13
---

# Phase 19 Verification

## Automated Verification

| Command | Result | Notes |
|---------|--------|-------|
| `dotnet build TaikoLocalServer.slnx` | passed | 0 warnings, 0 errors |
| `dotnet test Tests/Tests.csproj --no-build --filter "FullyQualifiedName~Red"` | passed | 50 passed, 0 failed |
| `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` | passed | 0 warnings, 0 errors |
| `dotnet test Tests/Tests.csproj --no-build --filter "FullyQualifiedName~Red|FullyQualifiedName~Ac15|FullyQualifiedName~StartupMovie"` | passed | 128 passed, 0 failed |
| `dotnet test Tests/Tests.csproj --no-build` | passed | 695 passed, 0 failed, 0 skipped |

## Scope Checks

- Red catalog/profile binding uses shared AC15 loaders, profiles, projection services, and Mapperly adapter mappers.
- Red item shop remains disabled and unadvertised.
- Only Phase 19-owned Red metadata probes were replaced: `initialdatacheck.php`, `getfolder.php`, `gettelop.php`, `recommend.php`, and `taikojuku.php`.
- Red gameplay persistence, BAID/userdata/crowns/self-best/playresult runtime handling, ChallengeCompe, AdminApi, WebUI, battle, WaiWai, and payment authority remain out of scope.

## Result

Phase 19 passed. Phase 20 can bind simple Red runtime compatibility on top of the Red catalog/profile support.
