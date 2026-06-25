using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetInitialDataQueryHandler
{
    private partial ValueTask<CommonInitialDataCheckResponse> HandleMomoiro(
        GetInitialDataQuery request,
        CancellationToken cancellationToken)
    {
        var snapshot = Ac15CatalogSnapshotFactory.FromMomoiro(gameDataService.Momoiro());
        var response = Ac15InitialDataService.BuildCommonInitialData(snapshot, Ac15EraProfiles.Momoiro);
        return ValueTask.FromResult(response);
    }
}
