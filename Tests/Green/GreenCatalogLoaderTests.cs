using Microsoft.Extensions.Logging;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenCatalogLoaderTests
{
    [Fact]
    public async Task MusicInfoLoader_ReadsVersionAndFileOrderSongs()
    {
        var repoRoot = FindRepoRoot();
        var file = Path.Combine(repoRoot, "Host", "wwwroot", "data", "green", "data", "config", "S11100-1", "musicinfo.xml");

        var result = await GreenMusicInfoLoader.LoadFromFileAsync(file, CancellationToken.None);

        Assert.True(result.SongHashVersion > 0);
        Assert.True(result.Entries.Count >= 20);
        Assert.Equal((uint)873, result.Entries[0].SongNo);
        Assert.Equal("ynzums", result.Entries[0].MusicId);
        Assert.Equal(0, result.Entries[0].FileOrder);
    }

    [Fact]
    public async Task TaikojukuLoader_ReadsMedleyPacks()
    {
        var repoRoot = FindRepoRoot();
        var file = Path.Combine(repoRoot, "Host", "wwwroot", "data", "green", "data", "config", "S11100-1", "musicmedleyinfo.xml");

        var entries = await GreenTaikojukuLoader.LoadFromFileAsync(file, CancellationToken.None);

        Assert.NotEmpty(entries);
        Assert.True(entries[0].UniqueId > 0);
        Assert.True(entries[0].ChallengeLevel > 0);
        Assert.NotEmpty(entries[0].Songs);
        Assert.True(entries[0].Songs[0].SongNo > 0);
        Assert.Equal(90u, entries[0].Conditions.SoulGauge);
        Assert.Equal(420u, entries[0].Conditions.TotalHitCount);
        Assert.Equal(95u, entries[0].ExcellentConditions.SoulGauge);
        Assert.Equal(460u, entries[0].ExcellentConditions.TotalHitCount);
    }

    [Fact]
    public async Task TuningLoader_ReadsExRecordUraStarsWithoutRejectingUnavailableCourses()
    {
        var repoRoot = FindRepoRoot();
        var file = Path.Combine(repoRoot, "Host", "wwwroot", "data", "green", "data", "fumen", "tuning.bin");

        var stars = await GreenTuningLoader.LoadFromFileAsync(file, CancellationToken.None);

        Assert.True(stars.TryGetValue("2ge8ji", out var starSet));
        Assert.Equal((byte)2, starSet.Easy);
        Assert.Equal((byte)3, starSet.Normal);
        Assert.Equal((byte)3, starSet.Hard);
        Assert.Equal((byte)6, starSet.Oni);
        Assert.Equal((byte)1, starSet.Ura);
    }

    [Fact]
    public async Task CatalogInitialize_DoesNotWarnForMedleyMusicInfoRowsMissingTuning()
    {
        CopyGreenCatalogFilesToProcessRoot();
        var logger = new RecordingLogger<GreenEraGameDataCatalog>();
        var catalog = new GreenEraGameDataCatalog(logger);

        await catalog.InitializeAsync(CancellationToken.None);

        Assert.DoesNotContain(
            logger.Events,
            log => log.Level == LogLevel.Warning
                && log.Message.Contains("musicinfo entries have no tuning record", StringComparison.Ordinal));
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate repository root.");
    }

    private static void CopyGreenCatalogFilesToProcessRoot()
    {
        var repoRoot = FindRepoRoot();
        var targetRoot = Path.Combine(
            Path.GetDirectoryName(Environment.ProcessPath)
                ?? throw new ApplicationException("Cannot resolve process directory."),
            "wwwroot",
            "data",
            "green");

        Copy(
            Path.Combine(repoRoot, "Host", "wwwroot", "data", "green", "data", "config", "S11100-1", "musicinfo.xml"),
            Path.Combine(targetRoot, "data", "config", "S11100-1", "musicinfo.xml"));
        Copy(
            Path.Combine(repoRoot, "Host", "wwwroot", "data", "green", "data", "config", "S11100-1", "musicmedleyinfo.xml"),
            Path.Combine(targetRoot, "data", "config", "S11100-1", "musicmedleyinfo.xml"));
        Copy(
            Path.Combine(repoRoot, "Host", "wwwroot", "data", "green", "data", "fumen", "tuning.bin"),
            Path.Combine(targetRoot, "data", "fumen", "tuning.bin"));
    }

    private static void Copy(string source, string destination)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination)
            ?? throw new ApplicationException($"Cannot resolve directory for {destination}."));
        File.Copy(source, destination, overwrite: true);
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
