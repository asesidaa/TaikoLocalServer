using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public sealed partial class Ac15ProfileSettingsController
{
    private Task<ActionResult<Ac15ProfileSettingsDto>> GetMurasaki(uint baid)
        => GetForAc15Async<UserSaveDataMurasaki, DanScoreDatumMurasaki>(
            baid,
            context.GetOrCreateMurasakiSaveDataAsync,
            context.DanScoreDataMurasaki,
            Ac15EraProfiles.Murasaki);

    private Task<IActionResult> PutMurasaki(uint baid, Ac15ProfileSettingsUpdateDto request)
        => PutForAc15Async<UserSaveDataMurasaki, DanScoreDatumMurasaki>(
            baid,
            request,
            context.GetOrCreateMurasakiSaveDataAsync,
            context.DanScoreDataMurasaki,
            Ac15EraProfiles.Murasaki);
}
