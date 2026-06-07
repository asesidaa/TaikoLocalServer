using Microsoft.Extensions.Logging;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;
using TaikoLocalServer.Tests.Green;

namespace TaikoLocalServer.Tests.Yellow;

[Collection(GreenRuntimeCatalogTestCollection.Name)]
public sealed class YellowCatalogLoaderTests
{
    [Fact]
    public async Task MusicInfoLoader_ReadsLocalYellowDataWhenPresent()
    {
        var file = FindRepoFileOrSkip("Host", "wwwroot", "data", "yellow", "data", "config", "ST9100-1", "musicinfo.xml");
        if (file is null)
        {
            return;
        }

        var result = await YellowMusicInfoLoader.LoadFromFileAsync(file, CancellationToken.None);

        Assert.True(result.SongHashVersion > 0);
        Assert.NotEmpty(result.Entries);
        Assert.Equal(0, result.Entries[0].FileOrder);
        Assert.True(result.Entries[0].SongNo > 0);
        Assert.NotEmpty(result.Entries[0].MusicId);
    }

    [Fact]
    public async Task TaikojukuLoader_ReadsLocalYellowDataWhenPresent()
    {
        var file = FindRepoFileOrSkip("Host", "wwwroot", "data", "yellow", "data", "config", "ST9100-1", "musicmedleyinfo.xml");
        if (file is null)
        {
            return;
        }

        var entries = await YellowTaikojukuLoader.LoadFromFileAsync(file, CancellationToken.None);

        Assert.NotEmpty(entries);
        Assert.True(entries[0].UniqueId > 0);
        Assert.True(entries[0].ChallengeLevel > 0);
        Assert.NotEmpty(entries[0].Songs);
    }

    [Fact]
    public async Task TuningLoader_ReadsLocalYellowDataWhenPresent()
    {
        var file = FindRepoFileOrSkip("Host", "wwwroot", "data", "yellow", "data", "fumen", "tuning.bin");
        if (file is null)
        {
            return;
        }

        var stars = await YellowTuningLoader.LoadFromFileAsync(file, CancellationToken.None);

        Assert.NotEmpty(stars);
        Assert.Contains(stars.Values, star => star.Easy > 0 || star.Normal > 0 || star.Hard > 0 || star.Oni > 0 || star.Ura > 0);
    }

