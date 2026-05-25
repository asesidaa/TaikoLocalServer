using System.Collections.Immutable;
using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using MovieData = TaikoLocalServer.Application.ServerData.MovieData;

namespace TaikoLocalServer.Tests.Green;

public sealed class StartupMovieDataQueryTests
{
    [Fact]
    public async Task Handle_GreenHddVersionReturnsGreenMovies()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(new GreenHandlerFixture.TestGreenCatalog
        {
            Movies =
            [
                new() { MovieId = 100, EnableDays = 999 }
            ]
        });
        var handler = CreateHandler(fixture.Catalog, Enabled(GameEra.Green));

        var movies = await handler.Handle(new GetStartupMovieDataQuery(1113), CancellationToken.None);

        Assert.Collection(movies, movie => AssertMovie(movie, 100, 999));
    }

    [Fact]
    public async Task Handle_NijiiroHddVersionReturnsNijiiroMovies()
    {
        var catalog = new FileGameDataCatalog(
        [
            new TestNijiiroCatalog(
            [
                new() { MovieId = 2, EnableDays = 10 },
                new() { MovieId = 1, EnableDays = 9 }
            ])
        ]);
        var handler = CreateHandler(catalog, Enabled(GameEra.Nijiiro));

        var movies = await handler.Handle(new GetStartupMovieDataQuery(1239), CancellationToken.None);

        Assert.Collection(
            movies,
            movie => AssertMovie(movie, 1, 9),
            movie => AssertMovie(movie, 2, 10));
    }

    [Fact]
    public async Task Handle_UnknownHddVersionWithOneEnabledEraUsesThatEra()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(new GreenHandlerFixture.TestGreenCatalog
        {
            Movies =
            [
                new() { MovieId = 102, EnableDays = 999 }
            ]
        });
        var handler = CreateHandler(fixture.Catalog, Enabled(GameEra.Green));

        var movies = await handler.Handle(new GetStartupMovieDataQuery(9999), CancellationToken.None);

        Assert.Collection(movies, movie => AssertMovie(movie, 102, 999));
    }

    [Fact]
    public async Task Handle_UnknownHddVersionWithMultipleEnabledErasReturnsNoMovies()
    {
        var catalog = new FileGameDataCatalog(
        [
            new GreenHandlerFixture.TestGreenCatalog
            {
                Movies =
                [
                    new() { MovieId = 100, EnableDays = 999 }
                ]
            },
            new TestNijiiroCatalog(
            [
                new() { MovieId = 1, EnableDays = 9 }
            ])
        ]);
        var handler = CreateHandler(catalog, Enabled(GameEra.Green, GameEra.Nijiiro));

        var movies = await handler.Handle(new GetStartupMovieDataQuery(9999), CancellationToken.None);

        Assert.Empty(movies);
    }

    [Fact]
    public async Task Handle_ResolvedDisabledEraReturnsNoMovies()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(new GreenHandlerFixture.TestGreenCatalog
        {
            Movies =
            [
                new() { MovieId = 100, EnableDays = 999 }
            ]
        });
        var handler = CreateHandler(fixture.Catalog, Enabled(GameEra.Nijiiro));

        var movies = await handler.Handle(new GetStartupMovieDataQuery(1113), CancellationToken.None);

        Assert.Empty(movies);
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

    private sealed class TestNijiiroCatalog(IReadOnlyList<MovieData> movies) : INijiiroCatalog
    {
        public GameEra Era => GameEra.Nijiiro;

        public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos { get; } =
            new Dictionary<uint, IMusicInfoEntry>();

        public Task InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public List<uint> GetMusicList() => [];

        public List<uint> GetMusicWithUraList() => [];

        public ImmutableDictionary<uint, SongIntroductionData> GetSongIntroductionDictionary()
            => ImmutableDictionary<uint, SongIntroductionData>.Empty;

        public ImmutableDictionary<uint, MovieData> GetMovieDataDictionary()
            => movies.ToImmutableDictionary(movie => movie.MovieId);

        public ImmutableDictionary<uint, EventFolderData> GetEventFolderDictionary()
            => ImmutableDictionary<uint, EventFolderData>.Empty;

        public ImmutableDictionary<uint, DanData> GetCommonDanDataDictionary()
            => ImmutableDictionary<uint, DanData>.Empty;

        public ImmutableDictionary<uint, DanData> GetCommonGaidenDataDictionary()
            => ImmutableDictionary<uint, DanData>.Empty;

        public List<ShopFolderData> GetShopFolderList() => [];

        public uint GetShopFolderVerup() => 0;

        public Dictionary<string, int> GetTokenDataDictionary() => [];

        public List<uint> GetLockedSongsList() => [];

        public List<uint> GetTimeLimitedSongsList() => [];

        public List<uint> GetLockedUraSongsList() => [];

        public Dictionary<uint, MusicDetail> GetMusicDetailDictionary() => [];

        public List<Costume> GetCostumeList() => [];

        public Dictionary<uint, Title> GetTitleDictionary() => [];

        public Dictionary<uint, Neiro> GetNeiroDictionary() => [];

        public Dictionary<string, List<uint>> GetLockedCostumeDataDictionary() => [];

        public Dictionary<string, List<uint>> GetLockedTitleDataDictionary() => [];

        public List<int> GetCostumeFlagArraySizes() => [];

        public int GetTitleFlagArraySize() => 0;

        public int GetToneFlagArraySize() => 0;

        public ImmutableDictionary<string, uint> GetQRCodeDataDictionary()
            => ImmutableDictionary<string, uint>.Empty;
    }
}
