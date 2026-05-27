# Green Default Selected Difficulty Design And Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Expose Green `disp_level_self` as a Green-only Web UI setting that controls the default selected song difficulty.

**Architecture:** Reuse the existing Green userdata settings path. Add a `GreenDispLevelSelf` Admin API field that maps directly to `UserSaveDataGreen.DispLevelSelf`, render a Green-only select in the profile page, validate incoming values to the client-supported `0..4` range, and keep Green userdata serialization on the existing saved-field path.

**Tech Stack:** C# 13, .NET 10, ASP.NET Core controllers, Entity Framework Core entities already in place, Blazor/MudBlazor Web UI, xUnit text and controller tests, protobuf-net generated Green wire DTOs.

---

## Design

Green `disp_level_self` sets the default selected difficulty when selecting a song. The server already stores this field on `UserSaveDataGreen.DispLevelSelf`, returns it from `UserDataQuery.Green`, and serializes it through `Adapters.GameProtocol.Green/Mappers/UserDataMappers.cs`.

This feature makes the field user-editable only for Green profiles:

- `0`: None
- `1`: Easy
- `2`: Normal
- `3`: Hard
- `4`: Oni

The Web UI label is `Default Selected Difficulty`. It should appear near the existing `Local Ranking Difficulty` select, but remain a separate control because `disp_level_chassis` and `disp_level_self` have different client behavior.

New Green saves keep the existing `DispLevelSelf = 0` default. No migration is needed because the column already exists and stored values are meaningful.

## Scope

Implement now:

- Add `GreenDispLevelSelf` to `Contracts.AdminApi/ViewModels/UserSetting.cs`.
- Expose safe stored values from `Adapters.AdminApi/Controllers/UserSettingsController.Green.cs`.
- Validate and persist posted values in `Adapters.AdminApi/Controllers/UserSettingsController.Green.cs`.
- Add a Green-only `Default Selected Difficulty` select to `TaikoWebUI/Pages/Profile.razor`.
- Add Green difficulty labels to `TaikoWebUI/Pages/Profile.razor.cs`.
- Update tests that currently assert `disp_level_self` is not exposed.
- Add focused API and UI tests for read, write, validation, and rendering.
- Keep existing userdata response behavior and cover it with the existing mapper/handler tests.

Do not implement now:

- Do not expose the field for Nijiiro, Blue, CnR00, WwR08, or shared profile routes.
- Do not change `disp_level_chassis`, `disp_level_total`, `difficulty_played_course`, or `difficulty_played_star`.
- Do not add a migration.
- Do not rename the existing `GreenDispLevelChassis` API field or UI label.

## File Structure

Modify:

- `Contracts.AdminApi/ViewModels/UserSetting.cs` - add the Green-only API property.
- `Adapters.AdminApi/Controllers/UserSettingsController.Green.cs` - read, validate, and persist `GreenDispLevelSelf`.
- `TaikoWebUI/Pages/Profile.razor` - render the Green-only select.
- `TaikoWebUI/Pages/Profile.razor.cs` - add the option labels.
- `Tests/Green/GreenAdminApiControllerTests.cs` - cover Green settings API behavior.
- `Tests/WebUi/GreenCustomizationWebUiTests.cs` - cover Green profile markup and remove the old absence assertion.

No generated files should change.

## Task 1: Admin API Contract

**Files:**

- Modify: `Contracts.AdminApi/ViewModels/UserSetting.cs`
- Modify: `Adapters.AdminApi/Controllers/UserSettingsController.Green.cs`
- Test: `Tests/Green/GreenAdminApiControllerTests.cs`

- [ ] **Step 1: Write the failing GET test**

Add this assertion to `UserSettings_Green_GetExposesTojiruAndLocalRankingDifficulty` or rename it to include default selected difficulty:

```csharp
save.DispLevelSelf = 2;
Assert.Equal(2u, setting.GreenDispLevelSelf);
```

Expected failure before implementation: `UserSetting` does not define `GreenDispLevelSelf`.

- [ ] **Step 2: Write the failing POST test coverage**

Extend `UserSettings_Green_PostPersistsTojiruAndLocalRankingDifficulty` with:

