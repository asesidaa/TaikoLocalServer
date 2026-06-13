using TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenIdentityHandlerTests
{
    [Fact]
    public async Task AddMyDonEntry_Green_CreatesIdentityAndEmptySaveData()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new AddMyDonEntryCommandHandler(
            fixture.Context,
            NullLogger<AddMyDonEntryCommandHandler>.Instance);

        var response = await handler.Handle(
            new AddMyDonEntryCommand(GameEra.Green, "12345678901234567890", "DON", 0),
            CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.Equal((uint)1, response.Baid);
        Assert.NotNull(await fixture.Context.Cards.FindAsync("12345678901234567890"));
        Assert.NotNull(await fixture.Context.UserSaveDataGreen.FindAsync(1u));
        Assert.Empty(await fixture.Context.SongBestDataGreen.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task BaidQuery_Green_KnownCardReturnsSaveDataWithoutGrantingFakeDan()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 7, MyDonName = "DON" });
        fixture.Context.Cards.Add(new Card { Baid = 7, AccessCode = "999" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(7));
        await fixture.Context.SaveChangesAsync();

        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new Ac15BaidQuery(GameEra.Green, "999"), CancellationToken.None);

        Assert.False(response.IsNewUser);
        Assert.Equal((uint)7, response.Baid);
        Assert.Equal("DON", response.Identity!.MyDonName);
        Assert.Equal(GreenProtocolBytes.DanFlagBytes, response.Dan!.GotDanFlg.Length);
        Assert.Equal(0, response.Dan.GotDanFlg[0]);
    }

    [Fact]
    public async Task BaidQuery_Green_EmitsSavedAutoCostumeOption()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 7, MyDonName = "DON" });
        fixture.Context.Cards.Add(new Card { Baid = 7, AccessCode = "999" });
        var saveData = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(7);
        saveData.IsAutoCostumeOn = false;
        fixture.Context.UserSaveDataGreen.Add(saveData);
        await fixture.Context.SaveChangesAsync();

        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new Ac15BaidQuery(GameEra.Green, "999"), CancellationToken.None);
        var wire = BaidResponseMapper.Map(response);

        Assert.True(response.Profile!.IsAutoCostumeOn.HasValue);
        Assert.False(response.Profile.IsAutoCostumeOn.GetValueOrDefault());
        Assert.True(wire.ShouldSerializeIsAutoCostumeOn());
        Assert.False(wire.IsAutoCostumeOn);
    }

    [Fact]
    public async Task BaidQuery_Green_MapsSelectedTitleIdToTitleRarityForTitleplate()
    {
        const string titleName = "\u30c4\u30f3\u30c7\u30ecCafe\u306e\u5e38\u9023";
        var greenCatalog = new GreenHandlerFixture.TestGreenCatalog
        {
            TitleDictionary = new Dictionary<uint, Title>
            {
                [228] = new()
                {
                    TitleId = 228,
                    TitleName = titleName,
                    TitleNameEN = titleName,
                    TitleNameCN = titleName,
                    TitleNameKO = titleName,
                    TitleRarity = 2
                }
            }
        };
        await using var fixture = await GreenHandlerFixture.CreateAsync(greenCatalog);
        fixture.Context.UserData.Add(new UserDatum { Baid = 7, MyDonName = "DON" });
        fixture.Context.Cards.Add(new Card { Baid = 7, AccessCode = "999" });
        var saveData = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(7);
        saveData.Title = titleName;
        saveData.TitleplateId = 228;
        fixture.Context.UserSaveDataGreen.Add(saveData);
        await fixture.Context.SaveChangesAsync();

        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new Ac15BaidQuery(GameEra.Green, "999"), CancellationToken.None);
        var wire = BaidResponseMapper.Map(response);

        Assert.Equal(2u, response.Profile!.TitlePlateId);
        Assert.Equal(2u, wire.TitleplateId);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(99, 1)]
    public async Task BaidQuery_Green_NormalizesDispDanTypeToOffOrOn(uint savedValue, uint expected)
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 7, MyDonName = "DON" });
        fixture.Context.Cards.Add(new Card { Baid = 7, AccessCode = "999" });
        var saveData = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(7);
        saveData.DispDanType = savedValue;
        fixture.Context.UserSaveDataGreen.Add(saveData);
        await fixture.Context.SaveChangesAsync();

        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new Ac15BaidQuery(GameEra.Green, "999"), CancellationToken.None);

        Assert.Equal(expected, response.Dan!.DispDanType);
    }

    [Fact]
    public async Task BaidQuery_Green_CardWithoutGreenSaveIsNewForGreenRegistration()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 7, MyDonName = "NIIRO" });
        fixture.Context.Cards.Add(new Card { Baid = 7, AccessCode = "999" });
        fixture.Context.Credentials.Add(new Credential { Baid = 7, Password = string.Empty, Salt = string.Empty });
        fixture.Context.UserSaveDataNijiiro.Add(UserSaveDataNijiiroExtensions.CreateDefaultNijiiroSaveData(7));
        await fixture.Context.SaveChangesAsync();

        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new Ac15BaidQuery(GameEra.Green, "999"), CancellationToken.None);

        Assert.True(response.IsNewUser);
        Assert.Equal((uint)7, response.Baid);
        Assert.Null(await fixture.Context.UserSaveDataGreen.FindAsync(7u));
    }

    [Fact]
    public async Task AddMyDonEntry_Green_CompletesExistingSharedIdentityRegistration()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 7, MyDonName = "NIIRO" });
        fixture.Context.Cards.Add(new Card { Baid = 7, AccessCode = "999" });
        fixture.Context.Credentials.Add(new Credential { Baid = 7, Password = string.Empty, Salt = string.Empty });
        fixture.Context.UserSaveDataNijiiro.Add(UserSaveDataNijiiroExtensions.CreateDefaultNijiiroSaveData(7));
        await fixture.Context.SaveChangesAsync();

        var handler = new AddMyDonEntryCommandHandler(
            fixture.Context,
            NullLogger<AddMyDonEntryCommandHandler>.Instance);

        var response = await handler.Handle(
            new AddMyDonEntryCommand(GameEra.Green, "999", "GREEN", 0),
            CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.Equal((uint)7, response.Baid);
        Assert.NotNull(await fixture.Context.UserSaveDataGreen.FindAsync(7u));
        Assert.Single(await fixture.Context.Cards.Where(card => card.AccessCode == "999").ToListAsync());
        Assert.Single(await fixture.Context.UserData.Where(user => user.Baid == 7).ToListAsync());
        Assert.Single(await fixture.Context.Credentials.Where(credential => credential.Baid == 7).ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataGreen.Where(row => row.Baid == 7).ToListAsync());
    }

    [Fact]
    public async Task BaidQuery_Nijiiro_CardWithoutNijiiroSaveIsNewForNijiiroRegistration()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 8, MyDonName = "GREEN" });
        fixture.Context.Cards.Add(new Card { Baid = 8, AccessCode = "888" });
        fixture.Context.Credentials.Add(new Credential { Baid = 8, Password = string.Empty, Salt = string.Empty });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(8));
        await fixture.Context.SaveChangesAsync();

        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new BaidQuery(GameEra.Nijiiro, "888"), CancellationToken.None);

        Assert.True(response.IsNewUser);
        Assert.Equal((uint)8, response.Baid);
        Assert.Null(await fixture.Context.UserSaveDataNijiiro.FindAsync(8u));
    }

    [Fact]
    public async Task AddMyDonEntry_Nijiiro_CompletesExistingSharedIdentityRegistration()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 8, MyDonName = "GREEN" });
        fixture.Context.Cards.Add(new Card { Baid = 8, AccessCode = "888" });
        fixture.Context.Credentials.Add(new Credential { Baid = 8, Password = string.Empty, Salt = string.Empty });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(8));
        await fixture.Context.SaveChangesAsync();

        var handler = new AddMyDonEntryCommandHandler(
            fixture.Context,
            NullLogger<AddMyDonEntryCommandHandler>.Instance);

        var response = await handler.Handle(
            new AddMyDonEntryCommand(GameEra.Nijiiro, "888", "NIIRO", 0),
            CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.Equal((uint)8, response.Baid);
        Assert.NotNull(await fixture.Context.UserSaveDataNijiiro.FindAsync(8u));
        Assert.Single(await fixture.Context.Cards.Where(card => card.AccessCode == "888").ToListAsync());
        Assert.Single(await fixture.Context.UserData.Where(user => user.Baid == 8).ToListAsync());
        Assert.Single(await fixture.Context.Credentials.Where(credential => credential.Baid == 8).ToListAsync());
    }

    [Fact]
    public async Task InitialData_Green_UnlocksAllCatalogSongs()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Green), CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.Equal(128, response.DefaultSongFlg.Length);
        foreach (var song in fixture.Catalog.Green().MusicInfoFileOrder)
        {
            Assert.True(BitIsSet(response.DefaultSongFlg, song.SongNo), $"Expected song {song.SongNo} to be unlocked.");
        }
    }

    [Fact]
    public async Task UserData_Green_UnlocksAllCatalogSongs()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 9, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(9));
        await fixture.Context.SaveChangesAsync();

        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new UserDataQuery(9, GameEra.Green), CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.Equal(128, response.ReleaseSongFlg.Length);
        foreach (var song in fixture.Catalog.Green().MusicInfoFileOrder)
        {
            Assert.True(BitIsSet(response.ReleaseSongFlg, song.SongNo), $"Expected song {song.SongNo} to be unlocked.");
        }
    }

    // New cards persist DispTaikojukuDan=0, but the wire response must carry
    // sentinel 1: the Green client reads message+0x31C unconditionally and a
    // 0 there crashes Taikojuku_GetDanSlotSongRange @ 0x127F98.
    [Fact]
    public async Task UserData_Green_NewSaveSendsSentinelOneForDispTaikojukuDan()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 9, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(9));
        await fixture.Context.SaveChangesAsync();

        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new UserDataQuery(9, GameEra.Green), CancellationToken.None);
        var wire = UserDataMappers.Map(response);

        Assert.Equal(1u, response.DispTaikojukuDan);
        Assert.True(wire.ShouldSerializeDispTaikojukuDan());
        Assert.Equal(1u, wire.DispTaikojukuDan);
    }

    [Fact]
    public async Task UserData_Green_SongCountersComeFromSaveDataColumns()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.SongPushedCnt = 7;
        save.SongFavoriteCnt = 42;
        save.SongRecentCnt = 99;
        save.CategJpopCnt = 12;
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new UserDataQuery(1, GameEra.Green), CancellationToken.None);

        Assert.Equal(7u, response.SongPushedCnt);
        Assert.Equal(42u, response.SongFavoriteCnt);
        Assert.Equal(99u, response.SongRecentCnt);
        Assert.Equal(12u, response.CategJpopCnt);
    }

    [Fact]
    public async Task UserData_Green_ReturnsPersistedToneAndTitleUnlockFlags()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.ToneFlg = BitsetCodec.Encode([0, 4], GreenProtocolBytes.ToneFlagBytes);
        save.TitleFlg = BitsetCodec.Encode([10, 131], GreenProtocolBytes.TitleFlagBytes);
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new UserDataQuery(1, GameEra.Green), CancellationToken.None);

        Assert.Equal(GreenProtocolBytes.ToneFlagBytes, response.ToneFlg.Length);
        Assert.Equal(GreenProtocolBytes.TitleFlagBytes, response.TitleFlg.Length);
        Assert.True(BitIsSet(response.ToneFlg, 4));
        Assert.True(BitIsSet(response.TitleFlg, 10));
        Assert.True(BitIsSet(response.TitleFlg, 131));
    }

    [Fact]
    public async Task UserData_Green_ReturnsPersistedTojiruAndDisplayLevels()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.IsTojiru = false;
        save.DispLevelTotal = 2;
        save.DispLevelChassis = 3;
        save.DispLevelSelf = 4;
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new UserDataQuery(1, GameEra.Green), CancellationToken.None);

        Assert.False(response.IsTojiru);
        Assert.Equal(2u, response.DispLevelTotal);
        Assert.Equal(3u, response.DispLevelChassis);
        Assert.Equal(4u, response.DispLevelSelf);
    }

    [Fact]
    public async Task UserData_Green_RecommendComesFromCatalog()
    {
        var greenCatalog = new GreenHandlerFixture.TestGreenCatalog
        {
            Recommend = new GreenRecommendEntry
            {
                RecommendSong = 102,
                RecommendBestSongs = [101, 102, 103]
            }
        };
        await using var fixture = await GreenHandlerFixture.CreateAsync(greenCatalog);
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new UserDataQuery(1, GameEra.Green), CancellationToken.None);

        Assert.Equal(102u, response.RecommendSong);
        Assert.Equal(new List<uint> { 101, 102, 103 }, response.RecommendBestSong);
    }

    [Fact]
    public async Task UserData_Green_RecentsOrderedByLastPlayed()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));

        fixture.Context.GreenRecentSongs.AddRange(
            new GreenRecentSongs { Baid = 1, SongNo = 101, LastPlayed = new DateTime(2026, 5, 15, 9, 0, 0) },
            new GreenRecentSongs { Baid = 1, SongNo = 102, LastPlayed = new DateTime(2026, 5, 15, 12, 0, 0) },
            new GreenRecentSongs { Baid = 1, SongNo = 103, LastPlayed = new DateTime(2026, 5, 15, 11, 0, 0) });
        await fixture.Context.SaveChangesAsync();

        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new UserDataQuery(1, GameEra.Green), CancellationToken.None);

        Assert.Equal(new uint[] { 102, 103, 101 }, response.AryRecentSongNoes);
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
