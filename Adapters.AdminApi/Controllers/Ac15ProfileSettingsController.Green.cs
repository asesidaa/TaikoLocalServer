using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public sealed partial class Ac15ProfileSettingsController
{
    private Task<ActionResult<Ac15ProfileSettingsDto>> GetGreen(uint baid)
        => GetForAc15Async<UserSaveDataGreen, DanScoreDatumGreen>(
            baid,
            context.GetOrCreateGreenSaveDataAsync,
            context.DanScoreDataGreen,
            Ac15EraProfiles.Green);

    private Task<IActionResult> PutGreen(uint baid, Ac15ProfileSettingsUpdateDto request)
        => PutForAc15Async<UserSaveDataGreen, DanScoreDatumGreen>(
            baid,
            request,
            context.GetOrCreateGreenSaveDataAsync,
            context.DanScoreDataGreen,
            Ac15EraProfiles.Green);
}
