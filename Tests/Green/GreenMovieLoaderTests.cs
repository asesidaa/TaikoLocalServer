using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenMovieLoaderTests
{
    [Fact]
    public async Task LoadFromFile_MissingConfigDefaultsToDiscoveredNonzeroMovies()
    {
        using var directory = TempDirectory.Create();
        var movieDirectory = Path.Combine(directory.Path, "movie");
        Directory.CreateDirectory(movieDirectory);
        CreateMovie(movieDirectory, "attract_cm_000.pam");
        CreateMovie(movieDirectory, "attract_cm_102.pam");
        CreateMovie(movieDirectory, "attract_cm_100.pam");
        CreateMovie(movieDirectory, "not_an_attract_movie.pam");

        var movies = await GreenMovieLoader.LoadFromFileAsync(
            Path.Combine(directory.Path, "movie_data.json"),
            movieDirectory,
            NullLogger.Instance,
            CancellationToken.None);

        Assert.Collection(
            movies,
            movie => AssertMovie(movie, 100, 999),
            movie => AssertMovie(movie, 102, 999));
    }

    [Fact]
    public async Task LoadFromFile_OverrideDefaultFalseIgnoresConfiguredMovies()
    {
        using var directory = TempDirectory.Create();
        var movieDirectory = Path.Combine(directory.Path, "movie");
        Directory.CreateDirectory(movieDirectory);
        CreateMovie(movieDirectory, "attract_cm_100.pam");
        CreateMovie(movieDirectory, "attract_cm_102.pam");
        var configPath = Path.Combine(directory.Path, "movie_data.json");
        await File.WriteAllTextAsync(configPath, """
            {
              "override_default": false,
              "movies": [
                { "movie_id": 102, "enable_days": 5 }
              ]
            }
            """);

        var movies = await GreenMovieLoader.LoadFromFileAsync(
            configPath,
            movieDirectory,
            NullLogger.Instance,
            CancellationToken.None);

        Assert.Collection(
            movies,
            movie => AssertMovie(movie, 100, 999),
            movie => AssertMovie(movie, 102, 999));
    }

    [Fact]
    public async Task LoadFromFile_OverrideDefaultTrueUsesConfiguredSubset()
    {
        using var directory = TempDirectory.Create();
        var movieDirectory = Path.Combine(directory.Path, "movie");
        Directory.CreateDirectory(movieDirectory);
        CreateMovie(movieDirectory, "attract_cm_100.pam");
        CreateMovie(movieDirectory, "attract_cm_102.pam");
        CreateMovie(movieDirectory, "attract_cm_103.pam");
        var configPath = Path.Combine(directory.Path, "movie_data.json");
        await File.WriteAllTextAsync(configPath, """
            {
              "override_default": true,
              "movies": [
                { "movie_id": 102, "enable_days": 5 }
              ]
            }
            """);

        var movies = await GreenMovieLoader.LoadFromFileAsync(
            configPath,
            movieDirectory,
            NullLogger.Instance,
            CancellationToken.None);

        Assert.Collection(movies, movie => AssertMovie(movie, 102, 5));
    }

    [Fact]
    public async Task LoadFromFile_EmptyOverrideReturnsNoMovies()
    {
        using var directory = TempDirectory.Create();
        var movieDirectory = Path.Combine(directory.Path, "movie");
        Directory.CreateDirectory(movieDirectory);
        CreateMovie(movieDirectory, "attract_cm_100.pam");
        var configPath = Path.Combine(directory.Path, "movie_data.json");
        await File.WriteAllTextAsync(configPath, """
            {
              "override_default": true,
              "movies": []
            }
            """);

        var movies = await GreenMovieLoader.LoadFromFileAsync(
            configPath,
            movieDirectory,
            NullLogger.Instance,
            CancellationToken.None);

        Assert.Empty(movies);
    }

    [Fact]
    public async Task LoadFromFile_MovieIdZeroIsIgnored()
    {
        using var directory = TempDirectory.Create();
        var movieDirectory = Path.Combine(directory.Path, "movie");
        Directory.CreateDirectory(movieDirectory);
        CreateMovie(movieDirectory, "attract_cm_000.pam");
        CreateMovie(movieDirectory, "attract_cm_100.pam");
        var configPath = Path.Combine(directory.Path, "movie_data.json");
        await File.WriteAllTextAsync(configPath, """
            {
              "override_default": true,
              "movies": [
                { "movie_id": 0, "enable_days": 999 },
                { "movie_id": 0, "enable_days": 1 },
                { "movie_id": 100, "enable_days": 7 }
              ]
            }
            """);

        var movies = await GreenMovieLoader.LoadFromFileAsync(
            configPath,
            movieDirectory,
            NullLogger.Instance,
            CancellationToken.None);

        Assert.Collection(movies, movie => AssertMovie(movie, 100, 7));
    }

    [Fact]
    public async Task LoadFromFile_DuplicateNonzeroMovieIdsFail()
    {
        using var directory = TempDirectory.Create();
        var movieDirectory = Path.Combine(directory.Path, "movie");
        Directory.CreateDirectory(movieDirectory);
        CreateMovie(movieDirectory, "attract_cm_100.pam");
        var configPath = Path.Combine(directory.Path, "movie_data.json");
        await File.WriteAllTextAsync(configPath, """
            {
              "override_default": true,
              "movies": [
                { "movie_id": 100, "enable_days": 7 },
                { "movie_id": 100, "enable_days": 9 }
              ]
            }
            """);

        var error = await Assert.ThrowsAsync<InvalidDataException>(() =>
            GreenMovieLoader.LoadFromFileAsync(
                configPath,
                movieDirectory,
                NullLogger.Instance,
                CancellationToken.None));

        Assert.Contains("Duplicate Green movie IDs", error.Message);
        Assert.Contains("100", error.Message);
    }

    [Fact]
    public async Task LoadFromFile_InvalidJsonFails()
    {
        using var directory = TempDirectory.Create();
        var movieDirectory = Path.Combine(directory.Path, "movie");
        Directory.CreateDirectory(movieDirectory);
        var configPath = Path.Combine(directory.Path, "movie_data.json");
        await File.WriteAllTextAsync(configPath, "{ invalid json");

        var error = await Assert.ThrowsAsync<InvalidDataException>(() =>
            GreenMovieLoader.LoadFromFileAsync(
                configPath,
                movieDirectory,
                NullLogger.Instance,
                CancellationToken.None));

        Assert.Contains("Invalid Green movie data JSON", error.Message);
    }

    [Fact]
    public async Task LoadFromFile_ExistingConfigMissingOverrideDefaultFails()
    {
        using var directory = TempDirectory.Create();
        var movieDirectory = Path.Combine(directory.Path, "movie");
        Directory.CreateDirectory(movieDirectory);
        var configPath = Path.Combine(directory.Path, "movie_data.json");
        await File.WriteAllTextAsync(configPath, """
            {
              "movies": [
                { "movie_id": 100, "enable_days": 999 }
              ]
            }
            """);

        var error = await Assert.ThrowsAsync<InvalidDataException>(() =>
            GreenMovieLoader.LoadFromFileAsync(
                configPath,
                movieDirectory,
                NullLogger.Instance,
                CancellationToken.None));

        Assert.Contains("override_default", error.Message);
    }

    [Fact]
    public async Task LoadFromFile_MissingMovieDirectoryReturnsEmptyDefaultList()
    {
        using var directory = TempDirectory.Create();

        var movies = await GreenMovieLoader.LoadFromFileAsync(
            Path.Combine(directory.Path, "movie_data.json"),
            Path.Combine(directory.Path, "missing-movie-directory"),
            NullLogger.Instance,
            CancellationToken.None);

        Assert.Empty(movies);
    }

    [Fact]
    public async Task LoadFromFile_OverrideIdsMissingFromDiscoveryAreSkipped()
    {
        using var directory = TempDirectory.Create();
        var movieDirectory = Path.Combine(directory.Path, "movie");
        Directory.CreateDirectory(movieDirectory);
        CreateMovie(movieDirectory, "attract_cm_100.pam");
        var configPath = Path.Combine(directory.Path, "movie_data.json");
        await File.WriteAllTextAsync(configPath, """
            {
              "override_default": true,
              "movies": [
                { "movie_id": 100, "enable_days": 7 },
                { "movie_id": 142, "enable_days": 7 }
              ]
            }
            """);

        var movies = await GreenMovieLoader.LoadFromFileAsync(
            configPath,
            movieDirectory,
            NullLogger.Instance,
            CancellationToken.None);

        Assert.Collection(movies, movie => AssertMovie(movie, 100, 7));
    }

    private static void CreateMovie(string directory, string fileName)
    {
        File.WriteAllText(Path.Combine(directory, fileName), string.Empty);
    }

    private static void AssertMovie(MovieData movie, uint movieId, uint enableDays)
    {
        Assert.Equal(movieId, movie.MovieId);
        Assert.Equal(enableDays, movie.EnableDays);
    }

    private sealed class TempDirectory : IDisposable
    {
        private TempDirectory(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static TempDirectory Create()
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return new TempDirectory(path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
