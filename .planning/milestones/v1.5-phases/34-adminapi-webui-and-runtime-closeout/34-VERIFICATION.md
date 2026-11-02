# Phase 34 Verification

## Automated Verification

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Murasaki|FullyQualifiedName~WebUi"`
  - Passed: 44 tests.
- `dotnet build TaikoLocalServer.slnx /p:EmitCompilerGeneratedFiles=true`
  - Passed.
  - Original Phase 34 run had pre-existing SQLite and Murasaki Mapperly warnings; follow-up warning cleanup resolved both.
  - 2026-06-23 final closeout rerun passed with 0 warnings and 0 errors.
  - Phase 34 did not add or change Mapperly mapper declarations, so no new generated Mapperly mapping body required inspection.
- `dotnet test Tests/Tests.csproj --no-build`
  - Passed: 857 tests during Phase 34 verification.
  - 2026-06-23 final closeout rerun passed: 865 tests.
- `git diff --check`
  - Passed with line-ending normalization warnings only.
- `dotnet list TaikoLocalServer.slnx package --vulnerable --include-transitive`
  - Passed: no vulnerable packages reported.

## Behavior Covered

- Murasaki WebUI era normalization, API route helpers, AC15 classification, catalog request paths, and Don Challenge exclusion.
- Murasaki AdminApi profile settings read/write against Murasaki save data.
- Murasaki score, play history, leaderboard, favorite song, and Dani best readback against Murasaki tables.
- Murasaki catalog and customization readback against Murasaki catalog slices.
- No-cross-era checks for seeded White/Red/Blue rows in Murasaki AdminApi read/write tests.

## Manual Acceptance

- 2026-06-23: User reported that the Murasaki in-game flow was checked and works.
- WebUI/AdminApi acceptance evidence for this closeout remains the focused automated WebUI/AdminApi test coverage above; no new WebUI runtime issue was reported during final acceptance.

Phase 34 and v1.5 are accepted for milestone closeout.
