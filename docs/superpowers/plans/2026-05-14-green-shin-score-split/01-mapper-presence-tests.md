# Task 1: Mapper Presence Tests

**Files:**
- Create: `Tests/Green/GreenPlayResultMapperTests.cs`
- Modify: `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`

## Steps

- [ ] **Step 1: Add failing mapper tests**

Create `Tests/Green/GreenPlayResultMapperTests.cs`:

```csharp
using TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Green.Wire;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenPlayResultMapperTests
{
    [Fact]
    public void Map_GreenPlayResult_PreservesStageModeAndIsPapamama()
    {
        var request = CreateRequest();
        request.AryStageInfoes.Add(new PlayResultDataRequest.StageData
        {
            SongNo = 101,
            Level = 1,
            PlayResult = 1,
            PlayScore = 123456,
            GoodCnt = 10,
            OkCnt = 2,
            NgCnt = 1,
            PoundCnt = 3,
            ComboCnt = 12,
            OptionFlg = [0, 0],
            ToneFlg = new byte[16],
            MusicCateg = 1,
            IsFavorite = false,
            IsRecent = false,
            IsPapamama = true,
            StageMode = 1,
            SelectedFolderId = 0,
            StarLevel = 3,
            SupportLevel = 0
        });

        var common = PlayResultMappers.Map(request);

        var stage = Assert.Single(common.AryStageInfoes);
        Assert.Equal(1u, stage.StageMode);
        Assert.True(stage.IsPapamama);
    }

    [Fact]
    public void Map_GreenPlayResult_TreatsZeroPlayDanAsNoDan()
    {
        var request = CreateRequest();
        request.AryStageInfoes.Add(new PlayResultDataRequest.StageData
        {
            SongNo = 101,
            Level = 1,
            PlayResult = 1,
            PlayScore = 123456,
            GoodCnt = 10,
            OkCnt = 2,
            NgCnt = 1,
            PoundCnt = 3,
            ComboCnt = 12,
            OptionFlg = [0, 0],
            ToneFlg = new byte[16],
            MusicCateg = 1,
            IsFavorite = false,
            IsRecent = false,
            IsPapamama = false,
            PlayDan = 0,
            StageMode = 0,
            SelectedFolderId = 0,
            StarLevel = 3,
            SupportLevel = 0
        });

        var common = PlayResultMappers.Map(request);

        Assert.Null(Assert.Single(common.AryStageInfoes).PlayDan);
    }

    [Fact]
    public void Map_GreenPlayResult_PreservesNonZeroPlayDan()
    {
        var request = CreateRequest();
        request.AryStageInfoes.Add(new PlayResultDataRequest.StageData
        {
            SongNo = 101,
            Level = 1,
            PlayResult = 1,
            PlayScore = 123456,
            GoodCnt = 10,
            OkCnt = 2,
            NgCnt = 1,
            PoundCnt = 3,
            ComboCnt = 12,
            OptionFlg = [0, 0],
            ToneFlg = new byte[16],
            MusicCateg = 1,
            IsFavorite = false,
            IsRecent = false,
            IsPapamama = false,
            PlayDan = 7,
            StageMode = 0,
            SelectedFolderId = 0,
            StarLevel = 3,
            SupportLevel = 0
        });

        var common = PlayResultMappers.Map(request);

        Assert.Equal(7u, Assert.Single(common.AryStageInfoes).PlayDan);
    }

    private static PlayResultDataRequest CreateRequest() => new()
    {
        Baid = 1,
        ChassisId = "268410000000",
        ShopId = "JPN0JPN0123",
        PlayDatetime = "20260514032442",
        IsRight = false,
        CardType = 1,
        IsTwoPlayers = false,
        BonusDailyFlg = false,
        BonusWeeklyFlg = false,
        BonusMonthlyFlg = false,
        GetDonmedal = 0,
        GetKatsumedal = 0,
        GenderType = 0,
        PlayerAge = 0,
        PlayMode = 0,
        AreaCode = 1,
        Reserved = new byte[16]
    };
}
```

- [ ] **Step 2: Run the mapper tests and confirm failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~GreenPlayResultMapperTests
```

Expected: fails because `StageMode` and `IsPapamama` are not mapped, and `PlayDan = 0` remains non-null.

- [ ] **Step 3: Preserve the Green stage fields in the mapper**

In `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`, replace the tail of `MapStage` with this property set:

```csharp
            StarLevel = stage.StarLevel,
            SoulGauge = stage.SoulGauge,
            PlayDan = stage.PlayDan == 0 ? null : stage.PlayDan,
            WaiwaiResult = stage.WaiwaiResult,
            WaiwaiGauge = stage.WaiwaiGauge,
            GhostStageData = MapGhostStage(stage.GhostStagedata),
            StageMode = stage.StageMode,
            IsPapamama = stage.IsPapamama
```

The full `MapStage` return object should still include the existing fields above this block.

- [ ] **Step 4: Run mapper tests and confirm pass**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~GreenPlayResultMapperTests
```

Expected: pass.

- [ ] **Step 5: Commit mapper tests and mapper fix**

Run:

```powershell
git add -- Tests/Green/GreenPlayResultMapperTests.cs Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs
git commit -m "Preserve Green stage mode mapping"
```
