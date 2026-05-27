# Blue A2 Catalog And Data Layout Spec

**Date:** 2026-05-28
**Status:** approved design; ready for implementation plan after written-spec review
**Scope:** Define Blue normal catalog interfaces, data paths, loader reuse, settings, docs, and verification. This stage does not implement Blue gameplay handlers or battle semantics.

## Purpose

Stage A2 gives later Blue normal-support stages a stable catalog surface. A1
proved the Blue adapter and route skeleton; A2 should make Blue a first-class
data era in the same catalog multiplexer used by Nijiiro and Green.

The target is all normal catalog data surfaces the server already models for
Green: music metadata, song hash version, tuning/star metadata, Dani/taikojuku
packs, item shop seasons, event folders, telops, recommended songs, attract
movies, customization catalogs, and current empty gacha/tournament surfaces.

Raw Blue files with no current server catalog owner, such as `defmusic.bin`,
`present.xml`, `spacialbaid.xml`, and `waiwaiconfig.xml`, should be documented
as observed source files but not parsed in A2. Their semantics belong to later
identity, reward, unlock, or cabinet-hardening stages. Raw Blue battle files
under `config/S10100-1/battle` are explicitly Track B.

## Evidence Inputs

- Roadmap: `docs/superpowers/specs/2026-05-27-blue-support-roadmap-design.md`
- A0 evidence: `docs/superpowers/specs/2026-05-27-blue-a0-evidence-bootstrap-design.md`
- A1 adapter skeleton: `docs/superpowers/specs/2026-05-27-blue-a1-era-foundation-adapter-skeleton-design.md`
- Blue local data symlink: `Host/wwwroot/data/blue/data`
- Green catalog pattern:
  - `Application/Abstractions/IGreenCatalog.cs`
  - `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`
  - `Infrastructure/GameDataCatalog/Green/GreenGameDataPaths.cs`
  - `Infrastructure/GameDataCatalog/Green/GreenRequiredDataFiles.cs`
  - `Infrastructure/DependencyInjection.cs`

Local Blue data uses the same AC15-style XML layout as Green with a different
release directory:

- Blue: `wwwroot/data/blue/data/config/S10100-1`
- Green: `wwwroot/data/green/data/config/S11100-1`

Observed Blue normal source files include:

- `config/S10100-1/musicinfo.xml`
- `config/S10100-1/musicmedleyinfo.xml`
- `config/S10100-1/defmusic.bin`
- `config/S10100-1/present.xml`
- `config/S10100-1/spacialbaid.xml`
- `config/S10100-1/waiwaiconfig.xml`
- `fumen/tuning.bin`
- `fumen/tuning_ext.bin`
- `movie/attract_cm_*.pam`
- `nutdata/S10100-1/appendable/...` customization textures

Observed Blue battle source files include `config/S10100-1/battle/*`; A2 must
not parse or expose them.

## Decisions

| Topic | Decision |
|---|---|
| Catalog boundary | Add `IBlueCatalog : IEraGameDataCatalog` and `BlueEraGameDataCatalog` instead of sharing `IGreenCatalog`. |
| Parser reuse | Extract shared AC15 parsing for matched file formats, but keep era-specific catalog records and facades. |
| Data root | Blue runtime data lives under `wwwroot/data/blue/data`, normally as a symlink or copy of the game's `USRDIR/data`. |
| Release directory | Blue required config files come from `config/S10100-1`; do not infer Green's `S11100-1`. |
| Required files | `musicinfo.xml`, `musicmedleyinfo.xml`, and `fumen/tuning.bin` are required when Blue is enabled. |
| Optional files | Blue JSON catalogs default to empty or discovery behavior unless their setting requires them. |
| Item shop | Blue item shop uses Blue-owned JSON and the existing AC15 shop contract; it is required only when Blue shop is enabled. |
| Customization | Blue costume/title/tone catalogs use Blue-owned JSON names. Auto-extraction can be generalized from the Green AC15 extractor if it remains read-only. |
| Battle | Battle data paths are documented only. Track B owns parsing and catalog surfaces for battle. |

