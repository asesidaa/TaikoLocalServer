# Green Event Folder Support - Design

**Date:** 2026-05-27
**Branch:** `feat/green-version-support`
**Scope:** Add data-driven normal Green event-folder support using the official
`.tools/featureboard.bin` cache as the default source artifact.

## Problem

Green has generated protocol types and application DTOs for event folders, but
the current Green path is still stubbed:

- `GetFolderController` returns `Result = 1` without folder rows.
- `GetFolderQuery.Green` logs a stub and returns no rows.
- `GetInitialDataQuery.Green` does not advertise
  `AryGreenEventFolderDatas`.
- `GreenEventFolderLoader` returns an empty dictionary.
- `IGreenCatalog.EventFolders` currently exposes a Green-only entry type that
  only contains `FolderId` and `Name`.

The client therefore never receives normal feature-board folders. The official
runtime evidence is the `.tools/featureboard.bin` cache supplied for this work.
It contains real folder membership, but its ids are the client's cached internal
ids, not server-facing protocol ids.

## Goals

- Generate committed Green event-folder JSON from `.tools/featureboard.bin`.
- Keep runtime folder support data-driven: if the JSON is missing, no Green
  event folders are enabled.
- Use protocol folder ids in runtime JSON and responses.
- Advertise enabled folders through Green `initialdatacheck.php`.
- Serve requested folder rows through Green `getfolder.php`.
- Share the common folder lookup path with Nijiiro where the data model matches.
- Validate Green folder data strictly enough that advertised folders cannot
  reference unknown songs or missing texture indices.

## Non-goals

- No server setting such as `EnableEventFolders`.
- No folder names, themes, or display labels in runtime data.
- No runtime parsing of `.tools/featureboard.bin`.
- No public-source reconstruction of folder membership.
- No AI Battle task-board support in this feature. Folder ids `200..205` from
  `localaitaskboard.xml` are a separate Green system.
- No support for normal feature-board protocol ids above `15` without new
  texture and client evidence.

## Client Evidence

IDA evidence from `.tools/ida-snap/EBOOT.ELF.codex.i64` established the normal
Green feature-board id rules:

- `game::net::OnFeatureBoardDataResponse(const GetfolderResponse&)` accepts a
  response folder id only when `folder_id != 0 && folder_id <= 0xF`.
- The client stores accepted ids internally as `folder_id - 1`.
- `game::net::OnFeatureBoardDataRequest(GetfolderRequest&)` sends queued
  protocol folder ids.
- Feature-board genre entries are type `12` and carry the stored 0-based id.
- Texture binding formats use the 0-based suffix:
  - `feature_board_yoko_%02d`
  - `feature_board_tate_%02d`
  - `feature_board_image_%02d`
  - `feature_board_icon_%02d`

Therefore server protocol `folderId = N` maps to texture suffix `N - 1`, and
cache id `C` from `.tools/featureboard.bin` maps to protocol `folderId = C + 1`.

Local Green data has complete normal feature-board texture banks for
`S11100-1/appendable/00` ids `1..15`. The official cache rows use protocol ids
`1,2,3,4,5,6,7,8,11`, which are in range for that texture bank.

## Default Data Source

`.tools/featureboard.bin` is treated as the source artifact for the default
runtime data. The server does not read `.tools` at runtime.

The cache parse shape is:

- Boost binary archive metadata before offset `0x32`.
- Repeated rows:
  - big-endian `uint32 cache_id`
  - big-endian `uint32 song_count`
  - `song_count` big-endian `uint32 song_no` values

The default cache currently contains 9 rows. All parsed songs exist in
`Host/wwwroot/data/green/data/config/S11100-1/musicinfo.xml`.

## Runtime Data File

Add a committed JSON file:

`Host/wwwroot/data/green/green_event_folder_data.json`

Runtime JSON is intentionally minimal:

```json
[
  {
    "folderId": 1,
    "verupNo": 1,
    "songNo": [877, 876, 873]
  }
]
```

Rules:

- `folderId` is the server-facing protocol id.
- `folderId = cache_id + 1` when generated from `.tools/featureboard.bin`.
- `verupNo` is explicit cache-invalidation data. It may be `0`, but it must be
  present in JSON.
- Operators should bump a folder row's `verupNo` when changing that row's song
  list.
- `songNo` contains Green `song_no` values.
- Names, themes, `cacheId`, source paths, confidence, and provenance fields stay
  out of runtime JSON.
- Row order is not semantically important because the client indexes by
  `folderId`. The generator may preserve cache order for traceability, but
  handlers must use `folderId` lookup as the contract.

## Generation Tool

Add a small one-off script under `.tools`, for example:

`.tools/parse_featureboard.py`

The script reads `.tools/featureboard.bin` and writes or prints the JSON rows.
It should:

- parse from offset `0x32`;
- read big-endian row fields;
- convert each `cache_id` to `folderId = cache_id + 1`;
- emit `verupNo` as an explicit field for every row, using `1` for the default
  generated data because the cache does not expose a proven server-side version
  stamp;
- preserve the cache song order inside each row;
- fail if parsing does not consume the full file.

The script exists so older or alternate feature-board caches can be regenerated
later. The committed JSON remains the runtime input.

