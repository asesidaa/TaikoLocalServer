# Green-era star data: tuning.bin parser & WebUI surface

Date: 2026-05-16
Status: Approved

## Problem

The Green-era admin endpoint `GET /api/Green/GameData/MusicDetails` hard-codes
`StarEasy/Normal/Hard/Oni/Ura = 0` for every song
(`Adapters.AdminApi/Controllers/GameDataController.cs:67-71`). As a result the
WebUI shows every Green chart as zero stars on `Song.razor` and `SongList.razor`,
even though `MusicDetail` already carries the five star fields and the WebUI
already reads them via `GameDataService.GetMusicStarLevel()`.

Green's `musicinfo.xml` legitimately has no star fields (verified by
reverse-engineering the PS3 EBOOT — schema parsed by `sub_7D98E0`). The stars
live in `/data/fumen/tuning.bin`, which the in-game C++ parses via
`AnalyzeTuningData(MusicCourseInfoMap&, void*)` into a map at
`singleton + 1124`, and which the binding `GetMusicInfo_Detail` reads to drive
the on-screen star display.

The user has placed (symlinked) the genuine `tuning.bin` and `tuning_ext.bin`
files under `Host/wwwroot/data/green/datatable/fumen/`. We want to parse
`tuning.bin` server-side, surface the stars on `GreenMusicInfoEntry`, and let
the existing WebUI star pipeline pick them up.

## Out of scope

- `tuning_ext.bin` (per-song score thresholds). The file is parsed by the game
  for scoring rules; it does not affect star display. Deferred.
- Game-protocol (protobuf) star surfacing. Out of scope; this design covers
  WebUI display only.
- Caching/precomputing the parsed result on disk. The file is 2.95 MB and
  parses in <50 ms; loading it on startup alongside the other Green catalog
  files is fine.
- Loader-level tests. The solution does have `Tests/Green/` (handler tests),
  but no catalog-loader has unit tests today; adding one for the tuning loader
  is out of scope. The catalog handler fixture `GreenHandlerFixture` constructs
  `GreenMusicInfoEntry` via object initializer with a partial field set; the
  new star fields default to 0 there, which keeps existing tests green.

## File format (reverse-engineered)

Verified against the binary at
`H:/TaikoLocalServer/.tools/ida-snap/EBOOT.ELF.codex.i64` (see memory entries
`reference-ida-green-eboot` and `green-stars-in-tuning-bin`) and confirmed by a
Python prototype that produced star values matching wikiwiki.jp/taiko-fumen for
multiple spot-checked songs.

All integers are big-endian 4-byte unsigned. `tuning.bin` is exactly
**2 954 473 bytes**, structured as:

```
+0x0000  uint32  song_count                  (= 0x000004BA = 1210)
+0x0004  Record[1210]                        (each 2316 bytes)
+0x2AC2BC  StringTable                       (152 109 bytes of null-terminated
                                              ASCII musicid strings)
```

Each `Record` (2316 bytes), in the portion we care about for stars:

```
+0x0000  uint32  musicid_offset              (byte offset into StringTable)
+0x0010  Cell[4] player0_difficulty_cells    (each 128 bytes; cell+0 = star)
+0x0490  ... gap / other fields we don't read ...
+0x0010 + 1152 = +0x478  Cell[4] player1_cells   (duet — identical to player 0
                                                  in practice; we use player 0)
```

The `AnalyzeTuningData` decompilation showed difficulty index 0..3 maps to
easy → normal → hard → oni at write-time, so the 4 player-0 cells at record
offsets `0x10, 0x90, 0x110, 0x190` are `easy, normal, hard, oni` respectively.

### `ex_<base>` records

Of the 1210 records, **157 have musicids starting with `ex_`**. These do not
represent separate songs; they supply the ura/extreme star for the base song.
The game's `AnalyzeTuningData` strips the `ex_` prefix when keying the map
(line 226 of the decompile: `sub_A1A2FC(v16, "ex_", 3)`) and routes the ex
record's "oni" cell into the URA slot of the merged `MusicCourseInfo` struct.

Our parser mirrors this:

1. Walk all 1210 records, collecting `(musicid_string, [c0, c1, c2, c3])`.
2. Split into base records (no `ex_` prefix) and ex supplements.
3. Final star set per base musicid:
   `{ Easy=c0, Normal=c1, Hard=c2, Oni=c3, Ura = ex_supplement.c3 or 0 }`.

### Verification (2026-05-16)

