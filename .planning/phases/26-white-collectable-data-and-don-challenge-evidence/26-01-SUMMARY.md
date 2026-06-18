# Phase 26 Plan 01 Summary: White Collectable Provenance Binding

## Completed

- Added `Ac15PresentItem` and `Ac15SpecialBaidEntry` catalog records.
- Added `Ac15PresentLoader` and `Ac15SpecialBaidLoader` for local White XML provenance.
- Exposed `Presents` and `SpecialBaids` through `IWhiteCatalog` and `WhiteEraGameDataCatalog`.
- Added `present.xml` and `spacialbaid.xml` to White required-data checks.
- Added focused White catalog tests for the ten present rows and two special BAID rows in local White data.
- Recorded White Don Challenge as absent/data-only because current White evidence proves embedded fields but not stateful route/data/reward semantics.

## Verification

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~White"` passed: 15 tests.
- `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` passed with 0 warnings and 0 errors.

## Notes

- Present item type codes are preserved as numeric values to avoid inventing semantic names.
- Special BAID rows are catalog facts only; they do not change identity behavior.
