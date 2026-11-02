# Green Attract Movies Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Send era-specific startup auth movie permissions so Green auto-enables discovered nonzero `attract_cm_###.pam` movies by default and supports an explicit override config.

**Architecture:** Green movie permissions are loaded into `IGreenCatalog` during catalog initialization, either from discovered files or from `Host/wwwroot/data/green/movie_data.json`. Startup auth stays on the shared route, but a new `GetStartupMovieDataQuery(uint HddVer)` resolves the era from `hdd_ver` and returns the correct movie list for the shared controller to map into `ary_movie_info`.

**Tech Stack:** .NET 10, ASP.NET Core controllers, Mediator source generator, protobuf-net, xUnit, JSON configuration under `Host/wwwroot/data`.

**Reference:** [docs/superpowers/specs/2026-05-25-green-attract-movies-design.md](../specs/2026-05-25-green-attract-movies-design.md) (commit `bef16d4c`).

---

## Worktree And Safety

The working tree currently has pre-existing edits in these files:

- `Adapters.GameProtocol.Shared/Adapters.GameProtocol.Shared.csproj`
- `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs`
- `Adapters.GameProtocol.Shared/GlobalUsings.cs`
- `Host/Configurations/ServerSettings.json`

Do not revert these files. Read them before editing and preserve unrelated user changes. Before every commit, run `git status --short` and stage only the files listed in that task.

## File Structure

- **Create** `Tests/Green/GreenMovieLoaderTests.cs`
  - Focused loader contract tests for discovery, override semantics, validation, and missing movie directory behavior.
- **Create** `Infrastructure/GameDataCatalog/Green/GreenMovieLoader.cs`
  - Loads `Host/wwwroot/data/green/movie_data.json` and discovers `Host/wwwroot/data/green/data/movie/attract_cm_*.pam`.
- **Modify** `Application/Abstractions/IGreenCatalog.cs`
  - Adds `IReadOnlyList<MovieData> Movies`.
- **Modify** `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`
  - Stores Green movie permissions and initializes them through `GreenMovieLoader`.
- **Modify** `Tests/Green/GreenHandlerFixture.cs`
  - Adds configurable `Movies` to the test Green catalog.
- **Create** `Tests/Green/StartupMovieDataQueryTests.cs`
  - Tests `hdd_ver` era resolution and returned movie permissions.
- **Create** `Application/Handlers/GetStartupMovieDataQuery.cs`
  - Application query handler used by the shared startup auth controller.
- **Modify** `Tests/Green/StartupAuthRouteTests.cs`
  - Extends protobuf compatibility coverage to include `ary_movie_info`.
  - Adds a controller integration test that verifies `hdd_ver = 1113` populates Green movie rows.
- **Modify** `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs`
  - Calls `GetStartupMovieDataQuery` and maps returned `MovieData` rows into shared wire rows.
- **Create** `Host/wwwroot/data/green/movie_data.json`
  - Documents the default Green movie config shape.
- **Modify** `Host/Host.csproj`
  - Copies the new Green movie config to output.
- **Modify** `Host/README.md`
  - Documents Green movie config and clarifies that the existing bare-array `movie_data.json` section is Nijiiro-specific.

## Task 1: Green Movie Loader Red Tests

**Files:**
- Create: `Tests/Green/GreenMovieLoaderTests.cs`

- [ ] **Step 1: Add the loader contract tests**

Create `Tests/Green/GreenMovieLoaderTests.cs`:

```csharp
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
```

