# AC15 Profile Settings Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a strict AC15-only profile settings API and WebUI path while leaving Nijiiro/AC16 `UserSetting` profile editing unchanged.

**Architecture:** AC15 profile settings move to a new `Ac15ProfileSettingsDto` contract with optional typed option groups and slot-based customization. `Ac15EraProfile` owns `Ac15ProfileCapabilities`; AdminApi controller partials bind concrete AC15 save rows and Dan tables, then call a shared Application service over narrow save-data interfaces. The WebUI branches once by era and renders AC15 controls from returned groups and slots instead of Green-shaped `UserSetting` fields.

**Tech Stack:** ASP.NET Core 10 controllers, EF Core/SQLite, System.Text.Json endpoint-local unmapped-member rejection, Blazor WebAssembly, MudBlazor, xUnit.

---

## Scope Check

This spec is one subsystem: AC15 profile settings across AdminApi, Application, contracts, and the paired WebUI. It should stay as one plan because each task builds toward one testable vertical API/UI contract. The plan keeps Nijiiro `UserSetting` behavior intact and does not add Mapperly mappings; if an executor decides to introduce a Mapperly mapper anyway, they must read current Mapperly docs and inspect generated source as required by `AGENTS.md`.

## File Structure

- Create `Contracts.AdminApi/Ac15ProfileSettings/Ac15ProfileSettingsDto.cs`: AC15 read/update contracts and endpoint-local JSON unknown-field rejection attributes on update records.
- Modify `TaikoWebUI/GlobalUsings.cs`, `TaikoWebUI/_Imports.razor`, and `Tests/GlobalUsings.cs`: make the new contract namespace visible where DTOs are consumed.
- Create `Application/Ac15/Ac15ProfileCapabilities.cs`: application-owned capability metadata with DTO projection.
- Modify `Application/Ac15/Ac15EraProfile.cs` and `Application/Ac15/Ac15EraProfiles.cs`: attach profile capabilities to each implemented AC15 era.
- Modify `Domain/Entities/IAc15SaveDataCapabilities.cs` and AC15 save entities: add `IAc15ProfileSettingsSaveData` and bind Blue, Green, Yellow, and Red save rows through property interfaces instead of the delegate-heavy accessor.
- Create `Application/Ac15/Ac15ProfileSettingsService.cs`: read and save AC15 profile settings through capability metadata, strict validation, slot helpers, and existing bitset semantics.
- Create `Adapters.AdminApi/Controllers/Ac15ProfileSettingsController.cs` plus `.Blue.cs`, `.Green.cs`, `.Yellow.cs`, `.Red.cs`: new `GET/PUT /api/{era}/Ac15ProfileSettings/{baid}` route family.
- Modify `Adapters.AdminApi/Controllers/UserSettingsController.cs` and `UserSettingsController.Nijiiro.cs`; delete AC15 `UserSettingsController.*.cs` partials after the WebUI stops using them.
- Delete `Application/Ac15/Ac15UserSettingsAccess.cs` and `Application/Ac15/Ac15UserSettingsService.cs` after all callers move to `Ac15ProfileSettingsService`.
- Modify `Contracts.AdminApi/ViewModels/UserSetting.cs`: remove AC15-only `Green*` and tutorial/display difficulty fields after AC15 WebUI/API migration.
- Create `TaikoWebUI/Shared/Customize/PlayerPreviewModel.cs` and modify `PlayerPreview.razor`: preview consumes a small shared preview model instead of the mixed `UserSetting`.
- Create `TaikoWebUI/Pages/ProfileEditor/Ac15ProfileEditorState.cs` and `Ac15ProfileEditor.razor`: AC15 editor state and rendering driven by DTO group presence.
- Create `TaikoWebUI/Services/ProfileSettingsHttpClient.cs`: WebUI helper for AC15 versus Nijiiro profile identity fetches used by non-profile pages.
- Modify `TaikoWebUI/Pages/Profile.razor` and `.cs`: branch to the AC15 editor and new `PUT` endpoint for AC15; keep existing Nijiiro controls on `UserSetting`.
- Modify `TaikoWebUI/Pages/HighScores.razor.cs`, `PlayHistory.razor.cs`, `SongList.razor.cs`, `Song.razor.cs`, and `DaniDojo.razor.cs`: fetch AC15 display identity through `Ac15ProfileSettings` instead of `UserSettings`.
- Tests:
  - Create `Tests/Ac15/Ac15ProfileCapabilitiesTests.cs`.
  - Create `Tests/Ac15/Ac15ProfileSettingsServiceTests.cs`.
  - Create `Tests/Ac15/Ac15ProfileSettingsControllerTests.cs`.
  - Create `Tests/WebUi/Ac15ProfileSettingsWebUiTests.cs`.
  - Update Green/Blue/Yellow/Red AdminApi profile tests that currently target `UserSettings` to target `Ac15ProfileSettings`.

## Task 1: Contracts And Capability Metadata

**Files:**
- Create: `Contracts.AdminApi/Ac15ProfileSettings/Ac15ProfileSettingsDto.cs`
- Create: `Application/Ac15/Ac15ProfileCapabilities.cs`
- Modify: `Application/Ac15/Ac15EraProfile.cs`
- Modify: `Application/Ac15/Ac15EraProfiles.cs`
- Modify: `TaikoWebUI/GlobalUsings.cs`
- Modify: `TaikoWebUI/_Imports.razor`
- Modify: `Tests/GlobalUsings.cs`
- Test: `Tests/Ac15/Ac15ProfileCapabilitiesTests.cs`

- [ ] **Step 1: Write the failing capability and JSON contract tests**

Create `Tests/Ac15/Ac15ProfileCapabilitiesTests.cs`:

```csharp
using System.Text.Json;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15ProfileCapabilitiesTests
{
    [Fact]
    public void CurrentImplementedAc15ProfilesExposeFullProfileSettingsCapabilities()
    {
        var profiles = new[] { Ac15EraProfiles.Blue, Ac15EraProfiles.Green, Ac15EraProfiles.Yellow, Ac15EraProfiles.Red };

        foreach (var profile in profiles)
        {
            Assert.Equal(["kigurumi", "head", "body", "face", "puchi"], profile.ProfileCapabilities.CostumeSlots);
            Assert.True(profile.ProfileCapabilities.SupportsTitle);
            Assert.True(profile.ProfileCapabilities.SupportsTone);
            Assert.True(profile.ProfileCapabilities.SupportsColors);
            Assert.True(profile.ProfileCapabilities.SupportsDisplayDanOnNamePlate);
            Assert.True(profile.ProfileCapabilities.SupportsFolderCloseButton);
            Assert.True(profile.ProfileCapabilities.SupportsAutoCostume);
            Assert.True(profile.ProfileCapabilities.SupportsHowToPlayTutorialFlag);
            Assert.True(profile.ProfileCapabilities.SupportsLocalRankingDifficulty);
            Assert.True(profile.ProfileCapabilities.SupportsDefaultSelectedSelfBestDifficulty);
            Assert.True(profile.ProfileCapabilities.SupportsTaikojukuFolderDan);
        }
    }

    [Fact]
    public void CapabilityDtoCanRepresentTitleOnlyOlderEra()
    {
        var capabilities = new Ac15ProfileCapabilities(
            CostumeSlots: [],
            SupportsTitle: true,
            SupportsTone: false,
            SupportsColors: false,
            SupportsDisplayDanOnNamePlate: true,
            SupportsFolderCloseButton: false,
            SupportsAutoCostume: false,
            SupportsHowToPlayTutorialFlag: false,
            SupportsLocalRankingDifficulty: false,
            SupportsDefaultSelectedSelfBestDifficulty: false,
            SupportsTaikojukuFolderDan: false);

        var dto = capabilities.ToDto();

        Assert.Empty(dto.CostumeSlots);
        Assert.True(dto.SupportsTitle);
        Assert.False(dto.SupportsTone);
        Assert.True(dto.SupportsDisplayDanOnNamePlate);
    }

    [Fact]
    public void UpdateDtoRejectsUnknownJsonFieldsLocally()
    {
        const string json = """
        {
          "identity": { "myDonName": "DON", "myDonNameLanguage": 0 },
          "customization": null,
          "options": {},
          "unexpectedField": 1
        }
        """;

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Ac15ProfileSettingsUpdateDto>(json, options));
    }
}
```

- [ ] **Step 2: Run the failing tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ProfileCapabilitiesTests"
```

Expected: FAIL because `Ac15ProfileCapabilities`, `ProfileCapabilities`, and `Ac15ProfileSettingsUpdateDto` do not exist.

- [ ] **Step 3: Add the AC15 AdminApi contracts**

Create `Contracts.AdminApi/Ac15ProfileSettings/Ac15ProfileSettingsDto.cs`:

```csharp
using System.Text.Json.Serialization;

namespace TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;

public sealed record Ac15ProfileSettingsDto(
    string Era,
    uint Baid,
    Ac15ProfileIdentityDto Identity,
    Ac15CustomizationDto? Customization,
    Ac15ProfileOptionGroupsDto Options,
    Ac15ProfileCapabilitiesDto Capabilities,
    DateTime LastPlayDateTime);

public sealed record Ac15ProfileIdentityDto(
    string MyDonName,
    uint MyDonNameLanguage);

public sealed record Ac15ProfileOptionGroupsDto(
    Ac15NamePlateOptionsDto? NamePlate,
    Ac15FolderOptionsDto? Folder,
    Ac15SongSelectOptionsDto? SongSelect,
    Ac15TaikojukuOptionsDto? Taikojuku,
    Ac15TutorialOptionsDto? Tutorials,
    Ac15CustomizationBehaviorOptionsDto? CustomizationBehavior);

public sealed record Ac15NamePlateOptionsDto(
    bool DisplayDanOnNamePlate);

public sealed record Ac15FolderOptionsDto(
    bool ShowFolderCloseButton);

public sealed record Ac15SongSelectOptionsDto(
    uint? LocalRankingDifficulty,
    uint? DefaultSelectedAndSelfBestDifficulty);

public sealed record Ac15TaikojukuOptionsDto(
    uint FolderDan,
    IReadOnlyList<uint> SelectableFolderDans);

public sealed record Ac15TutorialOptionsDto(
    bool? DisableHowToPlayTutorial);

public sealed record Ac15CustomizationBehaviorOptionsDto(
    bool ApplyCostumeChangesFromPlayResults);

public sealed record Ac15CustomizationDto(
    IReadOnlyList<Ac15CostumeSlotDto> CostumeSlots,
    Ac15TitleSelectionDto? Title,
    Ac15ToneSelectionDto? Tone,
    Ac15CostumeColorsDto? Colors);

public sealed record Ac15CostumeSlotDto(
    string Slot,
    uint CurrentId,
    IReadOnlyList<uint> UnlockedIds);

public sealed record Ac15TitleSelectionDto(
    string TitleText,
    uint TitleId,
    IReadOnlyList<uint> UnlockedTitleIds);

public sealed record Ac15ToneSelectionDto(
    uint ToneId,
    IReadOnlyList<uint> UnlockedToneIds);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record Ac15CostumeColorsDto(
    uint BodyColor,
    uint FaceColor,
    uint LimbColor);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record Ac15ProfileSettingsUpdateDto(
    Ac15ProfileIdentityDto Identity,
    Ac15CustomizationUpdateDto? Customization,
    Ac15ProfileOptionGroupsUpdateDto Options);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record Ac15ProfileOptionGroupsUpdateDto(
    Ac15NamePlateOptionsDto? NamePlate,
    Ac15FolderOptionsDto? Folder,
    Ac15SongSelectOptionsDto? SongSelect,
    Ac15TaikojukuFolderDanUpdateDto? Taikojuku,
    Ac15TutorialOptionsDto? Tutorials,
    Ac15CustomizationBehaviorOptionsDto? CustomizationBehavior);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record Ac15TaikojukuFolderDanUpdateDto(
    uint FolderDan);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record Ac15CustomizationUpdateDto(
    IReadOnlyList<Ac15CostumeSlotUpdateDto>? CostumeSlots,
    Ac15TitleSelectionUpdateDto? Title,
    Ac15ToneSelectionUpdateDto? Tone,
    Ac15CostumeColorsDto? Colors);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record Ac15CostumeSlotUpdateDto(
    string Slot,
    uint CurrentId,
    IReadOnlyList<uint>? UnlockedIds);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record Ac15TitleSelectionUpdateDto(
    string TitleText,
    uint TitleId,
    IReadOnlyList<uint>? UnlockedTitleIds);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record Ac15ToneSelectionUpdateDto(
    uint ToneId,
    IReadOnlyList<uint>? UnlockedToneIds);

public sealed record Ac15ProfileCapabilitiesDto(
    IReadOnlyList<string> CostumeSlots,
    bool SupportsTitle,
    bool SupportsTone,
    bool SupportsColors,
    bool SupportsDisplayDanOnNamePlate,
    bool SupportsFolderCloseButton,
    bool SupportsAutoCostume,
    bool SupportsHowToPlayTutorialFlag,
    bool SupportsLocalRankingDifficulty,
    bool SupportsDefaultSelectedSelfBestDifficulty,
    bool SupportsTaikojukuFolderDan);
```

- [ ] **Step 4: Add application profile capability metadata**

Create `Application/Ac15/Ac15ProfileCapabilities.cs`:

```csharp
using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;

namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15ProfileCapabilities(
    IReadOnlyList<string> CostumeSlots,
    bool SupportsTitle,
    bool SupportsTone,
    bool SupportsColors,
    bool SupportsDisplayDanOnNamePlate,
    bool SupportsFolderCloseButton,
    bool SupportsAutoCostume,
    bool SupportsHowToPlayTutorialFlag,
    bool SupportsLocalRankingDifficulty,
    bool SupportsDefaultSelectedSelfBestDifficulty,
    bool SupportsTaikojukuFolderDan)
{
    public static Ac15ProfileCapabilities CurrentFull { get; } = new(
        CostumeSlots: ["kigurumi", "head", "body", "face", "puchi"],
        SupportsTitle: true,
        SupportsTone: true,
        SupportsColors: true,
        SupportsDisplayDanOnNamePlate: true,
        SupportsFolderCloseButton: true,
        SupportsAutoCostume: true,
        SupportsHowToPlayTutorialFlag: true,
        SupportsLocalRankingDifficulty: true,
        SupportsDefaultSelectedSelfBestDifficulty: true,
        SupportsTaikojukuFolderDan: true);

    public Ac15ProfileCapabilitiesDto ToDto()
        => new(
            CostumeSlots,
            SupportsTitle,
            SupportsTone,
            SupportsColors,
            SupportsDisplayDanOnNamePlate,
            SupportsFolderCloseButton,
            SupportsAutoCostume,
            SupportsHowToPlayTutorialFlag,
            SupportsLocalRankingDifficulty,
            SupportsDefaultSelectedSelfBestDifficulty,
            SupportsTaikojukuFolderDan);
}
```

Replace `Application/Ac15/Ac15EraProfile.cs` with:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15EraProfile(
    GameEra Era,
    Ac15FeatureSet Features,
    Ac15ProtocolLimits Limits,
    Ac15WirePlacement WirePlacement,
    Ac15ProfileCapabilities ProfileCapabilities);
```

In `Application/Ac15/Ac15EraProfiles.cs`, add `Ac15ProfileCapabilities.CurrentFull` as the final constructor argument for `Blue`, `Green`, `Yellow`, and `Red`. The Blue constructor should end like this:

```csharp
        new Ac15WirePlacement(
            CrownPlacement: Ac15CrownWirePlacement.DedicatedEndpoint,
            HasInitialDataItemShopRows: true,
            HasInitialDataLegalTermsRows: true,
            HasTokkunTutorialFlagInUserData: true),
        Ac15ProfileCapabilities.CurrentFull);
```

Apply the same `Ac15ProfileCapabilities.CurrentFull` final argument to the Green, Yellow, and Red profile constructors.

- [ ] **Step 5: Add global imports for consumers**

Append this line to `TaikoWebUI/GlobalUsings.cs`:

```csharp
global using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;
```

Append this line to `Tests/GlobalUsings.cs`:

```csharp
global using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;
```

Append this line to `TaikoWebUI/_Imports.razor`:

```razor
@using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings
```

- [ ] **Step 6: Run contract tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ProfileCapabilitiesTests"
```

Expected: PASS.

- [ ] **Step 7: Commit contracts and capabilities**

Run:

```powershell
git add Contracts.AdminApi/Ac15ProfileSettings/Ac15ProfileSettingsDto.cs Application/Ac15/Ac15ProfileCapabilities.cs Application/Ac15/Ac15EraProfile.cs Application/Ac15/Ac15EraProfiles.cs TaikoWebUI/GlobalUsings.cs TaikoWebUI/_Imports.razor Tests/GlobalUsings.cs Tests/Ac15/Ac15ProfileCapabilitiesTests.cs
git commit -m "Add AC15 profile settings contracts"
```

## Task 2: Save-Data Interface And Read Service

**Files:**
- Modify: `Domain/Entities/IAc15SaveDataCapabilities.cs`
- Modify: `Domain/Entities/UserSaveDataBlue.cs`
- Modify: `Domain/Entities/UserSaveDataGreen.cs`
- Modify: `Domain/Entities/UserSaveDataYellow.cs`
- Modify: `Domain/Entities/UserSaveDataRed.cs`
- Create: `Application/Ac15/Ac15ProfileSettingsService.cs`
- Test: `Tests/Ac15/Ac15ProfileSettingsServiceTests.cs`

- [ ] **Step 1: Write failing read-service tests**

Create `Tests/Ac15/Ac15ProfileSettingsServiceTests.cs`:

```csharp
namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15ProfileSettingsServiceTests
{
    [Fact]
    public async Task GetAsync_ReturnsFullCurrentGroupsAndSlotBasedCustomization()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        var user = new UserDatum { Baid = 1, MyDonName = "DON", MyDonNameLanguage = 2 };
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        save.Title = "Title";
        save.TitleplateId = 10;
        save.DefaultToneSetting = 4;
        save.Costume1 = 12;
        save.CostumeFlg1 = Ac15ProtocolBytes.SetBits(save.CostumeFlg1, [12, 13], Ac15EraProfiles.Blue.Limits.CostumeFlagBytes);
        save.TitleFlg = Ac15ProtocolBytes.SetBits(save.TitleFlg, [10], Ac15EraProfiles.Blue.Limits.TitleFlagBytes);
        save.ToneFlg = Ac15ProtocolBytes.SetBits(save.ToneFlg, [0, 4], Ac15EraProfiles.Blue.Limits.ToneFlagBytes);
        save.ColorBody = 2;
        save.ColorFace = 3;
        save.ColorLimb = 4;
        save.DispDanType = 1;
        save.IsTojiru = true;
        save.IsAutoCostumeOn = false;
        save.IsExplain = true;
        save.DispLevelChassis = 3;
        save.DispLevelSelf = 2;
        save.DispTaikojukuDan = 5;
        database.Context.UserData.Add(user);
        database.Context.UserSaveDataBlue.Add(save);
        database.Context.DanScoreDataBlue.Add(new DanScoreDatumBlue
        {
            Baid = 1,
            DanId = 5,
            IsExtra = false,
            ClearGrade = Ac15DanClearGrade.NormalClear
        });
        await database.Context.SaveChangesAsync();

        var result = await Ac15ProfileSettingsService.GetAsync<UserSaveDataBlue, DanScoreDatumBlue>(
            user,
            save,
            database.Context.DanScoreDataBlue,
            Ac15EraProfiles.Blue,
            CancellationToken.None);

        Assert.Equal(Ac15ProfileSettingsResultStatus.Success, result.Status);
        var setting = result.Setting!;
        Assert.Equal("Blue", setting.Era);
        Assert.Equal(1u, setting.Baid);
        Assert.Equal("DON", setting.Identity.MyDonName);
        Assert.Equal(2u, setting.Identity.MyDonNameLanguage);
        Assert.NotNull(setting.Customization);
        var kigurumi = Assert.Single(setting.Customization!.CostumeSlots, slot => slot.Slot == "kigurumi");
        Assert.Equal(12u, kigurumi.CurrentId);
        Assert.Contains(13u, kigurumi.UnlockedIds);
        Assert.Equal("Title", setting.Customization.Title!.TitleText);
        Assert.Equal(10u, setting.Customization.Title.TitleId);
        Assert.Equal(4u, setting.Customization.Tone!.ToneId);
        Assert.Equal(2u, setting.Customization.Colors!.BodyColor);
        Assert.True(setting.Options.NamePlate!.DisplayDanOnNamePlate);
        Assert.True(setting.Options.Folder!.ShowFolderCloseButton);
        Assert.False(setting.Options.CustomizationBehavior!.ApplyCostumeChangesFromPlayResults);
        Assert.True(setting.Options.Tutorials!.DisableHowToPlayTutorial);
        Assert.Equal(3u, setting.Options.SongSelect!.LocalRankingDifficulty);
        Assert.Equal(2u, setting.Options.SongSelect.DefaultSelectedAndSelfBestDifficulty);
        Assert.DoesNotContain(5u, setting.Options.Taikojuku!.SelectableFolderDans);
    }

    [Fact]
    public async Task GetAsync_OmitsUnsupportedGroupsFromOlderCapabilityProfile()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        var user = new UserDatum { Baid = 1, MyDonName = "OLDER" };
        var save = UserSaveDataRedExtensions.CreateDefaultRedSaveData(1);
        save.Title = "Older Title";
        save.TitleplateId = 10;
        database.Context.UserData.Add(user);
        database.Context.UserSaveDataRed.Add(save);
        await database.Context.SaveChangesAsync();
        var profile = Ac15EraProfiles.Red with
        {
            ProfileCapabilities = new Ac15ProfileCapabilities(
                CostumeSlots: [],
                SupportsTitle: true,
                SupportsTone: false,
                SupportsColors: false,
                SupportsDisplayDanOnNamePlate: true,
                SupportsFolderCloseButton: false,
                SupportsAutoCostume: false,
                SupportsHowToPlayTutorialFlag: false,
                SupportsLocalRankingDifficulty: false,
                SupportsDefaultSelectedSelfBestDifficulty: false,
                SupportsTaikojukuFolderDan: false)
        };

        var result = await Ac15ProfileSettingsService.GetAsync<UserSaveDataRed, DanScoreDatumRed>(
            user,
            save,
            null,
            profile,
            CancellationToken.None);

        var setting = result.Setting!;
        Assert.NotNull(setting.Customization);
        Assert.Empty(setting.Customization!.CostumeSlots);
        Assert.NotNull(setting.Customization.Title);
        Assert.Null(setting.Customization.Tone);
        Assert.Null(setting.Customization.Colors);
        Assert.NotNull(setting.Options.NamePlate);
        Assert.Null(setting.Options.Folder);
        Assert.Null(setting.Options.SongSelect);
        Assert.Null(setting.Options.Taikojuku);
        Assert.Null(setting.Options.Tutorials);
        Assert.Null(setting.Options.CustomizationBehavior);
    }

    private sealed class SchemaDatabase(SqliteConnection connection) : IAsyncDisposable
    {
        public TaikoDbContext Context { get; } = CreateContext(connection);

        public static async Task<SchemaDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var database = new SchemaDatabase(connection);
            await database.Context.Database.EnsureCreatedAsync();
            return database;
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await connection.DisposeAsync();
        }

        private static TaikoDbContext CreateContext(SqliteConnection connection)
            => new(new DbContextOptionsBuilder<TaikoDbContext>()
                .UseSqlite(connection)
                .Options);
    }
}
```

- [ ] **Step 2: Run the failing read-service tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ProfileSettingsServiceTests"
```

Expected: FAIL because `IAc15ProfileSettingsSaveData`, `Ac15ProfileSettingsService`, and `Ac15ProfileSettingsResultStatus` do not exist.

- [ ] **Step 3: Add the narrow save-data interface**

Append this interface to `Domain/Entities/IAc15SaveDataCapabilities.cs`:

```csharp
public interface IAc15ProfileSettingsSaveData :
    IAc15CustomizationSaveData,
    IAc15PlayProfileSaveData
{
    string Title { get; set; }
    uint TitleplateId { get; set; }
    uint DefaultToneSetting { get; set; }
    uint ColorBody { get; set; }
    uint ColorFace { get; set; }
    uint ColorLimb { get; set; }
    uint DispDanType { get; set; }
    uint DispTaikojukuDan { get; set; }
    bool IsTojiru { get; set; }
    bool IsExplain { get; set; }
    uint DispLevelChassis { get; set; }
    uint DispLevelSelf { get; set; }
}
```

Update the implemented AC15 save entities:

```csharp
// Domain/Entities/UserSaveDataBlue.cs
public partial class UserSaveDataBlue :
    IAc15MedalSaveData,
    IAc15TutorialSaveData,
    IAc15ProfileSettingsSaveData,
    IAc15SongUnlockSaveData
```

```csharp
// Domain/Entities/UserSaveDataGreen.cs
public partial class UserSaveDataGreen :
    IAc15MedalSaveData,
    IAc15TutorialSaveData,
    IAc15ProfileSettingsSaveData
```

```csharp
// Domain/Entities/UserSaveDataYellow.cs
public partial class UserSaveDataYellow :
    IAc15MedalSaveData,
    IAc15TutorialSaveData,
    IAc15ProfileSettingsSaveData,
    IAc15SongUnlockSaveData
```

```csharp
// Domain/Entities/UserSaveDataRed.cs
public partial class UserSaveDataRed :
    IAc15DonPointSaveData,
    IAc15PlayTutorialSaveData,
    IAc15ProfileSettingsSaveData,
    IAc15SongUnlockSaveData
```

- [ ] **Step 4: Add the read side of `Ac15ProfileSettingsService`**

Create `Application/Ac15/Ac15ProfileSettingsService.cs` with the result type and read path:

