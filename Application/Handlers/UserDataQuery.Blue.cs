using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UserDataQueryHandler
{
    private partial async ValueTask<Ac15UserDataResponse> HandleBlue(
        Ac15UserDataQuery request,
        CancellationToken cancellationToken)
    {
        _ = await context.UserData.FindAsync([request.Baid], cancellationToken)
            ?? throw new InvalidOperationException($"User not found for Blue baid {request.Baid}.");
        var saveData = await context.GetOrCreateBlueSaveDataAsync(request.Baid, cancellationToken);
        var blue = gameDataService.Blue();
        var activeShopSeason = blue.ItemShopCatalog.ActiveSeason;
        var unlockedShopItems = activeShopSeason is null
            ? new HashSet<(uint ItemType, uint ItemId)>()
            : await context.BlueShopItemStates
                .Where(row => row.Baid == request.Baid
                    && row.SeasonId == activeShopSeason.SeasonId
                    && row.Status == Ac15ShopItemStatus.Unlocked)
                .Select(row => new ValueTuple<uint, uint>(row.ItemType, row.ItemId))
                .ToHashSetAsync(cancellationToken);

        var normalDanGrades = await context.DanScoreDataBlue
            .Where(row => row.Baid == request.Baid && !row.IsExtra && row.DanId >= 1 && row.DanId <= 25)
            .ToDictionaryAsync(row => row.DanId, row => row.ClearGrade, cancellationToken);
        var displayDan = Ac15DanHelpers.NormalizeDisplayDan(saveData.DispTaikojukuDan, normalDanGrades, Ac15EraProfiles.Blue.Limits);
        var favorites = await context.BlueFavoriteSongs
            .Where(song => song.Baid == request.Baid)
            .Select(song => song.SongNo)
            .ToArrayAsync(cancellationToken);
        var recent = await context.BlueRecentSongs
            .Where(song => song.Baid == request.Baid)
            .OrderByDescending(song => song.LastPlayed)
            .Select(song => song.SongNo)
            .Take(10)
            .ToArrayAsync(cancellationToken);

        var snapshot = Ac15CatalogSnapshotFactory.FromBlue(blue);
        var userdata = BlueAc15UserDataAdapter.CreateSnapshot(
            saveData,
            snapshot,
            favorites,
            recent,
            unlockedShopItems);
        var response = Ac15UserDataService.BuildResponse(userdata, Ac15EraProfiles.Blue);
        return response with
        {
            Display = response.Display with { DispTaikojukuDan = GetSafeBlueTaikojukuDanSlot(displayDan) },
            ModeFlags = new Ac15UserDataModeFlags(saveData.IsDevil, IsExplain: null),
            Tutorial = new Ac15UserDataTutorial(saveData.TokkunTutorialFlg, DifficultyTutorialFlg: null)
        };
    }

    private static uint GetSafeBlueTaikojukuDanSlot(uint value)
        => Ac15DanHelpers.IsNormalDanId(value, Ac15EraProfiles.Blue.Limits)
            ? value
            : Ac15EraProfiles.Blue.Limits.SafeDisplayDanFallback;
}
