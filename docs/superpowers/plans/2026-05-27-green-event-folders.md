# Green Event Folders Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add data-driven normal Green event-folder support, with default runtime JSON generated from the official `.tools/featureboard.bin` cache.

**Architecture:** Green uses the existing shared `EventFolderData` runtime shape, with Green-specific loader validation and shared request-time folder lookup with Nijiiro. The server reads committed JSON at runtime; `.tools/featureboard.bin` is a local source artifact used only by a committed one-off converter script.

**Tech Stack:** C#/.NET, xUnit, ASP.NET Core controllers, Mediator, protobuf-net generated Green wire types, Python for the one-off cache converter.

---

## Current Worktree Guardrail

At plan-writing time the worktree already contained unrelated changes:

- `M Host/.gitignore`
- `A Host/wwwroot/data/blue/.gitkeep`

Do not stage or commit those files while implementing this plan unless the user explicitly broadens scope.

## File Structure

- Create `.tools/parse_featureboard.py`
  - One-off converter from `.tools/featureboard.bin` to runtime JSON.
  - `.tools/` is ignored, so stage this script with `git add -f`.
- Create `Host/wwwroot/data/green/green_event_folder_data.json`
  - Runtime Green event-folder rows generated from `.tools/featureboard.bin`.
- Modify `Application/Abstractions/IGreenCatalog.cs`
  - Change `EventFolders` to expose shared `EventFolderData`.
- Delete `Application/Catalog/Green/GreenEventFolderEntry.cs`
  - Retire the old Green-only incomplete event-folder model.
- Modify `Infrastructure/GameDataCatalog/Green/GreenEventFolderLoader.cs`
  - Load and validate Green event-folder JSON.
- Modify `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`
  - Pass known Green song ids into the event-folder loader after music info loads.
- Modify `Tests/Green/GreenHandlerFixture.cs`
  - Update the test Green catalog to expose shared event-folder data.
- Create `Tests/Green/GreenEventFolderLoaderTests.cs`
  - Loader regression tests.
- Create `Tests/Green/GreenEventFolderProtocolTests.cs`
  - Initial-data, handler, and mapper regression tests.
- Modify `Application/Handlers/GetInitialDataQuery.Green.cs`
  - Advertise Green event folders in initial data.
- Modify `Application/Handlers/GetFolderQuery.cs`
  - Add shared folder lookup helper.
- Modify `Application/Handlers/GetFolderQuery.Nijiiro.cs`
  - Use shared folder lookup helper.
- Modify `Application/Handlers/GetFolderQuery.Green.cs`
  - Use shared folder lookup helper for Green.
- Modify `Adapters.GameProtocol.Green/Controllers/GetFolderController.cs`
  - Route Green `getfolder.php` through Mediator.
- Modify `Adapters.GameProtocol.Green/Mappers/FolderDataMappers.cs`
  - Populate Green wire folder rows.

---

### Task 1: Add Cache Converter And Default Runtime JSON

**Files:**
- Create: `.tools/parse_featureboard.py`
- Create: `Host/wwwroot/data/green/green_event_folder_data.json`

- [ ] **Step 1: Add the one-off converter script**

Create `.tools/parse_featureboard.py` with:

```python
from __future__ import annotations

import argparse
import json
import struct
from pathlib import Path


def parse_featureboard(path: Path, offset: int, verup_no: int) -> list[dict[str, object]]:
    data = path.read_bytes()
    if len(data) < offset:
        raise ValueError(f"{path} is shorter than parse offset 0x{offset:x}")

    rows: list[dict[str, object]] = []
    position = offset
    while position < len(data):
        if position + 8 > len(data):
            raise ValueError(f"Truncated row header at offset 0x{position:x}")

        cache_id, song_count = struct.unpack_from(">II", data, position)
        position += 8

        byte_count = song_count * 4
        if position + byte_count > len(data):
            raise ValueError(
                f"Truncated song list for cache id {cache_id} at offset 0x{position:x}"
            )

        songs = list(struct.unpack_from(f">{song_count}I", data, position))
        position += byte_count

        rows.append(
            {
                "folderId": cache_id + 1,
                "verupNo": verup_no,
                "songNo": songs,
            }
        )

    if position != len(data):
        raise ValueError(f"Parser stopped at 0x{position:x}, file length is 0x{len(data):x}")

    return rows


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Convert a Green featureboard.bin cache into runtime event-folder JSON."
    )
    parser.add_argument("--input", default=".tools/featureboard.bin")
    parser.add_argument("--output", default="Host/wwwroot/data/green/green_event_folder_data.json")
    parser.add_argument("--offset", default="0x32")
    parser.add_argument("--verup-no", type=int, default=1)
    args = parser.parse_args()

    input_path = Path(args.input)
    output_path = Path(args.output)
    offset = int(str(args.offset), 0)

    rows = parse_featureboard(input_path, offset, args.verup_no)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(json.dumps(rows, indent=2) + "\n", encoding="utf-8")

    folder_ids = ", ".join(str(row["folderId"]) for row in rows)
    song_counts = ", ".join(str(len(row["songNo"])) for row in rows)
    print(f"Wrote {len(rows)} folders to {output_path}")
    print(f"folderIds: {folder_ids}")
    print(f"songCounts: {song_counts}")


if __name__ == "__main__":
    main()
```

