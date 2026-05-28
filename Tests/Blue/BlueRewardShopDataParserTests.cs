using System.Text.Json;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueRewardShopDataParserTests
{
    private const string OfficialCachePath = @"H:\taiko\blue\rewardshopdata.bin";
    private const int FirstItemOffset = 0x73;

    [Fact]
    public async Task ParseFromFile_ReadsOfficialCacheSeasonEnvelope()
    {
        Assert.True(File.Exists(OfficialCachePath), $"Missing local Blue reward shop cache: {OfficialCachePath}");

        var data = await new BlueRewardShopDataParser().ParseFromFileAsync(
            OfficialCachePath,
            CancellationToken.None);

        var season = Assert.Single(data.Seasons);
        Assert.Equal(1u, season.SeasonId);
        Assert.Equal(20170404u, season.VerupNo);
        Assert.Equal(string.Empty, season.Telop);
        Assert.Equal("20181219070000", season.StartDatetime);
        Assert.Equal("20190314020000", season.EndDatetime);
        Assert.Equal(30u, season.AfterstartDays);
        Assert.Equal(0u, season.BeforecloseDays);
    }

    [Fact]
    public async Task ParseFromFile_ReadsOfficialCacheRowsInProtocolOrder()
    {
        Assert.True(File.Exists(OfficialCachePath), $"Missing local Blue reward shop cache: {OfficialCachePath}");

        var data = await new BlueRewardShopDataParser().ParseFromFileAsync(
            OfficialCachePath,
            CancellationToken.None);

        var items = Assert.Single(data.Seasons).Items;

        Assert.Collection(
            items,
            item => AssertItem(item, 1, 3, 12, 1300),
            item => AssertItem(item, 2, 3, 7, 1500),
            item => AssertItem(item, 3, 3, 9, 1500),
            item => AssertItem(item, 4, 3, 10, 1500));
        Assert.All(items, item => Assert.Equal(
            BlueRewardShopDataParser.BlueRewardShopCatalogDomain.Kigurumi,
            item.CatalogDomain));
    }

    [Fact]
    public async Task CommittedDefaultJsonMatchesOfficialCache()
    {
        Assert.True(File.Exists(OfficialCachePath), $"Missing local Blue reward shop cache: {OfficialCachePath}");

        var parsed = await new BlueRewardShopDataParser().ParseFromFileAsync(
            OfficialCachePath,
            CancellationToken.None);
        var parsedSeason = Assert.Single(parsed.Seasons);

        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(
            FindDefaultShopDataPath(),
            CancellationToken.None));
        var season = Assert.Single(document.RootElement.GetProperty("seasons").EnumerateArray());

        Assert.Equal(parsedSeason.SeasonId, season.GetProperty("season_id").GetUInt32());
        Assert.Equal(parsedSeason.VerupNo, season.GetProperty("verup_no").GetUInt32());
        Assert.Equal(parsedSeason.Telop, season.GetProperty("telop").GetString());
        Assert.Equal(parsedSeason.StartDatetime, season.GetProperty("start_datetime").GetString());
        Assert.Equal(parsedSeason.EndDatetime, season.GetProperty("end_datetime").GetString());
        Assert.Equal(parsedSeason.AfterstartDays, season.GetProperty("afterstart_days").GetUInt32());
        Assert.Equal(parsedSeason.BeforecloseDays, season.GetProperty("beforeclose_days").GetUInt32());

        var jsonItems = season.GetProperty("items").EnumerateArray().ToArray();
        Assert.Equal(parsedSeason.Items.Count, jsonItems.Length);
        for (var index = 0; index < jsonItems.Length; index++)
        {
            Assert.Equal(parsedSeason.Items[index].ItemType, jsonItems[index].GetProperty("item_type").GetUInt32());
            Assert.Equal(parsedSeason.Items[index].ItemId, jsonItems[index].GetProperty("item_id").GetUInt32());
            Assert.Equal(parsedSeason.Items[index].Price, jsonItems[index].GetProperty("item_price").GetUInt32());
        }
    }

    [Fact]
    public void Parse_RejectsInvalidBoostSignature()
    {
        var bytes = ReadOfficialBytes();
        bytes[4] = (byte)'x';

        var ex = Assert.Throws<InvalidDataException>(() => new BlueRewardShopDataParser().Parse(bytes));

        Assert.Contains("Boost", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_RejectsUnsupportedItemTypes()
    {
        var bytes = ReadOfficialBytes();
        bytes[FirstItemOffset + 10] = 8;

        var ex = Assert.Throws<InvalidDataException>(() => new BlueRewardShopDataParser().Parse(bytes));

        Assert.Contains("unsupported item_type 8", ex.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("item_id", 11, 11)]
    [InlineData("item_price", 12, 15)]
    public void Parse_RejectsZeroItemIdsAndPrices(string expectedText, int firstOffset, int lastOffset)
    {
        var bytes = ReadOfficialBytes();
        for (var offset = firstOffset; offset <= lastOffset; offset++)
        {
            bytes[FirstItemOffset + offset] = 0;
        }

        var ex = Assert.Throws<InvalidDataException>(() => new BlueRewardShopDataParser().Parse(bytes));

        Assert.Contains(expectedText, ex.Message, StringComparison.Ordinal);
    }

    private static byte[] ReadOfficialBytes()
    {
        Assert.True(File.Exists(OfficialCachePath), $"Missing local Blue reward shop cache: {OfficialCachePath}");
        return File.ReadAllBytes(OfficialCachePath);
    }

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
        BlueRewardShopDataParser.BlueRewardShopItem item,
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
}