```csharp
using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;

namespace TaikoLocalServer.Application.Ac15;

public enum Ac15ProfileSettingsResultStatus
{
    Success,
    BadRequest
}

public sealed record Ac15ProfileSettingsResult(
    Ac15ProfileSettingsResultStatus Status,
    Ac15ProfileSettingsDto? Setting,
    string? ErrorMessage)
{
    public static Ac15ProfileSettingsResult Success(Ac15ProfileSettingsDto? setting = null)
        => new(Ac15ProfileSettingsResultStatus.Success, setting, null);

    public static Ac15ProfileSettingsResult BadRequest(string message)
        => new(Ac15ProfileSettingsResultStatus.BadRequest, null, message);
}

public sealed record Ac15ProfileEditPolicy(bool AllowFreeProfileEditing);

public static class Ac15ProfileSettingsService
{
    private static readonly string[] KnownCostumeSlots = ["kigurumi", "head", "body", "face", "puchi"];

    public static async ValueTask<Ac15ProfileSettingsResult> GetAsync<TSave, TDanScore>(
        UserDatum user,
        TSave saveData,
        DbSet<TDanScore>? danScores,
        Ac15EraProfile profile,
        CancellationToken cancellationToken)
        where TSave : class, IAc15ProfileSettingsSaveData
        where TDanScore : class, IAc15DanScoreDatum
    {
        var capabilities = profile.ProfileCapabilities;
        var selectableTaikojukuDans = capabilities.SupportsTaikojukuFolderDan && danScores is not null
            ? await GetSelectableTaikojukuFolderDans(danScores, user.Baid, profile.Limits, cancellationToken)
            : [];
        var taikojukuDan = capabilities.SupportsTaikojukuFolderDan
            ? SelectTaikojukuFolderDan(selectableTaikojukuDans, saveData.DispTaikojukuDan, profile.Limits)
            : 0;

        var dto = new Ac15ProfileSettingsDto(
            Era: profile.Era.ToString(),
            Baid: user.Baid,
            Identity: new Ac15ProfileIdentityDto(user.MyDonName, user.MyDonNameLanguage),
            Customization: BuildCustomization(saveData, profile),
            Options: BuildOptions(saveData, capabilities, taikojukuDan, selectableTaikojukuDans),
            Capabilities: capabilities.ToDto(),
            LastPlayDateTime: saveData.LastPlayDatetime);

        return Ac15ProfileSettingsResult.Success(dto);
    }

    private static Ac15CustomizationDto? BuildCustomization<TSave>(TSave saveData, Ac15EraProfile profile)
        where TSave : IAc15ProfileSettingsSaveData
    {
        var capabilities = profile.ProfileCapabilities;
        if (capabilities.CostumeSlots.Count == 0
            && !capabilities.SupportsTitle
            && !capabilities.SupportsTone
            && !capabilities.SupportsColors)
        {
            return null;
        }

        var slots = capabilities.CostumeSlots
            .Where(slot => KnownCostumeSlots.Contains(slot, StringComparer.Ordinal))
            .Select(slot => new Ac15CostumeSlotDto(
                slot,
                GetCostume(saveData, slot),
                BitsetCodec.Decode(GetCostumeFlag(saveData, slot), profile.Limits.CostumeFlagBytes)))
            .ToArray();

        return new Ac15CustomizationDto(
            CostumeSlots: slots,
            Title: capabilities.SupportsTitle
                ? new Ac15TitleSelectionDto(
                    saveData.Title,
                    saveData.TitleplateId,
                    BitsetCodec.Decode(saveData.TitleFlg, profile.Limits.TitleFlagBytes))
                : null,
            Tone: capabilities.SupportsTone
                ? new Ac15ToneSelectionDto(
                    saveData.DefaultToneSetting,
                    BitsetCodec.Decode(saveData.ToneFlg, profile.Limits.ToneFlagBytes))
                : null,
            Colors: capabilities.SupportsColors
                ? new Ac15CostumeColorsDto(saveData.ColorBody, saveData.ColorFace, saveData.ColorLimb)
                : null);
    }

    private static Ac15ProfileOptionGroupsDto BuildOptions<TSave>(
        TSave saveData,
        Ac15ProfileCapabilities capabilities,
        uint taikojukuDan,
        IReadOnlyList<uint> selectableTaikojukuDans)
        where TSave : IAc15ProfileSettingsSaveData
        => new(
            NamePlate: capabilities.SupportsDisplayDanOnNamePlate
                ? new Ac15NamePlateOptionsDto(saveData.DispDanType != 0)
                : null,
            Folder: capabilities.SupportsFolderCloseButton
                ? new Ac15FolderOptionsDto(saveData.IsTojiru)
                : null,
            SongSelect: capabilities.SupportsLocalRankingDifficulty
                        || capabilities.SupportsDefaultSelectedSelfBestDifficulty
                ? new Ac15SongSelectOptionsDto(
                    capabilities.SupportsLocalRankingDifficulty ? SafeDisplayLevel(saveData.DispLevelChassis) : null,
                    capabilities.SupportsDefaultSelectedSelfBestDifficulty ? SafeDisplayLevel(saveData.DispLevelSelf) : null)
                : null,
            Taikojuku: capabilities.SupportsTaikojukuFolderDan
                ? new Ac15TaikojukuOptionsDto(taikojukuDan, selectableTaikojukuDans)
                : null,
            Tutorials: capabilities.SupportsHowToPlayTutorialFlag
                ? new Ac15TutorialOptionsDto(saveData.IsExplain)
                : null,
            CustomizationBehavior: capabilities.SupportsAutoCostume
                ? new Ac15CustomizationBehaviorOptionsDto(saveData.IsAutoCostumeOn)
                : null);

    private static async Task<List<uint>> GetSelectableTaikojukuFolderDans<TDanScore>(
        DbSet<TDanScore> danScores,
        uint baid,
        Ac15ProtocolLimits limits,
        CancellationToken cancellationToken)
        where TDanScore : class, IAc15DanScoreDatum
    {
        var clearGrades = await danScores
            .Where(row => row.Baid == baid && !row.IsExtra)
            .Select(row => new { row.DanId, row.ClearGrade })
            .ToListAsync(cancellationToken);
        var clearGradeMap = clearGrades.ToDictionary(row => row.DanId, row => row.ClearGrade);

        var selectable = new List<uint>();
        for (var danId = limits.MinNormalDanId; danId <= limits.MaxNormalDanId; danId++)
        {
            if (!clearGradeMap.TryGetValue(danId, out var grade) || !Ac15DanHelpers.IsClear(grade))
            {
                selectable.Add(danId);
            }
        }

        return selectable;
    }

    private static uint SelectTaikojukuFolderDan(
        IReadOnlyList<uint> selectableDans,
        uint requestedDan,
        Ac15ProtocolLimits limits)
        => selectableDans.Contains(requestedDan)
            ? requestedDan
            : selectableDans.FirstOrDefault(limits.MinNormalDanId);

    private static uint SafeDisplayLevel(uint value)
        => value <= 4 ? value : 0u;

    private static uint GetCostume<TSave>(TSave saveData, string slot)
        where TSave : IAc15CustomizationSaveData
        => slot switch
        {
            "kigurumi" => saveData.Costume1,
            "head" => saveData.Costume2,
            "body" => saveData.Costume3,
            "face" => saveData.Costume4,
            "puchi" => saveData.Costume5,
            _ => 0
        };

    private static byte[] GetCostumeFlag<TSave>(TSave saveData, string slot)
        where TSave : IAc15CustomizationSaveData
        => slot switch
        {
            "kigurumi" => saveData.CostumeFlg1,
            "head" => saveData.CostumeFlg2,
            "body" => saveData.CostumeFlg3,
            "face" => saveData.CostumeFlg4,
            "puchi" => saveData.CostumeFlg5,
            _ => []
        };
}
```

- [ ] **Step 5: Run read-service tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ProfileSettingsServiceTests"
```

Expected: PASS for both read tests.

- [ ] **Step 6: Commit read service**

Run:

```powershell
git add Domain/Entities/IAc15SaveDataCapabilities.cs Domain/Entities/UserSaveDataBlue.cs Domain/Entities/UserSaveDataGreen.cs Domain/Entities/UserSaveDataYellow.cs Domain/Entities/UserSaveDataRed.cs Application/Ac15/Ac15ProfileSettingsService.cs Tests/Ac15/Ac15ProfileSettingsServiceTests.cs
git commit -m "Add AC15 profile settings read service"
```

## Task 3: Strict Save Validation And Persistence

**Files:**
- Modify: `Application/Ac15/Ac15ProfileSettingsService.cs`
- Modify: `Tests/Ac15/Ac15ProfileSettingsServiceTests.cs`

- [ ] **Step 1: Add failing save behavior tests**

Append these tests to `Tests/Ac15/Ac15ProfileSettingsServiceTests.cs` before the `SchemaDatabase` nested class:

```csharp
[Fact]
public async Task SaveAsync_RejectsUnsupportedOptionGroupWithoutMutation()
{
    await using var database = await SchemaDatabase.CreateAsync();
    var user = new UserDatum { Baid = 1, MyDonName = "DON" };
    var save = UserSaveDataRedExtensions.CreateDefaultRedSaveData(1);
    save.DispDanType = 0;
    database.Context.UserData.Add(user);
    database.Context.UserSaveDataRed.Add(save);
    await database.Context.SaveChangesAsync();
    var profile = Ac15EraProfiles.Red with
    {
        ProfileCapabilities = Ac15EraProfiles.Red.ProfileCapabilities with
        {
            SupportsDisplayDanOnNamePlate = false
        }
    };

    var result = await Ac15ProfileSettingsService.SaveAsync<UserSaveDataRed, DanScoreDatumRed>(
        user,
        save,
        null,
        profile,
        new Ac15ProfileSettingsUpdateDto(
            new Ac15ProfileIdentityDto("BAD", 0),
            Customization: null,
            Options: new Ac15ProfileOptionGroupsUpdateDto(
                NamePlate: new Ac15NamePlateOptionsDto(true),
                Folder: null,
                SongSelect: null,
                Taikojuku: null,
                Tutorials: null,
                CustomizationBehavior: null)),
        new Ac15ProfileEditPolicy(AllowFreeProfileEditing: true),
        CancellationToken.None);

    Assert.Equal(Ac15ProfileSettingsResultStatus.BadRequest, result.Status);
    Assert.Equal("NamePlate options are not supported by Red.", result.ErrorMessage);
    Assert.Equal("DON", user.MyDonName);
    Assert.Equal(0u, save.DispDanType);
}

[Fact]
public async Task SaveAsync_RejectsUnsupportedCustomizationSlot()
{
    await using var database = await SchemaDatabase.CreateAsync();
    var user = new UserDatum { Baid = 1, MyDonName = "DON" };
    var save = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1);
    database.Context.UserData.Add(user);
    database.Context.UserSaveDataYellow.Add(save);
    await database.Context.SaveChangesAsync();
    var profile = Ac15EraProfiles.Yellow with
    {
        ProfileCapabilities = Ac15EraProfiles.Yellow.ProfileCapabilities with
        {
            CostumeSlots = ["head"]
        }
    };

    var result = await Ac15ProfileSettingsService.SaveAsync<UserSaveDataYellow, DanScoreDatumYellow>(
        user,
        save,
        database.Context.DanScoreDataYellow,
        profile,
        new Ac15ProfileSettingsUpdateDto(
            new Ac15ProfileIdentityDto("BAD", 0),
            new Ac15CustomizationUpdateDto(
                CostumeSlots:
                [
                    new Ac15CostumeSlotUpdateDto("puchi", 7, [0, 7])
                ],
                Title: null,
                Tone: null,
                Colors: null),
            Options: new Ac15ProfileOptionGroupsUpdateDto(null, null, null, null, null, null)),
        new Ac15ProfileEditPolicy(AllowFreeProfileEditing: true),
        CancellationToken.None);

    Assert.Equal(Ac15ProfileSettingsResultStatus.BadRequest, result.Status);
    Assert.Equal("Customization slot 'puchi' is not supported by Yellow.", result.ErrorMessage);
    Assert.Equal(0u, save.Costume5);
}

[Fact]
public async Task SaveAsync_PersistsSupportedGroupsAndLeavesOmittedUnlocksUnchanged()
{
    await using var database = await SchemaDatabase.CreateAsync();
    var user = new UserDatum { Baid = 1, MyDonName = "DON" };
    var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
    save.DispScoreType = 2;
    save.CostumeFlg1 = BitsetCodec.Encode([0, 5], Ac15EraProfiles.Green.Limits.CostumeFlagBytes);
    database.Context.UserData.Add(user);
    database.Context.UserSaveDataGreen.Add(save);
    database.Context.DanScoreDataGreen.Add(new DanScoreDatumGreen
    {
        Baid = 1,
        DanId = 3,
        IsExtra = false,
        ClearGrade = Ac15DanClearGrade.NotClear
    });
    await database.Context.SaveChangesAsync();

    var result = await Ac15ProfileSettingsService.SaveAsync<UserSaveDataGreen, DanScoreDatumGreen>(
        user,
        save,
        database.Context.DanScoreDataGreen,
        Ac15EraProfiles.Green,
        new Ac15ProfileSettingsUpdateDto(
            new Ac15ProfileIdentityDto("GREEN", 1),
            new Ac15CustomizationUpdateDto(
                CostumeSlots:
                [
                    new Ac15CostumeSlotUpdateDto("kigurumi", 12, null)
                ],
                Title: new Ac15TitleSelectionUpdateDto("Green Title", 10, [10]),
                Tone: new Ac15ToneSelectionUpdateDto(4, [0, 4]),
                Colors: new Ac15CostumeColorsDto(2, 3, 4)),
            Options: new Ac15ProfileOptionGroupsUpdateDto(
                NamePlate: new Ac15NamePlateOptionsDto(false),
                Folder: new Ac15FolderOptionsDto(false),
                SongSelect: new Ac15SongSelectOptionsDto(4, 3),
                Taikojuku: new Ac15TaikojukuFolderDanUpdateDto(3),
                Tutorials: new Ac15TutorialOptionsDto(true),
                CustomizationBehavior: new Ac15CustomizationBehaviorOptionsDto(false))),
        new Ac15ProfileEditPolicy(AllowFreeProfileEditing: false),
        CancellationToken.None);

    Assert.Equal(Ac15ProfileSettingsResultStatus.Success, result.Status);
    Assert.Equal("GREEN", user.MyDonName);
    Assert.Equal(1u, user.MyDonNameLanguage);
    Assert.Equal(12u, save.Costume1);
    Assert.Equal([0u, 5u], BitsetCodec.Decode(save.CostumeFlg1, Ac15EraProfiles.Green.Limits.CostumeFlagBytes));
    Assert.Equal("Green Title", save.Title);
    Assert.Equal(10u, save.TitleplateId);
    Assert.Contains(10u, BitsetCodec.Decode(save.TitleFlg, Ac15EraProfiles.Green.Limits.TitleFlagBytes));
    Assert.Equal(4u, save.DefaultToneSetting);
    Assert.Contains(4u, BitsetCodec.Decode(save.ToneFlg, Ac15EraProfiles.Green.Limits.ToneFlagBytes));
    Assert.Equal(2u, save.ColorBody);
    Assert.Equal(3u, save.ColorFace);
    Assert.Equal(4u, save.ColorLimb);
    Assert.Equal(0u, save.DispDanType);
    Assert.False(save.IsTojiru);
    Assert.False(save.IsAutoCostumeOn);
    Assert.True(save.IsExplain);
    Assert.Equal(4u, save.DispLevelChassis);
    Assert.Equal(3u, save.DispLevelSelf);
    Assert.Equal(3u, save.DispTaikojukuDan);
    Assert.Equal(2u, save.DispScoreType);
}

