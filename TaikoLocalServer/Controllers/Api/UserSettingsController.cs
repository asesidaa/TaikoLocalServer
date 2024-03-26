using SharedProject.Models;
using SharedProject.Utils;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("/api/[controller]/{baid}")]
public class UserSettingsController : BaseController<UserSettingsController>
{
    private readonly IUserDatumService userDatumService;

    public UserSettingsController(IUserDatumService userDatumService)
    {
        this.userDatumService = userDatumService;
    }

    [HttpGet]
    public async Task<ActionResult<UserSetting>> GetUserSetting(uint baid)
    {
        var user = await userDatumService.GetFirstUserDatumOrNull(baid);

        if (user is null)
        {
            return NotFound();
        }

        List<List<uint>> costumeUnlockData = 
            [user.UnlockedKigurumi, user.UnlockedHead, user.UnlockedBody, user.UnlockedFace, user.UnlockedPuchi];

        var unlockedTitle = user.TitleFlgArray
            .ToList();

        for (var i = 0; i < 5; i++)
        {
            if (!costumeUnlockData[i].Contains(0))
            {
                costumeUnlockData[i].Add(0);
            }
        }

        var response = new UserSetting
        {
            AchievementDisplayDifficulty = user.AchievementDisplayDifficulty,
            IsDisplayAchievement = user.DisplayAchievement,
            IsDisplayDanOnNamePlate = user.DisplayDan,
            DifficultySettingCourse = user.DifficultySettingCourse,
            DifficultySettingStar = user.DifficultySettingStar,
            DifficultySettingSort = user.DifficultySettingSort,
            IsVoiceOn = user.IsVoiceOn,
            IsSkipOn = user.IsSkipOn,
            NotesPosition = user.NotesPosition,
            PlaySetting = PlaySettingConverter.ShortToPlaySetting(user.OptionSetting),
            ToneId = user.SelectedToneId,
            MyDonName = user.MyDonName,
            MyDonNameLanguage = user.MyDonNameLanguage,
            Title = user.Title,
            TitlePlateId = user.TitlePlateId,
            Kigurumi = user.CurrentKigurumi,
            Head = user.CurrentHead,
            Body = user.CurrentBody,
            Face = user.CurrentFace,
            Puchi = user.CurrentPuchi,
            UnlockedKigurumi = costumeUnlockData[0],
            UnlockedHead = costumeUnlockData[1],
            UnlockedBody = costumeUnlockData[2],
            UnlockedFace = costumeUnlockData[3],
            UnlockedPuchi = costumeUnlockData[4],
            UnlockedTitle = unlockedTitle,
            BodyColor = user.ColorBody,
            FaceColor = user.ColorFace,
            LimbColor = user.ColorLimb,
            LastPlayDateTime = user.LastPlayDatetime
        };
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> SaveUserSetting(uint baid, UserSetting userSetting)
    {
        var user = await userDatumService.GetFirstUserDatumOrNull(baid);

        if (user is null)
        {
            return NotFound();
        }

        user.IsSkipOn = userSetting.IsSkipOn;
        user.IsVoiceOn = userSetting.IsVoiceOn;
        user.DisplayAchievement = userSetting.IsDisplayAchievement;
        user.DisplayDan = userSetting.IsDisplayDanOnNamePlate;
        user.DifficultySettingCourse = userSetting.DifficultySettingCourse;
        user.DifficultySettingStar = userSetting.DifficultySettingStar;
        user.DifficultySettingSort = userSetting.DifficultySettingSort;
        user.NotesPosition = userSetting.NotesPosition;
        user.SelectedToneId = userSetting.ToneId;
        user.AchievementDisplayDifficulty = userSetting.AchievementDisplayDifficulty;
        user.OptionSetting = PlaySettingConverter.PlaySettingToShort(userSetting.PlaySetting);
        user.MyDonName = userSetting.MyDonName;
        user.MyDonNameLanguage = userSetting.MyDonNameLanguage;
        user.Title = userSetting.Title;
        user.TitlePlateId = userSetting.TitlePlateId;
        user.ColorBody = userSetting.BodyColor;
        user.ColorFace = userSetting.FaceColor;
        user.ColorLimb = userSetting.LimbColor;
        user.CurrentKigurumi = userSetting.Kigurumi;
        user.CurrentHead = userSetting.Head;
        user.CurrentBody = userSetting.Body;
        user.CurrentFace = userSetting.Face;
        user.CurrentPuchi = userSetting.Puchi;

        // If a locked tone is selected, unlock it
        var toneFlg = user.ToneFlgArray;
        toneFlg = toneFlg.Append(0u).Append(userSetting.ToneId).Distinct().ToList();

        user.ToneFlgArray = toneFlg;

        await userDatumService.UpdateUserDatum(user);

        return NoContent();
    }

}