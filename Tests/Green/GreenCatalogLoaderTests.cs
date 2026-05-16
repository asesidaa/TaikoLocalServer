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

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