- [ ] **Step 2: Generate the runtime JSON**

Run:

```powershell
python .tools/parse_featureboard.py --input .tools/featureboard.bin --output Host/wwwroot/data/green/green_event_folder_data.json
```

Expected output:

```text
Wrote 9 folders to Host\wwwroot\data\green\green_event_folder_data.json
folderIds: 1, 2, 3, 4, 5, 6, 7, 11, 8
songCounts: 38, 20, 40, 5, 10, 36, 28, 16, 4
```

- [ ] **Step 3: Inspect the generated JSON**

Run:

```powershell
Get-Content -Path 'Host\wwwroot\data\green\green_event_folder_data.json' -TotalCount 30
```

Expected output starts with these lines:

```json
[
  {
    "folderId": 1,
    "verupNo": 1,
    "songNo": [
      877,
      876,
      873,
      872
```

The command only shows the beginning of the file; the generated file should contain 9 total rows.

- [ ] **Step 4: Commit the converter and generated data**

Run:

```powershell
git add -f .tools/parse_featureboard.py
git add Host/wwwroot/data/green/green_event_folder_data.json
git diff --cached --name-status
git commit -m "Add Green event folder default data"
```

Expected staged files before commit:

```text
A	.tools/parse_featureboard.py
A	Host/wwwroot/data/green/green_event_folder_data.json
```

---

### Task 2: Loader Tests And Catalog Model

**Files:**
- Create: `Tests/Green/GreenEventFolderLoaderTests.cs`
- Modify: `Application/Abstractions/IGreenCatalog.cs`
- Delete: `Application/Catalog/Green/GreenEventFolderEntry.cs`
- Modify: `Infrastructure/GameDataCatalog/Green/GreenEventFolderLoader.cs`
- Modify: `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`
- Modify: `Tests/Green/GreenHandlerFixture.cs`

- [ ] **Step 1: Write failing loader tests**

Create `Tests/Green/GreenEventFolderLoaderTests.cs` with:

