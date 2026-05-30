namespace TaikoLocalServer.Application.Handlers;

public partial class GetInitialDataQueryHandler
{
    private partial ValueTask<CommonInitialDataCheckResponse> HandleBlue(
        GetInitialDataQuery request,
        CancellationToken cancellationToken)
    {
        var blue = gameDataService.Blue();
        var activeShop = blue.ItemShopCatalog.ActiveSeason;
        var activeShopWithRows = blue.ItemShopCatalog.IsEnabled && activeShop is { Items.Count: > 0 }
            ? activeShop
            : null;
        var shopSongIds = activeShopWithRows is not null
            ? activeShopWithRows.Items.Where(item => item.ItemType == 1).Select(item => item.ItemId).ToHashSet()
            : [];
        var allSongs = blue.MusicInfoFileOrder
            .Select(song => song.SongNo)
            .Where(songNo => !shopSongIds.Contains(songNo));
        var battle = blue.BattleCatalog;

        return ValueTask.FromResult(new CommonInitialDataCheckResponse
        {
            Result = 1,
            DefaultSongFlg = BlueProtocolBytes.CreateFixedBitset(allSongs, BlueProtocolBytes.SongFlagBytes),
            AchievementSongBit = new byte[BlueProtocolBytes.SongFlagBytes],
            UraReleaseBit = new byte[BlueProtocolBytes.SongFlagBytes],
            SongHashVer = blue.SongHashVersion,
            IsDanplay = true,
            IsClose = false,
            IsItemshop = activeShopWithRows is not null,
            IsBattleplay = battle.EnablesBattleAdvertisement,
            ReleaseBattleStageFlg = battle.EnablesBattleAdvertisement
                ? BlueProtocolBytes.CreateFixedBitset(
                    battle.ReleaseBattleStageIds,
                    BlueProtocolBytes.BattleStageFlagBytes)
                : new byte[BlueProtocolBytes.BattleStageFlagBytes],
            ReleaseBattleSpecialFlg = battle.EnablesBattleAdvertisement
                ? BlueProtocolBytes.CreateFixedBitset(
                    battle.ReleaseBattleSpecialIds,
                    BlueProtocolBytes.BattleSpecialFlagBytes)
                : new byte[BlueProtocolBytes.BattleSpecialFlagBytes],
            BattleBondsLvCap = battle.EnablesBattleAdvertisement
                ? battle.BattleBondsLvCap ?? 0
                : 0,
            AryBlueItemShopDatas = activeShopWithRows is null
                ? []
                :
                [
                    new CommonInitialDataCheckResponse.InformationData
                    {
                        InfoId = activeShopWithRows.SeasonId,
                        VerupNo = activeShopWithRows.VerupNo
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
                    VerupNo = 3
                })
                .ToList(),
            AryBlueLegaltermsDatas = [],
            ServerCurrentDatetime = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds()
        });
    }
}
