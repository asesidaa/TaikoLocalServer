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
}
