# 01 - Settings And Catalog Contract

**Goal:** Add the Green shop settings and in-memory catalog types without changing runtime behavior yet.

**Files:**

- Modify: `Application/Settings/ServerSettings.cs`
- Modify: `Application/Catalog/Green/GreenItemShopEntry.cs`
- Create: `Application/Catalog/Green/GreenItemShopSeason.cs`
- Create: `Application/Catalog/Green/GreenItemShopCatalog.cs`
- Modify: `Application/Abstractions/IGreenCatalog.cs`
- Modify: `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`
- Modify: `Tests/Green/GreenHandlerFixture.cs`
- Create: `Tests/Green/GreenItemShopCatalogTests.cs`

## Acceptance Criteria

- [ ] `EraSettings` has `EnableShop` and nullable `ActiveShopSeasonId`.
- [ ] Shop catalog types represent all seasons, active season, enabled state, and inferred row numbers.
- [ ] Existing code can still use `IGreenCatalog.ItemShop` as the active item lookup by inferred `item_no`.
- [ ] Tests prove `item_no` lookup is row-order based.

## Steps

- [ ] **Step 1: Add failing catalog tests**

Create `Tests/Green/GreenItemShopCatalogTests.cs`:

```csharp
namespace TaikoLocalServer.Tests.Green;

public sealed class GreenItemShopCatalogTests
{
    [Fact]
    public void ActiveSeason_ItemsByNoUseInferredItemNo()
    {
        var season = new GreenItemShopSeason
        {
            SeasonId = 1,
            VerupNo = 7,
            Telop = "Shop",
            StartDatetime = "20190314000000",
            EndDatetime = "20190626075959",
            AfterstartDays = 3,
            BeforecloseDays = 4,
            Items =
            [
                new GreenItemShopEntry { ItemNo = 1, ItemType = 4, ItemId = 117, Price = 500 },
                new GreenItemShopEntry { ItemNo = 2, ItemType = 1, ItemId = 865, Price = 1300 }
            ]
        };

        var catalog = new GreenItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 1,
            Seasons = new Dictionary<uint, GreenItemShopSeason>
            {
                [1] = season
            }
        };

        Assert.Same(season, catalog.ActiveSeason);
        Assert.Equal(865u, catalog.ActiveItemsByNo[2].ItemId);
    }

    [Fact]
    public void DisabledCatalog_HasNoActiveSeasonOrItems()
    {
        var catalog = GreenItemShopCatalog.Disabled;

        Assert.False(catalog.IsEnabled);
        Assert.Null(catalog.ActiveSeason);
        Assert.Empty(catalog.ActiveItemsByNo);
    }
}
```

- [ ] **Step 2: Run tests and verify failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenItemShopCatalogTests"
```

Expected: fails because `GreenItemShopSeason`, `GreenItemShopCatalog`, and `GreenItemShopEntry.ItemNo` do not exist.

- [ ] **Step 3: Add settings**

Modify `Application/Settings/ServerSettings.cs`:

```csharp
public sealed class EraSettings
{
    public bool Enabled { get; set; }

    public bool AutoExtractCatalog { get; set; } = true;

    public string GameDataPath { get; set; } = "wwwroot/data/green/data";

    public string? CustomizationNameDataPath { get; set; }

    public bool EnableShop { get; set; }

    public uint? ActiveShopSeasonId { get; set; }
}
```

- [ ] **Step 4: Update item entry**

Modify `Application/Catalog/Green/GreenItemShopEntry.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Green;

public sealed class GreenItemShopEntry
{
    public uint ItemNo { get; init; }

    public uint ItemId { get; init; }

    public uint ItemType { get; init; }

    public uint Price { get; init; }
}
```

- [ ] **Step 5: Add season model**

Create `Application/Catalog/Green/GreenItemShopSeason.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Green;

public sealed class GreenItemShopSeason
{
    public uint SeasonId { get; init; }

    public uint VerupNo { get; init; }

    public string Telop { get; init; } = string.Empty;

    public string StartDatetime { get; init; } = string.Empty;

    public string EndDatetime { get; init; } = string.Empty;

