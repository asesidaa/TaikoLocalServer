# Infrastructure

Infrastructure implements the ports defined in `Application/Abstractions`. It owns EF Core SQLite persistence, migrations, filesystem catalogs, JWT issuance, and the system clock.

## Role

Application handlers depend on interfaces. This project provides the concrete database, catalog, identity, and time implementations used by the host process.

The filesystem catalog is era-aware. `FileGameDataCatalog` creates enabled-era catalogs for Nijiiro, Green, Blue, Yellow, Red, and White according to `Host/Configurations/ServerSettings.json`.

## Key Folders

- `Persistence/` - `TaikoDbContext`, entity configuration, and migrations.
- `GameDataCatalog/` - era catalog loaders, `FileGameDataCatalog`, `PathHelper`, and catalog constants.
- `Identity/` - JWT and credential-related implementations.
- `Time/` - system clock implementation.
- `Settings/` - Infrastructure settings types.
- `DependencyInjection.cs` - registers Infrastructure services and enabled-era catalogs.

## AC15 Notes

- AC15 game data is read from each era's configured `GameDataPath`, normally `wwwroot/data/<era>/data`.
- Green loads `config/S11100-1/musicinfo.xml`, `config/S11100-1/musicmedleyinfo.xml`, and `fumen/tuning.bin`.
- Blue loads `config/S10100-1/musicinfo.xml`, `config/S10100-1/musicmedleyinfo.xml`, and `fumen/tuning.bin`.
- Yellow loads `config/ST9100-1/musicinfo.xml`, `config/ST9100-1/musicmedleyinfo.xml`, `config/ST9100-1/defmusic.bin`, and `fumen/tuning.bin`.
- Red loads `config/ST8100-1/musicinfo.xml`, `config/ST8100-1/musicmedleyinfo.xml`, `config/ST8100-1/defmusic.bin`, and `fumen/tuning.bin`.
- White loads `config/ST7100-1/musicinfo.xml`, `config/ST7100-1/musicmedleyinfo.xml`, `config/ST7100-1/defmusic.bin`, `config/ST7100-1/present.xml`, `config/ST7100-1/spacialbaid.xml`, and `fumen/tuning.bin`.
- Green, Blue, and Yellow item-shop JSON use the shared AC15 item-shop loader, including string-first enum values with numeric compatibility at the loader edge.
- Red and White Don Challenge JSON use the shared AC15 Don Challenge loader and feed era-owned progress/readback state.

## Era-Specific Notes

- `BlueEraGameDataCatalog` loads normal catalog data, optional JSON catalogs, item-shop data, customization data, and battle catalog data.
- Blue battle availability is enabled only when the five battle XML files under `config/S10100-1/battle` are present and parseable.
- Blue customization catalogs can be generated from Blue AC15 data when `AutoExtractCatalog` is enabled.
- Blue Tokkun persistence belongs to `UserSaveDataBlue` and `BlueTokkunStageResult`; do not add Banacoin wallet/payment tables for Tokkun compatibility.
- Blue battle persistence belongs to BlueBattle entities and migrations; do not reuse Green AI Battle state.
- White customization catalogs can be generated from White AC15 data when `AutoExtractCatalog` is enabled, with shared or override display-name data.
- White Tokkun persistence belongs to `UserSaveDataWhite` and `WhiteTokkunStageResult`; White Banacoin compatibility remains stateless.

## When To Add Code Here

- Add a concrete implementation for an Application port.
- Add EF Core entity configuration or a migration.
- Add a filesystem loader for operator or game data.
- Add settings implementation code tied to persistence, identity, or catalog I/O.

Do not add port interfaces, HTTP controllers, AdminApi DTOs, or domain-only entities here.
