namespace TaikoLocalServer.Tests.Momoiro;

public sealed class MomoiroReadbackHandlerTests
{
    [Fact]
    public async Task AddMyDonEntry_Momoiro_CreatesSharedIdentityAndMomoiroSaveOnly()
    {
        await using var fixture = await MomoiroHandlerFixture.CreateAsync();
        var handler = new AddMyDonEntryCommandHandler(
            fixture.Context,
            NullLogger<AddMyDonEntryCommandHandler>.Instance);

        var response = await handler.Handle(
            new AddMyDonEntryCommand(GameEra.Momoiro, "12345678901234567890", "MOMO", 0),
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(1u, response.Baid);
        Assert.Equal("MOMO", response.MydonName);
        Assert.NotNull(await fixture.Context.Cards.FindAsync("12345678901234567890"));
        Assert.NotNull(await fixture.Context.UserData.FindAsync(1u));
        Assert.NotNull(await fixture.Context.Credentials.FindAsync(1u));
        Assert.True(await fixture.MomoiroSaveExistsAsync(1));
        Assert.Empty(await fixture.Context.UserSaveDataBlue.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataYellow.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataRed.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataWhite.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataMurasaki.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataKimidori.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataNijiiro.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task BaidQuery_Momoiro_SharedCardWithoutMomoiroSaveIsNewUserAndIgnoresAdjacentSave()
    {
        await using var fixture = await MomoiroHandlerFixture.CreateAsync();
        await fixture.SeedSharedIdentityAsync(10, "11112222333344445555", "SHARED");
        fixture.Context.UserSaveDataKimidori.Add(UserSaveDataKimidoriExtensions.CreateDefaultKimidoriSaveData(10));
        await fixture.Context.SaveChangesAsync();
        var handler = new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(
            new Ac15BaidQuery(GameEra.Momoiro, "11112222333344445555"),
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.True(response.IsNewUser);
        Assert.Equal(10u, response.Baid);
        Assert.Null(response.Identity);
        Assert.False(await fixture.MomoiroSaveExistsAsync(10));
    }

    [Fact]
    public async Task GetSelfBestQuery_Momoiro_ReadsMomoiroNormalUraAndShinRowsOnlyInRequestOrder()
    {
        await using var fixture = await MomoiroHandlerFixture.CreateAsync();
        await fixture.SeedSharedIdentityAsync(3, "33333333333333333333");
        await fixture.SeedMomoiroSaveAsync(3);
        await fixture.SeedMomoiroBestAsync(
            3,
            MomoiroHandlerFixture.HighSongNo,
            Difficulty.Oni,
            false,
            900_000,
            91,
            CrownType.Gold);
        await fixture.SeedMomoiroBestAsync(
            3,
            MomoiroHandlerFixture.HighSongNo,
            Difficulty.UraOni,
            false,
            910_000,
            92,
            CrownType.Clear);
        await fixture.SeedMomoiroBestAsync(3, 101, Difficulty.Oni, false, 101_000, 70, CrownType.Clear);
        await fixture.SeedMomoiroBestAsync(
            3,
            MomoiroHandlerFixture.HighSongNo,
            Difficulty.Oni,
            true,
            700_000,
            81,
            CrownType.Gold);
        await fixture.SeedMomoiroBestAsync(
            3,
            MomoiroHandlerFixture.HighSongNo,
            Difficulty.UraOni,
            true,
            710_000,
            82,
            CrownType.Dondaful);
        fixture.Context.SongBestDataKimidori.Add(new SongBestDatumKimidori
        {
            Baid = 3,
            SongId = MomoiroHandlerFixture.HighSongNo,
            Difficulty = Difficulty.Oni,
            BestScore = 999_999,
            BestRate = 99,
            BestCrown = CrownType.Dondaful
        });
        await fixture.Context.SaveChangesAsync();
        var handler = new GetSelfBestQueryHandler(
            fixture.Catalog,
            fixture.Context,
            NullLogger<GetSelfBestQueryHandler>.Instance);

        var response = await handler.Handle(
            new GetSelfBestQuery(3, GameEra.Momoiro, 4, [MomoiroHandlerFixture.HighSongNo, 101, 102]),
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(4u, response.Level);
        Assert.Equal(
            [MomoiroHandlerFixture.HighSongNo, 101u, 102u],
            response.ArySelfbestScores.Select(row => row.SongNo).ToArray());
        Assert.Equal(900_000u, response.ArySelfbestScores[0].SelfBestScore);
        Assert.Equal(910_000u, response.ArySelfbestScores[0].UraBestScore);
        Assert.Equal(101_000u, response.ArySelfbestScores[1].SelfBestScore);
        Assert.Equal(0u, response.ArySelfbestScores[2].SelfBestScore);
        var shin = Assert.Single(response.AryShinSelfbestScores);
        Assert.Equal(MomoiroHandlerFixture.HighSongNo, shin.SongNo);
        Assert.Equal(700_000u, shin.SelfBestScore);
        Assert.Equal(710_000u, shin.UraBestScore);
    }

    [Fact]
    public async Task UserDataQuery_Momoiro_ReadsMomoiroListsHashReleaseAndIgnoresAdjacentRows()
    {
        await using var fixture = await MomoiroHandlerFixture.CreateAsync();
        await fixture.SeedSharedIdentityAsync(4, "44444444444444444444");
        await fixture.SeedMomoiroSaveAsync(
            4,
            releaseSongNoes: [MomoiroHandlerFixture.HighSongNo],
            isDevil: true,
            isExplain: true,
            rewardProgress: 7,
            totalGetDonpoint: 120,
            totalUseDonpoint: 30,
            dispLevelSelf: 3);
        await fixture.SeedMomoiroFavoriteAsync(4, 300, 0);
        await fixture.SeedMomoiroFavoriteAsync(4, 101, 1);
        await fixture.SeedMomoiroFavoriteAsync(4, 250, 2);
        await fixture.SeedMomoiroFavoriteAsync(4, 120, 3);
        await fixture.SeedMomoiroFavoriteAsync(4, 110, 4);
        await fixture.SeedMomoiroFavoriteAsync(4, 105, 5);
        await fixture.SeedMomoiroRecentAsync(4, 101, new DateTime(2026, 6, 26, 10, 0, 0));
        await fixture.SeedMomoiroRecentAsync(4, 102, new DateTime(2026, 6, 26, 11, 0, 0));
        await fixture.SeedMomoiroRecentAsync(4, 103, new DateTime(2026, 6, 26, 12, 0, 0));
        await fixture.SeedMomoiroRecentAsync(4, 104, new DateTime(2026, 6, 26, 13, 0, 0));
        await fixture.SeedMomoiroRecentAsync(4, 105, new DateTime(2026, 6, 26, 14, 0, 0));
        await fixture.SeedMomoiroRecentAsync(4, 106, new DateTime(2026, 6, 26, 15, 0, 0));
        fixture.Context.KimidoriFavoriteSongs.Add(new KimidoriFavoriteSongs { Baid = 4, SongNo = 999 });
        fixture.Context.KimidoriRecentSongs.Add(new KimidoriRecentSongs
        {
            Baid = 4,
            SongNo = 998,
            LastPlayed = new DateTime(2026, 6, 26, 16, 0, 0)
        });
        await fixture.Context.SaveChangesAsync();
        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new Ac15UserDataQuery(4, GameEra.Momoiro), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(538_116_869u, response.SongFlags.SongHashVer);
        Assert.True(BitIsSet(response.SongFlags.ReleaseSongFlg, MomoiroHandlerFixture.HighSongNo));
        Assert.Equal([300u, 101u, 250u, 120u, 110u], response.SongLists.AryFavoriteSongNoes);
        Assert.Equal([106u, 105u, 104u, 103u, 102u], response.SongLists.AryRecentSongNoes);
        Assert.True(response.ModeFlags!.IsDevil);
        Assert.True(response.ModeFlags.IsExplain);
        Assert.Equal(3u, response.Display.DispLevelSelf);
        Assert.Equal(120u, response.Reward!.TotalGetDonpoint);
        Assert.Equal(30u, response.Reward.TotalUseDonpoint);
        Assert.Equal(7u, response.Reward.RewardProgress);
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
