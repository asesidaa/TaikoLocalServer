namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueUserDataTests
{
    [Fact]
    public async Task UserData_Blue_UnlocksCatalogAndSavedReleaseSongsAndReturnsFavoriteRecentArrays()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 9, MyDonName = "DON" });
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(9);
        save.ReleaseSongFlg = BlueProtocolBytes.CreateFixedBitset([104], BlueProtocolBytes.SongFlagBytes);
        fixture.Context.UserSaveDataBlue.Add(save);
        fixture.Context.BlueFavoriteSongs.AddRange(
            new BlueFavoriteSongs { Baid = 9, SongNo = 102 },
            new BlueFavoriteSongs { Baid = 9, SongNo = 101 });
        fixture.Context.BlueRecentSongs.AddRange(
            new BlueRecentSongs { Baid = 9, SongNo = 101, LastPlayed = new DateTime(2026, 5, 28, 12, 0, 0) },
            new BlueRecentSongs { Baid = 9, SongNo = 102, LastPlayed = new DateTime(2026, 5, 28, 13, 0, 0) });
        await fixture.Context.SaveChangesAsync();
        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new UserDataQuery(9, GameEra.Blue), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(BlueProtocolBytes.SongFlagBytes, response.ReleaseSongFlg.Length);
        foreach (var song in fixture.Catalog.Blue().MusicInfoFileOrder)
        {
            Assert.True(BitIsSet(response.ReleaseSongFlg, song.SongNo), $"Expected song {song.SongNo} to be unlocked.");
        }
        Assert.True(BitIsSet(response.ReleaseSongFlg, 104));
        Assert.Equal([102u, 101u], response.AryFavoriteSongNoes.OrderByDescending(song => song).ToArray());
        Assert.Equal([102u, 101u], response.AryRecentSongNoes);
    }

    [Fact]
    public async Task UserData_Blue_ReturnsPersistedToneTitleAndDisplaySettings()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        save.ToneFlg = BlueProtocolBytes.CreateFixedBitset([0, 4], BlueProtocolBytes.ToneFlagBytes);
        save.TitleFlg = BlueProtocolBytes.CreateFixedBitset([10, 131], BlueProtocolBytes.TitleFlagBytes);
        save.IsTojiru = false;
        save.DispLevelTotal = 2;
        save.DispLevelChassis = 3;
        save.DispLevelSelf = 4;
        save.IsDevil = true;
        fixture.Context.UserSaveDataBlue.Add(save);
        await fixture.Context.SaveChangesAsync();
        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new UserDataQuery(1, GameEra.Blue), CancellationToken.None);

        Assert.Equal(BlueProtocolBytes.ToneFlagBytes, response.ToneFlg.Length);
        Assert.Equal(BlueProtocolBytes.TitleFlagBytes, response.TitleFlg.Length);
        Assert.True(BitIsSet(response.ToneFlg, 4));
        Assert.True(BitIsSet(response.TitleFlg, 10));
        Assert.False(response.IsTojiru);
        Assert.Equal(2u, response.DispLevelTotal);
        Assert.Equal(3u, response.DispLevelChassis);
        Assert.Equal(4u, response.DispLevelSelf);
        Assert.True(response.IsDevilBlue);
    }

    [Fact]
    public async Task UserData_Blue_RecommendComesFromCatalog()
    {
        var blueCatalog = new BlueHandlerFixture.TestBlueCatalog(
            recommend: new TaikoLocalServer.Application.Catalog.Blue.BlueRecommendEntry
            {
                RecommendSong = 102,
                RecommendBestSongs = [101, 102, 103]
            });
        await using var fixture = await BlueHandlerFixture.CreateAsync(blueCatalog);
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new UserDataQuery(1, GameEra.Blue), CancellationToken.None);

        Assert.Equal(102u, response.RecommendSong);
        Assert.Equal(new List<uint> { 101, 102, 103 }, response.RecommendBestSong);
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
