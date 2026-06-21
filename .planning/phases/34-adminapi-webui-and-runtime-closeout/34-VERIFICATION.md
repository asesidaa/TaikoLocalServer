# Phase 34 Verification

## Automated Verification

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Murasaki|FullyQualifiedName~WebUi"`
  - Passed: 44 tests.
- `dotnet build TaikoLocalServer.slnx /p:EmitCompilerGeneratedFiles=true`
  - Passed.
  - Existing warnings remain:
    - `SQLitePCLRaw.lib.e_sqlite3` NU1903 high-severity advisory.
    - Existing Murasaki `PlayResultMappers.cs` Mapperly RMG020 unmapped-source warnings.
  - Phase 34 did not add or change Mapperly mapper declarations, so no new generated Mapperly mapping body required inspection.
- `dotnet test Tests/Tests.csproj --no-build`
  - Passed: 857 tests.
- `git diff --check`
  - Passed with line-ending normalization warnings only.

## Behavior Covered

- Murasaki WebUI era normalization, API route helpers, AC15 classification, catalog request paths, and Don Challenge exclusion.
- Murasaki AdminApi profile settings read/write against Murasaki save data.
- Murasaki score, play history, leaderboard, favorite song, and Dani best readback against Murasaki tables.
- Murasaki catalog and customization readback against Murasaki catalog slices.
- No-cross-era checks for seeded White/Red/Blue rows in Murasaki AdminApi read/write tests.

## Manual Acceptance Pending

Cabinet/RPCS3 and WebUI acceptance are still user-observed closeout gates. Do not mark Phase 34 or v1.5 shipped until the user accepts those runtime flows.
