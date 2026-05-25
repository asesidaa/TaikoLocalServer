namespace TaikoLocalServer.Tests.Green;

public sealed class GreenItemShopCatalogTests
{
    [Fact]
    public void ActiveSeason_ItemsByNoUseInferredItemNo()
    {
        var season = new GreenItemShopSeason
        {
            SeasonId = 1,
            VerupNo = 7,
            Telop = "Shop",
            StartDatetime = "20190314000000",
            EndDatetime = "20190626075959",
            AfterstartDays = 3,
            BeforecloseDays = 4,
            Items =
            [
                new GreenItemShopEntry { ItemNo = 1, ItemType = 4, ItemId = 117, Price = 500 },
                new GreenItemShopEntry { ItemNo = 2, ItemType = 1, ItemId = 865, Price = 1300 }
            ]
        };

        var catalog = new GreenItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 1,
            Seasons = new Dictionary<uint, GreenItemShopSeason>
            {
                [1] = season
            }
        };

        Assert.Same(season, catalog.ActiveSeason);
        Assert.Equal(865u, catalog.ActiveItemsByNo[2].ItemId);
    }

    [Fact]
    public void DisabledCatalog_HasNoActiveSeasonOrItems()
    {
        var catalog = GreenItemShopCatalog.Disabled;

        Assert.False(catalog.IsEnabled);
        Assert.Null(catalog.ActiveSeason);
        Assert.Empty(catalog.ActiveItemsByNo);
    }
}
