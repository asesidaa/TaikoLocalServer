---
status: complete
quick_id: 260623-2ff
task: support-murasaki-final-v06r01
completed: 2026-06-23
commit: 30552da7
---

# Quick Task 260623-2ff Summary: Murasaki Final `/v06r01`

Implemented Murasaki final support under `/v06r01/chassis/*` as a narrow delta on the existing `/v06r00` Murasaki implementation.

## Commit

- `30552da7` - `feat(260623-2ff): add Murasaki final protocol routes`

## What Changed

- Added Murasaki route prefix separation:
  - final: `/v06r01/chassis`
  - compatibility: `/v06r00/chassis`
- Added final route attributes to the already supported Murasaki controllers while preserving compatibility routes.
- Regenerated final Murasaki `Wire/Game.cs` from `proto/murasaki-final/taiko.proto`.
- Added `LegacyWire/Game.cs` from current `proto/murasaki/taiko.proto` for `/v06r00` compatibility.
- Split `getfolder.php` by protocol shape:
  - final reads repeated `FolderIds` and returns repeated `AryEventfolderDatas`.
  - compatibility reads scalar `FolderId` and returns scalar `FolderId` plus `SongNoes`.
- Preserved binary-supported final Dani/Taikojuku behavior:
  - final `taikojuku.php` returns catalog-backed packs.
  - final Dan-mode `playresult.php` uses the existing Murasaki Dan handler and creates Murasaki Dan state.
  - `/v06r00` Dani/Taikojuku behavior remains intact.
- Updated Host direct-protobuf fallback to accept both Murasaki prefixes.
- Added Murasaki protocol compatibility tests and the required test project reference.

## Evidence

- `proto/murasaki-final/vsinterface.proto` matched current `proto/murasaki/vsinterface.proto`.
- `proto/murasaki-final/taiko.proto` differed only in `getfolder.php` request/response shape.
- IDA route-string audit of `.tools/murasaki-final/EBOOT.ELF.i64` found `v06r01`, `v01r00`, and the already supported Murasaki route suffixes.
- No final binary/proto evidence was found for adding unsupported Murasaki `bestscore.php`, `songhash.php`, `shoppingresult.php`, `challengecompe.php`, Banacoin, Tokkun, battle, Yellow shop, or global-score persistence.

## Verification

- `git diff --no-index --stat -- proto/murasaki proto/murasaki-final`
  - Result: expected diff limited to `taiko.proto`.
- `rg -n "FolderIds|AryEventfolderDatas|EventfolderData" Adapters.GameProtocol.Murasaki/Wire/Game.cs`
  - Result: final wire contains repeated folder request/response rows.
- `rg -n "FolderId|SongNoes" Adapters.GameProtocol.Murasaki/LegacyWire/Game.cs`
  - Result: compatibility wire preserves scalar folder response shape.
- `dotnet build Adapters.GameProtocol.Murasaki/Adapters.GameProtocol.Murasaki.csproj`
  - Result: passed.
- `dotnet build Adapters.GameProtocol.Murasaki/Adapters.GameProtocol.Murasaki.csproj /p:EmitCompilerGeneratedFiles=true`
  - Result: passed. Folder mappers are handwritten, so there was no Mapperly-generated folder mapper to inspect.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MurasakiProtocolVersionCompatibilityTests|FullyQualifiedName~MurasakiRuntimeHandlerTests"`
  - Result: passed, 11 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Murasaki"`
  - Result: passed, 22 tests.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~WhiteProtocolVersionCompatibilityTests|FullyQualifiedName~WhiteRuntimeHandlerTests"`
  - Result: passed, 14 tests.
- `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"`
  - Result: passed.
- `rg -n -g "*.cs" "bestscore\.php|songhash\.php|shoppingresult\.php|challengecompe\.php|banacoin|battleuserdata\.php|getitemshopinfo\.php" Adapters.GameProtocol.Murasaki Host`
  - Result: no matches.

Known warnings observed during test/build runs:

- Existing `SQLitePCLRaw.lib.e_sqlite3` NU1903 advisory warning.
- Existing Murasaki `PlayResultMappers` Mapperly unmapped-member warnings during Host build.

## Deviations and Fixes

- Regenerated the new Murasaki wire without `+nullablevaluetype=yes` after an adapter build exposed incompatibility with the existing public DTO API used by current Murasaki code.
- Added a missing test import for `EventFolderData` after the first focused test compile failed.

## Manual Acceptance

2026-06-23: User reported that the Murasaki in-game flow was checked and works. This closes the quick task's manual RPCS3/cabinet compatibility gate for milestone closeout.
