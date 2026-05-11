using Microsoft.Extensions.Options;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize]
public class UserSettingsController(ITaikoDbContext context, IOptions<AuthSettings> authOptions) : BaseAdminController<UserSettingsController>
{
    private readonly AuthSettings authSettings = authOptions.Value;

    [HttpGet]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<ActionResult<List<UserSetting>>> GetAllUserSetting()
    {
        var users = await context.UserData.Include(d => d.Tokens).ToListAsync();

        var response = new List<UserSetting>();
        foreach (var user in users)
        {
            var saveData = await context.GetOrCreateNijiiroSaveDataAsync(user.Baid, HttpContext.RequestAborted);
            response.Add(BuildUserSetting(user, saveData));
        }

        return Ok(response);
    }


    [HttpGet("{baid}")]
    public async Task<ActionResult<UserSetting>> GetUserSetting(uint baid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        var user = await context.UserData.Include(d => d.Tokens).FirstOrDefaultAsync(d => d.Baid == baid);
        if (user is null)
        {
            return NotFound();
        }

        var saveData = await context.GetOrCreateNijiiroSaveDataAsync(baid, HttpContext.RequestAborted);
        return Ok(BuildUserSetting(user, saveData));
    }

    [HttpPost("{baid}")]
    public async Task<IActionResult> SaveUserSetting(uint baid, UserSetting userSetting)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        var user = await context.UserData.FindAsync(baid);
        if (user is null)
        {
            return NotFound();
        }
        var saveData = await context.GetOrCreateNijiiroSaveDataAsync(baid, HttpContext.RequestAborted);

        var enforceUnlockedOnly = authSettings.AuthenticationRequired
                                  && !authSettings.AllowFreeProfileEditing
                                  && !User.IsAdmin();

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
            // Costume / title fields can only contain values the user has already unlocked.
            // Ignore the request payload's *unlocked* lists entirely (those are server-owned),
            // and clamp equipped IDs + the current title to the persisted unlocked sets.
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
            // Body colors are independent of unlock state; users can always pick from the palette.
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

        // If a locked tone is selected, unlock it
        var toneFlg = saveData.ToneFlgArray;
        toneFlg = toneFlg.Append(0u).Append(userSetting.ToneId).Distinct().ToList();
        saveData.ToneFlgArray = toneFlg;

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private static UserSetting BuildUserSetting(UserDatum user, UserSaveDataNijiiro saveData)
    {
        List<List<uint>> costumeUnlockData =
            [saveData.UnlockedKigurumi, saveData.UnlockedHead, saveData.UnlockedBody, saveData.UnlockedFace, saveData.UnlockedPuchi];

        var unlockedTitle = saveData.TitleFlgArray.ToList();

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
            UnlockedTitle = unlockedTitle,
            BodyColor = saveData.ColorBody,
            FaceColor = saveData.ColorFace,
            LimbColor = saveData.ColorLimb,
            LastPlayDateTime = saveData.LastPlayDatetime
        };
    }
}
