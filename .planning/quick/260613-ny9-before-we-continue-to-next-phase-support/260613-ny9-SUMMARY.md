---
status: complete
quick_id: 260613-ny9
commit: d7032433
completed: 2026-06-13
---

# Quick Task 260613-ny9 Summary

## Result

Added older Red `/v08r00/chassis/*` compatibility while keeping Red state, handlers, catalogs, and persistence shared with the existing Red generation.

## Changes

- Generated older Red wire DTOs from `proto/red-int/taiko.proto` into `Adapters.GameProtocol.Red/Wire/V08R00/Game.cs`.
- Added `/v08r00/chassis/*` route aliases to Red controllers whose message shapes match `/v08r01`.
- Added a dedicated `/v08r00/chassis/baidcheck.php` controller method using the generated old wire request/response types.
- Added `BaidResponseMapper.MapV08R00` so older BAID readback omits `got_danextra_flg` and serializes trailing fields at the older field numbers.
- Added a focused protobuf serialization test for the old BAID field-number contract.

## Verification

- `dotnet test Tests\Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Red"` passed: 22 passed.
- `dotnet build Host\Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` passed with existing Mapperly warnings and 0 errors.
- `git diff --cached --check` passed before the code commit.

## Notes

- The worktree had unrelated dirty files before this task. They were left unstaged.
- `Adapters.GameProtocol.Red/Mappers/PlayResultMappers.cs` was already dirty; a working-tree nullable-helper adjustment was needed for the current dirty Mapperly refactor to pass focused Red tests, but it was not included in the quick-task code commit because the surrounding refactor was pre-existing user work.
