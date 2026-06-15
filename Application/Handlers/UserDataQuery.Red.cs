using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Ac15.ChallengeCompe;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UserDataQueryHandler
{
    private partial async ValueTask<Ac15UserDataResponse> HandleRed(
        Ac15UserDataQuery request,
        CancellationToken cancellationToken)
    {
        _ = await context.UserData.FindAsync([request.Baid], cancellationToken)
            ?? throw new InvalidOperationException($"User not found for Red baid {request.Baid}.");
        var saveData = await context.GetOrCreateRedSaveDataAsync(request.Baid, cancellationToken);
        var red = gameDataService.Red();
        var favorites = await context.RedFavoriteSongs
            .Where(song => song.Baid == request.Baid)
            .Select(song => song.SongNo)
            .ToArrayAsync(cancellationToken);
        var recent = await context.RedRecentSongs
            .Where(song => song.Baid == request.Baid)
            .OrderByDescending(song => song.LastPlayed)
            .Select(song => song.SongNo)
            .Take(Ac15EraProfiles.Red.Limits.MaxRecentSongs)
            .ToArrayAsync(cancellationToken);
        var normalDanGrades = await context.DanScoreDataRed
            .Where(row => row.Baid == request.Baid && !row.IsExtra)
            .ToDictionaryAsync(row => row.DanId, row => row.ClearGrade, cancellationToken);
        var displayDan = Ac15DanHelpers.NormalizeDisplayDan(saveData.DispTaikojukuDan, normalDanGrades, Ac15EraProfiles.Red.Limits);

        var snapshot = Ac15CatalogSnapshotFactory.FromRed(red);
        var challengeLockedSongIds = Ac15ChallengeCompeRewardDecisions.GetLockedRewardSongIds(
            red.ChallengeCompe,
            saveData.ReleaseSongFlg,
            Ac15EraProfiles.Red.Limits.SongFlagBytes);
        var userdata = RedAc15UserDataAdapter.CreateSnapshot(saveData, snapshot, favorites, recent, challengeLockedSongIds);
        var response = Ac15UserDataService.BuildResponse(userdata, Ac15EraProfiles.Red);
        return response with
        {
            Display = response.Display with { DispTaikojukuDan = GetSafeRedTaikojukuDanSlot(displayDan) },
            ModeFlags = new Ac15UserDataModeFlags(saveData.IsDevil, saveData.IsExplain),
            Tutorial = new Ac15UserDataTutorial(saveData.TokkunTutorialFlg, saveData.DifficultyTutorialFlg),
            Reward = new Ac15UserDataReward(
                saveData.TotalGetDonpoint,
                saveData.TotalUseDonpoint,
                saveData.RewardProgress)
        };
    }

    private static uint GetSafeRedTaikojukuDanSlot(uint value)
        => Ac15DanHelpers.IsNormalDanId(value, Ac15EraProfiles.Red.Limits)
            ? value
            : Ac15EraProfiles.Red.Limits.SafeDisplayDanFallback;
}
