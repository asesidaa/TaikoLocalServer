# Green Customization Design

Date: 2026-05-17

## Context

Nijiiro's admin WebUI lets operators view and edit player customization: the 5 costume slots (kigurumi / head / body / face / puchi), title text + title-plate id, neiro (tone), and body / face / limb colors. The pipeline is:

- `Domain/Entities/UserSaveDataNijiiro` persists currents + per-slot unlocked `List<uint>` and `TitleFlgArray` / `ToneFlgArray`.
- `Infrastructure/GameDataCatalog/Nijiiro/NijiiroEraGameDataCatalog` loads `don_cos_reward.bin`, `shougou.bin`, `neiro.bin`, `wordlist.bin`, `locked_*_data.json` into in-memory dictionaries.
- `Adapters.AdminApi/Controllers/UserSettingsController` (legacy `/api/usersettings/{baid}` route only) reads / writes the `UserSetting` ViewModel in `Contracts.AdminApi/ViewModels/UserSetting.cs`.
- `TaikoWebUI/Pages/Profile.razor` hard-codes the Nijiiro pickers.

Green has the schema for all of this — `UserSaveDataGreen` already exposes `Costume1..5`, `CostumeFlg1..5` (`byte[]` bitsets), `ToneFlg`, `TitleFlg`, `TitleplateId`, and color fields, pre-seeded with empty bitsets in `UserSaveDataGreenExtensions.CreateDefaultGreenSaveData()`. What is missing:

- No Green catalog data on disk for costumes, titles, or neiro.
- `UserSettingsController` is Nijiiro-only — no era split.
- `Profile.razor:31-33` is a TODO stub for Green customization controls.
- `BaidQuery.Green` surfaces `CostumeData` and `CostumeFlg1..5` to the running game but does not yet surface `TitleFlg` or `ToneFlg`, so unlocks set via the WebUI would not be visible in-game even if the WebUI existed.

The intended source of catalog data is the operator's live PS3 install (already symlinked into `Host/wwwroot/data/green/data` per the current setup). On disc:

- `nutdata/{cos,title,tone}_name/nutdatapack.ndp` — `NUT_PACK_TYPE1` containers of pre-rendered Japanese name images, one entry per id. Authoritative id enumeration per category.
- `don3d/cos/cos_NNNNNN.{nud,nut}` — 128 costume model pairs. Cross-check for cos id range.
- `don3d/{face,parts/{acc,body,head,paint},full/{cos,face}}` — per-slot costume assets; the mapping of these directories to the 5 `CostumeN` fields lives in EBOOT.ELF code, not on disc.
- `config/S11100-1/rewardtitlefiltering.xml` — boost::serialization XML listing 309 title ids that are usable as rewards. A subset filter, not the full title catalog.
- Names (Japanese text strings) are not on disc as text. They live in EBOOT.ELF string tables and on community wikis such as the taiko-fumen value-order page. The `.nut` files inside `nutdatapack.ndp` contain the rendered text as images, not strings.

No public parser exists for `NUT_PACK_TYPE1`, `nutdatapack`, or the Green boost::serialization XMLs (GitHub code search returns zero hits for those magic strings). Both parsers are small enough to implement: `NUT_PACK_TYPE1` is magic + boost::serialization header + per-entry `{length-prefixed filename, padding, offset, size}`, and the boost XML cases we touch are flat lists.

The design must also account for further older eras being added later, each with similar customization surfaces but different on-disc layouts.

## Goals

- Parse the Green on-disc catalog for costumes, titles, and neiro into JSON files that the server can read at runtime, mod-friendly.
- Auto-bootstrap the extraction on first server start when the catalog JSONs are missing and a Green game-data tree is present.
- Expose era-aware admin REST endpoints for reading / writing Green user customization, alongside the existing Nijiiro endpoints (legacy route preserved).
- Surface `TitleFlg` and `ToneFlg` in `BaidQuery.Green` so WebUI-set unlocks become visible to the running game.
- Refactor `Profile.razor`'s customization controls into reusable, era-parameterized Razor components, so older eras can plug into the same WebUI shape with minimal additions.
- Degrade gracefully when catalog data, game data, or names are missing.

