# Task 4: BAID and Userdata Projection

**Files:**
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- Modify: `Application/Handlers/BaidQuery.Green.cs`
- Modify: `Application/Handlers/UserDataQuery.Green.cs`
- Test: `Tests/Green/GreenPlayResultHandlerTests.cs`
- Test: `Tests/Green/GreenIdentityHandlerTests.cs`

- [ ] **Step 1: Write failing playresult flag/display tests**

Add these tests to `Tests/Green/GreenPlayResultHandlerTests.cs`:

```csharp
[Fact]
public async Task UpdatePlayResult_Green_DaniNormalClearUpdatesFlagsAndDisplayDan()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
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
            AryStageInfoes = [new() { SongNo = 101, Level = 1, PlayScore = 100, PlayDan = 1 }]
        }),
        CancellationToken.None);

    var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
    Assert.NotNull(save);
    Assert.Equal(1u, save!.GotDanMax);
    Assert.Equal(2u, save.DispTaikojukuDan);
    Assert.Equal(GreenDanClearGrade.NormalClear, GreenDanHelpers.GetPackedGrade(save.GotDanFlg, 0));
}

[Fact]
public async Task UpdatePlayResult_Green_DaniExtraClearUpdatesExtraFlagsOnly()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
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
            AryStageInfoes = [new() { SongNo = 104, Level = 2, PlayScore = 100, PlayDan = 101 }]
        }),
        CancellationToken.None);

    var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
    Assert.NotNull(save);
    Assert.Equal(0u, save!.GotDanMax);
    Assert.Equal(1u, save.DispTaikojukuDan);
    Assert.Equal(GreenDanClearGrade.GoldClear, GreenDanHelpers.GetPackedGrade(save.GotDanExtraFlg, 0));
    Assert.Equal(GreenDanClearGrade.NotClear, GreenDanHelpers.GetPackedGrade(save.GotDanFlg, 0));
}

[Fact]
public async Task UpdatePlayResult_Green_DaniFailedAttemptDoesNotAdvanceDisplayDan()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
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
            DanResult = 0,
            AryStageInfoes = [new() { SongNo = 101, Level = 1, PlayScore = 100, PlayDan = 1 }]
        }),
        CancellationToken.None);

    var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
    Assert.NotNull(save);
    Assert.Equal(0u, save!.GotDanMax);
    Assert.Equal(1u, save.DispTaikojukuDan);
    Assert.Equal(GreenDanClearGrade.NotClear, GreenDanHelpers.GetPackedGrade(save.GotDanFlg, 0));
}
```

- [ ] **Step 2: Run the tests and verify they fail**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~DaniNormalClearUpdatesFlagsAndDisplayDan|FullyQualifiedName~DaniExtraClearUpdatesExtraFlagsOnly|FullyQualifiedName~DaniFailedAttemptDoesNotAdvanceDisplayDan"
```

Expected: FAIL because summary flags and display slot are not updated.

- [ ] **Step 3: Implement summary update**

Replace the temporary `UpdateGreenDanSummaryAsync` in `Application/Handlers/UpdatePlayResultCommand.Green.cs` with:

```csharp
private async ValueTask UpdateGreenDanSummaryAsync(UserSaveDataGreen saveData, CancellationToken cancellationToken)
{
    var rows = await context.DanScoreDataGreen
        .Where(row => row.Baid == saveData.Baid)
        .ToListAsync(cancellationToken);

    var normalGrades = rows
        .Where(row => !row.IsExtra && GreenDanHelpers.IsNormalDanId(row.DanId))
        .ToDictionary(row => row.DanId, row => row.ClearGrade);

    var normalFlags = new byte[GreenProtocolBytes.DanFlagBytes];
    foreach (var row in rows.Where(row => !row.IsExtra && GreenDanHelpers.IsNormalDanId(row.DanId)))
    {
        normalFlags = GreenDanHelpers.SetPackedGrade(
            normalFlags,
            GreenDanHelpers.GetPackedIndex(row.DanId),
            row.ClearGrade);
    }

    var extraFlags = new byte[GreenProtocolBytes.DanExtraFlagBytes];
    foreach (var row in rows.Where(row => row.IsExtra && GreenDanHelpers.IsExtraDanId(row.DanId)))
    {
        extraFlags = GreenDanHelpers.SetPackedGrade(
            extraFlags,
            GreenDanHelpers.GetPackedIndex(row.DanId),
            row.ClearGrade);
    }

    saveData.GotDanFlg = normalFlags;
    saveData.GotDanExtraFlg = extraFlags;
    saveData.GotDanMax = GreenDanHelpers.GetGotDanMax(normalGrades);
    saveData.DispTaikojukuDan = GreenDanHelpers.NormalizeDisplayDan(saveData.DispTaikojukuDan, normalGrades);
}
```

- [ ] **Step 4: Sanitize BAID output**

In `Application/Handlers/BaidQuery.Green.cs`, compute fixed flags before the response:

```csharp
var gotDanFlg = GreenProtocolBytes.FixedOrZero(saveData.GotDanFlg, GreenProtocolBytes.DanFlagBytes);
var gotDanExtraFlg = GreenProtocolBytes.FixedOrZero(saveData.GotDanExtraFlg, GreenProtocolBytes.DanExtraFlagBytes);
var gotDanMax = Math.Min(saveData.GotDanMax, GreenDanHelpers.MaxNormalDanId);
```

Then use these values in the response:

```csharp
GotDanFlg = gotDanFlg,
GotDanMax = gotDanMax,
GotDanExtraFlg = gotDanExtraFlg,
```

- [ ] **Step 5: Make userdata recompute safe display from saved Dan rows**

In `Application/Handlers/UserDataQuery.Green.cs`, load Green Dan normal grades before the return:

```csharp
var normalDanGrades = await context.DanScoreDataGreen
    .Where(row => row.Baid == request.Baid && !row.IsExtra)
    .ToDictionaryAsync(row => row.DanId, row => row.ClearGrade, cancellationToken);
var displayDan = GreenDanHelpers.NormalizeDisplayDan(saveData.DispTaikojukuDan, normalDanGrades);
```

Then set:

```csharp
DispTaikojukuDan = GetSafeTaikojukuDanSlot(displayDan),
```

- [ ] **Step 6: Run projection tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~DaniNormalClearUpdatesFlagsAndDisplayDan|FullyQualifiedName~DaniExtraClearUpdatesExtraFlagsOnly|FullyQualifiedName~DaniFailedAttemptDoesNotAdvanceDisplayDan|FullyQualifiedName~UserData_Green_NewSaveSendsSentinelOneForDispTaikojukuDan"
```

Expected: PASS.

- [ ] **Step 7: Commit Task 4**

```powershell
git add Application/Handlers/UpdatePlayResultCommand.Green.cs Application/Handlers/BaidQuery.Green.cs Application/Handlers/UserDataQuery.Green.cs Tests/Green/GreenPlayResultHandlerTests.cs
git commit -m "Project Green Dani clear flags"
```

