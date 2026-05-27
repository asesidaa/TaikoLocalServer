using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

namespace TaikoLocalServer.Tests.Blue;

[Collection("Blue runtime catalog tests")]
public sealed class BlueCatalogLoaderTests
{
    [Fact]
    public async Task MusicInfoLoader_ReadsLocalBlueDataWhenPresent()
    {
        var file = FindRepoFileOrSkip("Host", "wwwroot", "data", "blue", "data", "config", "S10100-1", "musicinfo.xml");
        if (file is null)
        {
            return;
        }

        var result = await BlueMusicInfoLoader.LoadFromFileAsync(file, CancellationToken.None);

        Assert.True(result.SongHashVersion > 0);
        Assert.NotEmpty(result.Entries);
        Assert.Equal(0, result.Entries[0].FileOrder);
        Assert.True(result.Entries[0].SongNo > 0);
        Assert.NotEmpty(result.Entries[0].MusicId);
    }

    [Fact]
    public async Task TaikojukuLoader_ReadsLocalBlueDataWhenPresent()
    {
        var file = FindRepoFileOrSkip("Host", "wwwroot", "data", "blue", "data", "config", "S10100-1", "musicmedleyinfo.xml");
        if (file is null)
        {
            return;
        }

        var entries = await BlueTaikojukuLoader.LoadFromFileAsync(file, CancellationToken.None);

        Assert.NotEmpty(entries);
        Assert.True(entries[0].UniqueId > 0);
        Assert.True(entries[0].ChallengeLevel > 0);
        Assert.NotEmpty(entries[0].Songs);
    }

    [Fact]
    public async Task TuningLoader_ReadsLocalBlueDataWhenPresent()
    {
        var file = FindRepoFileOrSkip("Host", "wwwroot", "data", "blue", "data", "fumen", "tuning.bin");
        if (file is null)
        {
            return;
        }

        var stars = await BlueTuningLoader.LoadFromFileAsync(file, CancellationToken.None);

        Assert.NotEmpty(stars);
        Assert.Contains(stars.Values, star => star.Easy > 0 || star.Normal > 0 || star.Hard > 0 || star.Oni > 0 || star.Ura > 0);
    }

    [Fact]
    public void RequiredDataFiles_ThrowsBlueSpecificMessageForMissingFile()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}", "musicinfo.xml");

        var ex = Assert.Throws<FileNotFoundException>(() => BlueRequiredDataFiles.ThrowIfMissing([missingPath]));
        Assert.Contains("Blue required game data file is missing", ex.Message, StringComparison.Ordinal);
        Assert.Equal(missingPath, ex.FileName);
    }

    [Fact]
    public async Task CatalogInitialize_LoadsRequiredAndOptionalDefaultsWhenLocalDataPresent()
    {
        if (FindRepoFileOrSkip("Host", "wwwroot", "data", "blue", "data", "config", "S10100-1", "musicinfo.xml") is null
            || FindRepoFileOrSkip("Host", "wwwroot", "data", "blue", "data", "config", "S10100-1", "musicmedleyinfo.xml") is null
            || FindRepoFileOrSkip("Host", "wwwroot", "data", "blue", "data", "fumen", "tuning.bin") is null)
        {
            return;
        }

        CopyBlueCatalogFilesToProcessRoot();
        var logger = new RecordingLogger<BlueEraGameDataCatalog>();
        var settings = Options.Create(new ServerSettings
        {
            Eras = new Dictionary<string, EraSettings>
            {
                [nameof(GameEra.Blue)] = new()
                {
                    Enabled = true,
                    EnableShop = false,
                    AutoExtractCatalog = false,
                    GameDataPath = "wwwroot/data/blue/data",
                    CustomizationNameDataPath = string.Empty
                }
            }
        });
        var catalog = new BlueEraGameDataCatalog(logger, settings);

        await catalog.InitializeAsync(CancellationToken.None);

        Assert.Equal(GameEra.Blue, catalog.Era);
        Assert.True(catalog.SongHashVersion > 0);
        Assert.NotEmpty(catalog.MusicInfoFileOrder);
        Assert.NotEmpty(catalog.BlueMusicInfos);
        Assert.NotEmpty(catalog.MusicInfos);
        Assert.NotEmpty(catalog.TaikojukuFileOrder);
        Assert.False(catalog.ItemShopCatalog.IsEnabled);
        Assert.Empty(catalog.EventFolders);
        Assert.Empty(catalog.Telops);
        Assert.Empty(catalog.Gachas);
        Assert.Empty(catalog.Tournaments);
        Assert.NotNull(catalog.Movies);
        Assert.Contains(logger.Events, log => log.Message.Contains("Loaded Blue catalog", StringComparison.Ordinal));
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

    private static void CopyBlueCatalogFilesToProcessRoot()
    {
        Copy("config", "S10100-1", "musicinfo.xml");
        Copy("config", "S10100-1", "musicmedleyinfo.xml");
        Copy("fumen", "tuning.bin");

        static void Copy(params string[] relativeParts)
        {
            var source = FindRepoFileOrSkip(["Host", "wwwroot", "data", "blue", "data", .. relativeParts])
                ?? throw new FileNotFoundException(Path.Combine(relativeParts));
            var targetRoot = Path.Combine(
                Path.GetDirectoryName(Environment.ProcessPath)
                    ?? throw new ApplicationException("Cannot resolve process directory."),
                "wwwroot",
                "data",
                "blue",
                "data");
            var destination = Path.Combine([targetRoot, .. relativeParts]);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)
                ?? throw new ApplicationException($"Cannot resolve directory for {destination}."));
            File.Copy(source, destination, overwrite: true);
        }
    }

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

[CollectionDefinition("Blue runtime catalog tests")]
public sealed class BlueRuntimeCatalogTestCollection;
