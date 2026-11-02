using TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenItemShopLoaderTests
{
    [Fact]
    public async Task LoadFromFile_DisabledMissingFileReturnsDisabledCatalog()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        var settings = new EraSettings { EnableShop = false };

        var catalog = await GreenItemShopLoader.LoadFromFileAsync(path, settings, CancellationToken.None);

        Assert.False(catalog.IsEnabled);
        Assert.Null(catalog.ActiveSeason);
    }

    [Fact]
    public async Task LoadFromFile_InfersItemNoAndSelectsActiveSeason()
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, """
                {
                  "seasons": [
                    {
                      "season_id": 2,
                      "verup_no": 9,
                      "telop": "Shop",
                      "start_datetime": "20190314000000",
                      "end_datetime": "20190626075959",
                      "afterstart_days": 3,
                      "beforeclose_days": 4,
                      "items": [
                        { "item_type": 4, "item_id": 117, "item_price": 500 },
                        { "item_type": 1, "item_id": 865, "item_price": 1300 }
                      ]
                    }
                  ]
                }
                """);

            var catalog = await GreenItemShopLoader.LoadFromFileAsync(
                path,
                new EraSettings { EnableShop = true, ActiveShopSeasonId = 2 },
                CancellationToken.None);

            Assert.True(catalog.IsEnabled);
            Assert.Equal(2u, catalog.ActiveSeason!.SeasonId);
            Assert.Equal(9u, catalog.ActiveSeason.VerupNo);
            Assert.Equal(1u, catalog.ActiveSeason.Items[0].ItemNo);
            Assert.Equal(2u, catalog.ActiveSeason.Items[1].ItemNo);
            Assert.Equal(865u, catalog.ActiveItemsByNo[2].ItemId);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task LoadFromFile_DefaultGreenItemShopDataLoads()
    {
        var path = FindDefaultShopDataPath();

        Assert.NotNull(path);

        var expectedCounts = new Dictionary<uint, int>
        {
            [1] = 4,
            [2] = 8,
            [3] = 8,
            [4] = 8
        };

        foreach (var (seasonId, expectedCount) in expectedCounts)
        {
            var catalog = await GreenItemShopLoader.LoadFromFileAsync(
                path,
                new EraSettings { EnableShop = true, ActiveShopSeasonId = seasonId },
                CancellationToken.None);

            Assert.True(catalog.IsEnabled);
            Assert.Equal(4, catalog.Seasons.Count);
            Assert.NotNull(catalog.ActiveSeason);
            Assert.Equal(seasonId, catalog.ActiveSeason.SeasonId);
            Assert.Equal(expectedCount, catalog.ActiveSeason.Items.Count);
        }
    }

    [Theory]
    [InlineData(null, "active")]
    [InlineData(99u, "active")]
    public async Task LoadFromFile_EnabledRequiresKnownActiveSeason(uint? activeSeasonId, string failureText)
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, """
                {
                  "seasons": [
                    {
                      "season_id": 1,
                      "verup_no": 1,
                      "telop": "Shop",
                      "start_datetime": "20190314000000",
                      "end_datetime": "20190626075959",
                      "afterstart_days": 0,
                      "beforeclose_days": 0,
                      "items": [
                        { "item_type": 1, "item_id": 865, "item_price": 1300 }
                      ]
                    }
                  ]
                }
                """);

            var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
                GreenItemShopLoader.LoadFromFileAsync(
                    path,
                    new EraSettings { EnableShop = true, ActiveShopSeasonId = activeSeasonId },
                    CancellationToken.None));

            Assert.Contains(failureText, ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Theory]
    [InlineData("\"start_datetime\": \"2019-03-14\"", "datetime")]
    [InlineData("\"items\": []", "item")]
    public async Task LoadFromFile_RejectsInvalidSeasonData(string replacement, string failureText)
    {
        var json = """
            {
              "seasons": [
                {
                  "season_id": 1,
                  "verup_no": 1,
                  "telop": "Shop",
                  "start_datetime": "20190314000000",
                  "end_datetime": "20190626075959",
                  "afterstart_days": 0,
                  "beforeclose_days": 0,
                  "items": [
                    { "item_type": 1, "item_id": 865, "item_price": 1300 }
                  ]
                }
              ]
            }
            """;

        json = replacement.StartsWith("\"start_datetime\"", StringComparison.Ordinal)
            ? json.Replace("\"start_datetime\": \"20190314000000\"", replacement, StringComparison.Ordinal)
            : """
                {
                  "seasons": [
                    {
                      "season_id": 1,
                      "verup_no": 1,
                      "telop": "Shop",
                      "start_datetime": "20190314000000",
                      "end_datetime": "20190626075959",
                      "afterstart_days": 0,
                      "beforeclose_days": 0,
                      "items": []
                    }
                  ]
                }
                """;

        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, json);

            var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
                GreenItemShopLoader.LoadFromFileAsync(
                    path,
                    new EraSettings { EnableShop = true, ActiveShopSeasonId = 1 },
                    CancellationToken.None));

            Assert.Contains(failureText, ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string? FindDefaultShopDataPath()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var path = Path.Combine(
                directory.FullName,
                "Host",
                "wwwroot",
                "data",
                "green",
                GreenItemShopLoader.FileName);

            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }
}
