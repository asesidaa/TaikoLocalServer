# 02 - Extractor Phase 1

**Goal:** Build the read-only Green catalog extractor for ids-only output from mounted game data.

**Files:**
- Create: `Infrastructure/GameDataCatalog/Green/Extractor/GreenExtractorOptions.cs`
- Create: `Infrastructure/GameDataCatalog/Green/Extractor/GreenCatalogExtractor.cs`
- Create: `Infrastructure/GameDataCatalog/Green/Extractor/NdpReader.cs`
- Create: `Infrastructure/GameDataCatalog/Green/Extractor/BoostXmlReader.cs`
- Create: `Infrastructure/GameDataCatalog/Green/Extractor/Don3dDirScanner.cs`
- Create: `Infrastructure/GameDataCatalog/Green/Extractor/Sources/OverridesLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Green/Extractor/Sources/EbootStringResolver.cs`
- Create: `Infrastructure/GameDataCatalog/Green/Extractor/Sources/WikiScraper.cs`
- Create: `Infrastructure/GameDataCatalog/Green/Extractor/Merging/CostumeMerger.cs`
- Create: `Infrastructure/GameDataCatalog/Green/Extractor/Merging/TitleMerger.cs`
- Create: `Infrastructure/GameDataCatalog/Green/Extractor/Merging/NeiroMerger.cs`
- Create: `Infrastructure/GameDataCatalog/Green/Extractor/Output/GreenCatalogEnvelope.cs`
- Create: `Infrastructure/GameDataCatalog/Green/Extractor/Output/CatalogWriter.cs`
- Create: `GreenCatalogExtractor/GreenCatalogExtractor.csproj`
- Create: `GreenCatalogExtractor/Program.cs`
- Modify: `TaikoLocalServer.slnx`
- Create: `Tests/Green/GreenCustomizationExtractorTests.cs`

## Task 1: Container and XML Readers

- [ ] **Step 1: Add failing reader tests**

Create `Tests/Green/GreenCustomizationExtractorTests.cs`:

```csharp
using System.Buffers.Binary;
using System.Text;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenCustomizationExtractorTests
{
    [Fact]
    public void NdpReader_ReadsNutPackType1Entries()
    {
        var blob = BuildNdp(("cos_name_000.nut", 0x40u, 0x20u), ("cos_name_001.nut", 0x60u, 0x30u));

        var entries = NdpReader.Read(blob);

        Assert.Equal(2, entries.Count);
        Assert.Equal("cos_name_000.nut", entries[0].FileName);
        Assert.Equal(0x40u, entries[0].Offset);
        Assert.Equal(0x20u, entries[0].Size);
        Assert.Equal((uint)1, entries[1].Id);
    }

    [Fact]
    public async Task BoostXmlReader_ReadsRewardTitleIds()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xml");
        await File.WriteAllTextAsync(path, """
            <?xml version="1.0" encoding="UTF-8" standalone="yes" ?>
            <boost_serialization signature="serialization::archive" version="10">
              <RewardTitleFiltering>
                <support>
                  <size>2</size>
                  <rewardtitle>131</rewardtitle>
                  <rewardtitle>132</rewardtitle>
                </support>
              </RewardTitleFiltering>
            </boost_serialization>
            """);

        var ids = await BoostXmlReader.ReadRewardTitleIdsAsync(path, CancellationToken.None);

        Assert.Equal(new uint[] { 131, 132 }, ids);
        File.Delete(path);
    }

    [Fact]
    public async Task Don3dDirScanner_ReadsModelPairs()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "don3d", "cos"));
        await File.WriteAllTextAsync(Path.Combine(root, "don3d", "cos", "cos_000007.nud"), string.Empty);
        await File.WriteAllTextAsync(Path.Combine(root, "don3d", "cos", "cos_000007.nut"), string.Empty);
        await File.WriteAllTextAsync(Path.Combine(root, "don3d", "cos", "cos_000008.nud"), string.Empty);

        var result = Don3dDirScanner.Scan(root);

        Assert.Equal(new uint[] { 7 }, result.FullCosModelPairIds);
        Directory.Delete(root, recursive: true);
    }

    private static byte[] BuildNdp(params (string Name, uint Offset, uint Size)[] entries)
    {
        var bytes = new byte[0x50 + entries.Sum(entry => 4 + Align4(Encoding.ASCII.GetByteCount(entry.Name) + 1) + 8)];
        Encoding.ASCII.GetBytes("NUT_PACK_TYPE1").CopyTo(bytes, 0);
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(0x40), (uint)entries.Length);
        var cursor = 0x50;

        foreach (var entry in entries)
        {
            var nameBytes = Encoding.ASCII.GetBytes(entry.Name);
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(cursor), (uint)(nameBytes.Length + 1));
            cursor += 4;
            nameBytes.CopyTo(bytes.AsSpan(cursor));
            cursor += nameBytes.Length + 1;
            cursor = Align4(cursor);
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(cursor), entry.Offset);
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(cursor + 4), entry.Size);
            cursor += 8;
        }

        return bytes;
    }

    private static int Align4(int value) => (value + 3) & ~3;
}
```

