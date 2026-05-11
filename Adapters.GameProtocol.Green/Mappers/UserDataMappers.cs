using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class UserDataMappers
{
    public static UserDataResponse Map(CommonUserDataResponse common)
    {
        // TODO iter 2: map Green user-data arrays and category counters.
        return new UserDataResponse
        {
            Result = common.Result,
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
            DefaultShinSetting = common.DefaultShinSetting.GetValueOrDefault(),
            DispTaikojukuDan = common.DispTaikojukuDan.GetValueOrDefault(),
            IsChallengecompe = common.IsChallengeCompe.GetValueOrDefault(),
            IsTojiru = common.IsTojiru.GetValueOrDefault(),
            IsDevil = common.IsDevilGreen.GetValueOrDefault()
        };
    }
}
