using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Dtos;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

[Mapper]
public static partial class UserDataMappers
{
    [MapProperty(nameof(CommonUserDataResponse.ReleaseSongFlg), nameof(UserDataResponse.HashReleaseSongFlg))]
    [MapProperty(nameof(CommonUserDataResponse.RecommendBestSong), nameof(UserDataResponse.RecommendBestSongs))]
    [MapProperty(nameof(CommonUserDataResponse.CategJpopCnt), nameof(UserDataResponse.CategJpopCnt), Use = nameof(MapPresentUInt32))]
    [MapProperty(nameof(CommonUserDataResponse.CategAnimeCnt), nameof(UserDataResponse.CategAnimeCnt), Use = nameof(MapPresentUInt32))]
    [MapProperty(nameof(CommonUserDataResponse.CategDoyoCnt), nameof(UserDataResponse.CategDoyoCnt), Use = nameof(MapPresentUInt32))]
    [MapProperty(nameof(CommonUserDataResponse.CategVarietyCnt), nameof(UserDataResponse.CategVarietyCnt), Use = nameof(MapPresentUInt32))]
    [MapProperty(nameof(CommonUserDataResponse.CategClassicCnt), nameof(UserDataResponse.CategClassicCnt), Use = nameof(MapPresentUInt32))]
    [MapProperty(nameof(CommonUserDataResponse.CategGameCnt), nameof(UserDataResponse.CategGameCnt), Use = nameof(MapPresentUInt32))]
    [MapProperty(nameof(CommonUserDataResponse.CategNamcoCnt), nameof(UserDataResponse.CategNamcoCnt), Use = nameof(MapPresentUInt32))]
    [MapProperty(nameof(CommonUserDataResponse.CategVocaloidCnt), nameof(UserDataResponse.CategVocaloidCnt), Use = nameof(MapPresentUInt32))]
    [MapProperty(nameof(CommonUserDataResponse.SongPushedCnt), nameof(UserDataResponse.SongPushedCnt), Use = nameof(MapPresentUInt32))]
    [MapProperty(nameof(CommonUserDataResponse.SongFavoriteCnt), nameof(UserDataResponse.SongFavoriteCnt), Use = nameof(MapPresentUInt32))]
    [MapProperty(nameof(CommonUserDataResponse.PrevAreaCode), nameof(UserDataResponse.PrevAreaCode), Use = nameof(MapPresentUInt32))]
    [MapProperty(nameof(CommonUserDataResponse.ConsecAreaCnt), nameof(UserDataResponse.ConsecAreaCnt), Use = nameof(MapPresentUInt32))]
    [MapProperty(nameof(CommonUserDataResponse.RecommendSong), nameof(UserDataResponse.RecommendSong), Use = nameof(MapPresentUInt32))]
    [MapProperty(nameof(CommonUserDataResponse.DefaultShinSetting), nameof(UserDataResponse.DefaultShinSetting), Use = nameof(MapPresentBoolean))]
    [MapProperty(nameof(CommonUserDataResponse.DispTaikojukuDan), nameof(UserDataResponse.DispTaikojukuDan), Use = nameof(MapDispTaikojukuDan))]
    [MapProperty(nameof(CommonUserDataResponse.IsChallengeCompe), nameof(UserDataResponse.IsChallengecompe), Use = nameof(MapPresentBoolean))]
    [MapProperty(nameof(CommonUserDataResponse.IsTojiru), nameof(UserDataResponse.IsTojiru), Use = nameof(MapPresentBoolean))]
    [MapProperty(nameof(CommonUserDataResponse.IsDevilBlue), nameof(UserDataResponse.IsDevil), Use = nameof(MapPresentBoolean))]
    [MapperIgnoreTarget(nameof(UserDataResponse.AryFriendInfoes))]
    [MapperIgnoreTarget(nameof(UserDataResponse.IsExplain))]
    public static partial UserDataResponse Map(CommonUserDataResponse common);

    private static uint? MapPresentUInt32(uint? value) => value.GetValueOrDefault();

    private static bool? MapPresentBoolean(bool? value) => value.GetValueOrDefault();

    private static uint? MapDispTaikojukuDan(uint? value) => value is >= 1 and <= 25 ? value : 1u;
}
