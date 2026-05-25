namespace TaikoLocalServer.Tests.Green;

public sealed class GreenItemShopStateTests
{
    [Fact]
    public async Task GetOrCreateGreenShopSeasonState_FirstSeasonSeedsFromGlobalMedals()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.TotalGetDonmedal = 100;
        save.TotalUseDonmedal = 40;
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var state = await fixture.Context.GetOrCreateGreenShopSeasonStateAsync(save, 2, CancellationToken.None);
        await fixture.Context.SaveChangesAsync();

        Assert.Equal(2u, state.SeasonId);
        Assert.Equal(100u, state.TotalGetDonmedal);
        Assert.Equal(40u, state.TotalUseDonmedal);
    }

    [Fact]
    public async Task GetOrCreateGreenShopSeasonState_LaterSeasonStartsAtZero()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.TotalGetDonmedal = 100;
        save.TotalUseDonmedal = 40;
        fixture.Context.UserSaveDataGreen.Add(save);
        fixture.Context.GreenShopSeasonStates.Add(new GreenShopSeasonState
        {
            Baid = 1,
            SeasonId = 1,
            TotalGetDonmedal = 100,
            TotalUseDonmedal = 40,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await fixture.Context.SaveChangesAsync();

        var state = await fixture.Context.GetOrCreateGreenShopSeasonStateAsync(save, 2, CancellationToken.None);
        await fixture.Context.SaveChangesAsync();

        Assert.Equal(0u, state.TotalGetDonmedal);
        Assert.Equal(0u, state.TotalUseDonmedal);
    }

    [Fact]
    public async Task UpdatePlayResult_WhenShopEnabled_AddsDonMedalsToActiveSeasonState()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateSingleSongShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance,
            Options.Create(new ServerSettings()));

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                GetDonmedal = 25,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 102,
                        Level = 1,
                        StageMode = 0,
                        PlayResult = 1,
                        PlayScore = 1000
                    }
                ]
            }),
            CancellationToken.None);

        var state = await fixture.Context.GreenShopSeasonStates.FindAsync(1u, 2u);
        var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
        Assert.Equal(1u, result);
        Assert.Equal(25u, state!.TotalGetDonmedal);
        Assert.Equal(0u, save!.TotalGetDonmedal);
    }

    private static GreenHandlerFixture.TestGreenCatalog CreateSingleSongShopCatalog()
    {
        var season = new GreenItemShopSeason
        {
            SeasonId = 2,
            StartDatetime = "20190314000000",
            EndDatetime = "20190626075959",
            Items = [new GreenItemShopEntry { ItemNo = 1, ItemType = 1, ItemId = 101, Price = 1300 }]
        };

        return new GreenHandlerFixture.TestGreenCatalog(itemShopCatalog: new GreenItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, GreenItemShopSeason> { [2] = season }
        });
    }
}
