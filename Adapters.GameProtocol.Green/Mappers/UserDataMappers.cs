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

        // disp_taikojuku_dan MUST be 1..25 on the wire. Omitting it does not
        // help: the Green client reads disp_taikojuku_dan_ at message +0x31C
        // without checking proto2 presence (sub_19CFE0:151, sub_1016F8:506,
        // sub_24377C:176, sub_7FDFFC:755). Any out-of-range value - including
        // 0 from an absent tag - underflows Taikojuku_GetDanSlotSongRange's
        // 84-byte-per-slot table at 0x127F98 and crashes the client at boot.
        // sub_7FDFFC uses 1 as its baked-in "no data" default; mirror that.
        response.DispTaikojukuDan = common.DispTaikojukuDan is { } dispTaikojukuDan
                                    && dispTaikojukuDan is >= 1 and <= 25
            ? dispTaikojukuDan
            : 1u;

        return response;
    }
}
