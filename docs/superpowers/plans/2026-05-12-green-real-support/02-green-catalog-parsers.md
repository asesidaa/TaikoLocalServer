# 02 - Green Catalog Parsers

**Surface:** Replace stub Green catalog loaders with XML parsers for `musicinfo.xml` and `musicmedleyinfo.xml`, then wire required-file startup behavior.

**Why after 01:** Catalog tests use the helper project and later Green handlers depend on parsed song IDs and medley packs.

**Files:**
- Modify: `Application/Catalog/Green/GreenMusicInfoEntry.cs`
- Modify: `Application/Catalog/Green/GreenTaikojukuEntry.cs`
- Modify: `Application/Abstractions/IGreenCatalog.cs`
- Modify: `Infrastructure/GameDataCatalog/Green/GreenMusicInfoLoader.cs`
- Modify: `Infrastructure/GameDataCatalog/Green/GreenTaikojukuLoader.cs`
- Modify: `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`
- Create: `Infrastructure/GameDataCatalog/Green/GreenRequiredDataFiles.cs`
- Create: `Tests/Green/GreenCatalogLoaderTests.cs`

---

## Task 02.1: Expand Green Catalog Entry Types

**Acceptance Criteria:**
- [ ] `GreenMusicInfoEntry` captures fields needed by the spec.
- [ ] `GreenTaikojukuEntry` carries both medley unique ID and challenge level.

**Steps:**

- [ ] **Step 1: Update `Application/Catalog/Green/GreenMusicInfoEntry.cs`**

```csharp
using TaikoLocalServer.Application.Abstractions;

namespace TaikoLocalServer.Application.Catalog.Green;

public sealed class GreenMusicInfoEntry : IMusicInfoEntry
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
    public uint DemoPlay { get; init; }
    public IReadOnlyList<uint> Tags { get; init; } = [];
    public int FileOrder { get; init; }
}
```

- [ ] **Step 2: Update `Application/Catalog/Green/GreenTaikojukuEntry.cs`**

```csharp
namespace TaikoLocalServer.Application.Catalog.Green;

public sealed class GreenTaikojukuEntry
{
    public uint UniqueId { get; init; }
    public uint ChallengeLevel { get; init; }
    public string Name { get; init; } = string.Empty;
    public uint Difficulty { get; init; }
    public uint VerupNo { get; init; }
    public IReadOnlyList<GreenTaikojukuSong> Songs { get; init; } = [];
}

public sealed class GreenTaikojukuSong
{
    public string MusicId { get; init; } = string.Empty;
    public uint SongNo { get; init; }
    public uint Level { get; init; }
    public uint Notes { get; init; }
}
```

- [ ] **Step 3: Update `Application/Abstractions/IGreenCatalog.cs`**

Add ordered convenience properties:

```csharp
IReadOnlyList<GreenMusicInfoEntry> MusicInfoFileOrder { get; }
uint SongHashVersion { get; }
IReadOnlyList<GreenTaikojukuEntry> TaikojukuFileOrder { get; }
```

- [ ] **Step 4: Build**

Run: `dotnet build`

Expected: FAIL only if existing Green catalog implementation has not been updated for the new interface members. Continue to Task 02.2 before committing.

---

## Task 02.2: Parse `musicinfo.xml`

**Acceptance Criteria:**
- [ ] Parser reads the header version.
- [ ] Parser returns entries in file order.
- [ ] The first parsed entry has `SongNo = uniqueid`.

**Steps:**

- [ ] **Step 1: Add tests**

Create `Tests/Green/GreenCatalogLoaderTests.cs`:

```csharp
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenCatalogLoaderTests
{
    [Fact]
    public async Task MusicInfoLoader_ReadsVersionAndFileOrderSongs()
    {
        var repoRoot = FindRepoRoot();
        var file = Path.Combine(repoRoot, "Host", "wwwroot", "data", "green", "datatable", "musicinfo.xml");

        var result = await GreenMusicInfoLoader.LoadFromFileAsync(file, CancellationToken.None);

        Assert.True(result.SongHashVersion > 0);
        Assert.True(result.Entries.Count >= 20);
        Assert.Equal((uint)873, result.Entries[0].SongNo);
        Assert.Equal("ynzums", result.Entries[0].MusicId);
        Assert.Equal(0, result.Entries[0].FileOrder);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
```

- [ ] **Step 2: Run focused test and confirm failure**

Run: `dotnet test --filter MusicInfoLoader_ReadsVersionAndFileOrderSongs`

Expected: FAIL because `LoadFromFileAsync` does not exist.

- [ ] **Step 3: Replace `GreenMusicInfoLoader.cs`**

