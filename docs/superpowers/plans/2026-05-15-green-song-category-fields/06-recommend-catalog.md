# Task 6: Recommend-Songs Catalog

**Goal:** Allow operators to declare おすすめ曲 via `wwwroot/data/green/recommend_songs.json`. Expose them through `IGreenCatalog.Recommend`. Make `GetRecommendQuery.Green` return the configured values.

**Files:**
- Create: `Application/Catalog/Green/GreenRecommendEntry.cs`
- Create: `Infrastructure/GameDataCatalog/Green/GreenRecommendLoader.cs`
- Create: `Host/wwwroot/data/green/recommend_songs.json`
- Create: `Tests/Green/GreenRecommendCatalogLoaderTests.cs`
- Modify: `Application/Abstractions/IGreenCatalog.cs`
- Modify: `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`
- Modify: `Tests/Green/GreenHandlerFixture.cs` (the `TestGreenCatalog` inner class)
- Modify: `Application/Handlers/GetRecommendQuery.Green.cs`
- Modify: `Host/Host.csproj` (copy rule for new JSON)
- Test: `Tests/Green/GreenPlayResultHandlerTests.cs` (or a new `GreenRecommendHandlerTests.cs` — see Step 9)

**Acceptance Criteria:**
- [ ] `IGreenCatalog.Recommend` returns a `GreenRecommendEntry`.
- [ ] `GreenRecommendLoader` reads a valid JSON file, filters song IDs by `GreenMusicInfos` membership and `< SongFlagBytes * 8`, and falls back to `GreenRecommendEntry.Empty` if the file is missing.
- [ ] Default `recommend_songs.json` ships in-tree with empty fields (no songs advertised on fresh install).
- [ ] `GetRecommendQuery.Green` returns `Result=1` plus the catalog's recommend song + best list.
- [ ] `gender_type`/`player_age` request fields are accepted and ignored.
- [ ] `Host.csproj` copies the new JSON to the publish output via `CopyToOutputDirectory=PreserveNewest`.
- [ ] Existing `TestGreenCatalog` implements the new member with empty defaults.

**Verify:**
1. `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenRecommendCatalogLoaderTests"` → all pass.
2. `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenRecommend_ReturnsCatalogValues"` → passes.
3. `dotnet build` → solution builds clean.

---

- [ ] **Step 1: Write the failing loader tests**

Create `Tests/Green/GreenRecommendCatalogLoaderTests.cs`:

```csharp
using System.Text.Json;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenRecommendCatalogLoaderTests
{
    [Fact]
    public async Task LoadFromFile_ReturnsConfiguredSongs_FilteredByCatalog()
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, """
                {
                  "recommendSong": 101,
                  "recommendBestSongs": [101, 102, 9999]
                }
                """);

            var catalog = new HashSet<uint> { 101, 102 };
            var entry = await GreenRecommendLoader.LoadFromFileAsync(path, catalog, CancellationToken.None);

            Assert.Equal(101u, entry.RecommendSong);
            Assert.Equal(new uint[] { 101, 102 }, entry.RecommendBestSongs);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task LoadFromFile_DropsRecommendSongWhenOutOfCatalog()
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, """
                {
                  "recommendSong": 9999,
                  "recommendBestSongs": []
                }
                """);

            var catalog = new HashSet<uint> { 101, 102 };
            var entry = await GreenRecommendLoader.LoadFromFileAsync(path, catalog, CancellationToken.None);

            Assert.Equal(0u, entry.RecommendSong);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task LoadFromFile_MissingFileReturnsEmpty()
    {
        var entry = await GreenRecommendLoader.LoadFromFileAsync(
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()),
            new HashSet<uint> { 101 },
            CancellationToken.None);

        Assert.Equal(0u, entry.RecommendSong);
        Assert.Empty(entry.RecommendBestSongs);
    }

    [Fact]
    public async Task LoadFromFile_DropsSongIdsOutsideTableRange()
    {
        var path = Path.GetTempFileName();
        try
        {
            // SongFlagBytes is 128 → SongFlagBytes * 8 = 1024. ID 1024 must be dropped.
            await File.WriteAllTextAsync(path, """
                {
                  "recommendSong": 1024,
                  "recommendBestSongs": [1023, 1024]
                }
                """);

            var catalog = new HashSet<uint> { 1023, 1024 };
            var entry = await GreenRecommendLoader.LoadFromFileAsync(path, catalog, CancellationToken.None);

            Assert.Equal(0u, entry.RecommendSong);
            Assert.Equal(new uint[] { 1023 }, entry.RecommendBestSongs);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
```

- [ ] **Step 2: Run the tests and verify they fail**

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenRecommendCatalogLoaderTests"
```

Expected: COMPILE ERROR — `GreenRecommendLoader` and `GreenRecommendEntry` do not exist.

- [ ] **Step 3: Create the catalog entry**

Create `Application/Catalog/Green/GreenRecommendEntry.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Green;

public sealed class GreenRecommendEntry
{
    public uint RecommendSong { get; init; }

    public IReadOnlyList<uint> RecommendBestSongs { get; init; } = [];

