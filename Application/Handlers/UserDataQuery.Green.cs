using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UserDataQueryHandler
{
    private partial async ValueTask<Ac15UserDataResponse> HandleGreen(
        Ac15UserDataQuery request,
        CancellationToken cancellationToken)
    {
        _ = await context.UserData.FindAsync([request.Baid], cancellationToken)
            ?? throw new InvalidOperationException($"User not found for Green baid {request.Baid}.");
        var saveData = await context.GetOrCreateGreenSaveDataAsync(request.Baid, cancellationToken);
        var green = gameDataService.Green();
        var activeShopSeason = green.ItemShopCatalog.ActiveSeason;
        var unlockedShopItems = activeShopSeason is null
            ? new HashSet<(uint ItemType, uint ItemId)>()
            : await context.GreenShopItemStates
                .Where(row => row.Baid == request.Baid
                    && row.SeasonId == activeShopSeason.SeasonId
                    && row.Status == Ac15ShopItemStatus.Unlocked)
                .Select(row => new ValueTuple<uint, uint>(row.ItemType, row.ItemId))
                .ToHashSetAsync(cancellationToken);

        var favorites = await context.GreenFavoriteSongs
            .Where(song => song.Baid == request.Baid)
            .Select(song => song.SongNo)
            .ToArrayAsync(cancellationToken);
        var recent = await context.GreenRecentSongs
            .Where(song => song.Baid == request.Baid)
            .OrderByDescending(song => song.LastPlayed)
            .Select(song => song.SongNo)
            .Take(10)
            .ToArrayAsync(cancellationToken);
        var normalDanGrades = await context.DanScoreDataGreen
            .Where(row => row.Baid == request.Baid && !row.IsExtra)
            .ToDictionaryAsync(row => row.DanId, row => row.ClearGrade, cancellationToken);
        var displayDan = Ac15DanHelpers.NormalizeDisplayDan(saveData.DispTaikojukuDan, normalDanGrades, Ac15EraProfiles.Green.Limits);

        var snapshot = Ac15CatalogSnapshotFactory.FromGreen(green);
        var userdata = GreenAc15UserDataAdapter.CreateSnapshot(
            saveData,
            snapshot,
            favorites,
            recent,
            unlockedShopItems);
        var response = Ac15UserDataService.BuildResponse(userdata, Ac15EraProfiles.Green);
        return response with
        {
            Display = response.Display with { DispTaikojukuDan = GetSafeTaikojukuDanSlot(displayDan) },
            ModeFlags = new Ac15UserDataModeFlags(saveData.IsDevil, IsExplain: null)
        };
    }

    // Green client reads disp_taikojuku_dan_ at message offset +0x31C without
    // checking proto2 presence (verified at sub_19CFE0, sub_1016F8, sub_24377C,
    // sub_7FDFFC). Any value outside 1..25 - including 0 and the wire-absent
    // case decoded as 0 - drives Taikojuku_GetDanSlotSongRange @ 0x127F98 into
    // a table-underflow read (table + 84*dan - 84) and crashes the client.
    // sub_7FDFFC itself initialises its local slot to 1 as its "no data" path,
    // so 1 is the value the client treats as the safe absent sentinel.
    private static uint GetSafeTaikojukuDanSlot(uint value)
        => Ac15DanHelpers.IsNormalDanId(value, Ac15EraProfiles.Green.Limits)
            ? value
            : Ac15EraProfiles.Green.Limits.SafeDisplayDanFallback;
}