| musicid | easy | normal | hard | oni | ura | confirmed |
|---------|------|--------|------|-----|-----|-----------|
| tank    | 3    | 5      | 6    | 6   | 9   | yes (wikiwiki) |
| tttt    | 4    | 5      | 6    | 10  | -   | yes |
| totoro  | 2    | 3      | 1    | 5   | -   | yes (genuinely quirky chart) |
| kznhel  | 3    | 4      | 5    | 7   | 8   | yes |

Star histogram across all 4 343 (musicid×difficulty) cells: min=1, max=10,
distribution peaks 3-7★ — matches expected Taiko chart distribution.

## Architecture

One new file-loader class, plus modifications to four existing files. No new
DI ports, no new project references.

```
Application/Catalog/Green/
  GreenStarSet.cs                  NEW   readonly record struct, 5 byte fields
  GreenMusicInfoEntry.cs           MOD   add 5 uint properties (StarEasy..StarUra)
                                         and convert to `sealed record class`
                                         so the catalog can use `with` syntax

Infrastructure/GameDataCatalog/Green/
  GreenTuningLoader.cs             NEW   parses tuning.bin -> Dict<musicid,StarSet>
  GreenEraGameDataCatalog.cs       MOD   call GreenTuningLoader, enrich entries
  GreenRequiredDataFiles.cs        MOD   add fumen/tuning.bin to required list

Adapters.AdminApi/Controllers/
  GameDataController.cs            MOD   populate stars from the enriched entry
                                         instead of hardcoding zeros
```

`GreenMusicInfoEntry` going from `sealed class { get; init; }` to
`sealed record class { get; init; }` is a near-zero-risk change: the property
shape is identical, only `Equals/GetHashCode/ToString` semantics change. All
12 usages across the solution (verified 2026-05-16) construct entries via
object initializer (`new GreenMusicInfoEntry { ... }`) and consume them as
elements of `IReadOnlyList<>` / `IReadOnlyDictionary<,>`. None compare via
reference equality, none pattern-match on the type, none call `Equals/ToString`
explicitly. The conversion is safe.

## Data flow

```
Host startup
  → IGameDataCatalog.InitializeAsync
     → GreenEraGameDataCatalog.InitializeAsync
        → GreenRequiredDataFiles.ThrowIfMissing()    [now also gates tuning.bin]
        → musicInfo = await GreenMusicInfoLoader.LoadAsync()    [unchanged]
        → stars    = await GreenTuningLoader.LoadAsync()        [NEW]
        → enriched = musicInfo.Entries
              .Select(e => stars.TryGetValue(e.MusicId, out var s)
                  ? e with { StarEasy=s.Easy, ..., StarUra=s.Ura }
                  : e)
              .ToArray()
        → store enriched entries in dictionaries (file order + by SongNo)
        → log a warning if any musicinfo songs had no tuning match

WebUI page load
  → GET /api/Green/GameData/MusicDetails
     → GameDataController.BuildGreenMusicDetails
        → for each entry, MusicDetail.StarEasy = entry.StarEasy (etc.)
  → Song.razor / SongList.razor render via GameDataService.GetMusicStarLevel
```

## Component contracts

### `GreenStarSet` (Application/Catalog/Green/GreenStarSet.cs)

```csharp
public readonly record struct GreenStarSet(
    byte Easy,
    byte Normal,
    byte Hard,
    byte Oni,
    byte Ura);   // 0 when no ex_ supplement exists
```

`byte` because tuning stars are 1-10. Stored on the dictionary returned by
the loader; copied into `GreenMusicInfoEntry` as `uint` to match the existing
`MusicDetail` shape (which already uses `int`/`uint` for stars).

### `GreenTuningLoader` (Infrastructure/GameDataCatalog/Green/GreenTuningLoader.cs)

```csharp
public sealed class GreenTuningLoader
{
    public Task<IReadOnlyDictionary<string, GreenStarSet>> LoadAsync(
        CancellationToken cancellationToken);
}
```

Behavior:

1. Open `<datatable>/fumen/tuning.bin` via `PathHelper.GetDataTablePath(GameEra.Green)`.
2. Read full file into memory (single 2.95 MB read).
3. Validate: `BinaryPrimitives.ReadUInt32BigEndian(bytes[..4]) == 0x4BA` and
   `bytes.Length == 4 + 1210 * 2316 + stringTableSize`. Throw
   `InvalidDataException` with offsets on mismatch.
