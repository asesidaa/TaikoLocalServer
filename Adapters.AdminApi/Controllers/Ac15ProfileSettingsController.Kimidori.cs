using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public sealed partial class Ac15ProfileSettingsController
{
    private Task<ActionResult<Ac15ProfileSettingsDto>> GetKimidori(uint baid)
        => GetForAc15Async<UserSaveDataKimidori, DanScoreDatumKimidori>(
            baid,
            context.GetOrCreateKimidoriSaveDataAsync,
            context.DanScoreDataKimidori,
            Ac15EraProfiles.Kimidori);

    private Task<IActionResult> PutKimidori(uint baid, Ac15ProfileSettingsUpdateDto request)
        => PutForAc15Async<UserSaveDataKimidori, DanScoreDatumKimidori>(
            baid,
            request,
            context.GetOrCreateKimidoriSaveDataAsync,
            context.DanScoreDataKimidori,
            Ac15EraProfiles.Kimidori);
}
