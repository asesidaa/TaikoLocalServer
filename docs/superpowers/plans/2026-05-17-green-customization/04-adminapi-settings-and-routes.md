# 04 - AdminApi Settings and Catalog Routes

**Goal:** Preserve legacy Nijiiro `/api/usersettings/{baid}` behavior while adding `/api/{era}/usersettings/{baid}` and `/api/{era}/customization/*` for Green.

**Files:**
- Modify: `Adapters.AdminApi/Controllers/UserSettingsController.cs`
- Create: `Adapters.AdminApi/Controllers/UserSettingsController.Nijiiro.cs`
- Create: `Adapters.AdminApi/Controllers/UserSettingsController.Green.cs`
- Create: `Adapters.AdminApi/Controllers/CustomizationCatalogController.cs`
- Modify: `Tests/Green/GreenAdminApiControllerTests.cs`

## Task 1: Controller Tests

- [ ] **Step 1: Add failing AdminApi tests**

Append these tests to `Tests/Green/GreenAdminApiControllerTests.cs`:

```csharp
[Fact]
public async Task UserSettings_Green_GetDecodesCustomizationBitsets()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
    save.Costume1 = 5;
    save.CostumeFlg1 = BitsetCodec.Encode([0, 5], GreenProtocolBytes.CostumeFlagBytes);
    save.TitleFlg = BitsetCodec.Encode([10], GreenProtocolBytes.TitleFlagBytes);
    save.ToneFlg = BitsetCodec.Encode([0, 4], GreenProtocolBytes.ToneFlagBytes);
    save.DefaultToneSetting = 4;
    fixture.Context.UserSaveDataGreen.Add(save);
    await fixture.Context.SaveChangesAsync();

    var controller = CreateUserSettingsController(fixture.Context);
    var result = await controller.GetUserSetting("Green", 1);

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var setting = Assert.IsType<UserSetting>(ok.Value);
    Assert.Equal(5u, setting.Kigurumi);
    Assert.Equal(new List<uint> { 0, 5 }, setting.UnlockedKigurumi);
    Assert.Equal(new List<uint> { 10 }, setting.UnlockedTitle);
    Assert.Equal(new List<uint> { 0, 4 }, setting.UnlockedTone);
    Assert.Equal(4u, setting.ToneId);
}

[Fact]
public async Task UserSettings_Green_PostPersistsUnlockBitsetsWhenEditingIsFree()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
    await fixture.Context.SaveChangesAsync();

    var controller = CreateUserSettingsController(
        fixture.Context,
        new AuthSettings
        {
            AuthenticationRequired = false,
            AllowFreeProfileEditing = true
        });

    var result = await controller.SaveUserSetting("Green", 1, new UserSetting
    {
        MyDonName = "GREEN",
        Kigurumi = 7,
        Head = 8,
        Body = 9,
        Face = 10,
        Puchi = 11,
        UnlockedKigurumi = [0, 7],
        UnlockedHead = [0, 8],
        UnlockedBody = [0, 9],
        UnlockedFace = [0, 10],
        UnlockedPuchi = [0, 11],
        UnlockedTitle = [10],
        UnlockedTone = [0, 4],
        ToneId = 4,
        Title = "Green Title",
        TitlePlateId = 0,
        BodyColor = 2,
        FaceColor = 3,
        LimbColor = 4
    });

    Assert.IsType<NoContentResult>(result);
    var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
    Assert.NotNull(save);
    Assert.True(BitsetCodec.Decode(save!.CostumeFlg1, GreenProtocolBytes.CostumeFlagBytes).Contains(7));
    Assert.True(BitsetCodec.Decode(save.TitleFlg, GreenProtocolBytes.TitleFlagBytes).Contains(10));
    Assert.True(BitsetCodec.Decode(save.ToneFlg, GreenProtocolBytes.ToneFlagBytes).Contains(4));
    Assert.Equal(4u, save.DefaultToneSetting);
    Assert.Equal("GREEN", (await fixture.Context.UserData.FindAsync(1u))!.MyDonName);
}

[Fact]
public async Task CustomizationCatalog_Green_ReturnsCatalogSlices()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    var controller = new CustomizationCatalogController(fixture.Catalog);

    var costumes = Assert.IsType<OkObjectResult>(controller.GetCostumes("Green"));
    var titles = Assert.IsType<OkObjectResult>(controller.GetTitles("Green"));
    var neiros = Assert.IsType<OkObjectResult>(controller.GetNeiros("Green"));

    Assert.IsAssignableFrom<IReadOnlyList<Costume>>(costumes.Value);
    Assert.IsAssignableFrom<IReadOnlyDictionary<uint, Title>>(titles.Value);
    Assert.IsAssignableFrom<IReadOnlyDictionary<uint, Neiro>>(neiros.Value);
}
```

