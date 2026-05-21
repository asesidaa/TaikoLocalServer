namespace TaikoLocalServer.Application.Common;

public static class GreenProfileCounters
{
    public static void ApplyStage(UserSaveDataGreen saveData, CommonPlayResultData.StageData stage)
    {
        IncrementGenreCounter(saveData, stage.MusicCateg);
        if (stage.IsPushed) saveData.SongPushedCnt = SafeIncrement(saveData.SongPushedCnt);
        if (stage.IsFavorite) saveData.SongFavoriteCnt = SafeIncrement(saveData.SongFavoriteCnt);
        if (stage.IsRecent) saveData.SongRecentCnt = SafeIncrement(saveData.SongRecentCnt);
    }

    private static void IncrementGenreCounter(UserSaveDataGreen saveData, uint musicCateg)
    {
        switch (musicCateg)
        {
            case 1: saveData.CategJpopCnt = SafeIncrement(saveData.CategJpopCnt); break;
            case 2: saveData.CategAnimeCnt = SafeIncrement(saveData.CategAnimeCnt); break;
            case 3: saveData.CategVocaloidCnt = SafeIncrement(saveData.CategVocaloidCnt); break;
            case 4: saveData.CategDoyoCnt = SafeIncrement(saveData.CategDoyoCnt); break;
            case 5: saveData.CategVarietyCnt = SafeIncrement(saveData.CategVarietyCnt); break;
            case 6: saveData.CategClassicCnt = SafeIncrement(saveData.CategClassicCnt); break;
            case 7: saveData.CategGameCnt = SafeIncrement(saveData.CategGameCnt); break;
            case 8: saveData.CategNamcoCnt = SafeIncrement(saveData.CategNamcoCnt); break;
        }
    }

    private static uint SafeIncrement(uint current)
        => current == uint.MaxValue ? current : current + 1;
}
