# Task 1: DTO & Wire Mapper for IsPushed

**Goal:** Capture the `is_pushed` proto field (Green stage wire member 16) end-to-end into `CommonPlayResultData.StageData` so downstream handlers can read it.

**Files:**
- Modify: `Application/Dtos/CommonPlayResultData.Green.cs`
- Modify: `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`
- Test: `Tests/Green/GreenPlayResultMapperTests.cs`

**Acceptance Criteria:**
- [ ] `CommonPlayResultData.StageData` has a Green-only `IsPushed` bool.
- [ ] `PlayResultMappers.MapStage` copies `stage.IsPushed` from the wire request.
- [ ] New test `Map_GreenPlayResult_PreservesIsPushed` asserts `IsPushed=true` round-trips.

**Verify:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayResultMapperTests"` → all green tests pass; the new `IsPushed` test passes.

---

- [ ] **Step 1: Write the failing mapper test**

Append this test to `Tests/Green/GreenPlayResultMapperTests.cs`, right before the `CreateRequest` helper at the bottom of the class:

```csharp
[Fact]
public void Map_GreenPlayResult_PreservesIsPushed()
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
        MusicCateg = 0,
        IsFavorite = false,
        IsRecent = false,
        IsPapamama = false,
        IsPushed = true,
        StageMode = 0,
        SelectedFolderId = 0,
        StarLevel = 3,
        SupportLevel = 0
    });

    var common = PlayResultMappers.Map(request);

    Assert.True(Assert.Single(common.AryStageInfoes).IsPushed);
}
```

- [ ] **Step 2: Run the test and verify it fails**

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Map_GreenPlayResult_PreservesIsPushed"
```

Expected: COMPILE ERROR — `CommonPlayResultData.StageData` has no `IsPushed` member.

- [ ] **Step 3: Add `IsPushed` to the Green DTO partial**

Open `Application/Dtos/CommonPlayResultData.Green.cs`. Inside the `partial class StageData` block (currently containing `WaiwaiResult`, `WaiwaiGauge`, `SoulGauge`, `HitCount`, `PlayDan`, `GhostStageData`), add a new property below `GhostStageData`:

```csharp
public bool IsPushed { get; set; }
```

The resulting `StageData` partial body becomes:

```csharp
public partial class StageData
{
    public uint? WaiwaiResult { get; set; }
    public uint? WaiwaiGauge  { get; set; }
    public uint? SoulGauge    { get; set; }
    public uint? HitCount     { get; set; }
    public uint? PlayDan      { get; set; }
    public GhostStageData? GhostStageData { get; set; }
    public bool IsPushed { get; set; }
}
```

Note: `IsPushed` lives in the Green partial (not the base `CommonPlayResultData.cs`) because only the Green wire has it. The Nijiiro `StageData` is unaffected.

- [ ] **Step 4: Extend the Green mapper**

Open `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`. In `MapStage`, add `IsPushed = stage.IsPushed` to the object initializer next to the other per-stage bools. The final lines of `MapStage` should read:

```csharp
StageMode = stage.StageMode,
IsPapamama = stage.IsPapamama,
IsPushed = stage.IsPushed
```

The full updated `MapStage` ends:

```csharp
private static CommonPlayResultData.StageData MapStage(PlayResultDataRequest.StageData stage)
{
    return new CommonPlayResultData.StageData
    {
        SongNo = stage.SongNo,
        // ... (unchanged) ...
        GhostStageData = MapGhostStage(stage.GhostStagedata),
        StageMode = stage.StageMode,
        IsPapamama = stage.IsPapamama,
        IsPushed = stage.IsPushed
    };
}
```

- [ ] **Step 5: Run the test and verify it passes**

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayResultMapperTests"
```

Expected: all Green mapper tests pass, including the new `Map_GreenPlayResult_PreservesIsPushed`.

- [ ] **Step 6: Commit Task 1**

```bash
git add Application/Dtos/CommonPlayResultData.Green.cs Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs Tests/Green/GreenPlayResultMapperTests.cs
git commit -m "Map Green stage is_pushed into common DTO"
```