    [Fact]
    public void RequiredDataFiles_ThrowsYellowSpecificMessageForMissingFile()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}", "musicinfo.xml");

        var ex = Assert.Throws<FileNotFoundException>(() => YellowRequiredDataFiles.ThrowIfMissing([missingPath]));
        Assert.Contains("Yellow required game data file is missing", ex.Message, StringComparison.Ordinal);
        Assert.Equal(missingPath, ex.FileName);
    }

    [Fact]
    public async Task OptionalSidecars_ReturnEmptyOrDisabledWhenMissing()
    {
        var missingRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            var eventFolders = await YellowEventFolderLoader.LoadFromFileAsync(
                Path.Combine(missingRoot, YellowEventFolderLoader.FileName),
                new HashSet<uint> { 1 },
                CancellationToken.None);
            var telops = await YellowTelopLoader.LoadFromFileAsync(
                Path.Combine(missingRoot, YellowTelopLoader.FileName),
                CancellationToken.None);
            var itemShop = await YellowItemShopLoader.LoadFromFileAsync(
                Path.Combine(missingRoot, YellowItemShopLoader.FileName),
                new EraSettings { EnableShop = false },
                CancellationToken.None);

            Assert.Empty(eventFolders);
            Assert.Empty(telops);
            Assert.False(itemShop.IsEnabled);
        }
        finally
        {
            if (Directory.Exists(missingRoot))
            {
                Directory.Delete(missingRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task CatalogInitialize_LoadsRequiredAndOptionalDefaultsWhenLocalDataPresent()
    {
        if (FindRepoFileOrSkip("Host", "wwwroot", "data", "yellow", "data", "config", "ST9100-1", "musicinfo.xml") is null
            || FindRepoFileOrSkip("Host", "wwwroot", "data", "yellow", "data", "config", "ST9100-1", "musicmedleyinfo.xml") is null
            || FindRepoFileOrSkip("Host", "wwwroot", "data", "yellow", "data", "config", "ST9100-1", "defmusic.bin") is null
            || FindRepoFileOrSkip("Host", "wwwroot", "data", "yellow", "data", "fumen", "tuning.bin") is null)
        {
            return;
        }

        CopyYellowCatalogFilesToProcessRoot();
        DeleteYellowOptionalSidecarsFromProcessRoot();
        var logger = new RecordingLogger<YellowEraGameDataCatalog>();
        var settings = Options.Create(new ServerSettings
        {
            Eras = new Dictionary<string, EraSettings>
            {
                [nameof(GameEra.Yellow)] = new()
                {
                    Enabled = true,
                    EnableShop = false,
                    AutoExtractCatalog = false,
                    GameDataPath = "wwwroot/data/yellow/data",
                    CustomizationNameDataPath = string.Empty
                }
            }
        });
        var catalog = new YellowEraGameDataCatalog(logger, settings);

        await catalog.InitializeAsync(CancellationToken.None);

        Assert.Equal(GameEra.Yellow, catalog.Era);
        Assert.True(catalog.SongHashVersion > 0);
        Assert.NotEmpty(catalog.MusicInfoFileOrder);
        Assert.NotEmpty(catalog.YellowMusicInfos);
        Assert.NotEmpty(catalog.MusicInfos);
        Assert.NotEmpty(catalog.TaikojukuFileOrder);
        Assert.False(catalog.ItemShopCatalog.IsEnabled);
        Assert.Empty(catalog.EventFolders);
        Assert.Empty(catalog.Telops);
        Assert.Empty(catalog.Gachas);
        Assert.Empty(catalog.Tournaments);
        Assert.NotNull(catalog.Movies);
        Assert.Contains(logger.Events, log => log.Message.Contains("Loaded Yellow catalog", StringComparison.Ordinal));
    }

    private static string? FindRepoFileOrSkip(params string[] pathParts)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(pathParts).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static void CopyYellowCatalogFilesToProcessRoot()
    {
        Copy("config", "ST9100-1", "musicinfo.xml");
        Copy("config", "ST9100-1", "musicmedleyinfo.xml");
        Copy("config", "ST9100-1", "defmusic.bin");
        Copy("fumen", "tuning.bin");

        static void Copy(params string[] relativeParts)
        {
            var source = FindRepoFileOrSkip(["Host", "wwwroot", "data", "yellow", "data", .. relativeParts])
                ?? throw new FileNotFoundException(Path.Combine(relativeParts));
            var targetRoot = Path.Combine(
                Path.GetDirectoryName(Environment.ProcessPath)
                    ?? throw new ApplicationException("Cannot resolve process directory."),
                "wwwroot",
                "data",
                "yellow",
                "data");
            var destination = Path.Combine([targetRoot, .. relativeParts]);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)
                ?? throw new ApplicationException($"Cannot resolve directory for {destination}."));
            File.Copy(source, destination, overwrite: true);
        }
    }

    private static void DeleteYellowOptionalSidecarsFromProcessRoot()
    {
        var dataPath = GetProcessYellowDataPath();
        foreach (var fileName in OptionalSidecarFileNames)
        {
            File.Delete(Path.Combine(dataPath, fileName));
        }
    }

    private static string GetProcessYellowDataPath()
    {
        var processDirectory = Path.GetDirectoryName(Environment.ProcessPath)
            ?? throw new ApplicationException("Cannot resolve process directory.");
        return Path.Combine(processDirectory, "wwwroot", "data", "yellow");
    }

    private static readonly string[] OptionalSidecarFileNames =
    [
        YellowEventFolderLoader.FileName,
        YellowTelopLoader.FileName,
        YellowRecommendLoader.FileName,
        YellowItemShopLoader.FileName,
        YellowMovieLoader.FileName,
        YellowTaikojukuLoader.VerupFileName
    ];

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<LogEvent> Events { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Events.Add(new LogEvent(logLevel, formatter(state, exception)));
        }

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new();

            public void Dispose()
            {
            }
        }
    }

    private sealed record LogEvent(LogLevel Level, string Message);
}
