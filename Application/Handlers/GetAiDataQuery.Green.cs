namespace TaikoLocalServer.Application.Handlers;

public partial class GetAiDataQueryHandler
{
    private partial ValueTask<CommonAiDataResponse> HandleGreen(GetAiDataQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green GetAiData stub for baid {Baid}, returning empty", request.Baid);
        return ValueTask.FromResult(new CommonAiDataResponse { Result = 1 });
    }
}