```csharp
using System.Xml.Linq;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed record GreenMusicInfoLoadResult(
    uint SongHashVersion,
    IReadOnlyList<GreenMusicInfoEntry> Entries);

public sealed class GreenMusicInfoLoader
{
    public Task<GreenMusicInfoLoadResult> LoadAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataTablePath(GameEra.Green), "musicinfo.xml");
        return LoadFromFileAsync(path, cancellationToken);
    }

    public static async Task<GreenMusicInfoLoadResult> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
        var root = document.Root ?? throw new InvalidDataException($"Missing root element in {path}");

        var header = root.Element("MusicInfo")?.Element("Header")
            ?? throw new InvalidDataException($"Missing MusicInfo/Header in {path}");
        var version = ParseUInt(header.Element("version")?.Value);

        var entries = root.Element("MusicInfo")!
            .Elements("Data")
            .Select((element, index) => new GreenMusicInfoEntry
            {
                MusicId = ReadString(element, "musicid"),
                SongNo = ReadUInt(element, "uniqueid"),
                NewRelease = ReadUInt(element, "newrelease"),
                IsSecret = ReadUInt(element, "secret") != 0,
                IsPapaMama = ReadUInt(element, "papamama") != 0,
                HasExtreme = ReadUInt(element, "hasextreme") != 0,
                PartsSet = ReadString(element, "partsset"),
                WaiwaiPartsSet = ReadString(element, "wai2partsset"),
                Title = ReadString(element, "musicname"),
                GenreName = ReadString(element, "genrename"),
                DemoPlay = ReadUInt(element, "demoplay"),
                Tags = element.Elements("tag").Select(tag => ParseUInt(tag.Value)).ToArray(),
                FileOrder = index
            })
            .ToArray();

        return new GreenMusicInfoLoadResult(version, entries);
    }

    private static string ReadString(XContainer element, string name)
        => element.Element(name)?.Value ?? string.Empty;

    private static uint ReadUInt(XContainer element, string name)
        => ParseUInt(element.Element(name)?.Value);

    private static uint ParseUInt(string? value)
        => uint.TryParse(value, out var parsed) ? parsed : 0;
}
```

- [ ] **Step 4: Run focused test**

Run: `dotnet test --filter MusicInfoLoader_ReadsVersionAndFileOrderSongs`

Expected: PASS.

---

## Task 02.3: Parse `musicmedleyinfo.xml`

**Acceptance Criteria:**
- [ ] Parser reads medley entries.
- [ ] Each parsed pack includes song number and level from `Content`.
- [ ] The first pack has at least one song.

**Steps:**

- [ ] **Step 1: Add test**

Append to `GreenCatalogLoaderTests`:

```csharp
[Fact]
public async Task TaikojukuLoader_ReadsMedleyPacks()
{
    var repoRoot = FindRepoRoot();
    var file = Path.Combine(repoRoot, "Host", "wwwroot", "data", "green", "datatable", "musicmedleyinfo.xml");

    var entries = await GreenTaikojukuLoader.LoadFromFileAsync(file, CancellationToken.None);

    Assert.NotEmpty(entries);
    Assert.True(entries[0].UniqueId > 0);
    Assert.True(entries[0].ChallengeLevel > 0);
    Assert.NotEmpty(entries[0].Songs);
    Assert.True(entries[0].Songs[0].SongNo > 0);
}
```

- [ ] **Step 2: Run focused test and confirm failure**

Run: `dotnet test --filter TaikojukuLoader_ReadsMedleyPacks`

Expected: FAIL because `LoadFromFileAsync` does not exist.

- [ ] **Step 3: Replace `GreenTaikojukuLoader.cs`**

```csharp
using System.Xml.Linq;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenTaikojukuLoader
{
    public Task<IReadOnlyList<GreenTaikojukuEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataTablePath(GameEra.Green), "musicmedleyinfo.xml");
        return LoadFromFileAsync(path, cancellationToken);
    }

    public static async Task<IReadOnlyList<GreenTaikojukuEntry>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
        var root = document.Root ?? throw new InvalidDataException($"Missing root element in {path}");

        return root.Elements("MusicMedleyInfoData")
            .Select(element => new GreenTaikojukuEntry
            {
                UniqueId = ReadUInt(element, "uniqueid"),
                Name = ReadString(element, "medleyname"),
                Difficulty = ReadUInt(element, "difficulty"),
                ChallengeLevel = ReadUInt(element, "challengelv"),
                Songs = element.Elements("Content")
                    .Select(content => new GreenTaikojukuSong
                    {
                        MusicId = ReadString(content, "musicid"),
                        SongNo = ReadUInt(content, "uniqueid"),
                        Level = ReadUInt(content, "difficulty"),
                        Notes = ReadUInt(content, "notes")
                    })
                    .ToArray()
            })
            .ToArray();
    }

    private static string ReadString(XContainer element, string name)
        => element.Element(name)?.Value ?? string.Empty;

    private static uint ReadUInt(XContainer element, string name)
        => uint.TryParse(element.Element(name)?.Value, out var parsed) ? parsed : 0;
}
```

