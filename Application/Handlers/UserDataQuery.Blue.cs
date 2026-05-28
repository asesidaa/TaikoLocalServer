namespace TaikoLocalServer.Application.Handlers;

public partial class UserDataQueryHandler
{
    private partial async ValueTask<CommonUserDataResponse> HandleBlue(
        UserDataQuery request,
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
                    && row.Status == BlueShopItemStatus.Unlocked)
                .Select(row => new ValueTuple<uint, uint>(row.ItemType, row.ItemId))
                .ToHashSetAsync(cancellationToken);

        IEnumerable<uint> LockedIds(uint itemType) => activeShopSeason?.Items
            .Where(item => item.ItemType == itemType && !unlockedShopItems.Contains((item.ItemType, item.ItemId)))
            .Select(item => item.ItemId) ?? [];

        var normalDanGrades = await context.DanScoreDataBlue
            .Where(row => row.Baid == request.Baid && !row.IsExtra && row.DanId >= 1 && row.DanId <= 25)
            .ToDictionaryAsync(row => row.DanId, row => row.ClearGrade, cancellationToken);
        var displayDan = BlueDanHelpers.NormalizeDisplayDan(saveData.DispTaikojukuDan, normalDanGrades);
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
        var catalogReleaseFlags = BlueProtocolBytes.CreateFixedBitset(
            blue.MusicInfoFileOrder.Select(song => song.SongNo),
            BlueProtocolBytes.SongFlagBytes);

        return new CommonUserDataResponse
        {
            Result = 1,
            SongHashVer = blue.SongHashVersion,
            ReleaseSongFlg = BlueShopUnlocks.ClearBits(
                BlueProtocolBytes.OrBitsets(
                    catalogReleaseFlags,
                    saveData.ReleaseSongFlg,
                    BlueProtocolBytes.SongFlagBytes),
                LockedIds(1),
                BlueProtocolBytes.SongFlagBytes),
            ToneFlg = BlueShopUnlocks.ClearBits(
                saveData.ToneFlg,
                LockedIds(2),
                BlueProtocolBytes.ToneFlagBytes),
            TitleFlg = BlueProtocolBytes.FixedOrZero(saveData.TitleFlg, BlueProtocolBytes.TitleFlagBytes),
            DefaultOptionSetting = BlueProtocolBytes.FixedOrZero(saveData.DefaultOptionSetting, 2),
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
            RecommendSong = blue.Recommend.RecommendSong,
            RecommendBestSong = blue.Recommend.RecommendBestSongs.ToList(),
            SongFavoriteCnt = saveData.SongFavoriteCnt,
            SongRecentCnt = saveData.SongRecentCnt,
            TotalCreditCnt = saveData.TotalCreditCnt,
            PrevAreaCode = saveData.PrevAreaCode,
            ConsecAreaCnt = saveData.ConsecAreaCnt,
            DefaultShinSetting = saveData.DefaultShinSetting,
            DispLevelTotal = saveData.DispLevelTotal,
            DispLevelChassis = saveData.DispLevelChassis,
            DispLevelSelf = saveData.DispLevelSelf,
            DispTaikojukuDan = GetSafeBlueTaikojukuDanSlot(displayDan),
            DifficultyPlayedCourse = saveData.DifficultyPlayedCourse,
            DifficultyPlayedStar = saveData.DifficultyPlayedStar,
            IsChallengeCompe = saveData.IsChallengeCompe,
            IsTojiru = saveData.IsTojiru,
            IsDevilBlue = saveData.IsDevil
        };
    }

    private static uint GetSafeBlueTaikojukuDanSlot(uint value)
        => value is >= 1 and <= 25 ? value : 1u;
}
