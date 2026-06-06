using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetInitialDataQueryHandler
{
    private partial ValueTask<CommonInitialDataCheckResponse> HandleBlue(
        GetInitialDataQuery request,
        CancellationToken cancellationToken)
    {
        var blue = gameDataService.Blue();
        var battle = blue.BattleCatalog;
        var snapshot = Ac15CatalogSnapshotFactory.FromBlue(blue);
        var response = Ac15InitialDataService.BuildCommonInitialData(snapshot, Ac15EraProfiles.Blue);

        response.IsBattleplay = battle.EnablesBattleAdvertisement;
        response.ReleaseBattleStageFlg = battle.EnablesBattleAdvertisement
            ? BlueProtocolBytes.CreateFixedBitset(
                battle.ReleaseBattleStageIds,
                BlueProtocolBytes.BattleStageFlagBytes)
            : new byte[BlueProtocolBytes.BattleStageFlagBytes];
        response.ReleaseBattleSpecialFlg = battle.EnablesBattleAdvertisement
            ? BlueProtocolBytes.CreateBattleSpecialBitset(battle.ReleaseBattleSpecialIds)
            : new byte[BlueProtocolBytes.BattleSpecialFlagBytes];
        response.BattleBondsLvCap = battle.EnablesBattleAdvertisement
            ? battle.BattleBondsLvCap ?? 0
            : 0;
        response.AryBlueItemShopDatas = Ac15InitialDataService.BuildItemShopInfoRows(snapshot);
        response.AryBlueTelopDatas = Ac15InitialDataService.BuildTelopInfoRows(snapshot);
        response.AryBlueEventFolderDatas = Ac15InitialDataService.BuildEventFolderInfoRows(snapshot);
        response.AryBlueTaikojukuDatas = Ac15InitialDataService.BuildTaikojukuInfoRows(
            snapshot,
            Ac15EraProfiles.Blue,
            (_, _) => 3);
        response.AryBlueLegaltermsDatas = [];

        return ValueTask.FromResult(response);
    }
}
