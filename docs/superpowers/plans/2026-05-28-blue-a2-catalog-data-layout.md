# Blue A2 Catalog Data Layout Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a first-class Blue normal-data catalog surface, backed by the observed AC15 Blue data layout, without implementing Blue gameplay handlers or battle semantics.

**Architecture:** Extract shared AC15 file-format parsing for the Green/Blue-matched XML, tuning, and simple JSON catalog contracts, then keep era-specific catalog facades (`IGreenCatalog`, `IBlueCatalog`) and era-specific record types. Blue loads from `wwwroot/data/blue/data/config/S10100-1` and Blue-owned JSON file names, registers through the existing `IGameDataCatalog` multiplexer only when Blue is enabled, and leaves `config/S10100-1/battle` documented but unparsed.

**Tech Stack:** .NET 10, C#, xUnit, Microsoft.Extensions.Options validation, ASP.NET Core solution layout, PowerShell, `TaikoLocalServer.slnx`.

---

## Preconditions

- Work from the repo root: `H:\TaikoLocalServer`.
- Inspect current state before editing:

```powershell
git status --short
git diff --stat
git diff --cached --name-status
```

Expected at plan-writing time: clean worktree after commit `5a7cd353 Add Blue A2 catalog data layout spec`.

- Do not parse or expose `Host/wwwroot/data/blue/data/config/S10100-1/battle/*` in this plan.
- Do not replace Green's public catalog behavior. Green tests must continue to pass.
- Keep commits narrow by task. If local ignored Blue data exists under `Host/wwwroot/data/blue/data`, leave it untracked.

## File Structure

Shared AC15 format parsing:

- `Application/Catalog/Ac15/Ac15MusicInfoEntry.cs` - neutral music-info row parsed from AC15 `musicinfo.xml`.
- `Application/Catalog/Ac15/Ac15MusicInfoLoadResult.cs` - neutral music-info result with `SongHashVersion`.
- `Application/Catalog/Ac15/Ac15TaikojukuEntry.cs` - neutral medley/Dani rows parsed from AC15 `musicmedleyinfo.xml`.
- `Application/Catalog/Ac15/Ac15StarSet.cs` - neutral star set parsed from AC15 `tuning.bin`.
- `Application/Catalog/Ac15/Ac15ItemShopCatalog.cs`, `Ac15ItemShopSeason.cs`, `Ac15ItemShopEntry.cs` - neutral item-shop catalog.
- `Application/Catalog/Ac15/Ac15RecommendEntry.cs`, `Ac15TelopEntry.cs` - neutral simple JSON catalog rows.
- `Infrastructure/GameDataCatalog/Ac15/Ac15MusicInfoLoader.cs` - XML parser moved out of Green.
- `Infrastructure/GameDataCatalog/Ac15/Ac15TaikojukuLoader.cs` - XML medley parser moved out of Green.
- `Infrastructure/GameDataCatalog/Ac15/Ac15TuningLoader.cs` - binary tuning parser moved out of Green.
- `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs`, `Ac15RecommendLoader.cs`, `Ac15TelopLoader.cs`, `Ac15EventFolderLoader.cs`, `Ac15MovieLoader.cs`, `Ac15CustomizationCatalogLoader.cs` - shared optional catalog helpers that accept explicit paths and era labels.

Green compatibility wrappers:

- `Infrastructure/GameDataCatalog/Green/GreenMusicInfoLoader.cs` - maps AC15 rows to existing Green records.
- `Infrastructure/GameDataCatalog/Green/GreenTaikojukuLoader.cs` - maps AC15 rows to existing Green records.
- `Infrastructure/GameDataCatalog/Green/GreenTuningLoader.cs` - maps AC15 star sets to existing `GreenStarSet`.
- `Infrastructure/GameDataCatalog/Green/GreenItemShopLoader.cs`, `GreenRecommendLoader.cs`, `GreenTelopLoader.cs`, `GreenEventFolderLoader.cs`, `GreenMovieLoader.cs`, `GreenCostumeLoader.cs`, `GreenTitleLoader.cs`, `GreenNeiroLoader.cs` - keep Green file names and public behavior while delegating shared parsing for item shop, recommend, telop, event folder, movie, and customization JSON.

Blue catalog surface:

- `Application/Abstractions/IBlueCatalog.cs` - Blue catalog interface.
- `Application/Catalog/Blue/*.cs` - Blue era catalog records.
- `Infrastructure/GameDataCatalog/Blue/BlueGameDataPaths.cs` - Blue required source paths with `S10100-1`.
- `Infrastructure/GameDataCatalog/Blue/BlueRequiredDataFiles.cs` - fail-fast required file validation.
- `Infrastructure/GameDataCatalog/Blue/Blue*Loader.cs` - Blue wrappers that map shared AC15 records into Blue records and use Blue-owned JSON names.
- `Infrastructure/GameDataCatalog/Blue/BlueEraGameDataCatalog.cs` - Blue `IEraGameDataCatalog` implementation.
- `Infrastructure/DependencyInjection.cs` - registers Blue catalog only when Blue is enabled.

Settings, docs, tests:

- `Application/Settings/ServerSettings.cs` - set `EraSettings.GameDataPath` to a neutral empty-string default.
- `Application/Settings/ServerSettingsOptionsValidationExtensions.cs` - generalize AC15 shop validation for Green and Blue.
- `Host/Configurations/ServerSettings.json` - add explicit Blue data/shop settings.
- `README.md`, `Host/README.md` - document Blue data layout and deferred battle data.
- `Tests/Ac15/Ac15CatalogLoaderTests.cs` - shared parser tests.
- `Tests/Blue/BlueCatalogContractTests.cs`, `BlueCatalogLoaderTests.cs`, `BlueServerSettingsValidationTests.cs`, `BlueInfrastructureRegistrationTests.cs`, `BlueDocsTests.cs` - Blue A2 coverage.
- Existing Green tests under `Tests/Green/` remain the regression suite for compatibility.

---

### Task 1: Extract Shared AC15 XML And Tuning Parsers

**Files:**
- Create: `Application/Catalog/Ac15/Ac15MusicInfoEntry.cs`
- Create: `Application/Catalog/Ac15/Ac15MusicInfoLoadResult.cs`
- Create: `Application/Catalog/Ac15/Ac15TaikojukuEntry.cs`
- Create: `Application/Catalog/Ac15/Ac15StarSet.cs`
- Create: `Infrastructure/GameDataCatalog/Ac15/Ac15MusicInfoLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Ac15/Ac15TaikojukuLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Ac15/Ac15TuningLoader.cs`
- Modify: `Infrastructure/GameDataCatalog/Green/GreenMusicInfoLoader.cs`
- Modify: `Infrastructure/GameDataCatalog/Green/GreenTaikojukuLoader.cs`
- Modify: `Infrastructure/GameDataCatalog/Green/GreenTuningLoader.cs`
- Test: `Tests/Ac15/Ac15CatalogLoaderTests.cs`
- Test: `Tests/Green/GreenCatalogLoaderTests.cs`

- [ ] **Step 1: Write shared AC15 loader tests**

Create `Tests/Ac15/Ac15CatalogLoaderTests.cs`:

```csharp
using System.Buffers.Binary;
using System.Text;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15CatalogLoaderTests
{
    [Fact]
    public async Task MusicInfoLoader_ReadsBoostXmlVersionAndRows()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xml");
        await File.WriteAllTextAsync(path, """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <boost_serialization signature="serialization::archive" version="10">
              <MusicInfo>
                <Header>
                  <signature>TaikoAC15 MusicInfo</signature>
                  <version>12345</version>
                  <size>1</size>
                </Header>
                <Data>
                  <musicid>song_a</musicid>
                  <uniqueid>100</uniqueid>
                  <newrelease>1</newrelease>
                  <secret>0</secret>
                  <papamama>0</papamama>
                  <hasextreme>1</hasextreme>
                  <partsset>taiko</partsset>
                  <wai2partsset>taiko</wai2partsset>
                  <musicname>Song A</musicname>
                  <genrename>J-POP</genrename>
                  <demoplay>2</demoplay>
                  <tag>7</tag>
                  <tag>8</tag>
                </Data>
              </MusicInfo>
            </boost_serialization>
            """);

        try
        {
            var result = await Ac15MusicInfoLoader.LoadFromFileAsync(path, CancellationToken.None);

            Assert.Equal(12345u, result.SongHashVersion);
            var entry = Assert.Single(result.Entries);
            Assert.Equal("song_a", entry.MusicId);
            Assert.Equal(100u, entry.SongNo);
            Assert.True(entry.HasExtreme);
            Assert.Equal(0, entry.FileOrder);
            Assert.Equal([7u, 8u], entry.Tags);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task TaikojukuLoader_ReadsBoostXmlPackAndConditions()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xml");
        await File.WriteAllTextAsync(path, """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <boost_serialization signature="serialization::archive" version="8">
              <MusicMedleyInfoHeader>
                <signature>TaikoAC15 MusicMedleyInfo</signature>
                <version>54321</version>
                <size>1</size>
              </MusicMedleyInfoHeader>
              <MusicMedleyInfoData>
                <uniqueid>20001</uniqueid>
                <medleyname>First Dan</medleyname>
                <difficulty>3</difficulty>
                <challengelv>1</challengelv>
                <Content>
                  <musicid>song_a</musicid>
                  <uniqueid>100</uniqueid>
                  <difficulty>0</difficulty>
                  <notes>74</notes>
                </Content>
                <Conditions>
                  <tamashii>9000</tamashii>
                  <hit_ryo>10</hit_ryo>
                  <hit_ka>20</hit_ka>
                  <hit_fuka>30</hit_fuka>
                  <combo>40</combo>
                  <hits>50</hits>
                  <score>60</score>
                  <renda>70</renda>
                </Conditions>
                <ExcellentConditions>
                  <tamashii>9500</tamashii>
                  <hits>80</hits>
                </ExcellentConditions>
              </MusicMedleyInfoData>
            </boost_serialization>
            """);

        try
        {
            var entries = await Ac15TaikojukuLoader.LoadFromFileAsync(path, CancellationToken.None);

            var entry = Assert.Single(entries);
            Assert.Equal(20001u, entry.UniqueId);
            Assert.Equal(1u, entry.ChallengeLevel);
            Assert.Equal(90u, entry.Conditions.SoulGauge);
            Assert.Equal(50u, entry.Conditions.TotalHitCount);
            Assert.Equal(95u, entry.ExcellentConditions.SoulGauge);
            Assert.Equal(80u, entry.ExcellentConditions.TotalHitCount);
            Assert.Equal(100u, Assert.Single(entry.Songs).SongNo);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task TuningLoader_ReadsHeaderCountAndExRecords()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.bin");
        await File.WriteAllBytesAsync(
            path,
            CreateTuningBin(
                new TuningTestRecord("song_a", 1, 2, 3, 4),
                new TuningTestRecord("ex_song_a", 0, 0, 0, 5)),
            CancellationToken.None);

        try
        {
            var stars = await Ac15TuningLoader.LoadFromFileAsync(path, "Test", CancellationToken.None);

            var starSet = Assert.Contains("song_a", stars);
            Assert.Equal(new Ac15StarSet(1, 2, 3, 4, 5), starSet);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static byte[] CreateTuningBin(params TuningTestRecord[] records)
    {
        const int headerSize = 4;
        const int recordSize = 2_316;
        const int player0CellOffset = 0x10;
        const int difficultyCellStride = 0x80;

        var stringTableOffset = headerSize + records.Length * recordSize;
        var stringTableLength = records.Sum(record => Encoding.ASCII.GetByteCount(record.MusicId) + 1);
        var bytes = new byte[stringTableOffset + stringTableLength];

        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(0, 4), (uint)records.Length);

        var musicIdOffset = 0;
        for (var index = 0; index < records.Length; index++)
        {
            var record = records[index];
            var recordOffset = headerSize + index * recordSize;
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(recordOffset, 4), (uint)musicIdOffset);
            WriteStar(bytes, recordOffset, 0, record.Easy);
            WriteStar(bytes, recordOffset, 1, record.Normal);
            WriteStar(bytes, recordOffset, 2, record.Hard);
            WriteStar(bytes, recordOffset, 3, record.Oni);

            var musicIdBytes = Encoding.ASCII.GetBytes(record.MusicId);
            musicIdBytes.CopyTo(bytes.AsSpan(stringTableOffset + musicIdOffset));
            musicIdOffset += musicIdBytes.Length + 1;
        }

        return bytes;

        static void WriteStar(byte[] bytes, int recordOffset, int difficultyIndex, byte value)
        {
            BinaryPrimitives.WriteUInt32BigEndian(
                bytes.AsSpan(recordOffset + player0CellOffset + difficultyIndex * difficultyCellStride, 4),
                value);
        }
    }

    private sealed record TuningTestRecord(
        string MusicId,
        byte Easy,
        byte Normal,
        byte Hard,
        byte Oni);
}
```

