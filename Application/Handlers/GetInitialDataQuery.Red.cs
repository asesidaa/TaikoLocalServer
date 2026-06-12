using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetInitialDataQueryHandler
{
    private partial ValueTask<CommonInitialDataCheckResponse> HandleRed(
        GetInitialDataQuery request,
        CancellationToken cancellationToken)
    {
        var red = gameDataService.Red();
        var snapshot = Ac15CatalogSnapshotFactory.FromRed(red);
        var response = Ac15InitialDataService.BuildCommonInitialData(snapshot, Ac15EraProfiles.Red);

        return ValueTask.FromResult(response);
    }
}
