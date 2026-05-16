# Task 3: GreenProfileCounters Helper

**Goal:** Pure static helper that increments the matching `Categ*Cnt` for `music_categ` 0..7 and bumps `Song(Pushed|Favorite|Recent)Cnt` per stage. Saturates at `uint.MaxValue`.

**Files:**
- Create: `Application/Common/GreenProfileCounters.cs`
- Create: `Tests/Green/GreenProfileCountersTests.cs`

**Acceptance Criteria:**
- [ ] `GreenProfileCounters.ApplyStage(saveData, stage)` increments the correct `CategXxxCnt` for each `music_categ` 0..7.
- [ ] `music_categ` outside 0..7 is a no-op (defensive — upstream validation rejects it).
- [ ] `IsPushed=true` increments `SongPushedCnt`; `IsFavorite=true` increments `SongFavoriteCnt`; `IsRecent=true` increments `SongRecentCnt`.
- [ ] Counter at `uint.MaxValue` does not wrap or throw.
- [ ] Genre→counter mapping matches `Domain.Enums.SongGenre` ordering (Pop=0→Jpop, Anime=1, Kids=2→Doyo, Vocaloid=3, GameMusic=4, NamcoOriginal=5, Variety=6, Classical=7).

**Verify:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenProfileCountersTests"` → all pass.

---

- [ ] **Step 1: Write the failing helper tests**

Create `Tests/Green/GreenProfileCountersTests.cs`:

```csharp
namespace TaikoLocalServer.Tests.Green;

public sealed class GreenProfileCountersTests
{
    [Theory]
    [InlineData(0u, nameof(UserSaveDataGreen.CategJpopCnt))]
    [InlineData(1u, nameof(UserSaveDataGreen.CategAnimeCnt))]
    [InlineData(2u, nameof(UserSaveDataGreen.CategDoyoCnt))]
    [InlineData(3u, nameof(UserSaveDataGreen.CategVocaloidCnt))]
    [InlineData(4u, nameof(UserSaveDataGreen.CategGameCnt))]
    [InlineData(5u, nameof(UserSaveDataGreen.CategNamcoCnt))]
    [InlineData(6u, nameof(UserSaveDataGreen.CategVarietyCnt))]
    [InlineData(7u, nameof(UserSaveDataGreen.CategClassicCnt))]
    public void ApplyStage_IncrementsMatchingGenreCounter(uint musicCateg, string expectedProperty)
    {
        var save = new UserSaveDataGreen { Baid = 1 };
        var stage = new CommonPlayResultData.StageData { MusicCateg = musicCateg };

        GreenProfileCounters.ApplyStage(save, stage);

        var property = typeof(UserSaveDataGreen).GetProperty(expectedProperty)!;
        Assert.Equal(1u, (uint)property.GetValue(save)!);
    }

    [Theory]
    [InlineData(8u)]
    [InlineData(99u)]
    [InlineData(uint.MaxValue)]
    public void ApplyStage_GenreOutOfRangeDoesNotMoveAnyCounter(uint musicCateg)
    {
        var save = new UserSaveDataGreen { Baid = 1 };
        var stage = new CommonPlayResultData.StageData { MusicCateg = musicCateg };

        GreenProfileCounters.ApplyStage(save, stage);

        Assert.Equal(0u, save.CategJpopCnt);
        Assert.Equal(0u, save.CategAnimeCnt);
        Assert.Equal(0u, save.CategDoyoCnt);
        Assert.Equal(0u, save.CategVocaloidCnt);
        Assert.Equal(0u, save.CategGameCnt);
        Assert.Equal(0u, save.CategNamcoCnt);
        Assert.Equal(0u, save.CategVarietyCnt);
        Assert.Equal(0u, save.CategClassicCnt);
    }

    [Fact]
    public void ApplyStage_IsPushedTrue_IncrementsSongPushedCnt()
    {
        var save = new UserSaveDataGreen { Baid = 1 };
        var stage = new CommonPlayResultData.StageData { MusicCateg = 0, IsPushed = true };

        GreenProfileCounters.ApplyStage(save, stage);

        Assert.Equal(1u, save.SongPushedCnt);
    }

    [Fact]
    public void ApplyStage_IsFavoriteTrue_IncrementsSongFavoriteCnt()
    {
        var save = new UserSaveDataGreen { Baid = 1 };
        var stage = new CommonPlayResultData.StageData { MusicCateg = 0, IsFavorite = true };

        GreenProfileCounters.ApplyStage(save, stage);

        Assert.Equal(1u, save.SongFavoriteCnt);
    }

    [Fact]
    public void ApplyStage_IsRecentTrue_IncrementsSongRecentCnt()
    {
        var save = new UserSaveDataGreen { Baid = 1 };
        var stage = new CommonPlayResultData.StageData { MusicCateg = 0, IsRecent = true };

        GreenProfileCounters.ApplyStage(save, stage);

        Assert.Equal(1u, save.SongRecentCnt);
    }

    [Fact]
    public void ApplyStage_AllFlagsFalse_LeavesSongCountersAtZero()
    {
        var save = new UserSaveDataGreen { Baid = 1 };
        var stage = new CommonPlayResultData.StageData { MusicCateg = 0 };

        GreenProfileCounters.ApplyStage(save, stage);

        Assert.Equal(0u, save.SongPushedCnt);
        Assert.Equal(0u, save.SongFavoriteCnt);
        Assert.Equal(0u, save.SongRecentCnt);
    }

    [Fact]
    public void ApplyStage_SaturatesAtUintMaxValue()
    {
        var save = new UserSaveDataGreen
        {
            Baid = 1,
            CategJpopCnt = uint.MaxValue,
            SongPushedCnt = uint.MaxValue,
            SongFavoriteCnt = uint.MaxValue,
            SongRecentCnt = uint.MaxValue
        };
        var stage = new CommonPlayResultData.StageData
        {
            MusicCateg = 0,
            IsPushed = true,
            IsFavorite = true,
            IsRecent = true
        };

        GreenProfileCounters.ApplyStage(save, stage);

        Assert.Equal(uint.MaxValue, save.CategJpopCnt);
        Assert.Equal(uint.MaxValue, save.SongPushedCnt);
        Assert.Equal(uint.MaxValue, save.SongFavoriteCnt);
        Assert.Equal(uint.MaxValue, save.SongRecentCnt);
    }
}
```

