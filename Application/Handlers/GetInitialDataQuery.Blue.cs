namespace TaikoLocalServer.Application.Handlers;

public partial class GetInitialDataQueryHandler
{
    private partial ValueTask<CommonInitialDataCheckResponse> HandleBlue(
        GetInitialDataQuery request,
        CancellationToken cancellationToken)
    {
        var blue = gameDataService.Blue();
        var activeShop = blue.ItemShopCatalog.ActiveSeason;
        var shopSongIds = blue.ItemShopCatalog.IsEnabled && activeShop is not null
            ? activeShop.Items.Where(item => item.ItemType == 1).Select(item => item.ItemId).ToHashSet()
            : [];
        var allSongs = blue.MusicInfoFileOrder
            .Select(song => song.SongNo)
            .Where(songNo => !shopSongIds.Contains(songNo));

        return ValueTask.FromResult(new CommonInitialDataCheckResponse
        {
            Result = 1,
            DefaultSongFlg = BlueProtocolBytes.CreateFixedBitset(allSongs, BlueProtocolBytes.SongFlagBytes),
            AchievementSongBit = new byte[BlueProtocolBytes.SongFlagBytes],
            UraReleaseBit = new byte[BlueProtocolBytes.SongFlagBytes],
            SongHashVer = blue.SongHashVersion,
            IsDanplay = true,
            IsClose = false,
            IsItemshop = blue.ItemShopCatalog.IsEnabled && activeShop is not null && activeShop.Items.Count > 0,
            AryBlueItemShopDatas = activeShop is null
                ? []
                :
                [
                    new CommonInitialDataCheckResponse.InformationData
                    {
                        InfoId = activeShop.SeasonId,
                        VerupNo = activeShop.VerupNo
                    }
                ],
            AryBlueTelopDatas = blue.Telops.Values
                .OrderBy(entry => entry.TelopId)
                .Select(entry => new CommonInitialDataCheckResponse.InformationData
                {
                    InfoId = entry.TelopId,
                    VerupNo = entry.VerupNo
                })
                .ToList(),
            AryBlueEventFolderDatas = blue.EventFolders.Values
                .Select(entry => new CommonInitialDataCheckResponse.InformationData
                {
                    InfoId = entry.FolderId,
                    VerupNo = entry.VerupNo
                })
                .ToList(),
            AryBlueTaikojukuDatas = blue.TaikojukuFileOrder
                .Where(entry => entry.ChallengeLevel is >= 1 and <= 25)
                .Select(entry => new CommonInitialDataCheckResponse.InformationData
                {
                    InfoId = entry.ChallengeLevel,
                    VerupNo = entry.VerupNo + 1
                })
                .ToList(),
            AryBlueLegaltermsDatas = [],
            ServerCurrentDatetime = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds()
        });
    }
}
