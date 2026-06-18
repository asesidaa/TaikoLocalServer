using System.Text.Json;
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
        var json = await File.ReadAllTextAsync(
            FindDefaultShopDataPath(),
            CancellationToken.None);
        var activeSeasonId = GetFirstSeasonId(json);
        using var scope = ProcessShopDataScope.Write(json);

        var catalog = await new BlueItemShopLoader().LoadAsync(
            new EraSettings { EnableShop = true, ActiveShopSeasonId = activeSeasonId },
            CancellationToken.None);

        Assert.True(catalog.IsEnabled);
        Assert.Equal(activeSeasonId, catalog.ActiveSeasonId);
        Assert.NotEmpty(catalog.Seasons);

        var season = catalog.ActiveSeason;
        Assert.NotNull(season);
        Assert.Equal(activeSeasonId, season!.SeasonId);
        Assert.True(IsProtocolDateTime(season.StartDatetime));
        Assert.True(IsProtocolDateTime(season.EndDatetime));
        Assert.InRange(season.Items.Count, 1, 64);

        var expectedItemNo = 1u;
        var itemIdentities = new HashSet<(Ac15ShopItemType ItemType, uint ItemId)>();
        foreach (var item in season.Items)
        {
            AssertItem(item, expectedItemNo++);
            Assert.True(itemIdentities.Add((item.ItemType, item.ItemId)), $"Duplicate item identity {item.ItemType}:{item.ItemId}.");
        }
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

    private static uint GetFirstSeasonId(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement
            .GetProperty("seasons")
            .EnumerateArray()
            .First()
            .GetProperty("season_id")
            .GetUInt32();
    }

    private static bool IsProtocolDateTime(string value)
        => value is { Length: 14 } && value.All(char.IsAsciiDigit);

    private static void AssertItem(Ac15ItemShopEntry item, uint itemNo)
    {
        Assert.Equal(itemNo, item.ItemNo);
        Assert.True(item.ItemType.IsSupported());
        Assert.NotEqual(0u, item.ItemId);
        Assert.NotEqual(0u, item.Price);
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
