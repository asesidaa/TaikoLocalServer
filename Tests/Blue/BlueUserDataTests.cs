using TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Blue.Wire;

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

        var response = await handler.Handle(new Ac15UserDataQuery(9, GameEra.Blue), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(BlueProtocolBytes.SongFlagBytes, response.SongFlags.ReleaseSongFlg.Length);
        foreach (var song in fixture.Catalog.Blue().MusicInfoFileOrder)
        {
            Assert.True(BitIsSet(response.SongFlags.ReleaseSongFlg, song.SongNo), $"Expected song {song.SongNo} to be unlocked.");
        }
        Assert.True(BitIsSet(response.SongFlags.ReleaseSongFlg, 104));
        Assert.Equal([102u, 101u], response.SongLists.AryFavoriteSongNoes.OrderByDescending(song => song).ToArray());
        Assert.Equal([102u, 101u], response.SongLists.AryRecentSongNoes);
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
        save.DispScoreType = 2;
        save.DispLevelSelf = 4;
        save.IsDevil = true;
        fixture.Context.UserSaveDataBlue.Add(save);
        await fixture.Context.SaveChangesAsync();
        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new Ac15UserDataQuery(1, GameEra.Blue), CancellationToken.None);

        Assert.Equal(BlueProtocolBytes.ToneFlagBytes, response.SongFlags.ToneFlg.Length);
        Assert.Equal(BlueProtocolBytes.TitleFlagBytes, response.SongFlags.TitleFlg.Length);
        Assert.True(BitIsSet(response.SongFlags.ToneFlg, 4));
        Assert.True(BitIsSet(response.SongFlags.TitleFlg, 10));
        Assert.False(response.Display.IsTojiru);
        Assert.Equal(2u, response.Display.DispLevelTotal);
        Assert.Equal(3u, response.Display.DispLevelChassis);
        Assert.Equal(2u, response.Display.DispScoreType);
        Assert.Equal(4u, response.Display.DispLevelSelf);
        Assert.True(response.ModeFlags!.IsDevil);

        var wire = AssembleBlueUserDataResponse(response);
        Assert.True(wire.ShouldSerializeDispScoreType());
        Assert.Equal(2u, wire.DispScoreType);
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

        var response = await handler.Handle(new Ac15UserDataQuery(1, GameEra.Blue), CancellationToken.None);

        Assert.Equal(102u, response.Recommendations.RecommendSong);
        Assert.Equal(new List<uint> { 101, 102, 103 }, response.Recommendations.RecommendBestSong);
    }

    [Fact]
    public async Task UserData_Blue_ComputesSafeDisplayDanFromBlueDanRows()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        save.DispTaikojukuDan = 1;
        fixture.Context.UserSaveDataBlue.Add(save);
        fixture.Context.DanScoreDataBlue.Add(new DanScoreDatumBlue
        {
            Baid = 1,
            DanId = 1,
            IsExtra = false,
            ClearGrade = Ac15DanClearGrade.NormalClear
        });
        await fixture.Context.SaveChangesAsync();
        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new Ac15UserDataQuery(1, GameEra.Blue), CancellationToken.None);

        Assert.Equal(2u, response.Display.DispTaikojukuDan);
    }

    [Fact]
    public async Task UserData_Blue_OmitsAbsentTokkunTutorialFlag()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateUserDataHandler(fixture);

        var response = await handler.Handle(new Ac15UserDataQuery(1, GameEra.Blue), CancellationToken.None);
        var wire = AssembleBlueUserDataResponse(response);

        Assert.Null(response.Tutorial!.TokkunTutorialFlg);
        Assert.False(wire.ShouldSerializeTokkunTutorialFlg());
    }

    [Fact]
    public async Task UserData_Blue_ReturnsPersistedRawTokkunTutorialFlag()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        save.TokkunTutorialFlg = 7;
        fixture.Context.UserSaveDataBlue.Add(save);
        await fixture.Context.SaveChangesAsync();
        var handler = CreateUserDataHandler(fixture);

        var response = await handler.Handle(new Ac15UserDataQuery(1, GameEra.Blue), CancellationToken.None);
        var wire = AssembleBlueUserDataResponse(response);

        Assert.Equal(7u, response.Tutorial!.TokkunTutorialFlg);
        Assert.True(wire.ShouldSerializeTokkunTutorialFlg());
        Assert.Equal(7u, wire.TokkunTutorialFlg);
    }

    [Fact]
    public async Task UserData_Blue_ReadsBackTokkunTutorialFlagPersistedByPlayResult()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var playResultHandler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);
        var userDataHandler = CreateUserDataHandler(fixture);
        var request = CreateTokkunRequest(1);
        request.TokkunTutorialFlg = 7;

        var playResult = await playResultHandler.Handle(
            new UpdateAc15PlayResultCommand(request.Baid, GameEra.Blue, PlayResultMappers.Map(request)),
            CancellationToken.None);
        var response = await userDataHandler.Handle(new Ac15UserDataQuery(1, GameEra.Blue), CancellationToken.None);
        var wire = AssembleBlueUserDataResponse(response);

        Assert.Equal(1u, playResult);
        Assert.Equal(7u, response.Tutorial!.TokkunTutorialFlg);
        Assert.True(wire.ShouldSerializeTokkunTutorialFlg());
        Assert.Equal(7u, wire.TokkunTutorialFlg);
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;

    private static UserDataQueryHandler CreateUserDataHandler(BlueHandlerFixture fixture)
        => new(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

    private static UserDataResponse AssembleBlueUserDataResponse(Ac15UserDataResponse common)
    {
        var response = new UserDataResponse
        {
            Result = common.Result
        };

        UserDataMappers.Apply(common.SongFlags, response);
        UserDataMappers.Apply(common.SongLists, response);
        UserDataMappers.Apply(common.Recommendations, response);
        UserDataMappers.Apply(common.Counters, response);
        UserDataMappers.Apply(common.Display, response);

        if (common.ModeFlags is { } modeFlags)
        {
            UserDataMappers.Apply(modeFlags, response);
        }

        if (common.Tutorial is { } tutorial)
        {
            UserDataMappers.Apply(tutorial, response);
        }

        return response;
    }

    private static PlayResultRequest CreateTokkunRequest(uint baid) => new()
    {
        Baid = baid,
        ChassisId = "268410000000",
        ShopId = "JPN0JPN0123",
        PlayDatetime = "20260528120000",
        IsRight = false,
        CardType = 1,
        IsTwoPlayers = false,
        BonusDailyFlg = false,
        BonusWeeklyFlg = false,
        BonusMonthlyFlg = false,
        GetDonmedal = 0,
        GetKatsumedal = 0,
        GenderType = 0,
        PlayerAge = 0,
        PlayMode = (uint)PlayMode.Tokkun,
        AreaCode = 1,
        Reserved = new byte[16],
        TokkunTutorialFlg = 1,
        AryTokkunstageInfo = new PlayResultRequest.TokkunstageData
        {
            BanacoinDatetime = "20260528120000",
            TokkunSongCnt = 1,
            TookunSongnoes = [101],
            TokkunSpeedchangeCnt = 2,
            TokkunAutoplayCnt = 3,
            TokkunJumpCnt = 4
        }
    };
}