- [ ] **Step 2: Run reader tests and confirm failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenCustomizationExtractorTests"
```

Expected: FAIL because extractor reader classes do not exist.

- [ ] **Step 3: Create `NdpReader`**

Create `Infrastructure/GameDataCatalog/Green/Extractor/NdpReader.cs`:

```csharp
using System.Buffers.Binary;
using System.Text;
using System.Text.RegularExpressions;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

public sealed record NdpEntry(uint Id, string FileName, uint Offset, uint Size);

public static partial class NdpReader
{
    private const int EntryCountOffset = 0x40;
    private const int EntryTableOffset = 0x50;

    public static IReadOnlyList<NdpEntry> Read(byte[] bytes)
    {
        if (bytes.Length < EntryTableOffset)
        {
            throw new InvalidDataException("NUT_PACK_TYPE1 file is shorter than the fixed header.");
        }

        var magic = Encoding.ASCII.GetString(bytes.AsSpan(0, "NUT_PACK_TYPE1".Length));
        if (magic != "NUT_PACK_TYPE1")
        {
            throw new InvalidDataException($"Unexpected NDP magic '{magic}'.");
        }

        var count = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(EntryCountOffset, 4));
        var cursor = EntryTableOffset;
        var entries = new List<NdpEntry>(checked((int)count));

        for (var index = 0; index < count; index++)
        {
            EnsureAvailable(bytes, cursor, 4);
            var nameLength = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(cursor, 4));
            cursor += 4;

            EnsureAvailable(bytes, cursor, checked((int)nameLength));
            var rawName = bytes.AsSpan(cursor, checked((int)nameLength));
            var nul = rawName.IndexOf((byte)0);
            var fileName = Encoding.ASCII.GetString(nul >= 0 ? rawName[..nul] : rawName);
            cursor += checked((int)nameLength);
            cursor = Align4(cursor);

            EnsureAvailable(bytes, cursor, 8);
            var offset = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(cursor, 4));
            var size = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(cursor + 4, 4));
            cursor += 8;

            entries.Add(new NdpEntry(ParseId(fileName), fileName, offset, size));
        }

        return entries.OrderBy(entry => entry.Id).ThenBy(entry => entry.FileName, StringComparer.Ordinal).ToArray();
    }

    public static IReadOnlyList<NdpEntry> ReadFile(string path)
        => Read(File.ReadAllBytes(path));

    private static void EnsureAvailable(byte[] bytes, int offset, int length)
    {
        if (offset < 0 || length < 0 || offset + length > bytes.Length)
        {
            throw new InvalidDataException($"NDP entry table read exceeds file length at 0x{offset:X}.");
        }
    }

    private static int Align4(int value) => (value + 3) & ~3;

    private static uint ParseId(string fileName)
    {
        var match = LastNumberRegex().Matches(fileName).LastOrDefault();
        return match is null ? 0 : uint.Parse(match.Value);
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex LastNumberRegex();
}
```

- [ ] **Step 4: Create `BoostXmlReader`**

Create `Infrastructure/GameDataCatalog/Green/Extractor/BoostXmlReader.cs`:

```csharp
using System.Xml.Linq;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

