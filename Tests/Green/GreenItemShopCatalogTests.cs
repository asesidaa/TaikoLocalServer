namespace TaikoLocalServer.Tests.Green;

public sealed class Ac15ItemShopCatalogTests
{
    [Fact]
    public void ActiveSeason_ItemsByNoUseInferredItemNo()
    {
        var season = new Ac15ItemShopSeason
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
                new Ac15ItemShopEntry { ItemNo = 1, ItemType = Ac15ShopItemType.Body, ItemId = 117, Price = 500 },
                new Ac15ItemShopEntry { ItemNo = 2, ItemType = Ac15ShopItemType.Song, ItemId = 865, Price = 1300 }
            ]
        };

        var catalog = new Ac15ItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 1,
            Seasons = new Dictionary<uint, Ac15ItemShopSeason>
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
        var catalog = Ac15ItemShopCatalog.Disabled;

        Assert.False(catalog.IsEnabled);
        Assert.Null(catalog.ActiveSeason);
        Assert.Empty(catalog.ActiveItemsByNo);
    }
}
