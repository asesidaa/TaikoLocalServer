using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetInitialDataQueryHandler
{
    private partial ValueTask<CommonInitialDataCheckResponse> HandleMurasaki(
        GetInitialDataQuery request,
        CancellationToken cancellationToken)
    {
        var snapshot = Ac15CatalogSnapshotFactory.FromMurasaki(gameDataService.Murasaki());
        var response = Ac15InitialDataService.BuildCommonInitialData(snapshot, Ac15EraProfiles.Murasaki);

        return ValueTask.FromResult(response);
    }
}