- [ ] **Step 2: Run the failing shared tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~Ac15CatalogLoaderTests
```

Expected: FAIL to compile because the `Application.Catalog.Ac15` and `Infrastructure.GameDataCatalog.Ac15` types do not exist.

- [ ] **Step 3: Add shared AC15 application records**

Create `Application/Catalog/Ac15/Ac15StarSet.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Ac15;

public readonly record struct Ac15StarSet(
    byte Easy,
    byte Normal,
    byte Hard,
    byte Oni,
    byte Ura);
```

Create `Application/Catalog/Ac15/Ac15MusicInfoEntry.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Ac15;

public sealed record class Ac15MusicInfoEntry
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

Create `Application/Catalog/Ac15/Ac15MusicInfoLoadResult.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Ac15;

public sealed record Ac15MusicInfoLoadResult(
    uint SongHashVersion,
    IReadOnlyList<Ac15MusicInfoEntry> Entries);
```

Create `Application/Catalog/Ac15/Ac15TaikojukuEntry.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Ac15;

public sealed class Ac15TaikojukuEntry
{
    public uint UniqueId { get; init; }

    public uint DanLevel { get; init; }

    public uint ChallengeLevel { get; init; }

    public string Name { get; init; } = string.Empty;

    public uint Difficulty { get; init; }

    public uint VerupNo { get; init; }

    public Ac15TaikojukuConditions Conditions { get; init; } = Ac15TaikojukuConditions.Empty;

    public Ac15TaikojukuConditions ExcellentConditions { get; init; } = Ac15TaikojukuConditions.Empty;

    public IReadOnlyList<Ac15TaikojukuSong> Songs { get; init; } = [];
}

public sealed class Ac15TaikojukuConditions
{
    public static Ac15TaikojukuConditions Empty { get; } = new();

    public uint SoulGauge { get; init; }

    public uint GoodCount { get; init; }

    public uint OkCount { get; init; }

    public uint BadCount { get; init; }

    public uint ComboCount { get; init; }

    public uint TotalHitCount { get; init; }

    public uint Score { get; init; }

    public uint DrumrollCount { get; init; }
}

public sealed class Ac15TaikojukuSong
{
    public string MusicId { get; init; } = string.Empty;

    public uint SongNo { get; init; }

    public uint Level { get; init; }

    public uint Notes { get; init; }
}
```

- [ ] **Step 4: Add shared AC15 music-info and taikojuku loaders**

Create `Infrastructure/GameDataCatalog/Ac15/Ac15MusicInfoLoader.cs` by moving the current parser body from `GreenMusicInfoLoader` into this file and changing the result types:

```csharp
using System.Xml.Linq;
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

public sealed class Ac15MusicInfoLoader
{
    public static async Task<Ac15MusicInfoLoadResult> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
        var root = document.Root ?? throw new InvalidDataException($"Missing root element in {path}");

        var musicInfo = root.Element("MusicInfo")
            ?? throw new InvalidDataException($"Missing MusicInfo in {path}");
        var header = musicInfo.Element("Header")
            ?? throw new InvalidDataException($"Missing MusicInfo/Header in {path}");
        var version = ParseUInt(header.Element("version")?.Value);

        var entries = musicInfo
            .Elements("Data")
            .Select((element, index) => new Ac15MusicInfoEntry
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

        return new Ac15MusicInfoLoadResult(version, entries);
    }

    private static string ReadString(XContainer element, string name)
        => element.Element(name)?.Value ?? string.Empty;

    private static uint ReadUInt(XContainer element, string name)
        => ParseUInt(element.Element(name)?.Value);

    private static uint ParseUInt(string? value)
        => uint.TryParse(value, out var parsed) ? parsed : 0;
}
```

Create `Infrastructure/GameDataCatalog/Ac15/Ac15TaikojukuLoader.cs` by moving the current parser body from `GreenTaikojukuLoader` into this file and changing the result types:

```csharp
using System.Xml.Linq;
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

public sealed class Ac15TaikojukuLoader
{
    public static async Task<IReadOnlyList<Ac15TaikojukuEntry>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
        var root = document.Root ?? throw new InvalidDataException($"Missing root element in {path}");

        return root.Elements("MusicMedleyInfoData")
            .Select(element => new Ac15TaikojukuEntry
            {
                UniqueId = ReadUInt(element, "uniqueid"),
                DanLevel = ReadUInt(element, "challengelv"),
                Name = ReadString(element, "medleyname"),
                Difficulty = ReadUInt(element, "difficulty"),
                ChallengeLevel = ReadUInt(element, "challengelv"),
                Conditions = ReadConditions(element.Element("Conditions")),
                ExcellentConditions = ReadConditions(element.Element("ExcellentConditions")),
                Songs = element.Elements("Content")
                    .Select(content => new Ac15TaikojukuSong
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

    private static Ac15TaikojukuConditions ReadConditions(XContainer? element)
    {
        if (element is null)
        {
            return Ac15TaikojukuConditions.Empty;
        }

        return new Ac15TaikojukuConditions
        {
            SoulGauge = ReadUInt(element, "tamashii") / 100,
            GoodCount = ReadUInt(element, "hit_ryo"),
            OkCount = ReadUInt(element, "hit_ka"),
            BadCount = ReadUInt(element, "hit_fuka"),
            ComboCount = ReadUInt(element, "combo"),
            TotalHitCount = ReadUInt(element, "hits"),
            Score = ReadUInt(element, "score"),
            DrumrollCount = ReadUInt(element, "renda")
        };
    }
}
```

- [ ] **Step 5: Add shared AC15 tuning loader**

Create `Infrastructure/GameDataCatalog/Ac15/Ac15TuningLoader.cs` by moving the current binary parser body from `GreenTuningLoader` and parameterizing error text by era label:

```csharp
using System.Buffers.Binary;
using System.Text;
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

public sealed class Ac15TuningLoader
{
    private const int HeaderSize = 4;
    private const int RecordSize = 2_316;
    private const int Player0CellOffset = 0x10;
    private const int DifficultyCellStride = 0x80;
    private const string ExPrefix = "ex_";

    public static async Task<IReadOnlyDictionary<string, Ac15StarSet>> LoadFromFileAsync(
        string path,
        string eraName,
        CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
        return Parse(bytes, path, eraName);
    }

    private static IReadOnlyDictionary<string, Ac15StarSet> Parse(byte[] bytes, string path, string eraName)
    {
        var recordCount = ReadRecordCount(bytes, path, eraName);
        var stringTableOffset = HeaderSize + recordCount * RecordSize;

        var baseRecords = new Dictionary<string, TuningCourseStars>(StringComparer.Ordinal);
        var exRecords = new Dictionary<string, byte>(StringComparer.Ordinal);

        for (var index = 0; index < recordCount; index++)
        {
            var recordOffset = HeaderSize + index * RecordSize;
            var musicIdOffset = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(recordOffset, 4));
            var musicId = ReadMusicId(bytes, stringTableOffset, musicIdOffset, path, eraName, index);
            if (musicId.StartsWith(ExPrefix, StringComparison.Ordinal))
            {
                exRecords[musicId[ExPrefix.Length..]] = ReadStar(bytes, recordOffset, 3, path, eraName, index);
            }
            else
            {
                var stars = new TuningCourseStars(
                    ReadStar(bytes, recordOffset, 0, path, eraName, index),
                    ReadStar(bytes, recordOffset, 1, path, eraName, index),
                    ReadStar(bytes, recordOffset, 2, path, eraName, index),
                    ReadStar(bytes, recordOffset, 3, path, eraName, index));

                baseRecords[musicId] = stars;
            }
        }

        var result = new Dictionary<string, Ac15StarSet>(baseRecords.Count, StringComparer.Ordinal);
        foreach (var pair in baseRecords)
        {
            var baseStars = pair.Value;
            result[pair.Key] = new Ac15StarSet(
                baseStars.Easy,
                baseStars.Normal,
                baseStars.Hard,
                baseStars.Oni,
                exRecords.TryGetValue(pair.Key, out var uraStar) ? uraStar : (byte)0);
        }

        return result;
    }

    private static int ReadRecordCount(byte[] bytes, string path, string eraName)
    {
        if (bytes.Length < HeaderSize)
        {
            throw new InvalidDataException(
                $"Invalid {eraName} tuning.bin size for {path}: expected at least {HeaderSize} bytes for the header, actual {bytes.Length} bytes.");
        }

        var songCount = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(0, 4));
        var requiredRecordTableSize = HeaderSize + (long)songCount * RecordSize;
        if (requiredRecordTableSize > bytes.Length)
        {
            throw new InvalidDataException(
                $"Invalid {eraName} tuning.bin size for {path}: song_count 0x{songCount:X8} ({songCount}) requires at least {requiredRecordTableSize} bytes for the record table, actual {bytes.Length} bytes.");
        }

        return checked((int)songCount);
    }

    private static string ReadMusicId(byte[] bytes, int stringTableOffset, uint musicIdOffset, string path, string eraName, int recordIndex)
    {
        var stringTableLength = bytes.Length - stringTableOffset;
        if (musicIdOffset >= (uint)stringTableLength)
        {
            throw new InvalidDataException(
                $"Invalid {eraName} tuning.bin musicid offset in record {recordIndex} of {path}: string table offset 0x{musicIdOffset:X8} is outside the {stringTableLength}-byte string table.");
        }

        var absoluteOffset = stringTableOffset + (int)musicIdOffset;
        var end = Array.IndexOf(bytes, (byte)0, absoluteOffset);
        if (end < 0)
        {
            throw new InvalidDataException(
                $"Invalid {eraName} tuning.bin musicid in record {recordIndex} of {path}: missing null terminator after file offset 0x{absoluteOffset:X8}.");
        }

        return Encoding.ASCII.GetString(bytes, absoluteOffset, end - absoluteOffset);
    }

    private static byte ReadStar(byte[] bytes, int recordOffset, int difficultyIndex, string path, string eraName, int recordIndex)
    {
        var offset = recordOffset + Player0CellOffset + difficultyIndex * DifficultyCellStride;
        var value = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset, 4));
        if (value > byte.MaxValue)
        {
            throw new InvalidDataException(
                $"Invalid {eraName} tuning.bin star value in record {recordIndex}, difficulty {difficultyIndex}, offset 0x{offset:X8} of {path}: {value} does not fit in a byte.");
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

- [ ] **Step 6: Replace Green loaders with compatibility wrappers**

Modify `Infrastructure/GameDataCatalog/Green/GreenMusicInfoLoader.cs` so it delegates to `Ac15MusicInfoLoader`:

```csharp
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed record GreenMusicInfoLoadResult(
    uint SongHashVersion,
    IReadOnlyList<GreenMusicInfoEntry> Entries);

