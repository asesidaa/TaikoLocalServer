namespace TaikoLocalServer.Tests.Green;

public sealed class GreenGhostRewardTests
{
    [Fact]
    public async Task GetGhostData_ReturnsSavedGreenGhostState()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.GhostInputMedian = -12;
        save.GhostInputVariance = 34;
        save.GhostRankId = 5;
        save.GhostWinPoint = 6;
        save.GhostCertifiedLevelId = 7;
        save.GhostTotalWinnings = 8;
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(save);
        fixture.Context.GreenGhostTokens.Add(new GreenGhostTokens { Baid = 1, TokenId = 9, TokenValue = 10 });
        fixture.Context.GreenGhostWinnings.Add(new GreenGhostWinnings { Baid = 1, LevelId = 11, Winnings = 12 });
        await fixture.Context.SaveChangesAsync();

        var handler = new GetGhostDataQueryHandler(
            fixture.Context,
            NullLogger<GetGhostDataQueryHandler>.Instance);

        var response = await handler.Handle(new GetGhostDataQuery(1), CancellationToken.None);

        Assert.Equal(GreenProtocolBytes.GhostReleaseInfoBytes, response.ReleaseInfoFlag.Length);
        Assert.Equal(GreenProtocolBytes.GhostPlayedSongBytes, response.PlayedSongFlag.Length);
        Assert.Equal((uint)8, response.TotalWinnings);
        Assert.Single(response.AryTokenData);
        Assert.Single(response.GhostRecordData.AryWinningsData);
    }

    [Fact]
    public async Task GetGhostScore_ReturnsSavedSections()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var play = new SongPlayDatumGreen
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Easy,
            PlayTime = DateTime.UtcNow
        };
        fixture.Context.SongPlayDataGreen.Add(play);
        await fixture.Context.SaveChangesAsync();
        fixture.Context.GhostStageSectionDataGreen.Add(new GhostStageSectionDatumGreen
        {
            PlayId = play.Id,
            SectionNo = 1,
            GoodCount = 10,
            OkCount = 2,
            NgCount = 1,
            PoundCount = 3
        });
        await fixture.Context.SaveChangesAsync();

        var handler = new GetGhostScoreQueryHandler(
            fixture.Context,
            NullLogger<GetGhostScoreQueryHandler>.Instance);

        var response = await handler.Handle(new GetGhostScoreQuery(1, 101, 1), CancellationToken.None);

        Assert.Single(response.AryBestSectionData);
    }

    [Fact]
    public async Task RewardCardCheck_ReturnsKnownCardBaid()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 42, MyDonName = "DON" });
        fixture.Context.Cards.Add(new Card { Baid = 42, AccessCode = "abc" });
        await fixture.Context.SaveChangesAsync();

        var handler = new RewardCardCheckQueryHandler(
            fixture.Context,
            NullLogger<RewardCardCheckQueryHandler>.Instance);

        var response = await handler.Handle(new RewardCardCheckQuery("abc"), CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.Equal((uint)42, response.Baid);
    }

    [Fact]
    public async Task ItemPurchase_SpendsDonmedalsWhenAffordable()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreatePurchaseShopCatalog());
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.TotalGetDonmedal = 100;
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new ItemPurchaseCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<ItemPurchaseCommandHandler>.Instance);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Green, 10, 1, 2, 40), CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.Equal((uint)100, response.TotalGetDonmedal);
        Assert.Equal((uint)40, response.TotalUseDonmedal);
    }

    [Fact]
    public async Task ItemPurchase_RejectsUnknownGreenShopItem()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.TotalGetDonmedal = 100;
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new ItemPurchaseCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<ItemPurchaseCommandHandler>.Instance);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Green, 10, 1, 2, 40), CancellationToken.None);

        Assert.Equal((uint)0, response.Result);
        Assert.Equal((uint)0, response.TotalUseDonmedal);
    }

    [Fact]
    public async Task ItemPurchase_RejectsOverflowingGreenMedalBalance()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreatePurchaseShopCatalog());
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.TotalGetDonmedal = uint.MaxValue;
        save.TotalUseDonmedal = uint.MaxValue - 10;
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new ItemPurchaseCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<ItemPurchaseCommandHandler>.Instance);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Green, 10, 1, 2, 40), CancellationToken.None);

        Assert.Equal((uint)0, response.Result);
        Assert.Equal(uint.MaxValue - 10, response.TotalUseDonmedal);
    }

    private static GreenHandlerFixture.TestGreenCatalog CreatePurchaseShopCatalog()
    {
        var season = new GreenItemShopSeason
        {
            SeasonId = 2,
            StartDatetime = "20190314000000",
            EndDatetime = "20190626075959",
            Items = [new GreenItemShopEntry { ItemNo = 10, ItemType = 1, ItemId = 2, Price = 40 }]
        };

        return new GreenHandlerFixture.TestGreenCatalog(itemShopCatalog: new GreenItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, GreenItemShopSeason> { [2] = season }
        });
    }
}
