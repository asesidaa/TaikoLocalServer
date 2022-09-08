using System.Buffers.Binary;
using SharedProject.Models;
using SharedProject.Models.Responses;
using SharedProject.Utils;
using TaikoLocalServer.Services;
using TaikoLocalServer.Services.Interfaces;

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
            TitlePlateId = user.TitlePlateId
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
        user.NotesPosition = userSetting.NotesPosition;
        user.SelectedToneId = userSetting.ToneId;
        user.AchievementDisplayDifficulty = userSetting.AchievementDisplayDifficulty;
        user.OptionSetting = PlaySettingConverter.PlaySettingToShort(userSetting.PlaySetting);
        user.MyDonName = userSetting.MyDonName;
        user.Title = userSetting.Title;
        user.TitlePlateId = userSetting.TitlePlateId;

        await userDatumService.UpdateUserDatum(user);

        return NoContent();
    }

}