- [ ] **Step 4: Run focused test**

Run: `dotnet test --filter TaikojukuLoader_ReadsMedleyPacks`

Expected: PASS.

---

## Task 02.4: Wire `GreenEraGameDataCatalog`

**Acceptance Criteria:**
- [ ] Green catalog exposes song hash version, music entries by ID, music file order, Taikojuku dictionaries, and Taikojuku file order.
- [ ] Missing required files fail initialization.

**Steps:**

- [ ] **Step 1: Create `GreenRequiredDataFiles.cs`**

```csharp
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

internal static class GreenRequiredDataFiles
{
    public static IReadOnlyList<string> GetRequiredPaths()
    {
        var datatablePath = PathHelper.GetDataTablePath(GameEra.Green);
        return
        [
            Path.Combine(datatablePath, "musicinfo.xml"),
            Path.Combine(datatablePath, "musicmedleyinfo.xml")
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

- [ ] **Step 2: Rewrite `GreenEraGameDataCatalog.cs` fields/properties**

Use this shape:

```csharp
private uint songHashVersion;
private IReadOnlyList<GreenMusicInfoEntry> musicInfoFileOrder = [];
private IReadOnlyDictionary<uint, GreenMusicInfoEntry> musicInfos = new Dictionary<uint, GreenMusicInfoEntry>();
private IReadOnlyDictionary<uint, IMusicInfoEntry> sharedMusicInfos = new Dictionary<uint, IMusicInfoEntry>();
private IReadOnlyList<GreenTaikojukuEntry> taikojukuFileOrder = [];
private IReadOnlyDictionary<uint, GreenTaikojukuEntry> taikojuku = new Dictionary<uint, GreenTaikojukuEntry>();

public uint SongHashVersion => songHashVersion;
public IReadOnlyList<GreenMusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;
public IReadOnlyDictionary<uint, GreenMusicInfoEntry> GreenMusicInfos => musicInfos;
public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos => sharedMusicInfos;
public IReadOnlyList<GreenTaikojukuEntry> TaikojukuFileOrder => taikojukuFileOrder;
public IReadOnlyDictionary<uint, GreenTaikojukuEntry> Taikojuku => taikojuku;
```

- [ ] **Step 3: Rewrite `InitializeAsync` catalog loading**

```csharp
public async Task InitializeAsync(CancellationToken cancellationToken)
{
    GreenRequiredDataFiles.ThrowIfMissing();

    var musicInfo = await new GreenMusicInfoLoader().LoadAsync(cancellationToken);
    songHashVersion = musicInfo.SongHashVersion;
    musicInfoFileOrder = musicInfo.Entries;
    musicInfos = musicInfo.Entries.ToDictionary(entry => entry.SongNo);
    sharedMusicInfos = musicInfos.ToDictionary(
        pair => pair.Key,
        pair => (IMusicInfoEntry)pair.Value);

    taikojukuFileOrder = await new GreenTaikojukuLoader().LoadAsync(cancellationToken);
    taikojuku = taikojukuFileOrder
        .GroupBy(entry => entry.UniqueId)
        .ToDictionary(group => group.Key, group => group.First());

    itemShop = await new GreenItemShopLoader().LoadAsync(cancellationToken);
    eventFolders = await new GreenEventFolderLoader().LoadAsync(cancellationToken);
    telops = await new GreenTelopLoader().LoadAsync(cancellationToken);
    gachas = await new GreenGachaLoader().LoadAsync(cancellationToken);
    tournaments = await new GreenTournamentLoader().LoadAsync(cancellationToken);

    logger.LogInformation(
        "Loaded Green catalog: {SongCount} songs, song_hash_ver={SongHashVersion}, {TaikojukuCount} taikojuku packs",
        musicInfoFileOrder.Count,
        songHashVersion,
        taikojukuFileOrder.Count);
}
```

- [ ] **Step 4: Run tests and build**

Run:

```bash
dotnet test --filter GreenCatalogLoaderTests
dotnet build
```

Expected: both PASS.

- [ ] **Step 5: Commit**

```bash
git add Application/Catalog/Green Application/Abstractions/IGreenCatalog.cs Infrastructure/GameDataCatalog/Green Tests/Green/GreenCatalogLoaderTests.cs
git commit -m "feat(green): parse Green music and taikojuku datatables"
```
