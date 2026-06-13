using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

[Mapper]
public static partial class UserDataMappers
{
    [MapPropertyFromSource(nameof(UserDataResponse.HashReleaseSongFlg), Use = nameof(MapReleaseSongFlg))]
    [MapPropertyFromSource(nameof(UserDataResponse.SongHashVer), Use = nameof(MapSongHashVer))]
    [MapPropertyFromSource(nameof(UserDataResponse.ToneFlg), Use = nameof(MapToneFlg))]
    [MapPropertyFromSource(nameof(UserDataResponse.TitleFlg), Use = nameof(MapTitleFlg))]
    [MapPropertyFromSource(nameof(UserDataResponse.OptionFlg), Use = nameof(MapOptionFlg))]
    [MapPropertyFromSource(nameof(UserDataResponse.AryFavoriteSongNoes), Use = nameof(MapFavoriteSongs))]
    [MapPropertyFromSource(nameof(UserDataResponse.AryRecentSongNoes), Use = nameof(MapRecentSongs))]
    [MapPropertyFromSource(nameof(UserDataResponse.RecommendSong), Use = nameof(MapRecommendSong))]
    [MapPropertyFromSource(nameof(UserDataResponse.RecommendBestSongs), Use = nameof(MapRecommendBestSongs))]
    [MapPropertyFromSource(nameof(UserDataResponse.CategJpopCnt), Use = nameof(MapCategJpopCnt))]
    [MapPropertyFromSource(nameof(UserDataResponse.CategAnimeCnt), Use = nameof(MapCategAnimeCnt))]
    [MapPropertyFromSource(nameof(UserDataResponse.CategDoyoCnt), Use = nameof(MapCategDoyoCnt))]
    [MapPropertyFromSource(nameof(UserDataResponse.CategVarietyCnt), Use = nameof(MapCategVarietyCnt))]
    [MapPropertyFromSource(nameof(UserDataResponse.CategClassicCnt), Use = nameof(MapCategClassicCnt))]
    [MapPropertyFromSource(nameof(UserDataResponse.CategGameCnt), Use = nameof(MapCategGameCnt))]
    [MapPropertyFromSource(nameof(UserDataResponse.CategNamcoCnt), Use = nameof(MapCategNamcoCnt))]
    [MapPropertyFromSource(nameof(UserDataResponse.CategVocaloidCnt), Use = nameof(MapCategVocaloidCnt))]
    [MapPropertyFromSource(nameof(UserDataResponse.SongPushedCnt), Use = nameof(MapSongPushedCnt))]
    [MapPropertyFromSource(nameof(UserDataResponse.SongFavoriteCnt), Use = nameof(MapSongFavoriteCnt))]
    [MapPropertyFromSource(nameof(UserDataResponse.SongRecentCnt), Use = nameof(MapSongRecentCnt))]
    [MapPropertyFromSource(nameof(UserDataResponse.TotalCreditCnt), Use = nameof(MapTotalCreditCnt))]
    [MapPropertyFromSource(nameof(UserDataResponse.PrevAreaCode), Use = nameof(MapPrevAreaCode))]
    [MapPropertyFromSource(nameof(UserDataResponse.ConsecAreaCnt), Use = nameof(MapConsecAreaCnt))]
    [MapPropertyFromSource(nameof(UserDataResponse.DefaultOptionSetting), Use = nameof(MapDefaultOptionSetting))]
    [MapPropertyFromSource(nameof(UserDataResponse.DefaultShinSetting), Use = nameof(MapDefaultShinSetting))]
    [MapPropertyFromSource(nameof(UserDataResponse.DispLevelTotal), Use = nameof(MapDispLevelTotal))]
    [MapPropertyFromSource(nameof(UserDataResponse.DispLevelChassis), Use = nameof(MapDispLevelChassis))]
    [MapPropertyFromSource(nameof(UserDataResponse.DispLevelSelf), Use = nameof(MapDispLevelSelf))]
    [MapPropertyFromSource(nameof(UserDataResponse.DispTaikojukuDan), Use = nameof(MapDispTaikojukuDan))]
    [MapPropertyFromSource(nameof(UserDataResponse.DifficultyPlayedCourse), Use = nameof(MapDifficultyPlayedCourse))]
    [MapPropertyFromSource(nameof(UserDataResponse.DifficultyPlayedStar), Use = nameof(MapDifficultyPlayedStar))]
    [MapPropertyFromSource(nameof(UserDataResponse.IsChallengecompe), Use = nameof(MapIsChallengeCompe))]
    [MapPropertyFromSource(nameof(UserDataResponse.IsTojiru), Use = nameof(MapIsTojiru))]
    [MapPropertyFromSource(nameof(UserDataResponse.IsDevil), Use = nameof(MapIsDevil))]
    [MapPropertyFromSource(nameof(UserDataResponse.IsExplain), Use = nameof(MapIsExplain))]
    [MapPropertyFromSource(nameof(UserDataResponse.TokkunTutorialFlg), Use = nameof(MapTokkunTutorialFlg))]
    [MapperIgnoreTarget(nameof(UserDataResponse.AryFriendInfoes))]
    [MapperIgnoreTarget(nameof(UserDataResponse.DispScoreType))]
    public static partial UserDataResponse Map(Ac15UserDataResponse common);