[Fact]
public async Task SaveAsync_RejectsUnselectableTaikojukuFolderDan()
{
    await using var database = await SchemaDatabase.CreateAsync();
    var user = new UserDatum { Baid = 1, MyDonName = "DON" };
    var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
    save.DispTaikojukuDan = 4;
    database.Context.UserData.Add(user);
    database.Context.UserSaveDataBlue.Add(save);
    database.Context.DanScoreDataBlue.Add(new DanScoreDatumBlue
    {
        Baid = 1,
        DanId = 3,
        IsExtra = false,
        ClearGrade = Ac15DanClearGrade.NormalClear
    });
    await database.Context.SaveChangesAsync();

    var result = await Ac15ProfileSettingsService.SaveAsync<UserSaveDataBlue, DanScoreDatumBlue>(
        user,
        save,
        database.Context.DanScoreDataBlue,
        Ac15EraProfiles.Blue,
        new Ac15ProfileSettingsUpdateDto(
            new Ac15ProfileIdentityDto("DON", 0),
            Customization: null,
            Options: new Ac15ProfileOptionGroupsUpdateDto(
                NamePlate: null,
                Folder: null,
                SongSelect: null,
                Taikojuku: new Ac15TaikojukuFolderDanUpdateDto(3),
                Tutorials: null,
                CustomizationBehavior: null)),
        new Ac15ProfileEditPolicy(AllowFreeProfileEditing: true),
        CancellationToken.None);

    Assert.Equal(Ac15ProfileSettingsResultStatus.BadRequest, result.Status);
    Assert.Equal("Taikojuku folder Dan 3 is not selectable for Blue.", result.ErrorMessage);
    Assert.Equal(4u, save.DispTaikojukuDan);
}
```

- [ ] **Step 2: Run failing save tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ProfileSettingsServiceTests"
```

Expected: FAIL because `SaveAsync` does not exist.

- [ ] **Step 3: Add save helpers and validation to the service**

In `Application/Ac15/Ac15ProfileSettingsService.cs`, add this method inside `Ac15ProfileSettingsService` after `GetAsync`:

```csharp
public static async ValueTask<Ac15ProfileSettingsResult> SaveAsync<TSave, TDanScore>(
    UserDatum user,
    TSave saveData,
    DbSet<TDanScore>? danScores,
    Ac15EraProfile profile,
    Ac15ProfileSettingsUpdateDto request,
    Ac15ProfileEditPolicy policy,
    CancellationToken cancellationToken)
    where TSave : class, IAc15ProfileSettingsSaveData
    where TDanScore : class, IAc15DanScoreDatum
{
    _ = policy;

    var validation = await ValidateUpdateAsync(
        request,
        profile,
        danScores,
        user.Baid,
        cancellationToken);
    if (validation is not null)
    {
        return Ac15ProfileSettingsResult.BadRequest(validation);
    }

    user.MyDonName = request.Identity.MyDonName;
    user.MyDonNameLanguage = request.Identity.MyDonNameLanguage;

    if (request.Customization is { } customization)
    {
        ApplyCustomization(saveData, profile, customization);
    }

    ApplyOptions(saveData, request.Options);
    return Ac15ProfileSettingsResult.Success();
}
```

Add these private methods below `BuildOptions`:

```csharp
private static async Task<string?> ValidateUpdateAsync<TDanScore>(
    Ac15ProfileSettingsUpdateDto request,
    Ac15EraProfile profile,
    DbSet<TDanScore>? danScores,
    uint baid,
    CancellationToken cancellationToken)
    where TDanScore : class, IAc15DanScoreDatum
{
    if (request.Identity is null)
    {
        return "Identity is required.";
    }

    if (request.Options is null)
    {
        return "Options are required.";
    }

    var capabilities = profile.ProfileCapabilities;
    if (request.Options.NamePlate is not null && !capabilities.SupportsDisplayDanOnNamePlate)
    {
        return $"NamePlate options are not supported by {profile.Era}.";
    }

    if (request.Options.Folder is not null && !capabilities.SupportsFolderCloseButton)
    {
        return $"Folder options are not supported by {profile.Era}.";
    }

    if (request.Options.CustomizationBehavior is not null && !capabilities.SupportsAutoCostume)
    {
        return $"CustomizationBehavior options are not supported by {profile.Era}.";
    }

    if (request.Options.Tutorials is not null && !capabilities.SupportsHowToPlayTutorialFlag)
    {
        return $"Tutorial options are not supported by {profile.Era}.";
    }

    if (request.Options.SongSelect is { } songSelect)
    {
        if (!capabilities.SupportsLocalRankingDifficulty && songSelect.LocalRankingDifficulty is not null)
        {
            return $"Local ranking difficulty is not supported by {profile.Era}.";
        }

        if (!capabilities.SupportsDefaultSelectedSelfBestDifficulty
            && songSelect.DefaultSelectedAndSelfBestDifficulty is not null)
        {
            return $"Default selected and self best difficulty is not supported by {profile.Era}.";
        }

        if (songSelect.LocalRankingDifficulty is > 4)
        {
            return "Local ranking difficulty must be between 0 and 4.";
        }

        if (songSelect.DefaultSelectedAndSelfBestDifficulty is > 4)
        {
            return "Default selected and self best difficulty must be between 0 and 4.";
        }
    }

    if (request.Options.Taikojuku is { } taikojuku)
    {
        if (!capabilities.SupportsTaikojukuFolderDan || danScores is null)
        {
            return $"Taikojuku folder Dan is not supported by {profile.Era}.";
        }

        var selectableDans = await GetSelectableTaikojukuFolderDans(danScores, baid, profile.Limits, cancellationToken);
        if (!selectableDans.Contains(taikojuku.FolderDan))
        {
            return $"Taikojuku folder Dan {taikojuku.FolderDan} is not selectable for {profile.Era}.";
        }
    }

    if (request.Customization is { } customization)
    {
        foreach (var slot in customization.CostumeSlots ?? [])
        {
            if (!capabilities.CostumeSlots.Contains(slot.Slot, StringComparer.Ordinal))
            {
                return $"Customization slot '{slot.Slot}' is not supported by {profile.Era}.";
            }
        }

        if (customization.Title is not null && !capabilities.SupportsTitle)
        {
            return $"Title customization is not supported by {profile.Era}.";
        }

        if (customization.Tone is not null && !capabilities.SupportsTone)
        {
            return $"Tone customization is not supported by {profile.Era}.";
        }

        if (customization.Colors is not null && !capabilities.SupportsColors)
        {
            return $"Costume colors are not supported by {profile.Era}.";
        }
    }

    return null;
}

private static void ApplyCustomization<TSave>(
    TSave saveData,
    Ac15EraProfile profile,
    Ac15CustomizationUpdateDto customization)
    where TSave : IAc15ProfileSettingsSaveData
{
    foreach (var slot in customization.CostumeSlots ?? [])
    {
        SetCostume(saveData, slot.Slot, slot.CurrentId);
        if (slot.UnlockedIds is not null)
        {
            SetCostumeFlag(
                saveData,
                slot.Slot,
                EncodeUnlocks(slot.UnlockedIds, slot.CurrentId, profile.Limits.CostumeFlagBytes));
        }
    }

    if (customization.Title is { } title)
    {
        saveData.Title = title.TitleText;
        saveData.TitleplateId = title.TitleId;
        if (title.UnlockedTitleIds is not null)
        {
            saveData.TitleFlg = EncodeUnlocks(title.UnlockedTitleIds, title.TitleId, profile.Limits.TitleFlagBytes);
        }
    }

    if (customization.Tone is { } tone)
    {
        saveData.DefaultToneSetting = tone.ToneId;
        if (tone.UnlockedToneIds is not null)
        {
            saveData.ToneFlg = EncodeUnlocks(tone.UnlockedToneIds, tone.ToneId, profile.Limits.ToneFlagBytes);
        }
    }

    if (customization.Colors is { } colors)
    {
        saveData.ColorBody = colors.BodyColor;
        saveData.ColorFace = colors.FaceColor;
        saveData.ColorLimb = colors.LimbColor;
    }
}

private static void ApplyOptions<TSave>(TSave saveData, Ac15ProfileOptionGroupsUpdateDto options)
    where TSave : IAc15ProfileSettingsSaveData
{
    if (options.NamePlate is { } namePlate)
    {
        saveData.DispDanType = namePlate.DisplayDanOnNamePlate ? 1u : 0u;
    }

    if (options.Folder is { } folder)
    {
        saveData.IsTojiru = folder.ShowFolderCloseButton;
    }

    if (options.CustomizationBehavior is { } customizationBehavior)
    {
        saveData.IsAutoCostumeOn = customizationBehavior.ApplyCostumeChangesFromPlayResults;
    }

    if (options.Tutorials is { DisableHowToPlayTutorial: { } disabled })
    {
        saveData.IsExplain = disabled;
    }

    if (options.SongSelect is { } songSelect)
    {
        if (songSelect.LocalRankingDifficulty is { } localRankingDifficulty)
        {
            saveData.DispLevelChassis = localRankingDifficulty;
        }

        if (songSelect.DefaultSelectedAndSelfBestDifficulty is { } defaultSelectedDifficulty)
        {
            saveData.DispLevelSelf = defaultSelectedDifficulty;
        }
    }

    if (options.Taikojuku is { } taikojuku)
    {
        saveData.DispTaikojukuDan = taikojuku.FolderDan;
    }
}

private static byte[] EncodeUnlocks(IEnumerable<uint> requestedUnlocks, uint currentId, int bytes)
{
    var ids = requestedUnlocks.ToHashSet();
    ids.Add(0);
    ids.Add(currentId);
    return BitsetCodec.Encode(ids.OrderBy(id => id).ToList(), bytes);
}

private static void SetCostume<TSave>(TSave saveData, string slot, uint value)
    where TSave : IAc15CustomizationSaveData
{
    switch (slot)
    {
        case "kigurumi":
            saveData.Costume1 = value;
            break;
        case "head":
            saveData.Costume2 = value;
            break;
        case "body":
            saveData.Costume3 = value;
            break;
        case "face":
            saveData.Costume4 = value;
            break;
        case "puchi":
            saveData.Costume5 = value;
            break;
    }
}

private static void SetCostumeFlag<TSave>(TSave saveData, string slot, byte[] value)
    where TSave : IAc15CustomizationSaveData
{
    switch (slot)
    {
        case "kigurumi":
            saveData.CostumeFlg1 = value;
            break;
        case "head":
            saveData.CostumeFlg2 = value;
            break;
        case "body":
            saveData.CostumeFlg3 = value;
            break;
        case "face":
            saveData.CostumeFlg4 = value;
            break;
        case "puchi":
            saveData.CostumeFlg5 = value;
            break;
    }
}
```

- [ ] **Step 4: Run save tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ProfileSettingsServiceTests"
```

Expected: PASS.

- [ ] **Step 5: Commit save service**

Run:

```powershell
git add Application/Ac15/Ac15ProfileSettingsService.cs Tests/Ac15/Ac15ProfileSettingsServiceTests.cs
git commit -m "Add strict AC15 profile settings save service"
```

## Task 4: AdminApi Controller Route Family

**Files:**
- Create: `Adapters.AdminApi/Controllers/Ac15ProfileSettingsController.cs`
- Create: `Adapters.AdminApi/Controllers/Ac15ProfileSettingsController.Blue.cs`
- Create: `Adapters.AdminApi/Controllers/Ac15ProfileSettingsController.Green.cs`
- Create: `Adapters.AdminApi/Controllers/Ac15ProfileSettingsController.Yellow.cs`
- Create: `Adapters.AdminApi/Controllers/Ac15ProfileSettingsController.Red.cs`
- Test: `Tests/Ac15/Ac15ProfileSettingsControllerTests.cs`

- [ ] **Step 1: Write failing controller tests**

Create `Tests/Ac15/Ac15ProfileSettingsControllerTests.cs`:

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.AdminApi.Controllers;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15ProfileSettingsControllerTests
{
    [Fact]
    public async Task Get_RejectsNijiiroEra()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        var controller = CreateController(database.Context);

        var result = await controller.Get("Nijiiro", 1);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Unsupported AC15 profile settings era 'Nijiiro'.", badRequest.Value);
    }

    [Fact]
    public async Task Get_BlueReadsBlueProfileOnly()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        database.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        database.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        var blueSave = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        blueSave.Costume1 = 12;
        database.Context.UserSaveDataBlue.Add(blueSave);
        await database.Context.SaveChangesAsync();
        var controller = CreateController(database.Context);

        var result = await controller.Get("Blue", 1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var setting = Assert.IsType<Ac15ProfileSettingsDto>(ok.Value);
        Assert.Equal("Blue", setting.Era);
        Assert.Equal(12u, Assert.Single(setting.Customization!.CostumeSlots, slot => slot.Slot == "kigurumi").CurrentId);
    }

    [Fact]
    public async Task Put_RedSavesRedRowsAndDoesNotMutateOtherEras()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        database.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        database.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        database.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        database.Context.UserSaveDataYellow.Add(UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1));
        var redSave = UserSaveDataRedExtensions.CreateDefaultRedSaveData(1);
        database.Context.UserSaveDataRed.Add(redSave);
        await database.Context.SaveChangesAsync();
        var controller = CreateController(database.Context);

        var result = await controller.Put("Red", 1, new Ac15ProfileSettingsUpdateDto(
            new Ac15ProfileIdentityDto("RED", 1),
            new Ac15CustomizationUpdateDto(
                CostumeSlots:
                [
                    new Ac15CostumeSlotUpdateDto("kigurumi", 7, [0, 7])
                ],
                Title: new Ac15TitleSelectionUpdateDto("Red Title", 10, [10]),
                Tone: new Ac15ToneSelectionUpdateDto(4, [0, 4]),
                Colors: new Ac15CostumeColorsDto(2, 3, 4)),
            new Ac15ProfileOptionGroupsUpdateDto(
                NamePlate: new Ac15NamePlateOptionsDto(false),
                Folder: new Ac15FolderOptionsDto(false),
                SongSelect: new Ac15SongSelectOptionsDto(4, 3),
                Taikojuku: new Ac15TaikojukuFolderDanUpdateDto(1),
                Tutorials: new Ac15TutorialOptionsDto(false),
                CustomizationBehavior: new Ac15CustomizationBehaviorOptionsDto(false))));

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("RED", (await database.Context.UserData.FindAsync(1u))!.MyDonName);
        Assert.Equal(7u, redSave.Costume1);
        Assert.Equal(0u, redSave.DispDanType);
        Assert.Equal(0u, (await database.Context.UserSaveDataBlue.FindAsync(1u))!.Costume1);
        Assert.Equal(0u, (await database.Context.UserSaveDataGreen.FindAsync(1u))!.Costume1);
        Assert.Equal(0u, (await database.Context.UserSaveDataYellow.FindAsync(1u))!.Costume1);
    }

    [Fact]
    public async Task Put_ReturnsBadRequestForUnsupportedSubmittedGroup()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        database.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        database.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await database.Context.SaveChangesAsync();
        var controller = CreateController(database.Context);

        var result = await controller.Put("Blue", 1, new Ac15ProfileSettingsUpdateDto(
            new Ac15ProfileIdentityDto("DON", 0),
            Customization: null,
            Options: new Ac15ProfileOptionGroupsUpdateDto(
                NamePlate: null,
                Folder: null,
                SongSelect: new Ac15SongSelectOptionsDto(5, null),
                Taikojuku: null,
                Tutorials: null,
                CustomizationBehavior: null)));

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Local ranking difficulty must be between 0 and 4.", badRequest.Value);
    }

    private static Ac15ProfileSettingsController CreateController(ITaikoDbContext context)
    {
        var authSettings = new AuthSettings { AuthenticationRequired = false, AllowFreeProfileEditing = true };
        var services = new ServiceCollection()
            .AddSingleton(Options.Create(authSettings))
            .BuildServiceProvider();

        return new Ac15ProfileSettingsController(context, Options.Create(authSettings))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services }
            }
        };
    }

    private sealed class SchemaDatabase(SqliteConnection connection) : IAsyncDisposable
    {
        public TaikoDbContext Context { get; } = CreateContext(connection);

        public static async Task<SchemaDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var database = new SchemaDatabase(connection);
            await database.Context.Database.EnsureCreatedAsync();
            return database;
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await connection.DisposeAsync();
        }

        private static TaikoDbContext CreateContext(SqliteConnection connection)
            => new(new DbContextOptionsBuilder<TaikoDbContext>()
                .UseSqlite(connection)
                .Options);
    }
}
```

