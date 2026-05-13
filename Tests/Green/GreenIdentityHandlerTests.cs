using TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

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

        var response = await handler.Handle(new BaidQuery(GameEra.Green, "999"), CancellationToken.None);

        Assert.False(response.IsNewUser);
        Assert.Equal((uint)7, response.Baid);
        Assert.Equal("DON", response.MyDonName);
        Assert.Equal(GreenProtocolBytes.DanFlagBytes, response.GotDanFlg.Length);
        Assert.Equal(0, response.GotDanFlg[0]);
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

        var response = await handler.Handle(new BaidQuery(GameEra.Green, "999"), CancellationToken.None);

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

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