public static class BoostXmlReader
{
    public static async Task<IReadOnlyList<uint>> ReadRewardTitleIdsAsync(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

        return document
            .Descendants("rewardtitle")
            .Select(element => uint.TryParse(element.Value, out var id) ? id : (uint?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .OrderBy(id => id)
            .ToArray();
    }
}
```

- [ ] **Step 5: Create `Don3dDirScanner`**

Create `Infrastructure/GameDataCatalog/Green/Extractor/Don3dDirScanner.cs`:

```csharp
using System.Text.RegularExpressions;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

public sealed record Don3dScanResult(
    IReadOnlyList<uint> FullCosModelPairIds,
    IReadOnlyDictionary<string, IReadOnlyList<uint>> DirectoryIds);

public static partial class Don3dDirScanner
{
    private static readonly string[] CostumeDirectories =
    [
        Path.Combine("don3d", "cos"),
        Path.Combine("don3d", "face"),
        Path.Combine("don3d", "parts", "acc"),
        Path.Combine("don3d", "parts", "body"),
        Path.Combine("don3d", "parts", "head"),
        Path.Combine("don3d", "parts", "paint"),
        Path.Combine("don3d", "full", "cos"),
        Path.Combine("don3d", "full", "face")
    ];

    public static Don3dScanResult Scan(string gameDataRoot)
    {
        var byDirectory = new Dictionary<string, IReadOnlyList<uint>>(StringComparer.OrdinalIgnoreCase);

        foreach (var relative in CostumeDirectories)
        {
            var absolute = Path.Combine(gameDataRoot, relative);
            byDirectory[relative.Replace('\\', '/')] = Directory.Exists(absolute)
                ? ScanDirectoryForPairedIds(absolute)
                : [];
        }

        byDirectory.TryGetValue("don3d/cos", out var fullCosIds);
        return new Don3dScanResult(fullCosIds ?? [], byDirectory);
    }

    private static IReadOnlyList<uint> ScanDirectoryForPairedIds(string directory)
    {
        var nudIds = Directory.EnumerateFiles(directory, "*.nud", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileNameWithoutExtension)
            .Select(ParseLastNumber)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

        var nutIds = Directory.EnumerateFiles(directory, "*.nut", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileNameWithoutExtension)
            .Select(ParseLastNumber)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

        return nudIds.Intersect(nutIds).OrderBy(id => id).ToArray();
    }

    private static uint? ParseLastNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var match = NumberRegex().Matches(value).LastOrDefault();
        return match is null ? null : uint.Parse(match.Value);
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex NumberRegex();
}
```

- [ ] **Step 6: Run reader tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenCustomizationExtractorTests"
```

Expected: PASS for the three reader tests.

## Task 2: Merge and Write Catalogs

- [ ] **Step 1: Append merger and writer tests**

Append these tests to `Tests/Green/GreenCustomizationExtractorTests.cs`:

```csharp
[Fact]
public async Task GreenCatalogExtractor_WritesDeterministicIdsOnlyCatalogs()
{
    var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    var outDir = Path.Combine(root, "out");
    Directory.CreateDirectory(Path.Combine(root, "nutdata", "cos_name"));
    Directory.CreateDirectory(Path.Combine(root, "nutdata", "title_name"));
    Directory.CreateDirectory(Path.Combine(root, "nutdata", "tone_name"));
    Directory.CreateDirectory(Path.Combine(root, "config", "S11100-1"));
    Directory.CreateDirectory(Path.Combine(root, "don3d", "cos"));

    await File.WriteAllBytesAsync(Path.Combine(root, "nutdata", "cos_name", "nutdatapack.ndp"), BuildNdp(("cos_name_002.nut", 0, 1), ("cos_name_001.nut", 1, 1)));
    await File.WriteAllBytesAsync(Path.Combine(root, "nutdata", "title_name", "nutdatapack.ndp"), BuildNdp(("title_name_132.nut", 0, 1), ("title_name_131.nut", 1, 1)));
    await File.WriteAllBytesAsync(Path.Combine(root, "nutdata", "tone_name", "nutdatapack.ndp"), BuildNdp(("tone_name_004.nut", 0, 1)));
    await File.WriteAllTextAsync(Path.Combine(root, "config", "S11100-1", "rewardtitlefiltering.xml"), """
        <boost_serialization>
          <RewardTitleFiltering>
            <support>
              <rewardtitle>131</rewardtitle>
            </support>
          </RewardTitleFiltering>
        </boost_serialization>
        """);
    await File.WriteAllTextAsync(Path.Combine(root, "don3d", "cos", "cos_000002.nud"), string.Empty);
    await File.WriteAllTextAsync(Path.Combine(root, "don3d", "cos", "cos_000002.nut"), string.Empty);

    await GreenCatalogExtractor.ExtractAsync(
        new GreenExtractorOptions(GameDataPath: root, OutputDirectory: outDir),
        CancellationToken.None);

    var costumeJson = await File.ReadAllTextAsync(Path.Combine(outDir, "green_costume_data.json"));
    var titleJson = await File.ReadAllTextAsync(Path.Combine(outDir, "green_title_data.json"));
    var neiroJson = await File.ReadAllTextAsync(Path.Combine(outDir, "green_neiro_data.json"));

    Assert.Contains("\"schemaVersion\": 1", costumeJson);
    Assert.True(costumeJson.IndexOf("\"costumeId\": 1", StringComparison.Ordinal) < costumeJson.IndexOf("\"costumeId\": 2", StringComparison.Ordinal));
    Assert.Contains("\"source\": \"ndp+don3d\"", costumeJson);
    Assert.Contains("\"titleId\": 131", titleJson);
    Assert.Contains("\"source\": \"ndp+rewardtitlefiltering\"", titleJson);
    Assert.Contains("\"neiroId\": 4", neiroJson);

    Directory.Delete(root, recursive: true);
}
```

- [ ] **Step 2: Run extractor test and confirm failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenCatalogExtractor_WritesDeterministicIdsOnlyCatalogs"
```

Expected: FAIL because the extractor facade and merge/write classes do not exist.

- [ ] **Step 3: Create extractor options and envelope**

Create `Infrastructure/GameDataCatalog/Green/Extractor/GreenExtractorOptions.cs`:

```csharp
namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

public sealed record GreenExtractorOptions(
    string GameDataPath,
    string OutputDirectory,
    string? EbootPath = null,
    bool UseWiki = false,
    string? OverridesPath = null);
```

Create `Infrastructure/GameDataCatalog/Green/Extractor/Output/GreenCatalogEnvelope.cs`:

```csharp
namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Output;

public sealed class GreenCatalogEnvelope<T>
{
    public int SchemaVersion { get; init; } = 1;

    public IReadOnlyList<T> Items { get; init; } = [];
}
```

- [ ] **Step 4: Create optional source resolvers with deterministic empty behavior**

Create `Infrastructure/GameDataCatalog/Green/Extractor/Sources/EbootStringResolver.cs`:

```csharp
namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Sources;

public sealed class EbootStringResolver
{
    public Task<IReadOnlyDictionary<string, string>> ResolveAsync(
        string? ebootPath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyDictionary<string, string>>(new Dictionary<string, string>());
    }
}
```

Create `Infrastructure/GameDataCatalog/Green/Extractor/Sources/WikiScraper.cs`:

```csharp
namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Sources;

public sealed class WikiScraper
{
    public Task<IReadOnlyDictionary<string, string>> ResolveAsync(
        bool enabled,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyDictionary<string, string>>(new Dictionary<string, string>());
    }
}
```

Create `Infrastructure/GameDataCatalog/Green/Extractor/Sources/OverridesLoader.cs`:

```csharp
using System.Text.Json;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Sources;

public sealed class GreenCatalogOverrides
{
    public IReadOnlyDictionary<uint, GreenCostumeOverride> Costumes { get; init; } = new Dictionary<uint, GreenCostumeOverride>();
    public IReadOnlyDictionary<uint, GreenNamedOverride> Titles { get; init; } = new Dictionary<uint, GreenNamedOverride>();
    public IReadOnlyDictionary<uint, GreenNamedOverride> Neiros { get; init; } = new Dictionary<uint, GreenNamedOverride>();
}

public sealed class GreenCostumeOverride
{
    public string? Name { get; init; }
    public string? CostumeType { get; init; }
}

public sealed class GreenNamedOverride
{
    public string? Name { get; init; }
}

public static class OverridesLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<GreenCatalogOverrides> LoadAsync(
        string? path,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return new GreenCatalogOverrides();
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<GreenCatalogOverrides>(stream, JsonOptions, cancellationToken)
               ?? new GreenCatalogOverrides();
    }
}
```

- [ ] **Step 5: Create mergers**

Create `Infrastructure/GameDataCatalog/Green/Extractor/Merging/CostumeMerger.cs`:

```csharp
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Sources;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Merging;

public static class CostumeMerger
{
    public static IReadOnlyList<Costume> Merge(
        IEnumerable<NdpEntry> ndpEntries,
        Don3dScanResult don3d,
        GreenCatalogOverrides overrides)
    {
        var don3dIds = don3d.DirectoryIds.Values.SelectMany(ids => ids).ToHashSet();

        return ndpEntries
            .Select(entry =>
            {
                overrides.Costumes.TryGetValue(entry.Id, out var itemOverride);
                var hasDon3d = don3dIds.Contains(entry.Id);
                return new Costume
                {
                    CostumeId = entry.Id,
                    CostumeType = string.IsNullOrWhiteSpace(itemOverride?.CostumeType) ? "unknown" : itemOverride!.CostumeType!,
                    CostumeName = itemOverride?.Name ?? string.Empty,
                    CostumeNameEN = itemOverride?.Name ?? string.Empty,
                    CostumeNameCN = itemOverride?.Name ?? string.Empty,
                    CostumeNameKO = itemOverride?.Name ?? string.Empty,
                    Source = hasDon3d ? "ndp+don3d" : "ndp"
                };
            })
            .GroupBy(costume => costume.CostumeId)
            .Select(group => group.First())
            .OrderBy(costume => costume.CostumeId)
            .ToArray();
    }
}
```

Create `Infrastructure/GameDataCatalog/Green/Extractor/Merging/TitleMerger.cs`:

```csharp
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Sources;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Merging;

public static class TitleMerger
{
    public static IReadOnlyList<Title> Merge(
        IEnumerable<NdpEntry> ndpEntries,
        IEnumerable<uint> rewardTitleIds,
        GreenCatalogOverrides overrides)
    {
        var rewardSet = rewardTitleIds.ToHashSet();

        return ndpEntries
            .Select(entry =>
            {
                overrides.Titles.TryGetValue(entry.Id, out var itemOverride);
                return new Title
                {
                    TitleId = entry.Id,
                    TitleName = itemOverride?.Name ?? string.Empty,
                    TitleNameEN = itemOverride?.Name ?? string.Empty,
                    TitleNameCN = itemOverride?.Name ?? string.Empty,
                    TitleNameKO = itemOverride?.Name ?? string.Empty,
                    TitleRarity = 0,
                    Source = rewardSet.Contains(entry.Id) ? "ndp+rewardtitlefiltering" : "ndp"
                };
            })
            .GroupBy(title => title.TitleId)
            .Select(group => group.First())
            .OrderBy(title => title.TitleId)
            .ToArray();
    }
}
```

Create `Infrastructure/GameDataCatalog/Green/Extractor/Merging/NeiroMerger.cs`:

```csharp
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Sources;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Merging;

public static class NeiroMerger
{
    public static IReadOnlyList<Neiro> Merge(
        IEnumerable<NdpEntry> ndpEntries,
        GreenCatalogOverrides overrides)
    {
        return ndpEntries
            .Select(entry =>
            {
                overrides.Neiros.TryGetValue(entry.Id, out var itemOverride);
                return new Neiro
                {
                    NeiroId = entry.Id,
                    NeiroName = itemOverride?.Name ?? string.Empty,
                    NeiroNameEN = itemOverride?.Name ?? string.Empty,
                    NeiroNameCN = itemOverride?.Name ?? string.Empty,
                    NeiroNameKO = itemOverride?.Name ?? string.Empty,
                    Source = "ndp"
                };
            })
            .GroupBy(neiro => neiro.NeiroId)
            .Select(group => group.First())
            .OrderBy(neiro => neiro.NeiroId)
            .ToArray();
    }
}
```

- [ ] **Step 6: Create deterministic writer**

Create `Infrastructure/GameDataCatalog/Green/Extractor/Output/CatalogWriter.cs`:

```csharp
using System.Text.Json;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Output;

public static class CatalogWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public static async Task WriteAsync<T>(
        string outputDirectory,
        string fileName,
        IReadOnlyList<T> items,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, fileName);
        await using var stream = File.Create(path);
        var envelope = new GreenCatalogEnvelope<T>
        {
            Items = items
        };
        await JsonSerializer.SerializeAsync(stream, envelope, JsonOptions, cancellationToken);
    }
}
```

- [ ] **Step 7: Create extractor facade**

Create `Infrastructure/GameDataCatalog/Green/Extractor/GreenCatalogExtractor.cs`:

```csharp
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Merging;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Output;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Sources;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

