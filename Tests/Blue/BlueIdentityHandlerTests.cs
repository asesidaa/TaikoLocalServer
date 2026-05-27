namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueIdentityHandlerTests
{
    [Fact]
    public async Task BaidQuery_Blue_UnknownCardReturnsNewUserWithoutWrites()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new BaidQuery(GameEra.Blue, "12345678901234567890"), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.True(response.IsNewUser);
        Assert.Equal(1u, response.Baid);
        Assert.Empty(await fixture.Context.Cards.ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataBlue.ToListAsync());
    }

    [Fact]
    public async Task AddMyDonEntry_Blue_CreatesSharedIdentityAndBlueSaveOnly()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var handler = new AddMyDonEntryCommandHandler(
            fixture.Context,
            NullLogger<AddMyDonEntryCommandHandler>.Instance);

        var response = await handler.Handle(
            new AddMyDonEntryCommand(GameEra.Blue, "12345678901234567890", "BLUE", 0),
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(1u, response.Baid);
        Assert.Equal("BLUE", response.MydonName);
        Assert.NotNull(await fixture.Context.Cards.FindAsync("12345678901234567890"));
        Assert.NotNull(await fixture.Context.UserData.FindAsync(1u));
        Assert.NotNull(await fixture.Context.Credentials.FindAsync(1u));
        Assert.NotNull(await fixture.Context.UserSaveDataBlue.FindAsync(1u));
        Assert.Empty(await fixture.Context.SongBestDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.DanScoreDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.GreenShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.GreenFavoriteSongs.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.GreenRecentSongs.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task BaidQuery_Blue_KnownCardWithBlueSaveReturnsProfile()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 7, MyDonName = "DON" });
        fixture.Context.Cards.Add(new Card { Baid = 7, AccessCode = "999" });
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(7);
        save.Title = "Blue Title";
        save.TitleplateId = 10;
        save.ToneFlg = BlueProtocolBytes.CreateFixedBitset([0, 4], BlueProtocolBytes.ToneFlagBytes);
        fixture.Context.UserSaveDataBlue.Add(save);
        await fixture.Context.SaveChangesAsync();
        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new BaidQuery(GameEra.Blue, "999"), CancellationToken.None);

        Assert.False(response.IsNewUser);
        Assert.Equal(7u, response.Baid);
        Assert.Equal("DON", response.MyDonName);
        Assert.Equal("Blue Title", response.Title);
        Assert.Equal(0u, response.TitlePlateId);
        Assert.Equal(BlueProtocolBytes.DanFlagBytes, response.GotDanFlg.Length);
        Assert.Equal(BlueProtocolBytes.DanExtraFlagBytes, response.GotDanExtraFlg!.Length);
        Assert.True(response.IsAutoCostumeOn.GetValueOrDefault());
    }

    [Fact]
    public async Task BaidQuery_Blue_SharedIdentityWithoutBlueSaveIsNewForBlueRegistration()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 8, MyDonName = "GREEN" });
        fixture.Context.Cards.Add(new Card { Baid = 8, AccessCode = "888" });
        fixture.Context.Credentials.Add(new Credential { Baid = 8, Password = string.Empty, Salt = string.Empty });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(8));
        await fixture.Context.SaveChangesAsync();
        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new BaidQuery(GameEra.Blue, "888"), CancellationToken.None);

        Assert.True(response.IsNewUser);
        Assert.Equal(8u, response.Baid);
        Assert.Null(await fixture.Context.UserSaveDataBlue.FindAsync(8u));
    }

    [Fact]
    public async Task AddMyDonEntry_Blue_CompletesExistingSharedIdentityRegistration()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 8, MyDonName = "GREEN" });
        fixture.Context.Cards.Add(new Card { Baid = 8, AccessCode = "888" });
        fixture.Context.Credentials.Add(new Credential { Baid = 8, Password = string.Empty, Salt = string.Empty });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(8));
        await fixture.Context.SaveChangesAsync();
        var handler = new AddMyDonEntryCommandHandler(
            fixture.Context,
            NullLogger<AddMyDonEntryCommandHandler>.Instance);

        var response = await handler.Handle(
            new AddMyDonEntryCommand(GameEra.Blue, "888", "BLUE", 0),
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(8u, response.Baid);
        Assert.NotNull(await fixture.Context.UserSaveDataBlue.FindAsync(8u));
        Assert.Single(await fixture.Context.Cards.Where(card => card.AccessCode == "888").ToListAsync());
        Assert.Single(await fixture.Context.UserData.Where(user => user.Baid == 8).ToListAsync());
        Assert.Single(await fixture.Context.Credentials.Where(credential => credential.Baid == 8).ToListAsync());
    }
}
