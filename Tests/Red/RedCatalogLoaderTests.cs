using System.Runtime.CompilerServices;
using System.Text.Json;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Red;

namespace TaikoLocalServer.Tests.Red;

public sealed class RedCatalogLoaderTests
{
    [Fact]
    public async Task MusicInfoLoader_ReadsLocalActiveRedDataWhenPresent()
    {
        var file = FindRepoFileOrSkip("Host", "wwwroot", "data", "red", "data", "config", "ST8100-1", "musicinfo.xml");
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
    public async Task TaikojukuLoader_ReadsActiveRedChallengeLevelsWhenPresent()
    {
        var file = FindRepoFileOrSkip("Host", "wwwroot", "data", "red", "data", "config", "ST8100-1", "musicmedleyinfo.xml");
        var verupFile = FindRepoFileOrSkip("Host", "wwwroot", "data", "red", RedEraGameDataCatalog.TaikojukuVerupFileName);
        if (file is null || verupFile is null)
        {
            return;
        }

        var entries = await Ac15TaikojukuLoader.LoadFromFileAsync(
            file,
            verupFile,
            nameof(GameEra.Red),
            CancellationToken.None);

        Assert.NotEmpty(entries);
        Assert.Equal(1u, entries.Min(entry => entry.ChallengeLevel));
        Assert.Equal(113u, entries.Max(entry => entry.ChallengeLevel));
        Assert.All(entries, entry => Assert.NotEmpty(entry.Songs));
        Assert.All(entries.SelectMany(entry => entry.Songs), song => Assert.True(song.SongNo > 0));
    }

    [Fact]
    public async Task TuningLoader_ReadsLocalRedDataWhenPresent()
    {
        var file = FindRepoFileOrSkip("Host", "wwwroot", "data", "red", "data", "fumen", "tuning.bin");
        if (file is null)
        {
            return;
        }

        var stars = await Ac15TuningLoader.LoadFromFileAsync(file, nameof(GameEra.Red), CancellationToken.None);

        Assert.NotEmpty(stars);
        Assert.Contains(stars.Values, star => star.Easy > 0 || star.Normal > 0 || star.Hard > 0 || star.Oni > 0 || star.Ura > 0);
    }

    [Fact]
    public void RequiredDataFiles_ThrowsRedSpecificMessageForMissingFile()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}", "musicinfo.xml");

        var ex = Assert.Throws<FileNotFoundException>(() => RedRequiredDataFiles.ThrowIfMissing([missingPath]));
        Assert.Contains("Red required game data file is missing", ex.Message, StringComparison.Ordinal);
        Assert.Equal(missingPath, ex.FileName);
    }

    [Fact]
    public async Task DefaultRedSidecarFiles_ExistAndLoadAsDataContracts()
    {
        var eventFolderPath = FindRequiredRepoFile("Host", "wwwroot", "data", "red", RedEraGameDataCatalog.EventFolderFileName);
        var telopPath = FindRequiredRepoFile("Host", "wwwroot", "data", "red", RedEraGameDataCatalog.TelopFileName);
        var recommendPath = FindRequiredRepoFile("Host", "wwwroot", "data", "red", RedEraGameDataCatalog.RecommendFileName);
        var moviePath = FindRequiredRepoFile("Host", "wwwroot", "data", "red", RedEraGameDataCatalog.MovieFileName);
        var taikojukuVerupPath = FindRequiredRepoFile("Host", "wwwroot", "data", "red", RedEraGameDataCatalog.TaikojukuVerupFileName);

        var eventFolders = await Ac15EventFolderLoader.LoadFromFileAsync(
            eventFolderPath,
            new HashSet<uint>(),
            nameof(GameEra.Red),
            CancellationToken.None);
        var telops = await Ac15TelopLoader.LoadFromFileAsync(telopPath, CancellationToken.None);
        var recommend = await Ac15RecommendLoader.LoadFromFileAsync(
            recommendPath,
            new HashSet<uint>(),
            CancellationToken.None);
        var movies = await Ac15MovieLoader.LoadFromFileAsync(
            moviePath,
            Path.Combine(Path.GetTempPath(), "missing-red-movie-directory"),
            nameof(GameEra.Red),
            NullLogger.Instance,
            CancellationToken.None);

        Assert.Empty(eventFolders);
        Assert.Empty(telops);
        Assert.Empty(recommend.RecommendBestSongs);
        Assert.NotNull(movies);
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