    public static GreenRecommendEntry Empty { get; } = new();
}
```

- [ ] **Step 4: Create the loader**

Create `Infrastructure/GameDataCatalog/Green/GreenRecommendLoader.cs`:

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Application.Common;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

internal sealed class GreenRecommendLoader
{
    public async Task<GreenRecommendEntry> LoadAsync(
        IReadOnlySet<uint> catalogSongIds,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(Domain.Enums.GameEra.Green), "recommend_songs.json");
        return await LoadFromFileAsync(path, catalogSongIds, cancellationToken);
    }

    public static async Task<GreenRecommendEntry> LoadFromFileAsync(
        string path,
        IReadOnlySet<uint> catalogSongIds,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return GreenRecommendEntry.Empty;
        }

        await using var stream = File.OpenRead(path);
        var raw = await JsonSerializer.DeserializeAsync<RawRecommend>(stream, JsonOptions, cancellationToken)
                  ?? new RawRecommend();

        var maxBits = (uint)(GreenProtocolBytes.SongFlagBytes * 8);
        bool IsValid(uint id) => id > 0 && id < maxBits && catalogSongIds.Contains(id);

        var single = IsValid(raw.RecommendSong) ? raw.RecommendSong : 0u;
        var list = (raw.RecommendBestSongs ?? [])
            .Where(IsValid)
            .ToArray();

        return new GreenRecommendEntry
        {
            RecommendSong = single,
            RecommendBestSongs = list
        };
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private sealed class RawRecommend
    {
        [JsonPropertyName("recommendSong")]
        public uint RecommendSong { get; set; }

        [JsonPropertyName("recommendBestSongs")]
        public uint[]? RecommendBestSongs { get; set; }
    }
}
```