## Non-Goals

- Do not OCR the `.nut` name images.
- Do not parse `.ddp` containers, `.nud` 3D models, or `.nut` textures.
- Do not extract or translate names to English. Japanese only.
- Do not modify any file under the operator's game-data tree. The extractor is read-only over `--game-data`.
- Do not introduce per-era Razor pages or duplicated controller hierarchies. Era variance is expressed through parameters, partial files, and the existing `DanBestDataController`-style dispatcher pattern.
- Do not block server startup when catalogs are missing. Pickers hide gracefully.
- Do not add WebUI-level component tests in this PR. The project has no Razor test infrastructure today.

## Architecture

Five layers, each era-parameterized so older eras drop in cheaply:

```
GreenCatalogExtractor (CLI)  →  Green catalog JSONs (mod-friendly, gitignored)
                                            ↓
Infrastructure/GameDataCatalog/Green        →  Loaders + IGreenCatalog
                                            ↓
Contracts.AdminApi                           →  Costume / Title / Neiro made `partial`
                                            ↓
Adapters.AdminApi/UserSettingsController     →  Dual route: legacy + /api/{era}/...
                                                Per-era partials, BitsetCodec adapter
                                            ↓
TaikoWebUI/Shared/Customize/*.razor          →  Era-parameterized pickers
                                                Profile.razor composes per era
                                            ↓ side change
Application/Handlers/BaidQuery.Green         →  Surface TitleFlg + ToneFlg
```

## Extractor

A new project at the repo root:

- `GreenCatalogExtractor/` — System.CommandLine CLI, mirrors `LocalSaveModScoreMigrator`.
- Core logic lives under `Infrastructure/GameDataCatalog/Green/Extractor/` so the same code is callable from `GreenEraGameDataCatalog.InitializeAsync()` for the first-run auto-bootstrap. The CLI is a thin Program.cs that references `Infrastructure` and invokes the same public façade.

### Layout

```
GreenCatalogExtractor/
  GreenCatalogExtractor.csproj
  Program.cs

Infrastructure/GameDataCatalog/Green/Extractor/
  GreenCatalogExtractor.cs          # public façade: ExtractAsync(options, ct)
  GreenExtractorOptions.cs
  NdpReader.cs                      # NUT_PACK_TYPE1 container parser
  BoostXmlReader.cs                 # boost::serialization flat-list reader
  Don3dDirScanner.cs                # don3d/{cos,parts/*,face,full/*} enumeration
  Sources/
    EbootStringResolver.cs          # optional, no-op if EBOOT not provided
    WikiScraper.cs                  # optional, no-op unless --wiki
    OverridesLoader.cs              # operator-curated JSON
  Merging/
    CostumeMerger.cs
    TitleMerger.cs
    NeiroMerger.cs
  Output/CatalogWriter.cs           # deterministic JSON output
```

### CLI

```bash
GreenCatalogExtractor extract \
  --game-data <USRDIR/data>                   # default: Host/wwwroot/data/green/data
  --eboot <EBOOT.ELF or .i64>                 # optional, enables EBOOT name resolution
  --wiki                                       # optional, enables wiki fallback
  --overrides <overrides.json>                # optional, operator overrides
  --out <output-dir>                          # default: Host/wwwroot/data/green/
```

### Output schema

JSON envelope per category, items reuse `Contracts.AdminApi/ViewModels/{Costume,Title,Neiro}` (marked `partial` so Green-only metadata can extend them in a sibling partial without forking the type):

```json
{ "schemaVersion": 1, "items": [ /* Costume[] | Title[] | Neiro[] */ ] }
```

Files written to `<out>/green_{costume,title,neiro}_data.json`. Items sorted by id ascending. File overwritten in place; safe to re-run.

Field reuse:

