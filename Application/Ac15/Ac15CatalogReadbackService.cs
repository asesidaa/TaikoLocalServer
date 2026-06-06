namespace TaikoLocalServer.Application.Ac15;

public static class Ac15CatalogReadbackService
{
    public static CommonGetFolderResponse BuildFolderResponse(
        Ac15CatalogSnapshot snapshot,
        IEnumerable<uint> requestedFolderIds)
    {
        var response = new CommonGetFolderResponse { Result = 1 };
        foreach (var folderId in requestedFolderIds)
        {
            if (snapshot.EventFolders.TryGetValue(folderId, out var folderData))
            {
                response.AryEventfolderDatas.Add(folderData);
            }
        }

        return response;
    }

    public static CommonGetTelopResponse BuildTelopResponse(Ac15CatalogSnapshot snapshot, uint telopId)
    {
        if (!snapshot.Telops.TryGetValue(telopId, out var entry))
        {
            return new CommonGetTelopResponse { Result = 1 };
        }

        return new CommonGetTelopResponse
        {
            Result = 1,
            VerupNo = entry.VerupNo,
            StartDatetime = entry.StartDatetime,
            EndDatetime = entry.EndDatetime,
            Telop = entry.Message
        };
    }

    public static CommonRecommendResponse BuildRecommendResponse(Ac15CatalogSnapshot snapshot)
        => new()
        {
            Result = 1,
            RecommendSong = snapshot.RecommendSong,
            RecommendBestSong = snapshot.RecommendBestSongs.ToList()
        };

    public static CommonItemShopInfoResponse BuildItemShopInfo(Ac15CatalogSnapshot snapshot)
    {
        var catalog = snapshot.ItemShopCatalog;
        var season = catalog.ActiveSeason;
        if (!catalog.IsEnabled || season is null || season.Items.Count == 0)
        {
            return new CommonItemShopInfoResponse { Result = 1 };
        }

        return new CommonItemShopInfoResponse
        {
            Result = 1,
            VerupNo = season.VerupNo,
            SeasonId = season.SeasonId,
            Telop = season.Telop,
            StartDatetime = season.StartDatetime,
            EndDatetime = season.EndDatetime,
            AfterstartDays = season.AfterstartDays,
            BeforecloseDays = season.BeforecloseDays,
            AryItemshopData = season.Items
                .OrderBy(item => item.ItemNo)
                .Select(item => new CommonItemShopInfoResponse.ItemShopData
                {
                    ItemNo = item.ItemNo,
                    ItemType = item.ItemType.ToProtocolValue(),
                    ItemId = item.ItemId,
                    ItemPrice = item.Price
                })
                .ToList()
        };
    }
}
