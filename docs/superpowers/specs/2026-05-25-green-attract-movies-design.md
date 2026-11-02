# Green Attract Movies Startup Auth Design

Date: 2026-05-25

## Goal

Enable Green attract movie permissions through startup auth while keeping
Nijiiro movie permissions era-specific. Green should enable all available
nonzero attract movies by default, and operators should be able to replace that
default with an explicit list, including an empty list.

## Current State

Startup auth is served by the shared route
`/v01r00/chassis/startupauth.php`. The shared `StartupAuthResponse` already
contains `ary_movie_info`, but the current controller only sets `result` and
echoes `ary_operation_info`.

Green's generated protobuf has the same startup auth movie field shape as the
shared model:

- `ary_movie_info`
- `movie_id`
- `enable_days`

Nijiiro already has an operator-edited `Host/wwwroot/data/nijiiro/movie_data.json`
with a bare array of movie rows. Green does not have an era-specific movie
configuration yet.

## Binary Evidence

IDA analysis of the Green `S111_N` binary shows the startup auth path in
`game_net_StartupAuth.cpp`:

- `OnStartupAuthResponse` reads `ary_movie_info`.
- Rows with `movie_id = 0` are ignored by the response importer.
- Nonzero movie IDs are stored in a permission map with an expiry derived from
  `enable_days`.
- The permission map is exported to
  `/updates/S11100-1/moviepermission.bin`.
- Attract startup scans `/data/movie` for filenames matching
  `attract_cm_([0-9]{3}).pam`.
- The parsed three-digit file ID must exist in the permission map to become a
  playable attract movie.

This means the server should only send IDs that correspond to real
`attract_cm_###.pam` files. If a wrong ID is sent, Green stores it but no movie
is selected because no matching file exists.

The local Green data tree currently contains `attract_cm_000.pam`,
`attract_cm_100.pam`, `attract_cm_102.pam` through `attract_cm_141.pam`, and
`attract_cm_143.pam` through `attract_cm_161.pam`. ID `142` is not present.
ID `0` is the attract fallback file and is not a startup permission candidate.

## Green Configuration Contract

Green uses a new file:

`Host/wwwroot/data/green/movie_data.json`

The file is an object, not a bare array:

```json
{
  "override_default": false,
  "movies": []
}
```

When `override_default` is `false`, the loader ignores `movies` and auto-enables
all discovered nonzero `attract_cm_###.pam` files.

When `override_default` is `true`, the loader uses `movies` as the explicit
permission list. An empty array means no Green attract movies are enabled.

Example override:

```json
{
  "override_default": true,
  "movies": [
    { "movie_id": 100, "enable_days": 999 },
    { "movie_id": 102, "enable_days": 999 }
  ]
}
```

If `movie_data.json` is missing, Green behaves as though
`override_default` were `false`.

The default `enable_days` for discovered Green movies is `999`, matching the
existing README guidance for always-displayed movies.

## Era Resolution

Startup auth remains shared, so the controller must resolve the target era from
`StartupAuthRequest.HddVer`.

Resolution rules:

- `11xx` maps to `GameEra.Green`; Green normally sends `1113`.
- `12xx` maps to `GameEra.Nijiiro`; Nijiiro commonly sends values such as
  `1239`.
- If the version does not match a known range and exactly one era is enabled,
  use the enabled era.
- If the version does not match and multiple eras are enabled, return no movie
  permissions and log a warning.
- If the version resolves to an era that is disabled, return no movie
  permissions and log a warning.

This keeps the shared route compatible with Green without leaking movie IDs
across eras.

## Components

Add an application-level MediatR query,
`GetStartupMovieDataQuery(uint HddVer)`, plus a handler. The shared controller
should call this handler instead of directly reading catalogs. The handler
resolves the era from `HddVer`, reads the corresponding era catalog, and returns
`Application.ServerData.MovieData` rows.

Extend `IGreenCatalog` with a Green movie permission collection. Populate it in
`GreenEraGameDataCatalog.InitializeAsync` through a new `GreenMovieLoader`.

`GreenMovieLoader` responsibilities:

- Read `Host/wwwroot/data/green/movie_data.json` when present.
- Discover files under `Host/wwwroot/data/green/data/movie`.
- Parse filenames with the same practical rule as the client:
  `attract_cm_###.pam`.
- Ignore ID `0`.
- Sort emitted default rows by `movie_id`.
- Use `enable_days = 999` for default rows.
- In override mode, keep configured `enable_days` values.

Nijiiro keeps using the existing `INijiiroCatalog.GetMovieDataDictionary()`.
The existing Nijiiro JSON array format is unchanged.

## Validation And Error Handling

Green config validation:

- Missing `movie_data.json`: use default discovery.
- Invalid JSON: fail catalog initialization with a clear configuration error.
- Present file with missing or invalid `override_default`: fail catalog
  initialization with a clear configuration error.
- Duplicate nonzero `movie_id` values in `movies`: fail catalog initialization.
- `movie_id = 0`: ignore.
- Override IDs that do not correspond to discovered files: log a warning and
  skip them.
- Missing `green/data/movie` directory: log a warning and treat discovery as an
  empty set. Default mode emits no movie rows. Override mode can only emit rows
  whose IDs are discoverable, so it also emits no rows when the directory is
  missing.

## Data Flow

1. Green sends startup auth to `/v01r00/chassis/startupauth.php` with
   `hdd_ver = 1113`.
2. `StartupAuthController` logs the request and sends
   `GetStartupMovieDataQuery(request.HddVer)`.
3. The query handler resolves `1113` to `GameEra.Green`.
4. The handler reads the Green movie collection from `IGreenCatalog`.
5. The controller maps application `MovieData` rows into
   `StartupAuthResponse.MovieData` and appends them to `ary_movie_info`.
6. Green stores nonzero rows in `moviepermission.bin`.
7. Attract selects only permitted movie IDs that also exist as
   `attract_cm_###.pam` files.

For Nijiiro startup auth, the same flow resolves `12xx` to `GameEra.Nijiiro`
and uses the existing Nijiiro movie data catalog.

## Tests

Unit tests should cover `GreenMovieLoader`:

- Missing config defaults to discovered nonzero movies.
- `override_default = false` ignores `movies`.
- `override_default = true` uses the configured subset.
- Empty override returns no movies.
- ID `0` is ignored.
- Duplicate nonzero IDs fail.
- Invalid JSON fails.
- Missing `override_default` in an existing file fails.
- Missing movie directory warns and returns an empty default list.
- Override IDs missing from discovery are skipped.

Startup auth tests should cover:

- `HddVer = 1113` emits Green movie rows.
- `HddVer = 12xx` emits Nijiiro movie rows.
- Unknown `HddVer` with multiple enabled eras emits no movie rows.
- The shared response remains serializable as Green and WwR08 startup auth
  protobuf.

Verification for implementation should include the relevant Green test subset
and `dotnet build`. In-game verification is still required to confirm actual
movie playback on a cabinet or RPCS3 client.