- [ ] **Step 2: Run the focused loader tests and confirm red state**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenMovieLoaderTests"
```

Expected: FAIL with compiler errors that `GreenMovieLoader` does not exist.

Do not commit this red state.

## Task 2: Green Movie Loader Implementation

**Files:**
- Create: `Infrastructure/GameDataCatalog/Green/GreenMovieLoader.cs`
- Test: `Tests/Green/GreenMovieLoaderTests.cs`

- [ ] **Step 1: Add the loader implementation**

Create `Infrastructure/GameDataCatalog/Green/GreenMovieLoader.cs`:

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenMovieLoader
{
    private const uint DefaultEnableDays = 999;

    private static readonly Regex MovieFileRegex =
        new(@"^attract_cm_(\d{3})\.pam$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public Task<IReadOnlyList<MovieData>> LoadAsync(
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var greenPath = PathHelper.GetDataPath(GameEra.Green);
        return LoadFromFileAsync(
            Path.Combine(greenPath, "movie_data.json"),
            Path.Combine(greenPath, "data", "movie"),
            logger,
            cancellationToken);
    }

    public static async Task<IReadOnlyList<MovieData>> LoadFromFileAsync(
        string configPath,
        string movieDirectory,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var discoveredMovieIds = DiscoverMovieIds(movieDirectory, logger);
        if (!File.Exists(configPath))
        {
            return CreateDefaultMovies(discoveredMovieIds);
        }

        GreenMovieConfig? config;
        try
        {
            await using var stream = File.OpenRead(configPath);
            config = await JsonSerializer.DeserializeAsync<GreenMovieConfig>(
                stream,
                JsonOptions,
                cancellationToken);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"Invalid Green movie data JSON: {configPath}", ex);
        }

        if (config?.OverrideDefault is null)
        {
            throw new InvalidDataException(
                $"Green movie data must include boolean property override_default: {configPath}");
        }

        if (!config.OverrideDefault.Value)
        {
            return CreateDefaultMovies(discoveredMovieIds);
        }

        return CreateOverrideMovies(configPath, config.Movies ?? [], discoveredMovieIds, logger);
    }

    private static IReadOnlySet<uint> DiscoverMovieIds(string movieDirectory, ILogger logger)
    {
        if (!Directory.Exists(movieDirectory))
        {
            logger.LogWarning("Green movie directory does not exist: {MovieDirectory}", movieDirectory);
            return new HashSet<uint>();
        }

        var result = new HashSet<uint>();
        foreach (var path in Directory.EnumerateFiles(movieDirectory, "attract_cm_*.pam"))
        {
            var match = MovieFileRegex.Match(Path.GetFileName(path));
            if (!match.Success)
            {
                continue;
            }

            var movieId = uint.Parse(match.Groups[1].Value);
            if (movieId != 0)
            {
                result.Add(movieId);
            }
        }

        return result;
    }

    private static IReadOnlyList<MovieData> CreateDefaultMovies(IReadOnlySet<uint> discoveredMovieIds)
    {
        return discoveredMovieIds
            .Order()
            .Select(movieId => new MovieData
            {
                MovieId = movieId,
                EnableDays = DefaultEnableDays
            })
            .ToArray();
    }

    private static IReadOnlyList<MovieData> CreateOverrideMovies(
        string configPath,
        IReadOnlyList<MovieData> configuredMovies,
        IReadOnlySet<uint> discoveredMovieIds,
        ILogger logger)
    {
        var duplicateIds = configuredMovies
            .Where(movie => movie.MovieId != 0)
            .GroupBy(movie => movie.MovieId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .Order()
            .ToArray();

        if (duplicateIds.Length > 0)
        {
            throw new InvalidDataException(
                $"Duplicate Green movie IDs in {configPath}: {string.Join(", ", duplicateIds)}");
        }

        var result = new List<MovieData>();
        foreach (var movie in configuredMovies)
        {
            if (movie.MovieId == 0)
            {
                continue;
            }

            if (!discoveredMovieIds.Contains(movie.MovieId))
            {
                logger.LogWarning(
                    "Green movie config references missing attract movie ID {MovieId}; skipping.",
                    movie.MovieId);
                continue;
            }

            result.Add(new MovieData
            {
                MovieId = movie.MovieId,
                EnableDays = movie.EnableDays
            });
        }

        return result
            .OrderBy(movie => movie.MovieId)
            .ToArray();
    }

    private sealed class GreenMovieConfig
    {
        [JsonPropertyName("override_default")]
        public bool? OverrideDefault { get; set; }

        [JsonPropertyName("movies")]
        public MovieData[]? Movies { get; set; }
    }
}
```