| Existing field | Green semantics |
|---|---|
| `Costume.CostumeId` | on-disc id from `.ndp` / `don3d/...` |
| `Costume.CostumeType` | slot — reuse the Nijiiro vocabulary (`kigurumi`/`head`/`body`/`face`/`puchi`); `"unknown"` until Phase 2 IDA resolves the directory mapping |
| `Title.TitleId` / `TitleName` / `TitleRarity` | name nullable; `TitleRarity` defaults to `0` (concept may not apply) |
| `Neiro.NeiroId` / name | straight reuse |

Per-item provenance (`"source": "ndp+overrides"` etc.) lives in the Green partial of `Costume` / `Title` / `Neiro`, not the shared shape.

### Phasing

Each phase is shippable on its own. The WebUI and API tracks do not block on any extractor phase.

| Phase | What runs | Inputs needed | Output quality |
|---|---|---|---|
| **1 — Ids only** | `NdpReader` + `Don3dDirScanner` + `BoostXmlReader` | Mounted disc tree only | All ids enumerated; names null; some costume slots `"unknown"` |
| **2 — Slot mapping** | + IDA against `EBOOT.ELF.codex.i64` to find the `CostumeN → directory` loader | EBOOT.ELF | All ids correctly slotted; names still null |
| **3 — Names** | + `WikiScraper` and / or `EbootStringResolver` + operator overrides | Wiki access or EBOOT strings | Names populated; nulls only for items neither source covers |

