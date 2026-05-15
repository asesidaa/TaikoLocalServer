using TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenRecommendCatalogLoaderTests
{
    [Fact]
    public async Task LoadFromFile_ReturnsConfiguredSongs_FilteredByCatalog()
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, """
                {
                  "recommendSong": 101,
                  "recommendBestSongs": [101, 102, 9999]
                }
                """);

            var catalog = new HashSet<uint> { 101, 102 };
            var entry = await GreenRecommendLoader.LoadFromFileAsync(path, catalog, CancellationToken.None);

            Assert.Equal(101u, entry.RecommendSong);
            Assert.Equal(new uint[] { 101, 102 }, entry.RecommendBestSongs);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task LoadFromFile_DropsRecommendSongWhenOutOfCatalog()
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, """
                {
                  "recommendSong": 9999,
                  "recommendBestSongs": []
                }
                """);

            var catalog = new HashSet<uint> { 101, 102 };
            var entry = await GreenRecommendLoader.LoadFromFileAsync(path, catalog, CancellationToken.None);

            Assert.Equal(0u, entry.RecommendSong);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task LoadFromFile_MissingFileReturnsEmpty()
    {
        var entry = await GreenRecommendLoader.LoadFromFileAsync(
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()),
            new HashSet<uint> { 101 },
            CancellationToken.None);

        Assert.Equal(0u, entry.RecommendSong);
        Assert.Empty(entry.RecommendBestSongs);
    }

    [Fact]
    public async Task LoadFromFile_DropsSongIdsOutsideTableRange()
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, """
                {
                  "recommendSong": 1024,
                  "recommendBestSongs": [1023, 1024]
                }
                """);

            var catalog = new HashSet<uint> { 1023, 1024 };
            var entry = await GreenRecommendLoader.LoadFromFileAsync(path, catalog, CancellationToken.None);

            Assert.Equal(0u, entry.RecommendSong);
            Assert.Equal(new uint[] { 1023 }, entry.RecommendBestSongs);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