public sealed class GreenMusicInfoLoader
{
    public Task<GreenMusicInfoLoadResult> LoadAsync(CancellationToken cancellationToken)
    {
        return LoadFromFileAsync(GreenGameDataPaths.MusicInfoXml, cancellationToken);
    }

    public static async Task<GreenMusicInfoLoadResult> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var result = await Ac15MusicInfoLoader.LoadFromFileAsync(path, cancellationToken);
        return new GreenMusicInfoLoadResult(
            result.SongHashVersion,
            result.Entries.Select(Map).ToArray());
    }

    private static GreenMusicInfoEntry Map(Ac15MusicInfoEntry entry) => new()
    {
        MusicId = entry.MusicId,
        SongNo = entry.SongNo,
        NewRelease = entry.NewRelease,
        IsSecret = entry.IsSecret,
        IsPapaMama = entry.IsPapaMama,
        HasExtreme = entry.HasExtreme,
        PartsSet = entry.PartsSet,
        WaiwaiPartsSet = entry.WaiwaiPartsSet,
        Title = entry.Title,
        GenreName = entry.GenreName,
        DemoPlay = entry.DemoPlay,
        Tags = entry.Tags,
        FileOrder = entry.FileOrder
    };
}
```

Modify `Infrastructure/GameDataCatalog/Green/GreenTaikojukuLoader.cs` so it delegates to `Ac15TaikojukuLoader` and maps every nested type:

```csharp
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenTaikojukuLoader
{
    public Task<IReadOnlyList<GreenTaikojukuEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        return LoadFromFileAsync(GreenGameDataPaths.MusicMedleyInfoXml, cancellationToken);
    }

    public static async Task<IReadOnlyList<GreenTaikojukuEntry>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var entries = await Ac15TaikojukuLoader.LoadFromFileAsync(path, cancellationToken);
        return entries.Select(Map).ToArray();
    }

    private static GreenTaikojukuEntry Map(Ac15TaikojukuEntry entry) => new()
    {
        UniqueId = entry.UniqueId,
        DanLevel = entry.DanLevel,
        ChallengeLevel = entry.ChallengeLevel,
        Name = entry.Name,
        Difficulty = entry.Difficulty,
        VerupNo = entry.VerupNo,
        Conditions = Map(entry.Conditions),
        ExcellentConditions = Map(entry.ExcellentConditions),
        Songs = entry.Songs.Select(song => new GreenTaikojukuSong
        {
            MusicId = song.MusicId,
            SongNo = song.SongNo,
            Level = song.Level,
            Notes = song.Notes
        }).ToArray()
    };

    private static GreenTaikojukuConditions Map(Ac15TaikojukuConditions conditions) => new()
    {
        SoulGauge = conditions.SoulGauge,
        GoodCount = conditions.GoodCount,
        OkCount = conditions.OkCount,
        BadCount = conditions.BadCount,
        ComboCount = conditions.ComboCount,
        TotalHitCount = conditions.TotalHitCount,
        Score = conditions.Score,
        DrumrollCount = conditions.DrumrollCount
    };
}
```

Modify `Infrastructure/GameDataCatalog/Green/GreenTuningLoader.cs` so it delegates to `Ac15TuningLoader`:

```csharp
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenTuningLoader
{
    public Task<IReadOnlyDictionary<string, GreenStarSet>> LoadAsync(CancellationToken cancellationToken)
    {
        return LoadFromFileAsync(GreenGameDataPaths.TuningBin, cancellationToken);
    }

    public static async Task<IReadOnlyDictionary<string, GreenStarSet>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var stars = await Ac15TuningLoader.LoadFromFileAsync(path, nameof(GameEra.Green), cancellationToken);
        return stars.ToDictionary(
            pair => pair.Key,
            pair => Map(pair.Value),
            StringComparer.Ordinal);
    }

    private static GreenStarSet Map(Ac15StarSet starSet)
        => new(starSet.Easy, starSet.Normal, starSet.Hard, starSet.Oni, starSet.Ura);
}
```

Add `using TaikoLocalServer.Domain.Enums;` to `GreenTuningLoader.cs` if it is not available through globals.

- [ ] **Step 7: Run shared and Green parser tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15CatalogLoaderTests Or FullyQualifiedName~GreenCatalogLoaderTests"
```

Expected: PASS.

- [ ] **Step 8: Commit shared parser extraction**

Run:

```powershell
git diff -- Application/Catalog/Ac15 Infrastructure/GameDataCatalog/Ac15 Infrastructure/GameDataCatalog/Green/GreenMusicInfoLoader.cs Infrastructure/GameDataCatalog/Green/GreenTaikojukuLoader.cs Infrastructure/GameDataCatalog/Green/GreenTuningLoader.cs Tests/Ac15/Ac15CatalogLoaderTests.cs Tests/Green/GreenCatalogLoaderTests.cs
git add -- Application/Catalog/Ac15 Infrastructure/GameDataCatalog/Ac15 Infrastructure/GameDataCatalog/Green/GreenMusicInfoLoader.cs Infrastructure/GameDataCatalog/Green/GreenTaikojukuLoader.cs Infrastructure/GameDataCatalog/Green/GreenTuningLoader.cs Tests/Ac15/Ac15CatalogLoaderTests.cs Tests/Green/GreenCatalogLoaderTests.cs
git commit -m "Extract shared AC15 catalog parsers"
```

---

### Task 2: Add Blue Catalog Contracts, Records, And Required Paths

**Files:**
- Create: `Application/Abstractions/IBlueCatalog.cs`
- Create: `Application/Catalog/Blue/BlueMusicInfoEntry.cs`
- Create: `Application/Catalog/Blue/BlueTaikojukuEntry.cs`
- Create: `Application/Catalog/Blue/BlueItemShopCatalog.cs`
- Create: `Application/Catalog/Blue/BlueItemShopSeason.cs`
- Create: `Application/Catalog/Blue/BlueItemShopEntry.cs`
- Create: `Application/Catalog/Blue/BlueRecommendEntry.cs`
- Create: `Application/Catalog/Blue/BlueTelopEntry.cs`
- Create: `Application/Catalog/Blue/BlueGachaEntry.cs`
- Create: `Application/Catalog/Blue/BlueTournamentEntry.cs`
- Create: `Infrastructure/GameDataCatalog/Blue/BlueGameDataPaths.cs`
- Create: `Infrastructure/GameDataCatalog/Blue/BlueRequiredDataFiles.cs`
- Test: `Tests/Blue/BlueCatalogContractTests.cs`

- [ ] **Step 1: Write failing Blue catalog contract tests**

Create `Tests/Blue/BlueCatalogContractTests.cs`:

```csharp
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueCatalogContractTests
{
    [Fact]
    public void BlueRequiredDataPaths_UseBlueEraAndS10100Directory()
    {
        Assert.EndsWith(
            Path.Combine("wwwroot", "data", "blue", "data", "config", "S10100-1", "musicinfo.xml"),
            BlueGameDataPaths.MusicInfoXml,
            StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith(
            Path.Combine("wwwroot", "data", "blue", "data", "config", "S10100-1", "musicmedleyinfo.xml"),
            BlueGameDataPaths.MusicMedleyInfoXml,
            StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith(
            Path.Combine("wwwroot", "data", "blue", "data", "fumen", "tuning.bin"),
            BlueGameDataPaths.TuningBin,
            StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain("S11100-1", BlueGameDataPaths.MusicInfoXml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("green", BlueGameDataPaths.MusicInfoXml, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BlueRequiredDataFiles_ListsOnlyNormalCatalogInputs()
    {
        var paths = BlueRequiredDataFiles.GetRequiredPaths();

        Assert.Equal(3, paths.Count);
        Assert.Contains(BlueGameDataPaths.MusicInfoXml, paths);
        Assert.Contains(BlueGameDataPaths.MusicMedleyInfoXml, paths);
        Assert.Contains(BlueGameDataPaths.TuningBin, paths);
        Assert.DoesNotContain(paths, path => path.Contains("battle", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(paths, path => path.Contains("present.xml", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void IBlueCatalog_IsEraCatalog()
    {
        Assert.True(typeof(IEraGameDataCatalog).IsAssignableFrom(typeof(IBlueCatalog)));
    }
}
```

- [ ] **Step 2: Run failing Blue contract tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueCatalogContractTests
```

Expected: FAIL to compile because `IBlueCatalog`, `BlueGameDataPaths`, and `BlueRequiredDataFiles` do not exist.

- [ ] **Step 3: Add Blue catalog records**

Create `Application/Catalog/Blue/BlueMusicInfoEntry.cs`:

```csharp
using TaikoLocalServer.Application.Abstractions;

namespace TaikoLocalServer.Application.Catalog.Blue;

public sealed record class BlueMusicInfoEntry : IMusicInfoEntry
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

Create `Application/Catalog/Blue/BlueTaikojukuEntry.cs` with this content:

```csharp
namespace TaikoLocalServer.Application.Catalog.Blue;

public sealed class BlueTaikojukuEntry
{
    public uint UniqueId { get; init; }

    public uint DanLevel { get; init; }

    public uint ChallengeLevel { get; init; }

    public string Name { get; init; } = string.Empty;

    public uint Difficulty { get; init; }

    public uint VerupNo { get; init; }

    public BlueTaikojukuConditions Conditions { get; init; } = BlueTaikojukuConditions.Empty;

    public BlueTaikojukuConditions ExcellentConditions { get; init; } = BlueTaikojukuConditions.Empty;

    public IReadOnlyList<BlueTaikojukuSong> Songs { get; init; } = [];
}

public sealed class BlueTaikojukuConditions
{
    public static BlueTaikojukuConditions Empty { get; } = new();

    public uint SoulGauge { get; init; }

    public uint GoodCount { get; init; }

    public uint OkCount { get; init; }

    public uint BadCount { get; init; }

    public uint ComboCount { get; init; }

    public uint TotalHitCount { get; init; }

    public uint Score { get; init; }

    public uint DrumrollCount { get; init; }
}

public sealed class BlueTaikojukuSong
{
    public string MusicId { get; init; } = string.Empty;

    public uint SongNo { get; init; }

    public uint Level { get; init; }

    public uint Notes { get; init; }
}
```

