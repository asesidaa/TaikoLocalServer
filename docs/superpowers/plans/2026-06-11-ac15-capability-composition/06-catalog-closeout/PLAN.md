# AC15 Catalog Closeout Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Finish the capability-composition follow-up by simplifying AC15 catalog snapshot projection, running final verification, and updating the Phase 16.2 review artifact.

**Architecture:** Era catalog loaders remain era-owned. `Ac15CatalogSnapshotFactory` maps each era catalog to a canonical source record, then runs one snapshot assembly algorithm over that source.

**Tech Stack:** C# 13, .NET 10, Mapperly where mechanical mapping is useful, xUnit, `dotnet test`, temp-output Host build.

---

## File Structure

Create:

- `Application/Ac15/Ac15CatalogProjectionSource.cs` - canonical source record for snapshot assembly.
- `Tests/Ac15/Ac15CatalogSnapshotFactoryCompositionTests.cs` - behavior tests for the unified snapshot algorithm.

Modify:

- `Application/Ac15/Ac15CatalogSnapshotFactory.cs`
- `Tests/Ac15/Ac15CatalogReadbackServiceTests.cs`
- `.planning/phases/16.2-ac15-shared-core-simplification-and-reuse-cleanup/16.2-REVIEW.md`

## Task 1: Canonical Catalog Projection Source

**Files:**
- Create: `Application/Ac15/Ac15CatalogProjectionSource.cs`
- Modify: `Application/Ac15/Ac15CatalogSnapshotFactory.cs`
- Create: `Tests/Ac15/Ac15CatalogSnapshotFactoryCompositionTests.cs`

- [ ] **Step 1: Write catalog projection behavior tests**

Create `Tests/Ac15/Ac15CatalogSnapshotFactoryCompositionTests.cs`:

```csharp
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.ServerData;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15CatalogSnapshotFactoryCompositionTests
{
    [Fact]
    public void FromSource_PreservesMusicOrderTelopsRecommendationsShopAndTaikojuku()
    {
        var source = new Ac15CatalogProjectionSource(
            SongHashVersion: 123,
            SongNoesInFileOrder: [101, 102],
            EventFolders: new Dictionary<uint, EventFolderData>
            {
                [7] = new() { FolderId = 7, VerupNo = 20110301, Priority = 1, SongNoes = [101, 102] }
            },
            Telops: new Dictionary<uint, Ac15TelopEntry>
            {
                [4] = new()
                {
                    TelopId = 4,
                    VerupNo = 20110301,
                    StartDatetime = "20110301070000",
                    EndDatetime = "20110630020000",
                    Message = "HELLO"
                }
            },
            RecommendSong: 101,
            RecommendBestSongs: [102],
            ItemShopCatalog: new Ac15ItemShopCatalog
            {
                IsEnabled = true,
                ActiveSeasonId = 2,
                Seasons = new Dictionary<uint, Ac15ItemShopSeason>
                {
                    [2] = new()
                    {
                        SeasonId = 2,
                        Items = [new() { ItemNo = 1, ItemType = Ac15ShopItemType.Tone, ItemId = 4, Price = 100 }]
                    }
                }
            },
            TaikojukuPacks:
            [
                new Ac15TaikojukuEntry
                {
                    UniqueId = 20001,
                    ChallengeLevel = 1,
                    Songs = [new Ac15TaikojukuSong { SongNo = 101, Level = 1 }]
                }
            ]);

        var snapshot = Ac15CatalogSnapshotFactory.FromSource(source);

        Assert.Equal(123u, snapshot.SongHashVersion);
        Assert.Equal([101u, 102u], snapshot.SongNoesInFileOrder);
        Assert.Single(snapshot.EventFolders);
        Assert.True(snapshot.Telops.ContainsKey(4));
        Assert.Equal(101u, snapshot.RecommendSong);
        Assert.Equal([102u], snapshot.RecommendBestSongs);
        Assert.True(snapshot.ItemShopCatalog.IsEnabled);
        Assert.Single(snapshot.TaikojukuPacks);
    }
}
```

- [ ] **Step 2: Run catalog projection test and verify compile failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15CatalogSnapshotFactoryCompositionTests"
```

Expected: compile failure because `Ac15CatalogProjectionSource` and `Ac15CatalogSnapshotFactory.FromSource` do not exist.

- [ ] **Step 3: Add canonical source record**

Create `Application/Ac15/Ac15CatalogProjectionSource.cs`:

```csharp
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.ServerData;

namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15CatalogProjectionSource(
    uint SongHashVersion,
    IReadOnlyList<uint> SongNoesInFileOrder,
    IReadOnlyDictionary<uint, EventFolderData> EventFolders,
    IReadOnlyDictionary<uint, Ac15TelopEntry> Telops,
    uint RecommendSong,
    IReadOnlyList<uint> RecommendBestSongs,
    Ac15ItemShopCatalog ItemShopCatalog,
    IReadOnlyList<Ac15TaikojukuEntry> TaikojukuPacks);
```

- [ ] **Step 4: Add one snapshot assembly method**

In `Application/Ac15/Ac15CatalogSnapshotFactory.cs`, add:

```csharp
public static Ac15CatalogSnapshot FromSource(Ac15CatalogProjectionSource source)
    => new(
        source.SongHashVersion,
        source.SongNoesInFileOrder,
        source.EventFolders,
        source.Telops,
        source.RecommendSong,
        source.RecommendBestSongs,
        source.ItemShopCatalog,
        source.TaikojukuPacks);
