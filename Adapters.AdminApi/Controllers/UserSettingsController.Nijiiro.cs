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
        return Ok(BuildNijiiroUserSetting(user, saveData, await context.UserWebProfiles.FindAsync(baid)));
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
        // Custom titles off: free text is refused (the current title is kept); a game title in any language is fine.
        if (access.LoginRequired && !access.Rules.AllowCustomTitles && !User.IsAdmin() && userSetting.Title != saveData.Title
            && !catalog.Nijiiro().GetTitleDictionary().Values.Any(t => userSetting.Title == t.TitleName || userSetting.Title == t.TitleNameEN
                                                                      || userSetting.Title == t.TitleNameCN || userSetting.Title == t.TitleNameKO))
            userSetting.Title = saveData.Title;

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
        // Body colours are independent of unlock state: the palette is always free.
        saveData.ColorBody = userSetting.BodyColor;
        saveData.ColorFace = userSetting.FaceColor;
        saveData.ColorLimb = userSetting.LimbColor;

        if (enforceUnlockedOnly)
        {
            // Costume and title fields can only hold what the player has already unlocked: the request's unlocked
            // lists are ignored (they're the server's), and the equipped IDs and title are kept to the stored sets.
            var unlockedTitle = saveData.TitleFlgArray.ToHashSet();
            if (unlockedTitle.Contains(userSetting.TitlePlateId))
            {
                saveData.Title = userSetting.Title;
                saveData.TitlePlateId = userSetting.TitlePlateId;
            }
            saveData.CurrentKigurumi = saveData.UnlockedKigurumi.Contains(userSetting.Kigurumi) ? userSetting.Kigurumi : saveData.CurrentKigurumi;
            saveData.CurrentHead = saveData.UnlockedHead.Contains(userSetting.Head) ? userSetting.Head : saveData.CurrentHead;
            saveData.CurrentBody = saveData.UnlockedBody.Contains(userSetting.Body) ? userSetting.Body : saveData.CurrentBody;
            saveData.CurrentFace = saveData.UnlockedFace.Contains(userSetting.Face) ? userSetting.Face : saveData.CurrentFace;
            saveData.CurrentPuchi = saveData.UnlockedPuchi.Contains(userSetting.Puchi) ? userSetting.Puchi : saveData.CurrentPuchi;
        }
        else
        {
            saveData.Title = userSetting.Title;
            saveData.TitlePlateId = userSetting.TitlePlateId;
            saveData.CurrentKigurumi = userSetting.Kigurumi;
            saveData.CurrentHead = userSetting.Head;
            saveData.CurrentBody = userSetting.Body;
            saveData.CurrentFace = userSetting.Face;
            saveData.CurrentPuchi = userSetting.Puchi;
        }

        // A tone picked here is unlocked (as before; the tone list isn't restricted).
        saveData.ToneFlgArray = SortedDistinctWithZero(saveData.ToneFlgArray.Append(userSetting.ToneId));

        // WebUI-only card settings live in UserWebProfiles.
        var profile = await context.UserWebProfiles.FindAsync(baid);
        if (profile is not null)
        {
            profile.CardColor = userSetting.CardColor < 63 ? userSetting.CardColor : profile.CardColor;
            profile.CardPattern = Enum.IsDefined(userSetting.CardPattern) ? userSetting.CardPattern : profile.CardPattern;
            profile.CardFade = userSetting.CardFade is >= -1 and < 63 ? userSetting.CardFade : profile.CardFade;
            profile.CardAnimation = Enum.IsDefined(userSetting.CardAnimation) ? userSetting.CardAnimation : profile.CardAnimation;
        }

        this.LogIfSomeoneElse(context, "profile.edit", baid);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        // The costume or pose may have changed: render the avatar (it skips when it still matches).
        avatars.Enqueue(baid, changed: true);
        return NoContent();
    }

    internal static UserSetting BuildNijiiroUserSetting(UserDatum user, UserSaveDataNijiiro saveData, UserWebProfile? profile)
    {
        profile ??= new UserWebProfile();
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
            LastPlayDateTime = saveData.LastPlayDatetime,
            CardColor = profile.CardColor,
            CardPattern = profile.CardPattern,
            CardFade = profile.CardFade,
            CardAnimation = profile.CardAnimation,
        };
    }
}
