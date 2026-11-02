# Stage 4: Skipped-Dan Display Advancement

## Goal

Change Green Dan display advancement so a positive normal Dan clear advances from the cleared slot, not from the first uncleared slot starting at 1.

## Files

- Modify: `Tests/Green/GreenHandlerFixture.cs`
- Modify: `Tests/Green/GreenPlayResultHandlerTests.cs`
- Modify: `Application/Common/GreenDanHelpers.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`

## Steps

- [ ] **Step 1: Let Green tests define custom Taikojuku rows**

Modify `Tests/Green/GreenHandlerFixture.cs` so `TestGreenCatalog` stores configurable Taikojuku rows.

Replace the existing constructor and fixed `TaikojukuFileOrder` property with this shape:

```csharp
private readonly IReadOnlyList<GreenMusicInfoEntry> musicInfoFileOrder;
private readonly IReadOnlyList<GreenTaikojukuEntry> taikojukuFileOrder;

public TestGreenCatalog(
    IReadOnlyDictionary<uint, GreenItemShopEntry>? itemShop = null,
    IReadOnlyList<GreenMusicInfoEntry>? musicInfoFileOrder = null,
    IReadOnlyList<GreenTaikojukuEntry>? taikojukuFileOrder = null)
{
    ItemShop = itemShop ?? new Dictionary<uint, GreenItemShopEntry>();
    this.musicInfoFileOrder = musicInfoFileOrder ?? DefaultMusicInfoFileOrder;
    this.taikojukuFileOrder = taikojukuFileOrder ?? DefaultTaikojukuFileOrder;
}
```

Move the current inline Taikojuku list into:

```csharp
private static IReadOnlyList<GreenTaikojukuEntry> DefaultTaikojukuFileOrder { get; } =
[
    new()
    {
        UniqueId = 20001,
        ChallengeLevel = 1,
        Conditions = new GreenTaikojukuConditions
        {
            SoulGauge = 90,
            TotalHitCount = 420
        },
        ExcellentConditions = new GreenTaikojukuConditions
        {
            SoulGauge = 95,
            TotalHitCount = 460
        },
        Songs =
        [
            new() { SongNo = 101, Level = 0 },
            new() { SongNo = 102, Level = 0 },
            new() { SongNo = 103, Level = 0 }
        ]
    },
    new()
    {
        UniqueId = 20026,
        ChallengeLevel = 101,
        Songs =
        [
            new() { SongNo = 104, Level = 1 },
            new() { SongNo = 105, Level = 1 },
            new() { SongNo = 106, Level = 1 }
        ]
    }
];

public IReadOnlyList<GreenTaikojukuEntry> TaikojukuFileOrder => taikojukuFileOrder;
```

Keep the existing `Taikojuku` dictionary implementation based on `TaikojukuFileOrder`.

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayResultHandlerTests.UpdatePlayResult_Green_DaniNormalClearUpdatesFlagsAndDisplayDan"
```

Expected: existing Dan tests still pass.

- [ ] **Step 2: Add failing skipped-Dan and cap tests**

Add this helper near the bottom of `Tests/Green/GreenPlayResultHandlerTests.cs`:

```csharp
private static GreenTaikojukuEntry TestDanPack(uint danId)
    => new()
    {
        UniqueId = 20000 + danId,
        ChallengeLevel = danId,
        Songs =
        [
            new() { SongNo = 101, Level = 0 }
        ]
    };
```

Add these tests near the existing Dani tests:

```csharp
[Fact]
public async Task UpdatePlayResult_Green_DaniSkippedNormalClearAdvancesFromClearedDan()
{
    var catalog = new GreenHandlerFixture.TestGreenCatalog(
        taikojukuFileOrder:
        [
            TestDanPack(1),
            TestDanPack(5)
        ]);
    await using var fixture = await GreenHandlerFixture.CreateAsync(catalog);
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
    await fixture.Context.SaveChangesAsync();

    var handler = new UpdatePlayResultCommandHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UpdatePlayResultCommandHandler>.Instance);

    await handler.Handle(new UpdatePlayResultCommand(
        1,
        GameEra.Green,
        new CommonPlayResultData
        {
            Baid = 1,
            PlayMode = 1,
            DanResult = 1,
            AryStageInfoes = [new() { SongNo = 101, Level = 1, PlayScore = 100, PlayDan = 5 }]
        }),
        CancellationToken.None);

    var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
    Assert.NotNull(save);
    Assert.Equal(5u, save!.GotDanMax);
    Assert.Equal(6u, save.DispTaikojukuDan);
}

