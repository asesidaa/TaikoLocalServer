# Stage 2: Admin API Settings Contract

## Goal

Expose `GreenIsTojiru` and `GreenDispLevelChassis` through the Admin API settings contract with validation for the `0..4` local ranking difficulty domain.

## Files

- Modify: `Contracts.AdminApi/ViewModels/UserSetting.cs`
- Modify: `Adapters.AdminApi/Controllers/UserSettingsController.Green.cs`
- Modify: `Tests/Green/GreenAdminApiControllerTests.cs`

## Steps

- [ ] **Step 1: Add failing Admin API GET coverage**

Add this test to `Tests/Green/GreenAdminApiControllerTests.cs` after `UserSettings_Green_GetMapsDispDanTypeToProfileDisplayDan`:

```csharp
[Fact]
public async Task UserSettings_Green_GetExposesTojiruAndLocalRankingDifficulty()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
    save.IsTojiru = false;
    save.DispLevelChassis = 3;
    fixture.Context.UserSaveDataGreen.Add(save);
    await fixture.Context.SaveChangesAsync();

    var controller = CreateUserSettingsController(fixture.Context);
    var result = await controller.GetUserSetting("Green", 1);

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var setting = Assert.IsType<UserSetting>(ok.Value);
    Assert.False(setting.GreenIsTojiru);
    Assert.Equal(3u, setting.GreenDispLevelChassis);
}
```

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenAdminApiControllerTests.UserSettings_Green_GetExposesTojiruAndLocalRankingDifficulty"
```

Expected: compilation fails until the new `UserSetting` properties exist, then the test fails until GET mapping is implemented.

- [ ] **Step 2: Add failing Admin API POST and validation coverage**

Add these tests to `Tests/Green/GreenAdminApiControllerTests.cs` after the GET test from Step 1:

```csharp
[Fact]
public async Task UserSettings_Green_PostPersistsTojiruAndLocalRankingDifficulty()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
    await fixture.Context.SaveChangesAsync();

    var controller = CreateUserSettingsController(fixture.Context);

    var result = await controller.SaveUserSetting("Green", 1, new UserSetting
    {
        MyDonName = "GREEN",
        GreenIsTojiru = false,
        GreenDispLevelChassis = 4
    });

    Assert.IsType<NoContentResult>(result);
    var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
    Assert.NotNull(save);
    Assert.False(save!.IsTojiru);
    Assert.Equal(4u, save.DispLevelChassis);
}

[Fact]
public async Task UserSettings_Green_PostRejectsInvalidLocalRankingDifficulty()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
    save.DispLevelChassis = 2;
    fixture.Context.UserSaveDataGreen.Add(save);
    await fixture.Context.SaveChangesAsync();

    var controller = CreateUserSettingsController(fixture.Context);

    var result = await controller.SaveUserSetting("Green", 1, new UserSetting
    {
        MyDonName = "GREEN",
        GreenIsTojiru = true,
        GreenDispLevelChassis = 5
    });

    Assert.IsType<BadRequestObjectResult>(result);
    Assert.Equal(2u, save.DispLevelChassis);
}
```

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenAdminApiControllerTests.UserSettings_Green_PostPersistsTojiruAndLocalRankingDifficulty|FullyQualifiedName~GreenAdminApiControllerTests.UserSettings_Green_PostRejectsInvalidLocalRankingDifficulty"
```

Expected: tests fail until POST persistence and validation are implemented.

- [ ] **Step 3: Add contract properties**

In `Contracts.AdminApi/ViewModels/UserSetting.cs`, add these properties after `GreenSelectableTaikojukuDans`:

```csharp
public bool GreenIsTojiru { get; set; }

public uint GreenDispLevelChassis { get; set; }
```

- [ ] **Step 4: Implement Green settings GET mapping**

In `Adapters.AdminApi/Controllers/UserSettingsController.Green.cs`, add these assignments to the `new UserSetting` initializer in `BuildGreenUserSetting`:

```csharp
GreenIsTojiru = saveData.IsTojiru,
GreenDispLevelChassis = GetSafeGreenDispLevelChassis(saveData.DispLevelChassis),
```

Add this helper near `SelectGreenTaikojukuFolderDan`:

```csharp
private static uint GetSafeGreenDispLevelChassis(uint value)
    => value <= 4 ? value : 0u;
```

- [ ] **Step 5: Implement Green settings POST validation and persistence**

In `SaveGreenUserSetting`, after loading `saveData` and before mutating fields, add:

```csharp
if (userSetting.GreenDispLevelChassis > 4)
{
    return BadRequest("GreenDispLevelChassis must be between 0 and 4.");
}
```

Then add these assignments beside the existing Green profile setting assignments:

```csharp
saveData.IsTojiru = userSetting.GreenIsTojiru;
saveData.DispLevelChassis = userSetting.GreenDispLevelChassis;
```

- [ ] **Step 6: Run focused Admin API tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenAdminApiControllerTests.UserSettings_Green"
```

Expected: all Green user settings tests pass.

- [ ] **Step 7: Commit Stage 2**

Run:

```powershell
git status --short
git add -- Contracts.AdminApi/ViewModels/UserSetting.cs Adapters.AdminApi/Controllers/UserSettingsController.Green.cs Tests/Green/GreenAdminApiControllerTests.cs
git diff --cached --name-status
git commit -m "Expose Green userdata settings in Admin API"
```

Expected staged files: only the three files listed above.
