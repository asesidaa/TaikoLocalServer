using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UserDataQueryHandler
{
    private partial async ValueTask<Ac15UserDataResponse> HandleKimidori(
        Ac15UserDataQuery request,
        CancellationToken cancellationToken)
    {
        _ = await context.UserData.FindAsync([request.Baid], cancellationToken)
            ?? throw new InvalidOperationException($"User not found for Kimidori baid {request.Baid}.");
        var saveData = await context.GetOrCreateKimidoriSaveDataAsync(request.Baid, cancellationToken);
        var kimidori = gameDataService.Kimidori();
        var favorites = await context.KimidoriFavoriteSongs
            .Where(song => song.Baid == request.Baid)
            .Select(song => song.SongNo)
            .ToArrayAsync(cancellationToken);
        var recent = await context.KimidoriRecentSongs
            .Where(song => song.Baid == request.Baid)
            .OrderByDescending(song => song.LastPlayed)
            .Select(song => song.SongNo)
            .Take(Ac15EraProfiles.Kimidori.Limits.MaxRecentSongs)
            .ToArrayAsync(cancellationToken);
        var normalDanGrades = await context.DanScoreDataKimidori
            .Where(row => row.Baid == request.Baid && !row.IsExtra)
            .ToDictionaryAsync(row => row.DanId, row => row.ClearGrade, cancellationToken);
        var displayDan = Ac15DanHelpers.NormalizeDisplayDan(saveData.DispTaikojukuDan, normalDanGrades, Ac15EraProfiles.Kimidori.Limits);

        var snapshot = Ac15CatalogSnapshotFactory.FromKimidori(kimidori);
        var userdata = KimidoriAc15UserDataAdapter.CreateSnapshot(saveData, snapshot, favorites, recent);
        var response = Ac15UserDataService.BuildResponse(userdata, Ac15EraProfiles.Kimidori);
        return response with
        {
            Display = response.Display with { DispTaikojukuDan = GetSafeKimidoriDisplayDan(displayDan) },
            ModeFlags = new Ac15UserDataModeFlags(saveData.IsDevil, saveData.IsExplain),
            Tutorial = new Ac15UserDataTutorial(null, saveData.DifficultyTutorialFlg),
            Reward = new Ac15UserDataReward(
                saveData.TotalGetDonpoint,
                saveData.TotalUseDonpoint,
                saveData.RewardProgress)
        };
    }

    private static uint GetSafeKimidoriDisplayDan(uint value)
        => Ac15DanHelpers.IsNormalDanId(value, Ac15EraProfiles.Kimidori.Limits)
            ? value
            : Ac15EraProfiles.Kimidori.Limits.SafeDisplayDanFallback;
}
