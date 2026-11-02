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

        return ValueTask.FromResult(response);
    }
}
