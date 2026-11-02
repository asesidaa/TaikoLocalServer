# 02 - Shop Data Loader

**Goal:** Parse `green_item_shop_data.json`, validate required protocol fields, infer `item_no`, and select the active season from server settings.

**Files:**

- Modify: `Infrastructure/GameDataCatalog/Green/GreenItemShopLoader.cs`
- Modify: `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`
- Create: `Tests/Green/GreenItemShopLoaderTests.cs`

## Acceptance Criteria

- [ ] Missing shop file returns disabled catalog when `EnableShop=false`.
- [ ] Missing file, missing active season id, and unknown active season fail when `EnableShop=true`.
- [ ] Loader validates dates, item types, prices, row cap, duplicate seasons, and duplicate item identities.
- [ ] Loader infers 1-based `item_no` from item array order.

## Steps

- [ ] **Step 1: Add loader tests**

Create `Tests/Green/GreenItemShopLoaderTests.cs`:

```csharp
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenItemShopLoaderTests
{
    [Fact]
    public async Task LoadFromFile_DisabledMissingFileReturnsDisabledCatalog()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        var settings = new EraSettings { EnableShop = false };

        var catalog = await GreenItemShopLoader.LoadFromFileAsync(path, settings, CancellationToken.None);

        Assert.False(catalog.IsEnabled);
        Assert.Null(catalog.ActiveSeason);
    }

    [Fact]
    public async Task LoadFromFile_InfersItemNoAndSelectsActiveSeason()
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, """
                {
                  "seasons": [
                    {
                      "season_id": 2,
                      "verup_no": 9,
                      "telop": "Shop",
                      "start_datetime": "20190314000000",
                      "end_datetime": "20190626075959",
                      "afterstart_days": 3,
                      "beforeclose_days": 4,
                      "items": [
                        { "item_type": 4, "item_id": 117, "item_price": 500 },
                        { "item_type": 1, "item_id": 865, "item_price": 1300 }
                      ]
                    }
                  ]
                }
                """);

            var catalog = await GreenItemShopLoader.LoadFromFileAsync(
                path,
                new EraSettings { EnableShop = true, ActiveShopSeasonId = 2 },
                CancellationToken.None);

            Assert.True(catalog.IsEnabled);
            Assert.Equal(2u, catalog.ActiveSeason!.SeasonId);
            Assert.Equal(9u, catalog.ActiveSeason.VerupNo);
            Assert.Equal(1u, catalog.ActiveSeason.Items[0].ItemNo);
            Assert.Equal(2u, catalog.ActiveSeason.Items[1].ItemNo);
            Assert.Equal(865u, catalog.ActiveItemsByNo[2].ItemId);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Theory]
    [InlineData(null, "active")]
    [InlineData(99u, "active")]
    public async Task LoadFromFile_EnabledRequiresKnownActiveSeason(uint? activeSeasonId, string failureText)
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, """
                {
                  "seasons": [
                    {
                      "season_id": 1,
                      "verup_no": 1,
                      "telop": "Shop",
                      "start_datetime": "20190314000000",
                      "end_datetime": "20190626075959",
                      "afterstart_days": 0,
                      "beforeclose_days": 0,
                      "items": [
                        { "item_type": 1, "item_id": 865, "item_price": 1300 }
                      ]
                    }
                  ]
                }
                """);

            var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
                GreenItemShopLoader.LoadFromFileAsync(
                    path,
                    new EraSettings { EnableShop = true, ActiveShopSeasonId = activeSeasonId },
                    CancellationToken.None));

            Assert.Contains(failureText, ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Theory]
    [InlineData("\"start_datetime\": \"2019-03-14\"", "datetime")]
    [InlineData("\"items\": []", "item")]
    public async Task LoadFromFile_RejectsInvalidSeasonData(string replacement, string failureText)
    {
        var json = """
            {
              "seasons": [
                {
                  "season_id": 1,
                  "verup_no": 1,
                  "telop": "Shop",
                  "start_datetime": "20190314000000",
                  "end_datetime": "20190626075959",
                  "afterstart_days": 0,
                  "beforeclose_days": 0,
                  "items": [
                    { "item_type": 1, "item_id": 865, "item_price": 1300 }
                  ]
                }
              ]
            }
            """;

        json = replacement.StartsWith("\"start_datetime\"", StringComparison.Ordinal)
            ? json.Replace("\"start_datetime\": \"20190314000000\"", replacement, StringComparison.Ordinal)
            : json.Replace("""
                  "items": [
                    { "item_type": 1, "item_id": 865, "item_price": 1300 }
                  ]
                """, replacement, StringComparison.Ordinal);

        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, json);

            var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
                GreenItemShopLoader.LoadFromFileAsync(
                    path,
                    new EraSettings { EnableShop = true, ActiveShopSeasonId = 1 },
                    CancellationToken.None));

            Assert.Contains(failureText, ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
```

