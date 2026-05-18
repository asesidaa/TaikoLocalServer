# 03 - Runtime Catalog Loading

**Goal:** Load extracted Green customization JSON at startup, expose it through `IGreenCatalog`, and auto-bootstrap Phase 1 extraction when JSON files are missing.

**Files:**
- Modify: `Application/Abstractions/IGreenCatalog.cs`
- Modify: `Application/Abstractions/INijiiroCatalog.cs`
- Modify: `Application/Settings/ServerSettings.cs`
- Modify: `Host/Configurations/ServerSettings.json`
- Modify: `Infrastructure/GameDataCatalog/Nijiiro/NijiiroEraGameDataCatalog.cs`
- Create: `Infrastructure/GameDataCatalog/Green/GreenCustomizationCatalogLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Green/GreenCostumeLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Green/GreenTitleLoader.cs`
- Create: `Infrastructure/GameDataCatalog/Green/GreenNeiroLoader.cs`
- Modify: `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`
- Modify: `Tests/Green/GreenHandlerFixture.cs`
- Create: `Tests/Green/GreenCustomizationCatalogLoaderTests.cs`

## Task 1: Interfaces and Settings

- [ ] **Step 1: Add catalog loader tests**

Create `Tests/Green/GreenCustomizationCatalogLoaderTests.cs`:

```csharp
using System.Text.Json;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Output;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenCustomizationCatalogLoaderTests
{
    [Fact]
    public async Task CostumeLoader_MissingFileReturnsEmptyList()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");

        var items = await GreenCostumeLoader.LoadFromFileAsync(path, CancellationToken.None);

        Assert.Empty(items);
    }

    [Fact]
    public async Task TitleLoader_ReadsEnvelopeAsDictionary()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(new GreenCatalogEnvelope<Title>
        {
            Items =
            [
                new Title { TitleId = 132, TitleName = "B" },
                new Title { TitleId = 131, TitleName = "A" }
            ]
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

        var items = await GreenTitleLoader.LoadFromFileAsync(path, CancellationToken.None);

        Assert.Equal(new uint[] { 131, 132 }, items.Keys.OrderBy(id => id));
        File.Delete(path);
    }

    [Fact]
    public async Task NeiroLoader_ReadsEnvelopeAsDictionary()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(new GreenCatalogEnvelope<Neiro>
        {
            Items =
            [
                new Neiro { NeiroId = 4, NeiroName = "Tone" }
            ]
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

        var items = await GreenNeiroLoader.LoadFromFileAsync(path, CancellationToken.None);

        Assert.Equal("Tone", items[4].NeiroName);
        File.Delete(path);
    }
}
```

- [ ] **Step 2: Run loader tests and confirm failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenCustomizationCatalogLoaderTests"
```

Expected: FAIL because loaders do not exist.

- [ ] **Step 3: Add Green catalog accessors**

Modify `Application/Abstractions/IGreenCatalog.cs` by adding these members:

```csharp
IReadOnlyList<Costume> GetCostumeList();

IReadOnlyDictionary<uint, Title> GetTitleDictionary();

IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary();
```

- [ ] **Step 4: Add Nijiiro tone catalog accessor**

Modify `Application/Abstractions/INijiiroCatalog.cs` by adding this member after `GetTitleDictionary()`:

```csharp
Dictionary<uint, Neiro> GetNeiroDictionary();
```

- [ ] **Step 5: Add era settings**

Modify `Application/Settings/ServerSettings.cs` so `EraSettings` is:

```csharp
public sealed class EraSettings
{
    public bool Enabled { get; set; }

    public bool AutoExtractCatalog { get; set; } = true;

    public string GameDataPath { get; set; } = "wwwroot/data/green/data";
}
```

- [ ] **Step 6: Add Green settings defaults**

Modify `Host/Configurations/ServerSettings.json` so the Green era block is:

```json
"Green": {
  "Enabled": true,
  "AutoExtractCatalog": true,
  "GameDataPath": "wwwroot/data/green/data"
}
```

## Task 2: Loader Classes

- [ ] **Step 1: Create shared loader helper**

Create `Infrastructure/GameDataCatalog/Green/GreenCustomizationCatalogLoader.cs`:

```csharp
using System.Text.Json;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Output;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

internal static class GreenCustomizationCatalogLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<IReadOnlyList<T>> LoadListAsync<T>(
        string path,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return [];
        }

        await using var stream = File.OpenRead(path);
        var envelope = await JsonSerializer.DeserializeAsync<GreenCatalogEnvelope<T>>(stream, JsonOptions, cancellationToken)
                       ?? new GreenCatalogEnvelope<T>();
        return envelope.Items;
    }
}
```

- [ ] **Step 2: Create Green costume loader**

Create `Infrastructure/GameDataCatalog/Green/GreenCostumeLoader.cs`:

```csharp
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenCostumeLoader
{
    public Task<IReadOnlyList<Costume>> LoadAsync(CancellationToken cancellationToken)
        => LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.Green), GreenCatalogExtractor.CostumeFileName),
            cancellationToken);

    public static Task<IReadOnlyList<Costume>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
        => GreenCustomizationCatalogLoader.LoadListAsync<Costume>(path, cancellationToken);
}
```

- [ ] **Step 3: Create Green title loader**

Create `Infrastructure/GameDataCatalog/Green/GreenTitleLoader.cs`:

```csharp
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenTitleLoader
{
    public async Task<IReadOnlyDictionary<uint, Title>> LoadAsync(CancellationToken cancellationToken)
        => await LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.Green), GreenCatalogExtractor.TitleFileName),
            cancellationToken);

    public static async Task<IReadOnlyDictionary<uint, Title>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var items = await GreenCustomizationCatalogLoader.LoadListAsync<Title>(path, cancellationToken);
        return items
            .GroupBy(title => title.TitleId)
            .ToDictionary(group => group.Key, group => group.First());
    }
}
```

- [ ] **Step 4: Create Green tone loader**

Create `Infrastructure/GameDataCatalog/Green/GreenNeiroLoader.cs`:

```csharp
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenNeiroLoader
{
    public async Task<IReadOnlyDictionary<uint, Neiro>> LoadAsync(CancellationToken cancellationToken)
        => await LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.Green), GreenCatalogExtractor.NeiroFileName),
            cancellationToken);

    public static async Task<IReadOnlyDictionary<uint, Neiro>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var items = await GreenCustomizationCatalogLoader.LoadListAsync<Neiro>(path, cancellationToken);
        return items
            .GroupBy(neiro => neiro.NeiroId)
            .ToDictionary(group => group.Key, group => group.First());
    }
}
```

- [ ] **Step 5: Run loader tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenCustomizationCatalogLoaderTests"
```

Expected: PASS.

## Task 3: Catalog Implementations

- [ ] **Step 1: Add Nijiiro tone dictionary**

Modify `Infrastructure/GameDataCatalog/Nijiiro/NijiiroEraGameDataCatalog.cs`:

Add this field next to `titleDictionary`:

```csharp
private readonly Dictionary<uint, Neiro> neiroDictionary = new();
```

Add this accessor after `GetTitleDictionary()`:

```csharp
public Dictionary<uint, Neiro> GetNeiroDictionary()
{
    return neiroDictionary;
}
```

Replace `InitializeToneFlagArraySize` with:

```csharp
private void InitializeToneFlagArraySize(Neiros? neiroData)
{
    neiroData.ThrowIfNull("Shouldn't happen!");
    toneFlagArraySize = (int)neiroData.NeiroEntries.Max(entry => entry.UniqueId) + 1;

    foreach (var entry in neiroData.NeiroEntries.OrderBy(entry => entry.UniqueId))
    {
        neiroDictionary[entry.UniqueId] = new Neiro
        {
            NeiroId = entry.UniqueId
        };
    }
}
```

- [ ] **Step 2: Add Green customization fields and accessors**

Modify `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`:

Add these `using` directives:

```csharp
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;
```

Change the primary constructor to:

```csharp
public sealed class GreenEraGameDataCatalog(
    ILogger<GreenEraGameDataCatalog> logger,
    IOptions<ServerSettings> serverSettings) : IGreenCatalog
```

Add these fields next to the existing catalog fields:

```csharp
private IReadOnlyList<Costume> costumeList = [];
private IReadOnlyDictionary<uint, Title> titleDictionary = new Dictionary<uint, Title>();
private IReadOnlyDictionary<uint, Neiro> neiroDictionary = new Dictionary<uint, Neiro>();
```

Add these accessors:

```csharp
public IReadOnlyList<Costume> GetCostumeList() => costumeList;

public IReadOnlyDictionary<uint, Title> GetTitleDictionary() => titleDictionary;

public IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary() => neiroDictionary;
```

- [ ] **Step 3: Add auto-bootstrap helpers**

Add these private methods inside `GreenEraGameDataCatalog`:

```csharp
private async Task BootstrapCustomizationCatalogAsync(CancellationToken cancellationToken)
{
    var outputDirectory = PathHelper.GetDataPath(GameEra.Green);
    var required = new[]
    {
        Path.Combine(outputDirectory, GreenCatalogExtractor.CostumeFileName),
        Path.Combine(outputDirectory, GreenCatalogExtractor.TitleFileName),
        Path.Combine(outputDirectory, GreenCatalogExtractor.NeiroFileName)
    };

    if (required.All(File.Exists))
    {
        return;
    }

    var greenSettings = GetGreenSettings();
    if (!greenSettings.AutoExtractCatalog)
    {
        logger.LogInformation("Green customization catalog auto-extract is disabled; continuing with empty or partial customization catalogs.");
        return;
    }

    var gameDataPath = ResolveConfiguredPath(greenSettings.GameDataPath);
    if (!Directory.Exists(gameDataPath))
    {
        logger.LogWarning("Green customization catalog JSON is missing and game data path does not exist: {Path}", gameDataPath);
        return;
    }

    try
    {
        logger.LogInformation("Green customization catalog JSON is missing; running first-run Phase 1 extraction from {Path}", gameDataPath);
        await GreenCatalogExtractor.ExtractAsync(
            new GreenExtractorOptions(gameDataPath, outputDirectory),
            cancellationToken);
    }
    catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException)
    {
        logger.LogWarning(ex, "Green customization catalog extraction failed; continuing with empty or partial customization catalogs.");
    }
}

private EraSettings GetGreenSettings()
{
    return serverSettings.Value.Eras.TryGetValue(nameof(GameEra.Green), out var settings)
        ? settings
        : new EraSettings();
}

private static string ResolveConfiguredPath(string configuredPath)
{
    if (Path.IsPathRooted(configuredPath))
    {
        return configuredPath;
    }

    var root = Directory.GetParent(PathHelper.GetRootPath())?.FullName
               ?? throw new InvalidOperationException("Could not resolve server root.");
    return Path.GetFullPath(Path.Combine(root, configuredPath));
}
```

- [ ] **Step 4: Load Green customization catalogs during initialization**

In `GreenEraGameDataCatalog.InitializeAsync`, insert this call after `GreenRequiredDataFiles.ThrowIfMissing();`:

```csharp
await BootstrapCustomizationCatalogAsync(cancellationToken);
```

Insert these loads after `recommend = await new GreenRecommendLoader().LoadAsync(...)`:

```csharp
costumeList = await new GreenCostumeLoader().LoadAsync(cancellationToken);
titleDictionary = await new GreenTitleLoader().LoadAsync(cancellationToken);
neiroDictionary = await new GreenNeiroLoader().LoadAsync(cancellationToken);
```

Update the log message to include customization counts:

```csharp
logger.LogInformation(
    "Loaded Green catalog: {SongCount} songs, song_hash_ver={SongHashVersion}, {TaikojukuCount} taikojuku packs, {StarCount} tuning star rows, {CostumeCount} costumes, {TitleCount} titles, {NeiroCount} tones",
    musicInfoFileOrder.Count,
    songHashVersion,
    taikojukuFileOrder.Count,
    stars.Count,
    costumeList.Count,
    titleDictionary.Count,
    neiroDictionary.Count);
```

- [ ] **Step 5: Build and run loader tests**

Modify `Tests/Green/GreenHandlerFixture.cs` inside `TestGreenCatalog` by adding customization catalog properties and methods:

```csharp
public IReadOnlyList<Costume> CostumeList { get; init; } =
[
    new() { CostumeId = 0, CostumeType = "kigurumi" },
    new() { CostumeId = 1, CostumeType = "head" },
    new() { CostumeId = 2, CostumeType = "body" },
    new() { CostumeId = 3, CostumeType = "face" },
    new() { CostumeId = 4, CostumeType = "puchi" }
];

public IReadOnlyDictionary<uint, Title> TitleDictionary { get; init; } =
    new Dictionary<uint, Title>
    {
        [10] = new() { TitleId = 10, TitleName = "Green Title", TitleRarity = 0 }
    };

public IReadOnlyDictionary<uint, Neiro> NeiroDictionary { get; init; } =
    new Dictionary<uint, Neiro>
    {
        [0] = new() { NeiroId = 0, NeiroName = "Taiko" },
        [4] = new() { NeiroId = 4, NeiroName = "Tone 4" }
    };

public IReadOnlyList<Costume> GetCostumeList() => CostumeList;

public IReadOnlyDictionary<uint, Title> GetTitleDictionary() => TitleDictionary;

public IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary() => NeiroDictionary;
```

Keep the existing constructor, music catalog, Taikojuku, item shop, and `InitializeAsync` members unchanged.

- [ ] **Step 6: Build and run loader tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenCustomizationCatalogLoaderTests"
dotnet build
```

Expected: both PASS.

- [ ] **Step 7: Commit**

```powershell
git add -- Application/Abstractions/IGreenCatalog.cs Application/Abstractions/INijiiroCatalog.cs Application/Settings/ServerSettings.cs Host/Configurations/ServerSettings.json Infrastructure/GameDataCatalog/Nijiiro/NijiiroEraGameDataCatalog.cs Infrastructure/GameDataCatalog/Green/GreenCustomizationCatalogLoader.cs Infrastructure/GameDataCatalog/Green/GreenCostumeLoader.cs Infrastructure/GameDataCatalog/Green/GreenTitleLoader.cs Infrastructure/GameDataCatalog/Green/GreenNeiroLoader.cs Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs Tests/Green/GreenHandlerFixture.cs Tests/Green/GreenCustomizationCatalogLoaderTests.cs
git commit -m "Load Green customization catalogs at startup"
```
