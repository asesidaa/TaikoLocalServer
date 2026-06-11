using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public partial class UserSettingsController
{
    private async Task<ActionResult<UserSetting>> GetBlueUserSetting(uint baid)
    {
        var user = await context.UserData.Include(d => d.Tokens).FirstOrDefaultAsync(d => d.Baid == baid);
        if (user is null)
        {
            return NotFound();
        }

        var saveData = await context.GetOrCreateBlueSaveDataAsync(baid, HttpContext.RequestAborted);
        var result = await Ac15UserSettingsService.GetAsync(
            user,
            saveData,
            context.DanScoreDataBlue,
            Ac15UserSettingsAccess.Blue,
            Ac15EraProfiles.Blue.Limits,
            HttpContext.RequestAborted);

        return Ok(result.Setting);
    }

    private async Task<IActionResult> SaveBlueUserSetting(uint baid, UserSetting userSetting)
    {
        var user = await context.UserData.FindAsync(baid);
        if (user is null)
        {
            return NotFound();
        }

        var saveData = await context.GetOrCreateBlueSaveDataAsync(baid, HttpContext.RequestAborted);
        var result = await Ac15UserSettingsService.SaveAsync(
            user,
            saveData,
            userSetting,
            context.DanScoreDataBlue,
            Ac15UserSettingsAccess.Blue,
            Ac15EraProfiles.Blue.Limits,
            HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }
}
