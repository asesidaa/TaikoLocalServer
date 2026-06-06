using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Dtos;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

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
            DispLevelTotal = common.DispLevelTotal,
            DispLevelChassis = common.DispLevelChassis,
            DispLevelSelf = common.DispLevelSelf,
            DifficultyPlayedCourse = common.DifficultyPlayedCourse,
            DifficultyPlayedStar = common.DifficultyPlayedStar,
            IsChallengecompe = common.IsChallengeCompe.GetValueOrDefault(),
            IsTojiru = common.IsTojiru.GetValueOrDefault(),
            IsDevil = common.IsDevilBlue.GetValueOrDefault()
        };

        response.DispTaikojukuDan = common.DispTaikojukuDan is { } dispTaikojukuDan
                                    && dispTaikojukuDan is >= 1 and <= 25
            ? dispTaikojukuDan
            : 1u;

        if (common.TokkunTutorialFlg is { } tokkunTutorialFlg)
        {
            response.TokkunTutorialFlg = tokkunTutorialFlg;
        }

        return response;
    }
}