- [ ] **Step 2: Run failing controller tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ProfileSettingsControllerTests"
```

Expected: FAIL because `Ac15ProfileSettingsController` does not exist.

- [ ] **Step 3: Add the controller dispatcher**

Create `Adapters.AdminApi/Controllers/Ac15ProfileSettingsController.cs`:

```csharp
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/{era}/[controller]")]
[Authorize]
public sealed partial class Ac15ProfileSettingsController(
    ITaikoDbContext context,
    IOptions<AuthSettings> authOptions) : BaseAdminController<Ac15ProfileSettingsController>
{
    private readonly ITaikoDbContext context = context;
    private readonly AuthSettings authSettings = authOptions.Value;

    [HttpGet("{baid}")]
    public async Task<ActionResult<Ac15ProfileSettingsDto>> Get(string era, uint baid)
    {
        if (!TryGetAc15Era(era, out var gameEra, out var badEra))
        {
            return badEra!;
        }

        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
        {
            return forbid;
        }

        return gameEra switch
        {
            GameEra.Blue => await GetBlue(baid),
            GameEra.Green => await GetGreen(baid),
            GameEra.Yellow => await GetYellow(baid),
            GameEra.Red => await GetRed(baid),
            _ => BadAc15Era(era)
        };
    }

    [HttpPut("{baid}")]
    public async Task<IActionResult> Put(string era, uint baid, Ac15ProfileSettingsUpdateDto request)
    {
        if (!TryGetAc15Era(era, out var gameEra, out var badEra))
        {
            return badEra!;
        }

        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
        {
            return forbid;
        }

        return gameEra switch
        {
            GameEra.Blue => await PutBlue(baid, request),
            GameEra.Green => await PutGreen(baid, request),
            GameEra.Yellow => await PutYellow(baid, request),
            GameEra.Red => await PutRed(baid, request),
            _ => BadAc15Era(era)
        };
    }

    private async Task<ActionResult<Ac15ProfileSettingsDto>> GetForAc15Async<TSave, TDanScore>(
        uint baid,
        Func<uint, CancellationToken, ValueTask<TSave>> getSaveData,
        DbSet<TDanScore> danScores,
        Ac15EraProfile profile)
        where TSave : class, IAc15ProfileSettingsSaveData
        where TDanScore : class, IAc15DanScoreDatum
    {
        var user = await context.UserData.Include(d => d.Tokens).FirstOrDefaultAsync(d => d.Baid == baid);
        if (user is null)
        {
            return NotFound();
        }

        var saveData = await getSaveData(baid, HttpContext.RequestAborted);
        var result = await Ac15ProfileSettingsService.GetAsync(
            user,
            saveData,
            danScores,
            profile,
            HttpContext.RequestAborted);

        return Ok(result.Setting);
    }

    private async Task<IActionResult> PutForAc15Async<TSave, TDanScore>(
        uint baid,
        Ac15ProfileSettingsUpdateDto request,
        Func<uint, CancellationToken, ValueTask<TSave>> getSaveData,
        DbSet<TDanScore> danScores,
        Ac15EraProfile profile)
        where TSave : class, IAc15ProfileSettingsSaveData
        where TDanScore : class, IAc15DanScoreDatum
    {
        var user = await context.UserData.FindAsync(baid);
        if (user is null)
        {
            return NotFound();
        }

        var saveData = await getSaveData(baid, HttpContext.RequestAborted);
        var result = await Ac15ProfileSettingsService.SaveAsync(
            user,
            saveData,
            danScores,
            profile,
            request,
            new Ac15ProfileEditPolicy(authSettings.AllowFreeProfileEditing),
            HttpContext.RequestAborted);
        if (result.Status == Ac15ProfileSettingsResultStatus.BadRequest)
        {
            return BadRequest(result.ErrorMessage);
        }

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private static bool TryGetAc15Era(
        string era,
        out GameEra gameEra,
        [NotNullWhen(false)] out BadRequestObjectResult? badRequest)
    {
        if (!EraRoute.TryParse(era, out gameEra) || !Ac15EraProfiles.TryGet(gameEra, out _))
        {
            badRequest = BadAc15Era(era);
            return false;
        }

        badRequest = null;
        return true;
    }

    private static BadRequestObjectResult BadAc15Era(string era)
        => new($"Unsupported AC15 profile settings era '{era}'.");
}
```

- [ ] **Step 4: Add era controller partials**

Create `Adapters.AdminApi/Controllers/Ac15ProfileSettingsController.Blue.cs`:

```csharp
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public sealed partial class Ac15ProfileSettingsController
{
    private Task<ActionResult<Ac15ProfileSettingsDto>> GetBlue(uint baid)
        => GetForAc15Async<UserSaveDataBlue, DanScoreDatumBlue>(
            baid,
            context.GetOrCreateBlueSaveDataAsync,
            context.DanScoreDataBlue,
            Ac15EraProfiles.Blue);

    private Task<IActionResult> PutBlue(uint baid, Ac15ProfileSettingsUpdateDto request)
        => PutForAc15Async<UserSaveDataBlue, DanScoreDatumBlue>(
            baid,
            request,
            context.GetOrCreateBlueSaveDataAsync,
            context.DanScoreDataBlue,
            Ac15EraProfiles.Blue);
}
```

Create `Adapters.AdminApi/Controllers/Ac15ProfileSettingsController.Green.cs`:

```csharp
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public sealed partial class Ac15ProfileSettingsController
{
    private Task<ActionResult<Ac15ProfileSettingsDto>> GetGreen(uint baid)
        => GetForAc15Async<UserSaveDataGreen, DanScoreDatumGreen>(
            baid,
            context.GetOrCreateGreenSaveDataAsync,
            context.DanScoreDataGreen,
            Ac15EraProfiles.Green);

    private Task<IActionResult> PutGreen(uint baid, Ac15ProfileSettingsUpdateDto request)
        => PutForAc15Async<UserSaveDataGreen, DanScoreDatumGreen>(
            baid,
            request,
            context.GetOrCreateGreenSaveDataAsync,
            context.DanScoreDataGreen,
            Ac15EraProfiles.Green);
}
```

Create `Adapters.AdminApi/Controllers/Ac15ProfileSettingsController.Yellow.cs`:

```csharp
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public sealed partial class Ac15ProfileSettingsController
{
    private Task<ActionResult<Ac15ProfileSettingsDto>> GetYellow(uint baid)
        => GetForAc15Async<UserSaveDataYellow, DanScoreDatumYellow>(
            baid,
            context.GetOrCreateYellowSaveDataAsync,
            context.DanScoreDataYellow,
            Ac15EraProfiles.Yellow);

    private Task<IActionResult> PutYellow(uint baid, Ac15ProfileSettingsUpdateDto request)
        => PutForAc15Async<UserSaveDataYellow, DanScoreDatumYellow>(
            baid,
            request,
            context.GetOrCreateYellowSaveDataAsync,
            context.DanScoreDataYellow,
            Ac15EraProfiles.Yellow);
}
```

Create `Adapters.AdminApi/Controllers/Ac15ProfileSettingsController.Red.cs`:

```csharp
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public sealed partial class Ac15ProfileSettingsController
{
    private Task<ActionResult<Ac15ProfileSettingsDto>> GetRed(uint baid)
        => GetForAc15Async<UserSaveDataRed, DanScoreDatumRed>(
            baid,
            context.GetOrCreateRedSaveDataAsync,
            context.DanScoreDataRed,
            Ac15EraProfiles.Red);

    private Task<IActionResult> PutRed(uint baid, Ac15ProfileSettingsUpdateDto request)
        => PutForAc15Async<UserSaveDataRed, DanScoreDatumRed>(
            baid,
            request,
            context.GetOrCreateRedSaveDataAsync,
            context.DanScoreDataRed,
            Ac15EraProfiles.Red);
}
```

- [ ] **Step 5: Run controller tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ProfileSettingsControllerTests"
```

Expected: PASS.

- [ ] **Step 6: Commit controller route family**

Run:

```powershell
git add Adapters.AdminApi/Controllers/Ac15ProfileSettingsController.cs Adapters.AdminApi/Controllers/Ac15ProfileSettingsController.Blue.cs Adapters.AdminApi/Controllers/Ac15ProfileSettingsController.Green.cs Adapters.AdminApi/Controllers/Ac15ProfileSettingsController.Yellow.cs Adapters.AdminApi/Controllers/Ac15ProfileSettingsController.Red.cs Tests/Ac15/Ac15ProfileSettingsControllerTests.cs
git commit -m "Add AC15 profile settings AdminApi routes"
```

## Task 5: WebUI AC15 Models And HTTP Helpers

**Files:**
- Create: `TaikoWebUI/Shared/Customize/PlayerPreviewModel.cs`
- Modify: `TaikoWebUI/Shared/Customize/PlayerPreview.razor`
- Create: `TaikoWebUI/Pages/ProfileEditor/Ac15ProfileEditorState.cs`
- Create: `TaikoWebUI/Services/ProfileSettingsHttpClient.cs`
- Test: `Tests/WebUi/Ac15ProfileSettingsWebUiTests.cs`

- [ ] **Step 1: Write failing WebUI model tests**

Create `Tests/WebUi/Ac15ProfileSettingsWebUiTests.cs`:

```csharp
using System.Net;
using TaikoWebUI.Pages.ProfileEditor;
using TaikoWebUI.Services;
using TaikoWebUI.Shared.Customize;

namespace TaikoLocalServer.Tests.WebUi;

public sealed class Ac15ProfileSettingsWebUiTests
{
    [Fact]
    public void EditorState_UsesReturnedGroupsAndSlots()
    {
        var dto = CreateAc15Dto(
            customization: new Ac15CustomizationDto(
                CostumeSlots:
                [
                    new Ac15CostumeSlotDto("head", 8, [0, 8])
                ],
                Title: null,
                Tone: null,
                Colors: null),
            options: new Ac15ProfileOptionGroupsDto(
                NamePlate: new Ac15NamePlateOptionsDto(true),
                Folder: null,
                SongSelect: null,
                Taikojuku: null,
                Tutorials: null,
                CustomizationBehavior: null));

        var state = Ac15ProfileEditorState.From(dto);

        Assert.True(state.ShowNamePlateOptions);
        Assert.False(state.ShowFolderOptions);
        Assert.Equal(["head"], state.CostumeSlots.Select(slot => slot.Slot).ToArray());
        var update = state.ToUpdateDto(includeUnlockLists: false);
        Assert.NotNull(update.Options.NamePlate);
        Assert.Null(update.Options.Folder);
        Assert.Single(update.Customization!.CostumeSlots!);
        Assert.Null(update.Customization.CostumeSlots![0].UnlockedIds);
    }

    [Fact]
    public void PlayerPreviewModel_FromAc15DefaultsUnsupportedSlotsToZero()
    {
        var dto = CreateAc15Dto(
            customization: new Ac15CustomizationDto(
                CostumeSlots:
                [
                    new Ac15CostumeSlotDto("kigurumi", 12, [0, 12])
                ],
                Title: new Ac15TitleSelectionDto("Title", 10, [10]),
                Tone: null,
                Colors: new Ac15CostumeColorsDto(2, 3, 4)),
            options: new Ac15ProfileOptionGroupsDto(
                NamePlate: new Ac15NamePlateOptionsDto(true),
                Folder: null,
                SongSelect: null,
                Taikojuku: null,
                Tutorials: null,
                CustomizationBehavior: null));

        var preview = PlayerPreviewModel.FromAc15(dto);

        Assert.Equal("DON", preview.MyDonName);
        Assert.Equal("Title", preview.Title);
        Assert.Equal(10u, preview.TitlePlateId);
        Assert.Equal(12u, preview.Kigurumi);
        Assert.Equal(0u, preview.Head);
        Assert.Equal(2u, preview.BodyColor);
        Assert.True(preview.IsDisplayDanOnNamePlate);
    }

    [Fact]
    public async Task GetProfileDisplayNameAsync_UsesAc15RouteForAc15AndUserSettingsForNijiiro()
    {
        var handler = new RecordingHandler();
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var blueName = await client.GetProfileDisplayNameAsync("Blue", 1);
        var nijiiroName = await client.GetProfileDisplayNameAsync("Nijiiro", 2);

        Assert.Equal("BLUE", blueName);
        Assert.Equal("NIJIIRO", nijiiroName);
        Assert.Equal(
            [
                "api/Blue/Ac15ProfileSettings/1",
                "api/Nijiiro/UserSettings/2"
            ],
            handler.RequestPaths);
    }

    private static Ac15ProfileSettingsDto CreateAc15Dto(
        Ac15CustomizationDto? customization,
        Ac15ProfileOptionGroupsDto options)
        => new(
            Era: "Blue",
            Baid: 1,
            Identity: new Ac15ProfileIdentityDto("DON", 0),
            Customization: customization,
            Options: options,
            Capabilities: Ac15ProfileCapabilities.CurrentFull.ToDto(),
            LastPlayDateTime: DateTime.UnixEpoch);

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<string> RequestPaths { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.PathAndQuery.TrimStart('/') ?? string.Empty;
            RequestPaths.Add(path);
            var content = path switch
            {
                "api/Blue/Ac15ProfileSettings/1" => """
                {
                  "era": "Blue",
                  "baid": 1,
                  "identity": { "myDonName": "BLUE", "myDonNameLanguage": 0 },
                  "customization": null,
                  "options": {},
                  "capabilities": {
                    "costumeSlots": [],
                    "supportsTitle": false,
                    "supportsTone": false,
                    "supportsColors": false,
                    "supportsDisplayDanOnNamePlate": false,
                    "supportsFolderCloseButton": false,
                    "supportsAutoCostume": false,
                    "supportsHowToPlayTutorialFlag": false,
                    "supportsLocalRankingDifficulty": false,
                    "supportsDefaultSelectedSelfBestDifficulty": false,
                    "supportsTaikojukuFolderDan": false
                  },
                  "lastPlayDateTime": "1970-01-01T00:00:00Z"
                }
                """,
                "api/Nijiiro/UserSettings/2" => """{ "myDonName": "NIJIIRO" }""",
                _ => "{}"
            };

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            });
        }
    }
}
```

- [ ] **Step 2: Run failing WebUI model tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ProfileSettingsWebUiTests"
```

Expected: FAIL because `Ac15ProfileEditorState`, `PlayerPreviewModel`, and `GetProfileDisplayNameAsync` do not exist.

- [ ] **Step 3: Add the preview model and update `PlayerPreview`**

Create `TaikoWebUI/Shared/Customize/PlayerPreviewModel.cs`:

```csharp
namespace TaikoWebUI.Shared.Customize;

public sealed record PlayerPreviewModel(
    string MyDonName,
    string Title,
    uint TitlePlateId,
    uint Kigurumi,
    uint Head,
    uint Body,
    uint Face,
    uint Puchi,
    uint BodyColor,
    uint FaceColor,
    uint LimbColor,
    bool IsDisplayDanOnNamePlate)
{
    public static PlayerPreviewModel FromUserSetting(UserSetting setting)
        => new(
            setting.MyDonName,
            setting.Title,
            setting.TitlePlateId,
            setting.Kigurumi,
            setting.Head,
            setting.Body,
            setting.Face,
            setting.Puchi,
            setting.BodyColor,
            setting.FaceColor,
            setting.LimbColor,
            setting.IsDisplayDanOnNamePlate);

    public static PlayerPreviewModel FromAc15(Ac15ProfileSettingsDto setting)
    {
        var slots = setting.Customization?.CostumeSlots.ToDictionary(slot => slot.Slot, StringComparer.Ordinal)
                    ?? new Dictionary<string, Ac15CostumeSlotDto>(StringComparer.Ordinal);
        var colors = setting.Customization?.Colors;

        return new PlayerPreviewModel(
            setting.Identity.MyDonName,
            setting.Customization?.Title?.TitleText ?? string.Empty,
            setting.Customization?.Title?.TitleId ?? 0,
            GetSlot(slots, "kigurumi"),
            GetSlot(slots, "head"),
            GetSlot(slots, "body"),
            GetSlot(slots, "face"),
            GetSlot(slots, "puchi"),
            colors?.BodyColor ?? 1,
            colors?.FaceColor ?? 0,
            colors?.LimbColor ?? 3,
            setting.Options.NamePlate?.DisplayDanOnNamePlate ?? false);
    }

    private static uint GetSlot(IReadOnlyDictionary<string, Ac15CostumeSlotDto> slots, string slot)
        => slots.TryGetValue(slot, out var value) ? value.CurrentId : 0;
}
```

In `TaikoWebUI/Shared/Customize/PlayerPreview.razor`, replace the parameter and every `Setting.` reference with `Preview.`:

```razor
@code {
    [Parameter, EditorRequired] public PlayerPreviewModel Preview { get; set; } = default!;
    [Parameter] public IReadOnlyDictionary<uint, Title> TitleCatalog { get; set; } = new Dictionary<uint, Title>();
    [Parameter] public bool ResolveTitlePlateFromCatalog { get; set; }

    ...
}
```

The opening guard should become:

```razor
@if (Preview is not null)
{
```

The `ResolveTitlePlateId()` method should become:

```csharp
private uint ResolveTitlePlateId()
{
    return ResolveTitlePlateFromCatalog
           && TitleCatalog.TryGetValue(Preview.TitlePlateId, out var title)
           && TitleNameMatches(title, Preview.Title)
        ? title.TitleRarity
        : Preview.TitlePlateId;
}
```

- [ ] **Step 4: Add AC15 editor state**

Create `TaikoWebUI/Pages/ProfileEditor/Ac15ProfileEditorState.cs`:

```csharp
using TaikoWebUI.Shared.Customize;

namespace TaikoWebUI.Pages.ProfileEditor;

public sealed class Ac15ProfileEditorState
{
    private Ac15ProfileEditorState(Ac15ProfileSettingsDto source)
    {
        Source = source;
        MyDonName = source.Identity.MyDonName;
        MyDonNameLanguage = source.Identity.MyDonNameLanguage;
        DisplayDanOnNamePlate = source.Options.NamePlate?.DisplayDanOnNamePlate ?? false;
        ShowFolderCloseButton = source.Options.Folder?.ShowFolderCloseButton ?? false;
        LocalRankingDifficulty = source.Options.SongSelect?.LocalRankingDifficulty ?? 0;
        DefaultSelectedAndSelfBestDifficulty = source.Options.SongSelect?.DefaultSelectedAndSelfBestDifficulty ?? 0;
        TaikojukuFolderDan = source.Options.Taikojuku?.FolderDan ?? 0;
        SelectableTaikojukuDans = source.Options.Taikojuku?.SelectableFolderDans ?? [];
        DisableHowToPlayTutorial = source.Options.Tutorials?.DisableHowToPlayTutorial ?? false;
        ApplyCostumeChangesFromPlayResults = source.Options.CustomizationBehavior?.ApplyCostumeChangesFromPlayResults ?? false;
        CostumeSlots = source.Customization?.CostumeSlots
            .Select(slot => new Ac15CostumeSlotEditorState(
                slot.Slot,
                new CostumePickerValue(slot.CurrentId, slot.UnlockedIds)))
            .ToList() ?? [];
        Title = source.Customization?.Title is { } title
            ? new TitlePickerValue(title.TitleText, title.TitleId, title.UnlockedTitleIds)
            : null;
        Tone = source.Customization?.Tone is { } tone
            ? new NeiroPickerValue(tone.ToneId, tone.UnlockedToneIds)
            : null;
        Colors = source.Customization?.Colors is { } colors
            ? new ColorPickerValue(colors.BodyColor, colors.FaceColor, colors.LimbColor)
            : null;
    }

    public Ac15ProfileSettingsDto Source { get; }
    public string MyDonName { get; set; }
    public uint MyDonNameLanguage { get; set; }
    public bool DisplayDanOnNamePlate { get; set; }
    public bool ShowFolderCloseButton { get; set; }
    public uint LocalRankingDifficulty { get; set; }
    public uint DefaultSelectedAndSelfBestDifficulty { get; set; }
    public uint TaikojukuFolderDan { get; set; }
    public IReadOnlyList<uint> SelectableTaikojukuDans { get; }
    public bool DisableHowToPlayTutorial { get; set; }
    public bool ApplyCostumeChangesFromPlayResults { get; set; }
    public List<Ac15CostumeSlotEditorState> CostumeSlots { get; }
    public TitlePickerValue? Title { get; set; }
    public NeiroPickerValue? Tone { get; set; }
    public ColorPickerValue? Colors { get; set; }

    public bool ShowNamePlateOptions => Source.Options.NamePlate is not null;
    public bool ShowFolderOptions => Source.Options.Folder is not null;
    public bool ShowSongSelectOptions => Source.Options.SongSelect is not null;
    public bool ShowTaikojukuOptions => Source.Options.Taikojuku is not null;
    public bool ShowTutorialOptions => Source.Options.Tutorials is not null;
    public bool ShowCustomizationBehaviorOptions => Source.Options.CustomizationBehavior is not null;
    public bool ShowCustomization => Source.Customization is not null;
    public bool ShowTitle => Source.Customization?.Title is not null;
    public bool ShowTone => Source.Customization?.Tone is not null;
    public bool ShowColors => Source.Customization?.Colors is not null;

    public static Ac15ProfileEditorState From(Ac15ProfileSettingsDto source)
        => new(source);

    public Ac15ProfileSettingsUpdateDto ToUpdateDto(bool includeUnlockLists)
        => new(
            Identity: new Ac15ProfileIdentityDto(MyDonName, MyDonNameLanguage),
            Customization: ShowCustomization
                ? new Ac15CustomizationUpdateDto(
                    CostumeSlots: CostumeSlots
                        .Select(slot => new Ac15CostumeSlotUpdateDto(
                            slot.Slot,
                            slot.Value.CurrentId,
                            includeUnlockLists ? slot.Value.UnlockedIds : null))
                        .ToArray(),
                    Title: ShowTitle && Title is not null
                        ? new Ac15TitleSelectionUpdateDto(
                            Title.Title,
                            Title.TitlePlateId,
                            includeUnlockLists ? Title.UnlockedTitleIds : null)
                        : null,
                    Tone: ShowTone && Tone is not null
                        ? new Ac15ToneSelectionUpdateDto(
                            Tone.CurrentId,
                            includeUnlockLists ? Tone.UnlockedIds : null)
                        : null,
                    Colors: ShowColors && Colors is not null
                        ? new Ac15CostumeColorsDto(Colors.BodyColor, Colors.FaceColor, Colors.LimbColor)
                        : null)
                : null,
            Options: new Ac15ProfileOptionGroupsUpdateDto(
                NamePlate: ShowNamePlateOptions ? new Ac15NamePlateOptionsDto(DisplayDanOnNamePlate) : null,
                Folder: ShowFolderOptions ? new Ac15FolderOptionsDto(ShowFolderCloseButton) : null,
                SongSelect: ShowSongSelectOptions
                    ? new Ac15SongSelectOptionsDto(
                        Source.Options.SongSelect!.LocalRankingDifficulty is null ? null : LocalRankingDifficulty,
                        Source.Options.SongSelect.DefaultSelectedAndSelfBestDifficulty is null ? null : DefaultSelectedAndSelfBestDifficulty)
                    : null,
                Taikojuku: ShowTaikojukuOptions ? new Ac15TaikojukuFolderDanUpdateDto(TaikojukuFolderDan) : null,
                Tutorials: ShowTutorialOptions ? new Ac15TutorialOptionsDto(DisableHowToPlayTutorial) : null,
                CustomizationBehavior: ShowCustomizationBehaviorOptions
                    ? new Ac15CustomizationBehaviorOptionsDto(ApplyCostumeChangesFromPlayResults)
                    : null));

    public PlayerPreviewModel ToPreviewModel()
        => PlayerPreviewModel.FromAc15(Source with
        {
            Identity = new Ac15ProfileIdentityDto(MyDonName, MyDonNameLanguage),
            Customization = Source.Customization is null
                ? null
                : new Ac15CustomizationDto(
                    CostumeSlots.Select(slot => new Ac15CostumeSlotDto(slot.Slot, slot.Value.CurrentId, slot.Value.UnlockedIds)).ToArray(),
                    Title is null ? null : new Ac15TitleSelectionDto(Title.Title, Title.TitlePlateId, Title.UnlockedTitleIds),
                    Tone is null ? null : new Ac15ToneSelectionDto(Tone.CurrentId, Tone.UnlockedIds),
                    Colors is null ? null : new Ac15CostumeColorsDto(Colors.BodyColor, Colors.FaceColor, Colors.LimbColor)),
            Options = Source.Options with
            {
                NamePlate = ShowNamePlateOptions ? new Ac15NamePlateOptionsDto(DisplayDanOnNamePlate) : null
            }
        });
}

public sealed record Ac15CostumeSlotEditorState(
    string Slot,
    CostumePickerValue Value);
```

- [ ] **Step 5: Add profile HTTP helper**

Create `TaikoWebUI/Services/ProfileSettingsHttpClient.cs`:

```csharp
using System.Net.Http.Json;
using TaikoWebUI.Utilities;

namespace TaikoWebUI.Services;

public static class ProfileSettingsHttpClient
{
    public static async Task<string?> GetProfileDisplayNameAsync(
        this HttpClient client,
        string era,
        uint baid,
        CancellationToken cancellationToken = default)
    {
        if (WebUiEra.IsAc15(era))
        {
            var ac15 = await client.GetFromJsonAsync<Ac15ProfileSettingsDto>(
                WebUiEra.Api(era, $"Ac15ProfileSettings/{baid}"),
                cancellationToken);
            return ac15?.Identity.MyDonName;
        }

        var nijiiro = await client.GetFromJsonAsync<UserSetting>(
            WebUiEra.Api(era, $"UserSettings/{baid}"),
            cancellationToken);
        return nijiiro?.MyDonName;
    }
}
```

- [ ] **Step 6: Run WebUI model tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ProfileSettingsWebUiTests"
```

Expected: PASS.

- [ ] **Step 7: Commit WebUI model helpers**

Run:

```powershell
git add TaikoWebUI/Shared/Customize/PlayerPreviewModel.cs TaikoWebUI/Shared/Customize/PlayerPreview.razor TaikoWebUI/Pages/ProfileEditor/Ac15ProfileEditorState.cs TaikoWebUI/Services/ProfileSettingsHttpClient.cs Tests/WebUi/Ac15ProfileSettingsWebUiTests.cs
git commit -m "Add AC15 profile WebUI models"
```

## Task 6: Profile Page AC15 Editor Migration

**Files:**
- Create: `TaikoWebUI/Pages/ProfileEditor/Ac15ProfileEditor.razor`
- Modify: `TaikoWebUI/Pages/Profile.razor`
- Modify: `TaikoWebUI/Pages/Profile.razor.cs`

- [ ] **Step 1: Add the AC15 editor component**

Create `TaikoWebUI/Pages/ProfileEditor/Ac15ProfileEditor.razor`:

```razor
@using TaikoWebUI.Shared.Customize

<MudStack Spacing="4">
    <MudText Typo="Typo.h6">@Localizer["Profile Options"]</MudText>
    <MudGrid>
        <MudItem xs="12" md="8">
            <MudTextField TextChanged="OnNameChanged" Required="true" @bind-Value="State.MyDonName" Label="@Localizer["Name"]" />
        </MudItem>
        <MudItem xs="12" md="4">
            <MudSelect @bind-Value="State.MyDonNameLanguage" Label="@Localizer["Language"]" AnchorOrigin="Origin.BottomCenter">
                @for (uint i = 0; i < LanguageStrings.Length; i++)
                {
                    var index = i;
                    <MudSelectItem Value="@i">@Localizer[LanguageStrings[index]]</MudSelectItem>
                }
            </MudSelect>
        </MudItem>
    </MudGrid>

    <MudGrid>
        <MudItem xs="12" md="4">
            <MudStack Spacing="4">
                @if (State.ShowNamePlateOptions)
                {
                    <MudSwitch @bind-Value="State.DisplayDanOnNamePlate" Label="@Localizer["Display Dan Rank on Name Plate"]" Color="Color.Primary" />
                }
                @if (State.ShowFolderOptions)
                {
                    <MudSwitch @bind-Value="State.ShowFolderCloseButton" Label="@Localizer["Show Folder Close Button"]" Color="Color.Primary" />
                }
                @if (State.ShowCustomizationBehaviorOptions)
                {
                    <MudSwitch @bind-Value="State.ApplyCostumeChangesFromPlayResults" Label="@Localizer["Apply Costume Changes from Play Results"]" Color="Color.Primary" />
                }
                @if (State.ShowTutorialOptions)
                {
                    <MudSwitch @bind-Value="State.DisableHowToPlayTutorial" Label="@Localizer["Disable How to Play Tutorial"]" Color="Color.Primary" />
                }
            </MudStack>
        </MudItem>
        <MudItem xs="12" md="8">
            <MudStack Spacing="4">
                @if (State.ShowSongSelectOptions && State.Source.Options.SongSelect?.LocalRankingDifficulty is not null)
                {
                    <MudSelect @bind-Value="State.LocalRankingDifficulty" Label="@Localizer["Local Ranking Difficulty"]" AnchorOrigin="Origin.BottomCenter">
                        @for (uint i = 0; i < Ac15DifficultyStrings.Length; i++)
                        {
                            var index = i;
                            <MudSelectItem Value="@i">@Localizer[Ac15DifficultyStrings[index]]</MudSelectItem>
                        }
                    </MudSelect>
                }
                @if (State.ShowSongSelectOptions && State.Source.Options.SongSelect?.DefaultSelectedAndSelfBestDifficulty is not null)
                {
                    <MudSelect @bind-Value="State.DefaultSelectedAndSelfBestDifficulty" Label="@Localizer["Default Selected and Self Best Difficulty"]" AnchorOrigin="Origin.BottomCenter">
                        @for (uint i = 0; i < Ac15DifficultyStrings.Length; i++)
                        {
                            var index = i;
                            <MudSelectItem Value="@i">@Localizer[Ac15DifficultyStrings[index]]</MudSelectItem>
                        }
                    </MudSelect>
                }
            </MudStack>
        </MudItem>
    </MudGrid>
</MudStack>

@if (State.ShowCustomization)
{
    <MudStack Spacing="4">
        <MudText Typo="Typo.h6">@Localizer["Costume Options"]</MudText>

        @foreach (var slot in State.CostumeSlots)
        {
            <CostumePicker Era="@Era"
                           Label="@Localizer[GetSlotLabel(slot.Slot)]"
                           Catalog="@GetCostumeCatalog(slot.Slot)"
                           Value="@slot.Value"
                           ValueChanged="@(value => OnCostumeChanged(slot.Slot, value))"
                           AllowUnlockEditing="@AllowUnlockEditing" />
        }

        @if (State.ShowTitle && State.Title is not null)
        {
            <TitlePicker Era="@Era"
                         Catalog="@TitleCatalog"
                         Value="@State.Title"
                         ValueChanged="OnTitleChanged"
                         ReadOnlyTitleText="false"
                         SelectionMode="TitleSelectionMode.TitleId"
                         AllowUnlockEditing="@AllowUnlockEditing" />
        }

        @if (State.ShowTone && State.Tone is not null)
        {
            <NeiroPicker Era="@Era"
                         Catalog="@ToneCatalog"
                         Value="@State.Tone"
                         ValueChanged="OnToneChanged"
                         AllowUnlockEditing="@AllowUnlockEditing" />
        }

        @if (State.ShowColors && State.Colors is not null)
        {
            <ColorPicker Colors="@TaikoCustomizationVisuals.CostumeColors"
                         Value="@State.Colors"
                         ValueChanged="OnColorsChanged" />
        }

        @if (State.ShowTaikojukuOptions && State.SelectableTaikojukuDans.Count > 0)
        {
            <MudSelect T="uint"
                       @bind-Value="State.TaikojukuFolderDan"
                       Label="@Localizer["Taikojuku Folder Dan"]"
                       AnchorOrigin="Origin.BottomCenter">
                @foreach (var danId in State.SelectableTaikojukuDans)
                {
                    <MudSelectItem Value="@danId">@FormatDan(danId)</MudSelectItem>
                }
            </MudSelect>
        }
    </MudStack>
}

@code {
    [Parameter, EditorRequired] public Ac15ProfileEditorState State { get; set; } = default!;
    [Parameter, EditorRequired] public string Era { get; set; } = string.Empty;
    [Parameter] public IReadOnlyList<Costume> CostumeCatalog { get; set; } = [];
    [Parameter] public IReadOnlyDictionary<uint, Title> TitleCatalog { get; set; } = new Dictionary<uint, Title>();
    [Parameter] public IReadOnlyDictionary<uint, Neiro> ToneCatalog { get; set; } = new Dictionary<uint, Neiro>();
    [Parameter] public IReadOnlyDictionary<uint, DanData> DanCatalog { get; set; } = new Dictionary<uint, DanData>();
    [Parameter] public bool AllowUnlockEditing { get; set; }
    [Parameter] public EventCallback StateChanged { get; set; }

    private static readonly string[] LanguageStrings =
    {
        "Japanese", "English", "Chinese Traditional", "Korean", "Chinese Simplified"
    };

    private static readonly string[] Ac15DifficultyStrings =
    {
        "No Fixed Course", "Easy", "Normal", "Hard", "Oni"
    };

    private Task OnNameChanged(string _)
        => StateChanged.InvokeAsync();

    private Task OnCostumeChanged(string slot, CostumePickerValue value)
    {
        var state = State.CostumeSlots.Single(item => item.Slot == slot);
        var index = State.CostumeSlots.IndexOf(state);
        State.CostumeSlots[index] = state with { Value = value };
        return StateChanged.InvokeAsync();
    }

    private Task OnTitleChanged(TitlePickerValue value)
    {
        State.Title = value;
        return StateChanged.InvokeAsync();
    }

    private Task OnToneChanged(NeiroPickerValue value)
    {
        State.Tone = value;
        return StateChanged.InvokeAsync();
    }

    private Task OnColorsChanged(ColorPickerValue value)
    {
        State.Colors = value;
        return StateChanged.InvokeAsync();
    }

    private IReadOnlyList<Costume> GetCostumeCatalog(string slot)
        => CostumeCatalog
            .Where(costume => costume.CostumeType == slot || costume.CostumeType == "unknown")
            .OrderBy(costume => costume.CostumeType == "unknown" ? 1 : 0)
            .ThenBy(costume => costume.CostumeId)
            .ToArray();

    private static string GetSlotLabel(string slot)
        => slot switch
        {
            "kigurumi" => "Kigurumi",
            "head" => "Head",
            "body" => "Body",
            "face" => "Face",
            "puchi" => "Puchi",
            _ => slot
        };

    private string FormatDan(uint danId)
        => DanCatalog.TryGetValue(danId, out var dan) && !string.IsNullOrWhiteSpace(dan.Title)
            ? dan.Title
            : $"Dan {danId}";
}
```

- [ ] **Step 2: Modify `Profile.razor.cs` to load separate models**

In `TaikoWebUI/Pages/Profile.razor.cs`, add:

```csharp
using TaikoWebUI.Pages.ProfileEditor;
using TaikoWebUI.Shared.Customize;
```

Replace the single `private UserSetting? response;` field with:

```csharp
private UserSetting? response;
private Ac15ProfileSettingsDto? ac15Response;
private Ac15ProfileEditorState? ac15State;
private PlayerPreviewModel? previewModel;
private bool ProfileLoaded => IsAc15 ? ac15State is not null : response is not null;
```

In `OnInitializedAsync`, replace the current `response = await Client.GetFromJsonAsync<UserSetting>(...)` block with:

```csharp
if (IsAc15)
{
    ac15Response = await Client.GetFromJsonAsync<Ac15ProfileSettingsDto>(
        WebUiEra.Api(CurrentEra, $"Ac15ProfileSettings/{Baid}"));
    ac15Response.ThrowIfNull();
    ac15State = Ac15ProfileEditorState.From(ac15Response);
    previewModel = ac15State.ToPreviewModel();
}
else
{
    response = await Client.GetFromJsonAsync<UserSetting>(WebUiEra.Api(CurrentEra, $"UserSettings/{Baid}"));
    response.ThrowIfNull();
    previewModel = PlayerPreviewModel.FromUserSetting(response);
}
```

Update breadcrumb name selection:

```csharp
var profileName = IsAc15 ? ac15State?.MyDonName : response?.MyDonName;
BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem($"{profileName}", href: null, disabled: true));
```

In `SaveOptions`, replace the current method body with:

```csharp
private async Task SaveOptions()
{
    isSavingOptions = true;

    if (IsAc15)
    {
        ac15State.ThrowIfNull();
        var request = ac15State.ToUpdateDto(CanEditUnlocks);
        await Client.PutAsJsonAsync(WebUiEra.Api(CurrentEra, $"Ac15ProfileSettings/{Baid}"), request);
        previewModel = ac15State.ToPreviewModel();
        BreadcrumbsStateContainer.breadcrumbs[^2] = new BreadcrumbItem($"{ac15State.MyDonName}", href: null, disabled: true);
    }
    else
    {
        response.ThrowIfNull();
        ApplyCustomizationValues();
        await Client.PostAsJsonAsync(WebUiEra.Api(CurrentEra, $"UserSettings/{Baid}"), response);
        previewModel = PlayerPreviewModel.FromUserSetting(response);
        BreadcrumbsStateContainer.breadcrumbs[^2] = new BreadcrumbItem($"{response.MyDonName}", href: null, disabled: true);
    }

    isSavingOptions = false;
}
```

Add this method:

```csharp
private Task RefreshAc15Preview()
{
    if (ac15State is not null)
    {
        previewModel = ac15State.ToPreviewModel();
    }

    return Task.CompletedTask;
}
```

Leave the current Nijiiro customization helpers on `response`; the AC15 editor state owns AC15 slot mapping.

- [ ] **Step 3: Modify `Profile.razor` to render the AC15 editor**

In `TaikoWebUI/Pages/Profile.razor`, change the first guard from:

```razor
@if (response is not null)
```

to:

```razor
@if (ProfileLoaded)
```

Inside the first `MudTabPanel Text="@Localizer["Profile"]"`, insert this AC15 branch before the existing Nijiiro profile controls:

```razor
@if (IsAc15 && ac15State is not null)
{
    <Ac15ProfileEditor State="@ac15State"
                       Era="@CurrentEra"
                       CostumeCatalog="@costumeList"
                       TitleCatalog="@titleDictionary"
                       ToneCatalog="@neiroDictionary"
                       DanCatalog="@danDictionary"
                       AllowUnlockEditing="@CanEditUnlocks"
                       StateChanged="RefreshAc15Preview" />
}
```

Immediately after that AC15 branch, wrap the current non-AC15 profile controls in `else if (response is not null)` without changing their `UserSetting` bindings. Remove the old AC15 `MudSwitch` and `MudSelect` controls that reference `GreenIsTojiru`, `GreenIsAutoCostumeOn`, `Ac15HowToPlayTutorialDisabled`, `GreenDispLevelChassis`, or `GreenDispLevelSelf`.

Replace the `PlayerPreview` call with:

```razor
@if (previewModel is not null)
{
    <PlayerPreview Preview="@previewModel"
                   TitleCatalog="@titleDictionary"
                   ResolveTitlePlateFromCatalog="@IsAc15" />
}
```

For the costume tab, render the existing `UserSetting` pickers only when `!IsAc15 && response is not null`; the AC15 customization controls now live in `Ac15ProfileEditor`.

- [ ] **Step 4: Run a build for Razor compile errors**

Run:

```powershell
dotnet build TaikoLocalServer.slnx
```

Expected: PASS. If a running server locks WebUI outputs, use:

```powershell
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
```

Expected: PASS.

- [ ] **Step 5: Commit profile page migration**

Run:

```powershell
git add TaikoWebUI/Pages/ProfileEditor/Ac15ProfileEditor.razor TaikoWebUI/Pages/Profile.razor TaikoWebUI/Pages/Profile.razor.cs
git commit -m "Move AC15 profile page to profile settings contract"
```

## Task 7: Move Remaining AC15 WebUI Reads Off UserSettings

**Files:**
- Modify: `TaikoWebUI/Pages/HighScores.razor.cs`
- Modify: `TaikoWebUI/Pages/PlayHistory.razor.cs`
- Modify: `TaikoWebUI/Pages/SongList.razor.cs`
- Modify: `TaikoWebUI/Pages/Song.razor.cs`
- Modify: `TaikoWebUI/Pages/DaniDojo.razor.cs`
- Modify: `Tests/WebUi/Ac15ProfileSettingsWebUiTests.cs`

- [ ] **Step 1: Add failing route-helper coverage for breadcrumb reads**

Append this test to `Tests/WebUi/Ac15ProfileSettingsWebUiTests.cs`:

```csharp
[Fact]
public async Task BreadcrumbDisplayNameHelperPreservesEraSpecificRoutes()
{
    var handler = new RecordingHandler();
    using var client = new HttpClient(handler)
    {
        BaseAddress = new Uri("http://localhost/")
    };

    await client.GetProfileDisplayNameAsync("Red", 99);
    await client.GetProfileDisplayNameAsync("Green", 100);
    await client.GetProfileDisplayNameAsync("Nijiiro", 101);

    Assert.Equal(
        [
            "api/Red/Ac15ProfileSettings/99",
            "api/Green/Ac15ProfileSettings/100",
            "api/Nijiiro/UserSettings/101"
        ],
        handler.RequestPaths);
}
```

Update the `RecordingHandler` switch with these responses:

```csharp
"api/Red/Ac15ProfileSettings/99" => """
{
  "era": "Red",
  "baid": 99,
  "identity": { "myDonName": "RED", "myDonNameLanguage": 0 },
  "customization": null,
  "options": {},
  "capabilities": {
    "costumeSlots": [],
    "supportsTitle": false,
    "supportsTone": false,
    "supportsColors": false,
    "supportsDisplayDanOnNamePlate": false,
    "supportsFolderCloseButton": false,
    "supportsAutoCostume": false,
    "supportsHowToPlayTutorialFlag": false,
    "supportsLocalRankingDifficulty": false,
    "supportsDefaultSelectedSelfBestDifficulty": false,
    "supportsTaikojukuFolderDan": false
  },
  "lastPlayDateTime": "1970-01-01T00:00:00Z"
}
""",
"api/Green/Ac15ProfileSettings/100" => """
{
  "era": "Green",
  "baid": 100,
  "identity": { "myDonName": "GREEN", "myDonNameLanguage": 0 },
  "customization": null,
  "options": {},
  "capabilities": {
    "costumeSlots": [],
    "supportsTitle": false,
    "supportsTone": false,
    "supportsColors": false,
    "supportsDisplayDanOnNamePlate": false,
    "supportsFolderCloseButton": false,
    "supportsAutoCostume": false,
    "supportsHowToPlayTutorialFlag": false,
    "supportsLocalRankingDifficulty": false,
    "supportsDefaultSelectedSelfBestDifficulty": false,
    "supportsTaikojukuFolderDan": false
  },
  "lastPlayDateTime": "1970-01-01T00:00:00Z"
}
""",
"api/Nijiiro/UserSettings/101" => """{ "myDonName": "NIJIIRO" }""",
```

- [ ] **Step 2: Run WebUI route-helper tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ProfileSettingsWebUiTests"
```

Expected: PASS. The helper already supports this behavior; this test protects the migration.

- [ ] **Step 3: Replace breadcrumb-only `UserSettings` fetches**

In each of these files:

- `TaikoWebUI/Pages/HighScores.razor.cs`
- `TaikoWebUI/Pages/PlayHistory.razor.cs`
- `TaikoWebUI/Pages/SongList.razor.cs`
- `TaikoWebUI/Pages/Song.razor.cs`
- `TaikoWebUI/Pages/DaniDojo.razor.cs`

Replace:

```csharp
private UserSetting? userSetting;
```

with:

```csharp
private string? profileDisplayName;
```

Replace:

```csharp
userSetting = await Client.GetFromJsonAsync<UserSetting>(WebUiEra.Api(CurrentEra, $"UserSettings/{Baid}"));
```

with:

```csharp
profileDisplayName = await Client.GetProfileDisplayNameAsync(CurrentEra, (uint)Baid);
```

Replace breadcrumb lines of this shape:

```csharp
BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem($"{userSetting?.MyDonName}", href: null, disabled: true));
```

with:

```csharp
BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem($"{profileDisplayName}", href: null, disabled: true));
```

- [ ] **Step 4: Build WebUI migration**

Run:

```powershell
dotnet build TaikoLocalServer.slnx
```

Expected: PASS.

- [ ] **Step 5: Commit remaining WebUI route migration**

Run:

```powershell
git add TaikoWebUI/Pages/HighScores.razor.cs TaikoWebUI/Pages/PlayHistory.razor.cs TaikoWebUI/Pages/SongList.razor.cs TaikoWebUI/Pages/Song.razor.cs TaikoWebUI/Pages/DaniDojo.razor.cs Tests/WebUi/Ac15ProfileSettingsWebUiTests.cs
git commit -m "Move AC15 WebUI profile reads off UserSettings"
```

## Task 8: Retire AC15 UserSettings Path And Mixed DTO Fields

**Files:**
- Modify: `Adapters.AdminApi/Controllers/UserSettingsController.cs`
- Delete: `Adapters.AdminApi/Controllers/UserSettingsController.Blue.cs`
- Delete: `Adapters.AdminApi/Controllers/UserSettingsController.Green.cs`
- Delete: `Adapters.AdminApi/Controllers/UserSettingsController.Yellow.cs`
- Delete: `Adapters.AdminApi/Controllers/UserSettingsController.Red.cs`
- Delete: `Application/Ac15/Ac15UserSettingsAccess.cs`
- Delete: `Application/Ac15/Ac15UserSettingsService.cs`
- Modify: `Contracts.AdminApi/ViewModels/UserSetting.cs`
- Modify: existing Green/Blue/Yellow/Red AdminApi tests that assert AC15 `UserSettings` behavior
- Test: `Tests/Ac15/Ac15ProfileSettingsControllerTests.cs`

- [ ] **Step 1: Add a failing test that AC15 UserSettings is no longer accepted**

Append this test to `Tests/Ac15/Ac15ProfileSettingsControllerTests.cs`:

```csharp
[Fact]
public async Task UserSettingsController_RejectsAc15AfterMigrationAndKeepsNijiiro()
{
    await using var database = await SchemaDatabase.CreateAsync();
    database.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    database.Context.UserSaveDataNijiiro.Add(UserSaveDataNijiiroExtensions.CreateDefaultNijiiroSaveData(1));
    database.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
    await database.Context.SaveChangesAsync();
    var authSettings = new AuthSettings { AuthenticationRequired = false };
    var services = new ServiceCollection()
        .AddSingleton(Options.Create(authSettings))
        .BuildServiceProvider();
    var controller = new UserSettingsController(database.Context, Options.Create(authSettings))
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = services }
        }
    };

    var nijiiro = await controller.GetUserSetting("Nijiiro", 1);
    var blue = await controller.GetUserSetting("Blue", 1);

    Assert.IsType<OkObjectResult>(nijiiro.Result);
    var badRequest = Assert.IsType<BadRequestObjectResult>(blue.Result);
    Assert.Equal("Unsupported game era 'Blue'.", badRequest.Value);
}
```

- [ ] **Step 2: Run the failing cleanup test**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~UserSettingsController_RejectsAc15AfterMigrationAndKeepsNijiiro"
```

