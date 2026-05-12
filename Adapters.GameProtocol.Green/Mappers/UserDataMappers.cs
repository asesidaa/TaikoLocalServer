using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class UserDataMappers
{
    public static UserDataResponse Map(CommonUserDataResponse common)
    {
        var response = new UserDataResponse
        {
            Result = common.Result,
            AryFavoriteSongNoes = common.AryFavoriteSongNoes,
            AryRecentSongNoes = common.AryRecentSongNoes,
            SongHashVer = common.SongHashVer,
            HashReleaseSongFlg = common.ReleaseSongFlg,
            OptionFlg = common.OptionFlg,
            ToneFlg = common.ToneFlg,
            TitleFlg = common.TitleFlg,
            CategJpopCnt = common.CategJpopCnt.GetValueOrDefault(),
            CategAnimeCnt = common.CategAnimeCnt.GetValueOrDefault(),
            CategDoyoCnt = common.CategDoyoCnt.GetValueOrDefault(),
            CategVarietyCnt = common.CategVarietyCnt.GetValueOrDefault(),
            CategClassicCnt = common.CategClassicCnt.GetValueOrDefault(),
            CategGameCnt = common.CategGameCnt.GetValueOrDefault(),
            CategNamcoCnt = common.CategNamcoCnt.GetValueOrDefault(),
            CategVocaloidCnt = common.CategVocaloidCnt.GetValueOrDefault(),
            SongPushedCnt = common.SongPushedCnt.GetValueOrDefault(),
            SongFavoriteCnt = common.SongFavoriteCnt.GetValueOrDefault(),
            PrevAreaCode = common.PrevAreaCode.GetValueOrDefault(),
            ConsecAreaCnt = common.ConsecAreaCnt.GetValueOrDefault(),
            RecommendSong = common.RecommendSong.GetValueOrDefault(),
            RecommendBestSongs = common.RecommendBestSong.ToArray(),
            TotalCreditCnt = common.TotalCreditCnt,
            SongRecentCnt = common.SongRecentCnt,
            DefaultOptionSetting = common.DefaultOptionSetting,
            DefaultShinSetting = common.DefaultShinSetting.GetValueOrDefault(),
            DifficultyPlayedCourse = common.DifficultyPlayedCourse,
            DifficultyPlayedStar = common.DifficultyPlayedStar,
            IsChallengecompe = common.IsChallengeCompe.GetValueOrDefault(),
            IsTojiru = common.IsTojiru.GetValueOrDefault(),
            IsDevil = common.IsDevilGreen.GetValueOrDefault()
        };

        if (common.DispTaikojukuDan is { } dispTaikojukuDan && dispTaikojukuDan is >= 1 and <= 25)
        {
            response.DispTaikojukuDan = dispTaikojukuDan;
        }

        return response;
    }
}