public static class GreenCatalogExtractor
{
    public const string CostumeFileName = "green_costume_data.json";
    public const string TitleFileName = "green_title_data.json";
    public const string NeiroFileName = "green_neiro_data.json";

    public static async Task ExtractAsync(
        GreenExtractorOptions options,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var cosNamePack = Path.Combine(options.GameDataPath, "nutdata", "cos_name", "nutdatapack.ndp");
        var titleNamePack = Path.Combine(options.GameDataPath, "nutdata", "title_name", "nutdatapack.ndp");
        var toneNamePack = Path.Combine(options.GameDataPath, "nutdata", "tone_name", "nutdatapack.ndp");
        var rewardTitleFiltering = Path.Combine(options.GameDataPath, "config", "S11100-1", "rewardtitlefiltering.xml");

        var cosEntries = File.Exists(cosNamePack) ? NdpReader.ReadFile(cosNamePack) : [];
        var titleEntries = File.Exists(titleNamePack) ? NdpReader.ReadFile(titleNamePack) : [];
        var toneEntries = File.Exists(toneNamePack) ? NdpReader.ReadFile(toneNamePack) : [];
        var rewardTitleIds = File.Exists(rewardTitleFiltering)
            ? await BoostXmlReader.ReadRewardTitleIdsAsync(rewardTitleFiltering, cancellationToken)
            : [];

        var overrides = await OverridesLoader.LoadAsync(options.OverridesPath, cancellationToken);
        _ = await new EbootStringResolver().ResolveAsync(options.EbootPath, cancellationToken);
        _ = await new WikiScraper().ResolveAsync(options.UseWiki, cancellationToken);

        var don3d = Don3dDirScanner.Scan(options.GameDataPath);
        var costumes = CostumeMerger.Merge(cosEntries, don3d, overrides);
        var titles = TitleMerger.Merge(titleEntries, rewardTitleIds, overrides);
        var neiros = NeiroMerger.Merge(toneEntries, overrides);

        await CatalogWriter.WriteAsync(options.OutputDirectory, CostumeFileName, costumes, cancellationToken);
        await CatalogWriter.WriteAsync(options.OutputDirectory, TitleFileName, titles, cancellationToken);
        await CatalogWriter.WriteAsync(options.OutputDirectory, NeiroFileName, neiros, cancellationToken);
    }
}
```

- [ ] **Step 8: Run extractor tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenCustomizationExtractorTests"
```