Note: `GreenProtocolBytes` lives in `TaikoLocalServer.Application.Common` (already globally imported by `Application/GlobalUsings.cs` and re-exported into `Infrastructure` via that project's globals if needed). If the Infrastructure project doesn't currently glob-import `Application.Common`, prepend `using TaikoLocalServer.Application.Common;` to the file. Verify by checking `Infrastructure/GlobalUsings.cs` (or equivalent) when implementing.

- [ ] **Step 5: Run the loader tests and verify they pass**

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenRecommendCatalogLoaderTests"
```

Expected: all 4 loader tests PASS.

- [ ] **Step 6: Expose `Recommend` on `IGreenCatalog`**

Open `Application/Abstractions/IGreenCatalog.cs`. Add the new member at the end of the interface body:

```csharp
GreenRecommendEntry Recommend { get; }
```

The interface becomes:

```csharp
public interface IGreenCatalog : IEraGameDataCatalog
{
    IReadOnlyList<GreenMusicInfoEntry> MusicInfoFileOrder { get; }
    uint SongHashVersion { get; }
    IReadOnlyDictionary<uint, GreenMusicInfoEntry> GreenMusicInfos { get; }
    IReadOnlyList<GreenTaikojukuEntry> TaikojukuFileOrder { get; }
    IReadOnlyDictionary<uint, GreenTaikojukuEntry> Taikojuku { get; }
    IReadOnlyDictionary<uint, GreenItemShopEntry> ItemShop { get; }
    IReadOnlyDictionary<uint, GreenEventFolderEntry> EventFolders { get; }
    IReadOnlyDictionary<uint, GreenTelopEntry> Telops { get; }
    IReadOnlyDictionary<uint, GreenGachaEntry> Gachas { get; }
    IReadOnlyDictionary<uint, GreenTournamentEntry> Tournaments { get; }
    GreenRecommendEntry Recommend { get; }
}
```

- [ ] **Step 7: Wire the loader into `GreenEraGameDataCatalog`**

Open `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`. Add a private backing field, public property, and load call.

After the existing `private IReadOnlyDictionary<uint, GreenTournamentEntry> tournaments = ...` line, add:

```csharp
private GreenRecommendEntry recommend = GreenRecommendEntry.Empty;
```

After the public `Tournaments` property, add:

```csharp
public GreenRecommendEntry Recommend => recommend;
```

In `InitializeAsync`, immediately after the `tournaments = await new GreenTournamentLoader().LoadAsync(cancellationToken);` line and before the `logger.LogInformation(...)` call, add:

```csharp
recommend = await new GreenRecommendLoader().LoadAsync(
    new HashSet<uint>(musicInfos.Keys),
    cancellationToken);
```

The loader uses `musicInfos.Keys` (already populated earlier in this method) as the catalog whitelist.

- [ ] **Step 8: Update the test fixture's `TestGreenCatalog`**

Open `Tests/Green/GreenHandlerFixture.cs`. In the `TestGreenCatalog` inner class, add a new property near the other catalog members (e.g. right after the `Tournaments` property):

```csharp
public GreenRecommendEntry Recommend { get; init; } = GreenRecommendEntry.Empty;
```

This keeps existing fixtures compiling. Tests that exercise recommend behavior will construct a `TestGreenCatalog { Recommend = new GreenRecommendEntry { ... } }` inline.

- [ ] **Step 9: Write the failing `GetRecommendQuery` test**

Append to `Tests/Green/GreenPlayResultHandlerTests.cs` (it's the existing Green handler test class; we co-locate this test there to avoid a new file for one fact — but if you prefer, create `Tests/Green/GreenRecommendHandlerTests.cs` with the same content):

```csharp
[Fact]
public async Task GreenRecommend_ReturnsCatalogValues()
{
    var greenCatalog = new GreenHandlerFixture.TestGreenCatalog
    {
        Recommend = new GreenRecommendEntry
        {
            RecommendSong = 101,
            RecommendBestSongs = [101, 102]
        }
    };
    await using var fixture = await GreenHandlerFixture.CreateAsync(greenCatalog);

    var handler = new GetRecommendQueryHandler(
        NullLogger<GetRecommendQueryHandler>.Instance,
        fixture.Catalog);

    var response = await handler.Handle(
        new GetRecommendQuery(GenderType: 0, PlayerAge: 0),
        CancellationToken.None);

    Assert.Equal(1u, response.Result);
    Assert.Equal(101u, response.RecommendSong);
    Assert.Equal(new List<uint> { 101, 102 }, response.RecommendBestSong);
}
```

`TestGreenCatalog` is declared `internal sealed` in `GreenHandlerFixture.cs`; tests in the same assembly can reach it. `GetRecommendQuery` is `public readonly record struct GetRecommendQuery(uint GenderType, uint PlayerAge)` in `Application/Handlers/GetRecommendQuery.cs` — positional, so use the constructor syntax shown in the test (not object-initializer).

The current `GetRecommendQueryHandler` primary constructor only accepts `ILogger<GetRecommendQueryHandler> logger`. Step 11 adds `IGameDataCatalog gameDataService` to that primary constructor — the test instantiates the handler with both arguments in the order shown above.

- [ ] **Step 10: Run the test and verify it fails**

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenRecommend_ReturnsCatalogValues"
```

Expected: FAIL — current `GetRecommendQuery.Green.cs` returns an empty `CommonRecommendResponse`.

- [ ] **Step 11a: Add `IGameDataCatalog` to the handler's primary constructor**

Open `Application/Handlers/GetRecommendQuery.cs`. The current line is:

```csharp
public partial class GetRecommendQueryHandler(ILogger<GetRecommendQueryHandler> logger)
    : IRequestHandler<GetRecommendQuery, CommonRecommendResponse>
```

Replace with:

```csharp
public partial class GetRecommendQueryHandler(
    ILogger<GetRecommendQueryHandler> logger,
    IGameDataCatalog gameDataService)
    : IRequestHandler<GetRecommendQuery, CommonRecommendResponse>
```

`logger` stays first because the Green partial currently uses `logger.LogInformation(...)`. The new field `gameDataService` is consumed by the rewritten partial below.

- [ ] **Step 11b: Rewrite `GetRecommendQuery.Green.cs`**

Open `Application/Handlers/GetRecommendQuery.Green.cs`. Replace the entire file body with:

```csharp
namespace TaikoLocalServer.Application.Handlers;

public partial class GetRecommendQueryHandler
{
    public partial ValueTask<CommonRecommendResponse> Handle(GetRecommendQuery request, CancellationToken cancellationToken)
    {
        var green = gameDataService.Green();
        return ValueTask.FromResult(new CommonRecommendResponse
        {
            Result = 1,
            RecommendSong = green.Recommend.RecommendSong,
            RecommendBestSong = green.Recommend.RecommendBestSongs.ToList()
        });
    }
}
```

The previous `logger.LogInformation(...)` stub call is removed; the per-request log line is no longer useful now that the response is meaningful.

- [ ] **Step 12: Add the default `recommend_songs.json` and copy rule**

Create `Host/wwwroot/data/green/recommend_songs.json` with the empty default:

```json
{
  "recommendSong": 0,
  "recommendBestSongs": []
}
```

Open `Host/Host.csproj`. Find the `<!--Green Game Datatables-->` section near the bottom. Add one line just below the existing `musicmedleyinfo.xml` line:

```xml
<Content Update="wwwroot\data\green\recommend_songs.json"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></Content>
```

- [ ] **Step 13: Run the handler test and verify it passes**

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenRecommend_ReturnsCatalogValues"
```

Expected: PASS.

- [ ] **Step 14: Build the full solution**

```bash
dotnet build
```

Expected: build succeeds with no errors. Warnings tolerated only if unrelated to the new files.

- [ ] **Step 15: Commit Task 6**

```bash
git add Application/Catalog/Green/GreenRecommendEntry.cs Infrastructure/GameDataCatalog/Green/GreenRecommendLoader.cs Application/Abstractions/IGreenCatalog.cs Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs Tests/Green/GreenHandlerFixture.cs Tests/Green/GreenRecommendCatalogLoaderTests.cs Tests/Green/GreenPlayResultHandlerTests.cs Application/Handlers/GetRecommendQuery.Green.cs Host/wwwroot/data/green/recommend_songs.json Host/Host.csproj
git commit -m "Add Green recommend songs catalog and wire recommend.php"
```
