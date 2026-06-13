namespace TaikoLocalServer.Application.Ac15;

public static class GreenAc15UserDataAdapter
{
    public static Ac15UserDataSnapshot CreateSnapshot(
        UserSaveDataGreen saveData,
        Ac15CatalogSnapshot catalog,
        IReadOnlyList<uint> favorites,
        IReadOnlyList<uint> recent,
        IReadOnlyCollection<(uint ItemType, uint ItemId)> unlockedShopItems)
    {
        var lockedSongIds = LockedIds(catalog, Ac15ShopItemType.Song, unlockedShopItems).ToArray();
        var lockedToneIds = LockedIds(catalog, Ac15ShopItemType.Tone, unlockedShopItems).ToArray();

        return new Ac15UserDataSnapshot(
            catalog.SongHashVersion,
            catalog.SongNoesInFileOrder,
            [],
            saveData.ToneFlg,
            saveData.TitleFlg,
            saveData.DefaultOptionSetting,
            saveData.OptionFlg,
            favorites,
            recent,
            catalog.RecommendSong,
            catalog.RecommendBestSongs,
            Counters(saveData),
            saveData.DispTaikojukuDan,
            lockedSongIds,
            lockedToneIds);
    }

    private static IEnumerable<uint> LockedIds(
        Ac15CatalogSnapshot catalog,
        Ac15ShopItemType itemType,
        IReadOnlyCollection<(uint ItemType, uint ItemId)> unlockedShopItems)
        => catalog.ItemShopCatalog.ActiveSeason?.Items
            .Where(item => item.ItemType == itemType && !unlockedShopItems.Contains((item.ItemType.ToProtocolValue(), item.ItemId)))
            .Select(item => item.ItemId) ?? [];

    private static Ac15ProfileCounters Counters(UserSaveDataGreen saveData) => new()
    {
        CategJpopCnt = saveData.CategJpopCnt,
        CategAnimeCnt = saveData.CategAnimeCnt,
        CategDoyoCnt = saveData.CategDoyoCnt,
        CategVarietyCnt = saveData.CategVarietyCnt,
        CategClassicCnt = saveData.CategClassicCnt,
        CategGameCnt = saveData.CategGameCnt,
        CategNamcoCnt = saveData.CategNamcoCnt,
        CategVocaloidCnt = saveData.CategVocaloidCnt,
        SongPushedCnt = saveData.SongPushedCnt,
        SongFavoriteCnt = saveData.SongFavoriteCnt,
        SongRecentCnt = saveData.SongRecentCnt,
        TotalCreditCnt = saveData.TotalCreditCnt,
        PrevAreaCode = saveData.PrevAreaCode,
        ConsecAreaCnt = saveData.ConsecAreaCnt,
        DefaultShinSetting = saveData.DefaultShinSetting,
        DispLevelTotal = saveData.DispLevelTotal,
        DispLevelChassis = saveData.DispLevelChassis,
        DispLevelSelf = saveData.DispLevelSelf,
        DifficultyPlayedCourse = saveData.DifficultyPlayedCourse,
        DifficultyPlayedStar = saveData.DifficultyPlayedStar,
        IsChallengeCompe = saveData.IsChallengeCompe,
        IsTojiru = saveData.IsTojiru
    };
}
