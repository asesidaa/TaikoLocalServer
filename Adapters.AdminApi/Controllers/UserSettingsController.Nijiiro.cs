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

        saveData.ToneFlgArray = SortedDistinctWithZero(saveData.ToneFlgArray.Append(userSetting.ToneId));
        saveData.UnlockedKigurumi = SortedDistinctWithZero(saveData.UnlockedKigurumi.Append(saveData.CurrentKigurumi));
        saveData.UnlockedHead = SortedDistinctWithZero(saveData.UnlockedHead.Append(saveData.CurrentHead));
        saveData.UnlockedBody = SortedDistinctWithZero(saveData.UnlockedBody.Append(saveData.CurrentBody));
        saveData.UnlockedFace = SortedDistinctWithZero(saveData.UnlockedFace.Append(saveData.CurrentFace));
        saveData.UnlockedPuchi = SortedDistinctWithZero(saveData.UnlockedPuchi.Append(saveData.CurrentPuchi));

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