Expected: PASS.

## Task 3: CLI Project

- [ ] **Step 1: Create CLI project file**

Create `GreenCatalogExtractor/GreenCatalogExtractor.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="System.CommandLine" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Infrastructure\Infrastructure.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Create CLI program**

Create `GreenCatalogExtractor/Program.cs`:

```csharp
using System.CommandLine;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

var root = new RootCommand("Extract Green customization catalogs from a read-only game-data tree.");
var extract = new Command("extract", "Extract Green costume, title, and tone catalogs.");

var gameDataOption = new Option<string>(
    "--game-data",
    () => Path.Combine("Host", "wwwroot", "data", "green", "data"),
    "Path to the Green USRDIR/data tree.");
var ebootOption = new Option<string?>("--eboot", "Optional EBOOT.ELF or .i64 path used by enrichment stages.");
var wikiOption = new Option<bool>("--wiki", "Enable wiki name enrichment.");
var overridesOption = new Option<string?>("--overrides", "Optional operator overrides JSON path.");
var outOption = new Option<string>(
    "--out",
    () => Path.Combine("Host", "wwwroot", "data", "green"),
    "Output directory for green_*_data.json.");

extract.AddOption(gameDataOption);
extract.AddOption(ebootOption);
extract.AddOption(wikiOption);
extract.AddOption(overridesOption);
extract.AddOption(outOption);
root.AddCommand(extract);