Expected: FAIL because the old controller still serves AC15.

- [ ] **Step 3: Remove AC15 branches from `UserSettingsController`**

In `Adapters.AdminApi/Controllers/UserSettingsController.cs`, replace the `GetUserSetting(string era, uint baid)` switch with:

```csharp
return gameEra switch
{
    GameEra.Nijiiro => await GetNijiiroUserSetting(baid),
    _ => EraRoute.BadEra(era)
};
```

Replace the `SaveUserSetting(string era, uint baid, UserSetting userSetting)` switch with:

```csharp
return gameEra switch
{
    GameEra.Nijiiro => await SaveNijiiroUserSetting(baid, userSetting),
    _ => EraRoute.BadEra(era)
};
```

Delete these files:

```text
Adapters.AdminApi/Controllers/UserSettingsController.Blue.cs
Adapters.AdminApi/Controllers/UserSettingsController.Green.cs
Adapters.AdminApi/Controllers/UserSettingsController.Yellow.cs
Adapters.AdminApi/Controllers/UserSettingsController.Red.cs
Application/Ac15/Ac15UserSettingsAccess.cs
Application/Ac15/Ac15UserSettingsService.cs
```

- [ ] **Step 4: Remove AC15-only fields from `UserSetting`**

In `Contracts.AdminApi/ViewModels/UserSetting.cs`, delete these properties:

```csharp
public uint GreenTaikojukuDan { get; set; }

public List<uint> GreenSelectableTaikojukuDans { get; set; } = new();

public bool GreenIsTojiru { get; set; }

public bool GreenIsAutoCostumeOn { get; set; }

public bool Ac15HowToPlayTutorialDisabled { get; set; }

public uint GreenDispLevelChassis { get; set; }

public uint GreenDispLevelSelf { get; set; }
```

Keep `IsDisplayDanOnNamePlate`, customization fields, colors, titles, and tone fields because Nijiiro still uses them.

- [ ] **Step 5: Move old AC15 AdminApi assertions to the new controller tests**

For existing tests in:

- `Tests/Green/GreenAdminApiControllerTests.cs`
- `Tests/Blue/BlueAdminApiParityTests.cs`
- `Tests/Yellow/YellowAdminApiTests.cs`
- `Tests/Red/RedAdminApiTests.cs`

Replace AC15 `UserSettings_*` tests with `Ac15ProfileSettings_*` tests that call `Ac15ProfileSettingsController.Get` and `Put`. Use the DTO assertions from `Tests/Ac15/Ac15ProfileSettingsControllerTests.cs` as the exact shape: assert `Identity`, slot rows, option groups, no cross-era mutation, and `BadRequestObjectResult` for invalid difficulty.