[Fact]
public async Task UpdatePlayResult_Green_DaniLastNormalClearCapsDisplayDan()
{
    var catalog = new GreenHandlerFixture.TestGreenCatalog(
        taikojukuFileOrder:
        [
            TestDanPack(25)
        ]);
    await using var fixture = await GreenHandlerFixture.CreateAsync(catalog);
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
    await fixture.Context.SaveChangesAsync();

    var handler = new UpdatePlayResultCommandHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UpdatePlayResultCommandHandler>.Instance);

    await handler.Handle(new UpdatePlayResultCommand(
        1,
        GameEra.Green,
        new CommonPlayResultData
        {
            Baid = 1,
            PlayMode = 1,
            DanResult = 2,
            AryStageInfoes = [new() { SongNo = 101, Level = 1, PlayScore = 100, PlayDan = 25 }]
        }),
        CancellationToken.None);

    var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
    Assert.NotNull(save);
    Assert.Equal(25u, save!.GotDanMax);
    Assert.Equal(25u, save.DispTaikojukuDan);
}
```

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayResultHandlerTests.UpdatePlayResult_Green_DaniSkippedNormalClearAdvancesFromClearedDan|FullyQualifiedName~GreenPlayResultHandlerTests.UpdatePlayResult_Green_DaniLastNormalClearCapsDisplayDan"
```

Expected: the skipped-Dan test fails because current advancement normalizes to first uncleared Dan from slot 1.

- [ ] **Step 3: Add the clear-slot advancement helper**

In `Application/Common/GreenDanHelpers.cs`, add this method after `GetNextUnclearedNormalDan`:

```csharp
public static uint GetDisplayDanAfterNormalClear(uint clearedDanId)
{
    if (!IsNormalDanId(clearedDanId))
    {
        throw new ArgumentOutOfRangeException(nameof(clearedDanId), clearedDanId, "Green display Dan advancement only accepts normal Dan ids.");
    }

    return Math.Min(clearedDanId + 1, MaxNormalDanId);
}
```

- [ ] **Step 4: Use the cleared slot after positive normal Dan clears**

In `Application/Handlers/UpdatePlayResultCommand.Green.cs`, replace the last line of `UpdateGreenDanSummaryAsync`:

```csharp
saveData.DispTaikojukuDan = GreenDanHelpers.NormalizeDisplayDan(saveData.DispTaikojukuDan, normalGrades);
```

with:

```csharp
saveData.DispTaikojukuDan = !currentDanScore.IsExtra
                            && GreenDanHelpers.IsNormalDanId(currentDanScore.DanId)
                            && GreenDanHelpers.IsClear(currentDanScore.ClearGrade)
    ? GreenDanHelpers.GetDisplayDanAfterNormalClear(currentDanScore.DanId)
    : GreenDanHelpers.NormalizeDisplayDan(saveData.DispTaikojukuDan, normalGrades);
```

- [ ] **Step 5: Run focused Dan tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayResultHandlerTests.UpdatePlayResult_Green_Dani"
```

Expected: all selected Dani playresult tests pass.

- [ ] **Step 6: Commit Stage 4**

Run:

```powershell
git status --short
git add -- Tests/Green/GreenHandlerFixture.cs Tests/Green/GreenPlayResultHandlerTests.cs Application/Common/GreenDanHelpers.cs Application/Handlers/UpdatePlayResultCommand.Green.cs
git diff --cached --name-status
git commit -m "Fix Green skipped Dan display advancement"
```

Expected staged files: only the four files listed above.