    private static byte[] MapReleaseSongFlg(Ac15UserDataResponse response) => response.SongFlags.ReleaseSongFlg;
    private static uint? MapSongHashVer(Ac15UserDataResponse response) => response.SongFlags.SongHashVer;
    private static byte[] MapToneFlg(Ac15UserDataResponse response) => response.SongFlags.ToneFlg;
    private static byte[] MapTitleFlg(Ac15UserDataResponse response) => response.SongFlags.TitleFlg;
    private static byte[] MapOptionFlg(Ac15UserDataResponse response) => response.SongFlags.OptionFlg;
    private static uint[] MapFavoriteSongs(Ac15UserDataResponse response) => response.SongLists.AryFavoriteSongNoes;
    private static uint[] MapRecentSongs(Ac15UserDataResponse response) => response.SongLists.AryRecentSongNoes;
    private static uint? MapRecommendSong(Ac15UserDataResponse response) => response.Recommendations.RecommendSong;
    private static uint[] MapRecommendBestSongs(Ac15UserDataResponse response) => response.Recommendations.RecommendBestSong.ToArray();
    private static uint? MapCategJpopCnt(Ac15UserDataResponse response) => response.Counters.CategJpopCnt;
    private static uint? MapCategAnimeCnt(Ac15UserDataResponse response) => response.Counters.CategAnimeCnt;
    private static uint? MapCategDoyoCnt(Ac15UserDataResponse response) => response.Counters.CategDoyoCnt;
    private static uint? MapCategVarietyCnt(Ac15UserDataResponse response) => response.Counters.CategVarietyCnt;
    private static uint? MapCategClassicCnt(Ac15UserDataResponse response) => response.Counters.CategClassicCnt;
    private static uint? MapCategGameCnt(Ac15UserDataResponse response) => response.Counters.CategGameCnt;
    private static uint? MapCategNamcoCnt(Ac15UserDataResponse response) => response.Counters.CategNamcoCnt;
    private static uint? MapCategVocaloidCnt(Ac15UserDataResponse response) => response.Counters.CategVocaloidCnt;
    private static uint? MapSongPushedCnt(Ac15UserDataResponse response) => response.Counters.SongPushedCnt;
    private static uint? MapSongFavoriteCnt(Ac15UserDataResponse response) => response.Counters.SongFavoriteCnt;
    private static uint? MapSongRecentCnt(Ac15UserDataResponse response) => response.Counters.SongRecentCnt;
    private static uint? MapTotalCreditCnt(Ac15UserDataResponse response) => response.Counters.TotalCreditCnt;
    private static uint? MapPrevAreaCode(Ac15UserDataResponse response) => response.Counters.PrevAreaCode;
    private static uint? MapConsecAreaCnt(Ac15UserDataResponse response) => response.Counters.ConsecAreaCnt;
    private static byte[] MapDefaultOptionSetting(Ac15UserDataResponse response) => response.Display.DefaultOptionSetting;
    private static bool? MapDefaultShinSetting(Ac15UserDataResponse response) => response.Display.DefaultShinSetting;
    private static uint? MapDispLevelTotal(Ac15UserDataResponse response) => response.Display.DispLevelTotal;
    private static uint? MapDispLevelChassis(Ac15UserDataResponse response) => response.Display.DispLevelChassis;
    private static uint? MapDispLevelSelf(Ac15UserDataResponse response) => response.Display.DispLevelSelf;
    private static uint? MapDispTaikojukuDan(Ac15UserDataResponse response) => response.Display.DispTaikojukuDan is >= 1 and <= 25 ? response.Display.DispTaikojukuDan : 1u;
    private static uint? MapDifficultyPlayedCourse(Ac15UserDataResponse response) => response.Display.DifficultyPlayedCourse;
    private static uint? MapDifficultyPlayedStar(Ac15UserDataResponse response) => response.Display.DifficultyPlayedStar;
    private static bool? MapIsChallengeCompe(Ac15UserDataResponse response) => response.Display.IsChallengeCompe;
    private static bool? MapIsTojiru(Ac15UserDataResponse response) => response.Display.IsTojiru;
    private static bool? MapIsDevil(Ac15UserDataResponse response) => response.ModeFlags?.IsDevil;
    private static bool? MapIsExplain(Ac15UserDataResponse response) => response.ModeFlags?.IsExplain;
    private static uint? MapTokkunTutorialFlg(Ac15UserDataResponse response) => response.Tutorial?.TokkunTutorialFlg;
}
