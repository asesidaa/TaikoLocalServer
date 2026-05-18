using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Output;

namespace TaikoLocalServer.Tests.Green;

[Collection(GreenRuntimeCatalogTestCollection.Name)]
public sealed class GreenCustomizationCatalogLoaderTests
{
    [Fact]
    public void Composer_UsesNijiiroNamesForGreenTitleAndCostumeSlice()
    {
        var greenCostumes = new List<Costume>
        {
            new() { CostumeId = 1, CostumeType = "body", CostumeName = string.Empty },
            new() { CostumeId = 1, CostumeType = "unknown", CostumeName = string.Empty },
            new() { CostumeId = 999, CostumeType = "unknown", CostumeName = "Green Unknown" }
        };
        var greenTitles = new Dictionary<uint, Title>
        {
            [10] = new() { TitleId = 10, TitleName = string.Empty },
            [11] = new() { TitleId = 11, TitleName = "Green Fallback" }
        };
        var greenNeiros = new Dictionary<uint, Neiro>();
        var nijiiroCostumes = new List<Costume>
        {
            new() { CostumeId = 1, CostumeType = "body", CostumeName = "Nijiiro Body" },
            new() { CostumeId = 200, CostumeType = "body", CostumeName = "Nijiiro Only Body" }
        };
        var nijiiroTitles = new Dictionary<uint, Title>
        {
            [10] = new() { TitleId = 10, TitleName = "Nijiiro Title" },
            [12] = new() { TitleId = 12, TitleName = "Nijiiro Only Title" }
        };
        var nijiiroNeiros = new Dictionary<uint, Neiro>();

        var catalog = GreenCustomizationCatalogComposer.Compose(
            greenCostumes,
            greenTitles,
            greenNeiros,
            nijiiroCostumes,
            nijiiroTitles,
            nijiiroNeiros);

        Assert.Equal("Nijiiro Body", Assert.Single(catalog.Costumes, item => item.CostumeType == "body" && item.CostumeId == 1).CostumeName);
        Assert.DoesNotContain(catalog.Costumes, item => item.CostumeType == "body" && item.CostumeId == 200);
        Assert.DoesNotContain(catalog.Costumes, item => item.CostumeType == "unknown" && item.CostumeId == 1);
        Assert.Contains(catalog.Costumes, item => item.CostumeType == "unknown" && item.CostumeId == 999);
        Assert.Equal("Nijiiro Title", catalog.Titles[10].TitleName);
        Assert.Equal("Green Fallback", catalog.Titles[11].TitleName);
        Assert.False(catalog.Titles.ContainsKey(12));
    }

    [Fact]
    public void Composer_UsesNijiiroBaseToneRangeWhenGreenToneNamesAreIncomplete()
    {
        var greenNeiros = Enumerable.Range(0, 16)
            .Select(id => new Neiro { NeiroId = (uint)id, NeiroName = $"Green {id}" })
            .ToDictionary(neiro => neiro.NeiroId);
        var nijiiroNeiros = Enumerable.Range(0, 20)
            .Select(id => new Neiro { NeiroId = (uint)id, NeiroName = $"Nijiiro {id}" })
            .ToDictionary(neiro => neiro.NeiroId);

        var catalog = GreenCustomizationCatalogComposer.Compose(
            [],
            new Dictionary<uint, Title>(),
            greenNeiros,
            [],
            new Dictionary<uint, Title>(),
            nijiiroNeiros);

        Assert.Equal(Enumerable.Range(0, 20).Select(id => (uint)id), catalog.Neiros.Keys.OrderBy(id => id));
        Assert.Equal("Nijiiro 19", catalog.Neiros[19].NeiroName);
    }

    [Fact]
    public async Task FileCatalogInitialize_InitializesNijiiroBeforeGreenSoSharedCatalogIsAvailable()
    {
        var nijiiroInitialized = false;
        var green = new OrderingCatalog(GameEra.Green, () =>
        {
            Assert.True(nijiiroInitialized);
            return Task.CompletedTask;
        });
        var nijiiro = new OrderingCatalog(GameEra.Nijiiro, () =>
        {
            nijiiroInitialized = true;
            return Task.CompletedTask;
        });
        var catalog = new FileGameDataCatalog([green, nijiiro]);

        await catalog.InitializeAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CostumeLoader_MissingFileReturnsEmptyList()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");

        var items = await GreenCostumeLoader.LoadFromFileAsync(path, CancellationToken.None);

        Assert.Empty(items);
    }