Add this helper near the existing controller helpers:

```csharp
private static UserSettingsController CreateUserSettingsController(
    ITaikoDbContext context,
    AuthSettings? authSettings = null)
{
    var effectiveAuthSettings = authSettings ?? new AuthSettings { AuthenticationRequired = false };
    var httpContext = CreateHttpContext();
    httpContext.RequestServices = new ServiceCollection()
        .AddSingleton(Options.Create(effectiveAuthSettings))
        .BuildServiceProvider();

    return new UserSettingsController(
        context,
        Options.Create(effectiveAuthSettings))
    {
        ControllerContext = new ControllerContext { HttpContext = httpContext }
    };
}
```

- [ ] **Step 2: Run AdminApi tests and confirm failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenAdminApiControllerTests"
```

Expected: FAIL because era-aware user settings and customization catalog routes do not exist.

## Task 2: UserSettings Dispatcher

- [ ] **Step 1: Replace controller shell**

Replace `Adapters.AdminApi/Controllers/UserSettingsController.cs` with:

```csharp
using Microsoft.Extensions.Options;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public partial class UserSettingsController : BaseAdminController<UserSettingsController>
{
    private readonly ITaikoDbContext context;
    private readonly AuthSettings authSettings;

    public UserSettingsController(ITaikoDbContext context, IOptions<AuthSettings> authOptions)
    {
        this.context = context;
        authSettings = authOptions.Value;
    }

    [HttpGet]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<ActionResult<List<UserSetting>>> GetAllUserSetting()
    {
        var users = await context.UserData.Include(d => d.Tokens).ToListAsync();
        var response = new List<UserSetting>();

        foreach (var user in users)
        {
            var saveData = await context.GetOrCreateNijiiroSaveDataAsync(user.Baid, HttpContext.RequestAborted);
            response.Add(BuildNijiiroUserSetting(user, saveData));
        }

        return Ok(response);
    }

    [HttpGet("{baid}")]
    public Task<ActionResult<UserSetting>> GetUserSetting(uint baid)
        => GetUserSetting(nameof(GameEra.Nijiiro), baid);

    [HttpGet("/api/{era}/[controller]/{baid}")]
    public async Task<ActionResult<UserSetting>> GetUserSetting(string era, uint baid)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
        {
            return EraRoute.BadEra(era);
        }

        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
        {
            return forbid;
        }

        return gameEra switch
        {
            GameEra.Nijiiro => await GetNijiiroUserSetting(baid),
            GameEra.Green => await GetGreenUserSetting(baid),
            _ => EraRoute.BadEra(era)
        };
    }

    [HttpPost("{baid}")]
    public Task<IActionResult> SaveUserSetting(uint baid, UserSetting userSetting)
        => SaveUserSetting(nameof(GameEra.Nijiiro), baid, userSetting);

    [HttpPost("/api/{era}/[controller]/{baid}")]
    public async Task<IActionResult> SaveUserSetting(string era, uint baid, UserSetting userSetting)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
        {
            return EraRoute.BadEra(era);
        }

        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
        {
            return forbid;
        }

        return gameEra switch
        {
            GameEra.Nijiiro => await SaveNijiiroUserSetting(baid, userSetting),
            GameEra.Green => await SaveGreenUserSetting(baid, userSetting),
            _ => EraRoute.BadEra(era)
        };
    }

    private bool ShouldEnforceUnlockedOnly()
        => authSettings.AuthenticationRequired
           && !authSettings.AllowFreeProfileEditing
           && !User.IsAdmin();

    private static List<uint> SortedDistinctWithZero(IEnumerable<uint> ids)
        => ids.Append(0).Distinct().OrderBy(id => id).ToList();
}
```

- [ ] **Step 2: Create Nijiiro partial**

Create `Adapters.AdminApi/Controllers/UserSettingsController.Nijiiro.cs` by moving the existing Nijiiro GET, POST body, and `BuildUserSetting` logic from the old controller into these methods:

```csharp
namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public partial class UserSettingsController
{
    private async Task<ActionResult<UserSetting>> GetNijiiroUserSetting(uint baid)
    {
        var user = await context.UserData.Include(d => d.Tokens).FirstOrDefaultAsync(d => d.Baid == baid);
        if (user is null)
        {
            return NotFound();
        }

        var saveData = await context.GetOrCreateNijiiroSaveDataAsync(baid, HttpContext.RequestAborted);
        return Ok(BuildNijiiroUserSetting(user, saveData));
    }

    private async Task<IActionResult> SaveNijiiroUserSetting(uint baid, UserSetting userSetting)
    {
        var user = await context.UserData.FindAsync(baid);
        if (user is null)
        {
            return NotFound();
        }

        var saveData = await context.GetOrCreateNijiiroSaveDataAsync(baid, HttpContext.RequestAborted);
        var enforceUnlockedOnly = ShouldEnforceUnlockedOnly();

        saveData.IsSkipOn = userSetting.IsSkipOn;
        saveData.IsVoiceOn = userSetting.IsVoiceOn;
        saveData.DisplayAchievement = userSetting.IsDisplayAchievement;
        saveData.DisplayDan = userSetting.IsDisplayDanOnNamePlate;
        saveData.DisplaySouUchi = userSetting.IsDisplaySouUchi;
        saveData.DifficultySettingCourse = userSetting.DifficultySettingCourse;
        saveData.DifficultySettingStar = userSetting.DifficultySettingStar;
        saveData.DifficultySettingSort = userSetting.DifficultySettingSort;
        saveData.NotesPosition = userSetting.NotesPosition;
        saveData.SelectedToneId = userSetting.ToneId;
        saveData.AchievementDisplayDifficulty = userSetting.AchievementDisplayDifficulty;
        saveData.OptionSetting = PlaySettingConverter.PlaySettingToShort(userSetting.PlaySetting);
        user.MyDonName = userSetting.MyDonName;
        user.MyDonNameLanguage = userSetting.MyDonNameLanguage;

        if (enforceUnlockedOnly)
        {
            var unlockedKigurumi = saveData.UnlockedKigurumi.ToHashSet();
            var unlockedHead = saveData.UnlockedHead.ToHashSet();
            var unlockedBody = saveData.UnlockedBody.ToHashSet();
            var unlockedFace = saveData.UnlockedFace.ToHashSet();
            var unlockedPuchi = saveData.UnlockedPuchi.ToHashSet();
            var unlockedTitle = saveData.TitleFlgArray.ToHashSet();

            saveData.Title = unlockedTitle.Contains(userSetting.TitlePlateId) ? userSetting.Title : saveData.Title;
            saveData.TitlePlateId = unlockedTitle.Contains(userSetting.TitlePlateId) ? userSetting.TitlePlateId : saveData.TitlePlateId;
            saveData.CurrentKigurumi = unlockedKigurumi.Contains(userSetting.Kigurumi) ? userSetting.Kigurumi : saveData.CurrentKigurumi;
            saveData.CurrentHead = unlockedHead.Contains(userSetting.Head) ? userSetting.Head : saveData.CurrentHead;
            saveData.CurrentBody = unlockedBody.Contains(userSetting.Body) ? userSetting.Body : saveData.CurrentBody;
            saveData.CurrentFace = unlockedFace.Contains(userSetting.Face) ? userSetting.Face : saveData.CurrentFace;
            saveData.CurrentPuchi = unlockedPuchi.Contains(userSetting.Puchi) ? userSetting.Puchi : saveData.CurrentPuchi;
            saveData.ColorBody = userSetting.BodyColor;
            saveData.ColorFace = userSetting.FaceColor;
            saveData.ColorLimb = userSetting.LimbColor;
        }
        else
        {
            saveData.Title = userSetting.Title;
            saveData.TitlePlateId = userSetting.TitlePlateId;
            saveData.ColorBody = userSetting.BodyColor;
            saveData.ColorFace = userSetting.FaceColor;
            saveData.ColorLimb = userSetting.LimbColor;
            saveData.CurrentKigurumi = userSetting.Kigurumi;
            saveData.CurrentHead = userSetting.Head;
            saveData.CurrentBody = userSetting.Body;
            saveData.CurrentFace = userSetting.Face;
            saveData.CurrentPuchi = userSetting.Puchi;
        }

        saveData.ToneFlgArray = saveData.ToneFlgArray
            .Append(0u)
            .Append(userSetting.ToneId)
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private static UserSetting BuildNijiiroUserSetting(UserDatum user, UserSaveDataNijiiro saveData)
    {
        List<List<uint>> costumeUnlockData =
        [
            saveData.UnlockedKigurumi,
            saveData.UnlockedHead,
            saveData.UnlockedBody,
            saveData.UnlockedFace,
            saveData.UnlockedPuchi
        ];

        for (var i = 0; i < 5; i++)
        {
            if (!costumeUnlockData[i].Contains(0))
            {
                costumeUnlockData[i].Add(0);
            }
        }

        return new UserSetting
        {
            Baid = user.Baid,
            AchievementDisplayDifficulty = saveData.AchievementDisplayDifficulty,
            IsDisplayAchievement = saveData.DisplayAchievement,
            IsDisplayDanOnNamePlate = saveData.DisplayDan,
            IsDisplaySouUchi = saveData.DisplaySouUchi,
            DifficultySettingCourse = saveData.DifficultySettingCourse,
            DifficultySettingStar = saveData.DifficultySettingStar,
            DifficultySettingSort = saveData.DifficultySettingSort,
            IsVoiceOn = saveData.IsVoiceOn,
            IsSkipOn = saveData.IsSkipOn,
            NotesPosition = saveData.NotesPosition,
            PlaySetting = PlaySettingConverter.ShortToPlaySetting(saveData.OptionSetting),
            ToneId = saveData.SelectedToneId,
            MyDonName = user.MyDonName,
            MyDonNameLanguage = user.MyDonNameLanguage,
            Title = saveData.Title,
            TitlePlateId = saveData.TitlePlateId,
            Kigurumi = saveData.CurrentKigurumi,
            Head = saveData.CurrentHead,
            Body = saveData.CurrentBody,
            Face = saveData.CurrentFace,
            Puchi = saveData.CurrentPuchi,
            UnlockedKigurumi = costumeUnlockData[0],
            UnlockedHead = costumeUnlockData[1],
            UnlockedBody = costumeUnlockData[2],
            UnlockedFace = costumeUnlockData[3],
            UnlockedPuchi = costumeUnlockData[4],
            UnlockedTitle = saveData.TitleFlgArray.ToList(),
            UnlockedTone = saveData.ToneFlgArray.ToList(),
            BodyColor = saveData.ColorBody,
            FaceColor = saveData.ColorFace,
            LimbColor = saveData.ColorLimb,
            LastPlayDateTime = saveData.LastPlayDatetime
        };
    }
}
```

- [ ] **Step 3: Create Green partial**

Create `Adapters.AdminApi/Controllers/UserSettingsController.Green.cs`:

```csharp
namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public partial class UserSettingsController
{
    private async Task<ActionResult<UserSetting>> GetGreenUserSetting(uint baid)
    {
        var user = await context.UserData.Include(d => d.Tokens).FirstOrDefaultAsync(d => d.Baid == baid);
        if (user is null)
        {
            return NotFound();
        }

        var saveData = await context.GetOrCreateGreenSaveDataAsync(baid, HttpContext.RequestAborted);
        return Ok(BuildGreenUserSetting(user, saveData));
    }

    private async Task<IActionResult> SaveGreenUserSetting(uint baid, UserSetting userSetting)
    {
        var user = await context.UserData.FindAsync(baid);
        if (user is null)
        {
            return NotFound();
        }

        var saveData = await context.GetOrCreateGreenSaveDataAsync(baid, HttpContext.RequestAborted);
        var enforceUnlockedOnly = ShouldEnforceUnlockedOnly();

        user.MyDonName = userSetting.MyDonName;
        user.MyDonNameLanguage = userSetting.MyDonNameLanguage;
        saveData.Title = userSetting.Title;
        saveData.TitleplateId = userSetting.TitlePlateId;
        saveData.ColorBody = userSetting.BodyColor;
        saveData.ColorFace = userSetting.FaceColor;
        saveData.ColorLimb = userSetting.LimbColor;

        if (enforceUnlockedOnly)
        {
            var unlockedKigurumi = BitsetCodec.Decode(saveData.CostumeFlg1, GreenProtocolBytes.CostumeFlagBytes).ToHashSet();
            var unlockedHead = BitsetCodec.Decode(saveData.CostumeFlg2, GreenProtocolBytes.CostumeFlagBytes).ToHashSet();
            var unlockedBody = BitsetCodec.Decode(saveData.CostumeFlg3, GreenProtocolBytes.CostumeFlagBytes).ToHashSet();
            var unlockedFace = BitsetCodec.Decode(saveData.CostumeFlg4, GreenProtocolBytes.CostumeFlagBytes).ToHashSet();
            var unlockedPuchi = BitsetCodec.Decode(saveData.CostumeFlg5, GreenProtocolBytes.CostumeFlagBytes).ToHashSet();
            var unlockedTone = BitsetCodec.Decode(saveData.ToneFlg, GreenProtocolBytes.ToneFlagBytes).ToHashSet();

            saveData.Costume1 = unlockedKigurumi.Contains(userSetting.Kigurumi) ? userSetting.Kigurumi : saveData.Costume1;
            saveData.Costume2 = unlockedHead.Contains(userSetting.Head) ? userSetting.Head : saveData.Costume2;
            saveData.Costume3 = unlockedBody.Contains(userSetting.Body) ? userSetting.Body : saveData.Costume3;
            saveData.Costume4 = unlockedFace.Contains(userSetting.Face) ? userSetting.Face : saveData.Costume4;
            saveData.Costume5 = unlockedPuchi.Contains(userSetting.Puchi) ? userSetting.Puchi : saveData.Costume5;
            saveData.DefaultToneSetting = unlockedTone.Contains(userSetting.ToneId) ? userSetting.ToneId : saveData.DefaultToneSetting;
        }
        else
        {
            saveData.Costume1 = userSetting.Kigurumi;
            saveData.Costume2 = userSetting.Head;
            saveData.Costume3 = userSetting.Body;
            saveData.Costume4 = userSetting.Face;
            saveData.Costume5 = userSetting.Puchi;
            saveData.DefaultToneSetting = userSetting.ToneId;
            saveData.CostumeFlg1 = BitsetCodec.Encode(SortedDistinctWithZero(userSetting.UnlockedKigurumi).Append(userSetting.Kigurumi), GreenProtocolBytes.CostumeFlagBytes);
            saveData.CostumeFlg2 = BitsetCodec.Encode(SortedDistinctWithZero(userSetting.UnlockedHead).Append(userSetting.Head), GreenProtocolBytes.CostumeFlagBytes);
            saveData.CostumeFlg3 = BitsetCodec.Encode(SortedDistinctWithZero(userSetting.UnlockedBody).Append(userSetting.Body), GreenProtocolBytes.CostumeFlagBytes);
            saveData.CostumeFlg4 = BitsetCodec.Encode(SortedDistinctWithZero(userSetting.UnlockedFace).Append(userSetting.Face), GreenProtocolBytes.CostumeFlagBytes);
            saveData.CostumeFlg5 = BitsetCodec.Encode(SortedDistinctWithZero(userSetting.UnlockedPuchi).Append(userSetting.Puchi), GreenProtocolBytes.CostumeFlagBytes);
            saveData.TitleFlg = BitsetCodec.Encode(userSetting.UnlockedTitle, GreenProtocolBytes.TitleFlagBytes);
            saveData.ToneFlg = BitsetCodec.Encode(SortedDistinctWithZero(userSetting.UnlockedTone).Append(userSetting.ToneId), GreenProtocolBytes.ToneFlagBytes);
        }

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private static UserSetting BuildGreenUserSetting(UserDatum user, UserSaveDataGreen saveData)
    {
        return new UserSetting
        {
            Baid = user.Baid,
            ToneId = saveData.DefaultToneSetting,
            MyDonName = user.MyDonName,
            MyDonNameLanguage = user.MyDonNameLanguage,
            Title = saveData.Title,
            TitlePlateId = saveData.TitleplateId,
            Kigurumi = saveData.Costume1,
            Head = saveData.Costume2,
            Body = saveData.Costume3,
            Face = saveData.Costume4,
            Puchi = saveData.Costume5,
            UnlockedKigurumi = BitsetCodec.Decode(saveData.CostumeFlg1, GreenProtocolBytes.CostumeFlagBytes),
            UnlockedHead = BitsetCodec.Decode(saveData.CostumeFlg2, GreenProtocolBytes.CostumeFlagBytes),
            UnlockedBody = BitsetCodec.Decode(saveData.CostumeFlg3, GreenProtocolBytes.CostumeFlagBytes),
            UnlockedFace = BitsetCodec.Decode(saveData.CostumeFlg4, GreenProtocolBytes.CostumeFlagBytes),
            UnlockedPuchi = BitsetCodec.Decode(saveData.CostumeFlg5, GreenProtocolBytes.CostumeFlagBytes),
            UnlockedTitle = BitsetCodec.Decode(saveData.TitleFlg, GreenProtocolBytes.TitleFlagBytes),
            UnlockedTone = BitsetCodec.Decode(saveData.ToneFlg, GreenProtocolBytes.ToneFlagBytes),
            BodyColor = saveData.ColorBody,
            FaceColor = saveData.ColorFace,
            LimbColor = saveData.ColorLimb,
            LastPlayDateTime = saveData.LastPlayDatetime
        };
    }
}
```

## Task 3: Customization Catalog Routes

- [ ] **Step 1: Create catalog controller**

Create `Adapters.AdminApi/Controllers/CustomizationCatalogController.cs`:

```csharp
namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/{era}/customization")]
[Authorize]
public class CustomizationCatalogController(IGameDataCatalog catalog) : BaseAdminController<CustomizationCatalogController>
{
    [HttpGet("costumes")]
    public IActionResult GetCostumes(string era)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
        {
            return EraRoute.BadEra(era);
        }

        return gameEra switch
        {
            GameEra.Nijiiro => Ok(catalog.Nijiiro().GetCostumeList()),
            GameEra.Green => Ok(catalog.Green().GetCostumeList()),
            _ => EraRoute.BadEra(era)
        };
    }

    [HttpGet("titles")]
    public IActionResult GetTitles(string era)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
        {
            return EraRoute.BadEra(era);
        }

        return gameEra switch
        {
            GameEra.Nijiiro => Ok(catalog.Nijiiro().GetTitleDictionary()),
            GameEra.Green => Ok(catalog.Green().GetTitleDictionary()),
            _ => EraRoute.BadEra(era)
        };
    }

    [HttpGet("neiros")]
    public IActionResult GetNeiros(string era)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
        {
            return EraRoute.BadEra(era);
        }

        return gameEra switch
        {
            GameEra.Nijiiro => Ok(catalog.Nijiiro().GetNeiroDictionary()),
            GameEra.Green => Ok(catalog.Green().GetNeiroDictionary()),
            _ => EraRoute.BadEra(era)
        };
    }
}
```

- [ ] **Step 2: Run AdminApi tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenAdminApiControllerTests"
```

Expected: PASS.

- [ ] **Step 3: Build**

Run:

```powershell
dotnet build
```

Expected: PASS.

- [ ] **Step 4: Commit**

```powershell
git add -- Adapters.AdminApi/Controllers/UserSettingsController.cs Adapters.AdminApi/Controllers/UserSettingsController.Nijiiro.cs Adapters.AdminApi/Controllers/UserSettingsController.Green.cs Adapters.AdminApi/Controllers/CustomizationCatalogController.cs Tests/Green/GreenAdminApiControllerTests.cs
git commit -m "Add era-aware customization settings APIs"
```