## Catalog Boundary

A2 adds:

```text
Application/
  Abstractions/
    IBlueCatalog.cs
  Catalog/
    Ac15/
      Ac15StarSet.cs
    Blue/
      BlueMusicInfoEntry.cs
      BlueTaikojukuEntry.cs
      BlueItemShopCatalog.cs
      BlueItemShopSeason.cs
      BlueItemShopEntry.cs
      BlueRecommendEntry.cs
      BlueTelopEntry.cs
      BlueGachaEntry.cs
      BlueTournamentEntry.cs

Infrastructure/
  GameDataCatalog/
    Ac15/
      shared format-level loaders
    Blue/
      BlueEraGameDataCatalog.cs
      BlueGameDataPaths.cs
      BlueRequiredDataFiles.cs
      Blue*Loader.cs wrappers where needed
```

`IBlueCatalog` should expose the same normal-data concepts Green exposes today:

- `MusicInfoFileOrder`
- `SongHashVersion`
- `BlueMusicInfos`
- `TaikojukuFileOrder`
- `Taikojuku`
- `ItemShopCatalog`
- `ItemShop`
- `EventFolders`
- `Telops`
- `Gachas`
- `Tournaments`
- `Recommend`
- `Movies`
- `GetCostumeList()`
- `GetTitleDictionary()`
- `GetNeiroDictionary()`

It also implements the shared `IEraGameDataCatalog.MusicInfos` dictionary as
`IReadOnlyDictionary<uint, IMusicInfoEntry>`, mapped from the Blue music-info
records. Later Blue handlers should depend on `IBlueCatalog` or
`IGameDataCatalog.For(GameEra.Blue)` and should not cast to or depend on
`IGreenCatalog`.

## Data Layout

Blue's source and operator-edited data layout is:

```text
wwwroot/data/blue/
  data/                         # operator-owned USRDIR/data symlink or copy
    config/S10100-1/
      musicinfo.xml             # required
      musicmedleyinfo.xml       # required
      defmusic.bin              # observed, deferred
      present.xml               # observed, deferred
      spacialbaid.xml           # observed, deferred
      waiwaiconfig.xml          # observed, deferred
      battle/                   # observed, Track B only
    fumen/
      tuning.bin                # required
      tuning_ext.bin            # observed, optional/deferred
    movie/
      attract_cm_*.pam          # optional, discovered for startup movie data
  blue_event_folder_data.json   # optional, empty if absent
  blue_item_shop_data.json      # required only when Blue shop is enabled
  blue_recommend_songs.json     # optional, empty/default if absent
  blue_telop_data.json          # optional, empty if absent
  blue_movie_data.json          # optional override; discovery fallback if absent
  blue_costume_data.json        # optional, empty if absent
  blue_title_data.json          # optional, empty if absent
  blue_neiro_data.json          # optional, empty if absent
```

`Host/wwwroot/data/blue/data` remains gitignored. The repository should contain
only source-controlled placeholders and operator-edited or generated JSON files
that are intentionally part of the server setup.

A2 should not rename existing Green files. Shared loaders must accept explicit
paths so Blue can use `blue_*` file names while Green keeps its current
`green_*`, `telop_data.json`, `recommend_songs.json`, and `movie_data.json`
contracts.

## Required And Optional Startup Behavior

When Blue is disabled, Blue data files must not affect server startup.

When Blue is enabled, `BlueRequiredDataFiles.ThrowIfMissing()` should fail fast
if any of these paths are absent:

- `wwwroot/data/blue/data/config/S10100-1/musicinfo.xml`
- `wwwroot/data/blue/data/config/S10100-1/musicmedleyinfo.xml`
- `wwwroot/data/blue/data/fumen/tuning.bin`

The exception message must say `Blue` and include the missing path, matching the
diagnostic style used by Green.

