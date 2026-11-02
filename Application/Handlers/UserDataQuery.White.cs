using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Ac15.DonChallenge;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UserDataQueryHandler
{
    private partial async ValueTask<Ac15UserDataResponse> HandleWhite(
        Ac15UserDataQuery request,
        CancellationToken cancellationToken)
    {
        _ = await context.UserData.FindAsync([request.Baid], cancellationToken)
            ?? throw new InvalidOperationException($"User not found for White baid {request.Baid}.");
        var saveData = await context.GetOrCreateWhiteSaveDataAsync(request.Baid, cancellationToken);
        var white = gameDataService.White();
        var favorites = await context.WhiteFavoriteSongs
            .Where(song => song.Baid == request.Baid)
            .Select(song => song.SongNo)
            .ToArrayAsync(cancellationToken);
        var recent = await context.WhiteRecentSongs
            .Where(song => song.Baid == request.Baid)
            .OrderByDescending(song => song.LastPlayed)
            .Select(song => song.SongNo)
            .Take(Ac15EraProfiles.White.Limits.MaxRecentSongs)
            .ToArrayAsync(cancellationToken);
        var normalDanGrades = await context.DanScoreDataWhite
            .Where(row => row.Baid == request.Baid && !row.IsExtra)
            .ToDictionaryAsync(row => row.DanId, row => row.ClearGrade, cancellationToken);
        var displayDan = Ac15DanHelpers.NormalizeDisplayDan(saveData.DispTaikojukuDan, normalDanGrades, Ac15EraProfiles.White.Limits);

        var snapshot = Ac15CatalogSnapshotFactory.FromWhite(white);
        var challengeLockedSongIds = Ac15DonChallengeRewardDecisions.GetLockedRewardSongIds(
            white.DonChallenge,
            saveData.ReleaseSongFlg,
            Ac15EraProfiles.White.Limits.SongFlagBytes);
        var userdata = WhiteAc15UserDataAdapter.CreateSnapshot(saveData, snapshot, favorites, recent, challengeLockedSongIds);
        var response = Ac15UserDataService.BuildResponse(userdata, Ac15EraProfiles.White);
        return response with
        {
            Display = response.Display with { DispTaikojukuDan = GetSafeWhiteTaikojukuDanSlot(displayDan) },
            ModeFlags = new Ac15UserDataModeFlags(saveData.IsDevil, saveData.IsExplain),
            Tutorial = new Ac15UserDataTutorial(saveData.TokkunTutorialFlg, saveData.DifficultyTutorialFlg),
            Reward = new Ac15UserDataReward(
                saveData.TotalGetDonpoint,
                saveData.TotalUseDonpoint,
                saveData.RewardProgress)
        };
    }

    private static uint GetSafeWhiteTaikojukuDanSlot(uint value)
        => Ac15DanHelpers.IsNormalDanId(value, Ac15EraProfiles.White.Limits)
            ? value
            : Ac15EraProfiles.White.Limits.SafeDisplayDanFallback;
}
