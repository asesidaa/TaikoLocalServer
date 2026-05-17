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

        if (ShouldEnforceUnlockedOnly())
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

        saveData.ToneFlgArray = SortedDistinctWithZero(saveData.ToneFlgArray.Append(userSetting.ToneId));

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private static UserSetting BuildNijiiroUserSetting(UserDatum user, UserSaveDataNijiiro saveData)
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
            UnlockedTone = saveData.ToneFlgArray.ToList(),
            BodyColor = saveData.ColorBody,
            FaceColor = saveData.ColorFace,
            LimbColor = saveData.ColorLimb,
            LastPlayDateTime = saveData.LastPlayDatetime
        };
    }
}
