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

        return ValueTask.FromResult(response);
    }
}
