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
        response.AryItemShopDatas = Ac15InitialDataService.BuildItemShopInfoRows(snapshot);
        response.AryTelopDatas = Ac15InitialDataService.BuildTelopInfoRows(snapshot);
        response.AryEventFolderDatas = Ac15InitialDataService.BuildEventFolderInfoRows(snapshot);
        response.AryTaikojukuDatas = Ac15InitialDataService.BuildTaikojukuInfoRows(snapshot, Ac15EraProfiles.Green);

        return ValueTask.FromResult(response);
    }
}