Optional behavior:

- Missing `blue_event_folder_data.json` means no event folders.
- Missing `blue_telop_data.json` means no telop rows.
- Missing `blue_recommend_songs.json` means no recommended songs.
- Missing Blue customization JSON files means empty customization catalogs
  unless generalized auto-extraction successfully creates them.
- Missing `blue_movie_data.json` falls back to discovery of nonzero
  `attract_cm_*.pam` files under `wwwroot/data/blue/data/movie`.
- Missing `blue_item_shop_data.json` is valid only when
  `ServerSettings:Eras:Blue:EnableShop` is `false`.

## Shared AC15 Loader Design

The shared extraction point is the file format, not the era semantics.

Recommended shared loaders:

- AC15 music-info XML loader:
  - input: explicit path to `musicinfo.xml`
  - output: neutral AC15 music rows plus `song_hash_ver`
  - consumers: Green maps to `GreenMusicInfoEntry`; Blue maps to `BlueMusicInfoEntry`
- AC15 music-medley XML loader:
  - input: explicit path to `musicmedleyinfo.xml`
  - output: neutral medley/Dani rows
  - consumers: Green maps to `GreenTaikojukuEntry`; Blue maps to `BlueTaikojukuEntry`
- AC15 tuning loader:
  - input: explicit path to `tuning.bin`
  - output: neutral star records keyed by music id
  - consumers: both era catalogs enrich their music-info entries
- AC15 JSON helpers:
  - telop, event folder, recommend, item shop, and movie config loaders should
    accept file paths, era names for error text, and catalog-song-id sets.

Current Green loader tests should continue to pass. If implementation renames
types or moves parsing code, keep compatibility wrappers or update Green call
sites in the same task so no Green behavior changes.

## Blue Catalog Initialization Flow

`BlueEraGameDataCatalog.InitializeAsync()` should:

1. Validate required files.
2. Optionally run generalized customization auto-extraction if configured and
   missing Blue customization JSON files can be created read-only from
   `ServerSettings:Eras:Blue:GameDataPath`.
3. Load AC15 music-info XML.
4. Load AC15 tuning data and enrich music rows with star values.
5. Load AC15 music-medley rows for Dani/taikojuku.
6. Build Blue music dictionaries by `SongNo` and shared `IMusicInfoEntry`.
7. Load item shop, event folder, telop, gacha, tournament, recommend, movie,
   costume, title, and tone catalogs.
8. Log one summary line with song count, `song_hash_ver`, medley count, tuning
   row count, customization counts, movie count, and item-shop enabled state.

The missing-tuning warning should mirror Green's behavior: warn when normal
music-info rows have no tuning record, but do not warn for medley-only songs
that are not expected in `tuning.bin`.

## Settings

`EraSettings` can remain shared, but A2 must stop relying on Green defaults for
Blue. Shipped `ServerSettings:Eras:Blue` should include explicit values:

```json
"Blue": {
  "Enabled": false,
  "AutoExtractCatalog": true,
  "GameDataPath": "wwwroot/data/blue/data",
  "CustomizationNameDataPath": "",
  "EnableShop": false,
  "ActiveShopSeasonId": null
}
```

If Blue is enabled, options validation should require
`ServerSettings:Eras:Blue:EnableShop` to be present. If Blue shop is enabled,
`ServerSettings:Eras:Blue:ActiveShopSeasonId` must be a nonzero season id. This
should be generalized without weakening Green's existing validation messages or
coverage.

If implementation needs a safer model than Green's current `EraSettings`
default `GameDataPath = "wwwroot/data/green/data"`, prefer removing the
Green-specific default or replacing it with explicit shipped settings and
validation. Do not let missing Blue configuration silently resolve to the Green
data path.

## Documentation

A2 should update operator docs in `README.md` and `Host/README.md` with:

