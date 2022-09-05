using System.Buffers.Binary;
using SharedProject.Models;
using SharedProject.Models.Responses;
using SharedProject.Utils;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("/api/[controller]/{baid}")]
public class UserSettingsController : BaseController<UserSettingsController>
{
    private readonly TaikoDbContext context;

    public UserSettingsController(TaikoDbContext context)
    {
        this.context = context;
    }

    [HttpGet]
    public async Task<ActionResult<UserSetting>> GetUserSetting(uint baid)
    {
        var user = await context.UserData.FirstOrDefaultAsync(datum => datum.Baid == baid);

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
            MyDonName = user.MyDonName
        };
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> SaveUserSetting(uint baid, UserSetting userSetting)
    {
        var user = await context.UserData.FirstOrDefaultAsync(datum => datum.Baid == baid);

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

        context.Update(user);
        await context.SaveChangesAsync();

        return NoContent();
    }

}