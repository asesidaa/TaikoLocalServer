using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UserDataQueryHandler
{
    private partial async ValueTask<Ac15UserDataResponse> HandleMurasaki(
        Ac15UserDataQuery request,
        CancellationToken cancellationToken)
    {
        _ = await context.UserData.FindAsync([request.Baid], cancellationToken)
            ?? throw new InvalidOperationException($"User not found for Murasaki baid {request.Baid}.");
        var saveData = await context.GetOrCreateMurasakiSaveDataAsync(request.Baid, cancellationToken);
        var murasaki = gameDataService.Murasaki();
        var favorites = await context.MurasakiFavoriteSongs
            .Where(song => song.Baid == request.Baid)
            .Select(song => song.SongNo)
            .ToArrayAsync(cancellationToken);
        var recent = await context.MurasakiRecentSongs
            .Where(song => song.Baid == request.Baid)
            .OrderByDescending(song => song.LastPlayed)
            .Select(song => song.SongNo)
            .Take(Ac15EraProfiles.Murasaki.Limits.MaxRecentSongs)
            .ToArrayAsync(cancellationToken);
        var normalDanGrades = await context.DanScoreDataMurasaki
            .Where(row => row.Baid == request.Baid && !row.IsExtra)
            .ToDictionaryAsync(row => row.DanId, row => row.ClearGrade, cancellationToken);
        var displayDan = Ac15DanHelpers.NormalizeDisplayDan(saveData.DispTaikojukuDan, normalDanGrades, Ac15EraProfiles.Murasaki.Limits);

        var snapshot = Ac15CatalogSnapshotFactory.FromMurasaki(murasaki);
        var userdata = MurasakiAc15UserDataAdapter.CreateSnapshot(saveData, snapshot, favorites, recent);
        var response = Ac15UserDataService.BuildResponse(userdata, Ac15EraProfiles.Murasaki);
        return response with
        {
            Display = response.Display with { DispTaikojukuDan = GetSafeMurasakiTaikojukuDanSlot(displayDan) },
            ModeFlags = new Ac15UserDataModeFlags(saveData.IsDevil, saveData.IsExplain),
            Tutorial = new Ac15UserDataTutorial(null, saveData.DifficultyTutorialFlg),
            Reward = new Ac15UserDataReward(
                saveData.TotalGetDonpoint,
                saveData.TotalUseDonpoint,
                saveData.RewardProgress)
        };
    }

    private static uint GetSafeMurasakiTaikojukuDanSlot(uint value)
        => Ac15DanHelpers.IsNormalDanId(value, Ac15EraProfiles.Murasaki.Limits)
            ? value
            : Ac15EraProfiles.Murasaki.Limits.SafeDisplayDanFallback;
}
