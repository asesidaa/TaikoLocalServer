using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15UserDataService
{
    public static Ac15UserDataResponse BuildResponse(
        Ac15UserDataSnapshot snapshot,
        Ac15EraProfile profile)
    {
        var release = Ac15ProtocolBytes.OrBitsets(
            Ac15ProtocolBytes.CreateFixedBitset(snapshot.CatalogReleaseSongNoes, profile.Limits.SongFlagBytes),
            snapshot.SaveReleaseSongFlg,
            profile.Limits.SongFlagBytes);
        release = ClearBits(release, snapshot.LockedSongIds, profile.Limits.SongFlagBytes);

        return new Ac15UserDataResponse
        {
            Result = 1,
            SongFlags = new Ac15UserDataSongFlags
            {
                SongHashVer = snapshot.SongHashVersion,
                ReleaseSongFlg = release,
                ToneFlg = ClearBits(snapshot.ToneFlg, snapshot.LockedToneIds, profile.Limits.ToneFlagBytes),
                TitleFlg = Ac15ProtocolBytes.FixedOrZero(snapshot.TitleFlg, profile.Limits.TitleFlagBytes),
                OptionFlg = snapshot.OptionFlg
            },
            SongLists = new Ac15UserDataSongLists
            {
                AryFavoriteSongNoes = snapshot.Favorites.ToArray(),
                AryRecentSongNoes = snapshot.Recent.ToArray()
            },
            Recommendations = Ac15RecommendationService.BuildUserDataRecommendations(snapshot.CatalogReleaseSongNoes),
            Counters = new Ac15UserDataProfileCounters
            {
                CategJpopCnt = snapshot.Counters.CategJpopCnt,
                CategAnimeCnt = snapshot.Counters.CategAnimeCnt,
                CategDoyoCnt = snapshot.Counters.CategDoyoCnt,
                CategVarietyCnt = snapshot.Counters.CategVarietyCnt,
                CategClassicCnt = snapshot.Counters.CategClassicCnt,
                CategGameCnt = snapshot.Counters.CategGameCnt,
                CategNamcoCnt = snapshot.Counters.CategNamcoCnt,
                CategVocaloidCnt = snapshot.Counters.CategVocaloidCnt,
                SongPushedCnt = snapshot.Counters.SongPushedCnt,
                SongFavoriteCnt = snapshot.Counters.SongFavoriteCnt,
                SongRecentCnt = snapshot.Counters.SongRecentCnt,
                TotalCreditCnt = snapshot.Counters.TotalCreditCnt,
                PrevAreaCode = snapshot.Counters.PrevAreaCode,
                ConsecAreaCnt = snapshot.Counters.ConsecAreaCnt
            },
            Display = new Ac15UserDataDisplaySettings
            {
                DefaultOptionSetting = Ac15ProtocolBytes.FixedOrZero(snapshot.DefaultOptionSetting, 2),
                DefaultShinSetting = snapshot.Counters.DefaultShinSetting,
                DispLevelTotal = snapshot.Counters.DispLevelTotal,
                DispLevelChassis = snapshot.Counters.DispLevelChassis,
                DispScoreType = snapshot.Counters.DispScoreType,
                DispLevelSelf = snapshot.Counters.DispLevelSelf,
                DispTaikojukuDan = GetSafeDisplayDan(snapshot.DisplayDan, profile),
                DifficultyPlayedCourse = snapshot.Counters.DifficultyPlayedCourse,
                DifficultyPlayedStar = snapshot.Counters.DifficultyPlayedStar,
                IsChallengeCompe = snapshot.Counters.IsChallengeCompe,
                IsTojiru = snapshot.Counters.IsTojiru
            }
        };
    }

    private static byte[] ClearBits(byte[] source, IEnumerable<uint> ids, int byteCount)
    {
        var result = Ac15ProtocolBytes.FixedOrZero(source, byteCount);
        var maxBits = byteCount * 8;
        foreach (var id in ids)
        {
            if (id >= maxBits)
            {
                continue;
            }

            result[id >> 3] &= (byte)~(1 << ((int)id & 7));
        }

        return result;
    }

    private static uint GetSafeDisplayDan(uint value, Ac15EraProfile profile)
        => value >= profile.Limits.MinNormalDanId && value <= profile.Limits.MaxNormalDanId
            ? value
            : profile.Limits.SafeDisplayDanFallback;
}