- [ ] **Step 6: Run focused cleanup tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ProfileSettings|FullyQualifiedName~UserSettings"
```

Expected: PASS.

- [ ] **Step 7: Commit cleanup**

Run:

```powershell
git add Adapters.AdminApi/Controllers/UserSettingsController.cs Contracts.AdminApi/ViewModels/UserSetting.cs Tests/Ac15/Ac15ProfileSettingsControllerTests.cs Tests/Green/GreenAdminApiControllerTests.cs Tests/Blue/BlueAdminApiParityTests.cs Tests/Yellow/YellowAdminApiTests.cs Tests/Red/RedAdminApiTests.cs
git add -u Adapters.AdminApi/Controllers/UserSettingsController.Blue.cs Adapters.AdminApi/Controllers/UserSettingsController.Green.cs Adapters.AdminApi/Controllers/UserSettingsController.Yellow.cs Adapters.AdminApi/Controllers/UserSettingsController.Red.cs Application/Ac15/Ac15UserSettingsAccess.cs Application/Ac15/Ac15UserSettingsService.cs
git commit -m "Retire AC15 UserSettings contract"
```

## Task 9: Final Verification

**Files:**
- Verify entire solution and focused profile settings behavior.

- [ ] **Step 1: Run focused AC15 profile tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ProfileSettings|FullyQualifiedName~Ac15ProfileCapabilities|FullyQualifiedName~Ac15ProfileSettingsWebUiTests"
```

Expected: PASS.

- [ ] **Step 2: Run full test suite**

Run:

```powershell
dotnet test Tests/Tests.csproj
```

Expected: PASS.

If test output paths are locked, run:

```powershell
dotnet test Tests/Tests.csproj --artifacts-path "$env:TEMP\TaikoLocalServer-test-artifacts"
```

Expected: PASS.

- [ ] **Step 3: Run full solution build**

Run:

```powershell
dotnet build TaikoLocalServer.slnx
```

Expected: PASS with `0 Error(s)`.

If a running Host locks build output, run:

```powershell
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
```

Expected: PASS with `0 Error(s)`.

- [ ] **Step 4: Inspect for stale AC15 UserSetting references**

Run:

```powershell
rg -n "GreenTaikojukuDan|GreenSelectableTaikojukuDans|GreenIsTojiru|GreenIsAutoCostumeOn|Ac15HowToPlayTutorialDisabled|GreenDispLevelChassis|GreenDispLevelSelf|Ac15UserSettingsAccess|Ac15UserSettingsService" -S
```

Expected: no matches outside historical docs under `docs/superpowers/`.

- [ ] **Step 5: Commit final verification notes if tests required code fixes**

If verification required code edits, run:

```powershell
git add <changed-files>
git commit -m "Fix AC15 profile settings verification issues"
```

If verification required no code edits, do not create a verification-only commit.

## Self-Review

- Spec coverage: Tasks 1-4 implement the AC15-only route, contracts, capabilities, strict unsupported-group validation, slot-based customization, and save-data interfaces. Tasks 5-7 move AC15 WebUI reads and profile editing to `Ac15ProfileSettings` while keeping Nijiiro on `UserSetting`. Task 8 removes the AC15 `UserSettings` contract surface and delegate accessor. Task 9 covers focused and full verification.
- Placeholder scan: The plan contains concrete file paths, test bodies, command lines, expected outcomes, and code snippets for new contracts, service, controller, and WebUI model code.
- Type consistency: `Ac15ProfileSettingsDto`, `Ac15ProfileSettingsUpdateDto`, `Ac15ProfileCapabilities`, `IAc15ProfileSettingsSaveData`, `Ac15ProfileSettingsService`, `Ac15ProfileEditPolicy`, `Ac15ProfileEditorState`, and `PlayerPreviewModel` are introduced before later tasks consume them.
