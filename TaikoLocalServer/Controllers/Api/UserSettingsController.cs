using SharedProject.Models;
using SharedProject.Utils;
using System.Text.Json;

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
    public async Task<ActionResult<UserSetting>> GetUserSetting(ulong baid)
    {
        var user = await userDatumService.GetFirstUserDatumOrNull(baid);

        if (user is null)
        {
            return NotFound();
        }

        var difficultySettingArray = JsonHelper.GetUIntArrayFromJson(user.DifficultySettingArray, 3, Logger, nameof(user.DifficultySettingArray));

        var costumeData = JsonHelper.GetCostumeDataFromUserData(user, Logger);

        var costumeUnlockData = JsonHelper.GetCostumeUnlockDataFromUserData(user, Logger);

        var response = new UserSetting
        {
            AchievementDisplayDifficulty = user.AchievementDisplayDifficulty,
            IsDisplayAchievement = user.DisplayAchievement,
            IsDisplayDanOnNamePlate = user.DisplayDan,
            DifficultySettingCourse = difficultySettingArray[0],
            DifficultySettingStar = difficultySettingArray[1],
            DifficultySettingSort = difficultySettingArray[2],
            IsVoiceOn = user.IsVoiceOn,
            IsSkipOn = user.IsSkipOn,
            NotesPosition = user.NotesPosition,
            PlaySetting = PlaySettingConverter.ShortToPlaySetting(user.OptionSetting),
            ToneId = user.SelectedToneId,
            MyDonName = user.MyDonName,
            MyDonNameLanguage = user.MyDonNameLanguage,
            Title = user.Title,
            TitlePlateId = user.TitlePlateId,
            Kigurumi = costumeData[0],
            Head = costumeData[1],
            Body = costumeData[2],
            Face = costumeData[3],
            Puchi = costumeData[4],
            UnlockedKigurumi = costumeUnlockData[0],
            UnlockedHead = costumeUnlockData[1],
            UnlockedBody = costumeUnlockData[2],
            UnlockedFace = costumeUnlockData[3],
            UnlockedPuchi = costumeUnlockData[4],
            BodyColor = user.ColorBody,
            FaceColor = user.ColorFace,
            LimbColor = user.ColorLimb
        };
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> SaveUserSetting(ulong baid, UserSetting userSetting)
    {
        var user = await userDatumService.GetFirstUserDatumOrNull(baid);

        if (user is null)
        {
            return NotFound();
        }

        var costumes = new List<uint>
        {
            userSetting.Kigurumi,
            userSetting.Head,
            userSetting.Body,
            userSetting.Face,
            userSetting.Puchi,
        };

        var difficultySettings = new List<uint>
        {
            userSetting.DifficultySettingCourse,
            userSetting.DifficultySettingStar,
            userSetting.DifficultySettingSort
        };

        user.IsSkipOn = userSetting.IsSkipOn;
        user.IsVoiceOn = userSetting.IsVoiceOn;
        user.DisplayAchievement = userSetting.IsDisplayAchievement;
        user.DisplayDan = userSetting.IsDisplayDanOnNamePlate;
        user.DifficultySettingArray = JsonSerializer.Serialize(difficultySettings);
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
        user.CostumeData = JsonSerializer.Serialize(costumes);

        await userDatumService.UpdateUserDatum(user);

        return NoContent();
    }

}