using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UserDataQueryHandler
{
    private partial async ValueTask<Ac15UserDataResponse> HandleYellow(
        Ac15UserDataQuery request,
        CancellationToken cancellationToken)
    {
        _ = await context.UserData.FindAsync([request.Baid], cancellationToken)
            ?? throw new InvalidOperationException($"User not found for Yellow baid {request.Baid}.");
        var saveData = await context.GetOrCreateYellowSaveDataAsync(request.Baid, cancellationToken);
        var yellow = gameDataService.Yellow();
        var favorites = await context.YellowFavoriteSongs
            .Where(song => song.Baid == request.Baid)
            .Select(song => song.SongNo)
            .ToArrayAsync(cancellationToken);
        var recent = await context.YellowRecentSongs
            .Where(song => song.Baid == request.Baid)
            .OrderByDescending(song => song.LastPlayed)
            .Select(song => song.SongNo)
            .Take(Ac15EraProfiles.Yellow.Limits.MaxRecentSongs)
            .ToArrayAsync(cancellationToken);
        var normalDanGrades = await context.DanScoreDataYellow
            .Where(row => row.Baid == request.Baid && !row.IsExtra)
            .ToDictionaryAsync(row => row.DanId, row => row.ClearGrade, cancellationToken);
        var displayDan = Ac15DanHelpers.NormalizeDisplayDan(saveData.DispTaikojukuDan, normalDanGrades, Ac15EraProfiles.Yellow.Limits);

        var snapshot = Ac15CatalogSnapshotFactory.FromYellow(yellow);
        var unlockedShopItems = snapshot.ItemShopCatalog.ActiveSeason is null
            ? []
            : await context.GetUnlockedYellowShopItemsAsync(
                request.Baid,
                snapshot.ItemShopCatalog.ActiveSeason.SeasonId,
                cancellationToken);
        var userdata = YellowAc15UserDataAdapter.CreateSnapshot(
            saveData,
            snapshot,
            favorites,
            recent,
            unlockedShopItems);
        var response = Ac15UserDataService.BuildResponse(userdata, Ac15EraProfiles.Yellow);
        return response with
        {
            Display = response.Display with { DispTaikojukuDan = GetSafeYellowTaikojukuDanSlot(displayDan) },
            ModeFlags = new Ac15UserDataModeFlags(saveData.IsDevil, saveData.IsExplain),
            Tutorial = new Ac15UserDataTutorial(saveData.TokkunTutorialFlg, DifficultyTutorialFlg: null)
        };
    }

    private static uint GetSafeYellowTaikojukuDanSlot(uint value)
        => Ac15DanHelpers.IsNormalDanId(value, Ac15EraProfiles.Yellow.Limits)
            ? value
            : Ac15EraProfiles.Yellow.Limits.SafeDisplayDanFallback;
}
