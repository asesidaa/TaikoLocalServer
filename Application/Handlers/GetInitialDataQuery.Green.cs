using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetInitialDataQueryHandler
{
    private partial ValueTask<CommonInitialDataCheckResponse> HandleGreen(GetInitialDataQuery request, CancellationToken cancellationToken)
    {
        var green = gameDataService.Green();
        var snapshot = Ac15CatalogSnapshotFactory.FromGreen(green);
        var response = Ac15InitialDataService.BuildCommonInitialData(snapshot, Ac15EraProfiles.Green);

        response.IsGhostbattleplay = true;
        response.AryGreenItemShopDatas = Ac15InitialDataService.BuildItemShopInfoRows(snapshot);
        response.AryGreenTelopDatas = Ac15InitialDataService.BuildTelopInfoRows(snapshot);
        response.AryGreenEventFolderDatas = Ac15InitialDataService.BuildEventFolderInfoRows(snapshot);
        response.AryGreenTaikojukuDatas = Ac15InitialDataService.BuildTaikojukuInfoRows(
            snapshot,
            Ac15EraProfiles.Green,
            (_, verupNo) => verupNo + 1);

        return ValueTask.FromResult(response);
    }
}
