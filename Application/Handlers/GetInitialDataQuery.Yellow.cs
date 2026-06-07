using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetInitialDataQueryHandler
{
    private partial ValueTask<CommonInitialDataCheckResponse> HandleYellow(
        GetInitialDataQuery request,
        CancellationToken cancellationToken)
    {
        var yellow = gameDataService.Yellow();
        var snapshot = Ac15CatalogSnapshotFactory.FromYellow(yellow);
        var response = Ac15InitialDataService.BuildCommonInitialData(snapshot, Ac15EraProfiles.Yellow);

        response.AryYellowItemShopDatas = Ac15InitialDataService.BuildItemShopInfoRows(snapshot);
        response.AryYellowTelopDatas = Ac15InitialDataService.BuildTelopInfoRows(snapshot);
        response.AryYellowEventFolderDatas = Ac15InitialDataService.BuildEventFolderInfoRows(snapshot);
        response.AryYellowTaikojukuDatas = Ac15InitialDataService.BuildTaikojukuInfoRows(snapshot, Ac15EraProfiles.Yellow);
        response.AryYellowLegaltermsDatas = [];

        return ValueTask.FromResult(response);
    }
}
