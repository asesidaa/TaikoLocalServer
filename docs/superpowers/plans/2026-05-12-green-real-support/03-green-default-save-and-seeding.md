# 03 - Green Default Save And Seeding

**Surface:** Add Application helpers for default Green save data, fake score/crown seeds, fixed byte defaults, and first-Dan grant behavior.

**Why after 02:** Fake score/crown seeds need parsed Green song order and `hasextreme` metadata.

**Files:**
- Create: `Application/Common/UserSaveDataGreenExtensions.cs`
- Create: `Application/Common/GreenSeedDataService.cs`
- Create: `Tests/Green/GreenSaveDataTests.cs`

---

## Task 03.1: Create Default Green Save Data Helper

**Acceptance Criteria:**
- [ ] New helper creates fixed-width default byte arrays.
- [ ] `GetOrCreateGreenSaveDataAsync` mirrors the Nijiiro helper pattern.

**Steps:**

- [ ] **Step 1: Add tests**

Create `Tests/Green/GreenSaveDataTests.cs`:

```csharp
using TaikoLocalServer.Application.Common;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenSaveDataTests
{
    [Fact]
    public void CreateDefaultGreenSaveData_InitializesFixedWidthBytes()
    {
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(123);

        Assert.Equal((uint)123, save.Baid);
        Assert.Equal(GreenProtocolBytes.CostumeFlagBytes, save.CostumeFlg1.Length);
        Assert.Equal(GreenProtocolBytes.CostumeFlagBytes, save.CostumeFlg2.Length);
        Assert.Equal(GreenProtocolBytes.CostumeFlagBytes, save.CostumeFlg3.Length);
        Assert.Equal(GreenProtocolBytes.CostumeFlagBytes, save.CostumeFlg4.Length);
        Assert.Equal(GreenProtocolBytes.CostumeFlagBytes, save.CostumeFlg5.Length);
        Assert.Equal(GreenProtocolBytes.ToneFlagBytes, save.ToneFlg.Length);
        Assert.Equal(GreenProtocolBytes.TitleFlagBytes, save.TitleFlg.Length);
        Assert.Equal(2, save.DefaultOptionSetting.Length);
        Assert.Equal(GreenProtocolBytes.DanFlagBytes, save.GotDanFlg.Length);
        Assert.Equal(GreenProtocolBytes.DanExtraFlagBytes, save.GotDanExtraFlg.Length);
        Assert.Equal(GreenProtocolBytes.GhostReleaseInfoBytes, save.GhostReleaseInfoFlag.Length);
        Assert.Equal(GreenProtocolBytes.GhostPlayedSongBytes, save.GhostPlayedSongFlag.Length);
    }
}
```

- [ ] **Step 2: Run focused test and confirm failure**

Run: `dotnet test --filter CreateDefaultGreenSaveData_InitializesFixedWidthBytes`

Expected: FAIL because `UserSaveDataGreenExtensions` does not exist.

- [ ] **Step 3: Create `Application/Common/UserSaveDataGreenExtensions.cs`**