4. Two-pass parse:
   - Pass 1: for `i = 0..1209`, read record at `4 + i*2316`, read musicid via
     the string-table offset at record+0, read 4 stars from cells at
     record+0x10, +0x90, +0x110, +0x190 (offset 0 of each cell).
   - Pass 2: bucket `ex_*` records aside, build the final dictionary by
     pairing base records with their optional ex supplement.
5. Return as `IReadOnlyDictionary<string, GreenStarSet>` keyed by musicid
   (without `ex_` prefix).

Pure: no logger needed; throws are sufficient. Matches the pattern of the
other Green loaders that have no DI.

### `GreenMusicInfoEntry` modifications

Add five properties (with defaults so existing test/fake constructions don't
break):

```csharp
public uint StarEasy   { get; init; }
public uint StarNormal { get; init; }
public uint StarHard   { get; init; }
public uint StarOni    { get; init; }
public uint StarUra    { get; init; }
```

Change `sealed class` to `sealed record class`.

### `GreenEraGameDataCatalog.InitializeAsync` modifications

Insert after the existing `musicInfo = await new GreenMusicInfoLoader().LoadAsync(ct);`
line and before `musicInfos = musicInfo.Entries.ToDictionary(...);`:

```csharp
var stars = await new GreenTuningLoader().LoadAsync(cancellationToken);

var enrichedEntries = musicInfo.Entries
    .Select(entry => stars.TryGetValue(entry.MusicId, out var set)
        ? entry with {
            StarEasy = set.Easy,
            StarNormal = set.Normal,
            StarHard = set.Hard,
            StarOni = set.Oni,
            StarUra = set.Ura
          }
        : entry)
    .ToArray();

var missingTuning = enrichedEntries
    .Where(e => !stars.ContainsKey(e.MusicId))
    .Select(e => e.MusicId)
    .ToList();
if (missingTuning.Count > 0)
{
    logger.LogWarning(
        "Green: {Count} musicinfo entries have no tuning record; using star=0 (examples: {Examples})",
        missingTuning.Count,
        string.Join(", ", missingTuning.Take(5)));
}
```

Then `musicInfoFileOrder = enrichedEntries;` and the by-id dictionaries are
built from `enrichedEntries` instead of `musicInfo.Entries`.

The hash-version logging line gets a star-coverage detail appended.

### `GreenRequiredDataFiles` modifications

Add one line to `GetRequiredPaths`:

```csharp
Path.Combine(datatablePath, "fumen", "tuning.bin")
```

Symmetrical with the existing `musicinfo.xml` and `musicmedleyinfo.xml` entries.
`ThrowIfMissing()` already gives a clear `FileNotFoundException` message.

### `GameDataController.BuildGreenMusicDetails` modifications

Replace the five hard-coded `Star* = 0` lines with reads from `pair.Value`:

```csharp
StarEasy   = pair.Value.StarEasy,
StarNormal = pair.Value.StarNormal,
StarHard   = pair.Value.StarHard,
StarOni    = pair.Value.StarOni,
StarUra    = pair.Value.StarUra,
```

Everything else in `BuildGreenMusicDetails` is unchanged.

## Error handling summary

| Condition | Behavior |
|---|---|
| `tuning.bin` missing | `GreenRequiredDataFiles.ThrowIfMissing()` → `FileNotFoundException` at startup |
| `tuning.bin` wrong size or wrong header | `GreenTuningLoader` → `InvalidDataException` at startup |
| Song in `musicinfo.xml` not in `tuning.bin` | Star fields stay at default 0; one warning log lists count + first 5 musicids |
| Song in `tuning.bin` not in `musicinfo.xml` | Silently dropped (no consumer for unmatched stars) |
| Star value outside 1-10 in data | Accepted as-is; the on-disk file is the source of truth, and a malformed record would be flagged by the header/size check anyway |

## Verification plan (post-implementation)

1. `dotnet build` clean.
2. Boot the server. Confirm startup log shows the Green music count and no
   tuning warnings (since the user's musicinfo.xml and tuning.bin came from
   the same install).
3. Hit `GET /api/Green/GameData/MusicDetails` in the WebUI. Pick a few songs
   (tank, kznhel, lassen) and verify the returned star values match the
   verification table above.
4. Visit `Song.razor` / `SongList.razor` for a Green user and confirm star
   columns render non-zero values.

User runs the server (per memory `feedback-server-run-admin`); we build and
ship the diff.
