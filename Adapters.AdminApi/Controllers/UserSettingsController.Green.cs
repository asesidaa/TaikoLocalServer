using TaikoLocalServer.Application.Ac15;

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
        var result = await Ac15UserSettingsService.GetAsync(
            user,
            saveData,
            context.DanScoreDataGreen,
            Ac15UserSettingsAccess.Green,
            Ac15EraProfiles.Green.Limits,
            HttpContext.RequestAborted);

        return Ok(result.Setting);
    }

    private async Task<IActionResult> SaveGreenUserSetting(uint baid, UserSetting userSetting)
    {
        var user = await context.UserData.FindAsync(baid);
        if (user is null)
        {
            return NotFound();
        }

        var saveData = await context.GetOrCreateGreenSaveDataAsync(baid, HttpContext.RequestAborted);
        var result = await Ac15UserSettingsService.SaveAsync(
            user,
            saveData,
            userSetting,
            context.DanScoreDataGreen,
            Ac15UserSettingsAccess.Green,
            Ac15EraProfiles.Green.Limits,
            HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }
}