- [ ] **Step 2: Run the focused loader tests and confirm green state**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenMovieLoaderTests"
```

Expected: PASS. All `GreenMovieLoaderTests` pass.

- [ ] **Step 3: Commit the loader**

Run:

```powershell
git status --short
git add -- Tests/Green/GreenMovieLoaderTests.cs Infrastructure/GameDataCatalog/Green/GreenMovieLoader.cs
git diff --cached --name-status
git commit -m "Add Green attract movie loader"
```

Expected staged files:

```text
A	Tests/Green/GreenMovieLoaderTests.cs
A	Infrastructure/GameDataCatalog/Green/GreenMovieLoader.cs
```

## Task 3: Catalog Wiring

**Files:**
- Modify: `Application/Abstractions/IGreenCatalog.cs`
- Modify: `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`
- Modify: `Tests/Green/GreenHandlerFixture.cs`

- [ ] **Step 1: Extend the Green catalog contract**

Modify `Application/Abstractions/IGreenCatalog.cs`. Add this property after `GreenRecommendEntry Recommend { get; }`:

```csharp
IReadOnlyList<MovieData> Movies { get; }
```

If the file does not already resolve `MovieData` through global usings in the current checkout, add this using at the top:

```csharp
using TaikoLocalServer.Application.ServerData;
```

- [ ] **Step 2: Store and load movies in the real Green catalog**

Modify `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`.

Add this private field next to the other cached catalog fields:

```csharp
private IReadOnlyList<MovieData> movies = [];
```

Add this public property after `public GreenRecommendEntry Recommend => recommend;`:

```csharp
public IReadOnlyList<MovieData> Movies => movies;
```

Add this line in `InitializeAsync` after `recommend = await new GreenRecommendLoader().LoadAsync(...)`:

```csharp
movies = await new GreenMovieLoader().LoadAsync(logger, cancellationToken);
```

Update the final log statement to include the movie count. Replace the existing `logger.LogInformation(...)` call at the end of `InitializeAsync` with:

```csharp
logger.LogInformation(
    "Loaded Green catalog: {SongCount} songs, song_hash_ver={SongHashVersion}, {TaikojukuCount} taikojuku packs, {StarCount} tuning star rows, {CostumeCount} costumes, {TitleCount} titles, {NeiroCount} tones, {MovieCount} attract movies",
    musicInfoFileOrder.Count,
    songHashVersion,
    taikojukuFileOrder.Count,
    stars.Count,
    costumeList.Count,
    titleDictionary.Count,
    neiroDictionary.Count,
    movies.Count);
```

- [ ] **Step 3: Extend the test Green catalog**

Modify `Tests/Green/GreenHandlerFixture.cs`.

Add this using at the top if `MovieData` is not already resolved:

```csharp
using TaikoLocalServer.Application.ServerData;
```

Add this property to `GreenHandlerFixture.TestGreenCatalog` after `Recommend`:

```csharp
public IReadOnlyList<MovieData> Movies { get; init; } = [];
```

- [ ] **Step 4: Build to catch interface implementation misses**

Run:

```powershell
dotnet build Host/Host.csproj
```

Expected: PASS. If it fails with missing `Movies` implementations, add the property to the listed `IGreenCatalog` test doubles before continuing.

- [ ] **Step 5: Commit catalog wiring**

Run:

```powershell
git status --short
git add -- Application/Abstractions/IGreenCatalog.cs Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs Tests/Green/GreenHandlerFixture.cs
git diff --cached --name-status
git commit -m "Wire Green attract movies into catalog"
```

Expected staged files:

```text
M	Application/Abstractions/IGreenCatalog.cs
M	Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs
M	Tests/Green/GreenHandlerFixture.cs
```

## Task 4: Startup Movie Query

**Files:**
- Create: `Tests/Green/StartupMovieDataQueryTests.cs`
- Create: `Application/Handlers/GetStartupMovieDataQuery.cs`

- [ ] **Step 1: Add query handler tests**

Create `Tests/Green/StartupMovieDataQueryTests.cs`:

```csharp
using System.Collections.Immutable;
using TaikoLocalServer.Application.ServerData;
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
```

- [ ] **Step 2: Run query tests and confirm red state**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~StartupMovieDataQueryTests"
```

Expected: FAIL with compiler errors that `GetStartupMovieDataQuery` and `GetStartupMovieDataQueryHandler` do not exist.

Do not commit this red state.

- [ ] **Step 3: Add the startup movie query handler**

Create `Application/Handlers/GetStartupMovieDataQuery.cs`:

```csharp
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Settings;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetStartupMovieDataQuery(uint HddVer) : IRequest<IReadOnlyList<MovieData>>;

public sealed class GetStartupMovieDataQueryHandler(
    IGameDataCatalog gameDataService,
    ILogger<GetStartupMovieDataQueryHandler> logger,
    IOptions<ServerSettings> settings)
    : IRequestHandler<GetStartupMovieDataQuery, IReadOnlyList<MovieData>>
{
    private readonly ServerSettings settings = settings.Value;

    public ValueTask<IReadOnlyList<MovieData>> Handle(
        GetStartupMovieDataQuery query,
        CancellationToken cancellationToken)
    {
        var era = ResolveEra(query.HddVer);
        if (era is null)
        {
            return ValueTask.FromResult((IReadOnlyList<MovieData>)[]);
        }

        var movies = era.Value switch
        {
            GameEra.Green => gameDataService.Green().Movies,
            GameEra.Nijiiro => gameDataService.Nijiiro()
                .GetMovieDataDictionary()
                .Values
                .OrderBy(movie => movie.MovieId)
                .ToArray(),
            _ => []
        };

        return ValueTask.FromResult(movies);
    }

    private GameEra? ResolveEra(uint hddVer)
    {
        var requestedEra = (hddVer / 100) switch
        {
            11 => GameEra.Green,
            12 => GameEra.Nijiiro,
            _ => (GameEra?)null
        };

        if (requestedEra is not null)
        {
            if (IsEnabled(requestedEra.Value))
            {
                return requestedEra.Value;
            }

            logger.LogWarning(
                "Startup auth HDD version {HddVer} resolved to disabled era {Era}; no movie permissions will be sent.",
                hddVer,
                requestedEra.Value);
            return null;
        }

        var enabledEras = GetEnabledEras();
        if (enabledEras.Count == 1)
        {
            logger.LogWarning(
                "Startup auth HDD version {HddVer} is unknown; using the only enabled era {Era}.",
                hddVer,
                enabledEras[0]);
            return enabledEras[0];
        }

        logger.LogWarning(
            "Startup auth HDD version {HddVer} is unknown and {EnabledEraCount} eras are enabled; no movie permissions will be sent.",
            hddVer,
            enabledEras.Count);
        return null;
    }

    private bool IsEnabled(GameEra era)
    {
        return settings.Eras.TryGetValue(era.ToString(), out var eraSettings)
               && eraSettings.Enabled;
    }

    private IReadOnlyList<GameEra> GetEnabledEras()
    {
        return settings.Eras
            .Where(pair => pair.Value.Enabled)
            .Select(pair => Enum.TryParse<GameEra>(pair.Key, ignoreCase: true, out var era)
                ? era
                : (GameEra?)null)
            .Where(era => era is not null)
            .Select(era => era!.Value)
            .Distinct()
            .Order()
            .ToArray();
    }
}
```

