# Green Star Tuning Parser Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Parse Green-era chart star levels from `Host/wwwroot/data/green/datatable/fumen/tuning.bin`, enrich the Green catalog, and surface the values through the existing Admin API/WebUI music detail pipeline.

**Architecture:** Add a small Green star value type and a binary `tuning.bin` loader in the existing filesystem catalog layer. `GreenEraGameDataCatalog` enriches `GreenMusicInfoEntry` rows during startup, and `GameDataController` maps those enriched star fields into `MusicDetail`.

**Tech Stack:** C#/.NET 10, xUnit, ASP.NET Core Admin API, existing `IGameDataCatalog`/`IGreenCatalog` filesystem catalog.

---

## Scope Check

This plan implements one subsystem: Green WebUI star display. It does not parse `tuning_ext.bin`, does not add game-protocol protobuf star fields, and does not add `Host.csproj` copy metadata for `fumen/tuning.bin`. The approved spec states that the player/operator supplies Green game datatables; local IDE runs can continue using the existing `Host/wwwroot` data folder setup.

The approved spec also excludes loader-level automated tests. This plan adds model/API coverage where the behavior crosses existing tested surfaces, then verifies the parser through build and runtime smoke checks.

## Evidence Inputs

- Spec: `docs/superpowers/specs/2026-05-16-green-star-tuning-parser-design.md`
- Reverse-engineering reference folder: `C:\Users\10614\AppData\Local\Temp\ida-domain-20260516_002_lua_bindings\`
- Relevant prototype files if offsets need confirmation:
  - `C:\Users\10614\AppData\Local\Temp\ida-domain-20260516_002_lua_bindings\verify_stars_parser.py`
  - `C:\Users\10614\AppData\Local\Temp\ida-domain-20260516_002_lua_bindings\sub_7D5034_AnalyzeTuningData_FULL.c`

## File Structure

- Create: `Application/Catalog/Green/GreenStarSet.cs`
  - Holds five star values from `tuning.bin` as bytes: easy, normal, hard, oni, ura.
- Modify: `Application/Catalog/Green/GreenMusicInfoEntry.cs`
  - Convert to `sealed record class` so catalog enrichment can use `with`.
  - Add `uint StarEasy`, `StarNormal`, `StarHard`, `StarOni`, `StarUra`.
- Create: `Infrastructure/GameDataCatalog/Green/GreenTuningLoader.cs`
  - Reads `<datatable>/fumen/tuning.bin`.
  - Validates expected file size and `song_count`.
  - Parses base records and `ex_` supplements into `IReadOnlyDictionary<string, GreenStarSet>`.
- Modify: `Infrastructure/GameDataCatalog/Green/GreenRequiredDataFiles.cs`
  - Require `fumen/tuning.bin` at Green catalog startup.
- Modify: `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`
  - Load stars after `musicinfo.xml`.
  - Enrich entries before dictionaries are built.
  - Warn if any `musicinfo.xml` row has no tuning match.
- Modify: `Adapters.AdminApi/Controllers/GameDataController.cs`
  - Map Green `MusicDetail.Star*` values from catalog entries.
- Create: `Tests/Green/GreenMusicInfoEntryTests.cs`
  - Verifies zero defaults and `with`-based enrichment.
- Modify: `Tests/Green/GreenAdminApiControllerTests.cs`
  - Verifies `/api/Green/GameData/MusicDetails` returns catalog star values.

---

### Task 1: Green Star Model and Entry Shape

**Files:**
- Create: `Tests/Green/GreenMusicInfoEntryTests.cs`
- Create: `Application/Catalog/Green/GreenStarSet.cs`
- Modify: `Application/Catalog/Green/GreenMusicInfoEntry.cs`

- [ ] **Step 1: Write the failing model test**

Create `Tests/Green/GreenMusicInfoEntryTests.cs`:

```csharp
namespace TaikoLocalServer.Tests.Green;

