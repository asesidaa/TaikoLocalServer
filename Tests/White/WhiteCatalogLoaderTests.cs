using System.Runtime.CompilerServices;
using System.Text.Json;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;
using TaikoLocalServer.Infrastructure.GameDataCatalog.White;

namespace TaikoLocalServer.Tests.White;

public sealed class WhiteCatalogLoaderTests
{
    [Fact]
    public async Task MusicInfoLoader_ReadsLocalActiveWhiteDataWhenPresent()
    {
        var file = FindRepoFileOrSkip("Host", "wwwroot", "data", "white", "data", "config", "ST7100-1", "musicinfo.xml");
        if (file is null)
        {
            return;
        }

        var result = await Ac15MusicInfoLoader.LoadFromFileAsync(file, CancellationToken.None);

        Assert.True(result.SongHashVersion > 0);
        Assert.NotEmpty(result.Entries);
        Assert.Equal(0, result.Entries[0].FileOrder);
        Assert.True(result.Entries[0].SongNo > 0);
        Assert.NotEmpty(result.Entries[0].MusicId);
    }

    [Fact]
    public async Task TaikojukuLoader_ReadsActiveWhiteChallengeLevelsWhenPresent()
    {
        var file = FindRepoFileOrSkip("Host", "wwwroot", "data", "white", "data", "config", "ST7100-1", "musicmedleyinfo.xml");
        var verupFile = FindRepoFileOrSkip("Host", "wwwroot", "data", "white", WhiteEraGameDataCatalog.TaikojukuVerupFileName);
        if (file is null || verupFile is null)
        {
            return;
        }

        var entries = await Ac15TaikojukuLoader.LoadFromFileAsync(
            file,
            verupFile,
            nameof(GameEra.White),
            CancellationToken.None);

        Assert.NotEmpty(entries);
        Assert.Equal(1u, entries.Min(entry => entry.ChallengeLevel));
        Assert.All(entries, entry => Assert.NotEmpty(entry.Songs));
        Assert.All(entries.SelectMany(entry => entry.Songs), song => Assert.True(song.SongNo > 0));
    }

    [Fact]
    public async Task TuningLoader_ReadsLocalWhiteDataWhenPresent()
    {
        var file = FindRepoFileOrSkip("Host", "wwwroot", "data", "white", "data", "fumen", "tuning.bin");
        if (file is null)
        {
            return;
        }

        var stars = await Ac15TuningLoader.LoadFromFileAsync(file, nameof(GameEra.White), CancellationToken.None);

        Assert.NotEmpty(stars);
        Assert.Contains(stars.Values, star => star.Easy > 0 || star.Normal > 0 || star.Hard > 0 || star.Oni > 0 || star.Ura > 0);
    }

    [Fact]
    public async Task PresentLoader_ReadsLocalWhiteDonPointRewardsWhenPresent()
    {
        var file = FindRepoFileOrSkip("Host", "wwwroot", "data", "white", "data", "config", "ST7100-1", "present.xml");
        if (file is null)
        {
            return;
        }

        var rows = await Ac15PresentLoader.LoadFromFileAsync(file, CancellationToken.None);

        Assert.Equal(10, rows.Count);
        Assert.Equal(0u, rows[0].Index);
        Assert.Equal(4u, rows[0].ItemType);
        Assert.Equal(38u, rows[0].ItemNumber);
        Assert.Equal(1000u, rows[0].DonPoint);
        Assert.Equal(rows.Count, rows.Select(row => row.Index).Distinct().Count());
    }

    [Fact]
    public async Task SpecialBaidLoader_ReadsLocalWhiteSpecialBaidRowsWhenPresent()
    {
        var file = FindRepoFileOrSkip("Host", "wwwroot", "data", "white", "data", "config", "ST7100-1", "spacialbaid.xml");
        if (file is null)
        {
            return;
        }

        var rows = await Ac15SpecialBaidLoader.LoadFromFileAsync(file, CancellationToken.None);

        Assert.Equal(2, rows.Count);
        Assert.Contains(rows, row => row.Baid == 0 && row.AccessCode == "00000000000000000000");
        Assert.Contains(rows, row => row.Baid == 316 && row.AccessCode == "30028566915530313138");
    }

