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

        var favorites = await context.GreenFavoriteSongs
            .Where(song => song.Baid == request.Baid)
            .Select(song => song.SongNo)
            .ToArrayAsync(cancellationToken);
        var recent = await context.GreenRecentSongs
            .Where(song => song.Baid == request.Baid)
            .OrderByDescending(song => song.SongNo)
            .Select(song => song.SongNo)
            .Take(10)
            .ToArrayAsync(cancellationToken);

        return new CommonUserDataResponse
        {
            Result = 1,
            SongHashVer = green.SongHashVersion,
            ReleaseSongFlg = GreenProtocolBytes.CreateFixedBitset(
                green.MusicInfoFileOrder.Take(20).Select(song => song.SongNo),
                GreenProtocolBytes.SongFlagBytes),
            ToneFlg = GreenProtocolBytes.FixedOrZero(saveData.ToneFlg, GreenProtocolBytes.ToneFlagBytes),
            TitleFlg = GreenProtocolBytes.FixedOrZero(saveData.TitleFlg, GreenProtocolBytes.TitleFlagBytes),
            DefaultOptionSetting = GreenProtocolBytes.FixedOrZero(saveData.DefaultOptionSetting, 2),
            OptionFlg = saveData.OptionFlg,
            AryFavoriteSongNoes = favorites,
            AryRecentSongNoes = recent,
            SongFavoriteCnt = (uint)favorites.Length,
            SongRecentCnt = (uint)recent.Length,
            CategJpopCnt = saveData.CategJpopCnt,
            CategAnimeCnt = saveData.CategAnimeCnt,
            CategDoyoCnt = saveData.CategDoyoCnt,
            CategVarietyCnt = saveData.CategVarietyCnt,
            CategClassicCnt = saveData.CategClassicCnt,
            CategGameCnt = saveData.CategGameCnt,
            CategNamcoCnt = saveData.CategNamcoCnt,
            CategVocaloidCnt = saveData.CategVocaloidCnt,
            TotalCreditCnt = saveData.TotalCreditCnt,
            PrevAreaCode = saveData.PrevAreaCode,
            ConsecAreaCnt = saveData.ConsecAreaCnt,
            DefaultShinSetting = saveData.DefaultShinSetting,
            DispTaikojukuDan = saveData.DispTaikojukuDan,
            DifficultyPlayedCourse = saveData.DifficultyPlayedCourse,
            DifficultyPlayedStar = saveData.DifficultyPlayedStar,
            IsChallengeCompe = saveData.IsChallengeCompe,
            IsTojiru = saveData.IsTojiru,
            IsDevilGreen = saveData.IsDevil
        };
    }
}
