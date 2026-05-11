namespace TaikoLocalServer.Application.Handlers;

public partial class GetDanOdaiQueryHandler
{
    private partial ValueTask<List<DanData>> HandleGreen(GetDanOdaiQuery request, CancellationToken cancellationToken) =>
        ValueTask.FromResult(new List<DanData>());
}
