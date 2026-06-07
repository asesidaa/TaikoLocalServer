using TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowSelfBestTests
{
    [Fact]
    public async Task SelfBest_Yellow_PreservesRequestedOrderAndReturnsZeroForMissingRows()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.SongBestDataYellow.Add(new SongBestDatumYellow
        {
            Baid = 1,
            SongId = 102,
            Difficulty = Difficulty.Easy,
            IsShin = false,
            BestScore = 222222,
            BestRate = 88,
            BestCrown = CrownType.Gold
        });
        await fixture.Context.SaveChangesAsync();
        var handler = new GetSelfBestQueryHandler(
            fixture.Catalog,
            fixture.Context,
            NullLogger<GetSelfBestQueryHandler>.Instance);

        var response = await handler.Handle(
            new GetSelfBestQuery(1, GameEra.Yellow, 1, [103, 102, 101]),
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal([103u, 102u, 101u], response.ArySelfbestScores.Select(row => row.SongNo));
        Assert.Equal(0u, response.ArySelfbestScores[0].SelfBestScore);
        Assert.Equal(222222u, response.ArySelfbestScores[1].SelfBestScore);
        Assert.Equal(0u, response.ArySelfbestScores[2].SelfBestScore);
    }

    [Fact]
    public async Task SelfBest_Yellow_ReturnsNormalAndShinRowsFromYellowBestTableOnly()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.SongBestDataYellow.AddRange(
            new SongBestDatumYellow
            {
                Baid = 1,
                SongId = 101,
                Difficulty = Difficulty.Normal,
                IsShin = false,
                BestScore = 111111,
                BestRate = 77,
                BestCrown = CrownType.Clear
            },
            new SongBestDatumYellow
            {
                Baid = 1,
                SongId = 101,
                Difficulty = Difficulty.Normal,
                IsShin = true,
                BestScore = 333333,
                BestRate = 99,
                BestCrown = CrownType.Dondaful
            });
        fixture.Context.SongBestDataBlue.Add(new SongBestDatumBlue
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Normal,
            IsShin = false,
            BestScore = 999999,
            BestRate = 100,
            BestCrown = CrownType.Dondaful
        });
        await fixture.Context.SaveChangesAsync();
        var handler = new GetSelfBestQueryHandler(
            fixture.Catalog,
            fixture.Context,
            NullLogger<GetSelfBestQueryHandler>.Instance);

        var response = await handler.Handle(
            new GetSelfBestQuery(1, GameEra.Yellow, 2, [101]),
            CancellationToken.None);

        Assert.Equal(111111u, Assert.Single(response.ArySelfbestScores).SelfBestScore);
        Assert.Equal(333333u, Assert.Single(response.AryShinSelfbestScores).SelfBestScore);
    }

    [Fact]
    public void SelfBestMapper_Yellow_MapsCommonRowsIntoYellowWirePlacement()
    {
        var response = SelfBestMappers.Map(new CommonSelfBestResponse
        {
            Result = 1,
            Level = 2,
            ArySelfbestScores =
            [
                new CommonSelfBestResponse.SelfBestData
                {
                    SongNo = 101,
                    SelfBestScore = 123,
                    UraBestScore = 456
                }
            ],
            AryShinSelfbestScores =
            [
                new CommonSelfBestResponse.SelfBestData
                {
                    SongNo = 101,
                    SelfBestScore = 789,
                    UraBestScore = 987
                }
            ]
        });

        Assert.Equal(1u, response.Result);
        Assert.Equal(2u, response.Level);
        var normal = Assert.Single(response.ArySelfbestScores);
        Assert.Equal(101u, normal.SongNo);
        Assert.Equal(123u, normal.SelfBestScore);
        Assert.Equal(456u, normal.UraBestScore);
        var shin = Assert.Single(response.AryShinSelfbestScores);
        Assert.Equal(101u, shin.SongNo);
        Assert.Equal(789u, shin.SelfBestScore);
        Assert.Equal(987u, shin.UraBestScore);
    }
}