Phase 3 name-source priority on conflict: operator overrides → EBOOT strings → wiki. `module/taiko.sprx` and `module/libsmart.sprx` are not considered name sources; they are not original game code (`taiko.sprx` is the operator's modding module; `libsmart` is a peripheral library).

### First-run auto-bootstrap

In `GreenEraGameDataCatalog.InitializeAsync()`:

```
if (era == Green && Green enabled):
    if any of green_{costume,title,neiro}_data.json missing:
        if ServerSettings.Eras.Green.AutoExtractCatalog == true (default true):
            if game-data tree exists at ServerSettings.Eras.Green.GameDataPath:
                log info "Green catalog missing — running first-run extraction…"
                call GreenCatalogExtractor.ExtractAsync(defaults, ct)
                on success: continue with freshly-written JSONs
                on failure: log warning + reason, continue with empty catalog
            else:
                log warning explaining how to provide game data
        else:
            log info "auto-extract disabled; continuing with empty catalog"
    load whatever JSONs exist; missing files → empty list, no crash
```

The auto-bootstrap runs the same code as the CLI. Phase 2 / 3 features (EBOOT, wiki) are CLI-only opt-ins; first-run auto-extract is Phase 1 only.

### Non-extractor non-goals

OCR'ing `.nut` name images, unpacking `.ddp`, generating English translations, parsing fumen `.bin`, or any write back to the game tree.

## Server changes

### Infrastructure — `GreenEraGameDataCatalog` additions

New under `Infrastructure/GameDataCatalog/Green/`:

- `GreenCostumeLoader.cs` — reads `green_costume_data.json` → `List<Costume>`.
- `GreenTitleLoader.cs` — reads `green_title_data.json` → `Dictionary<uint, Title>`.
- `GreenNeiroLoader.cs` — reads `green_neiro_data.json` → `Dictionary<uint, Neiro>`.

`GreenEraGameDataCatalog` already implements `IGreenCatalog`; it gains the parallel accessors Nijiiro exposes (`GetCostumeList()`, `GetTitleDictionary()`, `GetNeiroDictionary()`). `InitializeAsync()` is the auto-bootstrap orchestration point above.

Missing-file fallback: empty collections + a warning log. Loaders never throw on absent files; format errors throw and abort startup.

### Contracts.AdminApi — minimal additions

- Mark `Costume`, `Title`, `Neiro` `partial`. No field changes required for v1.
- A new Green partial of each (under `Contracts.AdminApi/ServerData/Green/`) carries the optional `Source` provenance string used by the extractor. The WebUI ignores it.
- Add `List<uint> UnlockedTone` to `UserSetting`. The Nijiiro path populates it from its existing `ToneFlgArray` (currently never surfaced to the WebUI); the Green path populates it via `BitsetCodec` from `ToneFlg`. All other `UserSetting` fields already cover Green's needs — the existing `List<uint>` unlock fields and current-equipped uints map one-to-one to the byte[]-bitset translation.

### Adapters.AdminApi — `UserSettingsController` refactor

Refactor to the `DanBestDataController` template:

```csharp
[Route("api/[controller]")]
[HttpGet ("{baid}")]                        // legacy → Nijiiro
[HttpPost("{baid}")]
[HttpGet ("/api/{era}/[controller]/{baid}")] // era-aware
[HttpPost("/api/{era}/[controller]/{baid}")]
```

Era dispatch uses `EraRoute.TryParse`. Per-era partials hold the bodies:

- `UserSettingsController.Nijiiro.cs` — existing logic, moved verbatim.
- `UserSettingsController.Green.cs` — new. Reads / writes `UserSaveDataGreen` via `BitsetCodec` translation:
  - `CostumeFlg1..5` ↔ `UnlockedKigurumi / Head / Body / Face / Puchi`
  - `TitleFlg` ↔ `UnlockedTitle`
  - `ToneFlg` ↔ `UnlockedTone`

`enforceUnlockedOnly` clamp behaves identically to Nijiiro: on save, equipped ids are clamped to the unlocked set before persistence.

### `BitsetCodec`

New `Application/Common/BitsetCodec.cs`. Pure-CPU byte[] ↔ `List<uint>` translation, sized by `GreenProtocolBytes.{CostumeFlagBytes, TitleFlagBytes, ToneFlagBytes}` or any byte length. Shared so future eras using bitset storage can reuse it without duplicating logic.

### `BaidQuery.Green`

Add `TitleFlg` and `ToneFlg` to the `CommonBaidResponse` extraction in `Application/Handlers/BaidQuery.Green.cs` so unlocks set via the WebUI become visible to the running game. Uses `GreenProtocolBytes.FixedOrZero` for the same fixed-width padding pattern already applied to `CostumeFlg1..5`.

### WebUI catalog endpoints

New `Adapters.AdminApi/Controllers/CustomizationCatalogController.cs`, kept separate from `UserSettingsController` for single-responsibility:

```
GET /api/{era}/customization/costumes  → List<Costume>
GET /api/{era}/customization/titles    → Dictionary<uint, Title>
GET /api/{era}/customization/neiros    → Dictionary<uint, Neiro>
```

Pure catalog reads off `IEraGameDataCatalog.For(era).As<I{Nijiiro|Green}Catalog>()`. No mediator. `[AuthorizeIfRequired]` applied.

### Settings

Add to `Host/Configurations/ServerSettings.json` under `Eras.Green`:

```jsonc
"Green": {
  "Enabled": true,
  "AutoExtractCatalog": true,
  "GameDataPath": "wwwroot/data/green/data"
}
```

Bound through the existing `ServerSettings` partial pattern. Defaults preserved; existing configs continue to work.

## WebUI changes

### Component decomposition

Extract four era-agnostic components under `TaikoWebUI/Shared/Customize/`:

- `CostumePicker.razor` — one slot. Dropdown of unlocked + checklist of locked.
- `TitlePicker.razor` — title text + plate-id selector + title unlock list.
- `NeiroPicker.razor` — tone selector + unlock list.
- `ColorPicker.razor` — body / face / limb triple.

Each component takes a uniform parameter set:

- `Era` (string) — used for telemetry / labels only; components do not branch on it.
- `Catalog` — the era's catalog slice (`IReadOnlyList<Costume>` filtered to the slot, `IReadOnlyDictionary<uint, Title>`, etc.).
- `Value` — a WebUI-local record holding `(currentId, unlockedIds)`.
- `ValueChanged` — `EventCallback<TValue>`.
- `ReadOnly` — toggles disabled state.

Era-agnostic UI rules:

- Catalog item with name `null` → render as `"#{id:D3}"` placeholder. Covers Phase 1 extractor output and any name-source gaps.
- Catalog item with `CostumeType == "unknown"` → render in an "Unsorted" group at the bottom of the slot picker. Covers pre-Phase-2 slot mapping.
- Catalog absent (empty list) → component renders nothing; Profile.razor skips the section entirely.

### Profile.razor

Both `@page` routes (`/Users/{baid:int}/Profile` and `/Users/{baid:int}/{era}/Profile`) flow to the same component. The component:

1. On `OnParametersSetAsync`: fetches `WebUiEra.Api(CurrentEra, "usersettings/{baid}")` → `UserSetting`, plus the era's three catalogs.
2. Decomposes `UserSetting` into per-picker value records.
3. Renders pickers via a per-era composition function. The default composition serves Nijiiro and Green; future eras can override only what differs.

The `Profile.razor:31-33` TODO stub is removed.

### Routing housekeeping

- Profile.razor's API calls migrate from hard-coded `api/usersettings/...` to `WebUiEra.Api(CurrentEra, "usersettings/...")`.
- Nijiiro continues to work via the legacy `/api/usersettings/{baid}` route during the transition. `UserSettingsController` keeps both.
- No new pages.

## Tests

Tests live under `Tests/Green/`, mirroring the existing convention. The implementation phase decides class names, fixture shape, and how to slice cases. The coverage areas to hit:

- The `NUT_PACK_TYPE1` parser, including a small hand-crafted blob and an opportunistic round-trip against the real on-disc `nutdatapack.ndp` files when available.
- The boost::serialization XML reader for the flat-list cases we actually touch (e.g. `rewardtitlefiltering.xml`).
- `don3d/` directory scanning over a synthetic tree.
- Catalog merging: precedence between sources, provenance recording, and the `"unknown"` slot fallback.
- Catalog writer output: deterministic, sorted, stable JSON.
- Catalog loaders: JSON → typed; missing file → empty + warning, never throws on absence.
- `GreenEraGameDataCatalog` bootstrap orchestration: missing JSON + auto-extract disabled → empty; missing JSON + auto-extract enabled + synthetic game tree → extractor invoked once and JSONs land.
- `BitsetCodec` round-trips for the costume / title / tone bitset sizes, including boundary ids (0, last bit, beyond capacity).
- `UserSettingsController` Green path: GET decodes unlocks; POST persists and clamps to unlocked; legacy `/api/usersettings/{baid}` continues to serve Nijiiro.
- `BaidQuery.Green` surfaces `TitleFlg` and `ToneFlg` in `CommonBaidResponse`.

No WebUI / Razor component tests. The project has no Razor test infrastructure today and this PR does not add one.

## Rollout

- No EF migration required. All persisted fields already exist on `UserSaveDataGreen`.
- No config migration required. New settings have safe defaults.
- Catalogs are gitignored under `Host/wwwroot/data/green/green_*_data.json`. Add to `.gitignore` next to the existing `wwwroot/data/*/datatable/` rule.
- Operator playbook (single new paragraph in `Host/README.md`):
  1. Symlink or copy your Green install's `USRDIR/data/` to `Host/wwwroot/data/green/data/`.
  2. Start the server once. First-run auto-extract populates `green_*_data.json`.
  3. Optional: re-run `dotnet run --project GreenCatalogExtractor -- extract --eboot … --wiki` to enrich names after the first start.
  4. WebUI customization tab is available immediately for any Baid that has Green save data.

### Ship phasing

| Ship | Includes |
|---|---|
| **v1 (this work)** | Phase 1 extractor (ids only, names null, slots best-effort), server refactor + `BitsetCodec` + `CustomizationCatalogController` + `BaidQuery.Green` `TitleFlg` / `ToneFlg`, WebUI components + composition. Operator gets functional unlock / equip UI with placeholder names. |
| **v1.x (follow-up)** | Phase 2 IDA work for slot mapping; Phase 3 EBOOT / wiki name resolution. Pure extractor changes; no server or WebUI changes needed — operator re-runs the CLI, JSONs refresh, WebUI reads new names on next request. |
| **vN (future eras)** | New era plugs into the same `UserSettingsController` dispatcher, reuses `BitsetCodec`, reuses pickers; era-specific extractor module under `Infrastructure/GameDataCatalog/<Era>/Extractor/`. |