    [Fact]
    public void RequiredDataFiles_ThrowsWhiteSpecificMessageForMissingFile()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}", "musicinfo.xml");

        var ex = Assert.Throws<FileNotFoundException>(() => WhiteRequiredDataFiles.ThrowIfMissing([missingPath]));
        Assert.Contains("White required game data file is missing", ex.Message, StringComparison.Ordinal);
        Assert.Equal(missingPath, ex.FileName);
    }

    [Fact]
    public async Task DefaultWhiteSidecarFiles_ExistAndLoadAsDataContracts()
    {
        var eventFolderPath = FindRequiredRepoFile("Host", "wwwroot", "data", "white", WhiteEraGameDataCatalog.EventFolderFileName);
        var telopPath = FindRequiredRepoFile("Host", "wwwroot", "data", "white", WhiteEraGameDataCatalog.TelopFileName);
        var recommendPath = FindRequiredRepoFile("Host", "wwwroot", "data", "white", WhiteEraGameDataCatalog.RecommendFileName);
        var moviePath = FindRequiredRepoFile("Host", "wwwroot", "data", "white", WhiteEraGameDataCatalog.MovieFileName);
        var taikojukuVerupPath = FindRequiredRepoFile("Host", "wwwroot", "data", "white", WhiteEraGameDataCatalog.TaikojukuVerupFileName);
        var donChallengePath = FindRequiredRepoFile("Host", "wwwroot", "data", "white", WhiteEraGameDataCatalog.DonChallengeFileName);

        var eventFolders = await Ac15EventFolderLoader.LoadFromFileAsync(
            eventFolderPath,
            new HashSet<uint>(),
            nameof(GameEra.White),
            CancellationToken.None);
        var telops = await Ac15TelopLoader.LoadFromFileAsync(telopPath, CancellationToken.None);
        var recommend = await Ac15RecommendLoader.LoadFromFileAsync(
            recommendPath,
            new HashSet<uint>(),
            CancellationToken.None);
        var movies = await Ac15MovieLoader.LoadFromFileAsync(
            moviePath,
            Path.Combine(Path.GetTempPath(), "missing-white-movie-directory"),
            nameof(GameEra.White),
            NullLogger.Instance,
            CancellationToken.None);
        var donChallenge = await Ac15DonChallengeLoader.LoadFromFileAsync(
            donChallengePath,
            isEnabled: true,
            activeBundleId: "white-2016-06",
            nameof(GameEra.White),
            CancellationToken.None);

        Assert.NotNull(eventFolders);
        Assert.NotNull(telops);
        Assert.NotNull(recommend.RecommendBestSongs);
        Assert.NotNull(movies);
        Assert.True(donChallenge.Enabled);
        Assert.Equal("white-2016-06", donChallenge.ActiveBundleId);
        Assert.NotNull(donChallenge.ActiveBundle);
        Assert.Equal(10, donChallenge.ActiveBundle.PersonalTasks.Count);
        Assert.Contains(donChallenge.ActiveBundle.Rewards, reward => reward.RewardSongNoes.Contains(585u));
        using var recommendJson = JsonDocument.Parse(await File.ReadAllTextAsync(recommendPath, CancellationToken.None));
        using var movieJson = JsonDocument.Parse(await File.ReadAllTextAsync(moviePath, CancellationToken.None));
        using var taikojukuJson = JsonDocument.Parse(await File.ReadAllTextAsync(taikojukuVerupPath, CancellationToken.None));
        Assert.True(recommendJson.RootElement.TryGetProperty("recommendBestSongs", out _));
        Assert.True(movieJson.RootElement.TryGetProperty("override_default", out _));
        Assert.True(taikojukuJson.RootElement.TryGetProperty("packs", out _));
    }

    private static string? FindRepoFileOrSkip(params string[] pathParts)
    {
        foreach (var searchRoot in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory(), GetSourceDirectory() }
            .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            for (var directory = new DirectoryInfo(searchRoot);
                 directory is not null;
                 directory = directory.Parent)
            {
                var candidate = Path.Combine(new[] { directory.FullName }.Concat(pathParts).ToArray());
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static string GetSourceDirectory([CallerFilePath] string sourceFilePath = "")
        => Path.GetDirectoryName(sourceFilePath)
           ?? throw new ApplicationException("Cannot resolve source directory.");

    private static string FindRequiredRepoFile(params string[] pathParts)
        => FindRepoFileOrSkip(pathParts)
           ?? throw new FileNotFoundException($"Could not find {Path.Combine(pathParts)}.");
}
