using MovieData = TaikoLocalServer.Application.ServerData.MovieData;

namespace TaikoLocalServer.Tests.Momoiro;

public sealed class MomoiroStartupMovieDataQueryTests
{
    [Fact]
    public async Task Handle_MomoiroHddVersionReturnsMomoiroMovies()
    {
        await using var fixture = await MomoiroHandlerFixture.CreateAsync(
            new MomoiroHandlerFixture.TestMomoiroCatalog
            {
                Movies =
                [
                    new() { MovieId = 100, EnableDays = 999 }
                ]
            });
        var handler = CreateHandler(fixture.Catalog, Enabled(GameEra.Momoiro));

        var movies = await handler.Handle(new GetStartupMovieDataQuery(413), CancellationToken.None);

        Assert.Collection(movies, movie => AssertMovie(movie, 100, 999));
    }

    private static GetStartupMovieDataQueryHandler CreateHandler(
        IGameDataCatalog catalog,
        ServerSettings settings)
        => new(
            catalog,
            NullLogger<GetStartupMovieDataQueryHandler>.Instance,
            Options.Create(settings));

    private static ServerSettings Enabled(params GameEra[] eras)
    {
        return new ServerSettings
        {
            Eras = eras.ToDictionary(
                era => era.ToString(),
                _ => new EraSettings { Enabled = true })
        };
    }

    private static void AssertMovie(MovieData movie, uint movieId, uint enableDays)
    {
        Assert.Equal(movieId, movie.MovieId);
        Assert.Equal(enableDays, movie.EnableDays);
    }
}