Create `Application/Catalog/Blue/BlueItemShopEntry.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Blue;

public sealed class BlueItemShopEntry
{
    public uint ItemNo { get; init; }

    public uint ItemId { get; init; }

    public uint ItemType { get; init; }

    public uint Price { get; init; }
}
```

Create `Application/Catalog/Blue/BlueItemShopSeason.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Blue;

public sealed class BlueItemShopSeason
{
    public uint SeasonId { get; init; }

    public uint VerupNo { get; init; }

    public string Telop { get; init; } = string.Empty;

    public string StartDatetime { get; init; } = string.Empty;

    public string EndDatetime { get; init; } = string.Empty;

    public uint AfterstartDays { get; init; }

    public uint BeforecloseDays { get; init; }

    public IReadOnlyList<BlueItemShopEntry> Items { get; init; } = [];

    public IReadOnlyDictionary<uint, BlueItemShopEntry> ItemsByNo
        => Items.ToDictionary(item => item.ItemNo);
}
```

Create `Application/Catalog/Blue/BlueItemShopCatalog.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Blue;

public sealed class BlueItemShopCatalog
{
    public static BlueItemShopCatalog Disabled { get; } = new();

    public bool IsEnabled { get; init; }

    public uint? ActiveSeasonId { get; init; }

    public IReadOnlyDictionary<uint, BlueItemShopSeason> Seasons { get; init; }
        = new Dictionary<uint, BlueItemShopSeason>();

    public BlueItemShopSeason? ActiveSeason
        => ActiveSeasonId is { } id && Seasons.TryGetValue(id, out var season)
            ? season
            : null;

    public IReadOnlyDictionary<uint, BlueItemShopEntry> ActiveItemsByNo
        => ActiveSeason?.ItemsByNo ?? new Dictionary<uint, BlueItemShopEntry>();
}
```

Create `Application/Catalog/Blue/BlueRecommendEntry.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Blue;

public sealed class BlueRecommendEntry
{
    public uint RecommendSong { get; init; }

    public IReadOnlyList<uint> RecommendBestSongs { get; init; } = [];

    public static BlueRecommendEntry Empty { get; } = new();
}
```

Create `Application/Catalog/Blue/BlueTelopEntry.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Blue;

public sealed class BlueTelopEntry
{
    public uint TelopId { get; init; }

    public uint VerupNo { get; init; }

    public string StartDatetime { get; init; } = string.Empty;

    public string EndDatetime { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}
```

Create `Application/Catalog/Blue/BlueGachaEntry.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Blue;

public sealed class BlueGachaEntry
{
    public uint GachaId { get; init; }

    public string Name { get; init; } = string.Empty;
}
```

Create `Application/Catalog/Blue/BlueTournamentEntry.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Blue;

public sealed class BlueTournamentEntry
{
    public uint TournamentId { get; init; }

    public string Name { get; init; } = string.Empty;
}
```

- [ ] **Step 4: Add `IBlueCatalog`**

Create `Application/Abstractions/IBlueCatalog.cs`:

```csharp
using TaikoLocalServer.Application.Catalog.Blue;

namespace TaikoLocalServer.Application.Abstractions;

public interface IBlueCatalog : IEraGameDataCatalog
{
    IReadOnlyList<BlueMusicInfoEntry> MusicInfoFileOrder { get; }

    uint SongHashVersion { get; }

    IReadOnlyDictionary<uint, BlueMusicInfoEntry> BlueMusicInfos { get; }

    IReadOnlyList<BlueTaikojukuEntry> TaikojukuFileOrder { get; }

    IReadOnlyDictionary<uint, BlueTaikojukuEntry> Taikojuku { get; }

    BlueItemShopCatalog ItemShopCatalog { get; }

    IReadOnlyDictionary<uint, BlueItemShopEntry> ItemShop { get; }

    IReadOnlyDictionary<uint, EventFolderData> EventFolders { get; }

    IReadOnlyDictionary<uint, BlueTelopEntry> Telops { get; }

    IReadOnlyDictionary<uint, BlueGachaEntry> Gachas { get; }

    IReadOnlyDictionary<uint, BlueTournamentEntry> Tournaments { get; }

    BlueRecommendEntry Recommend { get; }

    IReadOnlyList<MovieData> Movies { get; }

    IReadOnlyList<Costume> GetCostumeList();

    IReadOnlyDictionary<uint, Title> GetTitleDictionary();

    IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary();
}
```

Because `Application/GlobalUsings.cs` already imports catalog/server data namespaces, add explicit `using` statements if the compiler requires them.

- [ ] **Step 5: Add Blue paths and required file validation**

Create `Infrastructure/GameDataCatalog/Blue/BlueGameDataPaths.cs`:

```csharp
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public static class BlueGameDataPaths
{
    private const string ConfigDirectory = "S10100-1";

    public static string GameDataRoot => Path.Combine(PathHelper.GetDataPath(GameEra.Blue), "data");

    public static string ConfigRoot => Path.Combine(GameDataRoot, "config", ConfigDirectory);

    public static string MusicInfoXml => Path.Combine(ConfigRoot, "musicinfo.xml");

    public static string MusicMedleyInfoXml => Path.Combine(ConfigRoot, "musicmedleyinfo.xml");

    public static string TuningBin => Path.Combine(GameDataRoot, "fumen", "tuning.bin");

    public static string MovieDirectory => Path.Combine(GameDataRoot, "movie");
}
```

Create `Infrastructure/GameDataCatalog/Blue/BlueRequiredDataFiles.cs`:

```csharp
namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public static class BlueRequiredDataFiles
{
    public static IReadOnlyList<string> GetRequiredPaths()
    {
        return
        [
            BlueGameDataPaths.MusicInfoXml,
            BlueGameDataPaths.MusicMedleyInfoXml,
            BlueGameDataPaths.TuningBin
        ];
    }

    public static void ThrowIfMissing()
        => ThrowIfMissing(GetRequiredPaths());

    public static void ThrowIfMissing(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Blue required game data file is missing: {path}", path);
            }
        }
    }
}
```