```csharp
namespace TaikoLocalServer.Application.Common;

public static class UserSaveDataGreenExtensions
{
    public static async ValueTask<UserSaveDataGreen> GetOrCreateGreenSaveDataAsync(
        this ITaikoDbContext context,
        uint baid,
        CancellationToken cancellationToken = default)
    {
        var saveData = await context.UserSaveDataGreen.FindAsync([baid], cancellationToken);
        if (saveData is not null)
        {
            return saveData;
        }

        saveData = CreateDefaultGreenSaveData(baid);
        context.UserSaveDataGreen.Add(saveData);
        return saveData;
    }

    public static UserSaveDataGreen CreateDefaultGreenSaveData(uint baid) => new()
    {
        Baid = baid,
        Title = string.Empty,
        TitleplateId = 0,
        ColorFace = 0,
        ColorBody = 1,
        ColorLimb = 3,
        Costume1 = 0,
        Costume2 = 0,
        Costume3 = 0,
        Costume4 = 0,
        Costume5 = 0,
        CostumeFlg1 = GreenProtocolBytes.CreateFixedBitset([0], GreenProtocolBytes.CostumeFlagBytes),
        CostumeFlg2 = GreenProtocolBytes.CreateFixedBitset([0], GreenProtocolBytes.CostumeFlagBytes),
        CostumeFlg3 = GreenProtocolBytes.CreateFixedBitset([0], GreenProtocolBytes.CostumeFlagBytes),
        CostumeFlg4 = GreenProtocolBytes.CreateFixedBitset([0], GreenProtocolBytes.CostumeFlagBytes),
        CostumeFlg5 = GreenProtocolBytes.CreateFixedBitset([0], GreenProtocolBytes.CostumeFlagBytes),
        ToneFlg = GreenProtocolBytes.CreateFixedBitset([0], GreenProtocolBytes.ToneFlagBytes),
        TitleFlg = new byte[GreenProtocolBytes.TitleFlagBytes],
        OptionFlg = [],
        DefaultOptionSetting = new byte[2],
        DefaultShinSetting = false,
        DefaultToneSetting = 0,
        DispDanType = 0,
        GotDanMax = 0,
        GotDanFlg = new byte[GreenProtocolBytes.DanFlagBytes],
        GotDanExtraFlg = new byte[GreenProtocolBytes.DanExtraFlagBytes],
        DispTaikojukuDan = 0,
        LastPlayDatetime = DateTime.UnixEpoch,
        GhostReleaseInfoFlag = new byte[GreenProtocolBytes.GhostReleaseInfoBytes],
        GhostPlayedSongFlag = new byte[GreenProtocolBytes.GhostPlayedSongBytes],
        IsAutoCostumeOn = false,
        IsDevil = false,
        IsExplain = false,
        IsChallengeCompe = false,
        IsTojiru = false
    };
}
```

- [ ] **Step 4: Run focused test**

Run: `dotnet test --filter CreateDefaultGreenSaveData_InitializesFixedWidthBytes`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Application/Common/UserSaveDataGreenExtensions.cs Tests/Green/GreenSaveDataTests.cs
git commit -m "feat(green): add default Green save data helper"
```

---

## Task 03.2: Add Fake Score/Crown Seed Service

**Acceptance Criteria:**
- [ ] Seed service creates deterministic `SongBestDatumGreen` rows from catalog file order.
- [ ] The fifth seed uses Ura only when a parsed song has `HasExtreme = true`.

**Steps:**

- [ ] **Step 1: Add tests**

Append to `GreenSaveDataTests`:

```csharp
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Domain.Entities;

[Fact]
public void CreateFakeBestSeeds_UsesCatalogOrder()
{
    GreenMusicInfoEntry[] songs =
    [
        new() { SongNo = 101, MusicId = "a", FileOrder = 0 },
        new() { SongNo = 102, MusicId = "b", FileOrder = 1 },
        new() { SongNo = 103, MusicId = "c", FileOrder = 2 },
        new() { SongNo = 104, MusicId = "d", FileOrder = 3 },
        new() { SongNo = 105, MusicId = "e", FileOrder = 4, HasExtreme = true }
    ];

    var seeds = GreenSeedDataService.CreateFakeBestSeeds(55, songs).ToArray();

    Assert.Equal(5, seeds.Length);
    Assert.Equal((uint)55, seeds[0].Baid);
    Assert.Equal((uint)101, seeds[0].SongId);
    Assert.Equal(Difficulty.Easy, seeds[0].Difficulty);
    Assert.Equal(CrownType.Clear, seeds[0].BestCrown);
    Assert.Equal(Difficulty.UraOni, seeds[4].Difficulty);
}
```

- [ ] **Step 2: Run focused test and confirm failure**

Run: `dotnet test --filter CreateFakeBestSeeds_UsesCatalogOrder`

Expected: FAIL because `GreenSeedDataService` does not exist.

- [ ] **Step 3: Create `Application/Common/GreenSeedDataService.cs`**

```csharp
using TaikoLocalServer.Application.Catalog.Green;

namespace TaikoLocalServer.Application.Common;

