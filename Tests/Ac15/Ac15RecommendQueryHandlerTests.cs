using TaikoLocalServer.Tests.Blue;
using TaikoLocalServer.Tests.Green;
using TaikoLocalServer.Tests.Red;
using TaikoLocalServer.Tests.White;
using TaikoLocalServer.Tests.Yellow;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15RecommendQueryHandlerTests
{
    [Theory]
    [InlineData(GameEra.Blue, 201u)]
    [InlineData(GameEra.Green, 202u)]
    [InlineData(GameEra.Yellow, 203u)]
    [InlineData(GameEra.Red, 204u)]
    [InlineData(GameEra.White, 205u)]
    public async Task Handle_ReturnsRandomCatalogSongAndLeavesBestSongUnset(GameEra era, uint expectedSongNo)
    {
        var catalog = new FileGameDataCatalog(
        [
            new BlueHandlerFixture.TestBlueCatalog(musicInfoFileOrder: [Song(201)]),
            new GreenHandlerFixture.TestGreenCatalog(musicInfoFileOrder: [Song(202)]),
            new YellowHandlerFixture.TestYellowCatalog(musicInfoFileOrder: [Song(203)]),
            new RedHandlerFixture.TestRedCatalog(musicInfoFileOrder: [Song(204)]),
            new WhiteHandlerFixture.TestWhiteCatalog(musicInfoFileOrder: [Song(205)])
        ]);
        var handler = new GetRecommendQueryHandler(
            NullLogger<GetRecommendQueryHandler>.Instance,
            catalog);

        var response = await handler.Handle(
            new GetRecommendQuery(era, GenderType: 0, PlayerAge: 0),
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(expectedSongNo, response.RecommendSong);
        Assert.Empty(response.RecommendBestSong);
    }

    private static Ac15MusicInfoEntry Song(uint songNo)
        => new() { SongNo = songNo, MusicId = $"song{songNo}", FileOrder = 0 };
}
