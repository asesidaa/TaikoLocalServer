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
