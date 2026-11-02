using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetInitialDataQueryHandler
{
    private partial ValueTask<CommonInitialDataCheckResponse> HandleKimidori(
        GetInitialDataQuery request,
        CancellationToken cancellationToken)
    {
        var snapshot = Ac15CatalogSnapshotFactory.FromKimidori(gameDataService.Kimidori());
        var response = Ac15InitialDataService.BuildCommonInitialData(snapshot, Ac15EraProfiles.Kimidori);
        return ValueTask.FromResult(response);
    }
}