```csharp
using System.Text;
using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenEventFolderLoaderTests
{
    [Fact]
    public async Task LoadFromFile_MissingFileReturnsEmptyDictionary()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");

        var folders = await GreenEventFolderLoader.LoadFromFileAsync(
            path,
            KnownSongs(101),
            CancellationToken.None);

        Assert.Empty(folders);
    }

    [Fact]
    public async Task LoadFromFile_ReadsFolderRowsAndAllowsZeroVerupNo()
    {
        var path = await WriteTempJsonAsync("""
            [
              { "folderId": 1, "verupNo": 0, "songNo": [101, 102] },
              { "folderId": 11, "verupNo": 3, "songNo": [103] }
            ]
            """);
        try
        {
            var folders = await GreenEventFolderLoader.LoadFromFileAsync(
                path,
                KnownSongs(101, 102, 103),
                CancellationToken.None);

            Assert.Equal(2, folders.Count);
            Assert.Equal(0u, folders[1].VerupNo);
            Assert.Equal(new uint[] { 101, 102 }, folders[1].SongNoes);
            Assert.Equal(3u, folders[11].VerupNo);
            Assert.Equal(new uint[] { 103 }, folders[11].SongNoes);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task LoadFromFile_DefaultGreenEventFolderDataLoads()
    {
        var path = FindDefaultEventFolderDataPath();

        Assert.NotNull(path);

        var folders = await GreenEventFolderLoader.LoadFromFileAsync(
            path,
            KnownSongs(Enumerable.Range(0, 1024).Select(id => (uint)id).ToArray()),
            CancellationToken.None);

        Assert.Equal(9, folders.Count);
        Assert.Contains(1u, folders.Keys);
        Assert.Contains(8u, folders.Keys);
        Assert.Contains(11u, folders.Keys);
        Assert.Equal(38, folders[1].SongNoes!.Length);
        Assert.Equal(16, folders[11].SongNoes!.Length);
        Assert.Equal(877u, folders[1].SongNoes![0]);
    }

    [Theory]
    [InlineData("""[{ "verupNo": 1, "songNo": [101] }]""", "folderId")]
    [InlineData("""[{ "folderId": 0, "verupNo": 1, "songNo": [101] }]""", "folderId")]
    [InlineData("""[{ "folderId": 16, "verupNo": 1, "songNo": [101] }]""", "folderId")]
    [InlineData("""[{ "folderId": 1, "songNo": [101] }]""", "verupNo")]
    [InlineData("""[{ "folderId": 1, "verupNo": 1 }]""", "songNo")]
    [InlineData("""[{ "folderId": 1, "verupNo": 1, "songNo": [] }]""", "songNo")]
    [InlineData("""[{ "folderId": 1, "verupNo": 1, "songNo": [1024] }]""", "1024")]
    [InlineData("""[{ "folderId": 1, "verupNo": 1, "songNo": [999] }]""", "unknown")]
    public async Task LoadFromFile_RejectsInvalidRows(string json, string failureText)
    {
        var path = await WriteTempJsonAsync(json);
        try
        {
            var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
                GreenEventFolderLoader.LoadFromFileAsync(
                    path,
                    KnownSongs(101),
                    CancellationToken.None));

            Assert.Contains(failureText, ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task LoadFromFile_RejectsDuplicateFolderIds()
    {
        var path = await WriteTempJsonAsync("""
            [
              { "folderId": 1, "verupNo": 1, "songNo": [101] },
              { "folderId": 1, "verupNo": 2, "songNo": [102] }
            ]
            """);
        try
        {
            var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
                GreenEventFolderLoader.LoadFromFileAsync(
                    path,
                    KnownSongs(101, 102),
                    CancellationToken.None));

            Assert.Contains("duplicate", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static HashSet<uint> KnownSongs(params uint[] songIds)
        => new(songIds);

    private static async Task<string> WriteTempJsonAsync(string json)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(path, json, Encoding.UTF8);
        return path;
    }

    private static string? FindDefaultEventFolderDataPath()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var path = Path.Combine(
                directory.FullName,
                "Host",
                "wwwroot",
                "data",
                "green",
                GreenEventFolderLoader.FileName);

            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }
}
```

