using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.ServerData;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15CatalogReadbackServiceTests
{
    [Fact]
    public void BuildFolderResponse_ReturnsOnlyRequestedKnownFolders()
    {
        var snapshot = Snapshot();

        var response = Ac15CatalogReadbackService.BuildFolderResponse(snapshot, [2, 99, 1]);

        Assert.Equal(1u, response.Result);
        Assert.Equal([2u, 1u], response.AryEventfolderDatas.Select(row => row.FolderId));
    }

    [Fact]
    public void BuildTelopResponse_ReturnsEmptySuccessWhenMissing()
    {
        var response = Ac15CatalogReadbackService.BuildTelopResponse(Snapshot(), 99);

        Assert.Equal(1u, response.Result);
        Assert.Null(response.VerupNo);
        Assert.Null(response.Telop);
    }

    [Fact]
    public void BuildItemShopInfo_ReturnsRowsOrderedByItemNoWhenEnabled()
    {
        var response = Ac15CatalogReadbackService.BuildItemShopInfo(Snapshot());

        Assert.Equal(1u, response.Result);
        Assert.Equal(7u, response.SeasonId);
        Assert.Equal([1u, 2u], response.AryItemshopData.Select(row => row.ItemNo));
    }

    [Fact]
    public void BuildRecommendResponse_ReturnsRandomSongWithoutBestSongAppendList()
    {
        var response = Ac15CatalogReadbackService.BuildRecommendResponse(Snapshot(songNoes: [101]));

        Assert.Equal(1u, response.Result);
        Assert.Equal(101u, response.RecommendSong);
        Assert.Empty(response.RecommendBestSong);
    }

    [Fact]
    public void BuildRecommendResponse_DoesNotUseReservedMedleyRowsAsSeed()
    {
        var response = Ac15CatalogReadbackService.BuildRecommendResponse(Snapshot(songNoes: [20001]));

        Assert.Equal(1u, response.Result);
        Assert.Equal(0u, response.RecommendSong);
        Assert.Empty(response.RecommendBestSong);
    }

    private static Ac15CatalogSnapshot Snapshot(IReadOnlyList<uint>? songNoes = null) => new(
        SongHashVersion: 456,
        SongNoesInFileOrder: songNoes ?? [101, 102, 103],
        EventFolders: new Dictionary<uint, EventFolderData>
        {
            [1] = new() { FolderId = 1, VerupNo = 8, SongNoes = [101] },
            [2] = new() { FolderId = 2, VerupNo = 9, SongNoes = [102] }
        },
        Telops: new Dictionary<uint, Ac15TelopEntry>
        {
            [5] = new() { TelopId = 5, VerupNo = 10, StartDatetime = "20260101000000", EndDatetime = "20261231235959", Message = "hello" }
        },
        ItemShopCatalog: new Ac15ItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 7,
            Seasons = new Dictionary<uint, Ac15ItemShopSeason>
            {
                [7] = new()
                {
                    SeasonId = 7,
                    VerupNo = 70,
                    Telop = "shop",
                    StartDatetime = "20260101000000",
                    EndDatetime = "20261231235959",
                    AfterstartDays = 1,
                    BeforecloseDays = 2,
                    Items =
                    [
                        new() { ItemNo = 2, ItemType = Ac15ShopItemType.Tone, ItemId = 44, Price = 100 },
                        new() { ItemNo = 1, ItemType = Ac15ShopItemType.Song, ItemId = 102, Price = 200 }
                    ]
                }
            }
        },
        TaikojukuPacks: []);
}
