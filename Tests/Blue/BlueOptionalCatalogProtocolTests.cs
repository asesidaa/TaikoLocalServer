using Microsoft.Extensions.Logging.Abstractions;
using TaikoLocalServer.Application.Catalog.Blue;
using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueOptionalCatalogProtocolTests
{
    [Fact]
    public async Task GetFolder_BlueReturnsKnownRequestedFoldersAndOmitsUnknownIds()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateCatalogWithFolders());
        var handler = new GetFolderQueryHandler(
            NullLogger<GetFolderQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new GetFolderQuery(GameEra.Blue, [11, 99, 1]), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Collection(
            response.AryEventfolderDatas,
            row =>
            {
                Assert.Equal(11u, row.FolderId);
                Assert.Equal(3u, row.VerupNo);
                Assert.Equal(new uint[] { 103 }, row.SongNoes);
            },
            row =>
            {
                Assert.Equal(1u, row.FolderId);
                Assert.Equal(0u, row.VerupNo);
                Assert.Equal(new uint[] { 101, 102 }, row.SongNoes);
            });
    }

    [Fact]
    public async Task GetTelop_BlueReturnsCatalogEntryWithOptionalFields()
    {
        var blueCatalog = new BlueHandlerFixture.TestBlueCatalog(
            telops: new Dictionary<uint, BlueTelopEntry>
            {
                [7] = new()
                {
                    TelopId = 7,
                    VerupNo = 4,
                    StartDatetime = "20240101000000",
                    EndDatetime = "20991231235959",
                    Message = "Hello Blue"
                }
            });
        await using var fixture = await BlueHandlerFixture.CreateAsync(blueCatalog);
        var handler = new GetTelopQueryHandler(fixture.Catalog);

        var response = await handler.Handle(new GetTelopQuery(GameEra.Blue, 7), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(4u, response.VerupNo);
        Assert.Equal("20240101000000", response.StartDatetime);
        Assert.Equal("20991231235959", response.EndDatetime);
        Assert.Equal("Hello Blue", response.Telop);
    }

    [Fact]
    public async Task StartupMovieData_BlueHddVersionReturnsBlueMovies()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(new BlueHandlerFixture.TestBlueCatalog
        {
            Movies =
            [
                new() { MovieId = 133, EnableDays = 999 }
            ]
        });
        var handler = new GetStartupMovieDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetStartupMovieDataQueryHandler>.Instance,
            Options.Create(new ServerSettings
            {
                Eras = new Dictionary<string, EraSettings>
                {
                    [nameof(GameEra.Blue)] = new() { Enabled = true }
                }
            }));

        var movies = await handler.Handle(new GetStartupMovieDataQuery(1003), CancellationToken.None);

        var movie = Assert.Single(movies);
        Assert.Equal(133u, movie.MovieId);
        Assert.Equal(999u, movie.EnableDays);
    }

    [Fact]
    public async Task DefaultBlueOptionalDataFiles_LoadWithSharedAc15Loaders()
    {
        var root = FindRepoRoot();
        var eventFolderPath = Path.Combine(root, "Host", "wwwroot", "data", "blue", BlueEventFolderLoader.FileName);
        var telopPath = Path.Combine(root, "Host", "wwwroot", "data", "blue", BlueTelopLoader.FileName);
        var moviePath = Path.Combine(root, "Host", "wwwroot", "data", "blue", BlueMovieLoader.FileName);

        Assert.True(File.Exists(eventFolderPath), $"Missing {eventFolderPath}");
        Assert.True(File.Exists(telopPath), $"Missing {telopPath}");
        Assert.True(File.Exists(moviePath), $"Missing {moviePath}");

        var eventFolders = await Ac15EventFolderLoader.LoadFromFileAsync(
            eventFolderPath,
            new HashSet<uint>(),
            nameof(GameEra.Blue),
            CancellationToken.None);
        var telops = await Ac15TelopLoader.LoadFromFileAsync(telopPath, CancellationToken.None);
        var movies = await Ac15MovieLoader.LoadFromFileAsync(
            moviePath,
            Path.Combine(root, "missing-blue-movie-directory"),
            nameof(GameEra.Blue),
            NullLogger.Instance,
            CancellationToken.None);

        Assert.Empty(eventFolders);
        Assert.NotNull(telops);
        Assert.NotNull(movies);
    }

    private static BlueHandlerFixture.TestBlueCatalog CreateCatalogWithFolders()
        => new(eventFolders: new Dictionary<uint, EventFolderData>
        {
            [1] = new()
            {
                FolderId = 1,
                VerupNo = 0,
                SongNoes = [101, 102]
            },
            [11] = new()
            {
                FolderId = 11,
                VerupNo = 3,
                SongNoes = [103]
            }
        });

    private static string FindRepoRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Could not find TaikoLocalServer.slnx.");
    }
}
