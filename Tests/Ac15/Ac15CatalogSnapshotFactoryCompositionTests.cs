using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.ServerData;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15CatalogSnapshotFactoryCompositionTests
{
    [Fact]
    public void FromSource_PreservesMusicOrderTelopsShopAndTaikojuku()
    {
        var source = new Ac15CatalogProjectionSource(
            SongHashVersion: 123,
            SongNoesInFileOrder: [101, 102],
            EventFolders: new Dictionary<uint, EventFolderData>
            {
                [7] = new() { FolderId = 7, VerupNo = 20110301, Priority = 1, SongNoes = [101, 102] }
            },
            Telops: new Dictionary<uint, Ac15TelopEntry>
            {
                [4] = new()
                {
                    TelopId = 4,
                    VerupNo = 20110301,
                    StartDatetime = "20110301070000",
                    EndDatetime = "20110630020000",
                    Message = "HELLO"
                }
            },
            ItemShopCatalog: new Ac15ItemShopCatalog
            {
                IsEnabled = true,
                ActiveSeasonId = 2,
                Seasons = new Dictionary<uint, Ac15ItemShopSeason>
                {
                    [2] = new()
                    {
                        SeasonId = 2,
                        Items = [new() { ItemNo = 1, ItemType = Ac15ShopItemType.Tone, ItemId = 4, Price = 100 }]
                    }
                }
            },
            TaikojukuPacks:
            [
                new Ac15TaikojukuEntry
                {
                    UniqueId = 20001,
                    ChallengeLevel = 1,
                    Songs = [new Ac15TaikojukuSong { SongNo = 101, Level = Difficulty.Easy }]
                }
            ]);

        var snapshot = Ac15CatalogSnapshotFactory.FromSource(source);

        Assert.Equal(123u, snapshot.SongHashVersion);
        Assert.Equal([101u, 102u], snapshot.SongNoesInFileOrder);
        Assert.Single(snapshot.EventFolders);
        Assert.True(snapshot.Telops.ContainsKey(4));
        Assert.True(snapshot.ItemShopCatalog.IsEnabled);
        Assert.Single(snapshot.TaikojukuPacks);
    }
}
