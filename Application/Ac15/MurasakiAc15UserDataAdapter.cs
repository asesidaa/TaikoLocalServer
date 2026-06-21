namespace TaikoLocalServer.Application.Ac15;

public static class MurasakiAc15UserDataAdapter
{
    public static Ac15UserDataSnapshot CreateSnapshot(
        UserSaveDataMurasaki saveData,
        Ac15CatalogSnapshot catalog,
        IReadOnlyList<uint> favorites,
        IReadOnlyList<uint> recent)
        => new(
            catalog.SongHashVersion,
            catalog.SongNoesInFileOrder,
            saveData.ReleaseSongFlg,
            saveData.ToneFlg,
            saveData.TitleFlg,
            saveData.DefaultOptionSetting,
            saveData.OptionFlg,
            favorites,
            recent,
            Counters(saveData),
            saveData.DispTaikojukuDan,
            LockedSongIds: [],
            LockedToneIds: []);

    private static Ac15ProfileCounters Counters(UserSaveDataMurasaki saveData) => new()
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
        DispScoreType = saveData.DispScoreType,
        DispLevelSelf = saveData.DispLevelSelf,
        DifficultyPlayedCourse = saveData.DifficultyPlayedCourse,
        DifficultyPlayedStar = saveData.DifficultyPlayedStar,
        IsChallengeCompe = false,
        IsTojiru = saveData.IsTojiru
    };
}
