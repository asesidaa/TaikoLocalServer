using TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowIdentityHandlerTests
{
    [Fact]
    public async Task BaidQuery_Yellow_UnknownCardReturnsNewUserWithoutWrites()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new BaidQuery(GameEra.Yellow, "12345678901234567890"), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.True(response.IsNewUser);
        Assert.Equal(1u, response.Baid);
        Assert.Empty(await fixture.Context.Cards.ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataYellow.ToListAsync());
    }

    [Fact]
    public async Task AddMyDonEntry_Yellow_CreatesSharedIdentityAndYellowSaveOnly()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        var handler = new AddMyDonEntryCommandHandler(
            fixture.Context,
            NullLogger<AddMyDonEntryCommandHandler>.Instance);

        var response = await handler.Handle(
            new AddMyDonEntryCommand(GameEra.Yellow, "12345678901234567890", "YELLOW", 0),
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(1u, response.Baid);
        Assert.Equal("YELLOW", response.MydonName);
        Assert.NotNull(await fixture.Context.Cards.FindAsync("12345678901234567890"));
        Assert.NotNull(await fixture.Context.UserData.FindAsync(1u));
        Assert.NotNull(await fixture.Context.Credentials.FindAsync(1u));
        Assert.NotNull(await fixture.Context.UserSaveDataYellow.FindAsync(1u));
        Assert.Empty(await fixture.Context.UserSaveDataBlue.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataNijiiro.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataBlue.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataNijiiro.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task BaidQuery_Yellow_KnownCardWithYellowSaveReturnsProfile()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 7, MyDonName = "DON" });
        fixture.Context.Cards.Add(new Card { Baid = 7, AccessCode = "999" });
        var save = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(7);
        save.Title = "Yellow Title";
        save.TitleplateId = 10;
        save.ToneFlg = Ac15ProtocolBytes.CreateFixedBitset([0, 4], Ac15EraProfiles.Yellow.Limits.ToneFlagBytes);
        fixture.Context.UserSaveDataYellow.Add(save);
        await fixture.Context.SaveChangesAsync();
        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new BaidQuery(GameEra.Yellow, "999"), CancellationToken.None);
        var wire = BaidResponseMapper.Map(response);

        Assert.False(response.IsNewUser);
        Assert.Equal(7u, response.Baid);
        Assert.Equal("DON", response.MyDonName);
        Assert.Equal("Yellow Title", response.Title);
        Assert.Equal(0u, response.TitlePlateId);
        Assert.Equal(Ac15EraProfiles.Yellow.Limits.DanFlagBytes, response.GotDanFlg.Length);
        Assert.Equal(Ac15EraProfiles.Yellow.Limits.DanExtraFlagBytes, response.GotDanExtraFlg!.Length);
        Assert.True(response.IsAutoCostumeOn.GetValueOrDefault());
        Assert.Equal(Ac15EraProfiles.Yellow.Limits.ContentInfoBytes, wire.ContentInfo.Length);
        Assert.Equal(Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes, wire.CostumeFlg1.Length);
    }

    [Fact]
    public async Task BaidQuery_Yellow_SharedIdentityWithoutYellowSaveIsNewForYellowRegistration()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 8, MyDonName = "BLUE" });
        fixture.Context.Cards.Add(new Card { Baid = 8, AccessCode = "888" });
        fixture.Context.Credentials.Add(new Credential { Baid = 8, Password = string.Empty, Salt = string.Empty });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(8));
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(8));
        await fixture.Context.SaveChangesAsync();
        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new BaidQuery(GameEra.Yellow, "888"), CancellationToken.None);

        Assert.True(response.IsNewUser);
        Assert.Equal(8u, response.Baid);
        Assert.Null(await fixture.Context.UserSaveDataYellow.FindAsync(8u));
    }

    [Fact]
    public async Task AddMyDonEntry_Yellow_CompletesExistingSharedIdentityRegistration()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 8, MyDonName = "BLUE" });
        fixture.Context.Cards.Add(new Card { Baid = 8, AccessCode = "888" });
        fixture.Context.Credentials.Add(new Credential { Baid = 8, Password = string.Empty, Salt = string.Empty });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(8));
        await fixture.Context.SaveChangesAsync();
        var handler = new AddMyDonEntryCommandHandler(
            fixture.Context,
            NullLogger<AddMyDonEntryCommandHandler>.Instance);

        var response = await handler.Handle(
            new AddMyDonEntryCommand(GameEra.Yellow, "888", "YELLOW", 0),
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(8u, response.Baid);
        Assert.NotNull(await fixture.Context.UserSaveDataYellow.FindAsync(8u));
        Assert.Single(await fixture.Context.Cards.Where(card => card.AccessCode == "888").ToListAsync());
        Assert.Single(await fixture.Context.UserData.Where(user => user.Baid == 8).ToListAsync());
        Assert.Single(await fixture.Context.Credentials.Where(credential => credential.Baid == 8).ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataYellow.Where(row => row.Baid == 8).ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataBlue.Where(row => row.Baid == 8).ToListAsync());
    }
}
