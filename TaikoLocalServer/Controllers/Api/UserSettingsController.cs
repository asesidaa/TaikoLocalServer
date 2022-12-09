using System.Text.Json;
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

        if (user is null) return NotFound();

        var costumeData = JsonHelper.GetCostumeDataFromUserData(user, Logger);

        var costumeUnlockData = JsonHelper.GetCostumeUnlockDataFromUserData(user, Logger);

        var response = new UserSetting
        {
            AchievementDisplayDifficulty = user.AchievementDisplayDifficulty,
            IsDisplayAchievement = user.DisplayAchievement,
            IsDisplayDanOnNamePlate = user.DisplayDan,
            IsVoiceOn = user.IsVoiceOn,
            IsSkipOn = user.IsSkipOn,
            NotesPosition = user.NotesPosition,
            PlaySetting = PlaySettingConverter.ShortToPlaySetting(user.OptionSetting),
            ToneId = user.SelectedToneId,
            MyDonName = user.MyDonName,
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
    public async Task<IActionResult> SaveUserSetting(uint baid, UserSetting userSetting)
    {
        var user = await userDatumService.GetFirstUserDatumOrNull(baid);

        if (user is null) return NotFound();

        var costumes = new List<uint>
        {
            userSetting.Kigurumi,
            userSetting.Head,
            userSetting.Body,
            userSetting.Face,
            userSetting.Puchi
        };

        user.IsSkipOn = userSetting.IsSkipOn;
        user.IsVoiceOn = userSetting.IsVoiceOn;
        user.DisplayAchievement = userSetting.IsDisplayAchievement;
        user.DisplayDan = userSetting.IsDisplayDanOnNamePlate;
        user.NotesPosition = userSetting.NotesPosition;
        user.SelectedToneId = userSetting.ToneId;
        user.AchievementDisplayDifficulty = userSetting.AchievementDisplayDifficulty;
        user.OptionSetting = PlaySettingConverter.PlaySettingToShort(userSetting.PlaySetting);
        user.MyDonName = userSetting.MyDonName;
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