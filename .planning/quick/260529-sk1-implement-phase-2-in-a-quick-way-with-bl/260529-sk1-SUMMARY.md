---
status: complete
quick_id: 260529-sk1
date: 2026-05-29
commit: faa2117c
---

# Quick Task 260529-sk1 Summary

Implemented Blue A7 AdminApi/WebUI parity as a quick slice.

## Completed

- Added Blue AdminApi branches for play data, play history, favorites, customization catalogs, user settings, and era-aware song leaderboards.
- Added Blue user-settings persistence in a Blue-owned partial controller using `UserSaveDataBlue`, `BlueFavoriteSongs`, `SongBestDataBlue`, `SongPlayDataBlue`, and Blue protocol bit widths.
- Updated WebUI era routing so Blue user pages call `/api/Blue/...` instead of falling back to unversioned Nijiiro settings or leaderboards.
- Added `WebUiEra.IsAc15` and made Blue use the AC15-style profile, favorite-limit, title-selection, and rank-display gates already used for Green.
- Added Blue AdminApi parity tests and WebUI source-route guard tests.

## Verification

- `dotnet test Tests\Tests.csproj --filter "FullyQualifiedName~BlueAdminApiParityTests|FullyQualifiedName~GreenCustomizationWebUiTests|FullyQualifiedName~GameDataServiceTests"`: passed, 37 tests.
- `dotnet test Tests\Tests.csproj --filter "FullyQualifiedName~Blue"`: failed only on four pre-existing dirty-file baseline assertions caused by local Blue enablement and shop-date edits in `Host/Configurations/ServerSettings.json` and `Host/wwwroot/data/blue/blue_item_shop_data.json`.
- `dotnet test Tests\Tests.csproj --filter "FullyQualifiedName~Blue&FullyQualifiedName!~ShippedServerSettings_DeclaresBlueDisabledByDefault&FullyQualifiedName!~ShippedServerSettings_DeclaresBlueCatalogSettings&FullyQualifiedName!~CommittedDefaultJsonMatchesOfficialCache&FullyQualifiedName!~LoadAsync_DefaultBlueItemShopDataLoads"`: passed, 182 tests.
- `dotnet build Host\Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-a7-quick"`: passed.

## Notes

- Left the pre-existing local Blue enablement/data edits unstaged.
- Source commit: `faa2117c`.
