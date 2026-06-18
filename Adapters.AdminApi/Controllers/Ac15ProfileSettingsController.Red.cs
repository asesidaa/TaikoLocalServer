using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public sealed partial class Ac15ProfileSettingsController
{
    private Task<ActionResult<Ac15ProfileSettingsDto>> GetRed(uint baid)
        => GetForAc15Async<UserSaveDataRed, DanScoreDatumRed>(
            baid,
            context.GetOrCreateRedSaveDataAsync,
            context.DanScoreDataRed,
            Ac15EraProfiles.Red);

    private Task<IActionResult> PutRed(uint baid, Ac15ProfileSettingsUpdateDto request)
        => PutForAc15Async<UserSaveDataRed, DanScoreDatumRed>(
            baid,
            request,
            context.GetOrCreateRedSaveDataAsync,
            context.DanScoreDataRed,
            Ac15EraProfiles.Red);
}
