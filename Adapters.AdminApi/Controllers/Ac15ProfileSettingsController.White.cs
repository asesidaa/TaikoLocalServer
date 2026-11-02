using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public sealed partial class Ac15ProfileSettingsController
{
    private Task<ActionResult<Ac15ProfileSettingsDto>> GetWhite(uint baid)
        => GetForAc15Async<UserSaveDataWhite, DanScoreDatumWhite>(
            baid,
            context.GetOrCreateWhiteSaveDataAsync,
            context.DanScoreDataWhite,
            Ac15EraProfiles.White);

    private Task<IActionResult> PutWhite(uint baid, Ac15ProfileSettingsUpdateDto request)
        => PutForAc15Async<UserSaveDataWhite, DanScoreDatumWhite>(
            baid,
            request,
            context.GetOrCreateWhiteSaveDataAsync,
            context.DanScoreDataWhite,
            Ac15EraProfiles.White);
}
