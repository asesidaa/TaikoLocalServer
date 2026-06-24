using TaikoLocalServer.Tests.Blue;
using TaikoLocalServer.Tests.Green;
using TaikoLocalServer.Tests.Kimidori;
using TaikoLocalServer.Tests.Murasaki;
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
    [InlineData(GameEra.Murasaki, 206u)]
    [InlineData(GameEra.Kimidori, 207u)]
    public async Task Handle_ReturnsRandomCatalogSongAndLeavesBestSongUnset(GameEra era, uint expectedSongNo)
    {
        var catalog = Catalog(
            blueSongs: [201],
            greenSongs: [202],
            yellowSongs: [203],
            redSongs: [204],
            whiteSongs: [205],
            murasakiSongs: [206],
            kimidoriSongs: [207]);
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

    [Theory]
    [InlineData(GameEra.Blue)]
    [InlineData(GameEra.Green)]
    [InlineData(GameEra.Yellow)]
    [InlineData(GameEra.Red)]
    [InlineData(GameEra.White)]
    [InlineData(GameEra.Murasaki)]
    [InlineData(GameEra.Kimidori)]
    public async Task Handle_DoesNotUseReservedMedleyRowsAsRecommendSeed(GameEra era)
    {
        var catalog = Catalog(
            blueSongs: [20001],
            greenSongs: [20001],
            yellowSongs: [20001],
            redSongs: [20001],
            whiteSongs: [20001],
            murasakiSongs: [20001],
            kimidoriSongs: [20001]);
        var handler = new GetRecommendQueryHandler(
            NullLogger<GetRecommendQueryHandler>.Instance,
            catalog);

        var response = await handler.Handle(
            new GetRecommendQuery(era, GenderType: 0, PlayerAge: 0),
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(0u, response.RecommendSong);
        Assert.Empty(response.RecommendBestSong);
    }

    private static Ac15MusicInfoEntry Song(uint songNo)
        => new() { SongNo = songNo, MusicId = $"song{songNo}", FileOrder = 0 };

    private static FileGameDataCatalog Catalog(
        IReadOnlyList<uint> blueSongs,
        IReadOnlyList<uint> greenSongs,
        IReadOnlyList<uint> yellowSongs,
        IReadOnlyList<uint> redSongs,
        IReadOnlyList<uint> whiteSongs,
        IReadOnlyList<uint> murasakiSongs,
        IReadOnlyList<uint> kimidoriSongs)
        => new(
        [
            new BlueHandlerFixture.TestBlueCatalog(musicInfoFileOrder: blueSongs.Select(Song).ToArray()),
            new GreenHandlerFixture.TestGreenCatalog(musicInfoFileOrder: greenSongs.Select(Song).ToArray()),
            new YellowHandlerFixture.TestYellowCatalog(musicInfoFileOrder: yellowSongs.Select(Song).ToArray()),
            new RedHandlerFixture.TestRedCatalog(musicInfoFileOrder: redSongs.Select(Song).ToArray()),
            new WhiteHandlerFixture.TestWhiteCatalog(musicInfoFileOrder: whiteSongs.Select(Song).ToArray()),
            new MurasakiHandlerFixture.TestMurasakiCatalog(musicInfoFileOrder: murasakiSongs.Select(Song).ToArray()),
            new KimidoriHandlerFixture.TestKimidoriCatalog(musicInfoFileOrder: kimidoriSongs.Select(Song).ToArray())
        ]);
}