- [ ] **Step 2: Run the loader tests and confirm the red state**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenEventFolderLoaderTests"
```

Expected: compile failure because `GreenEventFolderLoader.LoadFromFileAsync(string, HashSet<uint>, CancellationToken)` does not exist yet and `IGreenCatalog.EventFolders` still uses `GreenEventFolderEntry`.

- [ ] **Step 3: Change the Green catalog contract to shared folder rows**

In `Application/Abstractions/IGreenCatalog.cs`, replace:

```csharp
IReadOnlyDictionary<uint, GreenEventFolderEntry> EventFolders { get; }
```

with:

```csharp
IReadOnlyDictionary<uint, EventFolderData> EventFolders { get; }
```

Delete `Application/Catalog/Green/GreenEventFolderEntry.cs`.

- [ ] **Step 4: Implement the Green event-folder loader**

Replace `Infrastructure/GameDataCatalog/Green/GreenEventFolderLoader.cs` with:

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using TaikoLocalServer.Application.Common;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenEventFolderLoader
{
    public const string FileName = "green_event_folder_data.json";

    private const uint MaxProtocolFolderId = 15;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public Task<IReadOnlyDictionary<uint, EventFolderData>> LoadAsync(
        IReadOnlySet<uint> catalogSongIds,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Green), FileName);
        return LoadFromFileAsync(path, catalogSongIds, cancellationToken);
    }

    public static async Task<IReadOnlyDictionary<uint, EventFolderData>> LoadFromFileAsync(
        string path,
        IReadOnlySet<uint> catalogSongIds,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return new Dictionary<uint, EventFolderData>();
        }

        RawEventFolder[] raw;
        try
        {
            await using var stream = File.OpenRead(path);
            raw = await JsonSerializer.DeserializeAsync<RawEventFolder[]>(stream, JsonOptions, cancellationToken)
                  ?? [];
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"Green event folder data is malformed: {path}", ex);
        }

        var byId = new Dictionary<uint, EventFolderData>();
        for (var index = 0; index < raw.Length; index++)
        {
            var folder = Map(raw[index], index, catalogSongIds);
            if (!byId.TryAdd(folder.FolderId, folder))
            {
                throw new InvalidDataException($"Green event folder data contains duplicate folderId {folder.FolderId}.");
            }
        }

        return byId;
    }

    private static EventFolderData Map(
        RawEventFolder raw,
        int index,
        IReadOnlySet<uint> catalogSongIds)
    {
        var rowNumber = index + 1;
        if (raw.FolderId is not { } folderId)
        {
            throw new InvalidDataException($"Green event folder row {rowNumber} is missing folderId.");
        }

        if (folderId is 0 or > MaxProtocolFolderId)
        {
            throw new InvalidDataException($"Green event folder row {rowNumber} has unsupported folderId {folderId}.");
        }

        if (raw.VerupNo is not { } verupNo)
        {
            throw new InvalidDataException($"Green event folder row {rowNumber} is missing verupNo.");
        }

        if (raw.SongNoes is null)
        {
            throw new InvalidDataException($"Green event folder row {rowNumber} is missing songNo.");
        }

        if (raw.SongNoes.Length == 0)
        {
            throw new InvalidDataException($"Green event folder row {rowNumber} has empty songNo.");
        }

        var maxSongId = (uint)(GreenProtocolBytes.SongFlagBytes * 8);
        foreach (var songNo in raw.SongNoes)
        {
            if (songNo >= maxSongId)
            {
                throw new InvalidDataException($"Green event folder {folderId} contains songNo {songNo}, but the limit is {maxSongId - 1}.");
            }

            if (!catalogSongIds.Contains(songNo))
            {
                throw new InvalidDataException($"Green event folder {folderId} contains unknown songNo {songNo}.");
            }
        }

        return new EventFolderData
        {
            FolderId = folderId,
            VerupNo = verupNo,
            SongNoes = raw.SongNoes
        };
    }

    private sealed class RawEventFolder
    {
        [JsonPropertyName("folderId")]
        public uint? FolderId { get; set; }

        [JsonPropertyName("verupNo")]
        public uint? VerupNo { get; set; }

        [JsonPropertyName("songNo")]
        public uint[]? SongNoes { get; set; }
    }
}
```

- [ ] **Step 5: Wire the loader into `GreenEraGameDataCatalog`**

In `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`, replace the field:

```csharp
private IReadOnlyDictionary<uint, GreenEventFolderEntry> eventFolders = new Dictionary<uint, GreenEventFolderEntry>();
```

with:

```csharp
private IReadOnlyDictionary<uint, EventFolderData> eventFolders = new Dictionary<uint, EventFolderData>();
```

Replace the property:

```csharp
public IReadOnlyDictionary<uint, GreenEventFolderEntry> EventFolders => eventFolders;
```

with:

```csharp
public IReadOnlyDictionary<uint, EventFolderData> EventFolders => eventFolders;
```

Replace:

```csharp
eventFolders = await new GreenEventFolderLoader().LoadAsync(cancellationToken);
```

with:

```csharp
eventFolders = await new GreenEventFolderLoader().LoadAsync(
    new HashSet<uint>(musicInfos.Keys),
    cancellationToken);
```

- [ ] **Step 6: Update the Green test fixture catalog**

In `Tests/Green/GreenHandlerFixture.cs`, update the `TestGreenCatalog` constructor signature:

```csharp
public TestGreenCatalog(
    IReadOnlyDictionary<uint, GreenItemShopEntry>? itemShop = null,
    GreenItemShopCatalog? itemShopCatalog = null,
    IReadOnlyDictionary<uint, EventFolderData>? eventFolders = null,
    IReadOnlyList<GreenMusicInfoEntry>? musicInfoFileOrder = null,
    IReadOnlyList<GreenTaikojukuEntry>? taikojukuFileOrder = null)
```

Inside the constructor, after assigning `ItemShop`, add:

```csharp
EventFolders = eventFolders ?? new Dictionary<uint, EventFolderData>();
```

Replace the current event-folder property:

```csharp
public IReadOnlyDictionary<uint, GreenEventFolderEntry> EventFolders { get; } = new Dictionary<uint, GreenEventFolderEntry>();
```

with:

```csharp
public IReadOnlyDictionary<uint, EventFolderData> EventFolders { get; }
```