    [Fact]
    public async Task TitleLoader_ReadsEnvelopeAsDictionary()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        await WriteEnvelopeAsync(path,
        [
            new Title { TitleId = 132, TitleName = "B" },
            new Title { TitleId = 131, TitleName = "A" }
        ]);

        var items = await GreenTitleLoader.LoadFromFileAsync(path, CancellationToken.None);

        Assert.Equal(new uint[] { 131, 132 }, items.Keys.OrderBy(id => id));
        File.Delete(path);
    }

    [Fact]
    public async Task NeiroLoader_ReadsEnvelopeAsDictionary()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        await WriteEnvelopeAsync(path,
        [
            new Neiro { NeiroId = 4, NeiroName = "Tone" }
        ]);

        var items = await GreenNeiroLoader.LoadFromFileAsync(path, CancellationToken.None);

        Assert.Equal("Tone", items[4].NeiroName);
        File.Delete(path);
    }

    [Fact]
    public async Task TitleLoader_DuplicateIdsUseFirstItem()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        try
        {
            await WriteEnvelopeAsync(path,
            [
                new Title { TitleId = 131, TitleName = "A" },
                new Title { TitleId = 131, TitleName = "B" }
            ]);

            var items = await GreenTitleLoader.LoadFromFileAsync(path, CancellationToken.None);

            var item = Assert.Single(items);
            Assert.Equal(131u, item.Key);
            Assert.Equal("A", item.Value.TitleName);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task NeiroLoader_DuplicateIdsUseFirstItem()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        try
        {
            await WriteEnvelopeAsync(path,
            [
                new Neiro { NeiroId = 4, NeiroName = "Tone A" },
                new Neiro { NeiroId = 4, NeiroName = "Tone B" }
            ]);

            var items = await GreenNeiroLoader.LoadFromFileAsync(path, CancellationToken.None);

            var item = Assert.Single(items);
            Assert.Equal(4u, item.Key);
            Assert.Equal("Tone A", item.Value.NeiroName);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task CatalogInitialize_MalformedRewardTitleFilteringXmlDoesNotStopStartup()
    {
        CopyGreenRuntimeCatalogFilesToProcessRoot();
        DeleteGreenCustomizationFilesFromProcessRoot();

        var gameDataRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(gameDataRoot, "nutdata", "cos_name"));
            Directory.CreateDirectory(Path.Combine(gameDataRoot, "nutdata", "title_name"));
            Directory.CreateDirectory(Path.Combine(gameDataRoot, "nutdata", "tone_name"));
            Directory.CreateDirectory(Path.Combine(gameDataRoot, "config", "S11100-1"));

            await File.WriteAllBytesAsync(Path.Combine(gameDataRoot, "nutdata", "cos_name", "nutdatapack.ndp"), BuildNdp(("cos_name_001.nut", 0, 1)));
            await File.WriteAllBytesAsync(Path.Combine(gameDataRoot, "nutdata", "title_name", "nutdatapack.ndp"), BuildNdp(("title_name_131.nut", 0, 1)));
            await File.WriteAllBytesAsync(Path.Combine(gameDataRoot, "nutdata", "tone_name", "nutdatapack.ndp"), BuildNdp(("tone_name_004.nut", 0, 1)));
            await File.WriteAllTextAsync(Path.Combine(gameDataRoot, "config", "S11100-1", "rewardtitlefiltering.xml"), "<boost_serialization>");

            var settings = Options.Create(new ServerSettings
            {
                Eras = new Dictionary<string, EraSettings>
                {
                    [nameof(GameEra.Green)] = new()
                    {
                        Enabled = true,
                        AutoExtractCatalog = true,
                        GameDataPath = gameDataRoot
                    }
                }
            });
            var logger = new RecordingLogger<GreenEraGameDataCatalog>();
            var catalog = new GreenEraGameDataCatalog(logger, settings);

            await catalog.InitializeAsync(CancellationToken.None);

            Assert.Empty(catalog.GetCostumeList());
            Assert.Empty(catalog.GetTitleDictionary());
            Assert.Empty(catalog.GetNeiroDictionary());
            Assert.Contains(
                logger.Events,
                log => log.Level == LogLevel.Warning
                    && log.Message.Contains("customization catalog extraction failed", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (Directory.Exists(gameDataRoot))
            {
                Directory.Delete(gameDataRoot, recursive: true);
            }

            DeleteGreenCustomizationFilesFromProcessRoot();
        }
    }

    [Fact]
    public async Task CatalogInitialize_PublishesOnlyMissingGeneratedCustomizationFiles()
    {
        CopyGreenRuntimeCatalogFilesToProcessRoot();
        DeleteGreenCustomizationFilesFromProcessRoot();

        var existingCostumePath = Path.Combine(GetProcessGreenDataPath(), GreenCatalogExtractor.CostumeFileName);
        Directory.CreateDirectory(Path.GetDirectoryName(existingCostumePath)
            ?? throw new ApplicationException("Cannot resolve Green data directory."));
        await WriteEnvelopeAsync(existingCostumePath,
        [
            new Costume { CostumeId = 777, CostumeType = "operator", CostumeName = "Operator Costume" }
        ]);

        var gameDataRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            await CreateGreenExtractionSourceAsync(gameDataRoot, includeRewardTitleFiltering: true);

            var catalog = new GreenEraGameDataCatalog(
                NullLogger<GreenEraGameDataCatalog>.Instance,
                CreateGreenSettings(gameDataRoot, autoExtractCatalog: true));

            await catalog.InitializeAsync(CancellationToken.None);

            var costumes = await GreenCostumeLoader.LoadFromFileAsync(existingCostumePath, CancellationToken.None);
            Assert.Equal(777u, Assert.Single(costumes).CostumeId);
            Assert.True(File.Exists(Path.Combine(GetProcessGreenDataPath(), GreenCatalogExtractor.TitleFileName)));
            Assert.True(File.Exists(Path.Combine(GetProcessGreenDataPath(), GreenCatalogExtractor.NeiroFileName)));
            Assert.True(catalog.GetTitleDictionary().ContainsKey(131));
            Assert.True(catalog.GetNeiroDictionary().ContainsKey(4));
        }
        finally
        {
            if (Directory.Exists(gameDataRoot))
            {
                Directory.Delete(gameDataRoot, recursive: true);
            }

            DeleteGreenCustomizationFilesFromProcessRoot();
        }
    }

    [Fact]
    public async Task CatalogInitialize_AutoExtractDisabledDoesNotPublishMissingCustomizationFiles()
    {
        CopyGreenRuntimeCatalogFilesToProcessRoot();
        DeleteGreenCustomizationFilesFromProcessRoot();

        var gameDataRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            await CreateGreenExtractionSourceAsync(gameDataRoot, includeRewardTitleFiltering: true);

            var catalog = new GreenEraGameDataCatalog(
                NullLogger<GreenEraGameDataCatalog>.Instance,
                CreateGreenSettings(gameDataRoot, autoExtractCatalog: false));

            await catalog.InitializeAsync(CancellationToken.None);

            Assert.False(File.Exists(Path.Combine(GetProcessGreenDataPath(), GreenCatalogExtractor.CostumeFileName)));
            Assert.False(File.Exists(Path.Combine(GetProcessGreenDataPath(), GreenCatalogExtractor.TitleFileName)));
            Assert.False(File.Exists(Path.Combine(GetProcessGreenDataPath(), GreenCatalogExtractor.NeiroFileName)));
            Assert.Empty(catalog.GetCostumeList());
            Assert.Empty(catalog.GetTitleDictionary());
            Assert.Empty(catalog.GetNeiroDictionary());
        }
        finally
        {
            if (Directory.Exists(gameDataRoot))
            {
                Directory.Delete(gameDataRoot, recursive: true);
            }

            DeleteGreenCustomizationFilesFromProcessRoot();
        }
    }

    private static async Task CreateGreenExtractionSourceAsync(
        string gameDataRoot,
        bool includeRewardTitleFiltering)
    {
        Directory.CreateDirectory(Path.Combine(gameDataRoot, "nutdata", "cos_name"));
        Directory.CreateDirectory(Path.Combine(gameDataRoot, "nutdata", "title_name"));
        Directory.CreateDirectory(Path.Combine(gameDataRoot, "nutdata", "tone_name"));

        await File.WriteAllBytesAsync(Path.Combine(gameDataRoot, "nutdata", "cos_name", "nutdatapack.ndp"), BuildNdp(("cos_name_001.nut", 0, 1)));
        await File.WriteAllBytesAsync(Path.Combine(gameDataRoot, "nutdata", "title_name", "nutdatapack.ndp"), BuildNdp(("title_name_131.nut", 0, 1)));
        await File.WriteAllBytesAsync(Path.Combine(gameDataRoot, "nutdata", "tone_name", "nutdatapack.ndp"), BuildNdp(("tone_name_004.nut", 0, 1)));

        if (includeRewardTitleFiltering)
        {
            Directory.CreateDirectory(Path.Combine(gameDataRoot, "config", "S11100-1"));
            await File.WriteAllTextAsync(Path.Combine(gameDataRoot, "config", "S11100-1", "rewardtitlefiltering.xml"), """
                <boost_serialization>
                  <RewardTitleFiltering>
                    <support>
                      <rewardtitle>131</rewardtitle>
                    </support>
                  </RewardTitleFiltering>
                </boost_serialization>
                """);
        }
    }

    private static IOptions<ServerSettings> CreateGreenSettings(string gameDataRoot, bool autoExtractCatalog)
        => Options.Create(new ServerSettings
        {
            Eras = new Dictionary<string, EraSettings>
            {
                [nameof(GameEra.Green)] = new()
                {
                    Enabled = true,
                    AutoExtractCatalog = autoExtractCatalog,
                    GameDataPath = gameDataRoot
                }
            }
        });

    private static Task WriteEnvelopeAsync<T>(string path, IReadOnlyList<T> items)
        => File.WriteAllTextAsync(path, JsonSerializer.Serialize(new GreenCatalogEnvelope<T>
        {
            Items = items
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

    private static void CopyGreenRuntimeCatalogFilesToProcessRoot()
    {
        var repoRoot = FindRepoRoot();
        var sourceRoot = Path.Combine(repoRoot, "Host", "wwwroot", "data", "green", "data");
        var targetRoot = Path.Combine(GetProcessGreenDataPath(), "data");

        Copy(
            Path.Combine(sourceRoot, "config", "S11100-1", "musicinfo.xml"),
            Path.Combine(targetRoot, "config", "S11100-1", "musicinfo.xml"));
        Copy(
            Path.Combine(sourceRoot, "config", "S11100-1", "musicmedleyinfo.xml"),
            Path.Combine(targetRoot, "config", "S11100-1", "musicmedleyinfo.xml"));
        Copy(
            Path.Combine(sourceRoot, "fumen", "tuning.bin"),
            Path.Combine(targetRoot, "fumen", "tuning.bin"));
    }

    private static void DeleteGreenCustomizationFilesFromProcessRoot()
    {
        var dataPath = GetProcessGreenDataPath();
        File.Delete(Path.Combine(dataPath, GreenCatalogExtractor.CostumeFileName));
        File.Delete(Path.Combine(dataPath, GreenCatalogExtractor.TitleFileName));
        File.Delete(Path.Combine(dataPath, GreenCatalogExtractor.NeiroFileName));
    }

    private static string GetProcessGreenDataPath()
    {
        var processDirectory = Path.GetDirectoryName(Environment.ProcessPath)
            ?? throw new ApplicationException("Cannot resolve process directory.");
        return Path.Combine(processDirectory, "wwwroot", "data", "green");
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

    private static void Copy(string source, string destination)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination)
            ?? throw new ApplicationException($"Cannot resolve directory for {destination}."));
        File.Copy(source, destination, overwrite: true);
    }

    private static byte[] BuildNdp(params (string Name, uint Offset, uint Size)[] entries)
    {
        var bytes = new byte[0x50 + entries.Sum(entry => 4 + Align4(Encoding.ASCII.GetByteCount(entry.Name) + 1) + 8)];
        Encoding.ASCII.GetBytes("NUT_PACK_TYPE1").CopyTo(bytes, 0);
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(0x40), (uint)entries.Length);
        var cursor = 0x50;

        foreach (var entry in entries)
        {
            var nameBytes = Encoding.ASCII.GetBytes(entry.Name);
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(cursor), (uint)(nameBytes.Length + 1));
            cursor += 4;
            nameBytes.CopyTo(bytes.AsSpan(cursor));
            cursor += nameBytes.Length + 1;
            cursor = Align4(cursor);
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(cursor), entry.Offset);
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(cursor + 4), entry.Size);
            cursor += 8;
        }

        return bytes;
    }

    private static int Align4(int value) => (value + 3) & ~3;

    private sealed class OrderingCatalog(GameEra era, Func<Task> initialize) : IEraGameDataCatalog
    {
        public GameEra Era => era;

        public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos { get; } =
            new Dictionary<uint, IMusicInfoEntry>();

        public Task InitializeAsync(CancellationToken cancellationToken) => initialize();
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
