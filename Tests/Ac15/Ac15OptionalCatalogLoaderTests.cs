using Microsoft.Extensions.Logging.Abstractions;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15OptionalCatalogLoaderTests
{
    [Fact]
    public async Task ItemShopLoader_DisabledMissingFileReturnsDisabledCatalog()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");

        var catalog = await Ac15ItemShopLoader.LoadFromFileAsync(
            path,
            isEnabled: false,
            activeSeasonId: null,
            eraName: "Blue",
            CancellationToken.None);

        Assert.False(catalog.IsEnabled);
        Assert.Null(catalog.ActiveSeason);
    }

    [Fact]
    public async Task ItemShopLoader_EnabledRequiresKnownActiveSeason()
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, """
            {
              "seasons": [
                {
                  "season_id": 2,
                  "verup_no": 9,
                  "telop": "Shop",
                  "start_datetime": "20180315000000",
                  "end_datetime": "20180626075959",
                  "afterstart_days": 3,
                  "beforeclose_days": 4,
                  "items": [
                    { "item_type": 4, "item_id": 117, "item_price": 500 }
                  ]
                }
              ]
            }
            """);

        try
        {
            var catalog = await Ac15ItemShopLoader.LoadFromFileAsync(
                path,
                isEnabled: true,
                activeSeasonId: 2,
                eraName: "Blue",
                CancellationToken.None);

            Assert.True(catalog.IsEnabled);
            Assert.Equal(2u, catalog.ActiveSeason!.SeasonId);
            Assert.Equal(1u, Assert.Single(catalog.ActiveSeason.Items).ItemNo);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ItemShopLoader_AcceptsNamedItemTypesAndMapsBodyHeadCorrectly()
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, """
            {
              "seasons": [
                {
                  "season_id": 2,
                  "verup_no": 9,
                  "telop": "Shop",
                  "start_datetime": "20180315000000",
                  "end_datetime": "20180626075959",
                  "afterstart_days": 3,
                  "beforeclose_days": 4,
                  "items": [
                    { "item_type": "body", "item_id": 117, "item_price": 500 },
                    { "item_type": "head", "item_id": 117, "item_price": 500 },
                    { "item_type": "tone", "item_id": 4, "item_price": 500 }
                  ]
                }
              ]
            }
            """);

        try
        {
            var catalog = await Ac15ItemShopLoader.LoadFromFileAsync(
                path,
                isEnabled: true,
                activeSeasonId: 2,
                eraName: "Blue",
                CancellationToken.None);

            var items = catalog.ActiveSeason!.Items;
            Assert.Equal(Ac15ShopItemType.Body, items[0].ItemType);
            Assert.Equal(Ac15ShopItemType.Head, items[1].ItemType);
            Assert.Equal(Ac15ShopItemType.Tone, items[2].ItemType);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task EventFolderLoader_MissingFileReturnsEmpty()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");

        var folders = await Ac15EventFolderLoader.LoadFromFileAsync(
            path,
            new HashSet<uint> { 100 },
            eraName: "Blue",
            CancellationToken.None);

        Assert.Empty(folders);
    }

    [Fact]
    public async Task RecommendLoader_FiltersUnknownSongs()
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, """
            {
              "recommendSong": 100,
              "recommendBestSongs": [100, 999]
            }
            """);

        try
        {
            var recommend = await Ac15RecommendLoader.LoadFromFileAsync(
                path,
                new HashSet<uint> { 100 },
                CancellationToken.None);

            Assert.Equal(100u, recommend.RecommendSong);
            Assert.Equal([100u], recommend.RecommendBestSongs);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task MovieLoader_MissingConfigDiscoversNonzeroMovies()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        await File.WriteAllBytesAsync(Path.Combine(root, "attract_cm_000.pam"), []);
        await File.WriteAllBytesAsync(Path.Combine(root, "attract_cm_101.pam"), []);

        try
        {
            var movies = await Ac15MovieLoader.LoadFromFileAsync(
                Path.Combine(root, "missing.json"),
                root,
                eraName: "Blue",
                NullLogger.Instance,
                CancellationToken.None);

            var movie = Assert.Single(movies);
            Assert.Equal(101u, movie.MovieId);
            Assert.Equal(999u, movie.EnableDays);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