- [ ] **Step 7: Run loader tests and confirm green state**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenEventFolderLoaderTests"
```

Expected: all `GreenEventFolderLoaderTests` pass.

- [ ] **Step 8: Commit loader and model work**

Run:

```powershell
git add Application/Abstractions/IGreenCatalog.cs
git add Application/Catalog/Green/GreenEventFolderEntry.cs
git add Infrastructure/GameDataCatalog/Green/GreenEventFolderLoader.cs
git add Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs
git add Tests/Green/GreenHandlerFixture.cs
git add Tests/Green/GreenEventFolderLoaderTests.cs
git diff --cached --name-status
git commit -m "Load Green event folder data"
```

Expected staged files:

```text
M	Application/Abstractions/IGreenCatalog.cs
D	Application/Catalog/Green/GreenEventFolderEntry.cs
M	Infrastructure/GameDataCatalog/Green/GreenEventFolderLoader.cs
M	Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs
M	Tests/Green/GreenHandlerFixture.cs
A	Tests/Green/GreenEventFolderLoaderTests.cs
```

---

### Task 3: Advertise Green Event Folders In Initial Data

**Files:**
- Create: `Tests/Green/GreenEventFolderProtocolTests.cs`
- Modify: `Application/Handlers/GetInitialDataQuery.Green.cs`

- [ ] **Step 1: Write failing initial-data tests**

Create `Tests/Green/GreenEventFolderProtocolTests.cs` with:

```csharp
using TaikoLocalServer.Application.ServerData;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenEventFolderProtocolTests
{
    [Fact]
    public async Task InitialData_AdvertisesGreenEventFoldersByProtocolId()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateCatalogWithFolders());
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Green), CancellationToken.None);

        Assert.Contains(response.AryGreenEventFolderDatas, row => row.InfoId == 1 && row.VerupNo == 0);
        Assert.Contains(response.AryGreenEventFolderDatas, row => row.InfoId == 11 && row.VerupNo == 3);
        Assert.Equal(2, response.AryGreenEventFolderDatas.Count);
    }

    [Fact]
    public async Task InitialData_EmptyGreenEventFolderCatalogProducesNoRows()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Green), CancellationToken.None);

        Assert.Empty(response.AryGreenEventFolderDatas);
    }

    private static GreenHandlerFixture.TestGreenCatalog CreateCatalogWithFolders()
        => new(eventFolders: new Dictionary<uint, EventFolderData>
        {
            [1] = new()
            {
                FolderId = 1,
                VerupNo = 0,
                SongNoes = [101, 102]
            },
            [11] = new()
            {
                FolderId = 11,
                VerupNo = 3,
                SongNoes = [103]
            }
        });
}
```

- [ ] **Step 2: Run the protocol tests and confirm the red state**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenEventFolderProtocolTests"
```

Expected: `InitialData_AdvertisesGreenEventFoldersByProtocolId` fails because `AryGreenEventFolderDatas` is empty.

- [ ] **Step 3: Populate Green event-folder initial-data rows**

In `Application/Handlers/GetInitialDataQuery.Green.cs`, add this property initializer inside the `CommonInitialDataCheckResponse` object:

```csharp
AryGreenEventFolderDatas = green.EventFolders.Values
    .Select(entry => new CommonInitialDataCheckResponse.InformationData
    {
        InfoId = entry.FolderId,
        VerupNo = entry.VerupNo
    })
    .ToList(),
```

Place it near `AryGreenTelopDatas` and `AryGreenTaikojukuDatas` so Green information data stays grouped.