public sealed class GreenMusicInfoEntryTests
{
    [Fact]
    public void WithExpression_CanPopulateStarsWithoutMutatingOriginal()
    {
        var entry = new GreenMusicInfoEntry
        {
            MusicId = "tank",
            SongNo = 105
        };
        var set = new GreenStarSet(3, 5, 6, 6, 9);

        var enriched = entry with
        {
            StarEasy = set.Easy,
            StarNormal = set.Normal,
            StarHard = set.Hard,
            StarOni = set.Oni,
            StarUra = set.Ura
        };

        Assert.Equal(0u, entry.StarEasy);
        Assert.Equal(0u, entry.StarNormal);
        Assert.Equal(0u, entry.StarHard);
        Assert.Equal(0u, entry.StarOni);
        Assert.Equal(0u, entry.StarUra);

        Assert.Equal(3u, enriched.StarEasy);
        Assert.Equal(5u, enriched.StarNormal);
        Assert.Equal(6u, enriched.StarHard);
        Assert.Equal(6u, enriched.StarOni);
        Assert.Equal(9u, enriched.StarUra);
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run:

```powershell
dotnet test Tests\Tests.csproj --filter "FullyQualifiedName~GreenMusicInfoEntryTests"
```

Expected: FAIL at compile time with errors including `CS0246` for missing `GreenStarSet` and/or `CS0117` for missing `StarEasy` on `GreenMusicInfoEntry`.

- [ ] **Step 3: Add `GreenStarSet`**

Create `Application/Catalog/Green/GreenStarSet.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Green;

public readonly record struct GreenStarSet(
    byte Easy,
    byte Normal,
    byte Hard,
    byte Oni,
    byte Ura);
```

- [ ] **Step 4: Convert `GreenMusicInfoEntry` to a record and add star fields**

Replace `Application/Catalog/Green/GreenMusicInfoEntry.cs` with:

```csharp
using TaikoLocalServer.Application.Abstractions;

namespace TaikoLocalServer.Application.Catalog.Green;

public sealed record class GreenMusicInfoEntry : IMusicInfoEntry
{
    public string MusicId { get; init; } = string.Empty;

    public uint SongNo { get; init; }

    public uint NewRelease { get; init; }

    public bool IsSecret { get; init; }

    public bool IsPapaMama { get; init; }

    public bool HasExtreme { get; init; }

    public string PartsSet { get; init; } = string.Empty;

    public string WaiwaiPartsSet { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string GenreName { get; init; } = string.Empty;

    public uint CategoryId { get; init; }

    public uint DemoPlay { get; init; }

    public IReadOnlyList<uint> Tags { get; init; } = [];

    public int FileOrder { get; init; }

    public uint StarEasy { get; init; }

    public uint StarNormal { get; init; }

    public uint StarHard { get; init; }

    public uint StarOni { get; init; }

    public uint StarUra { get; init; }
}
```

- [ ] **Step 5: Run the model test to verify it passes**

Run:

```powershell
dotnet test Tests\Tests.csproj --filter "FullyQualifiedName~GreenMusicInfoEntryTests"
```

Expected: PASS with one test passing.

- [ ] **Step 6: Commit**

```powershell
git add -- Tests/Green/GreenMusicInfoEntryTests.cs Application/Catalog/Green/GreenStarSet.cs Application/Catalog/Green/GreenMusicInfoEntry.cs
git commit -m "Add Green music star fields"
```

---

### Task 2: Green `tuning.bin` Loader

**Files:**
- Create: `Infrastructure/GameDataCatalog/Green/GreenTuningLoader.cs`

- [ ] **Step 1: Add the loader**

Create `Infrastructure/GameDataCatalog/Green/GreenTuningLoader.cs`:

```csharp
using System.Buffers.Binary;
using System.Text;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenTuningLoader
{
    private const uint ExpectedSongCount = 0x4BA;
    private const int RecordCount = (int)ExpectedSongCount;
    private const int ExpectedFileSize = 2_954_473;
    private const int HeaderSize = 4;
    private const int RecordSize = 2_316;
    private const int StringTableOffset = HeaderSize + RecordCount * RecordSize;
    private const int Player0CellOffset = 0x10;
    private const int DifficultyCellStride = 0x80;
    private const string ExPrefix = "ex_";

    public Task<IReadOnlyDictionary<string, GreenStarSet>> LoadAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataTablePath(GameEra.Green), "fumen", "tuning.bin");
        return LoadFromFileAsync(path, cancellationToken);
    }

    public static async Task<IReadOnlyDictionary<string, GreenStarSet>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
        return Parse(bytes, path);
    }

    private static IReadOnlyDictionary<string, GreenStarSet> Parse(byte[] bytes, string path)
    {
        ValidateHeader(bytes, path);

        var baseRecords = new Dictionary<string, TuningCourseStars>(StringComparer.Ordinal);
        var exRecords = new Dictionary<string, TuningCourseStars>(StringComparer.Ordinal);

        for (var index = 0; index < RecordCount; index++)
        {
            var recordOffset = HeaderSize + index * RecordSize;
            var musicIdOffset = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(recordOffset, 4));
            var musicId = ReadMusicId(bytes, musicIdOffset, path, index);
            var stars = new TuningCourseStars(
                ReadStar(bytes, recordOffset, 0, path, index),
                ReadStar(bytes, recordOffset, 1, path, index),
                ReadStar(bytes, recordOffset, 2, path, index),
                ReadStar(bytes, recordOffset, 3, path, index));

            if (musicId.StartsWith(ExPrefix, StringComparison.Ordinal))
            {
                exRecords[musicId[ExPrefix.Length..]] = stars;
            }
            else
            {
                baseRecords[musicId] = stars;
            }
        }

        var result = new Dictionary<string, GreenStarSet>(baseRecords.Count, StringComparer.Ordinal);
        foreach (var pair in baseRecords)
        {
            var baseStars = pair.Value;
            result[pair.Key] = new GreenStarSet(
                baseStars.Easy,
                baseStars.Normal,
                baseStars.Hard,
                baseStars.Oni,
                exRecords.TryGetValue(pair.Key, out var exStars) ? exStars.Oni : (byte)0);
        }

        return result;
    }

    private static void ValidateHeader(byte[] bytes, string path)
    {
        if (bytes.Length != ExpectedFileSize)
        {
            throw new InvalidDataException(
                $"Invalid Green tuning.bin size for {path}: expected {ExpectedFileSize} bytes, actual {bytes.Length} bytes.");
        }

        var songCount = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(0, 4));
        if (songCount != ExpectedSongCount)
        {
            throw new InvalidDataException(
                $"Invalid Green tuning.bin song_count at 0x0000 in {path}: expected 0x{ExpectedSongCount:X8} ({ExpectedSongCount}), actual 0x{songCount:X8} ({songCount}).");
        }
    }

    private static string ReadMusicId(byte[] bytes, uint musicIdOffset, string path, int recordIndex)
    {
        var stringTableLength = bytes.Length - StringTableOffset;
        if (musicIdOffset >= (uint)stringTableLength)
        {
            throw new InvalidDataException(
                $"Invalid Green tuning.bin musicid offset in record {recordIndex} of {path}: string table offset 0x{musicIdOffset:X8} is outside the {stringTableLength}-byte string table.");
        }

        var absoluteOffset = StringTableOffset + (int)musicIdOffset;
        var end = Array.IndexOf(bytes, (byte)0, absoluteOffset);
        if (end < 0)
        {
            throw new InvalidDataException(
                $"Invalid Green tuning.bin musicid in record {recordIndex} of {path}: missing null terminator after file offset 0x{absoluteOffset:X8}.");
        }

        return Encoding.ASCII.GetString(bytes, absoluteOffset, end - absoluteOffset);
    }

    private static byte ReadStar(byte[] bytes, int recordOffset, int difficultyIndex, string path, int recordIndex)
    {
        var offset = recordOffset + Player0CellOffset + difficultyIndex * DifficultyCellStride;
        var value = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset, 4));
        if (value > byte.MaxValue)
        {
            throw new InvalidDataException(
                $"Invalid Green tuning.bin star value in record {recordIndex}, difficulty {difficultyIndex}, offset 0x{offset:X8} of {path}: {value} does not fit in a byte.");
        }

        return (byte)value;
    }

    private readonly record struct TuningCourseStars(
        byte Easy,
        byte Normal,
        byte Hard,
        byte Oni);
}
```

- [ ] **Step 2: Build to verify the loader compiles**

Run:

```powershell
dotnet build --no-restore
```

Expected: PASS with `0 Error(s)`.

- [ ] **Step 3: Commit**

```powershell
git add -- Infrastructure/GameDataCatalog/Green/GreenTuningLoader.cs
git commit -m "Parse Green tuning star data"
```

---

### Task 3: Required Green Data File

**Files:**
- Modify: `Infrastructure/GameDataCatalog/Green/GreenRequiredDataFiles.cs`

- [ ] **Step 1: Require `fumen/tuning.bin`**

Replace `Infrastructure/GameDataCatalog/Green/GreenRequiredDataFiles.cs` with:

```csharp
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

internal static class GreenRequiredDataFiles
{
    public static IReadOnlyList<string> GetRequiredPaths()
    {
        var datatablePath = PathHelper.GetDataTablePath(GameEra.Green);
        return
        [
            Path.Combine(datatablePath, "musicinfo.xml"),
            Path.Combine(datatablePath, "musicmedleyinfo.xml"),
            Path.Combine(datatablePath, "fumen", "tuning.bin")
        ];
    }

    public static void ThrowIfMissing()
    {
        foreach (var path in GetRequiredPaths())
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Green required datatable is missing: {path}", path);
            }
        }
    }
}
```

- [ ] **Step 2: Build to verify the required-file change compiles**

Run:

```powershell
dotnet build --no-restore
```

Expected: PASS with `0 Error(s)`.

- [ ] **Step 3: Commit**

```powershell
git add -- Infrastructure/GameDataCatalog/Green/GreenRequiredDataFiles.cs
git commit -m "Require Green tuning data file"
```

---

### Task 4: Catalog Star Enrichment

**Files:**
- Modify: `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`

- [ ] **Step 1: Enrich `musicinfo.xml` rows with tuning stars**

In `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`, replace the start of `InitializeAsync` through the existing `sharedMusicInfos = ...` assignment with this block:

```csharp
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        GreenRequiredDataFiles.ThrowIfMissing();

        var musicInfo = await new GreenMusicInfoLoader().LoadAsync(cancellationToken);
        var stars = await new GreenTuningLoader().LoadAsync(cancellationToken);
        var enrichedEntries = musicInfo.Entries
            .Select(entry => stars.TryGetValue(entry.MusicId, out var set)
                ? entry with
                {
                    StarEasy = set.Easy,
                    StarNormal = set.Normal,
                    StarHard = set.Hard,
                    StarOni = set.Oni,
                    StarUra = set.Ura
                }
                : entry)
            .ToArray();

        var missingTuning = enrichedEntries
            .Where(entry => !stars.ContainsKey(entry.MusicId))
            .Select(entry => entry.MusicId)
            .ToList();
        if (missingTuning.Count > 0)
        {
            logger.LogWarning(
                "Green: {Count} musicinfo entries have no tuning record; using star=0 (examples: {Examples})",
                missingTuning.Count,
                string.Join(", ", missingTuning.Take(5)));
        }

        songHashVersion = musicInfo.SongHashVersion;
        musicInfoFileOrder = enrichedEntries;
        musicInfos = enrichedEntries.ToDictionary(entry => entry.SongNo);
        sharedMusicInfos = musicInfos.ToDictionary(
            pair => pair.Key,
            pair => (IMusicInfoEntry)pair.Value);
```

Leave the existing `taikojukuFileOrder = ...` line and the rest of the method after it in place.

- [ ] **Step 2: Add star coverage to the startup log**

In the same method, replace the existing `logger.LogInformation(...)` block at the end of `InitializeAsync` with:

```csharp
        logger.LogInformation(
            "Loaded Green catalog: {SongCount} songs, song_hash_ver={SongHashVersion}, {TaikojukuCount} taikojuku packs, {StarCount} tuning star rows",
            musicInfoFileOrder.Count,
            songHashVersion,
            taikojukuFileOrder.Count,
            stars.Count);
```

- [ ] **Step 3: Build to verify catalog enrichment compiles**

Run:

```powershell
dotnet build --no-restore
```

Expected: PASS with `0 Error(s)`.

- [ ] **Step 4: Commit**

```powershell
git add -- Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs
git commit -m "Enrich Green catalog with tuning stars"
```

---

### Task 5: Admin API Star Surface

**Files:**
- Modify: `Tests/Green/GreenAdminApiControllerTests.cs`
- Modify: `Adapters.AdminApi/Controllers/GameDataController.cs`

- [ ] **Step 1: Replace the existing Green music details test with a star propagation test**

In `Tests/Green/GreenAdminApiControllerTests.cs`, replace the existing method named `GameData_Green_MusicDetailsRouteReturnsCatalog` with:

```csharp
    [Fact]
    public void GameData_Green_MusicDetailsRouteReturnsCatalogStarFields()
    {
        var catalog = new FileGameDataCatalog([new GreenHandlerFixture.TestGreenCatalog(
            musicInfoFileOrder:
            [
                new GreenMusicInfoEntry
                {
                    SongNo = 105,
                    MusicId = "tank",
                    FileOrder = 0,
                    Title = "Tank!",
                    CategoryId = (uint)SongGenre.Anime,
                    HasExtreme = true,
                    StarEasy = 3,
                    StarNormal = 5,
                    StarHard = 6,
                    StarOni = 6,
                    StarUra = 9
                }
            ])]);
        var controller = new GameDataController(catalog);

        var result = controller.GetMusicDetails("Green");

        var ok = Assert.IsType<OkObjectResult>(result);
        var rows = Assert.IsAssignableFrom<Dictionary<uint, MusicDetail>>(ok.Value);
        var row = rows[105];
        Assert.Equal(3, row.StarEasy);
        Assert.Equal(5, row.StarNormal);
        Assert.Equal(6, row.StarHard);
        Assert.Equal(6, row.StarOni);
        Assert.Equal(9, row.StarUra);
    }
```

- [ ] **Step 2: Run the test to verify it fails against the current controller mapping**

Run:

```powershell
dotnet test Tests\Tests.csproj --filter "FullyQualifiedName~GreenAdminApiControllerTests.GameData_Green_MusicDetailsRouteReturnsCatalogStarFields"
```

Expected: FAIL with an assertion showing `row.StarEasy` is `0` instead of `3`.

- [ ] **Step 3: Map Green catalog star fields into `MusicDetail`**

In `Adapters.AdminApi/Controllers/GameDataController.cs`, inside `BuildGreenMusicDetails`, replace the five hard-coded star assignments with:

```csharp
                StarEasy = (int)pair.Value.StarEasy,
                StarNormal = (int)pair.Value.StarNormal,
                StarHard = (int)pair.Value.StarHard,
                StarOni = (int)pair.Value.StarOni,
                StarUra = (int)pair.Value.StarUra
```

The complete `new MusicDetail` initializer should end as:

```csharp
            pair => new MusicDetail
            {
                SongId = pair.Value.SongNo,
                Index = pair.Value.FileOrder,
                SongName = string.IsNullOrWhiteSpace(pair.Value.Title) ? pair.Value.MusicId : pair.Value.Title,
                SongNameEN = string.IsNullOrWhiteSpace(pair.Value.Title) ? pair.Value.MusicId : pair.Value.Title,
                SongNameCN = string.IsNullOrWhiteSpace(pair.Value.Title) ? pair.Value.MusicId : pair.Value.Title,
                SongNameKO = string.IsNullOrWhiteSpace(pair.Value.Title) ? pair.Value.MusicId : pair.Value.Title,
                Genre = MapGreenGenre(pair.Value.CategoryId),
                StarEasy = (int)pair.Value.StarEasy,
                StarNormal = (int)pair.Value.StarNormal,
                StarHard = (int)pair.Value.StarHard,
                StarOni = (int)pair.Value.StarOni,
                StarUra = (int)pair.Value.StarUra
            });
```

- [ ] **Step 4: Run the focused Admin API test to verify it passes**

Run:

```powershell
dotnet test Tests\Tests.csproj --filter "FullyQualifiedName~GreenAdminApiControllerTests.GameData_Green_MusicDetailsRouteReturnsCatalogStarFields"
```

Expected: PASS with one test passing.

- [ ] **Step 5: Commit**

```powershell
git add -- Tests/Green/GreenAdminApiControllerTests.cs Adapters.AdminApi/Controllers/GameDataController.cs
git commit -m "Surface Green tuning stars in admin API"
```

---

### Task 6: Final Verification

**Files:**
- Verify: entire solution
- Verify: operator-supplied `Host/wwwroot/data/green/datatable/fumen/tuning.bin`

- [ ] **Step 1: Run all tests**

Run:

```powershell
dotnet test Tests\Tests.csproj --no-restore
```

Expected: PASS with `0 Failed`.

- [ ] **Step 2: Build the solution**

Run:

```powershell
dotnet build --no-restore
```

Expected: PASS with `0 Error(s)`.

- [ ] **Step 3: Runtime smoke when the server is running**

With the server running against the local `Host/wwwroot` Green data folder, request:

```powershell
Invoke-RestMethod -Uri 'http://localhost:5000/api/Green/GameData/MusicDetails'
```

Expected: Green rows include non-zero `starEasy`, `starNormal`, `starHard`, `starOni`, and `starUra` values for songs backed by `tuning.bin`. Spot-check entries corresponding to the spec verification examples:

```text
tank    easy=3 normal=5 hard=6 oni=6  ura=9
tttt    easy=4 normal=5 hard=6 oni=10 ura=0
totoro  easy=2 normal=3 hard=1 oni=5  ura=0
kznhel  easy=3 normal=4 hard=5 oni=7  ura=8
```

- [ ] **Step 4: Inspect the diff for accidental scope expansion**

Run:

```powershell
git diff -- Application/Catalog/Green Infrastructure/GameDataCatalog/Green Adapters.AdminApi/Controllers/GameDataController.cs Tests/Green
```

Expected: diff only includes the star value type, entry star fields, tuning loader, required-file list, catalog enrichment, Admin API mapping, and the two focused tests. No `Host/Host.csproj` changes should appear.

## Plan Self-Review

- Spec coverage: covered `GreenStarSet`, `GreenTuningLoader`, `GreenMusicInfoEntry` star fields and record conversion, `GreenEraGameDataCatalog` enrichment, `GreenRequiredDataFiles`, and `GameDataController` mapping.
- Out-of-scope coverage: no `tuning_ext.bin`, no game-protocol star surface, no disk cache, no `Host.csproj` copy metadata.
- Placeholder scan: no open-ended implementation placeholders are present.
- Type consistency: `GreenStarSet` uses `byte`; `GreenMusicInfoEntry` uses `uint`; `GameDataController` casts to `int` for `MusicDetail`.