```csharp
GreenDispLevelSelf = 4
```

and assert:

```csharp
Assert.Equal(4u, save.DispLevelSelf);
```

Expected failure before implementation: the property is missing or the saved value remains `0`.

- [ ] **Step 3: Write invalid-value coverage**

Extend `UserSettings_Green_PostRejectsInvalidLocalRankingDifficulty` or add a new test named `UserSettings_Green_PostRejectsInvalidDefaultSelectedDifficulty`:

```csharp
var result = await controller.SaveUserSetting("Green", 1, new UserSetting
{
    MyDonName = "GREEN",
    GreenIsTojiru = true,
    GreenDispLevelChassis = 2,
    GreenDispLevelSelf = 5
});

Assert.IsType<BadRequestObjectResult>(result);
Assert.Equal(2u, save.DispLevelChassis);
Assert.Equal(3u, save.DispLevelSelf);
```

Set `save.DispLevelSelf = 3` before saving so the unchanged assertion is meaningful.

- [ ] **Step 4: Run focused API tests and confirm the red state**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenAdminApiControllerTests.UserSettings_Green"
```

Expected: fails because `GreenDispLevelSelf` is not implemented yet.

- [ ] **Step 5: Add the API property**

Add to `Contracts.AdminApi/ViewModels/UserSetting.cs` near `GreenDispLevelChassis`:

```csharp
public uint GreenDispLevelSelf { get; set; }
```

- [ ] **Step 6: Implement GET, POST, and validation**

In `Adapters.AdminApi/Controllers/UserSettingsController.Green.cs`, validate before mutating save data:

```csharp
if (userSetting.GreenDispLevelSelf > 4)
{
    return BadRequest("GreenDispLevelSelf must be between 0 and 4.");
}
```

Persist on save:

```csharp
saveData.DispLevelSelf = userSetting.GreenDispLevelSelf;
```

Expose a safe value from `BuildGreenUserSetting`:

```csharp
GreenDispLevelSelf = GetSafeGreenDispLevelSelf(saveData.DispLevelSelf),
```

Add the helper beside `GetSafeGreenDispLevelChassis`:

```csharp
private static uint GetSafeGreenDispLevelSelf(uint value)
    => value <= 4 ? value : 0u;
```

- [ ] **Step 7: Run focused API tests and commit**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenAdminApiControllerTests.UserSettings_Green"
```

Expected: PASS.

Commit:

```powershell
git add -- Contracts.AdminApi/ViewModels/UserSetting.cs Adapters.AdminApi/Controllers/UserSettingsController.Green.cs Tests/Green/GreenAdminApiControllerTests.cs
git commit -m "Expose Green default selected difficulty setting"
```

## Task 2: Green Web UI Control

**Files:**

- Modify: `TaikoWebUI/Pages/Profile.razor`
- Modify: `TaikoWebUI/Pages/Profile.razor.cs`
- Test: `Tests/WebUi/GreenCustomizationWebUiTests.cs`

- [ ] **Step 1: Update the Web UI markup test**

In `Profile_RendersGreenUserdataSettingsOnlyForGreen`, add:

```csharp
Assert.Contains("@bind-Value=\"@response.GreenDispLevelSelf\"", greenBranch);
Assert.Contains("Default Selected Difficulty", greenBranch);
Assert.Contains("GreenDefaultSelectedDifficultyStrings", code);
```

Remove this old assertion:

```csharp
Assert.DoesNotContain("disp_level_self", markup, StringComparison.OrdinalIgnoreCase);
```

Keep the absence assertions for `disp_level_total`, `DifficultyPlayedCourse`, and `DifficultyPlayedStar`.