- [ ] **Step 4: Run the protocol tests and confirm green state**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenEventFolderProtocolTests"
```

Expected: both tests in `GreenEventFolderProtocolTests` pass.

- [ ] **Step 5: Commit initial-data advertisement**

Run:

```powershell
git add Application/Handlers/GetInitialDataQuery.Green.cs
git add Tests/Green/GreenEventFolderProtocolTests.cs
git diff --cached --name-status
git commit -m "Advertise Green event folders"
```

Expected staged files:

```text
M	Application/Handlers/GetInitialDataQuery.Green.cs
A	Tests/Green/GreenEventFolderProtocolTests.cs
```

---

### Task 4: Serve Green GetFolder Requests

**Files:**
- Modify: `Tests/Green/GreenEventFolderProtocolTests.cs`
- Modify: `Application/Handlers/GetFolderQuery.cs`
- Modify: `Application/Handlers/GetFolderQuery.Nijiiro.cs`
- Modify: `Application/Handlers/GetFolderQuery.Green.cs`
- Modify: `Adapters.GameProtocol.Green/Controllers/GetFolderController.cs`
- Modify: `Adapters.GameProtocol.Green/Mappers/FolderDataMappers.cs`
- Create: `Tests/Green/EventFolderLookupSharedTests.cs`

- [ ] **Step 1: Extend protocol tests for Green getfolder and mapping**

Replace `Tests/Green/GreenEventFolderProtocolTests.cs` with:

```csharp
using TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;
using TaikoLocalServer.Application.ServerData;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenEventFolderProtocolTests
{
    [Fact]
    public async Task InitialData_AdvertisesGreenEventFoldersByProtocolId()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateCatalogWithFolders());
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Green), CancellationToken.None);

        Assert.Contains(response.AryGreenEventFolderDatas, row => row.InfoId == 1 && row.VerupNo == 0);
        Assert.Contains(response.AryGreenEventFolderDatas, row => row.InfoId == 11 && row.VerupNo == 3);
        Assert.Equal(2, response.AryGreenEventFolderDatas.Count);
    }

    [Fact]
    public async Task InitialData_EmptyGreenEventFolderCatalogProducesNoRows()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Green), CancellationToken.None);

        Assert.Empty(response.AryGreenEventFolderDatas);
    }

    [Fact]
    public async Task GetFolder_GreenReturnsKnownRequestedFoldersAndOmitsUnknownIds()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateCatalogWithFolders());
        var handler = new GetFolderQueryHandler(
            NullLogger<GetFolderQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new GetFolderQuery(GameEra.Green, [11, 99, 1]), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Collection(
            response.AryEventfolderDatas,
            row =>
            {
                Assert.Equal(11u, row.FolderId);
                Assert.Equal(3u, row.VerupNo);
                Assert.Equal(new uint[] { 103 }, row.SongNoes);
            },
            row =>
            {
                Assert.Equal(1u, row.FolderId);
                Assert.Equal(0u, row.VerupNo);
                Assert.Equal(new uint[] { 101, 102 }, row.SongNoes);
            });
    }

    [Fact]
    public async Task GetFolder_GreenMapperPopulatesWireRows()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateCatalogWithFolders());
        var handler = new GetFolderQueryHandler(
            NullLogger<GetFolderQueryHandler>.Instance,
            fixture.Catalog);

        var common = await handler.Handle(new GetFolderQuery(GameEra.Green, [1]), CancellationToken.None);
        var wire = FolderDataMappers.Map(common);

        Assert.Equal(1u, wire.Result);
        var row = Assert.Single(wire.AryEventfolderDatas);
        Assert.Equal(1u, row.FolderId);
        Assert.Equal(0u, row.VerupNo);
        Assert.Equal(new uint[] { 101, 102 }, row.SongNoes);
        Assert.True(row.ShouldSerializeFolderId());
        Assert.True(row.ShouldSerializeVerupNo());
    }

    private static GreenHandlerFixture.TestGreenCatalog CreateCatalogWithFolders()
        => new(eventFolders: new Dictionary<uint, EventFolderData>
        {
            [1] = new()
            {
                FolderId = 1,
                VerupNo = 0,
                SongNoes = [101, 102]
            },
            [11] = new()
            {
                FolderId = 11,
                VerupNo = 3,
                SongNoes = [103]
            }
        });
}
```

- [ ] **Step 2: Run the protocol tests and confirm the red state**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenEventFolderProtocolTests"
```

Expected: getfolder tests fail because `GetFolderQuery.Green` returns no rows and the Green mapper only maps `Result`.

- [ ] **Step 3: Add a shared Nijiiro lookup guard test**

Create `Tests/Green/EventFolderLookupSharedTests.cs` with:

```csharp
using System.Collections.Immutable;
using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Tests.Green;

public sealed class EventFolderLookupSharedTests
{
    [Fact]
    public async Task GetFolder_NijiiroStillReturnsKnownRequestedFoldersAndOmitsUnknownIds()
    {
        var folders = new Dictionary<uint, EventFolderData>
        {
            [4] = new()
            {
                FolderId = 4,
                VerupNo = 2,
                SongNoes = [101]
            },
            [9] = new()
            {
                FolderId = 9,
                VerupNo = 5,
                SongNoes = [102, 103]
            }
        }.ToImmutableDictionary();
        var catalog = new FileGameDataCatalog([new TestNijiiroCatalog(folders)]);
        var handler = new GetFolderQueryHandler(
            NullLogger<GetFolderQueryHandler>.Instance,
            catalog);

        var response = await handler.Handle(new GetFolderQuery(GameEra.Nijiiro, [9, 99, 4]), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Collection(
            response.AryEventfolderDatas,
            row =>
            {
                Assert.Equal(9u, row.FolderId);
                Assert.Equal(5u, row.VerupNo);
                Assert.Equal(new uint[] { 102, 103 }, row.SongNoes);
            },
            row =>
            {
                Assert.Equal(4u, row.FolderId);
                Assert.Equal(2u, row.VerupNo);
                Assert.Equal(new uint[] { 101 }, row.SongNoes);
            });
    }

    private sealed class TestNijiiroCatalog(ImmutableDictionary<uint, EventFolderData> eventFolders) : INijiiroCatalog
    {
        public GameEra Era => GameEra.Nijiiro;

        public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos { get; } = new Dictionary<uint, IMusicInfoEntry>();

        public List<uint> GetMusicList() => [];

        public List<uint> GetMusicWithUraList() => [];

        public ImmutableDictionary<uint, SongIntroductionData> GetSongIntroductionDictionary()
            => ImmutableDictionary<uint, SongIntroductionData>.Empty;

        public ImmutableDictionary<uint, MovieData> GetMovieDataDictionary()
            => ImmutableDictionary<uint, MovieData>.Empty;

        public ImmutableDictionary<uint, EventFolderData> GetEventFolderDictionary()
            => eventFolders;

        public ImmutableDictionary<uint, DanData> GetCommonDanDataDictionary()
            => ImmutableDictionary<uint, DanData>.Empty;

        public ImmutableDictionary<uint, DanData> GetCommonGaidenDataDictionary()
            => ImmutableDictionary<uint, DanData>.Empty;

        public List<ShopFolderData> GetShopFolderList() => [];

        public uint GetShopFolderVerup() => 1;

        public Dictionary<string, int> GetTokenDataDictionary() => [];

        public List<uint> GetLockedSongsList() => [];

        public List<uint> GetTimeLimitedSongsList() => [];

        public List<uint> GetLockedUraSongsList() => [];

        public Dictionary<uint, MusicDetail> GetMusicDetailDictionary() => [];

        public List<Costume> GetCostumeList() => [];

        public Dictionary<uint, Title> GetTitleDictionary() => [];

        public Dictionary<uint, Neiro> GetNeiroDictionary() => [];

        public Dictionary<string, List<uint>> GetLockedCostumeDataDictionary() => [];

        public Dictionary<string, List<uint>> GetLockedTitleDataDictionary() => [];

        public List<int> GetCostumeFlagArraySizes() => [];

        public int GetTitleFlagArraySize() => 0;

        public int GetToneFlagArraySize() => 0;

        public ImmutableDictionary<string, uint> GetQRCodeDataDictionary()
            => ImmutableDictionary<string, uint>.Empty;

        public Task InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
```

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~EventFolderLookupSharedTests"
```

Expected: the guard test passes before the shared helper extraction and must still pass after the extraction.

- [ ] **Step 4: Add shared lookup helper**

In `Application/Handlers/GetFolderQuery.cs`, add this method inside `GetFolderQueryHandler`:

```csharp
private CommonGetFolderResponse BuildFolderResponse(
    IReadOnlyDictionary<uint, EventFolderData> eventFolders,
    IEnumerable<uint> requestedFolderIds)
{
    var response = new CommonGetFolderResponse
    {
        Result = 1
    };

    foreach (var folderId in requestedFolderIds)
    {
        if (!eventFolders.TryGetValue(folderId, out var folderData))
        {
            logger.LogWarning("Folder data for folder {FolderId} not found", folderId);
            continue;
        }

        response.AryEventfolderDatas.Add(folderData);
    }

    return response;
}
```

- [ ] **Step 5: Route Nijiiro handler through shared lookup**

Replace `Application/Handlers/GetFolderQuery.Nijiiro.cs` with:

```csharp
namespace TaikoLocalServer.Application.Handlers;

public partial class GetFolderQueryHandler
{
    private partial ValueTask<CommonGetFolderResponse> HandleNijiiro(GetFolderQuery request, CancellationToken cancellationToken)
    {
        var eventFolders = gameDataService.Nijiiro().GetEventFolderDictionary();
        return ValueTask.FromResult(BuildFolderResponse(eventFolders, request.FolderIds));
    }
}
```

- [ ] **Step 6: Implement Green handler through shared lookup**

Replace `Application/Handlers/GetFolderQuery.Green.cs` with:

```csharp
namespace TaikoLocalServer.Application.Handlers;

