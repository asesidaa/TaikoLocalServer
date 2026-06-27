using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;
using TaikoLocalServer.Tests.Green;

namespace TaikoLocalServer.Tests.Blue;

[Collection(GreenRuntimeCatalogTestCollection.Name)]
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
        var verupFile = FindRepoFileOrSkip("Host", "wwwroot", "data", "blue", BlueTaikojukuLoader.VerupFileName);
        if (file is null || verupFile is null)
        {
            return;
        }

        var entries = await BlueTaikojukuLoader.LoadFromFileAsync(file, verupFile, CancellationToken.None);

        Assert.NotEmpty(entries);
        Assert.True(entries[0].UniqueId > 0);
        Assert.True(entries[0].ChallengeLevel > 0);
        Assert.Equal(5u, entries[0].VerupNo);
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

    [Fact]
    public async Task CatalogInitialize_ParsesBlueCustomizationSourcesAndUsesSharedNames()
    {
        if (FindRepoFileOrSkip("Host", "wwwroot", "data", "blue", "data", "config", "S10100-1", "musicinfo.xml") is null
            || FindRepoFileOrSkip("Host", "wwwroot", "data", "blue", "data", "config", "S10100-1", "musicmedleyinfo.xml") is null
            || FindRepoFileOrSkip("Host", "wwwroot", "data", "blue", "data", "fumen", "tuning.bin") is null)
        {
            return;
        }

        CopyBlueCatalogFilesToProcessRoot();
        DeleteBlueCustomizationFilesFromProcessRoot();
        var sharedSnapshot = SnapshotSharedCustomizationFiles();
        var gameDataRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        try
        {
            await CreateBlueCustomizationSourceAsync(gameDataRoot);
            await WriteSharedCustomizationNameFilesToProcessRoot(
                [new Costume { CostumeId = 1, CostumeType = "kigurumi", CostumeName = "Shared Kigurumi" }],
                [new Title { TitleId = 131, TitleName = "Shared Title", TitleRarity = 7 }],
                Enumerable.Range(0, 20)
                    .Select(id => new Neiro { NeiroId = (uint)id, NeiroName = $"Shared Tone {id}" })
                    .ToList());

            var catalog = new BlueEraGameDataCatalog(
                NullLogger<BlueEraGameDataCatalog>.Instance,
                Options.Create(new ServerSettings
                {
                    Eras = new Dictionary<string, EraSettings>
                    {
                        [nameof(GameEra.Blue)] = new()
                        {
                            Enabled = true,
                            AutoExtractCatalog = true,
                            EnableShop = false,
                            GameDataPath = gameDataRoot
                        }
                    }
                }));

            await catalog.InitializeAsync(CancellationToken.None);

            Assert.Equal(
                "Shared Kigurumi",
                Assert.Single(catalog.GetCostumeList(), costume => costume.CostumeType == "kigurumi" && costume.CostumeId == 1).CostumeName);
            Assert.Equal("Shared Title", catalog.GetTitleDictionary()[131].TitleName);
            Assert.Equal(7u, catalog.GetTitleDictionary()[131].TitleRarity);
            Assert.Equal(Enumerable.Range(0, 20).Select(id => (uint)id), catalog.GetNeiroDictionary().Keys.OrderBy(id => id));
            Assert.Equal("Shared Tone 19", catalog.GetNeiroDictionary()[19].NeiroName);
        }
        finally
        {
            if (Directory.Exists(gameDataRoot))
            {
                Directory.Delete(gameDataRoot, recursive: true);
            }

            DeleteBlueCustomizationFilesFromProcessRoot();
            RestoreSharedCustomizationFiles(sharedSnapshot);
        }
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

    private static async Task CreateBlueCustomizationSourceAsync(string gameDataRoot)
    {
        var fullCosDirectory = Path.Combine(gameDataRoot, "don3d", "full", "cos");
        Directory.CreateDirectory(fullCosDirectory);
        await File.WriteAllBytesAsync(Path.Combine(fullCosDirectory, "cos_001000.nud"), [0]);
        await File.WriteAllBytesAsync(Path.Combine(fullCosDirectory, "cos_001000.nut"), [0]);

        var titleDirectory = Path.Combine(gameDataRoot, "nutdata", "S10100-1", "appendable", "00", "title_name");
        Directory.CreateDirectory(titleDirectory);
        await File.WriteAllBytesAsync(Path.Combine(titleDirectory, "title_name_00131_00131.nut"), [0]);
    }

    private static void DeleteBlueCustomizationFilesFromProcessRoot()
    {
        var dataPath = GetProcessBlueDataPath();
        File.Delete(Path.Combine(dataPath, BlueCostumeLoader.FileName));
        File.Delete(Path.Combine(dataPath, BlueTitleLoader.FileName));
        File.Delete(Path.Combine(dataPath, BlueNeiroLoader.FileName));
    }

    private static Task WriteSharedCustomizationNameFilesToProcessRoot(
        IReadOnlyList<Costume> costumes,
        IReadOnlyList<Title> titles,
        IReadOnlyList<Neiro> neiros)
    {
        var dataPath = GetProcessSharedDataPath();
        Directory.CreateDirectory(dataPath);
        return Task.WhenAll(
            WriteEnvelopeAsync(Path.Combine(dataPath, "costume_name_data.json"), costumes),
            WriteEnvelopeAsync(Path.Combine(dataPath, "title_name_data.json"), titles),
            WriteEnvelopeAsync(Path.Combine(dataPath, "neiro_name_data.json"), neiros));
    }

    private static Task WriteEnvelopeAsync<T>(string path, IReadOnlyList<T> items)
        => File.WriteAllTextAsync(path, JsonSerializer.Serialize(new
        {
            schemaVersion = 1,
            items
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

    private static IReadOnlyDictionary<string, byte[]?> SnapshotSharedCustomizationFiles()
    {
        var dataPath = GetProcessSharedDataPath();
        var result = new Dictionary<string, byte[]?>();
        foreach (var fileName in SharedCustomizationFileNames)
        {
            var path = Path.Combine(dataPath, fileName);
            result[path] = File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        return result;
    }

    private static void RestoreSharedCustomizationFiles(IReadOnlyDictionary<string, byte[]?> snapshot)
    {
        foreach (var (path, bytes) in snapshot)
        {
            if (bytes is null)
            {
                File.Delete(path);
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path)
                ?? throw new ApplicationException($"Cannot resolve directory for {path}."));
            File.WriteAllBytes(path, bytes);
        }
    }

    private static string GetProcessBlueDataPath()
    {
        var processDirectory = Path.GetDirectoryName(Environment.ProcessPath)
            ?? throw new ApplicationException("Cannot resolve process directory.");
        return Path.Combine(processDirectory, "wwwroot", "data", "blue");
    }

    private static string GetProcessSharedDataPath()
    {
        var processDirectory = Path.GetDirectoryName(Environment.ProcessPath)
            ?? throw new ApplicationException("Cannot resolve process directory.");
        return Path.Combine(processDirectory, "wwwroot", "data", "shared");
    }

    private static readonly string[] SharedCustomizationFileNames =
    [
        "costume_name_data.json",
        "title_name_data.json",
        "neiro_name_data.json"
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

[CollectionDefinition("Blue runtime catalog tests")]
public sealed class BlueRuntimeCatalogTestCollection;