public static class GreenSeedDataService
{
    public static IReadOnlyList<SongBestDatumGreen> CreateFakeBestSeeds(
        uint baid,
        IReadOnlyList<GreenMusicInfoEntry> musicInfoFileOrder)
    {
        var seeds = new List<SongBestDatumGreen>();

        AddSeed(seeds, baid, musicInfoFileOrder.ElementAtOrDefault(0), Difficulty.Easy, 123450, CrownType.Clear);
        AddSeed(seeds, baid, musicInfoFileOrder.ElementAtOrDefault(1), Difficulty.Normal, 234560, CrownType.Gold);
        AddSeed(seeds, baid, musicInfoFileOrder.ElementAtOrDefault(2), Difficulty.Hard, 345670, CrownType.Dondaful);
        AddSeed(seeds, baid, musicInfoFileOrder.ElementAtOrDefault(3), Difficulty.Oni, 456780, CrownType.Clear);

        var uraSong = musicInfoFileOrder.FirstOrDefault(song => song.HasExtreme);
        AddSeed(seeds, baid, uraSong, Difficulty.UraOni, 567890, CrownType.Gold);

        return seeds;
    }

    private static void AddSeed(
        List<SongBestDatumGreen> seeds,
        uint baid,
        GreenMusicInfoEntry? song,
        Difficulty difficulty,
        uint score,
        CrownType crown)
    {
        if (song is null)
        {
            return;
        }

        seeds.Add(new SongBestDatumGreen
        {
            Baid = baid,
            SongId = song.SongNo,
            Difficulty = difficulty,
            BestScore = score,
            BestRate = 0,
            BestCrown = crown
        });
    }
}
```

- [ ] **Step 4: Run focused test**

Run: `dotnet test --filter CreateFakeBestSeeds_UsesCatalogOrder`

Expected: PASS.

---

## Task 03.3: Add First-Dan Grant Helper

**Acceptance Criteria:**
- [ ] First fake Dan is granted only once.
- [ ] The helper updates `GotDanFlg`, `GotDanMax`, and `DispTaikojukuDan`.

**Steps:**

- [ ] **Step 1: Add tests**

Append to `GreenSaveDataTests`:

```csharp
[Fact]
public void GrantFirstFakeDanIfNeeded_GrantsOnlyOnce()
{
    var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);

    var first = GreenSeedDataService.GrantFirstFakeDanIfNeeded(save);
    var second = GreenSeedDataService.GrantFirstFakeDanIfNeeded(save);

    Assert.True(first);
    Assert.False(second);
    Assert.Equal((uint)1, save.GotDanMax);
    Assert.Equal((uint)1, save.DispTaikojukuDan);
    Assert.Equal(0b0000_0001, save.GotDanFlg[0]);
}
```

- [ ] **Step 2: Run focused test and confirm failure**

Run: `dotnet test --filter GrantFirstFakeDanIfNeeded_GrantsOnlyOnce`

Expected: FAIL because the method does not exist.

- [ ] **Step 3: Add method to `GreenSeedDataService`**

```csharp
public static bool GrantFirstFakeDanIfNeeded(UserSaveDataGreen saveData)
{
    saveData.GotDanFlg = GreenProtocolBytes.FixedOrZero(saveData.GotDanFlg, GreenProtocolBytes.DanFlagBytes);

    if ((saveData.GotDanFlg[0] & 0b11) != 0)
    {
        return false;
    }

    GreenProtocolBytes.SetTwoBitValue(saveData.GotDanFlg, 0, 1);
    saveData.GotDanMax = Math.Max(saveData.GotDanMax, 1);
    saveData.DispTaikojukuDan = Math.Max(saveData.DispTaikojukuDan, 1);
    return true;
}
```

- [ ] **Step 4: Run tests and build**

Run:

```bash
dotnet test --filter GreenSaveDataTests
dotnet build
```

Expected: both PASS.

- [ ] **Step 5: Commit**

```bash
git add Application/Common/GreenSeedDataService.cs Tests/Green/GreenSaveDataTests.cs
git commit -m "feat(green): add fake Green score and dan seed helpers"
```
