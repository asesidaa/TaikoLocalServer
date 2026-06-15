using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15UserDataServiceTests
{
    [Fact]
    public void BuildResponse_OrsCatalogAndSaveReleaseFlagsThenLocksShopSongs()
    {
        var response = Ac15UserDataService.BuildResponse(
            new Ac15UserDataSnapshot(
                SongHashVersion: 456,
                CatalogReleaseSongNoes: [101],
                SaveReleaseSongFlg: Ac15ProtocolBytes.CreateFixedBitset([102], 128),
                ToneFlg: Ac15ProtocolBytes.CreateFixedBitset([1], 16),
                TitleFlg: [],
                DefaultOptionSetting: [],
                OptionFlg: [],
                Favorites: [101],
                Recent: [102],
                RecommendSong: 101,
                RecommendBestSongs: [102],
                Counters: new Ac15ProfileCounters { SongFavoriteCnt = 1 },
                DisplayDan: 1,
                LockedSongIds: [102],
                LockedToneIds: []),
            Ac15EraProfiles.Blue);

        Assert.Equal(1u, response.Result);
        Assert.Equal(456u, response.SongFlags.SongHashVer);
        Assert.True((response.SongFlags.ReleaseSongFlg[101 >> 3] & (1 << (101 & 7))) != 0);
        Assert.True((response.SongFlags.ReleaseSongFlg[102 >> 3] & (1 << (102 & 7))) == 0);
        Assert.Equal([101u], response.SongLists.AryFavoriteSongNoes);
        Assert.Equal([102u], response.SongLists.AryRecentSongNoes);
        Assert.Equal(1u, response.Counters.SongFavoriteCnt);
    }

    [Fact]
    public void BuildResponse_BuildsSharedFlagsRecommendationsCountersAndDisplay()
    {
        var snapshot = MinimalSnapshot() with
        {
            ToneFlg = Ac15ProtocolBytes.CreateFixedBitset([4], Ac15EraProfiles.Green.Limits.ToneFlagBytes),
            TitleFlg = Ac15ProtocolBytes.CreateFixedBitset([10], Ac15EraProfiles.Green.Limits.TitleFlagBytes),
            DefaultOptionSetting = [7, 8],
            RecommendSong = 101,
            RecommendBestSongs = [101, 102],
            Counters = new Ac15ProfileCounters
            {
                CategJpopCnt = 2,
                SongRecentCnt = 3,
                DefaultShinSetting = true,
                DispLevelTotal = 4,
                DispLevelChassis = 5,
                DispScoreType = 2,
                DispLevelSelf = 6,
                DifficultyPlayedCourse = 7,
                DifficultyPlayedStar = 8,
                IsChallengeCompe = true,
                IsTojiru = true
            },
            DisplayDan = 0
        };

        var response = Ac15UserDataService.BuildResponse(snapshot, Ac15EraProfiles.Green);

        Assert.True((response.SongFlags.ToneFlg[4 >> 3] & (1 << (4 & 7))) != 0);
        Assert.True((response.SongFlags.TitleFlg[10 >> 3] & (1 << (10 & 7))) != 0);
        Assert.Equal([7, 8], response.Display.DefaultOptionSetting);
        Assert.Equal(101u, response.Recommendations.RecommendSong);
        Assert.Equal([101u, 102u], response.Recommendations.RecommendBestSong);
        Assert.Equal(2u, response.Counters.CategJpopCnt);
        Assert.Equal(3u, response.Counters.SongRecentCnt);
        Assert.True(response.Display.DefaultShinSetting);
        Assert.Equal(4u, response.Display.DispLevelTotal);
        Assert.Equal(5u, response.Display.DispLevelChassis);
        Assert.Equal(2u, response.Display.DispScoreType);
        Assert.Equal(6u, response.Display.DispLevelSelf);
        Assert.Equal(1u, response.Display.DispTaikojukuDan);
        Assert.Equal(7u, response.Display.DifficultyPlayedCourse);
        Assert.Equal(8u, response.Display.DifficultyPlayedStar);
        Assert.True(response.Display.IsChallengeCompe);
        Assert.True(response.Display.IsTojiru);
        Assert.Null(response.Tutorial);
        Assert.Null(response.ModeFlags);
    }

    private static Ac15UserDataSnapshot MinimalSnapshot() => new(
        SongHashVersion: 1,
        CatalogReleaseSongNoes: [],
        SaveReleaseSongFlg: [],
        ToneFlg: [],
        TitleFlg: [],
        DefaultOptionSetting: [],
        OptionFlg: [],
        Favorites: [],
        Recent: [],
        RecommendSong: 0,
        RecommendBestSongs: [],
        Counters: new Ac15ProfileCounters(),
        DisplayDan: 1,
        LockedSongIds: [],
        LockedToneIds: []);
}
