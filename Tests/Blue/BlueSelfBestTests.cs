using TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueSelfBestTests
{
    [Fact]
    public async Task SelfBest_Blue_PreservesRequestedOrderAndReturnsZeroForMissingRows()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.SongBestDataBlue.Add(new SongBestDatumBlue
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
            new GetSelfBestQuery(1, GameEra.Blue, 1, [103, 102, 101]),
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal([103u, 102u, 101u], response.ArySelfbestScores.Select(row => row.SongNo));
        Assert.Equal(0u, response.ArySelfbestScores[0].SelfBestScore);
        Assert.Equal(222222u, response.ArySelfbestScores[1].SelfBestScore);
        Assert.Equal(0u, response.ArySelfbestScores[2].SelfBestScore);
    }

    [Fact]
    public async Task SelfBest_Blue_ReturnsNormalAndShinRows()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.SongBestDataBlue.AddRange(
            new SongBestDatumBlue
            {
                Baid = 1,
                SongId = 101,
                Difficulty = Difficulty.Normal,
                IsShin = false,
                BestScore = 111111,
                BestRate = 77,
                BestCrown = CrownType.Clear
            },
            new SongBestDatumBlue
            {
                Baid = 1,
                SongId = 101,
                Difficulty = Difficulty.Normal,
                IsShin = true,
                BestScore = 333333,
                BestRate = 99,
                BestCrown = CrownType.Dondaful
            });
        await fixture.Context.SaveChangesAsync();
        var handler = new GetSelfBestQueryHandler(
            fixture.Catalog,
            fixture.Context,
            NullLogger<GetSelfBestQueryHandler>.Instance);

        var response = await handler.Handle(
            new GetSelfBestQuery(1, GameEra.Blue, 2, [101]),
            CancellationToken.None);

        Assert.Equal(111111u, Assert.Single(response.ArySelfbestScores).SelfBestScore);
        Assert.Equal(333333u, Assert.Single(response.AryShinSelfbestScores).SelfBestScore);
    }

    [Fact]
    public void SelfBestMapper_Blue_MapsCommonResponse()
    {
        var response = SelfBestMappers.Map(new CommonSelfBestResponse
        {
            Result = 1,
            Level = 1,
            ArySelfbestScores =
            [
                new CommonSelfBestResponse.SelfBestData { SongNo = 101, SelfBestScore = 123 }
            ],
            AryShinSelfbestScores =
            [
                new CommonSelfBestResponse.SelfBestData { SongNo = 101, SelfBestScore = 456 }
            ]
        });

        Assert.Equal(1u, response.Result);
        Assert.Equal(1u, response.Level);
        Assert.Equal(123u, Assert.Single(response.ArySelfbestScores).SelfBestScore);
        Assert.Equal(456u, Assert.Single(response.AryShinSelfbestScores).SelfBestScore);
    }
}
