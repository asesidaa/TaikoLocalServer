using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Application.Handlers;

public partial class UserDataQueryHandler
{
    private partial async ValueTask<CommonUserDataResponse> HandleGreen(
        UserDataQuery request,
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
                    && row.Status == GreenShopItemStatus.Unlocked)
                .Select(row => new ValueTuple<uint, uint>(row.ItemType, row.ItemId))
                .ToHashSetAsync(cancellationToken);

        IEnumerable<uint> LockedIds(Ac15ShopItemType itemType) => activeShopSeason?.Items
            .Where(item => item.ItemType == itemType && !unlockedShopItems.Contains((item.ItemType.ToProtocolValue(), item.ItemId)))
            .Select(item => item.ItemId) ?? [];

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
        var displayDan = GreenDanHelpers.NormalizeDisplayDan(saveData.DispTaikojukuDan, normalDanGrades);

        return new CommonUserDataResponse
        {
            Result = 1,
            SongHashVer = green.SongHashVersion,
            ReleaseSongFlg = GreenShopUnlocks.ClearBits(
                GreenProtocolBytes.CreateFixedBitset(
                    green.MusicInfoFileOrder.Select(song => song.SongNo),
                    GreenProtocolBytes.SongFlagBytes),
                LockedIds(Ac15ShopItemType.Song),
                GreenProtocolBytes.SongFlagBytes),
            ToneFlg = GreenShopUnlocks.ClearBits(
                saveData.ToneFlg,
                LockedIds(Ac15ShopItemType.Tone),
                GreenProtocolBytes.ToneFlagBytes),
            TitleFlg = GreenProtocolBytes.FixedOrZero(saveData.TitleFlg, GreenProtocolBytes.TitleFlagBytes),
            DefaultOptionSetting = GreenProtocolBytes.FixedOrZero(saveData.DefaultOptionSetting, 2),
            OptionFlg = saveData.OptionFlg,
            AryFavoriteSongNoes = favorites,
            AryRecentSongNoes = recent,
            CategJpopCnt = saveData.CategJpopCnt,
            CategAnimeCnt = saveData.CategAnimeCnt,
            CategDoyoCnt = saveData.CategDoyoCnt,
            CategVarietyCnt = saveData.CategVarietyCnt,
            CategClassicCnt = saveData.CategClassicCnt,
            CategGameCnt = saveData.CategGameCnt,
            CategNamcoCnt = saveData.CategNamcoCnt,
            CategVocaloidCnt = saveData.CategVocaloidCnt,
            SongPushedCnt = saveData.SongPushedCnt,
            RecommendSong = green.Recommend.RecommendSong,
            RecommendBestSong = green.Recommend.RecommendBestSongs.ToList(),
            SongFavoriteCnt = saveData.SongFavoriteCnt,
            SongRecentCnt = saveData.SongRecentCnt,
            TotalCreditCnt = saveData.TotalCreditCnt,
            PrevAreaCode = saveData.PrevAreaCode,
            ConsecAreaCnt = saveData.ConsecAreaCnt,
            DefaultShinSetting = saveData.DefaultShinSetting,
            DispLevelTotal = saveData.DispLevelTotal,
            DispLevelChassis = saveData.DispLevelChassis,
            DispLevelSelf = saveData.DispLevelSelf,
            DispTaikojukuDan = GetSafeTaikojukuDanSlot(displayDan),
            DifficultyPlayedCourse = saveData.DifficultyPlayedCourse,
            DifficultyPlayedStar = saveData.DifficultyPlayedStar,
            IsChallengeCompe = saveData.IsChallengeCompe,
            IsTojiru = saveData.IsTojiru,
            IsDevilGreen = saveData.IsDevil
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
        => value is >= 1 and <= 25 ? value : 1u;
}
