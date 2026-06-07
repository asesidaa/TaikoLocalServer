namespace TaikoLocalServer.Application.Ac15;

public static class Ac15InitialDataService
{
    public static CommonInitialDataCheckResponse BuildCommonInitialData(
        Ac15CatalogSnapshot snapshot,
        Ac15EraProfile profile)
    {
        var activeShop = snapshot.ItemShopCatalog.ActiveSeason;
        var activeShopWithRows = snapshot.ItemShopCatalog.IsEnabled && activeShop is { Items.Count: > 0 }
            ? activeShop
            : null;
        var shopSongIds = activeShopWithRows is not null
            ? activeShopWithRows.Items
                .Where(item => item.ItemType == Ac15ShopItemType.Song)
                .Select(item => item.ItemId)
                .ToHashSet()
            : [];
        var defaultSongNoes = snapshot.SongNoesInFileOrder.Where(songNo => !shopSongIds.Contains(songNo));

        return new CommonInitialDataCheckResponse
        {
            Result = 1,
            DefaultSongFlg = Ac15ProtocolBytes.CreateFixedBitset(defaultSongNoes, profile.Limits.SongFlagBytes),
            AchievementSongBit = new byte[profile.Limits.SongFlagBytes],
            UraReleaseBit = new byte[profile.Limits.SongFlagBytes],
            SongHashVer = snapshot.SongHashVersion,
            IsDanplay = profile.Features.Dani,
            IsClose = false,
            IsItemshop = profile.Features.ItemShop && activeShopWithRows is not null,
            ServerCurrentDatetime = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds()
        };
    }

    public static List<CommonInitialDataCheckResponse.InformationData> BuildItemShopInfoRows(Ac15CatalogSnapshot snapshot)
    {
        var activeShop = snapshot.ItemShopCatalog.ActiveSeason;
        return activeShop is { Items.Count: > 0 }
            ?
            [
                new CommonInitialDataCheckResponse.InformationData
                {
                    InfoId = activeShop.SeasonId,
                    VerupNo = activeShop.VerupNo
                }
            ]
            : [];
    }

    public static List<CommonInitialDataCheckResponse.InformationData> BuildTelopInfoRows(Ac15CatalogSnapshot snapshot)
        => snapshot.Telops.Values
            .OrderBy(entry => entry.TelopId)
            .Select(entry => new CommonInitialDataCheckResponse.InformationData
            {
                InfoId = entry.TelopId,
                VerupNo = entry.VerupNo
            })
            .ToList();

    public static List<CommonInitialDataCheckResponse.InformationData> BuildEventFolderInfoRows(Ac15CatalogSnapshot snapshot)
        => snapshot.EventFolders.Values
            .Select(entry => new CommonInitialDataCheckResponse.InformationData
            {
                InfoId = entry.FolderId,
                VerupNo = entry.VerupNo
            })
            .ToList();

    public static List<CommonInitialDataCheckResponse.InformationData> BuildTaikojukuInfoRows(
        Ac15CatalogSnapshot snapshot,
        Ac15EraProfile profile)
        => snapshot.TaikojukuPacks
            .Where(entry => entry.ChallengeLevel >= profile.Limits.MinNormalDanId
                && entry.ChallengeLevel <= profile.Limits.MaxNormalDanId)
            .Select(entry => new CommonInitialDataCheckResponse.InformationData
            {
                InfoId = entry.ChallengeLevel,
                VerupNo = entry.VerupNo
            })
            .ToList();
}
