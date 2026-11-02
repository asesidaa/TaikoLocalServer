using System.Buffers.Binary;
using System.Text;
using Microsoft.Extensions.Logging;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

namespace TaikoLocalServer.Tests.Green;

[Collection(GreenRuntimeCatalogTestCollection.Name)]
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
        var verupFile = Path.Combine(repoRoot, "Host", "wwwroot", "data", "green", GreenTaikojukuLoader.VerupFileName);

        var entries = await GreenTaikojukuLoader.LoadFromFileAsync(file, verupFile, CancellationToken.None);

        Assert.NotEmpty(entries);
        Assert.True(entries[0].UniqueId > 0);
        Assert.True(entries[0].ChallengeLevel > 0);
        Assert.Equal(5u, entries[0].VerupNo);
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
    public async Task TuningLoader_ReadsGeneratedFixtureWithNonStockSongCount()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.bin");
        await File.WriteAllBytesAsync(
            tempFile,
            CreateTuningBin(
                new TuningTestRecord("modsong", 3, 4, 5, 6),
                new TuningTestRecord("ex_modsong", 0, 0, 0, 9)),
            CancellationToken.None);

        try
        {
            await AssertNonStockTuningFixtureAsync(
                tempFile,
                songCount: 2,
                baseRecordCount: 1,
                musicId: "modsong",
                expectedStars: new Ac15StarSet(3, 4, 5, 6, 9));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task TuningLoader_ReadsLocalModdedFixtureWhenPresent()
    {
        var repoRoot = FindRepoRoot();
        var file = Path.Combine(repoRoot, ".tools", "tuning.bin");
        if (!File.Exists(file))
        {
            return;
        }

        await AssertNonStockTuningFixtureAsync(
            file,
            songCount: 1_211,
            baseRecordCount: 1_054,
            musicId: "kaibu2",
            expectedStars: new Ac15StarSet(0, 0, 0, 10, 0));
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

    private static async Task AssertNonStockTuningFixtureAsync(
        string file,
        uint songCount,
        int baseRecordCount,
        string musicId,
        Ac15StarSet expectedStars)
    {
        var bytes = await File.ReadAllBytesAsync(file, CancellationToken.None);
        var actualSongCount = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(0, 4));
        Assert.Equal(songCount, actualSongCount);
        Assert.NotEqual(1_210u, actualSongCount);

        var stars = await GreenTuningLoader.LoadFromFileAsync(file, CancellationToken.None);

        Assert.Equal(baseRecordCount, stars.Count);
        Assert.True(stars.TryGetValue(musicId, out var starSet));
        Assert.Equal(expectedStars.Easy, starSet.Easy);
        Assert.Equal(expectedStars.Normal, starSet.Normal);
        Assert.Equal(expectedStars.Hard, starSet.Hard);
        Assert.Equal(expectedStars.Oni, starSet.Oni);
        Assert.Equal(expectedStars.Ura, starSet.Ura);
    }

    private static byte[] CreateTuningBin(params TuningTestRecord[] records)
    {
        const int headerSize = 4;
        const int recordSize = 2_316;
        const int player0CellOffset = 0x10;
        const int difficultyCellStride = 0x80;

        var stringTableOffset = headerSize + records.Length * recordSize;
        var stringTableLength = records.Sum(record => Encoding.ASCII.GetByteCount(record.MusicId) + 1);
        var bytes = new byte[stringTableOffset + stringTableLength];

        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(0, 4), (uint)records.Length);

        var musicIdOffset = 0;
        for (var index = 0; index < records.Length; index++)
        {
            var record = records[index];
            var recordOffset = headerSize + index * recordSize;
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(recordOffset, 4), (uint)musicIdOffset);
            WriteStar(bytes, recordOffset, 0, record.Easy);
            WriteStar(bytes, recordOffset, 1, record.Normal);
            WriteStar(bytes, recordOffset, 2, record.Hard);
            WriteStar(bytes, recordOffset, 3, record.Oni);

            var musicIdBytes = Encoding.ASCII.GetBytes(record.MusicId);
            musicIdBytes.CopyTo(bytes.AsSpan(stringTableOffset + musicIdOffset));
            musicIdOffset += musicIdBytes.Length + 1;
        }

        return bytes;

        static void WriteStar(byte[] bytes, int recordOffset, int difficultyIndex, byte value)
        {
            BinaryPrimitives.WriteUInt32BigEndian(
                bytes.AsSpan(recordOffset + player0CellOffset + difficultyIndex * difficultyCellStride, 4),
                value);
        }
    }

    private sealed record TuningTestRecord(
        string MusicId,
        byte Easy,
        byte Normal,
        byte Hard,
        byte Oni);

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
