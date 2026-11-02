using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public sealed partial class Ac15ProfileSettingsController
{
    private Task<ActionResult<Ac15ProfileSettingsDto>> GetBlue(uint baid)
        => GetForAc15Async<UserSaveDataBlue, DanScoreDatumBlue>(
            baid,
            context.GetOrCreateBlueSaveDataAsync,
            context.DanScoreDataBlue,
            Ac15EraProfiles.Blue);

    private Task<IActionResult> PutBlue(uint baid, Ac15ProfileSettingsUpdateDto request)
        => PutForAc15Async<UserSaveDataBlue, DanScoreDatumBlue>(
            baid,
            request,
            context.GetOrCreateBlueSaveDataAsync,
            context.DanScoreDataBlue,
            Ac15EraProfiles.Blue);
}