- [ ] **Step 6: Run Blue catalog contract tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueCatalogContractTests
```

Expected: PASS.

- [ ] **Step 7: Commit Blue contract and path files**

Run:

```powershell
git diff -- Application/Abstractions/IBlueCatalog.cs Application/Catalog/Blue Infrastructure/GameDataCatalog/Blue/BlueGameDataPaths.cs Infrastructure/GameDataCatalog/Blue/BlueRequiredDataFiles.cs Tests/Blue/BlueCatalogContractTests.cs
git add -- Application/Abstractions/IBlueCatalog.cs Application/Catalog/Blue Infrastructure/GameDataCatalog/Blue/BlueGameDataPaths.cs Infrastructure/GameDataCatalog/Blue/BlueRequiredDataFiles.cs Tests/Blue/BlueCatalogContractTests.cs
git commit -m "Add Blue catalog contracts"
```

---

### Task 3: Add Shared Optional AC15 JSON Loaders And Green Wrappers

**Files:**
- Create: `Application/Catalog/Ac15/Ac15ItemShopCatalog.cs`
- Create: `Application/Catalog/Ac15/Ac15ItemShopEntry.cs`
- Create: `Application/Catalog/Ac15/Ac15ItemShopSeason.cs`
- Create: `Application/Catalog/Ac15/Ac15RecommendEntry.cs`
- Create: `Application/Catalog/Ac15/Ac15TelopEntry.cs`
- Create: `Infrastructure/GameDataCatalog/Ac15/Ac15EventFolderLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Ac15/Ac15MovieLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Ac15/Ac15RecommendLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Ac15/Ac15TelopLoader.cs`
- Modify: `Infrastructure/GameDataCatalog/Green/GreenEventFolderLoader.cs`
- Modify: `Infrastructure/GameDataCatalog/Green/GreenItemShopLoader.cs`
- Modify: `Infrastructure/GameDataCatalog/Green/GreenMovieLoader.cs`
- Modify: `Infrastructure/GameDataCatalog/Green/GreenRecommendLoader.cs`
- Modify: `Infrastructure/GameDataCatalog/Green/GreenTelopLoader.cs`
- Test: `Tests/Ac15/Ac15OptionalCatalogLoaderTests.cs`
- Existing tests to run when they exist in the checkout: `Tests/Green/GreenItemShopLoaderTests.cs`, `Tests/Green/GreenEventFolderTests.cs`, `Tests/Green/GreenAttractMovieTests.cs`, `Tests/Green/GreenRecommendCatalogLoaderTests.cs`.

- [ ] **Step 1: Write shared optional-loader tests**

Create `Tests/Ac15/Ac15OptionalCatalogLoaderTests.cs`:

```csharp
using Microsoft.Extensions.Logging.Abstractions;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15OptionalCatalogLoaderTests
{
    [Fact]
    public async Task ItemShopLoader_DisabledMissingFileReturnsDisabledCatalog()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");

        var catalog = await Ac15ItemShopLoader.LoadFromFileAsync(
            path,
            isEnabled: false,
            activeSeasonId: null,
            eraName: "Blue",
            CancellationToken.None);

        Assert.False(catalog.IsEnabled);
        Assert.Null(catalog.ActiveSeason);
    }

    [Fact]
    public async Task ItemShopLoader_EnabledRequiresKnownActiveSeason()
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, """
            {
              "seasons": [
                {
                  "season_id": 2,
                  "verup_no": 9,
                  "telop": "Shop",
                  "start_datetime": "20180315000000",
                  "end_datetime": "20180626075959",
                  "afterstart_days": 3,
                  "beforeclose_days": 4,
                  "items": [
                    { "item_type": 4, "item_id": 117, "item_price": 500 }
                  ]
                }
              ]
            }
            """);

        try
        {
            var catalog = await Ac15ItemShopLoader.LoadFromFileAsync(
                path,
                isEnabled: true,
                activeSeasonId: 2,
                eraName: "Blue",
                CancellationToken.None);

            Assert.True(catalog.IsEnabled);
            Assert.Equal(2u, catalog.ActiveSeason!.SeasonId);
            Assert.Equal(1u, Assert.Single(catalog.ActiveSeason.Items).ItemNo);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task EventFolderLoader_MissingFileReturnsEmpty()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");

        var folders = await Ac15EventFolderLoader.LoadFromFileAsync(
            path,
            new HashSet<uint> { 100 },
            eraName: "Blue",
            CancellationToken.None);

        Assert.Empty(folders);
    }

    [Fact]
    public async Task RecommendLoader_FiltersUnknownSongs()
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, """
            {
              "recommendSong": 100,
              "recommendBestSongs": [100, 999]
            }
            """);

        try
        {
            var recommend = await Ac15RecommendLoader.LoadFromFileAsync(
                path,
                new HashSet<uint> { 100 },
                CancellationToken.None);

            Assert.Equal(100u, recommend.RecommendSong);
            Assert.Equal([100u], recommend.RecommendBestSongs);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task MovieLoader_MissingConfigDiscoversNonzeroMovies()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        await File.WriteAllBytesAsync(Path.Combine(root, "attract_cm_000.pam"), []);
        await File.WriteAllBytesAsync(Path.Combine(root, "attract_cm_101.pam"), []);

        try
        {
            var movies = await Ac15MovieLoader.LoadFromFileAsync(
                Path.Combine(root, "missing.json"),
                root,
                eraName: "Blue",
                NullLogger.Instance,
                CancellationToken.None);

            var movie = Assert.Single(movies);
            Assert.Equal(101u, movie.MovieId);
            Assert.Equal(999u, movie.EnableDays);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
```

- [ ] **Step 2: Run failing shared optional-loader tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~Ac15OptionalCatalogLoaderTests
```

Expected: FAIL to compile because shared optional loader types do not exist.

- [ ] **Step 3: Add neutral optional catalog records**

Create `Application/Catalog/Ac15/Ac15ItemShopEntry.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Ac15;

public sealed class Ac15ItemShopEntry
{
    public uint ItemNo { get; init; }

    public uint ItemId { get; init; }

    public uint ItemType { get; init; }

    public uint Price { get; init; }
}
```

Create `Application/Catalog/Ac15/Ac15ItemShopSeason.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Ac15;

public sealed class Ac15ItemShopSeason
{
    public uint SeasonId { get; init; }

    public uint VerupNo { get; init; }

    public string Telop { get; init; } = string.Empty;

    public string StartDatetime { get; init; } = string.Empty;

    public string EndDatetime { get; init; } = string.Empty;

    public uint AfterstartDays { get; init; }

    public uint BeforecloseDays { get; init; }

    public IReadOnlyList<Ac15ItemShopEntry> Items { get; init; } = [];

    public IReadOnlyDictionary<uint, Ac15ItemShopEntry> ItemsByNo
        => Items.ToDictionary(item => item.ItemNo);
}
```

Create `Application/Catalog/Ac15/Ac15ItemShopCatalog.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Ac15;

public sealed class Ac15ItemShopCatalog
{
    public static Ac15ItemShopCatalog Disabled { get; } = new();

    public bool IsEnabled { get; init; }

    public uint? ActiveSeasonId { get; init; }

    public IReadOnlyDictionary<uint, Ac15ItemShopSeason> Seasons { get; init; }
        = new Dictionary<uint, Ac15ItemShopSeason>();

    public Ac15ItemShopSeason? ActiveSeason
        => ActiveSeasonId is { } id && Seasons.TryGetValue(id, out var season)
            ? season
            : null;

    public IReadOnlyDictionary<uint, Ac15ItemShopEntry> ActiveItemsByNo
        => ActiveSeason?.ItemsByNo ?? new Dictionary<uint, Ac15ItemShopEntry>();
}
```

Create `Application/Catalog/Ac15/Ac15RecommendEntry.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Ac15;

public sealed class Ac15RecommendEntry
{
    public uint RecommendSong { get; init; }

    public IReadOnlyList<uint> RecommendBestSongs { get; init; } = [];

    public static Ac15RecommendEntry Empty { get; } = new();
}
```

Create `Application/Catalog/Ac15/Ac15TelopEntry.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Ac15;

public sealed class Ac15TelopEntry
{
    public uint TelopId { get; init; }

    public uint VerupNo { get; init; }

    public string StartDatetime { get; init; } = string.Empty;

    public string EndDatetime { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}
```

- [ ] **Step 4: Add shared optional loaders**

Create the shared loaders with the signatures below. Copy the parsing and validation logic from the current Green loaders into these files, then replace hard-coded Green path resolution with explicit path parameters and hard-coded Green error labels with the `eraName` parameter:

```text
Infrastructure/GameDataCatalog/Ac15/Ac15EventFolderLoader.cs
Infrastructure/GameDataCatalog/Ac15/Ac15ItemShopLoader.cs
Infrastructure/GameDataCatalog/Ac15/Ac15MovieLoader.cs
Infrastructure/GameDataCatalog/Ac15/Ac15RecommendLoader.cs
Infrastructure/GameDataCatalog/Ac15/Ac15TelopLoader.cs
```

Use these signatures:

```csharp
public static Task<IReadOnlyDictionary<uint, EventFolderData>> Ac15EventFolderLoader.LoadFromFileAsync(
    string path,
    IReadOnlySet<uint> catalogSongIds,
    string eraName,
    CancellationToken cancellationToken);

public static Task<Ac15ItemShopCatalog> Ac15ItemShopLoader.LoadFromFileAsync(
    string path,
    bool isEnabled,
    uint? activeSeasonId,
    string eraName,
    CancellationToken cancellationToken);

public static Task<IReadOnlyList<MovieData>> Ac15MovieLoader.LoadFromFileAsync(
    string configPath,
    string movieDirectory,
    string eraName,
    ILogger logger,
    CancellationToken cancellationToken);

public static Task<Ac15RecommendEntry> Ac15RecommendLoader.LoadFromFileAsync(
    string path,
    IReadOnlySet<uint> catalogSongIds,
    CancellationToken cancellationToken);

public static Task<IReadOnlyDictionary<uint, Ac15TelopEntry>> Ac15TelopLoader.LoadFromFileAsync(
    string path,
    CancellationToken cancellationToken);
```

When moving Green logic, preserve:

- event folder max folder id `15`;
- song id max from `GreenProtocolBytes.SongFlagBytes * 8` until a Blue-specific protocol byte constant is introduced in A3/A4;
- recommend filtering to known catalog songs;
- movie discovery of nonzero `attract_cm_###.pam` with `enable_days = 999`;
- item shop validation for datetime, duplicate seasons, duplicate item identities, item type `1..7`, nonzero item id, nonzero price, and at most `64` items.

- [ ] **Step 5: Replace Green optional loaders with wrappers**

Update each Green optional loader to delegate to the shared AC15 loader and map neutral records back to Green record types where necessary:

- `GreenEventFolderLoader`: call `Ac15EventFolderLoader.LoadFromFileAsync(path, catalogSongIds, nameof(GameEra.Green), cancellationToken)`.
- `GreenRecommendLoader`: call `Ac15RecommendLoader.LoadFromFileAsync(...)` and map to `GreenRecommendEntry`.
- `GreenTelopLoader`: call `Ac15TelopLoader.LoadFromFileAsync(...)` and map to `GreenTelopEntry`.
- `GreenMovieLoader`: call `Ac15MovieLoader.LoadFromFileAsync(..., nameof(GameEra.Green), logger, cancellationToken)`.
- `GreenItemShopLoader`: call `Ac15ItemShopLoader.LoadFromFileAsync(path, greenSettings.EnableShop == true, greenSettings.ActiveShopSeasonId, nameof(GameEra.Green), cancellationToken)` and map to `GreenItemShopCatalog`.

Use this mapping pattern in `GreenItemShopLoader`:

```csharp
private static GreenItemShopCatalog Map(Ac15ItemShopCatalog catalog)
{
    if (!catalog.IsEnabled)
    {
        return GreenItemShopCatalog.Disabled;
    }

    return new GreenItemShopCatalog
    {
        IsEnabled = true,
        ActiveSeasonId = catalog.ActiveSeasonId,
        Seasons = catalog.Seasons.ToDictionary(
            pair => pair.Key,
            pair => new GreenItemShopSeason
            {
                SeasonId = pair.Value.SeasonId,
                VerupNo = pair.Value.VerupNo,
                Telop = pair.Value.Telop,
                StartDatetime = pair.Value.StartDatetime,
                EndDatetime = pair.Value.EndDatetime,
                AfterstartDays = pair.Value.AfterstartDays,
                BeforecloseDays = pair.Value.BeforecloseDays,
                Items = pair.Value.Items.Select(item => new GreenItemShopEntry
                {
                    ItemNo = item.ItemNo,
                    ItemType = item.ItemType,
                    ItemId = item.ItemId,
                    Price = item.Price
                }).ToArray()
            })
    };
}
```

- [ ] **Step 6: Run optional loader regression tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15OptionalCatalogLoaderTests Or FullyQualifiedName~GreenItemShopLoaderTests Or FullyQualifiedName~GreenEventFolder Or FullyQualifiedName~GreenRecommend Or FullyQualifiedName~GreenMovie"
```

Expected: PASS. If no tests match one of the Green filter terms, xUnit reports only the matched tests; confirm the AC15 and Green item-shop tests ran.

- [ ] **Step 7: Commit optional loader sharing**

Run:

```powershell
git diff -- Application/Catalog/Ac15 Infrastructure/GameDataCatalog/Ac15 Infrastructure/GameDataCatalog/Green Tests/Ac15/Ac15OptionalCatalogLoaderTests.cs
git add -- Application/Catalog/Ac15 Infrastructure/GameDataCatalog/Ac15 Infrastructure/GameDataCatalog/Green Tests/Ac15/Ac15OptionalCatalogLoaderTests.cs
git commit -m "Share AC15 optional catalog loaders"
```

---

### Task 4: Implement Blue Loaders And Runtime Catalog

**Files:**
- Create: `Infrastructure/GameDataCatalog/Blue/BlueMusicInfoLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Blue/BlueTaikojukuLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Blue/BlueTuningLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Blue/BlueEventFolderLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Blue/BlueItemShopLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Blue/BlueMovieLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Blue/BlueRecommendLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Blue/BlueTelopLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Blue/BlueCostumeLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Blue/BlueTitleLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Blue/BlueNeiroLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Blue/BlueGachaLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Blue/BlueTournamentLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Blue/BlueEraGameDataCatalog.cs`
- Test: `Tests/Blue/BlueCatalogLoaderTests.cs`

- [ ] **Step 1: Write failing Blue catalog loader tests**

Create `Tests/Blue/BlueCatalogLoaderTests.cs`:

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

namespace TaikoLocalServer.Tests.Blue;

[Collection("Blue runtime catalog tests")]
public sealed class BlueCatalogLoaderTests
{
    [Fact]
    public async Task MusicInfoLoader_ReadsLocalBlueDataWhenPresent()
    {
        var file = FindRepoFileOrSkip("Host", "wwwroot", "data", "blue", "data", "config", "S10100-1", "musicinfo.xml");
        if (file is null)
        {
            return;
        }

        var result = await BlueMusicInfoLoader.LoadFromFileAsync(file, CancellationToken.None);

        Assert.True(result.SongHashVersion > 0);
        Assert.NotEmpty(result.Entries);
        Assert.Equal(0, result.Entries[0].FileOrder);
        Assert.True(result.Entries[0].SongNo > 0);
        Assert.NotEmpty(result.Entries[0].MusicId);
    }

    [Fact]
    public async Task TaikojukuLoader_ReadsLocalBlueDataWhenPresent()
    {
        var file = FindRepoFileOrSkip("Host", "wwwroot", "data", "blue", "data", "config", "S10100-1", "musicmedleyinfo.xml");
        if (file is null)
        {
            return;
        }

        var entries = await BlueTaikojukuLoader.LoadFromFileAsync(file, CancellationToken.None);

        Assert.NotEmpty(entries);
        Assert.True(entries[0].UniqueId > 0);
        Assert.True(entries[0].ChallengeLevel > 0);
        Assert.NotEmpty(entries[0].Songs);
    }

    [Fact]
    public async Task TuningLoader_ReadsLocalBlueDataWhenPresent()
    {
        var file = FindRepoFileOrSkip("Host", "wwwroot", "data", "blue", "data", "fumen", "tuning.bin");
        if (file is null)
        {
            return;
        }

        var stars = await BlueTuningLoader.LoadFromFileAsync(file, CancellationToken.None);

        Assert.NotEmpty(stars);
        Assert.Contains(stars.Values, star => star.Easy > 0 || star.Normal > 0 || star.Hard > 0 || star.Oni > 0 || star.Ura > 0);
    }

    [Fact]
    public void RequiredDataFiles_ThrowsBlueSpecificMessageForMissingFile()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}", "musicinfo.xml");

        var ex = Assert.Throws<FileNotFoundException>(() => BlueRequiredDataFiles.ThrowIfMissing([missingPath]));
        Assert.Contains("Blue required game data file is missing", ex.Message, StringComparison.Ordinal);
        Assert.Equal(missingPath, ex.FileName);
    }

    [Fact]
    public async Task CatalogInitialize_LoadsRequiredAndOptionalDefaultsWhenLocalDataPresent()
    {
        if (FindRepoFileOrSkip("Host", "wwwroot", "data", "blue", "data", "config", "S10100-1", "musicinfo.xml") is null
            || FindRepoFileOrSkip("Host", "wwwroot", "data", "blue", "data", "config", "S10100-1", "musicmedleyinfo.xml") is null
            || FindRepoFileOrSkip("Host", "wwwroot", "data", "blue", "data", "fumen", "tuning.bin") is null)
        {
            return;
        }

        CopyBlueCatalogFilesToProcessRoot();
        var logger = new RecordingLogger<BlueEraGameDataCatalog>();
        var settings = Options.Create(new ServerSettings
        {
            Eras = new Dictionary<string, EraSettings>
            {
                [nameof(GameEra.Blue)] = new()
                {
                    Enabled = true,
                    EnableShop = false,
                    AutoExtractCatalog = false,
                    GameDataPath = "wwwroot/data/blue/data",
                    CustomizationNameDataPath = string.Empty
                }
            }
        });
        var catalog = new BlueEraGameDataCatalog(logger, settings);

        await catalog.InitializeAsync(CancellationToken.None);

        Assert.Equal(GameEra.Blue, catalog.Era);
        Assert.True(catalog.SongHashVersion > 0);
        Assert.NotEmpty(catalog.MusicInfoFileOrder);
        Assert.NotEmpty(catalog.BlueMusicInfos);
        Assert.NotEmpty(catalog.MusicInfos);
        Assert.NotEmpty(catalog.TaikojukuFileOrder);
        Assert.False(catalog.ItemShopCatalog.IsEnabled);
        Assert.Empty(catalog.EventFolders);
        Assert.Empty(catalog.Telops);
        Assert.Empty(catalog.Gachas);
        Assert.Empty(catalog.Tournaments);
        Assert.NotNull(catalog.Movies);
        Assert.Contains(logger.Events, log => log.Message.Contains("Loaded Blue catalog", StringComparison.Ordinal));
    }

    private static string? FindRepoFileOrSkip(params string[] pathParts)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(pathParts).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static void CopyBlueCatalogFilesToProcessRoot()
    {
        Copy("config", "S10100-1", "musicinfo.xml");
        Copy("config", "S10100-1", "musicmedleyinfo.xml");
        Copy("fumen", "tuning.bin");

        static void Copy(params string[] relativeParts)
        {
            var source = FindRepoFileOrSkip(["Host", "wwwroot", "data", "blue", "data", .. relativeParts])
                ?? throw new FileNotFoundException(Path.Combine(relativeParts));
            var targetRoot = Path.Combine(
                Path.GetDirectoryName(Environment.ProcessPath)
                    ?? throw new ApplicationException("Cannot resolve process directory."),
                "wwwroot",
                "data",
                "blue",
                "data");
            var destination = Path.Combine([targetRoot, .. relativeParts]);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)
                ?? throw new ApplicationException($"Cannot resolve directory for {destination}."));
            File.Copy(source, destination, overwrite: true);
        }
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<LogEvent> Events { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Events.Add(new LogEvent(logLevel, formatter(state, exception)));
        }

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new();

            public void Dispose()
            {
            }
        }
    }

    private sealed record LogEvent(LogLevel Level, string Message);
}

[CollectionDefinition("Blue runtime catalog tests")]
public sealed class BlueRuntimeCatalogTestCollection;
```

- [ ] **Step 2: Run failing Blue catalog loader tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueCatalogLoaderTests
```

Expected: FAIL to compile because Blue loaders and `BlueEraGameDataCatalog` do not exist.

- [ ] **Step 3: Add Blue required data loaders**

Create `BlueMusicInfoLoader`, `BlueTaikojukuLoader`, and `BlueTuningLoader` as wrappers over the shared AC15 loaders. Map every neutral AC15 property to the identically named Blue property, and leave Blue star fields at zero until `BlueEraGameDataCatalog` enriches the music rows.

Use these signatures:

```csharp
public sealed record BlueMusicInfoLoadResult(
    uint SongHashVersion,
    IReadOnlyList<BlueMusicInfoEntry> Entries);

public sealed class BlueMusicInfoLoader
{
    public Task<BlueMusicInfoLoadResult> LoadAsync(CancellationToken cancellationToken);

    public static Task<BlueMusicInfoLoadResult> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken);
}

public sealed class BlueTaikojukuLoader
{
    public Task<IReadOnlyList<BlueTaikojukuEntry>> LoadAsync(CancellationToken cancellationToken);

    public static Task<IReadOnlyList<BlueTaikojukuEntry>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken);
}

