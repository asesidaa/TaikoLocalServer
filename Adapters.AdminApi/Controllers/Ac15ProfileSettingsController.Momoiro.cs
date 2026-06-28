using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public sealed partial class Ac15ProfileSettingsController
{
    private Task<ActionResult<Ac15ProfileSettingsDto>> GetMomoiro(uint baid)
        => GetForAc15Async<UserSaveDataMomoiro, DanScoreDatumMomoiro>(
            baid,
            context.GetOrCreateMomoiroSaveDataAsync,
            context.DanScoreDataMomoiro,
            Ac15EraProfiles.Momoiro);

    private Task<IActionResult> PutMomoiro(uint baid, Ac15ProfileSettingsUpdateDto request)
        => PutForAc15Async<UserSaveDataMomoiro, DanScoreDatumMomoiro>(
            baid,
            request,
            context.GetOrCreateMomoiroSaveDataAsync,
            context.DanScoreDataMomoiro,
            Ac15EraProfiles.Momoiro);
}
