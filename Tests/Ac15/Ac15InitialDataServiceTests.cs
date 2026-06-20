using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.ServerData;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15InitialDataServiceTests
{
    [Fact]
    public void BuildCommonInitialData_HidesActiveShopSongsFromDefaultSongFlags()
    {
        var snapshot = new Ac15CatalogSnapshot(
            SongHashVersion: 456,
            SongNoesInFileOrder: [101, 102],
            EventFolders: new Dictionary<uint, EventFolderData>(),
            Telops: new Dictionary<uint, Ac15TelopEntry>(),
            ItemShopCatalog: new Ac15ItemShopCatalog
            {
                IsEnabled = true,
                ActiveSeasonId = 1,
                Seasons = new Dictionary<uint, Ac15ItemShopSeason>
                {
                    [1] = new()
                    {
                        SeasonId = 1,
                        VerupNo = 2,
                        Items = [new() { ItemNo = 1, ItemType = Ac15ShopItemType.Song, ItemId = 102, Price = 10 }]
                    }
                }
            },
            TaikojukuPacks: []);

        var response = Ac15InitialDataService.BuildCommonInitialData(snapshot, Ac15EraProfiles.Green);

        Assert.Equal(1u, response.Result);
        Assert.Equal(456u, response.SongHashVer);
        Assert.True(response.IsDanplay);
        Assert.True(response.IsItemshop);
        Assert.True((response.DefaultSongFlg[101 >> 3] & (1 << (101 & 7))) != 0);
        Assert.True((response.DefaultSongFlg[102 >> 3] & (1 << (102 & 7))) == 0);
    }
}