- [ ] **Step 2: Run the tests and verify they fail**

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenProfileCountersTests"
```

Expected: COMPILE ERROR — `GreenProfileCounters` does not exist.

- [ ] **Step 3: Implement the helper**

Create `Application/Common/GreenProfileCounters.cs`:

```csharp
namespace TaikoLocalServer.Application.Common;

public static class GreenProfileCounters
{
    public static void ApplyStage(UserSaveDataGreen saveData, CommonPlayResultData.StageData stage)
    {
        IncrementGenreCounter(saveData, stage.MusicCateg);
        if (stage.IsPushed)   saveData.SongPushedCnt   = SafeIncrement(saveData.SongPushedCnt);
        if (stage.IsFavorite) saveData.SongFavoriteCnt = SafeIncrement(saveData.SongFavoriteCnt);
        if (stage.IsRecent)   saveData.SongRecentCnt   = SafeIncrement(saveData.SongRecentCnt);
    }

    private static void IncrementGenreCounter(UserSaveDataGreen saveData, uint musicCateg)
    {
        switch (musicCateg)
        {
            case 0: saveData.CategJpopCnt     = SafeIncrement(saveData.CategJpopCnt); break;
            case 1: saveData.CategAnimeCnt    = SafeIncrement(saveData.CategAnimeCnt); break;
            case 2: saveData.CategDoyoCnt     = SafeIncrement(saveData.CategDoyoCnt); break;
            case 3: saveData.CategVocaloidCnt = SafeIncrement(saveData.CategVocaloidCnt); break;
            case 4: saveData.CategGameCnt     = SafeIncrement(saveData.CategGameCnt); break;
            case 5: saveData.CategNamcoCnt    = SafeIncrement(saveData.CategNamcoCnt); break;
            case 6: saveData.CategVarietyCnt  = SafeIncrement(saveData.CategVarietyCnt); break;
            case 7: saveData.CategClassicCnt  = SafeIncrement(saveData.CategClassicCnt); break;
        }
    }

    private static uint SafeIncrement(uint current)
        => current == uint.MaxValue ? current : current + 1;
}
```

The `Application/Common` namespace is already globally imported by tests (`global using TaikoLocalServer.Application.Common;` in `Tests/GlobalUsings.cs`), so the test file needs no `using` directive.

- [ ] **Step 4: Run the tests and verify they pass**

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenProfileCountersTests"
```

Expected: all 13 fact/theory cases PASS.

- [ ] **Step 5: Commit Task 3**

```bash
git add Application/Common/GreenProfileCounters.cs Tests/Green/GreenProfileCountersTests.cs
git commit -m "Add Green profile counter helper"
```
