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
            case 0: saveData.CategJpopCnt = SafeIncrement(saveData.CategJpopCnt); break;
            case 1: saveData.CategAnimeCnt = SafeIncrement(saveData.CategAnimeCnt); break;
            case 2: saveData.CategDoyoCnt = SafeIncrement(saveData.CategDoyoCnt); break;
            case 3: saveData.CategVocaloidCnt = SafeIncrement(saveData.CategVocaloidCnt); break;
            case 4: saveData.CategGameCnt = SafeIncrement(saveData.CategGameCnt); break;
            case 5: saveData.CategNamcoCnt = SafeIncrement(saveData.CategNamcoCnt); break;
            case 6: saveData.CategVarietyCnt = SafeIncrement(saveData.CategVarietyCnt); break;
            case 7: saveData.CategClassicCnt = SafeIncrement(saveData.CategClassicCnt); break;
        }
    }

    private static uint SafeIncrement(uint current)
        => current == uint.MaxValue ? current : current + 1;
}