- [ ] **Step 4: Run query tests and confirm green state**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~StartupMovieDataQueryTests"
```

Expected: PASS. All `StartupMovieDataQueryTests` pass.

- [ ] **Step 5: Commit the query handler**

Run:

```powershell
git status --short
git add -- Tests/Green/StartupMovieDataQueryTests.cs Application/Handlers/GetStartupMovieDataQuery.cs
git diff --cached --name-status
git commit -m "Resolve startup auth movie permissions by era"
```

Expected staged files:

```text
A	Tests/Green/StartupMovieDataQueryTests.cs
A	Application/Handlers/GetStartupMovieDataQuery.cs
```

## Task 5: Shared Startup Auth Integration

**Files:**
- Modify: `Tests/Green/StartupAuthRouteTests.cs`
- Modify: `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs`

- [ ] **Step 1: Extend startup auth tests**

Modify the using block at the top of `Tests/Green/StartupAuthRouteTests.cs` so it includes these namespaces and alias:

```csharp
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;
using TaikoLocalServer.Application;
using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Infrastructure.GameDataCatalog;
using AppMovieData = TaikoLocalServer.Application.ServerData.MovieData;
using GreenStartupAuthRequest = TaikoLocalServer.Adapters.GameProtocol.Green.Wire.StartupAuthRequest;
using GreenStartupAuthResponse = TaikoLocalServer.Adapters.GameProtocol.Green.Wire.StartupAuthResponse;
using SharedStartupAuthRequest = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.StartupAuthRequest;
using SharedStartupAuthResponse = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.StartupAuthResponse;
using WwR08StartupAuthResponse = taiko.vsinterface.StartupAuthResponse;
```

In `SharedStartupAuthResponse_WritesVsInterfacePayload`, add one movie before serializing:

```csharp
response.AryMovieInfoes.Add(new SharedStartupAuthResponse.MovieData
{
    MovieId = 100,
    EnableDays = 999
});
```

Add these assertions after the existing operation assertions:

```csharp
Assert.Equal(100u, Assert.Single(green.AryMovieInfoes).MovieId);
Assert.Equal(999u, Assert.Single(green.AryMovieInfoes).EnableDays);
Assert.Equal(100u, Assert.Single(wwR08.AryMovieInfoes).MovieId);
Assert.Equal(999u, Assert.Single(wwR08.AryMovieInfoes).EnableDays);
```

Add this new test after `SharedStartupAuthResponse_WritesVsInterfacePayload()`:

```csharp
[Fact]
public async Task StartupAuthController_UsesHddVersionToPopulateMovieInfo()
{
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddOptions();
    services.AddApplication();
    services.Configure<ServerSettings>(settings =>
    {
        settings.Eras = new Dictionary<string, EraSettings>
        {
            [nameof(GameEra.Green)] = new() { Enabled = true }
        };
    });
    services.AddSingleton<IGameDataCatalog>(new FileGameDataCatalog(
    [
        new GreenHandlerFixture.TestGreenCatalog
        {
            Movies =
            [
                new AppMovieData { MovieId = 100, EnableDays = 999 }
            ]
        }
    ]));

    using var provider = services.BuildServiceProvider();
    var controller = new StartupAuthController
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = provider
            }
        }
    };

    var result = await controller.StartupAuth(new SharedStartupAuthRequest
    {
        ChassisId = "chassis",
        HddVer = 1113,
        ShopId = "shop"
    });

    var ok = Assert.IsType<OkObjectResult>(result);
    var response = Assert.IsType<SharedStartupAuthResponse>(ok.Value);
    var movie = Assert.Single(response.AryMovieInfoes);
    Assert.Equal(100u, movie.MovieId);
    Assert.Equal(999u, movie.EnableDays);
}
```

- [ ] **Step 2: Run startup auth tests and confirm red state**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~StartupAuthRouteTests"
```

Expected: FAIL because `StartupAuthController.StartupAuth` still returns `IActionResult` synchronously and does not populate `AryMovieInfoes`.

Do not commit this red state.

- [ ] **Step 3: Integrate movie query in the shared controller**

Modify `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs`.

Change the action signature from:

```csharp
public IActionResult StartupAuth([FromBody] StartupAuthRequest request)
```

to:

```csharp
public async Task<IActionResult> StartupAuth([FromBody] StartupAuthRequest request)
```

After `var response = new StartupAuthResponse { Result = 1 };`, add:

```csharp
var movieData = await Mediator.Send(
    new GetStartupMovieDataQuery(request.HddVer),
    HttpContext.RequestAborted);
response.AryMovieInfoes.AddRange(movieData.Select(movie =>
    new StartupAuthResponse.MovieData
    {
        MovieId = movie.MovieId,
        EnableDays = movie.EnableDays
    }));
```

Leave the existing `AryOperationInfoes` echo logic in place after the movie mapping.

