using TaikoLocalServer.Application.Catalog.Blue;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

namespace TaikoLocalServer.Tests.Blue;

[Collection("Blue runtime catalog tests")]
public sealed class BlueItemShopLoaderTests
{
    [Fact]
    public async Task LoadAsync_DisabledMissingFileReturnsDisabledCatalog()
    {
        using var scope = ProcessShopDataScope.Missing();

        var catalog = await new BlueItemShopLoader().LoadAsync(
            new EraSettings { EnableShop = false },
            CancellationToken.None);

        Assert.False(catalog.IsEnabled);
        Assert.Null(catalog.ActiveSeason);
    }

    [Fact]
    public async Task LoadAsync_MissingEnabledFileFailsFast()
    {
        using var scope = ProcessShopDataScope.Missing();

        var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
            new BlueItemShopLoader().LoadAsync(
                new EraSettings { EnableShop = true, ActiveShopSeasonId = 1 },
                CancellationToken.None));

        Assert.Contains("Blue item shop is enabled but data file was not found", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoadAsync_DefaultBlueItemShopDataLoads()
    {
        using var scope = ProcessShopDataScope.Write(await File.ReadAllTextAsync(
            FindDefaultShopDataPath(),
            CancellationToken.None));

        var catalog = await new BlueItemShopLoader().LoadAsync(
            new EraSettings { EnableShop = true, ActiveShopSeasonId = 1 },
            CancellationToken.None);

        Assert.True(catalog.IsEnabled);
        Assert.Equal(1u, catalog.ActiveSeasonId);
        var season = Assert.Single(catalog.Seasons.Values);
        Assert.Equal(1u, season.SeasonId);
        Assert.Equal(20170404u, season.VerupNo);
        Assert.Equal("20181219070000", season.StartDatetime);
        Assert.Equal("20190314020000", season.EndDatetime);
        Assert.Equal(30u, season.AfterstartDays);
        Assert.Equal(0u, season.BeforecloseDays);

        Assert.Collection(
            season.Items,
            item => AssertItem(item, 1, 3, 12, 1300),
            item => AssertItem(item, 2, 3, 7, 1500),
            item => AssertItem(item, 3, 3, 9, 1500),
            item => AssertItem(item, 4, 3, 10, 1500));
    }

    [Theory]
    [InlineData(null, "ActiveShopSeasonId")]
    [InlineData(99u, "active season 99")]
    public async Task LoadAsync_EnabledRequiresKnownActiveSeason(uint? activeSeasonId, string failureText)
    {
        using var scope = ProcessShopDataScope.Write(ValidJson);

        var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
            new BlueItemShopLoader().LoadAsync(
                new EraSettings { EnableShop = true, ActiveShopSeasonId = activeSeasonId },
                CancellationToken.None));

        Assert.Contains(failureText, ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("\"start_datetime\": \"2018-12-19\"", "datetime")]
    [InlineData("\"items\": []", "item")]
    public async Task LoadAsync_RejectsInvalidSeasonData(string replacement, string failureText)
    {
        var json = replacement.StartsWith("\"start_datetime\"", StringComparison.Ordinal)
            ? ValidJson.Replace("\"start_datetime\": \"20181219070000\"", replacement, StringComparison.Ordinal)
            : BuildJson("[]");
        using var scope = ProcessShopDataScope.Write(json);

        var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
            new BlueItemShopLoader().LoadAsync(
                new EraSettings { EnableShop = true, ActiveShopSeasonId = 1 },
                CancellationToken.None));

        Assert.Contains(failureText, ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("""
        [
          { "item_type": 3, "item_id": 12, "item_price": 1300 },
          { "item_type": 3, "item_id": 12, "item_price": 1500 }
        ]
        """, "duplicate item identities")]
    [InlineData("""
        [
          { "item_type": 8, "item_id": 12, "item_price": 1300 }
        ]
        """, "unsupported item_type 8")]
    [InlineData("""
        [
          { "item_type": 3, "item_id": 0, "item_price": 1300 }
        ]
        """, "item_id 0")]
    [InlineData("""
        [
          { "item_type": 3, "item_id": 12, "item_price": 0 }
        ]
        """, "item_price 0")]
    public async Task LoadAsync_RejectsInvalidItemRows(string itemsJson, string failureText)
    {
        using var scope = ProcessShopDataScope.Write(BuildJson(itemsJson));

        var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
            new BlueItemShopLoader().LoadAsync(
                new EraSettings { EnableShop = true, ActiveShopSeasonId = 1 },
                CancellationToken.None));

        Assert.Contains(failureText, ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private const string ValidJson = """
        {
          "seasons": [
            {
              "season_id": 1,
              "verup_no": 20170404,
              "telop": "",
              "start_datetime": "20181219070000",
              "end_datetime": "20190314020000",
              "afterstart_days": 30,
              "beforeclose_days": 0,
              "items": [
                { "item_type": 3, "item_id": 12, "item_price": 1300 }
              ]
            }
          ]
        }
        """;

    private static string BuildJson(string itemsJson)
        => $$"""
            {
              "seasons": [
                {
                  "season_id": 1,
                  "verup_no": 20170404,
                  "telop": "",
                  "start_datetime": "20181219070000",
                  "end_datetime": "20190314020000",
                  "afterstart_days": 30,
                  "beforeclose_days": 0,
                  "items": {{itemsJson}}
                }
              ]
            }
            """;

    private static string FindDefaultShopDataPath()
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
                "blue",
                BlueItemShopLoader.FileName);

            if (File.Exists(path))
            {
                return path;
            }
        }

        throw new FileNotFoundException("Could not find committed Blue item shop data.");
    }

    private static void AssertItem(
        BlueItemShopEntry item,
        uint itemNo,
        uint itemType,
        uint itemId,
        uint price)
    {
        Assert.Equal(itemNo, item.ItemNo);
        Assert.Equal(itemType, item.ItemType);
        Assert.Equal(itemId, item.ItemId);
        Assert.Equal(price, item.Price);
    }

    private sealed class ProcessShopDataScope : IDisposable
    {
        private readonly string path;
        private readonly string? previousContent;

        private ProcessShopDataScope(string? content)
        {
            path = Path.Combine(
                Path.GetDirectoryName(Environment.ProcessPath)
                    ?? throw new ApplicationException("Cannot resolve process directory."),
                "wwwroot",
                "data",
                "blue",
                BlueItemShopLoader.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(path)
                ?? throw new ApplicationException($"Cannot resolve directory for {path}."));

            previousContent = File.Exists(path)
                ? File.ReadAllText(path)
                : null;

            if (content is null)
            {
                File.Delete(path);
            }
            else
            {
                File.WriteAllText(path, content);
            }
        }

        public static ProcessShopDataScope Missing()
            => new(null);

        public static ProcessShopDataScope Write(string content)
            => new(content);

        public void Dispose()
        {
            if (previousContent is null)
            {
                File.Delete(path);
            }
            else
            {
                File.WriteAllText(path, previousContent);
            }
        }
    }
}