```

Then make `FromBlue`, `FromGreen`, and `FromYellow` thin source mappers:

```csharp
public static Ac15CatalogSnapshot FromBlue(IBlueCatalog blue)
    => FromSource(new Ac15CatalogProjectionSource(
        blue.SongHashVersion,
        blue.MusicInfoFileOrder.Select(song => song.SongNo).ToArray(),
        blue.EventFolders,
        MapTelops(blue.Telops, entry => entry.TelopId, entry => entry.VerupNo, entry => entry.StartDatetime, entry => entry.EndDatetime, entry => entry.Message),
        blue.Recommend.RecommendSong,
        blue.Recommend.RecommendBestSongs.ToArray(),
        MapItemShop(blue.ItemShopCatalog),
        blue.TaikojukuFileOrder.Select(MapTaikojuku).ToArray()));
```

Apply the same pattern for Green and Yellow, using their existing catalog properties and existing `MapTelops`, `MapItemShop`, and `MapTaikojuku` helpers.

- [ ] **Step 5: Run catalog tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15CatalogSnapshotFactoryCompositionTests|FullyQualifiedName~Ac15CatalogReadbackServiceTests|FullyQualifiedName~Ac15InitialDataServiceTests|FullyQualifiedName~Ac15CatalogLoaderTests"
```

Expected: PASS.

## Task 2: Final Verification And Review Artifact

**Files:**
- Modify: `.planning/phases/16.2-ac15-shared-core-simplification-and-reuse-cleanup/16.2-REVIEW.md`

- [ ] **Step 1: Run focused capability-composition slices**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Ac15"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultHandlerTests|FullyQualifiedName~BlueBattlePlayResultHandlerTests|FullyQualifiedName~BlueItemShopPurchaseTests|FullyQualifiedName~BlueAdminApiParityTests"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayResultHandlerTests|FullyQualifiedName~GreenAiBattlePlayResultTests|FullyQualifiedName~GreenItemShopPurchaseTests|FullyQualifiedName~GreenAdminApiControllerTests"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowPlayResultHandlerTests|FullyQualifiedName~YellowDaniTests|FullyQualifiedName~YellowItemShopPurchaseTests|FullyQualifiedName~YellowAdminApiTests"
```

Expected: PASS for all four commands.

- [ ] **Step 2: Run full server tests**

Run:

```powershell
dotnet test Tests/Tests.csproj
```

Expected: PASS.

- [ ] **Step 3: Run temp-output Host build**

Run:

```powershell
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
```

Expected: build exits `0`.

- [ ] **Step 4: Update the Phase 16.2 review artifact**

Open `.planning/phases/16.2-ac15-shared-core-simplification-and-reuse-cleanup/16.2-REVIEW.md` and add this section near the existing Phase 16.2 review follow-up status:

```markdown
## 2026-06-11 AC15 Capability Composition Follow-Up

Status: completed

Implemented follow-up design: `docs/superpowers/specs/2026-06-11-ac15-capability-composition-design.md`

Resolution summary:

- Green compact AC15 play timestamps now persist consistently across save, play, and recent rows.
- Green unsupported normal stages are logged and skipped with protocol success; mixed payloads still persist valid stages.
- Normal play, Dani, item-shop purchase, AdminApi user settings, and catalog projection now compose shared behavior from handler-bound capabilities instead of shared modules choosing tables by `GameEra`.
- Blue battle, Blue Tokkun, Yellow Tokkun, Yellow WaiWai logging, and Green ghost/AI behavior remain explicit era behavior.
- Blue, Green, and Yellow EF tables remain physically separate.

Verification:

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Ac15"` passed.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultHandlerTests|FullyQualifiedName~BlueBattlePlayResultHandlerTests|FullyQualifiedName~BlueItemShopPurchaseTests|FullyQualifiedName~BlueAdminApiParityTests"` passed.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayResultHandlerTests|FullyQualifiedName~GreenAiBattlePlayResultTests|FullyQualifiedName~GreenItemShopPurchaseTests|FullyQualifiedName~GreenAdminApiControllerTests"` passed.
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowPlayResultHandlerTests|FullyQualifiedName~YellowDaniTests|FullyQualifiedName~YellowItemShopPurchaseTests|FullyQualifiedName~YellowAdminApiTests"` passed.
- `dotnet test Tests/Tests.csproj` passed.
- `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` passed.
```

If a command fails during execution, record the actual failing command and fix in this section instead of marking it passed.

- [ ] **Step 5: Run whitespace check for touched docs and code**

Run:

```powershell
git diff --check -- Application Domain Adapters.AdminApi Tests .planning/phases/16.2-ac15-shared-core-simplification-and-reuse-cleanup/16.2-REVIEW.md
```

Expected: no output and exit `0`.

- [ ] **Step 6: Commit stage 6**

Run:

```powershell
git add Application/Ac15/Ac15CatalogProjectionSource.cs Application/Ac15/Ac15CatalogSnapshotFactory.cs Tests/Ac15/Ac15CatalogSnapshotFactoryCompositionTests.cs Tests/Ac15/Ac15CatalogReadbackServiceTests.cs .planning/phases/16.2-ac15-shared-core-simplification-and-reuse-cleanup/16.2-REVIEW.md
git commit -m "Close AC15 capability composition follow-up"
```

## Self-Review

- Spec coverage: catalog projection and review artifact update are covered after runtime behavior is stable.
- Non-goals honored: era catalog loaders and source data directories stay era-owned; no unsupported routes or shared EF tables are added.
- Type consistency: `Ac15CatalogProjectionSource` and `Ac15CatalogSnapshotFactory.FromSource` are the only new catalog projection names.
