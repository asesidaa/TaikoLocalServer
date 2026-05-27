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

        return new CommonUserDataResponse
        {
            Result = 1,
            SongHashVer = blue.SongHashVersion,
            ReleaseSongFlg = BlueProtocolBytes.CreateFixedBitset(
                blue.MusicInfoFileOrder.Select(song => song.SongNo),
                BlueProtocolBytes.SongFlagBytes),
            ToneFlg = BlueProtocolBytes.FixedOrZero(saveData.ToneFlg, BlueProtocolBytes.ToneFlagBytes),
            TitleFlg = BlueProtocolBytes.FixedOrZero(saveData.TitleFlg, BlueProtocolBytes.TitleFlagBytes),
            DefaultOptionSetting = BlueProtocolBytes.FixedOrZero(saveData.DefaultOptionSetting, 2),
            OptionFlg = saveData.OptionFlg,
            AryFavoriteSongNoes = [],
            AryRecentSongNoes = [],
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
            DispTaikojukuDan = GetSafeBlueTaikojukuDanSlot(saveData.DispTaikojukuDan),
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