- [ ] **Step 2: Run the Web UI test and confirm the red state**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenCustomizationWebUiTests.Profile_RendersGreenUserdataSettingsOnlyForGreen"
```

Expected: fails because the markup and label array are not implemented yet.

- [ ] **Step 3: Add Web UI labels**

In `TaikoWebUI/Pages/Profile.razor.cs`, add:

```csharp
private static readonly string[] GreenDefaultSelectedDifficultyStrings =
{
    "None", "Easy", "Normal", "Hard", "Oni"
};
```

- [ ] **Step 4: Add the Green-only select**

In `TaikoWebUI/Pages/Profile.razor`, inside the Green branch near `GreenDispLevelChassis`, render:

```razor
<MudStack Spacing="4">
    <MudSelect @bind-Value="@response.GreenDispLevelChassis" Label=@Localizer["Local Ranking Difficulty"] AnchorOrigin="Origin.BottomCenter">
        @for (uint i = 0; i < GreenLocalRankingDifficultyStrings.Length; i++)
        {
            var index = i;
            <MudSelectItem Value="@i">@Localizer[GreenLocalRankingDifficultyStrings[index]]</MudSelectItem>
        }
    </MudSelect>

    <MudSelect @bind-Value="@response.GreenDispLevelSelf" Label=@Localizer["Default Selected Difficulty"] AnchorOrigin="Origin.BottomCenter">
        @for (uint i = 0; i < GreenDefaultSelectedDifficultyStrings.Length; i++)
        {
            var index = i;
            <MudSelectItem Value="@i">@Localizer[GreenDefaultSelectedDifficultyStrings[index]]</MudSelectItem>
        }
    </MudSelect>
</MudStack>
```

Keep it inside the existing `else` branch so it is Green-only.

- [ ] **Step 5: Run focused Web UI tests and commit**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenCustomizationWebUiTests.Profile_RendersGreenUserdataSettingsOnlyForGreen"
```

Expected: PASS.

Commit:

```powershell
git add -- TaikoWebUI/Pages/Profile.razor TaikoWebUI/Pages/Profile.razor.cs Tests/WebUi/GreenCustomizationWebUiTests.cs
git commit -m "Add Green default selected difficulty control"
```

## Task 3: Verification

**Files:**

- Inspect: `Application/Handlers/UserDataQuery.Green.cs`
- Inspect: `Adapters.GameProtocol.Green/Mappers/UserDataMappers.cs`
- Test: `Tests/Green/GreenIdentityHandlerTests.cs`
- Test: `Tests/Green/GreenUserDataMapperTests.cs`

- [ ] **Step 1: Confirm existing userdata response coverage**

Check that `Tests/Green/GreenIdentityHandlerTests.cs` test
`UserData_Green_ReturnsPersistedTojiruAndDisplayLevels` asserts:

```csharp
save.DispLevelSelf = 4;
Assert.Equal(4u, response.DispLevelSelf);
```

Check that `Tests/Green/GreenUserDataMapperTests.cs` asserts:

```csharp
Assert.True(response.ShouldSerializeDispLevelSelf());
Assert.Equal(4u, response.DispLevelSelf);
```

If either assertion is missing, add it to the existing focused test instead of creating broad new fixtures.

- [ ] **Step 2: Run focused Green userdata tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenIdentityHandlerTests|FullyQualifiedName~GreenUserDataMapperTests"
```

Expected: PASS.

- [ ] **Step 3: Run combined focused test slice**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenAdminApiControllerTests.UserSettings_Green|FullyQualifiedName~GreenCustomizationWebUiTests.Profile_RendersGreenUserdataSettingsOnlyForGreen|FullyQualifiedName~GreenIdentityHandlerTests|FullyQualifiedName~GreenUserDataMapperTests"
```

Expected: PASS.

- [ ] **Step 4: Run build**

Run:

```powershell
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-disp-level-self"
```

Expected: PASS. Use the temp output path to avoid locked `Host/bin` outputs.

- [ ] **Step 5: Final status**

Run:

```powershell
git status --short
```

Expected: only unrelated pre-existing workspace changes remain, or no changes remain if implementation commits included all touched files.

## Self-Review

- Spec coverage: every requirement maps to Task 1, Task 2, or Task 3.
- Placeholder scan: no placeholder markers or unspecified implementation steps remain.
- Scope check: one Green-only settings feature, no migration, no generated-code changes, and no cross-era contract expansion.
- Ambiguity check: the value domain is explicit as `0..4`; `0` means no default selected difficulty.