- Blue required data paths.
- Blue symlink/copy command examples pointing to `wwwroot/data/blue/data`.
- The `S10100-1` release directory name.
- Blue optional JSON file names and default behavior.
- Blue item-shop settings and `blue_item_shop_data.json` rules.
- A note that `config/S10100-1/battle` exists but is reserved for Track B.

Docs should not imply that A2 makes card login, play result persistence, self
best, crowns, rewards, Dani completion, AdminApi, WebUI, or battle playable.

## Testing And Verification

The implementation plan should use focused, test-first checks:

- Shared AC15 loader tests with small committed fixtures for:
  - music-info version/file-order parsing
  - music-medley/Dani pack parsing
  - tuning star parsing, including non-stock song counts
- Blue local-data smoke tests guarded by file existence for:
  - `Host/wwwroot/data/blue/data/config/S10100-1/musicinfo.xml`
  - `Host/wwwroot/data/blue/data/config/S10100-1/musicmedleyinfo.xml`
  - `Host/wwwroot/data/blue/data/fumen/tuning.bin`
- `BlueCatalogLoaderTests` proving:
  - Blue first song/file order can load from local data when present
  - Blue Dani packs load from `musicmedleyinfo.xml`
  - tuning enrichment populates star values
  - missing required files fail with Blue-specific messages
  - optional JSON files can be absent without failing startup
- item shop loader tests proving:
  - disabled missing file returns disabled catalog
  - enabled shop requires known active season
  - Blue data uses `blue_item_shop_data.json`
- settings validation tests proving:
  - enabled Blue requires explicit `EnableShop`
  - enabled Blue shop requires nonzero `ActiveShopSeasonId`
  - disabled Blue does not require local game files
- DI/catalog multiplex tests proving:
  - `IGameDataCatalog.For(GameEra.Blue)` resolves when Blue is enabled
  - it throws the existing disabled-era error when Blue is disabled
- docs/config checks proving shipped settings and docs mention the Blue data
  root and required files.

Build verification should include:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~Blue
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~GreenCatalog
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-a2"
```

If normal Host output is locked, use the temp output build path above.

## Acceptance Criteria

- `IBlueCatalog` exists and exposes Blue normal catalog surfaces without
  depending on `IGreenCatalog`.
- `BlueEraGameDataCatalog` is registered only when Blue is enabled.
- `IGameDataCatalog.For(GameEra.Blue)` returns the Blue catalog when enabled.
- Blue required data paths point to `wwwroot/data/blue/data/config/S10100-1`
  and `wwwroot/data/blue/data/fumen/tuning.bin`.
- Blue catalog initialization fails fast for missing required files and ignores
  missing optional JSON files according to the rules above.
- Shared AC15 parsing removes meaningful duplication without changing Green
  catalog behavior.
- Shipped settings include explicit Blue data/shop values.
- Operator docs describe how to supply Blue data and which raw files remain
  deferred.
- No battle catalog parsing or battle handler semantics are added in A2.

## Out Of Scope

- Blue card registration, identity persistence, default save state, and
  `userdata.php` behavior.
- Blue play-result persistence, self-best, crowns, and rewards.
- Blue Dani completion logic.
- Blue item purchase behavior beyond loading the item-shop catalog.
- Blue AdminApi or WebUI pages.
- Blue battle mode and every file under `config/S10100-1/battle`.
- Tokkun behavior.
- Banacoin balance or payment behavior.
- Cabinet smoke hardening beyond catalog startup and route-independent data
  validation.

## Handoff To A3 And Later

A3 can depend on `IBlueCatalog` for Blue music, customization, and item unlock
lookup while designing card/profile/userdata behavior. A4 can use the same
music and tuning surfaces for play-result persistence, self-best, crowns, and
reward arrays. A5 can use Blue taikojuku file-order and dictionary data for
Dani. A6 can use `BlueItemShopCatalog` for shop advertisement and purchases.
Track B must create a separate battle catalog design before using
`config/S10100-1/battle/*`.