public sealed class BlueTuningLoader
{
    public Task<IReadOnlyDictionary<string, Ac15StarSet>> LoadAsync(CancellationToken cancellationToken);

    public static Task<IReadOnlyDictionary<string, Ac15StarSet>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken);
}
```

`BlueTuningLoader` returns `Ac15StarSet` directly; do not create a separate Blue star type unless later gameplay code needs one.

- [ ] **Step 4: Add Blue optional loaders**

Create Blue optional loader wrappers with these file names:

```csharp
public sealed class BlueEventFolderLoader
{
    public const string FileName = "blue_event_folder_data.json";
}

public sealed class BlueItemShopLoader
{
    public const string FileName = "blue_item_shop_data.json";
}

public sealed class BlueRecommendLoader
{
    public const string FileName = "blue_recommend_songs.json";
}

public sealed class BlueTelopLoader
{
    public const string FileName = "blue_telop_data.json";
}

public sealed class BlueMovieLoader
{
    public const string FileName = "blue_movie_data.json";
}

public sealed class BlueCostumeLoader
{
    public const string FileName = "blue_costume_data.json";
}

public sealed class BlueTitleLoader
{
    public const string FileName = "blue_title_data.json";
}

public sealed class BlueNeiroLoader
{
    public const string FileName = "blue_neiro_data.json";
}
```

Each wrapper should resolve paths from `PathHelper.GetDataPath(GameEra.Blue)` and call the shared AC15 loader. Map neutral AC15 item-shop, recommend, and telop records into Blue record types.

Create `BlueGachaLoader` and `BlueTournamentLoader` as empty loaders matching Green's current behavior:

```csharp
namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

internal sealed class BlueGachaLoader
{
    public Task<IReadOnlyDictionary<uint, BlueGachaEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyDictionary<uint, BlueGachaEntry> empty = new Dictionary<uint, BlueGachaEntry>();
        return Task.FromResult(empty);
    }
}
```

Create `BlueTournamentLoader` with this content:

```csharp
using TaikoLocalServer.Application.Catalog.Blue;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

internal sealed class BlueTournamentLoader
{
    public Task<IReadOnlyDictionary<uint, BlueTournamentEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyDictionary<uint, BlueTournamentEntry> empty = new Dictionary<uint, BlueTournamentEntry>();
        return Task.FromResult(empty);
    }
}
```

- [ ] **Step 5: Add `BlueEraGameDataCatalog`**

Create `Infrastructure/GameDataCatalog/Blue/BlueEraGameDataCatalog.cs` with these fields, properties, and initialization flow:

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Blue;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public sealed class BlueEraGameDataCatalog(
    ILogger<BlueEraGameDataCatalog> logger,
    IOptions<ServerSettings>? serverSettings = null) : IBlueCatalog
{
    private uint songHashVersion;
    private IReadOnlyList<BlueMusicInfoEntry> musicInfoFileOrder = [];
    private IReadOnlyDictionary<uint, BlueMusicInfoEntry> musicInfos = new Dictionary<uint, BlueMusicInfoEntry>();
    private IReadOnlyDictionary<uint, IMusicInfoEntry> sharedMusicInfos = new Dictionary<uint, IMusicInfoEntry>();
    private IReadOnlyList<BlueTaikojukuEntry> taikojukuFileOrder = [];
    private IReadOnlyDictionary<uint, BlueTaikojukuEntry> taikojuku = new Dictionary<uint, BlueTaikojukuEntry>();
    private BlueItemShopCatalog itemShopCatalog = BlueItemShopCatalog.Disabled;
    private IReadOnlyDictionary<uint, BlueItemShopEntry> itemShop = new Dictionary<uint, BlueItemShopEntry>();
    private IReadOnlyDictionary<uint, EventFolderData> eventFolders = new Dictionary<uint, EventFolderData>();
    private IReadOnlyDictionary<uint, BlueTelopEntry> telops = new Dictionary<uint, BlueTelopEntry>();
    private IReadOnlyDictionary<uint, BlueGachaEntry> gachas = new Dictionary<uint, BlueGachaEntry>();
    private IReadOnlyDictionary<uint, BlueTournamentEntry> tournaments = new Dictionary<uint, BlueTournamentEntry>();
    private BlueRecommendEntry recommend = BlueRecommendEntry.Empty;
    private IReadOnlyList<MovieData> movies = [];
    private IReadOnlyList<Costume> costumeList = [];
    private IReadOnlyDictionary<uint, Title> titleDictionary = new Dictionary<uint, Title>();
    private IReadOnlyDictionary<uint, Neiro> neiroDictionary = new Dictionary<uint, Neiro>();

    public GameEra Era => GameEra.Blue;

    public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos => sharedMusicInfos;

    public uint SongHashVersion => songHashVersion;

    public IReadOnlyList<BlueMusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;

    public IReadOnlyDictionary<uint, BlueMusicInfoEntry> BlueMusicInfos => musicInfos;

    public IReadOnlyList<BlueTaikojukuEntry> TaikojukuFileOrder => taikojukuFileOrder;

    public IReadOnlyDictionary<uint, BlueTaikojukuEntry> Taikojuku => taikojuku;

    public BlueItemShopCatalog ItemShopCatalog => itemShopCatalog;

    public IReadOnlyDictionary<uint, BlueItemShopEntry> ItemShop => itemShop;

    public IReadOnlyDictionary<uint, EventFolderData> EventFolders => eventFolders;

    public IReadOnlyDictionary<uint, BlueTelopEntry> Telops => telops;

    public IReadOnlyDictionary<uint, BlueGachaEntry> Gachas => gachas;

    public IReadOnlyDictionary<uint, BlueTournamentEntry> Tournaments => tournaments;

    public BlueRecommendEntry Recommend => recommend;

    public IReadOnlyList<MovieData> Movies => movies;

    public IReadOnlyList<Costume> GetCostumeList() => costumeList;

    public IReadOnlyDictionary<uint, Title> GetTitleDictionary() => titleDictionary;

    public IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary() => neiroDictionary;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        BlueRequiredDataFiles.ThrowIfMissing();

        var musicInfo = await new BlueMusicInfoLoader().LoadAsync(cancellationToken);
        var stars = await new BlueTuningLoader().LoadAsync(cancellationToken);
        var loadedTaikojukuFileOrder = await new BlueTaikojukuLoader().LoadAsync(cancellationToken);
        var taikojukuUniqueIds = loadedTaikojukuFileOrder
            .Select(entry => entry.UniqueId)
            .ToHashSet();
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
            .Where(entry => !stars.ContainsKey(entry.MusicId)
                && !taikojukuUniqueIds.Contains(entry.SongNo))
            .Select(entry => entry.MusicId)
            .ToList();
        if (missingTuning.Count > 0)
        {
            logger.LogWarning(
                "Blue: {Count} musicinfo entries have no tuning record; using star=0 (examples: {Examples})",
                missingTuning.Count,
                string.Join(", ", missingTuning.Take(5)));
        }

        songHashVersion = musicInfo.SongHashVersion;
        musicInfoFileOrder = enrichedEntries;
        musicInfos = enrichedEntries.ToDictionary(entry => entry.SongNo);
        sharedMusicInfos = musicInfos.ToDictionary(
            pair => pair.Key,
            pair => (IMusicInfoEntry)pair.Value);
        taikojukuFileOrder = loadedTaikojukuFileOrder;
        taikojuku = taikojukuFileOrder
            .GroupBy(entry => entry.UniqueId)
            .ToDictionary(group => group.Key, group => group.First());

        var blueSettings = GetBlueSettings();
        itemShopCatalog = await new BlueItemShopLoader().LoadAsync(blueSettings, cancellationToken);
        itemShop = itemShopCatalog.ActiveItemsByNo;
        eventFolders = await new BlueEventFolderLoader().LoadAsync(new HashSet<uint>(musicInfos.Keys), cancellationToken);
        telops = await new BlueTelopLoader().LoadAsync(cancellationToken);
        gachas = await new BlueGachaLoader().LoadAsync(cancellationToken);
        tournaments = await new BlueTournamentLoader().LoadAsync(cancellationToken);
        recommend = await new BlueRecommendLoader().LoadAsync(new HashSet<uint>(musicInfos.Keys), cancellationToken);
        movies = await new BlueMovieLoader().LoadAsync(logger, cancellationToken);
        costumeList = await new BlueCostumeLoader().LoadAsync(cancellationToken);
        titleDictionary = await new BlueTitleLoader().LoadAsync(cancellationToken);
        neiroDictionary = await new BlueNeiroLoader().LoadAsync(cancellationToken);

        logger.LogInformation(
            "Loaded Blue catalog: {SongCount} songs, song_hash_ver={SongHashVersion}, {TaikojukuCount} taikojuku packs, {StarCount} tuning star rows, {CostumeCount} costumes, {TitleCount} titles, {NeiroCount} tones, {MovieCount} attract movies, item_shop_enabled={ItemShopEnabled}",
            musicInfoFileOrder.Count,
            songHashVersion,
            taikojukuFileOrder.Count,
            stars.Count,
            costumeList.Count,
            titleDictionary.Count,
            neiroDictionary.Count,
            movies.Count,
            itemShopCatalog.IsEnabled);
    }

    private EraSettings GetBlueSettings()
    {
        return serverSettings?.Value.Eras.TryGetValue(nameof(GameEra.Blue), out var settings) == true
            ? settings
            : new EraSettings { GameDataPath = "wwwroot/data/blue/data", EnableShop = false };
    }
}
```