public partial class GetFolderQueryHandler
{
    private partial ValueTask<CommonGetFolderResponse> HandleGreen(GetFolderQuery request, CancellationToken cancellationToken)
    {
        var eventFolders = gameDataService.Green().EventFolders;
        return ValueTask.FromResult(BuildFolderResponse(eventFolders, request.FolderIds));
    }
}
```

- [ ] **Step 7: Route Green controller through Mediator**

Replace `Adapters.GameProtocol.Green/Controllers/GetFolderController.cs` with:

```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/getfolder.php")]
public class GetFolderController : BaseProtocolController<GetFolderController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetFolder([FromBody] GetfolderRequest request)
    {
        Logger.LogInformation("Green GetFolder request: {Request}", request.Stringify());
        var common = await Mediator.Send(
            new GetFolderQuery(GameEra.Green, request.FolderIds ?? []),
            HttpContext.RequestAborted);
        return Ok(FolderDataMappers.Map(common));
    }
}
```

- [ ] **Step 8: Populate Green wire folder rows**

Replace `Adapters.GameProtocol.Green/Mappers/FolderDataMappers.cs` with:

```csharp
using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class FolderDataMappers
{
    public static GetfolderResponse Map(CommonGetFolderResponse common)
    {
        var response = new GetfolderResponse { Result = common.Result };
        response.AryEventfolderDatas.AddRange(common.AryEventfolderDatas.Select(folder => new GetfolderResponse.EventfolderData
        {
            FolderId = folder.FolderId,
            VerupNo = folder.VerupNo,
            SongNoes = folder.SongNoes ?? []
        }));

        return response;
    }
}
```

- [ ] **Step 9: Run focused protocol tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenEventFolderProtocolTests"
```

Expected: all `GreenEventFolderProtocolTests` pass.

- [ ] **Step 10: Run the shared Nijiiro getfolder guard**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~EventFolderLookupSharedTests"
```

Expected: `EventFolderLookupSharedTests` passes.

- [ ] **Step 11: Commit getfolder support**

Run:

```powershell
git add Tests/Green/GreenEventFolderProtocolTests.cs
git add Tests/Green/EventFolderLookupSharedTests.cs
git add Application/Handlers/GetFolderQuery.cs
git add Application/Handlers/GetFolderQuery.Nijiiro.cs
git add Application/Handlers/GetFolderQuery.Green.cs
git add Adapters.GameProtocol.Green/Controllers/GetFolderController.cs
git add Adapters.GameProtocol.Green/Mappers/FolderDataMappers.cs
git diff --cached --name-status
git commit -m "Serve Green event folder data"
```

Expected staged files:

```text
M	Tests/Green/GreenEventFolderProtocolTests.cs
A	Tests/Green/EventFolderLookupSharedTests.cs
M	Application/Handlers/GetFolderQuery.cs
M	Application/Handlers/GetFolderQuery.Nijiiro.cs
M	Application/Handlers/GetFolderQuery.Green.cs
M	Adapters.GameProtocol.Green/Controllers/GetFolderController.cs
M	Adapters.GameProtocol.Green/Mappers/FolderDataMappers.cs
```

---

### Task 5: Final Verification

**Files:**
- No source edits expected.

- [ ] **Step 1: Run focused event-folder tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenEventFolder"
```

Expected: all Green event-folder tests pass.

- [ ] **Step 2: Run the Green focused suite**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Green"
```

Expected: all Green tests pass.

- [ ] **Step 3: Build Host**

Run:

```powershell
dotnet build Host/Host.csproj
```

Expected: build succeeds.

If the build fails with `MSB3021`, `MSB3027`, or `CS2012` because files under `Host/bin` are locked, rerun:

```powershell
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
```

Expected fallback result: build succeeds using the temporary output directory.

- [ ] **Step 4: Verify git status and commit boundaries**

Run:

```powershell
git status --short
```

Expected after implementation commits: no staged changes. The unrelated pre-existing `Host/.gitignore` and Blue data changes may still appear if the user has not committed them separately.

- [ ] **Step 5: Report verification evidence**

Report:

- event-folder focused test command and result;
- Green focused test command and result;
- Host build command and result;
- final `git status --short` summary;
- commit hashes created during implementation.

Do not claim the feature is complete without fresh command output from this task.
