using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetInitialDataQueryHandler
{
    private partial ValueTask<CommonInitialDataCheckResponse> HandleWhite(
        GetInitialDataQuery request,
        CancellationToken cancellationToken)
    {
        var white = gameDataService.White();
        var snapshot = Ac15CatalogSnapshotFactory.FromWhite(white);
        var response = Ac15InitialDataService.BuildCommonInitialData(snapshot, Ac15EraProfiles.White);

        return ValueTask.FromResult(response);
    }
}