- [ ] **Step 2: Run tests and verify failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenItemShopLoaderTests"
```

Expected: fails because `LoadFromFileAsync` does not exist or returns disabled data only.

- [ ] **Step 3: Implement loader**

Replace `Infrastructure/GameDataCatalog/Green/GreenItemShopLoader.cs` with:

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

internal sealed class GreenItemShopLoader
{
    public const string FileName = "green_item_shop_data.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public Task<GreenItemShopCatalog> LoadAsync(
        EraSettings greenSettings,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Green), FileName);
        return LoadFromFileAsync(path, greenSettings, cancellationToken);
    }

    public static async Task<GreenItemShopCatalog> LoadFromFileAsync(
        string path,
        EraSettings greenSettings,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!greenSettings.EnableShop)
        {
            return GreenItemShopCatalog.Disabled;
        }

        if (!File.Exists(path))
        {
            throw new InvalidDataException($"Green item shop is enabled but data file was not found: {path}");
        }

        if (greenSettings.ActiveShopSeasonId is not { } activeSeasonId)
        {
            throw new InvalidDataException("Green item shop is enabled but ActiveShopSeasonId is not configured.");
        }

        await using var stream = File.OpenRead(path);
        var raw = await JsonSerializer.DeserializeAsync<RawShopData>(stream, JsonOptions, cancellationToken)
                  ?? new RawShopData();

        var seasons = (raw.Seasons ?? [])
            .Select(MapSeason)
            .ToList();

        if (seasons.Count == 0)
        {
            throw new InvalidDataException("Green item shop data must contain at least one season when enabled.");
        }

        var duplicates = seasons
            .GroupBy(season => season.SeasonId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        if (duplicates.Length > 0)
        {
            throw new InvalidDataException($"Green item shop data contains duplicate season_id values: {string.Join(", ", duplicates)}");
        }

        var bySeason = seasons.ToDictionary(season => season.SeasonId);
        if (!bySeason.ContainsKey(activeSeasonId))
        {
            throw new InvalidDataException($"Green item shop active season {activeSeasonId} was not found.");
        }

        return new GreenItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = activeSeasonId,
            Seasons = bySeason
        };
    }

    private static GreenItemShopSeason MapSeason(RawSeason raw)
    {
        if (raw.SeasonId == 0)
        {
            throw new InvalidDataException("Green item shop season_id must be nonzero.");
        }

        if (!IsDate(raw.StartDatetime) || !IsDate(raw.EndDatetime))
        {
            throw new InvalidDataException($"Green item shop season {raw.SeasonId} has invalid datetime fields.");
        }

        var rawItems = raw.Items ?? [];
        if (rawItems.Length == 0)
        {
            throw new InvalidDataException($"Green item shop season {raw.SeasonId} must contain at least one item.");
        }

        if (rawItems.Length > 64)
        {
            throw new InvalidDataException($"Green item shop season {raw.SeasonId} has {rawItems.Length} items; the client supports at most 64.");
        }

        var items = rawItems
            .Select((item, index) => MapItem(raw.SeasonId, item, (uint)index + 1))
            .ToArray();

        var duplicateItems = items
            .GroupBy(item => new { item.ItemType, item.ItemId })
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key.ItemType}:{group.Key.ItemId}")
            .ToArray();
        if (duplicateItems.Length > 0)
        {
            throw new InvalidDataException($"Green item shop season {raw.SeasonId} contains duplicate item identities: {string.Join(", ", duplicateItems)}");
        }

        return new GreenItemShopSeason
        {
            SeasonId = raw.SeasonId,
            VerupNo = raw.VerupNo,
            Telop = raw.Telop ?? string.Empty,
            StartDatetime = raw.StartDatetime ?? string.Empty,
            EndDatetime = raw.EndDatetime ?? string.Empty,
            AfterstartDays = raw.AfterstartDays,
            BeforecloseDays = raw.BeforecloseDays,
            Items = items
        };
    }

    private static GreenItemShopEntry MapItem(uint seasonId, RawItem raw, uint itemNo)
    {
        if (raw.ItemType is < 1 or > 7)
        {
            throw new InvalidDataException($"Green item shop season {seasonId} item {itemNo} has unsupported item_type {raw.ItemType}.");
        }

        if (raw.ItemId == 0)
        {
            throw new InvalidDataException($"Green item shop season {seasonId} item {itemNo} has item_id 0.");
        }

        if (raw.ItemPrice == 0)
        {
            throw new InvalidDataException($"Green item shop season {seasonId} item {itemNo} has item_price 0.");
        }

        return new GreenItemShopEntry
        {
            ItemNo = itemNo,
            ItemType = raw.ItemType,
            ItemId = raw.ItemId,
            Price = raw.ItemPrice
        };
    }

    private static bool IsDate(string? value)
        => value is { Length: 14 } && value.All(char.IsAsciiDigit);

    private sealed class RawShopData
    {
        [JsonPropertyName("seasons")]
        public RawSeason[]? Seasons { get; set; }
    }

    private sealed class RawSeason
    {
        [JsonPropertyName("season_id")]
        public uint SeasonId { get; set; }

        [JsonPropertyName("verup_no")]
        public uint VerupNo { get; set; }

        [JsonPropertyName("telop")]
        public string? Telop { get; set; }

        [JsonPropertyName("start_datetime")]
        public string? StartDatetime { get; set; }

        [JsonPropertyName("end_datetime")]
        public string? EndDatetime { get; set; }

        [JsonPropertyName("afterstart_days")]
        public uint AfterstartDays { get; set; }

        [JsonPropertyName("beforeclose_days")]
        public uint BeforecloseDays { get; set; }

        [JsonPropertyName("items")]
        public RawItem[]? Items { get; set; }
    }

    private sealed class RawItem
    {
        [JsonPropertyName("item_type")]
        public uint ItemType { get; set; }

        [JsonPropertyName("item_id")]
        public uint ItemId { get; set; }

        [JsonPropertyName("item_price")]
        public uint ItemPrice { get; set; }
    }
}
```

- [ ] **Step 4: Wire loader into Green catalog**

In `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`, replace the temporary disabled assignment from Stage 01 with:

```csharp
var greenSettings = GetGreenSettings();
itemShopCatalog = await new GreenItemShopLoader().LoadAsync(greenSettings, cancellationToken);
itemShop = itemShopCatalog.ActiveItemsByNo;
```

Use the existing `GetGreenSettings()` method. Do not add another settings lookup helper.

- [ ] **Step 5: Run loader tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenItemShopLoaderTests"
```

Expected: pass.

- [ ] **Step 6: Run Green tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Green"
```

Expected: pass or only unrelated pre-existing failures.

- [ ] **Step 7: Commit**

```powershell
git status --short
git add -- Infrastructure/GameDataCatalog/Green/GreenItemShopLoader.cs Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs Tests/Green/GreenItemShopLoaderTests.cs
git commit -m "Load Green item shop seasons from JSON"
```