Do not call the Green customization auto-extractor from Blue in A2. Blue's observed `nutdata/S10100-1/appendable` layout is not the same as the Green extractor's `nutdata/*/nutdatapack.ndp` inputs. Missing Blue customization JSON files therefore load as empty catalogs in A2.

- [ ] **Step 6: Run Blue catalog loader tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueCatalogLoaderTests
```

Expected: PASS. Tests that require local ignored Blue data may return early when files are absent.

- [ ] **Step 7: Commit Blue loaders and runtime catalog**

Run:

```powershell
git diff -- Infrastructure/GameDataCatalog/Blue Tests/Blue/BlueCatalogLoaderTests.cs
git add -- Infrastructure/GameDataCatalog/Blue Tests/Blue/BlueCatalogLoaderTests.cs
git commit -m "Add Blue runtime catalog loaders"
```

---

### Task 5: Wire Blue Catalog Into Settings And Infrastructure

**Files:**
- Modify: `Application/Settings/ServerSettings.cs`
- Modify: `Application/Settings/ServerSettingsOptionsValidationExtensions.cs`
- Modify: `Host/Configurations/ServerSettings.json`
- Modify: `Infrastructure/DependencyInjection.cs`
- Test: `Tests/Blue/BlueServerSettingsValidationTests.cs`
- Test: `Tests/Blue/BlueInfrastructureRegistrationTests.cs`
- Existing test: `Tests/Blue/BlueEraFoundationTests.cs`

- [ ] **Step 1: Write failing Blue settings validation tests**

Create `Tests/Blue/BlueServerSettingsValidationTests.cs`:

```csharp
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueServerSettingsValidationTests
{
    [Fact]
    public void OptionsValidation_BlueEnabledRequiresExplicitShopEnableSetting()
    {
        var configuration = BuildConfiguration("""
            {
              "ServerSettings": {
                "Eras": {
                  "Blue": {
                    "Enabled": true
                  }
                }
              }
            }
            """);

        using var provider = BuildProvider(configuration, [GameEra.Blue]);

        var ex = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<ServerSettings>>().Value);

        Assert.Contains(ex.Failures, failure => failure.Contains("ServerSettings:Eras:Blue:EnableShop", StringComparison.Ordinal));
    }

    [Fact]
    public void OptionsValidation_BlueShopEnabledRequiresActiveSeason()
    {
        var configuration = BuildConfiguration("""
            {
              "ServerSettings": {
                "Eras": {
                  "Blue": {
                    "Enabled": true,
                    "EnableShop": true
                  }
                }
              }
            }
            """);

        using var provider = BuildProvider(configuration, [GameEra.Blue]);

        var ex = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<ServerSettings>>().Value);

        Assert.Contains(ex.Failures, failure => failure.Contains("ServerSettings:Eras:Blue:ActiveShopSeasonId", StringComparison.Ordinal));
    }

    [Fact]
    public void OptionsValidation_BlueShopDisabledDoesNotRequireActiveSeason()
    {
        var configuration = BuildConfiguration("""
            {
              "ServerSettings": {
                "Eras": {
                  "Blue": {
                    "Enabled": true,
                    "EnableShop": false
                  }
                }
              }
            }
            """);

        using var provider = BuildProvider(configuration, [GameEra.Blue]);

        var settings = provider.GetRequiredService<IOptions<ServerSettings>>().Value;

        Assert.False(settings.Eras[nameof(GameEra.Blue)].EnableShop == true);
    }

    [Fact]
    public void ShippedServerSettings_DeclaresBlueCatalogSettings()
    {
        var path = FindServerSettingsPath();

        Assert.NotNull(path);

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(path)
            .Build();
        var blue = configuration.GetSection("ServerSettings:Eras:Blue");

        Assert.True(blue.GetSection("AutoExtractCatalog").Exists());
        Assert.Equal("wwwroot/data/blue/data", blue.GetValue<string>("GameDataPath"));
        Assert.True(blue.GetSection("CustomizationNameDataPath").Exists());
        Assert.True(blue.GetSection("EnableShop").Exists());
        Assert.False(blue.GetValue<bool>("EnableShop"));
        Assert.True(blue.GetSection("ActiveShopSeasonId").Exists());
    }

    private static IConfigurationRoot BuildConfiguration(string json)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();
    }

    private static ServiceProvider BuildProvider(IConfiguration configuration, ISet<GameEra> enabledEras)
    {
        var services = new ServiceCollection();
        services.AddOptions<ServerSettings>()
            .Bind(configuration.GetSection("ServerSettings"))
            .ValidateStartupSettings(enabledEras)
            .ValidateOnStart();

        return services.BuildServiceProvider();
    }

    private static string? FindServerSettingsPath()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, "Host", "Configurations", "ServerSettings.json");
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }
}
```

- [ ] **Step 2: Write failing Blue infrastructure registration tests**

Create `Tests/Blue/BlueInfrastructureRegistrationTests.cs`:

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueInfrastructureRegistrationTests
{
    [Fact]
    public void AddInfrastructure_RegistersBlueCatalogWhenBlueEnabled()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();

        services.AddInfrastructure(configuration, new HashSet<GameEra> { GameEra.Blue });
        using var provider = services.BuildServiceProvider();

        Assert.IsType<BlueEraGameDataCatalog>(provider.GetRequiredService<IBlueCatalog>());
        var gameDataCatalog = provider.GetRequiredService<IGameDataCatalog>();
        Assert.Equal(GameEra.Blue, gameDataCatalog.For(GameEra.Blue).Era);
    }

    [Fact]
    public void AddInfrastructure_DoesNotRegisterBlueCatalogWhenBlueDisabled()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();

        services.AddInfrastructure(configuration, new HashSet<GameEra> { GameEra.Green });
        using var provider = services.BuildServiceProvider();

        Assert.Null(provider.GetService<IBlueCatalog>());
        var gameDataCatalog = provider.GetRequiredService<IGameDataCatalog>();
        var ex = Assert.Throws<InvalidOperationException>(() => gameDataCatalog.For(GameEra.Blue));
        Assert.Contains("Era Blue is not enabled", ex.Message, StringComparison.Ordinal);
    }

    private static IConfigurationRoot BuildConfiguration()
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DbFileName"] = "test.db3",
                ["AuthSettings:JwtKey"] = "0123456789abcdef0123456789abcdef",
                ["AuthSettings:JwtIssuer"] = "tests",
                ["AuthSettings:JwtAudience"] = "tests",
                ["ServerSettings:Eras:Blue:Enabled"] = "true",
                ["ServerSettings:Eras:Blue:EnableShop"] = "false",
                ["ServerSettings:Eras:Green:Enabled"] = "true",
                ["ServerSettings:Eras:Green:EnableShop"] = "false"
            })
            .Build();
}
```

- [ ] **Step 3: Run failing settings and registration tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueServerSettingsValidationTests Or FullyQualifiedName~BlueInfrastructureRegistrationTests"
```

Expected: FAIL because Blue settings validation and Blue catalog DI are not implemented.

- [ ] **Step 4: Make `EraSettings.GameDataPath` neutral**

Modify `Application/Settings/ServerSettings.cs`:

```csharp
public sealed class EraSettings
{
    public bool Enabled { get; set; }

    public bool AutoExtractCatalog { get; set; } = true;

    public string GameDataPath { get; set; } = string.Empty;

    public string? CustomizationNameDataPath { get; set; }

    public bool? EnableShop { get; set; }

    public uint? ActiveShopSeasonId { get; set; }
}
```

This prevents a missing Blue `GameDataPath` from silently resolving to Green's path.

- [ ] **Step 5: Generalize AC15 shop options validation**

Modify `Application/Settings/ServerSettingsOptionsValidationExtensions.cs`:

```csharp
using Microsoft.Extensions.Options;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Application.Settings;

