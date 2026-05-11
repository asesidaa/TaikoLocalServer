namespace TaikoLocalServer.Tests.Green;

public sealed class GreenIdentityHandlerTests
{
    [Fact]
    public async Task AddMyDonEntry_Green_CreatesIdentitySaveAndSeeds()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new AddMyDonEntryCommandHandler(
            fixture.Context,
            NullLogger<AddMyDonEntryCommandHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(
            new AddMyDonEntryCommand(GameEra.Green, "12345678901234567890", "DON", 0),
            CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.Equal((uint)1, response.Baid);
        Assert.NotNull(await fixture.Context.Cards.FindAsync("12345678901234567890"));
        Assert.NotNull(await fixture.Context.UserSaveDataGreen.FindAsync(1u));
        Assert.NotEmpty(await fixture.Context.SongBestDataGreen.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task BaidQuery_Green_KnownCardReturnsSaveDataAndGrantsFirstDan()
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
        Assert.Equal(0b0000_0001, response.GotDanFlg[0]);
    }

    [Fact]
    public async Task InitialData_Green_UnlocksFirstTenSongs()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Green), CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.Equal(128, response.DefaultSongFlg.Length);
        Assert.True((response.DefaultSongFlg[101 >> 3] & (1 << (101 & 7))) != 0);
        Assert.True((response.DefaultSongFlg[110 >> 3] & (1 << (110 & 7))) != 0);
        Assert.False((response.DefaultSongFlg[111 >> 3] & (1 << (111 & 7))) != 0);
    }

    [Fact]
    public async Task UserData_Green_UnlocksFirstTwentySongs()
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
        Assert.True((response.ReleaseSongFlg[101 >> 3] & (1 << (101 & 7))) != 0);
        Assert.True((response.ReleaseSongFlg[120 >> 3] & (1 << (120 & 7))) != 0);
    }
}