extract.SetHandler(async (gameData, eboot, wiki, overrides, output) =>
{
    await GreenCatalogExtractor.ExtractAsync(
        new GreenExtractorOptions(gameData, output, eboot, wiki, overrides),
        CancellationToken.None);
}, gameDataOption, ebootOption, wikiOption, overridesOption, outOption);

return await root.InvokeAsync(args);
```

- [ ] **Step 3: Add CLI project to solution**

Modify `TaikoLocalServer.slnx` and insert the project next to `LocalSaveModScoreMigrator`:

```xml
  <Project Path="GreenCatalogExtractor/GreenCatalogExtractor.csproj" />
```

- [ ] **Step 4: Run CLI on the local symlinked data tree**

Run:

```powershell
dotnet run --project GreenCatalogExtractor -- extract --game-data Host/wwwroot/data/green/data --out artifacts/green-customization-smoke
```

Expected: command exits 0 and writes:

```text
artifacts/green-customization-smoke/green_costume_data.json
artifacts/green-customization-smoke/green_title_data.json
artifacts/green-customization-smoke/green_neiro_data.json
```

- [ ] **Step 5: Build**

Run:

```powershell
dotnet build
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add -- Infrastructure/GameDataCatalog/Green/Extractor GreenCatalogExtractor TaikoLocalServer.slnx Tests/Green/GreenCustomizationExtractorTests.cs
git commit -m "Add Green customization catalog extractor"
```