    public uint AfterstartDays { get; init; }

    public uint BeforecloseDays { get; init; }

    public IReadOnlyList<GreenItemShopEntry> Items { get; init; } = [];

    public IReadOnlyDictionary<uint, GreenItemShopEntry> ItemsByNo
        => Items.ToDictionary(item => item.ItemNo);
}
```

- [ ] **Step 6: Add catalog aggregate**

Create `Application/Catalog/Green/GreenItemShopCatalog.cs`:

```csharp
namespace TaikoLocalServer.Application.Catalog.Green;

public sealed class GreenItemShopCatalog
{
    public static GreenItemShopCatalog Disabled { get; } = new();

    public bool IsEnabled { get; init; }

    public uint? ActiveSeasonId { get; init; }

    public IReadOnlyDictionary<uint, GreenItemShopSeason> Seasons { get; init; }
        = new Dictionary<uint, GreenItemShopSeason>();

    public GreenItemShopSeason? ActiveSeason
        => ActiveSeasonId is { } id && Seasons.TryGetValue(id, out var season)
            ? season
            : null;

    public IReadOnlyDictionary<uint, GreenItemShopEntry> ActiveItemsByNo
        => ActiveSeason?.ItemsByNo ?? new Dictionary<uint, GreenItemShopEntry>();
}
```

- [ ] **Step 7: Expose catalog from `IGreenCatalog`**

Modify `Application/Abstractions/IGreenCatalog.cs`:

```csharp
GreenItemShopCatalog ItemShopCatalog { get; }
```

Keep the existing `ItemShop` property for active item lookup by `item_no`.

- [ ] **Step 8: Wire defaults in Green catalog**

In `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`, add a field and property:

```csharp
private GreenItemShopCatalog itemShopCatalog = GreenItemShopCatalog.Disabled;

public GreenItemShopCatalog ItemShopCatalog => itemShopCatalog;
```

For this stage only, keep initialization disabled:

```csharp
itemShopCatalog = GreenItemShopCatalog.Disabled;
itemShop = itemShopCatalog.ActiveItemsByNo;
```

Replace the old `itemShop = await new GreenItemShopLoader().LoadAsync(cancellationToken);` line with the temporary disabled assignment above. The real loader is added in Stage 02.

- [ ] **Step 9: Update test fixture**

Modify `Tests/Green/GreenHandlerFixture.cs` so `TestGreenCatalog` exposes the new property:

```csharp
public TestGreenCatalog(
    IReadOnlyDictionary<uint, GreenItemShopEntry>? itemShop = null,
    GreenItemShopCatalog? itemShopCatalog = null,
    IReadOnlyList<GreenMusicInfoEntry>? musicInfoFileOrder = null,
    IReadOnlyList<GreenTaikojukuEntry>? taikojukuFileOrder = null)
{
    ItemShopCatalog = itemShopCatalog ?? GreenItemShopCatalog.Disabled;
    ItemShop = itemShop ?? ItemShopCatalog.ActiveItemsByNo;
    this.musicInfoFileOrder = musicInfoFileOrder ?? DefaultMusicInfoFileOrder;
    this.taikojukuFileOrder = taikojukuFileOrder ?? DefaultTaikojukuFileOrder;
}

public GreenItemShopCatalog ItemShopCatalog { get; }
```

- [ ] **Step 10: Run focused tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenItemShopCatalogTests"
```

Expected: pass.

- [ ] **Step 11: Run Green compile smoke**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Green"
```

Expected: pass or only unrelated pre-existing failures. Investigate compile failures from constructor/interface changes.

- [ ] **Step 12: Commit**

```powershell
git status --short
git add -- Application/Settings/ServerSettings.cs Application/Catalog/Green/GreenItemShopEntry.cs Application/Catalog/Green/GreenItemShopSeason.cs Application/Catalog/Green/GreenItemShopCatalog.cs Application/Abstractions/IGreenCatalog.cs Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs Tests/Green/GreenHandlerFixture.cs Tests/Green/GreenItemShopCatalogTests.cs
git commit -m "Add Green item shop catalog contract"
```