- [ ] **Step 4: Run startup auth tests and confirm green state**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~StartupAuthRouteTests"
```

Expected: PASS. Startup auth route ownership remains shared, the response remains Green/WwR08 protobuf-compatible, and the controller test returns the configured Green movie row.

- [ ] **Step 5: Commit startup auth integration**

Run:

```powershell
git status --short
git add -- Tests/Green/StartupAuthRouteTests.cs Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs
git diff --cached --name-status
git commit -m "Populate startup auth movie permissions"
```

Expected staged files:

```text
M	Tests/Green/StartupAuthRouteTests.cs
M	Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs
```

## Task 6: Green Config And Documentation

**Files:**
- Create: `Host/wwwroot/data/green/movie_data.json`
- Modify: `Host/Host.csproj`
- Modify: `Host/README.md`

- [ ] **Step 1: Add the default Green movie config file**

Create `Host/wwwroot/data/green/movie_data.json`:

```json
{
  "override_default": false,
  "movies": []
}
```

- [ ] **Step 2: Copy the Green movie config to output**

Modify `Host/Host.csproj`. In the `<!--Green Game Data-->` item group, add this line after `green\telop_data.json`:

```xml
<Content Update="wwwroot\data\green\movie_data.json"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></Content>
```

- [ ] **Step 3: Document Green movie config in the data layout**

Modify `Host/README.md`. In the `wwwroot/data/green/` layout block, add this line after `telop_data.json` or the nearest Green operator-edited JSON file:

```text
|   |-- movie_data.json        Green attract movie permissions (default discovery or explicit override)
```

Under the Green AC15 Test Support section, add this subsection after `### Green customization catalogs`:

````markdown
### Green attract movies

Green startup auth sends attract movie permissions through `ary_movie_info`.
By default, Green auto-enables every nonzero `attract_cm_###.pam` file found
under `wwwroot/data/green/data/movie`.

`wwwroot/data/green/movie_data.json` controls whether discovery is used or
replaced:

```json
{
  "override_default": false,
  "movies": []
}
```

When `override_default` is `false`, the server ignores `movies` and sends all
discovered nonzero movie IDs with `enable_days = 999`. When
`override_default` is `true`, the server sends only the listed movies. An empty
`movies` array disables Green attract movies.

Example override:

```json
{
  "override_default": true,
  "movies": [
    { "movie_id": 100, "enable_days": 999 },
    { "movie_id": 102, "enable_days": 999 }
  ]
}
```

ID `0` is ignored because Green treats `attract_cm_000.pam` as a fallback
asset, not a startup permission candidate. Duplicate nonzero IDs are rejected
at startup.
````

In the existing `### movie_data.json` datatable documentation section, change the first sentence from:

```markdown
This is used to control which in-game movie is displayed before entering the game
```

to:

```markdown
For Nijiiro, this array controls which in-game movie is displayed before entering the game.
Green uses `wwwroot/data/green/movie_data.json` with the `override_default`
object format documented in [Green attract movies](#green-attract-movies).
```

- [ ] **Step 4: Build and run docs-sensitive tests**

Run:

```powershell
dotnet build Host/Host.csproj
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenMovieLoaderTests|FullyQualifiedName~StartupMovieDataQueryTests|FullyQualifiedName~StartupAuthRouteTests"
```

Expected: both commands PASS.

- [ ] **Step 5: Commit config and docs**

Run:

```powershell
git status --short
git add -- Host/wwwroot/data/green/movie_data.json Host/Host.csproj Host/README.md
git diff --cached --name-status
git commit -m "Document Green attract movie config"
```

Expected staged files:

```text
A	Host/wwwroot/data/green/movie_data.json
M	Host/Host.csproj
M	Host/README.md
```

## Task 7: Final Verification

**Files:**
- No source changes expected.

- [ ] **Step 1: Run focused tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenMovieLoaderTests|FullyQualifiedName~StartupMovieDataQueryTests|FullyQualifiedName~StartupAuthRouteTests"
```

Expected: PASS.

- [ ] **Step 2: Run host build**

Run:

```powershell
dotnet build Host/Host.csproj
```

Expected: PASS. If the build fails with file-lock errors such as `MSB3021`, `MSB3027`, or `CS2012`, stop any running server/test process and rerun the same build before treating it as a source failure.

- [ ] **Step 3: Review final git state**

Run:

```powershell
git status --short
git log --oneline -5
```

Expected:

- The only remaining dirty files are unrelated pre-existing user edits, or the tree is clean.
- The recent commits are:
  - `Document Green attract movie config`
  - `Populate startup auth movie permissions`
  - `Resolve startup auth movie permissions by era`
  - `Wire Green attract movies into catalog`
  - `Add Green attract movie loader`

- [ ] **Step 4: Note runtime verification gap**

Record in the final handoff that automated verification proves loader/query/controller behavior and build correctness, but actual Green attract playback still requires in-game RPCS3 or cabinet testing.
