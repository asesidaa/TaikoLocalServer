using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Kimidori;

namespace TaikoLocalServer.Tests.Kimidori;

public sealed class KimidoriCatalogLoaderTests
{
    [Fact]
    public async Task CatalogInitialize_DoesNotWarnForDaniMedleyMusicInfoRowsMissingTuning()
    {
        CopyKimidoriCatalogFilesToProcessRoot();
        var logger = new RecordingLogger<KimidoriEraGameDataCatalog>();
        var catalog = new KimidoriEraGameDataCatalog(logger);

        await catalog.InitializeAsync(CancellationToken.None);

        Assert.DoesNotContain(
            logger.Events,
            log => log.Level == LogLevel.Warning
                && log.Message.Contains("musicinfo entries have no tuning record", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CatalogInitialize_BuildsSongHashTableFromMusicInfoFileOrder()
    {
        CopyKimidoriCatalogFilesToProcessRoot();
        var logger = new RecordingLogger<KimidoriEraGameDataCatalog>();
        var catalog = new KimidoriEraGameDataCatalog(logger);

        await catalog.InitializeAsync(CancellationToken.None);

        Assert.Equal(427, catalog.SongHashTable.Count);
        Assert.Equal([236, 234, 128, 199, 5], catalog.SongHashTable.Take(5).Select(value => (int)value).ToArray());
        Assert.Equal(20015, catalog.SongHashTable[^1]);
        Assert.Equal([0x00, 0xec, 0x00, 0xea], Ac15SongHashCodec.EncodeTable(catalog.SongHashTable)[..4]);
    }

    [Fact]
    public async Task CatalogInitialize_ReextractsPackedTitlesWhenExistingTitleCatalogIsEmpty()
    {
        CopyKimidoriCatalogFilesToProcessRoot();
        WriteEmptyCustomizationCatalogsToProcessRoot();
        var repoRoot = FindRepoRoot();
        var logger = new RecordingLogger<KimidoriEraGameDataCatalog>();
        var settings = Options.Create(new ServerSettings
        {
            Eras =
            {
                [nameof(GameEra.Kimidori)] = new EraSettings
                {
                    Enabled = true,
                    AutoExtractCatalog = true,
                    GameDataPath = Path.Combine(repoRoot, "Host", "wwwroot", "data", "kimidori", "data")
                }
            }
        });
        var catalog = new KimidoriEraGameDataCatalog(logger, settings);

        await catalog.InitializeAsync(CancellationToken.None);

        Assert.Contains(320u, catalog.GetTitleDictionary().Keys);
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

    private static void CopyKimidoriCatalogFilesToProcessRoot()
    {
        var repoRoot = FindRepoRoot();
        var targetRoot = Path.Combine(
            Path.GetDirectoryName(Environment.ProcessPath)
                ?? throw new ApplicationException("Cannot resolve process directory."),
            "wwwroot",
            "data",
            "kimidori");

        Copy(
            Path.Combine(repoRoot, "Host", "wwwroot", "data", "kimidori", "data", "musicinfo.xml"),
            Path.Combine(targetRoot, "data", "musicinfo.xml"));
        Copy(
            Path.Combine(repoRoot, "Host", "wwwroot", "data", "kimidori", "data", "musicmedleyinfo.xml"),
            Path.Combine(targetRoot, "data", "musicmedleyinfo.xml"));
        Copy(
            Path.Combine(repoRoot, "Host", "wwwroot", "data", "kimidori", "data", "defmusic.bin"),
            Path.Combine(targetRoot, "data", "defmusic.bin"));
        Copy(
            Path.Combine(repoRoot, "Host", "wwwroot", "data", "kimidori", "data", "fumen", "tuning.bin"),
            Path.Combine(targetRoot, "data", "fumen", "tuning.bin"));
        Copy(
            Path.Combine(repoRoot, "Host", "wwwroot", "data", "kimidori", KimidoriEraGameDataCatalog.MovieFileName),
            Path.Combine(targetRoot, KimidoriEraGameDataCatalog.MovieFileName));

        Directory.CreateDirectory(Path.Combine(targetRoot, "data", "movie"));
    }

    private static void WriteEmptyCustomizationCatalogsToProcessRoot()
    {
        var targetRoot = Path.Combine(
            Path.GetDirectoryName(Environment.ProcessPath)
                ?? throw new ApplicationException("Cannot resolve process directory."),
            "wwwroot",
            "data",
            "kimidori");

        Directory.CreateDirectory(targetRoot);
        File.WriteAllText(
            Path.Combine(targetRoot, KimidoriEraGameDataCatalog.CostumeFileName),
            """{"schemaVersion":1,"items":[]}""");
        File.WriteAllText(
            Path.Combine(targetRoot, KimidoriEraGameDataCatalog.TitleFileName),
            """{"schemaVersion":1,"items":[]}""");
        File.WriteAllText(
            Path.Combine(targetRoot, KimidoriEraGameDataCatalog.NeiroFileName),
            """{"schemaVersion":1,"items":[]}""");
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
