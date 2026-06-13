using TaikoLocalServer.Adapters.GameProtocol.Red.Mappers;

namespace TaikoLocalServer.Tests.Red;

public sealed class RedIdentityHandlerTests
{
    [Fact]
    public async Task BaidQuery_Red_UnknownCardReturnsNewUserWithoutWrites()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new Ac15BaidQuery(GameEra.Red, "12345678901234567890"), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.True(response.IsNewUser);
        Assert.Equal(1u, response.Baid);
        Assert.Empty(await fixture.Context.Cards.ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataRed.ToListAsync());
    }

    [Fact]
    public async Task AddMyDonEntry_Red_CreatesSharedIdentityAndRedSaveOnly()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        var handler = new AddMyDonEntryCommandHandler(
            fixture.Context,
            NullLogger<AddMyDonEntryCommandHandler>.Instance);

        var response = await handler.Handle(
            new AddMyDonEntryCommand(GameEra.Red, "12345678901234567890", "RED", 0),
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(1u, response.Baid);
        Assert.Equal("RED", response.MydonName);
        Assert.NotNull(await fixture.Context.Cards.FindAsync("12345678901234567890"));
        Assert.NotNull(await fixture.Context.UserData.FindAsync(1u));
        Assert.NotNull(await fixture.Context.Credentials.FindAsync(1u));
        Assert.NotNull(await fixture.Context.UserSaveDataRed.FindAsync(1u));
        Assert.Empty(await fixture.Context.UserSaveDataBlue.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataYellow.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataNijiiro.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task BaidQuery_Red_KnownCardWithRedSaveReturnsRedProfile()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 7, MyDonName = "DON" });
        fixture.Context.Cards.Add(new Card { Baid = 7, AccessCode = "999" });
        var save = UserSaveDataRedExtensions.CreateDefaultRedSaveData(7);
        save.Title = "Red Title";
        save.TitleplateId = 10;
        save.RewardPtn = 4;
        fixture.Context.UserSaveDataRed.Add(save);
        await fixture.Context.SaveChangesAsync();
        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new Ac15BaidQuery(GameEra.Red, "999"), CancellationToken.None);
        var wire = BaidResponseMapper.Map(response);

        Assert.False(response.IsNewUser);
        Assert.Equal(7u, response.Baid);
        Assert.Equal("DON", response.Identity!.MyDonName);
        Assert.Equal("Red Title", response.Profile!.Title);
        Assert.Equal(0u, response.Profile.TitlePlateId);
        Assert.Equal(4u, response.Reward!.RewardPtn);
        Assert.Equal(Ac15EraProfiles.Red.Limits.DanFlagBytes, response.Dan!.GotDanFlg.Length);
        Assert.Equal(Ac15EraProfiles.Red.Limits.DanExtraFlagBytes, response.Dan.GotDanExtraFlg.Length);
        Assert.Equal(Ac15EraProfiles.Red.Limits.ContentInfoBytes, wire.ContentInfo.Length);
        Assert.Equal(Ac15EraProfiles.Red.Limits.CostumeFlagBytes, wire.CostumeFlg1.Length);
        Assert.Equal(4u, wire.RewardPtn);
    }

    [Fact]
    public async Task UserDataQuery_Red_ReadsRedDonPointsAndTokkunTutorial()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 3, MyDonName = "DON" });
        var save = UserSaveDataRedExtensions.CreateDefaultRedSaveData(3);
        save.TotalGetDonpoint = 120;
        save.TotalUseDonpoint = 30;
        save.RewardProgress = 8;
        save.DifficultyTutorialFlg = 2;
        save.TokkunTutorialFlg = 7;
        fixture.Context.UserSaveDataRed.Add(save);
        fixture.Context.RedFavoriteSongs.Add(new RedFavoriteSongs { Baid = 3, SongNo = 101 });
        fixture.Context.RedRecentSongs.Add(new RedRecentSongs { Baid = 3, SongNo = 102, LastPlayed = DateTime.UtcNow });
        await fixture.Context.SaveChangesAsync();
        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new Ac15UserDataQuery(3, GameEra.Red), CancellationToken.None);
        var wire = UserDataMappers.Map(response);

        Assert.Equal(1u, response.Result);
        Assert.NotNull(response.Reward);
        Assert.Equal(120u, response.Reward.TotalGetDonpoint);
        Assert.Equal(30u, response.Reward.TotalUseDonpoint);
        Assert.Equal(8u, response.Reward.RewardProgress);
        Assert.NotNull(response.Tutorial);
        Assert.Equal(2u, response.Tutorial.DifficultyTutorialFlg);
        Assert.Equal(7u, response.Tutorial.TokkunTutorialFlg);
        Assert.Equal([101u], response.SongLists.AryFavoriteSongNoes);
        Assert.Equal([102u], response.SongLists.AryRecentSongNoes);
        Assert.Equal(120u, wire.TotalGetDonpoint);
        Assert.Equal(30u, wire.TotalUseDonpoint);
        Assert.Equal(8u, wire.RewardProgress);
        Assert.Equal(2u, wire.DifficultyTutorialFlg);
        Assert.Equal(7u, wire.TokkunTutorialFlg);
    }
}
