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
                LockedToneIds: [],
                TokkunTutorialFlg: null,
                IsDevil: false),
            Ac15EraProfiles.Blue);

        Assert.Equal(1u, response.Result);
        Assert.Equal(456u, response.SongHashVer);
        Assert.True((response.ReleaseSongFlg[101 >> 3] & (1 << (101 & 7))) != 0);
        Assert.True((response.ReleaseSongFlg[102 >> 3] & (1 << (102 & 7))) == 0);
        Assert.Equal([101u], response.AryFavoriteSongNoes);
        Assert.Equal([102u], response.AryRecentSongNoes);
        Assert.Equal(1u, response.SongFavoriteCnt);
    }

    [Fact]
    public void BuildResponse_AddsTokkunTutorialOnlyWhenProfilePlacesItInUserdata()
    {
        var snapshot = MinimalSnapshot() with { TokkunTutorialFlg = 9 };

        var blue = Ac15UserDataService.BuildResponse(snapshot, Ac15EraProfiles.Blue);
        var green = Ac15UserDataService.BuildResponse(snapshot, Ac15EraProfiles.Green);

        Assert.Equal(9u, blue.TokkunTutorialFlg);
        Assert.Null(green.TokkunTutorialFlg);
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
        LockedToneIds: [],
        TokkunTutorialFlg: null,
        IsDevil: false);
}