## Shared Folder Model

Green should use `Application.ServerData.EventFolderData` for runtime folder
rows instead of the current Green-only `GreenEventFolderEntry` model. The shared
model already contains the fields Green needs:

- `FolderId`
- `VerupNo`
- `SongNoes`

The Nijiiro-only fields `Priority` and `ParentFolderId` remain harmless defaults
for Green. Green runtime JSON does not need to include them.

Update `IGreenCatalog.EventFolders` and `GreenEraGameDataCatalog` to expose:

```csharp
IReadOnlyDictionary<uint, EventFolderData> EventFolders
```

The implementation can retire `GreenEventFolderEntry` if no other code needs it.

## Loader Behavior

`GreenEventFolderLoader` should load:

`Host/wwwroot/data/green/green_event_folder_data.json`

Missing file behavior:

- Missing file loads an empty dictionary.
- Empty dictionary means Green event folders are disabled by data.

Present file behavior:

- Present but malformed JSON fails catalog initialization with
  `InvalidDataException`.
- The loader should deserialize into an internal raw type with nullable fields,
  not directly into `EventFolderData`, so it can distinguish missing `verupNo`
  from explicit `verupNo: 0`.

Validation:

- `folderId` must be present and in `1..15`.
- Duplicate `folderId` values fail.
- `verupNo` must be present; `0` is valid.
- `songNo` must be present and contain at least one song.
- Every song id must be `< 1024`.
- Every song id must exist in `green.GreenMusicInfos`.

`GreenEraGameDataCatalog` should call the loader after Green music info has
loaded, passing the known Green song ids for validation.

## Protocol Behavior

`initialdatacheck.php`:

- Advertise every enabled Green event folder in
  `CommonInitialDataCheckResponse.AryGreenEventFolderDatas`.
- Each info row uses `InfoId = FolderId` and `VerupNo = VerupNo`.
- Empty Green event-folder data produces no advertised folder rows.

`getfolder.php`:

- Route through Mediator using
  `GetFolderQuery(GameEra.Green, request.FolderIds ?? [])`.
- Return `Result = 1`.
- Include rows for requested folder ids that exist in `green.EventFolders`.
- Omit unknown requested ids and log a warning.
- Populate each Green wire `EventfolderData` row with `folder_id`, `verup_no`,
  and `song_no`.

This unknown-id behavior should match the existing Nijiiro handler behavior.

## Shared Lookup Code

The current Nijiiro folder handler already performs the desired request-time
lookup:

- iterate requested folder ids;
- add known rows to the response;
- warn and continue on unknown rows;
- return `Result = 1`.

Extract that behavior inside `GetFolderQueryHandler`, for example as a private
helper that accepts an `IReadOnlyDictionary<uint, EventFolderData>` and the
requested ids. `HandleNijiiro` and `HandleGreen` can both call the helper while
still obtaining their dictionaries from their own era catalogs.

This shares the behavior that is truly common without forcing Green to use
Nijiiro catalog loading or Nijiiro initial-data version grouping.

## Mapper Behavior

`Adapters.GameProtocol.Green.Mappers.FolderDataMappers.Map` currently only maps
`Result`. It must explicitly add `common.AryEventfolderDatas` rows to
`GetfolderResponse.AryEventfolderDatas`, including:

- `FolderId`
- `VerupNo`
- `SongNoes`

`FolderId` and `VerupNo` are optional in the Green wire row type, but the server
should set them for every returned known folder.

## Testing

Loader tests:

- missing file returns an empty dictionary;
- default committed JSON loads;
- duplicate folder ids fail;
- folder id `0` and ids above `15` fail;
- missing `verupNo` fails;
- explicit `verupNo: 0` passes;
- missing `songNo` fails;
- empty `songNo` fails;
- song ids `>= 1024` fail;
- song ids missing from the supplied Green song catalog fail.

Initial-data tests:

- Green folders are advertised with `InfoId = FolderId` and matching `VerupNo`.
- Empty Green folder catalog produces no `AryGreenEventFolderDatas`.
- Tests should assert by id rather than relying on row order.

Get-folder handler and mapper tests:

- Green requested known ids return matching folder rows and song lists.
- Unknown requested ids are omitted with `Result = 1`.
- Request ids are protocol ids, not cache ids.
- The Green mapper populates `AryEventfolderDatas`.

Shared behavior guard:

- Keep Nijiiro folder behavior covered after extracting the helper, either by
  existing tests or a small regression test.

Suggested verification:

- focused Green event-folder tests;
- focused existing Nijiiro folder tests if any are changed or added;
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Green"`;
- `dotnet build Host/Host.csproj`.

If `Host/bin` is locked, use a temporary output build rather than treating the
lock as a source failure.

## Risks And Open Edges

- `verupNo` semantics are inferred as cache invalidation data. The design makes
  the field explicit and editable so operators can bump it when content changes.
- The default cache does not expose a proven server-side per-folder version
  stamp. The generated default should use `verupNo: 1` for each row and rely on
  future operator edits to bump rows when membership changes.
- The design intentionally validates normal feature-board ids only. AI Battle
  task-board folders remain out of scope.
- If future Green data uses protocol ids above `15`, implementation should first
  add texture/client evidence for those ids.
