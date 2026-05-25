# Stage 3: Green Web UI Settings

## Goal

Add Green-only Web UI controls for the folder-close button and local machine ranking difficulty without exposing self, total, or filtered-folder fields.

## Files

- Modify: `TaikoWebUI/Pages/Profile.razor`
- Modify: `TaikoWebUI/Pages/Profile.razor.cs`
- Modify: `Tests/WebUi/GreenCustomizationWebUiTests.cs`

## Steps

- [ ] **Step 1: Add failing Web UI markup coverage**

Add this test to `Tests/WebUi/GreenCustomizationWebUiTests.cs` after `Profile_RendersGreenDanDisplaySwitchWithoutMovingNijiiroSwitch`:

```csharp
[Fact]
public void Profile_RendersGreenUserdataSettingsOnlyForGreen()
{
    var markup = ReadWebUiFile("Pages", "Profile.razor");
    var normalized = NormalizeLineEndings(markup);
    var code = ReadWebUiFile("Pages", "Profile.razor.cs");
    var achievementIndex = normalized.IndexOf("Achievement Panel Difficulty", StringComparison.Ordinal);
    Assert.True(achievementIndex >= 0, "Could not find the Nijiiro profile settings branch.");

    var greenBranchStart = normalized.IndexOf("else\n                                {", achievementIndex, StringComparison.Ordinal);
    Assert.True(greenBranchStart >= 0, "Could not find the Green profile settings branch.");

    var nextTabStart = normalized.IndexOf("</MudStack>\n                        </MudTabPanel>", greenBranchStart, StringComparison.Ordinal);
    Assert.True(nextTabStart > greenBranchStart, "Could not find the end of the Green profile settings branch.");
    var greenBranch = normalized[greenBranchStart..nextTabStart];

    Assert.Contains("@bind-Value=\"@response.GreenIsTojiru\"", greenBranch);
    Assert.Contains("Show Folder Close Button", greenBranch);
    Assert.Contains("@bind-Value=\"@response.GreenDispLevelChassis\"", greenBranch);
    Assert.Contains("Local Ranking Difficulty", greenBranch);
    Assert.Contains("GreenLocalRankingDifficultyStrings", code);

    Assert.DoesNotContain("disp_level_self", markup, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("disp_level_total", markup, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("DifficultyPlayedCourse", markup);
    Assert.DoesNotContain("DifficultyPlayedStar", markup);
}
```

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenCustomizationWebUiTests.Profile_RendersGreenUserdataSettingsOnlyForGreen"
```

Expected: the test fails because the markup and labels do not exist yet.

- [ ] **Step 2: Add Green ranking difficulty labels**

In `TaikoWebUI/Pages/Profile.razor.cs`, add this array after `DifficultySettingCourseStrings`:

```csharp
private static readonly string[] GreenLocalRankingDifficultyStrings =
{
    "No Fixed Course", "Easy", "Normal", "Hard", "Oni"
};
```

- [ ] **Step 3: Render Green-only controls**

In `TaikoWebUI/Pages/Profile.razor`, replace the current Green `else` branch under the profile options tab:

```razor
else
{
    <MudSwitch @bind-Value="@response.IsDisplayDanOnNamePlate" Label=@Localizer["Display Dan Rank on Name Plate"] Color="Color.Primary" />
}
```

with:

```razor
else
{
    <MudGrid>
        <MudItem xs="12" md="4">
            <MudStack Spacing="4">
                <MudSwitch @bind-Value="@response.IsDisplayDanOnNamePlate" Label=@Localizer["Display Dan Rank on Name Plate"] Color="Color.Primary" />
                <MudSwitch @bind-Value="@response.GreenIsTojiru" Label=@Localizer["Show Folder Close Button"] Color="Color.Primary" />
            </MudStack>
        </MudItem>
        <MudItem xs="12" md="8">
            <MudSelect @bind-Value="@response.GreenDispLevelChassis" Label=@Localizer["Local Ranking Difficulty"] AnchorOrigin="Origin.BottomCenter">
                @for (uint i = 0; i < GreenLocalRankingDifficultyStrings.Length; i++)
                {
                    var index = i;
                    <MudSelectItem Value="@i">@Localizer[GreenLocalRankingDifficultyStrings[index]]</MudSelectItem>
                }
            </MudSelect>
        </MudItem>
    </MudGrid>
}
```

- [ ] **Step 4: Run focused Web UI tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenCustomizationWebUiTests.Profile_RendersGreenUserdataSettingsOnlyForGreen|FullyQualifiedName~GreenCustomizationWebUiTests.Profile_RendersGreenDanDisplaySwitchWithoutMovingNijiiroSwitch"
```

Expected: selected tests pass.

- [ ] **Step 5: Build the Web UI project**

Run:

```powershell
dotnet build TaikoWebUI/TaikoWebUI.csproj
```

Expected: build succeeds.

- [ ] **Step 6: Commit Stage 3**

Run:

```powershell
git status --short
git add -- TaikoWebUI/Pages/Profile.razor TaikoWebUI/Pages/Profile.razor.cs Tests/WebUi/GreenCustomizationWebUiTests.cs
git diff --cached --name-status
git commit -m "Add Green userdata settings to Web UI"
```

Expected staged files: only the three files listed above.
