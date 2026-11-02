# Stage 1: Userdata Defaults And Green Response Mapping

## Goal

Make Green default `IsTojiru` true and return saved Green display-level fields through userdata.

## Files

- Modify: `Tests/Green/GreenSaveDataTests.cs`
- Modify: `Tests/Green/GreenIdentityHandlerTests.cs`
- Modify: `Tests/Green/GreenUserDataMapperTests.cs`
- Modify: `Application/Common/UserSaveDataGreenExtensions.cs`
- Modify: `Application/Handlers/UserDataQuery.Green.cs`
- Modify: `Adapters.GameProtocol.Green/Mappers/UserDataMappers.cs`

## Steps

- [ ] **Step 1: Add a failing default-save test**

Add this assertion to `CreateDefaultGreenSaveData_InitializesFixedWidthBytes` in `Tests/Green/GreenSaveDataTests.cs`:

```csharp
Assert.True(save.IsTojiru);
```

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenSaveDataTests"
```

Expected: the test fails because `CreateDefaultGreenSaveData` currently sets `IsTojiru = false`.

- [ ] **Step 2: Add a failing userdata handler test for saved Green fields**

Add this test to `Tests/Green/GreenIdentityHandlerTests.cs` before `UserData_Green_RecommendComesFromCatalog`:

```csharp
[Fact]
public async Task UserData_Green_ReturnsPersistedTojiruAndDisplayLevels()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
    save.IsTojiru = false;
    save.DispLevelTotal = 2;
    save.DispLevelChassis = 3;
    save.DispLevelSelf = 4;
    fixture.Context.UserSaveDataGreen.Add(save);
    await fixture.Context.SaveChangesAsync();

    var handler = new UserDataQueryHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UserDataQueryHandler>.Instance,
        Options.Create(new ServerSettings()));

    var response = await handler.Handle(new UserDataQuery(1, GameEra.Green), CancellationToken.None);

    Assert.False(response.IsTojiru);
    Assert.Equal(2u, response.DispLevelTotal);
    Assert.Equal(3u, response.DispLevelChassis);
    Assert.Equal(4u, response.DispLevelSelf);
}
```

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenIdentityHandlerTests.UserData_Green_ReturnsPersistedTojiruAndDisplayLevels"
```

Expected: the test fails because Green userdata does not currently copy display-level fields.

- [ ] **Step 3: Add failing mapper coverage for Green display-level fields**

Add this test to `Tests/Green/GreenUserDataMapperTests.cs` before `UserData_SerializesToneAndTitleFlagsOnWire`:

```csharp
[Fact]
public void UserData_MapsGreenDisplayLevelFields()
{
    var response = UserDataMappers.Map(new CommonUserDataResponse
    {
        Result = 1,
        DispLevelTotal = 2,
        DispLevelChassis = 3,
        DispLevelSelf = 4
    });

    Assert.True(response.ShouldSerializeDispLevelTotal());
    Assert.True(response.ShouldSerializeDispLevelChassis());
    Assert.True(response.ShouldSerializeDispLevelSelf());
    Assert.Equal(2u, response.DispLevelTotal);
    Assert.Equal(3u, response.DispLevelChassis);
    Assert.Equal(4u, response.DispLevelSelf);
}
```

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenUserDataMapperTests.UserData_MapsGreenDisplayLevelFields"
```

Expected: the test fails because `UserDataMappers.Map` does not currently set those generated protobuf fields.

- [ ] **Step 4: Implement the default and response mapping**

In `Application/Common/UserSaveDataGreenExtensions.cs`, change the last property initializer:

```csharp
IsTojiru = true
```

In `Application/Handlers/UserDataQuery.Green.cs`, add these assignments inside the `CommonUserDataResponse` initializer near the other saved profile fields:

```csharp
DispLevelTotal = saveData.DispLevelTotal,
DispLevelChassis = saveData.DispLevelChassis,
DispLevelSelf = saveData.DispLevelSelf,
```

Keep the existing `IsTojiru = saveData.IsTojiru` assignment.

In `Adapters.GameProtocol.Green/Mappers/UserDataMappers.cs`, add these assignments to the `new UserDataResponse` initializer:

```csharp
DispLevelTotal = common.DispLevelTotal,
DispLevelChassis = common.DispLevelChassis,
DispLevelSelf = common.DispLevelSelf,
```

- [ ] **Step 5: Run focused tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenSaveDataTests|FullyQualifiedName~GreenIdentityHandlerTests.UserData_Green_ReturnsPersistedTojiruAndDisplayLevels|FullyQualifiedName~GreenUserDataMapperTests.UserData_MapsGreenDisplayLevelFields"
```

Expected: all selected tests pass.

- [ ] **Step 6: Commit Stage 1**

Run:

```powershell
git status --short
git add -- Tests/Green/GreenSaveDataTests.cs Tests/Green/GreenIdentityHandlerTests.cs Tests/Green/GreenUserDataMapperTests.cs Application/Common/UserSaveDataGreenExtensions.cs Application/Handlers/UserDataQuery.Green.cs Adapters.GameProtocol.Green/Mappers/UserDataMappers.cs
git diff --cached --name-status
git commit -m "Add Green userdata display settings response"
```

Expected staged files: only the six files listed above.
