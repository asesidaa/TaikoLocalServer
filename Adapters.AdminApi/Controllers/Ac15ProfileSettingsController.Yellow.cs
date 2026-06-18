using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public sealed partial class Ac15ProfileSettingsController
{
    private Task<ActionResult<Ac15ProfileSettingsDto>> GetYellow(uint baid)
        => GetForAc15Async<UserSaveDataYellow, DanScoreDatumYellow>(
            baid,
            context.GetOrCreateYellowSaveDataAsync,
            context.DanScoreDataYellow,
            Ac15EraProfiles.Yellow);

    private Task<IActionResult> PutYellow(uint baid, Ac15ProfileSettingsUpdateDto request)
        => PutForAc15Async<UserSaveDataYellow, DanScoreDatumYellow>(
            baid,
            request,
            context.GetOrCreateYellowSaveDataAsync,
            context.DanScoreDataYellow,
            Ac15EraProfiles.Yellow);
}