public static class ServerSettingsOptionsValidationExtensions
{
    private static readonly GameEra[] Ac15ShopEras =
    [
        GameEra.Green,
        GameEra.Blue
    ];

    public static OptionsBuilder<ServerSettings> ValidateStartupSettings(
        this OptionsBuilder<ServerSettings> builder,
        ISet<GameEra> enabledEras)
    {
        foreach (var era in Ac15ShopEras)
        {
            builder = builder
                .Validate(
                    settings => HasExplicitShopSetting(settings, enabledEras, era),
                    $"ServerSettings:Eras:{era}:EnableShop is required when {era} is enabled.")
                .Validate(
                    settings => HasActiveShopSeason(settings, enabledEras, era),
                    $"ServerSettings:Eras:{era}:ActiveShopSeasonId must be a nonzero season id when {era} EnableShop is true.");
        }

        return builder;
    }

    private static bool HasExplicitShopSetting(ServerSettings settings, ISet<GameEra> enabledEras, GameEra era)
        => !enabledEras.Contains(era)
           || settings.Eras.TryGetValue(era.ToString(), out var eraSettings)
           && eraSettings.EnableShop.HasValue;

    private static bool HasActiveShopSeason(ServerSettings settings, ISet<GameEra> enabledEras, GameEra era)
    {
        if (!enabledEras.Contains(era)
            || !settings.Eras.TryGetValue(era.ToString(), out var eraSettings)
            || eraSettings.EnableShop != true)
        {
            return true;
        }

        return eraSettings.ActiveShopSeasonId is > 0;
    }
}
```

- [ ] **Step 6: Add explicit Blue shipped settings**

Modify `Host/Configurations/ServerSettings.json` so the Blue block is:

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

Keep current local Green/Nijiiro enabled values unchanged.

- [ ] **Step 7: Register Blue catalog in infrastructure**

Modify `Infrastructure/DependencyInjection.cs`:

```csharp
using TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;
```

Add this block after the Green registration block:

```csharp
if (enabledEras.Contains(GameEra.Blue))
{
    services.AddSingleton<BlueEraGameDataCatalog>();
    services.AddSingleton<IBlueCatalog>(sp => sp.GetRequiredService<BlueEraGameDataCatalog>());
    services.AddSingleton<IEraGameDataCatalog>(sp => sp.GetRequiredService<BlueEraGameDataCatalog>());
}
```

- [ ] **Step 8: Run settings, registration, and existing foundation tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueServerSettingsValidationTests Or FullyQualifiedName~BlueInfrastructureRegistrationTests Or FullyQualifiedName~BlueEraFoundationTests Or FullyQualifiedName~GreenServerSettingsValidationTests"
```

Expected: PASS.

- [ ] **Step 9: Commit settings and DI wiring**

Run:

```powershell
git diff -- Application/Settings/ServerSettings.cs Application/Settings/ServerSettingsOptionsValidationExtensions.cs Host/Configurations/ServerSettings.json Infrastructure/DependencyInjection.cs Tests/Blue/BlueServerSettingsValidationTests.cs Tests/Blue/BlueInfrastructureRegistrationTests.cs
git add -- Application/Settings/ServerSettings.cs Application/Settings/ServerSettingsOptionsValidationExtensions.cs Host/Configurations/ServerSettings.json Infrastructure/DependencyInjection.cs Tests/Blue/BlueServerSettingsValidationTests.cs Tests/Blue/BlueInfrastructureRegistrationTests.cs
git commit -m "Wire Blue catalog settings and DI"
```

---

### Task 6: Document Blue Data Layout And Guard Against Battle Scope Creep

**Files:**
- Modify: `README.md`
- Modify: `Host/README.md`
- Test: `Tests/Blue/BlueDocsTests.cs`

- [ ] **Step 1: Write failing docs tests**

Create `Tests/Blue/BlueDocsTests.cs`:

```csharp
namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueDocsTests
{
    [Fact]
    public void RootReadme_DocumentsBlueDataLayout()
    {
        var source = File.ReadAllText(FindRepoFile("README.md"));

        Assert.Contains("wwwroot/data/blue/data/config/S10100-1/musicinfo.xml", source, StringComparison.Ordinal);
        Assert.Contains("wwwroot/data/blue/data/config/S10100-1/musicmedleyinfo.xml", source, StringComparison.Ordinal);
        Assert.Contains("wwwroot/data/blue/data/fumen/tuning.bin", source, StringComparison.Ordinal);
    }

    [Fact]
    public void HostReadme_DocumentsBlueSymlinkAndBattleDeferral()
    {
        var source = File.ReadAllText(FindRepoFile("Host", "README.md"));

        Assert.Contains("Blue AC15 Test Support", source, StringComparison.Ordinal);
        Assert.Contains("S10100-1", source, StringComparison.Ordinal);
        Assert.Contains("wwwroot/data/blue/data", source, StringComparison.Ordinal);
        Assert.Contains("blue_item_shop_data.json", source, StringComparison.Ordinal);
        Assert.Contains("config/S10100-1/battle", source, StringComparison.Ordinal);
        Assert.Contains("Track B", source, StringComparison.Ordinal);
    }

    private static string FindRepoFile(params string[] pathParts)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(pathParts).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException($"Could not find {Path.Combine(pathParts)}.");
    }
}
```

- [ ] **Step 2: Run failing docs tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueDocsTests
```

Expected: FAIL because docs do not yet mention Blue A2 paths.

- [ ] **Step 3: Update root README data setup**

Modify `README.md` in the setup/data section to mention Blue required files:

```markdown
For Blue AC15, provide the game's `USRDIR/data` folder under
`wwwroot/data/blue/data`. The required normal catalog files are:

- `wwwroot/data/blue/data/config/S10100-1/musicinfo.xml`
- `wwwroot/data/blue/data/config/S10100-1/musicmedleyinfo.xml`
- `wwwroot/data/blue/data/fumen/tuning.bin`

Battle data under `wwwroot/data/blue/data/config/S10100-1/battle` is reserved
for the later Blue battle-mode track.
```

Place it next to the existing Green setup text.

- [ ] **Step 4: Update Host README data layout**

Modify `Host/README.md`:

1. In the `wwwroot/data/` layout block, add:

```text
|-- blue/                       Blue-era AC15 data
|   |-- blue_event_folder_data.json Optional Blue event folders
|   |-- blue_recommend_songs.json   Optional Blue pushed/recommended songs
|   |-- blue_telop_data.json        Optional Blue telops
|   |-- blue_movie_data.json        Blue attract movie permissions
|   |-- blue_item_shop_data.json    Blue item shop seasons and item rows
|   |-- blue_costume_data.json      Generated or curated Blue customization catalog
|   |-- blue_title_data.json        Generated or curated Blue title catalog
|   |-- blue_neiro_data.json        Generated or curated Blue tone catalog
|   `-- data/                       Blue game USRDIR/data tree, often symlinked
|       |-- config/S10100-1/
|       |   |-- musicinfo.xml
|       |   |-- musicmedleyinfo.xml
|       |   `-- battle/             Present but reserved for Track B
|       `-- fumen/
|           `-- tuning.bin
```

2. Add a `## Blue AC15 Test Support` section after the Green section:

```markdown
## Blue AC15 Test Support

When `ServerSettings:Eras:Blue:Enabled` is `true`, the server requires:

- `wwwroot/data/blue/data/config/S10100-1/musicinfo.xml`
- `wwwroot/data/blue/data/config/S10100-1/musicmedleyinfo.xml`
- `wwwroot/data/blue/data/fumen/tuning.bin`

Blue uses the original game `USRDIR/data` layout. From a source checkout:

```powershell
New-Item -ItemType SymbolicLink -Target 'path\to\rpcs3\dev_hdd0\game\SCEEXE001\USRDIR\data\' -Path '.\Host\wwwroot\data\blue\data'
```

From an extracted release folder:

```powershell
New-Item -ItemType SymbolicLink -Target 'path\to\rpcs3\dev_hdd0\game\SCEEXE001\USRDIR\data\' -Path '.\wwwroot\data\blue\data'
```

If copying instead of linking:

```powershell
New-Item -ItemType Directory -Force -Path '.\wwwroot\data\blue' | Out-Null
Copy-Item -Recurse -Path 'path\to\rpcs3\dev_hdd0\game\SCEEXE001\USRDIR\data' -Destination '.\wwwroot\data\blue\data'
```

Optional Blue JSON files under `wwwroot/data/blue/` default to empty catalog
data unless a feature setting requires them. `blue_item_shop_data.json` is
required only when `ServerSettings:Eras:Blue:EnableShop` is `true`; then
`ActiveShopSeasonId` must match a season in that file.

`config/S10100-1/battle` exists in Blue game data, but battle catalog parsing
and battle mode behavior are reserved for Track B.
```

- [ ] **Step 5: Run docs tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueDocsTests
```

Expected: PASS.

- [ ] **Step 6: Commit docs**

Run:

```powershell
git diff -- README.md Host/README.md Tests/Blue/BlueDocsTests.cs
git add -- README.md Host/README.md Tests/Blue/BlueDocsTests.cs
git commit -m "Document Blue catalog data layout"
```

---

### Task 7: Final Verification

**Files:**
- Read-only verification across the full solution.

- [ ] **Step 1: Verify focused Blue tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~Blue
```

Expected: PASS. Tests that depend on ignored local Blue data may skip by returning early when files are absent.

- [ ] **Step 2: Verify shared AC15 tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~Ac15
```

Expected: PASS.

- [ ] **Step 3: Verify Green catalog regressions**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenCatalog Or FullyQualifiedName~GreenItemShop Or FullyQualifiedName~GreenRecommend Or FullyQualifiedName~GreenMovie"
```

Expected: PASS. Confirm at least one Green catalog or item-shop test ran.

- [ ] **Step 4: Verify full test suite**

Run:

```powershell
dotnet test Tests/Tests.csproj
```

Expected: PASS.

- [ ] **Step 5: Verify Host build using temp output**

Run:

```powershell
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-a2"
```

Expected: PASS. Use this temp output path to avoid locked `Host/bin` outputs.

- [ ] **Step 6: Verify battle scope remains untouched**

Run:

```powershell
rg -n "S10100-1[\\/]+battle|battleadjsetting|battlenpcinfo|battlestageinfo|battlesupportinfo|battletokeninfo" Application Infrastructure Tests -g "*.cs"
```

Expected: no matches, except a docs test string if `Tests/Blue/BlueDocsTests.cs` includes the documented path. Do not add runtime parsing or catalog surfaces for these files in A2.

- [ ] **Step 7: Inspect final diff**

Run:

```powershell
git status --short
git diff --stat HEAD
git diff --name-status HEAD
```

Expected: only A2 implementation files are modified or added.

- [ ] **Step 8: Commit any final verification-only cleanup**

If Step 7 shows only intended A2 changes that have not been committed, commit them:

```powershell
git add -- Application Infrastructure Host README.md Tests
git commit -m "Complete Blue catalog data layout"
```

If all task commits are already complete and the worktree is clean, do not create an empty commit.
